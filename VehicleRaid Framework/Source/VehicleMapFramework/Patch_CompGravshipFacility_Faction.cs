using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VehicleRaidFramework
{
    [HarmonyPatch(typeof(CompGravshipFacility), "CanBeActive", MethodType.Getter)]
    public static class Patch_CompGravshipFacility_CanBeActive
    {
        public static void Postfix(CompGravshipFacility __instance, ref bool __result)
        {
            if (__result) return;
            if (!__instance.parent.Spawned) return;

            // If the facility is not player faction, vanilla logic fails because it uses AllBuildingsColonistOfClass.
            // We need to check engines of the SAME faction.
            if (__instance.parent.Faction != Faction.OfPlayer)
            {
                var allEngines = __instance.parent.Map.listerThings.AllThings.OfType<Building_GravEngine>();
                foreach (Building_GravEngine tmpEngine in allEngines)
                {
                    if (tmpEngine.Faction == __instance.parent.Faction)
                    {
                        if (__instance.parent.Position.InHorDistOf(tmpEngine.Position, __instance.Props.maxDistance))
                        {
                            if (__instance.Props.onlyRequiresLooseConnection && tmpEngine.LooselyConnectedToGravEngine(__instance.parent) || tmpEngine.OnValidSubstructure(__instance.parent))
                            {
                                __result = true;
                                return;
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(CompGravshipFacility), "CompInspectStringExtra")]
    public static class Patch_CompGravshipFacility_CompInspectStringExtra
    {
        public static void Postfix(CompGravshipFacility __instance, ref string __result)
        {
            if (!__instance.parent.Spawned || __instance.parent.Faction == Faction.OfPlayer) return;

            // Vanilla logic might add "NotConnectedToGravEngine" or "MessageMustBePlacedInRangeOfGravEngine" 
            // because it only checked colonist engines. If our patched CanBeActive is true, we should remove those errors.
            if (__instance.CanBeActive)
            {
                string notConnectedStr = "NotConnectedToGravEngine".Translate().Colorize(Verse.ColorLibrary.RedReadable);
                string notInRangeStr = "MessageMustBePlacedInRangeOfGravEngine".Translate().Colorize(Verse.ColorLibrary.RedReadable);
                
                if (__result.Contains(notConnectedStr))
                {
                    __result = __result.Replace("\n" + notConnectedStr, "").Replace(notConnectedStr, "");
                }
                if (__result.Contains(notInRangeStr))
                {
                    __result = __result.Replace("\n" + notInRangeStr, "").Replace(notInRangeStr, "");
                }
                __result = __result.TrimEnd('\n', '\r');
            }
        }
    }
}
