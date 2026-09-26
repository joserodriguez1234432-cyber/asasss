// Decompiled with JetBrains decompiler
// Type: Vehicles.JobGiver_GotoNearestHostile
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class JobGiver_GotoNearestHostile : ThinkNode_JobGiver
{
  private const int ExpiryInterval = 360;
  private LocomotionUrgency urgency = (LocomotionUrgency) 3;
  private bool ignoreNonCombatants;
  private bool humanlikesOnly;
  private int overrideExpiryInterval = -1;
  private int overrideInstancedExpiryInterval = -1;

  public virtual ThinkNode DeepCopy(bool resolve = true)
  {
    JobGiver_GotoNearestHostile gotoNearestHostile = (JobGiver_GotoNearestHostile) base.DeepCopy(resolve);
    gotoNearestHostile.ignoreNonCombatants = this.ignoreNonCombatants;
    gotoNearestHostile.humanlikesOnly = this.humanlikesOnly;
    gotoNearestHostile.overrideExpiryInterval = this.overrideExpiryInterval;
    gotoNearestHostile.overrideInstancedExpiryInterval = this.overrideInstancedExpiryInterval;
    return (ThinkNode) gotoNearestHostile;
  }

  protected virtual Job TryGiveJob(Pawn pawn)
  {
    VehiclePawn vehicle = pawn as VehiclePawn;
    float num = float.MaxValue;
    Thing thing1 = (Thing) null;
    List<IAttackTarget> potentialTargetsFor = ((Thing) vehicle).Map.attackTargetsCache.GetPotentialTargetsFor((IAttackTargetSearcher) vehicle);
    for (int index = 0; index < potentialTargetsFor.Count; ++index)
    {
      IAttackTarget iattackTarget = potentialTargetsFor[index];
      if (!iattackTarget.ThreatDisabled((IAttackTargetSearcher) vehicle) && AttackTargetFinder.IsAutoTargetable(iattackTarget) && (!this.humanlikesOnly || !(iattackTarget.Thing is Pawn thing3) || thing3.RaceProps.Humanlike))
      {
        Thing thing2 = (Thing) iattackTarget;
        int squared = IntVec3Utility.DistanceToSquared(thing2.Position, ((Thing) vehicle).Position);
        if ((double) squared < (double) num && vehicle.CanReachVehicle(LocalTargetInfo.op_Implicit(thing2.Position), (PathEndMode) 2, (Danger) 3, (TraverseMode) 0))
        {
          num = (float) squared;
          thing1 = thing2;
        }
      }
    }
    if (thing1 == null)
      return (Job) null;
    float radius = thing1 is VehiclePawn vehiclePawn ? (float) (Mathf.Max(((BuildableDef) vehiclePawn.VehicleDef).Size.x, ((BuildableDef) vehiclePawn.VehicleDef).Size.z) * 2) : 10f;
    IntVec3 result;
    if (!PathingHelper.TryFindNearestStandableCell(vehicle, thing1.Position, out result, radius))
    {
      Log.Error($"Couldn't find standable cell near {thing1}");
      return (Job) null;
    }
    Job job = JobMaker.MakeJob(JobDefOf.Goto, LocalTargetInfo.op_Implicit(result));
    job.locomotionUrgency = this.urgency;
    job.checkOverrideOnExpire = true;
    job.expiryInterval = this.overrideExpiryInterval > 0 ? this.overrideExpiryInterval : 360;
    job.collideWithPawns = true;
    return job;
  }
}
