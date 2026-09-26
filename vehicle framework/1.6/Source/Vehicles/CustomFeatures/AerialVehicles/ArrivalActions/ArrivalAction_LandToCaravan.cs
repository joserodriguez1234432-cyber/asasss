// Decompiled with JetBrains decompiler
// Type: Vehicles.World.ArrivalAction_LandToCaravan
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld.Planet;

#nullable disable
namespace Vehicles.World;

public class ArrivalAction_LandToCaravan : VehicleArrivalAction
{
  public ArrivalAction_LandToCaravan()
  {
  }

  public ArrivalAction_LandToCaravan(VehiclePawn vehicle)
    : base(vehicle)
  {
  }

  public override void Arrived(GlobalTargetInfo _) => this.AerialVehicle.SwitchToCaravan();
}
