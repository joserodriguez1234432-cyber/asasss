// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_RenderHelper_DrawLinesBetweenTargets
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
using Vehicles.Rendering;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (RenderHelper), "DrawLinesBetweenTargets")]
[PatchLevel(Level.Sensitive)]
public static class Patch_RenderHelper_DrawLinesBetweenTargets
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    int index = list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Callvirt && CodeInstructionExtensions.OperandIs(c, (MemberInfo) MethodInfoCache.CachedMethodInfo.g_Thing_Position)));
    list.RemoveRange(index, 4);
    MethodInfo methodInfo = AccessTools.PropertyGetter(typeof (Pawn), "DrawPos");
    list.Insert(index, new CodeInstruction(OpCodes.Callvirt, (object) methodInfo));
    MethodInfo g_CenterVector3 = AccessTools.PropertyGetter(typeof (LocalTargetInfo), "CenterVector3");
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo m_CenterVector3VehicleOffset = (Patch_RenderHelper_DrawLinesBetweenTargets.\u003C\u003EO.\u003C0\u003E__CenterVector3VehicleOffset ?? (Patch_RenderHelper_DrawLinesBetweenTargets.\u003C\u003EO.\u003C0\u003E__CenterVector3VehicleOffset = new \u003C\u003EF\u007B00000001\u007D<LocalTargetInfo, Pawn, Vector3>(Patch_Pawn_JobTracker_DrawLinesBetweenTargets.CenterVector3VehicleOffset))).Method;
    foreach (CodeInstruction code in list)
    {
      if (code.opcode == OpCodes.Call && CodeInstructionExtensions.OperandIs(code, (MemberInfo) g_CenterVector3))
      {
        yield return CodeInstruction.LoadArgument(0, false);
        code.operand = (object) m_CenterVector3VehicleOffset;
      }
      yield return code;
    }
  }
}
