// Decompiled with JetBrains decompiler
// Type: Vehicles.World.WorldVehiclePathGrid
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using JetBrains.Annotations;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.World;

public class WorldVehiclePathGrid : WorldComponent
{
  public const float ImpassableMovementDifficulty = 1000f;
  private static readonly Func<Hilliness, float> HillinessMovementDifficultyOffset;
  public readonly WorldVehiclePathGrid.PathGrid[] pathGrids;
  public readonly WorldVehicleReachability reachability;
  private readonly float[] winter;
  private int allPathCostsRecalculatedDayOfYear = -1;
  private CancellationTokenSource cts;
  private Task curTask;

  public event WorldVehiclePathGrid.ReachabilityGridDirtyed OnReachabilityDirty;

  static WorldVehiclePathGrid()
  {
    GameEvent.OnWorldRemoved += (Action) (() => WorldVehiclePathGrid.Instance = (WorldVehiclePathGrid) null);
    GameEvent.OnGameDisposing += new Action(WorldVehiclePathGrid.CancelGridRequests);
    WorldVehiclePathGrid.HillinessMovementDifficultyOffset = (Func<Hilliness, float>) Delegate.CreateDelegate(typeof (Func<Hilliness, float>), AccessTools.Method(typeof (WorldPathGrid), nameof (HillinessMovementDifficultyOffset), (System.Type[]) null, (System.Type[]) null));
  }

  public WorldVehiclePathGrid(RimWorld.Planet.World world)
    : base(world)
  {
    this.world = world;
    this.pathGrids = new WorldVehiclePathGrid.PathGrid[DefDatabase<VehicleDef>.DefCount];
    this.winter = new float[Find.WorldGrid.TilesCount];
    this.ResetPathGrid();
    this.Initialized = false;
    WorldVehiclePathGrid.Instance = this;
    this.reachability = new WorldVehicleReachability(this);
  }

  public static WorldVehiclePathGrid Instance { get; private set; }

  private bool Recalculating { get; set; }

  public bool Initialized { get; private set; }

  public WorldVehiclePathGrid.PathGrid this[VehicleDef vehicleDef]
  {
    get => this.pathGrids[vehicleDef.DefIndex];
  }

  private static int DayOfYearAt0Long => GenDate.DayOfYear((long) GenTicks.TicksAbs, 0.0f);

  private void ResetPathGrid()
  {
    foreach (VehicleDef owner in DefDatabase<VehicleDef>.AllDefsListForReading)
      this.pathGrids[owner.DefIndex] = new WorldVehiclePathGrid.PathGrid(owner, Find.WorldGrid.TilesCount);
  }

  public virtual void WorldComponentTick()
  {
    if (!this.Recalculating && this.allPathCostsRecalculatedDayOfYear != WorldVehiclePathGrid.DayOfYearAt0Long)
      this.RecalculateAllPathCostsAsync();
    if (!Prefs.DevMode)
      return;
    this.FlashWorldGrid();
  }

  private void FlashWorldGrid()
  {
    if (DebugHelper.World.VehicleDef == null)
      return;
    PlanetTile selectedTile1 = Find.WorldSelector.SelectedTile;
    if (!((PlanetTile) ref selectedTile1).Valid || Find.TickManager.TicksGame % 30 != 0)
      return;
    switch (DebugHelper.World.DebugType)
    {
      case WorldPathingDebugType.PathCosts:
        PlanetTile selectedTile2 = Find.WorldSelector.SelectedTile;
        List<PlanetTile> planetTileList1 = new List<PlanetTile>();
        Find.WorldGrid.GetTileNeighbors(selectedTile2, planetTileList1);
        float num1 = this.pathGrids[DebugHelper.World.VehicleDef.DefIndex][PlanetTile.op_Implicit(selectedTile2)];
        Find.World.debugDrawer.FlashTile(selectedTile2, (float) ((double) num1 * 10.0 / 1000.0), num1.ToString(), 15);
        using (List<PlanetTile>.Enumerator enumerator = planetTileList1.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            PlanetTile current = enumerator.Current;
            Find.World.debugDrawer.FlashTile(current, 0.0f, this.pathGrids[DebugHelper.World.VehicleDef.DefIndex][PlanetTile.op_Implicit(current)].ToString(), 30);
          }
          break;
        }
      case WorldPathingDebugType.Reachability:
        PlanetTile selectedTile3 = Find.WorldSelector.SelectedTile;
        List<PlanetTile> planetTileList2 = new List<PlanetTile>();
        Ext_World.Bfs(selectedTile3, new Action<PlanetTile>(planetTileList2.Add), 10);
        Find.World.debugDrawer.FlashTile(selectedTile3, 0.8f, IdStringAt(selectedTile3), 15);
        using (List<PlanetTile>.Enumerator enumerator = planetTileList2.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            PlanetTile current = enumerator.Current;
            float num2 = WorldVehiclePathGrid.Instance.reachability.CanReach(DebugHelper.World.VehicleDef, selectedTile3, current) ? 0.65f : 0.0f;
            Find.World.debugDrawer.FlashTile(current, num2, IdStringAt(current), 30);
          }
          break;
        }
      case WorldPathingDebugType.WinterPct:
        PlanetTile selectedTile4 = Find.WorldSelector.SelectedTile;
        List<PlanetTile> planetTileList3 = new List<PlanetTile>();
        Ext_World.Bfs(selectedTile4, new Action<PlanetTile>(planetTileList3.Add), 10);
        float num3 = this.WinterPercentAt(selectedTile4);
        Find.World.debugDrawer.FlashTile(selectedTile4, num3, num3.ToString("#.00"), 15);
        using (List<PlanetTile>.Enumerator enumerator = planetTileList3.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            PlanetTile current = enumerator.Current;
            float num4 = this.WinterPercentAt(current);
            Find.World.debugDrawer.FlashTile(current, num4, num4.ToString("#.00"), 30);
          }
          break;
        }
      default:
        throw new NotImplementedException("WorldPathingDebugType");
    }

    static string IdStringAt(PlanetTile t)
    {
      return WorldVehiclePathGrid.Instance.reachability.GetRegionId(DebugHelper.World.VehicleDef, PlanetTile.op_Implicit(t)).ToString();
    }
  }

  public bool Passable(PlanetTile tile, VehicleDef vehicleDef)
  {
    return Find.WorldGrid.InBounds(tile) && (double) this.pathGrids[vehicleDef.DefIndex][PlanetTile.op_Implicit(tile)] < 1000.0;
  }

  public bool PassableFast(PlanetTile tile, VehicleDef vehicleDef)
  {
    return (double) this.pathGrids[vehicleDef.DefIndex][PlanetTile.op_Implicit(tile)] < 1000.0;
  }

  public float PerceivedMovementDifficultyAt(PlanetTile tile, VehicleDef vehicleDef)
  {
    return this.pathGrids[vehicleDef.DefIndex][PlanetTile.op_Implicit(tile)];
  }

  public float WinterPercentAt(PlanetTile tile) => this.winter[PlanetTile.op_Implicit(tile)];

  [PublicAPI]
  public WorldVehiclePathGrid.GridState RecalculatePerceivedMovementDifficultyAt(
    PlanetTile tile,
    VehicleDef vehicleDef)
  {
    if (!Find.WorldGrid.InBounds(tile))
      return WorldVehiclePathGrid.GridState.None;
    WorldVehiclePathGrid.PathGrid pathGrid = this.pathGrids[vehicleDef.DefIndex];
    float num1 = pathGrid[tile.tileId];
    float num2 = WorldVehiclePathGrid.CalculatedMovementDifficultyAt(tile, vehicleDef);
    pathGrid[tile.tileId] = num2;
    return (double) num1 >= 1000.0 ^ (double) num2 >= 1000.0 ? WorldVehiclePathGrid.GridState.RegionsDirty : WorldVehiclePathGrid.GridState.PathGridDirty;
  }

  private void RecalculateAllPathCostsAsync()
  {
    if (this.Recalculating)
    {
      Trace.Fail("Attempting to regenerate world path grid for all vehicles but it is already running.");
    }
    else
    {
      this.allPathCostsRecalculatedDayOfYear = WorldVehiclePathGrid.DayOfYearAt0Long;
      Task curTask = this.curTask;
      if (curTask != null && curTask.Status == TaskStatus.Running)
      {
        Trace.Fail("Restarting task while it is ongoing. Cancelling before continuing.");
        this.cts.Cancel();
        Task.WaitAll(new Task[1]{ this.curTask }, 1000);
      }
      this.cts = new CancellationTokenSource();
      this.curTask = TaskManager.Run((Action) (() =>
      {
        try
        {
          this.RecalculateAllPerceivedPathCosts(this.cts.Token);
        }
        finally
        {
          this.cts.Dispose();
          this.cts = (CancellationTokenSource) null;
          this.curTask = (Task) null;
        }
      }), this.cts.Token);
    }
  }

  private void RecalculateAllPerceivedPathCosts(CancellationToken token, int? ticksAbs = null)
  {
    this.allPathCostsRecalculatedDayOfYear = WorldVehiclePathGrid.DayOfYearAt0Long;
    using (new WorldVehiclePathGrid.GridInitializerState(this))
    {
      foreach (VehicleDef vehicleDef in DefDatabase<VehicleDef>.AllDefsListForReading)
        this.RecalculateAllPerceivedPathCostsFor(vehicleDef, token);
      for (int index = 0; index < Find.WorldGrid.TilesCount; ++index)
        this.RecalculateWinterPercentAt(PlanetTile.op_Implicit(index), ticksAbs);
    }
  }

  [Profile]
  internal void RecalculateAllPerceivedPathCostsFor(VehicleDef vehicleDef, CancellationToken token)
  {
    bool flag = false;
    for (int index = 0; index < Find.WorldGrid.TilesCount; ++index)
    {
      if (token.IsCancellationRequested)
        return;
      WorldVehiclePathGrid.GridState gridState = this.RecalculatePerceivedMovementDifficultyAt(PlanetTile.op_Implicit(index), vehicleDef);
      flag |= gridState == WorldVehiclePathGrid.GridState.RegionsDirty;
    }
    if (!flag)
      return;
    WorldVehiclePathGrid.ReachabilityGridDirtyed reachabilityDirty = this.OnReachabilityDirty;
    if (reachabilityDirty == null)
      return;
    reachabilityDirty(vehicleDef, token);
  }

  private void RecalculateWinterPercentAt(PlanetTile tile, int? ticksAbs = null)
  {
    this.winter[PlanetTile.op_Implicit(tile)] = WinterPathingHelper.GetWinterPercent(tile, ticksAbs);
  }

  private static void CancelGridRequests()
  {
    WorldVehiclePathGrid component = Find.World.GetComponent<WorldVehiclePathGrid>();
    bool flag1 = component.curTask == null || component.curTask.IsCompleted;
    if (!flag1)
    {
      Task curTask = component.curTask;
      bool flag2;
      if (curTask != null)
      {
        switch (curTask.Status)
        {
          case TaskStatus.Canceled:
          case TaskStatus.Faulted:
            flag2 = true;
            goto label_5;
        }
      }
      flag2 = false;
label_5:
      flag1 = flag2;
    }
    if (flag1)
      return;
    component.cts.Cancel();
    Task.WaitAll(new Task[1]{ component.curTask }, 5000);
  }

  [Profile]
  public static float CalculatedMovementDifficultyAt(
    PlanetTile tile,
    VehicleDef vehicleDef,
    StringBuilder explanation = null,
    bool coastalTravel = true)
  {
    if (!(Find.WorldGrid[tile] is SurfaceTile surfaceTile))
    {
      Log.Error("Attempting to calculate movement difficulty for non-surface tile.");
      return 1000f;
    }
    if (explanation != null && explanation.Length > 0)
      explanation.AppendLine();
    List<SurfaceTile.RiverLink> rivers = surfaceTile.Rivers;
    if (!GenList.NullOrEmpty<SurfaceTile.RiverLink>((IList<SurfaceTile.RiverLink>) rivers))
    {
      SurfaceTile.RiverLink riverLink = WorldHelper.BiggestRiverOnTile(rivers);
      if (riverLink.river != null)
      {
        if ((vehicleDef.properties.defaultImpassable & DefaultImpassable.Rivers) != DefaultImpassable.None)
        {
          explanation?.Append($"{((Def) riverLink.river).LabelCap}: Impassable");
          return 1000f;
        }
        float num;
        if (vehicleDef.properties.customRiverCosts.TryGetValue(riverLink.river, out num) && !Mathf.Approximately(num, 1000f))
        {
          explanation?.Append($"{((Def) riverLink.river).LabelCap}: {GenText.ToStringWithSign(num, "0.#")}");
          return num;
        }
      }
    }
    float num1;
    if ((vehicleDef.properties.defaultImpassable & DefaultImpassable.Biomes) != DefaultImpassable.None)
    {
      num1 = 1000f;
    }
    else
    {
      BiomeDef primaryBiome = ((Tile) surfaceTile).PrimaryBiome;
      num1 = primaryBiome.impassable ? 1000f : primaryBiome.movementDifficulty;
    }
    if (coastalTravel && vehicleDef.CoastalTravel(tile))
      num1 = Mathf.Min(num1, vehicleDef.properties.customBiomeCosts[BiomeDefOf.Ocean]);
    float num2 = GenCollection.TryGetValue<BiomeDef, float>((IReadOnlyDictionary<BiomeDef, float>) vehicleDef.properties.customBiomeCosts, ((Tile) surfaceTile).PrimaryBiome, num1);
    float num3;
    if (!vehicleDef.properties.customHillinessCosts.TryGetValue(((Tile) surfaceTile).hilliness, out num3))
    {
      if ((vehicleDef.properties.defaultImpassable & DefaultImpassable.Roads) != DefaultImpassable.None)
        return 1000f;
      num3 = WorldVehiclePathGrid.HillinessMovementDifficultyOffset(((Tile) surfaceTile).hilliness);
    }
    if ((double) num2 >= 1000.0 || (double) num3 >= 1000.0)
    {
      explanation?.Append(TaggedString.op_Implicit(Translator.Translate("Impassable")));
      return 1000f;
    }
    if (!GenList.NullOrEmpty<SurfaceTile.RoadLink>((IList<SurfaceTile.RoadLink>) surfaceTile.Roads))
    {
      if ((vehicleDef.properties.defaultImpassable & DefaultImpassable.Roads) != DefaultImpassable.None && !surfaceTile.Roads.Exists(new Predicate<SurfaceTile.RoadLink>(PassableRoad)))
        return 1000f;
      if ((double) num2 < 1000.0 && VehicleMod.settings.main.ignoreBiomeCostOnRoads)
      {
        num2 = 1f;
        num3 = 0.0f;
      }
    }
    explanation?.Append(TaggedString.op_Implicit(TaggedString.op_Addition(TaggedString.op_Addition(((Def) ((Tile) surfaceTile).PrimaryBiome).LabelCap, ": "), GenText.ToStringWithSign(num2, "0.#"))));
    float num4 = num2 + num3;
    if (explanation != null && !Mathf.Approximately(num3, 0.0f))
    {
      explanation.AppendLine();
      explanation.Append($"{HillinessUtility.GetLabelCap(((Tile) surfaceTile).hilliness)}: {GenText.ToStringWithSign(num3, "0.#")}");
    }
    return num4 + WinterPathingHelper.GetCurrentWinterMovementDifficultyFor(vehicleDef, tile, explanation);

    bool PassableRoad(SurfaceTile.RoadLink roadLink)
    {
      return vehicleDef.properties.customRoadCosts.ContainsKey(roadLink.road);
    }
  }

  public static float ConsistentDirectionCost(
    PlanetTile tile,
    PlanetTile neighbor,
    VehicleDef vehicleDef)
  {
    return Mathf.Max(WorldVehiclePathGrid.CalculatedMovementDifficultyAt(tile, vehicleDef), WorldVehiclePathGrid.CalculatedMovementDifficultyAt(neighbor, vehicleDef));
  }

  [DebugAction("Vehicle Framework", null, false, false, false, false, false, 0, false)]
  private static void RecalculatePathGrid()
  {
    Find.World.GetComponent<WorldVehiclePathGrid>().RecalculateAllPerceivedPathCosts(CancellationToken.None);
  }

  [DebugAction("Vehicle Framework", null, false, false, false, false, false, 0, false)]
  private static void RecalculateReachabilityGrid()
  {
    WorldVehiclePathGrid pathGrid = Find.World.GetComponent<WorldVehiclePathGrid>();
    pathGrid.cts = new CancellationTokenSource();
    pathGrid.curTask = TaskManager.Run((Action) (() =>
    {
      foreach (VehicleDef def in DefDatabase<VehicleDef>.AllDefsListForReading)
      {
        WorldVehiclePathGrid.ReachabilityGridDirtyed reachabilityDirty = pathGrid.OnReachabilityDirty;
        if (reachabilityDirty != null)
          reachabilityDirty(def, pathGrid.cts.Token);
      }
    }), pathGrid.cts.Token);
  }

  public delegate void ReachabilityGridDirtyed(VehicleDef def, CancellationToken token);

  [PublicAPI]
  public enum GridState
  {
    None,
    PathGridDirty,
    RegionsDirty,
  }

  private readonly struct GridInitializerState : IDisposable
  {
    private readonly WorldVehiclePathGrid pathGrid;

    public GridInitializerState(WorldVehiclePathGrid pathGrid)
    {
      this.pathGrid = pathGrid;
      this.pathGrid.Initialized = false;
      this.pathGrid.Recalculating = true;
    }

    void IDisposable.Dispose()
    {
      this.pathGrid.Recalculating = false;
      this.pathGrid.Initialized = true;
    }
  }

  [PublicAPI]
  public class PathGrid
  {
    private VehicleDef owner;
    private readonly float[] costs;

    public float this[int index]
    {
      get => this.costs[index];
      set => this.costs[index] = value;
    }

    public PathGrid(VehicleDef owner, int size)
    {
      this.owner = owner;
      this.costs = new float[size];
    }

    public bool Enabled { get; internal set; }

    public VehicleDef Owner => this.owner;
  }
}
