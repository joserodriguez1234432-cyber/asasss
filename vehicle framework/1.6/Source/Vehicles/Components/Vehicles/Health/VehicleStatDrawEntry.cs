// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleStatDrawEntry
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class VehicleStatDrawEntry
{
  private StatCategoryDef category;
  public VehicleStatDef stat;
  public string categoryLabel;
  public int categoryDisplayOrder;
  private int displayOrderWithinCategory;
  private float value;
  public bool forceUnfinalizedMode;
  private IEnumerable<Dialog_InfoCard.Hyperlink> hyperlinks;
  private string label;
  private string valueString;
  private string explanationText;
  private ToStringNumberSense numberSense;
  private string overrideReportText;
  private string overrideReportTitle;

  public VehicleStatDrawEntry(StatCategoryDef category, VehicleStatDef stat, float value)
  {
    this.category = category;
    this.stat = stat;
    this.label = (string) null;
    this.value = value;
    this.valueString = (string) null;
    this.displayOrderWithinCategory = stat.displayPriorityInCategory;
  }

  public VehicleStatDrawEntry(
    StatCategoryDef category,
    string label,
    string valueString,
    string reportText,
    int displayPriorityWithinCategory,
    string overrideReportTitle = null,
    IEnumerable<Dialog_InfoCard.Hyperlink> hyperlinks = null,
    bool forceUnfinalizedMode = false)
  {
    this.category = category;
    this.stat = (VehicleStatDef) null;
    this.label = label;
    this.value = 0.0f;
    this.valueString = valueString;
    this.displayOrderWithinCategory = displayPriorityWithinCategory;
    this.numberSense = (ToStringNumberSense) 1;
    this.overrideReportText = reportText;
    this.overrideReportTitle = overrideReportTitle;
    this.hyperlinks = hyperlinks;
    this.forceUnfinalizedMode = forceUnfinalizedMode;
  }

  public VehicleStatDrawEntry(
    string categoryLabel,
    int categoryDisplayOrder,
    string label,
    string valueString,
    string reportText,
    int displayPriorityWithinCategory,
    string overrideReportTitle = null,
    IEnumerable<Dialog_InfoCard.Hyperlink> hyperlinks = null,
    bool forceUnfinalizedMode = false)
    : this((StatCategoryDef) null, label, valueString, reportText, displayPriorityWithinCategory, overrideReportTitle, hyperlinks, forceUnfinalizedMode)
  {
    this.categoryLabel = categoryLabel;
    this.categoryDisplayOrder = categoryDisplayOrder;
  }

  public string CategoryLabel
  {
    get
    {
      StatCategoryDef category = this.category;
      return TaggedString.op_Implicit(category != null ? ((Def) category).LabelCap : TaggedString.op_Implicit(this.categoryLabel));
    }
  }

  public int CategoryDisplayOrder
  {
    get
    {
      StatCategoryDef category = this.category;
      return category == null ? this.categoryDisplayOrder : category.displayOrder;
    }
  }

  public bool ShouldDisplay
  {
    get => this.stat == null || !Mathf.Approximately(this.value, this.stat.hideAtValue);
  }

  public int DisplayPriorityWithinCategory => this.displayOrderWithinCategory;

  public string LabelCap
  {
    get
    {
      return this.label != null ? GenText.CapitalizeFirst(this.label) : TaggedString.op_Implicit(this.stat.LabelCap);
    }
  }

  public string ValueString
  {
    get
    {
      if (this.numberSense == 2)
        return GenText.ToStringByStyle(this.value, (ToStringStyle) 8, (ToStringNumberSense) 1);
      return this.valueString == null ? this.stat.Worker.GetStatDrawEntryLabel(this.stat, this.value, this.numberSense, !this.forceUnfinalizedMode) : this.valueString;
    }
  }

  public IEnumerable<Dialog_InfoCard.Hyperlink> GetHyperlinks(VehiclePawn vehicle)
  {
    if (!this.hyperlinks.NullOrEmpty<Dialog_InfoCard.Hyperlink>())
    {
      foreach (Dialog_InfoCard.Hyperlink hyperlink in this.hyperlinks)
        yield return hyperlink;
    }
    if (this.stat != null && vehicle != null)
    {
      foreach (Dialog_InfoCard.Hyperlink infoCardHyperlink in this.stat.Worker.GetInfoCardHyperlinks(vehicle))
        yield return infoCardHyperlink;
    }
  }

  public string GetExplanationText(VehicleDef vehicleDef, VehiclePawn forVehicle = null)
  {
    if (this.explanationText == null)
      this.WriteExplanationTextInt();
    return this.stat != null ? this.explanationText + Environment.NewLine + Environment.NewLine + this.stat.Worker.GetExplanationFull(vehicleDef, this.numberSense, this.value, forVehicle) : this.explanationText;
  }

  private void WriteExplanationTextInt()
  {
    StringBuilder stringBuilder = new StringBuilder();
    if (!GenText.NullOrEmpty(this.overrideReportTitle))
      stringBuilder.AppendLine(this.overrideReportTitle);
    if (!GenText.NullOrEmpty(this.overrideReportText))
      stringBuilder.AppendLine(this.overrideReportText);
    else if (this.stat != null)
      stringBuilder.AppendLine(this.stat.description);
    stringBuilder.AppendLine();
    this.explanationText = GenText.TrimEndNewlines(stringBuilder.ToString());
  }

  public VehicleStatDrawEntry SetReportText(string reportText)
  {
    this.overrideReportText = reportText;
    return this;
  }

  public float Draw(
    float x,
    float y,
    float width,
    bool selected,
    bool highlightLabel,
    bool lowlightLabel,
    Action clickedCallback,
    Action mousedOverCallback,
    Vector2 scrollPosition,
    Rect scrollOutRect)
  {
    float num = width * 0.45f;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(x, y, width, Verse.Text.CalcHeight(this.ValueString, num));
    if ((double) y - (double) scrollPosition.y + (double) ((Rect) ref rect1).height >= 0.0 && (double) y - (double) scrollPosition.y <= (double) ((Rect) ref scrollOutRect).height)
    {
      GUI.color = Color.white;
      if (selected)
        Widgets.DrawHighlightSelected(rect1);
      else if (Mouse.IsOver(rect1))
        Widgets.DrawHighlight(rect1);
      if (highlightLabel)
        Widgets.DrawTextHighlight(rect1, 4f, new Color?());
      if (lowlightLabel)
        GUI.color = Color.grey;
      Rect rect2 = rect1;
      ref Rect local = ref rect2;
      ((Rect) ref local).width = ((Rect) ref local).width - num;
      Widgets.Label(rect2, this.LabelCap);
      Rect rect3 = rect1;
      ((Rect) ref rect3).xMin = ((Rect) ref rect2).xMax;
      Widgets.Label(rect3, this.ValueString);
      GUI.color = Color.white;
      if (this.stat != null && Mouse.IsOver(rect1))
        TooltipHandler.TipRegion(rect1, new TipSignal((Func<string>) (() => TaggedString.op_Implicit(TaggedString.op_Addition(TaggedString.op_Addition(this.stat.LabelCap, ": "), this.stat.description))), ((object) this.stat).GetHashCode()));
      if (Widgets.ButtonInvisible(rect1, true))
        clickedCallback();
      if (Mouse.IsOver(rect1))
        mousedOverCallback();
    }
    return ((Rect) ref rect1).height;
  }

  public bool Matching(VehicleStatDrawEntry entry)
  {
    if (entry == null)
      return false;
    if (this == entry)
      return true;
    return this.stat == entry.stat && this.label == entry.label;
  }

  public override string ToString() => $"({this.LabelCap}: {this.ValueString})";
}
