// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Map_TileInfo
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
public static class Patch_Map_TileInfo
{
  public static void Postfix(Map __instance, ref Tile __result)
  {
    if (!__instance.IsVehicleMapOf(out VehiclePawnWithMap _) || !Find.Maps.Contains(__instance))
      return;
    PlanetTile tile = __instance.Tile;
    if (!((PlanetTile) ref tile).Valid || !Find.WorldGrid.InBounds(__instance.Tile))
      return;
    __result = Find.WorldGrid[__instance.Tile];
  }
}
