// Decompiled with JetBrains decompiler
// Type: Vehicles.JobDriver_RefuelVehicleAtomic
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

[Obsolete("Needs more looking into.  Only added for potential use since it's also implemented for vanilla, despite never being used by the devs", true)]
public class JobDriver_RefuelVehicleAtomic : JobDriver
{
  private const int RefuelingDuration = 240 /*0xF0*/;

  protected VehiclePawn Vehicle
  {
    get
    {
      LocalTargetInfo target = this.job.GetTarget((TargetIndex) 1);
      return ((LocalTargetInfo) ref target).Thing as VehiclePawn;
    }
  }

  protected Thing Fuel
  {
    get
    {
      LocalTargetInfo target = this.job.GetTarget((TargetIndex) 2);
      return ((LocalTargetInfo) ref target).Thing;
    }
  }

  public virtual bool TryMakePreToilReservations(bool errorOnFailed)
  {
    return ReservationUtility.Reserve(this.pawn, LocalTargetInfo.op_Implicit((Thing) this.Vehicle), this.job, 1, -1, (ReservationLayerDef) null, errorOnFailed, false) && ReservationUtility.Reserve(this.pawn, LocalTargetInfo.op_Implicit(this.Fuel), this.job, 1, -1, (ReservationLayerDef) null, errorOnFailed, false);
  }

  protected virtual IEnumerable<Toil> MakeNewToils()
  {
    JobDriver_RefuelVehicleAtomic refuelVehicleAtomic = this;
    ToilFailConditions.FailOnDespawnedNullOrForbidden<JobDriver_RefuelVehicleAtomic>(refuelVehicleAtomic, (TargetIndex) 1);
    // ISSUE: reference to a compiler-generated method
    refuelVehicleAtomic.AddEndCondition(new Func<JobCondition>(refuelVehicleAtomic.\u003CMakeNewToils\u003Eb__6_0));
    // ISSUE: reference to a compiler-generated method
    yield return Toils_General.DoAtomic(new Action(refuelVehicleAtomic.\u003CMakeNewToils\u003Eb__6_1));
    Toil reserveFuel = Toils_Reserve.Reserve((TargetIndex) 2, 1, -1, (ReservationLayerDef) null, false);
    yield return reserveFuel;
    yield return ToilFailConditions.FailOnSomeonePhysicallyInteracting<Toil>(ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(Toils_Goto.GotoThing((TargetIndex) 2, (PathEndMode) 3, false), (TargetIndex) 2), (TargetIndex) 2);
    yield return ToilFailConditions.FailOnDestroyedNullOrForbidden<Toil>(Toils_Haul.StartCarryThing((TargetIndex) 2, false, true, false, true, false), (TargetIndex) 2);
    yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveFuel, (TargetIndex) 2, (TargetIndex) 0, false, (Predicate<Thing>) null);
    yield return Toils_Goto.GotoThing((TargetIndex) 1, (PathEndMode) 2, false);
    yield return ToilEffects.WithProgressBarToilDelay(ToilFailConditions.FailOnCannotTouch<Toil>(ToilFailConditions.FailOnDestroyedNullOrForbidden<Toil>(ToilFailConditions.FailOnDestroyedNullOrForbidden<Toil>(Toils_General.Wait(240 /*0xF0*/, (TargetIndex) 0), (TargetIndex) 2), (TargetIndex) 1), (TargetIndex) 1, (PathEndMode) 2), (TargetIndex) 1, false, -0.5f);
    yield return JobDriver_RefuelVehicleAtomic.FinalizeRefueling((TargetIndex) 1, (TargetIndex) 2);
  }

  public static Toil FinalizeRefueling(TargetIndex refuelableInd, TargetIndex fuelInd)
  {
    Toil toil = new Toil();
    toil.initAction = (Action) (() =>
    {
      Job curJob = toil.actor.CurJob;
      LocalTargetInfo target1 = curJob.GetTarget(refuelableInd);
      VehiclePawn thing1 = ((LocalTargetInfo) ref target1).Thing as VehiclePawn;
      if (GenList.NullOrEmpty<ThingCountClass>((IList<ThingCountClass>) toil.actor.CurJob.placedThings))
      {
        CompFueledTravel compFueledTravel = thing1.CompFueledTravel;
        List<Thing> fuelThings = new List<Thing>();
        LocalTargetInfo target2 = curJob.GetTarget(fuelInd);
        fuelThings.Add(((LocalTargetInfo) ref target2).Thing);
        compFueledTravel.Refuel(fuelThings);
      }
      else
        thing1.CompFueledTravel.Refuel(toil.actor.CurJob.placedThings.Select<ThingCountClass, Thing>((Func<ThingCountClass, Thing>) (thing => thing.thing)).ToList<Thing>());
    });
    toil.defaultCompleteMode = (ToilCompleteMode) 1;
    return toil;
  }
}
