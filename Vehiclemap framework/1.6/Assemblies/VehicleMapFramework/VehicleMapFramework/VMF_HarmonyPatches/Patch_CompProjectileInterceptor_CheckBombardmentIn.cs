// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_CompProjectileInterceptor_CheckBombardmentIntercept
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (CompProjectileInterceptor), "CheckBombardmentIntercept")]
[PatchLevel(Level.Mandatory)]
public static class Patch_CompProjectileInterceptor_CheckBombardmentIntercept
{
  [HarmonyReversePatch]
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static bool CheckBombardmentIntercept(
    CompProjectileInterceptor instance,
    Bombardment bombardment,
    Bombardment.BombardmentProjectile projectile)
  {
    Transpiler((IEnumerable<CodeInstruction>) null);
    throw new NotImplementedException();

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
      return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Thing_Position, MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMapSpawned);
    }
  }
}
