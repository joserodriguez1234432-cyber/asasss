// Decompiled with JetBrains decompiler
// Type: Vehicles.WorkGiver_CarryToVehicle`1
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

[PublicAPI]
public abstract class WorkGiver_CarryToVehicle<T> : WorkGiver_Scanner
{
  public virtual PathEndMode PathEndMode => (PathEndMode) 2;

  public virtual string ReservationName => "LoadVehicle";

  public virtual JobDef JobDef => JobDefOf_Vehicles.LoadVehicle;

  protected abstract List<T> GetThingsToLoad(VehiclePawn vehicle, Pawn pawn);

  protected abstract Thing FindThingToPack(VehiclePawn vehicle, Pawn pawn, List<T> things);

  public virtual IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
  {
    return (IEnumerable<Thing>) ((Thing) pawn).Map.GetCachedMapComponent<VehicleReservationManager>().VehicleListers(this.ReservationName);
  }

  protected virtual bool JobAvailable(Pawn pawn, VehiclePawn vehicle) => true;

  public virtual Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
  {
    if (!(t is VehiclePawn vehicle))
      return (Job) null;
    if (ForbidUtility.IsForbidden((Thing) vehicle, pawn))
      return (Job) null;
    if (!ReachabilityUtility.CanReach(pawn, LocalTargetInfo.op_Implicit(((Thing) vehicle).Position), (PathEndMode) 1, (Danger) 3, false, false, (TraverseMode) 0))
      return (Job) null;
    if (!this.JobAvailable(pawn, vehicle))
      return (Job) null;
    List<T> thingsToLoad = this.GetThingsToLoad(vehicle, pawn);
    if (GenList.NullOrEmpty<T>((IList<T>) thingsToLoad))
      return (Job) null;
    Thing thingToPack = this.FindThingToPack(vehicle, pawn, thingsToLoad);
    return thingToPack == null || thingToPack == pawn || thingToPack == vehicle ? (Job) null : JobMaker.MakeJob(this.JobDef, LocalTargetInfo.op_Implicit(thingToPack), LocalTargetInfo.op_Implicit((Thing) vehicle));
  }
}
