// Decompiled with JetBrains decompiler
// Type: Vehicles.JobDriver_DisassembleVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class JobDriver_DisassembleVehicle : JobDriver
{
  protected const float MinDeconstructWork = 50f;
  protected const float MaxDeconstructWork = 5000f;
  protected float workLeft;
  protected float totalNeededWork;

  protected VehiclePawn Vehicle => ((LocalTargetInfo) ref this.job.targetA).Thing as VehiclePawn;

  protected virtual DesignationDef Designation => DesignationDefOf.Deconstruct;

  protected virtual float TotalWork
  {
    get
    {
      return Mathf.Clamp(StatExtension.GetStatValueAbstract((BuildableDef) this.Vehicle.VehicleDef.buildDef, StatDefOf.WorkToBuild, ((Thing) this.Vehicle).Stuff), 50f, 5000f);
    }
  }

  public virtual bool TryMakePreToilReservations(bool errorOnFailed)
  {
    return ReservationUtility.Reserve(this.pawn, LocalTargetInfo.op_Implicit((Thing) this.Vehicle), this.job, 1, -1, (ReservationLayerDef) null, true, false);
  }

  protected virtual IEnumerable<Toil> MakeNewToils()
  {
    JobDriver_DisassembleVehicle disassembleVehicle1 = this;
    ToilFailConditions.FailOn<JobDriver_DisassembleVehicle>(disassembleVehicle1, (Func<bool>) (() => this.Vehicle == null || !this.Vehicle.DeconstructibleBy(((Thing) this.pawn).Faction)));
    ToilFailConditions.FailOnThingMissingDesignation<JobDriver_DisassembleVehicle>(disassembleVehicle1, (TargetIndex) 1, disassembleVehicle1.Designation);
    ToilFailConditions.FailOnForbidden<JobDriver_DisassembleVehicle>(disassembleVehicle1, (TargetIndex) 1);
    yield return Toils_Goto.GotoThing((TargetIndex) 1, (PathEndMode) 2, false);
    Toil disassembleVehicle = ToilFailConditions.FailOnDestroyedNullOrForbidden<Toil>(new Toil(), (TargetIndex) 1);
    disassembleVehicle.initAction = (Action) (() =>
    {
      this.totalNeededWork = this.TotalWork;
      this.workLeft = this.totalNeededWork;
    });
    disassembleVehicle.tickAction = (Action) (() =>
    {
      this.workLeft -= StatExtension.GetStatValue((Thing) this.pawn, StatDefOf.ConstructionSpeed, true, -1) * 1.7f;
      this.TickAction();
      if ((double) this.workLeft > 0.0)
        return;
      disassembleVehicle.actor.jobs.curDriver.ReadyForNextToil();
    });
    disassembleVehicle.FailOnMoving<Toil>((TargetIndex) 1);
    ToilFailConditions.FailOnCannotTouch<Toil>(disassembleVehicle, (TargetIndex) 1, (PathEndMode) 2);
    disassembleVehicle.defaultCompleteMode = (ToilCompleteMode) 5;
    ToilEffects.WithEffect(disassembleVehicle, ((BuildableDef) ((Thing) disassembleVehicle1.Vehicle).def).repairEffect, (TargetIndex) 1, new Color?());
    ToilEffects.WithProgressBar(disassembleVehicle, (TargetIndex) 1, (Func<float>) (() => (float) (1.0 - (double) this.workLeft / (double) this.totalNeededWork)), false, -0.5f, false);
    disassembleVehicle.activeSkill = (Func<SkillDef>) (() => SkillDefOf.Construction);
    yield return disassembleVehicle;
    yield return new Toil()
    {
      initAction = (Action) (() =>
      {
        this.FinishedRemoving();
        this.Map.designationManager.RemoveAllDesignationsOn((Thing) this.Vehicle, false);
      }),
      defaultCompleteMode = (ToilCompleteMode) 1
    };
  }

  protected virtual void FinishedRemoving()
  {
    ((Thing) this.Vehicle).Destroy((DestroyMode) 4);
    this.pawn.records.Increment(RecordDefOf.ThingsDeconstructed);
  }

  protected virtual void TickAction()
  {
    if (CostListCalculator.CostListAdjusted((BuildableDef) this.Vehicle.VehicleDef.buildDef, ((Thing) this.Vehicle).Stuff, true).Count <= 0)
      return;
    this.pawn.skills?.Learn(SkillDefOf.Construction, 0.25f, false, false);
  }
}
