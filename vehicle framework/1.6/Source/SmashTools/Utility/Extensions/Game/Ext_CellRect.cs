// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_CellRect
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace SmashTools;

[UsedImplicitly]
public static class Ext_CellRect
{
  public static IEnumerable<IntVec3> Cardinals(this CellRect cellRect)
  {
    if (!((CellRect) ref cellRect).IsEmpty)
    {
      if (((CellRect) ref cellRect).Area == 1)
        yield return new IntVec3(cellRect.minX, 0, cellRect.minZ);
      else if (((CellRect) ref cellRect).Width == 2 && ((CellRect) ref cellRect).Height == 2)
      {
        yield return new IntVec3(cellRect.maxX, 0, cellRect.maxZ);
        yield return new IntVec3(cellRect.maxX, 0, cellRect.minZ);
        yield return new IntVec3(cellRect.minX, 0, cellRect.minZ);
        yield return new IntVec3(cellRect.minX, 0, cellRect.maxZ);
      }
      else
      {
        int x;
        if (((CellRect) ref cellRect).Height > 1)
        {
          x = cellRect.minX + ((CellRect) ref cellRect).Width / 2;
          yield return new IntVec3(x, 0, cellRect.maxZ);
          yield return new IntVec3(x, 0, cellRect.minZ);
        }
        if (((CellRect) ref cellRect).Width > 1)
        {
          x = cellRect.minZ + ((CellRect) ref cellRect).Height / 2;
          yield return new IntVec3(cellRect.maxX, 0, x);
          yield return new IntVec3(cellRect.minX, 0, x);
        }
      }
    }
  }

  public static CellRect EdgeCellsSpan(this Map map, Rot4 rot, int size = 1)
  {
    return new CellRect(0, 0, map.Size.x, map.Size.z).EdgeCellsSpan(rot, size);
  }

  public static CellRect EdgeCellsSpan(this CellRect cellRect, Rot4 rot, int size = 1)
  {
    switch (((Rot4) ref rot).AsInt)
    {
      case 0:
        return new CellRect(cellRect.minX, cellRect.maxZ - size + 1, ((CellRect) ref cellRect).Width, size);
      case 1:
        return new CellRect(cellRect.maxX - size + 1, cellRect.minZ, size, ((CellRect) ref cellRect).Height);
      case 2:
        return new CellRect(cellRect.minX, cellRect.minZ, ((CellRect) ref cellRect).Width, size);
      case 3:
        return new CellRect(cellRect.minX, cellRect.minZ, size, ((CellRect) ref cellRect).Height);
      default:
        throw new NotImplementedException();
    }
  }

  public static IEnumerable<IntVec3> CellsNoOverlap(this CellRect cellRect, CellRect excludeRect)
  {
    HashSet<IntVec3> noOverlapCells = ((CellRect) ref excludeRect).Cells.ToHashSet<IntVec3>();
    foreach (IntVec3 intVec3 in cellRect)
    {
      if (!noOverlapCells.Contains(intVec3))
        yield return intVec3;
    }
  }

  public static IEnumerable<IntVec3> AllCellsNoRepeat(this CellRect cellRect, CellRect otherRect)
  {
    if (CellRect.op_Equality(cellRect, otherRect))
    {
      foreach (IntVec3 intVec3 in cellRect)
        yield return intVec3;
    }
    else if (!((CellRect) ref cellRect).Overlaps(otherRect))
    {
      foreach (IntVec3 intVec3 in cellRect)
        yield return intVec3;
      foreach (IntVec3 intVec3 in otherRect)
        yield return intVec3;
    }
    else
    {
      Ext_CellRect.RectEdge edge = Ext_CellRect.RectEdge.None;
      int maxTopZ;
      int maxBotZ;
      if (cellRect.maxZ > otherRect.maxZ)
      {
        maxTopZ = cellRect.maxZ + 1;
        maxBotZ = otherRect.maxZ + 1;
        if (((CellRect) ref cellRect).Width > ((CellRect) ref otherRect).Width)
          edge |= Ext_CellRect.RectEdge.Top;
      }
      else
      {
        maxTopZ = otherRect.maxZ + 1;
        maxBotZ = cellRect.maxZ + 1;
        if (((CellRect) ref otherRect).Width > ((CellRect) ref cellRect).Width)
          edge |= Ext_CellRect.RectEdge.Top;
      }
      int minTopZ;
      int minBotZ;
      if (cellRect.minZ > otherRect.minZ)
      {
        minTopZ = cellRect.minZ;
        minBotZ = otherRect.minZ;
        if (((CellRect) ref cellRect).Width < ((CellRect) ref otherRect).Width)
          edge |= Ext_CellRect.RectEdge.Bottom;
      }
      else
      {
        minTopZ = otherRect.minZ;
        minBotZ = cellRect.minZ;
        if (((CellRect) ref otherRect).Width < ((CellRect) ref cellRect).Width)
          edge |= Ext_CellRect.RectEdge.Bottom;
      }
      int minLeftX;
      int minRightX;
      if (cellRect.minX < otherRect.minX)
      {
        minLeftX = cellRect.minX;
        minRightX = otherRect.minX;
        if (((CellRect) ref cellRect).Height > ((CellRect) ref otherRect).Height)
          edge |= Ext_CellRect.RectEdge.Left;
      }
      else
      {
        minLeftX = otherRect.minX;
        minRightX = cellRect.minX;
        if (((CellRect) ref otherRect).Height > ((CellRect) ref cellRect).Height)
          edge |= Ext_CellRect.RectEdge.Left;
      }
      int maxLeftX;
      int maxRightX;
      if (cellRect.maxX < otherRect.maxX)
      {
        maxLeftX = cellRect.maxX + 1;
        maxRightX = otherRect.maxX + 1;
        if (((CellRect) ref cellRect).Height < ((CellRect) ref otherRect).Height)
          edge |= Ext_CellRect.RectEdge.Right;
      }
      else
      {
        maxLeftX = otherRect.maxX + 1;
        maxRightX = cellRect.maxX + 1;
        if (((CellRect) ref otherRect).Height < ((CellRect) ref cellRect).Height)
          edge |= Ext_CellRect.RectEdge.Right;
      }
      int x = minLeftX;
      int z;
      int xLimit;
      int zLimit;
      for (xLimit = minRightX; x < xLimit; ++x)
      {
        z = edge == Ext_CellRect.RectEdge.Left ? maxTopZ - 1 : maxBotZ - 1;
        for (zLimit = edge == Ext_CellRect.RectEdge.Left ? minBotZ : minTopZ; z >= zLimit; --z)
          yield return new IntVec3(x, 0, z);
      }
      x = maxLeftX;
      for (xLimit = maxRightX; x < xLimit; ++x)
      {
        z = edge == Ext_CellRect.RectEdge.Right ? maxTopZ - 1 : maxBotZ - 1;
        for (zLimit = edge == Ext_CellRect.RectEdge.Right ? minBotZ : minTopZ; z >= zLimit; --z)
          yield return new IntVec3(x, 0, z);
      }
      x = edge == Ext_CellRect.RectEdge.Top ? minLeftX : minRightX;
      xLimit = (edge & Ext_CellRect.RectEdge.Top) == Ext_CellRect.RectEdge.Top ? maxRightX : maxLeftX;
      switch (edge)
      {
        case Ext_CellRect.RectEdge.TopRight:
        case Ext_CellRect.RectEdge.BottomLeft:
          x = minRightX;
          xLimit = maxRightX;
          break;
        case Ext_CellRect.RectEdge.TopLeft:
        case Ext_CellRect.RectEdge.BottomRight:
          x = minLeftX;
          xLimit = maxLeftX;
          break;
      }
      for (; x < xLimit; ++x)
      {
        z = maxTopZ - 1;
        for (zLimit = maxBotZ; z >= zLimit; --z)
          yield return new IntVec3(x, 0, z);
      }
      x = edge == Ext_CellRect.RectEdge.Bottom ? minLeftX : minRightX;
      xLimit = (edge & Ext_CellRect.RectEdge.Bottom) == Ext_CellRect.RectEdge.Bottom ? maxRightX : maxLeftX;
      switch (edge)
      {
        case Ext_CellRect.RectEdge.TopRight:
        case Ext_CellRect.RectEdge.BottomLeft:
          x = minLeftX;
          xLimit = maxLeftX;
          break;
        case Ext_CellRect.RectEdge.TopLeft:
        case Ext_CellRect.RectEdge.BottomRight:
          x = minRightX;
          xLimit = maxRightX;
          break;
      }
      for (; x < xLimit; ++x)
      {
        z = minTopZ - 1;
        for (zLimit = minBotZ; z >= zLimit; --z)
          yield return new IntVec3(x, 0, z);
      }
      for (x = minRightX; x < maxLeftX; ++x)
      {
        for (z = maxBotZ - 1; z >= minTopZ; --z)
          yield return new IntVec3(x, 0, z);
      }
    }
  }

  [Flags]
  private enum RectEdge
  {
    None = 0,
    Left = 1,
    Right = 2,
    Top = 4,
    Bottom = 8,
    BottomLeft = Bottom | Right, // 0x0000000A
    TopLeft = Top | Right, // 0x00000006
    TopRight = Top | Left, // 0x00000005
    BottomRight = Bottom | Left, // 0x00000009
  }
}
