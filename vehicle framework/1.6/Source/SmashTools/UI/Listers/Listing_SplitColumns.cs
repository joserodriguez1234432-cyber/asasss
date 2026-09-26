// Decompiled with JetBrains decompiler
// Type: SmashTools.Listing_SplitColumns
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

public class Listing_SplitColumns : Listing
{
  public const float GapHeight = 34f;
  public const float DefSelectionLineHeight = 21f;
  public const float ColumnSplitWidth = 0.05f;
  protected readonly GameFont font;
  protected int columns = 2;
  protected int curColumn;
  public bool shiftRectScrollbar;
  public float columnGap = 4f;
  private GameFont oldFont;

  public Listing_SplitColumns(GameFont font) => this.font = font;

  public Listing_SplitColumns() => this.font = (GameFont) 0;

  protected int CurrentColumn => this.curColumn % this.columns;

  public virtual void Begin(Rect rect)
  {
    if (this.shiftRectScrollbar)
    {
      ref Rect local = ref rect;
      ((Rect) ref local).width = ((Rect) ref local).width - 10f;
    }
    base.Begin(rect);
    this.oldFont = Text.Font;
    Text.Font = this.font;
    this.curColumn = 0;
  }

  public void Begin(Rect rect, int columns)
  {
    base.Begin(rect);
    this.columns = columns;
  }

  public void BeginScrollView(
    Rect rect,
    ref Vector2 scrollPosition,
    ref Rect viewRect,
    int columns)
  {
    Widgets.BeginScrollView(rect, ref scrollPosition, viewRect, true);
    ((Rect) ref rect).height = 100000f;
    this.Begin(rect, columns);
  }

  public virtual void End()
  {
    Text.Font = this.oldFont;
    base.End();
  }

  public void EndScrollView(ref Rect viewRect)
  {
    viewRect = new Rect(0.0f, 0.0f, ((Rect) ref this.listingRect).width, this.curY);
    Widgets.EndScrollView();
    base.End();
  }

  public virtual void NextRow(float gapHeight = 16f)
  {
    if (this.curColumn <= 0)
      return;
    this.curColumn = 0;
    this.curX = 0.0f;
    this.curY += gapHeight;
  }

  public Rect GetSplitRect(float height)
  {
    this.NewColumnIfNeeded(height);
    Rect splitRect;
    // ISSUE: explicit constructor call
    ((Rect) ref splitRect).\u002Ector(this.curX + 2f, this.curY, this.ColumnWidth / ((float) this.columns + (float) (this.columns * this.columns) * 0.05f) - this.columnGap, height);
    return splitRect;
  }

  public Rect GetCurrentRect(float height)
  {
    return new Rect(this.curX, this.curY, this.ColumnWidth / ((float) this.columns + (float) (this.columns * this.columns) * 0.05f) - this.columnGap, height);
  }

  public virtual void Shift(float gapHeight = 34f)
  {
    if (this.curColumn > 0)
    {
      if (this.CurrentColumn == 0)
      {
        this.curX = 0.0f;
        this.curY += gapHeight;
      }
      else
        this.curX = this.ColumnWidth / ((float) this.columns - (float) this.columns * 0.05f) * (float) this.CurrentColumn;
    }
    ++this.curColumn;
  }

  public void Header(string header, GameFont fontSize = 2, TextAnchor anchor = 3)
  {
    this.Shift();
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(fontSize);
    try
    {
      Text.Font = fontSize;
      UIElements.Header(this.GetSplitRect(Text.CalcHeight(header, this.ColumnWidth)), header, ListingExtension.BannerColor, fontSize, anchor);
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  public bool ClickableLabel(
    string label,
    string value,
    Color mouseOver,
    Color textColor,
    Color? clickColor = null,
    float? lineHeight = null)
  {
    this.Shift();
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 0);
    try
    {
      Rect splitRect = this.GetSplitRect((float) ((double) lineHeight ?? (double) Text.LineHeight));
      Rect rect1;
      // ISSUE: explicit constructor call
      ((Rect) ref rect1).\u002Ector(((Rect) ref splitRect).x, ((Rect) ref splitRect).y, ((Rect) ref splitRect).width / 2f, ((Rect) ref splitRect).height);
      Rect rect2;
      // ISSUE: explicit constructor call
      ((Rect) ref rect2).\u002Ector(((Rect) ref splitRect).x + ((Rect) ref rect1).width, ((Rect) ref splitRect).y, ((Rect) ref splitRect).width / 2f, ((Rect) ref splitRect).height);
      if (Mouse.IsOver(rect2))
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
      if (!label.NullOrEmpty<char>())
      {
        Text.Anchor = (TextAnchor) 3;
        Widgets.Label(rect1, label);
      }
      Text.Anchor = (TextAnchor) 5;
      Widgets.Label(rect2, value);
      if (!Widgets.ButtonInvisible(rect2, true))
        return false;
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
      return true;
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  public bool Button(string label, float height = 24f, string highlightTag = null)
  {
    this.Shift();
    Rect splitRect = this.GetSplitRect(height);
    bool flag = Widgets.ButtonText(splitRect, label, true, true, true, new TextAnchor?());
    if (highlightTag != null)
      UIHighlighter.HighlightOpportunity(splitRect, highlightTag);
    return flag;
  }

  public void CheckboxLabeled(
    string label,
    ref bool checkState,
    string tooltip,
    string disabledTooltip,
    bool locked,
    float? lineHeight = null)
  {
    this.Shift();
    Rect splitRect = this.GetSplitRect((float) ((double) lineHeight ?? (double) Text.LineHeight));
    bool disabled = !disabledTooltip.NullOrEmpty<char>();
    if (disabled)
      TooltipHandler.TipRegion(splitRect, TipSignal.op_Implicit(disabledTooltip));
    else if (!tooltip.NullOrEmpty<char>())
    {
      if (Mouse.IsOver(splitRect))
        Widgets.DrawHighlight(splitRect);
      TooltipHandler.TipRegion(splitRect, TipSignal.op_Implicit(tooltip));
    }
    if (locked)
      checkState = false;
    UIElements.CheckboxLabeled(splitRect, label, ref checkState, disabled);
  }

  public void IntegerBox(
    string label,
    ref int value,
    string tooltip,
    string disabledTooltip,
    int min = -2147483648 /*0x80000000*/,
    int max = 2147483647 /*0x7FFFFFFF*/,
    float? lineHeight = null,
    float labelProportion = 0.75f)
  {
    this.Shift();
    UIElements.NumericBox<int>(this.GetSplitRect((float) ((double) lineHeight ?? (double) Text.LineHeight)), ref value, label, tooltip, disabledTooltip, (float) min, (float) max, labelProportion);
  }

  public void FloatBox(
    string label,
    ref float value,
    string tooltip,
    string disabledTooltip,
    float min = -2.14748365E+09f,
    float max = 2.14748365E+09f,
    float? lineHeight = null,
    float labelProportion = 0.75f)
  {
    this.Shift();
    UIElements.NumericBox<float>(this.GetSplitRect((float) ((double) lineHeight ?? (double) Text.LineHeight)), ref value, label, tooltip, disabledTooltip, min, max, labelProportion);
  }

  public void SliderPercentLabeled(
    string label,
    ref float value,
    string tooltip,
    string disabledTooltip,
    string endSymbol,
    float min,
    float max,
    int decimalPlaces = 2,
    float endValue = -1f,
    string endValueDisplay = "",
    bool translate = false)
  {
    this.Shift();
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(Color.white);
    try
    {
      Rect splitRect = this.GetSplitRect(24f);
      string str = $"{Math.Round((double) value * 100.0, decimalPlaces)}" + endSymbol;
      if (!endValueDisplay.NullOrEmpty<char>() && (double) endValue > 0.0 && (double) value >= (double) endValue)
      {
        str = endValueDisplay;
        if (translate)
          str = TaggedString.op_Implicit(Translator.Translate(str));
      }
      if (!disabledTooltip.NullOrEmpty<char>())
      {
        GUIState.Disable();
        TooltipHandler.TipRegion(splitRect, TipSignal.op_Implicit(disabledTooltip));
      }
      else if (!tooltip.NullOrEmpty<char>())
      {
        if (Mouse.IsOver(splitRect))
          Widgets.DrawHighlight(splitRect);
        TooltipHandler.TipRegion(splitRect, TipSignal.op_Implicit(tooltip));
      }
      value = Widgets.HorizontalSlider(splitRect, value, min, max, false, (string) null, label, str, -1f);
      float num = value;
      if ((double) endValue > 0.0 && (double) num >= (double) max)
        ;
      GUIState.Enable();
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  public float SliderLabeled(
    string label,
    float value,
    string tooltip,
    string disabledTooltip,
    string endSymbol,
    float min,
    float max,
    int decimalPlaces = 2,
    float endValue = -1f,
    float increment = 0.0f,
    string endValueDisplay = "",
    bool translate = false)
  {
    this.SliderLabeled(label, ref value, tooltip, disabledTooltip, endSymbol, min, max, decimalPlaces, endValue, increment, endValueDisplay, translate);
    return value;
  }

  public void SliderLabeled(
    string label,
    ref float value,
    string tooltip,
    string disabledTooltip,
    string endSymbol,
    float min,
    float max,
    int decimalPlaces = 2,
    float endValue = -1f,
    float increment = 0.0f,
    string endValueDisplay = "",
    bool translate = false)
  {
    this.Shift();
    Rect splitRect = this.GetSplitRect(24f);
    Rect rect = splitRect;
    ref Rect local = ref splitRect;
    ((Rect) ref local).y = ((Rect) ref local).y + ((Rect) ref splitRect).height / 2f;
    string str = $"{Math.Round((double) value, decimalPlaces)}" + endSymbol;
    if (!endValueDisplay.NullOrEmpty<char>() && (double) value >= (double) max)
    {
      str = endValueDisplay;
      if (translate)
        str = TaggedString.op_Implicit(Translator.Translate(str));
    }
    if (!disabledTooltip.NullOrEmpty<char>())
    {
      GUIState.Disable();
      TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(disabledTooltip));
    }
    else if (!tooltip.NullOrEmpty<char>())
    {
      if (Mouse.IsOver(rect))
        Widgets.DrawHighlight(rect);
      TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tooltip));
    }
    value = Widgets.HorizontalSlider(splitRect, value, min, max, false, (string) null, label, str, -1f);
    float num = value;
    if ((double) increment > 0.0)
    {
      value = value.RoundTo(increment);
      num = num.RoundTo(increment);
    }
    if ((double) endValue > 0.0 && (double) num >= (double) max)
      ;
    GUIState.Enable();
  }

  public int SliderLabeled(
    string label,
    int value,
    string tooltip,
    string disabledTooltip,
    string endSymbol,
    int min,
    int max,
    int endValue = -1,
    string maxValueDisplay = "",
    string minValueDisplay = "",
    bool translate = false)
  {
    this.SliderLabeled(label, ref value, tooltip, disabledTooltip, endSymbol, min, max, endValue, maxValueDisplay, minValueDisplay, translate);
    return value;
  }

  public void SliderLabeled(
    string label,
    ref int value,
    string tooltip,
    string disabledTooltip,
    string endSymbol,
    int min,
    int max,
    int endValue = -1,
    string maxValueDisplay = "",
    string minValueDisplay = "",
    bool translate = false)
  {
    this.Shift();
    Rect splitRect = this.GetSplitRect(24f);
    Rect rect = splitRect;
    ref Rect local = ref splitRect;
    ((Rect) ref local).y = ((Rect) ref local).y + ((Rect) ref splitRect).height / 2f;
    string str = string.Format("{0}" + endSymbol, (object) value);
    if (!maxValueDisplay.NullOrEmpty<char>() && value == max)
    {
      str = maxValueDisplay;
      if (translate)
        str = TaggedString.op_Implicit(Translator.Translate(str));
    }
    if (!minValueDisplay.NullOrEmpty<char>() && value == min)
    {
      str = minValueDisplay;
      if (translate)
        str = TaggedString.op_Implicit(Translator.Translate(str));
    }
    if (!disabledTooltip.NullOrEmpty<char>())
    {
      GUIState.Disable();
      TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(disabledTooltip));
    }
    else if (!tooltip.NullOrEmpty<char>())
    {
      if (Mouse.IsOver(rect))
        Widgets.DrawHighlight(rect);
      TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tooltip));
    }
    value = (int) Widgets.HorizontalSlider(splitRect, (float) value, (float) min, (float) max, false, (string) null, label, str, -1f);
    if (value >= max && endValue > 0)
      ;
    GUIState.Enable();
  }

  public void EnumSliderLabeled(
    string label,
    ref int value,
    string tooltip,
    string disabledTooltip,
    Type enumType,
    bool translate = false)
  {
    this.Shift();
    int[] array = Enum.GetValues(enumType).Cast<int>().ToArray<int>();
    Enum.GetNames(enumType);
    int num1 = array[0];
    int num2 = ((IEnumerable<int>) array).Last<int>();
    Rect splitRect = this.GetSplitRect(24f);
    Rect rect = splitRect;
    ref Rect local = ref splitRect;
    ((Rect) ref local).y = ((Rect) ref local).y + ((Rect) ref splitRect).height / 2f;
    string str = Enum.GetName(enumType, (object) value);
    if (translate)
      str = TaggedString.op_Implicit(Translator.Translate(str));
    if (!disabledTooltip.NullOrEmpty<char>())
    {
      GUIState.Disable();
      TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(disabledTooltip));
    }
    else if (!tooltip.NullOrEmpty<char>())
    {
      if (Mouse.IsOver(rect))
        Widgets.DrawHighlight(rect);
      TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tooltip));
    }
    value = (int) Widgets.HorizontalSlider(splitRect, (float) value, (float) num1, (float) num2, false, (string) null, label, str, -1f);
    GUIState.Enable();
  }

  public void FloatRangeBox(
    string label,
    ref FloatRange value,
    string tooltip = null,
    float labelProportion = 0.5f,
    float subLabelProportions = 0.25f,
    float buffer = 0.0f)
  {
    value = this.FloatRangeBox(label, value, tooltip, labelProportion, subLabelProportions, buffer);
  }

  public FloatRange FloatRangeBox(
    string label,
    FloatRange value,
    string tooltip = null,
    float labelProportion = 0.5f,
    float subLabelProportions = 0.25f,
    float buffer = 0.0f)
  {
    this.Shift();
    value = UIElements.FloatRangeBox(this.GetSplitRect(24f), label, value, tooltip, labelProportion, subLabelProportions, buffer);
    return value;
  }

  public void Vector2Box(
    string label,
    ref Vector2 value,
    string tooltip = null,
    float labelProportion = 0.5f,
    float subLabelProportions = 0.15f,
    float buffer = 0.0f)
  {
    value = this.Vector2Box(label, value, tooltip, labelProportion, subLabelProportions, buffer);
  }

  public Vector2 Vector2Box(
    string label,
    Vector2 value,
    string tooltip = null,
    float labelProportion = 0.5f,
    float subLabelProportions = 0.15f,
    float buffer = 0.0f)
  {
    this.Shift();
    value = UIElements.Vector2Box(this.GetSplitRect(24f), label, value, tooltip, labelProportion, subLabelProportions, buffer);
    return value;
  }

  public void Vector3Box(
    string label,
    ref Vector3 value,
    string tooltip = null,
    float labelProportion = 0.5f,
    float subLabelProportions = 0.15f,
    float buffer = 0.0f)
  {
    value = this.Vector3Box(label, value, tooltip, labelProportion, subLabelProportions, buffer);
  }

  public Vector3 Vector3Box(
    string label,
    Vector3 value,
    string tooltip = null,
    float labelProportion = 0.5f,
    float subLabelProportions = 0.15f,
    float buffer = 0.0f)
  {
    this.Shift();
    value = UIElements.Vector3Box(this.GetSplitRect(24f), label, value, tooltip, labelProportion, subLabelProportions, buffer);
    return value;
  }

  public Rect Label(string label, float maxHeight = -1f, string tooltip = null)
  {
    float num = Text.CalcHeight(label, this.ColumnWidth);
    this.Shift(num);
    if ((double) maxHeight >= 0.0 && (double) num > (double) maxHeight)
      num = maxHeight;
    Rect splitRect = this.GetSplitRect(num);
    Widgets.Label(splitRect, label);
    if (tooltip != null)
      TooltipHandler.TipRegion(splitRect, TipSignal.op_Implicit(tooltip));
    return splitRect;
  }
}
