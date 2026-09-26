// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_AttackTargetFinderAngle_BestAttackTarget
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_AttackTargetFinderAngle")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_AttackTargetFinderAngle_BestAttackTarget
{
  private static bool working;

  private static IEnumerable<MethodBase> TargetMethods()
  {
    return (IEnumerable<MethodBase>) GenTypes.AllTypes.Where<Type>((Func<Type, bool>) (type => type.Name == "AttackTargetFinderAngle")).Select<Type, MethodInfo>((Func<Type, MethodInfo>) (type => AccessTools.Method(type, "BestAttackTarget", (Type[]) null, (Type[]) null)));
  }

  public static void Postfix(
    IAttackTargetSearcher searcher,
    TargetScanFlags flags,
    Vector3 angle,
    Predicate<Thing> validator,
    float minDist,
    float maxDist,
    IntVec3 locus,
    float maxTravelRadiusFromLocus,
    bool canTakeTargetsCloserThanEffectiveMinRange,
    ref IAttackTarget __result)
  {
    if (Patch_AttackTargetFinderAngle_BestAttackTarget.working)
      return;
    Map map = searcher.Thing.Map;
    if (searcher.Thing is Pawn thing)
      CrossMapReachabilityUtility.set_DepartMap(thing, map);
    IntVec3 position = searcher.Thing.Position;
    IntVec3 positionOnBaseMap = VehicleMapUtility.get_PositionOnBaseMap(searcher.Thing);
    foreach (Map mapAndVehicleMap in map.BaseMapAndVehicleMaps(false))
    {
      IAttackTarget target2 = (IAttackTarget) null;
      try
      {
        Patch_AttackTargetFinderAngle_BestAttackTarget.working = true;
        VehiclePawnWithMap vehicle;
        searcher.Thing.VirtualMapTransfer(mapAndVehicleMap, mapAndVehicleMap.IsVehicleMapOf(out vehicle) ? positionOnBaseMap.ToVehicleMapCoord(vehicle) : positionOnBaseMap);
        target2 = Patches_AttackTargetFinderAngle.BestAttackTarget(searcher, flags, angle, validator, minDist, maxDist, locus, maxTravelRadiusFromLocus, canTakeTargetsCloserThanEffectiveMinRange);
      }
      finally
      {
        Patch_AttackTargetFinderAngle_BestAttackTarget.working = false;
        searcher.Thing.VirtualMapTransfer(map, position);
        if (thing != null)
          thing.RemoveDepartMap();
        __result = AttackTargetFinderOnVehicle.CompareTarget(__result, target2, searcher);
      }
    }
  }
}
