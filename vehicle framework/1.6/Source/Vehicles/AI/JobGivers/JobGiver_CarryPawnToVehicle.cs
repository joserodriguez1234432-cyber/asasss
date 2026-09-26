// Decompiled with JetBrains decompiler
// Type: Vehicles.JobGiver_CarryPawnToVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

public class JobGiver_CarryPawnToVehicle : ThinkNode_JobGiver
{
  protected virtual Job TryGiveJob(Pawn pawn)
  {
    if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
      return (Job) null;
    if (!(LordUtility.GetLord(pawn).LordJob is LordJob_FormAndSendVehicles lordJob))
      return (Job) null;
    Pawn downedPawn = JobGiver_CarryPawnToVehicle.FindDownedPawn(pawn);
    if (downedPawn == null)
      return (Job) null;
    AssignedSeat assignedSeat = lordJob.GetVehicleAssigned(downedPawn);
    if (assignedSeat == null)
    {
      VehicleRoleHandler availableVehicle = JobGiver_CarryPawnToVehicle.FindAvailableVehicle(downedPawn);
      if (availableVehicle != null)
        assignedSeat = new AssignedSeat(pawn, availableVehicle);
    }
    if (assignedSeat == null)
    {
      Log.ErrorOnce($"Unable to locate assigned or available vehicle for {downedPawn} in Caravan. Removing from caravan.", ((object) lordJob).GetHashCode());
      ((LordJob) lordJob).lord.RemovePawn(downedPawn);
      return (Job) null;
    }
    Job_Vehicle jobVehicle = new Job_Vehicle(JobDefOf_Vehicles.CarryPawnToVehicle, LocalTargetInfo.op_Implicit((Thing) downedPawn), LocalTargetInfo.op_Implicit((Thing) assignedSeat.Vehicle));
    jobVehicle.handler = assignedSeat.handler;
    jobVehicle.count = 1;
    return (Job) jobVehicle;
  }

  private static Pawn FindDownedPawn(Pawn pawn)
  {
    foreach (Pawn downedPawn in ((LordJob_FormAndSendCaravan) LordUtility.GetLord(pawn).LordJob).downedPawns)
    {
      if (downedPawn.Downed && downedPawn != pawn && ((Thing) downedPawn).Spawned && ReservationUtility.CanReserveAndReach(pawn, LocalTargetInfo.op_Implicit((Thing) downedPawn), (PathEndMode) 2, (Danger) 3, 1, -1, (ReservationLayerDef) null, false))
        return downedPawn;
    }
    return (Pawn) null;
  }

  private static VehicleRoleHandler FindAvailableVehicle(Pawn pawn)
  {
    LordJob_FormAndSendVehicles lordJob = (LordJob_FormAndSendVehicles) LordUtility.GetLord(pawn).LordJob;
    foreach (VehiclePawn vehicle in lordJob.vehicles)
    {
      foreach (VehicleRoleHandler handler in vehicle.handlers)
      {
        if (handler.CanOperateRole(pawn) && !lordJob.SeatAssigned(vehicle, handler))
          return handler;
      }
    }
    return (VehicleRoleHandler) null;
  }
}
