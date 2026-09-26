using System;
using System.Collections.Generic;
using HarmonyLib;
using Vehicles;
using Verse;

namespace VehicleRaidFramework.VehicleMapFramework
{
    /// <summary>
    /// Vehicle Map Framework gives each map vehicle a pre-generated, unique VehicleDef.
    /// Its gravship base ships with only four placeholders, which made the fifth live
    /// vehicle fall back to the shared base definition.  A shared dynamic definition is
    /// invalid: its size, graphics and map properties belong to another vehicle.
    ///
    /// Expand an exhausted pool before VMF claims a definition.  This applies to every
    /// VehiclePawnWithMap, not only gravships, and only creates extra placeholders when
    /// the existing pool has actually been used up.
    /// </summary>
    [HarmonyPatch(typeof(global::VehicleMapFramework.UniqueVehicleManager), nameof(global::VehicleMapFramework.UniqueVehicleManager.ClaimUniqueVehicleDef))]
    [HarmonyPriority(Priority.First)]
    public static class Patch_VMF_UniqueVehicleCapacity
    {
        // Creating a small batch keeps the normal four-placeholder setup lightweight,
        // while avoiding repeated definition creation during a large raid.
        private const int ExpansionBatchSize = 8;

        public static void Prefix(VehicleDef parentDef)
        {
            if (parentDef == null || parentDef.thingClass == null ||
                !typeof(global::VehicleMapFramework.VehiclePawnWithMap).IsAssignableFrom(parentDef.thingClass))
                return;

            try
            {
                if (!global::VehicleMapFramework.UniqueVehicleManager.PlaceholderDefs.TryGetValue(parentDef, out List<VehicleDef> placeholders))
                    return;

                var manager = Current.Game?.GetComponent<global::VehicleMapFramework.UniqueVehicleManager>();
                if (manager == null || manager.ClaimedCount(parentDef) < placeholders.Count)
                    return;

                int firstIndex = placeholders.Count;
                int added = 0;
                for (int index = firstIndex; index < firstIndex + ExpansionBatchSize; index++)
                {
                    VehicleDef uniqueDef = global::VehicleMapFramework.UniqueVehicleUtility.GenerateUniqueVehicleDef(parentDef, index);
                    if (uniqueDef == null || uniqueDef == parentDef)
                        break;

                    if (!placeholders.Contains(uniqueDef))
                    {
                        placeholders.Add(uniqueDef);
                        added++;
                    }
                }

                if (added == 0)
                    Log.Error($"[VehicleRaidFramework] Vehicle Map Framework could not expand the unique definition pool for {parentDef.defName}. The vehicle will not be spawned.");
                else
                    VRF_Log.Msg($"Expanded VMF unique vehicle pool for {parentDef.defName}: {firstIndex} -> {placeholders.Count}.");
            }
            catch (Exception ex)
            {
                Log.Error($"[VehicleRaidFramework] Failed to expand the VMF unique definition pool for {parentDef.defName}: {ex}");
            }
        }
    }
}
