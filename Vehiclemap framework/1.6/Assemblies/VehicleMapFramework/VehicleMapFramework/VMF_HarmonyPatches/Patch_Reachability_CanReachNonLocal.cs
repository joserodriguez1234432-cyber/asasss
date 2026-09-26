// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Reachability_CanReachNonLocal
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Reachability), "CanReachNonLocal", new Type[] {typeof (IntVec3), typeof (TargetInfo), typeof (PathEndMode), typeof (TraverseParms)})]
[PatchLevel(Level.Safe)]
public static class Patch_Reachability_CanReachNonLocal
{
  public static bool Prefix(
    IntVec3 start,
    TargetInfo dest,
    PathEndMode peMode,
    TraverseParms traverseParams,
    Map ___map,
    ref bool __result)
  {
    Map map = ((TargetInfo) ref dest).Map;
    if (VehicleMapUtility.get_BaseMapOrCaravan(___map) != VehicleMapUtility.get_BaseMapOrCaravan(map))
      return true;
    __result = CrossMapReachabilityUtility.CanReach(___map, start, TargetInfo.op_Explicit(dest), peMode, traverseParams, map);
    return false;
  }
}
