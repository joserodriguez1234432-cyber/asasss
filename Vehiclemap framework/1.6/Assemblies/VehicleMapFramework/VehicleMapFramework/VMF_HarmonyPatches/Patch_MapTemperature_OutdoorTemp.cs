// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_MapTemperature_OutdoorTemp
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld.Planet;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_MapTemperature_OutdoorTemp
{
  public static bool Prefix(Map ___map, ref float __result)
  {
    VehiclePawnWithMap vehicle;
    if (!___map.IsVehicleMapOf(out vehicle))
      return true;
    if (((Thing) vehicle).Spawned)
    {
      __result = GridsUtility.GetTemperature(((Thing) vehicle).Position, ((Thing) vehicle).Map);
    }
    else
    {
      PlanetTile tile = ((Thing) vehicle).Tile;
      if (((PlanetTile) ref tile).Valid)
        __result = Find.World.tileTemperatures.GetOutdoorTemp(((Thing) vehicle).Tile);
    }
    return false;
  }
}
