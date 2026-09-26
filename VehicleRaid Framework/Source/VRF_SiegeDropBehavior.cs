using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using Vehicles;
using UnityEngine;
using HarmonyLib;

namespace VehicleRaidFramework
{
    public static class VRF_SiegeDropBehavior
    {
        private const float DefendRadius = 18f;

        private static readonly List<(VehiclePawn pod, Map map, int disembarkTick)> _pendingDisembark =
            new List<(VehiclePawn, Map, int)>();

        private static readonly List<(Pawn pawn, IntVec3 podPos, int applyTick)> _pendingGuards =
            new List<(Pawn, IntVec3, int)>();

        public static void OnSiegeDropLanded(VehiclePawn pod, Map map)
        {
            if (pod == null || !pod.Spawned || map == null) return;
            if (!VRF_TransportUtil.IsSiegeDropVehicle(pod)) return;

            _pendingDisembark.Add((pod, map, Find.TickManager.TicksGame + 60));
        }

        public static void Tick()
        {
            int now = Find.TickManager.TicksGame;

            for (int i = _pendingDisembark.Count - 1; i >= 0; i--)
            {
                var (pod, map, disembarkTick) = _pendingDisembark[i];
                if (now < disembarkTick) continue;

                _pendingDisembark.RemoveAt(i);

                if (pod == null || pod.Destroyed || map == null || map.Disposed) continue;
                if (!pod.Spawned || pod.Map != map) continue;

                DoDisembark(pod, map);
            }

            for (int i = _pendingGuards.Count - 1; i >= 0; i--)
            {
                var (guard, podPos, applyTick) = _pendingGuards[i];
                if (now < applyTick) continue;

                _pendingGuards.RemoveAt(i);

                if (guard == null || guard.Dead || !guard.Spawned) continue;

                guard.mindState.duty = new PawnDuty(DutyDefOf.Defend, podPos, DefendRadius);
                guard.jobs?.StopAll();
            }
        }

        private static void DoDisembark(VehiclePawn pod, Map map)
        {
            List<Pawn> toEject = new List<Pawn>();
            foreach (VehicleRoleHandler handler in pod.handlers)
            {
                if (handler?.role == null) continue;
                if (handler.role.HandlingTypes != HandlingType.None) continue;
                foreach (Pawn p in handler.thingOwner)
                {
                    if (p == null || p.Dead || p.Downed) continue;
                    toEject.Add(p);
                }
            }

            if (toEject.Count == 0) return;

            int guardCount = toEject.Count > 1 ? Mathf.Max(1, toEject.Count / 2) : 0;
            List<Pawn> guards  = toEject.Take(guardCount).ToList();
            List<Pawn> raiders = toEject.Skip(guardCount).ToList();

            foreach (Pawn p in toEject)
            {
                IntVec3 exitCell = FindEjectCell(pod, map);
                VRF_TransportUtil.LastDisembarkTick[p.thingIDNumber] = Find.TickManager.TicksGame;
                pod.DisembarkPawn(p);
                if (exitCell.IsValid && p.Spawned && p.Map == map)
                {
                    p.Position = exitCell;
                    p.Notify_Teleported(true, false);
                }
            }

            foreach (Pawn raider in raiders)
            {
                if (!raider.Spawned) continue;
                CrewManager.SyncDisembarkedPawnLord(raider, pod);
                DutyDef assaultDuty =
                    VRF_DutyDefOf.VRF_InfantryAssault
                    ?? DefDatabase<DutyDef>.GetNamed("VRF_InfantryAssault", false)
                    ?? DutyDefOf.AssaultColony;
                if (assaultDuty != null)
                    raider.mindState.duty = new PawnDuty(assaultDuty);
                raider.jobs?.StopAll();
            }

            Lord podLord = pod.GetLord();
            int applyTick = Find.TickManager.TicksGame + 1;
            foreach (Pawn guard in guards)
            {
                if (!guard.Spawned) continue;
                if (podLord != null && !podLord.ownedPawns.Contains(guard))
                    podLord.AddPawn(guard);
                _pendingGuards.Add((guard, pod.Position, applyTick));
            }
        }

        private static IntVec3 FindEjectCell(VehiclePawn pod, Map map)
        {
            CellRect rect = pod.OccupiedRect();
            for (int radius = 1; radius <= 6; radius++)
            {
                foreach (IntVec3 cell in rect.ExpandedBy(radius).Cells)
                {
                    if (rect.Contains(cell)) continue;
                    if (!cell.InBounds(map)) continue;
                    if (!cell.Standable(map)) continue;
                    if (cell.Roofed(map)) continue;
                    return cell;
                }
            }
            return CellFinder.RandomClosewalkCellNear(pod.Position, map, 6, null);
        }

        private static readonly System.Reflection.FieldInfo _innerListField =
            HarmonyLib.AccessTools.Field(typeof(ThingOwner<Pawn>), "innerList");

        public static void ReassignSiegeDropCrew(VehiclePawn pod)
        {
            if (pod == null || !pod.Spawned || pod.Faction == null || pod.Faction.IsPlayer) return;
            if (!IsSiegeDropVehicle(pod)) return;

            var handlers = pod.handlers;
            if (handlers == null || handlers.Count == 0) return;

            List<Pawn> conscious = new List<Pawn>();
            List<Pawn> downed    = new List<Pawn>();
            foreach (var h in handlers)
            {
                foreach (Pawn p in h.thingOwner)
                {
                    if (p == null || p.Dead) continue;
                    if (p.Downed) downed.Add(p);
                    else          conscious.Add(p);
                }
            }

            var movementHandlers = handlers
                .Where(h => h?.role != null && (h.role.HandlingTypes & HandlingType.Movement) != 0)
                .OrderByDescending(h => h.role.SlotsToOperate)
                .ToList();
            var turretHandlers = handlers
                .Where(h => h?.role != null
                         && (h.role.HandlingTypes & HandlingType.Turret)   != 0
                         && (h.role.HandlingTypes & HandlingType.Movement) == 0)
                .ToList();
            var otherHandlers = handlers
                .Where(h => h != null && h.role != null
                         && (h.role.HandlingTypes & HandlingType.Movement) == 0
                         && (h.role.HandlingTypes & HandlingType.Turret)   == 0)
                .ToList();

            bool needsReassign = false;
            foreach (var h in movementHandlers)
                if (h.thingOwner.Count < h.role.SlotsToOperate) { needsReassign = true; break; }
            if (!needsReassign)
                foreach (var h in turretHandlers)
                    if (h.thingOwner.Count < h.role.Slots) { needsReassign = true; break; }
            if (!needsReassign) return;

            foreach (var h in handlers)
            {
                for (int i = h.thingOwner.Count - 1; i >= 0; i--)
                {
                    Pawn p = h.thingOwner[i] as Pawn;
                    if (p != null) h.thingOwner.Remove(p);
                }
            }

            FillHandlers(conscious, movementHandlers);
            FillHandlers(conscious, turretHandlers);
            FillHandlers(conscious, otherHandlers);
            FillHandlers(downed, otherHandlers);
            FillHandlers(downed, turretHandlers);
            FillHandlers(downed, movementHandlers);
        }

        private static bool IsSiegeDropVehicle(VehiclePawn vehicle)
            => VRF_TransportUtil.IsSiegeDropVehicle(vehicle);

        private static void FillHandlers(List<Pawn> pawns, List<VehicleRoleHandler> handlers)
        {
            foreach (var h in handlers)
            {
                if (h?.role == null) continue;
                while (pawns.Count > 0 && h.thingOwner.Count < h.role.Slots)
                {
                    Pawn p = pawns[0];
                    pawns.RemoveAt(0);
                    if (p == null) continue;
                    if (p.holdingOwner != null) p.holdingOwner.Remove(p);
                    if (_innerListField != null)
                    {
                        var innerList = _innerListField.GetValue(h.thingOwner) as List<Pawn>;
                        if (innerList != null) { p.holdingOwner = h.thingOwner; innerList.Add(p); }
                    }
                    else
                    {
                        h.thingOwner.TryAddOrTransfer(p, false);
                    }
                }
            }
        }
    }
}
