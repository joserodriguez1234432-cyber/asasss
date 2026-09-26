// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ActionController_GetGrappleReport
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_MeleeAnimation")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_ActionController_GetGrappleReport
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> codeInstructionList = instructions.MethodReplacer((MethodInfoCache.CachedMethodInfo.g_Thing_Position, MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMap), (MethodInfoCache.CachedMethodInfo.m_GenSight_LineOfSightToThing, MethodInfoCache.CachedMethodInfo.m_GenSightOnVehicle_LineOfSightToThing));
    int index = codeInstructionList.FindIndex(codeInstructionList.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Callvirt && CodeInstructionExtensions.OperandIs(c, (MemberInfo) MethodInfoCache.CachedMethodInfo.g_Thing_Map))) + 1, (Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Callvirt && CodeInstructionExtensions.OperandIs(c, (MemberInfo) MethodInfoCache.CachedMethodInfo.g_Thing_Map)));
    codeInstructionList[index].opcode = OpCodes.Call;
    codeInstructionList[index].operand = (object) MethodInfoCache.CachedMethodInfo.m_BaseMap_Thing;
    codeInstructionList.Insert(codeInstructionList.FindLastIndex(index, (Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Ldarg_1)), new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_BaseMap_Map));
    return (IEnumerable<CodeInstruction>) codeInstructionList;
  }
}
