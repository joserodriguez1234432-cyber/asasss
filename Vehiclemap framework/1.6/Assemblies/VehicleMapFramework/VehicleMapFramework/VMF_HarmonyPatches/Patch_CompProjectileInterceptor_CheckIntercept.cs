// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_CompProjectileInterceptor_CheckIntercept
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (CompProjectileInterceptor), "CheckIntercept")]
[PatchLevel(Level.Mandatory)]
public static class Patch_CompProjectileInterceptor_CheckIntercept
{
  [HarmonyReversePatch]
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static bool CheckIntercept(
    CompProjectileInterceptor instance,
    Projectile projectile,
    Vector3 lastExactPos,
    Vector3 newExactPos)
  {
    Transpiler((IEnumerable<CodeInstruction>) null);
    throw new NotImplementedException();

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
      return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Thing_Position, MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMapSpawned);
    }
  }
}
