// Decompiled with JetBrains decompiler
// Type: SmashTools.LagrangeCurve
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public class LagrangeCurve : LinearCurve
{
  public LagrangeCurve()
  {
  }

  public LagrangeCurve(List<CurvePoint> points)
    : base(points)
  {
  }

  private static Vector2 LagrangeFunction(List<CurvePoint> coordinates, float x)
  {
    float num1 = 0.0f;
    for (int index1 = 0; index1 < coordinates.Count; ++index1)
    {
      CurvePoint coordinate1 = coordinates[index1];
      float y = ((CurvePoint) ref coordinate1).y;
      float num2 = 1f;
      for (int index2 = 0; index2 < coordinates.Count; ++index2)
      {
        if (index1 != index2)
        {
          CurvePoint coordinate2 = coordinates[index2];
          y *= x - ((CurvePoint) ref coordinate2).x;
          num2 *= ((CurvePoint) ref coordinate1).x - ((CurvePoint) ref coordinate2).x;
        }
      }
      num1 += y / num2;
    }
    return new Vector2(x, num1);
  }

  public override Vector2 Function(float x)
  {
    if (this.points.Count < 2)
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
    return num2 >= x2 ? CurvePoint.op_Implicit(this.RightBound) : LagrangeCurve.LagrangeFunction(this.points, x);
  }

  public override Vector2 EvaluateT(float t)
  {
    if ((double) t <= 0.0)
      return CurvePoint.op_Implicit(this.LeftBound);
    return (double) t >= 1.0 ? CurvePoint.op_Implicit(this.RightBound) : LagrangeCurve.LagrangeFunction(this.points, t);
  }
}
