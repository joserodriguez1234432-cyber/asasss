// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Verb_Jump_DrawHighlight_Delegate
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_Verb_Jump_DrawHighlight_Delegate
{
  private static IEnumerable<MethodBase> TargetMethods()
  {
    yield return (MethodBase) GenCollection.FirstOrDefault<MethodInfo>(AccessToolsExtensions.GetDeclaredMethods(typeof (Verb_Jump)), (Predicate<MethodInfo>) (m => m.Name.Contains("<DrawHighlight>")));
    yield return (MethodBase) GenCollection.FirstOrDefault<MethodInfo>(AccessToolsExtensions.GetDeclaredMethods(typeof (Verb_CastAbilityJump)), (Predicate<MethodInfo>) (m => m.Name.Contains("<DrawHighlight>")));
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer((MethodInfoCache.CachedMethodInfo.g_Thing_Position, MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMapSpawned), (MethodInfoCache.CachedMethodInfo.g_Thing_Map, MethodInfoCache.CachedMethodInfo.m_BaseMap_Thing), (MethodInfoCache.CachedMethodInfo.m_GenSight_LineOfSight1, MethodInfoCache.CachedMethodInfo.m_GenSightOnVehicle_LineOfSight1));
  }
}
