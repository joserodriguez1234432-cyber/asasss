// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleReachability
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public sealed class VehicleReachability : VehicleGridManager
{
  private readonly Queue<VehicleRegion> openQueue = new Queue<VehicleRegion>();
  private readonly VehicleReachability.AStar chunkSearch;
  private readonly List<VehicleRegion> startingRegions = new List<VehicleRegion>();
  private readonly List<VehicleRegion> destRegions = new List<VehicleRegion>();
  private uint reachedIndex = 1;
  private readonly VehicleReachabilityCache cache = new VehicleReachabilityCache();
  private readonly VehiclePathGrid pathGrid;
  private readonly VehicleRegionGrid regionGrid;

  public VehicleReachability(
    VehiclePathingSystem mapping,
    VehicleDef createdFor,
    VehiclePathGrid pathGrid,
    VehicleRegionGrid regionGrid)
    : base(mapping, createdFor)
  {
    this.chunkSearch = new VehicleReachability.AStar(this, mapping, createdFor);
    this.pathGrid = pathGrid;
    this.regionGrid = regionGrid;
  }

  private bool CalculatingReachability { get; set; }

  public void ClearCache()
  {
    if (this.cache.Count <= 0)
      return;
    this.cache.Clear();
  }

  public void ClearCacheFor(VehiclePawn vehicle) => this.cache.ClearFor(vehicle);

  public void ClearCacheForHostile(Thing hostileTo) => this.cache.ClearForHostile(hostileTo);

  private void QueueNewOpenRegion(VehicleRegion region)
  {
    if (region == null)
      Log.Warning("Tried to queue null region (Vehicles).");
    else if ((int) region.reachedIndex == (int) this.reachedIndex)
    {
      Log.ErrorOnce($"VehicleRegion is already reached; you can't open it. VehicleRegion={region}", region.GetHashCode());
    }
    else
    {
      this.openQueue.Enqueue(region);
      region.reachedIndex = this.reachedIndex;
    }
  }

  private void QueueChunk(VehicleRegion region)
  {
    if (region == null)
      Log.ErrorOnce("[[VehicleFramework]] Tried to queue null region.", "NullVehicleRegion".GetHashCode());
    else if ((int) region.reachedIndex == (int) this.reachedIndex)
    {
      Log.ErrorOnce($"[{"[VehicleFramework]"}] VehicleRegion has already been reached, attempting to retrace which may result in infinite loops. VehicleRegion={region}", region.GetHashCode());
    }
    else
    {
      this.openQueue.Enqueue(region);
      region.reachedIndex = this.reachedIndex;
    }
  }

  public bool CanReachVehicleNonLocal(
    IntVec3 start,
    TargetInfo dest,
    PathEndMode peMode,
    TraverseMode traverseMode,
    Danger maxDanger)
  {
    return (((TargetInfo) ref dest).Map == null || ((TargetInfo) ref dest).Map == this.mapping.map) && this.CanReachVehicle(start, TargetInfo.op_Explicit(dest), peMode, traverseMode, maxDanger);
  }

  public bool CanReachVehicleNonLocal(
    IntVec3 start,
    TargetInfo dest,
    PathEndMode peMode,
    TraverseParms traverseParms)
  {
    return (((TargetInfo) ref dest).Map == null || ((TargetInfo) ref dest).Map == this.mapping.map) && this.CanReachVehicle(start, TargetInfo.op_Explicit(dest), peMode, traverseParms);
  }

  public bool CanReachVehicle(
    IntVec3 start,
    LocalTargetInfo dest,
    PathEndMode peMode,
    TraverseMode traverseMode,
    Danger maxDanger)
  {
    return this.CanReachVehicle(start, dest, peMode, TraverseParms.For(traverseMode, maxDanger, false, false, false, true, false));
  }

  public bool CanReachVehicle(
    IntVec3 start,
    LocalTargetInfo dest,
    PathEndMode peMode,
    TraverseParms traverseParms)
  {
    VehicleDef forVehicleDef;
    if (!this.ValidateCanStart(start, dest, traverseParms, out forVehicleDef))
      return false;
    if (!this.pathGrid.WalkableFast(start))
    {
      Debug.Message($"Unable to start pathing from {start} to {dest}. Not walkable at {start}");
      return false;
    }
    bool flag = traverseParms.mode != 5 && traverseParms.mode != 6;
    if (((peMode == 1 || peMode == 2 ? 1 : (peMode == 3 ? 1 : 0)) & (flag ? 1 : 0)) != 0)
    {
      VehicleRoom vehicleRoom = VehicleRegionAndRoomQuery.RoomAtFast(start, this.mapping.map, this.createdFor);
      if (vehicleRoom != null && vehicleRoom == VehicleRegionAndRoomQuery.RoomAtFast(((LocalTargetInfo) ref dest).Cell, this.mapping.map, this.createdFor))
        return true;
    }
    if (traverseParms.mode == 3)
    {
      IntVec3 start1 = start;
      LocalTargetInfo dest1 = dest;
      PathEndMode peMode1 = peMode;
      TraverseParms traverseParms1 = traverseParms;
      traverseParms1.mode = (TraverseMode) 1;
      TraverseParms traverseParms2 = traverseParms1;
      if (this.CanReachVehicle(start1, dest1, peMode1, traverseParms2))
        return true;
    }
    dest = TargetInfo.op_Explicit(GenPathVehicles.ResolvePathMode(forVehicleDef, this.mapping.map, ((LocalTargetInfo) ref dest).ToTargetInfo(this.mapping.map), ref peMode));
    this.CalculatingReachability = true;
    try
    {
      ++this.reachedIndex;
      this.destRegions.Clear();
      if (peMode == 1)
      {
        VehicleRegion vehicleRegion = VehicleRegionAndRoomQuery.RegionAt(((LocalTargetInfo) ref dest).Cell, this.mapping, this.createdFor);
        if (vehicleRegion != null && vehicleRegion.Allows(traverseParms))
          this.destRegions.Add(vehicleRegion);
      }
      else if (peMode == 2)
        TouchPathEndModeUtilityVehicles.AddAllowedAdjacentRegions(dest, traverseParms, this.mapping.map, this.createdFor, this.destRegions);
      if (this.destRegions.Count == 0 && traverseParms.mode != 3 && traverseParms.mode != 6)
        return false;
      GenList.RemoveDuplicates<VehicleRegion>(this.destRegions, (Func<VehicleRegion, VehicleRegion, bool>) null);
      this.openQueue.Clear();
      this.DetermineStartRegions(start);
      if (this.openQueue.Count == 0 && traverseParms.mode != 3 && traverseParms.mode != 6)
        return false;
      if (GenCollection.Any<VehicleRegion>(this.startingRegions) && GenCollection.Any<VehicleRegion>(this.destRegions) && this.CanUseCache(traverseParms.mode))
      {
        switch ((int) this.GetCachedResult(traverseParms))
        {
          case 0:
            return true;
          case 1:
            return false;
          case 2:
            break;
          default:
            throw new NotImplementedException("BoolUnknown");
        }
      }
      return traverseParms.mode == 3 || traverseParms.mode == 6 || traverseParms.mode == 5 ? this.CheckCellBasedReachability(start, dest, peMode, traverseParms) : this.CheckRegionBasedReachability(traverseParms);
    }
    finally
    {
      this.CalculatingReachability = false;
    }
  }

  private void DetermineStartRegions(IntVec3 start)
  {
    this.startingRegions.Clear();
    if (this.pathGrid.WalkableFast(start))
    {
      VehicleRegion validRegionAt = this.regionGrid.GetValidRegionAt(start);
      this.QueueNewOpenRegion(validRegionAt);
      this.startingRegions.Add(validRegionAt);
    }
    else
    {
      for (int index = 0; index < 8; ++index)
      {
        IntVec3 intVec3 = IntVec3.op_Addition(start, GenAdj.AdjacentCells[index]);
        if (GenGrid.InBounds(intVec3, this.mapping.map) && this.pathGrid.WalkableFast(intVec3))
        {
          VehicleRegion validRegionAt = this.regionGrid.GetValidRegionAt(intVec3);
          if (validRegionAt != null && (int) validRegionAt.reachedIndex != (int) this.reachedIndex)
          {
            this.QueueNewOpenRegion(validRegionAt);
            this.startingRegions.Add(validRegionAt);
          }
        }
      }
    }
  }

  private BoolUnknown GetCachedResult(TraverseParms traverseParms)
  {
    bool flag = false;
    for (int index1 = 0; index1 < this.startingRegions.Count; ++index1)
    {
      for (int index2 = 0; index2 < this.destRegions.Count; ++index2)
      {
        if (this.destRegions[index2] == this.startingRegions[index1])
          return (BoolUnknown) 0;
        BoolUnknown boolUnknown = this.cache.CachedResultFor(this.startingRegions[index1].Room, this.destRegions[index2].Room, traverseParms);
        if (boolUnknown == null)
          return (BoolUnknown) 0;
        if (boolUnknown == 2)
          flag = true;
      }
    }
    return !flag ? (BoolUnknown) 1 : (BoolUnknown) 2;
  }

  private bool CheckRegionBasedReachability(TraverseParms traverseParms)
  {
    while (this.openQueue.Count > 0)
    {
      using (ListSnapshot<VehicleRegionLink> links = this.openQueue.Dequeue().Links)
      {
        foreach (VehicleRegionLink vehicleRegionLink in links)
        {
          if (RegionReachable(vehicleRegionLink.regionA) || RegionReachable(vehicleRegionLink.regionB))
            return true;
        }
      }
    }
    foreach (VehicleRegion startingRegion in this.startingRegions)
    {
      foreach (VehicleRegion destRegion in this.destRegions)
        this.cache.AddCachedResult(startingRegion.Room, destRegion.Room, traverseParms, false);
    }
    return false;

    bool RegionReachable(VehicleRegion linkedRegion)
    {
      if (linkedRegion != null && (int) linkedRegion.reachedIndex != (int) this.reachedIndex && RegionTypeUtility.Passable(linkedRegion.type) && linkedRegion.Allows(traverseParms))
      {
        if (this.destRegions.Contains(linkedRegion))
        {
          foreach (VehicleRegion startingRegion in this.startingRegions)
            this.cache.AddCachedResult(startingRegion.Room, linkedRegion.Room, traverseParms, true);
          return true;
        }
        this.QueueNewOpenRegion(linkedRegion);
      }
      return false;
    }
  }

  public ChunkSet FindChunks(
    IntVec3 start,
    LocalTargetInfo dest,
    PathEndMode peMode,
    TraverseParms traverseParms,
    bool debugDrawSearch = false)
  {
    if (this.ValidateCanStart(start, dest, traverseParms, out VehicleDef _))
    {
      this.openQueue.Clear();
      ++this.reachedIndex;
      return this.chunkSearch.Run(start, dest, traverseParms, debugDrawSearch);
    }
    Log.Error("Can't validate for chunk search");
    return (ChunkSet) null;
  }

  private static void MarkRegionForDrawing(
    VehicleRegion region,
    Map map,
    bool drawRegions = true,
    bool drawWeights = false)
  {
    if (!drawRegions)
      return;
    foreach (IntVec3 cell in region.Cells)
      map.DrawCell_ThreadSafe(cell, 0.65f);
  }

  [Conditional("HIERARCHAL_PATHFINDING")]
  private static void MarkLinksForDrawing(
    VehicleRegion region,
    Map map,
    VehicleRegionLink from,
    VehicleRegionLink to)
  {
    float weight = region.WeightBetween(from, to);
    from.DrawWeight(map, to, weight);
  }

  private bool ValidateCanStart(
    IntVec3 start,
    LocalTargetInfo dest,
    TraverseParms traverseParms,
    out VehicleDef forVehicleDef)
  {
    if (this.CalculatingReachability)
    {
      Log.ErrorOnce("Called CanReachVehicle() while working. This should never happen. Suppressing further errors.", "CanReachVehicleWorkingError".GetHashCode());
      forVehicleDef = (VehicleDef) null;
      return false;
    }
    VehiclePawn pawn = traverseParms.pawn as VehiclePawn;
    forVehicleDef = pawn?.VehicleDef ?? this.createdFor;
    if (pawn != null)
    {
      if (!((Thing) pawn).Spawned)
      {
        Log.Error($"Attempting reachability check for unspawned vehicle {pawn}.");
        return false;
      }
      if (((Thing) pawn).Map != this.mapping.map)
      {
        Log.Error($"Called CanReach with a vehicle not spawned on this map. This means that we can't check its reachability here. Vehicle's current map should have been used instead. vehicle={pawn} vehicle.Map={((Thing) pawn).Map} map={this.mapping.map}");
        return false;
      }
    }
    if (!((LocalTargetInfo) ref dest).IsValid)
    {
      Debug.Warning("Destination Invalid.");
      return false;
    }
    if (((LocalTargetInfo) ref dest).HasThing && ((LocalTargetInfo) ref dest).Thing.Map != this.mapping.map)
    {
      Log.Error($"Called CanReach for regions of a different map than destination.  Destination={dest} Map={this.mapping.map} Destination.Map={((LocalTargetInfo) ref dest).Thing.Map}");
      return false;
    }
    if (GenGrid.InBounds(start, this.mapping.map) && GenGrid.InBounds(((LocalTargetInfo) ref dest).Cell, this.mapping.map))
      return true;
    Debug.Warning("Start or Destination out of bounds for reachability check.");
    return false;
  }

  private bool CheckCellBasedReachability(
    IntVec3 start,
    LocalTargetInfo dest,
    PathEndMode peMode,
    TraverseParms traverseParms)
  {
    IntVec3 foundCell = IntVec3.Invalid;
    this.mapping.map.floodFiller.FloodFill(start, (Predicate<IntVec3>) (cell => this.PassCheck(cell, this.mapping.map, traverseParms)), (Func<IntVec3, bool>) (cell =>
    {
      VehiclePawn pawn = traverseParms.pawn as VehiclePawn;
      if (!VehicleReachabilityImmediate.CanReachImmediateVehicle(cell, dest, this.mapping.map, pawn.VehicleDef, peMode))
        return false;
      foundCell = cell;
      return true;
    }), int.MaxValue, false, (IEnumerable<IntVec3>) null);
    if (((IntVec3) ref foundCell).IsValid)
    {
      if (this.CanUseCache(traverseParms.mode))
      {
        VehicleRegion validRegionAt = this.regionGrid.GetValidRegionAt(foundCell);
        if (validRegionAt != null)
        {
          foreach (VehicleRegion startingRegion in this.startingRegions)
            this.cache.AddCachedResult(startingRegion.Room, validRegionAt.Room, traverseParms, true);
        }
      }
      return true;
    }
    if (this.CanUseCache(traverseParms.mode))
    {
      foreach (VehicleRegion startingRegion in this.startingRegions)
      {
        foreach (VehicleRegion destRegion in this.destRegions)
          this.cache.AddCachedResult(startingRegion.Room, destRegion.Room, traverseParms, false);
      }
    }
    return false;
  }

  private bool PassCheck(IntVec3 cell, Map map, TraverseParms traverseParms)
  {
    int index = ((CellIndices) ref map.cellIndices).CellToIndex(cell);
    if ((traverseParms.mode == 6 || traverseParms.mode == 5) && GridsUtility.GetTerrain(cell, map).IsWater)
      return false;
    if (traverseParms.mode == 3 || traverseParms.mode == 6)
    {
      if (!this.pathGrid.WalkableFast(index))
      {
        Building edifice = GridsUtility.GetEdifice(cell, map);
        if (edifice == null || !VehiclePathFinder.IsDestroyable((Thing) edifice))
          return false;
      }
    }
    else if (traverseParms.mode != 5)
    {
      Log.ErrorOnce("Do not use this method for non-cell based modes!", 938476762);
      if (!this.pathGrid.WalkableFast(index))
        return false;
    }
    VehicleRegion vehicleRegion = this.regionGrid.DirectGrid[index];
    return vehicleRegion == null || vehicleRegion.Allows(traverseParms);
  }

  public bool CanReachBase(IntVec3 cell, VehicleDef vehicleDef)
  {
    TraverseParms traverseParms = TraverseParms.For((TraverseMode) 0, (Danger) 3, false, false, false, true, false);
    if (Current.ProgramState != 2)
      return this.CanReachVehicle(cell, LocalTargetInfo.op_Implicit(MapGenerator.PlayerStartSpot), (PathEndMode) 1, traverseParms);
    if (!cell.Walkable(vehicleDef, this.mapping))
      return false;
    Faction faction = this.mapping.map.ParentFaction ?? Faction.OfPlayer;
    foreach (Pawn pawn in this.mapping.map.mapPawns.SpawnedPawnsInFaction(faction))
    {
      if (ReachabilityUtility.CanReach(pawn, LocalTargetInfo.op_Implicit(cell), (PathEndMode) 1, (Danger) 3, false, false, (TraverseMode) 0))
        return true;
    }
    if (faction == Faction.OfPlayer)
    {
      foreach (Building building in this.mapping.map.listerBuildings.allBuildingsColonist)
      {
        if (this.CanReachVehicle(cell, LocalTargetInfo.op_Implicit((Thing) building), (PathEndMode) 2, traverseParms))
          return true;
      }
    }
    else
    {
      foreach (Thing thing in this.mapping.map.listerThings.ThingsInGroup((ThingRequestGroup) 10))
      {
        if (thing.Faction == faction && this.CanReachVehicle(cell, LocalTargetInfo.op_Implicit(thing), (PathEndMode) 2, traverseParms))
          return true;
      }
    }
    return this.CanReachBiggestMapEdgeRoom(cell);
  }

  public bool CanReachBiggestMapEdgeRoom(IntVec3 c)
  {
    VehicleRoom vehicleRoom = (VehicleRoom) null;
    foreach (VehicleRoom key in (IEnumerable<VehicleRoom>) this.regionGrid.allRooms.Keys)
    {
      if (key.TouchesMapEdge && (vehicleRoom == null || key.RegionCount > vehicleRoom.RegionCount))
        vehicleRoom = key;
    }
    return vehicleRoom != null && this.CanReachVehicle(c, LocalTargetInfo.op_Implicit(vehicleRoom.Regions.FirstOrDefault<KeyValuePair<VehicleRegion, byte>>().Key.AnyCell), (PathEndMode) 1, TraverseParms.For((TraverseMode) 1, (Danger) 3, false, false, false, true, false));
  }

  public bool CanReachMapEdge(IntVec3 cell, TraverseParms traverseParms)
  {
    if (traverseParms.pawn is VehiclePawn pawn)
    {
      if (!((Thing) pawn).Spawned)
        return false;
      if (((Thing) pawn).Map != this.mapping.map)
      {
        Log.Error($"Called CanReachMapEdge with vehicle not spawned on this map. Pawn's current map should have been used instead of this one. vehicle={pawn} vehicle.Map={((Thing) pawn).Map} map={this.mapping.map}");
        return false;
      }
    }
    VehicleRegion root = VehicleRegionAndRoomQuery.RegionAt(cell, this.mapping, this.createdFor);
    if (root == null)
      return false;
    if (root.Room.TouchesMapEdge)
      return true;
    bool foundReg = false;
    VehicleRegionTraverser.BreadthFirstTraverse(root, new VehicleRegionTraverser.VehicleRegionEntry(entryCondition), new VehicleRegionTraverser.VehicleRegionProcessor(regionProcessor), 9999);
    return foundReg;

    bool entryCondition(VehicleRegion from, VehicleRegion r) => r.Allows(traverseParms);

    bool regionProcessor(VehicleRegion r)
    {
      if (!r.Room.TouchesMapEdge)
        return false;
      foundReg = true;
      return true;
    }
  }

  public bool CanReachUnfogged(IntVec3 cell, TraverseParms traverseParms)
  {
    if (traverseParms.pawn != null)
    {
      if (!((Thing) traverseParms.pawn).Spawned)
        return false;
      if (((Thing) traverseParms.pawn).Map != this.mapping.map)
      {
        Log.Error($"Called CanReachUnfogged() with a pawn spawned not on this map. This means that we can't check his reachability here. Pawn's current map should have been used instead of this one. pawn={(object) traverseParms.pawn} pawn.Map={(object) ((Thing) traverseParms.pawn).Map} map={(object) this.mapping.map}");
        return false;
      }
    }
    if (!GenGrid.InBounds(cell, this.mapping.map))
      return false;
    if (!GridsUtility.Fogged(cell, this.mapping.map))
      return true;
    VehicleRegion root = VehicleRegionAndRoomQuery.RegionAt(cell, this.mapping, this.createdFor);
    if (root == null)
      return false;
    bool foundReg = false;
    VehicleRegionTraverser.BreadthFirstTraverse(root, new VehicleRegionTraverser.VehicleRegionEntry(entryCondition), new VehicleRegionTraverser.VehicleRegionProcessor(regionProcessor), 9999);
    return foundReg;

    bool entryCondition(VehicleRegion from, VehicleRegion r) => r.Allows(traverseParms);

    bool regionProcessor(VehicleRegion r)
    {
      if (GridsUtility.Fogged(r.AnyCell, this.mapping.map))
        return false;
      foundReg = true;
      return true;
    }
  }

  private bool CanUseCache(TraverseMode mode) => mode != 6 && mode != 5;

  private class AStar
  {
    private const int Status_Invalid = -1;
    private const int Status_Open = 0;
    private const int Status_Closed = 1;
    private const int Status_Starter = 2;
    private readonly PriorityQueue<VehicleReachability.AStar.Node, int> openQueue = new PriorityQueue<VehicleReachability.AStar.Node, int>();
    private readonly Dictionary<IntVec3, VehicleReachability.AStar.Node> nodes = new Dictionary<IntVec3, VehicleReachability.AStar.Node>();
    private readonly VehicleReachability vehicleReachability;
    private readonly VehiclePathingSystem mapping;
    private readonly VehicleDef vehicleDef;

    public AStar(
      VehicleReachability vehicleReachability,
      VehiclePathingSystem mapping,
      VehicleDef vehicleDef)
    {
      this.vehicleReachability = vehicleReachability;
      this.mapping = mapping;
      this.vehicleDef = vehicleDef;
    }

    public bool LogRetraceAttempts { get; set; } = true;

    public Map Map => this.mapping.map;

    public ChunkSet Run(
      IntVec3 start,
      LocalTargetInfo dest,
      TraverseParms traverseParms,
      bool debugDrawSearch = false)
    {
      VehicleRegion destinationRegion;
      VehicleRegion startingRegion;
      if (!this.InitRegions(start, dest, traverseParms, out startingRegion, out destinationRegion))
        return (ChunkSet) null;
      if (debugDrawSearch)
        UnityThread.ExecuteOnMainThread((Action) (() => VehicleReachability.MarkRegionForDrawing(startingRegion, this.Map)));
      VehicleRegionLink regionLink1 = (VehicleRegionLink) null;
      VehicleRegionLink regionLink2 = (VehicleRegionLink) null;
      float num1 = float.MaxValue;
      float num2 = float.MaxValue;
      using (ListSnapshot<VehicleRegionLink> links = startingRegion.Links)
      {
        foreach (VehicleRegionLink link in links)
        {
          float num3 = (float) VehicleRegion.EuclideanDistance(((LocalTargetInfo) ref dest).Cell, link);
          if ((double) num3 < (double) num1)
          {
            num1 = num3;
            num2 = num1;
            regionLink1 = link;
            regionLink2 = regionLink1;
          }
        }
      }
      VehicleRegion otherRegion1 = regionLink1.GetOtherRegion(startingRegion);
      VehicleReachability.AStar.Node node1 = this.CreateNode(((LocalTargetInfo) ref dest).Cell, (VehicleReachability.AStar.Node) null, regionLink1, startingRegion, otherRegion1, 0);
      node1.status = 2;
      this.nodes[regionLink1.anchor] = node1;
      this.openQueue.Enqueue(node1, node1.cost + node1.heuristicCost);
      if (regionLink2 != null)
      {
        VehicleRegion otherRegion2 = regionLink2.GetOtherRegion(startingRegion);
        VehicleReachability.AStar.Node node2 = this.CreateNode(((LocalTargetInfo) ref dest).Cell, (VehicleReachability.AStar.Node) null, regionLink2, startingRegion, otherRegion2, 1);
        node2.status = 2;
        this.nodes[regionLink2.anchor] = node2;
        this.openQueue.Enqueue(node2, node2.cost + node2.heuristicCost);
      }
      try
      {
        while (this.openQueue.Count > 0)
        {
          VehicleReachability.AStar.Node node3;
          int num4;
          if (!this.openQueue.TryDequeue(ref node3, ref num4))
          {
            Log.Error($"Failed to dequeue node. Count={this.openQueue.Count}");
            break;
          }
          if (node3.IsOpen || node3.status == 2)
          {
            foreach (VehicleRegionLink neighbor in this.Neighbors(node3))
            {
              VehicleReachability.AStar.Node node4 = this.GetNode(((LocalTargetInfo) ref dest).Cell, node3, neighbor);
              if (!node4.Passable || !node4.Allows(traverseParms))
                node4.status = -1;
              if (node4.IsOpen)
              {
                this.nodes[neighbor.anchor] = node4;
                node4.previous = node3;
                VehicleRegion otherRegion3;
                this.NodeRegions(node3, neighbor, out VehicleRegion _, out otherRegion3);
                if (otherRegion3 == destinationRegion)
                  return this.SolvePath(startingRegion, destinationRegion, node3);
                this.openQueue.Enqueue(node4, node4.cost + node4.heuristicCost);
                int num5 = debugDrawSearch ? 1 : 0;
              }
            }
            node3.status = 1;
          }
        }
        return (ChunkSet) null;
      }
      finally
      {
        this.CleanUp();
      }
    }

    private VehicleReachability.AStar.Node GetNode(
      IntVec3 dest,
      VehicleReachability.AStar.Node current,
      VehicleRegionLink neighbor)
    {
      VehicleReachability.AStar.Node node;
      if (this.nodes.TryGetValue(neighbor.anchor, out node))
        return node;
      VehicleRegion inFacingRegion;
      VehicleRegion otherRegion;
      this.NodeRegions(current, neighbor, out inFacingRegion, out otherRegion);
      float cost = inFacingRegion.WeightBetween(current.regionLink, neighbor);
      return this.CreateNode(dest, current, neighbor, inFacingRegion, otherRegion, (int) cost);
    }

    private VehicleReachability.AStar.Node CreateNode(
      IntVec3 dest,
      VehicleReachability.AStar.Node current,
      VehicleRegionLink regionLink,
      VehicleRegion regionA,
      VehicleRegion regionB,
      int cost)
    {
      return new VehicleReachability.AStar.Node(regionLink, regionA, regionB)
      {
        cost = cost,
        heuristicCost = VehicleRegion.EuclideanDistance(dest, regionLink)
      };
    }

    private IEnumerable<VehicleRegionLink> Neighbors(VehicleReachability.AStar.Node node)
    {
      ListSnapshot<VehicleRegionLink> linksA = node.regionA.Links;
      IEnumerator<VehicleRegionLink> enumerator = linksA.GetEnumerator();
      while (enumerator.MoveNext())
      {
        VehicleRegionLink current = enumerator.Current;
        VehicleReachability.AStar.Node node1;
        if (current != node.regionLink && (!this.nodes.TryGetValue(current.anchor, out node1) || node1.IsOpen))
          yield return current;
      }
      enumerator = (IEnumerator<VehicleRegionLink>) null;
      linksA = new ListSnapshot<VehicleRegionLink>();
      linksA = node.regionB.Links;
      try
      {
        enumerator = linksA.GetEnumerator();
        while (enumerator.MoveNext())
        {
          VehicleRegionLink current = enumerator.Current;
          VehicleReachability.AStar.Node node2;
          if (current != node.regionLink && (!this.nodes.TryGetValue(current.anchor, out node2) || node2.IsOpen))
            yield return current;
        }
        enumerator = (IEnumerator<VehicleRegionLink>) null;
      }
      finally
      {
        linksA.Dispose();
      }
      linksA = new ListSnapshot<VehicleRegionLink>();
    }

    private bool InitRegions(
      IntVec3 start,
      LocalTargetInfo dest,
      TraverseParms traverseParms,
      out VehicleRegion startingRegion,
      out VehicleRegion destinationRegion)
    {
      startingRegion = this.vehicleReachability.regionGrid.GetValidRegionAt(start);
      destinationRegion = (VehicleRegion) null;
      if (startingRegion == null)
      {
        Log.Error($"Unable to fetch valid starting region at {start}.");
        return false;
      }
      destinationRegion = VehicleRegionAndRoomQuery.RegionAt(((LocalTargetInfo) ref dest).Cell, this.mapping, this.vehicleReachability.createdFor);
      if (startingRegion == null || !destinationRegion.Allows(traverseParms))
      {
        Log.Error($"Unable to fetch valid starting region that allows traverseParms={traverseParms} at {start}.");
        return false;
      }
      return startingRegion != destinationRegion;
    }

    private void NodeRegions(
      VehicleReachability.AStar.Node current,
      VehicleRegionLink neighbor,
      out VehicleRegion inFacingRegion,
      out VehicleRegion otherRegion)
    {
      inFacingRegion = current.regionLink.GetInFacingRegion(neighbor);
      otherRegion = neighbor.GetOtherRegion(inFacingRegion);
    }

    private ChunkSet SolvePath(
      VehicleRegion start,
      VehicleRegion destination,
      VehicleReachability.AStar.Node finalNode)
    {
      HashSet<VehicleRegion> regions = new HashSet<VehicleRegion>();
      VehicleReachability.AStar.Node node = finalNode;
      regions.Add(destination);
      for (int index = 0; index < this.nodes.Count; ++index)
      {
        if (node == null || node == node.previous)
        {
          SmashLog.Error($"Unable to find hierarchal path from {start} to {destination}.  Couldn't backtrack {node} to starting node.");
          return (ChunkSet) null;
        }
        regions.Add(node.regionA);
        regions.Add(node.regionB);
        if (node.regionA == start || node.regionB == start)
          return new ChunkSet(regions);
        node = node.previous;
      }
      SmashLog.Error("Ran out of nodes to backtrace for solution.");
      return (ChunkSet) null;
    }

    private void CleanUp()
    {
      this.openQueue.Clear();
      this.nodes.Clear();
    }

    private class Node
    {
      public VehicleRegionLink regionLink;
      public VehicleReachability.AStar.Node previous;
      public VehicleRegion regionA;
      public VehicleRegion regionB;
      public int cost;
      public int heuristicCost;
      public int status;

      public Node(VehicleRegionLink regionLink, VehicleRegion regionA, VehicleRegion regionB)
      {
        this.regionLink = regionLink;
        this.regionA = regionA;
        this.regionB = regionB;
        this.cost = 0;
        this.heuristicCost = 0;
        this.status = 0;
      }

      public IntVec3 Pos => this.regionLink.anchor;

      public bool IsOpen => this.status == 0;

      public bool Passable
      {
        get
        {
          return this.regionA != null && RegionTypeUtility.Passable(this.regionA.type) && this.regionB != null && RegionTypeUtility.Passable(this.regionB.type);
        }
      }

      public bool Allows(TraverseParms traverseParms)
      {
        return this.regionA.Allows(traverseParms) && this.regionB.Allows(traverseParms);
      }

      public override string ToString() => this.Pos.ToString();

      public override int GetHashCode() => this.Pos.GetHashCode();
    }
  }
}
