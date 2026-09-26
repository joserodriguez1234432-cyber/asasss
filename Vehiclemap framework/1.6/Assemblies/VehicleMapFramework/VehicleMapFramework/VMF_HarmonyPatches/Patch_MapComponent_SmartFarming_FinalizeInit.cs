// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_MapComponent_SmartFarming_FinalizeInit
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_ReGrowth")]
[HarmonyPatch]
public static class Patch_MapComponent_SmartFarming_FinalizeInit
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    CodeMatcher codeMatcher = new CodeMatcher(instructions, (ILGenerator) null);
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      new CodeMatch((Func<CodeInstruction, bool>) (c => c.opcode == OpCodes.Isinst && CodeInstructionExtensions.OperandIs(c, (MemberInfo) typeof (PocketMapParent))), (string) null)
    });
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    codeMatcher.InsertAfter(new CodeInstruction[1]
    {
      PatchHelper.get_CallInstruction((Patch_MapComponent_SmartFarming_FinalizeInit.\u003C\u003EO.\u003C0\u003E__CheckNotVehicleMapParent ?? (Patch_MapComponent_SmartFarming_FinalizeInit.\u003C\u003EO.\u003C0\u003E__CheckNotVehicleMapParent = new Func<PocketMapParent, PocketMapParent>(Patch_MapComponent_SmartFarming_FinalizeInit.CheckNotVehicleMapParent))).Method)
    });
    return (IEnumerable<CodeInstruction>) codeMatcher.Instructions();
  }

  private static PocketMapParent CheckNotVehicleMapParent(PocketMapParent mapParent)
  {
    return !(mapParent is MapParent_Vehicle) ? mapParent : (PocketMapParent) null;
  }
}
