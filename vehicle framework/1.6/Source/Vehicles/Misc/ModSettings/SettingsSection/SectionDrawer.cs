// Decompiled with JetBrains decompiler
// Type: Vehicles.SectionDrawer
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

internal static class SectionDrawer
{
  private const GameFont ListHeaderFont = (GameFont) 1;
  private const GameFont ListItemFont = (GameFont) 0;
  private static List<VehicleDef> vehicleDefs;
  private static readonly List<VehicleDef> filteredVehicleDefs = new List<VehicleDef>();
  private static readonly HashSet<string> headers = new HashSet<string>();
  private static readonly QuickSearchFilter vehicleFilter = new QuickSearchFilter();
  internal static Vector2 saveableFieldsScrollPosition;
  private static Vector2 vehicleDefsScrollPosition;

  private static float VehicleListHeight { get; set; }

  internal static List<VehicleDef> VehicleDefs
  {
    get
    {
      if (GenList.NullOrEmpty<VehicleDef>((IList<VehicleDef>) SectionDrawer.vehicleDefs))
      {
        List<VehicleDef> defsListForReading = DefDatabase<VehicleDef>.AllDefsListForReading;
        if (!GenList.NullOrEmpty<VehicleDef>((IList<VehicleDef>) defsListForReading))
        {
          SectionDrawer.vehicleDefs = defsListForReading.OrderBy<VehicleDef, bool>((Func<VehicleDef, bool>) (d => ((Def) d).modContentPack.PackageId.Contains("SmashPhil.VehicleFramework"))).ThenBy<VehicleDef, string>((Func<VehicleDef, string>) (d2 => ((Def) d2).modContentPack.PackageId)).ToList<VehicleDef>();
          SectionDrawer.RecacheVehicleFilter();
        }
      }
      return SectionDrawer.vehicleDefs;
    }
  }

  private static void RecacheVehicleFilter()
  {
    SectionDrawer.filteredVehicleDefs.Clear();
    SectionDrawer.headers.Clear();
    if (GenList.NullOrEmpty<VehicleDef>((IList<VehicleDef>) SectionDrawer.VehicleDefs))
      return;
    foreach (VehicleDef vehicleDef in SectionDrawer.VehicleDefs)
    {
      if (GenText.NullOrEmpty(SectionDrawer.vehicleFilter.Text) || SectionDrawer.vehicleFilter.Matches(((Def) vehicleDef).defName) || SectionDrawer.vehicleFilter.Matches(((Def) vehicleDef).label) || SectionDrawer.vehicleFilter.Matches(((Def) vehicleDef).modContentPack.Name))
      {
        SectionDrawer.headers.Add(((Def) vehicleDef).modContentPack.Name);
        SectionDrawer.filteredVehicleDefs.Add(vehicleDef);
      }
    }
    SectionDrawer.VehicleListHeight = -1f;
  }

  private static void RecacheVehicleListHeight(float width)
  {
    float num = 0.0f;
    TextBlock textBlock1;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock1).\u002Ector((GameFont) 1, (TextAnchor) 4);
    try
    {
      foreach (string header in SectionDrawer.headers)
        num += Text.CalcHeight(header, width);
    }
    finally
    {
      textBlock1.Dispose();
    }
    TextBlock textBlock2;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock2).\u002Ector((GameFont) 0);
    try
    {
      foreach (VehicleDef filteredVehicleDef in SectionDrawer.filteredVehicleDefs)
        num += Text.CalcHeight(TaggedString.op_Implicit(((Def) filteredVehicleDef).LabelCap), width);
    }
    finally
    {
      textBlock2.Dispose();
    }
    SectionDrawer.VehicleListHeight = num;
  }

  public static void DrawVehicleList(
    Rect rect,
    Func<bool, string> tooltipGetter = null,
    Predicate<VehicleDef> validator = null)
  {
    Rect rect1 = GenUI.ContractedBy(rect, 10f);
    ref Rect local1 = ref rect1;
    ((Rect) ref local1).width = ((Rect) ref local1).width / 4f;
    Widgets.DrawBoxSolid(rect1, Color.grey);
    Rect rect2 = GenUI.ContractedBy(rect1, 1f);
    Widgets.DrawBoxSolid(rect2, ListingExtension.MenuSectionBGFillColor);
    Rect rect3 = rect2;
    ((Rect) ref rect3).height = Text.LineHeight;
    Rect rect4 = rect3;
    TextBlock textBlock1;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock1).\u002Ector((GameFont) 1);
    try
    {
      Widgets.Label(rect4, Translator.Translate("VF_ListSearchText"));
      ref Rect local2 = ref rect4;
      ((Rect) ref local2).y = ((Rect) ref local2).y + ((Rect) ref rect4).height;
      string str = Widgets.TextField(rect4, SectionDrawer.vehicleFilter.Text);
      if (str != SectionDrawer.vehicleFilter.Text)
      {
        SectionDrawer.vehicleFilter.Text = str;
        SectionDrawer.RecacheVehicleFilter();
      }
      if (GenList.NullOrEmpty<VehicleDef>((IList<VehicleDef>) SectionDrawer.filteredVehicleDefs) && GenList.NullOrEmpty<VehicleDef>((IList<VehicleDef>) SectionDrawer.VehicleDefs))
        return;
      if (VehicleMod.selectedDef != null)
      {
        if (KeyBindingDefOf.MapDolly_Up.KeyDownEvent)
        {
          int index = SectionDrawer.filteredVehicleDefs.IndexOf(VehicleMod.selectedDef) - 1;
          if (index < 0)
            index = SectionDrawer.filteredVehicleDefs.Count - 1;
          VehicleMod.SelectVehicle(SectionDrawer.filteredVehicleDefs[index]);
        }
        if (KeyBindingDefOf.MapDolly_Down.KeyDownEvent)
        {
          int index = SectionDrawer.filteredVehicleDefs.IndexOf(VehicleMod.selectedDef) + 1;
          if (index >= SectionDrawer.filteredVehicleDefs.Count)
            index = 0;
          VehicleMod.SelectVehicle(SectionDrawer.filteredVehicleDefs[index]);
        }
      }
      Rect rect5 = rect2;
      ((Rect) ref rect5).yMin = ((Rect) ref rect4).yMax;
      Rect rect6 = GenUI.ContractedBy(rect5, 1f);
      float width = ((Rect) ref rect6).width - 16f;
      if ((double) SectionDrawer.VehicleListHeight < 0.0)
        SectionDrawer.RecacheVehicleListHeight(width);
      Rect rect7 = new Rect(0.0f, 0.0f, width, SectionDrawer.VehicleListHeight);
      Widgets.BeginScrollView(rect6, ref SectionDrawer.vehicleDefsScrollPosition, rect7, true);
      string header = string.Empty;
      float num1 = 0.0f;
      foreach (VehicleDef filteredVehicleDef in SectionDrawer.filteredVehicleDefs)
      {
        try
        {
          if (header != ((Def) filteredVehicleDef).modContentPack.Name)
          {
            header = ((Def) filteredVehicleDef).modContentPack.Name;
            float num2 = Text.CalcHeight(header, ((Rect) ref rect7).width);
            Rect rect8;
            // ISSUE: explicit constructor call
            ((Rect) ref rect8).\u002Ector(0.0f, num1, ((Rect) ref rect7).width, num2);
            UIElements.Header(rect8, header, ListingExtension.BannerColor, (GameFont) 1, (TextAnchor) 4);
            num1 += ((Rect) ref rect8).height;
          }
          bool active = validator == null || validator(filteredVehicleDef);
          string disabledTooltip = tooltipGetter != null ? tooltipGetter(active) : string.Empty;
          TextBlock textBlock2;
          // ISSUE: explicit constructor call
          ((TextBlock) ref textBlock2).\u002Ector((GameFont) 0);
          try
          {
            float num3 = Text.CalcHeight(TaggedString.op_Implicit(((Def) filteredVehicleDef).LabelCap), ((Rect) ref rect7).width);
            if (SectionDrawer.ListItemSelectable(new Rect(0.0f, num1, ((Rect) ref rect7).width, num3), TaggedString.op_Implicit(((Def) filteredVehicleDef).LabelCap), Color.yellow, VehicleMod.selectedDef == filteredVehicleDef, active, disabledTooltip))
            {
              if (VehicleMod.selectedDef == filteredVehicleDef)
                VehicleMod.DeselectVehicle();
              else
                VehicleMod.SelectVehicle(filteredVehicleDef);
            }
            num1 += num3;
          }
          finally
          {
            textBlock2.Dispose();
          }
        }
        catch (Exception ex)
        {
          Log.Error($"Exception thrown while trying to select {filteredVehicleDef}. Disabling vehicle to preserve mod settings.\nException={ex}");
          VehicleMod.selectedDef = (VehicleDef) null;
          VehicleMod.selectedPatterns.Clear();
          VehicleMod.selectedDefUpgradeComp = (CompProperties_UpgradeTree) null;
          VehicleMod.selectedNode = (UpgradeNode) null;
          VehicleMod.SettingsDisabledFor.Add(((Def) filteredVehicleDef).defName);
        }
      }
      Widgets.EndScrollView();
    }
    finally
    {
      textBlock1.Dispose();
    }
  }

  private static bool ListItemSelectable(
    Rect rect,
    string label,
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
      if (selected)
        Widgets.DrawBoxSolid(rect, ListingExtension.HighlightColor);
      try
      {
        if (!active)
          GUIState.Disable();
        else if (Mouse.IsOver(rect))
          GUI.color = hoverColor;
        if (!GenText.NullOrEmpty(disabledTooltip))
          TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(disabledTooltip));
        Widgets.Label(rect, label);
        if (!Widgets.ButtonInvisible(rect, true))
          return false;
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
        return true;
      }
      finally
      {
        GUIState.Enable();
      }
    }
    finally
    {
      textBlock.Dispose();
    }
  }
}
