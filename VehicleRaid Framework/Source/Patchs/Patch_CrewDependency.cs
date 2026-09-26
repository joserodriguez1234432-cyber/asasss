using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Vehicles;
using UnityEngine;
using VehicleRaid;

namespace VehicleRaidFramework
{
    [HarmonyPatch(typeof(VehiclePawn), "Tick")]
    public static class Patch_RaidVehicle_CrewDependency
    {
        private static readonly List<Pawn> tmpToDisembark = new List<Pawn>();
        private static readonly List<Pawn> tmpDriverDisembark = new List<Pawn>();

        static void Postfix(VehiclePawn __instance)
        {
            if (__instance == null || !__instance.Spawned || __instance.Destroyed || __instance.Map == null) return;
            if (__instance.Faction == null || __instance.Faction.IsPlayer) return;

            // Only run on tick intervals 30 or 60, avoiding 58 out of 60 ticks of overhead completely
            bool tick30 = __instance.IsHashIntervalTick(30);
            bool tick60 = __instance.IsHashIntervalTick(60);
            if (!tick30 && !tick60) return;

            Lord lord = __instance.GetLord();
            bool isVRFLord = lord?.LordJob is LordJob_VehicleRaid || lord?.LordJob is LordJob_VehicleTrade || lord?.LordJob is LordJob_HelicopterTrade;
            
            if (!isVRFLord)
            {
                bool isNaturalRaid = lord != null && (lord.LordJob is LordJob_AssaultColony || lord.LordJob is LordJob_DefendBase ||
                    lord.LordJob.GetType().Name.Contains("AssaultColony") || lord.LordJob.GetType().Name.Contains("Raid"));
                if (!isNaturalRaid) return;

                if (__instance.GetComp<VehicleRaid.CompVehicleHover>() == null)
                {
                    var settings = VRF_Mod.Settings;
                    if (settings == null) return;

                    var factionConfig = settings.factionConfigs?.Find(c => c.factionDefName == __instance.Faction?.def?.defName);
                    if (factionConfig == null) return;

                    var entry = factionConfig.vehicleEntries?.Find(e => e.vehicleKindDefName == __instance.VehicleDef.defName && e.enabled);
                    if (entry == null) return;
                }
            }

            if (tick30)
            {
                Patch_VehicleNPCOnOff.UpdateVehiclePower(__instance);
            }

            if (!tick60) return;

            // Vehicle Map Framework keeps its spare crew inside a separate interior map.
            // Its pawns cannot be handled by the regular VehicleCrewUtility, which only
            // looks at vehicle handlers, so run the dedicated VMF transfer/retreat pass.
            if (__instance is global::VehicleMapFramework.VehiclePawnWithMap vehicleWithMap)
            {
                VehicleMapFramework.VRF_VehicleMapNpcUtility.MaintainVehicleMapCrew(vehicleWithMap);
                return;
            }

            if (__instance.VehicleDef.type == VehicleType.Air && __instance.GetComp<VehicleRaid.CompVehicleHover>() == null)
            {
                if (VRF_TransportUtil.IsSiegeDropVehicle(__instance))
                {
                    VRF_SiegeDropBehavior.ReassignSiegeDropCrew(__instance);

                    if (__instance.IsHashIntervalTick(300) && lord?.LordJob is LordJob_VehicleRaid
                        && lord.CurLordToil is LordToil_VehicleSearchAndDestroy
                        && Find.TickManager.TicksGame - __instance.TickSpawned >= 300)
                    {
                        CheckRaidLordExitSync(__instance, lord);
                    }
                }
                return;
            }

            CrewManager.ReassignCrew(__instance);
            CrewManager.CheckAbandonment(__instance);
            CrewManager.CheckRetreat(__instance);

            if (VRF_TransportUtil.IsTransportVehicle(__instance) || VRF_TransportUtil.IsArmedTransportVehicle(__instance))
            {
                if (lord?.CurLordToil is LordToil_VehicleExitMap exitToil)
                {
                    CheckTransportExitDuty(__instance, exitToil);
                }
                else
                {
                    HandleTransportDisembark(__instance);
                }
            }

            if (lord != null)
            {
                if (__instance.IsHashIntervalTick(1250))
                {
                    FeedCrewFromInventory(__instance);
                    RefuelFromInventory(__instance);
                }

                HandleOverlappingPawns(__instance);

                if (!__instance.AllPawnsAboard.Any() && (__instance.Dead || __instance.Destroyed))
                {
                    lord.Notify_PawnLost(__instance, (PawnLostCondition)3, null);
                }

                if (__instance.IsHashIntervalTick(300) && lord.LordJob is LordJob_VehicleRaid)
                {
                    if (lord.CurLordToil is LordToil_VehicleSearchAndDestroy)
                    {
                        CheckRaidLordExitSync(__instance, lord);
                    }
                    else if (lord.CurLordToil is LordToil_VehicleExitMap)
                    {
                        CheckRaidLordStartExit(__instance, lord);
                    }
                }
            }
        }

        private static void CheckRaidLordStartExit(VehiclePawn vehicle, Lord vehLord)
        {
            Map map = vehicle.Map;
            List<Lord> lords = map.lordManager.lords;
            for (int i = 0; i < lords.Count; i++)
            {
                Lord otherLord = lords[i];
                if (otherLord == vehLord) continue;
                if (otherLord.faction != vehLord.faction) continue;
                if (otherLord.LordJob is LordJob_VehicleRaid) continue;

                bool isNaturalRaid = otherLord.LordJob != null && 
                    (otherLord.LordJob is LordJob_AssaultColony ||
                     otherLord.LordJob is LordJob_DefendBase ||
                     otherLord.LordJob.GetType().Name.Contains("AssaultColony") || 
                     otherLord.LordJob.GetType().Name.Contains("Raid"));
                if (!isNaturalRaid) continue;

                bool alreadyExiting = false;
                LordToil curToil = otherLord.CurLordToil;
                if (curToil != null)
                {
                    if (curToil is LordToil_PanicFlee || curToil is LordToil_ExitMap || curToil is LordToil_TakeWoundedGuest)
                    {
                        alreadyExiting = true;
                    }
                    else
                    {
                        string toilType = curToil.GetType().Name;
                        if (toilType.Contains("Exit") || toilType.Contains("Leave") ||
                            toilType.Contains("Flee") || toilType.Contains("Escape") ||
                            toilType.Contains("Steal") || toilType.Contains("Kidnap"))
                        {
                            alreadyExiting = true;
                        }
                    }
                }

                if (!alreadyExiting && otherLord.Graph?.lordToils != null)
                {
                    LordToil newLordToil = null;
                    List<LordToil> toils = otherLord.Graph.lordToils;
                    for (int j = 0; j < toils.Count; j++)
                    {
                        if (toils[j] is LordToil_PanicFlee)
                        {
                            newLordToil = toils[j];
                            break;
                        }
                    }

                    if (newLordToil != null)
                    {
                        otherLord.GotoToil(newLordToil);
                    }
                    else
                    {
                        for (int j = 0; j < toils.Count; j++)
                        {
                            LordToil st = toils[j];
                            if (st is LordToil_ExitMap || st.GetType().Name.Contains("Exit") || 
                                st.GetType().Name.Contains("Leave") || st.GetType().Name.Contains("Flee"))
                            {
                                otherLord.GotoToil(st);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private static void CheckRaidLordExitSync(VehiclePawn vehicle, Lord vehLord)
        {
            Map map = vehicle.Map;
            List<Lord> lords = map.lordManager.lords;
            for (int i = 0; i < lords.Count; i++)
            {
                Lord otherLord = lords[i];
                if (otherLord == vehLord) continue;
                if (otherLord.faction != vehLord.faction) continue;
                if (otherLord.LordJob is LordJob_VehicleRaid) continue;

                int livingActiveCount = 0;
                List<Pawn> otherPawns = otherLord.ownedPawns;
                for (int j = 0; j < otherPawns.Count; j++)
                {
                    Pawn p = otherPawns[j];
                    if (p.Dead || p.Downed || !p.Spawned || p.Map != map) continue;
                    if (p.ParentHolder is VehicleRoleHandler) continue;
                    livingActiveCount++;
                }

                if (livingActiveCount == 0) continue;

                bool shouldExit = false;
                LordToil curToil = otherLord.CurLordToil;
                if (curToil != null)
                {
                    if (curToil is LordToil_PanicFlee || curToil is LordToil_ExitMap || curToil is LordToil_TakeWoundedGuest)
                    {
                        shouldExit = true;
                    }
                    else
                    {
                        string toilType = curToil.GetType().Name;
                        if (toilType.Contains("Exit") || toilType.Contains("Leave") ||
                            toilType.Contains("Flee") || toilType.Contains("Escape") ||
                            toilType.Contains("Steal") || toilType.Contains("Kidnap"))
                        {
                            shouldExit = true;
                        }
                    }
                }

                if (!shouldExit)
                {
                    for (int j = 0; j < otherPawns.Count; j++)
                    {
                        Pawn p = otherPawns[j];
                        if (p.Dead || p.Downed || !p.Spawned || p.Map != map) continue;
                        if (p.ParentHolder is VehicleRoleHandler) continue;
                        DutyDef duty = p.mindState?.duty?.def;
                        if (duty != null)
                        {
                            string dName = duty.defName;
                            if (dName.Contains("Exit") || dName.Contains("Leave") ||
                                dName.Contains("Flee") || dName == "ExitMapBest" ||
                                dName == "ExitMapRandom" || dName == "ExitMapNear")
                            {
                                shouldExit = true;
                                break;
                            }
                        }
                    }
                }

                if (shouldExit)
                {
                    vehLord.ReceiveMemo("RaidNaturalExit");
                    return;
                }
            }
        }

        private static void CheckTransportExitDuty(VehiclePawn vehicle, LordToil_VehicleExitMap exitToil)
        {
            if (exitToil.ExitToilStartTick >= 0 &&
                Find.TickManager.TicksGame - exitToil.ExitToilStartTick < LordToil_VehicleExitMap.MinTicksBeforeExit)
                return;

            int totalPassengerSlots = 0;
            int boardedPassengers = 0;
            for (int i = 0; i < vehicle.handlers.Count; i++)
            {
                var handler = vehicle.handlers[i];
                if (handler?.role == null) continue;
                bool isPassengerSlot = (handler.role.HandlingTypes & HandlingType.Movement) == 0 &&
                                       (handler.role.HandlingTypes & HandlingType.Turret) == 0;
                if (!isPassengerSlot) continue;
                totalPassengerSlots += handler.role.Slots;
                boardedPassengers += handler.thingOwner.Count;
            }

            bool allBoarded = totalPassengerSlots > 0 && boardedPassengers >= totalPassengerSlots;

            bool anyInfantryOnMap = false;
            Lord lord = vehicle.GetLord();
            if (lord != null && !allBoarded)
            {
                for (int i = 0; i < lord.ownedPawns.Count; i++)
                {
                    Pawn p = lord.ownedPawns[i];
                    if (p is VehiclePawn) continue;
                    if (p.Dead || p.Downed) continue;
                    if (!p.Spawned || p.Map != vehicle.Map) continue;
                    if (p.ParentHolder is VehicleRoleHandler) continue;
                    if (p.Position.DistanceToSquared(vehicle.Position) > 3600) continue; // 60 * 60
                    anyInfantryOnMap = true;
                    break;
                }
            }

            DutyDef targetDuty;
            if (allBoarded || !anyInfantryOnMap)
            {
                targetDuty = VRF_DutyDefOf.VRF_VehicleExitMap ?? DefDatabase<DutyDef>.GetNamedSilentFail("VRF_VehicleExitMap") ?? DutyDefOf.ExitMapBest;
            }
            else if (VRF_TransportUtil.IsArmedTransportVehicle(vehicle))
            {
                targetDuty = VRF_DutyDefOf.VRF_VehicleArmedTransport ?? DefDatabase<DutyDef>.GetNamedSilentFail("VRF_VehicleArmedTransport");
            }
            else
            {
                targetDuty = VRF_DutyDefOf.VRF_VehicleTransport ?? DefDatabase<DutyDef>.GetNamedSilentFail("VRF_VehicleTransport");
            }

            if (targetDuty != null && vehicle.mindState.duty?.def != targetDuty)
                vehicle.mindState.duty = new PawnDuty(targetDuty);
        }

        private static void HandleTransportDisembark(VehiclePawn vehicle)
        {
            float detectionRadius = VRF_TransportUtil.GetVehicleCombatRadius(vehicle);
            if (!VRF_TransportUtil.HasEnemy(vehicle, detectionRadius)) return;

            if (!VRF_TransportUtil.HasEnemy(vehicle, 32f))
                return;

            bool isUnarmedTransport = VRF_TransportUtil.IsTransportVehicle(vehicle);

            bool hasPawnsToDisembark = false;
            for (int i = 0; i < vehicle.handlers.Count; i++)
            {
                var handler = vehicle.handlers[i];
                if (handler?.role == null) continue;
                bool isPassengerSlot = (handler.role.HandlingTypes & HandlingType.Movement) == 0 &&
                                       (handler.role.HandlingTypes & HandlingType.Turret) == 0;
                if (!isPassengerSlot && !isUnarmedTransport) continue;
                for (int j = 0; j < handler.thingOwner.Count; j++)
                {
                    if (handler.thingOwner[j] is Pawn p && !p.Dead && !p.Downed)
                    {
                        hasPawnsToDisembark = true;
                        break;
                    }
                }
                if (hasPawnsToDisembark) break;
            }

            if (!hasPawnsToDisembark) return;

            Lord lord = vehicle.GetLord();
            bool isExiting = lord?.CurLordToil is LordToil_VehicleExitMap;
            if (isExiting) return;

            tmpToDisembark.Clear();
            for (int i = 0; i < vehicle.handlers.Count; i++)
            {
                var handler = vehicle.handlers[i];
                if (handler?.role == null) continue;
                bool isPassengerSlot = (handler.role.HandlingTypes & HandlingType.Movement) == 0 &&
                                       (handler.role.HandlingTypes & HandlingType.Turret) == 0;
                if (!isPassengerSlot && !isUnarmedTransport) continue;
                for (int j = 0; j < handler.thingOwner.Count; j++)
                {
                    if (!(handler.thingOwner[j] is Pawn p) || p.Dead || p.Downed) continue;
                    DutyDef pDuty = p.mindState?.duty?.def;
                    if (pDuty != null && (pDuty == VRF_DutyDefOf.VRF_InfantryExit ||
                        pDuty.defName == "VRF_InfantryExit")) continue;
                    tmpToDisembark.Add(p);
                }
            }

            if (tmpToDisembark.Count == 0) return;

            Map map = vehicle.Map;
            CellRect vehicleRect = vehicle.OccupiedRect();
            Thing nearestEnemy = FindNearestEnemy(vehicle);

            for (int i = 0; i < tmpToDisembark.Count; i++)
            {
                Pawn pawn = tmpToDisembark[i];
                IntVec3 exitCell = IntVec3.Invalid;

                // Step 1: Look at adjacent cells directly around vehicle boundary
                foreach (IntVec3 adj in vehicleRect.AdjacentCells)
                {
                    if (adj.InBounds(map) && adj.Standable(map) && !vehicleRect.Contains(adj))
                    {
                        exitCell = adj;
                        break;
                    }
                }

                // Step 2: Fallback expanding 1-3 cells
                if (!exitCell.IsValid)
                {
                    for (int radius = 1; radius <= 3 && !exitCell.IsValid; radius++)
                    {
                        foreach (IntVec3 cell in GenRadial.RadialCellsAround(vehicle.Position, radius, false))
                        {
                            if (!cell.InBounds(map) || !cell.Standable(map) || vehicleRect.Contains(cell)) continue;
                            exitCell = cell;
                            break;
                        }
                    }
                }

                if (!exitCell.IsValid) continue;

                VRF_TransportUtil.LastDisembarkTick[pawn.thingIDNumber] = Find.TickManager.TicksGame;
                vehicle.DisembarkPawn(pawn);

                if (!pawn.Spawned && pawn.ParentHolder == null) continue;

                CrewManager.SyncDisembarkedPawnLord(pawn, vehicle);

                DutyDef dutyDef =
                    VRF_DutyDefOf.VRF_InfantryAssault_Transport ??
                    DefDatabase<DutyDef>.GetNamedSilentFail("VRF_InfantryAssault_Transport") ??
                    VRF_DutyDefOf.VRF_InfantryAssault ??
                    DefDatabase<DutyDef>.GetNamedSilentFail("VRF_InfantryAssault");

                pawn.mindState.duty = nearestEnemy != null
                    ? new PawnDuty(dutyDef, nearestEnemy.Position)
                    : new PawnDuty(dutyDef);

                pawn.jobs?.StopAll();
            }
            tmpToDisembark.Clear();
        }

        private static Thing FindNearestEnemy(VehiclePawn vehicle)
        {
            var targets = vehicle.Map.attackTargetsCache.TargetsHostileToFaction(vehicle.Faction);
            if (targets == null || targets.Count == 0) return null;
            Thing best = null;
            float bestDistSq = float.MaxValue;
            foreach (var target in targets)
            {
                Thing thing = target?.Thing;
                if (thing == null || thing.Destroyed || thing.Map == null) continue;
                if (thing.Map.fogGrid.IsFogged(thing.Position)) continue;
                if (thing is Pawn p && (p.Dead || p.Downed)) continue;
                float d = thing.Position.DistanceToSquared(vehicle.Position);
                if (d < bestDistSq) { bestDistSq = d; best = thing; }
            }
            return best;
        }

        private static void FeedCrewFromInventory(VehiclePawn vehicle)
        {
            if (vehicle.inventory == null || vehicle.inventory.innerContainer == null || vehicle.inventory.innerContainer.Count == 0) return;

            Thing food = null;
            for (int i = 0; i < vehicle.inventory.innerContainer.Count; i++)
            {
                Thing t = vehicle.inventory.innerContainer[i];
                if (t.def.IsIngestible)
                {
                    food = t;
                    break;
                }
            }
            if (food == null) return;

            for (int h = 0; h < vehicle.handlers.Count; h++)
            {
                var handler = vehicle.handlers[h];
                for (int o = 0; o < handler.thingOwner.Count; o++)
                {
                    if (handler.thingOwner[o] is Pawn occupant && occupant.needs?.food != null)
                    {
                        if (occupant.needs.food.CurLevelPercentage < 0.4f)
                        {
                            float nutrition = food.def.ingestible.CachedNutrition;
                            occupant.needs.food.CurLevel += nutrition;
                            food.stackCount--;
                            if (food.stackCount <= 0)
                            {
                                food.Destroy();
                                food = null;
                                for (int i = 0; i < vehicle.inventory.innerContainer.Count; i++)
                                {
                                    Thing t = vehicle.inventory.innerContainer[i];
                                    if (t.def.IsIngestible)
                                    {
                                        food = t;
                                        break;
                                    }
                                }
                                if (food == null) return;
                            }
                        }
                    }
                }
            }
        }

        private static void RefuelFromInventory(VehiclePawn vehicle)
        {
            CompFueledTravel comp = vehicle.GetComp<CompFueledTravel>();
            if (comp == null || comp.Props.ElectricPowered || comp.FuelPercent > 0.10f) return;

            float needed = comp.FuelCapacity - comp.Fuel;
            if (needed <= 0) return;

            int availableFuel = 0;
            var fuelThings = CompFueledTravel.AllFuelFromInventory(vehicle);
            if (fuelThings != null)
            {
                foreach (var t in fuelThings)
                {
                    availableFuel += t.stackCount;
                }
            }

            if (availableFuel > 0)
            {
                int toConsume = Mathf.Min(availableFuel, Mathf.CeilToInt(needed));
                comp.ConsumeFuelFromInventory(toConsume);

                Patch_VehicleNPCOnOff.UpdateVehiclePower(vehicle);
            }
        }

        private static void HandleOverlappingPawns(VehiclePawn vehicle)
        {
            if (vehicle == null || !vehicle.Spawned || vehicle.Destroyed || vehicle.Map == null || vehicle.Faction == null) return;

            var hoverComp = vehicle.GetComp<CompVehicleHover>();
            if (hoverComp != null && hoverComp.IsAirborne)
            {
                return;
            }

            CellRect rect = vehicle.OccupiedRect();
            Map map = vehicle.Map;

            foreach (IntVec3 cell in rect)
            {
                if (!cell.InBounds(map)) continue;

                List<Thing> thingList = cell.GetThingList(map);
                for (int i = thingList.Count - 1; i >= 0; i--)
                {
                    if (thingList[i] is Pawn p && p != vehicle && !(p is VehiclePawn) && !IsPawnAboard(vehicle, p))
                    {
                        ResolvePawnOverlap(vehicle, p);
                    }
                }
            }
        }

        private static bool IsPawnAboard(VehiclePawn vehicle, Pawn p)
        {
            if (vehicle.handlers == null) return false;
            for (int i = 0; i < vehicle.handlers.Count; i++)
            {
                if (vehicle.handlers[i].thingOwner.Contains(p)) return true;
            }
            return false;
        }

        private static void ResolvePawnOverlap(VehiclePawn vehicle, Pawn pawn)
        {
            ApplyOverlapDamage(vehicle, pawn);

            Map map = vehicle.Map;
            IntVec3 bestPos = IntVec3.Invalid;
            float minDistSq = float.MaxValue;
            CellRect vehicleRect = vehicle.OccupiedRect();

            for (int radius = 1; radius <= 3; radius++)
            {
                foreach (IntVec3 cell in GenRadial.RadialCellsAround(pawn.Position, radius, false))
                {
                    if (cell.InBounds(map) && cell.Walkable(map) && !vehicleRect.Contains(cell))
                    {
                        float d = cell.DistanceToSquared(pawn.Position);
                        if (d < minDistSq)
                        {
                            minDistSq = d;
                            bestPos = cell;
                        }
                    }
                }
                if (bestPos.IsValid) break;
            }

            if (bestPos.IsValid)
            {
                pawn.Position = bestPos;
                pawn.Notify_Teleported(true, false);
            }
        }

        private static void ApplyOverlapDamage(VehiclePawn vehicle, Pawn pawn)
        {
            if (pawn.Faction == null || vehicle.Faction == null) return;

            float damageMultiplier = 0f;

            if (pawn.Faction == vehicle.Faction || !pawn.Faction.HostileTo(vehicle.Faction))
            {
                damageMultiplier = 0f;
            }
            else if (vehicle.Faction.RelationKindWith(pawn.Faction) == FactionRelationKind.Neutral)
            {
                damageMultiplier = 0.1f;
            }
            else
            {
                damageMultiplier = 1.0f;
            }

            if (damageMultiplier > 0)
            {
                var damages = VehiclePawn.CalculateImpactDamage(pawn, vehicle, 5f);
                float pawnDamage = damages.pawnDamage * damageMultiplier;

                if (pawnDamage > 0.5f)
                {
                    DamageInfo dinfo = new DamageInfo(DamageDefOf.Blunt, pawnDamage, 0f, -1f, vehicle);
                    pawn.TakeDamage(dinfo);
                }
            }
        }
    }

    [HarmonyPatch(typeof(VehicleActions), nameof(VehicleActions.DisembarkAll))]
    public static class Patch_DisembarkAll_ProtectHoverDriver
    {
        private static readonly List<Pawn> tmpDriverDisembark = new List<Pawn>();

        [HarmonyPrefix]
        public static bool Prefix(VehiclePawn vehicle)
        {
            if (vehicle == null || vehicle.Faction == null || vehicle.Faction.IsPlayer) return true;
            if (!(vehicle.GetLord()?.LordJob is LordJob_VehicleRaid)) return true;

            // Gravships manage their own crew — never force-disembark them
            if (CrewManager.IsGravshipVehicle(vehicle)) return false;

            if (VRF_TransportUtil.IsSiegeDropVehicle(vehicle)) return false;

            var hoverComp = vehicle.GetComp<CompVehicleHover>();
            if (hoverComp == null || hoverComp.State != HoverState.Hovering) return true;

            var turrets = vehicle.CompVehicleTurrets;
            if (turrets != null && turrets.Turrets != null && turrets.Turrets.Count > 0) return true;

            Pawn driver = null;
            for (int i = 0; i < vehicle.handlers.Count; i++)
            {
                var handler = vehicle.handlers[i];
                if (handler?.role == null) continue;
                if ((handler.role.HandlingTypes & HandlingType.Movement) == 0) continue;
                for (int j = 0; j < handler.thingOwner.Count; j++)
                {
                    if (handler.thingOwner[j] is Pawn p && !p.Dead && !p.Downed) { driver = p; break; }
                }
                if (driver != null) break;
            }

            if (driver == null) return true;

            tmpDriverDisembark.Clear();
            var allPawns = vehicle.AllPawnsAboard;
            foreach (Pawn p in allPawns)
            {
                if (p != driver) tmpDriverDisembark.Add(p);
            }

            for (int i = 0; i < tmpDriverDisembark.Count; i++)
                vehicle.DisembarkPawn(tmpDriverDisembark[i]);

            tmpDriverDisembark.Clear();
            return false;
        }
    }
}
