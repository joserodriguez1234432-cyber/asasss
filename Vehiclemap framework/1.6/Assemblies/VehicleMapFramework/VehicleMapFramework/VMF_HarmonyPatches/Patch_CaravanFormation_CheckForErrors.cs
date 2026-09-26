// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_CaravanFormation_CheckForErrors
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Vehicles;
using Vehicles.World;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (CaravanFormation), "CheckForErrors")]
[PatchLevel(Level.Sensitive)]
public static class Patch_CaravanFormation_CheckForErrors
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    CodeMatcher matcher = new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(AccessTools.PropertyGetter(typeof (List<VehiclePawn>.Enumerator), "Current"))
    }).Advance(1);
    int vehicleInd = StlocIndex(matcher.Instruction);
    matcher.MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(AccessTools.PropertyGetter(typeof (List<Pawn>.Enumerator), "Current"))
    }).Advance(1);
    int num = StlocIndex(matcher.Instruction);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return matcher.MatchStartForward(new CodeMatch[1]
    {
      new CodeMatch((Func<CodeInstruction, bool>) (c =>
      {
        switch (vehicleInd)
        {
          case 0:
            return c.opcode == OpCodes.Ldloc_0;
          case 1:
            return c.opcode == OpCodes.Ldloc_1;
          case 2:
            return c.opcode == OpCodes.Ldloc_2;
          case 3:
            return c.opcode == OpCodes.Ldloc_3;
          default:
            LocalBuilder localBuilder = matcher.Instructions().Select<CodeInstruction, object>((Func<CodeInstruction, object>) (c2 => c2.operand)).OfType<LocalBuilder>().First<LocalBuilder>((Func<LocalBuilder, bool>) (l => l.LocalIndex == vehicleInd));
            return CodeInstructionExtensions.IsLdloc(c, localBuilder);
        }
      }), (string) null)
    }).InsertAfter(new CodeInstruction[2]
    {
      CodeInstruction.LoadLocal(num, false),
      PatchHelper.get_CallInstruction((Patch_CaravanFormation_CheckForErrors.\u003C\u003EO.\u003C0\u003E__TargetThing ?? (Patch_CaravanFormation_CheckForErrors.\u003C\u003EO.\u003C0\u003E__TargetThing = new Func<VehiclePawn, Pawn, Thing>(Patch_CaravanFormation_CheckForErrors.TargetThing))).Method)
    }).InstructionEnumeration();

    static int StlocIndex(CodeInstruction instruction)
    {
      OpCode opcode = instruction.opcode;
      if (instruction.opcode == OpCodes.Stloc_0)
        return 0;
      if (instruction.opcode == OpCodes.Stloc_1)
        return 1;
      if (instruction.opcode == OpCodes.Stloc_2)
        return 2;
      if (instruction.opcode == OpCodes.Stloc_3)
        return 3;
      if (instruction.opcode == OpCodes.Stloc_S || instruction.opcode == OpCodes.Stloc)
        return ((LocalVariableInfo) instruction.operand).LocalIndex;
      throw new Exception("Local variable not found.");
    }
  }

  private static Thing TargetThing(VehiclePawn vehicle, Pawn pawn)
  {
    return CaravanHelper.assignedSeats.GetAssignment(pawn)?.handler.role is VehicleRoleBuildable role ? (Thing) role.upgradeComp.parent : (Thing) vehicle;
  }
}
