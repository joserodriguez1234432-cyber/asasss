// Decompiled with JetBrains decompiler
// Type: Vehicles.JobGiver_FollowVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class JobGiver_FollowVehicle : JobGiver_AIFollowPawn
{
  protected virtual int FollowJobExpireInterval => 100;

  protected virtual Pawn GetFollowee(Pawn pawn)
  {
    return (Pawn) ((LocalTargetInfo) ref pawn.mindState.duty.focus).Thing;
  }

  protected virtual float GetRadius(Pawn pawn) => pawn.mindState.duty.radius;

  protected virtual Job TryGiveJob(Pawn pawn)
  {
    if (!(base.GetFollowee(pawn) is VehiclePawn followee))
    {
      Log.Error($"{((object) this).GetType()} has null followee vehicle. pawn=\"{Gen.ToStringSafe<Pawn>(pawn)}\"");
      return (Job) null;
    }
    if (!((Thing) followee).Spawned)
      return (Job) null;
    if (!ReachabilityUtility.CanReach(pawn, LocalTargetInfo.op_Implicit(followee.FollowerCell), (PathEndMode) 1, (Danger) 3, false, false, (TraverseMode) 0))
    {
      followee.RecalculateFollowerCell();
      if (!ReachabilityUtility.CanReach(pawn, LocalTargetInfo.op_Implicit(followee.FollowerCell), (PathEndMode) 1, (Danger) 3, false, false, (TraverseMode) 0))
        return (Job) null;
    }
    float radius = base.GetRadius(pawn);
    Job job = JobMaker.MakeJob(JobDefOf_Vehicles.FollowVehicle, LocalTargetInfo.op_Implicit((Thing) followee));
    job.expiryInterval = base.FollowJobExpireInterval;
    job.checkOverrideOnExpire = true;
    job.followRadius = radius;
    return job;
  }
}
