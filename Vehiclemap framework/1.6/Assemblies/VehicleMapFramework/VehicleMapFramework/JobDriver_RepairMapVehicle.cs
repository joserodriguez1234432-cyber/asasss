// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.JobDriver_RepairMapVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public class JobDriver_RepairMapVehicle : JobDriver_RepairVehicle
{
  protected virtual JobDef JobDef => VMF_DefOf.VMF_RepairMapVehicle;

  public virtual bool TryMakePreToilReservations(bool errorOnFailed)
  {
    return ReservationUtility.Reserve(((JobDriver) this).pawn, ((JobDriver) this).job.GetTarget((TargetIndex) 2), ((JobDriver) this).job, 1, -1, (ReservationLayerDef) null, errorOnFailed, false);
  }

  protected virtual IEnumerable<Toil> MakeNewToils()
  {
    JobDriver_RepairMapVehicle repairMapVehicle = this;
    ToilFailConditions.FailOn<JobDriver_RepairMapVehicle>(repairMapVehicle, (Func<bool>) (() => !VehicleMapUtility.get_IsOnVehicleMap((Thing) ((JobDriver) this).pawn)));
    yield return Toils_Goto.GotoCell((TargetIndex) 2, (PathEndMode) 3);
    Toil workToil = ToilMaker.MakeToil(nameof (MakeNewToils));
    workToil.initAction = new Action(((JobDriver_WorkVehicle) repairMapVehicle).ResetWork);
    workToil.tickIntervalAction = new Action<int>(WorkAction);
    if (((JobDriver_WorkVehicle) repairMapVehicle).EffecterDef != null)
      ToilEffects.WithEffect(workToil, ((JobDriver_WorkVehicle) repairMapVehicle).EffecterDef, (TargetIndex) 1, new Color?());
    else
      ToilEffects.WithProgressBar(workToil, (TargetIndex) 1, new Func<float>(((JobDriver_WorkVehicle) repairMapVehicle).GetProgressPct), false, -0.5f, false);
    workToil.defaultCompleteMode = ((JobDriver_WorkVehicle) repairMapVehicle).ToilCompleteMode;
    workToil.defaultDuration = 2000;
    if (((JobDriver_WorkVehicle) repairMapVehicle).Skill != null)
      workToil.activeSkill = (Func<SkillDef>) (() => ((JobDriver_WorkVehicle) this).Skill);
    yield return workToil;

    void WorkAction(int interval)
    {
      Pawn actor = workToil.actor;
      if (((JobDriver_WorkVehicle) this).Skill != null)
        actor.skills?.Learn(((JobDriver_WorkVehicle) this).Skill, ((JobDriver_WorkVehicle) this).SkillAmount * (float) interval, false, false);
      ((JobDriver_WorkVehicle) this).Work = ((JobDriver_WorkVehicle) this).Work - StatExtension.GetStatValue((Thing) actor, ((JobDriver_WorkVehicle) this).Stat, true, -1) * (float) interval;
      if ((double) ((JobDriver_WorkVehicle) this).Work > 0.0)
        return;
      ((JobDriver_WorkVehicle) this).WorkComplete(actor);
    }
  }

  protected virtual void WorkComplete(Pawn actor)
  {
    IntVec2 hitbox = VehicleMapUtility.MapCellToHitbox((VehiclePawnWithMap) ((VehicleJobDriver) this).Vehicle);
    LocalTargetInfo targetB = ((JobDriver) this).TargetB;
    IntVec3 cell1 = ((LocalTargetInfo) ref targetB).Cell;
    IntVec2 toIntVec2 = ((IntVec3) ref cell1).ToIntVec2;
    IntVec2 cell = IntVec2.op_Addition(hitbox, toIntVec2);
    VehicleComponent vehicleComponent = GenCollection.FirstOrDefault<VehicleComponent>(((VehicleJobDriver) this).Vehicle.statHandler.ComponentsPrioritized, (Predicate<VehicleComponent>) (c => c.props.hitbox.Hitbox.Contains(cell) && (double) c.HealthPercent < 1.0));
    if (vehicleComponent == null)
    {
      if (((Thing) ((VehicleJobDriver) this).Vehicle).Spawned)
        MapComponentCache<ListerVehiclesRepairable>.GetComponent(((Thing) ((VehicleJobDriver) this).Vehicle).Map).NotifyVehicleRepaired(((VehicleJobDriver) this).Vehicle);
      actor.records.Increment(RecordDefOf.ThingsRepaired);
      actor.jobs.EndCurrentJob((JobCondition) 2, true, true);
    }
    else
    {
      ((JobDriver_WorkVehicle) this).ResetWork();
      vehicleComponent.HealComponent(((VehicleJobDriver) this).Vehicle.GetStatValue(VehicleStatDefOf.RepairRate));
      ((VehicleJobDriver) this).Vehicle.Transform.rotation = 0.0f;
      if (((GraphicData) ((VehicleJobDriver) this).Vehicle.VehicleDef.graphicData).drawRotated)
        return;
      ((Thing) ((VehicleJobDriver) this).Vehicle).Rotation = ((BuildableDef) ((VehicleJobDriver) this).Vehicle.VehicleDef).defaultPlacingRot;
    }
  }
}
