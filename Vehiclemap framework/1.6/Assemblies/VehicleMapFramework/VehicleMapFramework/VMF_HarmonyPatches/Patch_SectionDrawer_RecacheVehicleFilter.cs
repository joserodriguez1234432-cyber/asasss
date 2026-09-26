// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_SectionDrawer_RecacheVehicleFilter
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_SectionDrawer_RecacheVehicleFilter
{
  public static void Postfix(List<VehicleDef> ___filteredVehicleDefs)
  {
    ___filteredVehicleDefs.RemoveAll((Predicate<VehicleDef>) (d =>
    {
      VehicleMapProps_Unique modExtension = ((Def) d).GetModExtension<VehicleMapProps_Unique>();
      return modExtension != null && modExtension.baseDef != null;
    }));
  }
}
