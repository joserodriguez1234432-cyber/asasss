using HarmonyLib;
using Vehicles;
using Verse;
using RimWorld;
using System.Reflection;
using System.Collections.Generic;
using SmashTools.Rendering;
using Vehicles.Rendering;
using UnityEngine;

namespace VehicleRaid
{
    [HarmonyPatch(typeof(VehicleTurret), "TurretTargetValid", MethodType.Getter)]
    public static class VehicleHover_TurretTargetValid_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(VehicleTurret __instance, ref bool __result)
        {
            if (!__result) return;

            VehiclePawn vehicle = __instance.vehicle;
            if (vehicle == null) return;

            CompVehicleHover hoverComp = vehicle.GetComp<CompVehicleHover>();
            if (hoverComp == null || !hoverComp.IsAirborne) return;

            LocalTargetInfo target = __instance.targetInfo;
            if (!target.IsValid) return;

            Map map = vehicle.Map;
            if (map == null) return;

            IntVec3 targetCell;
            if (target.HasThing)
            {
                if (target.Thing == null || target.Thing.Destroyed || !target.Thing.Spawned) return;
                targetCell = target.Thing.Position;
            }
            else
            {
                targetCell = target.Cell;
            }

            if (!targetCell.InBounds(map)) return;

            RoofDef roof = map.roofGrid.RoofAt(targetCell);
            if (roof == null) return;

            if (HoverRoofUtil.IsBlockingRoof(roof))
            {
                __result = false;
                __instance.SetTarget(LocalTargetInfo.Invalid);
            }
        }
    }

    [HarmonyPatch(typeof(VehicleTurret), "ScanForTarget")]
    public static class VehicleHover_ScanForTarget_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(VehicleTurret __instance)
        {
            VehiclePawn vehicle = __instance.vehicle;
            if (vehicle == null) return true;

            CompVehicleHover hoverComp = vehicle.GetComp<CompVehicleHover>();
            if (hoverComp == null || !hoverComp.IsAirborne) return true;

            Map map = vehicle.Map;
            if (map == null) return true;

            LocalTargetInfo current = __instance.targetInfo;
            if (current.IsValid && current.HasThing && current.Thing != null && current.Thing.Spawned)
            {
                RoofDef roof = map.roofGrid.RoofAt(current.Thing.Position);
                if (roof != null && HoverRoofUtil.IsBlockingRoof(roof))
                {
                    __instance.SetTarget(LocalTargetInfo.Invalid);
                    return false;
                }
            }

            return true;
        }
    }

    internal static class HoverRoofUtil
    {
        public static bool IsBlockingRoof(RoofDef roof)
        {
            if (roof == RoofDefOf.RoofConstructed) return false;
            if (roof == RoofDefOf.RoofRockThin) return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(VehicleTurret), "TurretRotation", MethodType.Getter)]
    public static class VehicleHover_TurretRotation_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(VehicleTurret __instance, ref float __result)
        {
            var vehicle = __instance.vehicle;
            if (vehicle == null) return;
            var hoverComp = vehicle.GetComp<CompVehicleHover>();
            if (hoverComp == null || hoverComp.State == HoverState.Grounded) return;

            if (!__instance.IsTargetable && __instance.attachedTo == null)
            {
                __result += vehicle.Transform.rotation;
            }
        }
    }

    [HarmonyPatch(typeof(VehicleTurret), "ParallelPreRenderResults")]
    public static class VehicleHover_ParallelPreRenderResults_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(VehicleTurret __instance, ref PreRenderResults __result)
        {
            if (!__result.valid || !__result.draw) return;
            var vehicle = __instance.vehicle;
            if (vehicle == null) return;
            var hoverComp = vehicle.GetComp<CompVehicleHover>();
            if (hoverComp == null || hoverComp.State == HoverState.Grounded) return;


            __result.quaternion = Quaternion.Euler(0f, -vehicle.Transform.rotation, 0f) * __result.quaternion;
        }
    }
}
