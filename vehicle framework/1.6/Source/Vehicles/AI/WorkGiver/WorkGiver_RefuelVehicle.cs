// Decompiled with JetBrains decompiler
// Type: Vehicles.WorkGiver_RefuelVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools;
using System.Collections.Generic;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class WorkGiver_RefuelVehicle : WorkGiver_Scanner
{
  public virtual PathEndMode PathEndMode => (PathEndMode) 2;

  public virtual JobDef JobStandard => JobDefOf_Vehicles.RefuelVehicle;

  public virtual JobDef JobAtomic => JobDefOf_Vehicles.RefuelVehicleAtomic;

  public virtual IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
  {
    return (IEnumerable<Thing>) ((Thing) pawn).Map.GetCachedMapComponent<VehicleReservationManager>().VehicleListers("Refuel");
  }

  public virtual bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
  {
    if (t is VehiclePawn vehicle && vehicle.CompFueledTravel != null)
    {
      VehiclePathFollower vehiclePather = vehicle.vehiclePather;
      if (vehiclePather != null && !vehiclePather.Moving)
        return WorkGiver_RefuelVehicle.CanRefuel(pawn, vehicle, forced);
    }
    return false;
  }

  public virtual Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
  {
    if (!(t is VehiclePawn vehiclePawn) || vehiclePawn.CompFueledTravel == null)
      return (Job) null;
    Thing thing = vehiclePawn.CompFueledTravel.ClosestFuelAvailable(pawn);
    return thing == null ? (Job) null : JobMaker.MakeJob(JobDefOf_Vehicles.RefuelVehicle, LocalTargetInfo.op_Implicit((Thing) vehiclePawn), LocalTargetInfo.op_Implicit(thing));
  }

  [UsedImplicitly]
  public static bool CanRefuel(Pawn pawn, VehiclePawn vehicle, bool forced = false)
  {
    CompFueledTravel compFueledTravel = vehicle.CompFueledTravel;
    if (compFueledTravel == null || ((Thing) vehicle).Faction != ((Thing) pawn).Faction || compFueledTravel.FuelLeaking || compFueledTravel.FullTank || !forced && !compFueledTravel.ShouldAutoRefuelNow || ForbidUtility.IsForbidden((Thing) vehicle, pawn) || !ReservationUtility.CanReserve(pawn, LocalTargetInfo.op_Implicit((Thing) vehicle), 1, -1, (ReservationLayerDef) null, forced))
      return false;
    if (compFueledTravel.ClosestFuelAvailable(pawn) != null)
      return true;
    JobFailReason.Is(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("NoFuelToRefuel", NamedArgument.op_Implicit((Def) compFueledTravel.Props.fuelType))), (string) null);
    return false;
  }
}
