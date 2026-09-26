// Decompiled with JetBrains decompiler
// Type: Vehicles.World.ArrivalAction_LandInMap
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld.Planet;
using Verse;

#nullable disable
namespace Vehicles.World;

[PublicAPI]
public abstract class ArrivalAction_LandInMap : VehicleArrivalAction
{
  protected MapParent mapParent;

  protected ArrivalAction_LandInMap()
  {
  }

  protected ArrivalAction_LandInMap(VehiclePawn vehicle, MapParent mapParent)
    : base(vehicle)
  {
    this.mapParent = mapParent;
  }

  public override bool DestroyOnArrival => true;

  protected virtual void ExecuteEvents()
  {
    this.vehicle.EventRegistry[VehicleEventDefOf.AerialVehicleLanding].ExecuteEvents();
  }

  public override void ExposeData()
  {
    base.ExposeData();
    Scribe_References.Look<MapParent>(ref this.mapParent, "mapParent", false);
  }
}
