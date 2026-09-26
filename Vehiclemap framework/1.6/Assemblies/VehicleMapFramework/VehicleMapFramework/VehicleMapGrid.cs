// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleMapGrid
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class VehicleMapGrid : MapComponent
{
  private readonly VehiclePawnWithMap[] vehicleGrid;

  public VehicleMapGrid(Map map)
  {
    this.vehicleGrid = new VehiclePawnWithMap[((CellIndices) ref map.cellIndices).NumGridCells];
    // ISSUE: explicit constructor call
    base.\u002Ector(map);
  }

  public Dictionary<VehiclePawnWithMap, HashSet<IntVec3>> OccupiedCells { get; }

  public VehiclePawnWithMap VehicleAt(IntVec3 c)
  {
    return this.vehicleGrid[((CellIndices) ref this.map.cellIndices).CellToIndex(c)];
  }

  public Map VehicleMapAt(IntVec3 c)
  {
    return this.vehicleGrid[((CellIndices) ref this.map.cellIndices).CellToIndex(c)]?.VehicleMap;
  }

  public void Register(IntVec3 c, VehiclePawnWithMap vehicle)
  {
    this.vehicleGrid[((CellIndices) ref this.map.cellIndices).CellToIndex(c)] = vehicle;
  }

  public void DeRegister(IntVec3 c)
  {
    this.vehicleGrid[((CellIndices) ref this.map.cellIndices).CellToIndex(c)] = (VehiclePawnWithMap) null;
  }

  public virtual void MapComponentUpdate()
  {
    if (!VehicleMapFramework.VehicleMapFramework.settings.drawVehicleMapGrid)
      return;
    this.DebugDraw();
  }

  internal void DebugDraw()
  {
    for (int index = 0; index < this.vehicleGrid.Length; ++index)
    {
      if (this.vehicleGrid[index] != null)
        CellRenderer.RenderCell(((CellIndices) ref this.map.cellIndices).IndexToCell(index), 0.5f);
    }
  }
}
