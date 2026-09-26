// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Game_FindMap
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Game), "FindMap", new Type[] {typeof (PlanetTile)})]
[PatchLevel(Level.Sensitive)]
public static class Patch_Game_FindMap
{
  public static void Postfix(ref Map __result, List<Map> ___maps, PlanetTile tile)
  {
    if (!VehicleMapUtility.get_IsVehicleMap(__result))
      return;
    __result = GenCollection.FirstOrDefault<Map>(___maps, (Predicate<Map>) (m => PlanetTile.op_Equality(m.Tile, tile) && !VehicleMapUtility.get_IsVehicleMap(m)));
  }
}
