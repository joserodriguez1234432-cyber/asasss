using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using Vehicles;
using VehicleRaid;

namespace VehicleRaidFramework
{
    public class HoverNPC_TransportManager : MapComponent
    {
        private const float DropRange             = 12f;
        private const float TooCloseRange         = 6f;
        private const int   EjectIntervalTicks    = 30;
        private const int   PositionUpdateTicks   = 60;
        private const int   ScanIntervalTicks     = 30;
        private const int   AlternateSearchRadius = 20;
        private const int   InRangeDelayTicks     = 60;

        private readonly List<VehiclePawn>    activeVehicles      = new List<VehiclePawn>();
        private readonly List<VehiclePawn>    activeArmedVehicles = new List<VehiclePawn>();
        private readonly List<VehiclePawn>    exitingVehicles     = new List<VehiclePawn>();
        private readonly Dictionary<int, int> lastEjectTick       = new Dictionary<int, int>();
        private readonly Dictionary<int, int> lastPositionTick    = new Dictionary<int, int>();
        private readonly Dictionary<int, int> inRangeSinceTick    = new Dictionary<int, int>();

        public HoverNPC_TransportManager(Map map) : base(map) { }

        public static HoverNPC_TransportManager GetFor(Map map)
            => map?.GetComponent<HoverNPC_TransportManager>();

        public void RegisterVehicle(VehiclePawn vehicle)
        {
            if (!activeVehicles.Contains(vehicle))
                activeVehicles.Add(vehicle);
        }

        public void RegisterArmedVehicle(VehiclePawn vehicle)
        {
            if (!activeArmedVehicles.Contains(vehicle))
                activeArmedVehicles.Add(vehicle);
        }

        public void UnregisterVehicle(VehiclePawn vehicle)
        {
            activeVehicles.Remove(vehicle);
            activeArmedVehicles.Remove(vehicle);
            lastEjectTick.Remove(vehicle.thingIDNumber);
            lastPositionTick.Remove(vehicle.thingIDNumber);
            inRangeSinceTick.Remove(vehicle.thingIDNumber);
        }

        public override void MapComponentTick()
        {
            int tick = Find.TickManager.TicksGame;

            if (tick % 300 == 0)
            {
                var pawnsSnapshot = map.mapPawns.AllPawnsSpawned.ToList();
                foreach (Pawn p in pawnsSnapshot)
                {
                    if (!(p is VehiclePawn v)) continue;
                    if (v.Faction == null || v.Faction.IsPlayer || v.Dead) continue;
                    if (CrewManager.IsGravshipVehicle(v)) continue;
                    var hc = v.GetComp<CompVehicleHover>();
                    if (hc == null || hc.State != HoverState.Hovering) continue;
                    if (v.CompVehicleTurrets != null && v.CompVehicleTurrets.Turrets != null && v.CompVehicleTurrets.Turrets.Count > 0) continue;
                    if (!(v.GetLord()?.LordJob is LordJob_VehicleRaid)) continue;

                    var protectedPilots = BuildProtectedPilotSet(v);
                    bool hasPax = false;
                    foreach (Pawn aboard in v.AllPawnsAboard)
                    {
                        if (!aboard.Dead && !aboard.Downed && !protectedPilots.Contains(aboard)) { hasPax = true; break; }
                    }
                    if (!hasPax)
                    {
                        if (!exitingVehicles.Contains(v))
                            exitingVehicles.Add(v);
                        TriggerExit(v);
                    }
                }
            }

            if (activeVehicles.Count == 0 && exitingVehicles.Count == 0 && activeArmedVehicles.Count == 0) return;
            if (tick % ScanIntervalTicks != 0) return;

            for (int i = exitingVehicles.Count - 1; i >= 0; i--)
            {
                VehiclePawn v = exitingVehicles[i];
                if (v == null || v.Dead || !v.Spawned || v.Map != map)
                {
                    exitingVehicles.RemoveAt(i);
                    continue;
                }
                TriggerExit(v);
            }

            if (activeVehicles.Count == 0 && activeArmedVehicles.Count == 0) return;

            for (int i = activeVehicles.Count - 1; i >= 0; i--)
            {
                VehiclePawn vehicle = activeVehicles[i];

                if (vehicle == null || vehicle.Dead || !vehicle.Spawned || vehicle.Map != map)
                {
                    activeVehicles.RemoveAt(i);
                    continue;
                }

                var hoverComp = vehicle.GetComp<CompVehicleHover>();
                if (hoverComp == null || hoverComp.State != HoverState.Hovering)
                {
                    activeVehicles.RemoveAt(i);
                    continue;
                }

                ProcessHoverTransport(vehicle, hoverComp);
            }

            for (int i = activeArmedVehicles.Count - 1; i >= 0; i--)
            {
                VehiclePawn vehicle = activeArmedVehicles[i];

                if (vehicle == null || vehicle.Dead || !vehicle.Spawned || vehicle.Map != map)
                {
                    activeArmedVehicles.RemoveAt(i);
                    continue;
                }

                var hoverComp = vehicle.GetComp<CompVehicleHover>();
                if (hoverComp == null || hoverComp.State != HoverState.Hovering)
                {
                    activeArmedVehicles.RemoveAt(i);
                    continue;
                }

                ProcessArmedHoverTransport(vehicle, hoverComp);
            }
        }

        private void ProcessHoverTransport(VehiclePawn vehicle, CompVehicleHover hoverComp)
        {
            int id   = vehicle.thingIDNumber;
            int tick = Find.TickManager.TicksGame;

            var protectedPilots = BuildProtectedPilotSet(vehicle);

            Pawn toEject = null;
            foreach (Pawn p in vehicle.AllPawnsAboard)
            {
                if (!p.Dead && !p.Downed && !protectedPilots.Contains(p)) { toEject = p; break; }
            }

            if (toEject == null)
            {
                int idx = activeVehicles.IndexOf(vehicle);
                if (idx >= 0) activeVehicles.RemoveAt(idx);
                lastEjectTick.Remove(id);
                lastPositionTick.Remove(id);
                inRangeSinceTick.Remove(id);
                if (!exitingVehicles.Contains(vehicle))
                    exitingVehicles.Add(vehicle);
                TriggerExit(vehicle);
                return;
            }

            bool isHoldPosition = vehicle.GetLord()?.CurLordToil is LordToil_VehicleHoldPosition;

            if (isHoldPosition)
            {
                Thing targetInDefendRange = FindBestTarget(vehicle, hoverComp, 25f);
                if (targetInDefendRange != null)
                {
                    if (!lastEjectTick.TryGetValue(id, out int prevEject) || tick - prevEject >= EjectIntervalTicks)
                    {
                        lastEjectTick[id] = tick;
                        DoEject(vehicle, toEject);
                    }
                }
                return;
            }

            Thing target = FindBestTarget(vehicle, hoverComp);

            if (target == null)
            {
                hoverComp.isFacingTargetNPC = false;
                hoverComp.facingTargetNPC   = null;
                return;
            }

            Vector2 hoverPos  = hoverComp.realPos;
            Vector2 targetPos = new Vector2(target.DrawPos.x, target.DrawPos.z);
            float   dist      = Vector2.Distance(hoverPos, targetPos);

            bool targetUnderBlockingRoof = IsUnderBlockingRoof(target.Position);
            IntVec3 vehicleRealCell = new IntVec3(
                Mathf.RoundToInt(hoverComp.realPos.x - 0.5f), 0,
                Mathf.RoundToInt(hoverComp.realPos.y - 0.5f));
            bool vehicleUnderBlockingRoof = IsUnderBlockingRoof(vehicleRealCell);

            if (targetUnderBlockingRoof)
            {
                IntVec3 altPos = FindOpenPositionNear(target.Position, AlternateSearchRadius);
                if (!altPos.IsValid)
                    altPos = FindOpenPositionNear(target.Position, 150);
                if (!altPos.IsValid)
                {
                    hoverComp.isFacingTargetNPC = false;
                    hoverComp.facingTargetNPC   = null;
                    return;
                }
                targetPos = new Vector2(altPos.x + 0.5f, altPos.z + 0.5f);
                dist      = Vector2.Distance(hoverPos, targetPos);
            }

            if (!lastPositionTick.TryGetValue(id, out int lastPos) || tick - lastPos >= PositionUpdateTicks)
            {
                lastPositionTick[id] = tick;

                if (dist < TooCloseRange)
                {
                    inRangeSinceTick.Remove(id);
                    Vector2 awayDir = (hoverPos - targetPos).normalized;
                    if (awayDir.sqrMagnitude < 0.01f) awayDir = new Vector2(1f, 0f);
                    Vector2 evadePos = hoverPos + awayDir * (TooCloseRange + 4f);
                    evadePos = ClampToMap(evadePos);
                    hoverComp.SetTarget(new Vector3(evadePos.x, 0f, evadePos.y));
                    hoverComp.isFacingTargetNPC = true;
                    hoverComp.facingTargetNPC   = target;
                    return;
                }

                if (dist > DropRange)
                {
                    inRangeSinceTick.Remove(id);
                    Vector2 dir         = (targetPos - hoverPos).normalized;
                    float   moveDist    = dist - (DropRange - 1f);
                    Vector2 approachPos = hoverPos + dir * moveDist;
                    approachPos = ClampToMap(approachPos);

                    IntVec3 approachCell = new IntVec3(Mathf.RoundToInt(approachPos.x - 0.5f), 0, Mathf.RoundToInt(approachPos.y - 0.5f));
                    if (IsUnderBlockingRoof(approachCell))
                    {
                        IntVec3 safeApproach = FindOpenPositionNear(approachCell, 8);
                        if (safeApproach.IsValid)
                            approachPos = new Vector2(safeApproach.x + 0.5f, safeApproach.z + 0.5f);
                    }

                    hoverComp.SetTarget(new Vector3(approachPos.x, 0f, approachPos.y));
                    hoverComp.isFacingTargetNPC = true;
                    hoverComp.facingTargetNPC   = target;
                    return;
                }
            }

            hoverComp.isFacingTargetNPC = true;
            hoverComp.facingTargetNPC   = target;

            if (vehicleUnderBlockingRoof) return;

            if (!inRangeSinceTick.ContainsKey(id))
            {
                inRangeSinceTick[id] = tick;
                return;
            }

            if (tick - inRangeSinceTick[id] < InRangeDelayTicks) return;

            if (lastEjectTick.TryGetValue(id, out int lastEject) && tick - lastEject < EjectIntervalTicks) return;

            lastEjectTick[id] = tick;
            DoEject(vehicle, toEject);
        }

        private void ProcessArmedHoverTransport(VehiclePawn vehicle, CompVehicleHover hoverComp)
        {
            int id   = vehicle.thingIDNumber;
            int tick = Find.TickManager.TicksGame;

            Pawn toEject = FindNextArmedPassengerToEject(vehicle);
            if (toEject == null) return;

            Thing target = FindBestTarget(vehicle, hoverComp);
            if (target == null) return;

            Vector2 hoverPos  = hoverComp.realPos;
            Vector2 targetPos = new Vector2(target.DrawPos.x, target.DrawPos.z);
            float   dist      = Vector2.Distance(hoverPos, targetPos);

            bool targetUnderBlockingRoof = IsUnderBlockingRoof(target.Position);
            IntVec3 vehicleRealCell = new IntVec3(
                Mathf.RoundToInt(hoverComp.realPos.x - 0.5f), 0,
                Mathf.RoundToInt(hoverComp.realPos.y - 0.5f));
            bool vehicleUnderBlockingRoof = IsUnderBlockingRoof(vehicleRealCell);

            if (targetUnderBlockingRoof)
            {
                IntVec3 altPos = FindOpenPositionNear(target.Position, AlternateSearchRadius);
                if (!altPos.IsValid) altPos = FindOpenPositionNear(target.Position, 150);
                if (!altPos.IsValid) return;
                targetPos = new Vector2(altPos.x + 0.5f, altPos.z + 0.5f);
                dist      = Vector2.Distance(hoverPos, targetPos);
            }

            if (!lastPositionTick.TryGetValue(id, out int lastPos) || tick - lastPos >= PositionUpdateTicks)
            {
                lastPositionTick[id] = tick;

                if (dist < TooCloseRange)
                {
                    inRangeSinceTick.Remove(id);
                    return;
                }

                if (dist > DropRange)
                {
                    inRangeSinceTick.Remove(id);
                    return;
                }
            }

            if (vehicleUnderBlockingRoof) return;

            if (!inRangeSinceTick.ContainsKey(id))
            {
                inRangeSinceTick[id] = tick;
                return;
            }

            if (tick - inRangeSinceTick[id] < InRangeDelayTicks) return;
            if (lastEjectTick.TryGetValue(id, out int lastEject) && tick - lastEject < EjectIntervalTicks) return;

            lastEjectTick[id] = tick;
            DoEject(vehicle, toEject);
        }

        private static HashSet<Pawn> BuildProtectedPilotSet(VehiclePawn vehicle)
        {
            var protectedPawns = new HashSet<Pawn>();
            foreach (var handler in vehicle.handlers)
            {
                if (handler?.role == null) continue;

                bool isOperational = (handler.role.HandlingTypes & HandlingType.Movement) != 0
                                  || (handler.role.HandlingTypes & HandlingType.Turret)   != 0;
                if (!isOperational) continue;

                int slotsToOp = handler.role.SlotsToOperate;
                if (slotsToOp <= 0) continue;

                int protected_ = 0;
                foreach (var thing in handler.thingOwner)
                {
                    if (!(thing is Pawn p) || p.Dead || p.Downed) continue;
                    if (protected_ < slotsToOp) { protectedPawns.Add(p); protected_++; }
                }
            }
            return protectedPawns;
        }

        private static Pawn FindNextArmedPassengerToEject(VehiclePawn vehicle)
        {
            var protectedPawns = BuildProtectedPilotSet(vehicle);

            int ejectableCount = 0;
            foreach (Pawn p in vehicle.AllPawnsAboard)
            {
                if (!p.Dead && !p.Downed && !protectedPawns.Contains(p))
                    ejectableCount++;
            }

            if (ejectableCount <= 1) return null;

            foreach (Pawn p in vehicle.AllPawnsAboard)
            {
                if (!p.Dead && !p.Downed && !protectedPawns.Contains(p))
                    return p;
            }
            return null;
        }

        private void TriggerExit(VehiclePawn vehicle)
        {
            if (!vehicle.Spawned || vehicle.Map == null) return;

            var hoverComp = vehicle.GetComp<CompVehicleHover>();
            if (hoverComp == null) return;

            if (vehicle.mindState != null)
            {
                var exitDuty = VRF_DutyDefOf.VRF_VehicleExitMap
                    ?? DefDatabase<DutyDef>.GetNamed("VRF_VehicleExitMap", false)
                    ?? DutyDefOf.ExitMapBest;
                if (vehicle.mindState.duty?.def != exitDuty)
                {
                    vehicle.mindState.duty = new PawnDuty(exitDuty);
                    vehicle.jobs?.EndCurrentJob(JobCondition.InterruptForced);
                }
            }

            // The component owns both the stable exit cell and the arrival check.
            // It is also used by the exit job giver, so the two paths cannot overwrite
            // each other's destination.
            hoverComp.TryExitMapForNPC();
        }

        private static bool IsAtMapEdge(VehiclePawn vehicle)
        {
            if (!vehicle.Spawned || vehicle.Map == null) return false;
            IntVec3 pos = vehicle.Position;
            int margin = 4;
            return pos.x <= margin || pos.z <= margin ||
                   pos.x >= vehicle.Map.Size.x - margin ||
                   pos.z >= vehicle.Map.Size.z - margin;
        }

        private static void DoEject(VehiclePawn vehicle, Pawn pawn)
        {
            if (!vehicle.Spawned || vehicle.Map == null) return;

            Lord lord = vehicle.GetLord();

            if (!vehicle.RemovePawn(pawn)) return;

            if (lord != null && !lord.ownedPawns.Contains(pawn))
                lord.AddPawn(pawn);

            Map     map       = vehicle.Map;
            var     hoverComp = vehicle.GetComp<CompVehicleHover>();
            IntVec3 hoverCell = hoverComp != null
                ? new IntVec3(Mathf.RoundToInt(hoverComp.realPos.x - 0.5f), 0, Mathf.RoundToInt(hoverComp.realPos.y - 0.5f))
                : vehicle.Position;

            IntVec3 dropCell = IntVec3.Invalid;

            CellRect rect = GenAdj.OccupiedRect((Thing)vehicle).ExpandedBy(2);
            foreach (IntVec3 c in rect.EdgeCells)
            {
                if (!c.InBounds(map)) continue;
                if (!c.Standable(map)) continue;
                if (IsUnderBlockingRoofStatic(c, map)) continue;
                if (c.GetThingList(map).Any(t => t is Pawn)) continue;
                dropCell = c;
                break;
            }

            if (!dropCell.IsValid)
            {
                foreach (IntVec3 c in GenRadial.RadialCellsAround(hoverCell, 6, false))
                {
                    if (!c.InBounds(map)) continue;
                    if (!c.Standable(map)) continue;
                    if (IsUnderBlockingRoofStatic(c, map)) continue;
                    dropCell = c;
                    break;
                }
            }

            if (!dropCell.IsValid)
                dropCell = hoverCell.IsValid && hoverCell.InBounds(map) ? hoverCell : vehicle.Position;

            AirdropDef airdropDef = DefDatabase<AirdropDef>.GetNamedSilentFail("VRF_HoverEjectParatrooper");
            if (airdropDef != null)
            {
                try
                {
                    AirdropSkyfaller skyfaller = (AirdropSkyfaller)ThingMaker.MakeThing((ThingDef)airdropDef, null);
                    if (pawn.Spawned) pawn.DeSpawn(DestroyMode.Vanish);
                    skyfaller.innerContainer.TryAddOrTransfer((Thing)pawn, true);
                    GenSpawn.Spawn((Thing)skyfaller, dropCell, map, WipeMode.Vanish);
                }
                catch (Exception ex)
                {
                    Log.Warning("[VRF] Eject parachute failed: " + ex.Message);
                    if (!pawn.Spawned) GenSpawn.Spawn((Thing)pawn, dropCell, map, WipeMode.Vanish);
                }
            }
            else
            {
                if (!pawn.Spawned) GenSpawn.Spawn((Thing)pawn, dropCell, map, WipeMode.Vanish);
            }
        }

        private bool IsUnderBlockingRoof(IntVec3 cell)
            => IsUnderBlockingRoofStatic(cell, map);

        private static bool IsUnderBlockingRoofStatic(IntVec3 cell, Map map)
        {
            if (!cell.InBounds(map)) return false;
            RoofDef roof = map.roofGrid.RoofAt(cell);
            if (roof == null) return false;
            return HoverRoofUtil.IsBlockingRoof(roof);
        }

        private IntVec3 FindOpenPositionNear(IntVec3 center, int radius)
        {
            int maxCells = radius * radius * 2;
            int checked_ = 0;
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(center, radius, false))
            {
                if (checked_++ > maxCells) break;
                if (!cell.InBounds(map)) continue;
                if (IsUnderBlockingRoof(cell)) continue;
                if (!cell.Standable(map)) continue;
                return cell;
            }
            return IntVec3.Invalid;
        }

        private Vector2 ClampToMap(Vector2 pos)
        {
            const float margin = 3f;
            pos.x = Mathf.Clamp(pos.x, margin, map.Size.x - margin);
            pos.y = Mathf.Clamp(pos.y, margin, map.Size.z - margin);
            return pos;
        }

        /// <summary>
        /// True if the vehicle has integrated weapons (Vehicle Framework turrets built into the
        /// vehicle itself). Turrets inside a vehicle interior map (e.g. gravship) do NOT count.
        /// </summary>
        private static bool HasIntegratedWeapons(VehiclePawn vehicle)
        {
            var turretComp = vehicle.CompVehicleTurrets;
            return turretComp != null && turretComp.Turrets != null && turretComp.Turrets.Count > 0;
        }

        /// <summary>
        /// True if the thing is an animal that belongs to the player colony (including
        /// trained/owned colony animals).
        /// </summary>
        private static bool IsColonyAnimal(Thing t)
        {
            return t is Pawn p && p.Spawned && p.RaceProps.Animal
                && p.Faction == Faction.OfPlayer;
        }

        private Thing FindBestTarget(VehiclePawn vehicle, CompVehicleHover hoverComp, float maxDistance = -1f)
        {
            var targets = vehicle.Map.attackTargetsCache.TargetsHostileToFaction(vehicle.Faction);
            if (targets == null || targets.Count == 0) return null;

            Vector2 hoverPos     = hoverComp.realPos;
            Thing   bestOpen     = null;
            float   bestOpenSq   = float.MaxValue;
            Thing   bestRoofed   = null;
            float   bestRoofedSq = float.MaxValue;
            float   maxDistSq    = maxDistance > 0f ? maxDistance * maxDistance : float.MaxValue;

            foreach (var t in targets)
            {
                Thing thing = t.Thing;
                if (thing == null || thing.Destroyed || !thing.Spawned) continue;
                if (thing is Pawn p && (p.Dead || p.Downed)) continue;

                // Hover vehicle without integrated weapons never targets colony animals
                if (!HasIntegratedWeapons(vehicle) && IsColonyAnimal(thing)) continue;

                if (thing.Map.fogGrid.IsFogged(thing.Position)) continue;

                float dSq = (hoverPos - new Vector2(thing.DrawPos.x, thing.DrawPos.z)).sqrMagnitude;
                if (dSq > maxDistSq) continue;

                if (IsUnderBlockingRoof(thing.Position))
                {
                    if (dSq < bestRoofedSq) { bestRoofedSq = dSq; bestRoofed = thing; }
                }
                else
                {
                    if (dSq < bestOpenSq) { bestOpenSq = dSq; bestOpen = thing; }
                }
            }

            return bestOpen ?? bestRoofed;
        }

    }
}
