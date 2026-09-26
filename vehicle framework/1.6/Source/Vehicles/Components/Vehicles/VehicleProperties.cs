// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleProperties
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
[VehicleSettingsClass]
[HeaderTitle(Label = "VF_Properties", Translate = true)]
public class VehicleProperties
{
  [PostToSettings(Label = "VF_FishingEnabled", Tooltip = "VF_FishingEnabledTooltip", Translate = true, UISettingsType = UISettingsType.Checkbox, VehicleType = VehicleType.Sea)]
  [DisableSettingConditional(MayRequireAny = new string[] {"ludeon.rimworld.odyssey", "VanillaExpanded.VCEF"})]
  [LoadAlias("fishing")]
  [FeatureEnabled("Fishing")]
  public bool canFish;
  public VehicleTrack track;
  [PostToSettings(Label = "VF_CollisionMultiplier", Tooltip = "VF_CollisionMultiplierTooltip", Translate = true, UISettingsType = UISettingsType.SliderFloat)]
  [SliderValues(MinValue = 0.0f, MaxValue = 2f, Increment = 0.05f, RoundDecimalPlaces = 2)]
  public float pawnCollisionMultiplier = 0.5f;
  [PostToSettings(Label = "VF_CollisionVehicleMultiplier", Tooltip = "VF_CollisionVehicleMultiplierTooltip", Translate = true, UISettingsType = UISettingsType.SliderFloat)]
  [SliderValues(MinValue = 0.0f, MaxValue = 2f, Increment = 0.05f, RoundDecimalPlaces = 2)]
  public float pawnCollisionRecoilMultiplier = 0.5f;
  public List<VehicleJobLimitations> vehicleJobLimitations = new List<VehicleJobLimitations>();
  public bool diagonalRotation = true;
  [PostToSettings(Label = "VF_ManhunterTargetsVehicle", Tooltip = "VF_ManhunterTargetsVehicleTooltip", Translate = true, UISettingsType = UISettingsType.Checkbox)]
  public bool manhunterTargetsVehicle;
  [PostToSettings(Label = "VF_CanAdaptToEMP", Tooltip = "VF_CanAdaptToEMPTooltip", Translate = true, UISettingsType = UISettingsType.Checkbox)]
  [DisableSettingConditional(MemberType = typeof (VehicleDef), Property = "CanDisableEMPSetting", DisableIfEqualTo = true, DisableReason = "VF_VehicleCannotStun")]
  [LoadAlias("canAdaptToEMP")]
  public bool canAdaptToEmp;
  public float visibilityWeight = 1f;
  [Unsaved(false)]
  [PostToSettings(Label = "VF_EMPStuns", Tooltip = "VF_EMPStunsTooltip", Translate = true, UISettingsType = UISettingsType.Checkbox)]
  [DisableSettingConditional(MemberType = typeof (VehicleDef), Property = "CanDisableEMPSetting", DisableIfEqualTo = true, DisableReason = "VF_VehicleCannotStun")]
  public bool empStuns;
  public string iconTexPath;
  public SimpleDictionary<DamageDef, float> damageDefMultipliers;
  public DefaultImpassable defaultImpassable;
  public SimpleDictionary<WeatherBuildupCategory, int> customWeatherCosts;
  public SimpleDictionary<TerrainDef, int> customTerrainCosts;
  public SimpleDictionary<ThingDef, int> customThingCosts;
  [PostToSettings(Label = "VF_OffRoadMultiplier", Tooltip = "VF_OffRoadMultiplierTooltip", Translate = true, UISettingsType = UISettingsType.SliderFloat)]
  [SliderValues(MinValue = 0.01f, MaxValue = 2f, RoundDecimalPlaces = 1)]
  public float offRoadMultiplier = 1f;
  public float riverCost = -1f;
  public SimpleDictionary<RiverDef, float> customRiverCosts;
  public SimpleDictionary<BiomeDef, float> customBiomeCosts;
  public SimpleDictionary<Hilliness, float> customHillinessCosts;
  public SimpleDictionary<RoadDef, float> customRoadCosts;
  [PostToSettings(Label = "VF_WinterSpeedMultiplier", Tooltip = "VF_WinterSpeedMultiplierTooltip", Translate = true, UISettingsType = UISettingsType.SliderFloat)]
  [SliderValues(MinValue = 0.0f, MaxValue = 10f, RoundDecimalPlaces = 1)]
  [LoadAlias("winterSpeedMultiplier")]
  [LoadAlias("winterCostMultiplier")]
  public float winterCost = 2f;
  [PostToSettings(Label = "VF_WorldSpeedMultiplier", Tooltip = "VF_WorldSpeedMultiplierTooltip", Translate = true, UISettingsType = UISettingsType.SliderFloat)]
  [SliderValues(MinValue = 0.0f, MaxValue = 10f, RoundDecimalPlaces = 1)]
  public float worldSpeedMultiplier = 2.5f;
  public List<FactionDef> restrictToFactions;
  [TweakField]
  public List<VehicleRole> roles = new List<VehicleRole>();

  public int TotalSeats
  {
    get
    {
      int totalSeats = 0;
      foreach (VehicleRole role in this.roles)
        totalSeats += role.Slots;
      return totalSeats;
    }
  }

  public int RoleSeats(HandlingType handlingType)
  {
    int num = 0;
    foreach (VehicleRole role in this.roles)
    {
      if ((role.HandlingTypes & handlingType) == handlingType)
        num += role.Slots;
    }
    return num;
  }

  public int RoleSeatsToOperate(HandlingType handlingType)
  {
    int operate = 0;
    foreach (VehicleRole role in this.roles)
    {
      if ((role.HandlingTypes & handlingType) == handlingType)
        operate += role.SlotsToOperate;
    }
    return operate;
  }

  public IEnumerable<string> ConfigErrors(VehicleDef vehicleDef)
  {
    yield break;
  }

  public void ResolveReferences(VehicleDef vehicleDef)
  {
    if (this.vehicleJobLimitations == null)
      this.vehicleJobLimitations = new List<VehicleJobLimitations>();
    if (this.customRiverCosts == null)
      this.customRiverCosts = new SimpleDictionary<RiverDef, float>();
    if (this.customBiomeCosts == null)
      this.customBiomeCosts = new SimpleDictionary<BiomeDef, float>();
    if (this.customHillinessCosts == null)
      this.customHillinessCosts = new SimpleDictionary<Hilliness, float>();
    if (this.customRoadCosts == null)
      this.customRoadCosts = new SimpleDictionary<RoadDef, float>();
    if (this.customTerrainCosts == null)
      this.customTerrainCosts = new SimpleDictionary<TerrainDef, int>();
    if (this.customThingCosts == null)
      this.customThingCosts = new SimpleDictionary<ThingDef, int>();
    if (this.customWeatherCosts == null)
      this.customWeatherCosts = new SimpleDictionary<WeatherBuildupCategory, int>();
    if ((double) this.riverCost > 0.0)
    {
      float num = (float) ((BuildableDef) vehicleDef).Size.x * Ext_Math.Sqrt2;
      foreach (RiverDef riverDef in DefDatabase<RiverDef>.AllDefsListForReading)
      {
        if (!this.customRiverCosts.ContainsKey(riverDef) && (double) ModSettingsHelper.RiverSizeWithMultiplier(riverDef) >= (double) num)
          this.customRiverCosts[riverDef] = this.riverCost;
      }
    }
    if (!GenList.NullOrEmpty<VehicleRole>((IList<VehicleRole>) this.roles))
    {
      foreach (VehicleRole role in this.roles)
        role.ResolveReferences(vehicleDef);
    }
    this.empStuns = SettingsCache.TryGetValue<bool>(vehicleDef, typeof (VehicleProperties), "empStuns", vehicleDef.components.NotNullAndAny<VehicleComponentProperties>((Predicate<VehicleComponentProperties>) (props => props.empSeverity > VehicleEMPSeverity.None)));
  }

  public void PostDefDatabase(VehicleDef vehicleDef)
  {
    string defName = ((Def) vehicleDef).defName;
    XmlHelper.FillDefaults_Enum<WeatherBuildupCategory, int>(defName, "customWeatherCosts", (Dictionary<WeatherBuildupCategory, int>) this.customWeatherCosts);
    XmlHelper.FillDefaults_Def<TerrainDef, int>(defName, "customTerrainCosts", (Dictionary<TerrainDef, int>) this.customTerrainCosts);
    XmlHelper.FillDefaults_Def<ThingDef, int>(defName, "customThingCosts", (Dictionary<ThingDef, int>) this.customThingCosts);
    XmlHelper.FillDefaults_Def<RiverDef, float>(defName, "customRiverCosts", (Dictionary<RiverDef, float>) this.customRiverCosts);
    XmlHelper.FillDefaults_Def<BiomeDef, float>(defName, "customBiomeCosts", (Dictionary<BiomeDef, float>) this.customBiomeCosts);
    XmlHelper.FillDefaults_Enum<Hilliness, float>(defName, "customHillinessCosts", (Dictionary<Hilliness, float>) this.customHillinessCosts);
    XmlHelper.FillDefaults_Def<RoadDef, float>(defName, "customRoadCosts", (Dictionary<RoadDef, float>) this.customRoadCosts);
  }
}
