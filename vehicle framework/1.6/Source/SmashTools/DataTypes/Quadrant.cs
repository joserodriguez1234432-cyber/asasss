// Decompiled with JetBrains decompiler
// Type: SmashTools.Quadrant
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace SmashTools;

public struct Quadrant
{
  private byte quadInt;

  public Quadrant(byte q) => this.quadInt = q;

  public Quadrant(int q) => this.quadInt = (byte) q.Clamp(1, 4);

  public static Quadrant Q1 => new Quadrant(1);

  public static Quadrant Q2 => new Quadrant(2);

  public static Quadrant Q3 => new Quadrant(3);

  public static Quadrant Q4 => new Quadrant(4);

  public static Quadrant Invalid
  {
    get => new Quadrant() { quadInt = 100 };
  }

  public int AsInt
  {
    get => (int) this.quadInt;
    set => this.quadInt = (byte) value.Clamp(1, 4);
  }

  public static Quadrant QuadrantOfIntVec3(IntVec3 c, Map map)
  {
    if (c.x > map.Size.x / 2 && c.z >= map.Size.z / 2)
      return Quadrant.Q1;
    if (c.x >= map.Size.x / 2 && c.z < map.Size.z / 2)
      return Quadrant.Q2;
    if (c.x < map.Size.x / 2 && c.z <= map.Size.z / 2)
      return Quadrant.Q3;
    if (c.x <= map.Size.x / 2 && c.z > map.Size.z / 2)
      return Quadrant.Q4;
    return c.x == map.Size.x / 2 && c.z == map.Size.z / 2 ? Quadrant.Q1 : Quadrant.Invalid;
  }

  public static Quadrant QuadrantRelativeToPoint(IntVec3 c, IntVec3 point, Map map)
  {
    if (c.x > point.x && c.z >= point.z)
      return Quadrant.Q1;
    if (c.x >= point.x && c.z < point.z)
      return Quadrant.Q2;
    if (c.x < point.x && c.z <= point.z)
      return Quadrant.Q3;
    if (c.x <= point.x && c.z > point.z)
      return Quadrant.Q4;
    return c.x == point.x && c.z == point.z ? Quadrant.Q1 : Quadrant.Invalid;
  }

  public static IEnumerable<IntVec3> CellsInQuadrant(Quadrant q, Map map)
  {
    switch (q.AsInt)
    {
      case 1:
        CellRect cellRect1 = CellRect.WholeMap(map);
        return ((CellRect) ref cellRect1).Cells.Where<IntVec3>((Func<IntVec3, bool>) (c2 => c2.x > map.Size.x / 2 && c2.z >= map.Size.z / 2));
      case 2:
        CellRect cellRect2 = CellRect.WholeMap(map);
        return ((CellRect) ref cellRect2).Cells.Where<IntVec3>((Func<IntVec3, bool>) (c2 => c2.x <= map.Size.x / 2 && c2.z < map.Size.z / 2));
      case 3:
        CellRect cellRect3 = CellRect.WholeMap(map);
        return ((CellRect) ref cellRect3).Cells.Where<IntVec3>((Func<IntVec3, bool>) (c2 => c2.x < map.Size.x / 2 && c2.z <= map.Size.z / 2));
      case 4:
        CellRect cellRect4 = CellRect.WholeMap(map);
        return ((CellRect) ref cellRect4).Cells.Where<IntVec3>((Func<IntVec3, bool>) (c2 => c2.x <= map.Size.x / 2 && c2.z > map.Size.z / 2));
      default:
        throw new NotImplementedException("Quadrant Int is not valid.");
    }
  }

  public override string ToString() => this.quadInt.ToString();

  public static Quadrant FromString(string innerText)
  {
    byte result;
    if (byte.TryParse(innerText, out result))
      return new Quadrant(result);
    Log.Error("Unable to parse Quadrant: " + innerText);
    return Quadrant.Invalid;
  }
}
