// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_AnimalPenUtility_AnySuitablePens
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (AnimalPenUtility), "AnySuitablePens")]
[PatchLevel(Level.Safe)]
public static class Patch_AnimalPenUtility_AnySuitablePens
{
  public static void Postfix(Pawn animal, bool allowUnenclosedPens, ref bool __result)
  {
    if (__result)
      return;
    Map map = ((Thing) animal).Map;
    IntVec3 position = ((Thing) animal).Position;
    foreach (Map mapAndVehicleMap in map.BaseMapAndVehicleMaps(false))
    {
      foreach (Thing buildingsAnimalPenMarker in mapAndVehicleMap.listerBuildings.allBuildingsAnimalPenMarkers)
      {
        CompAnimalPenMarker comp = ThingCompUtility.TryGetComp<CompAnimalPenMarker>(buildingsAnimalPenMarker);
        if (AnimalPenUtilityOnVehicle.CanUseAndReach(animal, comp, allowUnenclosedPens))
        {
          __result = true;
          return;
        }
      }
    }
  }
}
