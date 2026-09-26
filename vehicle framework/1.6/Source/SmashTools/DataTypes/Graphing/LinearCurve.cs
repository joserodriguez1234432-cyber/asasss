// Decompiled with JetBrains decompiler
// Type: SmashTools.LinearCurve
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public class LinearCurve : IEnumerable<CurvePoint>, IEnumerable
{
  public List<CurvePoint> points = new List<CurvePoint>();
  public List<(CurvePoint lhs, CurvePoint rhs)> values = new List<(CurvePoint, CurvePoint)>();

  public LinearCurve()
  {
  }

  public LinearCurve(List<CurvePoint> points)
  {
    this.points = new List<CurvePoint>((IEnumerable<CurvePoint>) points);
  }

  public CurvePoint LeftBound => this.points.FirstOrDefault<CurvePoint>();

  public CurvePoint RightBound => this.points.LastOrDefault<CurvePoint>();

  public int PointsCount => this.points.Count;

  public bool IsValid => !this.points.NullOrEmpty<CurvePoint>();

  public CurvePoint this[int i]
  {
    get => this.points[i];
    set => this.points[i] = value;
  }

  public virtual void Add(CurvePoint curvePoint) => this.points.Add(curvePoint);

  public float Evaluate(float x) => this.Function(x).y;

  public virtual bool ValueLimit(float x, out float y)
  {
    y = 0.0f;
    if (!this.values.NullOrEmpty<(CurvePoint, CurvePoint)>())
    {
      for (int index = 0; index < this.values.Count; ++index)
      {
        (CurvePoint lhs, CurvePoint rhs) = this.values[index];
        if ((double) x >= (double) ((CurvePoint) ref lhs).x && (double) x <= (double) ((CurvePoint) ref rhs).x)
        {
          y = ((CurvePoint) ref lhs).y;
          return true;
        }
      }
    }
    return false;
  }

  public virtual Vector2 Function(float x)
  {
    if (this.points.NullOrEmpty<CurvePoint>())
      return Vector2.zero;
    if (this.points.Count == 1)
      return CurvePoint.op_Implicit(this.points[0]);
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
    CurvePoint point2 = this.points[this.points.Count - 1];
    for (int index = 0; index < this.points.Count; ++index)
    {
      double num3 = (double) x;
      CurvePoint point3 = this.points[index];
      double x3 = (double) ((CurvePoint) ref point3).x;
      if (num3 <= x3)
      {
        point2 = this.points[index];
        if (index > 0)
        {
          point1 = this.points[index - 1];
          break;
        }
        break;
      }
    }
    float num4 = (float) (((double) x - (double) ((CurvePoint) ref point1).x) / ((double) ((CurvePoint) ref point2).x - (double) ((CurvePoint) ref point1).x));
    return new Vector2(x, Mathf.Lerp(((CurvePoint) ref point1).y, ((CurvePoint) ref point2).y, num4));
  }

  public virtual Vector2 EvaluateT(float t)
  {
    if (this.PointsCount == 0)
      return Vector2.zero;
    if ((double) t <= 0.0)
      return CurvePoint.op_Implicit(this.LeftBound);
    if ((double) t >= 1.0)
      return CurvePoint.op_Implicit(this.RightBound);
    (CurvePoint leftPoint, CurvePoint rightPoint) = this.LerpPair(t);
    return new Vector2(Mathf.Lerp(((CurvePoint) ref leftPoint).x, ((CurvePoint) ref rightPoint).x, t), Mathf.Lerp(((CurvePoint) ref leftPoint).y, ((CurvePoint) ref rightPoint).y, t));
  }

  private (CurvePoint leftPoint, CurvePoint rightPoint) LerpPair(float t)
  {
    if (this.points.Count <= 1)
      return (this.LeftBound, this.RightBound);
    float num1 = this.TotalLength();
    float num2 = 0.0f;
    for (int index = 0; index < this.points.Count - 1; ++index)
    {
      CurvePoint point1 = this.points[index];
      CurvePoint point2 = this.points[index + 1];
      num2 += Vector2.Distance(CurvePoint.op_Implicit(point1), CurvePoint.op_Implicit(point2));
      if ((double) t * (double) num1 <= (double) num2)
        return (point1, point2);
    }
    return (this.LeftBound, this.RightBound);
  }

  private float TotalLength()
  {
    float num = 0.0f;
    for (int index = 0; index < this.points.Count - 1; ++index)
    {
      CurvePoint point1 = this.points[index];
      CurvePoint point2 = this.points[index + 1];
      num = Vector2.Distance(CurvePoint.op_Implicit(point1), CurvePoint.op_Implicit(point2));
    }
    return num;
  }

  public virtual void Graph()
  {
    FloatRange range;
    ref FloatRange local = ref range;
    CurvePoint curvePoint = this.LeftBound;
    double x1 = (double) ((CurvePoint) ref curvePoint).x;
    curvePoint = this.RightBound;
    double x2 = (double) ((CurvePoint) ref curvePoint).x;
    // ISSUE: explicit constructor call
    ((FloatRange) ref local).\u002Ector((float) x1, (float) x2);
    Find.WindowStack.Add((Window) new Dialog_Graph(new SmashTools.Graph.Function(this.Function), range, this.points));
  }

  public static implicit operator SmashTools.Graph.Function(LinearCurve curve)
  {
    return new SmashTools.Graph.Function(curve.Function);
  }

  IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.GetEnumerator();

  public IEnumerator<CurvePoint> GetEnumerator()
  {
    foreach (CurvePoint point in this.points)
      yield return point;
  }
}
