// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_TargetingHelper_BestAttackTarget
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using Vehicles;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (TargetingHelper), "BestAttackTarget")]
[PatchLevel(Level.Safe)]
public static class Patch_TargetingHelper_BestAttackTarget
{
  public static void Postfix(
    VehicleTurret turret,
    TargetScanFlags flags,
    Predicate<Thing> validator,
    float minDist,
    float maxDist,
    IntVec3 locus,
    float maxTravelRadiusFromLocus,
    bool canTakeTargetsCloserThanEffectiveMinRange,
    ref IAttackTarget __result)
  {
    VehiclePawn vehicle = turret.vehicle;
    IAttackTarget target2 = TargetingHelperOnVehicle.BestAttackTarget(turret, flags, validator, minDist, maxDist, locus, maxTravelRadiusFromLocus, canTakeTargetsCloserThanEffectiveMinRange);
    __result = AttackTargetFinderOnVehicle.CompareTarget(__result, target2, (IAttackTargetSearcher) vehicle);
  }
}
