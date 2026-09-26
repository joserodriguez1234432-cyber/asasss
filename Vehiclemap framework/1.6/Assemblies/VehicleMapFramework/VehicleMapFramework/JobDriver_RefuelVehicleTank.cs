// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.JobDriver_RefuelVehicleTank
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Collections.Generic;
using System.Linq;
using Vehicles;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public class JobDriver_RefuelVehicleTank : JobDriver
{
  private const int RefuelingDuration = 240 /*0xF0*/;

  protected Thing Tank
  {
    get
    {
      LocalTargetInfo target = this.job.GetTarget((TargetIndex) 1);
      return ((LocalTargetInfo) ref target).Thing;
    }
  }

  protected VehiclePawn Vehicle
  {
    get
    {
      VehiclePawnWithMap vehicle;
      return !this.Tank.IsOnVehicleMapOf(out vehicle) ? (VehiclePawn) null : (VehiclePawn) vehicle;
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
    return ReservationUtility.Reserve(this.pawn, LocalTargetInfo.op_Implicit(this.Tank), this.job, 1, -1, (ReservationLayerDef) null, errorOnFailed, false) && ReservationUtility.Reserve(this.pawn, LocalTargetInfo.op_Implicit(this.Fuel), this.job, 1, -1, (ReservationLayerDef) null, errorOnFailed, false);
  }

  protected virtual IEnumerable<Toil> MakeNewToils()
  {
    JobDriver_RefuelVehicleTank refuelVehicleTank = this;
    ToilFailConditions.FailOnDespawnedNullOrForbidden<JobDriver_RefuelVehicleTank>(refuelVehicleTank, (TargetIndex) 1);
    // ISSUE: reference to a compiler-generated method
    refuelVehicleTank.AddEndCondition(new Func<JobCondition>(refuelVehicleTank.\u003CMakeNewToils\u003Eb__7_0));
    // ISSUE: reference to a compiler-generated method
    yield return Toils_General.DoAtomic(new Action(refuelVehicleTank.\u003CMakeNewToils\u003Eb__7_1));
    Toil reserveFuel = Toils_Reserve.Reserve((TargetIndex) 2, 1, -1, (ReservationLayerDef) null, false);
    yield return reserveFuel;
    yield return ToilFailConditions.FailOnSomeonePhysicallyInteracting<Toil>(ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(Toils_Goto.GotoThing((TargetIndex) 2, (PathEndMode) 3, false), (TargetIndex) 2), (TargetIndex) 2);
    yield return ToilFailConditions.FailOnDestroyedNullOrForbidden<Toil>(Toils_Haul.StartCarryThing((TargetIndex) 2, false, true, false, true, false), (TargetIndex) 2);
    yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveFuel, (TargetIndex) 2, (TargetIndex) 0, true, (Predicate<Thing>) null);
    yield return Toils_Goto.GotoThing((TargetIndex) 1, (PathEndMode) 2, false);
    yield return ToilEffects.WithProgressBarToilDelay(ToilFailConditions.FailOnCannotTouch<Toil>(ToilFailConditions.FailOnDestroyedNullOrForbidden<Toil>(ToilFailConditions.FailOnDestroyedNullOrForbidden<Toil>(Toils_General.Wait(240 /*0xF0*/, (TargetIndex) 0), (TargetIndex) 2), (TargetIndex) 1), (TargetIndex) 1, (PathEndMode) 2), (TargetIndex) 1, false, -0.5f);
    yield return JobDriver_RefuelVehicleTank.FinalizeRefueling((TargetIndex) 1, (TargetIndex) 2);
  }

  public static Toil FinalizeRefueling(TargetIndex refuelableInd, TargetIndex fuelInd)
  {
    Toil toil = new Toil();
    toil.initAction = (Action) (() =>
    {
      Job curJob = toil.actor.CurJob;
      LocalTargetInfo target1 = curJob.GetTarget(refuelableInd);
      Thing vehicle = (Thing) ThingCompUtility.TryGetComp<CompFuelTank>(((LocalTargetInfo) ref target1).Thing).Vehicle;
      if (GenList.NullOrEmpty<ThingCountClass>((IList<ThingCountClass>) toil.actor.CurJob.placedThings))
      {
        if (vehicle == null)
          return;
        CompFueledTravel comp = ThingCompUtility.TryGetComp<CompFueledTravel>(vehicle);
        List<Thing> thingList = new List<Thing>(1);
        LocalTargetInfo target2 = curJob.GetTarget(fuelInd);
        thingList.Add(((LocalTargetInfo) ref target2).Thing);
        comp.Refuel(thingList);
      }
      else
      {
        if (vehicle == null)
          return;
        ThingCompUtility.TryGetComp<CompFueledTravel>(vehicle).Refuel(toil.actor.CurJob.placedThings.Select<ThingCountClass, Thing>((Func<ThingCountClass, Thing>) (p => p.thing)).ToList<Thing>());
      }
    });
    toil.defaultCompleteMode = (ToilCompleteMode) 1;
    return toil;
  }
}
