// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_Rect
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using UnityEngine;

#nullable disable
namespace SmashTools;

public static class Ext_Rect
{
  public static Rect ToSquare(this Rect rect)
  {
    if (Mathf.Approximately(((Rect) ref rect).width, ((Rect) ref rect).height))
      return rect;
    if ((double) ((Rect) ref rect).width > (double) ((Rect) ref rect).height)
    {
      float num = (float) (((double) ((Rect) ref rect).width - (double) ((Rect) ref rect).height) / 2.0);
      ref Rect local1 = ref rect;
      ((Rect) ref local1).xMin = ((Rect) ref local1).xMin + num;
      ref Rect local2 = ref rect;
      ((Rect) ref local2).xMax = ((Rect) ref local2).xMax - num;
      return rect;
    }
    float num1 = (float) (((double) ((Rect) ref rect).height - (double) ((Rect) ref rect).width) / 2.0);
    ref Rect local3 = ref rect;
    ((Rect) ref local3).yMin = ((Rect) ref local3).yMin + num1;
    ref Rect local4 = ref rect;
    ((Rect) ref local4).yMax = ((Rect) ref local4).yMax - num1;
    return rect;
  }

  public static Rect[] SplitVertically(this Rect rect, int splits, float buffer = 0.0f)
  {
    if (splits <= 1)
      throw new InvalidOperationException();
    float num = (float) ((double) ((Rect) ref rect).width / (double) splits - (double) buffer * (double) splits);
    Rect[] rectArray = new Rect[splits];
    for (int index = 0; index < splits; ++index)
    {
      Rect rect1;
      // ISSUE: explicit constructor call
      ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x + (float) index * (num + buffer), ((Rect) ref rect).y, num, ((Rect) ref rect).height);
      rectArray[index] = rect1;
    }
    return rectArray;
  }

  public static Rect[] SplitVertically(this Rect rect, float[] widthPercents, float buffer)
  {
    int length = widthPercents.Length > 1 ? widthPercents.Length : throw new InvalidOperationException();
    float num1 = (float) (length - 1) * buffer;
    float num2 = ((Rect) ref rect).width - num1;
    Rect[] rectArray = new Rect[length];
    for (int index = 0; index < length; ++index)
    {
      float num3 = widthPercents[index] * num2;
      Rect rect1;
      // ISSUE: explicit constructor call
      ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x + (float) index * (num3 + buffer), ((Rect) ref rect).y, num3, ((Rect) ref rect).height);
      rectArray[index] = rect1;
    }
    return rectArray;
  }

  [Flags]
  private enum RectEdge
  {
    None = 0,
    Left = 1,
    Right = 2,
    Top = 4,
    Bottom = 8,
    BottomLeft = Bottom | Right, // 0x0000000A
    TopLeft = Top | Right, // 0x00000006
    TopRight = Top | Left, // 0x00000005
    BottomRight = Bottom | Left, // 0x00000009
  }
}
