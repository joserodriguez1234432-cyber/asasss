// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patches_AttackTargetFinderAngle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[StaticConstructorOnStartupPriority(200)]
internal class Patches_AttackTargetFinderAngle
{
  public static readonly Patches_AttackTargetFinderAngle.FuncBestAttackTarget BestAttackTarget;

  static Patches_AttackTargetFinderAngle()
  {
    Type type = AccessTools.TypeByName("AttackTargetFinderAngle");
    if ((object) type == null)
      return;
    MethodInfo methodInfo = AccessTools.Method(type, nameof (BestAttackTarget), (Type[]) null, (Type[]) null);
    if ((object) methodInfo == null)
      return;
    Patches_AttackTargetFinderAngle.BestAttackTarget = AccessTools.MethodDelegate<Patches_AttackTargetFinderAngle.FuncBestAttackTarget>(methodInfo, (object) null, true, (Type[]) null);
    if (Patches_AttackTargetFinderAngle.BestAttackTarget == null)
      return;
    VMF_Harmony.PatchCategory("VMF_Patches_AttackTargetFinderAngle");
  }

  public delegate IAttackTarget FuncBestAttackTarget(
    IAttackTargetSearcher searcher,
    TargetScanFlags flags,
    Vector3 angle,
    Predicate<Thing> validator,
    float minDist,
    float maxDist,
    IntVec3 locus,
    float maxTravelRadiusFromLocus,
    bool canTakeTargetsCloserThanEffectiveMinRange);
}
