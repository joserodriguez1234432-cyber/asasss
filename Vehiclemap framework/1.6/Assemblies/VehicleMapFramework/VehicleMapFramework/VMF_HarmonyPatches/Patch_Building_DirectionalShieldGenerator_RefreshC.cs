// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_DirectionalShieldGenerator_RefreshCoveredCells
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_DefensiveNetwork")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_Building_DirectionalShieldGenerator_RefreshCoveredCells
{
  public static void Postfix(Building __instance, List<IntVec3> ___coveredCells)
  {
    VehiclePawnWithMap vehicle;
    if (!((Thing) __instance).IsOnVehicleMapOf(out vehicle))
      return;
    for (int index = 0; index < ___coveredCells.Count; ++index)
      ___coveredCells[index] = ___coveredCells[index].ToBaseMapCoord(vehicle);
  }
}
