// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CrossMapReachabilityUtility
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using JetBrains.Annotations;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public static class CrossMapReachabilityUtility
{
  public static bool working;
  public static Map DepartMapGlobal;
  [UsedImplicitly]
  public static Map DestMapGlobal;
  internal static readonly CrossMapReachabilityUtility.Traverser traverser = new CrossMapReachabilityUtility.Traverser();
  internal static readonly CrossMapReachabilityUtility.AStar<CrossMapReachabilityUtility.MapTraverse> aStar = new CrossMapReachabilityUtility.AStar<CrossMapReachabilityUtility.MapTraverse>(new Func<CrossMapReachabilityUtility.MapTraverse, CrossMapReachabilityUtility.MapTraverse, int>(CrossMapReachabilityUtility.Traverser.Cost), new Func<CrossMapReachabilityUtility.MapTraverse, IEnumerable<CrossMapReachabilityUtility.MapTraverse>>(CrossMapReachabilityUtility.traverser.Neighbors), new Func<CrossMapReachabilityUtility.MapTraverse, CrossMapReachabilityUtility.MapTraverse, bool>(CrossMapReachabilityUtility.traverser.FinalCheck), new Func<CrossMapReachabilityUtility.MapTraverse, CrossMapReachabilityUtility.MapTraverse, bool>(CrossMapReachabilityUtility.traverser.CanEnter), new Func<CrossMapReachabilityUtility.MapTraverse, int>(CrossMapReachabilityUtility.traverser.Heuristic), new Action<List<CrossMapReachabilityUtility.MapTraverse>, CrossMapReachabilityUtility.MapTraverse>(CrossMapReachabilityUtility.Traverser.ProcessPath), new Action<CrossMapReachabilityUtility.MapTraverse>(CrossMapReachabilityUtility.traverser.DebugDrawEnterNode));
  internal static readonly CrossMapReachabilityUtility.AStar<CrossMapReachabilityUtility.MapTraverse> aStar_new = new CrossMapReachabilityUtility.AStar<CrossMapReachabilityUtility.MapTraverse>(new Func<CrossMapReachabilityUtility.MapTraverse, CrossMapReachabilityUtility.MapTraverse, int>(CrossMapReachabilityUtility.Traverser.Cost), new Func<CrossMapReachabilityUtility.MapTraverse, IEnumerable<CrossMapReachabilityUtility.MapTraverse>>(CrossMapReachabilityUtility.traverser.Neighbors_New), new Func<CrossMapReachabilityUtility.MapTraverse, CrossMapReachabilityUtility.MapTraverse, bool>(CrossMapReachabilityUtility.traverser.FinalCheck), new Func<CrossMapReachabilityUtility.MapTraverse, CrossMapReachabilityUtility.MapTraverse, bool>(CrossMapReachabilityUtility.traverser.CanEnter), new Func<CrossMapReachabilityUtility.MapTraverse, int>(CrossMapReachabilityUtility.traverser.Heuristic), new Action<List<CrossMapReachabilityUtility.MapTraverse>, CrossMapReachabilityUtility.MapTraverse>(CrossMapReachabilityUtility.Traverser.ProcessPath), new Action<CrossMapReachabilityUtility.MapTraverse>(CrossMapReachabilityUtility.traverser.DebugDrawEnterNode));
  internal static readonly List<CrossMapReachabilityUtility.MapTraverse> traverseList = new List<CrossMapReachabilityUtility.MapTraverse>(16 /*0x10*/);
  private static readonly Stack<TraverseSpots> tmpTargets = new Stack<TraverseSpots>(16 /*0x10*/);
  private static readonly HashSet<Map> visitedMaps = new HashSet<Map>(16 /*0x10*/);
  private static readonly List<Map> candidateMaps = new List<Map>(16 /*0x10*/);
  private static readonly List<Region> destRegions = new List<Region>();

  private static ConditionalWeakTable<Pawn, Map> DestMaps { get; } = new ConditionalWeakTable<Pawn, Map>();

  private static ConditionalWeakTable<Pawn, Map> DepartMaps { get; } = new ConditionalWeakTable<Pawn, Map>();

  private static Dictionary<Pawn, IntVec3?> DepartPositions { get; } = new Dictionary<Pawn, IntVec3?>();

  [Conditional("DEBUG")]
  internal static void DebugLog(string message)
  {
  }

  public static Map get_DestMap(Pawn pawn)
  {
    if (pawn == null)
      return (Map) null;
    Map map;
    return !CrossMapReachabilityUtility.DestMaps.TryGetValue(pawn, out map) ? (Map) null : map;
  }

  public static void set_DestMap(Pawn pawn, Map value)
  {
    if (pawn == null)
      return;
    if (value == null)
      pawn.RemoveDestMap();
    else
      CrossMapReachabilityUtility.DestMaps.AddOrUpdate(pawn, value);
  }

  public static void RemoveDestMap(this Pawn pawn)
  {
    if (pawn == null)
      return;
    CrossMapReachabilityUtility.DestMaps.Remove(pawn);
  }

  public static Map get_DepartMap(Pawn pawn)
  {
    if (pawn == null)
      return (Map) null;
    Map map;
    return !CrossMapReachabilityUtility.DepartMaps.TryGetValue(pawn, out map) ? (Map) null : map;
  }

  public static void set_DepartMap(Pawn pawn, Map value)
  {
    if (pawn == null)
      return;
    if (value == null)
      pawn.RemoveDepartMap();
    else
      CrossMapReachabilityUtility.DepartMaps.AddOrUpdate(pawn, value);
  }

  public static void RemoveDepartMap(this Pawn pawn)
  {
    if (pawn == null)
      return;
    CrossMapReachabilityUtility.DepartMaps.Remove(pawn);
  }

  public static Map get_DepartMapOrPawnMap(Pawn pawn)
  {
    return CrossMapReachabilityUtility.get_DepartMap(pawn) ?? ((Thing) pawn).Map;
  }

  public static Map get_DepartMapOrPawnMapHeld(Pawn pawn)
  {
    return CrossMapReachabilityUtility.get_DepartMap(pawn) ?? ((Thing) pawn).MapHeld;
  }

  internal static IntVec3? get_DepartPosition(Pawn pawn)
  {
    return pawn != null ? CollectionExtensions.GetValueOrDefault<Pawn, IntVec3?>((IReadOnlyDictionary<Pawn, IntVec3?>) CrossMapReachabilityUtility.DepartPositions, pawn) : new IntVec3?();
  }

  internal static void set_DepartPosition(Pawn pawn, IntVec3? value)
  {
    if (pawn == null)
      return;
    if (!value.HasValue)
      CrossMapReachabilityUtility.DepartPositions.Remove(pawn);
    else
      CrossMapReachabilityUtility.DepartPositions[pawn] = value;
  }

  public static bool CanReach(
    this Pawn pawn,
    LocalTargetInfo dest3,
    PathEndMode peMode,
    Danger maxDanger,
    bool canBashDoors,
    bool canBashFences,
    TraverseMode mode,
    Map destMap)
  {
    TraverseParms traverseParms = TraverseParms.For(pawn, maxDanger, mode, canBashDoors, false, canBashFences, true);
    return ((Thing) pawn).Spawned && CrossMapReachabilityUtility.CanReach(CrossMapReachabilityUtility.get_DepartMap(pawn) ?? ((Thing) pawn).Map, CrossMapReachabilityUtility.get_DepartPosition(pawn) ?? ((Thing) pawn).Position, dest3, peMode, traverseParms, destMap, out TargetInfo _, out TargetInfo _, out List<TraverseSpots> _);
  }

  public static bool CanReach(
    this Pawn pawn,
    LocalTargetInfo dest3,
    PathEndMode peMode,
    Danger maxDanger,
    bool canBashDoors,
    bool canBashFences,
    TraverseMode mode,
    Map destMap,
    out TargetInfo exitSpot,
    out TargetInfo enterSpot,
    out List<TraverseSpots> spotsQueue)
  {
    TraverseParms traverseParms = TraverseParms.For(pawn, maxDanger, mode, canBashDoors, false, canBashFences, true);
    exitSpot = TargetInfo.Invalid;
    enterSpot = TargetInfo.Invalid;
    spotsQueue = (List<TraverseSpots>) null;
    return ((Thing) pawn).Spawned && CrossMapReachabilityUtility.CanReach(CrossMapReachabilityUtility.get_DepartMap(pawn) ?? ((Thing) pawn).Map, CrossMapReachabilityUtility.get_DepartPosition(pawn) ?? ((Thing) pawn).Position, dest3, peMode, traverseParms, destMap, out exitSpot, out enterSpot, out spotsQueue);
  }

  public static IntVec3 EnterVehiclePosition(TargetInfo enterSpot, VehiclePawn enterer = null)
  {
    VehiclePawnWithMap vehicle;
    if (!((TargetInfo) ref enterSpot).Map.IsVehicleMapOf(out vehicle) || vehicle == null || !((Thing) vehicle).Spawned)
      return IntVec3.Invalid;
    IntVec3 baseMapCoord = ((TargetInfo) ref enterSpot).Cell.ToBaseMapCoord(vehicle);
    IntVec3 facingCell;
    if (!((TargetInfo) ref enterSpot).HasThing)
    {
      Rot8 insideMap = ((TargetInfo) ref enterSpot).Cell.BaseFullDirectionToInsideMap(vehicle);
      facingCell = ((Rot8) ref insideMap).FacingCell;
    }
    else
    {
      Rot8 rot8 = ((TargetInfo) ref enterSpot).Thing.BaseFullRotation();
      facingCell = ((Rot8) ref rot8).FacingCell;
    }
    IntVec3 intVec3_1 = facingCell;
    int num = 0;
    CellRect cellRect = Ext_Vehicles.VehicleRect((VehiclePawn) vehicle, false);
    IntVec3 intVec3_2;
    do
    {
      ++num;
      intVec3_2 = IntVec3.op_Subtraction(baseMapCoord, IntVec3.op_Multiply(intVec3_1, num));
      if (!GenGrid.InBounds(intVec3_2, ((Thing) vehicle).Map))
        return IntVec3.Invalid;
    }
    while (((CellRect) ref cellRect).Contains(intVec3_2));
    if (((TargetInfo) ref enterSpot).Thing is Building_VehicleRamp && num < 2)
      ++num;
    if (enterer != null)
      num += enterer.HalfLength();
    return IntVec3.op_Subtraction(baseMapCoord, IntVec3.op_Multiply(intVec3_1, num));
  }

  public static bool CanReach(
    Map departMap,
    IntVec3 root,
    LocalTargetInfo dest,
    PathEndMode peMode,
    TraverseParms traverseParms,
    Map destMap,
    bool canUseAbility = true)
  {
    return CrossMapReachabilityUtility.CanReach(departMap, root, dest, peMode, traverseParms, destMap, out TargetInfo _, out TargetInfo _, out List<TraverseSpots> _, canUseAbility);
  }

  public static bool CanReach(
    Map departMap,
    IntVec3 root,
    LocalTargetInfo dest,
    PathEndMode peMode,
    TraverseParms traverseParms,
    Map destMap,
    out TargetInfo exitSpot,
    out TargetInfo enterSpot,
    out List<TraverseSpots> spotsQueue,
    bool canUseAbility = true)
  {
    exitSpot = TargetInfo.Invalid;
    enterSpot = TargetInfo.Invalid;
    spotsQueue = (List<TraverseSpots>) null;
    if (departMap == null || destMap == null)
      return false;
    if (departMap == destMap)
    {
      try
      {
        CrossMapReachabilityUtility.working = true;
        return destMap.reachability.CanReach(root, dest, peMode, traverseParms);
      }
      finally
      {
        CrossMapReachabilityUtility.working = false;
      }
    }
    else
    {
      if (traverseParms.pawn is VehiclePawn pawn1)
        return pawn1.CanReachVehicle(dest, peMode, traverseParms.maxDanger, traverseParms.mode, destMap, out exitSpot, out enterSpot);
      if (CrossMapReachabilityUtility.working)
      {
        Log.ErrorOnce("Called CanReach() while working. This should never happen. Suppressing further errors.", 7312233);
        return false;
      }
      Region region1 = GridsUtility.GetRegion(root, departMap, (RegionType) 14);
      TraverseParmsExtended traverseParms1 = (TraverseParmsExtended) traverseParms;
      Ability_MapTraverse ability = (Ability_MapTraverse) null;
      if (canUseAbility)
      {
        Pawn pawn2 = traverseParms.pawn;
        Ability ability1;
        if (pawn2 == null)
        {
          ability1 = (Ability) null;
        }
        else
        {
          Pawn_AbilityTracker abilities = pawn2.abilities;
          ability1 = abilities != null ? GenCollection.FirstOrDefault<Ability>(abilities.AllAbilitiesForReading, (Predicate<Ability>) (a =>
          {
            if (!(a is Ability_MapTraverse))
              return false;
            AcceptanceReport canCast = a.CanCast;
            return ((AcceptanceReport) ref canCast).Accepted;
          })) : (Ability) null;
        }
        ability = (Ability_MapTraverse) ability1;
        traverseParms1.ability = ability?.def;
      }
      dest = TargetInfo.op_Explicit(GenPath.ResolvePathMode(traverseParms.pawn, ((LocalTargetInfo) ref dest).ToTargetInfo(destMap), ref peMode));
      CrossMapReachabilityUtility.destRegions.Clear();
      switch ((int) peMode)
      {
        case 1:
          Region region2 = GridsUtility.GetRegion(((LocalTargetInfo) ref dest).Cell, destMap, (RegionType) 14);
          if (region2 != null && region2.Allows(traverseParms, true))
          {
            CrossMapReachabilityUtility.destRegions.Add(region2);
            break;
          }
          break;
        case 2:
          TouchPathEndModeUtility.AddAllowedAdjacentRegions(dest, traverseParms, destMap, CrossMapReachabilityUtility.destRegions);
          break;
      }
      GenList.RemoveDuplicates<Region>(CrossMapReachabilityUtility.destRegions, (Func<Region, Region, bool>) null);
      if (CrossMapReachabilityUtility.destRegions.Count == 0 && traverseParms.mode != 3 && traverseParms.mode != 4 && traverseParms.mode != 6)
        return false;
      bool result = false;
      foreach (Region destRegion in CrossMapReachabilityUtility.destRegions)
      {
        if (CrossMapReachabilityCache.TryGetCache(region1, destRegion, traverseParms1, out result, out exitSpot, out enterSpot, out spotsQueue))
          return result;
      }
      try
      {
        CrossMapReachabilityUtility.working = true;
        if (ModCompat.CompatBase<ModCompat.MultiFloors>.Active && ModCompat.MultiFloors.GetLevel(departMap) != ModCompat.MultiFloors.GetLevel(destMap))
          return false;
        VehiclePawnWithMap vehicle;
        Map map = !destMap.IsVehicleMapOf(out vehicle) || !((Thing) vehicle).Spawned ? destMap : ((Thing) vehicle).Map;
        VehiclePawnWithMap vehicle2;
        Map departBaseMap = !departMap.IsVehicleMapOf(out vehicle2) || !((Thing) vehicle2).Spawned ? departMap : ((Thing) vehicle2).Map;
        if (VehicleMapUtility.get_BaseMapOrCaravan(departMap) == VehicleMapUtility.get_BaseMapOrCaravan(destMap))
        {
          if (!VehicleMapFramework.VehicleMapFramework.settings.legacyCanReach)
          {
            if (!GenGrid.InBounds(root, departMap))
            {
              VMF_Log.Error($"Root {root} is out of bounds of departMap {departMap}. This should never happen.");
              return false;
            }
            CrossMapReachabilityUtility.MapTraverse start = new CrossMapReachabilityUtility.MapTraverse(TargetInfo.Invalid, new TargetInfo(root, departMap, false));
            CrossMapReachabilityUtility.MapTraverse destination = new CrossMapReachabilityUtility.MapTraverse(TargetInfo.Invalid, ((LocalTargetInfo) ref dest).ToTargetInfo(destMap));
            CrossMapReachabilityUtility.traverser.SetParameters(start.enterSpot, destination.enterSpot, traverseParms, ability);
            CrossMapReachabilityUtility.traverseList.Clear();
            CrossMapReachabilityUtility.aStar_new.Run(start, destination, CrossMapReachabilityUtility.traverseList);
            result = CrossMapReachabilityUtility.traverseList.Count > 0;
            if (CrossMapReachabilityUtility.traverseList.Count == 1)
            {
              exitSpot = CrossMapReachabilityUtility.traverseList[0].exitSpot;
              enterSpot = CrossMapReachabilityUtility.traverseList[0].enterSpot;
            }
            else if (result)
            {
              spotsQueue = SimplePool<List<TraverseSpots>>.Get();
              spotsQueue.Clear();
              foreach (CrossMapReachabilityUtility.MapTraverse traverse in CrossMapReachabilityUtility.traverseList)
                spotsQueue.Add(new TraverseSpots(traverse.exitSpot, traverse.enterSpot));
            }
            CrossMapReachabilityUtility.traverseList.Clear();
            return result;
          }
          using (new DeepProfilerScope("CrossMapReachability Legacy Run", CrossMapReachabilityUtility.aStar.debug))
          {
            int num = departMap == departBaseMap ? 1 : 0;
            bool flag = map == destMap;
            TraverseParms traverseParms2 = traverseParms.pawn != null ? TraverseParms.For(traverseParms.pawn, traverseParms.maxDanger, (TraverseMode) 1, traverseParms.canBashDoors, traverseParms.alwaysUseAvoidGrid, traverseParms.canBashFences, traverseParms.avoidPersistentDanger) : TraverseParms.For((TraverseMode) 1, traverseParms.maxDanger, traverseParms.canBashDoors, traverseParms.alwaysUseAvoidGrid, traverseParms.canBashFences, traverseParms.avoidPersistentDanger, false);
            CrossMapReachabilityUtility.traverser.destRegion = GridsUtility.GetRegion(((LocalTargetInfo) ref dest).Cell, destMap, (RegionType) 14);
            if (num == 0)
            {
              if (flag)
              {
                if (vehicle2 != null)
                {
                  if (!vehicle2.AllowExitFor(traverseParms.pawn))
                    return false;
                  foreach (CompVehicleEnterSpot sortedEnterComp in vehicle2.GetSortedEnterComps(((LocalTargetInfo) ref dest).Cell, CompVehicleEnterSpot.Kind.GroundAccessOnly))
                  {
                    if (sortedEnterComp != null)
                    {
                      TargetInfo availableAccessSpot = sortedEnterComp.AvailableAccessSpot;
                      if (((TargetInfo) ref availableAccessSpot).IsValid && ((TargetInfo) ref availableAccessSpot).Map == destMap)
                      {
                        IntVec3 cell = ((TargetInfo) ref availableAccessSpot).Cell;
                        result = CrossMapReachabilityUtility.CellCheck(cell, destMap, traverseParms, true) && CanReachLocal(((Thing) sortedEnterComp.parent).Position, cell);
                        if (result)
                        {
                          exitSpot = TargetInfo.op_Implicit((Thing) sortedEnterComp.parent);
                          return result;
                        }
                      }
                    }
                  }
                  foreach (IntVec3 cell in (IEnumerable<IntVec3>) vehicle2.CachedWalkableMapEdgeCells.Keys.OrderBy<IntVec3, int>((Func<IntVec3, int>) (c =>
                  {
                    IntVec3 intVec3 = IntVec3.op_Subtraction(c.ToBaseMapCoord(vehicle2), ((LocalTargetInfo) ref dest).Cell);
                    return ((IntVec3) ref intVec3).LengthHorizontalSquared;
                  })))
                  {
                    TargetInfo enterSpot1;
                    // ISSUE: explicit constructor call
                    ((TargetInfo) ref enterSpot1).\u002Ector(cell, departMap, false);
                    IntVec3 intVec3 = CrossMapReachabilityUtility.EnterVehiclePosition(enterSpot1);
                    result = CrossMapReachabilityUtility.CellCheck(intVec3, destMap, traverseParms, true) && CanReachLocal(cell, intVec3);
                    if (result)
                    {
                      exitSpot = enterSpot1;
                      return result;
                    }
                  }
                  result = ability != null && ability.TryFindCastPosition(((LocalTargetInfo) ref dest).ToTargetInfo(destMap), out exitSpot, out enterSpot);
                  return result;
                }
                goto label_89;
              }
            }
            else if (!flag)
            {
              if (vehicle != null)
              {
                if (!vehicle.AllowEnterFor(traverseParms.pawn))
                  return false;
                foreach (CompVehicleEnterSpot sortedEnterComp in vehicle.GetSortedEnterComps(root))
                {
                  if (sortedEnterComp != null)
                  {
                    TargetInfo availableAccessSpot = sortedEnterComp.AvailableAccessSpot;
                    if (((TargetInfo) ref availableAccessSpot).IsValid && ((TargetInfo) ref availableAccessSpot).Map == departMap)
                    {
                      IntVec3 cell = ((TargetInfo) ref availableAccessSpot).Cell;
                      result = CrossMapReachabilityUtility.CellCheck(cell, departMap, traverseParms) && CanReachLocal(cell, ((Thing) sortedEnterComp.parent).Position);
                      if (result)
                      {
                        enterSpot = TargetInfo.op_Implicit((Thing) sortedEnterComp.parent);
                        return result;
                      }
                    }
                  }
                }
                foreach (IntVec3 cell2 in (IEnumerable<IntVec3>) vehicle.CachedWalkableMapEdgeCells.Keys.OrderBy<IntVec3, int>((Func<IntVec3, int>) (c =>
                {
                  IntVec3 intVec3 = IntVec3.op_Subtraction(root, c.ToBaseMapCoord(vehicle));
                  return ((IntVec3) ref intVec3).LengthHorizontalSquared;
                })))
                {
                  TargetInfo enterSpot2;
                  // ISSUE: explicit constructor call
                  ((TargetInfo) ref enterSpot2).\u002Ector(cell2, destMap, false);
                  IntVec3 cell = CrossMapReachabilityUtility.EnterVehiclePosition(enterSpot2);
                  result = CrossMapReachabilityUtility.CellCheck(cell, departMap, traverseParms) && CanReachLocal(cell, cell2);
                  if (result)
                  {
                    enterSpot = enterSpot2;
                    return result;
                  }
                }
                result = ability != null && ability.TryFindCastPosition(((LocalTargetInfo) ref dest).ToTargetInfo(destMap), out exitSpot, out enterSpot);
                return result;
              }
              goto label_89;
            }
            if (vehicle2 == null || !vehicle2.AllowExitFor(traverseParms.pawn) || vehicle == null || !vehicle.AllowEnterFor(traverseParms.pawn))
              return false;
            result = CanReachBasic(out exitSpot, out enterSpot) || CanReachRecursive(out spotsQueue);
            return result;

            bool CanReachBasic(out TargetInfo exitSpot, out TargetInfo enterSpot)
            {
              exitSpot = TargetInfo.Invalid;
              enterSpot = TargetInfo.Invalid;
              IntVec3 destBaseMapCoord = ((LocalTargetInfo) ref dest).Cell.ToBaseMapCoord(vehicle);
              foreach (CompVehicleEnterSpot sortedEnterComp1 in vehicle2.GetSortedEnterComps(destBaseMapCoord))
              {
                if (sortedEnterComp1 != null)
                {
                  TargetInfo availableAccessSpot1 = sortedEnterComp1.AvailableAccessSpot;
                  if (((TargetInfo) ref availableAccessSpot1).IsValid && (((TargetInfo) ref availableAccessSpot1).Map == destMap || ((TargetInfo) ref availableAccessSpot1).Map == departBaseMap))
                  {
                    IntVec3 cell = ((TargetInfo) ref availableAccessSpot1).Cell;
                    if (((TargetInfo) ref availableAccessSpot1).Map == destMap)
                    {
                      IntVec3 position = ((Thing) sortedEnterComp1.parent).Position;
                      if (CrossMapReachabilityUtility.CellCheck(cell, destMap, traverseParms, true) && CanReachLocal(position, cell))
                      {
                        exitSpot = TargetInfo.op_Implicit((Thing) sortedEnterComp1.parent);
                        return true;
                      }
                    }
                    foreach (CompVehicleEnterSpot sortedEnterComp2 in vehicle.GetSortedEnterComps(cell))
                    {
                      if (sortedEnterComp2 != null)
                      {
                        TargetInfo availableAccessSpot2 = sortedEnterComp2.AvailableAccessSpot;
                        if (((TargetInfo) ref availableAccessSpot2).IsValid && ((TargetInfo) ref availableAccessSpot2).Map == departBaseMap)
                        {
                          IntVec3 cell1 = ((TargetInfo) ref availableAccessSpot2).Cell;
                          if (CanReach2(((Thing) sortedEnterComp1.parent).Position, cell, cell1, ((Thing) sortedEnterComp2.parent).Position))
                          {
                            exitSpot = TargetInfo.op_Implicit((Thing) sortedEnterComp1.parent);
                            enterSpot = TargetInfo.op_Implicit((Thing) sortedEnterComp2.parent);
                            return true;
                          }
                        }
                      }
                    }
                    foreach (IntVec3 cell4 in (IEnumerable<IntVec3>) vehicle.CachedWalkableMapEdgeCells.Keys.OrderBy<IntVec3, int>((Func<IntVec3, int>) (c2 =>
                    {
                      IntVec3 intVec3 = IntVec3.op_Subtraction(cell, c2.ToBaseMapCoord(vehicle));
                      return ((IntVec3) ref intVec3).LengthHorizontalSquared;
                    })))
                    {
                      TargetInfo enterSpot1;
                      // ISSUE: explicit constructor call
                      ((TargetInfo) ref enterSpot1).\u002Ector(cell4, destMap, false);
                      IntVec3 cell3 = CrossMapReachabilityUtility.EnterVehiclePosition(enterSpot1);
                      if (CanReach2(((Thing) sortedEnterComp1.parent).Position, cell, cell3, cell4))
                      {
                        exitSpot = TargetInfo.op_Implicit((Thing) sortedEnterComp1.parent);
                        enterSpot = enterSpot1;
                        return true;
                      }
                    }
                  }
                }
              }
              foreach (IntVec3 cell2 in (IEnumerable<IntVec3>) vehicle2.CachedWalkableMapEdgeCells.Keys.OrderBy<IntVec3, int>((Func<IntVec3, int>) (c =>
              {
                IntVec3 intVec3 = IntVec3.op_Subtraction(c.ToBaseMapCoord(vehicle2), destBaseMapCoord);
                return ((IntVec3) ref intVec3).LengthHorizontalSquared;
              })))
              {
                TargetInfo enterSpot2;
                // ISSUE: explicit constructor call
                ((TargetInfo) ref enterSpot2).\u002Ector(cell2, departMap, false);
                IntVec3 cell = CrossMapReachabilityUtility.EnterVehiclePosition(enterSpot2);
                foreach (CompVehicleEnterSpot sortedEnterComp in vehicle.GetSortedEnterComps(cell))
                {
                  if (sortedEnterComp != null)
                  {
                    TargetInfo availableAccessSpot = sortedEnterComp.AvailableAccessSpot;
                    if (((TargetInfo) ref availableAccessSpot).IsValid && ((TargetInfo) ref availableAccessSpot).Map == departBaseMap)
                    {
                      IntVec3 cell3 = ((TargetInfo) ref availableAccessSpot).Cell;
                      if (CanReach2(cell2, cell, cell3, ((Thing) sortedEnterComp.parent).Position))
                      {
                        exitSpot = enterSpot2;
                        enterSpot = TargetInfo.op_Implicit((Thing) sortedEnterComp.parent);
                        return true;
                      }
                    }
                  }
                }
                foreach (IntVec3 cell4 in (IEnumerable<IntVec3>) vehicle.CachedWalkableMapEdgeCells.Keys.OrderBy<IntVec3, int>((Func<IntVec3, int>) (c2 =>
                {
                  IntVec3 intVec3 = IntVec3.op_Subtraction(cell, c2.ToBaseMapCoord(vehicle));
                  return ((IntVec3) ref intVec3).LengthHorizontalSquared;
                })))
                {
                  TargetInfo enterSpot3;
                  // ISSUE: explicit constructor call
                  ((TargetInfo) ref enterSpot3).\u002Ector(cell4, destMap, false);
                  IntVec3 cell3 = CrossMapReachabilityUtility.EnterVehiclePosition(enterSpot3);
                  if (CanReach2(cell2, cell, cell3, cell4))
                  {
                    exitSpot = enterSpot2;
                    enterSpot = enterSpot3;
                    return true;
                  }
                }
              }
              return ability != null && ability.TryFindCastPosition(((LocalTargetInfo) ref dest).ToTargetInfo(destMap), out exitSpot, out enterSpot);
            }

            bool CanReach2(IntVec3 cell, IntVec3 cell2, IntVec3 cell3, IntVec3 cell4)
            {
              return CrossMapReachabilityUtility.CellCheck(cell2, departBaseMap, traverseParms, true) && CrossMapReachabilityUtility.CellCheck(cell3, departBaseMap, traverseParms) && departMap.reachability.CanReach(root, LocalTargetInfo.op_Implicit(cell), (PathEndMode) 1, traverseParms) && departBaseMap.reachability.CanReach(cell2, LocalTargetInfo.op_Implicit(cell3), (PathEndMode) 1, traverseParms2) && destMap.reachability.CanReach(cell4, dest, peMode, traverseParms2);
            }

            bool CanReachRecursive(out List<TraverseSpots> spotsQueue)
            {
              spotsQueue = (List<TraverseSpots>) null;
              IntVec3 destBaseMapCoord = ((LocalTargetInfo) ref dest).Cell.ToBaseMapCoord(vehicle);
              CrossMapReachabilityUtility.candidateMaps.AddRange((IEnumerable<Map>) departMap.BaseMapAndVehicleMaps(false));
              result = EnterMap(vehicle2.VehicleMap, root);
              if (result)
              {
                spotsQueue = SimplePool<List<TraverseSpots>>.Get();
                spotsQueue.Clear();
                foreach (TraverseSpots tmpTarget in CrossMapReachabilityUtility.tmpTargets)
                  spotsQueue.Add(tmpTarget);
              }
              CrossMapReachabilityUtility.tmpTargets.Clear();
              CrossMapReachabilityUtility.visitedMaps.Clear();
              CrossMapReachabilityUtility.candidateMaps.Clear();
              return result;
              TraverseParms traverseParms;
              Map destMap;
              LocalTargetInfo dest;
              Ability_MapTraverse ability;

              bool EnterMap(Map map, IntVec3 start)
              {
                if (map == destMap && destMap.reachability.CanReach(start, dest, (PathEndMode) 1, traverseParms2))
                  return true;
                CrossMapReachabilityUtility.visitedMaps.Add(map);
                VehiclePawnWithMap vehicle;
                foreach (CompVehicleEnterSpot vehicleEnterSpot in map.IsVehicleMapOf(out vehicle) ? vehicle.GetSortedEnterComps(destBaseMapCoord, CompVehicleEnterSpot.Kind.DirectAccessOnly).AsEnumerable<CompVehicleEnterSpot>() : RegionTraverserAcrossMaps.EnterSpotDefs.SelectMany<ThingDef, CompVehicleEnterSpot>((Func<ThingDef, IEnumerable<CompVehicleEnterSpot>>) (def => map.listerThings.ThingsOfDef(def).Select<Thing, CompVehicleEnterSpot>((Func<Thing, CompVehicleEnterSpot>) (t => ThingCompUtility.TryGetComp<CompVehicleEnterSpot>(t))))))
                {
                  if (vehicleEnterSpot != null)
                  {
                    TargetInfo availableAccessSpot = vehicleEnterSpot.AvailableAccessSpot;
                    if (((TargetInfo) ref availableAccessSpot).IsValid && !CrossMapReachabilityUtility.visitedMaps.Contains(((TargetInfo) ref availableAccessSpot).Map))
                    {
                      IntVec3 position = ((Thing) vehicleEnterSpot.parent).Position;
                      IntVec3 cell = ((TargetInfo) ref availableAccessSpot).Cell;
                      Map map1 = ((TargetInfo) ref availableAccessSpot).Map;
                      if (CrossMapReachabilityUtility.CellCheck(position, map, traverseParms) && CrossMapReachabilityUtility.CellCheck(cell, map1, traverseParms, true) && map.reachability.CanReach(start, LocalTargetInfo.op_Implicit(position), (PathEndMode) 1, traverseParms2))
                      {
                        CrossMapReachabilityUtility.tmpTargets.Push(new TraverseSpots(TargetInfo.op_Implicit((Thing) vehicleEnterSpot.parent), TargetInfo.Invalid));
                        if (EnterMap(map1, cell))
                          return true;
                        CrossMapReachabilityUtility.tmpTargets.Pop();
                        return false;
                      }
                    }
                  }
                }
                if (ability != null)
                {
                  foreach (Map candidateMap in CrossMapReachabilityUtility.candidateMaps)
                  {
                    TargetInfo castSpot;
                    TargetInfo targSpot;
                    if (!CrossMapReachabilityUtility.visitedMaps.Contains(candidateMap) && ability.TryFindCastPositionFromTo(new TargetInfo(start, map, false), new TargetInfo(candidateMap.Center, candidateMap, false), out castSpot, out targSpot))
                    {
                      CrossMapReachabilityUtility.tmpTargets.Push(new TraverseSpots(castSpot, targSpot));
                      if (EnterMap(candidateMap, ((TargetInfo) ref targSpot).Cell))
                        return true;
                      CrossMapReachabilityUtility.tmpTargets.Pop();
                      return false;
                    }
                  }
                }
                return false;
              }
            }
          }
        }
label_89:
        result = false;
        return result;
      }
      finally
      {
        if (result)
        {
          CrossMapReachabilityCache.Cache(region1, CrossMapReachabilityUtility.traverser.destRegion, traverseParms1, true, exitSpot, enterSpot, spotsQueue);
        }
        else
        {
          foreach (Region destRegion in CrossMapReachabilityUtility.destRegions)
            CrossMapReachabilityCache.Cache(region1, destRegion, traverseParms1, false, TargetInfo.Invalid, TargetInfo.Invalid, (List<TraverseSpots>) null);
        }
        CrossMapReachabilityUtility.working = false;
      }
    }

    bool CanReachLocal(IntVec3 cell, IntVec3 cell2)
    {
      return departMap.reachability.CanReach(root, LocalTargetInfo.op_Implicit(cell), (PathEndMode) 1, traverseParms) && destMap.reachability.CanReach(cell2, dest, peMode, traverseParms);
    }
  }

  private static bool CellCheck(
    IntVec3 cell,
    Map map,
    TraverseParms parms,
    bool destination = false,
    bool destroyMode = false)
  {
    Pawn pawn = parms.pawn;
    if (pawn != null)
    {
      if ((destroyMode || GenGrid.WalkableBy(cell, map, pawn)) && (destination || !cell.IsForbidden(pawn, map)))
      {
        Building_Door door = GridsUtility.GetDoor(cell, map);
        if (door == null || door.HoldOpen || door.PawnCanOpen(pawn) && !ForbidUtility.IsForbidden((Thing) door, pawn))
        {
          if (!destination || !parms.avoidPersistentDanger)
            return true;
          TerrainDef terrain = GridsUtility.GetTerrain(cell, map);
          if (terrain != null && !terrain.dangerous)
            return true;
          List<TerrainDef> terrainDefList;
          return CompAllowDangerTerrains.AllowedTerrains.TryGetValue(pawn, out terrainDefList) && terrainDefList.Contains(terrain);
        }
      }
      return false;
    }
    if (GenGrid.Walkable(cell, map))
    {
      Building_Door door = GridsUtility.GetDoor(cell, map);
      if (door == null || door.HoldOpen)
      {
        if (!destination || !parms.avoidPersistentDanger)
          return true;
        TerrainDef terrain = GridsUtility.GetTerrain(cell, map);
        return terrain != null && !terrain.dangerous;
      }
    }
    return false;
  }

  public static bool CanReachToMap(
    IntVec3 root,
    Map departMap,
    TraverseParms parms,
    Map destMap,
    bool canUseAbility = true)
  {
    return CrossMapReachabilityUtility.CanReachToMap(root, departMap, parms, destMap, out TargetInfo _, out TargetInfo _, out List<TraverseSpots> _, canUseAbility);
  }

  public static bool CanReachToMap(
    IntVec3 root,
    Map departMap,
    TraverseParms parms,
    Map destMap,
    out TargetInfo exitSpot,
    out TargetInfo enterSpot,
    out List<TraverseSpots> spotsQueue,
    bool canUseAbility = true)
  {
    exitSpot = TargetInfo.Invalid;
    enterSpot = TargetInfo.Invalid;
    spotsQueue = (List<TraverseSpots>) null;
    if (departMap == null || destMap == null)
      return false;
    if (departMap == destMap)
      return true;
    if (VehicleMapUtility.get_BaseMapOrCaravan(departMap) != VehicleMapUtility.get_BaseMapOrCaravan(destMap))
      return false;
    if (CrossMapReachabilityUtility.working)
    {
      Log.ErrorOnce("Called CanReachToMap() while working. This should never happen. Suppressing further errors.", 7312234);
      return false;
    }
    CrossMapReachabilityUtility.working = true;
    try
    {
      Region region = GridsUtility.GetRegion(root, departMap, (RegionType) 14);
      TraverseParmsExtended traverseParms = (TraverseParmsExtended) parms;
      Ability_MapTraverse ability1 = (Ability_MapTraverse) null;
      if (canUseAbility)
      {
        Pawn pawn = parms.pawn;
        Ability ability2;
        if (pawn == null)
        {
          ability2 = (Ability) null;
        }
        else
        {
          Pawn_AbilityTracker abilities = pawn.abilities;
          ability2 = abilities != null ? GenCollection.FirstOrDefault<Ability>(abilities.AllAbilitiesForReading, (Predicate<Ability>) (a =>
          {
            if (!(a is Ability_MapTraverse))
              return false;
            AcceptanceReport canCast = a.CanCast;
            return ((AcceptanceReport) ref canCast).Accepted;
          })) : (Ability) null;
        }
        ability1 = (Ability_MapTraverse) ability2;
        traverseParms.ability = ability1?.def;
      }
      CrossMapReachabilityUtility.destRegions.Clear();
      foreach (District allDistrict in destMap.regionGrid.allDistricts)
      {
        if (allDistrict.Passable)
          CrossMapReachabilityUtility.destRegions.AddRange((IEnumerable<Region>) allDistrict.Regions);
      }
      if (GenCollection.Empty<Region>(CrossMapReachabilityUtility.destRegions))
        return false;
      foreach (Region destRegion in CrossMapReachabilityUtility.destRegions)
      {
        bool result;
        if (CrossMapReachabilityCache.TryGetCache(region, destRegion, traverseParms, out result, out exitSpot, out enterSpot, out spotsQueue) & result)
          return true;
      }
      CrossMapReachabilityUtility.MapTraverse start = new CrossMapReachabilityUtility.MapTraverse(TargetInfo.Invalid, new TargetInfo(root, departMap, false));
      CrossMapReachabilityUtility.MapTraverse destination = new CrossMapReachabilityUtility.MapTraverse(TargetInfo.Invalid, new TargetInfo(CrossMapReachabilityUtility.destRegions[0].AnyCell, destMap, false));
      CrossMapReachabilityUtility.traverser.SetParameters(start.enterSpot, destination.enterSpot, parms, ability1);
      CrossMapReachabilityUtility.traverseList.Clear();
      CrossMapReachabilityUtility.aStar_new.Run(start, destination, CrossMapReachabilityUtility.traverseList);
      bool map = CrossMapReachabilityUtility.traverseList.Count > 0;
      if (CrossMapReachabilityUtility.traverseList.Count == 1)
      {
        exitSpot = CrossMapReachabilityUtility.traverseList[0].exitSpot;
        enterSpot = CrossMapReachabilityUtility.traverseList[0].enterSpot;
      }
      else if (map)
      {
        spotsQueue = SimplePool<List<TraverseSpots>>.Get();
        spotsQueue.Clear();
        foreach (CrossMapReachabilityUtility.MapTraverse traverse in CrossMapReachabilityUtility.traverseList)
          spotsQueue.Add(new TraverseSpots(traverse.exitSpot, traverse.enterSpot));
      }
      if (map)
        CrossMapReachabilityCache.Cache(region, CrossMapReachabilityUtility.traverser.destRegion, traverseParms, true, exitSpot, enterSpot, spotsQueue);
      CrossMapReachabilityUtility.traverseList.Clear();
      return map;
    }
    finally
    {
      CrossMapReachabilityUtility.working = false;
    }
  }

  public static bool TryFindNearestStandableCell(
    VehiclePawn vehicle,
    IntVec3 cell,
    Map map,
    out IntVec3 result,
    float radius = -1f)
  {
    if ((double) radius < 0.0)
      radius = (float) (Mathf.Min(((BuildableDef) vehicle.VehicleDef).Size.x, ((BuildableDef) vehicle.VehicleDef).Size.z) * 2);
    int num = GenRadial.NumCellsInRadius(radius);
    result = IntVec3.Invalid;
    for (int index = 0; index < num; ++index)
    {
      IntVec3 cell1 = IntVec3.op_Addition(GenRadial.RadialPattern[index], cell);
      if (GenGrid.InBounds(cell1, map) && GenGridVehicles.Standable(cell1, vehicle, map) && vehicle.DrivableRectOnCell(cell1, true, map) && (map == ((Thing) vehicle).Map && IntVec3.op_Equality(cell1, ((Thing) vehicle).Position) || vehicle.beached || CrossMapReachabilityUtility.AnyVehicleBlockingPathAt(cell1, vehicle, map) == null && vehicle.CanReachVehicle(LocalTargetInfo.op_Implicit(cell1), (PathEndMode) 1, (Danger) 3, (TraverseMode) 0, map, out TargetInfo _, out TargetInfo _)))
      {
        result = cell1;
        return true;
      }
    }
    return false;
  }

  public static VehiclePawn AnyVehicleBlockingPathAt(IntVec3 cell, VehiclePawn vehicle, Map map)
  {
    List<Thing> thingList = GridsUtility.GetThingList(cell, map);
    if (GenList.NullOrEmpty<Thing>((IList<Thing>) thingList))
      return (VehiclePawn) null;
    float num = Ext_Map.Distance(VehicleMapUtility.get_PositionOnBaseMap((Thing) vehicle), cell.ToBaseMapCoord(map));
    foreach (Thing thing in thingList)
    {
      if (thing is VehiclePawn vehiclePawn && vehiclePawn != vehicle && ((double) num < 20.0 || !vehiclePawn.vehiclePather.Moving))
        return vehiclePawn;
    }
    return (VehiclePawn) null;
  }

  public static bool DrivableRectOnCell(
    this VehiclePawn vehicle,
    IntVec3 cell,
    bool maxPossibleSize,
    Map map)
  {
    if (maxPossibleSize)
      return ((IEnumerable<IntVec3>) (object) Ext_Vehicles.VehicleRect(vehicle, cell, Rot8.op_Implicit(Rot8.North), false)).All<IntVec3>((Func<IntVec3, bool>) (rectCell => vehicle.Drivable(rectCell, map))) && ((IEnumerable<IntVec3>) (object) Ext_Vehicles.VehicleRect(vehicle, cell, Rot8.op_Implicit(Rot8.East), false)).All<IntVec3>((Func<IntVec3, bool>) (rectCell => vehicle.Drivable(rectCell, map)));
    CellRect cellRect = Ext_Vehicles.MinRect(vehicle, cell);
    return ((CellRect) ref cellRect).Cells.All<IntVec3>((Func<IntVec3, bool>) (c => vehicle.Drivable(c, map)));
  }

  public static bool Drivable(this VehiclePawn vehicle, IntVec3 cell, Map map)
  {
    return GenGrid.InBounds(cell, map) && vehicle.DrivableFast(cell, map);
  }

  public static bool DrivableFast(this VehiclePawn vehicle, int index, Map map)
  {
    IntVec3 cell = ((CellIndices) ref ((Thing) vehicle).Map.cellIndices).IndexToCell(index);
    return vehicle.DrivableFast(cell, map);
  }

  public static bool DrivableFast(this VehiclePawn vehicle, int x, int z, Map map)
  {
    IntVec3 cell;
    // ISSUE: explicit constructor call
    ((IntVec3) ref cell).\u002Ector(x, 0, z);
    return vehicle.DrivableFast(cell, map);
  }

  public static bool DrivableFast(this VehiclePawn vehicle, IntVec3 cell, Map map)
  {
    VehiclePawn vehiclePawn = ComponentCache.GetDetachedMapComponent<VehiclePositionManager>(map).ClaimedBy(cell);
    return (vehiclePawn == null || vehiclePawn == vehicle) && ComponentCache.GetCachedMapComponent<VehiclePathingSystem>(map)[vehicle.VehicleDef].VehiclePathGrid.WalkableFast(cell);
  }

  public static bool CanReachVehicle(
    this VehiclePawn vehicle,
    LocalTargetInfo dest,
    PathEndMode peMode,
    Danger maxDanger,
    TraverseMode mode,
    Map destMap,
    out TargetInfo exitSpot,
    out TargetInfo enterSpot)
  {
    exitSpot = TargetInfo.Invalid;
    enterSpot = TargetInfo.Invalid;
    TraverseParms traverseParms = TraverseParms.For((Pawn) vehicle, maxDanger, mode, false, false, false, true);
    if (IntVec3.op_Equality(((LocalTargetInfo) ref dest).Cell, ((Thing) vehicle).Position) && destMap == ((Thing) vehicle).Map)
      return true;
    if (!((Thing) vehicle).Spawned)
      return false;
    Map departMap1 = ((Thing) vehicle).Map;
    if (departMap1 == null || destMap == null)
      return false;
    if (departMap1 == destMap)
      return MapComponentCache<VehiclePathingSystem>.GetComponent(departMap1)[vehicle.VehicleDef].VehicleReachability.CanReachVehicle(((Thing) vehicle).Position, dest, peMode, traverseParms);
    if (ModCompat.CompatBase<ModCompat.MultiFloors>.Active && ModCompat.MultiFloors.GetLevel(departMap1) != ModCompat.MultiFloors.GetLevel(destMap) || vehicle is VehiclePawnWithMap)
      return false;
    VehiclePawnWithMap vehicle2;
    Map map1 = !destMap.IsVehicleMapOf(out vehicle2) || !((Thing) vehicle2).Spawned ? destMap : ((Thing) vehicle2).Map;
    VehiclePawnWithMap vehicle7;
    Map map2 = !departMap1.IsVehicleMapOf(out vehicle7) || !((Thing) vehicle7).Spawned ? departMap1 : ((Thing) vehicle7).Map;
    VehiclePathingSystem destMapPathing = MapComponentCache<VehiclePathingSystem>.GetComponent(destMap);
    if (!destMapPathing[vehicle.VehicleDef].VehiclePathGrid.Enabled)
      destMapPathing.RequestGridsFor(vehicle.VehicleDef, (DeferredGridGeneration.Urgency) 2);
    if (map2 == map1)
    {
      int num = departMap1 == map2 ? 1 : 0;
      bool flag1 = map2 == destMap;
      if (num == 0)
      {
        if (flag1 && vehicle7 != null)
        {
          Thing tmpThing = (Thing) null;
          bool flag2 = GenCollection.Any<CompVehicleEnterSpot>(vehicle7.GetSortedEnterComps(((LocalTargetInfo) ref dest).Cell, CompVehicleEnterSpot.Kind.RampOnly), (Predicate<CompVehicleEnterSpot>) (e =>
          {
            tmpThing = (Thing) e.parent;
            VehiclePawn vehicle1;
            Map departMap2;
            if (!AvailableEnterSpot(e) || ((IEnumerable<IntVec3>) (object) GenAdj.OccupiedRect(tmpThing)).Any<IntVec3>((Func<IntVec3, bool>) (c3 => !vehicle1.Drivable(c3, departMap2))))
              return false;
            IntVec3 position = tmpThing.Position;
            Rot4 rotation = tmpThing.Rotation;
            IntVec3 intVec3_1 = IntVec3.op_Multiply(((Rot4) ref rotation).FacingCell, vehicle.HalfLength());
            IntVec3 intVec3_2 = IntVec3.op_Addition(position, intVec3_1);
            IntVec3 intVec3_3 = CrossMapReachabilityUtility.EnterVehiclePosition(TargetInfo.op_Implicit(tmpThing), vehicle);
            return MapComponentCache<VehiclePathingSystem>.GetComponent(departMap)[vehicle.VehicleDef].VehicleReachability.CanReachVehicle(((Thing) vehicle).Position, LocalTargetInfo.op_Implicit(intVec3_2), (PathEndMode) 1, traverseParms) && destMapPathing[vehicle.VehicleDef].VehicleReachability.CanReachVehicle(intVec3_3, dest, peMode, (TraverseMode) 1, traverseParms.maxDanger);

            bool AvailableEnterSpot(CompVehicleEnterSpot comp)
            {
              return comp != null && ((Thing) comp.parent).def.size.x >= ((ThingDef) vehicle1.VehicleDef).size.x;
            }
          }));
          exitSpot = flag2 ? TargetInfo.op_Implicit(tmpThing) : TargetInfo.Invalid;
          return flag2;
        }
      }
      else if (!flag1 && vehicle2 != null)
      {
        Thing tmpThing = (Thing) null;
        bool flag3 = GenCollection.Any<CompVehicleEnterSpot>(vehicle2.GetSortedEnterComps(((Thing) vehicle).Position, CompVehicleEnterSpot.Kind.RampOnly), (Predicate<CompVehicleEnterSpot>) (e =>
        {
          tmpThing = (Thing) e.parent;
          VehiclePawn vehicle3;
          Map destMap1;
          if (!AvailableEnterSpot(e) || ((IEnumerable<IntVec3>) (object) GenAdj.OccupiedRect(tmpThing)).Any<IntVec3>((Func<IntVec3, bool>) (c3 => !vehicle3.Drivable(c3, destMap1))))
            return false;
          IntVec3 intVec3_4 = CrossMapReachabilityUtility.EnterVehiclePosition(TargetInfo.op_Implicit(tmpThing), vehicle);
          IntVec3 position = tmpThing.Position;
          Rot4 rotation = tmpThing.Rotation;
          IntVec3 intVec3_5 = IntVec3.op_Multiply(((Rot4) ref rotation).FacingCell, vehicle.HalfLength());
          IntVec3 intVec3_6 = IntVec3.op_Addition(position, intVec3_5);
          return MapComponentCache<VehiclePathingSystem>.GetComponent(departMap)[vehicle.VehicleDef].VehicleReachability.CanReachVehicle(((Thing) vehicle).Position, LocalTargetInfo.op_Implicit(intVec3_4), (PathEndMode) 1, traverseParms) && destMapPathing[vehicle.VehicleDef].VehicleReachability.CanReachVehicle(intVec3_6, dest, peMode, (TraverseMode) 1, traverseParms.maxDanger);
        }));
        enterSpot = flag3 ? TargetInfo.op_Implicit(tmpThing) : TargetInfo.Invalid;
        return flag3;
      }
      if (vehicle7 != null && vehicle2 != null)
      {
        VehiclePathingSystem departBaseMapPathing = MapComponentCache<VehiclePathingSystem>.GetComponent(map2);
        if (!departBaseMapPathing[vehicle.VehicleDef].VehiclePathGrid.Enabled)
          departBaseMapPathing.RequestGridsFor(vehicle.VehicleDef, (DeferredGridGeneration.Urgency) 2);
        Thing tmpThing = (Thing) null;
        Thing tmpThing2 = (Thing) null;
        bool flag4 = GenCollection.Any<CompVehicleEnterSpot>(vehicle7.GetSortedEnterComps(((LocalTargetInfo) ref dest).Cell.ToBaseMapCoord(vehicle2), CompVehicleEnterSpot.Kind.RampOnly), (Predicate<CompVehicleEnterSpot>) (e =>
        {
          tmpThing = (Thing) e.parent;
          VehiclePawn vehicle5;
          Map departMap3;
          if (!AvailableEnterSpot(e) || ((IEnumerable<IntVec3>) (object) GenAdj.OccupiedRect(tmpThing)).Any<IntVec3>((Func<IntVec3, bool>) (c => !vehicle5.Drivable(c, departMap3))))
            return false;
          IntVec3 cell = CrossMapReachabilityUtility.EnterVehiclePosition(TargetInfo.op_Implicit(tmpThing), vehicle);
          IntVec3 position1 = tmpThing.Position;
          Rot4 rotation1 = tmpThing.Rotation;
          IntVec3 intVec3_10 = IntVec3.op_Multiply(((Rot4) ref rotation1).FacingCell, vehicle.HalfLength());
          IntVec3 cell2 = IntVec3.op_Addition(position1, intVec3_10);
          return GenCollection.Any<CompVehicleEnterSpot>(vehicle2.GetSortedEnterComps(cell, CompVehicleEnterSpot.Kind.RampOnly), (Predicate<CompVehicleEnterSpot>) (e2 =>
          {
            tmpThing2 = (Thing) e2.parent;
            VehiclePawn vehicle6;
            Map destMap3;
            if (!AvailableEnterSpot(e2) || ((IEnumerable<IntVec3>) (object) GenAdj.OccupiedRect(tmpThing2)).Any<IntVec3>((Func<IntVec3, bool>) (c => !vehicle6.Drivable(c, destMap3))))
              return false;
            IntVec3 intVec3_11 = CrossMapReachabilityUtility.EnterVehiclePosition(TargetInfo.op_Implicit(tmpThing2), vehicle);
            IntVec3 position2 = tmpThing2.Position;
            Rot4 rotation2 = tmpThing2.Rotation;
            IntVec3 intVec3_12 = IntVec3.op_Multiply(((Rot4) ref rotation2).FacingCell, vehicle.HalfLength());
            IntVec3 intVec3_13 = IntVec3.op_Addition(position2, intVec3_12);
            return MapComponentCache<VehiclePathingSystem>.GetComponent(departMap)[vehicle.VehicleDef].VehicleReachability.CanReachVehicle(((Thing) vehicle).Position, LocalTargetInfo.op_Implicit(cell2), (PathEndMode) 1, traverseParms) && departBaseMapPathing[vehicle.VehicleDef].VehicleReachability.CanReachVehicle(cell, LocalTargetInfo.op_Implicit(intVec3_11), (PathEndMode) 1, (TraverseMode) 1, traverseParms.maxDanger) && destMapPathing[vehicle.VehicleDef].VehicleReachability.CanReachVehicle(intVec3_13, dest, peMode, (TraverseMode) 1, traverseParms.maxDanger);
          }));
        }));
        exitSpot = flag4 ? TargetInfo.op_Implicit(tmpThing) : TargetInfo.Invalid;
        enterSpot = flag4 ? TargetInfo.op_Implicit(tmpThing2) : TargetInfo.Invalid;
        return flag4;
      }
    }
    return false;
  }

  [DebugAction("Vehicle Map Framework", "Flash Traverse Points", false, false, false, false, false, 0, false)]
  private static void FlashTraversePoints(Pawn p)
  {
    DebugTools.curTool = new DebugTool($"{p}: Destination...", (Action) (() =>
    {
      VehiclePawnWithMap vehicle;
      IntVec3 intVec3;
      Map destMap;
      if (UI.MouseMapPosition().TryGetVehicleMap(Find.CurrentMap, out vehicle, VehicleMapFlag.None))
      {
        intVec3 = UI.MouseCell().ToVehicleMapCoord(vehicle);
        destMap = vehicle.VehicleMap;
      }
      else
      {
        intVec3 = UI.MouseCell();
        destMap = ((Thing) p).Map;
      }
      TargetInfo exitSpot;
      TargetInfo enterSpot;
      List<TraverseSpots> spotsQueue;
      if (p.CanReach(LocalTargetInfo.op_Implicit(intVec3), (PathEndMode) 1, (Danger) 3, false, false, (TraverseMode) 0, destMap, out exitSpot, out enterSpot, out spotsQueue))
      {
        int index = 0;
        if (!GenList.NullOrEmpty<TraverseSpots>((IList<TraverseSpots>) spotsQueue))
        {
          foreach (TraverseSpots traverseSpots in spotsQueue)
          {
            FlashCell(traverseSpots.exitSpot, false, ref index);
            FlashCell(traverseSpots.enterSpot, true, ref index);
          }
        }
        FlashCell(exitSpot, false, ref index);
        FlashCell(enterSpot, true, ref index);
      }
      else
        Messages.Message($"{p} can not reach to {new TargetInfo(intVec3, destMap, false)}", MessageTypeDefOf.RejectInput, false);
    }), (Action) null);

    static void FlashCell(TargetInfo target, bool enterSpot, ref int index)
    {
      ((TargetInfo) ref target).Map?.debugDrawer.FlashCell(((TargetInfo) ref target).Cell, enterSpot ? 0.2f : 0.4f, $"{index++}", 50);
    }
  }

  [DebugAction("Vehicle Map Framework", "Toggle A* Debug", false, false, false, false, false, 0, false)]
  private static void ToggleDebugTraverser()
  {
    CrossMapReachabilityUtility.aStar.debug = !CrossMapReachabilityUtility.aStar.debug;
    Messages.Message((CrossMapReachabilityUtility.aStar.debug ? "Enabled" : "Disabled") + " the CrossMapReachability's A* debug draw/profile.", MessageTypeDefOf.TaskCompletion, false);
  }

  internal struct MapTraverse : IEquatable<CrossMapReachabilityUtility.MapTraverse>
  {
    public TargetInfo exitSpot;
    public readonly TargetInfo enterSpot;
    public bool canMerge;

    public MapTraverse(TargetInfo exitSpot, TargetInfo enterSpot, bool canMerge = true)
    {
      this.exitSpot = exitSpot;
      this.enterSpot = enterSpot;
      this.canMerge = canMerge;
      District district = RegionAndRoomQuery.DistirctAtFast(((TargetInfo) ref enterSpot).Cell, ((TargetInfo) ref enterSpot).Map, (RegionType) 14);
      // ISSUE: reference to a compiler-generated field
      this.\u003CDistrictID\u003Ek__BackingField = district != null ? district.ID : -1;
    }

    public int DistrictID { get; init; }

    public bool Equals(CrossMapReachabilityUtility.MapTraverse other)
    {
      return this.DistrictID != -1 ? this.DistrictID == other.DistrictID : TargetInfo.op_Equality(this.enterSpot, other.enterSpot);
    }

    public override int GetHashCode()
    {
      return this.DistrictID == -1 ? this.enterSpot.GetHashCode() : Gen.HashCombineInt(this.DistrictID, 2821981);
    }

    public override string ToString()
    {
      VehiclePawnWithMap vehicle1;
      string str1 = $"Exit: {this.exitSpot} {(((TargetInfo) ref this.exitSpot).Map.IsVehicleMapOf(out vehicle1) ? (object) ((WorldObject) vehicle1.VehicleMap.Parent).Label : (object) (string) null)}, ";
      TargetInfo enterSpot1 = (object) this.enterSpot;
      TargetInfo enterSpot2 = this.enterSpot;
      VehiclePawnWithMap vehicle2;
      string label = ((TargetInfo) ref enterSpot2).Map.IsVehicleMapOf(out vehicle2) ? ((WorldObject) vehicle2.VehicleMap.Parent).Label : (string) null;
      string str2 = $"Enter: {enterSpot1} {label}";
      return str1 + str2;
    }
  }

  internal class Traverser
  {
    private IntVec3 _destBaseMapCoord;
    private TraverseParms _traverseParms;
    private TraverseParms _traverseParms2;
    private bool _destroyMode;
    private Ability_MapTraverse _ability;
    private readonly List<Map> _tmpCandidates = new List<Map>();
    private readonly HashSet<int> _visitedDistrictIDs = new HashSet<int>();
    private readonly List<CrossMapReachabilityUtility.MapTraverse> neighbors = new List<CrossMapReachabilityUtility.MapTraverse>(32 /*0x20*/);
    private int debugNodeNumber;
    public Region destRegion;

    public void SetParameters(
      TargetInfo start,
      TargetInfo destination,
      TraverseParms traverseParms,
      Ability_MapTraverse ability)
    {
      this._destBaseMapCoord = VehicleMapUtility.get_CellOnGroundMap(destination);
      this._traverseParms = traverseParms;
      TraverseMode traverseMode = traverseParms.mode > 1 ? traverseParms.mode : (TraverseMode) (object) 1;
      this._traverseParms2 = traverseParms.pawn != null ? TraverseParms.For(traverseParms.pawn, traverseParms.maxDanger, traverseMode, traverseParms.canBashDoors, traverseParms.alwaysUseAvoidGrid, traverseParms.canBashFences, traverseParms.avoidPersistentDanger) : TraverseParms.For(traverseMode, traverseParms.maxDanger, traverseParms.canBashDoors, traverseParms.alwaysUseAvoidGrid, traverseParms.canBashFences, traverseParms.avoidPersistentDanger, false);
      TraverseMode mode = this._traverseParms.mode;
      this._destroyMode = mode - 3 <= 1 || mode == 6;
      this._ability = ability;
      this._tmpCandidates.Clear();
      this._tmpCandidates.AddRange(((TargetInfo) ref start).Map.BaseMapAndVehicleMaps());
      Map destMap = ((TargetInfo) ref destination).Map;
      GenCollection.SortBy<Map, int>(this._tmpCandidates, (Func<Map, int>) (m =>
      {
        if (m == destMap)
          return 0;
        VehiclePawnWithMap vehicle;
        if (m.IsVehicleMapOf(out vehicle))
        {
          IntVec3 intVec3 = IntVec3.op_Subtraction(m.Center.ToBaseMapCoord(vehicle), this._destBaseMapCoord);
          return ((IntVec3) ref intVec3).LengthManhattan;
        }
        IntVec3 size = m.Size;
        return ((IntVec3) ref size).LengthManhattan / 2;
      }));
      this._visitedDistrictIDs.Clear();
      District district = RegionAndRoomQuery.DistirctAtFast(((TargetInfo) ref start).Cell, ((TargetInfo) ref start).Map, (RegionType) 14);
      if (district != null)
        this._visitedDistrictIDs.Add(district.ID);
      this.destRegion = (Region) null;
      this.debugNodeNumber = 0;
    }

    public IEnumerable<CrossMapReachabilityUtility.MapTraverse> Neighbors(
      CrossMapReachabilityUtility.MapTraverse current)
    {
      TargetInfo enterSpot1 = current.enterSpot;
      IntVec3 start = ((TargetInfo) ref enterSpot1).Cell;
      TargetInfo enterSpot2 = current.enterSpot;
      Map map = ((TargetInfo) ref enterSpot2).Map;
      VehiclePawnWithMap vehicle;
      DeepProfilerScope profiler;
      List<CompVehicleEnterSpot>.Enumerator enumerator1;
      Map map2;
      int startIndex;
      int count;
      int i;
      HashSet<District>.Enumerator enumerator2;
      if (map.IsVehicleMapOf(out vehicle))
      {
        profiler = new DeepProfilerScope("Vehicle Neighbors");
        if (!vehicle.AllowExitFor(this._traverseParms.pawn))
          yield break;
        bool spawned = ((Thing) vehicle).Spawned;
        enumerator1 = vehicle.GetSortedEnterComps(this._destBaseMapCoord.ToVehicleMapCoord(vehicle)).GetEnumerator();
        while (enumerator1.MoveNext())
        {
          CompVehicleEnterSpot current1 = enumerator1.Current;
          if (current1 != null)
          {
            TargetInfo availableAccessSpot = current1.AvailableAccessSpot;
            if (((TargetInfo) ref availableAccessSpot).IsValid)
              yield return new CrossMapReachabilityUtility.MapTraverse(TargetInfo.op_Implicit((Thing) current1.parent), availableAccessSpot, !VehicleMapUtility.get_IsVehicleMap(((TargetInfo) ref availableAccessSpot).Map));
          }
        }
        enumerator1 = new List<CompVehicleEnterSpot>.Enumerator();
        if (spawned)
        {
          map2 = ((Thing) vehicle).Map;
          IntVec3 intVec3 = this._destroyMode ? this._destBaseMapCoord.ClosestMapEdgeCell(vehicle) : this._destBaseMapCoord.ClosestWalkableEdgeCell(vehicle);
          if (((IntVec3) ref intVec3).IsValid)
          {
            startIndex = vehicle.CachedMapEdgeCells.IndexOf(intVec3);
            count = vehicle.CachedMapEdgeCells.Count;
            for (i = 0; i < count; ++i)
            {
              int index = GenMath.PositiveMod(startIndex + (i % 2 == 0 ? i / 2 : -(i / 2 + 1)), count);
              IntVec3 cachedMapEdgeCell = vehicle.CachedMapEdgeCells[index];
              if (this._destroyMode || vehicle.CachedWalkableMapEdgeCells.ContainsKey(cachedMapEdgeCell))
              {
                IntVec3 cachedEnterPosition = vehicle.GetCachedEnterPosition(index);
                if (((IntVec3) ref cachedEnterPosition).IsValid)
                {
                  CrossMapReachabilityUtility.MapTraverse traverse = new CrossMapReachabilityUtility.MapTraverse(new TargetInfo(cachedMapEdgeCell, map, false), new TargetInfo(cachedEnterPosition, map2, false));
                  yield return traverse;
                  if (!this._visitedDistrictIDs.Contains(traverse.DistrictID))
                    traverse = new CrossMapReachabilityUtility.MapTraverse();
                  else
                    break;
                }
              }
            }
          }
          map2 = (Map) null;
        }
      }
      else
      {
        profiler = new DeepProfilerScope("Ground Neighbors");
        for (count = 0; count < this._tmpCandidates.Count; ++count)
        {
          map2 = this._tmpCandidates[count];
          if (map != map2)
          {
            VehiclePawnWithMap vehicle2;
            if (map2.IsVehicleMapOf(out vehicle2))
            {
              if (vehicle2.AllowEnterFor(this._traverseParms.pawn))
              {
                enumerator1 = vehicle2.GetSortedEnterComps(start.ToVehicleMapCoord(vehicle2), CompVehicleEnterSpot.Kind.GroundAccessOnly).GetEnumerator();
                while (enumerator1.MoveNext())
                {
                  CompVehicleEnterSpot current2 = enumerator1.Current;
                  if (current2 != null)
                  {
                    TargetInfo availableAccessSpot = current2.AvailableAccessSpot;
                    if (((TargetInfo) ref availableAccessSpot).IsValid && ((TargetInfo) ref availableAccessSpot).Map == map)
                      yield return new CrossMapReachabilityUtility.MapTraverse(availableAccessSpot, TargetInfo.op_Implicit((Thing) current2.parent));
                  }
                }
                enumerator1 = new List<CompVehicleEnterSpot>.Enumerator();
                enumerator2 = vehicle2.CachedEdgeDistricts.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                  District district = enumerator2.Current;
                  if (ValidDistrict(district, this._visitedDistrictIDs))
                  {
                    List<IntVec3> cachedMapEdgeCells = vehicle2.CachedMapEdgeCells;
                    TargetInfo enterSpot3 = current.enterSpot;
                    IntVec3 intVec3 = ((TargetInfo) ref enterSpot3).Cell.ClosestWalkableEdgeCell(vehicle2);
                    startIndex = cachedMapEdgeCells.IndexOf(intVec3);
                    i = vehicle2.CachedMapEdgeCells.Count;
                    for (int j = 0; j < i; ++j)
                    {
                      int index = GenMath.PositiveMod(startIndex + (j % 2 == 0 ? j / 2 : -j / 2 + 1), i);
                      IntVec3 cachedEnterPosition = vehicle2.GetCachedEnterPosition(index);
                      if (((IntVec3) ref cachedEnterPosition).IsValid)
                      {
                        IntVec3 cachedMapEdgeCell = vehicle2.CachedMapEdgeCells[index];
                        yield return new CrossMapReachabilityUtility.MapTraverse(new TargetInfo(cachedEnterPosition, map, false), new TargetInfo(cachedMapEdgeCell, map2, false));
                        if (this._visitedDistrictIDs.Contains(district.ID))
                          break;
                      }
                    }
                    district = (District) null;
                  }
                }
                enumerator2 = new HashSet<District>.Enumerator();
              }
              else
                continue;
            }
            map2 = (Map) null;
            vehicle2 = (VehiclePawnWithMap) null;
          }
        }
      }
      if (this._ability != null)
      {
        profiler = new DeepProfilerScope("Ability Neighbors");
        try
        {
          for (count = 0; count < this._tmpCandidates.Count; ++count)
          {
            map2 = this._tmpCandidates[count];
            if (map != map2)
            {
              VehiclePawnWithMap vehicle1;
              if (map2.IsVehicleMapOf(out vehicle1))
              {
                if (vehicle1.AllowEnterFor(this._traverseParms.pawn))
                {
                  enumerator2 = vehicle1.CachedEdgeDistricts.GetEnumerator();
                  while (enumerator2.MoveNext())
                  {
                    District current3 = enumerator2.Current;
                    if (ValidDistrict(current3, this._visitedDistrictIDs))
                    {
                      IntVec3 anyCell = current3.Regions[0].AnyCell;
                      TargetInfo to;
                      // ISSUE: explicit constructor call
                      ((TargetInfo) ref to).\u002Ector(anyCell, map2, false);
                      TargetInfo castSpot;
                      TargetInfo targSpot;
                      if (this._ability.TryFindCastPositionFromTo(current.enterSpot, to, out castSpot, out targSpot, current3.ID))
                        yield return new CrossMapReachabilityUtility.MapTraverse(castSpot, targSpot, false);
                    }
                  }
                  enumerator2 = new HashSet<District>.Enumerator();
                }
                else
                  continue;
              }
              else
              {
                TargetInfo castSpot;
                TargetInfo targSpot;
                if (this._ability.TryFindCastPositionFromTo(current.enterSpot, new TargetInfo(this._destBaseMapCoord, map2, false), out castSpot, out targSpot))
                  yield return new CrossMapReachabilityUtility.MapTraverse(castSpot, targSpot, false);
              }
              map2 = (Map) null;
            }
          }
        }
        finally
        {
          profiler.Dispose();
        }
      }

      static bool ValidDistrict(District district, HashSet<int> visited)
      {
        return district.RegionCount != 0 && (district.RegionType & 14) != null && !visited.Contains(district.ID);
      }
    }

    public List<CrossMapReachabilityUtility.MapTraverse> Neighbors_New(
      CrossMapReachabilityUtility.MapTraverse current)
    {
      this.neighbors.Clear();
      TargetInfo enterSpot1 = current.enterSpot;
      IntVec3 cell = ((TargetInfo) ref enterSpot1).Cell;
      TargetInfo enterSpot2 = current.enterSpot;
      Map map1 = ((TargetInfo) ref enterSpot2).Map;
      VehiclePawnWithMap vehicle1;
      if (map1.IsVehicleMapOf(out vehicle1))
      {
        using (new DeepProfilerScope("Vehicle Neighbors"))
        {
          if (!vehicle1.AllowExitFor(this._traverseParms.pawn))
            return this.neighbors;
          bool spawned = ((Thing) vehicle1).Spawned;
          foreach (CompVehicleEnterSpot sortedEnterComp in vehicle1.GetSortedEnterComps(this._destBaseMapCoord.ToVehicleMapCoord(vehicle1)))
          {
            if (sortedEnterComp != null)
            {
              TargetInfo availableAccessSpot = sortedEnterComp.AvailableAccessSpot;
              if (((TargetInfo) ref availableAccessSpot).IsValid)
                this.neighbors.Add(new CrossMapReachabilityUtility.MapTraverse(TargetInfo.op_Implicit((Thing) sortedEnterComp.parent), availableAccessSpot, !VehicleMapUtility.get_IsVehicleMap(((TargetInfo) ref availableAccessSpot).Map)));
            }
          }
          if (spawned)
          {
            Map map2 = ((Thing) vehicle1).Map;
            IntVec3 intVec3 = this._destroyMode ? this._destBaseMapCoord.ClosestMapEdgeCell(vehicle1) : this._destBaseMapCoord.ClosestWalkableEdgeCell(vehicle1);
            if (((IntVec3) ref intVec3).IsValid)
            {
              int num1 = vehicle1.CachedMapEdgeCells.IndexOf(intVec3);
              int count = vehicle1.CachedMapEdgeCells.Count;
              for (int index1 = 0; index1 < count; ++index1)
              {
                int num2 = index1 % 2 == 0 ? index1 / 2 : -(index1 / 2 + 1);
                int index2 = GenMath.PositiveMod(num1 + num2, count);
                IntVec3 cachedMapEdgeCell = vehicle1.CachedMapEdgeCells[index2];
                if (this._destroyMode || vehicle1.CachedWalkableMapEdgeCells.ContainsKey(cachedMapEdgeCell))
                {
                  IntVec3 cachedEnterPosition = vehicle1.GetCachedEnterPosition(index2);
                  if (((IntVec3) ref cachedEnterPosition).IsValid)
                  {
                    CrossMapReachabilityUtility.MapTraverse mapTraverse = new CrossMapReachabilityUtility.MapTraverse(new TargetInfo(cachedMapEdgeCell, map1, false), new TargetInfo(cachedEnterPosition, map2, false));
                    this.neighbors.Add(mapTraverse);
                    if (this._visitedDistrictIDs.Contains(mapTraverse.DistrictID))
                      break;
                  }
                }
              }
            }
          }
        }
      }
      else
      {
        using (new DeepProfilerScope("Ground Neighbors"))
        {
          for (int index3 = 0; index3 < this._tmpCandidates.Count; ++index3)
          {
            Map tmpCandidate = this._tmpCandidates[index3];
            VehiclePawnWithMap vehicle2;
            if (map1 != tmpCandidate && tmpCandidate.IsVehicleMapOf(out vehicle2) && vehicle2.AllowEnterFor(this._traverseParms.pawn))
            {
              foreach (CompVehicleEnterSpot sortedEnterComp in vehicle2.GetSortedEnterComps(cell.ToVehicleMapCoord(vehicle2), CompVehicleEnterSpot.Kind.GroundAccessOnly))
              {
                if (sortedEnterComp != null)
                {
                  TargetInfo availableAccessSpot = sortedEnterComp.AvailableAccessSpot;
                  if (((TargetInfo) ref availableAccessSpot).IsValid && ((TargetInfo) ref availableAccessSpot).Map == map1)
                    this.neighbors.Add(new CrossMapReachabilityUtility.MapTraverse(availableAccessSpot, TargetInfo.op_Implicit((Thing) sortedEnterComp.parent)));
                }
              }
              foreach (District cachedEdgeDistrict in vehicle2.CachedEdgeDistricts)
              {
                if (CrossMapReachabilityUtility.Traverser.ValidateDistrict(cachedEdgeDistrict, this._visitedDistrictIDs))
                {
                  List<IntVec3> cachedMapEdgeCells = vehicle2.CachedMapEdgeCells;
                  TargetInfo enterSpot3 = current.enterSpot;
                  IntVec3 intVec3 = ((TargetInfo) ref enterSpot3).Cell.ClosestWalkableEdgeCell(vehicle2);
                  int num3 = cachedMapEdgeCells.IndexOf(intVec3);
                  int count = vehicle2.CachedMapEdgeCells.Count;
                  for (int index4 = 0; index4 < count; ++index4)
                  {
                    int num4 = index4 % 2 == 0 ? index4 / 2 : -index4 / 2 + 1;
                    int index5 = GenMath.PositiveMod(num3 + num4, count);
                    IntVec3 cachedEnterPosition = vehicle2.GetCachedEnterPosition(index5);
                    if (((IntVec3) ref cachedEnterPosition).IsValid)
                    {
                      IntVec3 cachedMapEdgeCell = vehicle2.CachedMapEdgeCells[index5];
                      this.neighbors.Add(new CrossMapReachabilityUtility.MapTraverse(new TargetInfo(cachedEnterPosition, map1, false), new TargetInfo(cachedMapEdgeCell, tmpCandidate, false)));
                      if (this._visitedDistrictIDs.Contains(cachedEdgeDistrict.ID))
                        break;
                    }
                  }
                }
              }
            }
          }
        }
      }
      if (this._ability != null)
      {
        using (new DeepProfilerScope("Ability Neighbors"))
        {
          for (int index = 0; index < this._tmpCandidates.Count; ++index)
          {
            Map tmpCandidate = this._tmpCandidates[index];
            if (map1 != tmpCandidate)
            {
              VehiclePawnWithMap vehicle3;
              if (tmpCandidate.IsVehicleMapOf(out vehicle3))
              {
                if (vehicle3.AllowEnterFor(this._traverseParms.pawn))
                {
                  foreach (District cachedEdgeDistrict in vehicle3.CachedEdgeDistricts)
                  {
                    if (CrossMapReachabilityUtility.Traverser.ValidateDistrict(cachedEdgeDistrict, this._visitedDistrictIDs))
                    {
                      IntVec3 anyCell = cachedEdgeDistrict.Regions[0].AnyCell;
                      TargetInfo to;
                      // ISSUE: explicit constructor call
                      ((TargetInfo) ref to).\u002Ector(anyCell, tmpCandidate, false);
                      TargetInfo castSpot;
                      TargetInfo targSpot;
                      if (this._ability.TryFindCastPositionFromTo(current.enterSpot, to, out castSpot, out targSpot, cachedEdgeDistrict.ID))
                        this.neighbors.Add(new CrossMapReachabilityUtility.MapTraverse(castSpot, targSpot, false));
                    }
                  }
                }
              }
              else
              {
                TargetInfo castSpot;
                TargetInfo targSpot;
                if (this._ability.TryFindCastPositionFromTo(current.enterSpot, new TargetInfo(this._destBaseMapCoord, tmpCandidate, false), out castSpot, out targSpot))
                  this.neighbors.Add(new CrossMapReachabilityUtility.MapTraverse(castSpot, targSpot, false));
              }
            }
          }
        }
      }
      return this.neighbors;
    }

    private static bool ValidateDistrict(District district, HashSet<int> visited)
    {
      return district.RegionCount != 0 && (district.RegionType & 14) != null && !visited.Contains(district.ID);
    }

    public bool CanEnter(
      CrossMapReachabilityUtility.MapTraverse from,
      CrossMapReachabilityUtility.MapTraverse to)
    {
      if (CrossMapReachabilityUtility.CellCheck(((TargetInfo) ref to.exitSpot).Cell, ((TargetInfo) ref to.exitSpot).Map, this._traverseParms, destroyMode: this._destroyMode))
      {
        TargetInfo enterSpot1 = to.enterSpot;
        IntVec3 cell1 = ((TargetInfo) ref enterSpot1).Cell;
        enterSpot1 = to.enterSpot;
        Map map = ((TargetInfo) ref enterSpot1).Map;
        TraverseParms traverseParms = this._traverseParms;
        if (CrossMapReachabilityUtility.CellCheck(cell1, map, traverseParms, true))
        {
          TargetInfo enterSpot2 = from.enterSpot;
          Reachability reachability = ((TargetInfo) ref enterSpot2).Map.reachability;
          enterSpot2 = from.enterSpot;
          IntVec3 cell2 = ((TargetInfo) ref enterSpot2).Cell;
          LocalTargetInfo localTargetInfo = LocalTargetInfo.op_Implicit(((TargetInfo) ref to.exitSpot).Cell);
          TraverseParms traverseParms2 = this._traverseParms2;
          if (reachability.CanReach(cell2, localTargetInfo, (PathEndMode) 1, traverseParms2))
            return this._visitedDistrictIDs.Add(to.DistrictID);
        }
      }
      return false;
    }

    public bool FinalCheck(
      CrossMapReachabilityUtility.MapTraverse from,
      CrossMapReachabilityUtility.MapTraverse to)
    {
      TargetInfo enterSpot1 = from.enterSpot;
      if (((TargetInfo) ref enterSpot1).Map != null)
      {
        TargetInfo enterSpot2 = from.enterSpot;
        Map map1 = ((TargetInfo) ref enterSpot2).Map;
        TargetInfo enterSpot3 = to.enterSpot;
        Map map2 = ((TargetInfo) ref enterSpot3).Map;
        if (map1 == map2)
        {
          foreach (Region destRegion in CrossMapReachabilityUtility.destRegions)
          {
            enterSpot3 = from.enterSpot;
            Reachability reachability = ((TargetInfo) ref enterSpot3).Map.reachability;
            enterSpot3 = from.enterSpot;
            IntVec3 cell = ((TargetInfo) ref enterSpot3).Cell;
            LocalTargetInfo localTargetInfo = LocalTargetInfo.op_Implicit(destRegion.AnyCell);
            TraverseParms traverseParms2 = this._traverseParms2;
            if (reachability.CanReach(cell, localTargetInfo, (PathEndMode) 1, traverseParms2))
            {
              this.destRegion = destRegion;
              return true;
            }
          }
          return false;
        }
      }
      return false;
    }

    public static int Cost(
      CrossMapReachabilityUtility.MapTraverse from,
      CrossMapReachabilityUtility.MapTraverse to)
    {
      TargetInfo enterSpot = from.enterSpot;
      IntVec3 intVec3 = IntVec3.op_Subtraction(((TargetInfo) ref enterSpot).Cell, ((TargetInfo) ref to.exitSpot).Cell);
      return ((IntVec3) ref intVec3).LengthManhattan + 1;
    }

    public int Heuristic(CrossMapReachabilityUtility.MapTraverse to)
    {
      IntVec3 intVec3 = IntVec3.op_Subtraction(VehicleMapUtility.get_CellOnGroundMap(to.enterSpot), this._destBaseMapCoord);
      return ((IntVec3) ref intVec3).LengthManhattan;
    }

    public static void ProcessPath(
      List<CrossMapReachabilityUtility.MapTraverse> path,
      CrossMapReachabilityUtility.MapTraverse current)
    {
      if (current.canMerge)
      {
        if (path != null)
        {
          int count = path.Count;
          if (count >= 1)
          {
            CrossMapReachabilityUtility.MapTraverse mapTraverse = path[count - 1];
            if (mapTraverse.canMerge)
            {
              mapTraverse.exitSpot = current.exitSpot;
              mapTraverse.canMerge = false;
              List<CrossMapReachabilityUtility.MapTraverse> mapTraverseList = path;
              mapTraverseList[mapTraverseList.Count - 1] = mapTraverse;
              return;
            }
          }
        }
        if (!VehicleMapUtility.get_IsVehicleMap(((TargetInfo) ref current.exitSpot).Map))
          current.exitSpot = TargetInfo.Invalid;
      }
      path.Add(current);
    }

    public void DebugDrawEnterNode(
      CrossMapReachabilityUtility.MapTraverse mapTraverse)
    {
      ((TargetInfo) ref mapTraverse.exitSpot).Map.debugDrawer.FlashCell(((TargetInfo) ref mapTraverse.exitSpot).Cell, 0.25f, $"{this.debugNodeNumber}:exit", 50);
      TargetInfo enterSpot = mapTraverse.enterSpot;
      DebugCellDrawer debugDrawer = ((TargetInfo) ref enterSpot).Map.debugDrawer;
      enterSpot = mapTraverse.enterSpot;
      IntVec3 cell = ((TargetInfo) ref enterSpot).Cell;
      string str = $"{this.debugNodeNumber}:enter";
      debugDrawer.FlashCell(cell, 0.5f, str, 50);
      ++this.debugNodeNumber;
    }
  }

  internal class AStar<T>(
    Func<T, T, int> cost,
    Func<T, IEnumerable<T>> neighbors,
    Func<T, T, bool> finalCheck,
    Func<T, T, bool> canEnter = null,
    Func<T, int> heuristic = null,
    Action<List<T>, T> processPath = null,
    Action<T> debugAction = null)
    where T : IEquatable<T>
  {
    private readonly PriorityQueue<T, int> openQueue = new PriorityQueue<T, int>();
    private readonly Dictionary<T, CrossMapReachabilityUtility.AStar<T>.Node> nodes = new Dictionary<T, CrossMapReachabilityUtility.AStar<T>.Node>();
    public bool debug;

    public void Run(T start, T destination, List<T> path)
    {
      using (new DeepProfilerScope("CrossMapReachability AStar Run", this.debug))
      {
        this.nodes.Clear();
        this.openQueue.Clear();
        this.openQueue.Enqueue(start, 0);
        if (start.Equals(destination))
          return;
        int num;
        T current;
        while (this.openQueue.Count > 0 && this.openQueue.TryDequeue(ref current, ref num))
        {
          using (new DeepProfilerScope("Neighbors"))
          {
            IEnumerable<T> objs = neighbors(current);
            if (objs is List<T> objList)
            {
              foreach (T neighbor in objList)
              {
                bool foundPath;
                EnterNeighbor(neighbor, out foundPath);
                if (foundPath)
                  return;
              }
            }
            else
            {
              foreach (T neighbor in objs)
              {
                bool foundPath;
                EnterNeighbor(neighbor, out foundPath);
                if (foundPath)
                  return;
              }
            }
          }
        }
        path.Clear();

        void EnterNeighbor(T neighbor, out bool foundPath)
        {
          foundPath = false;
          CrossMapReachabilityUtility.AStar<T>.Node node;
          if (!this.CreateNode(current, neighbor, out node))
            return;
          this.nodes[neighbor] = node;
          if (this.debug)
          {
            Action<T> debugActionP = debugAction;
            if (debugActionP != null)
              debugActionP(neighbor);
          }
          this.openQueue.Enqueue(neighbor, node.cost + node.heuristic);
          if (!finalCheck(neighbor, destination))
            return;
          this.SolvePath(start, neighbor, path);
          foundPath = true;
        }
      }
    }

    private bool CreateNode(
      T current,
      T neighbor,
      out CrossMapReachabilityUtility.AStar<T>.Node node)
    {
      using (new DeepProfilerScope(nameof (CreateNode)))
      {
        if (!this.nodes.TryGetValue(neighbor, out node))
        {
          Func<T, T, bool> canEnterP = canEnter;
          bool? nullable = canEnterP != null ? new bool?(canEnterP(current, neighbor)) : new bool?();
          if (!nullable.HasValue || nullable.GetValueOrDefault())
          {
            node = new CrossMapReachabilityUtility.AStar<T>.Node()
            {
              parent = current,
              cost = cost(current, neighbor),
              heuristic = heuristic(neighbor)
            };
            return true;
          }
        }
        return false;
      }
    }

    private void SolvePath(T start, T destination, List<T> path)
    {
      for (T obj = destination; !start.Equals(obj); obj = this.nodes[obj].parent)
        (processPath ?? (Action<List<T>, T>) ((list, cur) => list.Add(cur)))(path, obj);
      path.Reverse();
      if (path.Count != 0)
      {
        List<T> objList = path;
        if (objList[objList.Count - 1].Equals(destination))
          goto label_9;
      }
      StringBuilder stringBuilder = new StringBuilder($"A* failed to solve path from {start} to {destination}.");
      for (int index = 0; index < path.Count; ++index)
        stringBuilder.AppendLine($"  {index}: {path[index]}");
      VMF_Log.Error(stringBuilder.ToString());
label_9:
      if (!this.debug)
        return;
      Log.Message($"A* run finished\nfrom {start}\nto {destination}\npath:\n{string.Join<T>("\n", (IEnumerable<T>) path)}");
    }

    private struct Node
    {
      public T parent;
      public int cost;
      public int heuristic;
    }
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u00242F59771236D7C1AA57ADCD68358D448A
  {
    [ExtensionMarker("<M>$E15D13A036DCD41255EBD6266F8556E9")]
    public Map DestMap
    {
      [ExtensionMarker("<M>$E15D13A036DCD41255EBD6266F8556E9")] get
      {
        throw new NotSupportedException();
      }
      [ExtensionMarker("<M>$E15D13A036DCD41255EBD6266F8556E9")] set
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$E15D13A036DCD41255EBD6266F8556E9")]
    public void RemoveDestMap() => throw new NotSupportedException();

    [ExtensionMarker("<M>$E15D13A036DCD41255EBD6266F8556E9")]
    public Map DepartMap
    {
      [ExtensionMarker("<M>$E15D13A036DCD41255EBD6266F8556E9")] get
      {
        throw new NotSupportedException();
      }
      [ExtensionMarker("<M>$E15D13A036DCD41255EBD6266F8556E9")] set
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$E15D13A036DCD41255EBD6266F8556E9")]
    public void RemoveDepartMap() => throw new NotSupportedException();

    [ExtensionMarker("<M>$E15D13A036DCD41255EBD6266F8556E9")]
    public Map DepartMapOrPawnMap
    {
      [ExtensionMarker("<M>$E15D13A036DCD41255EBD6266F8556E9")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$E15D13A036DCD41255EBD6266F8556E9")]
    public Map DepartMapOrPawnMapHeld
    {
      [ExtensionMarker("<M>$E15D13A036DCD41255EBD6266F8556E9")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$E15D13A036DCD41255EBD6266F8556E9")]
    internal IntVec3? DepartPosition
    {
      [ExtensionMarker("<M>$E15D13A036DCD41255EBD6266F8556E9")] get
      {
        throw new NotSupportedException();
      }
      [ExtensionMarker("<M>$E15D13A036DCD41255EBD6266F8556E9")] set
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$E15D13A036DCD41255EBD6266F8556E9")]
    public bool CanReach(
      LocalTargetInfo dest3,
      PathEndMode peMode,
      Danger maxDanger,
      bool canBashDoors,
      bool canBashFences,
      TraverseMode mode,
      Map destMap)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$E15D13A036DCD41255EBD6266F8556E9")]
    public bool CanReach(
      LocalTargetInfo dest3,
      PathEndMode peMode,
      Danger maxDanger,
      bool canBashDoors,
      bool canBashFences,
      TraverseMode mode,
      Map destMap,
      out TargetInfo exitSpot,
      out TargetInfo enterSpot,
      out List<TraverseSpots> spotsQueue)
    {
      throw new NotSupportedException();
    }

    [SpecialName]
    public static class \u003CM\u003E\u0024E15D13A036DCD41255EBD6266F8556E9
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(Pawn pawn)
      {
      }
    }
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u0024ABFF6B6B8A5941EB4E8CC8D86BA457C5
  {
    [ExtensionMarker("<M>$F9A44A31E7799B8454B27FC6F4A26CDD")]
    public bool DrivableRectOnCell(IntVec3 cell, bool maxPossibleSize, Map map)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$F9A44A31E7799B8454B27FC6F4A26CDD")]
    public bool Drivable(IntVec3 cell, Map map) => throw new NotSupportedException();

    [ExtensionMarker("<M>$F9A44A31E7799B8454B27FC6F4A26CDD")]
    public bool DrivableFast(int index, Map map) => throw new NotSupportedException();

    [ExtensionMarker("<M>$F9A44A31E7799B8454B27FC6F4A26CDD")]
    public bool DrivableFast(int x, int z, Map map) => throw new NotSupportedException();

    [ExtensionMarker("<M>$F9A44A31E7799B8454B27FC6F4A26CDD")]
    public bool DrivableFast(IntVec3 cell, Map map) => throw new NotSupportedException();

    [ExtensionMarker("<M>$F9A44A31E7799B8454B27FC6F4A26CDD")]
    public bool CanReachVehicle(
      LocalTargetInfo dest,
      PathEndMode peMode,
      Danger maxDanger,
      TraverseMode mode,
      Map destMap,
      out TargetInfo exitSpot,
      out TargetInfo enterSpot)
    {
      throw new NotSupportedException();
    }

    [SpecialName]
    public static class \u003CM\u003E\u0024F9A44A31E7799B8454B27FC6F4A26CDD
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(VehiclePawn vehicle)
      {
      }
    }
  }
}
