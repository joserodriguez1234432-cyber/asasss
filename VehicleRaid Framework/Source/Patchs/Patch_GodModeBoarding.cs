using HarmonyLib;
using RimWorld;
using System;
using Vehicles;
using Verse;

namespace VehicleRaidFramework
{
    [HarmonyPatch(typeof(Vehicles.Ext_Thing), "CanBeHauledToVehicle")]
    public static class Patch_CanBeHauledToVehicle_GodMode
    {
        public static void Postfix(Thing thing, ref bool __result)
        {
            if (DebugSettings.godMode && !__result && thing is Pawn p && !p.Dead && p.Spawned && !(p is VehiclePawn))
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(Vehicles.HaulTargeter), "BeginTargeting")]
    public static class Patch_HaulTargeter_GodMode
    {
        public static void Prefix(TargetingParameters targetParams)
        {
            if (DebugSettings.godMode && targetParams != null)
            {
                targetParams.neverTargetHostileFaction = false;

                Predicate<TargetInfo> originalValidator = targetParams.validator;
                if (originalValidator != null)
                {
                    targetParams.validator = (TargetInfo target) =>
                    {
                        if (!target.HasThing || !(target.Thing is Pawn thing2) || thing2 is VehiclePawn)
                            return false;

                        if (DebugSettings.godMode)
                            return true;

                        return originalValidator(target);
                    };
                }
            }
        }
    }

    [HarmonyPatch(typeof(Vehicles.Command_TransferToVehicle_Order), "IsValidVehicle")]
    public static class Patch_Command_TransferToVehicle_Order_GodMode
    {
        public static void Postfix(TargetInfo target, ref bool __result)
        {
            if (DebugSettings.godMode && !__result)
            {
                if (target.Thing is VehiclePawn vehicle)
                {
                    Vehicles.CompUpgradeTree compUpgradeTree = vehicle.CompUpgradeTree;
                    if (compUpgradeTree == null || !compUpgradeTree.Upgrading)
                    {
                        __result = true;
                    }
                }
            }
        }
    }
}
