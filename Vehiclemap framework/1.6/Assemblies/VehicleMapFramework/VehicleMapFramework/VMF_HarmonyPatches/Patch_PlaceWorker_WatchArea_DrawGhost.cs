// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_PlaceWorker_WatchArea_DrawGhost
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

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (PlaceWorker_WatchArea), "DrawGhost")]
[PatchLevel(Level.Sensitive)]
public static class Patch_PlaceWorker_WatchArea_DrawGhost
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    int index1 = list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Stloc_0));
    Label label1 = generator.DefineLabel();
    Label label2 = generator.DefineLabel();
    list[index1].labels.Add(label2);
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index1 - 1, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[8]
    {
      CodeInstruction.LoadArgument(5, false),
      new CodeInstruction(OpCodes.Dup, (object) null),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label1),
      new CodeInstruction(OpCodes.Callvirt, (object) MethodInfoCache.CachedMethodInfo.g_Thing_Map),
      new CodeInstruction(OpCodes.Dup, (object) null),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label1),
      new CodeInstruction(OpCodes.Br_S, (object) label2),
      CodeInstructionExtensions.WithLabels(new CodeInstruction(OpCodes.Pop, (object) null), new Label[1]
      {
        label1
      })
    }));
    int index2 = list.FindIndex(index1, (Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Call && CodeInstructionExtensions.OperandIs(c, (MemberInfo) MethodInfoCache.CachedMethodInfo.m_GenDraw_DrawFieldEdges1)));
    list.Insert(index2, CodeInstruction.LoadLocal(0, false));
    return (IEnumerable<CodeInstruction>) list.MethodReplacer((MethodInfoCache.CachedMethodInfo.g_Find_CurrentMap, MethodInfoCache.CachedMethodInfo.g_VehicleMapUtility_CurrentMap), (MethodInfoCache.CachedMethodInfo.m_GenDraw_DrawFieldEdges1, MethodInfoCache.CachedMethodInfo.m_GenDrawOnVehicle_DrawFieldEdges1));
  }
}
