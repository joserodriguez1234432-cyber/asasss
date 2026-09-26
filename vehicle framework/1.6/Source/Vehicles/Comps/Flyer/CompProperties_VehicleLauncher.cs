// Decompiled with JetBrains decompiler
// Type: Vehicles.CompProperties_VehicleLauncher
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

[HeaderTitle(Label = "VF_LauncherProperties", Translate = true)]
public class CompProperties_VehicleLauncher : VehicleCompProperties
{
  [PostToSettings(Label = "VF_FuelConsumptionWorld", Translate = true, Tooltip = "VF_FuelConsumptionWorldTooltip", UISettingsType = UISettingsType.FloatBox, VehicleType = VehicleType.Air)]
  [NumericBoxValues(MinValue = 0.0f)]
  public float fuelConsumptionWorldMultiplier = 10f;
  [SliderValues(MinValue = 1f, MaxValue = 200f, EndValue = 999999f, RoundDecimalPlaces = 0, Increment = 1f, MaxValueDisplay = "VF_Instant")]
  [Unsaved(false)]
  public float rateOfClimb = 10f;
  [SliderValues(MinValue = 1f, MaxValue = 101f, EndValue = 100000f, RoundDecimalPlaces = 1, Increment = 1f, MaxValueDisplay = "VF_Instant")]
  [Unsaved(false)]
  public float maxFallRate = 20f;
  [NumericBoxValues(MinValue = 0.0f, MaxValue = 210000f)]
  [Unsaved(false)]
  public int maxAltitude = 10000;
  [NumericBoxValues(MinValue = 0.0f, MaxValue = 210000f)]
  [Unsaved(false)]
  public int landingAltitude = 1000;
  public int reconDistance = 1;
  [PostToSettings(Label = "VF_LaunchFixedMaxDistance", Translate = true, Tooltip = "VF_LaunchFixedMaxDistanceTooltip", UISettingsType = UISettingsType.SliderInt, VehicleType = VehicleType.Air)]
  [SliderValues(MinValue = 0.0f, MaxValue = 100f, MinValueDisplay = "VF_LaunchFixedMaxDistanceDisabled")]
  public int fixedLaunchDistanceMax;
  [PostToSettings(Label = "VF_ControlInFlight", Translate = true, Tooltip = "VF_ControlInFlightTooltip", UISettingsType = UISettingsType.Checkbox, VehicleType = VehicleType.Air)]
  public bool controlInFlight = true;
  [PostToSettings(Label = "VF_CanRoofPunch", Tooltip = "VF_CanRoofPunchTooltip", Translate = true, UISettingsType = UISettingsType.Checkbox)]
  public bool canRoofPunch;
  public float animationPunchAt = 0.95f;
  [PostToSettings(Label = "VF_SpaceFlight", Translate = true, Tooltip = "VF_SpaceFlightTooltip", UISettingsType = UISettingsType.Checkbox, VehicleType = VehicleType.Air)]
  [DisableSettingConditional(MayRequireAny = new string[] {"kentington.saveourship2", "sindre0830.rimnauts2", "sindre0830.universum", "ludeon.rimworld.odyssey"})]
  public bool spaceFlight;
  [PostToSettings(Label = "VF_SignalJamming", Translate = true, Tooltip = "VF_SignalJammingTooltip", UISettingsType = UISettingsType.Checkbox, VehicleType = VehicleType.Air)]
  [DisableSettingConditional(MemberType = typeof (CompProperties_VehicleLauncher), Field = "spaceFlight", DisableIfEqualTo = false)]
  [DisableSettingConditional(MayRequire = "ludeon.rimworld.odyssey")]
  public bool signalJammer;
  public bool faceDirectionOfTravel = true;
  public bool circleToLand = true;
  public int deployTicks;
  public string shadow = "Things/Skyfaller/SkyfallerShadowCircle";
  public BomberProperties bombing;
  public StrafingProperties strafing;
  public LaunchProtocol launchProtocol;
  public ThingDef skyfallerLeaving;
  public ThingDef skyfallerIncoming;
  public ThingDef skyfallerCrashing;
  public ThingDef skyfallerStrafing;
  public ThingDef skyfallerBombing;

  public CompProperties_VehicleLauncher() => this.compClass = typeof (CompVehicleLauncher);

  public virtual IEnumerable<string> ConfigErrors(ThingDef parentDef)
  {
    foreach (string configError in base.ConfigErrors(parentDef))
      yield return configError;
  }

  public override IEnumerable<VehicleStatDef> StatCategoryDefs()
  {
    yield return VehicleStatDefOf.FlightControl;
    yield return VehicleStatDefOf.FlightSpeed;
  }
}
