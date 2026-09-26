// Decompiled with JetBrains decompiler
// Type: Vehicles.Hitbox
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace Vehicles;

public struct Hitbox : IEnumerable<IntVec2>, IEnumerable
{
  public VehicleComponentPosition side;
  public List<IntVec2> cells;

  public List<IntVec2> Cells { get; set; }

  public void Initialize(VehicleDef def)
  {
    if (!GenList.NullOrEmpty<IntVec2>((IList<IntVec2>) this.cells))
    {
      this.Cells = this.cells;
    }
    else
    {
      CellRect cellRect = def.VehicleRect(new IntVec3(0, 0, 0), Rot4.North);
      List<IntVec3> intVec3List = this.side != VehicleComponentPosition.Body ? (this.side == VehicleComponentPosition.Empty ? new List<IntVec3>() : ((CellRect) ref cellRect).GetEdgeCells(ComponentHitbox.RotationFromSide(this.side)).ToList<IntVec3>()) : ((CellRect) ref cellRect).Cells.ToList<IntVec3>();
      List<IntVec2> intVec2List = new List<IntVec2>();
      foreach (IntVec3 intVec3 in intVec3List)
        intVec2List.Add(new IntVec2(intVec3.x, intVec3.z));
      this.Cells = intVec2List;
    }
  }

  public IEnumerator<IntVec2> GetEnumerator()
  {
    foreach (IntVec2 cell in this.Cells)
      yield return cell;
  }

  IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.GetEnumerator();
}
