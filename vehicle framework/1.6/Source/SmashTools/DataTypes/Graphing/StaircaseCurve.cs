// Decompiled with JetBrains decompiler
// Type: SmashTools.StaircaseCurve
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public class StaircaseCurve : LinearCurve
{
  public StaircaseCurve()
  {
  }

  public StaircaseCurve(List<CurvePoint> points)
    : base(points)
  {
  }

  public override Vector2 Function(float x)
  {
    if (this.points.NullOrEmpty<CurvePoint>())
      return Vector2.zero;
    double num1 = (double) x;
    CurvePoint leftBound = this.LeftBound;
    double x1 = (double) ((CurvePoint) ref leftBound).x;
    if (num1 <= x1)
      return CurvePoint.op_Implicit(this.LeftBound);
    double num2 = (double) x;
    CurvePoint rightBound = this.RightBound;
    double x2 = (double) ((CurvePoint) ref rightBound).x;
    if (num2 >= x2)
      return CurvePoint.op_Implicit(this.RightBound);
    float y;
    if (this.ValueLimit(x, out y))
      return new Vector2(x, y);
    CurvePoint point1 = this.points[0];
    for (int index = 0; index < this.points.Count; ++index)
    {
      double num3 = (double) x;
      CurvePoint point2 = this.points[index];
      double x3 = (double) ((CurvePoint) ref point2).x;
      if (num3 <= x3)
      {
        if (index > 0)
        {
          point1 = this.points[index - 1];
          break;
        }
        break;
      }
    }
    return new Vector2(x, ((CurvePoint) ref point1).y);
  }
}
