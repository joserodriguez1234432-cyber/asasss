// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_Bed_FindPreferredInteractionCell
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Building_Bed), "FindPreferredInteractionCell")]
[PatchLevel(Level.Mandatory)]
public static class Patch_Building_Bed_FindPreferredInteractionCell
{
  public static void Prefix(Building_Bed __instance, ref CellSearchPattern customSearchPattern)
  {
    if (!(__instance is Building_Hatch) || customSearchPattern != null)
      return;
    customSearchPattern = (CellSearchPattern) Building_Hatch.customBedInteractionCellsOrder;
  }
}
