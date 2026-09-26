// Decompiled with JetBrains decompiler
// Type: Vehicles.World.ArrivalAction_LandToCell
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld.Planet;
using SmashTools;
using Verse;

#nullable disable
namespace Vehicles.World;

[PublicAPI]
public class ArrivalAction_LandToCell : ArrivalAction_LandInMap
{
  protected IntVec3 landingCell;
  protected Rot4 landingRot;

  public ArrivalAction_LandToCell()
  {
  }

  public ArrivalAction_LandToCell(
    VehiclePawn vehicle,
    MapParent mapParent,
    IntVec3 landingCell,
    Rot4 landingRot)
    : base(vehicle, mapParent)
  {
    this.mapParent = mapParent;
    this.landingCell = landingCell;
    this.landingRot = landingRot;
  }

  public virtual bool CanArriveInMap => this.mapParent?.Map != null;

  public override void Arrived(GlobalTargetInfo target)
  {
    if (!this.mapParent.HasMap)
    {
      Trace.Fail($"Unable to land {this.vehicle} at destination. Map no longer exists, spawning as caravan nearby...");
      this.AerialVehicle.SwitchToCaravan();
    }
    else
    {
      base.Arrived(target);
      this.SpawnSkyfaller();
      this.ExecuteEvents();
    }
  }

  protected virtual void SpawnSkyfaller()
  {
    VehicleSkyfaller_Arriving skyfallerArriving = (VehicleSkyfaller_Arriving) VehicleSkyfallerMaker.MakeSkyfaller(this.vehicle.CompVehicleLauncher.Props.skyfallerIncoming, this.vehicle);
    skyfallerArriving.rotatePostLanding = this.landingRot;
    Rot4 rot4 = (Rot4?) this.vehicle.CompVehicleLauncher.launchProtocol.LandingProperties?.forcedRotation ?? this.landingRot;
    GenSpawn.Spawn((Thing) skyfallerArriving, this.landingCell, this.mapParent.Map, rot4, (WipeMode) 0, false, false);
  }

  public override void ExposeData()
  {
    base.ExposeData();
    Scribe_Values.Look<IntVec3>(ref this.landingCell, "landingCell", new IntVec3(), false);
    Scribe_Values.Look<Rot4>(ref this.landingRot, "landingRot", new Rot4(), false);
  }
}
