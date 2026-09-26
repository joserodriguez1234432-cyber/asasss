// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_World
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public static class Ext_World
{
  public static PlanetTile Bfs(
    PlanetTile tile,
    Action<PlanetTile> processor,
    int radius = 1,
    Func<PlanetTile, bool> validator = null,
    Func<PlanetTile, PlanetTile, bool> result = null)
  {
    if (radius < 1)
    {
      Log.Error("Attempting to perform BFS search with max radius < 1");
      return tile;
    }
    Queue<PlanetTile> planetTileQueue = new Queue<PlanetTile>();
    planetTileQueue.Enqueue(tile);
    List<PlanetTile> planetTileList = new List<PlanetTile>();
    HashSet<PlanetTile> planetTileSet = new HashSet<PlanetTile>()
    {
      tile
    };
    int num1 = 0;
    int num2 = 0;
    int num3 = planetTileQueue.Count;
    for (; num1 < radius && planetTileQueue.Count > 0; ++num1)
    {
      for (int index = 0; index < num3; ++index)
      {
        PlanetTile planetTile1 = planetTileQueue.Dequeue();
        planetTileList.Clear();
        Find.WorldGrid.GetTileNeighbors(planetTile1, planetTileList);
        foreach (PlanetTile planetTile2 in planetTileList)
        {
          if (!planetTileSet.Contains(planetTile2) && (validator == null || validator(planetTile2)))
          {
            processor(planetTile2);
            ++num2;
            if (result != null && result(planetTile2, PlanetTile.op_Implicit(num1)))
              return planetTile2;
            planetTileQueue.Enqueue(planetTile2);
            planetTileSet.Add(planetTile2);
          }
        }
      }
      num3 = num2;
      num2 = 0;
    }
    return tile;
  }

  public static IEnumerable<PlanetTile> GetTileNeighbors(PlanetTile tile)
  {
    NativeArray<int> neighborsOffsets = ((PlanetTile) ref tile).Layer.UnsafeTileIDToNeighbors_offsets;
    NativeArray<PlanetTile> values = ((PlanetTile) ref tile).Layer.UnsafeTileIDToNeighbors_values;
    int num = neighborsOffsets[tile.tileId];
    int count = tile.tileId + 1 < neighborsOffsets.Length ? neighborsOffsets[tile.tileId + 1] : values.Length;
    for (int i = num; i < count; ++i)
      yield return values[i];
  }

  public static void GetTileNeighbors(
    PlanetTile tile,
    List<PlanetTile> tileNeighbors,
    int radius = 1,
    Vector3? nearestTo = null)
  {
    // ISSUE: object of a compiler-generated type is created
    // ISSUE: variable of a compiler-generated type
    Ext_World.\u003C\u003Ec__DisplayClass2_0 cDisplayClass20 = new Ext_World.\u003C\u003Ec__DisplayClass2_0();
    // ISSUE: reference to a compiler-generated field
    cDisplayClass20.radius = radius;
    // ISSUE: reference to a compiler-generated field
    cDisplayClass20.tileNeighbors = tileNeighbors;
    // ISSUE: reference to a compiler-generated field
    cDisplayClass20.nearestTo = nearestTo;
    // ISSUE: reference to a compiler-generated field
    if (cDisplayClass20.radius == 1)
    {
      // ISSUE: reference to a compiler-generated field
      Find.WorldGrid.GetTileNeighbors(tile, cDisplayClass20.tileNeighbors);
    }
    else
    {
      // ISSUE: method pointer
      ((PlanetTile) ref tile).Layer.Filler.FloodFill(tile, (Predicate<PlanetTile>) (_ => true), new Predicate<PlanetTile, int>((object) cDisplayClass20, __methodptr(\u003CGetTileNeighbors\u003Eb__1)), int.MaxValue, (IEnumerable<PlanetTile>) null);
      // ISSUE: reference to a compiler-generated field
      cDisplayClass20.worldGrid = Find.WorldGrid;
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      cDisplayClass20.c = cDisplayClass20.worldGrid.GetTileCenter(tile);
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      cDisplayClass20.n = ((Vector3) ref cDisplayClass20.c).normalized;
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated method
      cDisplayClass20.tileNeighbors.Sort(new Comparison<PlanetTile>(cDisplayClass20.\u003CGetTileNeighbors\u003Eb__2));
      // ISSUE: reference to a compiler-generated field
      if (!cDisplayClass20.nearestTo.HasValue)
        return;
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated method
      int num = PlanetTile.op_Implicit(GenCollection.MinBy<PlanetTile, float>((IEnumerable<PlanetTile>) cDisplayClass20.tileNeighbors, new Func<PlanetTile, float>(cDisplayClass20.\u003CGetTileNeighbors\u003Eb__3)));
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      cDisplayClass20.tileNeighbors = cDisplayClass20.tileNeighbors.ReorderOn<PlanetTile>(PlanetTile.op_Implicit(num));
    }
  }
}
