// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleRegionAndRoomUpdater
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using SmashTools.Performance;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class VehicleRegionAndRoomUpdater(VehiclePathingSystem mapping, VehicleDef createdFor) : 
  VehicleGridManager(mapping, createdFor)
{
  private readonly List<VehicleRegion> newRegions = new List<VehicleRegion>();
  private readonly List<VehicleRoom> newRooms = new List<VehicleRoom>();
  private readonly HashSet<VehicleRoom> reusedOldRooms = new HashSet<VehicleRoom>();
  private readonly List<VehicleRegion> currentRegionGroup = new List<VehicleRegion>();
  private VehicleRegionGrid regionGrid;

  public bool Initialized { get; private set; }

  public bool UpdatingRegion { get; private set; }

  internal int UpdatingFromThreadId { get; private set; }

  public bool Enabled { get; private set; }

  public bool AnythingToRebuild
  {
    get
    {
      if (this.UpdatingRegion || !this.Enabled)
        return false;
      return !this.Initialized || this.mapping[this.createdFor].VehicleRegionDirtyer.AnyDirty;
    }
  }

  public void Init()
  {
    if (!this.mapping[this.createdFor].VehiclePathGrid.Enabled && !this.mapping.GridOwners.TryForfeitOwnership(this.createdFor))
    {
      Trace.Fail("Trying to initialize region grids with no vehicle to claim ownership.");
    }
    else
    {
      this.Enabled = true;
      this.regionGrid = this.mapping[this.createdFor].VehicleRegionGrid;
      this.regionGrid.Init();
    }
  }

  public void Release()
  {
    this.Initialized = false;
    this.Enabled = false;
    this.regionGrid.Release();
  }

  public void Disable() => this.Enabled = false;

  public void RebuildAllVehicleRegions()
  {
    if (!this.Enabled)
      Log.Warning("Called RebuildAllVehicleRegions but VehicleRegionAndRoomUpdater is disabled. VehicleRegions won't be rebuilt. StackTrace: " + StackTraceUtility.ExtractStackTrace());
    this.mapping[this.createdFor].VehicleRegionDirtyer.SetAllDirty();
    this.TryRebuildVehicleRegions();
  }

  public void TryRebuildVehicleRegions()
  {
    if (this.UpdatingRegion || !this.Enabled)
      return;
    this.UpdatingRegion = true;
    if (!this.Initialized)
      this.mapping[this.createdFor].VehicleRegionDirtyer.SetAllDirty();
    else if (!this.mapping[this.createdFor].VehicleRegionDirtyer.AnyDirty)
    {
      this.UpdatingRegion = false;
      return;
    }
    try
    {
      this.RegenerateNewVehicleRegions();
      this.CreateOrUpdateVehicleRooms();
    }
    finally
    {
      this.newRegions.Clear();
      this.Initialized = true;
      this.UpdatingRegion = false;
    }
  }

  [Profile]
  private void RegenerateNewVehicleRegions()
  {
    this.newRegions.Clear();
    VehiclePathingSystem.VehiclePathData vehiclePathData = this.mapping[this.createdFor];
    foreach (IntVec3 dirtyCell in vehiclePathData.VehicleRegionDirtyer.DirtyCells)
    {
      if (!GenGrid.InBounds(dirtyCell, this.mapping.map))
      {
        Trace.Fail($"Dirtied invalid cell at {dirtyCell}");
      }
      else
      {
        VehicleRegion regionAt = vehiclePathData.VehicleRegionGrid.GetRegionAt(dirtyCell);
        if (regionAt == null || !regionAt.valid)
        {
          switch (vehiclePathData.VehicleRegionMaker.TryGenerateRegionFrom(dirtyCell, ref regionAt))
          {
            case VehicleRegionMaker.RegionResult.NoRegion:
              if (regionAt != null)
              {
                this.regionGrid.SetRegionAt(dirtyCell, (VehicleRegion) null);
                continue;
              }
              continue;
            case VehicleRegionMaker.RegionResult.Success:
              this.newRegions.Add(regionAt);
              continue;
            default:
              continue;
          }
        }
      }
    }
  }

  [Profile]
  private void CreateOrUpdateVehicleRooms()
  {
    this.newRooms.Clear();
    this.reusedOldRooms.Clear();
    this.CreateOrAttachToExistingRooms(this.CombineNewRegionsIntoContiguousGroups());
    this.CombineNewAndReusedRoomsIntoContiguousGroups();
    this.newRooms.Clear();
    this.reusedOldRooms.Clear();
  }

  private int CombineNewAndReusedRoomsIntoContiguousGroups()
  {
    int newRegionGroupIndex = 0;
    for (int index = 0; index < this.newRegions.Count; ++index)
    {
      if (this.newRegions[index].newRegionGroupIndex < 0)
      {
        VehicleRegionTraverser.FloodAndSetNewRegionIndex(this.newRegions[index], newRegionGroupIndex);
        ++newRegionGroupIndex;
      }
    }
    return newRegionGroupIndex;
  }

  private void CreateOrAttachToExistingRooms(int numRegionGroups)
  {
    for (int index1 = 0; index1 < numRegionGroups; ++index1)
    {
      this.currentRegionGroup.Clear();
      for (int index2 = 0; index2 < this.newRegions.Count; ++index2)
      {
        if (this.newRegions[index2].newRegionGroupIndex == index1)
          this.currentRegionGroup.Add(this.newRegions[index2]);
      }
      if (!RegionTypeUtility.AllowsMultipleRegionsPerDistrict(this.currentRegionGroup[0].type))
      {
        if (this.currentRegionGroup.Count != 1)
          Log.Error("Region type doesn't allow multiple regions per room but there are >1 regions in this group.");
        VehicleRoom vehicleRoom = VehicleRoom.MakeNew(this.mapping.map, this.createdFor);
        this.currentRegionGroup[0].Room = vehicleRoom;
        this.newRooms.Add(vehicleRoom);
      }
      else
      {
        bool multipleOldNeighborRooms;
        VehicleRoom neighborWithMostRegions = this.FindCurrentRegionGroupNeighborWithMostRegions(out multipleOldNeighborRooms);
        if (neighborWithMostRegions == null)
          this.newRooms.Add(VehicleRegionTraverser.FloodAndSetRooms(this.currentRegionGroup[0], this.mapping.map, this.createdFor, (VehicleRoom) null));
        else if (!multipleOldNeighborRooms)
        {
          for (int index3 = 0; index3 < this.currentRegionGroup.Count; ++index3)
            this.currentRegionGroup[index3].Room = neighborWithMostRegions;
          this.reusedOldRooms.Add(neighborWithMostRegions);
        }
        else
        {
          VehicleRegionTraverser.FloodAndSetRooms(this.currentRegionGroup[0], this.mapping.map, this.createdFor, neighborWithMostRegions);
          this.reusedOldRooms.Add(neighborWithMostRegions);
        }
      }
    }
  }

  private int CombineNewRegionsIntoContiguousGroups()
  {
    int newRegionGroupIndex = 0;
    for (int index = 0; index < this.newRegions.Count; ++index)
    {
      if (this.newRegions[index].newRegionGroupIndex < 0)
      {
        VehicleRegionTraverser.FloodAndSetNewRegionIndex(this.newRegions[index], newRegionGroupIndex);
        ++newRegionGroupIndex;
      }
    }
    return newRegionGroupIndex;
  }

  private VehicleRoom FindCurrentRegionGroupNeighborWithMostRegions(
    out bool multipleOldNeighborRooms)
  {
    multipleOldNeighborRooms = false;
    VehicleRoom neighborWithMostRegions = (VehicleRoom) null;
    for (int index = 0; index < this.currentRegionGroup.Count; ++index)
    {
      foreach (VehicleRegion vehicleRegion in this.currentRegionGroup[index].NeighborsOfSameType)
      {
        if (vehicleRegion.Room != null && !this.reusedOldRooms.Contains(vehicleRegion.Room))
        {
          if (neighborWithMostRegions == null)
            neighborWithMostRegions = vehicleRegion.Room;
          else if (vehicleRegion.Room != neighborWithMostRegions)
          {
            multipleOldNeighborRooms = true;
            if (vehicleRegion.Room.RegionCount > neighborWithMostRegions.RegionCount)
              neighborWithMostRegions = vehicleRegion.Room;
          }
        }
      }
    }
    return neighborWithMostRegions;
  }
}
