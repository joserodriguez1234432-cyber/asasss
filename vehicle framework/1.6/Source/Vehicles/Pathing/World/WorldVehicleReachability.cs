// Decompiled with JetBrains decompiler
// Type: Vehicles.World.WorldVehicleReachability
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Algorithms;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles.World;

public class WorldVehicleReachability
{
  private readonly WorldVehiclePathGrid pathGrid;
  private readonly WorldVehicleReachability.WorldRegionGrid[] regionGrids;

  public WorldVehicleReachability(WorldVehiclePathGrid pathGrid)
  {
    this.pathGrid = pathGrid;
    this.regionGrids = new WorldVehicleReachability.WorldRegionGrid[DefDatabase<VehicleDef>.DefCount];
    this.InitReachabilityGrid();
    pathGrid.OnReachabilityDirty += new WorldVehiclePathGrid.ReachabilityGridDirtyed(this.RegenerateRegionsFor);
  }

  public WorldVehicleReachability.WorldRegionGrid GetRegionGrid(VehicleDef vehicleDef)
  {
    return this.regionGrids[GridOwners.World.GetOwner(vehicleDef).DefIndex];
  }

  public int GetRegionId(VehicleDef vehicleDef, int tile)
  {
    return this.regionGrids[GridOwners.World.GetOwner(vehicleDef).DefIndex].GetRegionId(tile);
  }

  private void InitReachabilityGrid()
  {
    foreach (VehicleDef allOwner in GridOwners.World.AllOwners)
      this.regionGrids[allOwner.DefIndex] = new WorldVehicleReachability.WorldRegionGrid(this.pathGrid, allOwner);
  }

  private void RegenerateAllRegions(CancellationToken token)
  {
    foreach (VehicleDef allOwner in GridOwners.World.AllOwners)
      this.RegenerateRegionsFor(allOwner, token);
  }

  private void RegenerateRegionsFor(VehicleDef vehicleDef, CancellationToken token)
  {
    this.regionGrids[GridOwners.World.GetOwner(vehicleDef).DefIndex].GenerateRegions(token);
  }

  public bool CanReach(VehicleCaravan caravan, PlanetTile destTile)
  {
    int startTile = PlanetTile.op_Implicit(((WorldObject) caravan).Tile);
    return caravan.UniqueVehicleDefsInCaravan().ToList<VehicleDef>().All<VehicleDef>((Func<VehicleDef, bool>) (v => this.CanReach(v, PlanetTile.op_Implicit(startTile), destTile)));
  }

  public bool CanReach(VehicleDef vehicleDef, PlanetTile startTile, PlanetTile destTile)
  {
    if (PlanetTile.op_Implicit(startTile) >= 0 && PlanetTile.op_Implicit(startTile) < Find.WorldGrid.TilesCount && PlanetTile.op_Implicit(destTile) >= 0 && PlanetTile.op_Implicit(destTile) < Find.WorldGrid.TilesCount)
      return this.regionGrids[GridOwners.World.GetOwner(vehicleDef).DefIndex].CanReach(PlanetTile.op_Implicit(startTile), PlanetTile.op_Implicit(destTile));
    Log.Error("Trying to reach tile that is out of bounds of the world grid.");
    return false;
  }

  [DebugAction("Vehicle Framework", null, false, false, false, false, false, 0, false)]
  private static void FlashRandomRegionGrid()
  {
    if (!GridOwners.World.AnyOwners)
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.ClickReject, (Map) null);
    }
    else
    {
      VehicleDef vehicleDef = GridOwners.World.AllOwners[0];
      WorldVehicleReachability reachability = WorldVehiclePathGrid.Instance.reachability;
      for (int index = 0; index < Find.WorldGrid.TilesCount; ++index)
        Find.World.debugDrawer.FlashTile(PlanetTile.op_Implicit(index), ColorPct(index), IdStringAt(index), 600);

      string IdStringAt(int t) => reachability.GetRegionId(vehicleDef, t).ToString();

      float ColorPct(int tile)
      {
        float num;
        switch (WorldVehiclePathGrid.Instance.reachability.regionGrids[vehicleDef.DefIndex].GetRegionId(tile))
        {
          case -1:
            num = 0.25f;
            break;
          case 0:
            num = 0.0f;
            break;
          default:
            num = 0.75f;
            break;
        }
        return num;
      }
    }
  }

  public class WorldRegionGrid
  {
    private readonly WorldVehiclePathGrid pathGrid;
    private readonly VehicleDef owner;
    private int[] regionIds = Array.Empty<int>();
    private int totalRegions;

    public WorldRegionGrid(WorldVehiclePathGrid pathGrid, VehicleDef vehicleDef)
    {
      this.pathGrid = pathGrid;
      this.owner = vehicleDef;
    }

    public int TotalRegions => this.totalRegions + 2;

    public int GetRegionId(int tile) => this.regionIds[tile];

    public bool CanReach(int fromTile, int toTile)
    {
      int regionId1 = this.regionIds[fromTile];
      int regionId2 = this.regionIds[toTile];
      return regionId1 > 0 && regionId2 > 0 && regionId1 == regionId2;
    }

    [Profile]
    public void GenerateRegions(CancellationToken token)
    {
      BFS<PlanetTile> bfs = new BFS<PlanetTile>();
      int[] tilesToId = new int[Find.WorldGrid.TilesCount];
      this.totalRegions = 1;
      for (int index = 0; index < Find.WorldGrid.TilesCount; ++index)
      {
        if (token.IsCancellationRequested)
          return;
        if (tilesToId[index] == 0)
        {
          if (!this.pathGrid.PassableFast(PlanetTile.op_Implicit(index), this.owner))
          {
            tilesToId[index] = -1;
          }
          else
          {
            int id = this.totalRegions;
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            bfs.FloodFill(PlanetTile.op_Implicit(index), WorldVehicleReachability.WorldRegionGrid.\u003C\u003EO.\u003C0\u003E__GetTileNeighbors ?? (WorldVehicleReachability.WorldRegionGrid.\u003C\u003EO.\u003C0\u003E__GetTileNeighbors = new Func<PlanetTile, IEnumerable<PlanetTile>>(Ext_World.GetTileNeighbors)), (Action<PlanetTile>) null, new Action<PlanetTile>(OnEnter), (Action<PlanetTile>) null, token, new Func<PlanetTile, bool>(CanEnter));
            ++this.totalRegions;

            void OnEnter(PlanetTile t) => tilesToId[PlanetTile.op_Implicit(t)] = id;
          }
        }
      }
      this.regionIds = tilesToId;

      bool CanEnter(PlanetTile t)
      {
        return tilesToId[PlanetTile.op_Implicit(t)] == 0 && this.pathGrid.PassableFast(t, this.owner);
      }
    }
  }
}
