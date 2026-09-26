// Decompiled with JetBrains decompiler
// Type: Vehicles.JobGiver_ExitMap
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public abstract class JobGiver_ExitMap : ThinkNode_JobGiver
{
  protected LocomotionUrgency defaultLocomotion;
  protected int jobMaxDuration = 999999;
  protected bool forceDitchIfCantReachMapEdge;
  protected bool delayDitchIfActiveThreat;
  protected bool sabotageVehicleOnDitch = true;
  protected bool failIfCantJoinOrCreateCaravan;

  protected abstract bool TryFindGoodExitDest(VehiclePawn vehicle, out IntVec3 cell);

  public virtual ThinkNode DeepCopy(bool resolve = true)
  {
    JobGiver_ExitMap jobGiverExitMap = (JobGiver_ExitMap) base.DeepCopy(resolve);
    jobGiverExitMap.defaultLocomotion = this.defaultLocomotion;
    jobGiverExitMap.jobMaxDuration = this.jobMaxDuration;
    jobGiverExitMap.delayDitchIfActiveThreat = this.delayDitchIfActiveThreat;
    jobGiverExitMap.forceDitchIfCantReachMapEdge = this.forceDitchIfCantReachMapEdge;
    jobGiverExitMap.failIfCantJoinOrCreateCaravan = this.failIfCantJoinOrCreateCaravan;
    return (ThinkNode) jobGiverExitMap;
  }

  protected virtual Job TryGiveJob(Pawn pawn)
  {
    VehiclePawn vehicle = pawn as VehiclePawn;
    bool flag1 = !((Thing) vehicle).Map.GetCachedMapComponent<VehiclePathingSystem>()[vehicle.VehicleDef].VehicleReachability.CanReachMapEdge(((Thing) vehicle).Position, TraverseParms.For((Pawn) vehicle, (Danger) 3, (TraverseMode) 0, false, false, false, true));
    bool flag2 = ((Thing) vehicle).Faction != null && GenHostility.AnyHostileActiveThreatTo(((Thing) vehicle).Map, ((Thing) vehicle).Faction, false, false);
    IntVec3 cell;
    if (!this.TryFindGoodExitDest(vehicle, out cell))
    {
      bool flag3 = !vehicle.CanMoveFinal || !flag1;
      return (Job) null;
    }
    if (vehicle.VehicleDef.npcProperties != null)
    {
      int num = vehicle.VehicleDef.npcProperties.reverseWhileFleeing ? 1 : 0;
    }
    Job job = JobMaker.MakeJob(JobDefOf.Goto, LocalTargetInfo.op_Implicit(cell));
    job.exitMapOnArrival = true;
    job.failIfCantJoinOrCreateCaravan = this.failIfCantJoinOrCreateCaravan;
    job.locomotionUrgency = PawnUtility.ResolveLocomotion((Pawn) vehicle, this.defaultLocomotion, (LocomotionUrgency) 3);
    job.expiryInterval = this.jobMaxDuration;
    return job;
  }
}
