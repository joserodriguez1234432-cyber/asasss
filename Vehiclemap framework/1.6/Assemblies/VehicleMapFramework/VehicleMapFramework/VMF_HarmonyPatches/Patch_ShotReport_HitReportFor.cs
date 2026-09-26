// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ShotReport_HitReportFor
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

[HarmonyPatch(typeof (ShotReport), "HitReportFor")]
[PatchLevel(Level.Sensitive)]
public static class Patch_ShotReport_HitReportFor
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    LocalBuilder localBuilder1 = generator.DeclareLocal(typeof (Thing));
    LocalBuilder localBuilder2 = generator.DeclareLocal(typeof (Map));
    LocalBuilder localBuilder3 = generator.DeclareLocal(typeof (IntVec3));
    Label label1 = generator.DefineLabel();
    Label label2 = generator.DefineLabel();
    Label label3 = generator.DefineLabel();
    Label label4 = generator.DefineLabel();
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(0, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[20]
    {
      CodeInstruction.LoadArgument(2, true),
      PatchHelper.get_CallInstruction(AccessTools.PropertyGetter(typeof (LocalTargetInfo), "Thing")),
      new CodeInstruction(OpCodes.Stloc_S, (object) localBuilder1),
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder1),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label1),
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder1),
      PatchHelper.get_CallvirtInstruction(MethodInfoCache.CachedMethodInfo.g_Thing_Map),
      new CodeInstruction(OpCodes.Br_S, (object) label2),
      CodeInstructionExtensions.WithLabels(CodeInstruction.LoadArgument(0, false), new Label[1]
      {
        label1
      }),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_BaseMap_Thing),
      CodeInstructionExtensions.WithLabels(new CodeInstruction(OpCodes.Stloc_S, (object) localBuilder2), new Label[1]
      {
        label2
      }),
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder1),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label3),
      CodeInstruction.LoadArgument(0, false),
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder1),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_PositionOnAnotherThingMap),
      new CodeInstruction(OpCodes.Br_S, (object) label4),
      CodeInstructionExtensions.WithLabels(CodeInstruction.LoadArgument(0, false), new Label[1]
      {
        label3
      }),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMap),
      CodeInstructionExtensions.WithLabels(new CodeInstruction(OpCodes.Stloc_S, (object) localBuilder3), new Label[1]
      {
        label4
      })
    }));
    int num = 0;
    for (int index1 = 0; index1 < 3; ++index1)
    {
      int index2 = list.FindIndex(num, (Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Callvirt && CodeInstructionExtensions.OperandIs(c, (MemberInfo) MethodInfoCache.CachedMethodInfo.g_Thing_Position)));
      list[index2].opcode = OpCodes.Ldloc_S;
      list[index2].operand = (object) localBuilder3;
      list.RemoveAt(index2 - 1);
      num = list.FindIndex(index2, (Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Callvirt && CodeInstructionExtensions.OperandIs(c, (MemberInfo) MethodInfoCache.CachedMethodInfo.g_Thing_Map)));
      list[num].opcode = OpCodes.Ldloc_S;
      list[num].operand = (object) localBuilder2;
      list.RemoveAt(num - 1);
    }
    return list.Take<CodeInstruction>(num).Concat<CodeInstruction>((IEnumerable<CodeInstruction>) list.Skip<CodeInstruction>(num).MethodReplacer((MethodInfoCache.CachedMethodInfo.g_Thing_Position, MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMapSpawned), (MethodInfoCache.CachedMethodInfo.g_LocalTargetInfo_Cell, MethodInfoCache.CachedMethodInfo.m_CellOnBaseMapSpawned), (MethodInfoCache.CachedMethodInfo.g_Thing_Map, MethodInfoCache.CachedMethodInfo.m_BaseMap_Thing)));
  }
}
