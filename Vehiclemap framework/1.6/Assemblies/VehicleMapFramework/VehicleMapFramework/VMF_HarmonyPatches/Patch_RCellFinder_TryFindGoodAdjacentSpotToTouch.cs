// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_RCellFinder_TryFindGoodAdjacentSpotToTouch
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (RCellFinder), "TryFindGoodAdjacentSpotToTouch")]
[PatchLevel(Level.Safe)]
public static class Patch_RCellFinder_TryFindGoodAdjacentSpotToTouch
{
  public static bool Prefix(Pawn toucher, Thing touchee, ref IntVec3 result, ref bool __result)
  {
    Map mapHeld = touchee.MapHeld;
    if (mapHeld == null || ((Thing) toucher).Map == mapHeld || VehicleMapUtility.get_BaseMapOrCaravan(mapHeld) != VehicleMapUtility.get_BaseMapOrCaravan((Thing) toucher))
      return true;
    __result = CrossMapRCellFinder.TryFindGoodAdjacentSpotToTouch(toucher, touchee, out result);
    return false;
  }
}
