// Decompiled with JetBrains decompiler
// Type: SmashTools.UIData
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

[StaticConstructorOnStartup]
public static class UIData
{
  public static readonly Texture2D FillableBarTexture = SolidColorMaterials.NewSolidColorTexture(0.5f, 0.5f, 0.5f, 0.5f);
  public static readonly Texture2D ClearBarTexture = BaseContent.ClearTex;
  public static readonly Texture2D FillableBarBackgroundTex = SolidColorMaterials.NewSolidColorTexture(Color.black);
  public static readonly Texture2D FillableBarInnerTex;
  public static readonly Color ProgressBarRed;
  public static readonly Texture2D FillableBarProgressBar;
  public static readonly Texture2D FillableBarProgressBarBG;
  public static readonly Texture2D TransparentBlackBG;
  public static readonly Texture2D CurvePoint;
  public static readonly Texture2D RadioButOffTex;
  public static readonly Texture2D TargetLevelArrow;

  static UIData()
  {
    ColorInt colorInt = new ColorInt(19, 22, 27);
    UIData.FillableBarInnerTex = SolidColorMaterials.NewSolidColorTexture(((ColorInt) ref colorInt).ToColor);
    UIData.ProgressBarRed = new Color(0.9f, 0.15f, 0.1f, 1f);
    UIData.FillableBarProgressBar = SolidColorMaterials.NewSolidColorTexture(UIData.ProgressBarRed);
    UIData.FillableBarProgressBarBG = SolidColorMaterials.NewSolidColorTexture(0.35f, 0.35f, 0.35f, 1f);
    UIData.TransparentBlackBG = SolidColorMaterials.NewSolidColorTexture(0.1f, 0.1f, 0.1f, 0.1f);
    UIData.CurvePoint = ContentFinder<Texture2D>.Get("UI/Widgets/Dev/CurvePoint", true);
    UIData.RadioButOffTex = ContentFinder<Texture2D>.Get("UI/Widgets/RadioButOff", true);
    UIData.TargetLevelArrow = ContentFinder<Texture2D>.Get("UI/Misc/BarInstantMarkerRotated", true);
  }
}
