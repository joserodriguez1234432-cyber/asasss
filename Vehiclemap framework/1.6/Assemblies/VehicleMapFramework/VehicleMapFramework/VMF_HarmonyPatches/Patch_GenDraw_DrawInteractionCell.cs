// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_GenDraw_DrawInteractionCell
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (GenDraw), "DrawInteractionCell")]
[PatchLevel(Level.Sensitive)]
public static class Patch_GenDraw_DrawInteractionCell
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    int index1 = list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Ldloc_S && ((LocalVariableInfo) c.operand).LocalIndex == 4));
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index1, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[2]
    {
      CodeInstruction.LoadArgument(2, false),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_SelectedDrawPosOffset)
    }));
    int index2 = list.FindIndex(index1, (Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Call && CodeInstructionExtensions.OperandIs(c, (MemberInfo) MethodInfoCache.CachedMethodInfo.g_Quaternion_identity)));
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index2, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[2]
    {
      CodeInstruction.LoadArgument(2, false),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_FocusedOrSelectedDrawPosOffset)
    }));
    return (IEnumerable<CodeInstruction>) list;
  }
}
