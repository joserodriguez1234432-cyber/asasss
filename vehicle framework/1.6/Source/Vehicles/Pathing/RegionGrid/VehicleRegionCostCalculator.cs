// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleRegionCostCalculator
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class VehicleRegionCostCalculator
{
  private const int SampleCount = 11;
  private static readonly int DefaultTicksPerMoveCardinal = Mathf.RoundToInt(13.333333f);
  private static readonly int DefaultTicksPerMoveDiagonal = Mathf.RoundToInt((float) VehicleRegionCostCalculator.DefaultTicksPerMoveCardinal * Ext_Math.Sqrt2);
  private static int[] pathCostSamples = new int[11];
  private static readonly List<int> tmpCellIndices = new List<int>();
  private static readonly List<int> tmpPathableNeighborIndices = new List<int>();
  private static readonly Dictionary<int, float> tmpDistances = new Dictionary<int, float>();
  private readonly VehiclePathingSystem mapping;
  private readonly VehicleDef vehicleDef;
  private AvoidGrid avoidGrid;
  private TraverseParms traverseParms;
  private IntVec3 destinationCell;
  private float moveTicksCardinal;
  private float moveTicksDiagonal;
  private bool drafted;
  private Func<int, int, float> preciseRegionLinkDistancesDistanceGetter;
  private readonly Dictionary<int, VehicleRegionLink> regionMinLink = new Dictionary<int, VehicleRegionLink>();
  private readonly Dictionary<VehicleRegionLink, int> distances = new Dictionary<VehicleRegionLink, int>();
  private readonly Dictionary<VehicleRegionLink, IntVec3> linkTargetCells = new Dictionary<VehicleRegionLink, IntVec3>();
  private readonly Dictionary<VehicleRegion, int> minPathCosts = new Dictionary<VehicleRegion, int>();
  private readonly FastPriorityQueue<VehicleRegionCostCalculator.RegionLinkQueueEntry> queue = new FastPriorityQueue<VehicleRegionCostCalculator.RegionLinkQueueEntry>((IComparer<VehicleRegionCostCalculator.RegionLinkQueueEntry>) new VehicleRegionCostCalculator.DistanceComparer());
  private readonly List<Pair<VehicleRegionLink, int>> preciseRegionLinkDistances = new List<Pair<VehicleRegionLink, int>>();

  public VehicleRegionCostCalculator(VehiclePathingSystem mapping, VehicleDef vehicleDef)
  {
    this.mapping = mapping;
    this.vehicleDef = vehicleDef;
    this.preciseRegionLinkDistancesDistanceGetter = new Func<int, int, float>(this.PreciseRegionLinkDistancesDistanceGetter);
  }

  public void Init(
    CellRect destination,
    HashSet<VehicleRegion> destRegions,
    TraverseParms parms,
    float moveTicksCardinal,
    float moveTicksDiagonal,
    AvoidGrid avoidGrid,
    bool drafted)
  {
    this.traverseParms = parms;
    this.destinationCell = ((CellRect) ref destination).CenterCell;
    this.moveTicksCardinal = moveTicksCardinal;
    this.moveTicksDiagonal = moveTicksDiagonal;
    this.avoidGrid = avoidGrid;
    this.drafted = drafted;
    this.regionMinLink.Clear();
    this.distances.Clear();
    this.linkTargetCells.Clear();
    this.queue.Clear();
    this.minPathCosts.Clear();
    foreach (VehicleRegion destRegion in destRegions)
    {
      int minPathCost = this.RegionMedianPathCost(destRegion);
      using (ListSnapshot<VehicleRegionLink> links = destRegion.Links)
      {
        foreach (VehicleRegionLink vehicleRegionLink in links)
        {
          if (vehicleRegionLink.GetOtherRegion(destRegion).Allows(this.traverseParms))
          {
            int val2 = this.RegionLinkDistance(this.destinationCell, vehicleRegionLink, minPathCost);
            int val1;
            if (this.distances.TryGetValue(vehicleRegionLink, out val1))
            {
              if (val2 < val1)
                this.linkTargetCells[vehicleRegionLink] = VehicleRegionCostCalculator.GetLinkTargetCell(this.destinationCell, vehicleRegionLink);
              val2 = Math.Min(val1, val2);
            }
            else
              this.linkTargetCells[vehicleRegionLink] = VehicleRegionCostCalculator.GetLinkTargetCell(this.destinationCell, vehicleRegionLink);
            this.distances[vehicleRegionLink] = val2;
          }
        }
        this.GetPreciseRegionLinkDistances(destRegion, destination, this.preciseRegionLinkDistances);
        for (int index = 0; index < this.preciseRegionLinkDistances.Count; ++index)
        {
          Pair<VehicleRegionLink, int> regionLinkDistance = this.preciseRegionLinkDistances[index];
          VehicleRegionLink first = regionLinkDistance.First;
          int distance = this.distances[first];
          int num;
          if (regionLinkDistance.Second > distance)
          {
            this.distances[first] = regionLinkDistance.Second;
            num = regionLinkDistance.Second;
          }
          else
            num = distance;
          this.queue.Push(new VehicleRegionCostCalculator.RegionLinkQueueEntry(destRegion, first, num, num));
        }
      }
    }
  }

  public int GetRegionDistance(VehicleRegion region, out VehicleRegionLink minLink)
  {
    if (this.regionMinLink.TryGetValue(region.Id, out minLink))
      return this.distances[minLink];
    while (this.queue.Count != 0)
    {
      VehicleRegionCostCalculator.RegionLinkQueueEntry regionLinkQueueEntry = this.queue.Pop();
      int distance = this.distances[regionLinkQueueEntry.Link];
      if (regionLinkQueueEntry.Cost == distance)
      {
        VehicleRegion otherRegion = regionLinkQueueEntry.Link.GetOtherRegion(regionLinkQueueEntry.From);
        if (otherRegion != null && otherRegion.valid)
        {
          int minPathCost = this.RegionMedianPathCost(otherRegion);
          using (ListSnapshot<VehicleRegionLink> links = otherRegion.Links)
          {
            foreach (VehicleRegionLink vehicleRegionLink in links)
            {
              if (vehicleRegionLink != regionLinkQueueEntry.Link && RegionTypeUtility.Passable(vehicleRegionLink.GetOtherRegion(otherRegion).type))
              {
                int num1 = Math.Max(this.RegionLinkDistance(regionLinkQueueEntry.Link, vehicleRegionLink, minPathCost), 1);
                int cost = distance + num1;
                int estimatedPathCost = this.MinimumRegionLinkDistance(this.destinationCell, vehicleRegionLink) + cost;
                int num2;
                if (this.distances.TryGetValue(vehicleRegionLink, out num2))
                {
                  if (cost < num2)
                  {
                    this.distances[vehicleRegionLink] = cost;
                    this.queue.Push(new VehicleRegionCostCalculator.RegionLinkQueueEntry(otherRegion, vehicleRegionLink, cost, estimatedPathCost));
                  }
                }
                else
                {
                  this.distances.Add(vehicleRegionLink, cost);
                  this.queue.Push(new VehicleRegionCostCalculator.RegionLinkQueueEntry(otherRegion, vehicleRegionLink, cost, estimatedPathCost));
                }
              }
            }
            if (!this.regionMinLink.ContainsKey(otherRegion.Id))
            {
              this.regionMinLink.Add(otherRegion.Id, regionLinkQueueEntry.Link);
              if (otherRegion == region)
              {
                minLink = regionLinkQueueEntry.Link;
                return regionLinkQueueEntry.Cost;
              }
            }
          }
        }
      }
    }
    return 10000;
  }

  public int GetRegionBestDistances(
    VehicleRegion region,
    out VehicleRegionLink bestLink,
    out VehicleRegionLink secondBestLink,
    out int secondBestCost)
  {
    int regionDistance = this.GetRegionDistance(region, out bestLink);
    secondBestLink = (VehicleRegionLink) null;
    secondBestCost = int.MaxValue;
    using (ListSnapshot<VehicleRegionLink> links = region.Links)
    {
      foreach (VehicleRegionLink key in links)
      {
        int num;
        if (key != bestLink && RegionTypeUtility.Passable(key.GetOtherRegion(region).type) && this.distances.TryGetValue(key, out num) && num < secondBestCost)
        {
          secondBestCost = num;
          secondBestLink = key;
        }
      }
      return regionDistance;
    }
  }

  public int RegionMedianPathCost(VehicleRegion region)
  {
    int num;
    if (this.minPathCosts.TryGetValue(region, out num))
      return num;
    CellIndices cellIndices = this.mapping.map.cellIndices;
    Rand.PushState();
    Rand.Seed = ((CellIndices) ref cellIndices).CellToIndex(((CellRect) ref region.extentsClose).CenterCell) * (region.LinksCount + 1);
    for (int index = 0; index < 11; ++index)
      VehicleRegionCostCalculator.pathCostSamples[index] = this.GetCellCostFast(((CellIndices) ref cellIndices).CellToIndex(region.RandomCell));
    Rand.PopState();
    Array.Sort<int>(VehicleRegionCostCalculator.pathCostSamples);
    int pathCostSample = VehicleRegionCostCalculator.pathCostSamples[4];
    this.minPathCosts[region] = pathCostSample;
    return pathCostSample;
  }

  private int GetCellCostFast(int index)
  {
    int inner = this.mapping[this.vehicleDef].VehiclePathGrid.innerArray[index];
    if (this.avoidGrid != null)
      inner += (int) this.avoidGrid.Grid[index] * 8;
    return inner + (this.drafted ? this.mapping.map.terrainGrid.topGrid[index].extraDraftedPerceivedPathCost : this.mapping.map.terrainGrid.topGrid[index].extraNonDraftedPerceivedPathCost);
  }

  private int RegionLinkDistance(VehicleRegionLink a, VehicleRegionLink b, int minPathCost)
  {
    IntVec3 intVec3 = IntVec3.op_Subtraction(!this.linkTargetCells.ContainsKey(a) ? VehicleRegionCostCalculator.RegionLinkCenter(a) : this.linkTargetCells[a], !this.linkTargetCells.ContainsKey(b) ? VehicleRegionCostCalculator.RegionLinkCenter(b) : this.linkTargetCells[b]);
    int num1 = Math.Abs(intVec3.x);
    int num2 = Math.Abs(intVec3.z);
    return VehicleRegionCostCalculator.OctileDistance(num1, num2, Mathf.RoundToInt(this.moveTicksCardinal), Mathf.RoundToInt(this.moveTicksDiagonal)) + minPathCost * Math.Max(num1, num2) + minPathCost * Math.Min(num1, num2);
  }

  public int RegionLinkDistance(IntVec3 cell, VehicleRegionLink link, int minPathCost)
  {
    IntVec3 linkTargetCell = VehicleRegionCostCalculator.GetLinkTargetCell(cell, link);
    IntVec3 intVec3 = IntVec3.op_Subtraction(cell, linkTargetCell);
    int num1 = Math.Abs(intVec3.x);
    int num2 = Math.Abs(intVec3.z);
    return VehicleRegionCostCalculator.OctileDistance(num1, num2, Mathf.RoundToInt(this.moveTicksCardinal), Mathf.RoundToInt(this.moveTicksDiagonal)) + minPathCost * Math.Max(num1, num2) + minPathCost * Math.Min(num1, num2);
  }

  private static int SpanCenterX(EdgeSpan e) => e.root.x + (e.dir != 1 ? 0 : e.length / 2);

  private static int SpanCenterZ(EdgeSpan e) => e.root.z + (e.dir != null ? 0 : e.length / 2);

  public static IntVec3 RegionLinkCenter(VehicleRegionLink link)
  {
    return new IntVec3(VehicleRegionCostCalculator.SpanCenterX(link.span), 0, VehicleRegionCostCalculator.SpanCenterZ(link.span));
  }

  private int MinimumRegionLinkDistance(IntVec3 cell, VehicleRegionLink link)
  {
    IntVec3 intVec3 = IntVec3.op_Subtraction(cell, VehicleRegionCostCalculator.LinkClosestCell(cell, link));
    return VehicleRegionCostCalculator.OctileDistance(Math.Abs(intVec3.x), Math.Abs(intVec3.z), Mathf.RoundToInt(this.moveTicksCardinal), Mathf.RoundToInt(this.moveTicksDiagonal));
  }

  internal static int OctileDistance(int dx, int dz, int moveTicksCardinal = -1, int moveTicksDiagonal = -1)
  {
    if (moveTicksCardinal < 0)
      moveTicksCardinal = VehicleRegionCostCalculator.DefaultTicksPerMoveCardinal;
    if (moveTicksDiagonal < 0)
      moveTicksDiagonal = VehicleRegionCostCalculator.DefaultTicksPerMoveDiagonal;
    return GenMath.OctileDistance(dx, dz, moveTicksCardinal, moveTicksDiagonal);
  }

  private static IntVec3 GetLinkTargetCell(IntVec3 cell, VehicleRegionLink link)
  {
    return VehicleRegionCostCalculator.LinkClosestCell(cell, link);
  }

  private static IntVec3 LinkClosestCell(IntVec3 cell, VehicleRegionLink link)
  {
    EdgeSpan span = link.span;
    int num1 = 0;
    int num2 = 0;
    if (span.dir == null)
      num2 = span.length - 1;
    else
      num1 = span.length - 1;
    IntVec3 root = span.root;
    return new IntVec3(Mathf.Clamp(cell.x, root.x, root.x + num1), 0, Mathf.Clamp(cell.z, root.z, root.z + num2));
  }

  private void GetPreciseRegionLinkDistances(
    VehicleRegion region,
    CellRect destination,
    List<Pair<VehicleRegionLink, int>> outDistances)
  {
    outDistances.Clear();
    VehicleRegionCostCalculator.tmpCellIndices.Clear();
    if (((CellRect) ref destination).Width == 1 && ((CellRect) ref destination).Height == 1)
    {
      VehicleRegionCostCalculator.tmpCellIndices.Add(((CellIndices) ref this.mapping.map.cellIndices).CellToIndex(((CellRect) ref destination).CenterCell));
    }
    else
    {
      foreach (IntVec3 intVec3 in destination)
      {
        if (GenGrid.InBounds(intVec3, this.mapping.map))
          VehicleRegionCostCalculator.tmpCellIndices.Add(((CellIndices) ref this.mapping.map.cellIndices).CellToIndex(intVec3));
      }
    }
    Dijkstra<int>.Run((IEnumerable<int>) VehicleRegionCostCalculator.tmpCellIndices, (Func<int, IEnumerable<int>>) (x => this.PreciseRegionLinkDistancesNeighborsGetter(x, region)), this.preciseRegionLinkDistancesDistanceGetter, VehicleRegionCostCalculator.tmpDistances, (Dictionary<int, int>) null);
    using (ListSnapshot<VehicleRegionLink> links = region.Links)
    {
      foreach (VehicleRegionLink key in links)
      {
        if (key.GetOtherRegion(region).Allows(this.traverseParms))
        {
          float num;
          if (!VehicleRegionCostCalculator.tmpDistances.TryGetValue(((CellIndices) ref this.mapping.map.cellIndices).CellToIndex(this.linkTargetCells[key]), out num))
          {
            Log.ErrorOnce("Dijkstra couldn't reach one of the cells even though they are in the same region. There is most likely something wrong with the neighbor nodes getter.", ((object) this.vehicleDef).GetHashCode() ^ "VehiclesDijkstraRegionLinkDistanceCalculator".GetHashCode());
            num = 100f;
          }
          outDistances.Add(new Pair<VehicleRegionLink, int>(key, (int) num));
        }
      }
    }
  }

  private IEnumerable<int> PreciseRegionLinkDistancesNeighborsGetter(int node, VehicleRegion region)
  {
    VehicleRegion[] directGrid = this.mapping[this.vehicleDef].VehicleRegionGrid.DirectGrid;
    return directGrid == null || directGrid[node] == null || directGrid[node] != region ? (IEnumerable<int>) null : (IEnumerable<int>) this.PathableNeighborIndices(node);
  }

  private float PreciseRegionLinkDistancesDistanceGetter(int a, int b)
  {
    float num = !this.AreCellsDiagonal(a, b) ? this.moveTicksCardinal : this.moveTicksDiagonal;
    return (float) this.GetCellCostFast(b) + num;
  }

  private bool AreCellsDiagonal(int a, int b)
  {
    int x = this.mapping.map.Size.x;
    return a % x != b % x && a / x != b / x;
  }

  private List<int> PathableNeighborIndices(int index)
  {
    VehicleRegionCostCalculator.tmpPathableNeighborIndices.Clear();
    VehiclePathGrid vehiclePathGrid = this.mapping[this.vehicleDef].VehiclePathGrid;
    int x = this.mapping.map.Size.x;
    bool flag1 = index % x > 0;
    bool flag2 = index % x < x - 1;
    bool flag3 = index >= x;
    bool flag4 = index / x < this.mapping.map.Size.z - 1;
    if (flag3 && vehiclePathGrid.WalkableFast(index - x))
      VehicleRegionCostCalculator.tmpPathableNeighborIndices.Add(index - x);
    if (flag2 && vehiclePathGrid.WalkableFast(index + 1))
      VehicleRegionCostCalculator.tmpPathableNeighborIndices.Add(index + 1);
    if (flag1 && vehiclePathGrid.WalkableFast(index - 1))
      VehicleRegionCostCalculator.tmpPathableNeighborIndices.Add(index - 1);
    if (flag4 && vehiclePathGrid.WalkableFast(index + x))
      VehicleRegionCostCalculator.tmpPathableNeighborIndices.Add(index + x);
    bool flag5 = !flag1 || VehiclePathFinder.BlocksDiagonalMovement(this.mapping.map, this.vehicleDef, index - 1);
    bool flag6 = !flag2 || VehiclePathFinder.BlocksDiagonalMovement(this.mapping.map, this.vehicleDef, index + 1);
    if (flag3 && !VehiclePathFinder.BlocksDiagonalMovement(this.mapping.map, this.vehicleDef, index - x))
    {
      if (!flag6 && vehiclePathGrid.WalkableFast(index - x + 1))
        VehicleRegionCostCalculator.tmpPathableNeighborIndices.Add(index - x + 1);
      if (!flag5 && vehiclePathGrid.WalkableFast(index - x - 1))
        VehicleRegionCostCalculator.tmpPathableNeighborIndices.Add(index - x - 1);
    }
    if (flag4 && !VehiclePathFinder.BlocksDiagonalMovement(this.mapping.map, this.vehicleDef, index + x))
    {
      if (!flag6 && vehiclePathGrid.WalkableFast(index + x + 1))
        VehicleRegionCostCalculator.tmpPathableNeighborIndices.Add(index + x + 1);
      if (!flag5 && vehiclePathGrid.WalkableFast(index + x - 1))
        VehicleRegionCostCalculator.tmpPathableNeighborIndices.Add(index + x - 1);
    }
    return VehicleRegionCostCalculator.tmpPathableNeighborIndices;
  }

  private struct RegionLinkQueueEntry(
    VehicleRegion from,
    VehicleRegionLink link,
    int cost,
    int estimatedPathCost)
  {
    private readonly VehicleRegion from = from;
    private readonly VehicleRegionLink link = link;
    private readonly int cost = cost;
    private readonly int estimatedPathCost = estimatedPathCost;

    public VehicleRegion From => this.from;

    public VehicleRegionLink Link => this.link;

    public int Cost => this.cost;

    public int EstimatedPathCost => this.estimatedPathCost;
  }

  private class DistanceComparer : IComparer<VehicleRegionCostCalculator.RegionLinkQueueEntry>
  {
    public int Compare(
      VehicleRegionCostCalculator.RegionLinkQueueEntry a,
      VehicleRegionCostCalculator.RegionLinkQueueEntry b)
    {
      return a.EstimatedPathCost.CompareTo(b.EstimatedPathCost);
    }
  }
}
