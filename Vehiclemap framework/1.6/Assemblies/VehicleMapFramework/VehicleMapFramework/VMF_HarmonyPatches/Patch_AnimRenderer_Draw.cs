// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_AnimRenderer_Draw
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_MeleeAnimation")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_AnimRenderer_Draw
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    FieldInfo f_AnimRenderer_Map = AccessTools.Field("AM.AnimRenderer:Map");
    FieldInfo f_RootTransform = AccessTools.Field("AM.AnimRenderer:RootTransform");
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo m_BaseMap = (Patch_AnimRenderer_Draw.\u003C\u003EO.\u003C0\u003E__BaseMap ?? (Patch_AnimRenderer_Draw.\u003C\u003EO.\u003C0\u003E__BaseMap = new Func<object, Map>(Patch_AnimRenderer_Draw.BaseMap))).Method;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo m_RootTransformOffset = (Patch_AnimRenderer_Draw.\u003C\u003EO.\u003C1\u003E__RootTransformOffset ?? (Patch_AnimRenderer_Draw.\u003C\u003EO.\u003C1\u003E__RootTransformOffset = new Func<object, Matrix4x4>(Patch_AnimRenderer_Draw.RootTransformOffset))).Method;
    return Transpilers.Manipulator(Transpilers.Manipulator(instructions, (Func<CodeInstruction, bool>) (c => CodeInstructionExtensions.LoadsField(c, f_AnimRenderer_Map, false)), (Action<CodeInstruction>) (c =>
    {
      c.opcode = OpCodes.Call;
      c.operand = (object) m_BaseMap;
    })), (Func<CodeInstruction, bool>) (c => CodeInstructionExtensions.LoadsField(c, f_RootTransform, false)), (Action<CodeInstruction>) (c =>
    {
      c.opcode = OpCodes.Call;
      c.operand = (object) m_RootTransformOffset;
    }));
  }

  public static Map BaseMap(object instance)
  {
    return ModCompat.MeleeAnimation.AnimRenderer_Map.Invoke(instance).BaseMap();
  }

  public static Matrix4x4 RootTransformOffset(object instance)
  {
    Matrix4x4 matrix4x4 = ModCompat.MeleeAnimation.AnimRenderer_RootTransform.Invoke(instance);
    VehiclePawnWithMap vehicle;
    if (ModCompat.MeleeAnimation.AnimRenderer_Map.Invoke(instance).IsNonFocusedVehicleMapOf(out vehicle) && ModCompat.MeleeAnimation.AnimRenderer_cellData.Invoke(ModCompat.MeleeAnimation.AnimRenderer_Def.Invoke(instance)).Count > 0)
    {
      Vector3 original = GenMath.Position(matrix4x4);
      ((Matrix4x4) ref matrix4x4).SetColumn(3, Vector4.op_Implicit(Vector3Utility.WithY(original.ToBaseMapCoord(vehicle), original.y)));
    }
    return matrix4x4;
  }
}
