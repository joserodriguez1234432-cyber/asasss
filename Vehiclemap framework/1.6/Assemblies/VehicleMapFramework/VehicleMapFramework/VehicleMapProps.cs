// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleMapProps
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using JetBrains.Annotations;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

[UsedImplicitly]
public class VehicleMapProps : DefModExtension
{
  public IntVec2 size;
  public Vector3 offset = Vector3.zero;
  public Vector3? offsetNorth;
  public Vector3? offsetSouth;
  public Vector3? offsetEast;
  public Vector3? offsetWest;
  public Vector3? offsetNorthEast;
  public Vector3? offsetNorthWest;
  public Vector3? offsetSouthEast;
  public Vector3? offsetSouthWest;
  public List<IntVec2> filledStructureCells = new List<IntVec2>();
  public List<CellRect> filledStructureCellRects = new List<CellRect>();
  public List<IntVec2> emptyStructureCells = new List<IntVec2>();
  public List<CellRect> emptyStructureCellRects = new List<CellRect>();
  public List<IntVec2> expandableCells = new List<IntVec2>();
  public List<CellRect> expandableCellRects = new List<CellRect>();
  public List<IntVec2> outOfBoundsCells = new List<IntVec2>();
  public List<CellRect> outOfBoundsCellRects = new List<CellRect>();
  public VehicleMapProps.EdgeSpace edgeSpace;
  public VehicleMapProps.EdgeSpace? edgeSpaceNorth;
  public VehicleMapProps.EdgeSpace? edgeSpaceNorthEast;
  public VehicleMapProps.EdgeSpace? edgeSpaceEast;
  public VehicleMapProps.EdgeSpace? edgeSpaceSouthEast;
  public VehicleMapProps.EdgeSpace? edgeSpaceSouth;
  public VehicleMapProps.EdgeSpace? edgeSpaceSouthWest;
  public VehicleMapProps.EdgeSpace? edgeSpaceWest;
  public VehicleMapProps.EdgeSpace? edgeSpaceNorthWest;

  public IEnumerable<IntVec2> FilledStructureCells
  {
    get
    {
      return this.filledStructureCells.Union<IntVec2>(this.filledStructureCellRects.SelectMany<CellRect, IntVec2>((Func<CellRect, IEnumerable<IntVec2>>) (r => ((CellRect) ref r).Cells2D))).Select<IntVec2, IntVec2>((Func<IntVec2, IntVec2>) (c => IntVec2.op_Addition(c, IntVec2.One)));
    }
  }

  public IEnumerable<IntVec2> EmptyStructureCells
  {
    get
    {
      return this.emptyStructureCells.Union<IntVec2>(this.emptyStructureCellRects.SelectMany<CellRect, IntVec2>((Func<CellRect, IEnumerable<IntVec2>>) (r => ((CellRect) ref r).Cells2D))).Select<IntVec2, IntVec2>((Func<IntVec2, IntVec2>) (c => IntVec2.op_Addition(c, IntVec2.One)));
    }
  }

  public IEnumerable<IntVec2> ExpandableCells
  {
    get
    {
      return this.expandableCells.Union<IntVec2>(this.expandableCellRects.SelectMany<CellRect, IntVec2>((Func<CellRect, IEnumerable<IntVec2>>) (r => ((CellRect) ref r).Cells2D))).Select<IntVec2, IntVec2>((Func<IntVec2, IntVec2>) (c => IntVec2.op_Addition(c, IntVec2.One)));
    }
  }

  public IEnumerable<IntVec2> OutOfBoundsCells
  {
    get
    {
      CellRect cellRect = new CellRect(0, 0, this.size.x + 2, this.size.z + 2);
      return ((CellRect) ref cellRect).EdgeCells.Select<IntVec3, IntVec2>((Func<IntVec3, IntVec2>) (c => ((IntVec3) ref c).ToIntVec2)).Union<IntVec2>(this.outOfBoundsCells.Union<IntVec2>(this.outOfBoundsCellRects.SelectMany<CellRect, IntVec2>((Func<CellRect, IEnumerable<IntVec2>>) (r => ((CellRect) ref r).Cells2D))).Select<IntVec2, IntVec2>((Func<IntVec2, IntVec2>) (c => IntVec2.op_Addition(c, IntVec2.One))));
    }
  }

  public virtual IEnumerable<string> ConfigErrors()
  {
    CellRect mapRect = new CellRect(0, 0, this.size.x + 2, this.size.z + 2);
    foreach (IntVec2 intVec2 in this.FilledStructureCells.Union<IntVec2>(this.EmptyStructureCells))
    {
      if (!((CellRect) ref mapRect).Contains(((IntVec2) ref intVec2).ToIntVec3))
        yield return "[VehicleMapFramework] Structure cells contain out of map range.";
    }
    foreach (IntVec2 intVec2 in this.FilledStructureCells.Intersect<IntVec2>(this.EmptyStructureCells))
      yield return $"[VehicleMapFramework] Cell {intVec2} is designated both filled and empty structure.";
  }

  public float EdgeSpaceValue(Rot8 vehicleRot, Rot4 thingRot)
  {
    return this.EdgeSpaceByRot(vehicleRot).SpaceByRot(thingRot);
  }

  public VehicleMapProps.EdgeSpace EdgeSpaceByRot(Rot8 rot)
  {
    switch (((Rot8) ref rot).AsInt)
    {
      case 0:
        VehicleMapProps.EdgeSpace? edgeSpaceNorth = this.edgeSpaceNorth;
        if (edgeSpaceNorth.HasValue)
          return edgeSpaceNorth.GetValueOrDefault();
        ref VehicleMapProps.EdgeSpace? local1 = ref this.edgeSpaceSouth;
        return (this.edgeSpaceNorth = new VehicleMapProps.EdgeSpace?(local1.HasValue ? local1.GetValueOrDefault().FlipVertical() : this.edgeSpace)).Value;
      case 1:
        VehicleMapProps.EdgeSpace? edgeSpaceEast = this.edgeSpaceEast;
        if (edgeSpaceEast.HasValue)
          return edgeSpaceEast.GetValueOrDefault();
        ref VehicleMapProps.EdgeSpace? local2 = ref this.edgeSpaceWest;
        return (this.edgeSpaceEast = new VehicleMapProps.EdgeSpace?(local2.HasValue ? local2.GetValueOrDefault().FlipHorizontal() : this.edgeSpace)).Value;
      case 2:
        VehicleMapProps.EdgeSpace? edgeSpaceSouth = this.edgeSpaceSouth;
        if (edgeSpaceSouth.HasValue)
          return edgeSpaceSouth.GetValueOrDefault();
        ref VehicleMapProps.EdgeSpace? local3 = ref this.edgeSpaceNorth;
        return (this.edgeSpaceSouth = new VehicleMapProps.EdgeSpace?(local3.HasValue ? local3.GetValueOrDefault().FlipVertical() : this.edgeSpace)).Value;
      case 3:
        VehicleMapProps.EdgeSpace? edgeSpaceWest = this.edgeSpaceWest;
        if (edgeSpaceWest.HasValue)
          return edgeSpaceWest.GetValueOrDefault();
        ref VehicleMapProps.EdgeSpace? local4 = ref this.edgeSpaceEast;
        return (this.edgeSpaceWest = new VehicleMapProps.EdgeSpace?(local4.HasValue ? local4.GetValueOrDefault().FlipHorizontal() : this.edgeSpace)).Value;
      case 4:
        VehicleMapProps.EdgeSpace? edgeSpaceNorthEast = this.edgeSpaceNorthEast;
        if (edgeSpaceNorthEast.HasValue)
          return edgeSpaceNorthEast.GetValueOrDefault();
        ref VehicleMapProps.EdgeSpace? local5 = ref this.edgeSpaceNorth;
        VehicleMapProps.EdgeSpace diagonal1;
        if (!local5.HasValue)
        {
          ref VehicleMapProps.EdgeSpace? local6 = ref this.edgeSpaceSouth;
          VehicleMapProps.EdgeSpace valueOrDefault;
          VehicleMapProps.EdgeSpace edgeSpace;
          if (!local6.HasValue)
          {
            edgeSpace = this.edgeSpace;
          }
          else
          {
            valueOrDefault = local6.GetValueOrDefault();
            edgeSpace = valueOrDefault.FlipVertical();
          }
          valueOrDefault = (this.edgeSpaceNorth = new VehicleMapProps.EdgeSpace?(edgeSpace)).Value;
          diagonal1 = valueOrDefault.ToDiagonal();
        }
        else
          diagonal1 = local5.GetValueOrDefault().ToDiagonal();
        return (this.edgeSpaceNorthEast = new VehicleMapProps.EdgeSpace?(diagonal1)).Value;
      case 5:
        VehicleMapProps.EdgeSpace? edgeSpaceSouthEast = this.edgeSpaceSouthEast;
        if (edgeSpaceSouthEast.HasValue)
          return edgeSpaceSouthEast.GetValueOrDefault();
        ref VehicleMapProps.EdgeSpace? local7 = ref this.edgeSpaceSouth;
        VehicleMapProps.EdgeSpace diagonal2;
        if (!local7.HasValue)
        {
          ref VehicleMapProps.EdgeSpace? local8 = ref this.edgeSpaceNorth;
          VehicleMapProps.EdgeSpace valueOrDefault;
          VehicleMapProps.EdgeSpace edgeSpace;
          if (!local8.HasValue)
          {
            edgeSpace = this.edgeSpace;
          }
          else
          {
            valueOrDefault = local8.GetValueOrDefault();
            edgeSpace = valueOrDefault.FlipVertical();
          }
          valueOrDefault = (this.edgeSpaceSouth = new VehicleMapProps.EdgeSpace?(edgeSpace)).Value;
          diagonal2 = valueOrDefault.ToDiagonal();
        }
        else
          diagonal2 = local7.GetValueOrDefault().ToDiagonal();
        return (this.edgeSpaceSouthEast = new VehicleMapProps.EdgeSpace?(diagonal2)).Value;
      case 6:
        VehicleMapProps.EdgeSpace? edgeSpaceSouthWest = this.edgeSpaceSouthWest;
        if (edgeSpaceSouthWest.HasValue)
          return edgeSpaceSouthWest.GetValueOrDefault();
        ref VehicleMapProps.EdgeSpace? local9 = ref this.edgeSpaceSouth;
        VehicleMapProps.EdgeSpace diagonal3;
        if (!local9.HasValue)
        {
          ref VehicleMapProps.EdgeSpace? local10 = ref this.edgeSpaceNorth;
          VehicleMapProps.EdgeSpace valueOrDefault;
          VehicleMapProps.EdgeSpace edgeSpace;
          if (!local10.HasValue)
          {
            edgeSpace = this.edgeSpace;
          }
          else
          {
            valueOrDefault = local10.GetValueOrDefault();
            edgeSpace = valueOrDefault.FlipVertical();
          }
          valueOrDefault = (this.edgeSpaceSouth = new VehicleMapProps.EdgeSpace?(edgeSpace)).Value;
          diagonal3 = valueOrDefault.ToDiagonal();
        }
        else
          diagonal3 = local9.GetValueOrDefault().ToDiagonal();
        return (this.edgeSpaceSouthWest = new VehicleMapProps.EdgeSpace?(diagonal3)).Value;
      case 7:
        VehicleMapProps.EdgeSpace? edgeSpaceNorthWest = this.edgeSpaceNorthWest;
        if (edgeSpaceNorthWest.HasValue)
          return edgeSpaceNorthWest.GetValueOrDefault();
        ref VehicleMapProps.EdgeSpace? local11 = ref this.edgeSpaceNorth;
        VehicleMapProps.EdgeSpace diagonal4;
        if (!local11.HasValue)
        {
          ref VehicleMapProps.EdgeSpace? local12 = ref this.edgeSpaceSouth;
          VehicleMapProps.EdgeSpace valueOrDefault;
          VehicleMapProps.EdgeSpace edgeSpace;
          if (!local12.HasValue)
          {
            edgeSpace = this.edgeSpace;
          }
          else
          {
            valueOrDefault = local12.GetValueOrDefault();
            edgeSpace = valueOrDefault.FlipVertical();
          }
          valueOrDefault = (this.edgeSpaceNorth = new VehicleMapProps.EdgeSpace?(edgeSpace)).Value;
          diagonal4 = valueOrDefault.ToDiagonal();
        }
        else
          diagonal4 = local11.GetValueOrDefault().ToDiagonal();
        return (this.edgeSpaceNorthWest = new VehicleMapProps.EdgeSpace?(diagonal4)).Value;
      default:
        return this.edgeSpace;
    }
  }

  public struct EdgeSpace
  {
    public float space;
    public float? north;
    public float? east;
    public float? south;
    public float? west;
    private const float sin45 = 0.707106769f;

    public float SpaceByRot(Rot4 rot)
    {
      switch (((Rot4) ref rot).AsInt)
      {
        case 0:
          float? north = this.north;
          if (north.HasValue)
            return north.GetValueOrDefault();
          float? nullable1 = this.south;
          double num1 = (double) nullable1 ?? (double) this.space;
          this.north = nullable1 = new float?((float) num1);
          nullable1 = nullable1;
          return nullable1.Value;
        case 1:
          float? east = this.east;
          if (east.HasValue)
            return east.GetValueOrDefault();
          float? nullable2 = this.west;
          double num2 = (double) nullable2 ?? (double) this.space;
          this.east = nullable2 = new float?((float) num2);
          nullable2 = nullable2;
          return nullable2.Value;
        case 2:
          float? south = this.south;
          if (south.HasValue)
            return south.GetValueOrDefault();
          float? nullable3 = this.north;
          double num3 = (double) nullable3 ?? (double) this.space;
          this.south = nullable3 = new float?((float) num3);
          nullable3 = nullable3;
          return nullable3.Value;
        case 3:
          float? west = this.west;
          if (west.HasValue)
            return west.GetValueOrDefault();
          float? nullable4 = this.east;
          double num4 = (double) nullable4 ?? (double) this.space;
          this.west = nullable4 = new float?((float) num4);
          nullable4 = nullable4;
          return nullable4.Value;
        default:
          return this.space;
      }
    }

    public VehicleMapProps.EdgeSpace ToDiagonal()
    {
      VehicleMapProps.EdgeSpace diagonal = new VehicleMapProps.EdgeSpace();
      diagonal.space = this.space * 0.707106769f;
      ref VehicleMapProps.EdgeSpace local1 = ref diagonal;
      float? north = this.north;
      float num1 = 0.707106769f;
      float? nullable1 = north.HasValue ? new float?(north.GetValueOrDefault() * num1) : new float?();
      local1.north = nullable1;
      ref VehicleMapProps.EdgeSpace local2 = ref diagonal;
      float? nullable2 = this.east;
      float num2 = 0.707106769f;
      float? nullable3 = nullable2.HasValue ? new float?(nullable2.GetValueOrDefault() * num2) : new float?();
      local2.east = nullable3;
      ref VehicleMapProps.EdgeSpace local3 = ref diagonal;
      nullable2 = this.south;
      float num3 = 0.707106769f;
      float? nullable4 = nullable2.HasValue ? new float?(nullable2.GetValueOrDefault() * num3) : new float?();
      local3.south = nullable4;
      ref VehicleMapProps.EdgeSpace local4 = ref diagonal;
      nullable2 = this.west;
      float num4 = 0.707106769f;
      float? nullable5 = nullable2.HasValue ? new float?(nullable2.GetValueOrDefault() * num4) : new float?();
      local4.west = nullable5;
      return diagonal;
    }

    public VehicleMapProps.EdgeSpace FlipHorizontal()
    {
      return this with
      {
        east = this.west,
        west = this.east
      };
    }

    public VehicleMapProps.EdgeSpace FlipVertical()
    {
      return this with
      {
        north = this.south,
        south = this.north
      };
    }
  }
}
