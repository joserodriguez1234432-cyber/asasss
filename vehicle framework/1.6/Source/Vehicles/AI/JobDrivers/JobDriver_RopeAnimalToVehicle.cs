// Decompiled with JetBrains decompiler
// Type: Vehicles.JobDriver_RopeAnimalToVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

[Obsolete("Incomplete")]
public class JobDriver_RopeAnimalToVehicle : JobDriver_RopeToDestination
{
  protected virtual int TicksToRope => 60;

  public Pawn Animal
  {
    get
    {
      LocalTargetInfo target = ((JobDriver) this).job.GetTarget((TargetIndex) 1);
      return ((LocalTargetInfo) ref target).Pawn;
    }
  }

  public VehiclePawn Vehicle
  {
    get
    {
      LocalTargetInfo target = ((JobDriver) this).job.GetTarget((TargetIndex) 2);
      return ((LocalTargetInfo) ref target).Pawn as VehiclePawn;
    }
  }

  public IntVec3 RopingCell
  {
    get
    {
      LocalTargetInfo target = ((JobDriver) this).job.GetTarget((TargetIndex) 3);
      return ((LocalTargetInfo) ref target).Cell;
    }
  }

  public virtual bool TryMakePreToilReservations(bool errorOnFailed)
  {
    if (!GenList.NullOrEmpty<Pawn>((IList<Pawn>) ((JobDriver) this).pawn.roping.Ropees))
    {
      foreach (Thing ropee in ((JobDriver) this).pawn.roping.Ropees)
        ReservationUtility.Reserve(((JobDriver) this).pawn, LocalTargetInfo.op_Implicit(ropee), ((JobDriver) this).job, 1, -1, (ReservationLayerDef) null, errorOnFailed, false);
    }
    this.UpdateDestination();
    return ReservationUtility.Reserve(((JobDriver) this).pawn, LocalTargetInfo.op_Implicit((Thing) this.Animal), ((JobDriver) this).job, 1, -1, (ReservationLayerDef) null, errorOnFailed, false);
  }

  protected virtual bool ShouldOpportunisticallyRopeAnimal(Pawn animal)
  {
    return JobGiver_PrepareCaravan_CollectPawns.DoesAnimalNeedGathering(((JobDriver) this).pawn, animal);
  }

  protected virtual bool HasRopeeArrived(Pawn ropee, bool roperWaitingAtDest)
  {
    PawnDuty duty = ((JobDriver) this).pawn.mindState.duty;
    LocalTargetInfo localTargetInfo = duty != null ? duty.focus : LocalTargetInfo.Invalid;
    if (!((LocalTargetInfo) ref localTargetInfo).IsValid || !(((LocalTargetInfo) ref localTargetInfo).Pawn is VehiclePawn))
      return false;
    VehiclePawn pawn = ((LocalTargetInfo) ref localTargetInfo).Pawn as VehiclePawn;
    IntVec3 position = ((Thing) ((JobDriver) this).pawn).Position;
    if (!((IntVec3) ref position).InHorDistOf(this.RopingCell, 2f))
      return false;
    District district = GridsUtility.GetDistrict(((Thing) pawn).Position, ((Thing) pawn).Map, (RegionType) 14);
    return district == RegionAndRoomQuery.GetDistrict((Thing) pawn, (RegionType) 14) && district == RegionAndRoomQuery.GetDistrict((Thing) ropee, (RegionType) 14);
  }

  protected virtual IEnumerable<Toil> MakeNewToils()
  {
    JobDriver_RopeAnimalToVehicle ropeAnimalToVehicle = this;
    // ISSUE: reference to a compiler-generated method
    yield return Toils_General.Do(new Action(ropeAnimalToVehicle.\u003CMakeNewToils\u003Eb__11_0));
    // ISSUE: reference to a compiler-generated method
    ((JobDriver) ropeAnimalToVehicle).AddFinishAction(new Action<JobCondition>(ropeAnimalToVehicle.\u003CMakeNewToils\u003Eb__11_1));
    Toil findAnotherAnimal = Toils_General.Label();
    Toil topOfLoop = Toils_General.Label();
    yield return topOfLoop;
    // ISSUE: reference to a compiler-generated method
    yield return Toils_Jump.JumpIf(findAnotherAnimal, new Func<bool>(ropeAnimalToVehicle.\u003CMakeNewToils\u003Eb__11_2));
    yield return Toils_Reserve.Reserve((TargetIndex) 1, 1, -1, (ReservationLayerDef) null, false);
    yield return Toils_Rope.GotoRopeAttachmentInteractionCell((TargetIndex) 1);
    yield return Toils_Rope.RopePawn((TargetIndex) 1);
    Toil toil = Toils_Goto.GotoCell(ropeAnimalToVehicle.RopingCell, (PathEndMode) 2);
    ropeAnimalToVehicle.MatchLocomotionUrgency(toil);
    // ISSUE: reference to a compiler-generated method
    toil.AddPreTickAction(new Action(ropeAnimalToVehicle.\u003CMakeNewToils\u003Eb__11_3));
    // ISSUE: reference to a compiler-generated method
    ToilFailConditions.FailOn<Toil>(toil, new Func<bool>(ropeAnimalToVehicle.\u003CMakeNewToils\u003Eb__11_4));
    yield return toil;
    yield return JobDriver_RopeAnimalToVehicle.TransferRope((TargetIndex) 1, (TargetIndex) 2);
    yield return findAnotherAnimal;
    yield return Toils_Jump.JumpIf(topOfLoop, new Func<bool>(ropeAnimalToVehicle.FindAnotherAnimalToRope));
    topOfLoop = Toils_General.Label();
    yield return topOfLoop;
    yield return Toils_Jump.JumpIf(topOfLoop, new Func<bool>(((JobDriver_RopeToDestination) ropeAnimalToVehicle).UpdateDestination));
    topOfLoop = Toils_General.Wait(ropeAnimalToVehicle.TicksToRope, (TargetIndex) 1);
    // ISSUE: reference to a compiler-generated method
    topOfLoop.AddPreTickAction(new Action(ropeAnimalToVehicle.\u003CMakeNewToils\u003Eb__11_5));
    yield return topOfLoop;
    // ISSUE: reference to a compiler-generated method
    yield return Toils_Jump.JumpIf(topOfLoop, new Func<bool>(ropeAnimalToVehicle.\u003CMakeNewToils\u003Eb__11_6));
  }

  public static Toil TransferRope(TargetIndex ropeeIndex, TargetIndex targetIndex)
  {
    Toil toil = new Toil();
    toil.initAction = (Action) (() =>
    {
      Pawn actor = toil.actor;
      LocalTargetInfo target1 = actor.jobs.curJob.GetTarget(targetIndex);
      Pawn pawn = ((LocalTargetInfo) ref target1).Pawn;
      LocalTargetInfo target2 = actor.jobs.curJob.GetTarget(ropeeIndex);
      if (!(((LocalTargetInfo) ref target2).Thing is Pawn thing2))
        return;
      pawn.roping.RopePawn(thing2);
      thing2.caller?.DoCall(false);
      PawnUtility.ForceWait(thing2, 30, (Thing) actor, false, false);
    });
    toil.defaultCompleteMode = (ToilCompleteMode) 3;
    toil.defaultDuration = 30;
    ToilFailConditions.FailOnDespawnedOrNull<Toil>(toil, ropeeIndex);
    ToilFailConditions.FailOnDespawnedOrNull<Toil>(toil, targetIndex);
    ToilEffects.PlaySustainerOrSound(toil, (Func<SoundDef>) (() => SoundDefOf.Roping), 1f);
    return toil;
  }

  protected virtual void ProcessRopeesThatHaveArrived(bool roperWaitingAtDest)
  {
    for (int index = ((JobDriver) this).pawn.roping.Ropees.Count - 1; index >= 0; --index)
    {
      Pawn ropee = ((JobDriver) this).pawn.roping.Ropees[index];
      if (base.HasRopeeArrived(((JobDriver) this).pawn, roperWaitingAtDest))
      {
        ((JobDriver) this).pawn.roping.DropRope(ropee);
        if (ropee.jobs != null && ropee.CurJob != null && ropee.jobs.curDriver is JobDriver_FollowRoper)
          ropee.jobs.EndCurrentJob((JobCondition) 16 /*0x10*/, true, true);
        base.ProcessArrivedRopee(((JobDriver) this).pawn);
      }
    }
  }

  protected virtual void MatchLocomotionUrgency(Toil toil)
  {
    toil.AddPreInitAction((Action) (() => ((JobDriver) this).locomotionUrgencySameAs = this.SlowestRopee()));
    toil.AddFinishAction((Action) (() => ((JobDriver) this).locomotionUrgencySameAs = (Pawn) null));
  }

  protected virtual Pawn SlowestRopee()
  {
    Pawn pawn;
    return !GenCollection.TryMaxBy<Pawn, float>((IEnumerable<Pawn>) ((JobDriver) this).pawn.roping.Ropees, (Func<Pawn, float>) (p => p.TicksPerMoveCardinal), ref pawn) ? (Pawn) null : pawn;
  }

  protected virtual bool FindAnotherAnimalToRope()
  {
    if (((JobDriver) this).pawn.roping.Ropees.Count >= ((int?) ((JobDriver) this).pawn.mindState?.duty?.ropeeLimit ?? 10))
      return false;
    Thing thing1 = GenClosest.ClosestThingReachable(((Thing) ((JobDriver) this).pawn).Position, ((Thing) ((JobDriver) this).pawn).Map, ThingRequest.ForGroup((ThingRequestGroup) 12), (PathEndMode) 2, TraverseParms.For(((JobDriver) this).pawn, (Danger) 3, (TraverseMode) 0, false, false, false, true), 10f, (Predicate<Thing>) (thing => thing is Pawn pawn && base.ShouldOpportunisticallyRopeAnimal(pawn)), (IEnumerable<Thing>) null, 0, -1, false, (RegionType) 14, false, false) ?? this.FindDistantAnimalToRope();
    if (thing1 == null)
      return false;
    ((JobDriver) this).job.SetTarget((TargetIndex) 1, LocalTargetInfo.op_Implicit(thing1));
    return true;
  }

  protected virtual void ProcessArrivedRopee(Pawn ropee)
  {
    PawnDuty duty = ropee.mindState.duty;
    LocalTargetInfo localTargetInfo = duty != null ? duty.focus : LocalTargetInfo.Invalid;
    if (!((LocalTargetInfo) ref localTargetInfo).IsValid)
      return;
    ropee.roping.RopePawn(((LocalTargetInfo) ref localTargetInfo).Pawn);
  }

  public static Toil GotoRopeAttachmentInteractionCellForVehicle(
    IntVec3 ropingCell,
    TargetIndex ropeeIndex,
    TargetIndex vehicleIndex)
  {
    Toil toil = new Toil();
    toil.initAction = (Action) (() =>
    {
      Pawn actor = toil.actor;
      LocalTargetInfo target1 = actor.CurJob.GetTarget(ropeeIndex);
      Pawn pawn1 = ((LocalTargetInfo) ref target1).Pawn;
      LocalTargetInfo target2 = actor.CurJob.GetTarget(vehicleIndex);
      VehiclePawn pawn2 = ((LocalTargetInfo) ref target2).Pawn as VehiclePawn;
      if (!((IntVec3) ref ropingCell).IsValid)
        actor.jobs.curDriver.EndJobWith((JobCondition) 4);
      if (IntVec3.op_Equality(((Thing) actor).Position, ropingCell))
      {
        actor.jobs.curDriver.ReadyForNextToil();
      }
      else
      {
        ((Thing) actor).Map.debugDrawer.FlashCell(ropingCell, 0.0f, (string) null, 50);
        actor.pather.StartPath(LocalTargetInfo.op_Implicit(ropingCell), (PathEndMode) 1);
      }
    });
    toil.tickAction = (Action) (() =>
    {
      Pawn actor = toil.actor;
      LocalTargetInfo target3 = actor.CurJob.GetTarget(ropeeIndex);
      Pawn ropee = ((LocalTargetInfo) ref target3).Pawn;
      LocalTargetInfo target4 = actor.CurJob.GetTarget(vehicleIndex);
      VehiclePawn pawn3 = ((LocalTargetInfo) ref target4).Pawn as VehiclePawn;
      Pawn pawn4 = actor;
      Pawn pawn5 = ropee;
      LocalTargetInfo destination1 = actor.pather.Destination;
      IntVec3 cell1 = ((LocalTargetInfo) ref destination1).Cell;
      bool flag = !AnimalPenUtility.IsGoodRopeAttachmentInteractionCell(pawn4, pawn5, cell1);
      if (!(actor.pather.Moving & flag))
        return;
      DebugCellDrawer debugDrawer = ((Thing) actor).Map.debugDrawer;
      LocalTargetInfo destination2 = actor.pather.Destination;
      IntVec3 cell2 = ((LocalTargetInfo) ref destination2).Cell;
      debugDrawer.FlashCell(cell2, 0.0f, (string) null, 50);
      ropingCell = pawn3.SurroundingCells.FirstOrDefault<IntVec3>((Func<IntVec3, bool>) (cell => ((IntVec3) ref cell).IsValid && GenGrid.WalkableBy(cell, ((Thing) ropee).Map, ropee) && GenGrid.WalkableBy(cell, ((Thing) actor).Map, actor)));
      if (((IntVec3) ref ropingCell).IsValid)
      {
        actor.CurJob.SetTarget((TargetIndex) 3, LocalTargetInfo.op_Implicit(ropingCell));
        actor.pather.StartPath(LocalTargetInfo.op_Implicit(ropingCell), (PathEndMode) 1);
      }
      else
        actor.jobs.curDriver.EndJobWith((JobCondition) 4);
    });
    toil.defaultCompleteMode = (ToilCompleteMode) 2;
    ToilFailConditions.FailOnDespawnedOrNull<Toil>(toil, ropeeIndex);
    return toil;
  }
}
