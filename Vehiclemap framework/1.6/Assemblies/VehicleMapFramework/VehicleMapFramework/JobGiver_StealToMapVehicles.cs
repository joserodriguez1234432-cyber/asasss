// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.JobGiver_StealToMapVehicles
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public class JobGiver_StealToMapVehicles : JobGiver_Steal
{
  private const float ItemsSearchRadiusOngoing = 12f;

  protected virtual Job TryGiveJob(Pawn pawn)
  {
    Thing thing;
    TargetInfo to;
    if (!StealToMapVehiclesAIUtility.TryFindBestItemToSteal(pawn, 12f, out thing, out to) || GenAI.InDangerousCombat(pawn))
      return (Job) null;
    Job job = JobMaker.MakeJob(JobDefOf.HaulToCell);
    job.targetA = LocalTargetInfo.op_Implicit(thing);
    job.targetB = LocalTargetInfo.op_Implicit(((TargetInfo) ref to).Cell);
    job.globalTarget = GlobalTargetInfo.op_Implicit(to);
    job.count = Mathf.Min(thing.stackCount, (int) ((double) StatExtension.GetStatValue((Thing) pawn, StatDefOf.CarryingCapacity, true, -1) / (double) thing.def.VolumePerUnit));
    return job;
  }
}
