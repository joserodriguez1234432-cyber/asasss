// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Designator_SelectSimilar_ProcessSingleCellClick
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System.Collections.Generic;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_AllowTool")]
[HarmonyPatch]
[PatchLevel(Level.Cautious)]
public static class Patch_Designator_SelectSimilar_ProcessSingleCellClick
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Find_CurrentMap, MethodInfoCache.CachedMethodInfo.g_VehicleMapUtility_CurrentMap);
  }
}
