// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_CMCTurretGun_ScoreTarget
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_CeleTechArsenal")]
[HarmonyPatch]
[PatchLevel(Level.Cautious)]
public static class Patch_Building_CMCTurretGun_ScoreTarget
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Position)
    }).Set(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMapSpawned).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Position)
    }).Set(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMapSpawned).MatchStartForward(new CodeMatch[3]
    {
      new CodeMatch(new OpCode?(OpCodes.Ldarg_0), (object) null, (string) null),
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Map),
      CodeMatch.Calls((Patch_Building_CMCTurretGun_ScoreTarget.\u003C\u003EO.\u003C0\u003E__CalculateOverallBlockChance ?? (Patch_Building_CMCTurretGun_ScoreTarget.\u003C\u003EO.\u003C0\u003E__CalculateOverallBlockChance = new Func<LocalTargetInfo, IntVec3, Map, float>(CoverUtility.CalculateOverallBlockChance))).Method)
    }).SetInstruction(CodeInstruction.LoadArgument(2, false)).Advance(-1).Set(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_PositionOnAnotherThingMap).Insert(new CodeInstruction[1]
    {
      CodeInstruction.LoadArgument(2, false)
    }).InstructionEnumeration();
  }
}
