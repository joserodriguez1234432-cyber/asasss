// Decompiled with JetBrains decompiler
// Type: Vehicles.JobGiver_BoardVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

public class JobGiver_BoardVehicle : ThinkNode_JobGiver
{
  private const float FollowRadius = 5f;

  protected virtual Job TryGiveJob(Pawn pawn)
  {
    if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
      return (Job) null;
    if (!(LordUtility.GetLord(pawn).LordJob is LordJob_FormAndSendVehicles lordJob))
      return (Job) null;
    AssignedSeat vehicleAssigned = lordJob.GetVehicleAssigned(pawn);
    if (vehicleAssigned?.Vehicle == null)
      return (Job) null;
    if (vehicleAssigned.handler != null)
      return new Job(JobDefOf_Vehicles.Board, LocalTargetInfo.op_Implicit((Thing) vehicleAssigned.Vehicle));
    if (!JobDriver_FollowClose.FarEnoughAndPossibleToStartJob(pawn, (Pawn) vehicleAssigned.Vehicle, 5f))
      return (Job) null;
    return new Job(JobDefOf.FollowClose, LocalTargetInfo.op_Implicit((Thing) vehicleAssigned.Vehicle))
    {
      lord = LordUtility.GetLord(pawn),
      expiryInterval = 140,
      checkOverrideOnExpire = true,
      followRadius = 5f
    };
  }
}
