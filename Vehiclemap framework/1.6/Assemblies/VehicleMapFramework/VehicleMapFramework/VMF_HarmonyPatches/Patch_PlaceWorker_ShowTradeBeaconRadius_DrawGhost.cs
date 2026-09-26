// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_PlaceWorker_ShowTradeBeaconRadius_DrawGhost
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

[HarmonyPatch(typeof (PlaceWorker_ShowTradeBeaconRadius), "DrawGhost")]
[PatchLevel(Level.Sensitive)]
public static class Patch_PlaceWorker_ShowTradeBeaconRadius_DrawGhost
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    int index = list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Call && CodeInstructionExtensions.OperandIs(c, (MemberInfo) MethodInfoCache.CachedMethodInfo.m_GenDraw_DrawFieldEdges1)));
    Label label = generator.DefineLabel();
    list[index].operand = (object) MethodInfoCache.CachedMethodInfo.m_GenDrawOnVehicle_DrawFieldEdges1;
    list[index].labels.Add(label);
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[6]
    {
      new CodeInstruction(OpCodes.Ldnull, (object) null),
      CodeInstruction.LoadArgument(5, false),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label),
      new CodeInstruction(OpCodes.Pop, (object) null),
      CodeInstruction.LoadArgument(5, false),
      new CodeInstruction(OpCodes.Callvirt, (object) MethodInfoCache.CachedMethodInfo.g_Thing_Map)
    }));
    return (IEnumerable<CodeInstruction>) list;
  }
}
