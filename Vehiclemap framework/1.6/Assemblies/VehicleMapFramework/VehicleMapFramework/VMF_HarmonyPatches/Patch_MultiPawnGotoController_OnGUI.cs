// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_MultiPawnGotoController_OnGUI
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
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (MultiPawnGotoController), "OnGUI")]
[PatchLevel(Level.Sensitive)]
public static class Patch_MultiPawnGotoController_OnGUI
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    MethodInfo m_ToUIRect = AccessTools.Method(typeof (IntVec3), "ToUIRect", (Type[]) null, (Type[]) null);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo m_ToUIRectOffset = (Patch_MultiPawnGotoController_OnGUI.\u003C\u003EO.\u003C0\u003E__ToUIRectOffset ?? (Patch_MultiPawnGotoController_OnGUI.\u003C\u003EO.\u003C0\u003E__ToUIRectOffset = new \u003C\u003EF\u007B00000001\u007D<IntVec3, Pawn, Rect>(Patch_MultiPawnGotoController_OnGUI.ToUIRectOffset))).Method;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo m_Fogged = (Patch_MultiPawnGotoController_OnGUI.\u003C\u003EO.\u003C1\u003E__Fogged ?? (Patch_MultiPawnGotoController_OnGUI.\u003C\u003EO.\u003C1\u003E__Fogged = new Func<IntVec3, Map, bool>(GridsUtility.Fogged))).Method;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo m_FoggedOffset = (Patch_MultiPawnGotoController_OnGUI.\u003C\u003EO.\u003C2\u003E__FoggedOffset ?? (Patch_MultiPawnGotoController_OnGUI.\u003C\u003EO.\u003C2\u003E__FoggedOffset = new Func<IntVec3, Pawn, bool>(Patch_MultiPawnGotoController_Draw.FoggedOffset))).Method;
    foreach (CodeInstruction instruction in instructions)
    {
      if (instruction.opcode == OpCodes.Call && CodeInstructionExtensions.OperandIs(instruction, (MemberInfo) m_ToUIRect))
      {
        yield return CodeInstruction.LoadLocal(1, false);
        instruction.operand = (object) m_ToUIRectOffset;
      }
      if (instruction.opcode == OpCodes.Call && CodeInstructionExtensions.OperandIs(instruction, (MemberInfo) m_Fogged))
      {
        yield return new CodeInstruction(OpCodes.Pop, (object) null);
        yield return CodeInstruction.LoadLocal(1, false);
        instruction.operand = (object) m_FoggedOffset;
      }
      yield return instruction;
    }
  }

  private static Rect ToUIRectOffset(ref IntVec3 intVec, Pawn pawn)
  {
    Vector3 vector3Offset = Patch_MultiPawnGotoController_OnGUI.ToVector3Offset(intVec, pawn);
    Vector2 uiPosition1 = UI.MapToUIPosition(vector3Offset);
    Vector2 uiPosition2 = UI.MapToUIPosition(Vector3.op_Addition(vector3Offset, new Vector3(1f, 0.0f, 1f)));
    return new Rect(uiPosition1.x, uiPosition2.y, uiPosition2.x - uiPosition1.x, uiPosition1.y - uiPosition2.y);
  }

  private static Vector3 ToVector3Offset(IntVec3 intVec, Pawn pawn)
  {
    Map map;
    VehiclePawnWithMap vehicle;
    if (!((Thing) pawn).TryGetTargetMap(out map) || !map.IsNonFocusedVehicleMapOf(out vehicle))
      return ((IntVec3) ref intVec).ToVector3();
    Vector3 vector3 = ((IntVec3) ref intVec).ToVector3();
    Vector3 vector3Shifted = ((IntVec3) ref intVec).ToVector3Shifted();
    Rot8 fullRotation = vehicle.FullRotation;
    double asAngle = (double) ((Rot8) ref fullRotation).AsAngle;
    return Ext_Math.RotatePoint(vector3, vector3Shifted, (float) asAngle).ToBaseMapCoord(vehicle);
  }
}
