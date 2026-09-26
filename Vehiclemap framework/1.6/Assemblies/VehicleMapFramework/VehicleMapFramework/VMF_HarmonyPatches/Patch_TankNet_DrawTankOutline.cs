// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_TankNet_DrawTankOutline
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_Aquariums")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_TankNet_DrawTankOutline
{
  public static bool Prefix(List<IntVec3> ___netCells, Map ___map)
  {
    List<IntVec3> cells = ___netCells;
    Color lightBlue = ColorLibrary.LightBlue;
    Map map1 = ___map;
    float? altOffset = new float?();
    Map map2 = map1;
    GenDrawOnVehicle.DrawFieldEdges(cells, lightBlue, altOffset, map: map2);
    return false;
  }
}
