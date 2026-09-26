// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleReachabilityCache
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Verse;

#nullable disable
namespace Vehicles;

public class VehicleReachabilityCache
{
  private ConcurrentDictionary<VehicleReachabilityCache.CachedEntry, bool> cacheDict = new ConcurrentDictionary<VehicleReachabilityCache.CachedEntry, bool>();
  [ThreadStatic]
  private static HashSet<VehicleReachabilityCache.CachedEntry> tmpCachedEntries;

  private static HashSet<VehicleReachabilityCache.CachedEntry> CachedEntries
  {
    get
    {
      if (VehicleReachabilityCache.tmpCachedEntries == null)
        VehicleReachabilityCache.tmpCachedEntries = new HashSet<VehicleReachabilityCache.CachedEntry>();
      return VehicleReachabilityCache.tmpCachedEntries;
    }
  }

  public int Count => this.cacheDict.Count;

  public void Clear() => this.cacheDict.Clear();

  public BoolUnknown CachedResultFor(VehicleRoom from, VehicleRoom to, TraverseParms traverseParms)
  {
    bool flag;
    if (!this.cacheDict.TryGetValue(new VehicleReachabilityCache.CachedEntry(from.id, to.id, traverseParms), out flag))
      return (BoolUnknown) 2;
    return !flag ? (BoolUnknown) 1 : (BoolUnknown) 0;
  }

  public void AddCachedResult(
    VehicleRoom from,
    VehicleRoom to,
    TraverseParms traverseParams,
    bool reachable)
  {
    this.cacheDict.TryAdd(new VehicleReachabilityCache.CachedEntry(from.id, to.id, traverseParams), reachable);
  }

  public void ClearFor(VehiclePawn vehicle)
  {
    VehicleReachabilityCache.CachedEntries.Clear();
    bool flag;
    foreach (KeyValuePair<VehicleReachabilityCache.CachedEntry, bool> keyValuePair in this.cacheDict)
    {
      VehicleReachabilityCache.CachedEntry cachedEntry1;
      keyValuePair.Deconstruct(ref cachedEntry1, ref flag);
      VehicleReachabilityCache.CachedEntry cachedEntry2 = cachedEntry1;
      if (cachedEntry2.traverseParms.pawn == vehicle)
        VehicleReachabilityCache.CachedEntries.Add(cachedEntry2);
    }
    foreach (VehicleReachabilityCache.CachedEntry cachedEntry in VehicleReachabilityCache.CachedEntries)
      this.cacheDict.TryRemove(cachedEntry, out flag);
    VehicleReachabilityCache.CachedEntries.Clear();
  }

  public void ClearForHostile(Thing hostileTo)
  {
    VehicleReachabilityCache.CachedEntries.Clear();
    bool flag;
    foreach (KeyValuePair<VehicleReachabilityCache.CachedEntry, bool> keyValuePair in this.cacheDict)
    {
      VehicleReachabilityCache.CachedEntry cachedEntry1;
      keyValuePair.Deconstruct(ref cachedEntry1, ref flag);
      VehicleReachabilityCache.CachedEntry cachedEntry2 = cachedEntry1;
      Pawn pawn = cachedEntry2.traverseParms.pawn;
      if (pawn != null && GenHostility.HostileTo((Thing) pawn, hostileTo))
        VehicleReachabilityCache.CachedEntries.Add(cachedEntry2);
    }
    foreach (VehicleReachabilityCache.CachedEntry cachedEntry in VehicleReachabilityCache.CachedEntries)
      this.cacheDict.TryRemove(cachedEntry, out flag);
    VehicleReachabilityCache.CachedEntries.Clear();
  }

  private readonly record struct CachedEntry
  {
    public readonly int from;
    public readonly int to;
    public readonly TraverseParms traverseParms;

    public CachedEntry(int from, int to, TraverseParms traverseParms)
      : this()
    {
      if (from < to)
      {
        this.from = from;
        this.to = to;
      }
      else
      {
        this.from = to;
        this.to = from;
      }
      this.traverseParms = traverseParms;
    }

    public override int GetHashCode()
    {
      return Gen.HashCombineStruct<TraverseParms>(Gen.HashCombineInt(this.from, this.to), this.traverseParms);
    }

    [CompilerGenerated]
    public bool Equals(VehicleReachabilityCache.CachedEntry other)
    {
      return EqualityComparer<int>.Default.Equals(this.from, other.from) && EqualityComparer<int>.Default.Equals(this.to, other.to) && EqualityComparer<TraverseParms>.Default.Equals(this.traverseParms, other.traverseParms);
    }
  }
}
