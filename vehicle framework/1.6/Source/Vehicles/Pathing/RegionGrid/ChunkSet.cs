// Decompiled with JetBrains decompiler
// Type: Vehicles.ChunkSet
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public class ChunkSet
{
  private HashSet<IntVec3> cells;

  public HashSet<IntVec3> Cells => this.cells;

  public ChunkSet(List<VehicleRegion> regions)
  {
    this.CacheCells((IEnumerable<VehicleRegion>) regions);
  }

  public ChunkSet(HashSet<VehicleRegion> regions)
  {
    this.CacheCells((IEnumerable<VehicleRegion>) regions);
  }

  public bool NullOrEmpty() => this.cells == null || this.cells.Count == 0;

  private void CacheCells(IEnumerable<VehicleRegion> regions)
  {
    this.cells = new HashSet<IntVec3>();
    foreach (VehicleRegion region in regions)
      GenCollection.AddRange<IntVec3>(this.cells, region.Cells);
  }
}
