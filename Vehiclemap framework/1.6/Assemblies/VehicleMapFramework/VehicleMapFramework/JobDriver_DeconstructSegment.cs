// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.JobDriver_DeconstructSegment
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public class JobDriver_DeconstructSegment : JobDriver_RemoveBuilding
{
  protected virtual DesignationDef Designation => VMF_DefOf.VMF_RemoveSegment;

  protected virtual EffecterDef WorkEffecter => (EffecterDef) null;

  protected virtual float TotalNeededWork
  {
    get
    {
      return Mathf.Clamp(StatExtension.GetStatValue((Thing) this.Building, StatDefOf.WorkToBuild, true, -1), 20f, 3000f);
    }
  }

  protected virtual IEnumerable<Toil> MakeNewToils()
  {
    JobDriver_DeconstructSegment deconstructSegment = this;
    // ISSUE: reference to a compiler-generated method
    ToilFailConditions.FailOn<JobDriver_DeconstructSegment>(deconstructSegment, new Func<bool>(deconstructSegment.\u003CMakeNewToils\u003Eb__6_0));
    // ISSUE: reference to a compiler-generated method
    foreach (Toil toil1 in deconstructSegment.\u003C\u003En__0())
    {
      yield return toil1;
      if (toil1.debugName == "GotoThing")
      {
        Toil toil2 = ToilMaker.MakeToil(nameof (MakeNewToils));
        // ISSUE: reference to a compiler-generated method
        toil2.initAction = new Action(deconstructSegment.\u003CMakeNewToils\u003Eb__6_1);
        toil2.defaultCompleteMode = (ToilCompleteMode) 2;
        yield return toil2;
      }
    }
  }

  protected virtual void FinishedRemoving()
  {
    Thing.allowDestroyNonDestroyable = true;
    this.Target.Destroy((DestroyMode) 4);
    Thing.allowDestroyNonDestroyable = false;
    ((JobDriver) this).pawn.records.Increment(RecordDefOf.ThingsDeconstructed);
  }

  protected virtual void TickActionInterval(int delta)
  {
    if (((JobDriver) this).pawn.skills == null || CostListCalculator.CostListAdjusted((BuildableDef) ((Thing) this.Building).def, ((Thing) this.Building).Stuff, true).Count <= 0)
      return;
    ((JobDriver) this).pawn.skills.Learn(SkillDefOf.Construction, 0.25f * (float) delta, false, false);
  }
}
