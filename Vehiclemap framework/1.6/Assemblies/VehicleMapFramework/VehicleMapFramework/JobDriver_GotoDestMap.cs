// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.JobDriver_GotoDestMap
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public class JobDriver_GotoDestMap : JobDriverAcrossMaps
{
  public Job nextJob;

  protected virtual string ReportStringProcessed(string str) => this.nextJob?.GetReport(this.pawn);

  public virtual bool TryMakePreToilReservations(bool errorOnFailed)
  {
    using (new VirtualTeleporter((Thing) this.pawn, this.DestMap))
    {
      Job nextJob = this.nextJob;
      return nextJob == null || nextJob.TryMakePreToilReservations(this.pawn, false);
    }
  }

  public virtual void Notify_Starting()
  {
    base.Notify_Starting();
    this.nextJob?.GetCachedDriver(this.pawn).Notify_Starting();
  }

  protected override IEnumerable<Toil> MakeNewToils()
  {
    JobDriver_GotoDestMap driverGotoDestMap = this;
    // ISSUE: reference to a compiler-generated method
    foreach (Toil toil in driverGotoDestMap.\u003C\u003En__0())
      yield return toil;
    foreach (Toil gotoTarget in driverGotoDestMap.GotoTargetMap((TargetIndex) 1))
      yield return gotoTarget;
    if (driverGotoDestMap.nextJob != null)
      yield return driverGotoDestMap.TryStartNextJob();
  }

  public override void ExposeData()
  {
    Scribe_Deep.Look<Job>(ref this.nextJob, "nextJob", Array.Empty<object>());
    base.ExposeData();
  }

  private Toil TryStartNextJob()
  {
    Toil toil = ToilMaker.MakeToil(nameof (TryStartNextJob));
    toil.defaultCompleteMode = (ToilCompleteMode) 1;
    toil.initAction = (Action) (() =>
    {
      ref bool local = ref this.nextJob.def.allowOpportunisticPrefix;
      bool flag = local;
      try
      {
        local = false;
        this.pawn.ClearAllReservations(true);
        Pawn_JobTracker jobs = this.pawn.jobs;
        Job nextJob = this.nextJob;
        ThinkNode thinkRoot = VMF_DefOf.VMF_GotoDestMapThinkTree.thinkRoot;
        ThinkTreeDef destMapThinkTree = VMF_DefOf.VMF_GotoDestMapThinkTree;
        bool? nullable1 = new bool?(true);
        JobTag? nullable2 = new JobTag?();
        bool? nullable3 = nullable1;
        jobs.StartJob(nextJob, (JobCondition) 1, thinkRoot, false, true, destMapThinkTree, nullable2, false, false, nullable3, false, true, true);
      }
      finally
      {
        local = flag;
      }
    });
    return toil;
  }

  public class ThinkNode_JobFromGotoDestMap : ThinkNode
  {
    public virtual ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
    {
      return ThinkResult.NoJob;
    }
  }
}
