// Decompiled with JetBrains decompiler
// Type: Vehicles.CompProperties_FueledTravel
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
[HeaderTitle(Label = "VF_FueledTravelPropertes", Translate = true)]
public class CompProperties_FueledTravel : VehicleCompProperties
{
  public ThingDef fuelType;
  public ThingDef leakDef;
  public bool electricPowered;
  [PostToSettings(Label = "VF_DischargePerTick", Tooltip = "VF_DischargePerTickTooltip", Translate = true, UISettingsType = UISettingsType.FloatBox)]
  [NumericBoxValues(MinValue = 0.0f)]
  [DisableSettingConditional(MemberType = typeof (CompProperties_FueledTravel), Property = "ElectricPowered", DisableIfEqualTo = false, DisableReason = "VF_NotElectricPowered")]
  public float dischargeRate = 2f;
  [PostToSettings(Label = "VF_TicksPerCharge", Tooltip = "VF_TicksPerCharge", Translate = true, UISettingsType = UISettingsType.FloatBox)]
  [NumericBoxValues(MinValue = 0.0f)]
  [DisableSettingConditional(MemberType = typeof (CompProperties_FueledTravel), Property = "ElectricPowered", DisableIfEqualTo = false, DisableReason = "VF_NotElectricPowered")]
  public float chargeRate = 1f;
  [PostToSettings(Label = "VF_FuelConsumptionRate", Tooltip = "VF_FuelConsumptionRateTooltip", Translate = true, UISettingsType = UISettingsType.FloatBox)]
  public float fuelConsumptionRate;
  [PostToSettings(Label = "VF_FuelCapacity", Tooltip = "VF_FuelCapacityTooltip", Translate = true, UISettingsType = UISettingsType.IntegerBox)]
  public int fuelCapacity;
  [PostToSettings(Label = "VF_FuelConsumptionRateWorldMultiplier", Tooltip = "VF_FuelConsumptionRateWorldMultiplierTooltip", Translate = true, UISettingsType = UISettingsType.SliderFloat)]
  [SliderValues(MinValue = 0.0f, MaxValue = 2f, RoundDecimalPlaces = 1)]
  public float fuelConsumptionWorldMultiplier = 1f;
  [PostToSettings(Label = "VF_AutoRefuelPercent", Tooltip = "VF_AutoRefuelPercentTooltip", Translate = true, UISettingsType = UISettingsType.SliderFloat)]
  [SliderValues(MinValue = 0.0f, MaxValue = 1f, RoundDecimalPlaces = 2)]
  public float autoRefuelPercent = 1f;
  [PostToSettings(Label = "VF_TargetFuelConfigurable", Tooltip = "VF_TargetFuelConfigurableTooltip", Translate = true, UISettingsType = UISettingsType.Checkbox)]
  public bool targetFuelLevelConfigurable = true;
  [PostToSettings(Label = "VF_AmbientHeat", Tooltip = "VF_AmbientHeatTooltip", Translate = true, UISettingsType = UISettingsType.FloatBox)]
  public float ambientHeat;
  [MustTranslate]
  public string gizmoLabel;
  public FuelConsumptionCondition fuelConsumptionCondition = FuelConsumptionCondition.All;
  public List<OffsetMote> motesGenerated;
  public ThingDef moteDisplayed;
  public int ticksToSpawnMote;
  public string fuelIconPath;
  private Texture2D fuelIcon;

  public CompProperties_FueledTravel() => this.compClass = typeof (CompFueledTravel);

  public bool ElectricPowered => this.electricPowered;

  public string GizmoLabel
  {
    get
    {
      return !GenText.NullOrEmpty(this.gizmoLabel) ? this.gizmoLabel : TaggedString.op_Implicit(this.electricPowered ? Translator.Translate("VF_Electricity") : Translator.Translate("Fuel"));
    }
  }

  public Texture2D FuelIcon
  {
    get
    {
      if (this.fuelIcon == null)
        this.fuelIcon = !GenText.NullOrEmpty(this.fuelIconPath) ? ContentFinder<Texture2D>.Get(this.fuelIconPath, true) : ((BuildableDef) this.fuelType).uiIcon;
      return this.fuelIcon;
    }
  }
}
