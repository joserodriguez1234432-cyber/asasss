using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Vehicles;
using Verse;

namespace VehicleRaidFramework.VehicleMapFramework
{
    /// <summary>
    /// Vehicle Map Framework gives each map vehicle a pre-generated, unique VehicleDef.
    /// Its gravship base ships with only four placeholders, which made the fifth live
    /// vehicle fall back to the shared base definition. A shared dynamic definition is
    /// invalid: its size, graphics and map properties belong to another vehicle.
    ///
    /// Expand an exhausted pool before VMF claims a definition. This applies to every
    /// VehiclePawnWithMap, not only gravships, and only creates extra placeholders when
    /// the existing pool has actually been used up.
    /// </summary>
    [HarmonyPatch]
    [HarmonyPriority(Priority.First)]
    public static class Patch_VMF_UniqueVehicleCapacity
    {
        private const int ExpansionBatchSize = 8;

        public static bool Prepare()
        {
            return VRF_VehicleMapCompat.IsVMFActive && AccessTools.TypeByName("VehicleMapFramework.UniqueVehicleManager") != null;
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("VehicleMapFramework.UniqueVehicleManager:ClaimUniqueVehicleDef");
        }

        public static void Prefix(VehicleDef parentDef)
        {
            if (parentDef == null || parentDef.thingClass == null)
                return;

            Type vehiclePawnWithMapType = AccessTools.TypeByName("VehicleMapFramework.VehiclePawnWithMap");
            if (vehiclePawnWithMapType == null || !vehiclePawnWithMapType.IsAssignableFrom(parentDef.thingClass))
                return;

            try
            {
                Type uvmType = AccessTools.TypeByName("VehicleMapFramework.UniqueVehicleManager");
                if (uvmType == null) return;

                FieldInfo placeholderDefsField = AccessTools.Field(uvmType, "PlaceholderDefs");
                if (placeholderDefsField == null) return;

                var placeholderDefs = placeholderDefsField.GetValue(null) as IDictionary;
                if (placeholderDefs == null || !placeholderDefs.Contains(parentDef))
                    return;

                var placeholdersList = placeholderDefs[parentDef] as List<VehicleDef>;
                if (placeholdersList == null)
                    return;

                var game = Current.Game;
                if (game == null) return;

                MethodInfo getCompGeneric = typeof(Game).GetMethod(nameof(Game.GetComponent), Type.EmptyTypes);
                if (getCompGeneric == null) return;
                object manager = getCompGeneric.MakeGenericMethod(uvmType).Invoke(game, null);
                if (manager == null) return;

                MethodInfo claimedCountMethod = AccessTools.Method(uvmType, "ClaimedCount", new[] { typeof(VehicleDef) });
                if (claimedCountMethod == null) return;

                int claimed = (int)claimedCountMethod.Invoke(manager, new object[] { parentDef });
                if (claimed < placeholdersList.Count)
                    return;

                int firstIndex = placeholdersList.Count;
                int added = 0;

                Type uvuType = AccessTools.TypeByName("VehicleMapFramework.UniqueVehicleUtility");
                MethodInfo generateMethod = AccessTools.Method(uvuType, "GenerateUniqueVehicleDef", new[] { typeof(VehicleDef), typeof(int) });

                for (int index = firstIndex; index < firstIndex + ExpansionBatchSize; index++)
                {
                    if (generateMethod == null) break;
                    VehicleDef uniqueDef = generateMethod.Invoke(null, new object[] { parentDef, index }) as VehicleDef;
                    if (uniqueDef == null || uniqueDef == parentDef)
                        break;

                    if (!placeholdersList.Contains(uniqueDef))
                    {
                        placeholdersList.Add(uniqueDef);
                        added++;
                    }
                }

                if (added == 0)
                    Log.Error($"[VehicleRaidFramework] Vehicle Map Framework could not expand the unique definition pool for {parentDef.defName}. The vehicle will not be spawned.");
                else
                    VRF_Log.Msg($"Expanded VMF unique vehicle pool for {parentDef.defName}: {firstIndex} -> {placeholdersList.Count}.");
            }
            catch (Exception ex)
            {
                Log.Error($"[VehicleRaidFramework] Failed to expand the VMF unique definition pool for {parentDef.defName}: {ex}");
            }
        }
    }
}
