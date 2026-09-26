// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleTurret_AutoRefuel
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools;
using System;
using System.Linq;
using Vehicles;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

[UsedImplicitly]
[StaticConstructorOnStartup]
public class VehicleTurret_AutoRefuel : VehicleTurret
{
  public VehicleTurret_AutoRefuel()
  {
  }

  public VehicleTurret_AutoRefuel(VehiclePawn vehicle)
    : base(vehicle)
  {
  }

  public VehicleTurret_AutoRefuel(VehiclePawn vehicle, VehicleTurret reference)
    : base(vehicle, reference)
  {
  }

  static VehicleTurret_AutoRefuel()
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() => VehicleTurret_AutoRefuel.RefuelVehicleTurret = (WorkGiver_RefuelVehicleTurret) DefDatabase<WorkGiverDef>.GetNamed("PackVehicleTurret", true).Worker));
  }

  private static WorkGiver_RefuelVehicleTurret RefuelVehicleTurret { get; set; }

  public virtual void PostTurretFire()
  {
    base.PostTurretFire();
    if (this.loadedAmmo != null || GenCollection.Any<ReservationManager.Reservation>(((Thing) this.vehicle).Map.reservationManager.ReservationsReadOnly, (Predicate<ReservationManager.Reservation>) (r => r.Job != null && r.Job.workGiverDef == ((WorkGiver) VehicleTurret_AutoRefuel.RefuelVehicleTurret).def && LocalTargetInfo.op_Equality(r.Job.targetB, LocalTargetInfo.op_Implicit((Thing) this.vehicle)))))
      return;
    VehicleRoleHandler vehicleRoleHandler = GenCollection.FirstOrDefault<VehicleRoleHandler>(this.vehicle.handlers, (Predicate<VehicleRoleHandler>) (handler =>
    {
      if ((handler.role.HandlingTypes & 2) != 2)
        return false;
      return handler.role.TurretIds.Contains(this.key) || handler.role.TurretIds.Contains(this.groupKey);
    }));
    Pawn pawn = vehicleRoleHandler != null ? vehicleRoleHandler.thingOwner.InnerListForReading.FirstOrDefault<Pawn>() : (Pawn) null;
    if (pawn == null)
      return;
    this.vehicle.DisembarkPawn(pawn);
    Job job1 = ((WorkGiver_Scanner) VehicleTurret_AutoRefuel.RefuelVehicleTurret).JobOnThing(pawn, (Thing) this.vehicle, false);
    if (job1 == null)
      return;
    pawn.jobs.TryTakeOrderedJob(job1, new JobTag?((JobTag) 0), false);
    Job job2 = JobMaker.MakeJob(JobDefOf_Vehicles.Board, LocalTargetInfo.op_Implicit((Thing) this.vehicle));
    this.vehicle.GiveLoadJob(pawn, vehicleRoleHandler);
    pawn.jobs.TryTakeOrderedJob(job2, new JobTag?((JobTag) 0), true);
    ComponentCache.GetCachedMapComponent<VehicleReservationManager>(((Thing) this.vehicle).Map).Reserve<VehicleRoleHandler, VehicleHandlerReservation>(this.vehicle, pawn, job2, vehicleRoleHandler);
  }
}
