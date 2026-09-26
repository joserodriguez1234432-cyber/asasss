// Decompiled with JetBrains decompiler
// Type: Vehicles.World.VehicleArrivalAction
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld.Planet;
using Verse;

#nullable disable
namespace Vehicles.World;

[PublicAPI]
public abstract class VehicleArrivalAction : IArrivalAction, IExposable
{
  protected VehiclePawn vehicle;

  protected VehicleArrivalAction()
  {
  }

  protected VehicleArrivalAction(VehiclePawn vehicle) => this.vehicle = vehicle;

  public virtual bool DestroyOnArrival => false;

  public AerialVehicleInFlight AerialVehicle => this.vehicle.GetAerialVehicle();

  public virtual void Arrived(GlobalTargetInfo target)
  {
    if (!this.DestroyOnArrival)
      return;
    this.AerialVehicle.ClearAndDestroy();
  }

  public virtual void ExposeData()
  {
    Scribe_References.Look<VehiclePawn>(ref this.vehicle, "vehicle", true);
  }
}
