// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Frame_DrawAt
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Frame), "DrawAt")]
[PatchLevel(Level.Sensitive)]
public static class Patch_Frame_DrawAt
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    foreach (CodeInstruction instruction in instructions)
    {
      if (CodeInstructionExtensions.OperandIs(instruction, (MemberInfo) MethodInfoCache.CachedMethodInfo.m_Matrix4x4_SetTRS))
      {
        yield return CodeInstruction.LoadArgument(0, false);
        instruction.operand = (object) MethodInfoCache.CachedMethodInfo.m_SetTRSOnVehicle;
      }
      yield return instruction;
    }
  }
}
