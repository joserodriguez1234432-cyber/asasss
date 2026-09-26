using HarmonyLib;
using Verse;
using Vehicles;

namespace VehicleRaidFramework
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            var harmony = new Harmony("VRF.VehicleRaidFramework");
            harmony.PatchAll();

            Patch_VVE_MovementController_NPC.TryApply(harmony);

            var spawnPawnLifestage = AccessTools.Method(typeof(DebugToolsSpawning), "SpawnPawnWithLifestage");
            var spawnNewborn       = AccessTools.Method(typeof(DebugToolsSpawning), "SpawnNewborn");
            var spawnChild         = AccessTools.Method(typeof(DebugToolsSpawning), "SpawnChild");
            var filterPostfix      = new HarmonyMethod(typeof(HarmonyInit), nameof(FilterVehiclesFromSpawnList));

            if (spawnPawnLifestage != null) harmony.Patch(spawnPawnLifestage, postfix: filterPostfix);
            if (spawnNewborn       != null) harmony.Patch(spawnNewborn,       postfix: filterPostfix);
            if (spawnChild         != null) harmony.Patch(spawnChild,         postfix: filterPostfix);

            if (VRF_Mod.Settings != null && VRF_Mod.Settings.autoLoadPresets)
                VRF_PresetIO.AutoLoadAllPresets(VRF_Mod.Settings);

            if (VRF_Mod.Settings != null)
                VRF_PresetIO.ApplyHoverConfigsToVehicleDefs(VRF_Mod.Settings);
        }

        private static void FilterVehiclesFromSpawnList(System.Collections.Generic.List<LudeonTK.DebugActionNode> __result)
        {
            if (__result == null) return;
            for (int i = __result.Count - 1; i >= 0; i--)
            {
                string lbl = __result[i]?.label;
                if (lbl == null) continue;
                PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamedSilentFail(lbl);
                if (kind != null && kind.race is VehicleDef)
                {
                    __result.RemoveAt(i);
                    continue;
                }
                if (kind == null)
                {
                    foreach (PawnKindDef k in DefDatabase<PawnKindDef>.AllDefsListForReading)
                    {
                        if (k.race is VehicleDef && (k.label ?? k.defName) == lbl)
                        {
                            __result.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }
    }
}



