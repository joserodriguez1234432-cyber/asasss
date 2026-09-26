// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_PartWithSweep_Draw
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_MeleeAnimation")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_PartWithSweep_Draw
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    FieldInfo f_RootTransform = AccessTools.Field("AM.AnimRenderer:RootTransform");
    return Transpilers.Manipulator(instructions, (Func<CodeInstruction, bool>) (c => c.opcode == OpCodes.Ldfld && CodeInstructionExtensions.OperandIs(c, (MemberInfo) f_RootTransform)), (Action<CodeInstruction>) (c =>
    {
      c.opcode = OpCodes.Call;
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      c.operand = (object) (Patch_PartWithSweep_Draw.\u003C\u003EO.\u003C0\u003E__RootTransformOffset ?? (Patch_PartWithSweep_Draw.\u003C\u003EO.\u003C0\u003E__RootTransformOffset = new Func<object, Matrix4x4>(Patch_AnimRenderer_Draw.RootTransformOffset))).Method;
    }));
  }
}
