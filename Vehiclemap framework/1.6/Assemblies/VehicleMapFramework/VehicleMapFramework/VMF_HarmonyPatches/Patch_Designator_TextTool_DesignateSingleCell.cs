// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Designator_TextTool_DesignateSingleCell
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_TextTool")]
[HarmonyPatch]
[PatchLevel(Level.Cautious)]
public static class Patch_Designator_TextTool_DesignateSingleCell
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer((Patch_Designator_TextTool_DesignateSingleCell.\u003C\u003EO.\u003C0\u003E__MouseMapPosition ?? (Patch_Designator_TextTool_DesignateSingleCell.\u003C\u003EO.\u003C0\u003E__MouseMapPosition = new Func<Vector3>(UI.MouseMapPosition))).Method, (Patch_Designator_TextTool_DesignateSingleCell.\u003C\u003EO.\u003C1\u003E__MouseVehicleMapPosition ?? (Patch_Designator_TextTool_DesignateSingleCell.\u003C\u003EO.\u003C1\u003E__MouseVehicleMapPosition = new Func<Vector3>(Patch_Designator_TextTool_DesignateSingleCell.MouseVehicleMapPosition))).Method);
  }

  private static Vector3 MouseVehicleMapPosition()
  {
    VehiclePawnWithMap vehicle;
    return !VehicleMapUtility.CurrentMap.IsVehicleMapOf(out vehicle) ? UI.MouseMapPosition() : UI.MouseMapPosition().ToVehicleMapCoord(vehicle);
  }
}
