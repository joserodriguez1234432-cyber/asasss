// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_MapText_DrawActiveState
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_TextTool")]
[HarmonyPatch]
public static class Patch_MapText_DrawActiveState
{
  [PatchLevel(Level.Safe)]
  public static void Prefix(Thing __instance, ref Command_FocusVehicleMap.FocusVehicle? __state)
  {
    VehiclePawnWithMap vehicle;
    if (!__instance.IsOnNonFocusedVehicleMapOf(out vehicle))
      return;
    __state = new Command_FocusVehicleMap.FocusVehicle?(new Command_FocusVehicleMap.FocusVehicle(vehicle));
  }

  [PatchLevel(Level.Safe)]
  public static void Finalizer(Command_FocusVehicleMap.FocusVehicle? __state) => __state?.Dispose();

  [PatchLevel(Level.Cautious)]
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return Patch_Designator_TextTool_DesignateSingleCell.Transpiler(instructions);
  }
}
