// Decompiled with JetBrains decompiler
// Type: Vehicles.Reactor_TurretPopper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using LudeonTK;
using RimWorld;
using SmashTools;
using SmashTools.Rendering;
using System;
using System.Collections.Generic;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class Reactor_TurretPopper : Reactor_Explosive, ITweakFields
{
  public string turretKey;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public float speed = 30f;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public FloatRange angle = new FloatRange(-5f, 5f);
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public FloatRange rotationRate = new FloatRange(-35f, 35f);

  string ITweakFields.Category => string.Empty;

  string ITweakFields.Label => "Reactor_Explosive";

  protected override TimedExplosion CreateExploder(VehiclePawn vehicle, VehicleComponent component)
  {
    TimedExplosion exploder = base.CreateExploder(vehicle, component);
    if (exploder == null)
      return (TimedExplosion) null;
    exploder.explosionCallback = new Action<TimedExplosion>(this.PopTurret);
    return exploder;
  }

  private void PopTurret(TimedExplosion explosion)
  {
    VehiclePawn vehicle = explosion.vehicle;
    VehicleTurret turret = vehicle.CompVehicleTurrets.GetTurret(this.turretKey);
    if (turret == null)
    {
      Log.Error($"Unable to find {this.turretKey} for turret pop.");
    }
    else
    {
      vehicle.statHandler.SetComponentHealth(turret.component.Component.props.key, 0.0f);
      FlyingObject flyingObject = (FlyingObject) ThingMaker.MakeThing(ThingDefOf_VehicleMotes.MoteLaunchedTurret, (ThingDef) null);
      flyingObject.Add((IParallelRenderer) turret, Rot8.North, turret.TurretRotation);
      foreach (VehicleTurret childTurret in turret.childTurrets)
        flyingObject.Add((IParallelRenderer) childTurret, Rot8.North, childTurret.TurretRotation);
      flyingObject.Launch(((Thing) vehicle).Map, ((Thing) vehicle).DrawPos, ((FloatRange) ref this.rotationRate).RandomInRange, this.speed, ((FloatRange) ref this.angle).RandomInRange);
    }
  }

  void ITweakFields.OnFieldChanged()
  {
  }

  [DebugAction("Vehicle Framework", "Pop Turret", false, false, false, false, false, 0, false)]
  private static void PopVehicleTurret(Pawn pawn)
  {
    if (!(pawn is VehiclePawn vehicle) || vehicle.CompVehicleTurrets == null)
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.ClickReject, (Map) null);
    }
    else
    {
      foreach (VehicleComponent component in vehicle.statHandler.components)
      {
        if (!GenList.NullOrEmpty<Reactor>((IList<Reactor>) component.props.reactors))
        {
          Reactor_TurretPopper reactorTurretPopper = (Reactor_TurretPopper) GenCollection.FirstOrDefault<Reactor>(component.props.reactors, (Predicate<Reactor>) (reactor => reactor is Reactor_TurretPopper));
          if (reactorTurretPopper != null)
          {
            reactorTurretPopper.SpawnExploder(vehicle, component);
            break;
          }
        }
      }
    }
  }
}
