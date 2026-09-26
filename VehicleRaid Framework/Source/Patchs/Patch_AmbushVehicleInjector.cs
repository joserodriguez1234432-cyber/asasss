using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Vehicles;

namespace VehicleRaidFramework
{
    [HarmonyPatch(typeof(CaravanIncidentUtility), "SetupCaravanAttackMap")]
    public static class Patch_CaravanAmbushVehicleInjector
    {
        [HarmonyPostfix]
        public static void Postfix(Map __result, List<Pawn> enemies)
        {
            if (__result == null || __result.Disposed) return;
            if (enemies == null || enemies.Count == 0) return;

            Faction faction = enemies
                .Select(p => p.Faction)
                .FirstOrDefault(f => f != null && f != Faction.OfPlayer);
            if (faction == null) return;

            var factionConfig = VRF_Mod.Settings?.GetFactionConfig(faction.def.defName);
            if (factionConfig == null) return;

            var eligible = new List<(PawnKindDef kind, VRF_NaturalRaidVehicleEntry entry, VehicleDef vdef)>();
            foreach (var e in factionConfig.ambushVehicleEntries.Where(e => e.enabled))
            {
                PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamedSilentFail(e.vehicleKindDefName);
                if (kind == null || !(kind.race is VehicleDef vd)) continue;
                eligible.Add((kind, e, vd));
            }
            if (eligible.Count == 0) return;

            var toSpawn = BuildSpawnList(factionConfig, eligible, factionConfig.ambushBudget);
            if (toSpawn.Count == 0) return;

            SpawnVehiclesOnMap(__result, faction, toSpawn);
        }

        internal static List<(PawnKindDef kind, VRF_NaturalRaidVehicleEntry entry, VehicleDef vdef)> BuildSpawnList(
            VRF_NaturalRaidFactionConfig factionConfig,
            List<(PawnKindDef kind, VRF_NaturalRaidVehicleEntry entry, VehicleDef vdef)> eligible,
            float budget)
        {
            var toSpawn = new List<(PawnKindDef kind, VRF_NaturalRaidVehicleEntry entry, VehicleDef vdef)>();

            foreach (var f in eligible.Where(e => e.entry.forceSpawn))
            {
                toSpawn.Add(f);
                float fc = f.entry.combatPowerOverride > 0f ? f.entry.combatPowerOverride : f.kind.combatPower;
                budget -= fc;
            }

            var budgetEligible = eligible.Where(e => !e.entry.forceSpawn).ToList();
            bool firstPass = true;
            int safetyLimit = 15;

            while (safetyLimit > 0 && budget > 0f)
            {
                var affordable = budgetEligible.Where(e =>
                {
                    float cost = e.entry.combatPowerOverride > 0f ? e.entry.combatPowerOverride : e.kind.combatPower;
                    return firstPass || cost <= budget;
                }).ToList();

                if (affordable.Count == 0) break;

                var picked = affordable.RandomElement();
                float pickedCost = picked.entry.combatPowerOverride > 0f
                    ? picked.entry.combatPowerOverride
                    : picked.kind.combatPower;

                toSpawn.Add(picked);
                budget -= pickedCost;
                firstPass = false;
                safetyLimit--;
            }

            return toSpawn;
        }

        internal static void SpawnVehiclesOnMap(
            Map map,
            Faction faction,
            List<(PawnKindDef kind, VRF_NaturalRaidVehicleEntry entry, VehicleDef vdef)> toSpawn)
        {
            if (map == null || map.Disposed) return;

            List<VehiclePawn> spawnedVehicles = new List<VehiclePawn>();

            foreach (var (spawnKind, spawnEntry, spawnVDef) in toSpawn)
            {
                IntVec3 spawnCell;
                if (!VehicleTrafficManager.TryFindGroupEntryPoint(map, new List<VehicleDef> { spawnVDef }, out spawnCell))
                    spawnCell = CellFinder.RandomEdgeCell(map);

                Rot4 rot = spawnCell.OnEdge(map)
                    ? spawnCell.GetBeginningOfRoadDirection(map)
                    : Rot4.North;

                VehiclePawn vehicle = VehicleSpawner.GenerateVehicle(spawnVDef, faction);
                if (vehicle == null) continue;

                VehicleRaidUtility.ApplyColorConfig(vehicle,
                    new VehicleColorConfig { mode = VehicleColorMode.Faction }, faction);

                if (!spawnEntry.upgradeLoadouts.NullOrEmpty())
                {
                    var chosenLoadout = spawnEntry.upgradeLoadouts.RandomElement();
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
                    float currentFuel = fuelComp.Fuel;
                    if (currentFuel > 0f) fuelComp.ConsumeFuel(currentFuel);
                    float targetFuel = fuelComp.FuelCapacity;
                    if (targetFuel > 0f) fuelComp.Refuel(targetFuel);
                }

                if (vehicle.CompVehicleTurrets != null)
                {
                    float cargoCapacity = vehicle.VehicleDef.GetStatValueAbstract(VehicleStatDefOf.CargoCapacity);

                    var existingAmmo = vehicle.inventory.innerContainer
                        .Where(t => t.def?.projectile != null || t.def?.projectileWhenLoaded != null)
                        .ToList();
                    foreach (Thing old in existingAmmo)
                    {
                        vehicle.inventory.innerContainer.Remove(old);
                        old.Destroy();
                    }

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

                GenSpawn.Spawn(vehicle, spawnCell, map, rot);
                Patch_VehicleNPCOnOff.UpdateVehiclePower(vehicle);
                spawnedVehicles.Add(vehicle);
            }

            if (spawnedVehicles.Count == 0) return;

            Lord existingLord = map.lordManager.lords
                .FirstOrDefault(l => l.faction == faction
                                  && l.LordJob is LordJob_VehicleRaid
                                  && l.CurLordToil is LordToil_VehicleSearchAndDestroy);

            if (existingLord != null)
            {
                foreach (VehiclePawn v in spawnedVehicles)
                    existingLord.AddPawn(v);
                foreach (VehiclePawn v in spawnedVehicles)
                    Patch_VehicleNPCOnOff.UpdateVehiclePower(v);
                existingLord.CurLordToil?.UpdateAllDuties();
                DelayedDutyRefresh.Schedule(existingLord, map);
            }
            else
            {
                LordJob_VehicleRaid vehicleLord = new LordJob_VehicleRaid(faction, 35000);
                Lord newLord = LordMaker.MakeNewLord(faction, vehicleLord, map, spawnedVehicles.Cast<Pawn>().ToList());
                newLord.CurLordToil?.UpdateAllDuties();
                DelayedDutyRefresh.Schedule(newLord, map);
            }
        }
    }

    [HarmonyPatch(typeof(SignalAction_Ambush), "DoAction")]
    public static class Patch_EdgeAmbushVehicleInjector
    {
        [HarmonyPostfix]
        public static void Postfix(SignalAction_Ambush __instance)
        {
            if (__instance.ambushType == SignalActionAmbushType.Manhunters) return;
            if (!__instance.spawnPawnsOnEdge) return;

            Map map = __instance.Map;
            if (map == null || map.Disposed) return;

            Faction faction = map.ParentFaction != null && map.ParentFaction.HostileTo(Faction.OfPlayer)
                ? map.ParentFaction
                : Find.FactionManager.RandomEnemyFaction(allowNonHumanlike: false);

            if (faction == null) return;

            var factionConfig = VRF_Mod.Settings?.GetFactionConfig(faction.def.defName);
            if (factionConfig == null) return;

            var eligible = new List<(PawnKindDef kind, VRF_NaturalRaidVehicleEntry entry, VehicleDef vdef)>();
            foreach (var e in factionConfig.ambushVehicleEntries.Where(e => e.enabled))
            {
                PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamedSilentFail(e.vehicleKindDefName);
                if (kind == null || !(kind.race is VehicleDef vd)) continue;
                eligible.Add((kind, e, vd));
            }
            if (eligible.Count == 0) return;

            var toSpawn = Patch_CaravanAmbushVehicleInjector.BuildSpawnList(factionConfig, eligible, factionConfig.ambushBudget);
            if (toSpawn.Count == 0) return;

            Patch_CaravanAmbushVehicleInjector.SpawnVehiclesOnMap(map, faction, toSpawn);
        }
    }
}
