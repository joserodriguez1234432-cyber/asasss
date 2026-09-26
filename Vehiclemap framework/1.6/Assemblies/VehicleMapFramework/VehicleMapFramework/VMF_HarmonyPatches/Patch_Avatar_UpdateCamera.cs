// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Avatar_UpdateCamera
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_PerspectiveShift")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_Avatar_UpdateCamera
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    CodeInstruction[] code = new CodeInstruction[1]
    {
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_BaseMapOrCaravan_Map)
    };
    CodeMatcher codeMatcher = new CodeMatcher(instructions, generator);
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Find_CurrentMap)
    }).Repeat((Action<CodeMatcher>) (matcher2 => matcher2.InsertAndAdvance(code).InsertAfter(code).Advance(1)), (Action<string>) null).Reset(true).MatchEndForward(new CodeMatch[2]
    {
      CodeMatch.Calls(AccessTools.Method(typeof (Vector3?), "GetValueOrDefault", (Type[]) null, (Type[]) null)),
      new CodeMatch(new OpCode?(OpCodes.Stloc_2), (object) null, (string) null)
    });
    CodeInstruction codeInstruction = CodeInstructionExtensions.MoveLabelsFrom(CodeInstruction.LoadArgument(0, false), codeMatcher.Instruction);
    Label label;
    LocalBuilder localBuilder;
    return codeMatcher.CreateLabel(ref label).DeclareLocal(typeof (VehiclePawnWithMap), ref localBuilder).Insert(new CodeInstruction[7]
    {
      codeInstruction,
      new CodeInstruction(OpCodes.Ldfld, (object) AccessTools.Field("PerspectiveShift.Avatar:pawn")),
      new CodeInstruction(OpCodes.Ldloca_S, (object) localBuilder),
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_IsOnNonFocusedVehicleMapOf),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label),
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder),
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_ToBaseMapCoord2)
    }).InstructionEnumeration();
  }
}
