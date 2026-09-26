// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Patch_WorkGiver_Scanner_GetPriority_PriorityPostfix
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

[HarmonyPatchCategory("VMF_Patches_SmarterConstruction")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_Patch_WorkGiver_Scanner_GetPriority_PriorityPostfix
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo m_DistanceTo = (Patch_Patch_WorkGiver_Scanner_GetPriority_PriorityPostfix.\u003C\u003EO.\u003C0\u003E__DistanceTo ?? (Patch_Patch_WorkGiver_Scanner_GetPriority_PriorityPostfix.\u003C\u003EO.\u003C0\u003E__DistanceTo = new Func<IntVec3, IntVec3, float>(IntVec3Utility.DistanceTo))).Method;
    int index = list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Call && CodeInstructionExtensions.OperandIs(c, (MemberInfo) m_DistanceTo))) - 1;
    list[index].operand = (object) MethodInfoCache.CachedMethodInfo.m_CellOnBaseMap_TargetInfo;
    list[index - 2].opcode = OpCodes.Call;
    list[index - 2].operand = (object) MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMap;
    return (IEnumerable<CodeInstruction>) list;
  }
}
