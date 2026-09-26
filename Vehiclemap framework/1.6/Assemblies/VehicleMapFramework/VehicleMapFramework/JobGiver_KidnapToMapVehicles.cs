// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.JobGiver_KidnapToMapVehicles
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public class JobGiver_KidnapToMapVehicles : ThinkNode_JobGiver
{
  private const float VictimSearchRadiusOngoing = 18f;

  protected virtual Job TryGiveJob(Pawn pawn)
  {
    Pawn victim;
    TargetInfo to;
    if (!KidnapToMapVehiclesAIUtility.TryFindGoodKidnapVictim(pawn, 18f, out victim, out to) || GenAI.InDangerousCombat(pawn))
      return (Job) null;
    Job job = JobMaker.MakeJob(JobDefOf.HaulToCell);
    job.targetA = LocalTargetInfo.op_Implicit((Thing) victim);
    job.targetB = LocalTargetInfo.op_Implicit(((TargetInfo) ref to).Cell);
    job.globalTarget = GlobalTargetInfo.op_Implicit(to);
    job.count = 1;
    return job;
  }
}
