// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Patch_Projectile_CheckForFreeInterceptBetween_Prefix
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_UFHeavyIndustries")]
[HarmonyPatch]
[PatchLevel(Level.Mandatory)]
public static class Patch_Patch_Projectile_CheckForFreeInterceptBetween_Prefix
{
  [HarmonyReversePatch]
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static bool PrefixPatch(
    Projectile __instance,
    Vector3 lastExactPos,
    Vector3 newExactPos,
    ref bool __result)
  {
    Transpiler((IEnumerable<CodeInstruction>) null);
    throw new NotImplementedException();

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
      return (IEnumerable<CodeInstruction>) new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[2]
      {
        CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Map),
        new CodeMatch(new OpCode?(OpCodes.Bne_Un_S), (object) null, (string) null)
      }).Set(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_BaseMapOrCaravan_Thing).MatchStartBackwards(new CodeMatch[1]
      {
        CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Map)
      }).Set(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_BaseMapOrCaravan_Thing).InstructionEnumeration().MethodReplacer((MethodInfoCache.CachedMethodInfo.g_Thing_Map, MethodInfoCache.CachedMethodInfo.m_TargetMapOrThingMap), (MethodInfoCache.CachedMethodInfo.g_Thing_Position, MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMapSpawned));
    }
  }
}
