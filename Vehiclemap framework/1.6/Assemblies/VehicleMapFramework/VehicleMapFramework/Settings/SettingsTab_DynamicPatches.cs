// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Settings.SettingsTab_DynamicPatches
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using UnityEngine;
using VehicleMapFramework.VMF_HarmonyPatches;
using Verse;

#nullable disable
namespace VehicleMapFramework.Settings;

internal class SettingsTab_DynamicPatches : SettingsTabDrawer
{
  public override int Index => 1;

  public override string Label
  {
    get => TaggedString.op_Implicit(Translator.Translate("VMF_Settings.Tab.DynamicPatches"));
  }

  protected override void ResetSettings()
  {
    base.ResetSettings();
    this.settings.dynamicPatchEnabled = false;
    this.settings.dynamicUnpatchEnabled = false;
    this.settings.dynamicPatchLevel = Level.Safe;
    this.settings.roofedPatch = false;
    this.settings.debugToolPatches = false;
  }

  public override void Draw(Rect inRect)
  {
    base.Draw(inRect);
    Listing_Standard listingStandard = new Listing_Standard();
    ((Listing) listingStandard).Begin(inRect);
    listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VMF_Settings.EnableDynamicPatches")), ref this.settings.dynamicPatchEnabled, TaggedString.op_Implicit(Translator.Translate("VMF_Settings.EnableDynamicPatches.Tooltip")), 0.0f, 1f);
    if (this.settings.dynamicPatchEnabled)
    {
      listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VMF_Settings.EnableDynamicUnpatches")), ref this.settings.dynamicUnpatchEnabled, TaggedString.op_Implicit(Translator.Translate("VMF_Settings.EnableDynamicUnpatches.Tooltip")), 0.0f, 1f);
      TaggedString taggedString = Translator.Translate("VMF_Settings.DynamicPatchLevel");
      Rect rect1 = ((Listing) listingStandard).GetRect(Text.CalcHeight(TaggedString.op_Implicit(taggedString), ((Listing) listingStandard).ColumnWidth * 0.5f), 1f);
      Widgets.Label(GenUI.LeftPart(rect1, 0.5f), taggedString);
      Level dynamicPatchLevel = this.settings.dynamicPatchLevel;
      Rect rect2 = GenUI.RightPart(rect1, 0.5f);
      this.settings.dynamicPatchLevel = (Level) Widgets.HorizontalSlider(rect2, (float) dynamicPatchLevel, 1f, 3f, false, TaggedString.op_Implicit(Translator.Translate($"VMF_PatchLevel.{dynamicPatchLevel}")), (string) null, (string) null, 1f);
      TooltipHandler.TipRegion(rect2, TipSignal.op_Implicit(Translator.Translate($"VMF_PatchLevel.{dynamicPatchLevel}.Tooltip")));
    }
    listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VMF_Settings.RoofedPatch")), ref this.settings.roofedPatch, (string) null, 0.0f, 1f);
    listingStandard.CheckboxLabeled("(Debug) Enable debug tool patches", ref this.settings.debugToolPatches, (string) null, 0.0f, 1f);
    ((Listing) listingStandard).End();
  }
}
