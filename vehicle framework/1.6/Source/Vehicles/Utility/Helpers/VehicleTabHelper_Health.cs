// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleTabHelper_Health
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

[StaticConstructorOnStartup]
public static class VehicleTabHelper_Health
{
  public const float LeftWindowWidth = 250f;
  public const float WindowHeight = 430f;
  public const float LabelColumnWidth = 200f;
  public const float ColumnWidth = 100f;
  public const float ComponentRowHeight = 20f;
  public const float ComponentIndicatorIconSize = 20f;
  public const float MoreInfoIconSize = 24f;
  private const int ColumnCount = 2;
  private static readonly Color SlightlyUpgraded = new Color(0.7f, 0.75f, 1f);
  private static readonly Color HeavilyUpgraded = Color.cyan;
  private static readonly Color MouseOverColor = new Color(0.85f, 0.85f, 0.85f, 0.1f);
  private static readonly Color AlternatingColor = new Color(0.75f, 0.75f, 0.75f, 0.1f);
  private static readonly List<DamageArmorCategoryDef> ArmorRatingDefs;
  private static readonly StringBuilder TooltipBuilder = new StringBuilder();
  private static float componentListHeight;
  private static VehiclePawn inspectingVehicle;
  private static Vector2 size;
  private static bool compressed;
  private static bool moreInfo;
  private static ITab_Vehicle_Health.VehicleHealthTab onTab;
  private static Vector2 componentTabScrollPos;
  private static VehicleComponent selectedComponent;

  public static Vector2 Size => VehicleTabHelper_Health.size;

  static VehicleTabHelper_Health()
  {
    VehicleTabHelper_Health.ArmorRatingDefs = DefDatabase<DamageArmorCategoryDef>.AllDefsListForReading;
  }

  public static void Init()
  {
    VehicleTabHelper_Health.componentTabScrollPos = Vector2.zero;
    VehicleTabHelper_Health.selectedComponent = (VehicleComponent) null;
    VehicleTabHelper_Health.moreInfo = false;
    VehicleTabHelper_Health.RecacheWindowWidth();
  }

  public static void Clear()
  {
    VehicleTabHelper_Health.componentTabScrollPos = Vector2.zero;
    VehicleTabHelper_Health.selectedComponent = (VehicleComponent) null;
    VehicleTabHelper_Health.moreInfo = false;
  }

  public static Vector2 Start(VehiclePawn vehicle, bool compressed = false, float height = 430f)
  {
    VehicleTabHelper_Health.size.y = height;
    if (vehicle != VehicleTabHelper_Health.inspectingVehicle)
    {
      VehicleTabHelper_Health.inspectingVehicle = vehicle;
      VehicleTabHelper_Health.compressed = compressed;
      VehicleTabHelper_Health.RecacheWindowWidth();
      VehicleTabHelper_Health.RecacheComponentListHeight();
    }
    return VehicleTabHelper_Health.Size;
  }

  public static void End()
  {
  }

  public static void DrawHealthPanel(VehiclePawn vehicle)
  {
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(0.0f, 20f, VehicleTabHelper_Health.Size.x, VehicleTabHelper_Health.Size.y - 20f);
    Rect rect2 = GenUI.Rounded(new Rect(((Rect) ref rect1).x, ((Rect) ref rect1).y, 250f, ((Rect) ref rect1).height));
    Rect rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector(((Rect) ref rect2).xMax, ((Rect) ref rect1).y, VehicleTabHelper_Health.Size.x - 250f, ((Rect) ref rect1).height);
    ref Rect local = ref rect2;
    ((Rect) ref local).yMin = ((Rect) ref local).yMin + 11f;
    VehicleTabHelper_Health.DrawHealthInfo(rect2, vehicle);
    VehicleTabHelper_Health.DrawComponentsInfo(rect3, vehicle);
  }

  private static void DrawHealthInfo(Rect rect, VehiclePawn vehicle)
  {
    Widgets.DrawMenuSection(rect);
    TabDrawer.DrawTabs<TabRecord>(rect, new List<TabRecord>()
    {
      new TabRecord(TaggedString.op_Implicit(Translator.Translate("HealthOverview")), (Action) (() => VehicleTabHelper_Health.onTab = ITab_Vehicle_Health.VehicleHealthTab.Overview), VehicleTabHelper_Health.onTab == ITab_Vehicle_Health.VehicleHealthTab.Overview)
    }, 200f);
    rect = GenUI.ContractedBy(rect, 9f);
    Widgets.BeginGroup(rect);
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(new GameFont?((GameFont) 1), new TextAnchor?((TextAnchor) 0), new Color?(Color.white));
    try
    {
      switch (VehicleTabHelper_Health.onTab)
      {
        case ITab_Vehicle_Health.VehicleHealthTab.Overview:
          VehicleTabHelper_Health.DrawVehicleInformation(rect, vehicle);
          goto case ITab_Vehicle_Health.VehicleHealthTab.JobSettings;
        case ITab_Vehicle_Health.VehicleHealthTab.JobSettings:
          Widgets.EndGroup();
          break;
        default:
          throw new NotImplementedException("onTab");
      }
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  private static void DrawVehicleInformation(Rect leftRect, VehiclePawn vehicle)
  {
    float num1 = 0.0f;
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(0.0f, num1, ((Rect) ref leftRect).width, 34f);
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((TextAnchor) 1);
    try
    {
      Widgets.Label(rect, ((Entity) vehicle).LabelCap);
    }
    finally
    {
      textBlock.Dispose();
    }
    if (Mouse.IsOver(rect))
    {
      string dateReadout = $"{Find.ActiveLanguageWorker.OrdinalNumber(vehicle.ageTracker.BirthDayOfSeasonZeroBased + 1, (Gender) 0)} {QuadrumUtility.Label(vehicle.ageTracker.BirthQuadrum)}, {vehicle.ageTracker.BirthYear}";
      int num2;
      int num3;
      int num4;
      float num5;
      GenDate.TicksToPeriod((long) GenTicks.TicksAbs - vehicle.ageTracker.BirthAbsTicks, ref num2, ref num3, ref num4, ref num5);
      string chronologicalReadout = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("AgeChronological", NamedArgument.op_Implicit(num2), NamedArgument.op_Implicit(num3), NamedArgument.op_Implicit(num4)));
      TooltipHandler.TipRegion(rect, (Func<string>) (() => $"{TranslatorFormattedStringExtensions.Translate("VF_VehicleAgeReadout", NamedArgument.op_Implicit(dateReadout))}\n{chronologicalReadout}"), "HealthTab".GetHashCode());
      Widgets.DrawHighlight(rect);
    }
    float curY = num1 + 34f;
    Rect leftRect1;
    // ISSUE: explicit constructor call
    ((Rect) ref leftRect1).\u002Ector(0.0f, curY, ((Rect) ref leftRect).width, 34f);
    foreach (VehicleStatDef vehicleStatDef in vehicle.VehicleDef.StatCategoryDefs().Distinct<VehicleStatDef>())
    {
      curY = vehicleStatDef.Worker.DrawVehicleStat(leftRect1, curY, vehicle);
      ((Rect) ref leftRect1).y = curY;
    }
  }

  private static void DrawComponentsInfo(Rect rect, VehiclePawn vehicle)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 1, (TextAnchor) 4);
    try
    {
      float y1 = Verse.Text.CalcSize(TaggedString.op_Implicit(Translator.Translate("VF_ComponentHealth"))).y;
      Rect rect1 = new Rect(((Rect) ref rect).x + 200f, ((Rect) ref rect).y, 100f, y1);
      Widgets.Label(rect1, Translator.Translate("VF_ComponentHealth"));
      Rect rect2 = rect1;
      ((Rect) ref rect2).x = ((Rect) ref rect1).x + 100f;
      Rect rect3 = rect2;
      Widgets.Label(rect3, Translator.Translate("VF_ComponentEfficiency"));
      ((Rect) ref rect1).x = ((Rect) ref rect3).xMax;
      if (!VehicleTabHelper_Health.compressed)
      {
        Rect rect4;
        // ISSUE: explicit constructor call
        ((Rect) ref rect4).\u002Ector(((Rect) ref rect3).x + 50f, 0.0f, 24f, 24f);
        Color color1 = GUI.color;
        Color color2 = !VehicleTabHelper_Health.moreInfo ? Color.white : Color.green;
        Color color3 = !VehicleTabHelper_Health.moreInfo ? GenUI.MouseoverColor : new Color(0.0f, 0.5f, 0.0f);
        if (Widgets.ButtonImageFitted(rect4, CaravanThingsTabUtility.SpecificTabButtonTex, color2, color3))
        {
          VehicleTabHelper_Health.moreInfo = !VehicleTabHelper_Health.moreInfo;
          VehicleTabHelper_Health.RecacheWindowWidth();
          if (VehicleTabHelper_Health.moreInfo)
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.TabOpen, (Map) null);
          else
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.TabClose, (Map) null);
        }
        GUI.color = color1;
        if (VehicleTabHelper_Health.moreInfo)
        {
          foreach (DamageArmorCategoryDef armorRatingDef in VehicleTabHelper_Health.ArmorRatingDefs)
          {
            Widgets.Label(rect1, ((Def) armorRatingDef.armorRatingStat).LabelCap);
            ref Rect local = ref rect1;
            ((Rect) ref local).x = ((Rect) ref local).x + ((Rect) ref rect1).width;
          }
        }
      }
      using (new TextBlock(UIElements.MenuSectionBgBorderColor))
        Widgets.DrawLineHorizontal(((Rect) ref rect).x, ((Rect) ref rect1).y + y1 / 1.25f, ((Rect) ref rect).width);
      ref Rect local1 = ref rect;
      ((Rect) ref local1).yMin = ((Rect) ref local1).yMin + (float) ((double) y1 / 1.25 + 1.0);
      ref Rect local2 = ref rect;
      ((Rect) ref local2).x = ((Rect) ref local2).x + 2.5f;
      ref Rect local3 = ref rect;
      ((Rect) ref local3).width = ((Rect) ref local3).width - 5f;
      Rect rect5 = new Rect(((Rect) ref rect).x, ((Rect) ref rect).y + ((Rect) ref rect1).height * 2f, ((Rect) ref rect).width - 16f, VehicleTabHelper_Health.componentListHeight);
      bool highlighted = false;
      Widgets.BeginScrollView(rect, ref VehicleTabHelper_Health.componentTabScrollPos, rect5, true);
      float y2 = ((Rect) ref rect5).y;
      bool flag = false;
      foreach (VehicleComponent component in vehicle.statHandler.components)
      {
        Rect rect6;
        // ISSUE: explicit constructor call
        ((Rect) ref rect6).\u002Ector(((Rect) ref rect).x, y2, ((Rect) ref rect).width - 16f, 20f);
        float num = VehicleTabHelper_Health.DrawCompRow(rect6, component, 200f, 100f, highlighted);
        Rect rect7 = new Rect(rect6);
        ((Rect) ref rect7).height = num;
        Rect rect8 = rect7;
        if (Mouse.IsOver(rect8))
        {
          Widgets.DrawBoxSolid(rect8, VehicleTabHelper_Health.MouseOverColor);
          vehicle.HighlightedComponent = component;
          flag = true;
        }
        else if (VehicleTabHelper_Health.selectedComponent == component)
        {
          Widgets.DrawBoxSolid(rect8, VehicleTabHelper_Health.MouseOverColor);
          flag = true;
        }
        if (Widgets.ButtonInvisible(rect6, true))
        {
          SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
          VehicleTabHelper_Health.selectedComponent = VehicleTabHelper_Health.selectedComponent != component ? component : (VehicleComponent) null;
        }
        y2 += num;
        highlighted = !highlighted;
      }
      if (!flag)
        vehicle.HighlightedComponent = (VehicleComponent) null;
      Widgets.EndScrollView();
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  private static float DrawCompRow(
    Rect rect,
    VehicleComponent component,
    float labelWidth,
    float columnWidth,
    bool highlighted)
  {
    float num1 = Verse.Text.CalcHeight(component.props.label, labelWidth);
    float num2 = Mathf.Max(((Rect) ref rect).height, num1);
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y, labelWidth, num2);
    if (highlighted)
      Widgets.DrawBoxSolid(new Rect(((Rect) ref rect).x, ((Rect) ref rect).y, ((Rect) ref rect).width + 16f, num2), VehicleTabHelper_Health.AlternatingColor);
    Verse.Text.Anchor = (TextAnchor) 3;
    Widgets.Label(rect1, component.props.label);
    ref Rect local1 = ref rect1;
    ((Rect) ref local1).x = ((Rect) ref local1).x + ((Rect) ref rect1).width;
    ((Rect) ref rect1).width = columnWidth;
    Verse.Text.Anchor = (TextAnchor) 4;
    Widgets.Label(rect1, ColoredText.Colorize(GenText.ToStringPercent(component.HealthPercent), component.ComponentEfficiencyColor()));
    TooltipHandler.TipRegion(rect1, TipSignal.op_Implicit($"{component.Health:F0}/{component.MaxHealth:F0}"));
    ref Rect local2 = ref rect1;
    ((Rect) ref local2).x = ((Rect) ref local2).x + columnWidth;
    string str1 = (string) null;
    string str2;
    if (!GenList.NullOrEmpty<VehicleStatDef>((IList<VehicleStatDef>) component.props.categories))
    {
      str2 = ColoredText.Colorize(GenText.ToStringPercent(component.Efficiency), component.ComponentEfficiencyColor());
      using (new ClearStringOnDispose(VehicleTabHelper_Health.TooltipBuilder))
      {
        VehicleTabHelper_Health.TooltipBuilder.AppendLine(TaggedString.op_Implicit(Translator.Translate("VF_EfficiencyEffector")));
        foreach (VehicleStatDef category in component.props.categories)
          VehicleTabHelper_Health.TooltipBuilder.AppendLine($" - {category.LabelCap}");
        str1 = VehicleTabHelper_Health.TooltipBuilder.ToString();
      }
    }
    else
      str2 = "-";
    Widgets.Label(rect1, str2);
    if (str1 != null)
      TooltipHandler.TipRegion(rect1, TipSignal.op_Implicit(str1));
    if (!VehicleTabHelper_Health.compressed && VehicleTabHelper_Health.moreInfo)
    {
      foreach (DamageArmorCategoryDef armorRatingDef in VehicleTabHelper_Health.ArmorRatingDefs)
      {
        ref Rect local3 = ref rect1;
        ((Rect) ref local3).x = ((Rect) ref local3).x + columnWidth;
        float upgraded;
        float num3 = component.ArmorRating(armorRatingDef, out upgraded);
        string str3 = ColoredText.Colorize(GenText.ToStringByStyle(num3, armorRatingDef.armorRatingStat.toStringStyle, (ToStringNumberSense) 1), VehicleTabHelper_Health.ArmorUpgradeQualityColor(upgraded));
        Widgets.Label(rect1, str3);
        if (!Mathf.Approximately(upgraded, 0.0f))
        {
          string stringByStyle = GenText.ToStringByStyle(num3 - upgraded, armorRatingDef.armorRatingStat.toStringStyle, (ToStringNumberSense) 1);
          TooltipHandler.TipRegion(rect1, TipSignal.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_BaseArmorRating", NamedArgument.op_Implicit(stringByStyle))));
        }
      }
    }
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).xMax, ((Rect) ref rect1).y, 20f, 20f);
    component.DrawIcon(rect2);
    return num2;
  }

  private static Color ArmorUpgradeQualityColor(float upgraded)
  {
    return (double) upgraded < -0.5 ? HealthUtility.RedColor : ((double) upgraded < -0.5 ? HealthUtility.GoodConditionColor : ((double) upgraded < -0.25 ? HealthUtility.ImpairedColor : ((double) upgraded < 0.0 ? HealthUtility.SlightlyImpairedColor : ((double) upgraded == 0.0 ? HealthUtility.GoodConditionColor : ((double) upgraded < 0.5 ? VehicleTabHelper_Health.SlightlyUpgraded : VehicleTabHelper_Health.HeavilyUpgraded)))));
  }

  private static void RecacheWindowWidth()
  {
    VehicleTabHelper_Health.size.x = 690f;
    if (VehicleTabHelper_Health.compressed || !VehicleTabHelper_Health.moreInfo)
      return;
    VehicleTabHelper_Health.size.x += 100f * (float) VehicleTabHelper_Health.ArmorRatingDefs.Count;
  }

  private static void RecacheComponentListHeight(float lineHeight = 20f)
  {
    VehicleTabHelper_Health.componentListHeight = 0.0f;
    foreach (VehicleComponent component in VehicleTabHelper_Health.inspectingVehicle.statHandler.components)
    {
      float num = Verse.Text.CalcHeight(component.props.label, VehicleTabHelper_Health.Size.x - 250f);
      VehicleTabHelper_Health.componentListHeight += Mathf.Max(lineHeight, num);
    }
  }

  public static Color ComponentEfficiencyColor(this VehicleComponent component)
  {
    float efficiency = component.Efficiency;
    Color color;
    if ((double) efficiency > 0.0)
    {
      if ((double) efficiency > 0.0)
      {
        if ((double) efficiency >= 0.40000000596046448)
        {
          if ((double) efficiency >= 0.699999988079071)
          {
            if ((double) efficiency < 0.99900001287460327)
            {
              color = HealthUtility.SlightlyImpairedColor;
              goto label_10;
            }
          }
          else
          {
            color = HealthUtility.ImpairedColor;
            goto label_10;
          }
        }
        else
        {
          color = HealthUtility.RedColor;
          goto label_10;
        }
      }
      color = HealthUtility.GoodConditionColor;
    }
    else
      color = Color.gray;
label_10:
    return color;
  }

  [StructLayout(LayoutKind.Sequential, Size = 1)]
  public readonly struct DrawBlock : IDisposable
  {
    public DrawBlock(VehiclePawn vehicle, bool compressed)
    {
      VehicleTabHelper_Health.Start(vehicle, compressed);
    }

    void IDisposable.Dispose() => VehicleTabHelper_Health.End();
  }
}
