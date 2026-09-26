// Decompiled with JetBrains decompiler
// Type: Vehicles.SharedJob
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public sealed class SharedJob : IExposable
{
  public float workDone;
  private JobDef jobDef;
  private List<Pawn> pawnsOnJob = new List<Pawn>();

  public bool HasJob => this.jobDef != null;

  public bool SameJob(JobDef jobDef) => this.jobDef == jobDef;

  public void JobStarted(JobDef jobDef, Pawn pawn)
  {
    if (!jobDef.driverClass.IsSubclassOf(typeof (VehicleJobDriver)))
    {
      SmashLog.Error($"Attempting to start SharedJob with incompatible type <type>{jobDef.driverClass}</type>. SharedJobs must use <type>VehicleJobDriver</type> subtypes.");
    }
    else
    {
      this.pawnsOnJob.Add(pawn);
      if (this.SameJob(jobDef))
        return;
      this.jobDef = jobDef;
      this.workDone = 0.0f;
    }
  }

  public void JobEnded(Pawn pawn)
  {
    if (!this.pawnsOnJob.Remove(pawn))
      GenCollection.RemoveWhere<Pawn>((IList<Pawn>) this.pawnsOnJob, new Func<Pawn, bool>(InvalidPawn));
    if (this.pawnsOnJob.Count != 0)
      return;
    this.jobDef = (JobDef) null;
    this.workDone = 0.0f;

    static bool InvalidPawn(Pawn pawn)
    {
      return (pawn == null || !((Thing) pawn).Spawned ? 0 : (!pawn.Dead ? 1 : 0)) == 0;
    }
  }

  public void ExposeData()
  {
    Scribe_Defs.Look<JobDef>(ref this.jobDef, "jobDef");
    Scribe_Values.Look<float>(ref this.workDone, "workDone", 0.0f, false);
    Scribe_Collections.Look<Pawn>(ref this.pawnsOnJob, "pawnsOnJob", (LookMode) 3, Array.Empty<object>());
  }
}
