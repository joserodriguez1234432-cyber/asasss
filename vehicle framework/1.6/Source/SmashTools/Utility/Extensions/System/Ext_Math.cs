// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_Math
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using SmashTools.Rendering;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

[PublicAPI]
public static class Ext_Math
{
  public static readonly float Sqrt2 = Mathf.Sqrt(2f);
  private static readonly float[] Factorials = new float[17]
  {
    1f,
    1f,
    2f,
    6f,
    24f,
    120f,
    720f,
    5040f,
    40320f,
    362880f,
    3628800f,
    3.99168E+07f,
    4.790016E+08f,
    6.227021E+09f,
    8.717829E+10f,
    1.30767441E+12f,
    2.092279E+13f
  };

  public static float Binomial(int n, int i)
  {
    return Ext_Math.Factorials[n] / (Ext_Math.Factorials[i] * Ext_Math.Factorials[n - i]);
  }

  public static float Bernstein(int n, int i, float t)
  {
    float num1 = Mathf.Pow(t, (float) i);
    float num2 = Mathf.Pow(1f - t, (float) (n - i));
    return Ext_Math.Binomial(n, i) * num1 * num2;
  }

  public static Vector2 DeCasteljau(List<CurvePoint> points, float t)
  {
    Vector2 vector2_1 = Vector2.Lerp(CurvePoint.op_Implicit(points[0]), CurvePoint.op_Implicit(points[1]), t);
    Vector2 vector2_2 = Vector2.Lerp(CurvePoint.op_Implicit(points[1]), CurvePoint.op_Implicit(points[2]), t);
    Vector2 vector2_3 = Vector2.Lerp(CurvePoint.op_Implicit(points[2]), CurvePoint.op_Implicit(points[3]), t);
    return Vector2.Lerp(Vector2.Lerp(vector2_1, vector2_2, t), Vector2.Lerp(vector2_2, vector2_3, t), t);
  }

  public static float SmoothStep(float start, float end, float t)
  {
    if ((double) start >= (double) end)
      throw new ArgumentException(nameof (start));
    return (double) t <= 0.0 ? start : ((double) t >= 1.0 ? end : (float) ((double) t * (double) t * (3.0 - 2.0 * (double) t)));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Sign(bool value) => !value ? -1 : 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsOdd(this int value) => (value & 1) != 0;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsEven(this int value) => (value & 1) == 0;

  public static float ReverseInterpolate(float value, float a, float b)
  {
    return (float) (((double) value - (double) a) / ((double) b - (double) a));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int ArithmeticSeries(int n, int a, int k)
  {
    if (n < 0)
      throw new InvalidOperationException("n cannot be negative.");
    return n * ((a + k) / 2);
  }

  public static IntVec2 Shifted(this IntVec2 cell, Rot4 dir, int a, int b = 0)
  {
    if (!((Rot4) ref dir).IsValid)
      return cell;
    switch (((Rot4) ref dir).AsInt)
    {
      case 0:
        return new IntVec2(cell.x + b, cell.z + a);
      case 1:
        return new IntVec2(cell.x + a, cell.z - b);
      case 2:
        return new IntVec2(cell.x - b, cell.z - a);
      case 3:
        return new IntVec2(cell.x - a, cell.z + b);
      default:
        throw new NotImplementedException("Beyond what Rot4 supports");
    }
  }

  public static float RoundTo(this float num, float roundTo)
  {
    if ((double) roundTo <= 0.0)
      throw new InvalidOperationException("roundTo must be greater than 0.");
    return Mathf.Round(num / roundTo) * roundTo;
  }

  public static int RoundTo(this int num, int roundTo)
  {
    if (roundTo <= 0)
      throw new InvalidOperationException("roundTo must be greater than 0.");
    return Mathf.RoundToInt((float) num / (float) roundTo) * roundTo;
  }

  public static int PowTwo(int n) => 1 << n;

  public static long Pow(this int x, int y) => (long) Math.Pow((double) x, (double) y);

  public static int Take(this int value, int take, out int remaining)
  {
    remaining = 0;
    if (take >= value)
      return value;
    remaining = value - take;
    return take;
  }

  public static Vector2 RotatePointClockwise(this Vector2 coord, float theta)
  {
    return Ext_Math.RotatePointClockwise(coord.x, coord.y, theta);
  }

  public static Vector2 RotatePointCounterClockwise(this Vector2 coord, float theta)
  {
    return Ext_Math.RotatePointCounterClockwise(coord.x, coord.y, theta);
  }

  public static Vector2 RotatePointClockwise(float x, float y, float theta)
  {
    return Ext_Math.RotatePointCounterClockwise(x, y, -theta);
  }

  public static Vector2 RotatePointCounterClockwise(float x, float y, float theta)
  {
    if (Mathf.Approximately(theta, 0.0f))
      return new Vector2(x, y);
    float num = theta * ((float) Math.PI / 180f);
    return new Vector2((float) ((double) x * (double) Mathf.Cos(num) - (double) y * (double) Mathf.Sin(num)), (float) ((double) x * (double) Mathf.Sin(num) + (double) y * (double) Mathf.Cos(num)));
  }

  public static Vector3 RotatePoint(Vector3 point, Vector3 origin, float angle)
  {
    float num1 = (float) ((double) Mathf.Cos(angle * ((float) Math.PI / 180f)) * ((double) point.x - (double) origin.x) - (double) Mathf.Sin(angle * ((float) Math.PI / 180f)) * ((double) point.z - (double) origin.z)) + origin.x;
    float num2 = (float) ((double) Mathf.Sin(angle * ((float) Math.PI / 180f)) * ((double) point.x - (double) origin.x) + (double) Mathf.Cos(angle * ((float) Math.PI / 180f)) * ((double) point.z - (double) origin.z)) + origin.z;
    return new Vector3(num1, point.y, num2);
  }

  public static float RotateAngle(float angle, float rotation)
  {
    angle += rotation;
    return angle.ClampAngle();
  }

  public static double AngleThroughOrigin(this IntVec3 c, Map map)
  {
    int num1 = c.x - map.Size.x / 2;
    float num2 = Mathf.Abs(Mathf.Atan((float) (c.z - map.Size.z / 2) / (float) num1) * ((float) Math.PI / 180f));
    float num3;
    switch (Quadrant.QuadrantOfIntVec3(c, map).AsInt)
    {
      case 2:
        num3 = 360f - num2;
        break;
      case 3:
        num3 = 180f + num2;
        break;
      case 4:
        num3 = 180f - num2;
        break;
      default:
        num3 = num2;
        break;
    }
    return (double) num3;
  }

  public static float AngleToCell(this IntVec3 pos, IntVec3 point)
  {
    return ((IntVec3) ref pos).ToVector3Shifted().AngleToPoint(((IntVec3) ref point).ToVector3Shifted());
  }

  public static float AngleToPointRelative(this Vector3 pos, Vector3 point)
  {
    float num = pos.x - point.x;
    return (float) ((360.0 + (double) Mathf.Atan2(pos.z - point.z, num) * 57.295780181884766) % 360.0);
  }

  public static float AngleToPoint(float x1, float y1, float x2, float y2)
  {
    return (float) ((180.0 + (double) Mathf.Atan2(x1 - x2, y1 - y2) * 57.295780181884766) % 360.0);
  }

  public static float AngleToPoint(this Vector2 pos, Vector2 point)
  {
    return Ext_Math.AngleToPoint(pos.x, pos.y, point.x, point.y);
  }

  public static float AngleToPoint(this Vector3 pos, Vector3 point)
  {
    return Ext_Math.AngleToPoint(pos.x, pos.z, point.x, point.z);
  }

  public static float AngleToPoint(this IntVec3 start, IntVec3 end)
  {
    return ((IntVec3) ref start).ToVector3Shifted().AngleToPoint(((IntVec3) ref end).ToVector3Shifted());
  }

  public static Vector3 PointFromAngle(this Vector3 pos, float distance, float angle)
  {
    float num1 = pos.x + distance * Mathf.Sin(angle * ((float) Math.PI / 180f));
    float num2 = pos.z + distance * Mathf.Cos(angle * ((float) Math.PI / 180f));
    return new Vector3(num1, pos.y, num2);
  }

  public static IntVec3 PointFromAngle(this IntVec3 pos, float distance, float angle)
  {
    int num1 = Mathf.CeilToInt((float) pos.x + distance * Mathf.Sin(angle * ((float) Math.PI / 180f)));
    int num2 = Mathf.CeilToInt((float) pos.z + distance * Mathf.Cos(angle * ((float) Math.PI / 180f)));
    return new IntVec3(num1, pos.y, num2);
  }

  public static Vector3 PointToEdge(this Vector3 origin, Map map, float angle)
  {
    float num1 = angle.ClampAngle().RoundTo(0.01f);
    float x = (float) map.Size.x;
    float z = (float) map.Size.z;
    Vector3 zero = Vector3.zero;
    if ((double) num1 == 0.0)
    {
      // ISSUE: explicit constructor call
      ((Vector3) ref zero).\u002Ector(origin.x, origin.y, z);
    }
    else if ((double) num1 == 90.0)
    {
      // ISSUE: explicit constructor call
      ((Vector3) ref zero).\u002Ector(x, origin.y, origin.z);
    }
    else if ((double) num1 == 180.0)
    {
      // ISSUE: explicit constructor call
      ((Vector3) ref zero).\u002Ector(origin.x, origin.y, 0.0f);
    }
    else if ((double) num1 == 270.0)
    {
      // ISSUE: explicit constructor call
      ((Vector3) ref zero).\u002Ector(0.0f, origin.y, origin.z);
    }
    else if ((double) num1 >= 0.0 && (double) num1 <= 45.0)
    {
      float num2 = num1;
      float num3 = origin.x + (z - origin.z) * Mathf.Tan(num2 * ((float) Math.PI / 180f));
      // ISSUE: explicit constructor call
      ((Vector3) ref zero).\u002Ector(num3, origin.y, z);
    }
    else if ((double) num1 >= 45.0 && (double) num1 <= 90.0)
    {
      float num4 = 90f - num1;
      float num5 = origin.z + (x - origin.x) * Mathf.Tan(num4 * ((float) Math.PI / 180f));
      // ISSUE: explicit constructor call
      ((Vector3) ref zero).\u002Ector(x, origin.y, num5);
    }
    else if ((double) num1 >= 90.0 && (double) num1 <= 135.0)
    {
      float num6 = num1 - 90f;
      float num7 = origin.z - (x - origin.x) * Mathf.Tan(num6 * ((float) Math.PI / 180f));
      // ISSUE: explicit constructor call
      ((Vector3) ref zero).\u002Ector(x, origin.y, num7);
    }
    else if ((double) num1 >= 135.0 && (double) num1 <= 180.0)
    {
      float num8 = 180f - num1;
      float num9 = origin.x + origin.z * Mathf.Tan(num8 * ((float) Math.PI / 180f));
      // ISSUE: explicit constructor call
      ((Vector3) ref zero).\u002Ector(num9, origin.y, 0.0f);
    }
    else if ((double) num1 >= 180.0 && (double) num1 <= 225.0)
    {
      float num10 = num1 - 180f;
      float num11 = origin.x - origin.z * Mathf.Tan(num10 * ((float) Math.PI / 180f));
      // ISSUE: explicit constructor call
      ((Vector3) ref zero).\u002Ector(num11, origin.y, 0.0f);
    }
    else if ((double) num1 >= 225.0 && (double) num1 <= 270.0)
    {
      float num12 = 270f - num1;
      float num13 = origin.z - origin.x * Mathf.Tan(num12 * ((float) Math.PI / 180f));
      // ISSUE: explicit constructor call
      ((Vector3) ref zero).\u002Ector(0.0f, origin.y, num13);
    }
    else if ((double) num1 >= 270.0 && (double) num1 <= 315.0)
    {
      float num14 = num1 - 270f;
      float num15 = origin.z + origin.x * Mathf.Tan(num14 * ((float) Math.PI / 180f));
      // ISSUE: explicit constructor call
      ((Vector3) ref zero).\u002Ector(0.0f, origin.y, num15);
    }
    else if ((double) num1 >= 315.0 && (double) num1 <= 360.0)
    {
      float num16 = 360f - num1;
      float num17 = origin.x - (z - origin.z) * Mathf.Tan(num16 * ((float) Math.PI / 180f));
      // ISSUE: explicit constructor call
      ((Vector3) ref zero).\u002Ector(num17, origin.y, z);
    }
    return zero;
  }

  public static IntVec3 PointFromOrigin(float angle, Map map)
  {
    int x = map.Size.x;
    int z = map.Size.z;
    if ((double) angle < 0.0 || (double) angle > 360.0)
      return IntVec3.Invalid;
    Rot4 invalid = Rot4.Invalid;
    Rot4 rot4;
    if ((double) angle <= 45.0 || (double) angle > 315.0)
      rot4 = Rot4.East;
    else if ((double) angle <= 135.0 && (double) angle >= 45.0)
      rot4 = Rot4.North;
    else if ((double) angle <= 225.0 && (double) angle >= 135.0)
    {
      rot4 = Rot4.West;
    }
    else
    {
      if ((double) angle > 315.0 || (double) angle < 225.0)
        return new IntVec3(z / 2, 0, 1);
      rot4 = Rot4.South;
    }
    float num = Mathf.Tan(angle * ((float) Math.PI / 180f));
    IntVec3 intVec3;
    switch (((Rot4) ref rot4).AsInt)
    {
      case 0:
        intVec3 = new IntVec3((int) ((double) z / (2.0 * (double) num) + (double) (z / 2)), 0, z - 1);
        break;
      case 1:
        intVec3 = new IntVec3(x - 1, 0, (int) ((double) (x / 2) * (double) num) + x / 2);
        break;
      case 2:
        intVec3 = new IntVec3((int) ((double) z - ((double) z / (2.0 * (double) num) + (double) (z / 2))), 0, 1);
        break;
      case 3:
        intVec3 = new IntVec3(1, 0, (int) ((double) x - ((double) (x / 2) * (double) num + (double) (x / 2))));
        break;
      default:
        intVec3 = IntVec3.Invalid;
        break;
    }
    return intVec3;
  }

  public static float SphericalDistance(Vector3 source, Vector3 target)
  {
    return Find.WorldGrid.ApproxDistanceInTiles(GenMath.SphericalDistance(((Vector3) ref source).normalized, ((Vector3) ref target).normalized));
  }

  public static List<LineSegment> GetLineSegmentsFromCircle(float radius)
  {
    List<LineSegment> segmentsFromCircle = new List<LineSegment>();
    int num1 = Mathf.Clamp(Mathf.RoundToInt(24f * radius), 12, 48 /*0x30*/);
    float num2 = 6.28318548f / (float) num1;
    float num3 = Mathf.Cos(num2);
    float num4 = Mathf.Sin(num2);
    Vector3 from;
    // ISSUE: explicit constructor call
    ((Vector3) ref from).\u002Ector(radius, 0.0f, 0.0f);
    for (int index = 0; index < num1; ++index)
    {
      Vector3 vector3 = from;
      float num5 = (float) ((double) vector3.x * (double) num3 - (double) vector3.z * (double) num4);
      float num6 = (float) ((double) vector3.x * (double) num4 + (double) vector3.z * (double) num3);
      Vector3 to;
      // ISSUE: explicit constructor call
      ((Vector3) ref to).\u002Ector(num5, 0.0f, num6);
      segmentsFromCircle.Add(new LineSegment(from, to));
      from = to;
    }
    return segmentsFromCircle;
  }

  public static List<LineSegment> GetLineSegmentsFromCone(
    Vector2 coneAngle,
    float minRange,
    float maxRange)
  {
    List<LineSegment> segmentsFromCone = new List<LineSegment>();
    Vector3 zero = Vector3.zero;
    int angle1 = Mathf.RoundToInt(coneAngle.x.ClampAngle());
    int angle2 = Mathf.RoundToInt(coneAngle.y.ClampAngle());
    float num = ((float) (((double) (angle2 - angle1) + 360.0) % 360.0)).ClampAngle();
    Vector3 vector3_1 = zero.PointFromAngle(minRange, (float) angle1);
    Vector3 vector3_2 = zero.PointFromAngle(minRange, (float) angle2);
    Vector3 to1 = zero.PointFromAngle(maxRange, (float) angle1);
    Vector3 to2 = zero.PointFromAngle(maxRange, (float) angle2);
    segmentsFromCone.Add(new LineSegment(vector3_1, to1));
    segmentsFromCone.Add(new LineSegment(vector3_2, to2));
    if ((double) minRange > 0.0)
    {
      segmentsFromCone.Add(new LineSegment(zero, vector3_1, Color.red));
      segmentsFromCone.Add(new LineSegment(zero, vector3_2, Color.red));
    }
    Vector3 from1 = to1;
    Vector3 from2 = vector3_1;
    for (int index = angle1; (double) index <= (double) num; ++index)
    {
      float angle3 = (float) (angle1 + index);
      Vector3 to3 = zero.PointFromAngle(maxRange, angle3);
      segmentsFromCone.Add(new LineSegment(from1, to3));
      from1 = to3;
      if ((double) minRange > 0.0)
      {
        Vector3 to4 = zero.PointFromAngle(minRange, angle3);
        segmentsFromCone.Add(new LineSegment(from2, to4, Color.red));
        from2 = to4;
      }
    }
    return segmentsFromCone;
  }
}
