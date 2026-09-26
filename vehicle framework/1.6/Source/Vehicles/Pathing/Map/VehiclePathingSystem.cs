// Decompiled with JetBrains decompiler
// Type: Vehicles.VehiclePathingSystem
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[StaticConstructorOnStartup]
public sealed class VehiclePathingSystem : MapComponent
{
  private const int EventMapThreadId = 25;
  private const VehiclePathingSystem.GridSelection DefaultGrids = VehiclePathingSystem.GridSelection.All;
  private const VehiclePathingSystem.GridDeferment DefaultDeferment = VehiclePathingSystem.GridDeferment.Lazy;
  private VehiclePathingSystem.VehiclePathData[] vehicleData;
  private VehicleDef buildingFor;
  private int ownerCleanIndex;
  public DedicatedThread dedicatedThread;
  public DeferredGridGeneration deferredGridGeneration;
  private int defGridCalculatedDayOfYear;

  public VehiclePathingSystem(Map map)
    : base(map)
  {
    this.deferredGridGeneration = new DeferredGridGeneration(this);
    this.GridOwners = new MapGridOwners(this);
    this.GridOwners.OnOwnershipTransfer += new GridOwnerList<MapGridOwners.PathConfig>.OwnershipTransferred(this.SwapRegionManagerOwners);
    this.GridOwners.Init();
    this.ConstructComponents();
  }

  public MapGridOwners GridOwners { get; }

  private static int DayOfYearAt0Long => GenDate.DayOfYear((long) GenTicks.TicksAbs, 0.0f);

  public bool ThreadAlive
  {
    get
    {
      DedicatedThread dedicatedThread = this.dedicatedThread;
      if (dedicatedThread != null)
      {
        DedicatedThread.ThreadState state = dedicatedThread.State;
        if (state != DedicatedThread.ThreadState.Uninitialized)
          return state != DedicatedThread.ThreadState.Terminated;
      }
      return false;
    }
  }

  public bool ThreadAvailable => this.ThreadAlive && !this.dedicatedThread.IsSuspended;

  public VehiclePathingSystem.VehiclePathData this[VehicleDef vehicleDef]
  {
    get
    {
      if (vehicleDef == null)
        throw new ArgumentNullException(nameof (vehicleDef));
      return this.vehicleData[vehicleDef.DefIndex];
    }
  }

  internal void InitThread()
  {
    if (this.dedicatedThread != null)
    {
      Log.Warning("Reinitializing dedicatedThread. It should only be done once on map generation.");
      this.ReleaseThread();
    }
    if (!VehicleMod.settings.debug.debugUseMultithreading)
    {
      Log.Warning($"Loading map without DedicatedThread. This will cause performance issues. Map={this.map}.");
    }
    else
    {
      if (this.map.info?.parent == null)
        return;
      this.dedicatedThread = VehiclePathingSystem.GetDedicatedThread(this.map);
    }
  }

  private static DedicatedThread GetDedicatedThread(Map map)
  {
    if (map.IsPlayerHome)
    {
      DedicatedThread dedicatedThread = ThreadManager.CreateNew();
      Debug.Message($"Creating thread (id={dedicatedThread?.id})");
      return dedicatedThread;
    }
    DedicatedThread shared = ThreadManager.GetOrCreateShared(25);
    Debug.Message($"Fetching thread with shared ownership (id={shared?.id})");
    return shared;
  }

  public virtual void FinalizeInit()
  {
    base.FinalizeInit();
    if (!this.ThreadAlive)
      this.InitThread();
    this.RegenerateGrids();
  }

  public void RegenerateGrids(
    VehiclePathingSystem.GridSelection grids = VehiclePathingSystem.GridSelection.All,
    VehiclePathingSystem.GridDeferment deferment = VehiclePathingSystem.GridDeferment.Lazy)
  {
    using (new LongEventText())
    {
      switch (deferment)
      {
        case VehiclePathingSystem.GridDeferment.Lazy:
          break;
        case VehiclePathingSystem.GridDeferment.Deferred:
          if ((grids & VehiclePathingSystem.GridSelection.PathGrids) != VehiclePathingSystem.GridSelection.None)
            this.deferredGridGeneration.GenerateAllPathGrids();
          if ((grids & VehiclePathingSystem.GridSelection.Regions) == VehiclePathingSystem.GridSelection.None)
            break;
          this.deferredGridGeneration.GenerateAllRegionGrids();
          break;
        case VehiclePathingSystem.GridDeferment.Forced:
          this.GeneratePathGrids();
          this.GenerateRegionsParallel();
          break;
        default:
          throw new NotImplementedException();
      }
    }
  }

  private void GeneratePathGrids()
  {
    for (int index = 0; index < this.vehicleData.Length; ++index)
    {
      VehiclePathingSystem.VehiclePathData vehiclePathData = this.vehicleData[index];
      LongEventHandler.SetCurrentEventText($"{Translator.Translate("VF_GeneratingPathGrids")} {index}/{this.vehicleData.Length}");
      vehiclePathData.VehiclePathGrid.RecalculateAllPerceivedPathCosts();
    }
  }

  private void GenerateRegions()
  {
    int length = this.GridOwners.AllOwners.Length;
    for (int index = 0; index < length; ++index)
    {
      VehicleDef allOwner = this.GridOwners.AllOwners[index];
      LongEventHandler.SetCurrentEventText($"{Translator.Translate("VF_GeneratingRegions")} {index}/{length}");
      VehiclePathingSystem.VehiclePathData vehiclePathData = this[allOwner];
      vehiclePathData.VehicleRegionAndRoomUpdater.Init();
      vehiclePathData.VehicleRegionAndRoomUpdater.RebuildAllVehicleRegions();
    }
  }

  private void GenerateGridConnections()
  {
    int length = this.GridOwners.AllOwners.Length;
    for (int index = 0; index < length; ++index)
    {
      VehicleDef allOwner = this.GridOwners.AllOwners[index];
      LongEventHandler.SetCurrentEventText($"{Translator.Translate("VF_GeneratingRegions")} {index}/{length}");
      this[allOwner].VehicleRegionConnector.RebuildAllConnections();
    }
  }

  private void GenerateRegionsParallel()
  {
    if (!this.GridOwners.AnyOwners)
      return;
    if (this.GridOwners.AllOwners.Length <= 3)
    {
      this.GenerateRegions();
    }
    else
    {
      DeepProfiler.Start("Vehicle Regions");
      Parallel.ForEach<VehicleDef>((IEnumerable<VehicleDef>) this.GridOwners.AllOwners, (Action<VehicleDef>) (vehicleDef =>
      {
        LongEventHandler.SetCurrentEventText(TaggedString.op_Implicit(Translator.Translate("VF_GeneratingRegions")));
        VehiclePathingSystem.VehiclePathData vehiclePathData = this[vehicleDef];
        vehiclePathData.VehicleRegionAndRoomUpdater.Init();
        vehiclePathData.VehicleRegionAndRoomUpdater.RebuildAllVehicleRegions();
      }));
      DeepProfiler.End();
    }
  }

  public void ConstructComponents()
  {
    this.vehicleData = new VehiclePathingSystem.VehiclePathData[DefDatabase<VehicleDef>.DefCount];
    this.GenerateAllPathData();
    this.DisableAllRegionUpdaters();
  }

  public void DisableAllRegionUpdaters()
  {
    foreach (VehicleDef allOwner in this.GridOwners.AllOwners)
      this[allOwner].VehicleRegionAndRoomUpdater.Disable();
  }

  public virtual void ExposeData()
  {
    base.ExposeData();
    if (Scribe.mode != 4 || this.dedicatedThread != null)
      return;
    this.InitThread();
  }

  public void RequestGridsFor(VehiclePawn vehicle)
  {
    this.RequestGridsFor(vehicle.VehicleDef, DeferredGridGeneration.UrgencyFor(this.map, vehicle));
  }

  public void RequestGridsFor(VehicleDef vehicleDef, DeferredGridGeneration.Urgency urgency)
  {
    this.deferredGridGeneration.RequestGridsFor(vehicleDef, urgency);
  }

  public virtual void MapRemoved() => this.ReleaseThread();

  internal void ReleaseThread()
  {
    if (this.dedicatedThread == null || this.dedicatedThread.IsTerminated)
      return;
    Debug.Message($"Releasing thread (id={this.dedicatedThread.id})");
    ThreadManager.ReleaseAndJoin(this.dedicatedThread);
    this.dedicatedThread = (DedicatedThread) null;
  }

  public virtual void MapComponentTick()
  {
    base.MapComponentTick();
    if (!this.ThreadAlive)
      return;
    int dayOfYearAt0Long = VehiclePathingSystem.DayOfYearAt0Long;
    if (this.defGridCalculatedDayOfYear == dayOfYearAt0Long)
      return;
    this.deferredGridGeneration.DoIncrementalPass();
    this.defGridCalculatedDayOfYear = dayOfYearAt0Long;
  }

  public virtual void MapComponentDraw()
  {
    FlashGridType debugDrawFlashGrid = SectionDebug.debugDrawFlashGrid;
    if (debugDrawFlashGrid <= FlashGridType.None || Find.CurrentMap == null || WorldRendererUtility.WorldRendered)
      return;
    switch (debugDrawFlashGrid)
    {
      case FlashGridType.CoverGrid:
        this.FlashCoverGrid();
        break;
      case FlashGridType.GasGrid:
        this.FlashGasGrid();
        break;
      case FlashGridType.PositionManager:
        this.FlashClaimants();
        break;
      case FlashGridType.ThingGrid:
        this.FlashThingGrid();
        break;
      case FlashGridType.ListerThings:
        this.FlashListerThings();
        break;
      default:
        Log.ErrorOnce($"Not Implemented: {debugDrawFlashGrid}", debugDrawFlashGrid.GetHashCode());
        break;
    }
  }

  public virtual void MapComponentUpdate() => this.UpdateRegions();

  private void FlashListerThings()
  {
    foreach (Region allRegion in this.map.regionGrid.AllRegions)
    {
      if (allRegion.ListerThings.ThingsInGroup((ThingRequestGroup) 12).Exists((Predicate<Thing>) (pawn => pawn is VehiclePawn)))
        Draw(allRegion);
    }

    static void Draw(Region region)
    {
      float num = (float) (1.0 - (double) (Find.TickManager.TicksGame % 60) / 60.0);
      GenDraw.DrawFieldEdges(region.Cells.ToList<IntVec3>(), new Color(0.0f, 0.0f, 1f, num), new float?(), (HashSet<IntVec3>) null, 2900);
    }
  }

  private void FlashCoverGrid()
  {
    if (Find.TickManager.Paused)
      return;
    CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
    foreach (IntVec3 intVec3 in currentViewRect)
    {
      float num = CoverUtility.TotalSurroundingCoverScore(intVec3, this.map);
      this.map.debugDrawer.FlashCell(intVec3, num / 8f, num.ToString("F2"), 1);
    }
  }

  private void FlashGasGrid()
  {
    if (Find.TickManager.Paused)
      return;
    CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
    foreach (IntVec3 intVec3 in currentViewRect)
    {
      if (this.map.gasGrid.GasCanMoveTo(intVec3))
      {
        float num = this.map.gasGrid.DensityPercentAt(intVec3, (GasType) 0);
        this.map.debugDrawer.FlashCell(intVec3, num / 8f, num.ToString("F2"), 1);
      }
    }
  }

  private void FlashClaimants()
  {
    if (Find.TickManager.Paused)
      return;
    VehiclePositionManager detachedMapComponent = this.map.GetDetachedMapComponent<VehiclePositionManager>();
    CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
    foreach (IntVec3 cell in currentViewRect)
    {
      if (detachedMapComponent.PositionClaimed(cell))
        this.map.debugDrawer.FlashCell(cell, 1f, (string) null, 1);
    }
  }

  private void FlashThingGrid()
  {
    if (Find.TickManager.Paused)
      return;
    CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
    foreach (IntVec3 intVec3 in currentViewRect)
    {
      if (this.map.thingGrid.ThingAt(intVec3, (ThingCategory) 1) is VehiclePawn)
        this.map.debugDrawer.FlashCell(intVec3, 1f, (string) null, 1);
    }
  }

  private void UpdateRegions()
  {
    if (!this.GridOwners.AnyOwners || this.ownerCleanIndex >= this.GridOwners.AllOwners.Length)
      return;
    VehiclePathingSystem.VehiclePathData pathData = this[this.GridOwners.AllOwners[this.ownerCleanIndex]];
    if (!pathData.Suspended && pathData.VehicleRegionDirtyer.AnyDirty)
    {
      if (this.ThreadAvailable)
      {
        AsyncRebuildRegionsAction action = AsyncPool<AsyncRebuildRegionsAction>.Get();
        action.Set(pathData);
        this.dedicatedThread.Enqueue((AsyncAction) action);
      }
      else
      {
        pathData.VehicleRegionGrid.UpdateClean();
        pathData.VehicleRegionAndRoomUpdater.TryRebuildVehicleRegions();
      }
    }
    ++this.ownerCleanIndex;
    if (this.ownerCleanIndex < this.GridOwners.AllOwners.Length)
      return;
    this.ownerCleanIndex = 0;
  }

  private void GenerateAllPathData()
  {
    foreach (VehicleDef vehicleDef in DefDatabase<VehicleDef>.AllDefsListForReading)
      this.GeneratePathData(vehicleDef);
  }

  private void GeneratePathData(VehicleDef vehicleDef)
  {
    VehiclePathingSystem.VehiclePathData pathData = new VehiclePathingSystem.VehiclePathData();
    this.vehicleData[vehicleDef.DefIndex] = pathData;
    bool flag = this.GridOwners.IsOwner(vehicleDef);
    this.buildingFor = vehicleDef;
    pathData.VehiclePathGrid = new VehiclePathGrid(this, vehicleDef);
    pathData.VehicleRegionConnector = new VehicleRegionConnector(this, vehicleDef);
    pathData.VehiclePathFinder = new VehiclePathFinder(this, vehicleDef);
    if (flag)
    {
      pathData.ReachabilityData = new VehiclePathingSystem.VehicleReachabilitySettings(this, vehicleDef, pathData);
    }
    else
    {
      VehicleDef owner = this.GridOwners.GetOwner(vehicleDef);
      pathData.ReachabilityData = this.vehicleData[owner.DefIndex].ReachabilityData;
    }
    this.buildingFor = (VehicleDef) null;
    pathData.VehiclePathGrid.PostInit();
    pathData.VehicleRegionConnector.PostInit();
    pathData.VehiclePathFinder.PostInit();
    if (!flag)
      return;
    pathData.ReachabilityData.PostInit();
  }

  private void SwapRegionManagerOwners(VehicleDef fromVehicleDef, VehicleDef toVehicleDef)
  {
    this[fromVehicleDef].ReachabilityData.ChangeOwner(toVehicleDef);
  }

  [Flags]
  public enum GridSelection
  {
    None = 0,
    Regions = 1,
    PathGrids = 2,
    All = PathGrids | Regions, // 0x00000003
  }

  public enum GridDeferment
  {
    Lazy,
    Deferred,
    Forced,
  }

  public class VehiclePathData
  {
    public bool Suspended => !this.VehicleRegionAndRoomUpdater.Enabled;

    public VehiclePathingSystem.VehicleReachabilitySettings ReachabilityData { get; set; }

    public VehiclePathGrid VehiclePathGrid { get; set; }

    public VehicleRegionConnector VehicleRegionConnector { get; set; }

    public VehiclePathFinder VehiclePathFinder { get; set; }

    public VehicleReachability VehicleReachability => this.ReachabilityData.reachability;

    public VehicleRegionGrid VehicleRegionGrid => this.ReachabilityData.regionGrid;

    public VehicleRegionMaker VehicleRegionMaker => this.ReachabilityData.regionMaker;

    public VehicleRegionAndRoomUpdater VehicleRegionAndRoomUpdater
    {
      get => this.ReachabilityData.regionAndRoomUpdater;
    }

    public VehicleRegionDirtyer VehicleRegionDirtyer => this.ReachabilityData.regionDirtyer;
  }

  public class VehicleReachabilitySettings
  {
    public readonly VehicleRegionGrid regionGrid;
    public readonly VehicleRegionMaker regionMaker;
    public readonly VehicleRegionAndRoomUpdater regionAndRoomUpdater;
    public readonly VehicleRegionDirtyer regionDirtyer;
    public readonly VehicleReachability reachability;

    public VehicleReachabilitySettings(
      VehiclePathingSystem vehicleMapping,
      VehicleDef vehicleDef,
      VehiclePathingSystem.VehiclePathData pathData)
    {
      this.regionGrid = new VehicleRegionGrid(vehicleMapping, vehicleDef);
      this.regionMaker = new VehicleRegionMaker(vehicleMapping, vehicleDef);
      this.regionAndRoomUpdater = new VehicleRegionAndRoomUpdater(vehicleMapping, vehicleDef);
      this.regionDirtyer = new VehicleRegionDirtyer(vehicleMapping, vehicleDef);
      this.reachability = new VehicleReachability(vehicleMapping, vehicleDef, pathData.VehiclePathGrid, this.regionGrid);
    }

    public void PostInit()
    {
      this.regionGrid.PostInit();
      this.regionMaker.PostInit();
      this.regionAndRoomUpdater.PostInit();
      this.regionDirtyer.PostInit();
      this.reachability.PostInit();
    }

    public void ChangeOwner(VehicleDef vehicleDef)
    {
      this.regionGrid.createdFor = vehicleDef;
      this.regionMaker.createdFor = vehicleDef;
      this.regionAndRoomUpdater.createdFor = vehicleDef;
      this.regionDirtyer.createdFor = vehicleDef;
      this.reachability.createdFor = vehicleDef;
    }
  }
}
