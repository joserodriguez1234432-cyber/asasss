// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_PawnFlyer_RecomputePosition
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

[HarmonyPatch(typeof (PawnFlyer), "RecomputePosition")]
[PatchLevel(Level.Sensitive)]
public static class Patch_PawnFlyer_RecomputePosition
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    MethodInfo s_Position = AccessTools.PropertySetter(typeof (Thing), "Position");
    int lastIndex = list.FindLastIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Call && CodeInstructionExtensions.OperandIs(c, (MemberInfo) s_Position)));
    Label label = generator.DefineLabel();
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo method = (Patch_PawnFlyer_RecomputePosition.\u003C\u003EO.\u003C0\u003E__InBounds ?? (Patch_PawnFlyer_RecomputePosition.\u003C\u003EO.\u003C0\u003E__InBounds = new Func<IntVec3, Map, bool>(GenGrid.InBounds))).Method;
    list[lastIndex].labels.Add(label);
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(lastIndex, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[8]
    {
      new CodeInstruction(OpCodes.Dup, (object) null),
      CodeInstruction.LoadArgument(0, false),
      new CodeInstruction(OpCodes.Callvirt, (object) MethodInfoCache.CachedMethodInfo.g_Thing_Map),
      new CodeInstruction(OpCodes.Call, (object) method),
      new CodeInstruction(OpCodes.Brtrue_S, (object) label),
      new CodeInstruction(OpCodes.Pop, (object) null),
      new CodeInstruction(OpCodes.Pop, (object) null),
      new CodeInstruction(OpCodes.Ret, (object) null)
    }));
    return (IEnumerable<CodeInstruction>) list;
  }
}
