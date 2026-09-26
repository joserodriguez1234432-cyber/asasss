// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_DraftedFloatMenuOptionsUI_ExecutionEnabledOnClick
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_MeleeAnimation")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_DraftedFloatMenuOptionsUI_ExecutionEnabledOnClick
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    int index = list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Ldstr && ((string) c.operand).StartsWith("CRITICAL ERROR: Failed to force interrupt")));
    Label label = generator.DefineLabel();
    CodeInstruction codeInstruction = CodeInstruction.LoadArgument(1, false);
    CodeInstructionExtensions.MoveLabelsTo(list[index], codeInstruction);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[12]
    {
      codeInstruction,
      PatchHelper.get_CallInstruction((Patch_DraftedFloatMenuOptionsUI_ExecutionEnabledOnClick.\u003C\u003EO.\u003C0\u003E__NextJobOfGotoDestMapJob ?? (Patch_DraftedFloatMenuOptionsUI_ExecutionEnabledOnClick.\u003C\u003EO.\u003C0\u003E__NextJobOfGotoDestMapJob = new Func<Pawn, Job>(JobAcrossMapsUtility.NextJobOfGotoDestMapJob))).Method),
      new CodeInstruction(OpCodes.Dup, (object) null),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label),
      new CodeInstruction(OpCodes.Dup, (object) null),
      CodeInstruction.LoadField(typeof (Job), "def", false),
      new CodeInstruction(OpCodes.Ldsfld, (object) AccessTools.Field("AM.AM_DefOf:AM_WalkToExecution")),
      new CodeInstruction(OpCodes.Ceq, (object) null),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label),
      new CodeInstruction(OpCodes.Pop, (object) null),
      new CodeInstruction(OpCodes.Ret, (object) null),
      CodeInstructionExtensions.WithLabels(new CodeInstruction(OpCodes.Pop, (object) null), new Label[1]
      {
        label
      })
    }));
    return (IEnumerable<CodeInstruction>) list;
  }
}
