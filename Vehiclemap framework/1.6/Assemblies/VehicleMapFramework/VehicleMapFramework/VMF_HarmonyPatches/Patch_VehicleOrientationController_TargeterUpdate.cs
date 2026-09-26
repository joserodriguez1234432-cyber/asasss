// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_VehicleOrientationController_TargeterUpdate
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (VehicleOrientationController), "TargeterUpdate")]
[PatchLevel(Level.Sensitive)]
public static class Patch_VehicleOrientationController_TargeterUpdate
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo m_ToVector3ShiftedOffsetWithAltitude = (Patch_VehicleOrientationController_TargeterUpdate.\u003C\u003EO.\u003C0\u003E__ToVector3ShiftedOffsetWithAltitude ?? (Patch_VehicleOrientationController_TargeterUpdate.\u003C\u003EO.\u003C0\u003E__ToVector3ShiftedOffsetWithAltitude = new \u003C\u003EF\u007B00000001\u007D<IntVec3, float, Pawn, Vector3>(Patch_MultiPawnGotoController_Draw.ToVector3ShiftedOffsetWithAltitude))).Method;
    int num = 0;
    int ind = list.Select<CodeInstruction, object>((Func<CodeInstruction, object>) (c => c.operand)).OfType<LocalBuilder>().First<LocalBuilder>((Func<LocalBuilder, bool>) (l => l.LocalType == typeof (VehiclePawn))).LocalIndex;
    foreach (CodeInstruction code in list)
    {
      if (CodeInstructionExtensions.Calls(code, MethodInfoCache.CachedMethodInfo.m_IntVec3_ToVector3ShiftedWithAltitude))
      {
        ++num;
        if (num > 2)
        {
          yield return CodeInstruction.LoadLocal(ind, false);
          code.operand = (object) m_ToVector3ShiftedOffsetWithAltitude;
        }
      }
      yield return code;
    }
  }
}
