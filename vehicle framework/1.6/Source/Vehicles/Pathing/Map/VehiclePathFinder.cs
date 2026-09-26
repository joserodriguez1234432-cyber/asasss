// Decompiled with JetBrains decompiler
// Type: Vehicles.VehiclePathFinder
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class VehiclePathFinder : VehicleGridManager
{
  private const float RoadCostMultiplier = 0.5f;
  private const float RoadAvoidalCost = 250f;
  private const float RoadHeuristicWeight = 0.15f;
  public const int DefaultMoveTicksCardinal = 13;
  public const int DefaultMoveTicksDiagonal = 18;
  private const int NodesToOpenBeforeRegionBasedPathing = 100000;
  private const int SearchLimit = 160000;
  private const int TurnCostTicks = 3;
  private const float RootPosWeight = 0.75f;
  private const int CostWallBlocker = 50;
  private readonly Map map;
  private readonly ObjectPool<VehiclePathFinder.PathFinderContext> contextPool;
  private VehiclePathGrid vehiclePathGrid;
  private readonly VehicleRegionCostCalculatorWrapper regionCostCalculator;
  private Area_Road roadGrid;
  private Area_RoadAvoidal roadAvoidalGrid;
  private readonly EdificeGrid edificeGrid;
  private readonly BlueprintGrid blueprintGrid;
  private readonly CellIndices cellIndices;
  private readonly List<int> disallowedCornerIndices = new List<int>(4);
  internal static readonly int[] neighborOffsets = new int[16 /*0x10*/]
  {
    0,
    1,
    0,
    -1,
    1,
    1,
    -1,
    -1,
    -1,
    0,
    1,
    0,
    -1,
    1,
    1,
    -1
  };
  private static readonly SimpleCurve nonRegionBasedHeuristicCurve;
  private static readonly SimpleCurve heuristicWeightByNodesOpened;
  private static readonly SimpleCurve regionHeuristicWeightByNodesOpened;

  public VehiclePathFinder(VehiclePathingSystem mapping, VehicleDef vehicleDef)
    : base(mapping, vehicleDef)
  {
    this.map = mapping.map;
    this.roadGrid = this.map.areaManager.Get<Area_Road>();
    this.roadAvoidalGrid = this.map.areaManager.Get<Area_RoadAvoidal>();
    this.edificeGrid = this.map.edificeGrid;
    this.blueprintGrid = this.map.blueprintGrid;
    this.cellIndices = this.map.cellIndices;
    this.contextPool = new ObjectPool<VehiclePathFinder.PathFinderContext>(10, 5);
    this.regionCostCalculator = new VehicleRegionCostCalculatorWrapper(mapping, vehicleDef);
  }

  public override void PostInit()
  {
    this.vehiclePathGrid = this.mapping[this.createdFor].VehiclePathGrid;
  }

  public VehiclePath FindPath(
    IntVec3 start,
    LocalTargetInfo dest,
    VehiclePawn vehicle,
    CancellationToken token,
    PathEndMode peMode = 1)
  {
    if (vehicle.DrivableRectOnCell(((LocalTargetInfo) ref dest).Cell, Ext_Vehicles.DestinationHitboxReq.AnyRotation))
      return this.FindPath(start, dest, TraverseParms.For((Pawn) vehicle, (Danger) 3, (TraverseMode) 0, false, false, false, true), token, peMode);
    Messages.Message(TaggedString.op_Implicit(Translator.Translate("VF_CannotFit")), MessageTypeDefOf.RejectInput, true);
    return VehiclePath.NotFound;
  }

  [Profile]
  public VehiclePath FindPath(
    IntVec3 start,
    LocalTargetInfo dest,
    TraverseParms traverseParms,
    CancellationToken token,
    PathEndMode peMode = 1)
  {
    if (DebugSettings.pathThroughWalls)
      traverseParms.mode = (TraverseMode) 3;
    VehiclePawn pawn = traverseParms.pawn as VehiclePawn;
    if (!this.ValidatePathRequest(start, dest, traverseParms, peMode))
      return VehiclePath.NotFound;
    int x1 = ((LocalTargetInfo) ref dest).Cell.x;
    int z1 = ((LocalTargetInfo) ref dest).Cell.z;
    int num1 = ((BuildableDef) this.createdFor).Size.x * ((BuildableDef) this.createdFor).Size.z;
    int index1 = ((CellIndices) ref this.cellIndices).CellToIndex(start);
    int index2 = ((CellIndices) ref this.cellIndices).CellToIndex(((LocalTargetInfo) ref dest).Cell);
    AvoidGrid avoidGrid;
    PawnUtility.TryGetAvoidGrid((Pawn) pawn, ref avoidGrid, true);
    VehiclePathFinder.PathFinderContext context = this.contextPool.Get();
    context.Init(this.mapping);
    if (this.roadGrid == null)
      this.roadGrid = this.map.areaManager.Get<Area_Road>();
    if (this.roadAvoidalGrid == null)
      this.roadAvoidalGrid = this.map.areaManager.Get<Area_RoadAvoidal>();
    int x2 = this.map.Size.x;
    TraverseMode mode = traverseParms.mode;
    bool flag1 = mode == 3 || mode == 6;
    bool flag2 = traverseParms.mode != 5 && traverseParms.mode != 6;
    CellRect destinationRect = VehiclePathFinder.CalculateDestinationRect(dest, peMode);
    bool flag3 = ((CellRect) ref destinationRect).Width == 1 && ((CellRect) ref destinationRect).Height == 1;
    int[] innerArray = this.vehiclePathGrid.innerArray;
    int num2 = 0;
    int num3 = 0;
    bool pathfinderSearch = VehicleMod.settings.debug.debugDrawPathfinderSearch;
    bool flag4 = ((flag1 ? 0 : (VehicleRegionAndRoomQuery.RegionAt(start, this.mapping, this.createdFor) != null ? 1 : 0)) & (flag2 ? 1 : 0)) != 0;
    bool usedRegionHeuristics = false;
    bool drafted = pawn.Drafted;
    float heuristicStrength = VehiclePathFinder.DetermineHeuristicStrength(start, dest);
    float ticksPerMoveCardinal = pawn.TicksPerMoveCardinal;
    float ticksPerMoveDiagonal = pawn.TicksPerMoveDiagonal;
    int num4 = VehicleMod.settings.main.fullVehiclePathing ? Mathf.Min(((BuildableDef) this.createdFor).Size.x, ((BuildableDef) this.createdFor).Size.z) : 1;
    context.InitStatusesAndPushStartNode(index1);
    while (context.openList.Count > 0)
    {
      if (token.IsCancellationRequested)
      {
        Debug.Message("Path request canceled. Exiting...");
        return VehiclePath.NotFound;
      }
      VehiclePathFinder.CostNode costNode = context.openList.Dequeue();
      int index3 = costNode.index;
      if (Mathf.Approximately(costNode.cost, context.calcGrid[index3].costNodeCost) && (int) context.calcGrid[index3].status != (int) context.statusClosedValue)
      {
        IntVec3 cell1 = ((CellIndices) ref this.cellIndices).IndexToCell(index3);
        int x3 = cell1.x;
        int z2 = cell1.z;
        if (pathfinderSearch)
        {
          float num5 = Mathf.Lerp(5000f, 15000f, (float) num1 / 15f);
          VehiclePathFinder.DebugFlash(this.mapping, cell1, context.calcGrid[index3].knownCost / num5, context.calcGrid[index3].knownCost.ToString("0"));
        }
        if (flag3 && index3 == index2 || !flag3 && ((CellRect) ref destinationRect).Contains(cell1) && !this.disallowedCornerIndices.Contains(index3))
          return this.FinalizedPath(context, index3, usedRegionHeuristics);
        if (num2 > 160000)
        {
          Log.Warning($"Vehicle {pawn} pathing from {start} to {dest} hit search limit of {160000}.");
          context.DebugDrawRichData();
          return VehiclePath.NotFound;
        }
label_81:
        for (int index4 = 0; index4 < 8; ++index4)
        {
          int num6 = x3 + VehiclePathFinder.neighborOffsets[index4];
          int num7 = z2 + VehiclePathFinder.neighborOffsets[index4 + 8];
          if (num6 >= 0 && num6 < this.map.Size.x && num7 >= 0 && num7 < this.map.Size.z)
          {
            int index5 = ((CellIndices) ref this.cellIndices).CellToIndex(num6, num7);
            IntVec3 intVec3;
            // ISSUE: explicit constructor call
            ((IntVec3) ref intVec3).\u002Ector(num6, 0, num7);
            Rot8 rot8 = Rot8.DirectionFromCells(cell1, intVec3);
            if ((int) context.calcGrid[index5].status != (int) context.statusClosedValue | usedRegionHeuristics)
            {
              int num8 = 0;
              if (!pawn.DrivableFast(index5))
              {
                if (!flag1)
                {
                  if (pathfinderSearch)
                  {
                    VehiclePathFinder.DebugFlash(this.mapping, intVec3, 0.22f, "impass");
                    continue;
                  }
                  continue;
                }
                int num9 = num8 + 70;
                Building building = this.edificeGrid[index5];
                if (building == null || !VehiclePathFinder.IsDestroyable((Thing) building))
                {
                  if (pathfinderSearch)
                  {
                    VehiclePathFinder.DebugFlash(this.mapping, intVec3, 0.22f, "impass");
                    continue;
                  }
                  continue;
                }
                num8 = num9 + (int) ((double) ((Thing) building).HitPoints * 0.20000000298023224);
              }
              if (index4 >= 4 && index4 <= 7)
              {
                int num10;
                switch (index4)
                {
                  case 4:
                  case 7:
                    num10 = index3 - x2;
                    break;
                  case 5:
                  case 6:
                    num10 = index3 + x2;
                    break;
                  default:
                    throw new InvalidOperationException();
                }
                int index6 = num10;
                int num11;
                switch (index4)
                {
                  case 4:
                  case 5:
                    num11 = index3 + 1;
                    break;
                  case 6:
                  case 7:
                    num11 = index3 - 1;
                    break;
                  default:
                    throw new InvalidOperationException();
                }
                int index7 = num11;
                if (VehiclePathFinder.BlocksDiagonalMovement(pawn, this.map, index6) || VehiclePathFinder.BlocksDiagonalMovement(pawn, this.map, index7))
                {
                  if (flag1)
                    num8 += 50;
                  else
                    continue;
                }
              }
              float num12 = (index4 <= 3 ? ticksPerMoveCardinal : ticksPerMoveDiagonal) + (float) num8;
              if (VehicleMod.settings.main.smoothVehiclePaths && (pawn.VehicleDef.size.x != 1 || pawn.VehicleDef.size.z != 1) && rot8 != costNode.direction)
              {
                int num13 = costNode.direction.Difference(rot8) * 3;
                num12 += (float) num13;
              }
              float num14 = 0.0f;
              float num15 = 0.0f;
              CellRect cellRect = pawn.VehicleRect(intVec3, (Rot4) rot8);
              foreach (IntVec3 cell2 in cellRect)
              {
                if (!pawn.Drivable(cell2))
                {
                  if (pathfinderSearch)
                  {
                    VehiclePathFinder.DebugFlash(this.mapping, cell2, 0.22f, "impass");
                    goto label_81;
                  }
                  goto label_81;
                }
                int index8 = ((CellIndices) ref this.cellIndices).CellToIndex(cell2);
                float num16 = 1f;
                float num17 = 0.0f;
                if (!FactionUtility.HostileTo(((Thing) pawn).Faction, Faction.OfPlayer))
                {
                  if (this.roadGrid[index8])
                    num16 = 0.5f;
                  else if (this.roadAvoidalGrid[index8])
                    num17 = 250f;
                }
                float num18 = (float) innerArray[index8] * num16 + num17;
                if (IntVec3.op_Equality(cell2, intVec3))
                  num15 = num18 * 0.75f;
                else
                  num14 += num18 * 0.25f;
              }
              if (num1 > 1)
                num12 += (float) Mathf.RoundToInt(num14 / (float) (num1 - 1));
              float num19 = num12 + (float) Mathf.RoundToInt(num15);
              if (avoidGrid != null)
                num19 += (float) ((int) avoidGrid.Grid[index5] * 8);
              if (!GenList.NullOrEmpty<Blueprint>((IList<Blueprint>) this.blueprintGrid.InnerArray[index5]))
                num19 += 1000f;
              float num20 = num19 + context.calcGrid[index3].knownCost;
              ushort status = context.calcGrid[index5].status;
              if ((int) status == (int) context.statusClosedValue || (int) status == (int) context.statusOpenValue)
              {
                float num21 = 0.0f;
                if ((int) status == (int) context.statusClosedValue)
                  num21 = ticksPerMoveCardinal;
                if ((double) context.calcGrid[index5].knownCost <= (double) num20 + (double) num21)
                  continue;
              }
              if (VehicleMod.settings.debug.debugDrawVehiclePathCosts)
                context.postCalculatedCells.Add((intVec3, num20));
              if (usedRegionHeuristics)
              {
                int num22 = Mathf.RoundToInt((float) this.regionCostCalculator.GetPathCostFromDestToRegion(index5));
                float num23 = VehiclePathFinder.regionHeuristicWeightByNodesOpened.Evaluate((float) num3);
                context.calcGrid[index5].heuristicCost = (float) num22 * num23;
                if ((double) context.calcGrid[index5].heuristicCost < 0.0)
                {
                  Log.ErrorOnce($"Heuristic cost overflow for vehicle {pawn} pathing from {start} to {dest}.", ((object) pawn).GetHashCode() ^ "FVPHeuristicCostOverflow".GetHashCode());
                  context.calcGrid[index5].heuristicCost = 0.0f;
                }
              }
              else if ((int) status != (int) context.statusClosedValue && (int) status != (int) context.statusOpenValue)
              {
                int num24 = GenMath.OctileDistance(Math.Abs(num6 - x1), Math.Abs(num7 - z1), Mathf.RoundToInt(ticksPerMoveCardinal), Mathf.RoundToInt(ticksPerMoveDiagonal));
                float num25 = VehiclePathFinder.heuristicWeightByNodesOpened.Evaluate((float) num3);
                float num26 = 1f;
                if (!FactionUtility.HostileTo(((Thing) pawn).Faction, Faction.OfPlayer) && this.roadGrid[index5])
                  num26 *= 0.15f;
                context.calcGrid[index5].heuristicCost = (float) Mathf.RoundToInt((float) num24 * heuristicStrength * num25) * num26;
              }
              float cost = num20 + context.calcGrid[index5].heuristicCost;
              if ((double) cost < 0.0)
              {
                Log.ErrorOnce($"Node cost overflow for vehicle {pawn} pathing from {start} to {dest}.", ((object) pawn).GetHashCode() ^ "FVPNodeCostOverflow".GetHashCode());
                cost = 0.0f;
              }
              context.calcGrid[index5].parentIndex = index3;
              context.calcGrid[index5].knownCost = num20;
              context.calcGrid[index5].status = context.statusOpenValue;
              context.calcGrid[index5].costNodeCost = cost;
              ++num3;
              context.openList.Enqueue(new VehiclePathFinder.CostNode(index5, cost, rot8), cost);
            }
          }
        }
        ++num2;
        context.calcGrid[index3].status = context.statusClosedValue;
        if (num3 >= 100000 & flag4 && !usedRegionHeuristics)
        {
          usedRegionHeuristics = true;
          this.regionCostCalculator.Init(destinationRect, traverseParms, ticksPerMoveCardinal, ticksPerMoveDiagonal, avoidGrid, drafted, this.disallowedCornerIndices);
          context.InitStatusesAndPushStartNode(index3);
          num3 = 0;
          num2 = 0;
        }
      }
    }
    string str1 = pawn.CurJob?.ToString() ?? "NULL";
    string str2 = ((Thing) pawn).Faction?.ToString() ?? "NULL";
    Log.Warning($"Vehicle {pawn} pathing from {start} to {dest} ran out of cells to process. Job={str1} Faction={str2}");
    context.DebugDrawRichData();
    return VehiclePath.NotFound;
  }

  private bool ValidatePathRequest(
    IntVec3 start,
    LocalTargetInfo dest,
    TraverseParms traverseParms,
    PathEndMode peMode = 1)
  {
    if (!(traverseParms.pawn is VehiclePawn pawn))
    {
      Log.Error("Tried to find Vehicle path for null vehicle.");
      return false;
    }
    if (((Thing) pawn).Map != this.map)
    {
      Log.Error($"Tried to FindVehiclePath for vehicle which is spawned in another map. Their map PathFinder should  have been used, not this one. vehicle={pawn} vehicle's map={((Thing) pawn).Map} map={this.map}");
      return false;
    }
    if (!((IntVec3) ref start).IsValid)
    {
      Log.Error($"Tried to FindVehiclePath with invalid start {start}. vehicle={pawn}");
      return false;
    }
    if (!((LocalTargetInfo) ref dest).IsValid)
    {
      Log.Error($"Tried to FindVehiclePath with invalid destination {dest}. vehicle={pawn}");
      return false;
    }
    if (traverseParms.mode != null || pawn.CanReachVehicle(dest, peMode, (Danger) 3, traverseParms.mode))
      return true;
    Log.Error("Trying to path to region not reachable, this should be blocked by reachability checks.");
    return false;
  }

  public static bool IsDestroyable(Thing thing) => thing.def.useHitPoints && thing.def.destroyable;

  public static bool BlocksDiagonalMovement(Map map, VehicleDef vehicleDef, int x, int z)
  {
    return VehiclePathFinder.BlocksDiagonalMovement(map, vehicleDef, ((CellIndices) ref map.cellIndices).CellToIndex(x, z));
  }

  public static bool BlocksDiagonalMovement(Map map, VehicleDef vehicleDef, int index)
  {
    return map.GetCachedMapComponent<VehiclePathingSystem>()[vehicleDef].VehiclePathGrid.WalkableFast(index) || map.edificeGrid[index] is Building_Door;
  }

  public static bool BlocksDiagonalMovement(VehiclePawn vehicle, int x, int z)
  {
    return VehiclePathFinder.BlocksDiagonalMovement(vehicle, ((Thing) vehicle).Map, ((CellIndices) ref ((Thing) vehicle).Map.cellIndices).CellToIndex(x, z));
  }

  private static bool BlocksDiagonalMovement(VehiclePawn vehicle, Map map, int index)
  {
    return !vehicle.DrivableFast(index) || map.edificeGrid[index] is Building_Door;
  }

  private static void DebugFlash(
    VehiclePathingSystem mapping,
    IntVec3 cell,
    float colorPct,
    string label)
  {
    if (!GenGrid.InBounds(cell, mapping.map))
      return;
    VehiclePathFinder.DebugFlash(cell, mapping.map, colorPct, label);
  }

  private static void DebugFlash(
    IntVec3 cell,
    Map map,
    float colorPct,
    string label,
    int duration = 50)
  {
    map.DrawCell_ThreadSafe(cell, colorPct, label, duration);
  }

  private VehiclePath FinalizedPath(
    VehiclePathFinder.PathFinderContext context,
    int finalIndex,
    bool usedRegionHeuristics)
  {
    context.DebugDrawPathCost();
    VehiclePath vehiclePath = AsyncPool<VehiclePath>.Get();
    int index = finalIndex;
    while (true)
    {
      int parentIndex = context.calcGrid[index].parentIndex;
      IntVec3 cell = ((CellIndices) ref this.mapping.map.cellIndices).IndexToCell(index);
      vehiclePath.AddNode(cell);
      if (index != parentIndex)
        index = parentIndex;
      else
        break;
    }
    vehiclePath.Init(usedRegionHeuristics);
    return vehiclePath;
  }

  private static float DetermineHeuristicStrength(IntVec3 start, LocalTargetInfo dest)
  {
    IntVec3 intVec3 = IntVec3.op_Subtraction(start, ((LocalTargetInfo) ref dest).Cell);
    float lengthHorizontal = ((IntVec3) ref intVec3).LengthHorizontal;
    return (float) Mathf.RoundToInt(VehiclePathFinder.nonRegionBasedHeuristicCurve.Evaluate(lengthHorizontal));
  }

  private static CellRect CalculateDestinationRect(LocalTargetInfo dest, PathEndMode peMode)
  {
    CellRect cellRect = !((LocalTargetInfo) ref dest).HasThing || peMode == 1 ? CellRect.SingleCell(((LocalTargetInfo) ref dest).Cell) : GenAdj.OccupiedRect(((LocalTargetInfo) ref dest).Thing);
    return peMode == 2 ? ((CellRect) ref cellRect).ExpandedBy(1) : cellRect;
  }

  static VehiclePathFinder()
  {
    SimpleCurve simpleCurve1 = new SimpleCurve();
    simpleCurve1.Add(new CurvePoint(50f, 1f), true);
    simpleCurve1.Add(new CurvePoint(120f, 2f), true);
    VehiclePathFinder.nonRegionBasedHeuristicCurve = simpleCurve1;
    SimpleCurve simpleCurve2 = new SimpleCurve();
    simpleCurve2.Add(new CurvePoint(0.0f, 0.0f), true);
    simpleCurve2.Add(new CurvePoint(25f, 0.0f), true);
    simpleCurve2.Add(new CurvePoint(50f, 0.5f), true);
    simpleCurve2.Add(new CurvePoint(150f, 1f), true);
    VehiclePathFinder.heuristicWeightByNodesOpened = simpleCurve2;
    SimpleCurve simpleCurve3 = new SimpleCurve();
    simpleCurve3.Add(new CurvePoint(0.0f, 0.0f), true);
    simpleCurve3.Add(new CurvePoint(250f, 0.0f), true);
    simpleCurve3.Add(new CurvePoint(3500f, 1f), true);
    simpleCurve3.Add(new CurvePoint(4500f, 5f), true);
    simpleCurve3.Add(new CurvePoint(30000f, 50f), true);
    simpleCurve3.Add(new CurvePoint(100000f, 500f), true);
    VehiclePathFinder.regionHeuristicWeightByNodesOpened = simpleCurve3;
  }

  private struct CostNode(int index, float cost, Rot8 direction)
  {
    public readonly int index = index;
    public readonly float cost = cost;
    public Rot8 direction = direction;
  }

  private struct VehiclePathFinderNodeFast
  {
    public float knownCost;
    public float heuristicCost;
    public int parentIndex;
    public float costNodeCost;
    public ushort status;
  }

  private class PathFinderContext : IPoolable
  {
    public readonly List<(IntVec3, float)> postCalculatedCells = new List<(IntVec3, float)>();
    private VehiclePathingSystem mapping;
    public PriorityQueue<VehiclePathFinder.CostNode, float> openList;
    public VehiclePathFinder.VehiclePathFinderNodeFast[] calcGrid;
    public ushort statusOpenValue = 1;
    public ushort statusClosedValue = 2;

    bool IPoolable.InPool { get; set; }

    public void Init(VehiclePathingSystem mapping)
    {
      if (this.mapping != null)
        return;
      this.mapping = mapping;
      this.calcGrid = new VehiclePathFinder.VehiclePathFinderNodeFast[mapping.map.Size.x * mapping.map.Size.z];
      this.openList = new PriorityQueue<VehiclePathFinder.CostNode, float>();
    }

    void IPoolable.Reset()
    {
      this.openList?.Clear();
      this.postCalculatedCells.Clear();
    }

    public void InitStatusesAndPushStartNode(int startIndex)
    {
      this.statusOpenValue += (ushort) 2;
      this.statusClosedValue += (ushort) 2;
      if (this.statusClosedValue >= (ushort) 65435)
        this.ResetStatuses();
      this.calcGrid[startIndex].knownCost = 0.0f;
      this.calcGrid[startIndex].heuristicCost = 0.0f;
      this.calcGrid[startIndex].costNodeCost = 0.0f;
      this.calcGrid[startIndex].parentIndex = startIndex;
      this.calcGrid[startIndex].status = this.statusOpenValue;
      this.openList.Clear();
      this.openList.Enqueue(new VehiclePathFinder.CostNode(startIndex, 0.0f, Rot8.Invalid), 0.0f);
    }

    private void ResetStatuses()
    {
      for (int index = 0; index < this.calcGrid.Length; ++index)
        this.calcGrid[index].status = (ushort) 0;
      this.statusOpenValue = (ushort) 1;
      this.statusClosedValue = (ushort) 2;
    }

    public void DebugDrawRichData()
    {
      if (!VehicleMod.settings.debug.debugDrawVehiclePathCosts)
        return;
      int x = this.mapping.map.Size.x;
      int z = this.mapping.map.Size.z;
      while (this.openList.Count > 0)
      {
        int index = this.openList.Dequeue().index;
        IntVec3 cell;
        // ISSUE: explicit constructor call
        ((IntVec3) ref cell).\u002Ector(index % x, 0, index / z);
        VehiclePathFinder.DebugFlash(this.mapping, cell, 0.0f, "open");
      }
    }

    public void DebugDrawPathCost(float colorPct = 0.0f, int duration = 50)
    {
      if (!VehicleMod.settings.debug.debugDrawVehiclePathCosts)
        return;
      foreach ((IntVec3 cell, float num) in this.postCalculatedCells)
        VehiclePathFinder.DebugFlash(cell, this.mapping.map, colorPct, num.ToString(), duration);
    }
  }
}
