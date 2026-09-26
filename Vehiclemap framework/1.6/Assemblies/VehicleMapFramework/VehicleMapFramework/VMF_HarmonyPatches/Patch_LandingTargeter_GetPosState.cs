// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_LandingTargeter_GetPosState
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (LandingTargeter), "GetPosState")]
[PatchLevel(Level.Cautious)]
public static class Patch_LandingTargeter_GetPosState
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    MethodInfo g_CurrentMap = AccessTools.PropertyGetter(typeof (Game), "CurrentMap");
    foreach (CodeInstruction instruction in instructions)
    {
      yield return instruction;
      if (CodeInstructionExtensions.Calls(instruction, g_CurrentMap))
      {
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        yield return PatchHelper.get_CallInstruction((Patch_LandingTargeter_GetPosState.\u003C\u003EO.\u003C0\u003E__FocusedMapOrCurrentMap ?? (Patch_LandingTargeter_GetPosState.\u003C\u003EO.\u003C0\u003E__FocusedMapOrCurrentMap = new Func<Map, Map>(Patch_LandingTargeter_GetPosState.FocusedMapOrCurrentMap))).Method);
      }
    }
  }

  private static Map FocusedMapOrCurrentMap(Map map)
  {
    return Command_FocusVehicleMap.FocusedVehicle == null ? map : Command_FocusVehicleMap.FocusedVehicle.VehicleMap;
  }
}
