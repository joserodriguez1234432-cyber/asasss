// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehiclePawnWithMapCache
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using SmashTools;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class VehiclePawnWithMapCache(Map map) : MapComponent(map)
{
  private readonly List<VehiclePawnWithMap> allVehicles = new List<VehiclePawnWithMap>();
  public (int lastCachedTick, HashSet<Map> includeItself, HashSet<Map> excludeItself) cachedBaseMapAndVehicleMaps = (-1, new HashSet<Map>(), new HashSet<Map>());
  public readonly Dictionary<Thing, Vector3> cachedDrawPos = new Dictionary<Thing, Vector3>();
  public readonly Dictionary<VehiclePawn, Rot8> cachedFullRot = new Dictionary<VehiclePawn, Rot8>();
  public readonly Dictionary<Thing, IntVec3> cachedPosOnBaseMap = new Dictionary<Thing, IntVec3>();
  private int lastCachedFrame = -1;
  private int lastCachedTick = -1;

  public static bool CacheMode { get; set; }

  private static List<VehiclePawnWithMap> EmptyList { get; } = new List<VehiclePawnWithMap>();

  public virtual void FinalizeInit()
  {
    VehicleMapParentsComponent.SetCachedVehicle(this.map, this.map.Parent as MapParent_Vehicle);
    if (!ModCompat.CompatBase<ModCompat.MultiFloors>.Active || VehicleMapParentsComponent.GetCachedVehicle(this.map) != null)
      return;
    VehicleMapParentsComponent.SetCachedVehicle(this.map, ModCompat.MultiFloors.GroundMap(this.map)?.Parent as MapParent_Vehicle);
  }

  public static void RegisterVehicle(VehiclePawnWithMap vehicle)
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      Map map = ((Thing) vehicle).Map;
      if (map == null)
        return;
      VehiclePawnWithMapCache component = map.GetComponent<VehiclePawnWithMapCache>();
      if (component == null)
        return;
      GenCollection.AddUnique<VehiclePawnWithMap>(component.allVehicles, vehicle);
    }));
    foreach (Map map in Find.Maps)
    {
      VehiclePawnWithMapCache component = map.GetComponent<VehiclePawnWithMapCache>();
      if (component != null)
        component.cachedBaseMapAndVehicleMaps.lastCachedTick = -1;
    }
  }

  public static void DeRegisterVehicle(VehiclePawnWithMap vehicle)
  {
    foreach (Map map in Find.Maps)
    {
      VehiclePawnWithMapCache component = map.GetComponent<VehiclePawnWithMapCache>();
      if (component != null)
      {
        component.allVehicles.Remove(vehicle);
        component.cachedBaseMapAndVehicleMaps.lastCachedTick = -1;
      }
    }
    if (Command_FocusVehicleMap.FocusedVehicle != vehicle)
      return;
    Command_FocusVehicleMap.FocusLockedVehicle = (VehiclePawnWithMap) null;
    Command_FocusVehicleMap.FocusedVehicle = (VehiclePawnWithMap) null;
  }

  public static List<VehiclePawnWithMap> AllVehiclesOn(Map map)
  {
    return map.mapPawns.AllPawnsSpawnedCount == 0 ? VehiclePawnWithMapCache.EmptyList : ComponentCache.GetCachedMapComponent<VehiclePawnWithMapCache>(map)?.allVehicles ?? VehiclePawnWithMapCache.EmptyList;
  }

  public static ReadOnlySpan<VehiclePawnWithMap> AllVehiclesOnAsReadOnlySpan(Map map)
  {
    if (map.mapPawns.AllPawnsSpawnedCount == 0)
      return new ReadOnlySpan<VehiclePawnWithMap>();
    VehiclePawnWithMapCache cachedMapComponent = ComponentCache.GetCachedMapComponent<VehiclePawnWithMapCache>(map);
    return cachedMapComponent != null ? GenList.AsReadOnlySpan<VehiclePawnWithMap>(cachedMapComponent.allVehicles) : new ReadOnlySpan<VehiclePawnWithMap>();
  }

  public void ForceResetPositionCache()
  {
    this.lastCachedTick = Find.TickManager.TicksGame;
    this.cachedPosOnBaseMap.Clear();
    this.cachedFullRot.Clear();
  }

  public void ForceResetDrawPosCache()
  {
    this.lastCachedFrame = Time.frameCount;
    this.cachedDrawPos.Clear();
  }

  public void ResetCache()
  {
    if (this.lastCachedTick != Find.TickManager.TicksGame)
      this.ForceResetPositionCache();
    if (this.lastCachedFrame == Time.frameCount)
      return;
    this.ForceResetDrawPosCache();
  }

  public virtual void MapComponentUpdate() => this.ResetCache();

  public virtual void MapRemoved()
  {
    VehicleMapParentsComponent.SetCachedVehicle(this.map, (MapParent_Vehicle) null);
    CrossMapReachabilityCache.ClearCacheFor(this.map, true);
    CrossMapMapPawnsCache.RemoveMap(this.map);
  }
}
