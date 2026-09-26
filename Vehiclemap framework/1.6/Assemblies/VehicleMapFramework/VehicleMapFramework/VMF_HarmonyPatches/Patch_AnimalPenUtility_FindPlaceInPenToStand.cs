// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_AnimalPenUtility_FindPlaceInPenToStand
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

[HarmonyPatch(typeof (AnimalPenUtility), "FindPlaceInPenToStand")]
[PatchLevel(Level.Sensitive)]
public static class Patch_AnimalPenUtility_FindPlaceInPenToStand
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    int index = list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Callvirt && CodeInstructionExtensions.OperandIs(c, (MemberInfo) MethodInfoCache.CachedMethodInfo.g_Thing_Map)));
    list[index - 2] = CodeInstruction.LoadArgument(0, false);
    list[index - 1] = CodeInstruction.LoadField(typeof (ThingComp), "parent", false);
    return (IEnumerable<CodeInstruction>) list;
  }
}
