// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleRegionGrid
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Verse;

#nullable disable
namespace Vehicles;

public sealed class VehicleRegionGrid(VehiclePathingSystem mapping, VehicleDef createdFor) : 
  VehicleGridManager(mapping, createdFor)
{
  private const int CleanSquaresPerFrame = 16 /*0x10*/;
  private readonly ThreadLocal<HashSet<VehicleRegion>> allRegionsYielded = new ThreadLocal<HashSet<VehicleRegion>>((Func<HashSet<VehicleRegion>>) (() => new HashSet<VehicleRegion>()));
  private int curCleanIndex;
  private VehicleRegion[] regionGrid;
  public ConcurrentSet<VehicleRoom> allRooms = new ConcurrentSet<VehicleRoom>();
  private VehicleRegionAndRoomUpdater regionUpdater;

  public VehicleRegion[] DirectGrid => this.regionGrid;

  public IEnumerable<VehicleRegion> AllRegionsNoRebuildInvalidAllowed
  {
    get
    {
      VehicleRegionGrid vehicleRegionGrid = this;
      VehicleRegionAndRoomUpdater regionUpdater = vehicleRegionGrid.regionUpdater;
      if (regionUpdater != null && regionUpdater.Enabled)
      {
        try
        {
          int count = ((CellIndices) ref vehicleRegionGrid.mapping.map.cellIndices).NumGridCells;
          for (int i = 0; i < count; ++i)
          {
            VehicleRegion regionAt = vehicleRegionGrid.GetRegionAt(i);
            if (regionAt != null && vehicleRegionGrid.allRegionsYielded.Value.Add(regionAt))
              yield return regionAt;
          }
        }
        finally
        {
          this.allRegionsYielded.Value.Clear();
        }
      }
    }
  }

  internal bool AnyInvalidRegions
  {
    get
    {
      VehicleRegionAndRoomUpdater regionUpdater = this.regionUpdater;
      if (regionUpdater == null || !regionUpdater.Enabled)
        return false;
      foreach (VehicleRegion vehicleRegion in this.regionGrid)
      {
        if (vehicleRegion != null && !vehicleRegion.valid)
          return true;
      }
      return false;
    }
  }

  public void GetAllRegions(List<VehicleRegion> regions)
  {
    VehicleRegionAndRoomUpdater regionUpdater = this.regionUpdater;
    if (regionUpdater == null || !regionUpdater.Enabled)
      return;
    try
    {
      Parallel.ForEach<Tuple<int, int>>((Partitioner<Tuple<int, int>>) Partitioner.Create(0, ((CellIndices) ref this.mapping.map.cellIndices).NumGridCells), (Action<Tuple<int, int>, ParallelLoopState>) ((range, _) =>
      {
        for (int index = range.Item1; index < range.Item2; ++index)
        {
          VehicleRegion regionAt = this.GetRegionAt(index);
          if (regionAt != null && regionAt.valid && this.allRegionsYielded.Value.Add(regionAt))
            regions.Add(regionAt);
        }
      }));
    }
    finally
    {
      this.allRegionsYielded.Value.Clear();
    }
  }

  public void Release()
  {
    this.regionGrid = (VehicleRegion[]) null;
    this.allRooms.Clear();
  }

  public void Init()
  {
    if (this.regionGrid == null)
      this.regionGrid = new VehicleRegion[((CellIndices) ref this.mapping.map.cellIndices).NumGridCells];
    if (this.regionUpdater != null)
      return;
    this.regionUpdater = this.mapping[this.createdFor].VehicleRegionAndRoomUpdater;
  }

  public VehicleRegion GetValidRegionAt(IntVec3 cell, bool rebuild = true)
  {
    if (!GenGrid.InBounds(cell, this.mapping.map))
    {
      Log.Error($"Tried to get valid vehicle region for {this.createdFor} out of bounds at {cell}");
      return (VehicleRegion) null;
    }
    if (rebuild)
    {
      if (!this.regionUpdater.Enabled && this.regionUpdater.AnythingToRebuild)
        Log.Warning($"Trying to get valid vehicle region for {this.createdFor} at {cell} but " + "RegionAndRoomUpdater is disabled. The result may be incorrect.");
      this.regionUpdater.TryRebuildVehicleRegions();
    }
    VehicleRegion regionAt = this.GetRegionAt(cell);
    return regionAt == null || !regionAt.valid ? (VehicleRegion) null : regionAt;
  }

  public VehicleRegion GetRegionAt(IntVec3 cell)
  {
    return this.GetRegionAt(((CellIndices) ref this.mapping.map.cellIndices).CellToIndex(cell));
  }

  public VehicleRegion GetRegionAt(int index) => this.regionGrid?[index];

  public void SetRegionAt(IntVec3 cell, VehicleRegion region)
  {
    this.SetRegionAt(((CellIndices) ref this.mapping.map.cellIndices).CellToIndex(cell), region);
  }

  public void SetRegionAt(int index, VehicleRegion region)
  {
    this.regionGrid[index]?.DecrementRefCount();
    region?.IncrementRefCount();
    Interlocked.CompareExchange<VehicleRegion>(ref this.regionGrid[index], region, this.regionGrid[index]);
  }

  internal void ClearFromGrid(VehicleRegion region)
  {
    foreach (IntVec3 cell in region.Cells)
    {
      int index = ((CellIndices) ref this.mapping.map.cellIndices).CellToIndex(cell);
      Interlocked.CompareExchange<VehicleRegion>(ref this.regionGrid[index], (VehicleRegion) null, this.regionGrid[index]);
    }
    region.Reset();
  }

  public void UpdateClean()
  {
    for (int index = 0; index < 16 /*0x10*/; ++index)
    {
      if (this.curCleanIndex >= this.regionGrid.Length)
        this.curCleanIndex = 0;
      VehicleRegion vehicleRegion = this.regionGrid[this.curCleanIndex];
      if (vehicleRegion != null && !vehicleRegion.valid)
      {
        Trace.Fail("Cleaning region which should have already been returned to pool.");
        this.SetRegionAt(this.curCleanIndex, (VehicleRegion) null);
      }
      ++this.curCleanIndex;
    }
  }

  public void DebugDraw(DebugRegionType debugRegionType)
  {
    if (this.mapping.map != Find.CurrentMap)
      return;
    foreach (VehicleRoom key in (IEnumerable<VehicleRoom>) this.allRooms.Keys)
      key.DebugDraw(debugRegionType);
    if (DebugProperties.DrawAllRegions)
    {
      foreach (VehicleRegion vehicleRegion in this.AllRegionsNoRebuildInvalidAllowed)
        vehicleRegion.DebugDraw(debugRegionType);
    }
    IntVec3 cell = UI.MouseCell();
    if (!GenGrid.InBounds(cell, this.mapping.map))
      return;
    this.GetRegionAt(cell)?.DebugDraw(debugRegionType);
  }

  public void DebugOnGUI(DebugRegionType debugRegionType)
  {
    IntVec3 cell = UI.MouseCell();
    if (!GenGrid.InBounds(cell, this.mapping.map))
      return;
    this.GetRegionAt(cell)?.DebugOnGUIMouseover(debugRegionType);
  }
}
