// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.AnimationGraph
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools.Animations;

public static class AnimationGraph
{
  public const float DragHandleSize = 12f;

  public static void DrawAnimationCurve(
    Rect rect,
    Rect visibleRect,
    AnimationCurve curve,
    Color color,
    float spacing)
  {
    if (curve == null || curve.points.NullOrEmpty<KeyFrame>())
      return;
    AnimationGraph.DrawCurve(rect, spacing, curve, color);
  }

  private static void DrawCurve(Rect rect, float spacing, AnimationCurve curve, Color color)
  {
    float num1 = 1f;
    if ((double) num1 <= 0.0)
      return;
    FloatRange rangeX = curve.RangeX;
    float min = curve.RangeX.min;
    float num2 = curve.Function(min);
    Vector2 vector2 = AnimationGraph.GraphCoordToScreenPos(rect, new Vector2(min, num2), rangeX, spacing);
    for (float num3 = rangeX.min + num1; (double) num3 <= (double) rangeX.max; num3 += num1)
    {
      float f = curve.Function(num3);
      if (!float.IsNaN(f) && !float.IsNaN(num3))
      {
        Vector2 screenPos = AnimationGraph.GraphCoordToScreenPos(rect, new Vector2(num3, f), rangeX, spacing);
        Widgets.DrawLine(vector2, screenPos, color, 1f);
        vector2 = screenPos;
      }
    }
  }

  public static Vector2 GraphCoordToScreenPos(
    Rect rect,
    Vector2 coord,
    FloatRange xRange,
    float spacing)
  {
    float num1 = (float) (((double) coord.x - (double) xRange.min) / ((double) xRange.max - (double) xRange.min));
    float num2 = coord.y * spacing;
    return new Vector2(((Rect) ref rect).x + ((Rect) ref rect).width * num1, ((Rect) ref rect).y + ((Rect) ref rect).height / 2f - num2);
  }

  public static (int frame, float value) ScreenPosToGraphCoord(
    Rect rect,
    Rect visibleRect,
    Vector2 mousePos,
    FloatRange xRange,
    float scrollY,
    float spacing)
  {
    float num1 = mousePos.x + ((Rect) ref visibleRect).x;
    float num2 = (float) ((double) mousePos.y + (double) ((Rect) ref visibleRect).y - 16.0) + scrollY;
    float num3 = (num1 - ((Rect) ref rect).x) / ((Rect) ref rect).width;
    float num4 = ((Rect) ref rect).y + ((Rect) ref rect).height / 2f - num2;
    return (Mathf.RoundToInt(num3 * (xRange.max - xRange.min) + xRange.min), (num4 / spacing).RoundTo(1f / 1000f));
  }

  public delegate float Function(float frame);
}
