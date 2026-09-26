// Decompiled with JetBrains decompiler
// Type: Vehicles.WorkGiver_RefuelVehicleTurret
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System.Collections.Generic;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class WorkGiver_RefuelVehicleTurret : WorkGiver_CarryToVehicle<ThingDefCountClass>
{
  public override string ReservationName => "LoadVehicleForTurret";

  public override JobDef JobDef => JobDefOf_Vehicles.CarryItemToVehicle;

  public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
  {
    return (IEnumerable<Thing>) ((Thing) pawn).Map.GetCachedMapComponent<VehicleReservationManager>().VehicleListers("LoadVehicleForTurret");
  }

  public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
  {
    return base.JobOnThing(pawn, thing, forced);
  }

  protected override bool JobAvailable(Pawn pawn, VehiclePawn vehicle)
  {
    return vehicle.CompVehicleTurrets != null && !FireUtility.IsBurning((Thing) vehicle) && !vehicle.vehiclePather.Moving && !MassUtility.IsOverEncumbered((Pawn) vehicle);
  }

  protected override List<ThingDefCountClass> GetThingsToLoad(VehiclePawn vehicle, Pawn pawn)
  {
    return JobDriver_GiveItemToVehicle.FindThingDefsToPack(vehicle, pawn);
  }

  protected override Thing FindThingToPack(
    VehiclePawn vehicle,
    Pawn pawn,
    List<ThingDefCountClass> things)
  {
    return JobDriverGetItemForVehicleBase.FindThingToPack(vehicle, pawn, this.JobDef, (IEnumerable<ThingDefCountClass>) things);
  }
}
