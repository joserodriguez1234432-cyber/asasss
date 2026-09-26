// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleWorkGiver
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public abstract class VehicleWorkGiver : WorkGiver_Scanner
{
  public abstract JobDef JobDef { get; }

  public virtual PathEndMode PathEndMode => (PathEndMode) 2;

  public abstract bool CanBeWorkedOn(VehiclePawn vehicle);

  public virtual Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
  {
    if (t.Faction != ((Thing) pawn).Faction)
      return (Job) null;
    VehiclePawn vehicle = t as VehiclePawn;
    if (vehicle != null && !vehicle.vehiclePather.Moving && ReachabilityUtility.CanReach(pawn, new LocalTargetInfo(t.Position), (PathEndMode) 2, (Danger) 3, false, false, (TraverseMode) 0) && this.CanBeWorkedOn(vehicle))
    {
      VehicleReservationManager reservationManager = ((Thing) pawn).Map.GetCachedMapComponent<VehicleReservationManager>();
      if (reservationManager.CanReserve(vehicle, pawn, this.JobDef))
      {
        IntVec3 intVec3 = vehicle.SurroundingCells.RandomOrFallback<IntVec3>((Predicate<IntVec3>) (cell => reservationManager.CanReserve<LocalTargetInfo, VehicleTargetReservation>(vehicle, pawn, LocalTargetInfo.op_Implicit(cell))), IntVec3.Invalid);
        if (((IntVec3) ref intVec3).IsValid)
          return JobMaker.MakeJob(this.JobDef, LocalTargetInfo.op_Implicit((Thing) vehicle), LocalTargetInfo.op_Implicit(intVec3));
      }
    }
    return (Job) null;
  }
}
