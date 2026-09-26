// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_Pawn
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

[PublicAPI]
public static class Ext_Pawn
{
  public static void ClampToMap(this Pawn pawn, ref IntVec3 exitPoint, Map map, int extraOffset = 0)
  {
    int x = ((Thing) pawn).def.size.x;
    int z = ((Thing) pawn).def.size.z;
    int num1 = x > z ? x + extraOffset : z + extraOffset;
    int num2 = Mathf.CeilToInt((float) num1 / 2f);
    if (exitPoint.x < num1)
      exitPoint.x = num2;
    else if (exitPoint.x >= map.Size.x - num2)
      exitPoint.x = map.Size.x - num2;
    if (exitPoint.z < num1)
    {
      exitPoint.z = num2;
    }
    else
    {
      if (exitPoint.z <= map.Size.z - num2)
        return;
      exitPoint.z = map.Size.z - num2;
    }
  }

  public static IntVec3 ClampToMap(this Pawn pawn, IntVec3 spawnPoint, Map map, int extraOffset = 0)
  {
    return Ext_Pawn.ClampToMap(((Thing) pawn).def.size.x, ((Thing) pawn).def.size.z, spawnPoint, map, extraOffset);
  }

  public static IntVec3 ClampToMap(
    int width,
    int height,
    IntVec3 spawnPoint,
    Map map,
    int extraOffset = 0)
  {
    int num = width > height ? width + extraOffset : height + extraOffset;
    if (spawnPoint.x < num)
      spawnPoint.x = num / 2;
    else if (spawnPoint.x >= map.Size.x - num / 2)
      spawnPoint.x = map.Size.x - num / 2;
    if (spawnPoint.z < num)
      spawnPoint.z = num / 2;
    else if (spawnPoint.z > map.Size.z - num / 2)
      spawnPoint.z = map.Size.z - num / 2;
    return spawnPoint;
  }

  public static bool InsideMap(this Pawn pawn, IntVec3 cell, Map map)
  {
    int num1 = ((Thing) pawn).def.size.x % 2 == 0 ? ((Thing) pawn).def.size.x / 2 : (((Thing) pawn).def.size.x + 1) / 2;
    int num2 = ((Thing) pawn).def.size.z % 2 == 0 ? ((Thing) pawn).def.size.z / 2 : (((Thing) pawn).def.size.z + 1) / 2;
    int num3 = num1 > num2 ? num1 : num2;
    return cell.x + num3 > map.Size.x || cell.z + num3 > map.Size.z || cell.x - num3 < 0 || cell.z - num3 < 0;
  }

  public static IntVec3 ThingPositionFromRect(this CellRect cellRect)
  {
    return new IntVec3(cellRect.minX + (((CellRect) ref cellRect).Width - 1) / 2, 0, cellRect.minZ + (((CellRect) ref cellRect).Height - 1) / 2);
  }

  public static CellRect PawnOccupiedCells(this Pawn pawn, IntVec3 centerPoint, Rot4 rot)
  {
    return GenAdj.OccupiedRect(centerPoint, rot, ((BuildableDef) ((Thing) pawn).def).Size);
  }

  public static CellRect MinRectShifted(this Pawn pawn, IntVec2 shift, Rot4? rot = null)
  {
    rot.GetValueOrDefault();
    if (!rot.HasValue)
      rot = new Rot4?(((Thing) pawn).Rotation);
    int num = Mathf.Min(((Thing) pawn).def.size.x, ((Thing) pawn).def.size.z);
    return Ext_Pawn.RectShifted(((Thing) pawn).Position, shift, new IntVec2(num, num), (Rot8) rot.Value);
  }

  public static CellRect OccupiedRectShifted(this Pawn pawn, IntVec2 shift, Rot4? rot = null)
  {
    rot.GetValueOrDefault();
    if (!rot.HasValue)
      rot = new Rot4?(((Thing) pawn).Rotation);
    return Ext_Pawn.RectShifted(((Thing) pawn).Position, shift, ((Thing) pawn).def.size, (Rot8) rot.Value);
  }

  private static CellRect RectShifted(IntVec3 center, IntVec2 shift, IntVec2 size, Rot8 rot)
  {
    int num1 = rot == Rot8.North ? 1 : 0;
    if (rot == Rot8.East || rot == Rot8.NorthEast || rot == Rot8.SouthEast)
    {
      ref int local1 = ref shift.x;
      ref int local2 = ref shift.z;
      int z = shift.z;
      int x = shift.x;
      local1 = z;
      int num2 = x;
      local2 = num2;
    }
    if (rot == Rot8.South || rot == Rot8.SouthEast || rot == Rot8.SouthWest)
      shift.z *= -1;
    if (rot == Rot8.West || rot == Rot8.SouthWest || rot == Rot8.NorthWest)
    {
      int x = shift.x;
      shift.x = -shift.z;
      shift.z = x;
    }
    center.x += shift.x;
    center.z += shift.z;
    GenAdj.AdjustForRotation(ref center, ref size, (Rot4) rot);
    return new CellRect(center.x - (size.x - 1) / 2, center.z - (size.z - 1) / 2, size.x, size.z);
  }

  public static void CalculateSelectionBracketPositionsWorldForMultiCellPawns<T>(
    Vector3[] bracketLocs,
    T obj,
    Vector3 worldPos,
    Vector2 worldSize,
    Dictionary<T, float> dict,
    Vector2 textureSize,
    float pawnAngle = 0.0f,
    float jumpDistanceFactor = 1f)
  {
    float num1;
    float num2 = (dict.TryGetValue(obj, out num1) ? Mathf.Max(0.0f, (float) (1.0 - ((double) Time.realtimeSinceStartup - (double) num1) / 0.070000000298023224)) : 1f) * 0.2f * jumpDistanceFactor;
    float num3 = (float) (0.5 * ((double) worldSize.x - (double) textureSize.x)) + num2;
    float num4 = (float) (0.5 * ((double) worldSize.y - (double) textureSize.y)) + num2;
    float num5 = Altitudes.AltitudeFor((AltitudeLayer) 39);
    bracketLocs[0] = new Vector3(worldPos.x - num3, num5, worldPos.z - num4);
    bracketLocs[1] = new Vector3(worldPos.x + num3, num5, worldPos.z - num4);
    bracketLocs[2] = new Vector3(worldPos.x + num3, num5, worldPos.z + num4);
    bracketLocs[3] = new Vector3(worldPos.x - num3, num5, worldPos.z + num4);
    for (int index = 0; index < 4; ++index)
    {
      Vector2 vector2 = Ext_Math.RotatePointClockwise(bracketLocs[index].x - worldPos.x, bracketLocs[index].z - worldPos.z, pawnAngle);
      bracketLocs[index].x = vector2.x + worldPos.x;
      bracketLocs[index].z = vector2.y + worldPos.z;
    }
  }
}
