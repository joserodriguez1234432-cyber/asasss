// Decompiled with JetBrains decompiler
// Type: Vehicles.WorkGiver_RemoveFuelFromVehicle
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

public class WorkGiver_RemoveFuelFromVehicle : WorkGiver_Scanner
{
  public virtual PathEndMode PathEndMode => (PathEndMode) 2;

  public virtual JobDef JobStandard => JobDefOf_Vehicles.RemoveFuelFromVehicle;

  public virtual IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
  {
    return (IEnumerable<Thing>) ((Thing) pawn).Map.GetCachedMapComponent<VehicleReservationManager>().VehicleListers("RemoveFuel");
  }

  public virtual bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
  {
    if (t is VehiclePawn vehicle && vehicle.CompFueledTravel != null)
    {
      VehiclePathFollower vehiclePather = vehicle.vehiclePather;
      if (vehiclePather != null && !vehiclePather.Moving)
        return WorkGiver_RemoveFuelFromVehicle.CanRemoveFuel(pawn, vehicle, forced);
    }
    return false;
  }

  public virtual Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
  {
    if (t is VehiclePawn vehiclePawn && vehiclePawn.CompFueledTravel != null)
    {
      VehiclePathFollower vehiclePather = vehiclePawn.vehiclePather;
      if (vehiclePather != null && !vehiclePather.Moving)
        return JobMaker.MakeJob(JobDefOf_Vehicles.RemoveFuelFromVehicle, LocalTargetInfo.op_Implicit((Thing) vehiclePawn));
    }
    return (Job) null;
  }

  [PublicAPI]
  public static bool CanRemoveFuel(Pawn pawn, VehiclePawn vehicle, bool forced = false)
  {
    CompFueledTravel compFueledTravel = vehicle.CompFueledTravel;
    return compFueledTravel != null && ((Thing) vehicle).Faction == ((Thing) pawn).Faction && compFueledTravel.CanEjectFuel && !ForbidUtility.IsForbidden((Thing) vehicle, pawn) && ReservationUtility.CanReserve(pawn, LocalTargetInfo.op_Implicit((Thing) vehicle), 1, -1, (ReservationLayerDef) null, forced);
  }
}
