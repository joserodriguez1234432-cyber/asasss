// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_BeamAutoOperator_CanReserve
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_ManipulatorBeamEmitter")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_BeamAutoOperator_CanReserve
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      new CodeMatch(new OpCode?(OpCodes.Stloc_0), (object) null, (string) null)
    }).Insert(new CodeInstruction[2]
    {
      CodeInstruction.LoadArgument(1, false),
      PatchHelper.get_CallInstruction((Patch_BeamAutoOperator_CanReserve.\u003C\u003EO.\u003C0\u003E__ReplaceMap ?? (Patch_BeamAutoOperator_CanReserve.\u003C\u003EO.\u003C0\u003E__ReplaceMap = new Func<Map, Thing, Map>(Patch_BeamAutoOperator_CanReserve.ReplaceMap))).Method)
    }).InstructionEnumeration();
  }

  private static Map ReplaceMap(Map map, Thing thing) => thing.MapHeld ?? map;
}
