using HarmonyLib;
using RimWorld;
using Vehicles;
using Verse;
using Verse.AI.Group;
using Verse.Sound;
using System;
using System.Reflection;

namespace VehicleRaidFramework
{
    [StaticConstructorOnStartup]
    public static class Patch_VVE_Handbrake
    {
        private static Type compType;
        private static PropertyInfo vehicleProp;
        private static FieldInfo curMovementModeField;
        private static FieldInfo currentSpeedField;

        static Patch_VVE_Handbrake()
        {
            try { ApplyPatches(); } catch { }
        }

        private static void ApplyPatches()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    compType = asm.GetType("VanillaVehiclesExpanded.CompVehicleMovementController");
                    if (compType != null) break;
                }
                catch { }
            }

            if (compType == null) return;

            vehicleProp = compType.GetProperty("Vehicle",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            curMovementModeField = AccessTools.Field(compType, "curMovementMode");
            currentSpeedField = AccessTools.Field(compType, "currentSpeed");

            var harmony = new Harmony("com.vrf.patches.vve.handbrake");

            MethodInfo slowdownMethod = compType.GetMethod("Slowdown",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (slowdownMethod != null)
                harmony.Patch(slowdownMethod,
                    prefix: new HarmonyMethod(typeof(Patch_VVE_Handbrake), nameof(Slowdown_Prefix)),
                    postfix: new HarmonyMethod(typeof(Patch_VVE_Handbrake), nameof(Slowdown_Postfix)));

            MethodInfo compTickMethod = compType.GetMethod("CompTick",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (compTickMethod != null)
                harmony.Patch(compTickMethod,
                    postfix: new HarmonyMethod(typeof(Patch_VVE_Handbrake), nameof(CompTick_Postfix)));
        }

        private static bool IsVRFNPC(object instance)
        {
            if (vehicleProp == null) return false;
            var vehicle = vehicleProp.GetValue(instance) as VehiclePawn;
            if (vehicle == null || vehicle.Faction == null || vehicle.Faction.IsPlayer) return false;
            Lord lord = vehicle.GetLord();
            return lord?.LordJob is LordJob_VehicleRaid
                || lord?.LordJob is LordJob_VehicleTrade
                || lord?.LordJob is LordJob_HelicopterTrade;
        }

        private static Type settingsType;
        private static FieldInfo handbrakeDealsDamageField;

        private static void InitSettings()
        {
            if (settingsType != null) return;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    settingsType = asm.GetType("VanillaVehiclesExpanded.VanillaVehiclesExpandedSettings");
                    if (settingsType != null) break;
                }
                catch { }
            }
            if (settingsType != null)
                handbrakeDealsDamageField = settingsType.GetField("handbrakeDealsDamage",
                    BindingFlags.Public | BindingFlags.Static);
        }

        public static bool Slowdown_Prefix(object __instance, bool stopImmediately, out bool __state)
        {
            __state = false;
            try
            {
                if (!IsVRFNPC(__instance)) return true;

                if (!stopImmediately)
                {
                    var vehicle = vehicleProp.GetValue(__instance) as VehiclePawn;
                    if (vehicle?.vehiclePather?.curPath == null) return true;
                    if (vehicle.vehiclePather.curPath.NodesLeft <= 3) goto allowWithSuppression;
                    ClearScreeching(__instance);
                    return false;
                }

                allowWithSuppression:
                InitSettings();
                if (handbrakeDealsDamageField != null && (bool)handbrakeDealsDamageField.GetValue(null))
                {
                    handbrakeDealsDamageField.SetValue(null, false);
                    __state = true;
                }
                return true;
            }
            catch { return true; }
        }

        public static void Slowdown_Postfix(object __instance, bool __state)
        {
            try
            {
                if (!__state) return;
                InitSettings();
                if (handbrakeDealsDamageField != null)
                    handbrakeDealsDamageField.SetValue(null, true);
                ClearScreeching(__instance);
            }
            catch { }
        }

        private static void ClearScreeching(object instance)
        {
            try
            {
                var isScreechingField = instance.GetType().GetField("isScreeching",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var sustainerField = instance.GetType().GetField("screechingSustainer",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (isScreechingField != null)
                    isScreechingField.SetValue(instance, false);

                if (sustainerField != null)
                {
                    var sustainer = sustainerField.GetValue(instance) as Sustainer;
                    if (sustainer != null && !sustainer.Ended)
                        sustainer.End();
                    sustainerField.SetValue(instance, null);
                }
            }
            catch { }
        }

        public static void CompTick_Postfix(object __instance)
        {
            try
            {
                if (!IsVRFNPC(__instance)) return;
                if (curMovementModeField == null || currentSpeedField == null) return;

                var vehicle = vehicleProp.GetValue(__instance) as VehiclePawn;
                if (vehicle == null || !vehicle.vehiclePather.Moving) return;

                int modeVal = Convert.ToInt32(curMovementModeField.GetValue(__instance));
                if (modeVal != 3) return;

                float speed = (float)currentSpeedField.GetValue(__instance);
                float maxSpeed = vehicle.GetStatValue(VehicleStatDefOf.MoveSpeed);

                if (speed < maxSpeed * 0.9f)
                {
                    curMovementModeField.SetValue(__instance, Enum.ToObject(curMovementModeField.FieldType, 1));
                    ClearScreeching(__instance);
                }
            }
            catch { }
        }
    }
}
