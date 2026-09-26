// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleRegionCostCalculatorWrapper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class VehicleRegionCostCalculatorWrapper
{
  private readonly VehiclePathingSystem mapping;
  private readonly VehicleDef vehicleDef;
  private IntVec3 endCell;
  private float moveTicksCardinal;
  private float moveTicksDiagonal;
  private VehicleRegionCostCalculator vehicleRegionCostCalculator;
  private VehicleRegion cachedRegion;
  private VehicleRegionLink cachedBestLink;
  private VehicleRegionLink cachedSecondBestLink;
  private readonly HashSet<VehicleRegion> destRegions = new HashSet<VehicleRegion>();
  private int cachedBestLinkCost;
  private int cachedSecondBestLinkCost;
  private bool cachedRegionIsDestination;

  public VehicleRegionCostCalculatorWrapper(VehiclePathingSystem mapping, VehicleDef vehicleDef)
  {
    this.mapping = mapping;
    this.vehicleDef = vehicleDef;
    this.vehicleRegionCostCalculator = new VehicleRegionCostCalculator(mapping, this.vehicleDef);
  }

  public void Init(
    CellRect end,
    TraverseParms traverseParms,
    float moveTicksCardinal,
    float moveTicksDiagonal,
    AvoidGrid avoidGrid,
    bool drafted,
    List<int> disallowedCorners)
  {
    this.moveTicksCardinal = moveTicksCardinal;
    this.moveTicksDiagonal = moveTicksDiagonal;
    this.endCell = ((CellRect) ref end).CenterCell;
    this.cachedRegion = (VehicleRegion) null;
    this.cachedBestLink = (VehicleRegionLink) null;
    this.cachedSecondBestLink = (VehicleRegionLink) null;
    this.cachedBestLinkCost = 0;
    this.cachedSecondBestLinkCost = 0;
    this.cachedRegionIsDestination = false;
    this.destRegions.Clear();
    if (((CellRect) ref end).Width == 1 && ((CellRect) ref end).Height == 1)
    {
      VehicleRegion vehicleRegion = VehicleRegionAndRoomQuery.RegionAt(this.endCell, this.mapping, this.vehicleDef);
      if (vehicleRegion != null)
        this.destRegions.Add(vehicleRegion);
    }
    else
    {
      foreach (IntVec3 cell in end)
      {
        if (GenGrid.InBounds(cell, this.mapping.map) && !disallowedCorners.Contains(((CellIndices) ref this.mapping.map.cellIndices).CellToIndex(cell)))
        {
          VehicleRegion vehicleRegion = VehicleRegionAndRoomQuery.RegionAt(cell, this.mapping, this.vehicleDef);
          if (vehicleRegion != null && vehicleRegion.Allows(traverseParms))
            this.destRegions.Add(vehicleRegion);
        }
      }
    }
    if (this.destRegions.Count == 0)
      Log.Error("Couldn't find any destination regions. This shouldn't ever happen because we've checked reachability.");
    this.vehicleRegionCostCalculator.Init(end, this.destRegions, traverseParms, moveTicksCardinal, moveTicksDiagonal, avoidGrid, drafted);
  }

  public int GetPathCostFromDestToRegion(int cellIndex)
  {
    VehicleRegion region = this.mapping[this.vehicleDef].VehicleRegionGrid.DirectGrid[cellIndex];
    IntVec3 cell = ((CellIndices) ref this.mapping.map.cellIndices).IndexToCell(cellIndex);
    if (region != this.cachedRegion)
    {
      this.cachedRegionIsDestination = this.destRegions.Contains(region);
      if (this.cachedRegionIsDestination)
        return this.OctileDistanceToEnd(cell);
      this.cachedBestLinkCost = this.vehicleRegionCostCalculator.GetRegionBestDistances(region, out this.cachedBestLink, out this.cachedSecondBestLink, out this.cachedSecondBestLinkCost);
      this.cachedRegion = region;
    }
    else if (this.cachedRegionIsDestination)
      return this.OctileDistanceToEnd(cell);
    if (this.cachedBestLink == null)
      return 10000;
    int num = this.vehicleRegionCostCalculator.RegionLinkDistance(cell, this.cachedBestLink, 1);
    return this.cachedSecondBestLink != null ? Mathf.Min(this.cachedSecondBestLinkCost + this.vehicleRegionCostCalculator.RegionLinkDistance(cell, this.cachedSecondBestLink, 1), this.cachedBestLinkCost + num) + this.OctileDistanceToEndEps(cell) : this.cachedBestLinkCost + num + this.OctileDistanceToEndEps(cell);
  }

  private int OctileDistanceToEnd(IntVec3 cell)
  {
    return GenMath.OctileDistance(Mathf.Abs(cell.x - this.endCell.x), Mathf.Abs(cell.z - this.endCell.z), Mathf.RoundToInt(this.moveTicksCardinal), Mathf.RoundToInt(this.moveTicksDiagonal));
  }

  private int OctileDistanceToEndEps(IntVec3 cell)
  {
    return GenMath.OctileDistance(Mathf.Abs(cell.x - this.endCell.x), Mathf.Abs(cell.z - this.endCell.z), 2, 3);
  }
}
