// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_CompTargeter_IsValidTarget
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_DefensiveNetwork")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_CompTargeter_IsValidTarget
{
  private static IEnumerable<MethodBase> TargetMethods()
  {
    yield return (MethodBase) AccessTools.Method("DNX.CompSpecialGrenadeLauncher:IsValidTarget", (Type[]) null, (Type[]) null);
    yield return (MethodBase) AccessTools.Method("DNX.CompManualMissileLauncher:IsValidTarget", (Type[]) null, (Type[]) null);
    yield return (MethodBase) AccessTools.Method("DNX.CompLongbowManualTargeter:IsValidTarget", (Type[]) null, (Type[]) null);
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    if (UnitTestDetector.IsTestingContext)
      return instructions;
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(((MethodBase) MethodInfoCache.CachedMethodInfo.g_Thing_Map, (MethodBase) MethodInfoCache.CachedMethodInfo.m_BaseMap_Thing), ((MethodBase) MethodInfoCache.CachedMethodInfo.g_Thing_Position, (MethodBase) MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMapSpawned), ((MethodBase) MethodInfoCache.CachedMethodInfo.g_TargetInfo_Map, (MethodBase) MethodInfoCache.CachedMethodInfo.m_BaseMap_TargetInfo), ((MethodBase) MethodInfoCache.CachedMethodInfo.g_TargetInfo_Cell, (MethodBase) MethodInfoCache.CachedMethodInfo.m_CellOnBaseMapSpawned_TargetInfo));
  }
}
