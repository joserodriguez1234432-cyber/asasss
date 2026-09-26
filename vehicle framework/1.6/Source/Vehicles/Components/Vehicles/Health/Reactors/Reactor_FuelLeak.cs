// Decompiled with JetBrains decompiler
// Type: Vehicles.Reactor_FuelLeak
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class Reactor_FuelLeak : Reactor, ITweakFields
{
  [TweakField(SettingsType = UISettingsType.SliderFloat)]
  [SliderValues(MinValue = 0.0f, MaxValue = 1f, Increment = 0.05f, RoundDecimalPlaces = 2)]
  [LoadAlias("maxHealth")]
  public float healthPercent = 0.8f;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  [NumericBoxValues(MinValue = 0.0f)]
  public FloatRange rate = new FloatRange(1f, 10f);

  string ITweakFields.Category => string.Empty;

  string ITweakFields.Label => nameof (Reactor_FuelLeak);

  public override void Hit(
    VehiclePawn vehicle,
    VehicleComponent component,
    ref DamageInfo dinfo,
    VehicleComponent.Penetration penetration)
  {
  }

  void ITweakFields.OnFieldChanged()
  {
  }
}
