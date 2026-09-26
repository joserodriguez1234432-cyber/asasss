// Decompiled with JetBrains decompiler
// Type: Vehicles.World.WorldVehiclePathfinder
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.World;

public class WorldVehiclePathfinder : WorldComponent
{
  private const int SearchLimit = 500000;
  private const int HeuristicTickCost = 1200;
  private static readonly SimpleCurve HeuristicWeights;
  private readonly FastPriorityQueue<WorldVehiclePathfinder.CostNode> openList;
  private readonly WorldVehiclePathfinder.PathFinderNodeFast[] calcGrid;
  private readonly float[] tileCache;
  private readonly WorldVehiclePathfinder.TileFeatureLookup tileFeatureLookup;
  private ushort statusOpenValue = 1;
  private ushort statusClosedValue = 2;

  public WorldVehiclePathfinder(RimWorld.Planet.World world)
    : base(world)
  {
    this.world = world;
    this.calcGrid = new WorldVehiclePathfinder.PathFinderNodeFast[Find.WorldGrid.TilesCount];
    this.openList = new FastPriorityQueue<WorldVehiclePathfinder.CostNode>((IComparer<WorldVehiclePathfinder.CostNode>) new WorldVehiclePathfinder.CostNodeComparer());
    this.tileFeatureLookup = new WorldVehiclePathfinder.TileFeatureLookup(Find.WorldGrid);
    this.tileFeatureLookup.RegisterAllFeatureTypes();
    this.tileCache = new float[this.tileFeatureLookup.TileCacheSize];
    WorldVehiclePathfinder.Instance = this;
  }

  public static WorldVehiclePathfinder Instance { get; private set; }

  private void ClearTileCache() => Array.Clear((Array) this.tileCache, 0, this.tileCache.Length);

  private float TileTypeCost(int tile, List<VehicleDef> vehicleDefs)
  {
    float num = this.tileCache[this.tileFeatureLookup.IndexFor(PlanetTile.op_Implicit(tile))];
    if ((double) num <= 0.0)
    {
      num = GenList.NullOrEmpty<VehicleDef>((IList<VehicleDef>) vehicleDefs) ? -1f : vehicleDefs.Max<VehicleDef>((Func<VehicleDef, float>) (vehicleDef => WorldVehiclePathGrid.Instance.pathGrids[vehicleDef.DefIndex][tile]));
      this.tileCache[this.tileFeatureLookup.IndexFor(PlanetTile.op_Implicit(tile))] = num;
    }
    return num;
  }

  public WorldPath FindPath(
    PlanetTile startTile,
    PlanetTile destTile,
    VehicleCaravan caravan,
    Func<float, bool> terminator = null)
  {
    return this.FindPath(startTile, destTile, caravan.VehiclesListForReading, caravan.TicksPerMove, terminator);
  }

  public WorldPath FindPath(
    PlanetTile startTile,
    PlanetTile destTile,
    List<VehiclePawn> vehicles,
    int ticksPerMove = 3300,
    Func<float, bool> terminator = null)
  {
    return this.FindPath(startTile, destTile, vehicles.UniqueVehicleDefsInList(), ticksPerMove, terminator);
  }

  public WorldPath FindPath(
    PlanetTile startTile,
    PlanetTile destTile,
    List<VehicleDef> vehicleDefs,
    int ticksPerMove = 3300,
    Func<float, bool> terminator = null)
  {
    if (GenList.NullOrEmpty<VehicleDef>((IList<VehicleDef>) vehicleDefs))
    {
      Log.Error("Attempting to find path with no vehicles.");
      return WorldPath.NotFound;
    }
    this.ClearTileCache();
    try
    {
      string str = string.Join(",", vehicleDefs.Select<VehicleDef, string>((Func<VehicleDef, string>) (v => ((Def) v).defName)));
      if (PlanetTile.op_Implicit(startTile) < 0)
      {
        Log.Error($"Tried to FindPath with invalid startTile={startTile} vehicles={str}");
        return WorldPath.NotFound;
      }
      if (PlanetTile.op_Implicit(destTile) < 0)
      {
        Log.Error($"Tried to FindPath with invalid destTile={destTile} vehicles={str}");
        return WorldPath.NotFound;
      }
      if (!vehicleDefs.All<VehicleDef>((Func<VehicleDef, bool>) (vehicleDef => WorldVehiclePathGrid.Instance.reachability.CanReach(vehicleDef, startTile, destTile))))
        return WorldPath.NotFound;
      WorldGrid grid = this.world.grid;
      bool flag1 = vehicleDefs.All<VehicleDef>((Func<VehicleDef, bool>) (v => v.properties.customBiomeCosts.ContainsKey(BiomeDefOf.Ocean)));
      NativeArray<int> neighborsOffsets = grid.UnsafeTileIDToNeighbors_offsets;
      NativeArray<PlanetTile> toNeighborsValues = grid.UnsafeTileIDToNeighbors_values;
      Vector3 tileCenter1 = grid.GetTileCenter(destTile);
      Vector3 normalized = ((Vector3) ref tileCenter1).normalized;
      float num1 = DefDatabase<RoadDef>.AllDefsListForReading.Min<RoadDef>((Func<RoadDef, float>) (road => RoadCostHelper.GetRoadMovementDifficultyMultiplier(vehicleDefs, road)));
      int num2 = 0;
      int heuristicStrength = WorldVehiclePathfinder.CalculateHeuristicStrength(PlanetTile.op_Implicit(startTile), PlanetTile.op_Implicit(destTile));
      this.statusOpenValue += (ushort) 2;
      this.statusClosedValue += (ushort) 2;
      if (this.statusClosedValue >= (ushort) 65435)
        this.ResetStatuses();
      this.calcGrid[PlanetTile.op_Implicit(startTile)].knownCost = 0;
      this.calcGrid[PlanetTile.op_Implicit(startTile)].heuristicCost = 0;
      this.calcGrid[PlanetTile.op_Implicit(startTile)].costNodeCost = 0;
      this.calcGrid[PlanetTile.op_Implicit(startTile)].parentTile = PlanetTile.op_Implicit(startTile);
      this.calcGrid[PlanetTile.op_Implicit(startTile)].status = this.statusOpenValue;
      this.openList.Clear();
      this.openList.Push(new WorldVehiclePathfinder.CostNode(PlanetTile.op_Implicit(startTile), 0));
      while (this.openList.Count > 0)
      {
        WorldVehiclePathfinder.CostNode costNode = this.openList.Pop();
        if (costNode.cost == this.calcGrid[costNode.tile].costNodeCost)
        {
          int tile = costNode.tile;
          if ((int) this.calcGrid[tile].status != (int) this.statusClosedValue)
          {
            if (PlanetTile.op_Equality(PlanetTile.op_Implicit(tile), destTile))
              return this.FinalizedPath(tile);
            if (num2 > 500000)
            {
              Log.Warning($"{str} pathing from {startTile} to {destTile}. Hit search limit of {500000} tiles.");
              return WorldPath.NotFound;
            }
            int num3 = tile + 1 < neighborsOffsets.Length ? neighborsOffsets[tile + 1] : toNeighborsValues.Length;
            for (int index = neighborsOffsets[tile]; index < num3; ++index)
            {
              int neighbor = PlanetTile.op_Implicit(toNeighborsValues[index]);
              if ((int) this.calcGrid[neighbor].status != (int) this.statusClosedValue && vehicleDefs.All<VehicleDef>((Func<VehicleDef, bool>) (vehicleDef => WorldVehiclePathGrid.Instance.Passable(PlanetTile.op_Implicit(neighbor), vehicleDef))))
              {
                float num4 = this.TileTypeCost(neighbor, vehicleDefs);
                if (flag1 && PlanetTile.op_Inequality(PlanetTile.op_Implicit(tile), startTile) && PlanetTile.op_Inequality(PlanetTile.op_Implicit(neighbor), destTile))
                  num4 = vehicleDefs.Max<VehicleDef>((Func<VehicleDef, float>) (vehicleDef => WorldVehiclePathGrid.ConsistentDirectionCost(PlanetTile.op_Implicit(tile), PlanetTile.op_Implicit(neighbor), vehicleDef)));
                float difficultyMultiplier = RoadCostHelper.GetRoadMovementDifficultyMultiplier(vehicleDefs, tile, neighbor);
                int num5 = (int) ((double) ticksPerMove * (double) num4 * (double) difficultyMultiplier) + this.calcGrid[tile].knownCost;
                ushort status = this.calcGrid[neighbor].status;
                bool flag2 = (int) status != (int) this.statusClosedValue && (int) status != (int) this.statusOpenValue;
                if (flag2 || this.calcGrid[neighbor].knownCost > num5)
                {
                  Vector3 tileCenter2 = grid.GetTileCenter(PlanetTile.op_Implicit(neighbor));
                  if (flag2)
                  {
                    float num6 = grid.ApproxDistanceInTiles(GenMath.SphericalDistance(((Vector3) ref tileCenter2).normalized, normalized));
                    this.calcGrid[neighbor].heuristicCost = Mathf.RoundToInt((float) ticksPerMove * num6 * (float) heuristicStrength * num1);
                  }
                  int cost = num5 + this.calcGrid[neighbor].heuristicCost;
                  this.calcGrid[neighbor].parentTile = tile;
                  this.calcGrid[neighbor].knownCost = num5;
                  this.calcGrid[neighbor].status = this.statusOpenValue;
                  this.calcGrid[neighbor].costNodeCost = cost;
                  if (DebugHelper.World.VehicleDef != null)
                  {
                    if (DebugHelper.World.DebugType == WorldPathingDebugType.PathCosts)
                      Find.World.debugDrawer.FlashTile(PlanetTile.op_Implicit(neighbor), (float) this.calcGrid[neighbor].knownCost / 150f, $"t:{ticksPerMove} h:{this.calcGrid[neighbor].heuristicCost}", 50);
                    else if (DebugHelper.World.DebugType == WorldPathingDebugType.Reachability)
                      Find.World.debugDrawer.FlashTile(PlanetTile.op_Implicit(neighbor), 0.55f, (string) null, 50);
                  }
                  this.openList.Push(new WorldVehiclePathfinder.CostNode(neighbor, cost));
                }
              }
            }
            ++num2;
            this.calcGrid[tile].status = this.statusClosedValue;
            if (terminator != null && terminator((float) this.calcGrid[tile].costNodeCost))
              return WorldPath.NotFound;
          }
        }
      }
      Log.Warning($"{str} pathing from {startTile} to {destTile} ran out of tiles to process.");
      return WorldPath.NotFound;
    }
    finally
    {
      this.ClearTileCache();
    }
  }

  private WorldPath FinalizedPath(int lastTile)
  {
    WorldPath emptyWorldPath = Find.WorldPathPool.GetEmptyWorldPath();
    int index = lastTile;
    while (true)
    {
      int parentTile = this.calcGrid[index].parentTile;
      int num = index;
      emptyWorldPath.AddNodeAtStart(PlanetTile.op_Implicit(num));
      if (num != parentTile)
        index = parentTile;
      else
        break;
    }
    emptyWorldPath.SetupFound((float) this.calcGrid[lastTile].knownCost, (PlanetLayer) Find.WorldGrid.Surface);
    return emptyWorldPath;
  }

  private void ResetStatuses()
  {
    int length = this.calcGrid.Length;
    for (int index = 0; index < length; ++index)
      this.calcGrid[index].status = (ushort) 0;
    this.statusOpenValue = (ushort) 1;
    this.statusClosedValue = (ushort) 2;
  }

  private static int CalculateHeuristicStrength(int startTile, int destTile)
  {
    float num = Find.WorldGrid.ApproxDistanceInTiles(PlanetTile.op_Implicit(startTile), PlanetTile.op_Implicit(destTile));
    return Mathf.RoundToInt(WorldVehiclePathfinder.HeuristicWeights.Evaluate(num));
  }

  static WorldVehiclePathfinder()
  {
    SimpleCurve simpleCurve = new SimpleCurve();
    simpleCurve.Add(new CurvePoint(30f, 1f), true);
    simpleCurve.Add(new CurvePoint(40f, 1.3f), true);
    simpleCurve.Add(new CurvePoint(130f, 2f), true);
    WorldVehiclePathfinder.HeuristicWeights = simpleCurve;
  }

  private readonly struct CostNode(int tile, int cost)
  {
    public readonly int tile = tile;
    public readonly int cost = cost;
  }

  private struct PathFinderNodeFast
  {
    public int knownCost;
    public int heuristicCost;
    public int parentTile;
    public int costNodeCost;
    public ushort status;
  }

  private class CostNodeComparer : IComparer<WorldVehiclePathfinder.CostNode>
  {
    public int Compare(WorldVehiclePathfinder.CostNode a, WorldVehiclePathfinder.CostNode b)
    {
      int cost1 = a.cost;
      int cost2 = b.cost;
      if (cost1 > cost2)
        return 1;
      return cost1 < cost2 ? -1 : 0;
    }
  }

  private class TileFeatureLookup
  {
    private readonly List<BiomeDef> biomeDefs = new List<BiomeDef>();
    private readonly List<RiverDef> riverDefs = new List<RiverDef>();
    private readonly List<RoadDef> roadDefs = new List<RoadDef>();
    private readonly List<Hilliness> hills = new List<Hilliness>();

    public TileFeatureLookup(WorldGrid worldGrid) => this.WorldGrid = worldGrid;

    private WorldGrid WorldGrid { get; }

    public void RegisterAllFeatureTypes()
    {
      this.biomeDefs.AddRange((IEnumerable<BiomeDef>) DefDatabase<BiomeDef>.AllDefsListForReading);
      this.roadDefs.AddRange((IEnumerable<RoadDef>) DefDatabase<RoadDef>.AllDefsListForReading);
      this.riverDefs.AddRange((IEnumerable<RiverDef>) DefDatabase<RiverDef>.AllDefsListForReading);
      this.hills.AddRange(Enum.GetValues(typeof (Hilliness)).Cast<Hilliness>());
    }

    public int TileCacheSize
    {
      get => this.biomeDefs.Count * this.riverDefs.Count * this.roadDefs.Count * this.hills.Count;
    }

    private int IndexFor(BiomeDef biomeDef) => this.biomeDefs.IndexOf(biomeDef) + 1;

    private int IndexFor(RiverDef riverDef) => this.riverDefs.IndexOf(riverDef) + 1;

    private int IndexFor(RoadDef roadDef) => this.roadDefs.IndexOf(roadDef) + 1;

    private int IndexFor(Hilliness hilliness) => this.hills.IndexOf(hilliness) + 1;

    public int IndexFor(PlanetTile tileId)
    {
      if (!(this.WorldGrid[tileId] is SurfaceTile surfaceTile))
      {
        Log.Error("Invalid tile for indexing.");
        return -1;
      }
      BiomeDef primaryBiome = ((Tile) surfaceTile).PrimaryBiome;
      List<SurfaceTile.RiverLink> rivers = surfaceTile.Rivers;
      RiverDef river1 = rivers != null ? GenCollection.MaxBy<SurfaceTile.RiverLink, float>((IEnumerable<SurfaceTile.RiverLink>) rivers, (Func<SurfaceTile.RiverLink, float>) (river => river.river.widthOnWorld)).river : (RiverDef) null;
      List<SurfaceTile.RoadLink> roads = surfaceTile.Roads;
      RoadDef road1 = roads != null ? GenCollection.MinBy<SurfaceTile.RoadLink, float>((IEnumerable<SurfaceTile.RoadLink>) roads, (Func<SurfaceTile.RoadLink, float>) (road => road.road.movementCostMultiplier)).road : (RoadDef) null;
      Hilliness hilliness = ((Tile) surfaceTile).hilliness;
      return this.IndexFor(primaryBiome) * this.biomeDefs.Count + this.IndexFor(river1) * this.riverDefs.Count + this.IndexFor(road1) * this.roadDefs.Count + this.IndexFor(hilliness) * this.hills.Count;
    }
  }
}
