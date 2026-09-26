// Decompiled with JetBrains decompiler
// Type: Vehicles.SectionDebug
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using LudeonTK;
using RimWorld;
using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UpdateLogTool;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

public class SectionDebug : SettingsSection
{
  private const float ButtonHeight = 30f;
  private const float VerticalGap = 2f;
  private const int ButtonRows = 4;
  private const int DebugSectionColumns = 2;
  public bool debugDraftAnyVehicle;
  public bool debugInstantSendOff;
  public bool debugShootAnyTurret;
  public bool debugDrawCannonGrid;
  public bool debugDrawNodeGrid;
  public bool debugDrawHitbox;
  public bool debugDrawVehicleTracks;
  public bool debugDrawBumpers;
  public bool debugDrawLordMeetingPoint;
  public bool debugDrawFleePoint;
  public static FlashGridType debugDrawFlashGrid;
  public bool debugLogging;
  public bool debugPathCostChanges;
  public bool debugDrawVehiclePathCosts;
  public bool debugDrawPathfinderSearch;
  public bool debugSpawnVehicleBuildingGodMode;
  public bool debugUseMultithreading = true;
  public bool debugAllowRaiders;
  public bool hierarchalPathfinding;

  public override void ResetSettings()
  {
    base.ResetSettings();
    this.debugDraftAnyVehicle = false;
    this.debugInstantSendOff = false;
    this.debugShootAnyTurret = false;
    this.debugDrawCannonGrid = false;
    this.debugDrawNodeGrid = false;
    this.debugDrawHitbox = false;
    this.debugDrawVehicleTracks = false;
    this.debugDrawBumpers = false;
    this.debugDrawLordMeetingPoint = false;
    this.debugDrawFleePoint = false;
    SectionDebug.debugDrawFlashGrid = FlashGridType.None;
    this.debugLogging = false;
    this.debugPathCostChanges = false;
    this.debugDrawVehiclePathCosts = false;
    this.debugDrawPathfinderSearch = false;
    this.debugSpawnVehicleBuildingGodMode = false;
    this.debugUseMultithreading = true;
    this.debugAllowRaiders = false;
    this.hierarchalPathfinding = false;
  }

  public override void ExposeData()
  {
    Scribe_Values.Look<bool>(ref this.debugDraftAnyVehicle, "debugDraftAnyVehicle", false, false);
    Scribe_Values.Look<bool>(ref this.debugInstantSendOff, "debugInstantSendOff", false, false);
    Scribe_Values.Look<bool>(ref this.debugShootAnyTurret, "debugShootAnyTurret", false, false);
    Scribe_Values.Look<bool>(ref this.debugDrawCannonGrid, "debugDrawCannonGrid", false, false);
    Scribe_Values.Look<bool>(ref this.debugDrawNodeGrid, "debugDrawNodeGrid", false, false);
    Scribe_Values.Look<bool>(ref this.debugDrawHitbox, "debugDrawHitbox", false, false);
    Scribe_Values.Look<bool>(ref this.debugDrawVehicleTracks, "debugDrawVehicleTracks", false, false);
    Scribe_Values.Look<bool>(ref this.debugDrawBumpers, "debugDrawBumpers", false, false);
    Scribe_Values.Look<bool>(ref this.debugDrawLordMeetingPoint, "debugDrawLordMeetingPoint", false, false);
    Scribe_Values.Look<bool>(ref this.debugDrawFleePoint, "debugDrawFleePoint", false, false);
    Scribe_Values.Look<FlashGridType>(ref SectionDebug.debugDrawFlashGrid, "debugDrawFlashGrid", FlashGridType.None, false);
    Scribe_Values.Look<bool>(ref this.debugLogging, "debugLogging", false, false);
    Scribe_Values.Look<bool>(ref this.debugPathCostChanges, "debugPathCostChanges", false, false);
    Scribe_Values.Look<bool>(ref this.debugDrawVehiclePathCosts, "debugDrawVehiclePathCosts", false, false);
    Scribe_Values.Look<bool>(ref this.debugDrawPathfinderSearch, "debugDrawPathfinderSearch", false, false);
    if (!DebugProperties.Debug)
      return;
    Scribe_Values.Look<bool>(ref this.debugSpawnVehicleBuildingGodMode, "debugSpawnVehicleBuildingGodMode", false, false);
    Scribe_Values.Look<bool>(ref this.debugUseMultithreading, "debugUseMultithreading", true, false);
  }

  public override void OnGUI(Rect rect)
  {
    Rect rect1 = GenUI.ContractedBy(rect, 10f);
    ref Rect local = ref rect1;
    ((Rect) ref local).yMin = ((Rect) ref local).yMin + 27f;
    float buttonRowHeight = 126f;
    ((Rect) ref rect1).height = ((Rect) ref rect1).height - buttonRowHeight;
    SettingsSection.listingStandard = new Listing_Standard();
    ((Listing) SettingsSection.listingStandard).ColumnWidth = (float) ((double) ((Rect) ref rect1).width / 2.0 - 8.0);
    ((Listing) SettingsSection.listingStandard).Begin(rect1);
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(Color.white);
    try
    {
      ((Listing) SettingsSection.listingStandard).Header(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_Logging")), ListingExtension.BannerColor, (GameFont) 1, (TextAnchor) 4);
      SettingsSection.listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugLogging")), ref this.debugLogging, TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugLoggingTooltip")), 0.0f, 1f);
      ((Listing) SettingsSection.listingStandard).Header(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_Troubleshooting")), ListingExtension.BannerColor, (GameFont) 1, (TextAnchor) 4);
      SettingsSection.listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugDraftAnyVehicle")), ref this.debugDraftAnyVehicle, TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugDraftAnyVehicleTooltip")), 0.0f, 1f);
      bool debugShootAnyTurret = this.debugShootAnyTurret;
      SettingsSection.listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugShootAnyTurret")), ref this.debugShootAnyTurret, TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugShootAnyTurretTooltip")), 0.0f, 1f);
      if (debugShootAnyTurret != this.debugShootAnyTurret && Current.ProgramState == 2 && !GenList.NullOrEmpty<Map>((IList<Map>) Find.Maps))
      {
        foreach (Map map in Find.Maps)
        {
          foreach (VehiclePawn allPawnsOn in map.AllPawnsOnMap<VehiclePawn>(Faction.OfPlayer))
            allPawnsOn.CompVehicleTurrets?.RecacheTurretPermissions();
        }
      }
      ((Listing) SettingsSection.listingStandard).Header(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_Drawers")), ListingExtension.BannerColor, (GameFont) 1, (TextAnchor) 4);
      SettingsSection.listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugDrawUpgradeNodeGrid")), ref this.debugDrawNodeGrid, TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugDrawUpgradeNodeGridTooltip")), 0.0f, 1f);
      SettingsSection.listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugDrawHitbox")), ref this.debugDrawHitbox, TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugDrawHitboxTooltip")), 0.0f, 1f);
      SettingsSection.listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugDrawBumpers")), ref this.debugDrawBumpers, TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugDrawBumpersTooltip")), 0.0f, 1f);
      SettingsSection.listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugDrawLordMeetingPoint")), ref this.debugDrawLordMeetingPoint, TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugDrawLordMeetingPointTooltip")), 0.0f, 1f);
      SettingsSection.listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugDrawFleePoints")), ref this.debugDrawFleePoint, TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugDrawFleePointsTooltip")), 0.0f, 1f);
      ((Listing) SettingsSection.listingStandard).EnumSliderLabeled<FlashGridType>(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugDrawGrid")), ref SectionDebug.debugDrawFlashGrid, TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugDrawGridTooltip")), string.Empty, (Func<FlashGridType, string>) (flashGridType => flashGridType.ToString()));
      ((Listing) SettingsSection.listingStandard).Header(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_Pathing")), ListingExtension.BannerColor, (GameFont) 1, (TextAnchor) 4);
      SettingsSection.listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugDrawVehiclePathingCosts")), ref this.debugDrawVehiclePathCosts, TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugDrawVehiclePathingCostsTooltip")), 0.0f, 1f);
      SettingsSection.listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugDrawPathfinderSearch")), ref this.debugDrawPathfinderSearch, TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugDrawPathfinderSearchTooltip")), 0.0f, 1f);
    }
    finally
    {
      textBlock.Dispose();
    }
    ((Listing) SettingsSection.listingStandard).End();
    this.DoBottomButtons(rect1, buttonRowHeight);
    ((Listing) SettingsSection.listingStandard).End();
  }

  private void DoBottomButtons(Rect rect, float buttonRowHeight)
  {
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(rect);
    ((Rect) ref rect1).y = ((Rect) ref rect).yMax;
    ((Rect) ref rect1).height = buttonRowHeight;
    ((Listing) SettingsSection.listingStandard).ColumnWidth = (float) ((double) ((Rect) ref rect1).width / 3.0 - 17.0);
    ((Listing) SettingsSection.listingStandard).Begin(rect1);
    if (SettingsSection.listingStandard.ButtonText(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_ShowRecentNews")), (string) null, 1f))
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
      this.ShowAllUpdates();
    }
    if (SettingsSection.listingStandard.ButtonText(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_LogThreadActivity")), "VF_DevMode_LogThreadActivityTooltip", 1f))
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
      Find.WindowStack.Add((Window) new Dialog_DedicatedThreadActivity((Func<DedicatedThread>) (() => Find.CurrentMap == null ? (DedicatedThread) null : Find.CurrentMap.GetCachedMapComponent<VehiclePathingSystem>().dedicatedThread)));
    }
    if (SettingsSection.listingStandard.ButtonText(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_GraphEditor")), (string) null, 1f))
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
      Find.WindowStack.Add((Window) new Dialog_GraphEditor());
    }
    if (SettingsSection.listingStandard.ButtonText(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugPathfinderDebugging")), "VF_DevMode_DebugPathfinderDebuggingTooltip", 1f))
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
      SectionDebug.RegionDebugMenu();
    }
    if (!SettingsSection.listingStandard.ButtonText(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugWorldPathfinderDebugging")), "VF_DevMode_DebugWorldPathfinderDebuggingTooltip", 1f))
      return;
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
    SectionDebug.WorldPathingDebugMenu();
  }

  private void RevalidateAllMapThreads()
  {
    if (Current.ProgramState != 2 || GenList.NullOrEmpty<Map>((IList<Map>) Find.Maps))
      return;
    foreach (Map map in Find.Maps)
    {
      VehiclePathingSystem cachedMapComponent = map.GetCachedMapComponent<VehiclePathingSystem>();
      if (this.debugUseMultithreading)
        cachedMapComponent.InitThread();
      else
        cachedMapComponent.ReleaseThread();
    }
  }

  public static void RegionDebugMenu()
  {
    List<Toggle> toggles = new List<Toggle>();
    toggles.Add(new Toggle("None", (Func<bool>) (() => true), new Action<bool>(SetNone)));
    foreach (VehicleDef vehicleDef1 in (IEnumerable<VehicleDef>) DefDatabase<VehicleDef>.AllDefsListForReading.OrderBy<VehicleDef, bool>((Func<VehicleDef, bool>) (def => ((Def) def).modContentPack.ModMetaData.SamePackageId("SmashPhil.VehicleFramework", true))).ThenBy<VehicleDef, string>((Func<VehicleDef, string>) (def => ((Def) def).modContentPack.Name)).ThenBy<VehicleDef, string>((Func<VehicleDef, string>) (d => ((Def) d).defName)))
    {
      VehicleDef vehicleDef = vehicleDef1;
      toggles.Add(new Toggle(((Def) vehicleDef).defName, ((Def) vehicleDef).modContentPack.Name, (Func<bool>) (() => false), (Action<bool>) (value =>
      {
        if (!value)
          return;
        List<Toggle> list = DebugHelper.DebugToggles<DebugRegionType>(vehicleDef, DebugHelper.Local).ToList<Toggle>();
        Find.WindowStack.Add((Window) new Dialog_ToggleMenu(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugPathfinderDebugging")), list));
      }))
      {
        Disabled = !PathingHelper.ShouldCreateRegions(vehicleDef)
      });
    }
    Find.WindowStack.Add((Window) new Dialog_RadioButtonMenu(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugPathfinderDebugging")), toggles));

    static void SetNone(bool value)
    {
      if (!value)
        return;
      DebugHelper.Local.VehicleDef = (VehicleDef) null;
      DebugHelper.Local.DebugType = DebugRegionType.None;
    }
  }

  private static void WorldPathingDebugMenu()
  {
    List<Toggle> toggles = new List<Toggle>();
    toggles.Add(new Toggle("None", (Func<bool>) (() => true), new Action<bool>(SetNone)));
    foreach (VehicleDef vehicleDef1 in (IEnumerable<VehicleDef>) DefDatabase<VehicleDef>.AllDefsListForReading.OrderBy<VehicleDef, bool>((Func<VehicleDef, bool>) (def => ((Def) def).modContentPack.ModMetaData.SamePackageId("SmashPhil.VehicleFramework", true))).ThenBy<VehicleDef, string>((Func<VehicleDef, string>) (def => ((Def) def).modContentPack.Name)).ThenBy<VehicleDef, string>((Func<VehicleDef, string>) (d => ((Def) d).defName)))
    {
      VehicleDef vehicleDef = vehicleDef1;
      toggles.Add(new Toggle(((Def) vehicleDef).defName, ((Def) vehicleDef).modContentPack.Name, (Func<bool>) (() => DebugHelper.World.VehicleDef == vehicleDef), (Action<bool>) (value =>
      {
        if (!value)
          return;
        List<Toggle> list = DebugHelper.DebugToggles<WorldPathingDebugType>(vehicleDef, DebugHelper.World).ToList<Toggle>();
        Find.WindowStack.Add((Window) new Dialog_RadioButtonMenu(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugWorldPathfinderDebugging")), list));
      }))
      {
        Disabled = !PathingHelper.ShouldCreateRegions(vehicleDef)
      });
    }
    Find.WindowStack.Add((Window) new Dialog_RadioButtonMenu(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugPathfinderDebugging")), toggles));

    static void SetNone(bool value)
    {
      if (!value)
        return;
      DebugHelper.World.VehicleDef = (VehicleDef) null;
      DebugHelper.World.DebugType = WorldPathingDebugType.None;
    }
  }

  public void ShowAllUpdates()
  {
    string str1 = "Null";
    VehicleHarmony.updates.Clear();
    foreach (UpdateLog updateLog in (IEnumerable<UpdateLog>) VehicleMod.content.ReadPreviousFiles().OrderByDescending<UpdateLog, int>((Func<UpdateLog, int>) (log => Ext_Settings.CombineVersionString(log.UpdateData.currentVersion))))
      VehicleHarmony.updates.Add(updateLog);
    try
    {
      List<DebugMenuOption> debugMenuOptionList = new List<DebugMenuOption>();
      foreach (UpdateLog update1 in VehicleHarmony.updates)
      {
        UpdateLog update = update1;
        str1 = update.UpdateData.currentVersion;
        string str2 = str1;
        if (str1 == VehicleMod.metaData.ModVersion)
          str2 += " (Current)";
        debugMenuOptionList.Add(new DebugMenuOption(str2, (DebugMenuOptionMode) 0, (Action) (() => Find.WindowStack.Add((Window) new Dialog_NewUpdate(new HashSet<UpdateLog>()
        {
          update
        })))));
      }
      Find.WindowStack.Add((Window) new Dialog_DebugOptionListLister((IEnumerable<DebugMenuOption>) debugMenuOptionList, (string) null));
    }
    catch (Exception ex)
    {
      Log.Error($"{"[VehicleFramework]"} Unable to show update for {str1} Exception = {ex}");
    }
  }
}
