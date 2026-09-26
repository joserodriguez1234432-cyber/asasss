// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_RoofGrid_Roofed
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (RoofGrid), "Roofed", new Type[] {typeof (IntVec3)})]
[PatchLevel(Level.Safe)]
public static class Patch_RoofGrid_Roofed
{
  private static bool Prepare()
  {
    VehicleMapSettings settings = VehicleMapFramework.VehicleMapFramework.settings;
    return settings != null && settings.roofedPatch;
  }

  public static void Postfix(IntVec3 c, Map ___map, ref bool __result)
  {
    VehiclePawnWithMap vehicle;
    if (!___map.IsVehicleMapOf(out vehicle) || !((Thing) vehicle).Spawned)
      return;
    IntVec3 baseMapCoord;
    __result = __result || GenGrid.InBounds(baseMapCoord = c.ToBaseMapCoord(vehicle), ((Thing) vehicle).Map) && ((Thing) vehicle).Map.roofGrid.RoofAt(baseMapCoord) != null;
  }
}
