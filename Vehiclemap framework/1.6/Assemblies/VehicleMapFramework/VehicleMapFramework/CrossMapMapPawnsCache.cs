// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CrossMapMapPawnsCache
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CrossMapMapPawnsCache
{
  private readonly List<Map> tmpMaps = new List<Map>(128 /*0x80*/);
  private readonly Dictionary<(Map map, Faction faction), CrossMapMapPawnsCache.Cache> cacheDict = new Dictionary<(Map, Faction), CrossMapMapPawnsCache.Cache>();
  private readonly CrossMapMapPawnsCache.PawnsGetter GetPawns;

  internal int CacheCount => this.cacheDict.Count;

  internal static List<CrossMapMapPawnsCache> AllInstances { get; } = new List<CrossMapMapPawnsCache>();

  public CrossMapMapPawnsCache(CrossMapMapPawnsCache.PawnsGetter getter)
  {
    this.GetPawns = getter;
    CrossMapMapPawnsCache.AllInstances.Add(this);
  }

  static CrossMapMapPawnsCache()
  {
    GameEvent.OnWorldRemoved += (Action) (() =>
    {
      foreach (CrossMapMapPawnsCache allInstance in CrossMapMapPawnsCache.AllInstances)
        allInstance.cacheDict.Clear();
    });
  }

  public List<Pawn> Get(Map map, IEnumerable<Pawn> result, Faction faction = null)
  {
    CrossMapMapPawnsCache.Cache cache;
    if (!this.cacheDict.TryGetValue((map, faction), out cache))
    {
      cache = new CrossMapMapPawnsCache.Cache();
      this.cacheDict.Add((map, faction), cache);
    }
    if (cache.lastCachedTick != GenTicks.TicksGame)
    {
      cache.lastCachedTick = GenTicks.TicksGame;
      this.Sum(map, result, cache.cachedPawns, faction);
    }
    return cache.cachedPawns;
  }

  private void Sum(Map map, IEnumerable<Pawn> result, List<Pawn> list, Faction faction)
  {
    list.Clear();
    list.AddRange(result);
    this.tmpMaps.Clear();
    map.VehicleMapsOnMap(this.tmpMaps);
    ReadOnlySpan<Map> readOnlySpan = GenList.AsReadOnlySpan<Map>(this.tmpMaps);
    for (int index = 0; index < readOnlySpan.Length; ++index)
    {
      Map map1 = readOnlySpan[index];
      list.AddRange((IEnumerable<Pawn>) this.GetPawns(map1.mapPawns, faction));
    }
  }

  public static void RemoveMap(Map map)
  {
    foreach (CrossMapMapPawnsCache allInstance in CrossMapMapPawnsCache.AllInstances)
      GenCollection.RemoveAll<(Map, Faction), CrossMapMapPawnsCache.Cache>(allInstance.cacheDict, (Predicate<KeyValuePair<(Map, Faction), CrossMapMapPawnsCache.Cache>>) (x => x.Key.map == map));
  }

  public static void ClearAll()
  {
    foreach (CrossMapMapPawnsCache allInstance in CrossMapMapPawnsCache.AllInstances)
    {
      foreach (KeyValuePair<(Map map, Faction faction), CrossMapMapPawnsCache.Cache> keyValuePair in allInstance.cacheDict)
        keyValuePair.Value.Clear();
    }
  }

  public delegate List<Pawn> PawnsGetter(MapPawns instance, Faction faction = null);

  private class Cache
  {
    public int lastCachedTick = -1;
    public readonly List<Pawn> cachedPawns = new List<Pawn>();

    public void Clear()
    {
      this.lastCachedTick = -1;
      this.cachedPawns.Clear();
    }
  }
}
