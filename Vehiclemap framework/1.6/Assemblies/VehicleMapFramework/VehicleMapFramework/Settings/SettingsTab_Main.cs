// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Settings.SettingsTab_Main
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using SmashTools;
using System;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.Settings;

internal class SettingsTab_Main : SettingsTabDrawer
{
  public override int Index => 0;

  public override string Label
  {
    get => TaggedString.op_Implicit(Translator.Translate("VMF_Settings.Tab.Main"));
  }

  protected override void ResetSettings()
  {
    base.ResetSettings();
    this.settings.drawPlanet = true;
    this.settings.forceRotated = VehicleMapSettings.ForceRotated.None;
    this.settings.weightFactor = 1f;
    this.settings.autoGetOffPlayer = false;
    this.settings.autoGetOffNonPlayer = true;
    this.settings.crossMapJobProtect = true;
    this.settings.includeMapThings = true;
    this.settings.legacyCanReach = false;
    this.settings.joyPatches = false;
    this.settings.treatAsPlayerHome = false;
    this.settings.colonistBarMode = VehicleMapSettings.ShowVehiclesOnColonistBar.MouseIsOver;
    this.settings.drawVehicleMapGrid = false;
  }

  public override void Draw(Rect inRect)
  {
    base.Draw(inRect);
    Listing_Standard listingStandard = new Listing_Standard();
    ((Listing) listingStandard).Begin(inRect);
    listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VMF_Settings.DrawPlanet")), ref this.settings.drawPlanet, (string) null, 0.0f, 1f);
    if (this.settings.drawPlanet)
    {
      TaggedString taggedString = Translator.Translate("VMF_Settings.ForceRotated");
      Rect rect = ((Listing) listingStandard).GetRect(Text.CalcHeight(TaggedString.op_Implicit(taggedString), ((Listing) listingStandard).ColumnWidth * 0.5f), 1f);
      Widgets.Label(GenUI.LeftPart(rect, 0.5f), taggedString);
      VehicleMapSettings.ForceRotated forceRotated = this.settings.forceRotated;
      this.settings.forceRotated = (VehicleMapSettings.ForceRotated) Widgets.HorizontalSlider(GenUI.RightPart(rect, 0.5f), (float) forceRotated, -1f, 7f, false, Enum.GetName(typeof (VehicleMapSettings.ForceRotated), (object) forceRotated), (string) null, (string) null, 1f);
    }
    ListingExtension.SliderLabeled((Listing) listingStandard, TaggedString.op_Implicit(Translator.Translate("VMF_Settings.WeightFactor")), (string) null, (string) null, ref this.settings.weightFactor, 0.0f, 3f, 1f, 2, -1f, "");
    listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VMF_Settings.AutoGetOffPlayer")), ref this.settings.autoGetOffPlayer, (string) null, 0.0f, 1f);
    listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VMF_Settings.AutoGetOffNonPlayer")), ref this.settings.autoGetOffNonPlayer, (string) null, 0.0f, 1f);
    listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VMF_Settings.CrossMapJobProtect")), ref this.settings.crossMapJobProtect, TaggedString.op_Implicit(Translator.Translate("VMF_Settings.CrossMapJobProtect.Tooltip")), 0.0f, 1f);
    listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VMF_Settings.TreatAsCaravanInventory")), ref this.settings.includeMapThings, (string) null, 0.0f, 1f);
    listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VMF_Settings.LegacyCanReach")), ref this.settings.legacyCanReach, (string) null, 0.0f, 1f);
    listingStandard.CheckboxLabeled("(Experimental) Cross map joy search.", ref this.settings.joyPatches, (string) null, 0.0f, 1f);
    listingStandard.CheckboxLabeled("(Experimental) Treat vehicle map as player home.", ref this.settings.treatAsPlayerHome, (string) null, 0.0f, 1f);
    TaggedString taggedString1 = Translator.Translate("VMF_Settings.ColonistBarMode");
    Rect rect1 = ((Listing) listingStandard).GetRect(Text.CalcHeight(TaggedString.op_Implicit(taggedString1), ((Listing) listingStandard).ColumnWidth * 0.5f), 1f);
    Widgets.Label(GenUI.LeftPart(rect1, 0.5f), taggedString1);
    VehicleMapSettings.ShowVehiclesOnColonistBar colonistBarMode = this.settings.colonistBarMode;
    this.settings.colonistBarMode = (VehicleMapSettings.ShowVehiclesOnColonistBar) Widgets.HorizontalSlider(GenUI.RightPart(rect1, 0.5f), (float) colonistBarMode, 0.0f, 2f, false, TaggedString.op_Implicit(Translator.Translate($"VMF_ColonistBarMode.{colonistBarMode}")), (string) null, (string) null, 1f);
    listingStandard.CheckboxLabeled("(Debug) Draw vehicle map grid.", ref this.settings.drawVehicleMapGrid, (string) null, 0.0f, 1f);
    ((Listing) listingStandard).End();
  }
}
