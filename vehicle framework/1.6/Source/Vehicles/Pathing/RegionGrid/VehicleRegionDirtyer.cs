// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleRegionDirtyer
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public class VehicleRegionDirtyer(VehiclePathingSystem mapping, VehicleDef createdFor) : 
  VehicleGridManager(mapping, createdFor)
{
  private VehicleRegionMaker regionMaker;
  private readonly ConcurrentSet<IntVec3> dirtyCells = new ConcurrentSet<IntVec3>();
  private readonly HashSet<VehicleRegion> regionsToDirty = new HashSet<VehicleRegion>();

  public bool AnyDirty => this.dirtyCells.Count > 0;

  public IEnumerable<IntVec3> DirtyCells
  {
    get
    {
      foreach (KeyValuePair<IntVec3, byte> dirtyCell1 in (ConcurrentDictionary<IntVec3, byte>) this.dirtyCells)
      {
        IntVec3 dirtyCell2;
        byte num;
        dirtyCell1.Deconstruct(ref dirtyCell2, ref num);
        yield return dirtyCell2;
      }
      this.dirtyCells.Clear();
    }
  }

  public override void PostInit()
  {
    this.regionMaker = this.mapping[this.createdFor].VehicleRegionMaker;
  }

  internal void SetAllDirty()
  {
    this.dirtyCells.Clear();
    foreach (IntVec3 intVec3 in this.mapping.map)
      this.dirtyCells.Add(intVec3);
    foreach (VehicleRegion region in this.mapping[this.createdFor].VehicleRegionGrid.AllRegionsNoRebuildInvalidAllowed)
      this.SetRegionDirty(region, false);
  }

  public void NotifyWalkabilityChanged(IntVec3 cell)
  {
    int num = this.createdFor.SizePadding > 0 ? this.createdFor.SizePadding : 1;
    CellRect cellRect = CellRect.CenteredOn(cell, num);
    foreach (IntVec3 cell1 in cellRect)
    {
      if (GenGrid.InBounds(cell1, this.mapping.map))
      {
        VehicleRegion regionAt = this.mapping[this.createdFor].VehicleRegionGrid.GetRegionAt(cell1);
        if (regionAt != null && regionAt.valid)
          this.SetRegionDirty(regionAt);
        else
          this.dirtyCells.Add(cell1);
      }
    }
  }

  public void NotifyThingAffectingRegionsSpawned(CellRect occupiedRect)
  {
    if (this.mapping[this.createdFor].Suspended)
      return;
    CellRect cellRect = ((CellRect) ref occupiedRect).ExpandedBy(this.createdFor.SizePadding + 1);
    cellRect = ((CellRect) ref cellRect).ClipInsideMap(this.mapping.map);
    foreach (IntVec3 cell in cellRect)
    {
      VehicleRegion validRegionAt = this.mapping[this.createdFor].VehicleRegionGrid.GetValidRegionAt(cell, false);
      if (validRegionAt != null)
        this.SetRegionDirty(validRegionAt);
    }
  }

  public void NotifyThingAffectingRegionsDespawned(CellRect occupiedRect)
  {
    if (this.mapping[this.createdFor].Suspended)
      return;
    CellRect cellRect = ((CellRect) ref occupiedRect).ExpandedBy(this.createdFor.SizePadding + 1);
    cellRect = ((CellRect) ref cellRect).ClipInsideMap(this.mapping.map);
    foreach (IntVec3 cell in cellRect)
    {
      if (GenGrid.InBounds(cell, this.mapping.map))
      {
        VehicleRegion validRegionAt = this.mapping[this.createdFor].VehicleRegionGrid.GetValidRegionAt(cell, false);
        if (validRegionAt != null)
          this.SetRegionDirty(validRegionAt);
      }
    }
  }

  private void SetRegionDirty(
    VehicleRegion region,
    bool addCellsToDirtyCells = true,
    bool dirtyLinkedRegions = false)
  {
    try
    {
      if (!region.valid)
        return;
      region.valid = false;
      region.Room = (VehicleRoom) null;
      using (ListSnapshot<VehicleRegionLink> links = region.Links)
      {
        foreach (VehicleRegionLink regionLink in links)
        {
          regionLink.Deregister(region);
          if (!regionLink.IsValid)
            this.regionMaker.Return(regionLink);
          VehicleRegion otherRegion = regionLink.GetOtherRegion(region);
          if (otherRegion != null & dirtyLinkedRegions)
            this.SetRegionDirty(otherRegion, addCellsToDirtyCells);
        }
        if (!addCellsToDirtyCells)
          return;
        foreach (IntVec3 cell in region.Cells)
          this.dirtyCells.Add(cell);
      }
    }
    catch (Exception ex)
    {
      Log.Error($"Exception thrown in SetRegionDirty. Exception={ex}");
    }
  }
}
