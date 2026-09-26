using HarmonyLib;
using Vehicles;
using Verse;
using Verse.AI.Group;
using RimWorld;

namespace VehicleRaidFramework
{
    [HarmonyPatch(typeof(VehicleTurret), nameof(VehicleTurret.ConsumeChamberedShot))]
    public static class Patch_NPCBreacherAmmo
    {
        [HarmonyPrefix]
        public static bool Prefix(VehicleTurret __instance)
        {
            VehiclePawn vehicle = __instance.vehicle;

            if (vehicle == null || vehicle.Faction == null || vehicle.Faction.IsPlayer)
                return true;

            Thing target = __instance.targetInfo.Thing;
            if (target == null && __instance.targetInfo.Cell.IsValid && vehicle.Map != null)
            {
                target = __instance.targetInfo.Cell.GetEdifice(vehicle.Map);
            }

            if (target is Building)
            {
                Lord lord = vehicle.GetLord();
                if (lord?.LordJob is LordJob_VehicleRaid)
                    return false;

                if (lord?.LordJob != null && (lord.LordJob is LordJob_AssaultColony || 
                    lord.LordJob.GetType().Name.Contains("AssaultColony") || 
                    lord.LordJob.GetType().Name.Contains("Raid")))
                    return false;
            }

            return true;
        }
    }
}
