// Decompiled with JetBrains decompiler
// Type: Vehicles.JobDriver_PaintVehicle
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

public class JobDriver_PaintVehicle : VehicleJobDriver
{
  protected const float WorkPerCell = 30f;
  protected const float MaxMultiplier = 20f;

  protected virtual float SizeMultiplier
  {
    get
    {
      return Mathf.Min((float) (((BuildableDef) this.Vehicle.VehicleDef).Size.x * ((BuildableDef) this.Vehicle.VehicleDef).Size.z), 20f);
    }
  }

  protected override JobDef JobDef => JobDefOf_Vehicles.PaintVehicle;

  protected virtual IEnumerable<Toil> MakeNewToils()
  {
    JobDriver_PaintVehicle driverPaintVehicle = this;
    ToilFailConditions.FailOnDespawnedNullOrForbidden<JobDriver_PaintVehicle>(driverPaintVehicle, (TargetIndex) 1);
    Toil jobEndable = Toils_Goto.GotoCell((TargetIndex) 2, (PathEndMode) 1);
    jobEndable.FailOnMoving<Toil>((TargetIndex) 1);
    yield return jobEndable;
    Toil paintVehicle = new Toil();
    paintVehicle.initAction = (Action) (() =>
    {
      this.Vehicle.sharedJob.JobStarted(this.JobDef, this.pawn);
      GenClamor.DoClamor((Thing) paintVehicle.actor, 5f, VehicleClamorDefOf.VF_Painting);
    });
    paintVehicle.tickAction = (Action) (() =>
    {
      this.Vehicle.sharedJob.workDone += StatExtension.GetStatValue((Thing) paintVehicle.actor, StatDefOf.WorkSpeedGlobal, true, -1) / (30f * this.SizeMultiplier);
      if ((double) this.Vehicle.sharedJob.workDone < 1.0)
        return;
      this.Vehicle.SetColor();
      paintVehicle.actor.jobs.EndCurrentJob((JobCondition) 2, true, true);
    });
    paintVehicle.FailOnMoving<Toil>((TargetIndex) 1);
    ToilFailConditions.FailOnCannotTouch<Toil>(paintVehicle, (TargetIndex) 1, (PathEndMode) 2);
    ToilEffects.WithEffect(paintVehicle, ((BuildableDef) ((Thing) driverPaintVehicle.Vehicle).def).repairEffect, (TargetIndex) 1, new Color?());
    ToilEffects.WithProgressBar(paintVehicle, (TargetIndex) 1, (Func<float>) (() => this.Vehicle.sharedJob.workDone), false, -0.5f, false);
    paintVehicle.defaultCompleteMode = (ToilCompleteMode) 5;
    paintVehicle.activeSkill = (Func<SkillDef>) (() => SkillDefOf.Construction);
    paintVehicle.AddFinishAction((Action) (() => this.Vehicle.sharedJob.JobEnded(this.pawn)));
    yield return paintVehicle;
  }
}
