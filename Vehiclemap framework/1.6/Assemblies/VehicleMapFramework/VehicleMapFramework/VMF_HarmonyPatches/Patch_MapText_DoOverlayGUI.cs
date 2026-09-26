// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_MapText_DoOverlayGUI
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_TextTool")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_MapText_DoOverlayGUI
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.LoadsField(AccessTools.Field("TextTool.MapText:exactPosition"), false)
    }).InsertAfter(new CodeInstruction[2]
    {
      new CodeInstruction(OpCodes.Ldarg_0, (object) null),
      PatchHelper.get_CallInstruction((Patch_MapText_DoOverlayGUI.\u003C\u003EO.\u003C0\u003E__ToBaseMapCoord ?? (Patch_MapText_DoOverlayGUI.\u003C\u003EO.\u003C0\u003E__ToBaseMapCoord = new Func<Vector3, Thing, Vector3>(Patch_MapText_DoOverlayGUI.ToBaseMapCoord))).Method)
    }).InstructionEnumeration();
  }

  private static Vector3 ToBaseMapCoord(Vector3 original, Thing thing)
  {
    VehiclePawnWithMap vehicle;
    return !thing.IsOnNonFocusedVehicleMapOf(out vehicle) ? original : original.ToBaseMapCoord(vehicle);
  }
}
