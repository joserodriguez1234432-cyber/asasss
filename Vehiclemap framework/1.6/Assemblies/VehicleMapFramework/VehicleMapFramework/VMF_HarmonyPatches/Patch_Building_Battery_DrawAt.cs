// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_Battery_DrawAt
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Building_Battery), "DrawAt")]
[PatchLevel(Level.Sensitive)]
public static class Patch_Building_Battery_DrawAt
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    CodeInstruction codeInstruction = list.Find((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Ldc_R4 && CodeInstructionExtensions.OperandIs(c, (object) 0.1f)));
    if (codeInstruction != null)
      codeInstruction.operand = (object) 0.75f;
    return (IEnumerable<CodeInstruction>) list.MethodReplacer((MethodInfoCache.CachedMethodInfo.g_Thing_Rotation, MethodInfoCache.CachedMethodInfo.m_BaseFullRotation_Thing), (MethodInfoCache.CachedMethodInfo.m_Rot4_Rotate, MethodInfoCache.CachedMethodInfo.m_Rot8_Rotate));
  }
}
