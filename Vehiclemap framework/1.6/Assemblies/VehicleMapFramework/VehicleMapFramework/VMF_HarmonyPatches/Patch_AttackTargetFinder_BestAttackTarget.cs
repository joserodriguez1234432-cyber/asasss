// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_AttackTargetFinder_BestAttackTarget
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (AttackTargetFinder), "BestAttackTarget")]
[PatchLevel(Level.Safe)]
public static class Patch_AttackTargetFinder_BestAttackTarget
{
  public static void Postfix(
    IAttackTargetSearcher searcher,
    TargetScanFlags flags,
    Predicate<Thing> validator,
    float minDist,
    float maxDist,
    IntVec3 locus,
    float maxTravelRadiusFromLocus,
    bool canBashDoors,
    bool canTakeTargetsCloserThanEffectiveMinRange,
    bool canBashFences,
    bool onlyRanged,
    ref IAttackTarget __result)
  {
    if (!VehicleMapUtility.get_CrossMapContext(searcher.Thing.Map))
      return;
    IAttackTarget target2 = AttackTargetFinderOnVehicle.BestAttackTarget(searcher, flags, validator, minDist, maxDist, locus, maxTravelRadiusFromLocus, canBashDoors, canTakeTargetsCloserThanEffectiveMinRange, canBashFences, onlyRanged);
    __result = AttackTargetFinderOnVehicle.CompareTarget(__result, target2, searcher);
  }
}
