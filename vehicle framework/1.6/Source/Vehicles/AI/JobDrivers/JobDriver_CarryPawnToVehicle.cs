// Decompiled with JetBrains decompiler
// Type: Vehicles.JobDriver_CarryPawnToVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class JobDriver_CarryPawnToVehicle : JobDriver
{
  public VehiclePawn Vehicle
  {
    get
    {
      LocalTargetInfo target = this.job.GetTarget((TargetIndex) 2);
      return ((LocalTargetInfo) ref target).Thing as VehiclePawn;
    }
  }

  public VehicleRoleHandler VehicleHandler
  {
    get
    {
      return this.job is Job_Vehicle job ? job.handler : GenCollection.FirstOrDefault<VehicleRoleHandler>(this.Vehicle.handlers, (Predicate<VehicleRoleHandler>) (handler => handler.CanOperateRole(this.Pawn))) ?? GenCollection.FirstOrDefault<VehicleRoleHandler>(this.Vehicle.handlers, (Predicate<VehicleRoleHandler>) (handler => handler.CanOperateRole(this.Pawn)));
    }
  }

  public Pawn Pawn
  {
    get
    {
      LocalTargetInfo target = this.job.GetTarget((TargetIndex) 1);
      return (Pawn) ((LocalTargetInfo) ref target).Thing;
    }
  }

  public virtual bool TryMakePreToilReservations(bool errorOnFailed)
  {
    return ReservationUtility.Reserve(this.pawn, this.job.GetTarget((TargetIndex) 1), this.job, 1, -1, (ReservationLayerDef) null, errorOnFailed, false);
  }

  protected virtual IEnumerable<Toil> MakeNewToils()
  {
    JobDriver_CarryPawnToVehicle carryPawnToVehicle = this;
    ToilFailConditions.FailOnDestroyedOrNull<JobDriver_CarryPawnToVehicle>(carryPawnToVehicle, (TargetIndex) 1);
    ToilFailConditions.FailOnDestroyedOrNull<JobDriver_CarryPawnToVehicle>(carryPawnToVehicle, (TargetIndex) 2);
    ToilFailConditions.FailOnAggroMentalState<JobDriver_CarryPawnToVehicle>(carryPawnToVehicle, (TargetIndex) 1);
    ToilFailConditions.FailOnBurningImmobile<JobDriver_CarryPawnToVehicle>(carryPawnToVehicle, (TargetIndex) 2);
    // ISSUE: reference to a compiler-generated method
    // ISSUE: reference to a compiler-generated method
    yield return ToilFailConditions.FailOnSomeonePhysicallyInteracting<Toil>(ToilFailConditions.FailOn<Toil>(ToilFailConditions.FailOn<Toil>(ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(ToilFailConditions.FailOnDestroyedNullOrForbidden<Toil>(Toils_Goto.GotoThing((TargetIndex) 1, (PathEndMode) 1, false), (TargetIndex) 1), (TargetIndex) 2), new Func<bool>(carryPawnToVehicle.\u003CMakeNewToils\u003Eb__7_0)), new Func<bool>(carryPawnToVehicle.\u003CMakeNewToils\u003Eb__7_1)), (TargetIndex) 1);
    yield return Toils_Haul.StartCarryThing((TargetIndex) 1, false, false, false, true, false);
    yield return Toils_Goto.GotoThing((TargetIndex) 2, (PathEndMode) 2, false);
    yield return ToilEffects.WithProgressBarToilDelay(ToilFailConditions.FailOnCannotTouch<Toil>(Toils_General.Wait(250, (TargetIndex) 0), (TargetIndex) 2, (PathEndMode) 2), (TargetIndex) 2, false, -0.5f);
    yield return JobDriver_CarryPawnToVehicle.PutPawnOnVehicle(carryPawnToVehicle.Pawn, carryPawnToVehicle.Vehicle, carryPawnToVehicle.VehicleHandler);
  }

  public static Toil PutPawnOnVehicle(Pawn pawn, VehiclePawn vehicle, VehicleRoleHandler handler)
  {
    return new Toil()
    {
      initAction = (Action) (() => vehicle.TryAddPawn(pawn, handler)),
      defaultCompleteMode = (ToilCompleteMode) 1
    };
  }
}
