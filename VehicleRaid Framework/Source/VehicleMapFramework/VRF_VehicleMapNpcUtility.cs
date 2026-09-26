using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Vehicles;

namespace VehicleRaidFramework.VehicleMapFramework
{
    /// <summary>
    /// Adds the stationary crew that VMF's regular NPC incident path normally obtains
    /// from its prefab definitions.  Raid presets are already-built interiors instead
    /// of VMF prefabs, so they need an equivalent pass after their interior is restored.
    /// </summary>
    public static class VRF_VehicleMapNpcUtility
    {
        private const float LowFuelExitPercent = 0.10f;
        private const int LoneDriverExitDelayTicks = 120;

        private static readonly Dictionary<global::VehicleMapFramework.VehiclePawnWithMap, int> loneDriverSinceTick =
            new Dictionary<global::VehicleMapFramework.VehiclePawnWithMap, int>();
        private static int lastLoneDriverCleanupTick;

        /// <summary>
        /// Creates one hostile gunner for every vanilla-compatible manned turret in a
        /// VehicleMap.  Building_TurretGun is deliberately used as the compatibility
        /// boundary: vanilla mortars and third-party turrets derived from it all use
        /// the game's JobDefOf.ManTurret job, while arbitrary CompMannable buildings
        /// may require a completely different job driver.
        /// </summary>
        public static int SpawnInteriorTurretCrew(global::VehicleMapFramework.VehiclePawnWithMap vehicle)
        {
            if (vehicle == null || vehicle.Destroyed || vehicle.Dead || vehicle.Faction == null)
                return 0;

            Map interiorMap = vehicle.VehicleMap;
            if (interiorMap == null || interiorMap.Disposed)
                return 0;

            Faction faction = vehicle.Faction;
            List<Building_TurretGun> turrets = interiorMap.listerThings
                .ThingsInGroup(ThingRequestGroup.BuildingArtificial)
                .OfType<Building_TurretGun>()
                .Where(t => IsMannableTurretForFaction(t, faction, interiorMap))
                .ToList();

            if (turrets.Count == 0)
                return 0;

            // This is the same vanilla LordJob used by SymbolResolver_MannedMortar.
            // It gives the NPCs the ManClosestTurret duty so they resume manning after
            // combat/job interruptions, rather than behaving like free passengers.
            Lord turretLord = interiorMap.lordManager.lords.FirstOrDefault(l =>
                l.faction == faction && l.LordJob is LordJob_ManTurrets);
            if (turretLord == null)
                turretLord = LordMaker.MakeNewLord(faction, new LordJob_ManTurrets(), interiorMap);

            int spawned = 0;
            foreach (Building_TurretGun turret in turrets)
            {
                if (HasAssignedGunner(turret, faction))
                    continue;

                Pawn gunner = GenerateCapableGunner(faction, interiorMap);
                if (gunner == null)
                    continue;

                if (!TryFindSpawnCell(turret, interiorMap, out IntVec3 spawnCell))
                {
                    gunner.Destroy();
                    continue;
                }

                GenSpawn.Spawn(gunner, spawnCell, interiorMap, Rot4.South);
                turretLord.AddPawn(gunner);

                // Assign the exact turret now.  The LordJob above maintains the duty
                // afterwards and can select a replacement if the original is lost.
                gunner.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.ManTurret, turret));
                spawned++;
            }

            if (spawned > 0)
            {
                turretLord.CurLordToil?.UpdateAllDuties();
                VRF_Log.Msg($"Added {spawned} interior turret crew member(s) to VMF vehicle '{vehicle.LabelShort}'.");
            }

            return spawned;
        }

        /// <summary>
        /// VMF's stock NPC incident code gets its crew from CompNpcVehicleMap prefab data.
        /// Preset raids restore an already-built interior instead, so this mirrors the
        /// important runtime behavior: keep a driver assigned from the interior crew and
        /// make the vehicle withdraw if only that driver remains or fuel is critically low.
        /// </summary>
        public static void MaintainVehicleMapCrew(global::VehicleMapFramework.VehiclePawnWithMap vehicle)
        {
            if (vehicle == null || vehicle.Destroyed || vehicle.Dead || !vehicle.Spawned ||
                vehicle.Faction == null || vehicle.Faction.IsPlayer)
                return;

            Map interiorMap = vehicle.VehicleMap;
            if (interiorMap == null || interiorMap.Disposed)
                return;

            RemoveStaleLoneDriverEntries();
            EnsureInteriorPawnDrives(vehicle, interiorMap);

            if (!CrewManager.HasOperationalDriver(vehicle))
            {
                loneDriverSinceTick.Remove(vehicle);
                Patch_VehicleNPCOnOff.UpdateVehiclePower(vehicle);
                return;
            }

            if (ShouldLeaveForLowFuel(vehicle))
            {
                RequestMapExit(vehicle, "MessageVRF_VehicleRetreatingLowFuel");
                Patch_VehicleNPCOnOff.UpdateVehiclePower(vehicle);
                return;
            }

            int consciousCrew = CountConsciousCrew(vehicle, interiorMap);
            if (consciousCrew == 1)
            {
                if (!loneDriverSinceTick.TryGetValue(vehicle, out int sinceTick))
                {
                    loneDriverSinceTick[vehicle] = Find.TickManager.TicksGame;
                }
                else if (Find.TickManager.TicksGame - sinceTick >= LoneDriverExitDelayTicks)
                {
                    RequestMapExit(vehicle, "MessageVRF_VehicleRetreating");
                }
            }
            else
            {
                loneDriverSinceTick.Remove(vehicle);
            }

            Patch_VehicleNPCOnOff.UpdateVehiclePower(vehicle);
        }

        /// <summary>
        /// Gives an empty movement role an NPC from the interior map. Pawns not using a
        /// turret are preferred; an active turret gunner is taken only when it is the
        /// only remaining eligible crew member.
        /// </summary>
        private static bool EnsureInteriorPawnDrives(
            global::VehicleMapFramework.VehiclePawnWithMap vehicle,
            Map interiorMap)
        {
            if (CrewManager.HasOperationalDriver(vehicle) || vehicle.handlers == null)
                return false;

            List<VehicleRoleHandler> movementHandlers = vehicle.handlers
                .Where(h => h?.role != null && (h.role.HandlingTypes & HandlingType.Movement) != 0 &&
                            !h.RoleFulfilled)
                .OrderByDescending(h => h.AreSlotsAvailable)
                .ToList();
            if (movementHandlers.Count == 0)
                return false;

            List<Pawn> interiorPawns = interiorMap.mapPawns.AllPawnsSpawned
                .Where(p => IsEligibleInteriorDriver(p, vehicle.Faction, interiorMap))
                .ToList();

            // First preserve every artillery position that can be preserved.
            foreach (bool isTurretGunner in new[] { false, true })
            {
                foreach (Pawn pawn in interiorPawns.Where(p => IsManningTurret(p) == isTurretGunner))
                {
                    VehicleRoleHandler handler = movementHandlers.FirstOrDefault(h =>
                        h.CanOperateRole(pawn) && (h.AreSlotsAvailable || HasDisplaceableOccupant(h)));
                    if (handler == null)
                        continue;

                    if (!MakeMovementSlotAvailable(vehicle, handler, interiorMap))
                        continue;

                    Lord oldLord = pawn.GetLord();
                    oldLord?.RemovePawn(pawn);
                    pawn.jobs?.EndCurrentJob(JobCondition.InterruptForced, startNewJob: false);

                    if (!vehicle.TryAddPawn(pawn, handler))
                    {
                        // This should only fail if another system reserved the seat during
                        // this tick. Restore the pawn to the interior instead of losing it.
                        if (!pawn.Spawned && pawn.holdingOwner == null && TryFindFreeInteriorCell(interiorMap, out IntVec3 retryCell))
                            GenSpawn.Spawn(pawn, retryCell, interiorMap, Rot4.South);
                        oldLord?.AddPawn(pawn);
                        continue;
                    }

                    vehicle.CompVehicleTurrets?.RecacheTurretPermissions();
                    return true;
                }
            }

            return false;
        }

        private static bool IsEligibleInteriorDriver(Pawn pawn, Faction faction, Map interiorMap)
        {
            return pawn != null && pawn.Spawned && pawn.Map == interiorMap && pawn.Faction == faction &&
                !(pawn is VehiclePawn) && !pawn.Dead && !pawn.Downed && !pawn.InMentalState &&
                pawn.RaceProps.Humanlike && pawn.RaceProps.ToolUser && !pawn.IsPrisoner &&
                pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) &&
                pawn.health.capacities.CapableOf(PawnCapacityDefOf.Consciousness);
        }

        private static bool IsManningTurret(Pawn pawn)
        {
            return pawn?.CurJobDef == JobDefOf.ManTurret;
        }

        private static bool HasDisplaceableOccupant(VehicleRoleHandler handler)
        {
            if (handler?.thingOwner == null)
                return false;

            foreach (Pawn pawn in handler.thingOwner)
            {
                if (pawn == null || !handler.CanOperateRole(pawn))
                    return true;
            }
            return false;
        }

        private static bool MakeMovementSlotAvailable(
            global::VehicleMapFramework.VehiclePawnWithMap vehicle,
            VehicleRoleHandler handler,
            Map interiorMap)
        {
            if (handler.AreSlotsAvailable)
                return true;

            Pawn displaced = null;
            foreach (Pawn pawn in handler.thingOwner)
            {
                if (pawn == null || !handler.CanOperateRole(pawn))
                {
                    displaced = pawn;
                    break;
                }
            }
            if (displaced == null || !vehicle.TryRemovePawn(displaced, handler))
                return false;

            // A downed or otherwise incapable former driver belongs in the interior,
            // not outside on the ground map. Dead pawns stay in cargo to avoid deleting
            // a corpse when their interior has no valid floor cell.
            if (displaced.Dead)
            {
                vehicle.inventory.innerContainer.TryAdd(displaced);
                return handler.AreSlotsAvailable;
            }

            if (TryFindFreeInteriorCell(interiorMap, out IntVec3 cell))
                GenSpawn.Spawn(displaced, cell, interiorMap, Rot4.South);
            else
                vehicle.inventory.innerContainer.TryAdd(displaced);

            return handler.AreSlotsAvailable;
        }

        private static bool TryFindFreeInteriorCell(Map map, out IntVec3 cell)
        {
            return CellFinder.TryFindRandomCell(map,
                c => c.Standable(map) && c.GetFirstPawn(map) == null,
                out cell);
        }

        private static bool ShouldLeaveForLowFuel(global::VehicleMapFramework.VehiclePawnWithMap vehicle)
        {
            CompFueledTravel fuel = vehicle.GetComp<CompFueledTravel>();
            if (fuel == null || fuel.Props == null || fuel.Props.ElectricPowered || fuel.FuelCapacity <= 0f)
                return false;

            // Cargo fuel can still be used by the existing refuel logic, so do not order
            // an exit while the vehicle still has a way to replenish itself.
            return fuel.FuelPercent <= LowFuelExitPercent && !CompFueledTravel.AllFuelFromInventory(vehicle).Any();
        }

        private static int CountConsciousCrew(global::VehicleMapFramework.VehiclePawnWithMap vehicle, Map interiorMap)
        {
            HashSet<Pawn> crew = new HashSet<Pawn>();
            foreach (Pawn pawn in vehicle.AllPawnsAboard)
            {
                if (IsConsciousCrewPawn(pawn, vehicle.Faction))
                    crew.Add(pawn);
            }

            foreach (Pawn pawn in interiorMap.mapPawns.AllPawnsSpawned)
            {
                if (IsConsciousCrewPawn(pawn, vehicle.Faction) && pawn.Map == interiorMap && !(pawn is VehiclePawn))
                    crew.Add(pawn);
            }

            return crew.Count;
        }

        private static bool IsConsciousCrewPawn(Pawn pawn, Faction faction)
        {
            return pawn != null && pawn.Faction == faction && !pawn.Dead && !pawn.Downed &&
                pawn.RaceProps.Humanlike && pawn.RaceProps.ToolUser;
        }

        private static void RequestMapExit(global::VehicleMapFramework.VehiclePawnWithMap vehicle, string messageKey)
        {
            DutyDef exitDuty = VRF_AIDutyDefs.ExitMap ?? DutyDefOf.ExitMapBest;
            if (vehicle.mindState?.duty?.def == exitDuty || vehicle.mindState?.duty?.def == DutyDefOf.ExitMapBest)
                return;

            if (vehicle.mindState == null)
                vehicle.mindState = new Pawn_MindState(vehicle);
            vehicle.mindState.duty = new PawnDuty(exitDuty);
            vehicle.jobs?.EndCurrentJob(JobCondition.InterruptForced, startNewJob: true);
            Messages.Message(messageKey.Translate(vehicle.LabelShort), vehicle, MessageTypeDefOf.NeutralEvent);
        }

        private static void RemoveStaleLoneDriverEntries()
        {
            if (Find.TickManager.TicksGame < lastLoneDriverCleanupTick + 60000)
                return;

            foreach (global::VehicleMapFramework.VehiclePawnWithMap vehicle in loneDriverSinceTick.Keys
                         .Where(v => v == null || v.Destroyed || !v.Spawned)
                         .ToList())
            {
                loneDriverSinceTick.Remove(vehicle);
            }

            lastLoneDriverCleanupTick = Find.TickManager.TicksGame;
        }

        private static bool IsMannableTurretForFaction(Building_TurretGun turret, Faction faction, Map map)
        {
            if (turret == null || turret.Destroyed || !turret.Spawned || turret.Map != map)
                return false;
            if (turret.Faction != null && turret.Faction != faction)
                return false;
            if (!turret.def.hasInteractionCell || turret.GetComp<CompMannable>() == null)
                return false;

            IntVec3 interactionCell = turret.InteractionCell;
            return interactionCell.InBounds(map) && interactionCell.Standable(map);
        }

        private static bool HasAssignedGunner(Building_TurretGun turret, Faction faction)
        {
            CompMannable mannable = turret.GetComp<CompMannable>();
            Pawn current = mannable?.ManningPawn;
            if (current != null && current.Faction == faction && !current.Dead)
                return true;

            return turret.Map.mapPawns.AllPawnsSpawned.Any(p =>
                p.Faction == faction && !p.Dead && !p.Downed &&
                p.CurJobDef == JobDefOf.ManTurret && p.CurJob != null &&
                p.CurJob.targetA.Thing == turret);
        }

        private static Pawn GenerateCapableGunner(Faction faction, Map map)
        {
            // Match the generation context used by VMF's and RimWorld's hostile NPCs.
            // A few faction pawn kinds can still be incapable of violence, so retry
            // before giving up rather than putting an invalid pawn in a turret role.
            for (int attempt = 0; attempt < 6; attempt++)
            {
                PawnKindDef kind = faction.RandomPawnKind() ?? faction.def.basicMemberKind;
                if (kind == null || kind.RaceProps == null || !kind.RaceProps.ToolUser)
                    continue;

                Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                    kind,
                    faction,
                    PawnGenerationContext.NonPlayer,
                    map.Tile,
                    forceGenerateNewPawn: true,
                    mustBeCapableOfViolence: true,
                    inhabitant: true));

                if (CanManTurret(pawn))
                    return pawn;

                pawn?.Destroy();
            }

            return null;
        }

        private static bool CanManTurret(Pawn pawn)
        {
            return pawn != null && pawn.RaceProps.ToolUser && !pawn.Dead && !pawn.Downed &&
                !pawn.WorkTagIsDisabled(WorkTags.Violent) &&
                pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) &&
                pawn.health.capacities.CapableOf(PawnCapacityDefOf.Consciousness);
        }

        private static bool TryFindSpawnCell(Building_TurretGun turret, Map map, out IntVec3 cell)
        {
            IntVec3 interactionCell = turret.InteractionCell;
            if (interactionCell.InBounds(map) && interactionCell.Standable(map) &&
                interactionCell.GetFirstPawn(map) == null)
            {
                cell = interactionCell;
                return true;
            }

            return CellFinder.TryFindRandomCellNear(
                interactionCell,
                map,
                6,
                c => c.Standable(map) && c.GetFirstPawn(map) == null,
                out cell);
        }
    }
}
