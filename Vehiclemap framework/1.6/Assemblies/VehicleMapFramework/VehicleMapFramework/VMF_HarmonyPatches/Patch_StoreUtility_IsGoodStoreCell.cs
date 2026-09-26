// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_StoreUtility_IsGoodStoreCell
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (StoreUtility), "IsGoodStoreCell")]
[PatchLevel(Level.Safe)]
public static class Patch_StoreUtility_IsGoodStoreCell
{
  public static bool Prefix(
    IntVec3 c,
    Map map,
    Thing t,
    Pawn carrier,
    Faction faction,
    ref bool __result)
  {
    if (carrier == null)
      return true;
    Map departMap = CrossMapReachabilityUtility.get_DepartMap(carrier);
    Map targetMap = TargetMapUtility.get_TargetMap((Thing) carrier);
    if (departMap == null && targetMap == null)
      return true;
    carrier.RemoveDepartMap();
    __result = StoreAcrossMapsUtility.IsGoodStoreCell(c, targetMap ?? map, t, carrier, faction);
    CrossMapReachabilityUtility.set_DepartMap(carrier, departMap);
    return false;
  }
}
