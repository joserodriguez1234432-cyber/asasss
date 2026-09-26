// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Patch_CanReserve_SAL_Postfix
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_ProjectRimFactory")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_Patch_CanReserve_SAL_Postfix
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    list[list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Callvirt && CodeInstructionExtensions.OperandIs(c, (MemberInfo) MethodInfoCache.CachedMethodInfo.g_Thing_Map))) - 1].opcode = OpCodes.Ldloc_2;
    return (IEnumerable<CodeInstruction>) list;
  }
}
