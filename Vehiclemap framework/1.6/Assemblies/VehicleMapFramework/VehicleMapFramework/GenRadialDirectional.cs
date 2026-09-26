// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.GenRadialDirectional
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace VehicleMapFramework;

[StaticConstructorOnStartup]
public static class GenRadialDirectional
{
  private static readonly IntVec3[][] Patterns = new IntVec3[8][];
  private static readonly float[][] PatternRadii = new float[8][];
  private static readonly int[][] LengthSquaredToIndexArrays = new int[8][];
  private const int RadialPatternCount = 20000;
  private const int MAX_RADIUS = 80 /*0x50*/;

  static GenRadialDirectional()
  {
    List<IntVec3>[] intVec3ListArray = new List<IntVec3>[8];
    for (int index = 0; index < 8; ++index)
    {
      int capacity = index < 4 ? 3200 : 1600;
      intVec3ListArray[index] = new List<IntVec3>(capacity);
    }
    for (int index1 = 0; index1 < 6400; ++index1)
    {
      IntVec3 c = GenRadial.RadialPattern[index1];
      for (int index2 = 0; index2 < 8; ++index2)
      {
        if (GenRadialDirectional.IsInRotation(c, new Rot8(index2)))
          intVec3ListArray[index2].Add(c);
      }
    }
    for (int r = 0; r < 8; ++r)
    {
      GenRadialDirectional.Patterns[r] = intVec3ListArray[r].ToArray();
      GenRadialDirectional.PatternRadii[r] = ((IEnumerable<IntVec3>) GenRadialDirectional.Patterns[r]).Select<IntVec3, float>((Func<IntVec3, float>) (c => ((IntVec3) ref c).LengthHorizontal)).ToArray<float>();
      BuildLookupTable(r);
    }

    static void BuildLookupTable(int r)
    {
      int[] numArray = new int[20001];
      for (int index = 0; index <= 20000; ++index)
        numArray[index] = -1;
      IntVec3[] pattern = GenRadialDirectional.Patterns[r];
      for (int index = 0; index < pattern.Length; ++index)
      {
        int horizontalSquared = ((IntVec3) ref pattern[index]).LengthHorizontalSquared;
        if (horizontalSquared <= 20000 && numArray[horizontalSquared] == -1)
          numArray[horizontalSquared] = index;
      }
      int num = 0;
      for (int index = 0; index <= 20000; ++index)
      {
        if (numArray[index] != -1)
          num = numArray[index];
        else
          numArray[index] = num;
      }
      GenRadialDirectional.LengthSquaredToIndexArrays[r] = numArray;
    }
  }

  private static bool IsInRotation(IntVec3 c, Rot8 rot)
  {
    bool flag;
    switch (((Rot8) ref rot).AsInt)
    {
      case 0:
        flag = c.z > 0;
        break;
      case 1:
        flag = c.x > 0;
        break;
      case 2:
        flag = c.z < 0;
        break;
      case 3:
        flag = c.x < 0;
        break;
      case 4:
        flag = c.x > 0 && c.z > 0;
        break;
      case 5:
        flag = c.x > 0 && c.z < 0;
        break;
      case 6:
        flag = c.x < 0 && c.z < 0;
        break;
      case 7:
        flag = c.x < 0 && c.z > 0;
        break;
      default:
        flag = false;
        break;
    }
    return flag;
  }

  private static Rot8 Rot8ToCellRect(IntVec3 from, CellRect to)
  {
    return from.x >= to.minX || from.z >= to.minZ ? (from.x >= to.minX || from.z <= to.maxZ ? (from.x <= to.maxX || from.z <= to.maxZ ? (from.x <= to.maxX || from.z >= to.minZ ? (from.z >= to.minZ ? (from.x >= to.minX ? (from.z <= to.maxZ ? (from.x <= to.maxX ? Rot8.Invalid : Rot8.West) : Rot8.South) : Rot8.East) : Rot8.North) : Rot8.NorthWest) : Rot8.SouthWest) : Rot8.SouthEast) : Rot8.NorthEast;
  }

  public static int NumCellsInRadius(float radius, Rot8 rot)
  {
    if (!((Rot8) ref rot).IsValid)
      return GenRadial.NumCellsInRadius(radius);
    if ((double) radius >= (double) GenRadial.MaxRadialPatternRadius)
    {
      Log.Error($"Not enough squares to get to radius {radius}. Max is {GenRadial.MaxRadialPatternRadius}");
      return 20000;
    }
    float num = radius + float.Epsilon;
    int index1 = (int) Math.Floor((double) num * (double) num);
    if (index1 > 6400)
      index1 = 6400;
    int asInt = ((Rot8) ref rot).AsInt;
    for (int index2 = GenRadialDirectional.LengthSquaredToIndexArrays[asInt][index1]; index2 < GenRadialDirectional.PatternRadii[asInt].Length; ++index2)
    {
      if ((double) GenRadialDirectional.PatternRadii[asInt][index2] > (double) num)
        return index2;
    }
    return 20000;
  }

  public static IntVec3[] PatternFor(
    IntVec3 from,
    CellRect to,
    float minRange,
    float maxRange,
    out IntRange indexRange)
  {
    Rot8 cellRect = GenRadialDirectional.Rot8ToCellRect(from, to);
    if (!((Rot8) ref cellRect).IsValid)
    {
      int num = (double) minRange <= 1.0 ? 0 : GenRadial.NumCellsInRadius(minRange - 1f);
      indexRange = new IntRange(num, GenRadial.NumCellsInRadius(maxRange));
      return GenRadial.RadialPattern;
    }
    int num1 = (double) minRange <= 1.0 ? 0 : GenRadialDirectional.NumCellsInRadius(minRange - 1f, cellRect);
    indexRange = new IntRange(num1, GenRadialDirectional.NumCellsInRadius(maxRange, cellRect));
    return GenRadialDirectional.Patterns[((Rot8) ref cellRect).AsInt];
  }
}
