// Decompiled with JetBrains decompiler
// Type: SmashTools.UIElements
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using RimWorld;
using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

#nullable disable
namespace SmashTools;

[StaticConstructorOnStartup]
[UsedImplicitly]
public static class UIElements
{
  public const float CheckboxSize = 24f;
  private static readonly Regex ValidInputRegex = new Regex("^(\\#[A-Fa-f0-9]{0,7}$)");
  public static readonly Color InactiveColor = new Color(0.37f, 0.37f, 0.37f, 0.8f);
  public static readonly Color RangeControlTextColor = new Color(0.6f, 0.6f, 0.6f);
  public static readonly Color WindowBgBorderColor;
  public static readonly Color MenuSectionBgBorderColor;
  private static int sliderDraggingID;
  private static float lastDragSliderSoundTime;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToHex(this Color c) => "#" + ColorUtility.ToHtmlStringRGB(c);

  public static void Header(
    Rect rect,
    string header,
    Color highlightColor,
    GameFont fontSize = 2,
    TextAnchor anchor = 3)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(fontSize, anchor);
    try
    {
      using (new TextBlock(highlightColor))
        GUI.DrawTexture(rect, (Texture) BaseContent.WhiteTex);
      Widgets.Label(rect, header);
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  public static void CheckboxDraw(
    float x,
    float y,
    bool active,
    bool disabled,
    float size = 24f,
    Texture2D texChecked = null,
    Texture2D texUnchecked = null)
  {
    UIElements.CheckboxDraw(new Rect(x, y, size, size), active, disabled, texChecked, texUnchecked);
  }

  public static void CheckboxDraw(
    Rect rect,
    bool active,
    bool disabled,
    Texture2D texChecked = null,
    Texture2D texUnchecked = null)
  {
    if (disabled)
      GUIState.Disable();
    Texture2D texture2D = !active ? (Object.op_Inequality((Object) texUnchecked, (Object) null) ? texUnchecked : Widgets.CheckboxOffTex) : (Object.op_Inequality((Object) texChecked, (Object) null) ? texChecked : Widgets.CheckboxOnTex);
    GUI.DrawTexture(rect, (Texture) texture2D);
    GUIState.Enable();
  }

  public static bool CheckboxLabeled(
    Rect rect,
    string label,
    bool checkOn,
    bool disabled = false,
    Texture2D texChecked = null,
    Texture2D texUnchecked = null,
    TextAnchor labelAnchor = 3)
  {
    bool checkOn1 = checkOn;
    UIElements.CheckboxLabeled(rect, label, ref checkOn1, disabled, texChecked, texUnchecked, labelAnchor);
    return checkOn1;
  }

  public static bool CheckboxLabeled(
    Rect rect,
    string label,
    ref bool checkOn,
    bool disabled = false,
    Texture2D texChecked = null,
    Texture2D texUnchecked = null,
    TextAnchor labelAnchor = 3)
  {
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(rect);
    ((Rect) ref rect1).width = ((Rect) ref rect).width - 24f;
    Widgets.Label(rect1, label);
    return UIElements.CheckboxButton(rect, ref checkOn, disabled);
  }

  public static bool CheckboxButton(Rect rect, ref bool value, bool disabled = false)
  {
    bool flag = false;
    if (!disabled && Widgets.ButtonInvisible(rect, true))
    {
      value = !value;
      flag = true;
      if (value)
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Checkbox_TurnedOn, (Map) null);
      else
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Checkbox_TurnedOff, (Map) null);
    }
    UIElements.CheckboxDraw((float) ((double) ((Rect) ref rect).x + (double) ((Rect) ref rect).width - 24.0), ((Rect) ref rect).y, value, disabled, 20f);
    return flag;
  }

  public static bool CollapseButton(
    Rect rect,
    ref bool expanded,
    bool doMouseoverSound = true,
    string tooltip = null)
  {
    return UIElements.CollapseButton(rect, ref expanded, Color.white, doMouseoverSound, tooltip);
  }

  public static bool CollapseButton(
    Rect rect,
    ref bool expanded,
    Color baseColor,
    bool doMouseoverSound = true,
    string tooltip = null)
  {
    return UIElements.CollapseButton(rect, ref expanded, baseColor, GenUI.MouseoverColor, doMouseoverSound, tooltip);
  }

  public static bool CollapseButton(
    Rect rect,
    ref bool expanded,
    Color baseColor,
    Color mouseoverColor,
    bool doMouseoverSound = true,
    string tooltip = null)
  {
    bool flag = Widgets.ButtonImage(rect, expanded ? TexButton.Collapse : TexButton.Reveal, baseColor, mouseoverColor, doMouseoverSound, tooltip);
    if (flag)
      expanded = !expanded;
    return flag;
  }

  public static bool ReverseRadioButton(Rect rect, string label, bool enabled)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(Color.white);
    bool flag;
    try
    {
      Verse.Text.Anchor = (TextAnchor) 3;
      flag = Widgets.ButtonInvisible(rect, true);
      if (flag && !enabled)
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_Tiny, (Map) null);
      Rect rect1;
      // ISSUE: explicit constructor call
      ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x + 28f, ((Rect) ref rect).y, ((Rect) ref rect).width - 24f, ((Rect) ref rect).height);
      Widgets.Label(rect1, label);
      UIElements.RadioButtonDraw(((Rect) ref rect).x, (float) ((double) ((Rect) ref rect).y + (double) ((Rect) ref rect).height / 2.0 - 12.0), enabled);
    }
    finally
    {
      textBlock.Dispose();
    }
    return flag;
  }

  public static void RadioButtonDraw(float x, float y, bool chosen)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(Color.white);
    try
    {
      Texture2D texture2D = chosen ? Widgets.RadioButOnTex : UIData.RadioButOffTex;
      GUI.DrawTexture(new Rect(x, y, 24f, 24f), (Texture) texture2D);
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  public static void Vector2Box(
    Rect rect,
    string label,
    ref Vector2 value,
    string tooltip = null,
    float labelProportion = 0.45f,
    float subLabelProportions = 0.15f,
    float buffer = 0.0f)
  {
    value = UIElements.Vector2Box(rect, label, value, tooltip, labelProportion, subLabelProportions, buffer);
  }

  public static Vector2 Vector2Box(
    Rect rect,
    string label,
    Vector2 value,
    string tooltip = null,
    float labelProportion = 0.45f,
    float subLabelProportions = 0.15f,
    float buffer = 0.0f)
  {
    (float left, float right) tuple = UIElements.SplitFloatBoxes(rect, label, (value.x, "x"), (value.y, "y"), tooltip, labelProportion, subLabelProportions, buffer);
    return new Vector2(tuple.left, tuple.right);
  }

  public static void FloatRangeBox(
    Rect rect,
    string label,
    ref FloatRange value,
    string tooltip = null,
    float labelProportion = 0.45f,
    float subLabelProportions = 0.25f,
    float buffer = 0.0f)
  {
    value = UIElements.FloatRangeBox(rect, label, value, tooltip, labelProportion, subLabelProportions, buffer);
  }

  public static FloatRange FloatRangeBox(
    Rect rect,
    string label,
    FloatRange value,
    string tooltip = null,
    float labelProportion = 0.45f,
    float subLabelProportions = 0.25f,
    float buffer = 0.0f)
  {
    (float left, float right) tuple = UIElements.SplitFloatBoxes(rect, label, (value.min, "min"), (value.max, "max"), tooltip, labelProportion, subLabelProportions, buffer);
    return new FloatRange(tuple.left, tuple.right);
  }

  private static (float left, float right) SplitFloatBoxes(
    Rect rect,
    string label,
    (float value, string label) leftBox,
    (float value, string label) rightBox,
    string tooltip = null,
    float labelProportion = 0.45f,
    float subLabelProportions = 0.15f,
    float buffer = 0.0f)
  {
    float num1 = leftBox.value;
    float num2 = rightBox.value;
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(Color.white);
    try
    {
      if (!tooltip.NullOrEmpty<char>())
        TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tooltip));
      Rect rect1;
      // ISSUE: explicit constructor call
      ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y, ((Rect) ref rect).width * labelProportion, ((Rect) ref rect).height);
      if (!label.NullOrEmpty<char>())
        Widgets.Label(rect1, label);
      Rect rect2;
      // ISSUE: explicit constructor call
      ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).xMax, ((Rect) ref rect).y, ((Rect) ref rect).width - ((Rect) ref rect1).width, ((Rect) ref rect).height);
      Rect[] rectArray = rect2.SplitVertically(2, buffer);
      UIElements.NumericBox<float>(rectArray[0], ref num1, leftBox.label, string.Empty, string.Empty, float.MinValue, float.MaxValue, subLabelProportions);
      UIElements.NumericBox<float>(rectArray[1], ref num2, rightBox.label, string.Empty, string.Empty, float.MinValue, float.MaxValue, subLabelProportions);
    }
    finally
    {
      textBlock.Dispose();
    }
    return (num1, num2);
  }

  public static void Vector3Box(
    Rect rect,
    string label,
    ref Vector3 value,
    string tooltip = null,
    float labelProportion = 0.45f,
    float subLabelProportions = 0.15f,
    float buffer = 0.0f)
  {
    value = UIElements.Vector3Box(rect, label, value, tooltip, labelProportion, subLabelProportions, buffer);
  }

  public static Vector3 Vector3Box(
    Rect rect,
    string label,
    Vector3 value,
    string tooltip = null,
    float labelProportion = 0.45f,
    float subLabelProportions = 0.15f,
    float buffer = 0.0f)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(Color.white);
    try
    {
      float x = value.x;
      float y = value.y;
      float z = value.z;
      if (!tooltip.NullOrEmpty<char>())
        TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tooltip));
      Rect rect1;
      // ISSUE: explicit constructor call
      ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y, ((Rect) ref rect).width * labelProportion, ((Rect) ref rect).height);
      Widgets.Label(rect1, label);
      Rect rect2;
      // ISSUE: explicit constructor call
      ((Rect) ref rect2).\u002Ector(((Rect) ref rect).x + ((Rect) ref rect1).width, ((Rect) ref rect).y, ((Rect) ref rect).width - ((Rect) ref rect1).width, ((Rect) ref rect).height);
      Rect[] rectArray = rect2.SplitVertically(3, buffer);
      UIElements.NumericBox<float>(rectArray[0], ref x, "x", string.Empty, string.Empty, float.MinValue, float.MaxValue, subLabelProportions);
      UIElements.NumericBox<float>(rectArray[1], ref y, "y", string.Empty, string.Empty, float.MinValue, float.MaxValue, subLabelProportions);
      UIElements.NumericBox<float>(rectArray[2], ref z, "z", string.Empty, string.Empty, float.MinValue, float.MaxValue, subLabelProportions);
      value.x = x;
      value.y = y;
      value.z = z;
    }
    finally
    {
      textBlock.Dispose();
    }
    return value;
  }

  public static void NumericBox<T>(
    Rect rect,
    ref T value,
    string label,
    string tooltip,
    string disabledTooltip,
    float min = -2.14748365E+09f,
    float max = 2.14748365E+09f,
    float labelProportion = 0.45f)
    where T : struct
  {
    value = UIElements.NumericBox<T>(rect, value, label, tooltip, disabledTooltip, min, max, labelProportion);
  }

  public static T NumericBox<T>(
    Rect rect,
    T value,
    string label,
    string tooltip,
    string disabledTooltip,
    float min = -2.14748365E+09f,
    float max = 2.14748365E+09f,
    float labelProportion = 0.45f)
    where T : struct
  {
    string buffer = value.ToString();
    return UIElements.NumericBox<T>(rect, value, ref buffer, label, tooltip, disabledTooltip, min, max, labelProportion);
  }

  public static T NumericBox<T>(
    Rect rect,
    T value,
    ref string buffer,
    string label,
    string tooltip,
    string disabledTooltip,
    float min = -2.14748365E+09f,
    float max = 2.14748365E+09f,
    float labelProportion = 0.45f)
    where T : struct
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(Color.white);
    try
    {
      float num1 = Mathf.Clamp01(labelProportion);
      bool flag1 = !disabledTooltip.NullOrEmpty<char>();
      float num2 = ((Rect) ref rect).y + (float) (((double) ((Rect) ref rect).height - (double) Verse.Text.LineHeight) / 2.0);
      float num3 = ((Rect) ref rect).width * num1;
      float num4 = ((Rect) ref rect).width * (1f - num1);
      Rect rect1;
      // ISSUE: explicit constructor call
      ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x, num2, num3, ((Rect) ref rect).height);
      Rect rect2;
      // ISSUE: explicit constructor call
      ((Rect) ref rect2).\u002Ector(((Rect) ref rect).x + ((Rect) ref rect).width - num4, num2, num4, Verse.Text.LineHeight);
      bool flag2 = Mouse.IsOver(rect);
      if (flag1)
      {
        GUIState.Disable();
        TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(disabledTooltip));
      }
      else if (!tooltip.NullOrEmpty<char>())
      {
        if (flag2)
          Widgets.DrawHighlight(rect);
        TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tooltip));
      }
      Widgets.Label(rect1, label);
      Verse.Text.Anchor = (TextAnchor) 5;
      Widgets.TextFieldNumeric<T>(rect2, ref value, ref buffer, min, max);
      GUIState.Enable();
    }
    finally
    {
      textBlock.Dispose();
    }
    return value;
  }

  public static string HexField(string label, Rect rect, string text)
  {
    Widgets.Label(GenUI.LeftPart(rect, 0.3f), label);
    return Widgets.TextField(GenUI.RightPart(rect, 0.7f), "#" + text, 7, UIElements.ValidInputRegex).Replace("#", "");
  }

  public static void DrawLabel(
    Rect rect,
    string label,
    Color highlight,
    Color textColor,
    GameFont fontSize = 2,
    TextAnchor anchor = 3)
  {
    TextBlock textBlock1;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock1).\u002Ector(fontSize, highlight);
    try
    {
      GUI.DrawTexture(rect, (Texture) BaseContent.WhiteTex);
    }
    finally
    {
      textBlock1.Dispose();
    }
    TextBlock textBlock2;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock2).\u002Ector(anchor, textColor);
    try
    {
      Widgets.Label(rect, label);
    }
    finally
    {
      textBlock2.Dispose();
    }
  }

  public static bool ClickableLabel(
    Rect rect,
    string label,
    Color mouseOver,
    Color textColor,
    GameFont fontSize = 2,
    TextAnchor anchor = 3,
    Color? clickColor = null)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(fontSize, anchor);
    try
    {
      if (Mouse.IsOver(rect))
      {
        GUI.color = mouseOver;
        if (Input.GetMouseButton(0))
        {
          clickColor.GetValueOrDefault();
          if (!clickColor.HasValue)
            clickColor = new Color?(Color.grey);
          GUI.color = clickColor.Value;
        }
      }
      else
        GUI.color = textColor;
      Widgets.Label(rect, label);
    }
    finally
    {
      textBlock.Dispose();
    }
    if (!Widgets.ButtonInvisible(rect, true))
      return false;
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
    return true;
  }

  public static int HorizontalSlider(Rect rect, string tooltip, int value, int min, int max)
  {
    Rect rect1 = rect;
    ref Rect local = ref rect;
    ((Rect) ref local).y = ((Rect) ref local).y + ((Rect) ref rect).height / 4f;
    if (!tooltip.NullOrEmpty<char>())
    {
      if (Mouse.IsOver(rect1))
        Widgets.DrawHighlight(rect1);
      TooltipHandler.TipRegion(rect1, TipSignal.op_Implicit(tooltip));
    }
    return Mathf.RoundToInt(Widgets.HorizontalSlider(rect, (float) value, (float) min, (float) max, false, (string) null, (string) null, (string) null, -1f));
  }

  public static bool SliderLabeled(
    Rect rect,
    string label,
    string tooltip,
    string endSymbol,
    ref float value,
    float min,
    float max,
    float multiplier = 1f,
    int decimalPlaces = 2,
    float endValue = -1f,
    string maxValueDisplay = "")
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 0);
    try
    {
      Rect rect1 = rect;
      ref Rect local1 = ref rect;
      ((Rect) ref local1).y = ((Rect) ref local1).y + ((Rect) ref rect).height / 2f;
      ref Rect local2 = ref rect;
      ((Rect) ref local2).height = ((Rect) ref local2).height / 2f;
      string str = $"{Math.Round((double) value * (double) multiplier, decimalPlaces)}" + endSymbol;
      if (!maxValueDisplay.NullOrEmpty<char>() && (double) endValue > 0.0 && (double) value >= (double) endValue)
        str = maxValueDisplay;
      if (Mouse.IsOver(rect1))
        Widgets.DrawHighlight(rect1);
      if (!tooltip.NullOrEmpty<char>())
        TooltipHandler.TipRegion(rect1, TipSignal.op_Implicit(tooltip));
      float num = value;
      value = Widgets.HorizontalSlider(rect, value, min, max, false, (string) null, label, str, -1f);
      if ((double) endValue > 0.0 && (double) value >= (double) max)
        value = endValue;
      return !Mathf.Approximately(value, num);
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  public static void DrawLineHorizontal(float x, float y, float length, Color color)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(color);
    try
    {
      GUI.DrawTexture(new Rect(x, y, length, 1f), (Texture) BaseContent.WhiteTex);
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  public static void DrawLineVertical(float x, float y, float length, Color color)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(color);
    try
    {
      GUI.DrawTexture(new Rect(x, y, 1f, length), (Texture) BaseContent.WhiteTex);
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  public static void DrawLineHorizontalGrey(float x, float y, float length)
  {
    GUI.DrawTexture(new Rect(x, y, length, 1f), (Texture) BaseContent.GreyTex);
  }

  public static void DrawLineVerticalGrey(float x, float y, float length)
  {
    GUI.DrawTexture(new Rect(x, y, 1f, length), (Texture) BaseContent.GreyTex);
  }

  public static float HorizontalSlider_Arrow(
    Rect rect,
    float value,
    float min,
    float max,
    float roundTo = 0.0f,
    float handleScale = 20f,
    Texture2D railAtlas = null)
  {
    int num1 = Gen.HashCombine<float>(Gen.HashCombine<float>(Gen.HashCombine<float>(Gen.HashCombine<float>(UI.GUIToScreenPoint(new Vector2(((Rect) ref rect).x, ((Rect) ref rect).y)).GetHashCode(), max), min), ((Rect) ref rect).height), ((Rect) ref rect).width);
    float num2 = value;
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(Color.white);
    try
    {
      Rect rect1;
      // ISSUE: explicit constructor call
      ((Rect) ref rect1).\u002Ector(rect);
      ref Rect local1 = ref rect1;
      ((Rect) ref local1).xMin = ((Rect) ref local1).xMin + 6f;
      ref Rect local2 = ref rect1;
      ((Rect) ref local2).xMax = ((Rect) ref local2).xMax - 6f;
      Rect rect2;
      // ISSUE: explicit constructor call
      ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).x, ((Rect) ref rect1).y + 2f, ((Rect) ref rect1).width, 8f);
      if (Object.op_Implicit((Object) railAtlas))
      {
        GUI.color = UIElements.RangeControlTextColor;
        Widgets.DrawAtlas(rect2, railAtlas);
      }
      GUI.color = Color.white;
      GUI.DrawTexture(new Rect(Mathf.Clamp((float) ((double) ((Rect) ref rect1).x - 6.0 + (double) ((Rect) ref rect1).width * (double) Mathf.InverseLerp(min, max, num2)), ((Rect) ref rect1).xMin - 6f, ((Rect) ref rect1).xMax - 6f), ((Rect) ref rect2).center.y - 6f, handleScale, handleScale), (Texture) UIData.TargetLevelArrow);
      if (Event.current.type == null && Mouse.IsOver(rect) && UIElements.sliderDraggingID != num1)
      {
        UIElements.sliderDraggingID = num1;
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.DragSlider, (Map) null);
        Event.current.Use();
      }
      if (UIElements.sliderDraggingID == num1 && UnityGUIBugsFixer.MouseDrag(0))
      {
        num2 = Mathf.Clamp((float) (((double) Event.current.mousePosition.x - (double) ((Rect) ref rect1).x) / (double) ((Rect) ref rect1).width * ((double) max - (double) min)) + min, min, max);
        if (Event.current.type == 3)
          Event.current.Use();
      }
      if ((double) roundTo > 0.0)
        num2 = num2.RoundTo(roundTo);
      if (!Mathf.Approximately(value, num2))
        UIElements.CheckPlayDragSliderSound();
    }
    finally
    {
      textBlock.Dispose();
    }
    return num2;
  }

  private static void CheckPlayDragSliderSound()
  {
    if ((double) Time.realtimeSinceStartup <= (double) UIElements.lastDragSliderSoundTime + 0.075000002980232239)
      return;
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.DragSlider, (Map) null);
    UIElements.lastDragSliderSoundTime = Time.realtimeSinceStartup;
  }

  public static void LabelStyled(Rect rect, string label, GUIStyle style)
  {
    Rect rect1 = rect;
    float num1 = Prefs.UIScale / 2f;
    if ((double) Prefs.UIScale > 1.0)
    {
      double num2 = (double) Math.Abs(num1 - Mathf.Floor(num1));
    }
    GUI.Label(rect1, label, style);
  }

  public static void LabelOutlineStyled(Rect rect, string label, GUIStyle style, Color outerColor)
  {
    Rect rect1 = rect;
    float num1 = Prefs.UIScale / 2f;
    if ((double) Prefs.UIScale > 1.0)
    {
      double num2 = (double) Math.Abs(num1 - Mathf.Floor(num1));
    }
    Color textColor = style.normal.textColor;
    style.normal.textColor = outerColor;
    ref Rect local1 = ref rect1;
    ((Rect) ref local1).x = ((Rect) ref local1).x - 1f;
    GUI.Label(rect1, label, style);
    ref Rect local2 = ref rect1;
    ((Rect) ref local2).x = ((Rect) ref local2).x + 2f;
    GUI.Label(rect1, label, style);
    ref Rect local3 = ref rect1;
    ((Rect) ref local3).x = ((Rect) ref local3).x - 1f;
    ref Rect local4 = ref rect1;
    ((Rect) ref local4).y = ((Rect) ref local4).y - 1f;
    GUI.Label(rect1, label, style);
    ref Rect local5 = ref rect1;
    ((Rect) ref local5).y = ((Rect) ref local5).y + 2f;
    GUI.Label(rect1, label, style);
    ref Rect local6 = ref rect1;
    ((Rect) ref local6).y = ((Rect) ref local6).y - 1f;
    style.normal.textColor = textColor;
    GUI.Label(rect1, label, style);
  }

  public static void DrawTextureWithMaterialOnGUI(
    Rect rect,
    Texture texture,
    Material material,
    float angle,
    Rect texCoords = default (Rect))
  {
    Matrix4x4 matrix = GUI.matrix;
    try
    {
      angle = angle.ClampAngle();
      if (!Mathf.Approximately(angle, 0.0f))
        UI.RotateAroundPivot(angle, ((Rect) ref rect).center);
      GenUI.DrawTextureWithMaterial(rect, texture, material, texCoords);
    }
    finally
    {
      GUI.matrix = matrix;
    }
  }

  public static Rect VerticalFillableBar(Rect rect, float fillPercent, bool flip = false)
  {
    return UIElements.VerticalFillableBar(rect, fillPercent, UIData.FillableBarTexture, flip);
  }

  public static Rect VerticalFillableBar(
    Rect rect,
    float fillPercent,
    Texture2D fillTex,
    bool flip = false)
  {
    bool doBorder = (double) ((Rect) ref rect).height > 15.0 && (double) ((Rect) ref rect).width > 20.0;
    return UIElements.VerticalFillableBar(rect, fillPercent, fillTex, UIData.ClearBarTexture, doBorder, flip);
  }

  public static Rect VerticalFillableBar(
    Rect rect,
    float fillPercent,
    Texture2D fillTex,
    Texture2D bgTex,
    bool doBorder = false,
    bool flip = false)
  {
    if (Object.op_Inequality((Object) bgTex, (Object) null))
    {
      GUI.DrawTexture(rect, (Texture) bgTex);
      if (doBorder)
        rect = GenUI.ContractedBy(rect, 3f);
    }
    if (!flip)
    {
      ref Rect local1 = ref rect;
      ((Rect) ref local1).y = ((Rect) ref local1).y + ((Rect) ref rect).height;
      ref Rect local2 = ref rect;
      ((Rect) ref local2).height = ((Rect) ref local2).height * -1f;
    }
    Rect rect1 = rect;
    ref Rect local = ref rect;
    ((Rect) ref local).height = ((Rect) ref local).height * fillPercent;
    GUI.DrawTexture(rect, (Texture) fillTex);
    return rect1;
  }

  public static void FillableBar(
    Rect rect,
    float fillPercent,
    Texture2D fillTex,
    Texture2D bgTex,
    float innerContractedBy = 0.0f)
  {
    GUI.DrawTexture(rect, (Texture) bgTex);
    rect = GenUI.ContractedBy(rect, innerContractedBy);
    Rect rect1 = rect;
    ref Rect local = ref rect1;
    ((Rect) ref local).width = ((Rect) ref local).width * fillPercent;
    GUI.DrawTexture(rect1, (Texture) fillTex);
  }

  public static void FillableBarLabeled(
    Rect rect,
    float fillPercent,
    string label,
    Texture2D fillTex,
    Texture2D addedFillTex,
    Texture2D innerTex,
    Texture2D outlineTex,
    float? actualValue = null,
    float addedValue = 0.0f,
    float bgFillPercent = 0.0f)
  {
    if ((double) fillPercent < 0.0)
      fillPercent = 0.0f;
    if ((double) fillPercent > 1.0)
      fillPercent = 1f;
    if (!Object.op_Inequality((Object) fillTex, (Object) null) || !Object.op_Inequality((Object) addedFillTex, (Object) null))
      return;
    UIElements.FillableBarHollowed(rect, fillPercent, bgFillPercent, fillTex, addedFillTex, innerTex, outlineTex);
    Rect rect1 = rect;
    ref Rect local1 = ref rect1;
    ((Rect) ref local1).x = ((Rect) ref local1).x + 5f;
    GUIStyle style = new GUIStyle(Verse.Text.CurFontStyle);
    UIElements.LabelOutlineStyled(rect1, label, style, Color.black);
    if (!actualValue.HasValue)
      return;
    Rect rect2 = rect;
    ref Rect local2 = ref rect2;
    ((Rect) ref local2).width = ((Rect) ref local2).width / 2f;
    ((Rect) ref rect2).x = (float) ((double) ((Rect) ref rect1).x + (double) ((Rect) ref rect1).width / 2.0 - 6.0);
    style.alignment = (TextAnchor) 5;
    string label1 = string.Format("{1} {0}", (object) actualValue.ToString(), (double) addedValue != 0.0 ? (object) $"({((double) addedValue > 0.0 ? "+" : "")}{addedValue.ToString()})" : (object) "");
    UIElements.LabelOutlineStyled(rect2, label1, style, Color.black);
  }

  public static void FillableBarHollowed(
    Rect rect,
    float fillPercent,
    float bgFillPercent,
    Texture2D fillTex,
    Texture2D addedFillTex,
    Texture2D innerTex,
    Texture2D bgTex)
  {
    GUI.DrawTexture(rect, (Texture) bgTex);
    rect = GenUI.ContractedBy(rect, 2f);
    Rect rect1 = rect;
    ref Rect local1 = ref rect1;
    ((Rect) ref local1).width = ((Rect) ref local1).width - 2f;
    GUI.DrawTexture(rect1, (Texture) innerTex);
    Rect rect2 = rect;
    ref Rect local2 = ref rect2;
    ((Rect) ref local2).width = ((Rect) ref local2).width * fillPercent;
    GUI.DrawTexture(rect2, (Texture) fillTex);
    if ((double) bgFillPercent == 0.0)
      return;
    if ((double) bgFillPercent < 0.0)
    {
      Rect rect3 = rect;
      if ((double) fillPercent + (double) bgFillPercent < 0.0)
      {
        ref Rect local3 = ref rect3;
        ((Rect) ref local3).width = ((Rect) ref local3).width * fillPercent;
        ((Rect) ref rect3).x = ((Rect) ref rect2).x;
      }
      else
      {
        ref Rect local4 = ref rect3;
        ((Rect) ref local4).width = ((Rect) ref local4).width * bgFillPercent;
        ((Rect) ref rect3).x = ((Rect) ref rect2).x + ((Rect) ref rect2).width;
      }
      GUI.DrawTexture(rect3, (Texture) innerTex);
    }
    else
    {
      Rect rect4 = rect;
      ((Rect) ref rect4).x = ((Rect) ref rect2).x + ((Rect) ref rect2).width;
      ref Rect local5 = ref rect4;
      ((Rect) ref local5).width = ((Rect) ref local5).width * bgFillPercent;
      GUI.DrawTexture(rect4, (Texture) addedFillTex);
    }
  }

  public static void DrawBarThreshold(Rect barRect, float threshPct, float curLevel)
  {
    Color color = GUI.color;
    float num = (double) ((Rect) ref barRect).width > 60.0 ? 2f : 1f;
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector((float) ((double) ((Rect) ref barRect).x + (double) ((Rect) ref barRect).width * (double) threshPct - ((double) num - 1.0)), ((Rect) ref barRect).y + ((Rect) ref barRect).height / 2f, num, ((Rect) ref barRect).height / 2f);
    Texture2D texture2D;
    if ((double) threshPct < (double) curLevel)
    {
      texture2D = BaseContent.BlackTex;
      GUI.color = new Color(1f, 1f, 1f, 0.9f);
    }
    else
    {
      texture2D = BaseContent.GreyTex;
      GUI.color = new Color(1f, 1f, 1f, 0.5f);
    }
    GUI.DrawTexture(rect, (Texture) texture2D);
    GUI.color = color;
  }

  public static void LabelUnderlined(Rect rect, string label, Color labelColor, Color lineColor)
  {
    Color color = GUI.color;
    ref Rect local1 = ref rect;
    ((Rect) ref local1).y = ((Rect) ref local1).y + 3f;
    GUI.color = labelColor;
    Verse.Text.Anchor = (TextAnchor) 0;
    Widgets.Label(rect, label);
    ref Rect local2 = ref rect;
    ((Rect) ref local2).y = ((Rect) ref local2).y + 20f;
    GUI.color = lineColor;
    Widgets.DrawLineHorizontal(((Rect) ref rect).x - 1f, ((Rect) ref rect).y, ((Rect) ref rect).width - 1f);
    ref Rect local3 = ref rect;
    ((Rect) ref local3).y = ((Rect) ref local3).y + 2f;
    GUI.color = color;
  }

  public static void LabelUnderlined(
    Rect rect,
    string label,
    string label2,
    Color labelColor,
    Color label2Color,
    Color lineColor)
  {
    Color color = GUI.color;
    ref Rect local1 = ref rect;
    ((Rect) ref local1).y = ((Rect) ref local1).y + 3f;
    GUI.color = labelColor;
    Verse.Text.Anchor = (TextAnchor) 0;
    Widgets.Label(rect, label);
    GUI.color = label2Color;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(rect);
    ref Rect local2 = ref rect1;
    ((Rect) ref local2).x = ((Rect) ref local2).x + (Verse.Text.CalcSize(label).x + 5f);
    Widgets.Label(rect1, label2);
    ref Rect local3 = ref rect;
    ((Rect) ref local3).y = ((Rect) ref local3).y + 20f;
    GUI.color = lineColor;
    Widgets.DrawLineHorizontal(((Rect) ref rect).x - 1f, ((Rect) ref rect).y, ((Rect) ref rect).width - 1f);
    ref Rect local4 = ref rect;
    ((Rect) ref local4).y = ((Rect) ref local4).y + 2f;
    GUI.color = color;
  }

  public static bool InfoCardButton(Rect rect)
  {
    MouseoverSounds.DoRegion(rect);
    TooltipHandler.TipRegionByKey(rect, "DefInfoTip");
    bool flag = Widgets.ButtonImage(rect, TexButton.Info, GUI.color, true, (string) null);
    UIHighlighter.HighlightOpportunity(rect, "InfoCard");
    return flag;
  }

  public static void BeginScrollView(
    Rect outRect,
    ref Vector2 scrollPosition,
    Rect viewRect,
    bool showHorizontalScrollbar = true,
    bool showVerticalScrollbar = true)
  {
    if (Widgets.mouseOverScrollViewStack.Count > 0)
      Widgets.mouseOverScrollViewStack.Push(Widgets.mouseOverScrollViewStack.Peek() && ((Rect) ref outRect).Contains(Event.current.mousePosition));
    else
      Widgets.mouseOverScrollViewStack.Push(((Rect) ref outRect).Contains(Event.current.mousePosition));
    SteamDeck.HandleTouchScreenScrollViewScroll(outRect, ref scrollPosition);
    GUIStyle guiStyle1 = showHorizontalScrollbar ? GUI.skin.horizontalScrollbar : GUIStyle.none;
    GUIStyle guiStyle2 = showVerticalScrollbar ? GUI.skin.verticalScrollbar : GUIStyle.none;
    scrollPosition = GUI.BeginScrollView(outRect, scrollPosition, viewRect, guiStyle1, guiStyle2);
    UnityGUIBugsFixer.Notify_BeginScrollView();
  }

  public static void EndScrollView(bool handleScrollWheel = true)
  {
    Widgets.mouseOverScrollViewStack.Pop();
    GUI.EndScrollView(handleScrollWheel);
  }

  static UIElements()
  {
    ColorInt colorInt1 = new ColorInt(97, 108, 122);
    UIElements.WindowBgBorderColor = ((ColorInt) ref colorInt1).ToColor;
    ColorInt colorInt2 = new ColorInt(135, 135, 135);
    UIElements.MenuSectionBgBorderColor = ((ColorInt) ref colorInt2).ToColor;
    UIElements.lastDragSliderSoundTime = -1f;
  }
}
