// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_TMJobDriver_CastAbilityVerb_MakeNewToils
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_RimWorldOfMagic")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_TMJobDriver_CastAbilityVerb_MakeNewToils
{
  private static MethodBase TargetMethod()
  {
    return (MethodBase) AccessTools.FindIncludingInnerTypes<MethodInfo>(GenTypes.GetTypeInAnyAssembly("TorannMagic.TMJobDriver_CastAbilityVerb", "TorannMagic"), (Func<Type, MethodInfo>) (t => GenCollection.FirstOrDefault<MethodInfo>(AccessToolsExtensions.GetDeclaredMethods(t), (Predicate<MethodInfo>) (m => m.Name == "MoveNext"))));
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    MethodInfo methodInfo = AccessTools.PropertyGetter(typeof (JobDriver), "TargetLocA");
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo method = (Patch_TMJobDriver_CastAbilityVerb_MakeNewToils.\u003C\u003EO.\u003C0\u003E__TargetLocAOnBaseMap ?? (Patch_TMJobDriver_CastAbilityVerb_MakeNewToils.\u003C\u003EO.\u003C0\u003E__TargetLocAOnBaseMap = new Func<JobDriver, IntVec3>(Patch_TMJobDriver_CastAbilityVerb_MakeNewToils.TargetLocAOnBaseMap))).Method;
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer((MethodInfoCache.CachedMethodInfo.g_Thing_Position, MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMapSpawned), (methodInfo, method), (MethodInfoCache.CachedMethodInfo.g_Thing_Map, MethodInfoCache.CachedMethodInfo.m_BaseMap_Thing));
  }

  private static IntVec3 TargetLocAOnBaseMap(JobDriver instance)
  {
    return instance.job.targetA.CellOnBaseMapSpawned();
  }
}
