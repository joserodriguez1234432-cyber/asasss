// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_RCellFinder_BestOrderedGotoDestNear
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (RCellFinder), "BestOrderedGotoDestNear")]
[PatchLevel(Level.Safe)]
public static class Patch_RCellFinder_BestOrderedGotoDestNear
{
  public static bool Prefix(
    IntVec3 root,
    Pawn searcher,
    Predicate<IntVec3> cellValidator,
    bool reachable,
    ref IntVec3 __result)
  {
    VehiclePawnWithMap vehicle1 = (VehiclePawnWithMap) null;
    VehiclePawnWithMap vehicle2 = (VehiclePawnWithMap) null;
    Map map1;
    if (((Thing) searcher).TryGetTargetMap(out map1))
    {
      __result = CrossMapRCellFinder.BestOrderedGotoDestNear(root, searcher, cellValidator, reachable, map1);
      if (((IntVec3) ref __result).IsValid)
      {
        TargetMapUtility.set_TargetInfo((Thing) searcher, new TargetInfo(__result, map1, false));
        return false;
      }
    }
    else if (GenGrid.InBounds(root, Find.CurrentMap) && root.TryGetVehicleMap(Find.CurrentMap, out vehicle1) || ((Thing) searcher).IsOnNonFocusedVehicleMapOf(out vehicle2))
    {
      if (vehicle1 == null && (vehicle2 == null || !((Thing) vehicle2).Spawned))
        UI.MouseMapPosition().TryGetVehicleMap(Find.CurrentMap, out vehicle1, VehicleMapFlag.None);
      IntVec3 root1 = vehicle1 != null ? root.ToVehicleMapCoord(vehicle1) : root;
      Map map2 = vehicle1 != null ? vehicle1.CurrentLevel : Find.CurrentMap;
      __result = CrossMapRCellFinder.BestOrderedGotoDestNear(root1, searcher, cellValidator, reachable, map2);
      if (((IntVec3) ref __result).IsValid)
      {
        TargetMapUtility.set_TargetInfo((Thing) searcher, new TargetInfo(__result, map2, false));
        return false;
      }
    }
    return true;
  }
}
