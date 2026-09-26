// Decompiled with JetBrains decompiler
// Type: Vehicles.SectionMain
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System;
using UnityEngine;
using Vehicles.Compatibility;
using Vehicles.Config;
using Verse;

#nullable disable
namespace Vehicles;

public class SectionMain : SettingsSection
{
  private const int MainSectionColumns = 3;
  private const int MaxSettlementAdjustRadius = 15;
  private const int MaxRoadAdjustRadius = 4;
  public const float BeachMultMin = 0.0f;
  public const float BeachMultMax = 2f;
  public const float RiverMultMin = 0.0f;
  public const float RiverMultMax = 2f;
  private const float MinFishingMultiplier = 1f;
  private const float MaxFishingMultiplier = 5f;
  private const float DefaultFishingMultiplier = 1f;
  private const float MinFishingSkillGain = 0.0f;
  private const float MaxFishingSkillGain = 0.25f;
  private const float DefaultFishingSkillGain = 0.025f;
  private const int DefaultSettlementAdjustRadius = 1;
  private const float DefaultCoastWeight = 1f;
  private const float DefaultRiverWeight = 1f;
  public float beachMultiplier;
  public float riverMultiplier;
  public int adjustSettlementRadius = 1;
  public float adjustCoastWeight = 1f;
  public float adjustRiverWeight = 1f;
  public bool modifiableSettings = true;
  public bool useCustomShaders = true;
  public bool allowDiagonalRendering = true;
  public bool fullVehiclePathing = true;
  public bool smoothVehiclePaths = true;
  public bool ignoreBiomeCostOnRoads = true;
  public bool multiplePawnsPerJob = true;
  public bool passiveWaterWaves = true;
  public bool aerialVehicleEffects = true;
  public bool overheatMechanics = true;
  public float fishingMultiplier = 1f;
  public float fishingSkillIncrease = 0.05f;
  public bool drawLandingGhost;
  public bool burnRadiusOnRockets = true;
  public bool deployOnLanding = true;
  public float delayDeployOnLanding;
  public bool airDefenses = true;
  public float meleeDamageMultiplier = 1f;
  public float rangedDamageMultiplier = 1f;
  public float explosiveDamageMultiplier = 1f;
  public bool reduceExplosionsOnWater = true;
  public bool runOverPawns = true;
  public VehicleTracksFriendlyFire friendlyFire;
  public float friendlyFireChance = 0.5f;
  private string inputBufferCoast;
  private string inputBufferRiver;

  public override void ResetSettings()
  {
    base.ResetSettings();
    this.inputBufferCoast = (string) null;
    this.inputBufferRiver = (string) null;
    this.beachMultiplier = 0.0f;
    this.riverMultiplier = 0.0f;
    this.adjustSettlementRadius = 1;
    this.adjustCoastWeight = 1f;
    this.adjustRiverWeight = 1f;
    this.modifiableSettings = true;
    this.useCustomShaders = true;
    this.allowDiagonalRendering = true;
    this.fullVehiclePathing = true;
    this.smoothVehiclePaths = true;
    this.ignoreBiomeCostOnRoads = true;
    this.multiplePawnsPerJob = true;
    this.meleeDamageMultiplier = 1f;
    this.rangedDamageMultiplier = 1f;
    this.explosiveDamageMultiplier = 1f;
    this.overheatMechanics = true;
    this.passiveWaterWaves = true;
    this.aerialVehicleEffects = true;
    this.fishingMultiplier = 1f;
    this.fishingSkillIncrease = 0.05f;
    this.drawLandingGhost = false;
    this.burnRadiusOnRockets = true;
    this.deployOnLanding = true;
    this.airDefenses = true;
    this.delayDeployOnLanding = 0.0f;
    this.reduceExplosionsOnWater = true;
    this.runOverPawns = true;
    this.friendlyFire = VehicleTracksFriendlyFire.None;
    this.friendlyFireChance = 0.5f;
  }

  public override void ExposeData()
  {
    Scribe_Values.Look<float>(ref this.beachMultiplier, "beachMultiplier", 0.0f, false);
    Scribe_Values.Look<float>(ref this.riverMultiplier, "riverMultiplier", 0.0f, false);
    Scribe_Values.Look<int>(ref this.adjustSettlementRadius, "adjustSettlementRadius", 1, false);
    Scribe_Values.Look<float>(ref this.adjustCoastWeight, "adjustCoastWeight", 1f, false);
    Scribe_Values.Look<float>(ref this.adjustRiverWeight, "adjustRiverWeight", 1f, false);
    Scribe_Values.Look<bool>(ref this.modifiableSettings, "modifiableSettings", true, false);
    Scribe_Values.Look<bool>(ref this.useCustomShaders, "useCustomShaders", true, false);
    Scribe_Values.Look<bool>(ref this.allowDiagonalRendering, "allowDiagonalRendering", true, false);
    Scribe_Values.Look<bool>(ref this.fullVehiclePathing, "fullVehiclePathing", true, false);
    Scribe_Values.Look<bool>(ref this.smoothVehiclePaths, "smoothVehiclePaths", true, false);
    Scribe_Values.Look<bool>(ref this.ignoreBiomeCostOnRoads, "ignoreBiomeCostOnRoads", true, false);
    Scribe_Values.Look<bool>(ref this.multiplePawnsPerJob, "multiplePawnsPerJob", true, false);
    Scribe_Values.Look<float>(ref this.meleeDamageMultiplier, "meleeDamageMultiplier", 1f, false);
    Scribe_Values.Look<float>(ref this.rangedDamageMultiplier, "rangedDamageMultiplier", 1f, false);
    Scribe_Values.Look<float>(ref this.explosiveDamageMultiplier, "explosiveDamageMultiplier", 1f, false);
    Scribe_Values.Look<bool>(ref this.overheatMechanics, "overheatMechanics", true, false);
    Scribe_Values.Look<bool>(ref this.passiveWaterWaves, "passiveWaterWaves", true, false);
    Scribe_Values.Look<bool>(ref this.aerialVehicleEffects, "aerialVehicleEffects", true, false);
    Scribe_Values.Look<float>(ref this.fishingMultiplier, "fishingMultiplier", 1f, false);
    Scribe_Values.Look<float>(ref this.fishingSkillIncrease, "fishingSkillIncrease", 0.025f, false);
    Scribe_Values.Look<bool>(ref this.drawLandingGhost, "drawLandingGhost", false, false);
    Scribe_Values.Look<bool>(ref this.burnRadiusOnRockets, "burnRadiusOnRockets", true, false);
    Scribe_Values.Look<bool>(ref this.deployOnLanding, "deployOnLanding", true, false);
    Scribe_Values.Look<bool>(ref this.airDefenses, "airDefenses", true, false);
    Scribe_Values.Look<float>(ref this.delayDeployOnLanding, "delayDeployOnLanding", 0.0f, false);
    Scribe_Values.Look<bool>(ref this.reduceExplosionsOnWater, "reduceExplosionsOnWater", true, false);
    Scribe_Values.Look<bool>(ref this.runOverPawns, "runOverPawns", true, false);
    Scribe_Values.Look<VehicleTracksFriendlyFire>(ref this.friendlyFire, "friendlyFire", VehicleTracksFriendlyFire.None, false);
    Scribe_Values.Look<float>(ref this.friendlyFireChance, "friendlyFireChance", 0.5f, false);
  }

  public override void OnGUI(Rect rect)
  {
    SettingsSection.listingStandard = new Listing_Standard();
    Rect rect1 = GenUI.ContractedBy(rect, 10f);
    float num = 27f;
    ref Rect local1 = ref rect1;
    ((Rect) ref local1).y = ((Rect) ref local1).y + num;
    ref Rect local2 = ref rect1;
    ((Rect) ref local2).height = ((Rect) ref local2).height - num;
    ((Listing) SettingsSection.listingStandard).ColumnWidth = (float) ((double) ((Rect) ref rect1).width / 3.0 - 12.0);
    ((Listing) SettingsSection.listingStandard).Begin(rect1);
    ((Listing) SettingsSection.listingStandard).Header(TaggedString.op_Implicit(Translator.Translate("VF_WorldMapGen")), ListingExtension.BannerColor, (GameFont) 1, (TextAnchor) 4);
    ((Listing) SettingsSection.listingStandard).Gap(4f);
    ((Listing) SettingsSection.listingStandard).SliderLabeled(TaggedString.op_Implicit(Translator.Translate("VF_BeachGenMultiplier")), TaggedString.op_Implicit(Translator.Translate("VF_BeachGenMultiplierTooltip")), "+", "%", ref this.beachMultiplier, 0.0f, 2f, 100f, 0);
    ((Listing) SettingsSection.listingStandard).SliderLabeled(TaggedString.op_Implicit(Translator.Translate("VF_RiverGenMultiplier")), TaggedString.op_Implicit(Translator.Translate("VF_RiverGenMultiplierTooltip")), "+", "%", ref this.riverMultiplier, 0.0f, 2f, 100f, 0);
    ((Listing) SettingsSection.listingStandard).SliderLabeled(TaggedString.op_Implicit(Translator.Translate("VF_AdjustSettlementRadius")), TaggedString.op_Implicit(Translator.Translate("VF_AdjustSettlementRadiusTooltip")), $" {Translator.Translate("VF_WorldTiles")}", ref this.adjustSettlementRadius, 0, 15);
    if (this.adjustSettlementRadius > 0)
    {
      SettingsSection.listingStandard.TextFieldNumericLabeled<float>(TaggedString.op_Implicit(Translator.Translate("VF_AdjustCoastWeight")), ref this.adjustCoastWeight, ref this.inputBufferCoast, 0.0f, 1E+09f);
      SettingsSection.listingStandard.TextFieldNumericLabeled<float>(TaggedString.op_Implicit(Translator.Translate("VF_AdjustRiverWeight")), ref this.adjustRiverWeight, ref this.inputBufferRiver, 0.0f, 1E+09f);
    }
    ((Listing) SettingsSection.listingStandard).Gap(8f);
    ((Listing) SettingsSection.listingStandard).Header(TaggedString.op_Implicit(Translator.Translate("VF_SettingsGeneral")), ListingExtension.BannerColor, (GameFont) 1, (TextAnchor) 4);
    ((Listing) SettingsSection.listingStandard).Gap(4f);
    SettingsSection.listingStandard.CheckboxLabeledWithMessage(TaggedString.op_Implicit(Translator.Translate("VF_ModifiableSettings")), (Func<bool, Message>) (_param1 => new Message(TaggedString.op_Implicit(Translator.Translate("VF_WillRequireRestart")), MessageTypeDefOf.CautionInput)), ref this.modifiableSettings, TaggedString.op_Implicit(Translator.Translate("VF_ModifiableSettingsTooltip")));
    SettingsSection.listingStandard.CheckboxLabeledWithMessage(TaggedString.op_Implicit(Translator.Translate("VF_CustomShaders")), (Func<bool, Message>) (_param1 => new Message(TaggedString.op_Implicit(Translator.Translate("VF_WillRequireRestart")), MessageTypeDefOf.CautionInput)), ref this.useCustomShaders, TaggedString.op_Implicit(Translator.Translate("VF_CustomShadersTooltip")));
    SettingsSection.listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VF_DiagonalVehicleRendering")), ref this.allowDiagonalRendering, TaggedString.op_Implicit(Translator.Translate("VF_DiagonalVehicleRenderingTooltip")), 0.0f, 1f);
    SettingsSection.listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VF_FullVehiclePathing")), ref this.fullVehiclePathing, TaggedString.op_Implicit(Translator.Translate("VF_FullVehiclePathingTooltip")), 0.0f, 1f);
    SettingsSection.listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VF_SmoothVehiclePathing")), ref this.smoothVehiclePaths, TaggedString.op_Implicit(Translator.Translate("VF_SmoothVehiclePathingTooltip")), 0.0f, 1f);
    SettingsSection.listingStandard.CheckboxLabeledWithMessage(TaggedString.op_Implicit(Translator.Translate("VF_RoadBiomeCostPathing")), (Func<bool, Message>) (_param1 => new Message(TaggedString.op_Implicit(Translator.Translate("VF_WillRequireRestart")), MessageTypeDefOf.CautionInput)), ref this.ignoreBiomeCostOnRoads, TaggedString.op_Implicit(Translator.Translate("VF_RoadBiomeCostPathingTooltip")));
    SettingsSection.listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VF_MultiplePawnsPerJob")), ref this.multiplePawnsPerJob, TaggedString.op_Implicit(Translator.Translate("VF_MultiplePawnsPerJobTooltip")), 0.0f, 1f);
    ((Listing) SettingsSection.listingStandard).Gap(8f);
    ((Listing) SettingsSection.listingStandard).Header(TaggedString.op_Implicit(Translator.Translate("VF_GraphicsSettings")), ListingExtension.BannerColor, (GameFont) 1, (TextAnchor) 4);
    SettingsSection.listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VF_PassiveWaterWaves")), ref this.passiveWaterWaves, TaggedString.op_Implicit(Translator.Translate("VF_PassiveWaterWavesTooltip")), 0.0f, 1f);
    SettingsSection.listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VF_AerialVehicleEffects")), ref this.aerialVehicleEffects, TaggedString.op_Implicit(Translator.Translate("VF_AerialVehicleEffectsTooltip")), 0.0f, 1f);
    ((Listing) SettingsSection.listingStandard).Gap(8f);
    ((Listing) SettingsSection.listingStandard).NewColumn();
    if (FeatureFlags.FishingEnabled)
    {
      string header = TaggedString.op_Implicit(Translator.Translate("VF_Fishing"));
      if (!FishingCompatibility.Active)
      {
        GUIState.Disable();
        header = TaggedString.op_Implicit(Translator.Translate("VF_FishingInactive"));
      }
      ((Listing) SettingsSection.listingStandard).Header(header, ListingExtension.BannerColor, (GameFont) 1, (TextAnchor) 4);
      ((Listing) SettingsSection.listingStandard).SliderLabeled(TaggedString.op_Implicit(Translator.Translate("VF_FishingMultiplier")), TaggedString.op_Implicit(Translator.Translate("VF_FishingMultiplierTooltip")), "%", ref this.fishingMultiplier, 1f, 5f, 100f);
      ((Listing) SettingsSection.listingStandard).Gap(8f);
      ((Listing) SettingsSection.listingStandard).SliderLabeled(TaggedString.op_Implicit(Translator.Translate("VF_FishingSkill")), TaggedString.op_Implicit(Translator.Translate("VF_FishingSkillTooltip")), "%", ref this.fishingSkillIncrease, 0.0f, 0.25f, 100f);
      ((Listing) SettingsSection.listingStandard).Gap(8f);
      GUIState.Enable();
    }
    ((Listing) SettingsSection.listingStandard).Header(TaggedString.op_Implicit(Translator.Translate("VF_AerialVehicles")), ListingExtension.BannerColor, (GameFont) 1, (TextAnchor) 4);
    SettingsSection.listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VF_DrawLandingGhost")), ref this.drawLandingGhost, TaggedString.op_Implicit(Translator.Translate("VF_DrawLandingGhostTooltip")), 0.0f, 1f);
    SettingsSection.listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VF_RocketsBurnRadius")), ref this.burnRadiusOnRockets, TaggedString.op_Implicit(Translator.Translate("VF_RocketsBurnRadiusTooltip")), 0.0f, 1f);
    SettingsSection.listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VF_DeployOnLanding")), ref this.deployOnLanding, TaggedString.op_Implicit(Translator.Translate("VF_DeployOnLandingTooltip")), 0.0f, 1f);
    if (this.deployOnLanding)
    {
      ((Listing) SettingsSection.listingStandard).Gap(16f);
      ((Listing) SettingsSection.listingStandard).SliderLabeled(TaggedString.op_Implicit(Translator.Translate("VF_DelayOnLanding")), TaggedString.op_Implicit(Translator.Translate("VF_DelayOnLandingTooltip")), $" {Translator.Translate("VF_DelaySeconds")}", ref this.delayDeployOnLanding, 0.0f, 5f, decimalPlaces: 1);
    }
    ((Listing) SettingsSection.listingStandard).Gap(8f);
    ((Listing) SettingsSection.listingStandard).Header(TaggedString.op_Implicit(Translator.Translate("VF_CombatSettings")), ListingExtension.BannerColor, (GameFont) 1, (TextAnchor) 4);
    ((Listing) SettingsSection.listingStandard).Gap(4f);
    ((Listing) SettingsSection.listingStandard).SliderLabeled(TaggedString.op_Implicit(Translator.Translate("VF_MeleeDamageMultiplier")), string.Empty, "%", ref this.meleeDamageMultiplier, 0.0f, 2f, 100f, 0);
    ((Listing) SettingsSection.listingStandard).SliderLabeled(TaggedString.op_Implicit(Translator.Translate("VF_RangedDamageMultiplier")), string.Empty, "%", ref this.rangedDamageMultiplier, 0.0f, 2f, 100f, 0);
    ((Listing) SettingsSection.listingStandard).SliderLabeled(TaggedString.op_Implicit(Translator.Translate("VF_ExplosiveDamageMultiplier")), string.Empty, "%", ref this.explosiveDamageMultiplier, 0.0f, 2f, 100f, 0);
    SettingsSection.listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VF_ShellExplosionsOnWater")), ref this.reduceExplosionsOnWater, TaggedString.op_Implicit(Translator.Translate("VF_ShellExplosionsOnWaterTooltip")), 0.0f, 1f);
    SettingsSection.listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VF_RunOverPawns")), ref this.runOverPawns, TaggedString.op_Implicit(Translator.Translate("VF_RunOverPawnsTooltip")), 0.0f, 1f);
    if (this.runOverPawns)
    {
      ((Listing) SettingsSection.listingStandard).EnumSliderLabeled<VehicleTracksFriendlyFire>(TaggedString.op_Implicit(Translator.Translate("VF_ChanceToRunOverFriendlies")), ref this.friendlyFire, TaggedString.op_Implicit(Translator.Translate("VF_ChanceToRunOverFriendliesTooltip")), string.Empty, (Func<VehicleTracksFriendlyFire, string>) (friendlyFire =>
      {
        string str;
        switch (friendlyFire)
        {
          case VehicleTracksFriendlyFire.None:
            str = TaggedString.op_Implicit(Translator.Translate("VF_VehicleTracksNone"));
            break;
          case VehicleTracksFriendlyFire.Vanilla:
            str = TaggedString.op_Implicit(Translator.Translate("VF_VehicleTracksVanilla"));
            break;
          case VehicleTracksFriendlyFire.Custom:
            str = TaggedString.op_Implicit(Translator.Translate("ScenariosCustom"));
            break;
          default:
            str = friendlyFire.ToString();
            break;
        }
        return str;
      }));
      if (this.friendlyFire == VehicleTracksFriendlyFire.Custom)
        ((Listing) SettingsSection.listingStandard).SliderLabeled(TaggedString.op_Implicit(Translator.Translate("VF_ChanceToRunOverFriendlies")), TaggedString.op_Implicit(Translator.Translate("VF_ChanceToRunOverFriendliesTooltip")), "%", ref this.friendlyFireChance, 0.0f, 1f, 100f, 0);
    }
    ((Listing) SettingsSection.listingStandard).Gap(8f);
    ((Listing) SettingsSection.listingStandard).Header(TaggedString.op_Implicit(Translator.Translate("VF_VehicleTurrets")), ListingExtension.BannerColor, (GameFont) 1, (TextAnchor) 4);
    SettingsSection.listingStandard.CheckboxLabeled(TaggedString.op_Implicit(Translator.Translate("VF_TurretOverheatMechanics")), ref this.overheatMechanics, TaggedString.op_Implicit(Translator.Translate("VF_TurretOverheatMechanicsTooltip")), 0.0f, 1f);
    ((Listing) SettingsSection.listingStandard).Gap(4f);
    ((Listing) SettingsSection.listingStandard).End();
  }
}
