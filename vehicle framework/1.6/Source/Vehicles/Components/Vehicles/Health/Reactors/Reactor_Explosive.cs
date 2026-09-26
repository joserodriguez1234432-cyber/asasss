// Decompiled with JetBrains decompiler
// Type: Vehicles.Reactor_Explosive
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class Reactor_Explosive : Reactor, ITweakFields
{
  [TweakField(SettingsType = UISettingsType.SliderFloat)]
  [SliderValues(MinValue = 0.0f, MaxValue = 1f, Increment = 0.01f, RoundDecimalPlaces = 2)]
  public float chance = 1f;
  [TweakField(SettingsType = UISettingsType.SliderFloat)]
  [SliderValues(MinValue = 0.0f, MaxValue = 1f, Increment = 0.05f, RoundDecimalPlaces = 2)]
  [LoadAlias("maxHealth")]
  public float healthPercent = 1f;
  [TweakField(SettingsType = UISettingsType.IntegerBox)]
  public int damage = -1;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public float armorPenetration = -1f;
  [TweakField(SettingsType = UISettingsType.IntegerBox)]
  public int radius;
  public DamageDef damageDef;
  [TweakField(SettingsType = UISettingsType.IntegerBox)]
  public int wickTicks = 180;
  [TweakField]
  public DrawOffsets drawOffsets;

  string ITweakFields.Category => string.Empty;

  string ITweakFields.Label => nameof (Reactor_Explosive);

  public override void Hit(
    VehiclePawn vehicle,
    VehicleComponent component,
    ref DamageInfo dinfo,
    VehicleComponent.Penetration penetration)
  {
    if (!((Thing) vehicle).Spawned || (double) component.Health <= 0.0 || (double) component.HealthPercent > (double) this.healthPercent || !Rand.Chance(this.chance))
      return;
    this.SpawnExploder(vehicle, component);
  }

  protected virtual TimedExplosion CreateExploder(VehiclePawn vehicle, VehicleComponent component)
  {
    IntVec2 zero;
    if (!GenCollection.TryRandomElement<IntVec2>((IEnumerable<IntVec2>) component.props.hitbox.cells, ref zero))
      zero = IntVec2.Zero;
    TimedExplosion.Data data = new TimedExplosion.Data(zero, this.wickTicks, this.radius, this.damageDef, this.damage, this.armorPenetration);
    return new TimedExplosion(vehicle, data, this.drawOffsets);
  }

  internal virtual void SpawnExploder(VehiclePawn vehicle, VehicleComponent component)
  {
    TimedExplosion exploder = this.CreateExploder(vehicle, component);
    vehicle.AddTimedExplosion(exploder);
  }

  void ITweakFields.OnFieldChanged()
  {
  }
}
