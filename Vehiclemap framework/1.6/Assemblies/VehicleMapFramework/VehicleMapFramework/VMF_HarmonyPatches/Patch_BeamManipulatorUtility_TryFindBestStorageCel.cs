// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_BeamManipulatorUtility_TryFindBestStorageCellCore
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_ManipulatorBeamEmitter")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_BeamManipulatorUtility_TryFindBestStorageCellCore
{
  private static bool working;

  public static void Postfix(
    Map map,
    Thing thing,
    IntVec3 referenceCell,
    ref IntVec3 destination,
    ref bool __result)
  {
    if (Patch_BeamManipulatorUtility_TryFindBestStorageCellCore.working)
      return;
    if (__result)
    {
      TargetMapUtility.set_TargetMap(thing, map);
    }
    else
    {
      Patch_BeamManipulatorUtility_TryFindBestStorageCellCore.working = true;
      referenceCell = !((IntVec3) ref referenceCell).IsValid ? VehicleMapUtility.get_PositionOnBaseMap(thing) : referenceCell.ToBaseMapCoord(map);
      try
      {
        object obj = (object) destination;
        foreach (Map mapAndVehicleMap in map.BaseMapAndVehicleMaps(false))
        {
          VehiclePawnWithMap vehicle;
          IntVec3 intVec3 = mapAndVehicleMap.IsVehicleMapOf(out vehicle) ? referenceCell.ToVehicleMapCoord(vehicle) : referenceCell;
          __result = (bool) ModCompat.ManipulatorBeamEmitter.TryFindBestStorageCellCore.Invoke((object) null, VehicleMapFramework.Params<(object, object, IntVec3, object, object)>.Get(((object) mapAndVehicleMap, (object) thing, intVec3, (object) null, obj)));
          if (__result)
          {
            destination = (IntVec3) obj;
            TargetMapUtility.set_TargetMap(thing, mapAndVehicleMap);
            return;
          }
        }
        thing.RemoveTargetInfo();
      }
      finally
      {
        Patch_BeamManipulatorUtility_TryFindBestStorageCellCore.working = false;
      }
    }
  }
}
