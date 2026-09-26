// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_RadarUtility_GetActiveRadars
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_IRBM")]
[HarmonyPatch]
[PatchLevel(Level.Cautious)]
public static class Patch_RadarUtility_GetActiveRadars
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    MethodInfo from = AccessTools.PropertyGetter(typeof (Map), "IsPlayerHome");
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo method = (Patch_RadarUtility_GetActiveRadars.\u003C\u003EO.\u003C0\u003E__IsPlayerHomeOrVehicleMap ?? (Patch_RadarUtility_GetActiveRadars.\u003C\u003EO.\u003C0\u003E__IsPlayerHomeOrVehicleMap = new Func<Map, bool>(Patch_RadarUtility_GetActiveRadars.IsPlayerHomeOrVehicleMap))).Method;
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(from, method);
  }

  private static bool IsPlayerHomeOrVehicleMap(Map map)
  {
    return map.IsPlayerHome || VehicleMapUtility.get_IsVehicleMap(map);
  }
}
