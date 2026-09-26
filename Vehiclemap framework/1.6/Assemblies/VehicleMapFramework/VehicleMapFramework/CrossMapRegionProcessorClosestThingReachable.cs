// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CrossMapRegionProcessorClosestThingReachable
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using VehicleMapFramework.VMF_HarmonyPatches;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public class CrossMapRegionProcessorClosestThingReachable : RegionProcessorClosestThingReachable
{
  private Map rootMap;
  private TraverseParms traverseParams;
  private float maxDistance;
  private float maxDistSquared;
  private IntVec3 root;
  private ThingRequest req;

  public void SetParameters(
    TraverseParms _traverseParams,
    float _maxDistance,
    IntVec3 _root,
    bool ignoreEntirelyForbiddenRegions,
    ThingRequest req,
    PathEndMode peMode,
    Func<Thing, float> priorityGetter,
    Predicate<Thing> validator,
    int minRegions,
    float closestDistSquared = 9999999f,
    int _regionsSeenScan = 0,
    float bestPrio = -3.40282347E+38f,
    Thing _closestThing = null,
    bool lookInHaulSources = false,
    Map _rootMap = null)
  {
    this.SetParameters(_traverseParams, _maxDistance, _root, ignoreEntirelyForbiddenRegions, req, peMode, priorityGetter, validator, minRegions, closestDistSquared, _regionsSeenScan, bestPrio, _closestThing, lookInHaulSources);
    this.rootMap = _rootMap;
    this.traverseParams = _traverseParams;
    this.maxDistance = _maxDistance;
    this.root = _root;
    this.maxDistSquared = _maxDistance * _maxDistance;
    this.req = req;
  }

  public void Clear()
  {
    base.Clear();
    this.rootMap = (Map) null;
  }

  protected virtual bool RegionEntryPredicate(Region from, Region to)
  {
    if (to.Room == null || !to.Allows(this.traverseParams, false))
      return false;
    VehiclePawnWithMap vehicle1;
    if (this.traverseParams.avoidPersistentDanger && from.Map != to.Map && from.Map.IsVehicleMapOf(out vehicle1) && !VehicleMapUtility.get_IsVehicleMap(to.Map))
    {
      TerrainDef terrain = GridsUtility.GetTerrain(((Thing) vehicle1).Position, ((Thing) vehicle1).Map);
      if (terrain == null || terrain.dangerous)
        return false;
    }
    if ((double) this.maxDistance > 5000.0)
      return true;
    VehiclePawnWithMap vehicle2;
    IntVec3 intVec3 = to.Map.IsVehicleMapOf(out vehicle2) ? this.root.ToVehicleMapCoord(vehicle2) : this.root;
    return (double) ((CellRect) ref to.extentsClose).ClosestDistSquaredTo(intVec3) < (double) this.maxDistSquared;
  }

  protected virtual bool RegionProcessor(Region reg)
  {
    if (reg.Map != this.rootMap)
      return this.RegionProcessorBaseMapCoord(reg);
    if (RegionTraverser.ShouldCountRegion(reg))
      ++this.regionsSeenScan;
    return false;
  }
}
