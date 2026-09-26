// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.PawnFlyer_PersistentJob
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public class PawnFlyer_PersistentJob : PawnFlyer
{
  private readonly AccessTools.FieldRef<PawnFlyer, JobQueue> jobQueue = AccessTools.FieldRefAccess<PawnFlyer, JobQueue>(nameof (jobQueue));

  public event Action OnLanded;

  protected virtual void RespawnPawn()
  {
    JobQueue source = this.jobQueue.Invoke((PawnFlyer) this);
    QueuedJob queuedJob = source != null ? ((IEnumerable<QueuedJob>) source).FirstOrDefault<QueuedJob>() : (QueuedJob) null;
    Pawn flyingPawn = this.FlyingPawn;
    bool flag;
    if (queuedJob != null)
    {
      Job job = queuedJob.job;
      if ((job == null || !(job.verbToUse is Verb_LaunchZipline)) && flyingPawn != null)
      {
        flag = true;
        goto label_4;
      }
    }
    flag = false;
label_4:
    if (flag)
    {
      Job job = queuedJob.job;
      queuedJob.job = JobMaker.MakeJob(JobDefOf.Wait_Combat);
      base.RespawnPawn();
      flyingPawn.jobs.StartJob(job, (JobCondition) 16 /*0x10*/, (ThinkNode) null, false, true, (ThinkTreeDef) null, new JobTag?(), false, false, new bool?(), false, true, false);
      Action onLanded = this.OnLanded;
      if (onLanded == null)
        return;
      onLanded();
    }
    else
    {
      base.RespawnPawn();
      Action onLanded = this.OnLanded;
      if (onLanded == null)
        return;
      onLanded();
    }
  }
}
