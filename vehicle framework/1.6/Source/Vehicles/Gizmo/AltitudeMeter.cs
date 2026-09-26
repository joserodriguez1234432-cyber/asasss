// Decompiled with JetBrains decompiler
// Type: Vehicles.World.AltitudeMeter
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld.Planet;
using SmashTools;
using System;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.World;

public static class AltitudeMeter
{
  public const float MinimumAltitude = 0.0f;
  public const float MaximumAltitude = 210000f;
  public const float MinAltitudeScreenHeight = 16f;
  public const float MaxAltitudeScreenHeight = 49f;
  public const float WindowHeight = 200f;
  public const float InfoWindoHeight = 75f;
  public static Vector2 scrollPos = Vector2.zero;
  public static readonly Color WindowBGBorderColor;

  public static Vector2 MeterSize => new Vector2(75f, 710f);

  public static Vector2 AltitudeScreenPos => new Vector2(0.0f, AltitudeMeter.PaneTopY - 275f);

  public static float PaneTopY
  {
    get
    {
      float paneTopY = (float) UI.screenHeight - 165f;
      if (Current.ProgramState == 2)
        paneTopY -= 35f;
      return paneTopY;
    }
  }

  public static void DrawAltitudeMeter(AerialVehicleInFlight aerialVehicle)
  {
    try
    {
      Rect rect = new Rect(AltitudeMeter.AltitudeScreenPos, AltitudeMeter.MeterSize);
      Rect rect1 = new Rect(rect);
      ((Rect) ref rect1).width = (float) ((double) ((Rect) ref rect).width * 3.0 + 10.0);
      ((Rect) ref rect1).height = 275f;
      Rect windowRect = rect1;
      float elevation = (AltitudeMeter.MeterSize.y - aerialVehicle.Elevation / 210000f * AltitudeMeter.MeterSize.y).Clamp(49f, AltitudeMeter.MeterSize.y - 16f);
      Find.WindowStack.ImmediateWindow(((object) aerialVehicle).GetHashCode(), windowRect, (WindowLayer) 0, (Action) (() =>
      {
        TextAnchor anchor = Text.Anchor;
        GameFont font = Text.Font;
        Color color = GUI.color;
        Rect rect2 = GenUI.AtZero(rect);
        ((Rect) ref windowRect).x = ((Rect) ref rect).width + 5f;
        ((Rect) ref windowRect).y = 5f;
        ((Rect) ref windowRect).height = 200f;
        GUI.BeginScrollView(windowRect, new Vector2(((Rect) ref windowRect).x, elevation - 100f), rect2, GUIStyle.none, GUIStyle.none);
        if ((double) elevation <= 210000.0)
        {
          Rect rect3;
          // ISSUE: explicit constructor call
          ((Rect) ref rect3).\u002Ector(0.0f, ((Rect) ref windowRect).y + elevation, ((Rect) ref rect2).width, 1f);
          GUI.DrawTexture(rect3, (double) elevation >= (double) AltitudeMeter.MeterSize.y / 2.0 ? (Texture) BaseContent.BlackTex : (Texture) BaseContent.WhiteTex);
        }
        GUI.color = AltitudeMeter.WindowBGBorderColor;
        Widgets.DrawLineHorizontal(0.0f, (float) ((double) ((Rect) ref windowRect).y + (double) elevation + (double) AltitudeMeter.MeterSize.y / 2.0), ((Rect) ref rect2).width);
        Widgets.DrawLineVertical(((Rect) ref rect2).width, ((Rect) ref windowRect).y, AltitudeMeter.MeterSize.y);
        GUI.color = color;
        Text.Font = (GameFont) 1;
        float num1 = Text.CalcHeight(aerialVehicle.Elevation.ToString(), ((Rect) ref rect2).width);
        Rect rect4;
        // ISSUE: explicit constructor call
        ((Rect) ref rect4).\u002Ector(((Rect) ref rect2).width + 5f, (float) ((double) ((Rect) ref windowRect).y + (double) elevation - (double) num1 / 2.0), ((Rect) ref rect2).width - 5f, num1);
        Widgets.DrawMenuSection(rect4);
        Text.Font = (GameFont) 0;
        Text.Anchor = (TextAnchor) 4;
        int num2 = Mathf.RoundToInt(aerialVehicle.Elevation);
        GUI.Label(rect4, num2.ToString(), Text.CurFontStyle);
        GUI.EndScrollView(false);
        Text.Anchor = anchor;
        Text.Font = font;
        GUI.color = color;
      }), true, false, 0.0f, (Action) null, false);
    }
    catch (Exception ex)
    {
      SmashLog.Error($"Exception thrown while trying to draw <type>AltitudeMeter</type> for {((WorldObject) aerialVehicle)?.Label ?? "NULL"}. Exception=\"{ex}\"");
    }
  }

  static AltitudeMeter()
  {
    ColorInt colorInt = new ColorInt(97, 108, 122);
    AltitudeMeter.WindowBGBorderColor = ((ColorInt) ref colorInt).ToColor;
  }
}
