// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_CompPowerPlantSolar_PostDraw
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (CompPowerPlantSolar), "PostDraw")]
[PatchLevel(Level.Sensitive)]
public static class Patch_CompPowerPlantSolar_PostDraw
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    List<CodeInstruction> codeInstructionList = instructions.MethodReplacer((MethodInfoCache.CachedMethodInfo.g_Thing_Rotation, MethodInfoCache.CachedMethodInfo.m_BaseFullRotation_Thing), (MethodInfoCache.CachedMethodInfo.m_Rot4_Rotate, MethodInfoCache.CachedMethodInfo.m_Rot8_Rotate));
    Label label1 = generator.DefineLabel();
    Label label2 = generator.DefineLabel();
    int index = codeInstructionList.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Call && CodeInstructionExtensions.OperandIs(c, (MemberInfo) MethodInfoCache.CachedMethodInfo.m_Rot8_Rotate))) - 1;
    codeInstructionList[index].labels.Add(label1);
    codeInstructionList[index + 2].labels.Add(label2);
    // ISSUE: object of a compiler-generated type is created
    codeInstructionList.InsertRange(index, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[5]
    {
      new CodeInstruction(OpCodes.Dup, (object) null),
      PatchHelper.get_CallInstruction(AccessTools.PropertyGetter(typeof (Rot8), "IsHorizontal")),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label1),
      new CodeInstruction(OpCodes.Pop, (object) null),
      new CodeInstruction(OpCodes.Br_S, (object) label2)
    }));
    return (IEnumerable<CodeInstruction>) codeInstructionList;
  }
}
