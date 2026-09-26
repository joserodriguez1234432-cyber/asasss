// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_MultiPawnGotoController_Draw
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (MultiPawnGotoController), "Draw")]
[PatchLevel(Level.Sensitive)]
public static class Patch_MultiPawnGotoController_Draw
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo m_ToVector3ShiftedOffsetWithAltitude = (Patch_MultiPawnGotoController_Draw.\u003C\u003EO.\u003C0\u003E__ToVector3ShiftedOffsetWithAltitude ?? (Patch_MultiPawnGotoController_Draw.\u003C\u003EO.\u003C0\u003E__ToVector3ShiftedOffsetWithAltitude = new \u003C\u003EF\u007B00000001\u007D<IntVec3, float, Pawn, Vector3>(Patch_MultiPawnGotoController_Draw.ToVector3ShiftedOffsetWithAltitude))).Method;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo m_Fogged = (Patch_MultiPawnGotoController_Draw.\u003C\u003EO.\u003C1\u003E__Fogged ?? (Patch_MultiPawnGotoController_Draw.\u003C\u003EO.\u003C1\u003E__Fogged = new Func<IntVec3, Map, bool>(GridsUtility.Fogged))).Method;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo m_FoggedOffset = (Patch_MultiPawnGotoController_Draw.\u003C\u003EO.\u003C2\u003E__FoggedOffset ?? (Patch_MultiPawnGotoController_Draw.\u003C\u003EO.\u003C2\u003E__FoggedOffset = new Func<IntVec3, Pawn, bool>(Patch_MultiPawnGotoController_Draw.FoggedOffset))).Method;
    int num = 0;
    foreach (CodeInstruction instruction in instructions)
    {
      if (num < 2 && instruction.opcode == OpCodes.Call && CodeInstructionExtensions.OperandIs(instruction, (MemberInfo) MethodInfoCache.CachedMethodInfo.m_IntVec3_ToVector3ShiftedWithAltitude))
      {
        yield return CodeInstruction.LoadLocal(5, false);
        instruction.operand = (object) m_ToVector3ShiftedOffsetWithAltitude;
        ++num;
      }
      if (instruction.opcode == OpCodes.Call && CodeInstructionExtensions.OperandIs(instruction, (MemberInfo) m_Fogged))
      {
        yield return new CodeInstruction(OpCodes.Pop, (object) null);
        yield return CodeInstruction.LoadLocal(5, false);
        instruction.operand = (object) m_FoggedOffset;
      }
      yield return instruction;
    }
  }

  public static Vector3 ToVector3ShiftedOffsetWithAltitude(
    ref IntVec3 intVec,
    float AddedAltitude,
    Pawn pawn)
  {
    Map map;
    return !((Thing) pawn).TryGetTargetMap(out map) ? ((IntVec3) ref intVec).ToVector3ShiftedWithAltitude(AddedAltitude) : Vector3Utility.WithY(((IntVec3) ref intVec).ToVector3Shifted().ToBaseMapCoord(map), AddedAltitude);
  }

  public static bool FoggedOffset(IntVec3 intVec, Pawn pawn)
  {
    Map map;
    return !((Thing) pawn).TryGetTargetMap(out map) ? GridsUtility.Fogged(intVec, ((Thing) pawn).Map) : GridsUtility.Fogged(intVec.ToBaseMapCoord(map), map.BaseMap());
  }
}
