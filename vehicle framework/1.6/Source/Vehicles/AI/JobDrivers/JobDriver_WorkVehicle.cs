// Decompiled with JetBrains decompiler
// Type: Vehicles.JobDriver_WorkVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

[PublicAPI]
public abstract class JobDriver_WorkVehicle : VehicleJobDriver
{
  protected abstract float TotalWork { get; }

  protected virtual float Work { get; set; }

  protected abstract StatDef Stat { get; }

  protected virtual SkillDef Skill => (SkillDef) null;

  protected virtual EffecterDef EffecterDef
  {
    get => ((BuildableDef) ((Thing) this.Vehicle).def).repairEffect;
  }

  protected virtual float SkillAmount => 0.08f;

  protected virtual ToilCompleteMode ToilCompleteMode => (ToilCompleteMode) 3;

  protected float GetProgressPct() => this.Work / this.TotalWork;

  protected virtual IEnumerable<Toil> MakeNewToils()
  {
    JobDriver_WorkVehicle driverWorkVehicle = this;
    ToilFailConditions.FailOnDespawnedNullOrForbidden<JobDriver_WorkVehicle>(driverWorkVehicle, (TargetIndex) 1);
    Toil jobEndable = Toils_Goto.GotoCell((TargetIndex) 2, (PathEndMode) 1);
    jobEndable.FailOnMoving<Toil>((TargetIndex) 1);
    yield return jobEndable;
    Toil workToil = new Toil();
    workToil.FailOnMoving<Toil>((TargetIndex) 1);
    ToilFailConditions.FailOnCannotTouch<Toil>(workToil, (TargetIndex) 1, (PathEndMode) 2);
    workToil.initAction = new Action(driverWorkVehicle.ResetWork);
    workToil.tickAction = new Action(WorkAction);
    if (driverWorkVehicle.EffecterDef != null)
      ToilEffects.WithEffect(workToil, driverWorkVehicle.EffecterDef, (TargetIndex) 1, new Color?());
    else
      ToilEffects.WithProgressBar(workToil, (TargetIndex) 1, new Func<float>(driverWorkVehicle.GetProgressPct), false, -0.5f, false);
    workToil.defaultCompleteMode = driverWorkVehicle.ToilCompleteMode;
    workToil.defaultDuration = 2000;
    if (driverWorkVehicle.Skill != null)
      workToil.activeSkill = (Func<SkillDef>) (() => this.Skill);
    yield return workToil;

    void WorkAction()
    {
      Pawn actor = workToil.actor;
      if (this.Skill != null)
        actor.skills?.Learn(this.Skill, this.SkillAmount, false, false);
      this.Work -= StatExtension.GetStatValue((Thing) actor, this.Stat, true, -1);
      if ((double) this.Work > 0.0)
        return;
      this.WorkComplete(actor);
    }
  }

  protected virtual void ResetWork() => this.Work = this.TotalWork;

  protected abstract void WorkComplete(Pawn actor);
}
