using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Vehicles;
using VehicleRaid;

namespace VehicleRaidFramework
{
    [HarmonyPatch(typeof(IncidentWorker_Raid), "TryExecuteWorker")]
    public static class Patch_NaturalRaidVehicleInjector
    {
        internal static readonly Dictionary<IIncidentTarget, List<(PawnKindDef kind, VRF_NaturalRaidVehicleEntry entry, VehicleDef vdef)>>
            PendingVehicles = new Dictionary<IIncidentTarget, List<(PawnKindDef, VRF_NaturalRaidVehicleEntry, VehicleDef)>>();

        private static bool IsVehicleWithMap(VehicleDef vehicleDef)
        {
            return vehicleDef != null && vehicleDef.thingClass != null &&
                typeof(global::VehicleMapFramework.VehiclePawnWithMap).IsAssignableFrom(vehicleDef.thingClass);
        }

        public static bool IsDropPodArrival(IncidentParms parms)
        {
            if (parms?.raidArrivalMode == null) return false;
            if (parms.raidArrivalMode.defName != null && parms.raidArrivalMode.defName.ToLowerInvariant().Contains("drop"))
                return true;
            Type wc = parms.raidArrivalMode.workerClass;
            if (wc == null) return false;
            return typeof(PawnsArrivalModeWorker_CenterDrop).IsAssignableFrom(wc) ||
                   typeof(PawnsArrivalModeWorker_ClusterDrop).IsAssignableFrom(wc) ||
                   typeof(PawnsArrivalModeWorker_EdgeDrop).IsAssignableFrom(wc) ||
                   typeof(PawnsArrivalModeWorker_EdgeDropGroups).IsAssignableFrom(wc) ||
                   typeof(PawnsArrivalModeWorker_RandomDrop).IsAssignableFrom(wc) ||
                   typeof(PawnsArrivalModeWorker_SpecificLocationDrop).IsAssignableFrom(wc) ||
                   wc.Name.Contains("Drop");
        }

        [HarmonyPrefix]
        public static void Prefix(IncidentParms parms)
        {
            if (parms == null || parms.faction == null || parms.target == null) return;

            if (parms.raidStrategy?.GetModExtension<VehicleRaidExtension>() != null) return;

            if (VRF_Mod.Settings?.globalExcludedRaidStrategies != null
                && VRF_Mod.Settings.globalExcludedRaidStrategies.Count > 0)
            {
                if (parms.raidStrategy != null && VRF_Mod.Settings.globalExcludedRaidStrategies.Contains(parms.raidStrategy.defName))
                    return;
                if (IsDropPodArrival(parms) && VRF_Mod.Settings.globalExcludedRaidStrategies.Contains("DropPod"))
                    return;
            }

            var factionConfig = VRF_Mod.Settings?.GetFactionConfig(parms.faction.def.defName);
            if (factionConfig == null) return;

            // Gravships deliberately do not use vehicleEntries. Their configuration is per preset
            // in gravshipEntries, so a base VMF vehicle can never be spawned without a structure.
            var enabledEntries = factionConfig.vehicleEntries.Where(e => e.enabled).ToList();

            bool isDropArrival = IsDropPodArrival(parms);
            if (isDropArrival && (VRF_Mod.Settings != null && !VRF_Mod.Settings.allowGlobalDropPodRaids))
                return;

            var eligible = new List<(VRF_NaturalRaidVehicleEntry entry, VehicleDef vdef, float defaultCombatPower)>();
            
            Map map = parms.target as Map;
            bool mapHasWater = false;
            if (map != null)
            {
                mapHasWater = SeaVehicleSpawnUtility.TryFindSeaEntryCell(map, null, out _);
            }

            foreach (var e in enabledEntries)
            {
                PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamedSilentFail(e.vehicleKindDefName);
                if (kind == null || !(kind.race is VehicleDef vdef) || IsVehicleWithMap(vdef)) continue;
                if (e.minRaidPoints > 0f && parms.points < e.minRaidPoints) continue;
                if (e.maxRaidPoints > 0f && parms.points > e.maxRaidPoints) continue;
                if (e.allowedRaidStrategies != null && e.allowedRaidStrategies.Count > 0)
                {
                    string stratName = parms.raidStrategy?.defName;
                    if (stratName != null && e.allowedRaidStrategies.Contains(stratName)) continue;
                    if (isDropArrival && e.allowedRaidStrategies.Contains("DropPod")) continue;
                }

                if (isDropArrival && !e.allowDropPod)
                    continue;

                if (e.isSiegeDrop
                    && (VRF_Mod.Settings?.SiegeDropOnlyOnDropRaids ?? true)
                    && !isDropArrival)
                    continue;

                if (isDropArrival &&
                    !e.isSiegeDrop &&
                    (vdef.type == VehicleType.Air ||
                     vdef.type == VehicleType.Sea ||
                     vdef.comps.Any(c => c is CompProperties_VehicleHover)))
                    continue;

                if (vdef.type == VehicleType.Sea && !mapHasWater)
                    continue;

                eligible.Add((e, vdef, kind.combatPower));
            }

            // VMF vehicles are candidates only through a real, enabled preset.  Copy the preset
            // raid settings into an ephemeral entry so the common budget loop never reads the
            // old base-vehicle values (including its usual 100 combat power).
            VehicleDef gravshipBaseDef = DefDatabase<VehicleDef>.GetNamedSilentFail("VMF_GravshipVehicleBase");
            if (gravshipBaseDef == null)
                gravshipBaseDef = DefDatabase<VehicleDef>.AllDefsListForReading.FirstOrDefault(IsVehicleWithMap);

            // The drop-pod handler has its own generic construction path. VMF vehicles need
            // their interior preset, so they are edge-spawned only.
            if (!isDropArrival)
            {
                var presetFiles = VehicleMapFramework.VRF_GravshipPresetUtility.FindAllVehicleMapPresetFiles();
                foreach (var gravEntry in factionConfig.gravshipEntries ?? Enumerable.Empty<VRF_GravshipRaidEntry>())
                {
                    if (!gravEntry.enabled || gravEntry.combatPower <= 0f) continue;
                    if (gravEntry.minRaidPoints > 0f && parms.points < gravEntry.minRaidPoints) continue;
                    if (gravEntry.maxRaidPoints > 0f && parms.points > gravEntry.maxRaidPoints) continue;

                    string strategyName = parms.raidStrategy?.defName;
                    if (gravEntry.allowedRaidStrategies != null && gravEntry.allowedRaidStrategies.Count > 0)
                    {
                        if (strategyName != null && gravEntry.allowedRaidStrategies.Contains(strategyName)) continue;
                        if (isDropArrival && gravEntry.allowedRaidStrategies.Contains("DropPod")) continue;
                    }

                    string presetFile = presetFiles.FirstOrDefault(f =>
                        string.Equals(System.IO.Path.GetFileNameWithoutExtension(f), gravEntry.presetName, StringComparison.OrdinalIgnoreCase));
                    var presetData = string.IsNullOrEmpty(presetFile) ? null
                        : VehicleMapFramework.VRF_GravshipPresetUtility.LoadGravshipPresetFromFile(presetFile);
                    if (presetData == null || presetData.cells == null || presetData.cells.Count == 0) continue;

                    VehicleDef presetVehicleDef = DefDatabase<VehicleDef>.GetNamedSilentFail(presetData.vehicleDefName);
                    bool isActualGravship = VehicleMapFramework.VRF_GravshipPresetUtility.IsGravshipVehicleDef(presetVehicleDef);
                    if (presetVehicleDef == null || !IsVehicleWithMap(presetVehicleDef)) continue;
                    VehicleDef vehicleDefToSpawn = isActualGravship ? gravshipBaseDef : presetVehicleDef;
                    if (vehicleDefToSpawn == null) continue;

                    eligible.Add((new VRF_NaturalRaidVehicleEntry
                    {
                        enabled = true,
                        combatPowerOverride = gravEntry.combatPower,
                        minRaidPoints = gravEntry.minRaidPoints,
                        maxRaidPoints = gravEntry.maxRaidPoints,
                        helicopterMode = gravEntry.hoverMode,
                        airVehicleType = isActualGravship ? "Gravship" : "VehicleMap",
                        gravshipPresetName = gravEntry.presetName
                    }, vehicleDefToSpawn, gravEntry.combatPower));
                }
            }
            if (eligible.Count == 0) return;

            float fraction      = Mathf.Clamp01(VRF_Mod.Settings?.VehiclePointsFraction ?? 0.5f);
            float vehicleBudget = (parms.points - 250f) * fraction;
            if (vehicleBudget <= 0f) return;

            var toSpawn = new List<(PawnKindDef kind, VRF_NaturalRaidVehicleEntry entry, VehicleDef vdef)>();
            float budgetLeft = vehicleBudget;
            float pointsConsumed = 0f;
            bool firstPass = true;
            int selectedVehicleMapCount = 0;
            int vehicleMapLimit = Mathf.Clamp(VRF_Mod.Settings?.MaxVehicleMapSpawnsPerRaid ?? 4, 0, 100);

            while (budgetLeft > 0f || firstPass)
            {
                var affordable = eligible.Where(e =>
                {
                    // VMF vehicles have their own live interior map. Once the configured
                    // cap is reached, leave them out of this roll so remaining points can
                    // select any ordinary injected vehicle instead.
                    if (IsVehicleWithMap(e.vdef) && selectedVehicleMapCount >= vehicleMapLimit)
                        return false;

                    float cost = e.entry.combatPowerOverride > 0f
                        ? e.entry.combatPowerOverride
                        : e.defaultCombatPower;
                    return firstPass || cost <= budgetLeft;
                }).ToList();

                if (affordable.Count == 0) break;

                var picked = affordable.RandomElement();
                float pickedCost = picked.entry.combatPowerOverride > 0f
                    ? picked.entry.combatPowerOverride
                    : picked.defaultCombatPower;

                toSpawn.Add((null, picked.entry, picked.vdef));
                if (IsVehicleWithMap(picked.vdef))
                    selectedVehicleMapCount++;
                budgetLeft -= pickedCost;
                pointsConsumed += pickedCost;
                firstPass = false;
            }

            if (toSpawn.Count == 0) return;

            parms.points -= pointsConsumed;
            if (parms.points < 35f) parms.points = 35f;

            PendingVehicles[parms.target] = toSpawn;
        }

        internal static bool IsNaturalRaidHoldPhase(IncidentParms parms)
        {
            Map map = parms.target as Map;
            if (map == null) return false;

            Lord naturalLord = map.lordManager.lords.LastOrDefault(l =>
                l.faction == parms.faction &&
                !(l.LordJob is LordJob_VehicleRaid) &&
                !(l.LordJob is LordJob_VehicleTrade) &&
                !(l.LordJob is LordJob_HelicopterTrade));

            if (naturalLord == null) return false;

            LordToil toil = naturalLord.CurLordToil;
            if (toil == null) return false;

            string toilName = toil.GetType().Name;
            string jobName  = naturalLord.LordJob?.GetType().Name ?? "";
            return toilName.Contains("Travel") || toilName.Contains("Stage") ||
                   toilName.Contains("Siege")  || toilName.Contains("Defend") ||
                   jobName .Contains("Siege");
        }

        [HarmonyPostfix]
        public static void Postfix(IncidentParms parms, bool __result)
        {
            if (!__result) return;
            if (parms == null || parms.faction == null || parms.target == null) return;

            if (!PendingVehicles.TryGetValue(parms.target, out var toSpawn))
                return;
            PendingVehicles.Remove(parms.target);

            if (toSpawn.NullOrEmpty()) return;

            Map map = parms.target as Map;
            if (map == null) return;

            SpawnVehiclesNormal(map, parms, toSpawn);

            InjectVehiclesIntoRaidLetter(parms, map);
        }

        private static void InjectVehiclesIntoRaidLetter(IncidentParms parms, Map map)
        {
            Letter raidLetter = null;
            var letters = Find.LetterStack.LettersListForReading;
            for (int i = letters.Count - 1; i >= 0; i--)
            {
                Letter l = letters[i];
                if (l.relatedFaction == parms.faction &&
                    l.lookTargets != null &&
                    (l.def == LetterDefOf.ThreatBig || l.def == LetterDefOf.PositiveEvent))
                {
                    raidLetter = l;
                    break;
                }
            }
            if (raidLetter == null) return;

            foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
            {
                if (!(pawn is VehiclePawn)) continue;
                if (pawn.Dead || pawn.Destroyed) continue;
                if (pawn.Faction != parms.faction) continue;
                GlobalTargetInfo gti = (GlobalTargetInfo)pawn;
                if (!raidLetter.lookTargets.targets.Contains(gti))
                    raidLetter.lookTargets.targets.Add(gti);
            }
        }

        internal static void SpawnVehiclesNormal(
            Map map,
            IncidentParms parms,
            List<(PawnKindDef kind, VRF_NaturalRaidVehicleEntry entry, VehicleDef vdef)> toSpawn)
        {
            if (IsNaturalRaidHoldPhase(parms))
            {
                toSpawn = toSpawn.Where(t =>
                {
                    if (t.vdef.type != VehicleType.Air) return true;
                    bool isHoverMode      = t.entry.helicopterMode || t.vdef.comps.Any(c => c is CompProperties_VehicleHover);
                    bool isAirplaneFlight = t.entry.airVehicleType == "Airplane" || VRF_AerialVehicleClassifier.IsAirplane(t.vdef);
                    bool exclude          = isHoverMode && isAirplaneFlight;
                    if (exclude)
                        Log.Message($"[VRF_Debug] HoldThenAssault — skipping hover airplane '{t.vdef.defName}' (cannot hold; excluded before spawn)");
                    return !exclude;
                }).ToList();

                if (toSpawn.Count == 0)
                {
                    Log.Message("[VRF_Debug] HoldThenAssault — all vehicles were hover airplanes, nothing to spawn.");
                    return;
                }
            }

            List<Pawn> vehiclePawns = new List<Pawn>();
            List<(VehicleDef vdef, CompProperties_VehicleHover props)> injectedHoverList =
                new List<(VehicleDef, CompProperties_VehicleHover)>();

            List<VehiclePawn> preparedVehicles = new List<VehiclePawn>();

            try
            {
                foreach (var (_, spawnEntry, spawnVDef) in toSpawn)
                {
                    bool spawnIsGravship = spawnEntry.airVehicleType == "Gravship";
                    CompProperties_VehicleHover gravshipHoverInjected = null;
                    if (spawnIsGravship && spawnEntry.helicopterMode &&
                        !spawnVDef.comps.Any(c => c is CompProperties_VehicleHover))
                    {
                        // The dynamic VehicleDef created from the VMF preset inherits this comp.
                        // Keeping it on the base def only until creation avoids leaking hover mode
                        // to later presets or to ordinary VMF vehicles.
                        var gravshipHover = new CompProperties_VehicleHover
                        {
                            flightType = FlightType.Gravship,
                            maxTicks = 180,
                            maxTicksVertical = 180,
                            maxTicksPropeller = 180,
                            hoverAltitude = 4f,
                            hoverShadowOffset = 1.5f,
                            hoverBobAmount = 0.22f,
                            hoverBobSpeed = 2f,
                            hoverMoveSpeed = VehicleMapFramework.VRF_GravshipSpeedUtility.CalculatePresetSpeed(spawnEntry.gravshipPresetName),
                            hoverRotationSpeed = 90f
                        };
                        spawnVDef.comps.Add(gravshipHover);
                        injectedHoverList.Add((spawnVDef, gravshipHover));
                        gravshipHoverInjected = gravshipHover;
                    }
                    if (spawnEntry.helicopterMode && !spawnIsGravship && spawnVDef.type == VehicleType.Air && !spawnEntry.isSiegeDrop)
                    {
                        bool isAirplane = spawnEntry.airVehicleType == "Airplane";
                                    bool isGravship = spawnEntry.airVehicleType == "Gravship";

                        bool alreadyHasHover = spawnVDef.comps.Any(c => c is CompProperties_VehicleHover);
                        if (alreadyHasHover)
                        {
                        }
                        else
                        {
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

                    VehiclePawn vehicle = null;
                    string targetPresetName = spawnEntry.gravshipPresetName;
                    bool spawnIsVehicleWithMap = IsVehicleWithMap(spawnVDef);

                    if (!string.IsNullOrEmpty(targetPresetName))
                    {
                        // The regular VMF vehicle presets are deliberately not in the
                        // gravship-only list. Search the shared VMF preset folder here so
                        // a Dreadnought (or any other map vehicle) can find its own file.
                        var allFiles = spawnIsVehicleWithMap
                            ? VehicleMapFramework.VRF_GravshipPresetUtility.FindAllVehicleMapPresetFiles()
                            : VehicleMapFramework.VRF_GravshipPresetUtility.FindAllGravshipPresetFiles();
                        string targetFile = allFiles.FirstOrDefault(f => string.Equals(System.IO.Path.GetFileNameWithoutExtension(f), targetPresetName, StringComparison.OrdinalIgnoreCase));
                        if (!string.IsNullOrEmpty(targetFile) && System.IO.File.Exists(targetFile))
                        {
                            var presetData = VehicleMapFramework.VRF_GravshipPresetUtility.LoadGravshipPresetFromFile(targetFile);
                            if (presetData != null)
                            {
                                // Gravships require their dynamic bounding-box definition;
                                // all other VMF vehicles must retain their own VehicleDef.
                                vehicle = spawnIsGravship
                                    ? VehicleMapFramework.VRF_GravshipPresetUtility.CreateGravshipVehicleFromPreset(presetData, parms.faction, spawnVDef)
                                    : VehicleMapFramework.VRF_GravshipPresetUtility.CreateVehicleWithMapFromPreset(presetData, parms.faction, spawnVDef);
                            }
                        }
                    }

                    // The generated VMF definition has already copied this component. Remove it
                    // now so a non-hover preset selected later in the same raid stays grounded.
                    if (gravshipHoverInjected != null)
                    {
                        spawnVDef.comps.Remove(gravshipHoverInjected);
                        injectedHoverList.Remove((spawnVDef, gravshipHoverInjected));
                    }

                    // A VMF vehicle is valid only when its matching preset built it. Never
                    // fall back to GenerateVehicle here: that would create an empty map or,
                    // for gravships, the wrong shared base vehicle.
                    if (vehicle == null && spawnIsVehicleWithMap)
                    {
                        Log.Warning($"[VehicleRaidFramework] Skipping VMF vehicle '{spawnVDef.defName}' with preset '{targetPresetName ?? "(no preset)"}': it could not be created.");
                        continue;
                    }

                    if (vehicle == null)
                    {
                        vehicle = VehicleSpawner.GenerateVehicle(spawnVDef, parms.faction);
                    }
                    if (vehicle == null) continue;

                    VehicleRaidUtility.ApplyColorConfig(vehicle,
                        spawnEntry.paintConfig != null
                            ? new VehicleColorConfig
                            {
                                mode       = spawnEntry.paintConfig.mode == VRF_PaintMode.Fixed  ? VehicleColorMode.Fixed
                                           : spawnEntry.paintConfig.mode == VRF_PaintMode.Faction ? VehicleColorMode.Faction
                                           : VehicleColorMode.Faction,
                                colorOne   = spawnEntry.paintConfig.mode == VRF_PaintMode.Fixed ? (Color?)spawnEntry.paintConfig.colorOne   : null,
                                colorTwo   = spawnEntry.paintConfig.mode == VRF_PaintMode.Fixed ? (Color?)spawnEntry.paintConfig.colorTwo   : null,
                                colorThree = spawnEntry.paintConfig.mode == VRF_PaintMode.Fixed ? (Color?)spawnEntry.paintConfig.colorThree : null,
                                pattern    = spawnEntry.paintConfig.mode == VRF_PaintMode.Fixed ? spawnEntry.paintConfig.ResolvedPattern    : null
                            }
                            : new VehicleColorConfig { mode = VehicleColorMode.Faction },
                        parms.faction);

                    VRF_UpgradeLoadout chosenLoadout = null;
                    if (!spawnEntry.upgradeLoadouts.NullOrEmpty())
                    {
                        chosenLoadout = spawnEntry.upgradeLoadouts.RandomElement();
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

                    List<Pawn> crew = new List<Pawn>();
                    if (vehicle.Handlers != null)
                    {
                        foreach (VehicleRoleHandler handler in vehicle.Handlers)
                        {
                            if (handler.role == null || handler.role.Slots <= 0) continue;
                            VehicleCrewUtility.FillRole(vehicle, handler, null, parms.faction, map, crew);
                        }
                    }

                    Patch_VehicleNPCOnOff.UpdateVehiclePower(vehicle);

                    CompFueledTravel fuelComp = vehicle.GetComp<CompFueledTravel>();
                    if (fuelComp != null)
                    {
                        // Gravships fill their own tanks in CreateGravshipVehicleFromPreset (step 10).
                        // Calling ConsumeFuel/Refuel here before spawn would corrupt their fuel state
                        // because CompFueledTravelGravship.Engine is null until after GenSpawn.Spawn.
                        if (!CrewManager.IsGravshipVehicle(vehicle))
                        {
                            float currentFuel = fuelComp.Fuel;
                            if (currentFuel > 0f) fuelComp.ConsumeFuel(currentFuel);
                            float targetFuel = fuelComp.FuelCapacity;
                            if (targetFuel > 0f) fuelComp.Refuel(targetFuel);
                        }
                    }

                    if (vehicle.CompVehicleTurrets != null)
                    {
                        float realCargoCapacity = vehicle.GetStatValue(VehicleStatDefOf.CargoCapacity);
                        float maxAmmoWeight = realCargoCapacity * 0.75f;

                        var existingAmmo = vehicle.inventory.innerContainer
                            .Where(t => t.def?.projectile != null || t.def?.projectileWhenLoaded != null)
                            .ToList();
                        foreach (Thing old in existingAmmo)
                        {
                            vehicle.inventory.innerContainer.Remove(old);
                            old.Destroy();
                        }

                        var baseTurretKeys = new HashSet<string>(
                            spawnVDef.CompPropsVehicleTurrets?.turrets?.Select(t => t.def?.defName).Where(k => k != null)
                            ?? System.Linq.Enumerable.Empty<string>());

                        var desiredAmmo = new List<(ThingDef ammoDef, float targetKg)>();
                        float totalDesiredWeight = 0f;

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
                                ammoPercent = spawnEntry.GetOrCreateTurretAmmo(turret.def.defName).ammoPercent;

                            if (ammoPercent <= 0f) continue;
                            
                            float targetKg = realCargoCapacity * (ammoPercent / 100f);
                            desiredAmmo.Add((ammoDef, targetKg));
                            totalDesiredWeight += targetKg;
                        }

                        float scaleFactor = 1f;
                        if (totalDesiredWeight > maxAmmoWeight)
                        {
                            scaleFactor = maxAmmoWeight / totalDesiredWeight;
                        }

                        foreach (var (ammoDef, targetKg) in desiredAmmo)
                        {
                            float actualKg = targetKg * scaleFactor;
                            float ammoMass = ammoDef.GetStatValueAbstract(StatDefOf.Mass);
                            if (ammoMass <= 0f) ammoMass = 0.1f;

                            int count = Mathf.FloorToInt(actualKg / ammoMass);
                            if (count > 0)
                            {
                                Thing ammo = ThingMaker.MakeThing(ammoDef);
                                ammo.stackCount = count;
                                vehicle.inventory.innerContainer.TryAdd(ammo);
                            }
                        }
                    }

                    EnforceCargoSafetyLimit(vehicle);

                    preparedVehicles.Add(vehicle);
                }
            }
            finally
            {
                foreach (var (vdef, hprops) in injectedHoverList)
                    vdef.comps.Remove(hprops);
            }

            if (preparedVehicles.Count == 0) return;

            Action<List<VehiclePawn>, List<IntVec3>, Rot4> spawnGroup = (vehList, cells, rot) =>
            {
                for (int i = 0; i < vehList.Count; i++)
                {
                    VehiclePawn vehicle = vehList[i];
                    IntVec3 rawCell = (cells != null && i < cells.Count) ? cells[i] : vehicle.Position;
                    IntVec3 spawnCell = (vehicle.VehicleDef.type != VehicleType.Sea && vehicle.GetComp<CompVehicleHover>() == null)
                        ? FindSafeLandSpawnCell(rawCell, vehicle, map, rot)
                        : rawCell;

                    GenSpawn.Spawn(vehicle, spawnCell, map, rot);
                    vehiclePawns.Add(vehicle);

                    // Post-spawn fuel sync and faction fix for gravships
                    if (vehicle is global::VehicleMapFramework.VehiclePawnWithMap gravshipVehicle)
                    {
                        // Re-apply faction to all interior buildings (some may reset on spawn)
                        Faction spawnFaction = gravshipVehicle.Faction;
                        if (spawnFaction != null)
                        {
                            Map intMap = gravshipVehicle.VehicleMap;
                            if (intMap != null)
                            {
                                foreach (Thing t in intMap.listerThings.AllThings.ToList())
                                {
                                    if (t.def.CanHaveFaction && t.Faction != spawnFaction)
                                        t.SetFaction(spawnFaction);
                                }
                            }
                        }
                        VehicleRaidFramework.VehicleMapFramework.VRF_GravshipPresetUtility.SyncGravshipFuelPostSpawnPublic(gravshipVehicle);
                        VehicleRaidFramework.VehicleMapFramework.VRF_VehicleMapNpcUtility.SpawnInteriorTurretCrew(gravshipVehicle);
                    }

                    map.GetComponent<VRF_LeaderManager>()?.RegisterLeader(vehicle, vehiclePawns.Count - 1);

                    int originalIndex = preparedVehicles.IndexOf(vehicle);
                    if (originalIndex >= 0)
                    {
                        bool isMortarVehicle = false;
                        var turretComp = vehicle.CompVehicleTurrets;
                        if (turretComp != null)
                        {
                            isMortarVehicle = turretComp.Props.deployTime > 0f;

                            if (!isMortarVehicle && turretComp.Turrets != null)
                            {
                                foreach (var turret in turretComp.Turrets)
                                {
                                    if (turret.ProjectileDef?.projectile?.flyOverhead == true)
                                    {
                                        isMortarVehicle = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (isMortarVehicle)
                            map.GetComponent<VRF_LeaderManager>()?.RegisterMortar(vehicle);
                    }
                    if (originalIndex >= 0 && toSpawn[originalIndex].entry.helicopterMode && !toSpawn[originalIndex].entry.isSiegeDrop &&
                        (vehicle.VehicleDef.type == VehicleType.Air || CrewManager.IsGravshipVehicle(vehicle)))
                    {
                        var hoverComp = vehicle.GetComp<CompVehicleHover>();
                        if (hoverComp != null)
                        {
                            hoverComp.ActivateHoverNPC();
                            // Gravships activate hover but are never registered as transports
                            if (!CrewManager.IsGravshipVehicle(vehicle))
                            {
                                int aboardCount = vehicle.AllPawnsAboard.Count;
                                if (VRF_TransportUtil.IsTransportVehicle(vehicle) && aboardCount > 1)
                                    HoverNPC_TransportManager.GetFor(map)?.RegisterVehicle(vehicle);
                                else if (!VRF_TransportUtil.IsTransportVehicle(vehicle) && aboardCount > 1)
                                    HoverNPC_TransportManager.GetFor(map)?.RegisterArmedVehicle(vehicle);
                            }
                        }
                    }
                }
            };

            var seaVehicles = preparedVehicles.Where(v => v.VehicleDef.type == VehicleType.Sea).ToList();
            var landVehicles = preparedVehicles.Where(v => v.VehicleDef.type != VehicleType.Sea).ToList();

            if (seaVehicles.Count > 0)
            {
                List<VehicleDef> seaVDefs = seaVehicles.Select(v => v.VehicleDef).ToList();
                IntVec3 seaGroupBase;
                if (!VehicleTrafficManager.TryFindGroupEntryPoint(map, seaVDefs, out seaGroupBase))
                {
                    Log.Warning("[VehicleRaidFramework] Could not find a valid water path for sea vehicles to spawn. They will not be spawned.");
                }
                else
                {
                    Rot4 groupRot = seaGroupBase.GetBeginningOfRoadDirection(map);
                    List<IntVec3> formationCells = VehicleTrafficManager.CalculateFormationCells(seaGroupBase, map, groupRot, seaVehicles, new List<IntVec3>());
                    spawnGroup(seaVehicles, formationCells, groupRot);
                }
            }

            if (landVehicles.Count > 0)
            {
                List<VehicleDef> landVDefs = landVehicles.Select(v => v.VehicleDef).ToList();
                IntVec3 landGroupBase;
                if (parms.spawnCenter.IsValid)
                {
                    VehicleDef widest = landVDefs.OrderByDescending(v => Mathf.Max(v.size.x, v.size.z)).First();
                    landGroupBase = VehicleTrafficManager.GetSafeSpawnCell(parms.spawnCenter, map, widest, 8);
                    if (!VehicleTrafficManager.CanReachCenter(landGroupBase, map, widest))
                    {
                        if (!VehicleTrafficManager.TryFindGroupEntryPoint(map, landVDefs, out landGroupBase))
                            landGroupBase = CellFinder.TryFindRandomEdgeCellWith(c => c.GetTerrain(map) != null && !c.GetTerrain(map).IsWater && c.Standable(map), map, 0f, out IntVec3 safeEdge) ? safeEdge : CellFinder.RandomEdgeCell(map);
                    }
                }
                else if (!VehicleTrafficManager.TryFindGroupEntryPoint(map, landVDefs, out landGroupBase))
                {
                    landGroupBase = CellFinder.TryFindRandomEdgeCellWith(c => c.GetTerrain(map) != null && !c.GetTerrain(map).IsWater && c.Standable(map), map, 0f, out IntVec3 safeEdge) ? safeEdge : CellFinder.RandomEdgeCell(map);
                }

                Rot4 groupRot = landGroupBase.GetBeginningOfRoadDirection(map);
                List<IntVec3> formationCells = VehicleTrafficManager.CalculateFormationCells(landGroupBase, map, groupRot, landVehicles, new List<IntVec3>());
                spawnGroup(landVehicles, formationCells, groupRot);
            }

            var factionCfg   = VRF_Mod.Settings?.GetFactionConfig(parms.faction?.def?.defName);
            var behavior     = factionCfg?.raidBehavior ?? VRF_NaturalRaidBehavior.ImmediateAssault;
            int cfgHoldTicks = factionCfg?.holdTicks    ?? 3000;

            bool willHold = false;
            {
                Lord preCheckLord = map.lordManager.lords.LastOrDefault(l =>
                    l.faction == parms.faction &&
                    !(l.LordJob is LordJob_VehicleRaid) &&
                    !(l.LordJob is LordJob_VehicleTrade) &&
                    !(l.LordJob is LordJob_HelicopterTrade));
                if (preCheckLord != null)
                {
                    LordToil preToil = preCheckLord.CurLordToil;
                    if (preToil != null)
                    {
                        string tn = preToil.GetType().Name;
                        string jn = preCheckLord.LordJob?.GetType().Name ?? "";
                        willHold = tn.Contains("Travel") || tn.Contains("Stage") ||
                                   tn.Contains("Siege")  || tn.Contains("Defend") ||
                                   jn.Contains("Siege");
                    }
                }
            }
            if (willHold)
            {
                for (int _vi = vehiclePawns.Count - 1; _vi >= 0; _vi--)
                {
                    if (!(vehiclePawns[_vi] is VehiclePawn _vp)) continue;
                    if (_vp.VehicleDef.type != VehicleType.Air) continue;
                    if (_vp.GetComp<CompVehicleHover>() == null) continue;          
                    if (!VRF_AerialVehicleClassifier.IsAirplane(_vp.VehicleDef)) continue; 
                    Log.Message($"[VRF_Debug] HoldThenAssault — removing hover airplane '{_vp.LabelShort}' (cannot hold)");
                    Lord _existingLordForCleanup = _vp.GetLord();
                    _existingLordForCleanup?.RemovePawn(_vp);
                    _vp.Destroy();
                    vehiclePawns.RemoveAt(_vi);
                }
                if (vehiclePawns.Count == 0) return;
            }

            Lord detectedNaturalLord = map.lordManager.lords.LastOrDefault(l =>
                l.faction == parms.faction &&
                !(l.LordJob is LordJob_VehicleRaid) &&
                !(l.LordJob is LordJob_VehicleTrade) &&
                !(l.LordJob is LordJob_HelicopterTrade));

            Log.Message($"[VRF_Debug] SpawnVehiclesNormal — faction={parms.faction?.def?.defName} vehicleCount={vehiclePawns.Count}");
            Log.Message($"[VRF_Debug] detectedNaturalLord={(detectedNaturalLord != null ? detectedNaturalLord.LordJob?.GetType().Name : "NULL")} toil={(detectedNaturalLord?.CurLordToil?.GetType().Name ?? "NULL")}");

            if (detectedNaturalLord != null)
            {
                LordToil parentToil = detectedNaturalLord.CurLordToil;
                if (parentToil != null)
                {
                    string toilName  = parentToil.GetType().Name;
                    string jobName   = detectedNaturalLord.LordJob?.GetType().Name ?? "";
                    bool parentIsNotYetAssaulting =
                        toilName.Contains("Travel")  ||
                        toilName.Contains("Stage")   ||
                        toilName.Contains("Siege")   ||
                        toilName.Contains("Defend")  ||
                        jobName .Contains("Siege");

                    Log.Message($"[VRF_Debug] parentIsNotYetAssaulting={parentIsNotYetAssaulting} (toilName={toilName} jobName={jobName})");

                    if (parentIsNotYetAssaulting)
                    {
                        behavior     = VRF_NaturalRaidBehavior.HoldThenAssault;
                        cfgHoldTicks = 120000;
                        Log.Message($"[VRF_Debug] → Forcing HoldThenAssault (holdTicks={cfgHoldTicks})");
                    }
                }
            }

            Lord existingLord = map.lordManager.lords
                .Where(l => l.faction == parms.faction
                         && l.LordJob is LordJob_VehicleRaid
                         && (l.CurLordToil is LordToil_VehicleSearchAndDestroy
                             || l.CurLordToil is LordToil_VehicleHoldPosition))
                .OrderByDescending(l => l.ownedPawns.Count)
                .FirstOrDefault();

            Log.Message($"[VRF_Debug] existingVRFLord={(existingLord != null ? existingLord.LordJob?.GetType().Name + " toil=" + existingLord.CurLordToil?.GetType().Name : "NULL")}");
            Log.Message($"[VRF_Debug] Final behavior={behavior} holdTicks={cfgHoldTicks}");

            if (existingLord != null)
            {
                LordJob_VehicleRaid existingVJob = existingLord.LordJob as LordJob_VehicleRaid;
                if (existingVJob != null && existingVJob.naturalRaidLord == null)
                    existingVJob.naturalRaidLord = detectedNaturalLord;

                foreach (Pawn p in vehiclePawns)
                    existingLord.AddPawn(p);
                foreach (Pawn p in vehiclePawns)
                    if (p is VehiclePawn vp) Patch_VehicleNPCOnOff.UpdateVehiclePower(vp);
                existingLord.CurLordToil?.UpdateAllDuties();
                map.GetComponent<VRF_LeaderManager>()?.NotifyRaidStarted();
                DelayedDutyRefresh.Schedule(existingLord, map);
                Log.Message($"[VRF_Debug] → Added to EXISTING lord, toil={existingLord.CurLordToil?.GetType().Name}");
            }
            else
            {
                foreach (Pawn p in vehiclePawns)
                    if (p is VehiclePawn vp) Patch_VehicleNPCOnOff.UpdateVehiclePower(vp);

                LordJob_VehicleRaid vehicleLord = new LordJob_VehicleRaid(
                    parms.faction,
                    stayTicks: 35000,
                    behavior:  behavior,
                    holdTicks: behavior == VRF_NaturalRaidBehavior.HoldThenAssault ? cfgHoldTicks : 0);

                vehicleLord.naturalRaidLord = detectedNaturalLord;

                Lord newLord = LordMaker.MakeNewLord(parms.faction, vehicleLord, map, vehiclePawns);
                newLord.CurLordToil?.UpdateAllDuties();
                map.GetComponent<VRF_LeaderManager>()?.NotifyRaidStarted();
                DelayedDutyRefresh.Schedule(newLord, map);
                Log.Message($"[VRF_Debug] → Created NEW lord, startingToil={newLord.CurLordToil?.GetType().Name}");
                foreach (Pawn p in vehiclePawns)
                    if (p is VehiclePawn vp)
                        Log.Message($"[VRF_Debug]   vehicle={vp.LabelShort} duty={vp.mindState?.duty?.def?.defName ?? "NULL"} job={vp.CurJobDef?.defName ?? "NULL"}");
            }
        }

        private static void EnforceCargoSafetyLimit(VehiclePawn vehicle)
        {
            float cargoCapacity = vehicle.GetStatValue(VehicleStatDefOf.CargoCapacity);
            if (cargoCapacity <= 0f) return;
            float limit = cargoCapacity * 0.76f;
            float currentMass = MassUtility.GearAndInventoryMass(vehicle);
            if (currentMass <= limit) return;

            var items = vehicle.inventory.innerContainer
                .OrderBy(t => t.def.GetStatValueAbstract(StatDefOf.Mass))
                .ToList();

            foreach (Thing item in items)
            {
                if (currentMass <= limit) break;
                float massPerUnit = item.def.GetStatValueAbstract(StatDefOf.Mass);
                if (massPerUnit <= 0f) continue;
                float excess = currentMass - limit;
                int unitsToRemove = Mathf.Min(Mathf.CeilToInt(excess / massPerUnit), item.stackCount);
                item.stackCount -= unitsToRemove;
                currentMass -= massPerUnit * unitsToRemove;
                if (item.stackCount <= 0) item.Destroy();
            }
        }

        private static bool IsValidCellForVehicle(IntVec3 cell, VehiclePawn vehicle, Map map, Rot4 rot)
        {
            if (!cell.InBounds(map) || cell.Fogged(map)) return false;

            CellRect rect = GenAdj.OccupiedRect(cell, rot, vehicle.VehicleDef.Size);
            foreach (IntVec3 c in rect)
            {
                if (!c.InBounds(map) || !c.Standable(map)) return false;

                if (vehicle.VehicleDef.type != VehicleType.Sea && vehicle.GetComp<CompVehicleHover>() == null)
                {
                    TerrainDef terr = c.GetTerrain(map);
                    if (terr != null && (terr.IsWater || terr.IsRiver || terr.passability == Traversability.Impassable))
                        return false;
                }
            }
            return true;
        }

        private static IntVec3 FindSafeLandSpawnCell(IntVec3 root, VehiclePawn vehicle, Map map, Rot4 rot)
        {
            if (IsValidCellForVehicle(root, vehicle, map, rot)) return root;

            for (int r = 1; r <= 15; r++)
            {
                foreach (IntVec3 c in GenRadial.RadialCellsAround(root, r, true))
                {
                    if (IsValidCellForVehicle(c, vehicle, map, rot))
                        return c;
                }
            }
            return root;
        }

    }

    public static class DelayedDutyRefresh
    {
        private static readonly List<(Lord lord, Map map, int triggerTick, bool secondPass)> Pending =
            new List<(Lord, Map, int, bool)>();

        public static void Schedule(Lord lord, Map map)
        {
            Pending.RemoveAll(e => e.lord == lord);
            int firstTick = Find.TickManager.TicksGame + 60;
            Pending.Add((lord, map, firstTick, false));
        }

        public static void Tick()
        {
            if (Pending.Count == 0) return;
            int now = Find.TickManager.TicksGame;
            for (int i = Pending.Count - 1; i >= 0; i--)
            {
                var (lord, map, tick, secondPass) = Pending[i];
                if (lord == null || map == null || map.Disposed)
                {
                    Pending.RemoveAt(i);
                    continue;
                }
                if (now < tick) continue;
                Pending.RemoveAt(i);
                if (!map.lordManager.lords.Contains(lord)) continue;

                foreach (Pawn p in lord.ownedPawns)
                    if (p is VehiclePawn vp && vp.Spawned)
                        Patch_VehicleNPCOnOff.UpdateVehiclePower(vp);
                lord.CurLordToil?.UpdateAllDuties();

                if (!secondPass)
                {
                    int secondTick = now + 180;
                    Pending.Add((lord, map, secondTick, true));
                }
            }
        }
    }

    public static class VehicleNPCJobNudger
    {
        private static readonly JobDef IdleVehicle = DefDatabase<JobDef>.GetNamed("IdleVehicle", false);
        private static readonly JobDef Wait_Combat = JobDefOf.Wait_Combat;

        public static void Tick(Map map)
        {
            if (map == null || map.Disposed) return;

            foreach (Lord lord in map.lordManager.lords)
            {
                if (!(lord.LordJob is LordJob_VehicleRaid)) continue;
                if (!(lord.CurLordToil is LordToil_VehicleSearchAndDestroy)) continue;

                foreach (Pawn pawn in lord.ownedPawns)
                {
                    if (!(pawn is VehiclePawn vehicle)) continue;
                    if (!vehicle.Spawned || vehicle.Dead || vehicle.Destroyed) continue;
                    if (vehicle.mindState?.duty == null) continue;

                    if (vehicle.vehiclePather != null && vehicle.vehiclePather.Moving) continue;

                    JobDef curDef = vehicle.CurJobDef;
                    if (curDef == null) continue;

                    bool isStuck = (IdleVehicle != null && curDef == IdleVehicle)
                                || curDef == Wait_Combat;

                    if (!isStuck) continue;

                    if (!vehicle.IsHashIntervalTick(120)) continue;

                    vehicle.jobs?.EndCurrentJob(JobCondition.Succeeded, startNewJob: true);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Map), nameof(Map.MapPreTick))]
    public static class Patch_DelayedDutyRefreshTick
    {
        [HarmonyPostfix]
        public static void Postfix(Map __instance)
        {
            if (!__instance.IsHashIntervalTick(10)) return;
            DelayedDutyRefresh.Tick();
            LordAirdropLanding.Tick();
            VRF_SiegeDropBehavior.Tick();
            if (__instance.IsHashIntervalTick(120))
                VehicleNPCJobNudger.Tick(__instance);
        }
    }
}
