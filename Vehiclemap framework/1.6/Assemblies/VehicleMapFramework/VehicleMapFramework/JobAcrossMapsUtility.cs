// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.JobAcrossMapsUtility
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vehicles;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public static class JobAcrossMapsUtility
{
  private static readonly AccessTools.FieldRef<JobDriver, int> curToilIndex = AccessTools.FieldRefAccess<JobDriver, int>(nameof (curToilIndex));

  public static List<Type> WorkGiverClassesNonScanAll { get; } = new List<Type>(1)
  {
    typeof (WorkGiver_Fish)
  };

  public static List<Type> WorkGiverClassesNeedWrap { get; } = new List<Type>();

  public static List<Type> JobDriverClassesNeedWrap { get; } = new List<Type>(1)
  {
    typeof (JobDriver_RemoveFloor)
  };

  public static List<WorkGiverDef> DisabledCrossMapWorkGiverDefs { get; } = new List<WorkGiverDef>();

  public static void StartGotoDestMapJob(
    Pawn pawn,
    TargetInfo? exitSpot = null,
    TargetInfo? enterSpot = null,
    List<TraverseSpots> spotsQueue = null)
  {
    Pawn_JobTracker jobs1 = pawn.jobs;
    if (jobs1 == null || jobs1.curDriver is JobDriverAcrossMaps)
      return;
    Job nextJob = pawn.CurJob.Clone();
    JobDriver cachedDriver = nextJob.GetCachedDriver(pawn);
    JobAcrossMapsUtility.curToilIndex.Invoke(cachedDriver) = pawn.jobs.curDriver.CurToilIndex - 1;
    pawn.jobs.curDriver.globalFinishActions.Clear();
    Job job1 = JobAcrossMapsUtility.GotoDestMapJob(pawn, exitSpot, enterSpot, spotsQueue, nextJob);
    job1.playerForced = nextJob.playerForced;
    Pawn_JobTracker jobs2 = pawn.jobs;
    Job job2 = job1;
    bool? nullable1 = new bool?(true);
    JobTag? nullable2 = new JobTag?();
    bool? nullable3 = nullable1;
    jobs2.StartJob(job2, (JobCondition) 1, (ThinkNode) null, false, true, (ThinkTreeDef) null, nullable2, false, false, nullable3, false, true, true);
  }

  public static Job GotoDestMapJob(
    Pawn pawn,
    TargetInfo? exitSpot = null,
    TargetInfo? enterSpot = null,
    List<TraverseSpots> spotsQueue = null,
    Job nextJob = null)
  {
    if (!GenList.NullOrEmpty<TraverseSpots>((IList<TraverseSpots>) spotsQueue) && GenCollection.Any<TraverseSpots>(spotsQueue, (Predicate<TraverseSpots>) (s =>
    {
      TargetInfo exitSpot1 = s.exitSpot;
      if (((TargetInfo) ref exitSpot1).Map != null)
        return true;
      TargetInfo enterSpot1 = s.enterSpot;
      return ((TargetInfo) ref enterSpot1).Map != null;
    })))
      return JobMaker.MakeJob(VMF_DefOf.VMF_GotoDestMap).SetSpotsAndNextJob(pawn, spotsQueue, nextJob: nextJob);
    TargetInfo valueOrDefault;
    if (enterSpot.HasValue)
    {
      valueOrDefault = enterSpot.GetValueOrDefault();
      if (((TargetInfo) ref valueOrDefault).Map != null)
        goto label_6;
    }
    if (exitSpot.HasValue)
    {
      valueOrDefault = exitSpot.GetValueOrDefault();
      if (((TargetInfo) ref valueOrDefault).Map != null)
        goto label_6;
    }
    return nextJob;
label_6:
    Job job1 = JobMaker.MakeJob(VMF_DefOf.VMF_GotoDestMap);
    Pawn pawn1 = pawn;
    TargetInfo? exitSpotA = exitSpot;
    TargetInfo? enterSpotA = enterSpot;
    Job job2 = nextJob;
    TargetInfo? exitSpotB = new TargetInfo?();
    TargetInfo? enterSpotB = new TargetInfo?();
    Job nextJob1 = job2;
    return job1.SetSpotsAndNextJob(pawn1, exitSpotA, enterSpotA, exitSpotB, enterSpotB, nextJob1);
  }

  public static Job SetSpotsToJobAcrossMaps(
    this Job job,
    Pawn pawn,
    TargetInfo? exitSpot = null,
    TargetInfo? enterSpot = null,
    List<TraverseSpots> spotsQueue = null)
  {
    if (!(job.GetCachedDriver(pawn) is JobDriverAcrossMaps cachedDriver))
      return (Job) null;
    if (GenList.NullOrEmpty<TraverseSpots>((IList<TraverseSpots>) spotsQueue))
      cachedDriver.SetSpots(exitSpot, enterSpot);
    else
      cachedDriver.SetSpots(spotsQueue);
    return job;
  }

  public static Job SetSpotsAndNextJob(
    this Job job,
    Pawn pawn,
    List<TraverseSpots> spotsQueueA = null,
    List<TraverseSpots> spotsQueueB = null,
    Job nextJob = null)
  {
    if (!(job.GetCachedDriver(pawn) is JobDriver_GotoDestMap cachedDriver))
      return (Job) null;
    cachedDriver.SetSpots(spotsQueueA, spotsQueueB);
    cachedDriver.nextJob = nextJob;
    return job;
  }

  public static Job SetSpotsAndNextJob(
    this Job job,
    Pawn pawn,
    TargetInfo? exitSpotA = null,
    TargetInfo? enterSpotA = null,
    TargetInfo? exitSpotB = null,
    TargetInfo? enterSpotB = null,
    Job nextJob = null)
  {
    if (!(job.GetCachedDriver(pawn) is JobDriver_GotoDestMap cachedDriver))
      return (Job) null;
    cachedDriver.SetSpots(exitSpotA, enterSpotA, exitSpotB, enterSpotB);
    cachedDriver.nextJob = nextJob;
    return job;
  }

  public static Job NextJobOfGotoDestMapJob(Pawn pawn)
  {
    return !(pawn.jobs.curDriver is JobDriver_GotoDestMap curDriver) ? (Job) null : curDriver.nextJob;
  }

  public static bool NoNeedVirtualMapTransfer(Map pawnMap, Map targetMap, WorkGiverDef workGiver)
  {
    return pawnMap == targetMap || !VehicleMapUtility.get_CrossMapContext(pawnMap) || JobAcrossMapsUtility.DisabledCrossMapWorkGiverDefs.Contains(workGiver);
  }

  public static bool NoNeedWrapGotoDestMapJob(WorkGiver_Scanner scanner)
  {
    return scanner is WorkGiver_PaintFloor;
  }

  public static bool NeedWrapGotoDestMapJob(WorkGiver_Scanner scanner, Job job = null)
  {
    bool flag;
    switch (scanner)
    {
      case WorkGiver_Merge _:
      case WorkGiver_HunterHunt _:
      case WorkGiver_Miner _:
      case VehicleWorkGiver _:
        flag = true;
        break;
      default:
        flag = false;
        break;
    }
    return flag || job != null && JobAcrossMapsUtility.JobDriverClassesNeedWrap.Contains(job.def.driverClass) || JobAcrossMapsUtility.WorkGiverClassesNeedWrap.Contains(((WorkGiver) scanner).def.giverClass);
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u00245A55F0C633C07BCA6C3D410F79284FF7
  {
    [ExtensionMarker("<M>$19BE777892CC96E6B67DDE377BAFFB7A")]
    public Job SetSpotsToJobAcrossMaps(
      Pawn pawn,
      TargetInfo? exitSpot = null,
      TargetInfo? enterSpot = null,
      List<TraverseSpots> spotsQueue = null)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$19BE777892CC96E6B67DDE377BAFFB7A")]
    public Job SetSpotsAndNextJob(
      Pawn pawn,
      List<TraverseSpots> spotsQueueA = null,
      List<TraverseSpots> spotsQueueB = null,
      Job nextJob = null)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$19BE777892CC96E6B67DDE377BAFFB7A")]
    public Job SetSpotsAndNextJob(
      Pawn pawn,
      TargetInfo? exitSpotA = null,
      TargetInfo? enterSpotA = null,
      TargetInfo? exitSpotB = null,
      TargetInfo? enterSpotB = null,
      Job nextJob = null)
    {
      throw new NotSupportedException();
    }

    [SpecialName]
    public static class \u003CM\u003E\u002419BE777892CC96E6B67DDE377BAFFB7A
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(Job job)
      {
      }
    }
  }
}
