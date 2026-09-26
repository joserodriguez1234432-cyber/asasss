// Decompiled with JetBrains decompiler
// Type: Vehicles.JobGiver_SabotageVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

public class JobGiver_SabotageVehicle : ThinkNode_JobGiver
{
  private float healthPct = 0.35f;
  private float maxDistance = 10f;

  public virtual ThinkNode DeepCopy(bool resolve = true)
  {
    JobGiver_SabotageVehicle giverSabotageVehicle = (JobGiver_SabotageVehicle) base.DeepCopy(resolve);
    giverSabotageVehicle.maxDistance = this.maxDistance;
    giverSabotageVehicle.healthPct = this.healthPct;
    return base.DeepCopy(resolve);
  }

  protected virtual Job TryGiveJob(Pawn pawn)
  {
    if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
      return (Job) null;
    Lord lord;
    if (!LordUtility.TryGetLord(pawn, ref lord))
      return (Job) null;
    foreach (Pawn ownedPawn in lord.ownedPawns)
    {
      VehiclePawn vehicle = ownedPawn as VehiclePawn;
      if (vehicle != null && !vehicle.CanMove && vehicle.AttachedExplosives <= 0 && (double) vehicle.statHandler.HealthPercent > (double) this.healthPct)
      {
        IntVec3 position = ((Thing) pawn).Position;
        if (((IntVec3) ref position).InHorDistOf(((Thing) vehicle).Position, this.maxDistance))
        {
          VehicleReservationManager resMgr = ((Thing) pawn).Map.GetCachedMapComponent<VehicleReservationManager>();
          if (resMgr.CanReserve(vehicle, pawn, JobDefOf_Vehicles.SabotageVehicle))
          {
            IntVec3 intVec3 = vehicle.SurroundingCells.RandomOrFallback<IntVec3>((Predicate<IntVec3>) (cell => resMgr.CanReserve<LocalTargetInfo, VehicleTargetReservation>(vehicle, pawn, LocalTargetInfo.op_Implicit(cell))), IntVec3.Invalid);
            return new Job(JobDefOf_Vehicles.SabotageVehicle, LocalTargetInfo.op_Implicit((Thing) vehicle), LocalTargetInfo.op_Implicit(intVec3));
          }
        }
      }
    }
    return (Job) null;
  }
}
