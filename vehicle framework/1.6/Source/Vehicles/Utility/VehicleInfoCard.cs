// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleInfoCard
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

public static class VehicleInfoCard
{
  private static VehiclePawn vehicle;
  private static VehicleDef vehicleDef;
  private static Dialog_InfoCard infoCard;
  private static float listHeight;
  private static float rightPanelHeight;
  private static Vector2 scrollPosition = Vector2.zero;
  private static Vector2 scrollPositionRightPanel = Vector2.zero;
  private static ScrollPositioner scrollPositioner = new ScrollPositioner();
  private static VehicleStatDrawEntry selectedEntry;
  private static VehicleStatDrawEntry mousedOverEntry;
  private static List<VehicleStatDrawEntry> cachedDrawEntries = new List<VehicleStatDrawEntry>();
  internal static List<StatDef> displayedStatDefs = new List<StatDef>();
  private static FieldInfo tabFieldInfo = AccessTools.Field(typeof (Dialog_InfoCard), "tab");

  public static void RegisterStatDef(StatDef statDef)
  {
    VehicleInfoCard.displayedStatDefs.Add(statDef);
  }

  public static void Init(VehiclePawn vehicle, Dialog_InfoCard infoCard)
  {
    VehicleInfoCard.vehicle = vehicle;
    VehicleInfoCard.vehicleDef = vehicle.VehicleDef;
    VehicleInfoCard.infoCard = infoCard;
    VehicleInfoCard.Reset();
  }

  public static void Init(VehicleDef vehicleDef, Dialog_InfoCard infoCard)
  {
    VehicleInfoCard.vehicleDef = vehicleDef;
    VehicleInfoCard.infoCard = infoCard;
    VehicleInfoCard.Reset();
  }

  public static void Reset()
  {
    ((Window) VehicleInfoCard.infoCard).CommonSearchWidget.Reset();
    VehicleInfoCard.tabFieldInfo.SetValue((object) VehicleInfoCard.infoCard, (object) (Dialog_InfoCard.InfoCardTab) 0);
    VehicleInfoCard.scrollPosition = Vector2.zero;
    VehicleInfoCard.cachedDrawEntries.Clear();
    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.InfoCard, (KnowledgeAmount) 6);
  }

  private static bool Matching(VehicleStatDrawEntry drawEntry)
  {
    return ((Window) VehicleInfoCard.infoCard).CommonSearchWidget.filter.Matches(drawEntry.LabelCap);
  }

  public static void Clear()
  {
    VehicleInfoCard.vehicle = (VehiclePawn) null;
    VehicleInfoCard.vehicleDef = (VehicleDef) null;
    VehicleInfoCard.infoCard = (Dialog_InfoCard) null;
    VehicleInfoCard.mousedOverEntry = (VehicleStatDrawEntry) null;
    VehicleInfoCard.selectedEntry = (VehicleStatDrawEntry) null;
  }

  public static void DrawFor(
    Rect rect,
    VehicleDef vehicleDef,
    Dialog_InfoCard infoCard,
    Dialog_InfoCard.InfoCardTab tab)
  {
    if (VehicleInfoCard.vehicleDef != vehicleDef)
    {
      VehicleInfoCard.Clear();
      VehicleInfoCard.Init(vehicleDef, infoCard);
    }
    VehicleInfoCard.Draw(GenUI.ContractedBy(rect, 18f), tab);
  }

  public static void DrawFor(
    Rect rect,
    VehiclePawn vehicle,
    Dialog_InfoCard infoCard,
    Dialog_InfoCard.InfoCardTab tab)
  {
    if (VehicleInfoCard.vehicle != vehicle)
    {
      VehicleInfoCard.Clear();
      VehicleInfoCard.Init(vehicle, infoCard);
    }
    VehicleInfoCard.Draw(GenUI.ContractedBy(rect, 18f), tab);
  }

  public static void Draw(Rect rect, Dialog_InfoCard.InfoCardTab tab)
  {
    if (VehicleInfoCard.vehicle == null && VehicleInfoCard.vehicleDef == null)
      return;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(rect);
    ((Rect) ref rect1).height = 34f;
    Text.Font = (GameFont) 2;
    string str = VehicleInfoCard.vehicle != null ? ((Thing) VehicleInfoCard.vehicle).LabelCapNoCount : ((Def) VehicleInfoCard.vehicleDef).LabelCap.ToString();
    Widgets.Label(rect1, str);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(rect);
    ((Rect) ref rect2).yMin = ((Rect) ref rect1).yMax + 45f;
    ((Rect) ref rect2).yMax = ((Rect) ref rect).yMax - 20f;
    Rect rect3 = rect2;
    List<TabRecord> tabRecordList = new List<TabRecord>()
    {
      new TabRecord(TaggedString.op_Implicit(Translator.Translate("TabStats")), (Action) (() => VehicleInfoCard.tabFieldInfo.SetValue((object) VehicleInfoCard.infoCard, (object) (Dialog_InfoCard.InfoCardTab) 0)), tab == 0),
      new TabRecord(TaggedString.op_Implicit(Translator.Translate("TabHealth")), (Action) (() => VehicleInfoCard.tabFieldInfo.SetValue((object) VehicleInfoCard.infoCard, (object) (Dialog_InfoCard.InfoCardTab) 2)), tab == 2),
      new TabRecord(TaggedString.op_Implicit(Translator.Translate("TabRecords")), (Action) (() => VehicleInfoCard.tabFieldInfo.SetValue((object) VehicleInfoCard.infoCard, (object) (Dialog_InfoCard.InfoCardTab) 3)), tab == 3)
    };
    VehicleInfoCard.FillCard(GenUI.ContractedBy(rect3, 18f), tab);
  }

  private static void FillCard(Rect rect, Dialog_InfoCard.InfoCardTab tab)
  {
    switch ((int) tab)
    {
      case 0:
        VehicleInfoCard.DrawStatsReport(rect);
        break;
      case 2:
        VehicleInfoCard.DrawHealthScreen();
        break;
    }
  }

  private static void DrawHealthScreen()
  {
  }

  public static bool StatListContains(
    this List<VehicleStatModifier> modList,
    VehicleStatDef statDef)
  {
    if (!GenList.NullOrEmpty<VehicleStatModifier>((IList<VehicleStatModifier>) modList))
    {
      for (int index = 0; index < modList.Count; ++index)
      {
        if (modList[index].statDef == statDef)
          return true;
      }
    }
    return false;
  }

  private static VehicleStatDrawEntry DescriptionEntry()
  {
    string reportText = VehicleInfoCard.vehicle != null ? ((Thing) VehicleInfoCard.vehicle).DescriptionFlavor : ((Def) VehicleInfoCard.vehicleDef).description;
    return new VehicleStatDrawEntry(VehicleStatCategoryDefOf.VehicleBasicsImportant, TaggedString.op_Implicit(Translator.Translate("Description")), string.Empty, reportText, 99999, hyperlinks: Dialog_InfoCard.DefsToHyperlinks((IEnumerable<DefHyperlink>) ((Def) VehicleInfoCard.vehicleDef).descriptionHyperlinks));
  }

  private static IEnumerable<VehicleStatDrawEntry> StatsToDraw()
  {
    yield return VehicleInfoCard.DescriptionEntry();
    foreach (VehicleStatDef vehicleStatDef in DefDatabase<VehicleStatDef>.AllDefsListForReading.Where<VehicleStatDef>((Func<VehicleStatDef, bool>) (statDef => statDef.Worker.ShouldShowFor(VehicleInfoCard.vehicleDef))))
    {
      float num = VehicleInfoCard.vehicle == null ? VehicleInfoCard.vehicleDef.GetStatValueAbstract(vehicleStatDef) : VehicleInfoCard.vehicle.GetStatValue(vehicleStatDef);
      yield return new VehicleStatDrawEntry(vehicleStatDef.category, vehicleStatDef, num);
    }
  }

  private static void FinalizeCachedDrawEntries(IEnumerable<VehicleStatDrawEntry> stats)
  {
    VehicleInfoCard.cachedDrawEntries = stats.OrderBy<VehicleStatDrawEntry, int>((Func<VehicleStatDrawEntry, int>) (drawEntry => drawEntry.CategoryDisplayOrder)).ThenByDescending<VehicleStatDrawEntry, int>((Func<VehicleStatDrawEntry, int>) (drawEntry => drawEntry.DisplayPriorityWithinCategory)).ThenBy<VehicleStatDrawEntry, string>((Func<VehicleStatDrawEntry, string>) (drawEntry => drawEntry.LabelCap)).ToList<VehicleStatDrawEntry>();
    ((Window) VehicleInfoCard.infoCard).CommonSearchWidget.noResultsMatched = !GenCollection.Any<VehicleStatDrawEntry>(VehicleInfoCard.cachedDrawEntries);
    if (VehicleInfoCard.selectedEntry != null)
      VehicleInfoCard.selectedEntry = GenCollection.FirstOrDefault<VehicleStatDrawEntry>(VehicleInfoCard.cachedDrawEntries, (Predicate<VehicleStatDrawEntry>) (drawEntry => drawEntry.Matching(VehicleInfoCard.selectedEntry)));
    if (!((Window) VehicleInfoCard.infoCard).CommonSearchWidget.filter.Active)
      return;
    foreach (VehicleStatDrawEntry cachedDrawEntry in VehicleInfoCard.cachedDrawEntries)
    {
      if (VehicleInfoCard.Matching(cachedDrawEntry))
      {
        VehicleInfoCard.selectedEntry = cachedDrawEntry;
        VehicleInfoCard.scrollPositioner.Arm(true);
        break;
      }
    }
  }

  private static void DrawStatsReport(Rect rect)
  {
    VehicleInfoCard.TryRecacheEntries();
    VehicleInfoCard.DrawStatsWorker(rect);
  }

  private static void TryRecacheEntries()
  {
    if (!GenList.NullOrEmpty<VehicleStatDrawEntry>((IList<VehicleStatDrawEntry>) VehicleInfoCard.cachedDrawEntries))
      return;
    VehicleInfoCard.cachedDrawEntries.AddRange(VehicleInfoCard.StatsToDraw().Where<VehicleStatDrawEntry>((Func<VehicleStatDrawEntry, bool>) (statDrawEntry => statDrawEntry.ShouldDisplay)));
    VehicleInfoCard.cachedDrawEntries.AddRange(VehicleInfoCard.vehicleDef.SpecialDisplayStats(VehicleInfoCard.vehicle).Where<VehicleStatDrawEntry>((Func<VehicleStatDrawEntry, bool>) (statDrawEntry => statDrawEntry.ShouldDisplay)));
    VehicleInfoCard.FinalizeCachedDrawEntries((IEnumerable<VehicleStatDrawEntry>) VehicleInfoCard.cachedDrawEntries);
  }

  public static void SelectEntry(int index)
  {
    if (index < 0 || index > VehicleInfoCard.cachedDrawEntries.Count)
      return;
    VehicleInfoCard.SelectEntry(VehicleInfoCard.cachedDrawEntries[index]);
  }

  public static void SelectEntry(VehicleStatDef statDef, bool playSound = false)
  {
    foreach (VehicleStatDrawEntry cachedDrawEntry in VehicleInfoCard.cachedDrawEntries)
    {
      if (cachedDrawEntry.stat == statDef)
      {
        VehicleInfoCard.SelectEntry(cachedDrawEntry, playSound);
        return;
      }
    }
    Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("MessageCannotSelectInvisibleStat", NamedArgument.op_Implicit((Def) statDef))), MessageTypeDefOf.RejectInput, false);
  }

  private static void SelectEntry(VehicleStatDrawEntry rec, bool playSound = true)
  {
    VehicleInfoCard.selectedEntry = rec;
    VehicleInfoCard.scrollPositioner.Arm(true);
    if (!playSound)
      return;
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, (Map) null);
  }

  private static void DrawStatsWorker(Rect rect)
  {
    Rect scrollOutRect;
    // ISSUE: explicit constructor call
    ((Rect) ref scrollOutRect).\u002Ector(rect);
    ref Rect local1 = ref scrollOutRect;
    ((Rect) ref local1).width = ((Rect) ref local1).width * 0.5f;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(rect);
    ((Rect) ref rect1).x = ((Rect) ref scrollOutRect).xMax;
    ((Rect) ref rect1).width = ((Rect) ref rect).xMax - ((Rect) ref rect1).x;
    VehicleInfoCard.scrollPositioner.ClearInterestRects();
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 1);
    try
    {
      Rect rect2 = new Rect(0.0f, 0.0f, ((Rect) ref scrollOutRect).width - 16f, VehicleInfoCard.listHeight);
      Widgets.BeginScrollView(scrollOutRect, ref VehicleInfoCard.scrollPosition, rect2, true);
      float num1 = 0.0f;
      string str = (string) null;
      VehicleInfoCard.mousedOverEntry = (VehicleStatDrawEntry) null;
      foreach (VehicleStatDrawEntry cachedDrawEntry in VehicleInfoCard.cachedDrawEntries)
      {
        VehicleStatDrawEntry drawEntry = cachedDrawEntry;
        if (drawEntry.CategoryLabel != str)
        {
          Widgets.ListSeparator(ref num1, ((Rect) ref rect2).width, drawEntry.CategoryLabel);
          str = drawEntry.CategoryLabel;
        }
        bool highlightLabel = false;
        bool lowlightLabel = false;
        bool selected = VehicleInfoCard.selectedEntry == drawEntry;
        bool flag = false;
        if (((Window) VehicleInfoCard.infoCard).CommonSearchWidget.filter.Active)
        {
          if (VehicleInfoCard.Matching(drawEntry))
          {
            highlightLabel = true;
            flag = true;
          }
          else
            lowlightLabel = true;
        }
        Rect rect3;
        // ISSUE: explicit constructor call
        ((Rect) ref rect3).\u002Ector(8f, num1, ((Rect) ref rect2).width - 8f, 30f);
        num1 += drawEntry.Draw(((Rect) ref rect3).x, ((Rect) ref rect3).y, ((Rect) ref rect3).width, selected, highlightLabel, lowlightLabel, (Action) (() => VehicleInfoCard.SelectEntry(drawEntry)), (Action) (() => VehicleInfoCard.mousedOverEntry = drawEntry), VehicleInfoCard.scrollPosition, scrollOutRect);
        ((Rect) ref rect3).yMax = num1;
        if (selected | flag)
          VehicleInfoCard.scrollPositioner.RegisterInterestRect(rect3);
      }
      VehicleInfoCard.listHeight = num1 + 100f;
      Widgets.EndScrollView();
      VehicleInfoCard.scrollPositioner.ScrollVertically(ref VehicleInfoCard.scrollPosition, ((Rect) ref scrollOutRect).size);
      Rect rect4 = GenUI.ContractedBy(rect1, 10f);
      VehicleStatDrawEntry statDrawEntry = VehicleInfoCard.selectedEntry ?? VehicleInfoCard.mousedOverEntry ?? VehicleInfoCard.cachedDrawEntries.FirstOrDefault<VehicleStatDrawEntry>();
      if (statDrawEntry == null)
        return;
      Rect rect5;
      // ISSUE: explicit constructor call
      ((Rect) ref rect5).\u002Ector(0.0f, 0.0f, ((Rect) ref rect4).width - 16f, VehicleInfoCard.rightPanelHeight);
      string explanationText = statDrawEntry.GetExplanationText(VehicleInfoCard.vehicleDef, VehicleInfoCard.vehicle);
      float num2 = 0.0f;
      Widgets.BeginScrollView(rect4, ref VehicleInfoCard.scrollPositionRightPanel, rect5, true);
      Rect rect6 = rect5;
      ref Rect local2 = ref rect6;
      ((Rect) ref local2).width = ((Rect) ref local2).width - 4f;
      Widgets.Label(rect6, explanationText);
      float textHeight = Text.CalcHeight(explanationText, ((Rect) ref rect6).width) + 10f;
      float num3 = num2 + textHeight + VehicleInfoCard.DrawHyperlinks(rect6, statDrawEntry, textHeight);
      Widgets.EndScrollView();
      VehicleInfoCard.rightPanelHeight = num3;
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  private static float DrawHyperlinks(
    Rect rect,
    VehicleStatDrawEntry statDrawEntry,
    float textHeight)
  {
    float num1 = 0.0f;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y + textHeight, ((Rect) ref rect).width, Text.LineHeight);
    Color color = GUI.color;
    GUI.color = Widgets.NormalOptionColor;
    foreach (Dialog_InfoCard.Hyperlink hyperlink in statDrawEntry.GetHyperlinks(VehicleInfoCard.vehicle))
    {
      float num2 = Mathf.Max(Text.LineHeight, Text.CalcHeight(((Dialog_InfoCard.Hyperlink) ref hyperlink).Label, ((Rect) ref rect1).width));
      ((Rect) ref rect1).height = num2;
      Widgets.HyperlinkWithIcon(rect1, hyperlink, TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("ViewHyperlink", NamedArgument.op_Implicit(((Dialog_InfoCard.Hyperlink) ref hyperlink).Label))), 2f, 6f, new Color?(), false, (string) null);
      ref Rect local = ref rect1;
      ((Rect) ref local).y = ((Rect) ref local).y + num2;
      num1 += num2;
    }
    GUI.color = color;
    return num1;
  }
}
