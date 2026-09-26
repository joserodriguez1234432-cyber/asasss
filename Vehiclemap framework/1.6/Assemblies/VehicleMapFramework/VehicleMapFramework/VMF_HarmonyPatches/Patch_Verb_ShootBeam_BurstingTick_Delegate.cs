// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Verb_ShootBeam_BurstingTick_Delegate
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_Verb_ShootBeam_BurstingTick_Delegate
{
  private static MethodInfo TargetMethod()
  {
    return AccessToolsExtensions.GetDeclaredMethods(typeof (Verb_ShootBeam)).First<MethodInfo>((Func<MethodInfo, bool>) (m => m.Name.Contains("<BurstingTick>")));
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer((MethodInfoCache.CachedMethodInfo.g_Thing_Map, MethodInfoCache.CachedMethodInfo.m_BaseMap_Thing), (MethodInfoCache.CachedMethodInfo.m_CanBeSeenOverFast, MethodInfoCache.CachedMethodInfo.m_CanBeSeenOverOnVehicleFast));
  }
}
