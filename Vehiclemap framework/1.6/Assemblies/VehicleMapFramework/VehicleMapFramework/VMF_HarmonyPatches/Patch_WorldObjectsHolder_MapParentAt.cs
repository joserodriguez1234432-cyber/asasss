// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_WorldObjectsHolder_MapParentAt
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

[HarmonyPatch(typeof (WorldObjectsHolder), "MapParentAt")]
[PatchLevel(Level.Sensitive)]
public static class Patch_WorldObjectsHolder_MapParentAt
{
  public static void Postfix(
    ref MapParent __result,
    List<MapParent> ___mapParents,
    PlanetTile tile)
  {
    if (!(__result is MapParent_Vehicle))
      return;
    __result = GenCollection.FirstOrDefault<MapParent>(___mapParents, (Predicate<MapParent>) (p => PlanetTile.op_Equality(((WorldObject) p).Tile, tile) && !(p is MapParent_Vehicle)));
  }
}
