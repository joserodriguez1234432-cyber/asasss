using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Vehicles;

namespace VehicleRaidFramework.VehicleMapFramework
{
    public static class VRF_GravshipSpeedUtility
    {
        public const float SmallThrusterDefaultRange = 10f;
        public const float SmallThrusterDefaultSpeed = 1.0f;
        public const float DefaultSpeedPerRangeCell = SmallThrusterDefaultSpeed / SmallThrusterDefaultRange; // 0.01f per cell
        public const float DefaultFallbackSpeed = 0.5f;

        public static bool IsThrusterDef(ThingDef def)
        {
            if (def == null || def.comps == null) return false;
            for (int i = 0; i < def.comps.Count; i++)
            {
                var compProps = def.comps[i];
                if (compProps is CompProperties_GravshipThruster) return true;
                if (compProps is CompProperties_GravshipFacility fac && fac.componentTypeDef != null)
                {
                    if (string.Equals(fac.componentTypeDef.defName, "Thruster", StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            return false;
        }

        public static float GetRangeForThrusterDef(ThingDef def)
        {
            if (def == null || def.comps == null) return 0f;
            for (int i = 0; i < def.comps.Count; i++)
            {
                var compProps = def.comps[i];
                if (compProps is CompProperties_GravshipThruster thrusterProps)
                {
                    if (thrusterProps.statOffsets != null)
                    {
                        var rangeStat = DefDatabase<StatDef>.GetNamedSilentFail("GravshipRange");
                        if (rangeStat != null)
                        {
                            for (int s = 0; s < thrusterProps.statOffsets.Count; s++)
                            {
                                if (thrusterProps.statOffsets[s].stat == rangeStat)
                                    return thrusterProps.statOffsets[s].value;
                            }
                        }
                    }
                    return SmallThrusterDefaultRange;
                }
            }
            return 0f;
        }

        public static float GetDefaultSpeedForThruster(ThingDef def)
        {
            if (def == null) return SmallThrusterDefaultSpeed;
            float range = GetRangeForThrusterDef(def);
            if (range > 0f)
            {
                return Mathf.Round(range * DefaultSpeedPerRangeCell * 1000f) / 1000f;
            }
            return SmallThrusterDefaultSpeed;
        }

        public static float GetConfiguredSpeedForThruster(ThingDef def)
        {
            if (def == null) return SmallThrusterDefaultSpeed;
            var settings = VRF_Mod.Settings;
            if (settings != null && settings.thrusterSpeedSettings != null &&
                settings.thrusterSpeedSettings.TryGetValue(def.defName, out float customSpeed))
            {
                return customSpeed;
            }
            return GetDefaultSpeedForThruster(def);
        }

        public static void SetConfiguredSpeedForThruster(ThingDef def, float speed)
        {
            if (def == null) return;
            var settings = VRF_Mod.Settings;
            if (settings == null) return;
            if (settings.thrusterSpeedSettings == null)
            {
                settings.thrusterSpeedSettings = new Dictionary<string, float>();
            }
            settings.thrusterSpeedSettings[def.defName] = Mathf.Max(0.01f, Mathf.Round(speed * 100f) / 100f);
            VRF_Mod.Instance.WriteSettings();
        }

        public static List<ThingDef> GetAllThrusterDefs()
        {
            var list = new List<ThingDef>();
            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (IsThrusterDef(def))
                {
                    list.Add(def);
                }
            }
            return list;
        }

        public static float CalculateGravshipSpeed(VehiclePawn vehicle)
        {
            if (vehicle == null) return DefaultFallbackSpeed;

            // 1. Direct runtime check on VehicleMap (pocket map) via compatibility layer
            Map vehicleMap = VRF_VehicleMapCompat.GetInteriorVehicleMap(vehicle);
            if (vehicleMap != null && vehicleMap.listerThings != null)
            {
                float totalSpeed = 0f;
                int thrusterCount = 0;

                var allThings = vehicleMap.listerThings.AllThings;
                for (int i = 0; i < allThings.Count; i++)
                {
                    Thing t = allThings[i];
                    if (t is Building b && !b.Destroyed && IsThrusterDef(b.def))
                    {
                        var breakdown = b.TryGetComp<CompBreakdownable>();
                        if (breakdown != null && breakdown.BrokenDown)
                        {
                            continue;
                        }

                        totalSpeed += GetConfiguredSpeedForThruster(b.def);
                        thrusterCount++;
                    }
                }

                if (thrusterCount > 0 && totalSpeed > 0f)
                {
                    return Mathf.Max(0.05f, Mathf.Round(totalSpeed * 100f) / 100f);
                }
            }

            // 2. Check preset assigned to this vehicle in faction configs
            var settings = VRF_Mod.Settings;
            if (settings != null && settings.factionConfigs != null && vehicle != null)
            {
                string vDefName = vehicle.def?.defName;
                string vKindName = vehicle.kindDef?.defName;

                for (int f = 0; f < settings.factionConfigs.Count; f++)
                {
                    var fConfig = settings.factionConfigs[f];
                    if (fConfig.vehicleEntries == null) continue;
                    for (int e = 0; e < fConfig.vehicleEntries.Count; e++)
                    {
                        var entry = fConfig.vehicleEntries[e];
                        if (!string.IsNullOrEmpty(entry.vehicleKindDefName) &&
                            (entry.vehicleKindDefName == vDefName || entry.vehicleKindDefName == vKindName) &&
                            !string.IsNullOrEmpty(entry.gravshipPresetName))
                        {
                            float presetSpeed = CalculatePresetSpeed(entry.gravshipPresetName);
                            if (presetSpeed > 0f) return presetSpeed;
                        }
                    }
                }
            }

            return DefaultFallbackSpeed;
        }

        public static float CalculatePresetSpeed(string presetName)
        {
            if (string.IsNullOrEmpty(presetName)) return DefaultFallbackSpeed;

            var allFiles = VRF_GravshipPresetUtility.FindAllGravshipPresetFiles();
            string targetFile = null;
            for (int i = 0; i < allFiles.Count; i++)
            {
                if (System.IO.Path.GetFileNameWithoutExtension(allFiles[i]) == presetName)
                {
                    targetFile = allFiles[i];
                    break;
                }
            }

            if (targetFile != null)
            {
                try
                {
                    var presetData = VRF_GravshipPresetUtility.LoadGravshipPresetFromFile(targetFile);
                    if (presetData != null && presetData.buildings != null)
                    {
                        float totalSpeed = 0f;
                        int count = 0;
                        for (int i = 0; i < presetData.buildings.Count; i++)
                        {
                            var bData = presetData.buildings[i];
                            if (!string.IsNullOrEmpty(bData.defName))
                            {
                                ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(bData.defName);
                                if (def != null && IsThrusterDef(def))
                                {
                                    totalSpeed += GetConfiguredSpeedForThruster(def);
                                    count++;
                                }
                            }
                        }
                        if (count > 0 && totalSpeed > 0f)
                        {
                            return Mathf.Max(0.05f, Mathf.Round(totalSpeed * 100f) / 100f);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning($"[VehicleRaidFramework] Error calculating speed for preset {presetName}: {ex.Message}");
                }
            }

            return DefaultFallbackSpeed;
        }
    }
}
