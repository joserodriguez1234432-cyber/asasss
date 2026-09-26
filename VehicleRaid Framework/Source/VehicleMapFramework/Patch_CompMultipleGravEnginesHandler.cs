using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Reflection;

namespace VehicleRaidFramework
{
    [HarmonyPatch]
    public static class Patch_CompMultipleGravEnginesHandler_ActiveGravEngines
    {
        public static bool Prepare()
        {
            // Only patch if Vanilla Gravship Expanded is active (type exists)
            return AccessTools.TypeByName("VanillaGravshipExpanded.CompMultipleGravEnginesHandler") != null;
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(AccessTools.TypeByName("VanillaGravshipExpanded.CompMultipleGravEnginesHandler"), "MultipleGravEnginesPresent");
        }

        public static bool Prefix(ref bool __result)
        {
            // We want to count only engines that are on player maps (not vehicle/enemy maps).
            // Since the original property just does ActiveGravEngineCount >= 2, we recalculate it.
            var activeEnginesField = AccessTools.Field(AccessTools.TypeByName("VanillaGravshipExpanded.CompMultipleGravEnginesHandler"), "ActiveGravEngines");
            if (activeEnginesField != null)
            {
                var activeEngines = activeEnginesField.GetValue(null) as IEnumerable<ThingComp>;
                if (activeEngines != null)
                {
                    int count = 0;
                    foreach (var comp in activeEngines)
                    {
                        if (comp.parent != null && comp.parent.Spawned)
                        {
                            // If it's an enemy engine on a vehicle map, don't count it towards the limit!
                            if (comp.parent.Faction != Faction.OfPlayer || comp.parent.Map.Parent.GetType().Name == "VehiclePawnWithMap")
                            {
                                continue;
                            }
                            count++;
                        }
                    }
                    __result = count >= 2;
                    return false; // Skip original
                }
            }
            return true;
        }
    }
}
