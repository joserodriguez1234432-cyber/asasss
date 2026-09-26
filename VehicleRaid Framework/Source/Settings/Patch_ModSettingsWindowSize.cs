using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VehicleRaidFramework
{
    [HarmonyPatch(typeof(Dialog_ModSettings), "get_InitialSize")]
    public static class Patch_ModSettingsWindowSize
    {
        public static void Postfix(Dialog_ModSettings __instance, ref Vector2 __result)
        {
            if (!(__instance.mod is VRF_Mod)) return;
            float w = Mathf.Min(__result.x * 1.4f, UI.screenWidth  * 0.95f);
            float h = Mathf.Min(__result.y * 1.4f, UI.screenHeight * 0.95f);
            __result = new Vector2(w, h);
        }
    }
}
