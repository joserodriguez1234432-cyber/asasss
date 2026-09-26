// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.GenClosestCrossMap
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public static class GenClosestCrossMap
{
  private static readonly List<Thing> tmpThings = new List<Thing>();

  private static bool EarlyOutSearch(
    IntVec3 start,
    ref Map map,
    ThingRequest thingReq,
    IEnumerable<Thing> customGlobalSearchSet)
  {
    if (thingReq.group == 2)
    {
      Log.Error("Cannot do ClosestThingReachable searching everything without restriction.");
      return true;
    }
    if (!GenGrid.InBounds(start, map))
    {
      map = VehicleMapUtility.get_GroundMap(map);
      if (!GenGrid.InBounds(start, map))
      {
        Log.Error($"Did FindClosestThing with start out of bounds ({(object) start}), thingReq={(object) thingReq}");
        return true;
      }
    }
    if (thingReq.group == 1)
      return true;
    bool isUndefined = ((ThingRequest) ref thingReq).IsUndefined;
    bool flag = true;
    if (!isUndefined)
    {
      foreach (Map mapAndVehicleMap in map.BaseMapAndVehicleMaps(false))
      {
        if (GenCollection.Any<Thing>(mapAndVehicleMap.listerThings.ThingsMatching(thingReq)))
        {
          flag = false;
          break;
        }
      }
    }
    return isUndefined | flag && GenCollection.EnumerableNullOrEmpty<Thing>(customGlobalSearchSet);
  }

  public static Thing ClosestThingReachable(
    IntVec3 root,
    Map map,
    ThingRequest thingReq,
    PathEndMode peMode,
    TraverseParms traverseParams,
    float maxDistance = 9999f,
    Predicate<Thing> validator = null,
    IEnumerable<Thing> customGlobalSearchSet = null,
    int searchRegionsMin = 0,
    int searchRegionsMax = -1,
    bool forceAllowGlobalSearch = false,
    RegionType traversableRegionTypes = 14,
    bool ignoreEntirelyForbiddenRegions = false,
    bool lookInHaulSources = false)
  {
    bool flag1 = searchRegionsMax < 0 | forceAllowGlobalSearch;
    if (!flag1 && customGlobalSearchSet != null)
      Log.ErrorOnce("searchRegionsMax >= 0 && customGlobalSearchSet != null && !forceAllowGlobalSearch. customGlobalSearchSet will never be used.", 634984);
    if (!flag1 && !((ThingRequest) ref thingReq).IsUndefined && !((ThingRequest) ref thingReq).CanBeFoundInRegion)
    {
      Log.ErrorOnce($"ClosestThingReachable with thing request group {thingReq.group.ToString()} and global search not allowed. This will never find anything because this group is never stored in regions. Either allow global search or don't call this method at all.", 518498981);
      return (Thing) null;
    }
    if (map == null)
      return (Thing) null;
    if (GenClosestCrossMap.EarlyOutSearch(root, ref map, thingReq, customGlobalSearchSet))
      return (Thing) null;
    Thing thing = (Thing) null;
    bool flag2 = false;
    if (!((ThingRequest) ref thingReq).IsUndefined && ((ThingRequest) ref thingReq).CanBeFoundInRegion)
    {
      int maxRegions = searchRegionsMax > 0 ? searchRegionsMax : 30;
      int regionsSeen;
      thing = GenClosestCrossMap.RegionwiseBFSWorker(root, map, thingReq, peMode, traverseParams, validator, (Func<Thing, float>) null, searchRegionsMin, maxRegions, maxDistance, out regionsSeen, traversableRegionTypes, ignoreEntirelyForbiddenRegions, lookInHaulSources);
      flag2 = thing == null && regionsSeen < maxRegions;
    }
    if (thing == null & flag1 && !flag2)
    {
      if (traversableRegionTypes != 14)
        Log.ErrorOnce("ClosestThingReachable had to do a global search, but traversableRegionTypes is not set to passable only. It's not supported, because Reachability is based on passable regions only.", 14384767);
      VehiclePawnWithMap vehicle;
      IntVec3 centerOnBaseMap = map.IsVehicleMapOf(out vehicle) ? root.ToBaseMapCoord(vehicle) : root;
      if (customGlobalSearchSet == null)
      {
        GenClosestCrossMap.tmpThings.Clear();
        foreach (Map mapAndVehicleMap in map.BaseMapAndVehicleMaps(false))
        {
          List<Thing> thingList = mapAndVehicleMap.listerThings.ThingsMatching(thingReq);
          for (int index = 0; index < thingList.Count; ++index)
            GenClosestCrossMap.tmpThings.Add(thingList[index]);
        }
      }
      Pawn pawn = traverseParams.pawn;
      Map departMap = (pawn != null ? CrossMapReachabilityUtility.get_DepartMap(pawn) : (Map) null) ?? map;
      thing = GenClosestCrossMap.ClosestThing_Global(centerOnBaseMap, (IEnumerable) (customGlobalSearchSet ?? (IEnumerable<Thing>) GenClosestCrossMap.tmpThings), maxDistance, new Predicate<Thing>(GlobalValidator));

      bool GlobalValidator(Thing t)
      {
        if (!CrossMapReachabilityUtility.CanReach(departMap, root, LocalTargetInfo.op_Implicit(t), peMode, traverseParams, t.MapHeld))
          return false;
        return validator == null || validator(t);
      }
    }
    return thing;
  }

  public static Thing ClosestThing_Regionwise_ReachablePrioritized(
    IntVec3 root,
    Map map,
    ThingRequest thingReq,
    PathEndMode peMode,
    TraverseParms traverseParams,
    float maxDistance = 9999f,
    Predicate<Thing> validator = null,
    Func<Thing, float> priorityGetter = null,
    int minRegions = 24,
    int maxRegions = 30,
    bool lookInHaulSources = false)
  {
    if (!((ThingRequest) ref thingReq).IsUndefined && !((ThingRequest) ref thingReq).CanBeFoundInRegion)
    {
      Log.ErrorOnce($"ClosestThing_Regionwise_ReachablePrioritized with thing request group {thingReq.group.ToString()}. This will never find anything because this group is never stored in regions. Most likely a global search should have been used.", 738476712);
      return (Thing) null;
    }
    if (GenClosestCrossMap.EarlyOutSearch(root, ref map, thingReq, (IEnumerable<Thing>) null))
      return (Thing) null;
    if (maxRegions < minRegions)
      Log.ErrorOnce("maxRegions < minRegions", 754343);
    Thing thing = (Thing) null;
    if (!((ThingRequest) ref thingReq).IsUndefined)
      thing = GenClosestCrossMap.RegionwiseBFSWorker(root, map, thingReq, peMode, traverseParams, validator, priorityGetter, minRegions, maxRegions, maxDistance, out int _, lookInHaulSources: lookInHaulSources);
    return thing;
  }

  public static Thing RegionwiseBFSWorker(
    IntVec3 root,
    Map map,
    ThingRequest req,
    PathEndMode peMode,
    TraverseParms traverseParams,
    Predicate<Thing> validator,
    Func<Thing, float> priorityGetter,
    int minRegions,
    int maxRegions,
    float maxDistance,
    out int regionsSeen,
    RegionType traversableRegionTypes = 14,
    bool ignoreEntirelyForbiddenRegions = false,
    bool lookInHaulSources = false)
  {
    regionsSeen = 0;
    switch ((int) traverseParams.mode)
    {
      case 3:
        Log.Error("RegionwiseBFSWorker with traverseParams.mode PassAllDestroyableThings. Use ClosestThingGlobal.");
        return (Thing) null;
      case 4:
        Log.Error("RegionwiseBFSWorker with traverseParams.mode PassAllDestroyablePlayerOwnedThings. Use ClosestThingGlobal.");
        return (Thing) null;
      case 6:
        Log.Error("RegionwiseBFSWorker with traverseParams.mode PassAllDestroyableThingsNotWater. Use ClosestThingGlobal.");
        return (Thing) null;
      default:
        if (!((ThingRequest) ref req).IsUndefined && !((ThingRequest) ref req).CanBeFoundInRegion)
        {
          Log.ErrorOnce($"RegionwiseBFSWorker with thing request group {(object) req.group}. This group is never stored in regions. Most likely a global search should have been used.", 385766189);
          return (Thing) null;
        }
        Region region = GridsUtility.GetRegion(root, map, traversableRegionTypes);
        if (region == null)
          return (Thing) null;
        CrossMapRegionProcessorClosestThingReachable processor = SimplePool<CrossMapRegionProcessorClosestThingReachable>.Get();
        VehiclePawnWithMap vehicle;
        IntVec3 _root = map.IsVehicleMapOf(out vehicle) ? root.ToBaseMapCoord(vehicle) : root;
        processor.SetParameters(traverseParams, maxDistance, _root, ignoreEntirelyForbiddenRegions, req, peMode, priorityGetter, validator, minRegions, lookInHaulSources: lookInHaulSources, _rootMap: map);
        RegionTraverserAcrossMaps.BreadthFirstTraverse(region, (RegionProcessorDelegateCache) processor, maxRegions, traversableRegionTypes);
        regionsSeen = processor.regionsSeenScan;
        Thing closestThing = processor.closestThing;
        processor.Clear();
        SimplePool<CrossMapRegionProcessorClosestThingReachable>.Return(processor);
        return closestThing;
    }
  }

  public static Thing ClosestThing_Global(
    IntVec3 centerOnBaseMap,
    IEnumerable searchSet,
    float maxDistance = 99999f,
    Predicate<Thing> validator = null,
    Func<Thing, float> priorityGetter = null,
    bool lookInHaulSources = false)
  {
    if (searchSet == null)
      return (Thing) null;
    float closestDistSquared = (float) int.MaxValue;
    Thing chosen = (Thing) null;
    float bestPrio = float.MinValue;
    float maxDistanceSquared = maxDistance * maxDistance;
    if (searchSet is IList<Thing> thingList)
    {
      for (int index = 0; index < thingList.Count; ++index)
        Process(thingList[index]);
    }
    else if (searchSet is IList<Pawn> pawnList)
    {
      for (int index = 0; index < pawnList.Count; ++index)
        Process((Thing) pawnList[index]);
    }
    else if (searchSet is IList<Building> buildingList)
    {
      for (int index = 0; index < buildingList.Count; ++index)
        Process((Thing) buildingList[index]);
    }
    else if (searchSet is IList<IAttackTarget> iattackTargetList)
    {
      for (int index = 0; index < iattackTargetList.Count; ++index)
        Process((Thing) iattackTargetList[index]);
    }
    else
    {
      foreach (Thing search in searchSet)
        Process(search);
    }
    return chosen;

    void Process(Thing t)
    {
      if (!t.Spawned && !HaulAIUtility.IsInHaulableInventory(t))
        return;
      IntVec3 intVec3 = IntVec3.op_Subtraction(centerOnBaseMap, VehicleMapUtility.get_PositionHeldOnBaseMap(t));
      float horizontalSquared = (float) ((IntVec3) ref intVec3).LengthHorizontalSquared;
      if ((double) horizontalSquared > (double) maxDistanceSquared || priorityGetter == null && (double) horizontalSquared >= (double) closestDistSquared)
        return;
      ValidateThing(t, horizontalSquared);
      if (!lookInHaulSources || !(t is IHaulSource ihaulSource))
        return;
      ThingOwner directlyHeldThings = ((IThingHolder) ihaulSource).GetDirectlyHeldThings();
      for (int index = 0; index < directlyHeldThings.Count; ++index)
        ValidateThing(directlyHeldThings[index], horizontalSquared);
    }

    void ValidateThing(Thing t, float distSquared)
    {
      if (validator != null && !validator(t))
        return;
      float num = 0.0f;
      if (priorityGetter != null)
      {
        num = priorityGetter(t);
        if ((double) num < (double) bestPrio || Mathf.Approximately(num, bestPrio) && (double) distSquared >= (double) closestDistSquared)
          return;
      }
      chosen = t;
      closestDistSquared = distSquared;
      bestPrio = num;
    }
  }

  public static Thing ClosestThing_Global_Reachable(
    IntVec3 center,
    Map map,
    IEnumerable<Thing> searchSet,
    PathEndMode peMode,
    TraverseParms traverseParams,
    float maxDistance = 9999f,
    Predicate<Thing> validator = null,
    Func<Thing, float> priorityGetter = null,
    bool canLookInHaulableSources = false)
  {
    // ISSUE: variable of a compiler-generated type
    GenClosestCrossMap.\u003C\u003Ec__DisplayClass6_0 cDisplayClass60;
    // ISSUE: reference to a compiler-generated field
    cDisplayClass60.priorityGetter = priorityGetter;
    // ISSUE: reference to a compiler-generated field
    cDisplayClass60.canLookInHaulableSources = canLookInHaulableSources;
    // ISSUE: reference to a compiler-generated field
    cDisplayClass60.center = center;
    // ISSUE: reference to a compiler-generated field
    cDisplayClass60.peMode = peMode;
    // ISSUE: reference to a compiler-generated field
    cDisplayClass60.traverseParams = traverseParams;
    // ISSUE: reference to a compiler-generated field
    cDisplayClass60.validator = validator;
    if (searchSet == null)
      return (Thing) null;
    VehiclePawnWithMap vehicle;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    cDisplayClass60.basePos = map.IsVehicleMapOf(out vehicle) ? cDisplayClass60.center.ToBaseMapCoord(vehicle) : cDisplayClass60.center;
    // ISSUE: reference to a compiler-generated field
    cDisplayClass60.bestThing = (Thing) null;
    // ISSUE: reference to a compiler-generated field
    cDisplayClass60.bestPrio = float.MinValue;
    // ISSUE: reference to a compiler-generated field
    cDisplayClass60.maxDistanceSquared = maxDistance * maxDistance;
    // ISSUE: reference to a compiler-generated field
    cDisplayClass60.closestDistSquared = (float) int.MaxValue;
    ref GenClosestCrossMap.\u003C\u003Ec__DisplayClass6_0 local1 = ref cDisplayClass60;
    int num;
    // ISSUE: reference to a compiler-generated field
    if (cDisplayClass60.canLookInHaulableSources)
    {
      // ISSUE: reference to a compiler-generated field
      Pawn pawn = cDisplayClass60.traverseParams.pawn;
      num = pawn == null ? 0 : (pawn.IsColonist ? 1 : 0);
    }
    else
      num = 0;
    // ISSUE: reference to a compiler-generated field
    local1.careAboutHaulSourceEnabled = num != 0;
    ref GenClosestCrossMap.\u003C\u003Ec__DisplayClass6_0 local2 = ref cDisplayClass60;
    // ISSUE: reference to a compiler-generated field
    Pawn pawn1 = cDisplayClass60.traverseParams.pawn;
    Map map1 = (pawn1 != null ? CrossMapReachabilityUtility.get_DepartMap(pawn1) : (Map) null) ?? map;
    // ISSUE: reference to a compiler-generated field
    local2.departMap = map1;
    switch (searchSet)
    {
      case IList<Thing> thingList:
        for (int index = 0; index < thingList.Count; ++index)
          GenClosestCrossMap.\u003CClosestThing_Global_Reachable\u003Eg__Process\u007C6_0(thingList[index], ref cDisplayClass60);
        break;
      case IList<Pawn> pawnList:
        for (int index = 0; index < pawnList.Count; ++index)
          GenClosestCrossMap.\u003CClosestThing_Global_Reachable\u003Eg__Process\u007C6_0((Thing) pawnList[index], ref cDisplayClass60);
        break;
      case IList<Building> buildingList:
        for (int index = 0; index < buildingList.Count; ++index)
          GenClosestCrossMap.\u003CClosestThing_Global_Reachable\u003Eg__Process\u007C6_0((Thing) buildingList[index], ref cDisplayClass60);
        break;
      default:
        using (IEnumerator<Thing> enumerator = searchSet.GetEnumerator())
        {
          while (enumerator.MoveNext())
            GenClosestCrossMap.\u003CClosestThing_Global_Reachable\u003Eg__Process\u007C6_0(enumerator.Current, ref cDisplayClass60);
          break;
        }
    }
    // ISSUE: reference to a compiler-generated field
    return cDisplayClass60.bestThing;
  }
}
