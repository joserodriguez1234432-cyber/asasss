using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;
using Vehicles;
using HarmonyLib;
using Verse.AI.Group;

namespace VehicleRaidFramework
{
    public static class CrewManager
    {
        /// <summary>
        /// Returns true if this vehicle is a Gravship (VehiclePawnWithMap from Vehicle Map Framework).
        /// Gravships use their own crew/AI system and must not be affected by ReassignCrew,
        /// CheckAbandonment, or the hover-crew patches.
        /// </summary>
        public static bool IsGravshipVehicle(VehiclePawn vehicle)
        {
            if (vehicle == null) return false;
            return vehicle is global::VehicleMapFramework.VehiclePawnWithMap;
        }

        public static void ReassignCrew(VehiclePawn vehicle)
        {
            if (vehicle == null || vehicle.Faction == null || vehicle.Faction.IsPlayer || !vehicle.Spawned) return;
            if (IsGravshipVehicle(vehicle)) return;
            if (vehicle.VehicleDef.type == VehicleType.Air
                && vehicle.GetComp<VehicleRaid.CompVehicleHover>() == null
                && !VRF_TransportUtil.IsSiegeDropVehicle(vehicle))
                return;

            var handlers = vehicle.handlers;
            if (handlers == null || handlers.Count == 0) return;

            List<Pawn> consciousPawns = new List<Pawn>();
            List<Pawn> downedPawns = new List<Pawn>();

            foreach (var h in handlers)
            {
                foreach (Pawn p in h.thingOwner)
                {
                    if (p == null || p.Dead) continue;
                    if (p.Downed) downedPawns.Add(p);
                    else consciousPawns.Add(p);
                }
            }

            foreach (var h in handlers)
            {
                h.thingOwner.Clear();
            }

            var movementHandlers = new List<VehicleRoleHandler>();
            var turretHandlers = new List<VehicleRoleHandler>();
            var otherHandlers = new List<VehicleRoleHandler>();
            foreach (var h in handlers)
            {
                if (h?.role == null) continue;
                bool isMovement = (h.role.HandlingTypes & HandlingType.Movement) != 0;
                bool isTurret = (h.role.HandlingTypes & HandlingType.Turret) != 0;
                if (isMovement) movementHandlers.Add(h);
                else if (isTurret) turretHandlers.Add(h);
                else otherHandlers.Add(h);
            }
            movementHandlers.Sort((a, b) => b.role.SlotsToOperate.CompareTo(a.role.SlotsToOperate));

            DistributePawns(consciousPawns, movementHandlers);
            DistributePawns(consciousPawns, turretHandlers);
            DistributePawns(consciousPawns, otherHandlers);

            DistributePawns(downedPawns, otherHandlers);
            DistributePawns(downedPawns, turretHandlers);
            DistributePawns(downedPawns, movementHandlers);

            SyncLord(vehicle);
            CheckRetreat(vehicle);
        }

        public static void SyncLord(VehiclePawn vehicle)
        {
            if (vehicle == null || vehicle.Faction == null || vehicle.Faction.IsPlayer || !vehicle.Spawned) return;

            Lord vLord = vehicle.GetLord();
            foreach (Pawn occupant in vehicle.AllPawnsAboard)
            {
                if (occupant.Dead || occupant.Downed) continue;
                Lord pLord = occupant.GetLord();
                if (pLord != null && pLord != vLord && pLord.LordJob is LordJob_VehicleRaid)
                {
                    if (vLord != null) vLord.RemovePawn(vehicle);
                    pLord.AddPawn(vehicle);
                    return;
                }
            }
        }

        private static void DistributePawns(List<Pawn> pawns, List<VehicleRoleHandler> targetHandlers)
        {
            int index = 0;
            foreach (var h in targetHandlers)
            {
                if (h?.role == null) continue;
                while (index < pawns.Count && h.thingOwner.Count < h.role.Slots)
                {
                    Pawn p = pawns[index];
                    index++;
                    if (p != null) h.thingOwner.TryAdd(p);
                }
            }
            if (index > 0) pawns.RemoveRange(0, index);
        }

        private static Dictionary<VehiclePawn, int> loneDriverTicks = new Dictionary<VehiclePawn, int>();
        private static int lastCleanupTick = 0;

        public static void CheckRetreat(VehiclePawn vehicle)
        {
            if (Find.TickManager.TicksGame > lastCleanupTick + 60000)
            {
                var keysToRemove = loneDriverTicks.Keys.Where(k => k == null || k.Destroyed || !k.Spawned).ToList();
                foreach (var k in keysToRemove) loneDriverTicks.Remove(k);
                lastCleanupTick = Find.TickManager.TicksGame;
            }

            if (vehicle == null || vehicle.Faction == null || vehicle.Faction.IsPlayer || !vehicle.Spawned) return;
            if (IsGravshipVehicle(vehicle)) return;
            if (vehicle.VehicleDef.type == VehicleType.Air
                && vehicle.GetComp<VehicleRaid.CompVehicleHover>() == null
                && !VRF_TransportUtil.IsSiegeDropVehicle(vehicle))
                return;

            DutyDef vDuty = vehicle.mindState?.duty?.def;
            if (vDuty != null && (vDuty == VRF_AIDutyDefs.ExitMap || vDuty == DutyDefOf.ExitMapBest)) return;

            Lord lord = vehicle.GetLord();
            if (lord?.LordJob is LordJob_VehicleTrade || lord?.LordJob is LordJob_HelicopterTrade) return;

            if (IsOutOfAmmo(vehicle))
            {
                TriggerRetreat(vehicle, "MessageVRF_VehicleRetreatingNoAmmo");
                return;
            }

            int totalConscious = vehicle.AllPawnsAboard.Count(p => !p.Dead && !p.Downed);
            if (totalConscious != 1)
            {
                loneDriverTicks.Remove(vehicle);
            }

            if (VRF_TransportUtil.IsTransportVehicle(vehicle)) return;

            if (totalConscious == 1 && HasOperationalDriver(vehicle))
            {
                if (!loneDriverTicks.ContainsKey(vehicle))
                {
                    loneDriverTicks[vehicle] = Find.TickManager.TicksGame;
                    return;
                }

                if (Find.TickManager.TicksGame - loneDriverTicks[vehicle] < 120)
                {
                    return;
                }

                bool isDesignedForMore = vehicle.handlers.Any(h => h.role != null && (h.role.HandlingTypes & HandlingType.Movement) == 0 && h.role.Slots > 0);
                if (!isDesignedForMore)
                {
                    return;
                }

                bool hasPrioritySlots = vehicle.handlers.Any(h => h.role != null && (h.role.HandlingTypes & (HandlingType.Movement | HandlingType.Turret)) != 0 && h.thingOwner.Count < h.role.Slots);
                if (hasPrioritySlots && (AnyFriendlyInfantryNearby(vehicle) || IsAnyPawnBoarding(vehicle)))
                {
                    return;
                }

                TriggerRetreat(vehicle, "MessageVRF_VehicleRetreating");
                return;
            }
        }

        private static void TriggerRetreat(VehiclePawn vehicle, string messageKey)
        {
            if (vehicle.mindState == null)
            {
                vehicle.mindState = new Verse.AI.Pawn_MindState(vehicle);
            }
            vehicle.mindState.duty = new PawnDuty(VRF_AIDutyDefs.ExitMap ?? DutyDefOf.ExitMapBest);
            Messages.Message(messageKey.Translate(vehicle.LabelShort), vehicle, MessageTypeDefOf.NeutralEvent);
        }







        public static bool IsOutOfAmmo(VehiclePawn vehicle)
        {
            CompVehicleTurrets turretComp = vehicle.CompVehicleTurrets;
            if (turretComp == null) return false;

            bool hasAnyAmmoTurret = false;

            foreach (VehicleTurret turret in turretComp.Turrets)
            {

                if (turret.def.ammunition == null) continue;

                hasAnyAmmoTurret = true;

                if (turret.shellCount > 0) return false;

                foreach (Thing item in vehicle.inventory.innerContainer)
                {
                    if (turret.def.ammunition.Allows(item.def))
                    {


                        if (item.stackCount >= turret.def.chargePerAmmoCount)
                        {
                            return false;
                        }
                    }
                }
            }


            return hasAnyAmmoTurret;
        }

        private static Pawn FindCandidateInHandlers(IEnumerable<VehicleRoleHandler> handlers)
        {
            foreach (var h in handlers)
            {
                foreach (Pawn p in h.thingOwner)
                {
                    if (p != null && !p.Dead && !p.Downed) return p;
                }
            }
            return null;
        }

        private static void SwapSeat(VehiclePawn vehicle, Pawn pawn, VehicleRoleHandler targetHandler)
        {
            foreach (var h in vehicle.handlers)
            {
                if (h.thingOwner.Contains(pawn))
                {
                    h.thingOwner.Remove(pawn);
                    break;
                }
            }
            targetHandler.thingOwner.TryAdd(pawn);
        }

        public static bool CanMove(VehiclePawn vehicle)
        {
            if (!vehicle.CanMove) return false;

            if (TDMS_Compatibility.IsAutonomousVehicle(vehicle))
            {
                var fuelComp2 = vehicle.GetComp<CompFueledTravel>();
                if (fuelComp2 != null && fuelComp2.Fuel <= 0 && !HasFuelInInventory(vehicle)) return false;
                return true;
            }

            if (!vehicle.HasEnoughOperators) return false;

            var fuelComp = vehicle.GetComp<CompFueledTravel>();
            if (fuelComp != null && fuelComp.Fuel <= 0 && !HasFuelInInventory(vehicle)) return false;

            if (!HasOperationalDriver(vehicle)) return false;

            return true;
        }

        public static bool HasOperationalDriver(VehiclePawn vehicle)
        {
            if (TDMS_Compatibility.IsAutonomousVehicle(vehicle)) return true;

            if (vehicle.handlers == null) return false;
            foreach (var h in vehicle.handlers)
            {
                if (h.role != null && (h.role.HandlingTypes & HandlingType.Movement) != 0)
                {
                    foreach (var thing in h.thingOwner)
                    {
                        if (thing is Pawn p && !p.Dead && !p.Downed) return true;
                    }
                }
            }
            return false;
        }

        private static bool HasFuelInInventory(VehiclePawn vehicle)
        {
            foreach (Thing fuel in CompFueledTravel.AllFuelFromInventory(vehicle))
            {
                return true;
            }
            return false;
        }

        public static void CheckAbandonment(VehiclePawn vehicle)
        {
            if (!vehicle.AllPawnsAboard.Any()) return;

            Lord lord = vehicle.GetLord();
            if (lord?.LordJob is LordJob_VehicleTrade || lord?.LordJob is LordJob_HelicopterTrade) return;

            if (VRF_TransportUtil.IsSiegeDropVehicle(vehicle)) return;
            if (IsGravshipVehicle(vehicle)) return;

            var hoverComp = vehicle.GetComp<VehicleRaid.CompVehicleHover>();
            if (hoverComp != null && hoverComp.IsAirborne) return;

            if (!CanMove(vehicle))
            {
                if (IsCriticallyFailing(vehicle))
                {
                    AbandonVehicle(vehicle);
                }
            }
        }

        private static bool IsCriticallyFailing(VehiclePawn vehicle)
        {
            if (TDMS_Compatibility.IsAutonomousVehicle(vehicle))
            {
                if (!vehicle.CanMove) return true;
                var fuelComp2 = vehicle.GetComp<CompFueledTravel>();
                if (fuelComp2 != null && fuelComp2.Fuel <= 0 && !HasFuelInInventory(vehicle)) return true;
                return false;
            }

            if (!vehicle.CanMove) return true;

            if (!HasOperationalDriver(vehicle)) return true;

            var fuelComp = vehicle.GetComp<CompFueledTravel>();
            if (fuelComp != null && fuelComp.Fuel <= 0 && !HasFuelInInventory(vehicle))
            {
                return true;
            }

            return false;
        }

        public static bool HasFunctionalEngine(VehiclePawn vehicle)
        {
            if (vehicle.statHandler?.components == null) return true;

            foreach (var part in vehicle.statHandler.components)
            {
                if (part.Health <= 0 && part.props?.tags != null && (part.props.tags.Contains("engine") || part.props.tags.Contains("fuel_tank") || part.props.tags.Contains("transmission")))
                {
                    return false;
                }
            }
            return true;
        }

        public static void AbandonVehicle(VehiclePawn vehicle)
        {
            if (!vehicle.Spawned || vehicle.Dead) return;

            var crew = vehicle.AllPawnsAboard.ToList();
            if (!crew.Any()) return;

            Messages.Message("MessageVRF_VehicleAbandoned".Translate(vehicle.LabelShort), vehicle, MessageTypeDefOf.NegativeEvent);

            for (int i = crew.Count - 1; i >= 0; i--)
            {
                Pawn p = crew[i];
                vehicle.DisembarkPawn(p);

                if (p.Dead || p.Downed || !p.Spawned) continue;
                SyncDisembarkedPawnLord(p, vehicle);
                p.jobs.StopAll();
            }
        }

        public static void SyncDisembarkedPawnLord(Pawn pawn, VehiclePawn vehicle)
        {
            if (pawn == null || vehicle == null || vehicle.Map == null) return;

            Lord targetLord = null;
            foreach (Lord lord in vehicle.Map.lordManager.lords)
            {
                if (lord.faction == vehicle.Faction && !(lord.LordJob is LordJob_VehicleRaid))
                {
                    bool isNaturalRaid = lord.LordJob != null && 
                        (lord.LordJob.GetType().Name.Contains("AssaultColony") || 
                         lord.LordJob.GetType().Name.Contains("Raid") || 
                         lord.LordJob is LordJob_DefendBase);
                    if (isNaturalRaid)
                    {
                        targetLord = lord;
                        break;
                    }
                }
            }

            if (targetLord == null)
            {
                targetLord = vehicle.GetLord();
            }

            if (targetLord != null)
            {
                pawn.GetLord()?.RemovePawn(pawn);
                targetLord.AddPawn(pawn);
            }
        }

        public static bool AnyFriendlyInfantryNearby(VehiclePawn vehicle)
        {
            if (!vehicle.Spawned || vehicle.Map == null) return false;

            float radius = 50f;
            float radiusSq = radius * radius;

            foreach (Pawn p in vehicle.Map.mapPawns.AllPawnsSpawned)
            {
                if (p != vehicle && p.Faction == vehicle.Faction && !p.Dead && !p.Downed && p.Spawned && !(p is VehiclePawn))
                {
                    if (p.Position.DistanceToSquared(vehicle.Position) <= radiusSq)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsAnyPawnBoarding(VehiclePawn vehicle)
        {
            if (vehicle?.Map == null || !vehicle.Spawned) return false;
            var mapPawns = vehicle.Map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < mapPawns.Count; i++)
            {
                Pawn p = mapPawns[i];
                if (p == vehicle || p is VehiclePawn || p.Dead || p.Downed || !p.Spawned) continue;
                if (p.CurJob != null && p.CurJob.def == VRF_AIDutyDefs.Board && p.CurJob.targetA.Thing == vehicle)
                {
                    return true;
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static class Patch_CrewCasualty
    {
        public static void Postfix(Pawn __instance)
        {
            if (__instance.ParentHolder is VehicleRoleHandler handler)
            {
                if (CrewManager.IsGravshipVehicle(handler.vehicle)) return;
                if (handler.vehicle?.VehicleDef?.type == VehicleType.Air
                    && handler.vehicle?.GetComp<VehicleRaid.CompVehicleHover>() == null
                    && !VRF_TransportUtil.IsSiegeDropVehicle(handler.vehicle))
                    return;
                CrewManager.ReassignCrew(handler.vehicle);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
    public static class Patch_CrewDowned
    {
        public static void Postfix(Pawn_HealthTracker __instance, Pawn ___pawn)
        {
            Pawn pawn = ___pawn;
            if (pawn?.ParentHolder is VehicleRoleHandler handler)
            {
                if (CrewManager.IsGravshipVehicle(handler.vehicle)) return;
                if (handler.vehicle?.VehicleDef?.type == VehicleType.Air
                    && handler.vehicle?.GetComp<VehicleRaid.CompVehicleHover>() == null
                    && !VRF_TransportUtil.IsSiegeDropVehicle(handler.vehicle))
                    return;
                CrewManager.ReassignCrew(handler.vehicle);
            }
        }
    }

    [HarmonyPatch(typeof(KidnapAIUtility), nameof(KidnapAIUtility.TryFindGoodKidnapVictim))]
    public static class Patch_KidnapAIUtility_VehicleIgnore
    {
        public static bool Prefix(Pawn kidnapper, ref bool __result, ref Pawn victim)
        {
            if (kidnapper is VehiclePawn || kidnapper?.health?.capacities == null)
            {
                victim = null;
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KidnapAIUtility), nameof(KidnapAIUtility.ReachableWoundedGuest))]
    public static class Patch_ReachableWoundedGuest_VehicleIgnore
    {
        public static bool Prefix(Pawn searcher, ref Pawn __result)
        {
            if (searcher is VehiclePawn || searcher?.health?.capacities == null)
            {
                __result = null;
                return false;
            }
            return true;
        }
    }
}
