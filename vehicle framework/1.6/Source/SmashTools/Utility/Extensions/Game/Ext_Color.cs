// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_Color
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using UnityEngine;

#nullable disable
namespace SmashTools;

public static class Ext_Color
{
  public static Color AddNoAlpha(this Color color, float r, float g, float b)
  {
    return new Color(color.r + r, color.g + g, color.b + b);
  }

  public static Color SubtractNoAlpha(this Color color, float r, float g, float b)
  {
    return new Color(color.r - r, color.g - g, color.b - b);
  }

  public static Color Add255NoAlpha(this Color color, int r, int g, int b)
  {
    float r1 = (float) r / (float) byte.MaxValue;
    float g1 = (float) g / (float) byte.MaxValue;
    float b1 = (float) b / (float) byte.MaxValue;
    return color.AddNoAlpha(r1, g1, b1);
  }

  public static Color Subtract255NoAlpha(this Color color, int r, int g, int b)
  {
    float r1 = (float) r / (float) byte.MaxValue;
    float g1 = (float) g / (float) byte.MaxValue;
    float b1 = (float) b / (float) byte.MaxValue;
    return color.SubtractNoAlpha(r1, g1, b1);
  }
}
