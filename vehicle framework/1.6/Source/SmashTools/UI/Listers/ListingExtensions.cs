// Decompiled with JetBrains decompiler
// Type: SmashTools.ListingExtension
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace SmashTools;

[StaticConstructorOnStartup]
public static class ListingExtension
{
  public static readonly Color HighlightColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
  public static readonly Color BannerColor = new Color(0.0f, 0.0f, 0.0f, 0.25f);
  public static readonly Color MenuSectionBGFillColor;
  public static readonly Color LightHighlightColor;
  public static readonly Texture2D ButtonBGAtlas;
  public static readonly Texture2D ButtonBGAtlasMouseover;
  public static readonly Texture2D ButtonBGAtlasClick;

  public static void CheckboxLabeledWithMessage(
    this Listing_Standard lister,
    string label,
    Func<bool, Message> messageGetter,
    ref bool checkOn,
    string tooltip = null)
  {
    bool flag = checkOn;
    lister.CheckboxLabeled(label, ref checkOn, tooltip, 0.0f, 1f);
    if (checkOn == flag)
      return;
    Message message = messageGetter(checkOn);
    if (message == null)
      return;
    Messages.Message(message, false);
  }

  public static bool ReverseRadioButton(
    this Listing_Standard lister,
    string label,
    bool enabled,
    string tooltip = null,
    float? tooltipDelay = null)
  {
    float lineHeight = Text.LineHeight;
    Rect rect = ((Listing) lister).GetRect(lineHeight, 1f);
    if (lister.BoundingRectCached.HasValue && !((Rect) ref rect).Overlaps(lister.BoundingRectCached.Value))
      return false;
    if (!tooltip.NullOrEmpty<char>())
    {
      if (Mouse.IsOver(rect))
        Widgets.DrawHighlight(rect);
      TipSignal tipSignal = tooltipDelay.HasValue ? new TipSignal(tooltip, tooltipDelay.Value) : new TipSignal(tooltip);
      TooltipHandler.TipRegion(rect, tipSignal);
    }
    bool flag = UIElements.ReverseRadioButton(rect, label, enabled);
    ((Listing) lister).Gap(((Listing) lister).verticalSpacing);
    return flag;
  }

  public static void IntegerBox(
    this Listing lister,
    string text,
    string tooltip,
    ref int value,
    float labelLength,
    float padding,
    int min = -2147483648 /*0x80000000*/,
    int max = 2147483647 /*0x7FFFFFFF*/)
  {
    Rect rect1 = lister.GetRect(Text.LineHeight, 1f);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).x, ((Rect) ref rect1).y, labelLength, ((Rect) ref rect1).height);
    Rect rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector(((Rect) ref rect1).x + labelLength + padding, ((Rect) ref rect1).y, ((Rect) ref rect1).width - labelLength - padding, ((Rect) ref rect1).height);
    Widgets.Label(rect2, text);
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((TextAnchor) (int) (Text.Anchor = (TextAnchor) 3));
    try
    {
      string str = value.ToString();
      Widgets.TextFieldNumeric<int>(rect3, ref value, ref str, (float) min, (float) max);
      if (tooltip.NullOrEmpty<char>())
        return;
      if (Mouse.IsOver(rect1))
        Widgets.DrawHighlight(rect1);
      TooltipHandler.TipRegion(rect1, TipSignal.op_Implicit(tooltip));
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  public static void Numericbox(
    this Listing lister,
    string text,
    string tooltip,
    ref float value,
    float labelLength,
    float padding,
    float min = -1E+09f,
    float max = 1E+09f)
  {
    lister.Gap(12f);
    Rect rect1 = lister.GetRect(Text.LineHeight, 1f);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).x, ((Rect) ref rect1).y, labelLength, ((Rect) ref rect1).height);
    Rect rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector(((Rect) ref rect1).x + labelLength + padding, ((Rect) ref rect1).y, ((Rect) ref rect1).width - labelLength - padding, ((Rect) ref rect1).height);
    Widgets.Label(rect2, text);
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((TextAnchor) (int) (Text.Anchor = (TextAnchor) 3));
    try
    {
      string str = value.ToString();
      Widgets.TextFieldNumeric<float>(rect3, ref value, ref str, min, max);
      if (tooltip.NullOrEmpty<char>())
        return;
      if (Mouse.IsOver(rect1))
        Widgets.DrawHighlight(rect1);
      TooltipHandler.TipRegion(rect1, TipSignal.op_Implicit(tooltip));
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  public static void EnumSliderLabeled<T>(
    this Listing lister,
    string label,
    ref T value,
    string tooltip,
    string disabledTooltip,
    Func<T, string> valueNameGetter = null)
    where T : Enum
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(Color.white);
    try
    {
      int[] array = Enum.GetValues(typeof (T)).Cast<int>().ToArray<int>();
      int num1 = array[0];
      int num2 = ((IEnumerable<int>) array).Last<int>();
      Rect rect1 = lister.GetRect(24f, 1f);
      Rect rect2 = rect1;
      ref Rect local = ref rect1;
      ((Rect) ref local).y = ((Rect) ref local).y + ((Rect) ref rect1).height / 2f;
      string str = (valueNameGetter != null ? valueNameGetter(value) : (string) null) ?? Enum.GetName(typeof (T), (object) value);
      if (!disabledTooltip.NullOrEmpty<char>())
      {
        GUI.enabled = false;
        TooltipHandler.TipRegion(rect2, TipSignal.op_Implicit(disabledTooltip));
      }
      else if (!tooltip.NullOrEmpty<char>())
      {
        if (Mouse.IsOver(rect2))
          Widgets.DrawHighlight(rect2);
        TooltipHandler.TipRegion(rect2, TipSignal.op_Implicit(tooltip));
      }
      GUI.enabled = true;
      int num3 = (int) Convert.ChangeType((object) value, typeof (int));
      int num4 = (int) Widgets.HorizontalSlider(rect1, (float) num3, (float) num1, (float) num2, false, (string) null, label, str, -1f);
      value = (T) Enum.ToObject(typeof (T), num4);
    }
    catch (Exception ex)
    {
      Log.Error($"Exception thrown for slider of enum type = {typeof (T)}.\nException={ex}");
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  public static float SliderLabeled(
    this Listing lister,
    string label,
    string tooltip,
    string endSymbol,
    float value,
    float min,
    float max,
    float multiplier = 1f,
    int decimalPlaces = 2,
    float endValue = -1f,
    string maxValueDisplay = "")
  {
    ListingExtension.SliderLabeled(lister, label, tooltip, endSymbol, ref value, min, max, multiplier, decimalPlaces, endValue, maxValueDisplay);
    return value;
  }

  public static void SliderLabeled(
    this Listing lister,
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
    lister.SliderLabeled(label, tooltip, string.Empty, endSymbol, ref value, min, max, multiplier, decimalPlaces, endValue, maxValueDisplay);
  }

  public static void SliderLabeled(
    this Listing lister,
    string label,
    string tooltip,
    string startSymbol,
    string endSymbol,
    ref float value,
    float min,
    float max,
    float multiplier = 1f,
    int decimalPlaces = 2,
    float endValue = -1f,
    string maxValueDisplay = "")
  {
    lister.Gap(12f);
    Rect rect1 = lister.GetRect(24f, 1f);
    Rect rect2 = rect1;
    ref Rect local = ref rect1;
    ((Rect) ref local).y = ((Rect) ref local).y + ((Rect) ref rect1).height / 2f;
    string str = $"{startSymbol}{Math.Round((double) value * (double) multiplier, decimalPlaces)}{endSymbol}";
    if (!maxValueDisplay.NullOrEmpty<char>() && (double) endValue > 0.0 && (double) value >= (double) endValue)
      str = maxValueDisplay;
    if (!tooltip.NullOrEmpty<char>())
    {
      if (Mouse.IsOver(rect2))
        Widgets.DrawHighlight(rect2);
      TooltipHandler.TipRegion(rect2, TipSignal.op_Implicit(tooltip));
    }
    value = Widgets.HorizontalSlider(rect1, value, min, max, false, (string) null, label, str, -1f);
    if ((double) endValue <= 0.0 || (double) value < (double) max)
      return;
    value = endValue;
  }

  public static void SliderLabeled(
    this Listing lister,
    string label,
    string tooltip,
    string endSymbol,
    ref int value,
    int min,
    int max,
    int roundTo = 1,
    string maxValueDisplay = "",
    string minValueDisplay = "")
  {
    lister.Gap(12f);
    Rect rect1 = lister.GetRect(24f, 1f);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(rect1);
    ref Rect local = ref rect1;
    ((Rect) ref local).y = ((Rect) ref local).y + ((Rect) ref rect1).height / 2f;
    string str = string.Format("{0}" + endSymbol, (object) value);
    if (!maxValueDisplay.NullOrEmpty<char>() && value == max)
      str = maxValueDisplay;
    if (!minValueDisplay.NullOrEmpty<char>() && value == min)
      str = minValueDisplay;
    if (!tooltip.NullOrEmpty<char>())
    {
      if (Mouse.IsOver(rect2))
        Widgets.DrawHighlight(rect2);
      TooltipHandler.TipRegion(rect2, TipSignal.op_Implicit(tooltip));
    }
    value = (int) Widgets.HorizontalSlider(rect1, (float) value, (float) min, (float) max, false, (string) null, label, str, -1f).RoundTo((float) roundTo);
  }

  public static void SliderPercentLabeled(
    this Listing listing,
    string label,
    string tooltip,
    string endSymbol,
    ref float value,
    float min,
    float max,
    int decimalPlaces = 2,
    float endValue = -1f,
    string endValueDisplay = "")
  {
    Rect rect1 = listing.GetRect(24f, 1f);
    Rect rect2 = rect1;
    ref Rect local = ref rect1;
    ((Rect) ref local).y = ((Rect) ref local).y + ((Rect) ref rect1).height / 2f;
    string str = $"{Math.Round((double) value * 100.0, decimalPlaces)}" + endSymbol;
    if (!endValueDisplay.NullOrEmpty<char>() && (double) endValue > 0.0 && (double) value >= (double) endValue)
      str = endValueDisplay;
    bool flag = Mouse.IsOver(rect2);
    if (!tooltip.NullOrEmpty<char>())
    {
      if (flag)
        Widgets.DrawHighlight(rect2);
      TooltipHandler.TipRegion(rect2, TipSignal.op_Implicit(tooltip));
    }
    value = Widgets.HorizontalSlider(rect1, value, min, max, false, (string) null, label, str, -1f);
    if ((double) endValue <= 0.0 || (double) value < (double) max)
      return;
    value = endValue;
  }

  public static void Header(
    this Listing lister,
    string header,
    Color highlight,
    GameFont fontSize = 2,
    TextAnchor anchor = 3,
    float rowGap = 22f)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(fontSize);
    try
    {
      if (lister is Listing_SplitColumns listingSplitColumns)
        listingSplitColumns.NextRow(rowGap);
      UIElements.Header(lister.GetRect(Text.CalcHeight(header, lister.ColumnWidth), 1f), header, highlight, fontSize, anchor);
      listingSplitColumns?.Gap(2f);
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  public static void Vector2Box(
    this Listing lister,
    string label,
    ref Vector2 value,
    string tooltip = null,
    float labelProportion = 0.5f,
    float subLabelProportions = 0.15f)
  {
    value = lister.Vector2Box(label, value, tooltip, labelProportion, subLabelProportions);
  }

  public static Vector2 Vector2Box(
    this Listing lister,
    string label,
    Vector2 value,
    string tooltip = null,
    float labelProportion = 0.5f,
    float subLabelProportions = 0.15f)
  {
    UIElements.Vector2Box(lister.GetRect(24f, 1f), label, value, tooltip, labelProportion, subLabelProportions);
    return value;
  }

  public static void Vector3Box(
    this Listing lister,
    string label,
    ref Vector3 value,
    string tooltip = null,
    float labelProportion = 0.5f,
    float subLabelProportions = 0.15f)
  {
    value = lister.Vector3Box(label, value, tooltip, labelProportion, subLabelProportions);
  }

  public static Vector3 Vector3Box(
    this Listing lister,
    string label,
    Vector3 value,
    string tooltip = null,
    float labelProportion = 0.5f,
    float subLabelProportions = 0.15f)
  {
    UIElements.Vector3Box(lister.GetRect(24f, 1f), label, value, tooltip, labelProportion, subLabelProportions);
    return value;
  }

  public static bool ListItemSelectable(
    this Listing lister,
    string header,
    Color hoverColor,
    bool selected = false,
    bool active = true,
    string disabledTooltip = null)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(Color.white);
    try
    {
      Rect rect = lister.GetRect(20f, 1f);
      if (selected)
        Widgets.DrawBoxSolid(rect, ListingExtension.HighlightColor);
      GUI.color = Color.white;
      if (!active)
        GUI.color = Color.grey;
      else if (Mouse.IsOver(rect))
        GUI.color = hoverColor;
      if (!disabledTooltip.NullOrEmpty<char>())
        TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(disabledTooltip));
      Text.Anchor = (TextAnchor) 3;
      Widgets.Label(rect, header);
      if (!active || !Widgets.ButtonInvisible(rect, true))
        return false;
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
      return true;
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  public static bool ClickableLabel(this Listing lister, string label, bool doMouseoverSound = false)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((TextAnchor) 3, Color.white);
    try
    {
      Rect rect = lister.GetRect(Text.LineHeight, 1f);
      if (Mouse.IsOver(rect))
        Widgets.DrawHighlight(rect);
      Widgets.Label(rect, label);
      lister.Gap(lister.verticalSpacing);
      return Widgets.ButtonInvisible(rect, doMouseoverSound);
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  public static bool CheckboxLabeledReturned(
    this Listing_Standard lister,
    string label,
    ref bool checkOn,
    string tooltip = null)
  {
    Rect rect = ((Listing) lister).GetRect(Text.LineHeight, 1f);
    if (!tooltip.NullOrEmpty<char>())
    {
      if (Mouse.IsOver(rect))
        Widgets.DrawHighlight(rect);
      TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tooltip));
    }
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((TextAnchor) (int) (Text.Anchor = (TextAnchor) 3));
    try
    {
      Widgets.Label(rect, label);
      bool flag = false;
      if (Widgets.ButtonInvisible(rect, true))
      {
        checkOn = !checkOn;
        flag = true;
        if (checkOn)
          SoundStarter.PlayOneShotOnCamera(SoundDefOf.Checkbox_TurnedOn, (Map) null);
        else
          SoundStarter.PlayOneShotOnCamera(SoundDefOf.Checkbox_TurnedOff, (Map) null);
      }
      Texture2D texture2D = checkOn ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex;
      GUI.DrawTexture(new Rect((float) ((double) ((Rect) ref rect).x + (double) ((Rect) ref rect).width - 24.0), ((Rect) ref rect).y, 24f, 24f), (Texture) texture2D);
      ((Listing) lister).Gap(((Listing) lister).verticalSpacing);
      return flag;
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  static ListingExtension()
  {
    ColorInt colorInt = new ColorInt(42, 43, 44);
    ListingExtension.MenuSectionBGFillColor = ((ColorInt) ref colorInt).ToColor;
    ListingExtension.LightHighlightColor = new Color(1f, 1f, 1f, 0.04f);
    ListingExtension.ButtonBGAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBG", true);
    ListingExtension.ButtonBGAtlasMouseover = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBGMouseover", true);
    ListingExtension.ButtonBGAtlasClick = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBGClick", true);
  }
}
