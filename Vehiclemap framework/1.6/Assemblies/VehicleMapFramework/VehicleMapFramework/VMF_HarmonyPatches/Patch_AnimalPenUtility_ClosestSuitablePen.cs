// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_AnimalPenUtility_ClosestSuitablePen
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (AnimalPenUtility), "ClosestSuitablePen")]
[PatchLevel(Level.Safe)]
public static class Patch_AnimalPenUtility_ClosestSuitablePen
{
  public static void Postfix(
    Pawn animal,
    bool allowUnenclosedPens,
    ref CompAnimalPenMarker __result)
  {
    if (__result != null)
      return;
    Map map = ((Thing) animal).Map;
    float num = 0.0f;
    foreach (Map mapAndVehicleMap in map.BaseMapAndVehicleMaps(false))
    {
      foreach (Thing buildingsAnimalPenMarker in mapAndVehicleMap.listerBuildings.allBuildingsAnimalPenMarkers)
      {
        CompAnimalPenMarker comp = ThingCompUtility.TryGetComp<CompAnimalPenMarker>(buildingsAnimalPenMarker);
        if (AnimalPenUtilityOnVehicle.CanUseAndReach(animal, comp, allowUnenclosedPens))
        {
          int squared = IntVec3Utility.DistanceToSquared(VehicleMapUtility.get_PositionOnBaseMap((Thing) animal), VehicleMapUtility.get_PositionOnBaseMap((Thing) ((ThingComp) comp).parent));
          if (__result == null || (double) squared < (double) num)
          {
            __result = comp;
            num = (float) squared;
          }
        }
      }
    }
  }
}
