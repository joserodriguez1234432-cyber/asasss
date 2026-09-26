// Decompiled with JetBrains decompiler
// Type: Vehicles.World.ArrivalAction_CrashInMap
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld.Planet;
using Verse;

#nullable disable
namespace Vehicles.World;

public class ArrivalAction_CrashInMap : ArrivalAction_LandToCell
{
  public ArrivalAction_CrashInMap()
  {
  }

  public ArrivalAction_CrashInMap(
    VehiclePawn vehicle,
    MapParent mapParent,
    IntVec3 landingCell,
    Rot4 landingRot)
    : base(vehicle, mapParent, landingCell, landingRot)
  {
  }

  protected override void SpawnSkyfaller()
  {
    VehicleSkyfaller_Crashing skyfallerCrashing = (VehicleSkyfaller_Crashing) ThingMaker.MakeThing(this.vehicle.CompVehicleLauncher.Props.skyfallerCrashing, (ThingDef) null);
    skyfallerCrashing.vehicle = this.vehicle;
    skyfallerCrashing.rotCrashing = Rot4.East;
    GenSpawn.Spawn((Thing) skyfallerCrashing, this.landingCell, this.mapParent.Map, this.landingRot, (WipeMode) 0, false, false);
  }

  protected override void ExecuteEvents()
  {
    this.vehicle.EventRegistry[VehicleEventDefOf.AerialVehicleCrashLanding].ExecuteEvents();
  }
}
