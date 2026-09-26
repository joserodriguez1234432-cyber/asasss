// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_AnimalPenUtility_AnySuitableHitch
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (AnimalPenUtility), "AnySuitableHitch")]
[PatchLevel(Level.Safe)]
public static class Patch_AnimalPenUtility_AnySuitableHitch
{
  public static void Postfix(Pawn animal, ref bool __result)
  {
    if (__result)
      return;
    foreach (Map mapAndVehicleMap in ((Thing) animal).Map.BaseMapAndVehicleMaps(false))
    {
      foreach (Building buildingsAnimalPenMarker in mapAndVehicleMap.listerBuildings.allBuildingsAnimalPenMarkers)
      {
        if (animal.CanReach(LocalTargetInfo.op_Implicit((Thing) buildingsAnimalPenMarker), (PathEndMode) 2, (Danger) 3, false, false, (TraverseMode) 0, ((Thing) buildingsAnimalPenMarker).Map))
        {
          __result = true;
          return;
        }
      }
    }
  }
}
