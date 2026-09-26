// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_PathingHelper_TryFindNearestStandableCell
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (PathingHelper), "TryFindNearestStandableCell")]
[PatchLevel(Level.Safe)]
public static class Patch_PathingHelper_TryFindNearestStandableCell
{
  public static bool Prefix(
    VehiclePawn vehicle,
    IntVec3 cell,
    ref IntVec3 result,
    ref float radius,
    ref bool __result)
  {
    if ((double) radius < 0.0)
      radius = (float) (Mathf.Min(((BuildableDef) vehicle.VehicleDef).Size.x, ((BuildableDef) vehicle.VehicleDef).Size.z) * 2);
    radius = Mathf.Min(radius, GenRadial.MaxRadialPatternRadius);
    VehiclePawnWithMap vehicle1 = (VehiclePawnWithMap) null;
    Map map;
    if (((Thing) vehicle).TryGetTargetMap(out map))
    {
      if (((Thing) vehicle).Map != map)
      {
        __result = CrossMapReachabilityUtility.TryFindNearestStandableCell(vehicle, cell, map, out result, radius);
        if (((IntVec3) ref result).IsValid)
          return false;
      }
    }
    else if (GenGrid.InBounds(cell, Find.CurrentMap) && cell.TryGetVehicleMap(Find.CurrentMap, out vehicle1) || ((Thing) vehicle).IsOnNonFocusedVehicleMapOf(out VehiclePawnWithMap _))
    {
      IntVec3 cell1 = vehicle1 != null ? cell.ToVehicleMapCoord(vehicle1) : cell;
      map = vehicle1 != null ? vehicle1.VehicleMap : Find.CurrentMap;
      __result = CrossMapReachabilityUtility.TryFindNearestStandableCell(vehicle, cell1, map, out result, radius);
      if (((IntVec3) ref result).IsValid)
      {
        TargetMapUtility.set_TargetMap((Thing) vehicle, map);
        return false;
      }
    }
    return true;
  }
}
