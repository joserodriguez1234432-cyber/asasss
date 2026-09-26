// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Patch_Projectile_CheckForFreeInterceptBetween_Postfix
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_TabulaRasa")]
[HarmonyPatch]
[PatchLevel(Level.Mandatory)]
public static class Patch_Patch_Projectile_CheckForFreeInterceptBetween_Postfix
{
  [HarmonyReversePatch]
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static void PostfixPatch(
    Projectile __instance,
    ref bool __result,
    Vector3 lastExactPos,
    Vector3 newExactPos)
  {
    Transpiler((IEnumerable<CodeInstruction>) null);
    throw new NotImplementedException();

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
      return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Thing_Map, MethodInfoCache.CachedMethodInfo.m_TargetMapOrThingMap);
    }
  }
}
