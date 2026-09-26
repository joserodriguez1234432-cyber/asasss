// Decompiled with JetBrains decompiler
// Type: Vehicles.JobDriver_RemoveFuelFromVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class JobDriver_RemoveFuelFromVehicle : JobDriver
{
  private const int RemovingFuelDuration = 120;

  private VehiclePawn Vehicle
  {
    get
    {
      LocalTargetInfo target = this.job.GetTarget((TargetIndex) 1);
      return ((LocalTargetInfo) ref target).Thing as VehiclePawn;
    }
  }

  public virtual bool TryMakePreToilReservations(bool errorOnFailed)
  {
    return ReservationUtility.Reserve(this.pawn, LocalTargetInfo.op_Implicit((Thing) this.Vehicle), this.job, 1, -1, (ReservationLayerDef) null, errorOnFailed, false);
  }

  protected virtual IEnumerable<Toil> MakeNewToils()
  {
    JobDriver_RemoveFuelFromVehicle jobEndable = this;
    ToilFailConditions.FailOnDespawnedNullOrForbidden<JobDriver_RemoveFuelFromVehicle>(jobEndable, (TargetIndex) 1);
    jobEndable.FailOnMoving<JobDriver_RemoveFuelFromVehicle>((TargetIndex) 1);
    // ISSUE: reference to a compiler-generated method
    jobEndable.AddEndCondition(new Func<JobCondition>(jobEndable.\u003CMakeNewToils\u003Eb__4_0));
    yield return ToilFailConditions.FailOnSomeonePhysicallyInteracting<Toil>(ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(Toils_Goto.GotoThing((TargetIndex) 1, (PathEndMode) 3, false), (TargetIndex) 1), (TargetIndex) 1);
    yield return ToilFailConditions.FailOnCannotTouch<Toil>(ToilFailConditions.FailOnDespawnedOrNull<Toil>(ToilEffects.WithProgressBarToilDelay(Toils_General.Wait(120, (TargetIndex) 0), (TargetIndex) 1, false, -0.5f), (TargetIndex) 1), (TargetIndex) 1, (PathEndMode) 2);
    yield return Toils_General.Do(new Action(jobEndable.Vehicle.CompFueledTravel.EjectFuel));
  }
}
