// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CrossMapReachabilityCache
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using LudeonTK;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CrossMapReachabilityCache(World world) : WorldComponent(world)
{
  private readonly Dictionary<CrossMapReachabilityCache.CachedEntry, (bool result, TargetInfo exitSpot, TargetInfo enterSpot, List<TraverseSpots> spotsQueue)> cache = new Dictionary<CrossMapReachabilityCache.CachedEntry, (bool, TargetInfo, TargetInfo, List<TraverseSpots>)>();
  private readonly Dictionary<int, HashSet<CrossMapReachabilityCache.CachedEntry>> removalDic = new Dictionary<int, HashSet<CrossMapReachabilityCache.CachedEntry>>();

  public static CrossMapReachabilityCache Instance { get; private set; }

  public virtual void FinalizeInit(bool fromLoad)
  {
    base.FinalizeInit(fromLoad);
    CrossMapReachabilityCache.Instance = Find.World.GetComponent<CrossMapReachabilityCache>();
  }

  public static void ClearCache()
  {
    foreach ((bool result, TargetInfo exitSpot, TargetInfo enterSpot, List<TraverseSpots> spotsQueue) tuple in CrossMapReachabilityCache.Instance.cache.Values)
    {
      if (tuple.spotsQueue != null)
      {
        tuple.spotsQueue.Clear();
        SimplePool<List<TraverseSpots>>.Return(tuple.spotsQueue);
      }
    }
    CrossMapReachabilityCache.Instance.cache.Clear();
    foreach (HashSet<CrossMapReachabilityCache.CachedEntry> cachedEntrySet in CrossMapReachabilityCache.Instance.removalDic.Values)
      cachedEntrySet.Clear();
  }

  public static void ClearCacheFor(Map map, bool cleanup = false)
  {
    if (map == null)
      return;
    ClearInner(map, cleanup);
    foreach (VehiclePawnWithMap vehiclePawnWithMap in VehiclePawnWithMapCache.AllVehiclesOn(map))
      ClearInner(vehiclePawnWithMap.VehicleMap, cleanup);

    static void ClearInner(Map map, bool cleanup)
    {
      HashSet<CrossMapReachabilityCache.CachedEntry> cachedEntrySet;
      if (CrossMapReachabilityCache.Instance.removalDic.TryGetValue(map.uniqueID, out cachedEntrySet))
      {
        foreach (CrossMapReachabilityCache.CachedEntry key in cachedEntrySet)
        {
          (bool result, TargetInfo exitSpot, TargetInfo enterSpot, List<TraverseSpots> spotsQueue) tuple;
          if (CrossMapReachabilityCache.Instance.cache.TryGetValue(key, out tuple) && tuple.spotsQueue != null)
          {
            tuple.spotsQueue.Clear();
            SimplePool<List<TraverseSpots>>.Return(tuple.spotsQueue);
            tuple.spotsQueue = (List<TraverseSpots>) null;
          }
          CrossMapReachabilityCache.Instance.cache.Remove(key);
        }
        cachedEntrySet.Clear();
      }
      if (!cleanup)
        return;
      CrossMapReachabilityCache.Instance.removalDic.Remove(map.uniqueID);
    }
  }

  public static bool TryGetCache(
    Region A,
    Region B,
    TraverseParmsExtended traverseParms,
    out bool result,
    out TargetInfo exitSpot,
    out TargetInfo enterSpot,
    out List<TraverseSpots> spotsQueue)
  {
    if (A == null || B == null)
    {
      result = false;
      exitSpot = TargetInfo.Invalid;
      enterSpot = TargetInfo.Invalid;
      spotsQueue = (List<TraverseSpots>) null;
      return false;
    }
    (bool result, TargetInfo exitSpot, TargetInfo enterSpot, List<TraverseSpots> spotsQueue) tuple;
    if (CrossMapReachabilityCache.Instance.cache.TryGetValue(new CrossMapReachabilityCache.CachedEntry(A, B, traverseParms), out tuple))
    {
      result = tuple.result;
      exitSpot = tuple.exitSpot;
      enterSpot = tuple.enterSpot;
      spotsQueue = tuple.spotsQueue;
      return true;
    }
    result = false;
    exitSpot = TargetInfo.Invalid;
    enterSpot = TargetInfo.Invalid;
    spotsQueue = (List<TraverseSpots>) null;
    return false;
  }

  public static void Cache(
    Region A,
    Region B,
    TraverseParmsExtended traverseParms,
    bool result,
    TargetInfo exitSpot,
    TargetInfo enterSpot,
    List<TraverseSpots> spotsQueue)
  {
    if (A == null || B == null)
      return;
    CrossMapReachabilityCache.CachedEntry key = new CrossMapReachabilityCache.CachedEntry(A, B, traverseParms);
    CrossMapReachabilityCache.Instance.cache[key] = (result, exitSpot, enterSpot, spotsQueue);
    if (spotsQueue == null)
    {
      int uniqueId1 = A.Map.uniqueID;
      HashSet<CrossMapReachabilityCache.CachedEntry> cachedEntrySet1;
      if (!CrossMapReachabilityCache.Instance.removalDic.TryGetValue(uniqueId1, out cachedEntrySet1))
        CrossMapReachabilityCache.Instance.removalDic[uniqueId1] = cachedEntrySet1 = new HashSet<CrossMapReachabilityCache.CachedEntry>();
      cachedEntrySet1.Add(key);
      int uniqueId2 = B.Map.uniqueID;
      HashSet<CrossMapReachabilityCache.CachedEntry> cachedEntrySet2;
      if (!CrossMapReachabilityCache.Instance.removalDic.TryGetValue(uniqueId2, out cachedEntrySet2))
        CrossMapReachabilityCache.Instance.removalDic[uniqueId2] = cachedEntrySet2 = new HashSet<CrossMapReachabilityCache.CachedEntry>();
      cachedEntrySet2.Add(key);
    }
    else
    {
      foreach (TraverseSpots spots in spotsQueue)
      {
        TargetInfo targetInfo;
        if (((TargetInfo) ref exitSpot).Map != null)
        {
          targetInfo = spots.exitSpot;
          int uniqueId = ((TargetInfo) ref targetInfo).Map.uniqueID;
          HashSet<CrossMapReachabilityCache.CachedEntry> cachedEntrySet;
          if (!CrossMapReachabilityCache.Instance.removalDic.TryGetValue(uniqueId, out cachedEntrySet))
            CrossMapReachabilityCache.Instance.removalDic[uniqueId] = cachedEntrySet = new HashSet<CrossMapReachabilityCache.CachedEntry>();
          cachedEntrySet.Add(key);
        }
        if (((TargetInfo) ref enterSpot).Map != null)
        {
          targetInfo = spots.enterSpot;
          int uniqueId = ((TargetInfo) ref targetInfo).Map.uniqueID;
          HashSet<CrossMapReachabilityCache.CachedEntry> cachedEntrySet;
          if (!CrossMapReachabilityCache.Instance.removalDic.TryGetValue(uniqueId, out cachedEntrySet))
            CrossMapReachabilityCache.Instance.removalDic[uniqueId] = cachedEntrySet = new HashSet<CrossMapReachabilityCache.CachedEntry>();
          cachedEntrySet.Add(key);
        }
      }
    }
  }

  [DebugAction("Vehicle Map Framework", "Clear CrossMapReachabilityCache", false, false, false, false, false, 0, false)]
  private static void ClearCacheAction() => CrossMapReachabilityCache.ClearCache();

  private readonly struct CachedEntry : IEquatable<CrossMapReachabilityCache.CachedEntry>
  {
    private Region FirstRegion { get; }

    private Region SecondRegion { get; }

    private TraverseParmsExtended TraverseParms { get; }

    public CachedEntry(
      Region firstRegion,
      Region secondRegion,
      TraverseParmsExtended traverseParms)
      : this()
    {
      this.FirstRegion = firstRegion;
      this.SecondRegion = secondRegion;
      this.TraverseParms = traverseParms;
    }

    public static bool operator ==(
      CrossMapReachabilityCache.CachedEntry lhs,
      CrossMapReachabilityCache.CachedEntry rhs)
    {
      return lhs.Equals(rhs);
    }

    public static bool operator !=(
      CrossMapReachabilityCache.CachedEntry lhs,
      CrossMapReachabilityCache.CachedEntry rhs)
    {
      return !lhs.Equals(rhs);
    }

    public override bool Equals(object obj)
    {
      return obj is CrossMapReachabilityCache.CachedEntry other && this.Equals(other);
    }

    public bool Equals(CrossMapReachabilityCache.CachedEntry other)
    {
      return this.FirstRegion == other.FirstRegion && this.SecondRegion == other.SecondRegion && this.TraverseParms == other.TraverseParms;
    }

    public override int GetHashCode()
    {
      return Gen.HashCombineStruct<TraverseParmsExtended>(Gen.HashCombineInt(this.FirstRegion.id, this.SecondRegion.id), this.TraverseParms);
    }
  }
}
