using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Vehicles;
using VehicleRaid;

namespace VehicleRaidFramework
{
    public static class VRF_SettlementSpawnHelper
    {

        private static readonly HashSet<int> InjectedMapIds = new HashSet<int>();

        public static bool TryMarkInjected(Map map)
        {
            if (map == null) return false;
            return InjectedMapIds.Add(map.uniqueID);
        }

        public static void ClearInjected(Map map)
        {
            if (map != null)
                InjectedMapIds.Remove(map.uniqueID);
        }

        public static CellRect GetFallbackRect(Map map, Faction faction)
        {
            if (MapGenerator.TryGetVar<CellRect>("SettlementRect", out CellRect stored) && stored.Width > 0)
                return stored;

            int bMinX = int.MaxValue, bMinZ = int.MaxValue;
            int bMaxX = int.MinValue, bMaxZ = int.MinValue;
            bool foundAny = false;

            foreach (Building b in map.listerBuildings.allBuildingsColonist)
            {
                if (b.Faction != faction) continue;
                if (!b.Spawned) continue;
                IntVec3 pos = b.Position;
                if (pos.x < bMinX) bMinX = pos.x;
                if (pos.z < bMinZ) bMinZ = pos.z;
                if (pos.x > bMaxX) bMaxX = pos.x;
                if (pos.z > bMaxZ) bMaxZ = pos.z;
                foundAny = true;
            }

            if (foundAny)
            {
                return new CellRect(bMinX, bMinZ, bMaxX - bMinX + 1, bMaxZ - bMinZ + 1);
            }

            int halfW = map.Size.x / 4;
            int halfH = map.Size.z / 4;
            return CellRect.CenteredOn(map.Center, halfW, halfH);
        }

        public static void SpawnVehiclesDeferred(
            Map map,
            CellRect settlementRect,
            Faction faction,
            List<(PawnKindDef kind, VRF_NaturalRaidVehicleEntry entry, VehicleDef vdef)> toSpawn)
        {
            if (map == null || map.Disposed) return;

            List<VehiclePawn> spawnedVehicles = new List<VehiclePawn>();

            var injectedHoverList = new List<(VehicleDef vdef, CompProperties_VehicleHover props)>();

            try
            {
            foreach (var (spawnKind, spawnEntry, spawnVDef) in toSpawn)
            {
                if (spawnEntry.helicopterMode && spawnVDef.type == VehicleType.Air && !spawnEntry.isSiegeDrop)
                {
                    bool alreadyHasHover = spawnVDef.comps.Any(c => c is CompProperties_VehicleHover);
                    if (!alreadyHasHover)
                    {
                        bool isAirplane = spawnEntry.airVehicleType == "Airplane";
                                bool isGravship = spawnEntry.airVehicleType == "Gravship";
                        CompProperties_VehicleHover injected;

                        if (isAirplane)
                        {
                            VehicleDef warbirdDef = DefDatabase<VehicleDef>.GetNamedSilentFail("VVE_WarbirdNPC")
                                                 ?? DefDatabase<VehicleDef>.GetNamedSilentFail("VVE_Warbird");
                            CompProperties_VehicleHover refProps = warbirdDef?.comps
                                .OfType<CompProperties_VehicleHover>().FirstOrDefault();

                            injected = refProps != null
                                ? new CompProperties_VehicleHover
                                {
                                    flightType            = FlightType.Airplane,
                                    maxTicks              = refProps.maxTicks,
                                    maxTicksVertical      = refProps.maxTicksVertical,
                                    maxTicksPropeller     = refProps.maxTicksPropeller,
                                    hoverAltitude         = refProps.hoverAltitude,
                                    hoverShadowOffset     = refProps.hoverShadowOffset,
                                    hoverMoveSpeed        = isAirplane ? spawnEntry.helicopterMoveSpeed : (isGravship ? VehicleMapFramework.VRF_GravshipSpeedUtility.CalculatePresetSpeed(spawnEntry.gravshipPresetName) : spawnEntry.helicopterMoveSpeed),
                                    hoverRotationSpeed    = refProps.hoverRotationSpeed,
                                    shadowAlphaPropellerCurve = refProps.shadowAlphaPropellerCurve,
                                    xPositionCurve        = refProps.xPositionCurve,
                                    zPositionCurve        = refProps.zPositionCurve,
                                    rotationCurve         = refProps.rotationCurve,
                                    runwayClearCells      = refProps.runwayClearCells,
                                    landingMaxTicks       = refProps.landingMaxTicks,
                                    landingForwardCurve   = refProps.landingForwardCurve,
                                    landingAltitudeCurve  = refProps.landingAltitudeCurve,
                                    landingRotationCurve  = refProps.landingRotationCurve,
                                }
                                : new CompProperties_VehicleHover
                                {
                                    flightType            = FlightType.Airplane,
                                    maxTicks              = 300,
                                    maxTicksVertical      = 300,
                                    maxTicksPropeller     = 300,
                                    hoverAltitude         = 0.5f,
                                    hoverShadowOffset     = 1.5f,
                                    hoverMoveSpeed        = isAirplane ? spawnEntry.helicopterMoveSpeed : (isGravship ? VehicleMapFramework.VRF_GravshipSpeedUtility.CalculatePresetSpeed(spawnEntry.gravshipPresetName) : spawnEntry.helicopterMoveSpeed),
                                    hoverRotationSpeed    = 60f,
                                    runwayClearCells      = 30,
                                    landingMaxTicks       = 600,
                                };
                        }
                        else
                        {
                            VehicleDef mosquitoDef = DefDatabase<VehicleDef>.GetNamedSilentFail("VVE_MosquitoNPC");
                            CompProperties_VehicleHover refProps = mosquitoDef?.comps
                                .OfType<CompProperties_VehicleHover>().FirstOrDefault();

                            injected = refProps != null
                                ? new CompProperties_VehicleHover
                                {
                                    maxTicks                  = refProps.maxTicks,
                                    maxTicksVertical          = refProps.maxTicksVertical,
                                    maxTicksPropeller         = refProps.maxTicksPropeller,
                                    hoverAltitude             = refProps.hoverAltitude,
                                    hoverShadowOffset         = refProps.hoverShadowOffset,
                                    hoverBobAmount            = refProps.hoverBobAmount,
                                    hoverBobSpeed             = refProps.hoverBobSpeed,
                                    hoverMoveSpeed            = spawnEntry.helicopterMoveSpeed,
                                    angularVelocityPropeller  = refProps.angularVelocityPropeller,
                                    rotationCurve             = refProps.rotationCurve,
                                    rotationVerticalCurve     = refProps.rotationVerticalCurve,
                                    zPositionVerticalCurve    = refProps.zPositionVerticalCurve,
                                    xPositionVerticalCurve    = refProps.xPositionVerticalCurve,
                                    shadowAlphaPropellerCurve = refProps.shadowAlphaPropellerCurve,
                                    fleckDataVertical         = refProps.fleckDataVertical,
                                    fleckDataPropeller        = refProps.fleckDataPropeller
                                }
                                : new CompProperties_VehicleHover
                                {
                                    maxTicks          = 600,
                                    maxTicksVertical  = 400,
                                    maxTicksPropeller = 800,
                                    hoverAltitude     = 4f,
                                    hoverShadowOffset = 1.5f,
                                    hoverBobAmount    = 0.22f,
                                    hoverBobSpeed     = 2.0f,
                                    hoverMoveSpeed    = spawnEntry.helicopterMoveSpeed,
                                    angularVelocityPropeller = new SmashTools.BezierCurve(
                                        new List<CurvePoint>
                                        {
                                            new CurvePoint(0f, 0f), new CurvePoint(0.3f, 0f),
                                            new CurvePoint(0.5f, 30f), new CurvePoint(1f, 59f)
                                        })
                                };
                        }

                        spawnVDef.comps.Add(injected);
                        injectedHoverList.Add((spawnVDef, injected));

                        if (!VehicleMod.settings.vehicles.vehicleStats.ContainsKey(spawnVDef.defName))
                            VehicleMod.settings.vehicles.vehicleStats[spawnVDef.defName] =
                                new Dictionary<string, float>();
                        VehicleMod.settings.vehicles.vehicleStats[spawnVDef.defName]
                            [VehicleStatDefOf.MoveSpeed.defName] = isAirplane ? spawnEntry.helicopterMoveSpeed : (isGravship ? VehicleMapFramework.VRF_GravshipSpeedUtility.CalculatePresetSpeed(spawnEntry.gravshipPresetName) : 4.5f);
                    }
                }

                if (!TryFindSettlementSpawnCell(map, settlementRect, spawnVDef, out IntVec3 cell, out Rot4 spawnRot))
                    continue;

                VehiclePawn vehicle = VehicleSpawner.GenerateVehicle(spawnVDef, faction);
                if (vehicle == null) continue;

                VehicleRaidUtility.ApplyColorConfig(vehicle,
                    new VehicleColorConfig { mode = VehicleColorMode.Faction }, faction);

                if (!spawnEntry.upgradeLoadouts.NullOrEmpty())
                {
                    VRF_UpgradeLoadout chosenLoadout = spawnEntry.upgradeLoadouts.RandomElement();
                    var upgradeComp = vehicle.CompUpgradeTree;
                    if (upgradeComp != null && !chosenLoadout.nodeKeys.NullOrEmpty())
                    {
                        foreach (string key in chosenLoadout.nodeKeys)
                        {
                            UpgradeNode node = upgradeComp.Props.def?.GetNode(key);
                            if (node != null && !upgradeComp.NodeUnlocked(node))
                                upgradeComp.FinishUnlock(node);
                        }
                    }
                }

                if (vehicle.Handlers != null)
                {
                    List<Pawn> crewList = new List<Pawn>();
                    foreach (VehicleRoleHandler handler in vehicle.Handlers)
                    {
                        if (handler.role == null || handler.role.Slots <= 0) continue;
                        VehicleCrewUtility.FillRole(vehicle, handler, null, faction, map, crewList);
                    }
                }

                CompFueledTravel fuelComp = vehicle.GetComp<CompFueledTravel>();
                if (fuelComp != null)
                {
                    fuelComp.ConsumeFuel(fuelComp.Fuel);
                    float targetFuel = fuelComp.FuelCapacity * (spawnEntry.fuelPercent / 100f);
                    if (targetFuel > 0f) fuelComp.Refuel(targetFuel);
                }

                if (vehicle.CompVehicleTurrets != null)
                {
                    float cargoCapacity = vehicle.VehicleDef.GetStatValueAbstract(VehicleStatDefOf.CargoCapacity);
                    foreach (VehicleTurret turret in vehicle.CompVehicleTurrets.Turrets)
                    {
                        if (turret?.def?.ammunition == null) continue;
                        ThingDef ammoDef = turret.def.ammunition.AllowedThingDefs.FirstOrDefault();
                        if (ammoDef == null) continue;

                        var tEntry = spawnEntry.GetOrCreateTurretAmmo(turret.def.defName);
                        if (tEntry.ammoPercent <= 0f) continue;

                        float ammoMass = ammoDef.GetStatValueAbstract(StatDefOf.Mass);
                        if (ammoMass <= 0f) ammoMass = 0.1f;
                        float targetKg = cargoCapacity * (tEntry.ammoPercent / 100f);
                        int count = Mathf.FloorToInt(targetKg / ammoMass);
                        if (count > 0)
                        {
                            Thing ammo = ThingMaker.MakeThing(ammoDef);
                            ammo.stackCount = count;
                            vehicle.inventory.innerContainer.TryAdd(ammo);
                        }
                    }
                }

                GenSpawn.Spawn(vehicle, cell, map, spawnRot);
                Patch_VehicleNPCOnOff.UpdateVehiclePower(vehicle);

                var hoverComp = vehicle.GetComp<VehicleRaid.CompVehicleHover>();
                if (hoverComp != null && hoverComp.State == VehicleRaid.HoverState.Grounded)
                {
                    hoverComp.ActivateHoverNPC();
                    // Gravships activate hover but are never registered as transports
                    if (!CrewManager.IsGravshipVehicle(vehicle))
                    {
                        if (VRF_TransportUtil.IsTransportVehicle(vehicle) && vehicle.AllPawnsAboard.Count > 1)
                            HoverNPC_TransportManager.GetFor(map)?.RegisterVehicle(vehicle);
                        else if (!VRF_TransportUtil.IsTransportVehicle(vehicle) && vehicle.AllPawnsAboard.Count > 1)
                            HoverNPC_TransportManager.GetFor(map)?.RegisterArmedVehicle(vehicle);
                    }
                }

                spawnedVehicles.Add(vehicle);
            }
            }
            finally
            {
                foreach (var (vdef, hprops) in injectedHoverList)
                    vdef.comps.Remove(hprops);
            }

            if (spawnedVehicles.Count == 0) return;

            var leaderManager = map.GetComponent<VRF_LeaderManager>();
            foreach (VehiclePawn v in spawnedVehicles)
            {
                var turretComp = v.CompVehicleTurrets;
                if (turretComp == null) continue;
                bool isMortar = turretComp.Props.deployTime > 0f;
                if (!isMortar && turretComp.Turrets != null)
                {
                    foreach (var turret in turretComp.Turrets)
                    {
                        if (turret.ProjectileDef?.projectile?.flyOverhead == true)
                        { isMortar = true; break; }
                    }
                }
                if (isMortar) leaderManager?.RegisterSettlementMortar(v);
            }

            Lord defendLord = map.lordManager.lords
                .FirstOrDefault(l => l.faction == faction
                                  && l.LordJob is LordJob_DefendBase);

            const int SafetyHoldTicks = 10000000;

            LordJob_VehicleRaid settlementJob = new LordJob_VehicleRaid(
                faction,
                stayTicks: 35000000,
                behavior:  VRF_NaturalRaidBehavior.HoldThenAssault,
                holdTicks: SafetyHoldTicks);

            settlementJob.naturalRaidLord = defendLord;

            Lord vrfLord = LordMaker.MakeNewLord(faction, settlementJob, map,
                spawnedVehicles.Cast<Pawn>().ToList());

            leaderManager?.NotifyRaidStarted();
            vrfLord.CurLordToil?.UpdateAllDuties();
            DelayedDutyRefresh.Schedule(vrfLord, map);
        }

        public static bool TryFindSettlementSpawnCell(
            Map map,
            CellRect settlementRect,
            VehicleDef vehicleDef,
            out IntVec3 foundCell,
            out Rot4 foundRot)
        {
            Rot4[] rotations = { Rot4.North, Rot4.East, Rot4.South, Rot4.West };
            CellRect searchArea = settlementRect.ExpandedBy(15);

            for (int attempt = 0; attempt < 100; attempt++)
            {
                IntVec3 c = searchArea.RandomCell;
                if (!c.InBounds(map) || settlementRect.Contains(c)) continue;

                foreach (Rot4 rot in rotations)
                {
                    CellRect vehicleRect = vehicleDef.VehicleRect(c, rot);
                    CellRect padded      = vehicleRect.ExpandedBy(1);

                    if (!padded.InBounds(map)) continue;

                    bool valid = true;
                    foreach (IntVec3 cell in padded.Cells)
                    {
                        if (!IsCellValidForVehicle(cell, vehicleDef, map))
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (valid)
                    {
                        foundCell = c;
                        foundRot  = rot;
                        return true;
                    }
                }
            }

            foundCell = IntVec3.Invalid;
            foundRot  = Rot4.North;
            return false;
        }

        public static bool IsCellValidForVehicle(IntVec3 cell, VehicleDef vehicleDef, Map map)
        {
            if (!cell.InBounds(map)) return false;
            if (cell.Roofed(map)) return false;

            TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
            if (terrain == null) return false;
            if (terrain.passability == Traversability.Impassable) return false;
            if (terrain.IsWater) return false;

            if (map.edificeGrid[cell] != null) return false;

            List<Thing> things = map.thingGrid.ThingsListAt(cell);
            for (int i = 0; i < things.Count; i++)
            {
                Thing t = things[i];
                if (t == null || !t.Spawned) continue;
                if (t.def.passability == Traversability.Impassable) return false;
                if (t.def.fillPercent > 0.4f) return false;
            }

            int cost = VehiclePathGrid.CalculatePathCostFor(vehicleDef, map, cell);
            return cost < VehiclePathGrid.ImpassableCost;
        }
    }
}
