// Decompiled with JetBrains decompiler
// Type: SmashTools.BezierCurve
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public class BezierCurve : LinearCurve
{
  public BezierCurve()
  {
  }

  public BezierCurve(List<CurvePoint> points)
    : base(points)
  {
  }

  private static Vector2 BezierFunction(List<CurvePoint> controlPoints, float t)
  {
    int n = controlPoints.Count - 1;
    if (n > 16 /*0x10*/)
    {
      Log.Error("Max number of control points is 16, factorials are precalculated.");
      n = 16 /*0x10*/;
    }
    Vector2 vector2_1 = Vector2.zero;
    for (int index = 0; index < controlPoints.Count; ++index)
    {
      Vector2 vector2_2 = vector2_1;
      double num = (double) Ext_Math.Bernstein(n, index, t);
      CurvePoint controlPoint = controlPoints[index];
      Vector2 loc = ((CurvePoint) ref controlPoint).Loc;
      Vector2 vector2_3 = Vector2.op_Multiply((float) num, loc);
      vector2_1 = Vector2.op_Addition(vector2_2, vector2_3);
    }
    return vector2_1;
  }

  public override Vector2 Function(float x)
  {
    if (this.points.Count < 3)
      return base.Function(x);
    float y;
    if (this.ValueLimit(x, out y))
      return new Vector2(x, y);
    double num1 = (double) x;
    CurvePoint curvePoint = this.LeftBound;
    double x1 = (double) ((CurvePoint) ref curvePoint).x;
    if (num1 <= x1)
      return CurvePoint.op_Implicit(this.LeftBound);
    double num2 = (double) x;
    curvePoint = this.RightBound;
    double x2 = (double) ((CurvePoint) ref curvePoint).x;
    if (num2 >= x2)
      return CurvePoint.op_Implicit(this.RightBound);
    double num3 = (double) x;
    curvePoint = this.LeftBound;
    double x3 = (double) ((CurvePoint) ref curvePoint).x;
    double num4 = num3 - x3;
    curvePoint = this.RightBound;
    double x4 = (double) ((CurvePoint) ref curvePoint).x;
    curvePoint = this.LeftBound;
    double x5 = (double) ((CurvePoint) ref curvePoint).x;
    double num5 = x4 - x5;
    return BezierCurve.BezierFunction(this.points, (float) (num4 / num5));
  }

  public override Vector2 EvaluateT(float t)
  {
    if ((double) t <= 0.0)
      return CurvePoint.op_Implicit(this.LeftBound);
    return (double) t >= 1.0 ? CurvePoint.op_Implicit(this.RightBound) : BezierCurve.BezierFunction(this.points, t);
  }
}
