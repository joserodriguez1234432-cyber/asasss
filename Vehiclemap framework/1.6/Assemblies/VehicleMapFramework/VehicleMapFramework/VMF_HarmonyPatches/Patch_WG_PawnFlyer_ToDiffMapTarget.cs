// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_WG_PawnFlyer_ToDiffMapTarget
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_ExosuitFramework")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_WG_PawnFlyer_ToDiffMapTarget
{
  private static readonly Type t_Building_EjectorBay = GenTypes.GetTypeInAnyAssembly("Exosuit.Building_EjectorBay", "Exosuit");

  public static void Prefix(ref Thing ___eBay, Pawn pawn)
  {
    if (___eBay != null)
      return;
    HashSet<Map> source = ((Thing) pawn).MapHeld.BaseMapAndVehicleMaps(false);
    ___eBay = (Thing) source.SelectMany<Map, Building>((Func<Map, IEnumerable<Building>>) (m => (IEnumerable<Building>) m.listerBuildings.allBuildingsColonist)).FirstOrDefault<Building>((Func<Building, bool>) (b => b.GetType() == Patch_WG_PawnFlyer_ToDiffMapTarget.t_Building_EjectorBay));
  }
}
