// Decompiled with JetBrains decompiler
// Type: Vehicles.TexData
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
[StaticConstructorOnStartup]
public static class TexData
{
  public const int CloseRange = 5;
  public const int MidRange = 15;
  public const int FarRange = 25;
  public static readonly Texture2D YellowTex;
  public static readonly Texture2D YellowOrangeTex;
  public static readonly Texture2D OrangeTex;
  public static readonly Texture2D OrangeRedTex;
  public static readonly Texture2D RedTex;
  public static readonly Texture2D FillableBarTexture;
  public static readonly Texture2D FullBarTex;
  public static readonly Texture2D EmptyBarTex;
  public static readonly Texture2D ClearBarTexture;
  public static readonly Material LineMatWhite;
  public static readonly Material LineMatRed;
  public static readonly Material WorldLineMatWhite;
  public static readonly Material WorldLineMatYellow;
  public static readonly Material WorldLineMatRed;
  public static readonly Material OneSidedWorldLineMatWhite;
  public static readonly Material OneSidedWorldLineMatRed;
  public static readonly Texture2D TutorArrowRight;
  public static readonly Texture2D Rename;
  public static readonly Texture2D Drop;
  public static readonly List<Texture2D> FireIcons;
  public static readonly Texture2D TargeterMouseAttachment;
  public static readonly Texture2D CaravanIcon;
  public static readonly Texture2D FlickerIcon;
  public static readonly Texture2D LaunchCommandTex;
  public static readonly Texture2D TradeCommandTex;
  public static readonly Texture2D OfferGiftsCommandTex;
  public static readonly Texture2D TradeArrow;
  public static readonly Color IconColor;
  public static readonly Color RedReadable;
  public static readonly Color YellowReadable;
  public static readonly Color HighlightColor;
  public static readonly Color StaticHighlightColor;
  public static readonly Color SevereDamage;
  public static readonly Color ModerateDamage;
  public static readonly Color MinorDamage;
  public static readonly Color WorkingCondition;
  public static readonly Color Enhanced;

  public static Texture2D HeatColorPercent(float percent)
  {
    return (double) percent <= 0.25 ? TexData.YellowTex : ((double) percent <= 0.5 ? TexData.YellowOrangeTex : ((double) percent <= 0.75 ? TexData.OrangeTex : ((double) percent <= 1.0 ? TexData.OrangeRedTex : TexData.RedTex)));
  }

  static TexData()
  {
    ColorInt colorInt1 = new ColorInt((int) byte.MaxValue, 210, 45);
    TexData.YellowTex = SolidColorMaterials.NewSolidColorTexture(((ColorInt) ref colorInt1).ToColor);
    ColorInt colorInt2 = new ColorInt((int) byte.MaxValue, 175, 45);
    TexData.YellowOrangeTex = SolidColorMaterials.NewSolidColorTexture(((ColorInt) ref colorInt2).ToColor);
    ColorInt colorInt3 = new ColorInt((int) byte.MaxValue, 110, 15);
    TexData.OrangeTex = SolidColorMaterials.NewSolidColorTexture(((ColorInt) ref colorInt3).ToColor);
    ColorInt colorInt4 = new ColorInt((int) byte.MaxValue, 75, 15);
    TexData.OrangeRedTex = SolidColorMaterials.NewSolidColorTexture(((ColorInt) ref colorInt4).ToColor);
    ColorInt colorInt5 = new ColorInt(155, 30, 30);
    TexData.RedTex = SolidColorMaterials.NewSolidColorTexture(((ColorInt) ref colorInt5).ToColor);
    TexData.FillableBarTexture = SolidColorMaterials.NewSolidColorTexture(0.5f, 0.5f, 0.5f, 0.5f);
    TexData.FullBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.35f, 0.35f, 0.2f));
    TexData.EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(Color.black);
    TexData.ClearBarTexture = BaseContent.ClearTex;
    TexData.LineMatWhite = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, Color.white);
    TexData.LineMatRed = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, Color.red);
    TexData.WorldLineMatWhite = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.WorldOverlayTransparent, Color.white, 3590);
    TexData.WorldLineMatYellow = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.WorldOverlayTransparent, Color.yellow, 3590);
    TexData.WorldLineMatRed = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.WorldOverlayTransparent, Color.red, 3590);
    TexData.OneSidedWorldLineMatWhite = MaterialPool.MatFrom(GenDraw.OneSidedLineTexPath, ShaderDatabase.WorldOverlayTransparent, Color.white, 3590);
    TexData.OneSidedWorldLineMatRed = MaterialPool.MatFrom(GenDraw.OneSidedLineTexPath, ShaderDatabase.WorldOverlayTransparent, Color.red, 3590);
    TexData.TutorArrowRight = ContentFinder<Texture2D>.Get("UI/Overlays/TutorArrowRight", true);
    TexData.Rename = ContentFinder<Texture2D>.Get("UI/Buttons/Rename", true);
    TexData.Drop = ContentFinder<Texture2D>.Get("UI/Buttons/Drop", true);
    TexData.FireIcons = ContentFinder<Texture2D>.GetAllInFolder("Things/Special/Fire").ToList<Texture2D>();
    TexData.TargeterMouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/LaunchableMouseAttachment", true);
    TexData.CaravanIcon = ContentFinder<Texture2D>.Get("UI/Commands/FormCaravan", true);
    TexData.FlickerIcon = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower", true);
    TexData.LaunchCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip", true);
    TexData.TradeCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/Trade", true);
    TexData.OfferGiftsCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/OfferGifts", true);
    TexData.TradeArrow = ContentFinder<Texture2D>.Get("UI/Widgets/TradeArrow", true);
    TexData.IconColor = new Color(0.84f, 0.84f, 0.84f);
    TexData.RedReadable = new Color(1f, 0.2f, 0.2f);
    TexData.YellowReadable = new Color(1f, 1f, 0.2f);
    TexData.HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    TexData.StaticHighlightColor = new Color(0.75f, 0.75f, 0.85f, 1f);
    TexData.SevereDamage = new Color(0.75f, 0.45f, 0.45f);
    TexData.ModerateDamage = new Color(0.55f, 0.55f, 0.55f);
    TexData.MinorDamage = new Color(0.7f, 0.7f, 0.7f);
    TexData.WorkingCondition = new Color(0.6f, 0.8f, 0.65f);
    TexData.Enhanced = new Color(0.5f, 0.5f, 0.9f);
  }
}
