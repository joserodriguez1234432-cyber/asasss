// Decompiled with JetBrains decompiler
// Type: Vehicles.ComponentHitbox
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class ComponentHitbox
{
  public VehicleComponentPosition side;
  public IntVec2 from = IntVec2.Invalid;
  public IntVec2 to = IntVec2.Invalid;
  public List<IntVec2> cells = new List<IntVec2>();
  public bool fallthrough = true;

  public List<IntVec2> Hitbox { get; set; } = new List<IntVec2>();

  public bool Empty => this.Hitbox.Count == 0;

  public bool Contains(IntVec2 cell)
  {
    return !GenList.NullOrEmpty<IntVec2>((IList<IntVec2>) this.Hitbox) && this.Hitbox.Contains(cell);
  }

  public IntVec2 NearestTo(IntVec2 cell)
  {
    return this.Hitbox.Count == 1 ? this.Hitbox[0] : GenCollection.MinBy<IntVec2, float>((IEnumerable<IntVec2>) this.Hitbox, (Func<IntVec2, float>) (hb =>
    {
      IntVec2 intVec2 = IntVec2.op_Subtraction(hb, cell);
      return ((IntVec2) ref intVec2).Magnitude;
    }));
  }

  public void Initialize(VehicleDef def)
  {
    if (!GenList.NullOrEmpty<IntVec2>((IList<IntVec2>) this.cells))
      this.Hitbox.AddRange((IEnumerable<IntVec2>) this.cells);
    else if (((IntVec2) ref this.from).IsValid && ((IntVec2) ref this.to).IsValid)
    {
      CellRect cellRect = CellRect.FromLimits(((IntVec2) ref this.from).ToIntVec3, ((IntVec2) ref this.to).ToIntVec3);
      foreach (IntVec3 intVec3 in cellRect)
        this.Hitbox.Add(((IntVec3) ref intVec3).ToIntVec2);
    }
    else if (this.side == VehicleComponentPosition.Empty)
    {
      this.Hitbox.Add(IntVec2.Zero);
    }
    else
    {
      CellRect cellRect = def.VehicleRect(new IntVec3(0, 0, 0), Rot4.North);
      if (this.side == VehicleComponentPosition.Body)
      {
        foreach (IntVec3 cell in ((CellRect) ref cellRect).Cells)
          this.Hitbox.Add(((IntVec3) ref cell).ToIntVec2);
      }
      else
      {
        foreach (IntVec3 edgeCell in ((CellRect) ref cellRect).GetEdgeCells(ComponentHitbox.RotationFromSide(this.side)))
          this.Hitbox.Add(((IntVec3) ref edgeCell).ToIntVec2);
      }
    }
  }

  public static Rot4 RotationFromSide(VehicleComponentPosition pos)
  {
    Rot4 rot4;
    switch (pos)
    {
      case VehicleComponentPosition.Front:
        rot4 = Rot4.North;
        break;
      case VehicleComponentPosition.Right:
        rot4 = Rot4.East;
        break;
      case VehicleComponentPosition.Back:
        rot4 = Rot4.South;
        break;
      case VehicleComponentPosition.Left:
        rot4 = Rot4.West;
        break;
      default:
        rot4 = Rot4.Invalid;
        break;
    }
    return rot4;
  }
}
