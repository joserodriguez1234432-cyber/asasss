// Decompiled with JetBrains decompiler
// Type: Vehicles.DropZoneFinder
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using Verse;

#nullable disable
namespace Vehicles;

public static class DropZoneFinder
{
  private const string MountainCategory = "Mountain";

  public static DropZone GetDropZone(Map map, Rot4 fromEdge, int points)
  {
    return map.GetCachedMapComponent<AirdropManager>().GetDropZoneFor(fromEdge, points);
  }

  public static bool CanAirdropInMap(this Map map)
  {
    PlanetTile tile = map.Tile;
    return !((PlanetTile) ref tile).Tile.Mutators.NotNullAndAny<TileMutatorDef>(new Predicate<TileMutatorDef>(InvalidDropArea));

    static bool InvalidDropArea(TileMutatorDef def) => def.IsCave;
  }
}
