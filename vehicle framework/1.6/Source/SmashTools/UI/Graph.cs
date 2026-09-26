// Decompiled with JetBrains decompiler
// Type: SmashTools.Graph
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public static class Graph
{
  public const int AxisMajorNotchCount = 5;
  public const int AxisNotchCount = 50;
  public const float NotchSize = 5f;
  public const float GraphNodeSize = 8f;
  public const float DistToPlotCurvePoint = 50f;
  private static int draggingPlotPointIndex = -1;
  private static readonly Color progressColor = new Color(0.0f, 0.75f, 0.0f, 0.5f);

  public static void DrawGraph(
    Rect rect,
    Graph.Function function,
    FloatRange xRange,
    FloatRange yRange,
    List<CurvePoint> plotPoints = null,
    float progress = -1f,
    bool simplified = false,
    bool editable = true,
    bool drawCoordLabels = true)
  {
    Rect rect1 = GenUI.ContractedBy(rect, 5f);
    Graph.DrawAxis(rect1, xRange, yRange, !simplified);
    if (function == null || plotPoints.NullOrEmpty<CurvePoint>())
      return;
    Graph.PlotFunction(rect1, function, xRange, yRange, plotPoints, progress, simplified, editable, drawCoordLabels);
  }

  public static void DrawAxis(Rect rect, FloatRange xRange, FloatRange yRange, bool drawAxisT = false)
  {
    float num1 = 0.0f;
    float num2 = 0.0f;
    if ((double) xRange.min < 0.0)
    {
      num1 = Mathf.Abs(xRange.min / xRange.max) * ((Rect) ref rect).width;
      num2 = Mathf.Abs(yRange.min / yRange.max) * ((Rect) ref rect).height;
    }
    Vector2 vector2_1;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_1).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y);
    if (drawAxisT)
      Widgets.DrawLineHorizontal(vector2_1.x, vector2_1.y, ((Rect) ref rect).width);
    Vector2 vector2_2;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_2).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y + ((Rect) ref rect).height - num2);
    Widgets.DrawLineHorizontal(vector2_2.x, vector2_2.y, ((Rect) ref rect).width);
    Vector2 vector2_3;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_3).\u002Ector(((Rect) ref rect).x + num1, ((Rect) ref rect).y);
    Widgets.DrawLineVertical(vector2_3.x, vector2_3.y, ((Rect) ref rect).height);
    float num3 = ((Rect) ref rect).width / 50f;
    float num4 = ((Rect) ref rect).height / 50f;
    for (int index = 0; index < 51; ++index)
    {
      bool flag = index % 5 == 0;
      float num5 = flag ? 10f : 5f;
      Vector2 vector2_4;
      // ISSUE: explicit constructor call
      ((Vector2) ref vector2_4).\u002Ector(vector2_1.x + num3 * (float) index, vector2_1.y - num5 / 2f);
      if (drawAxisT)
        Widgets.DrawLineVertical(vector2_4.x, vector2_4.y, num5);
      Vector2 vector2_5;
      // ISSUE: explicit constructor call
      ((Vector2) ref vector2_5).\u002Ector(vector2_2.x + num3 * (float) index, vector2_2.y - num5 / 2f);
      Widgets.DrawLineVertical(vector2_5.x, vector2_5.y, num5);
      Vector2 vector2_6;
      // ISSUE: explicit constructor call
      ((Vector2) ref vector2_6).\u002Ector(vector2_3.x - num5 / 2f, (float) ((double) ((Rect) ref rect).y + (double) ((Rect) ref rect).height - (double) num4 * (double) index));
      Widgets.DrawLineHorizontal(vector2_6.x, vector2_6.y, num5);
      if (flag)
      {
        if (drawAxisT)
        {
          string str = ((float) (0.0 + 1.0 * ((double) index / 50.0))).RoundTo(0.1f).ToString();
          Vector2 vector2_7 = Text.CalcSize(str);
          Rect rect1;
          // ISSUE: explicit constructor call
          ((Rect) ref rect1).\u002Ector(vector2_4.x - vector2_7.x / 2f, vector2_1.y - 25f, vector2_7.x, vector2_7.y);
          Widgets.Label(rect1, str);
        }
        string str1 = (xRange.min + (float) (((double) xRange.max - (double) xRange.min) * ((double) index / 50.0))).RoundTo(0.1f).ToString();
        Vector2 vector2_8 = Text.CalcSize(str1);
        Rect rect2;
        // ISSUE: explicit constructor call
        ((Rect) ref rect2).\u002Ector(vector2_5.x - vector2_8.x / 2f, vector2_2.y + 10f, vector2_8.x, vector2_8.y);
        Widgets.Label(rect2, str1);
        string str2 = (yRange.min + (float) (((double) yRange.max - (double) yRange.min) * ((double) index / 50.0))).RoundTo(0.1f).ToString();
        Vector2 vector2_9 = Text.CalcSize(str2);
        Rect rect3;
        // ISSUE: explicit constructor call
        ((Rect) ref rect3).\u002Ector((float) ((double) ((Rect) ref rect).x + (double) num1 - 10.0) - vector2_9.x, vector2_6.y - vector2_9.y / 2f, vector2_9.x, vector2_9.y);
        Widgets.Label(rect3, str2);
      }
    }
  }

  private static void PlotFunction(
    Rect rect,
    Graph.Function function,
    FloatRange xRange,
    FloatRange yRange,
    List<CurvePoint> plotPoints,
    float progress = -1f,
    bool simplified = false,
    bool editable = true,
    bool drawCoordLabels = true)
  {
    bool mouseOverAnyPlotPoint = false;
    if (!plotPoints.NullOrEmpty<CurvePoint>())
    {
      if (plotPoints.Count > 1)
        Graph.DrawCurve(rect, function, xRange, yRange, progress, simplified);
      mouseOverAnyPlotPoint = Graph.DoDrawHandles(rect, xRange, yRange, plotPoints, editable, drawCoordLabels);
    }
    Graph.DrawCoordLabels(rect, function, xRange, yRange, mouseOverAnyPlotPoint, simplified);
  }

  private static void DrawCurve(
    Rect rect,
    Graph.Function function,
    FloatRange xRange,
    FloatRange yRange,
    float progress = -1f,
    bool simplified = false)
  {
    float num1 = Mathf.Abs((float) (((double) xRange.max - (double) xRange.min) / 500.0));
    if ((double) num1 <= 0.0)
      return;
    float min = xRange.min;
    Vector2 coord = function(min);
    if (simplified)
      coord.x = min;
    Vector2 vector2 = Graph.GraphCoordToScreenPos(rect, coord, xRange, yRange);
    float num2 = xRange.min + num1;
    for (float x = num2; (double) x <= (double) xRange.max; x += num1)
    {
      coord = function(x);
      if (simplified)
        coord.x = x;
      if (!float.IsNaN(coord.y) && !float.IsNaN(coord.x) && xRange.InRange(coord.x) && yRange.InRange(coord.y))
      {
        Vector2 screenPos = Graph.GraphCoordToScreenPos(rect, coord, xRange, yRange);
        if (xRange.InRange(coord.x) && yRange.InRange(coord.y))
        {
          Widgets.DrawLine(vector2, screenPos, Color.white, 1f);
          if ((double) progress >= 0.0 && (double) progress <= 1.0)
          {
            float num3 = (float) (((double) x - (double) num2) / ((double) xRange.max - (double) num2));
            float num4 = (float) (((double) x + (double) num1 - (double) num2) / ((double) xRange.max - (double) num2));
            if ((double) num3 < (double) progress && (double) num4 > (double) progress)
            {
              float length1 = vector2.x - ((Rect) ref rect).x;
              if ((double) length1 > 0.0)
                UIElements.DrawLineHorizontal(((Rect) ref rect).x, vector2.y, length1, Graph.progressColor);
              float length2 = (float) -((double) ((Rect) ref rect).yMax - (double) vector2.y);
              if ((double) length2 < 0.0)
                UIElements.DrawLineVertical(vector2.x, ((Rect) ref rect).yMax, length2, Graph.progressColor);
            }
          }
        }
        vector2 = screenPos;
      }
    }
  }

  private static bool DoDrawHandles(
    Rect rect,
    FloatRange xRange,
    FloatRange yRange,
    List<CurvePoint> plotPoints,
    bool editable = true,
    bool drawCoordLabels = true)
  {
    bool flag1 = false;
    for (int index = 0; index < plotPoints.Count; ++index)
    {
      CurvePoint plotPoint = plotPoints[index];
      Vector2 screenPos = Graph.GraphCoordToScreenPos(rect, CurvePoint.op_Implicit(plotPoint), xRange, yRange);
      if (xRange.InRange(((CurvePoint) ref plotPoint).x))
      {
        Rect rect1;
        // ISSUE: explicit constructor call
        ((Rect) ref rect1).\u002Ector(screenPos.x - 4f, screenPos.y - 4f, 8f, 8f);
        bool flag2 = Mouse.IsOver(rect1);
        flag1 |= flag2;
        GUI.DrawTexture(rect1, (Texture) BaseContent.WhiteTex);
        if (drawCoordLabels)
        {
          string str = $" P{index} ({((CurvePoint) ref plotPoint).x:0.##}, {((CurvePoint) ref plotPoint).y:0.##}) ";
          Vector2 vector2 = Text.CalcSize(str);
          (float, float) valueTuple = (double) ((CurvePoint) ref plotPoint).x <= (double) xRange.min + (double) xRange.max / 2.0 ? (screenPos.x + ((Rect) ref rect1).width, screenPos.y - ((Rect) ref rect1).height / 2f) : (screenPos.x - ((Rect) ref rect1).width - vector2.x, screenPos.y - ((Rect) ref rect1).height / 2f);
          Rect rect2;
          // ISSUE: explicit constructor call
          ((Rect) ref rect2).\u002Ector(valueTuple.Item1, valueTuple.Item2, vector2.x, vector2.y);
          Widgets.DrawMenuSection(rect2);
          Widgets.Label(rect2, str);
        }
        if (editable)
        {
          if (((Event.current.type != null ? 0 : (Event.current.button == 0 ? 1 : 0)) & (flag2 ? 1 : 0)) != 0)
          {
            Graph.draggingPlotPointIndex = index;
            Event.current.Use();
          }
          if (Event.current.type == 3 && Event.current.button == 0 && Graph.draggingPlotPointIndex == index)
          {
            Vector2 mousePosition = Event.current.mousePosition;
            (float x, float y) graphCoord = Graph.ScreenPosToGraphCoord(rect, mousePosition, xRange, yRange);
            FloatRange floatRange1;
            // ISSUE: explicit constructor call
            ((FloatRange) ref floatRange1).\u002Ector(Mathf.Min(xRange.min, xRange.max), Mathf.Max(xRange.min, xRange.max));
            FloatRange floatRange2;
            // ISSUE: explicit constructor call
            ((FloatRange) ref floatRange2).\u002Ector(Mathf.Min(yRange.min, yRange.max), Mathf.Max(yRange.min, yRange.max));
            graphCoord.x = graphCoord.x.Clamp(floatRange1.min, floatRange1.max);
            graphCoord.y = graphCoord.y.Clamp(floatRange2.min, floatRange2.max);
            plotPoints[index] = new CurvePoint(graphCoord.x, graphCoord.y);
            Event.current.Use();
          }
          if (Event.current.type == 1 && Event.current.button == 0 && Graph.draggingPlotPointIndex >= 0)
          {
            Graph.draggingPlotPointIndex = -1;
            Event.current.Use();
          }
        }
      }
    }
    return flag1;
  }

  private static void DrawCoordLabels(
    Rect rect,
    Graph.Function function,
    FloatRange xRange,
    FloatRange yRange,
    bool mouseOverAnyPlotPoint,
    bool simplified = false)
  {
    Vector2 mousePosition = Event.current.mousePosition;
    (float x, float y) graphCoord = Graph.ScreenPosToGraphCoord(rect, mousePosition, xRange, yRange);
    Vector2 coord = function(graphCoord.x);
    if (simplified)
      coord.x = graphCoord.x;
    Vector2 screenPos = Graph.GraphCoordToScreenPos(rect, coord, xRange, yRange);
    if (!xRange.InRange(coord.x) || (double) Vector2.Distance(mousePosition, screenPos) > 50.0 || mouseOverAnyPlotPoint || Graph.draggingPlotPointIndex >= 0)
      return;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(screenPos.x - 4f, screenPos.y - 4f, 8f, 8f);
    Widgets.DrawTextureFitted(rect1, (Texture) UIData.CurvePoint, 1f, 1f);
    string str = $" ({coord.x:0.##}, {coord.y:0.##}) ";
    Vector2 vector2 = Text.CalcSize(str);
    (float, float) valueTuple = (double) coord.x <= (double) xRange.min + (double) xRange.max / 2.0 ? (screenPos.x + ((Rect) ref rect1).width, screenPos.y - ((Rect) ref rect1).height / 2f) : (screenPos.x - ((Rect) ref rect1).width - vector2.x, screenPos.y - ((Rect) ref rect1).height / 2f);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(valueTuple.Item1, valueTuple.Item2, vector2.x, vector2.y);
    Widgets.DrawMenuSection(rect2);
    Widgets.Label(rect2, str);
  }

  private static Vector2 GraphCoordToScreenPos(
    Rect rect,
    Vector2 coord,
    FloatRange xRange,
    FloatRange yRange)
  {
    float num1 = (float) (((double) coord.x - (double) xRange.min) / ((double) xRange.max - (double) xRange.min));
    float num2 = (float) (((double) coord.y - (double) yRange.min) / ((double) yRange.max - (double) yRange.min));
    return new Vector2(((Rect) ref rect).x + ((Rect) ref rect).width * num1, (float) ((double) ((Rect) ref rect).y + (double) ((Rect) ref rect).height - (double) ((Rect) ref rect).height * (double) num2));
  }

  private static (float x, float y) ScreenPosToGraphCoord(
    Rect rect,
    Vector2 mousePos,
    FloatRange xRange,
    FloatRange yRange)
  {
    float num1 = (mousePos.x - ((Rect) ref rect).x) / ((Rect) ref rect).width;
    float num2 = (((Rect) ref rect).y + ((Rect) ref rect).height - mousePos.y) / ((Rect) ref rect).height;
    float num3 = num1 * (xRange.max - xRange.min) + xRange.min;
    float num4 = num2 * (yRange.max - yRange.min) + yRange.min;
    return (num3.RoundTo(0.01f), num4.RoundTo(0.01f));
  }

  public delegate Vector2 Function(float x);

  public enum GraphType
  {
    Linear,
    Bezier,
    Lagrange,
    Staircase,
    Freeform,
  }
}
