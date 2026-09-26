using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using SmashTools;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using Vehicles;
using VehicleRaid;

namespace VehicleRaidFramework
{
    [HarmonyPatch(typeof(PawnsArrivalModeWorker_CenterDrop), "Arrive")]
    public static class Patch_CenterDrop_VehicleAirdrop
    {
        [HarmonyPostfix]
        public static void Postfix(List<Pawn> pawns, IncidentParms parms)
        {
            VehicleDropAirdropHandler.TryHandleDropArrival(parms, VehicleDropMode.Center);
        }
    }

    [HarmonyPatch(typeof(PawnsArrivalModeWorker_EdgeDrop), "Arrive")]
    public static class Patch_EdgeDrop_VehicleAirdrop
    {
        [HarmonyPostfix]
        public static void Postfix(List<Pawn> pawns, IncidentParms parms)
        {
            VehicleDropAirdropHandler.TryHandleDropArrival(parms, VehicleDropMode.Edge);
        }
    }

    [HarmonyPatch(typeof(PawnsArrivalModeWorker_EdgeDropGroups), "Arrive")]
    public static class Patch_EdgeDropGroups_VehicleAirdrop
    {
        [HarmonyPostfix]
        public static void Postfix(List<Pawn> pawns, IncidentParms parms)
        {
            VehicleDropAirdropHandler.TryHandleDropArrival(parms, VehicleDropMode.EdgeGroups);
        }
    }

    [HarmonyPatch(typeof(PawnsArrivalModeWorker_RandomDrop), "Arrive")]
    public static class Patch_RandomDrop_VehicleAirdrop
    {
        [HarmonyPostfix]
        public static void Postfix(List<Pawn> pawns, IncidentParms parms)
        {
            VehicleDropAirdropHandler.TryHandleDropArrival(parms, VehicleDropMode.Random);
        }
    }

    [HarmonyPatch(typeof(PawnsArrivalModeWorker_SpecificLocationDrop), "Arrive")]
    public static class Patch_SpecificLocationDrop_VehicleAirdrop
    {
        [HarmonyPostfix]
        public static void Postfix(List<Pawn> pawns, IncidentParms parms)
        {
            VehicleDropAirdropHandler.TryHandleDropArrival(parms, VehicleDropMode.Specific);
        }
    }

    public enum VehicleDropMode { Center, Edge, EdgeGroups, Random, Specific }

    public static class VehicleDropAirdropHandler
    {
        private const int VehicleEdgeMargin    = 8;
        private const int VehicleSafeRadius    = 20;
        private const int SearchAttempts       = 200;
        private const int FallbackAttempts     = 80;
        private const int MinSeparationBetweenDrops = 16;

        public static void TryHandleDropArrival(IncidentParms parms, VehicleDropMode mode)
        {
            if (parms == null || parms.target == null) return;

            if (!Patch_NaturalRaidVehicleInjector.PendingVehicles
                    .TryGetValue(parms.target, out var toSpawn))
                return;

            Patch_NaturalRaidVehicleInjector.PendingVehicles.Remove(parms.target);

            if (toSpawn.NullOrEmpty()) return;

            Map map = parms.target as Map;
            if (map == null) return;

            if (!map.CanAirdropInMap())
            {
                Patch_NaturalRaidVehicleInjector.SpawnVehiclesNormal(map, parms, toSpawn);
                return;
            }

            List<VehiclePawn> spawnedVehicles = new List<VehiclePawn>();
            List<IntVec3> usedCells = new List<IntVec3>();

            foreach (var (spawnKind, spawnEntry, spawnVDef) in toSpawn)
            {
                if (!spawnEntry.isSiegeDrop &&
                    (spawnVDef.type == VehicleType.Air ||
                     spawnVDef.type == VehicleType.Sea ||
                     spawnVDef.comps.Any(c => c is CompProperties_VehicleHover)))
                    continue;

                VehiclePawn vehicle = BuildVehicle(spawnVDef, spawnEntry, parms.faction, map);
                if (vehicle == null) continue;

                if (spawnEntry.isSiegeDrop)
                {
                    IntVec3 landCell = FindSiegeDropCell(map, spawnVDef, parms, usedCells);
                    if (!landCell.IsValid)
                    {
                        vehicle.Destroy();
                        continue;
                    }
                    usedCells.Add(landCell);
                    SpawnSiegeDropLanding(vehicle, map, landCell, parms.faction, parms);
                }
                else
                {
                    IntVec3 dropCell = FindDropCell(map, spawnVDef, parms, mode, usedCells);
                    if (!dropCell.IsValid)
                    {
                        vehicle.Destroy();
                        continue;
                    }
                    usedCells.Add(dropCell);
                    ScheduleAirdrop(vehicle, map, dropCell, parms.faction, parms.spawnRotation);
                }
                spawnedVehicles.Add(vehicle);
            }
        }

        private static VehiclePawn BuildVehicle(
            VehicleDef vdef,
            VRF_NaturalRaidVehicleEntry entry,
            Faction faction,
            Map map)
        {
            VehiclePawn vehicle = VehicleSpawner.GenerateVehicle(vdef, faction);
            if (vehicle == null) return null;

            VehicleRaidUtility.ApplyColorConfig(vehicle,
                new VehicleColorConfig { mode = VehicleColorMode.Faction }, faction);

            if (!entry.upgradeLoadouts.NullOrEmpty())
            {
                var loadout = entry.upgradeLoadouts.RandomElement();
                var upgradeComp = vehicle.CompUpgradeTree;
                if (upgradeComp != null && !loadout.nodeKeys.NullOrEmpty())
                {
                    foreach (string key in loadout.nodeKeys)
                    {
                        UpgradeNode node = upgradeComp.Props.def?.GetNode(key);
                        if (node != null && !upgradeComp.NodeUnlocked(node))
                            upgradeComp.FinishUnlock(node);
                    }
                }
            }

            if (vehicle.Handlers != null)
            {
                List<Pawn> crew = new List<Pawn>();
                foreach (VehicleRoleHandler handler in vehicle.Handlers)
                {
                    if (handler.role == null || handler.role.Slots <= 0) continue;
                    VehicleCrewUtility.FillRole(vehicle, handler, null, faction, map, crew);
                }
            }

            Patch_VehicleNPCOnOff.UpdateVehiclePower(vehicle);

            CompFueledTravel fuelComp = vehicle.GetComp<CompFueledTravel>();
            if (fuelComp != null)
            {
                fuelComp.ConsumeFuel(fuelComp.Fuel);
                float target = fuelComp.FuelCapacity * (entry.fuelPercent / 100f);
                if (target > 0f) fuelComp.Refuel(target);
            }

            if (vehicle.CompVehicleTurrets != null)
            {
                float cargo = vdef.GetStatValueAbstract(VehicleStatDefOf.CargoCapacity);

                var baseTurretKeys = new HashSet<string>();
                if (vdef.CompPropsVehicleTurrets?.turrets != null)
                {
                    for (int tIdx = 0; tIdx < vdef.CompPropsVehicleTurrets.turrets.Count; tIdx++)
                    {
                        string tName = vdef.CompPropsVehicleTurrets.turrets[tIdx].def?.defName;
                        if (tName != null) baseTurretKeys.Add(tName);
                    }
                }

                VRF_UpgradeLoadout chosenLoadout = entry.upgradeLoadouts.NullOrEmpty()
                    ? null
                    : entry.upgradeLoadouts.RandomElement();

                foreach (VehicleTurret turret in vehicle.CompVehicleTurrets.Turrets)
                {
                    if (turret?.def?.ammunition == null) continue;
                    ThingDef ammoDef = turret.def.ammunition.AllowedThingDefs.FirstOrDefault();
                    if (ammoDef == null) continue;

                    bool isUpgradeTurret = !baseTurretKeys.Contains(turret.def.defName);
                    float ammoPercent;
                    if (isUpgradeTurret && chosenLoadout != null)
                        ammoPercent = chosenLoadout.GetOrCreateUpgradeAmmo(turret.def.defName).ammoPercent;
                    else
                        ammoPercent = entry.GetOrCreateTurretAmmo(turret.def.defName).ammoPercent;

                    if (ammoPercent <= 0f) continue;
                    float ammoMass = ammoDef.GetStatValueAbstract(StatDefOf.Mass);
                    if (ammoMass <= 0f) ammoMass = 0.1f;
                    int count = Mathf.FloorToInt(cargo * (ammoPercent / 100f) / ammoMass);
                    if (count > 0)
                    {
                        Thing ammo = ThingMaker.MakeThing(ammoDef);
                        ammo.stackCount = count;
                        vehicle.inventory.innerContainer.TryAdd(ammo);
                    }
                }
            }

            return vehicle;
        }

        private static IntVec3 FindSiegeDropCell(Map map, VehicleDef vdef, IncidentParms parms, List<IntVec3> usedCells)
        {
            int halfW   = Mathf.CeilToInt(vdef.Size.x / 2f);
            int halfH   = Mathf.CeilToInt(vdef.Size.z / 2f);
            int padding = Mathf.Max(halfW, halfH) + 2;
            int minSep  = Mathf.Max(MinSeparationBetweenDrops, padding * 2 + 2);
            int minEdge = padding + 6;

            IntVec3 anchor = parms.spawnCenter.IsValid ? parms.spawnCenter : map.Center;

            for (int i = 0; i < SearchAttempts; i++)
            {
                float angle = Rand.Range(0f, 360f) * Mathf.Deg2Rad;
                float dist  = Rand.Range(padding + 2, padding + minSep + 12);
                IntVec3 candidate = new IntVec3(
                    anchor.x + Mathf.RoundToInt(Mathf.Cos(angle) * dist),
                    0,
                    anchor.z + Mathf.RoundToInt(Mathf.Sin(angle) * dist));

                if (!candidate.InBounds(map)) continue;
                if (candidate.DistanceToEdge(map) < minEdge) continue;
                if (!IsValidCell(candidate, map, padding)) continue;
                if (!IsFarEnoughFromUsed(candidate, usedCells, minSep)) continue;
                return candidate;
            }

            for (int i = 0; i < FallbackAttempts; i++)
            {
                float angle = Rand.Range(0f, 360f) * Mathf.Deg2Rad;
                float dist  = Rand.Range(padding + 2, padding + minSep + 40);
                IntVec3 candidate = new IntVec3(
                    anchor.x + Mathf.RoundToInt(Mathf.Cos(angle) * dist),
                    0,
                    anchor.z + Mathf.RoundToInt(Mathf.Sin(angle) * dist));

                if (!candidate.InBounds(map)) continue;
                if (candidate.DistanceToEdge(map) < minEdge) continue;
                if (!IsValidCell(candidate, map, padding)) continue;
                if (!IsFarEnoughFromUsed(candidate, usedCells, minSep / 2)) continue;
                return candidate;
            }

            return IntVec3.Invalid;
        }

        private static void SpawnSiegeDropLanding(
            VehiclePawn vehicle, Map map, IntVec3 landCell, Faction faction, IncidentParms parms)
        {
            if (vehicle.patternData == null)
            {
                var graphicDataRgb = vehicle.VehicleDef.graphicData as Vehicles.GraphicDataRGB;
                vehicle.patternData = graphicDataRgb != null
                    ? new PatternData(graphicDataRgb)
                    : new PatternData(
                        vehicle.VehicleDef.graphicData.color,
                        vehicle.VehicleDef.graphicData.colorTwo,
                        vehicle.VehicleDef.graphicData.colorThree,
                        PatternDefOf.Default,
                        Vector2.zero,
                        0f);
            }

            CompVehicleLauncher launcher = vehicle.CompVehicleLauncher;
            if (launcher == null)
            {
                GenSpawn.Spawn(vehicle, landCell, map, Rot4.North);
                LordAirdropLanding.Schedule(vehicle, map);
                return;
            }

            launcher.inFlight = true;
            VehicleSkyfaller_Arriving skyfaller =
                (VehicleSkyfaller_Arriving)VehicleSkyfallerMaker.MakeSkyfaller(
                    launcher.Props.skyfallerIncoming, vehicle);

            GenSpawn.Spawn(skyfaller, landCell, map, Rot4.North);

            LordAirdropLanding.Schedule(vehicle, map);
        }

        private static IntVec3 FindDropCell(Map map, VehicleDef vdef, IncidentParms parms, VehicleDropMode mode, List<IntVec3> usedCells)
        {
            int halfW   = Mathf.CeilToInt(vdef.Size.x / 2f);
            int halfH   = Mathf.CeilToInt(vdef.Size.z / 2f);
            int padding = Mathf.Max(halfW, halfH) + 2;
            int minSep  = Mathf.Max(MinSeparationBetweenDrops, padding * 2 + 2);

            switch (mode)
            {
                case VehicleDropMode.Edge:
                    return FindEdgeDropCell(map, parms, padding, minSep, usedCells);

                case VehicleDropMode.EdgeGroups:
                    return FindEdgeGroupsDropCell(map, parms, padding, minSep, usedCells);

                case VehicleDropMode.Center:
                    return FindCenterDropCell(map, parms, padding, minSep, usedCells);

                case VehicleDropMode.Random:
                case VehicleDropMode.Specific:
                    return FindHostileProximityCell(map, padding, minSep, usedCells);

                default:
                    return IntVec3.Invalid;
            }
        }

        private static bool IsFarEnoughFromUsed(IntVec3 candidate, List<IntVec3> usedCells, int minSep)
        {
            foreach (IntVec3 used in usedCells)
            {
                if ((candidate - used).LengthHorizontalSquared < minSep * minSep)
                    return false;
            }
            return true;
        }

        private static IntVec3 FindCenterDropCell(Map map, IncidentParms parms, int padding, int minSep, List<IntVec3> usedCells)
        {
            IntVec3 anchor = parms.spawnCenter.IsValid ? parms.spawnCenter : map.Center;
            int minEdge = padding + 4;

            int innerRing = padding + 2;
            int outerRing = innerRing + minSep * Mathf.Max(1, usedCells.Count);

            for (int i = 0; i < SearchAttempts; i++)
            {
                float angle = Rand.Range(0f, 360f) * Mathf.Deg2Rad;
                float dist  = Rand.Range(innerRing, Mathf.Min(outerRing, 30));
                IntVec3 candidate = new IntVec3(
                    anchor.x + Mathf.RoundToInt(Mathf.Cos(angle) * dist),
                    0,
                    anchor.z + Mathf.RoundToInt(Mathf.Sin(angle) * dist));

                if (!candidate.InBounds(map)) continue;
                if (candidate.DistanceToEdge(map) < minEdge) continue;
                if (!IsValidCell(candidate, map, padding)) continue;
                if (!IsFarEnoughFromUsed(candidate, usedCells, minSep)) continue;

                return candidate;
            }

            for (int i = 0; i < FallbackAttempts; i++)
            {
                float angle = Rand.Range(0f, 360f) * Mathf.Deg2Rad;
                float dist  = Rand.Range(innerRing, 50);
                IntVec3 candidate = new IntVec3(
                    anchor.x + Mathf.RoundToInt(Mathf.Cos(angle) * dist),
                    0,
                    anchor.z + Mathf.RoundToInt(Mathf.Sin(angle) * dist));

                if (!candidate.InBounds(map)) continue;
                if (candidate.DistanceToEdge(map) < minEdge) continue;
                if (!IsValidCell(candidate, map, padding)) continue;
                if (!IsFarEnoughFromUsed(candidate, usedCells, minSep / 2)) continue;

                return candidate;
            }

            return IntVec3.Invalid;
        }

        private static IntVec3 FindEdgeGroupsDropCell(Map map, IncidentParms parms, int padding, int minSep, List<IntVec3> usedCells)
        {
            int minDistFromEdge = VehicleEdgeMargin + padding;

            List<IntVec3> groupCenters = CollectRecentDropPodCenters(map, parms.faction);

            if (groupCenters.Count > 0)
            {
                IntVec3 anchor = groupCenters.RandomElement();

                for (int i = 0; i < SearchAttempts; i++)
                {
                    float angle = Rand.Range(0f, 360f) * Mathf.Deg2Rad;
                    float dist  = Rand.Range(minSep, minSep + 20);
                    IntVec3 candidate = new IntVec3(
                        anchor.x + Mathf.RoundToInt(Mathf.Cos(angle) * dist),
                        0,
                        anchor.z + Mathf.RoundToInt(Mathf.Sin(angle) * dist));

                    if (!candidate.InBounds(map)) continue;
                    if (candidate.DistanceToEdge(map) < minDistFromEdge) continue;
                    if (!IsValidCell(candidate, map, padding)) continue;
                    if (!IsFarEnoughFromUsed(candidate, usedCells, minSep)) continue;

                    return candidate;
                }

                for (int i = 0; i < FallbackAttempts; i++)
                {
                    float angle = Rand.Range(0f, 360f) * Mathf.Deg2Rad;
                    float dist  = Rand.Range(minSep, minSep + 40);
                    IntVec3 candidate = new IntVec3(
                        anchor.x + Mathf.RoundToInt(Mathf.Cos(angle) * dist),
                        0,
                        anchor.z + Mathf.RoundToInt(Mathf.Sin(angle) * dist));

                    if (!candidate.InBounds(map)) continue;
                    if (candidate.DistanceToEdge(map) < minDistFromEdge) continue;
                    if (!IsValidCell(candidate, map, padding)) continue;
                    if (!IsFarEnoughFromUsed(candidate, usedCells, minSep / 2)) continue;

                    return candidate;
                }
            }

            return FindEdgeDropCell(map, parms, padding, minSep, usedCells);
        }

        private static List<IntVec3> CollectRecentDropPodCenters(Map map, Faction faction)
        {
            var centers = new List<IntVec3>();

            List<Thing> holders = map.listerThings.ThingsInGroup(ThingRequestGroup.ThingHolder);
            for (int hIdx = 0; hIdx < holders.Count; hIdx++)
            {
                if (!(holders[hIdx] is Skyfaller skyfaller)) continue;

                ThingOwner skyInner = skyfaller.GetDirectlyHeldThings();
                if (skyInner == null || skyInner.Count == 0) continue;

                Faction skyFaction = null;

                for (int i = 0; i < skyInner.Count; i++)
                {
                    Thing held = skyInner[i];

                    Pawn directPawn = held as Pawn;
                    if (directPawn != null)
                    {
                        skyFaction = directPawn.Faction;
                        break;
                    }

                    IActiveTransporter transporter = held as IActiveTransporter;
                    if (transporter != null)
                    {
                        ThingOwner contents = transporter.Contents?.innerContainer;
                        if (contents != null)
                        {
                            for (int j = 0; j < contents.Count; j++)
                            {
                                Pawn p = contents[j] as Pawn;
                                if (p != null)
                                {
                                    skyFaction = p.Faction;
                                    break;
                                }
                            }
                        }
                        if (skyFaction != null) break;
                    }

                    IThingHolder holder = held as IThingHolder;
                    if (holder != null)
                    {
                        ThingOwner nested = holder.GetDirectlyHeldThings();
                        if (nested != null)
                        {
                            for (int j = 0; j < nested.Count; j++)
                            {
                                Pawn p = nested[j] as Pawn;
                                if (p != null)
                                {
                                    skyFaction = p.Faction;
                                    break;
                                }
                            }
                        }
                        if (skyFaction != null) break;
                    }
                }

                if (skyFaction == faction)
                    centers.Add(skyfaller.Position);
            }

            return centers;
        }

        private static IntVec3 FindEdgeDropCell(Map map, IncidentParms parms, int padding, int minSep, List<IntVec3> usedCells)
        {
            int minDistFromEdge = VehicleEdgeMargin + padding;

            IntVec3 anchor = parms.spawnCenter.IsValid ? parms.spawnCenter : IntVec3.Invalid;

            if (anchor.IsValid)
            {
                for (int i = 0; i < SearchAttempts; i++)
                {
                    float angle = Rand.Range(0f, 360f) * Mathf.Deg2Rad;
                    float dist  = Rand.Range(minSep, minSep + 20);
                    IntVec3 candidate = new IntVec3(
                        anchor.x + Mathf.RoundToInt(Mathf.Cos(angle) * dist),
                        0,
                        anchor.z + Mathf.RoundToInt(Mathf.Sin(angle) * dist));

                    if (!candidate.InBounds(map)) continue;
                    if (candidate.DistanceToEdge(map) < minDistFromEdge) continue;
                    if (!IsValidCell(candidate, map, padding)) continue;
                    if (!IsFarEnoughFromUsed(candidate, usedCells, minSep)) continue;

                    return candidate;
                }

                for (int i = 0; i < FallbackAttempts; i++)
                {
                    float angle = Rand.Range(0f, 360f) * Mathf.Deg2Rad;
                    float dist  = Rand.Range(minSep, minSep + 40);
                    IntVec3 candidate = new IntVec3(
                        anchor.x + Mathf.RoundToInt(Mathf.Cos(angle) * dist),
                        0,
                        anchor.z + Mathf.RoundToInt(Mathf.Sin(angle) * dist));

                    if (!candidate.InBounds(map)) continue;
                    if (candidate.DistanceToEdge(map) < minDistFromEdge) continue;
                    if (!IsValidCell(candidate, map, padding)) continue;
                    if (!IsFarEnoughFromUsed(candidate, usedCells, minSep / 2)) continue;

                    return candidate;
                }
            }

            for (int i = 0; i < SearchAttempts; i++)
            {
                IntVec3 edge = CellFinder.RandomEdgeCell(map);
                IntVec3 toward = StepTowardCenter(edge, map, minDistFromEdge);

                if (!toward.IsValid || !toward.InBounds(map)) continue;
                if (toward.DistanceToEdge(map) < minDistFromEdge) continue;
                if (!IsValidCell(toward, map, padding)) continue;
                if (!IsFarEnoughFromUsed(toward, usedCells, minSep)) continue;

                return toward;
            }

            return IntVec3.Invalid;
        }

        private static IntVec3 FindHostileProximityCell(Map map, int padding, int minSep, List<IntVec3> usedCells)
        {
            List<IntVec3> anchors = CollectHostileAnchors(map);
            int minEdge = padding + 4;
            int searchR = VehicleSafeRadius + padding;

            for (int i = 0; i < SearchAttempts; i++)
            {
                IntVec3 anchor = anchors.RandomElement();

                float angle = Rand.Range(0f, 360f) * Mathf.Deg2Rad;
                float dist  = Rand.Range(padding + 4, searchR);
                IntVec3 candidate = new IntVec3(
                    anchor.x + Mathf.RoundToInt(Mathf.Cos(angle) * dist),
                    0,
                    anchor.z + Mathf.RoundToInt(Mathf.Sin(angle) * dist));

                if (!candidate.InBounds(map)) continue;
                if (candidate.DistanceToEdge(map) < minEdge) continue;
                if (!IsValidCell(candidate, map, padding)) continue;
                if (!IsFarEnoughFromUsed(candidate, usedCells, minSep)) continue;

                return candidate;
            }

            for (int i = 0; i < FallbackAttempts; i++)
            {
                IntVec3 candidate = CellFinderLoose.RandomCellWith(
                    c => c.InBounds(map)
                      && c.DistanceToEdge(map) >= minEdge
                      && IsValidCell(c, map, padding)
                      && IsFarEnoughFromUsed(c, usedCells, minSep),
                    map);
                if (candidate.IsValid) return candidate;
            }

            return IntVec3.Invalid;
        }

        private static List<IntVec3> CollectHostileAnchors(Map map)
        {
            var anchors = new List<IntVec3>();

            var pawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn p = pawns[i];
                if (p.Faction != null && p.Faction.HostileTo(Faction.OfPlayer))
                    anchors.Add(p.Position);
            }

            // Prefer combat targets / major structures over every single conduit and floor tile
            var combatTargets = map.listerBuildings.allBuildingsColonistCombatTargets;
            if (combatTargets != null && combatTargets.Count > 0)
            {
                int count = 0;
                foreach (Building b in combatTargets)
                {
                    anchors.Add(b.Position);
                    if (++count >= 40) break;
                }
            }
            else
            {
                var buildings = map.listerBuildings.allBuildingsColonist;
                if (buildings != null)
                {
                    int count = 0;
                    foreach (Building b in buildings)
                    {
                        anchors.Add(b.Position);
                        if (++count >= 40) break;
                    }
                }
            }

            if (anchors.Count == 0)
                anchors.Add(map.Center);

            return anchors;
        }

        private static bool IsValidCell(IntVec3 center, Map map, int radius)
        {
            if (!center.IsValid || !center.InBounds(map)) return false;
            if (center.Roofed(map) || !center.Walkable(map) || center.GetTerrain(map).IsWater) return false;

            // Fast check on 4 outer corners before scanning interior cells
            IntVec3 c1 = new IntVec3(center.x - radius, 0, center.z - radius);
            if (!c1.InBounds(map) || c1.Roofed(map) || !c1.Walkable(map) || c1.GetTerrain(map).IsWater) return false;
            IntVec3 c2 = new IntVec3(center.x + radius, 0, center.z - radius);
            if (!c2.InBounds(map) || c2.Roofed(map) || !c2.Walkable(map) || c2.GetTerrain(map).IsWater) return false;
            IntVec3 c3 = new IntVec3(center.x - radius, 0, center.z + radius);
            if (!c3.InBounds(map) || c3.Roofed(map) || !c3.Walkable(map) || c3.GetTerrain(map).IsWater) return false;
            IntVec3 c4 = new IntVec3(center.x + radius, 0, center.z + radius);
            if (!c4.InBounds(map) || c4.Roofed(map) || !c4.Walkable(map) || c4.GetTerrain(map).IsWater) return false;

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dz = -radius; dz <= radius; dz++)
                {
                    IntVec3 c = center + new IntVec3(dx, 0, dz);
                    if (!c.InBounds(map))       return false;
                    if (c.Roofed(map))          return false;
                    if (!c.Walkable(map))        return false;
                    if (c.GetTerrain(map).IsWater) return false;
                }
            }
            return true;
        }

        private static IntVec3 StepTowardCenter(IntVec3 edge, Map map, int targetDist)
        {
            IntVec3 dir = new IntVec3(
                Math.Sign(map.Center.x - edge.x),
                0,
                Math.Sign(map.Center.z - edge.z));

            IntVec3 pos = edge;
            for (int step = 0; step <= targetDist + 4; step++)
            {
                if (pos.DistanceToEdge(map) >= targetDist) return pos;
                pos += dir;
                if (!pos.InBounds(map)) return IntVec3.Invalid;
            }
            return pos.InBounds(map) ? pos : IntVec3.Invalid;
        }

        private static void ScheduleAirdrop(VehiclePawn vehicle, Map map, IntVec3 dropCell, Faction faction, Rot4 fromEdge)
        {
            VehicleAirDroppable droppable = new VehicleAirDroppable(vehicle);

            DropZone dropZone = new DropZone(
                GetEdgeOrigin(map, fromEdge),
                dropCell,
                1);
            dropZone.dropPoints[0] = dropCell;

            DropShip.Properties props = new DropShip.Properties
            {
                lifetime          = 720,
                delayDropByTicks  = 360,
                ticksBetweenDrops = 10
            };

            DropShip dropShip = new DropShip(map, dropZone, props)
            {
                FlyoverSoundDef = null,
                Faction         = faction
            };
            dropShip.Add(droppable);

            map.GetCachedMapComponent<AirdropManager>().Spawn(dropShip);

            SoundDef flyoverSound = DefDatabase<SoundDef>.GetNamedSilentFail("VRF_VehicleAirdrop_FlyOver");
            if (flyoverSound != null)
                flyoverSound.PlayOneShotOnCamera(map);
        }

        private static IntVec3 GetEdgeOrigin(Map map, Rot4 edge)
        {
            if (!edge.IsValid) edge = Rot4.Random;
            switch (edge.AsInt)
            {
                case 0: return new IntVec3(map.Size.x / 2, 0, map.Size.z - 1);
                case 1: return new IntVec3(map.Size.x - 1, 0, map.Size.z / 2);
                case 2: return new IntVec3(map.Size.x / 2, 0, 0);
                case 3: return new IntVec3(0,               0, map.Size.z / 2);
                default: return new IntVec3(map.Size.x / 2, 0, 0);
            }
        }

        private static void RegisterVehicleLord(Map map, Faction faction, List<VehiclePawn> vehicles)
        {
            Lord existingLord = map.lordManager.lords
                .Where(l => l.faction == faction && l.LordJob is LordJob_VehicleRaid)
                .OrderByDescending(l => l.ownedPawns.Count)
                .FirstOrDefault();

            if (existingLord != null)
            {
                LordJob_VehicleRaid existingVJob = existingLord.LordJob as LordJob_VehicleRaid;
                if (existingVJob != null && existingVJob.naturalRaidLord == null)
                {
                    Lord naturalLord = map.lordManager.lords.LastOrDefault(l => 
                        l.faction == faction && 
                        !(l.LordJob is LordJob_VehicleRaid) &&
                        !(l.LordJob is LordJob_VehicleTrade) &&
                        !(l.LordJob is LordJob_HelicopterTrade)
                    );
                    if (naturalLord != null)
                    {
                        existingVJob.naturalRaidLord = naturalLord;
                    }
                }

                foreach (VehiclePawn v in vehicles)
                    existingLord.AddPawn(v);
            }
            else
            {
                LordJob_VehicleRaid lordJob = new LordJob_VehicleRaid(faction, 35000);

                Lord naturalLord = map.lordManager.lords.LastOrDefault(l => 
                    l.faction == faction && 
                    !(l.LordJob is LordJob_VehicleRaid) &&
                    !(l.LordJob is LordJob_VehicleTrade) &&
                    !(l.LordJob is LordJob_HelicopterTrade)
                );
                if (naturalLord != null)
                {
                    lordJob.naturalRaidLord = naturalLord;
                }

                LordMaker.MakeNewLord(faction, lordJob, map, vehicles.Cast<Pawn>().ToList());
            }

            map.GetComponent<VRF_LeaderManager>()?.NotifyRaidStarted();
        }
    }

    public class VehicleAirDroppable : IAirDroppable, IExposable
    {
        public VehiclePawn vehicle;

        Thing IAirDroppable.Thing       => vehicle;
        ThingDef IAirDroppable.SkyfallerDef => SkyfallerDefOf.AirdropParatrooper;

        public VehicleAirDroppable() { }
        public VehicleAirDroppable(VehiclePawn vehicle) { this.vehicle = vehicle; }

        void IAirDroppable.OnDropped(Map map, IntVec3 pos)
        {
            if (vehicle == null || map == null) return;
            LordAirdropLanding.Schedule(vehicle, map);
        }

        void IAirDroppable.OnFailureToDrop(Map map, IntVec3 simPos)
        {
            IntVec3 fallback = CellFinder.RandomClosewalkCellNear(simPos, map, 20, null);
            if (fallback.IsValid && fallback.InBounds(map))
                GenSpawn.Spawn(vehicle, fallback, map, Rot4.Random);
            else
                vehicle.Destroy();
        }

        void IExposable.ExposeData()
        {
            Scribe_Deep.Look(ref vehicle, "vehicle");
        }
    }

    public static class LordAirdropLanding
    {
        private static readonly List<(VehiclePawn vehicle, Map map, int tick)> Pending =
            new List<(VehiclePawn, Map, int)>();

        public static void Schedule(VehiclePawn vehicle, Map map)
        {
            Pending.RemoveAll(e => e.vehicle == vehicle);
            Pending.Add((vehicle, map, Find.TickManager.TicksGame + 10));
        }

        public static void Tick()
        {
            if (Pending.Count == 0) return;
            int now = Find.TickManager.TicksGame;
            for (int i = Pending.Count - 1; i >= 0; i--)
            {
                var (vehicle, map, tick) = Pending[i];
                if (now < tick) continue;
                
                if (vehicle == null || vehicle.Destroyed || map == null || map.Disposed)
                {
                    Pending.RemoveAt(i);
                    continue;
                }

                if (!vehicle.Spawned)
                {
                    if (now > tick + 5000)
                        Pending.RemoveAt(i);
                    continue;
                }

                Pending.RemoveAt(i);
                if (vehicle.Map != map) continue;

                TryFinalizeLanding(vehicle, map);
            }
        }

        private static void TryFinalizeLanding(VehiclePawn vehicle, Map map)
        {
            Patch_VehicleNPCOnOff.UpdateVehiclePower(vehicle);

            Lord detectedNaturalLord = FindClosestNaturalLord(vehicle, map);

            var factionCfg   = VRF_Mod.Settings?.GetFactionConfig(vehicle.Faction?.def?.defName);
            var behavior     = factionCfg?.raidBehavior ?? VRF_NaturalRaidBehavior.ImmediateAssault;
            int cfgHoldTicks = factionCfg?.holdTicks    ?? 3000;

            if (detectedNaturalLord != null)
            {
                LordToil parentToil = detectedNaturalLord.CurLordToil;
                if (parentToil != null)
                {
                    string toilName = parentToil.GetType().Name;
                    string jobName  = detectedNaturalLord.LordJob?.GetType().Name ?? "";
                    bool parentIsNotYetAssaulting =
                        toilName.Contains("Travel") || toilName.Contains("Stage") ||
                        toilName.Contains("Siege")  || toilName.Contains("Defend") ||
                        jobName .Contains("Siege");

                    if (parentIsNotYetAssaulting)
                    {
                        behavior     = VRF_NaturalRaidBehavior.HoldThenAssault;
                        cfgHoldTicks = 120000;
                        VRF_Log.Msg($"[VRF_Debug] TryFinalizeLanding — forcing HoldThenAssault for {vehicle.LabelShort} (parentToil={toilName})");
                    }
                }
            }

            Lord existingLord = vehicle.GetLord();
            if (existingLord != null && !(existingLord.LordJob is LordJob_VehicleRaid))
            {
                existingLord.RemovePawn(vehicle);
                existingLord = null;
            }

            Lord lord = existingLord;
            if (lord == null)
            {
                lord = FindMatchingVRFLord(vehicle, map, detectedNaturalLord);

                if (lord != null && detectedNaturalLord != null)
                {
                    LordJob_VehicleRaid existingVJob = lord.LordJob as LordJob_VehicleRaid;
                    if (existingVJob?.naturalRaidLord != null &&
                        existingVJob.naturalRaidLord != detectedNaturalLord)
                    {
                        VRF_Log.Msg($"[VRF_Debug] TryFinalizeLanding — lord has different naturalRaidLord, creating NEW lord for group at {detectedNaturalLord.CurLordToil?.FlagLoc}");
                        lord = null;
                    }
                }

                if (lord == null)
                    lord = map.lordManager.lords
                        .FirstOrDefault(l => l.faction == vehicle.Faction && l.LordJob is LordJob_VehicleRaid
                                          && (l.LordJob as LordJob_VehicleRaid)?.naturalRaidLord == detectedNaturalLord);

                if (lord != null)
                {
                    LordJob_VehicleRaid existingVJob = lord.LordJob as LordJob_VehicleRaid;
                    if (existingVJob != null && existingVJob.naturalRaidLord == null)
                        existingVJob.naturalRaidLord = detectedNaturalLord;

                    if (!lord.ownedPawns.Contains(vehicle))
                        lord.AddPawn(vehicle);
                }
                else
                {
                    LordJob_VehicleRaid job = new LordJob_VehicleRaid(
                        vehicle.Faction,
                        stayTicks: 35000,
                        behavior:  behavior,
                        holdTicks: behavior == VRF_NaturalRaidBehavior.HoldThenAssault ? cfgHoldTicks : 0);

                    job.naturalRaidLord = detectedNaturalLord;

                    lord = LordMaker.MakeNewLord(vehicle.Faction, job, map, new List<Pawn> { vehicle });
                    VRF_Log.Msg($"[VRF_Debug] TryFinalizeLanding — created NEW lord behavior={behavior} for {vehicle.LabelShort}");
                }
            }

            if (!(lord.CurLordToil is LordToil_VehicleHoldPosition))
            {
                DutyDef duty;
                if (VRF_TransportUtil.IsSiegeDropVehicle(vehicle))
                {
                    duty = VRF_DutyDefOf.VRF_VehicleSearchAndDestroy
                        ?? DefDatabase<DutyDef>.GetNamed("VRF_VehicleSearchAndDestroy", false);
                    if (duty != null)
                        vehicle.mindState.duty = new PawnDuty(duty, vehicle.Position);
                }
                else if (VRF_TransportUtil.IsTransportVehicle(vehicle))
                    duty = VRF_DutyDefOf.VRF_VehicleTransport
                        ?? DefDatabase<DutyDef>.GetNamed("VRF_VehicleTransport", false);
                else if (VRF_TransportUtil.IsArmedTransportVehicle(vehicle))
                    duty = VRF_DutyDefOf.VRF_VehicleArmedTransport
                        ?? DefDatabase<DutyDef>.GetNamed("VRF_VehicleArmedTransport", false)
                        ?? DefDatabase<DutyDef>.GetNamed("VRF_VehicleSearchAndDestroy", false);
                else
                    duty = VRF_DutyDefOf.VRF_VehicleSearchAndDestroy
                        ?? DefDatabase<DutyDef>.GetNamed("VRF_VehicleSearchAndDestroy", false);

                if (!VRF_TransportUtil.IsSiegeDropVehicle(vehicle) && duty != null)
                    vehicle.mindState.duty = new PawnDuty(duty);

                vehicle.jobs?.StopAll();
                vehicle.jobs?.StartJob(JobMaker.MakeJob(JobDefOf.Wait_Combat, 300, true), JobCondition.InterruptForced);
            }

            lord.CurLordToil?.UpdateAllDuties();
            DelayedDutyRefresh.Schedule(lord, map);
            map.GetComponent<VRF_LeaderManager>()?.NotifyRaidStarted();

            if (VRF_TransportUtil.IsSiegeDropVehicle(vehicle))
                VRF_SiegeDropBehavior.OnSiegeDropLanded(vehicle, map);
        }

        private static Lord FindClosestNaturalLord(VehiclePawn vehicle, Map map)
        {
            Lord  closest  = null;
            float bestDist = float.MaxValue;

            int naturalLordCount = 0;
            foreach (Lord l in map.lordManager.lords)
            {
                if (l.faction != vehicle.Faction)          continue;
                if (l.LordJob is LordJob_VehicleRaid)      continue;
                if (l.LordJob is LordJob_VehicleTrade)     continue;
                if (l.LordJob is LordJob_HelicopterTrade)  continue;

                naturalLordCount++;
                IntVec3 flag = l.CurLordToil?.FlagLoc ?? IntVec3.Invalid;
                float dist = flag.IsValid
                    ? vehicle.Position.DistanceTo(flag)
                    : vehicle.Position.DistanceTo(
                        l.ownedPawns.FirstOrDefault()?.Position ?? map.Center);

                VRF_Log.Msg($"[VRF_Debug] FindClosestNaturalLord — lord#{naturalLordCount} job={l.LordJob?.GetType().Name} toil={l.CurLordToil?.GetType().Name} flag={flag} dist={dist:F1}");
                if (dist < bestDist) { bestDist = dist; closest = l; }
            }
            VRF_Log.Msg($"[VRF_Debug] FindClosestNaturalLord — {naturalLordCount} natural lords found, closest FlagLoc={closest?.CurLordToil?.FlagLoc}");
            return closest;
        }

        private static Lord FindMatchingVRFLord(VehiclePawn vehicle, Map map, Lord naturalLord)
        {
            if (naturalLord != null)
            {
                Lord exact = map.lordManager.lords.FirstOrDefault(l =>
                    l.faction == vehicle.Faction &&
                    l.LordJob is LordJob_VehicleRaid &&
                    (l.LordJob as LordJob_VehicleRaid)?.naturalRaidLord == naturalLord &&
                    (l.CurLordToil is LordToil_VehicleSearchAndDestroy ||
                     l.CurLordToil is LordToil_VehicleHoldPosition));
                if (exact != null) return exact;
            }

            Lord  closest  = null;
            float bestDist = float.MaxValue;
            foreach (Lord l in map.lordManager.lords)
            {
                if (l.faction != vehicle.Faction)      continue;
                if (!(l.LordJob is LordJob_VehicleRaid)) continue;
                if (l.CurLordToil is LordToil_VehicleExitMap) continue;

                IntVec3 flag = l.CurLordToil?.FlagLoc ?? IntVec3.Invalid;
                float dist = flag.IsValid ? vehicle.Position.DistanceTo(flag) : float.MaxValue;
                if (dist < bestDist) { bestDist = dist; closest = l; }
            }
            return closest;
        }
    }
}