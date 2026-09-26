// Decompiled with JetBrains decompiler
// Type: Vehicles.JobGiver_CombatFormation
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using System;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

[StaticConstructorOnStartup]
public abstract class JobGiver_CombatFormation : ThinkNode_JobGiver
{
  protected static readonly Action<Pawn_MindState> Notify_EngagedTarget = (Action<Pawn_MindState>) Delegate.CreateDelegate(typeof (Action<Pawn_MindState>), AccessTools.Method(typeof (Pawn_MindState), nameof (Notify_EngagedTarget), (System.Type[]) null, (System.Type[]) null));
  protected bool humanlikesOnly = true;
  protected bool ignoreNonCombatants;

  protected virtual IntRange ExpiryInterval => new IntRange(30, 30);

  protected virtual int TicksSinceEngageToLoseTarget => 400;

  protected virtual bool OnlyUseRanged => true;

  protected abstract bool TryFindCombatPosition(VehiclePawn vehicle, out IntVec3 dest);

  protected virtual float TargetAcquireRadius(VehiclePawn vehicle) => 56f;

  protected virtual bool CanRam(VehiclePawn vehicle) => false;

  protected virtual float GetFlagRadius(VehiclePawn vehicle) => 999999f;

  protected virtual IntVec3 GetFlagPosition(VehiclePawn vehicle) => IntVec3.Invalid;

  protected virtual bool ExtraTargetValidator(VehiclePawn vehicle, Thing target)
  {
    if (!FactionUtility.HostileTo(target.Faction, ((Thing) vehicle).Faction))
      return false;
    return !this.humanlikesOnly || !(target is Pawn pawn) || pawn.RaceProps.Humanlike;
  }

  public virtual ThinkNode DeepCopy(bool resolve = true)
  {
    JobGiver_CombatFormation giverCombatFormation = (JobGiver_CombatFormation) base.DeepCopy(resolve);
    giverCombatFormation.humanlikesOnly = this.humanlikesOnly;
    giverCombatFormation.ignoreNonCombatants = this.ignoreNonCombatants;
    return (ThinkNode) giverCombatFormation;
  }

  protected virtual Job TryGiveJob(Pawn pawn)
  {
    VehiclePawn vehicle = pawn as VehiclePawn;
    this.UpdateEnemyTarget(vehicle);
    Thing enemyTarget = vehicle.mindState.enemyTarget;
    if (enemyTarget == null)
      return (Job) null;
    if (enemyTarget is Pawn pawn1 && InvisibilityUtility.IsPsychologicallyInvisible(pawn1))
      return (Job) null;
    if (!this.OnlyUseRanged)
      return (Job) null;
    IntVec3 dest;
    if (!this.TryFindCombatPosition(vehicle, out dest))
      return (Job) null;
    Job job1 = !IntVec3.op_Equality(dest, ((Thing) vehicle).Position) ? JobMaker.MakeJob(JobDefOf.Goto, LocalTargetInfo.op_Implicit(dest)) : JobMaker.MakeJob(JobDefOf_Vehicles.IdleVehicle, LocalTargetInfo.op_Implicit((Thing) vehicle));
    Job job2 = job1;
    IntRange expiryInterval = this.ExpiryInterval;
    int randomInRange = ((IntRange) ref expiryInterval).RandomInRange;
    job2.expiryInterval = randomInRange;
    job1.checkOverrideOnExpire = true;
    return job1;
  }

  protected virtual bool ShouldLoseTarget(VehiclePawn vehicle)
  {
    Thing enemyTarget = vehicle.mindState.enemyTarget;
    float num = Mathf.Pow(vehicle.VehicleDef.npcProperties.targetKeepRadius, 2f);
    if (!enemyTarget.Destroyed && enemyTarget.Spawned && Find.TickManager.TicksGame - vehicle.mindState.lastEngageTargetTick <= this.TicksSinceEngageToLoseTarget && vehicle.CanReachVehicle(LocalTargetInfo.op_Implicit(enemyTarget), (PathEndMode) 2, (Danger) 3, (TraverseMode) 0))
    {
      IntVec3 intVec3 = IntVec3.op_Subtraction(((Thing) vehicle).Position, enemyTarget.Position);
      if ((double) ((IntVec3) ref intVec3).LengthHorizontalSquared <= (double) num)
        return enemyTarget is IAttackTarget iattackTarget && iattackTarget.ThreatDisabled((IAttackTargetSearcher) vehicle);
    }
    return true;
  }

  protected abstract void UpdateEnemyTarget(VehiclePawn vehicle);
}
