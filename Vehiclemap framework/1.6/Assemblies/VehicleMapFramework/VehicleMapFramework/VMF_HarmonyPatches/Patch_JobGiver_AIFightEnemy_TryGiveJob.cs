// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_JobGiver_AIFightEnemy_TryGiveJob
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (JobGiver_AIFightEnemy), "TryGiveJob")]
[PatchLevel(Level.Sensitive)]
public static class Patch_JobGiver_AIFightEnemy_TryGiveJob
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    MethodInfo g_LengthHorizontalSquared = AccessTools.PropertyGetter(typeof (IntVec3), "LengthHorizontalSquared");
    int index1 = list.FindIndex((Predicate<CodeInstruction>) (c => CodeInstructionExtensions.Calls(c, g_LengthHorizontalSquared)));
    for (int index2 = 0; index2 < 2; ++index2)
    {
      index1 = list.FindLastIndex(index1 - 1, (Predicate<CodeInstruction>) (c => CodeInstructionExtensions.Calls(c, MethodInfoCache.CachedMethodInfo.g_Thing_Position)));
      list[index1].opcode = OpCodes.Call;
      list[index1].operand = (object) MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMap;
    }
    return (IEnumerable<CodeInstruction>) list;
  }
}
