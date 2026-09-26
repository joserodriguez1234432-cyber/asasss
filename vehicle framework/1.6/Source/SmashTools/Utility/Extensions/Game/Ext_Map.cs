// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_Map
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using RimWorld;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public static class Ext_Map
{
  private static readonly AccessTools.FieldRef<AreaManager, List<Area>> AreaListFieldRef = (AccessTools.FieldRef<AreaManager, List<Area>>) AccessTools.FieldRefAccess<List<Area>>(typeof (AreaManager), "areas");

  public static void EnsureAreaInitialized<T>(this Map map) where T : Area, new()
  {
    if (map.areaManager == null)
    {
      Log.Error("Trying to add registered area types before AreaManager has been initialized.");
    }
    else
    {
      if ((object) map.areaManager.Get<T>() != null)
        return;
      T instance = (T) Activator.CreateInstance(typeof (T), (object) map.areaManager);
      Ext_Map.AreaListFieldRef.Invoke(map.areaManager).Add((Area) instance);
    }
  }

  public static void DrawCell_ThreadSafe(
    this Map map,
    IntVec3 cell,
    float colorPct = 0.0f,
    string text = null,
    int duration = 50)
  {
    if (UnityData.IsInMainThread)
      map.debugDrawer.FlashCell(cell, colorPct, text, duration);
    else
      UnityThread.ExecuteOnMainThread((Action) (() => Ext_Map.DrawCell(cell, map, colorPct, text, duration)));
  }

  public static void DrawLine_ThreadSafe(
    this Map map,
    IntVec3 from,
    IntVec3 to,
    SimpleColor color = 0,
    int duration = 50)
  {
    if (UnityData.IsInMainThread)
    {
      DebugCellDrawer debugDrawer = map.debugDrawer;
      IntVec3 intVec3_1 = from;
      IntVec3 intVec3_2 = to;
      SimpleColor simpleColor1 = color;
      int num = duration;
      SimpleColor simpleColor2 = simpleColor1;
      debugDrawer.FlashLine(intVec3_1, intVec3_2, num, simpleColor2);
    }
    else
      UnityThread.ExecuteOnMainThread((Action) (() => Ext_Map.DrawLine(from, to, map, color, duration)));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void DrawCell(IntVec3 cell, Map map, float colorPct, string label, int duration)
  {
    map.debugDrawer.FlashCell(cell, colorPct, label, duration);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void DrawLine(
    IntVec3 from,
    IntVec3 to,
    Map map,
    SimpleColor color = 0,
    int duration = 50)
  {
    map.debugDrawer.FlashLine(from, to, duration, color);
  }

  public static float Distance(IntVec3 c1, IntVec3 c2)
  {
    int x1 = Mathf.Abs(c1.x - c2.x);
    int x2 = Mathf.Abs(c1.z - c2.z);
    return Mathf.Sqrt((float) (x1.Pow(2) + x2.Pow(2)));
  }

  public static bool WithinDistanceToEdge(this IntVec3 position, int distance, Map map)
  {
    return position.x < distance || position.z < distance || map.Size.x - position.x < distance || map.Size.z - position.z < distance;
  }

  public static IEnumerable<IntVec3> AdjacentCellsCardinal(this IntVec3 c, Map map)
  {
    IntVec3 intVec3;
    // ISSUE: explicit constructor call
    ((IntVec3) ref intVec3).\u002Ector(c.x, c.y, c.z + 1);
    IntVec3 east = new IntVec3(c.x + 1, c.y, c.z);
    IntVec3 south = new IntVec3(c.x, c.y, c.z - 1);
    IntVec3 west = new IntVec3(c.x - 1, c.y, c.z);
    if (GenGrid.InBounds(intVec3, map))
      yield return intVec3;
    if (GenGrid.InBounds(east, map))
      yield return east;
    if (GenGrid.InBounds(south, map))
      yield return south;
    if (GenGrid.InBounds(west, map))
      yield return west;
  }

  public static IEnumerable<IntVec3> AdjacentCellsDiagonal(this IntVec3 c, Map map)
  {
    IntVec3 intVec3;
    // ISSUE: explicit constructor call
    ((IntVec3) ref intVec3).\u002Ector(c.x + 1, c.y, c.z + 1);
    IntVec3 SE = new IntVec3(c.x + 1, c.y, c.z - 1);
    IntVec3 SW = new IntVec3(c.x - 1, c.y, c.z - 1);
    IntVec3 NW = new IntVec3(c.x - 1, c.y, c.z + 1);
    if (GenGrid.InBounds(intVec3, map))
      yield return intVec3;
    if (GenGrid.InBounds(SE, map))
      yield return SE;
    if (GenGrid.InBounds(SW, map))
      yield return SW;
    if (GenGrid.InBounds(NW, map))
      yield return NW;
  }

  public static IEnumerable<IntVec3> AdjacentCells8Way(this IntVec3 c, Map map)
  {
    return c.AdjacentCellsCardinal(map).Concat<IntVec3>(c.AdjacentCellsDiagonal(map));
  }

  public static Rot4 Max4IntToRot(
    int northCellCount,
    int eastCellCount,
    int southCellCount,
    int westCellCount)
  {
    int num1 = northCellCount > eastCellCount ? northCellCount : eastCellCount;
    int num2 = southCellCount > westCellCount ? southCellCount : westCellCount;
    int num3 = num1 > num2 ? num1 : num2;
    if (num3 == northCellCount)
      return Rot4.North;
    if (num3 == eastCellCount)
      return Rot4.East;
    if (num3 == southCellCount)
      return Rot4.South;
    return num3 == westCellCount ? Rot4.West : Rot4.Invalid;
  }

  public static Rot8 DirectionToCell(IntVec3 c1, IntVec3 c2)
  {
    int num1 = c1.x - c2.x;
    int num2 = c1.z - c2.z;
    if (num1 < 0)
    {
      if (num2 < 0)
        return Rot8.NorthEast;
      return num2 > 0 ? Rot8.SouthEast : Rot8.East;
    }
    if (num1 > 0)
    {
      if (num2 < 0)
        return Rot8.NorthWest;
      return num2 > 0 ? Rot8.SouthWest : Rot8.West;
    }
    if (num2 < 0)
      return Rot8.North;
    return num2 > 0 ? Rot8.South : Rot8.Invalid;
  }

  public static List<T> AllPawnsOnMap<T>(this Map map, Faction faction = null, Predicate<T> validator = null) where T : Pawn
  {
    return map.mapPawns.AllPawnsSpawned.Where<Pawn>((Func<Pawn, bool>) (p =>
    {
      if (!(p is T obj2) || faction != null && ((Thing) p).Faction != faction)
        return false;
      return validator == null || validator(obj2);
    })).Cast<T>().ToList<T>();
  }
}
