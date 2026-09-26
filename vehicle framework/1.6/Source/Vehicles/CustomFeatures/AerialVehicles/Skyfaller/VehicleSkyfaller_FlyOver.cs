// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleSkyfaller_FlyOver
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using System;
using UnityEngine;
using Vehicles.World;
using Verse;

#nullable disable
namespace Vehicles;

public abstract class VehicleSkyfaller_FlyOver : VehicleSkyfaller
{
  public IntVec3 start;
  public IntVec3 end;
  public AerialVehicleInFlight aerialVehicle;

  [UsedImplicitly]
  [Obsolete("Implemented for Xml Deserialization only. Use VehicleSkyfallerMaker instead.", true)]
  public VehicleSkyfaller_FlyOver()
  {
  }

  public override Vector3 DrawPos
  {
    get
    {
      switch ((int) this.def.skyfaller.movementType)
      {
        case 0:
          return SkyfallerHelper.DrawPos_Accelerate(base.DrawPos, this.vehicle.CompVehicleLauncher.launchProtocol.TicksPassed, this.angle, this.CurrentSpeed);
        case 1:
          return SkyfallerHelper.DrawPos_ConstantSpeed(base.DrawPos, this.vehicle.CompVehicleLauncher.launchProtocol.TicksPassed, this.angle, this.CurrentSpeed);
        case 2:
          return SkyfallerHelper.DrawPos_Decelerate(base.DrawPos, this.vehicle.CompVehicleLauncher.launchProtocol.TicksPassed, this.angle, this.CurrentSpeed);
        default:
          Log.ErrorOnce("SkyfallerMovementType not handled: " + this.def.skyfaller.movementType.ToString(), this.thingIDNumber);
          return SkyfallerHelper.DrawPos_Accelerate(base.DrawPos, this.vehicle.CompVehicleLauncher.launchProtocol.TicksPassed, this.angle, this.CurrentSpeed);
      }
    }
  }

  protected virtual float CurrentSpeed
  {
    get
    {
      return this.def.skyfaller.speedCurve == null ? this.def.skyfaller.speed : this.def.skyfaller.speedCurve.Evaluate(this.TimeInAnimation) * this.def.skyfaller.speed;
    }
  }

  protected virtual float TimeInAnimation
  {
    get
    {
      return this.def.skyfaller.reversed ? (float) this.vehicle.CompVehicleLauncher.launchProtocol.TicksPassed / (float) this.def.skyfaller.ticksToImpactRange.max : (float) (1.0 - (double) this.vehicle.CompVehicleLauncher.launchProtocol.TicksPassed / (double) this.def.skyfaller.ticksToImpactRange.max);
    }
  }

  public Vector3 DistanceAtMin
  {
    get
    {
      return SkyfallerHelper.DrawPos_ConstantSpeed(base.DrawPos, -this.def.skyfaller.ticksToImpactRange.min, this.angle, this.CurrentSpeed);
    }
  }

  protected virtual void DrawAt(Vector3 drawLoc, bool flip = false)
  {
    float num1 = 0.0f;
    if (this.def.skyfaller.rotateGraphicTowardsDirection)
      num1 = this.angle;
    if (this.def.skyfaller.angleCurve != null)
      this.angle = this.def.skyfaller.angleCurve.Evaluate(this.TimeInAnimation);
    if (this.def.skyfaller.rotationCurve != null)
      num1 += this.def.skyfaller.rotationCurve.Evaluate(this.TimeInAnimation);
    if (this.def.skyfaller.xPositionCurve != null)
      drawLoc.x += this.def.skyfaller.xPositionCurve.Evaluate(this.TimeInAnimation);
    if (this.def.skyfaller.zPositionCurve != null)
      drawLoc.z += this.def.skyfaller.zPositionCurve.Evaluate(this.TimeInAnimation);
    VehiclePawn vehicle = this.vehicle;
    ref Vector3 local = ref drawLoc;
    Rot8 rotation1 = (Rot8) this.Rotation;
    double num2 = (double) num1;
    Rot4 rotation2 = this.Rotation;
    double num3 = (double) (((Rot4) ref rotation2).AsInt * 90);
    double rotation3 = num2 + num3;
    vehicle.DrawAt(in local, rotation1, (float) rotation3);
  }

  protected override void Tick()
  {
    if (GenGrid.InBounds(base.DrawPos, this.Map) || this.vehicle.CompVehicleLauncher.launchProtocol.TicksPassed <= this.def.skyfaller.ticksToImpactRange.max)
      return;
    this.ExitMap();
  }

  protected virtual void ExitMap()
  {
    this.Destroy((DestroyMode) 0);
    throw new NotImplementedException("TODO");
  }

  public override void ExposeData()
  {
    base.ExposeData();
    Scribe_Values.Look<IntVec3>(ref this.start, "start", new IntVec3(), false);
    Scribe_Values.Look<IntVec3>(ref this.end, "end", new IntVec3(), false);
    Scribe_References.Look<AerialVehicleInFlight>(ref this.aerialVehicle, "aerialVehicle", false);
  }

  public override void SpawnSetup(Map map, bool respawningAfterLoad)
  {
    base.SpawnSetup(map, respawningAfterLoad);
    if (respawningAfterLoad)
      return;
    this.vehicle.CompVehicleLauncher.launchProtocol.Prepare(this.Map, this.Position, Rot4.North);
    this.vehicle.CompVehicleLauncher.launchProtocol.OrderProtocol(LaunchProtocol.LaunchType.Takeoff);
    this.vehicle.CompVehicleLauncher.launchProtocol.SetTickCount(-this.def.skyfaller.ticksToImpactRange.max);
  }
}
