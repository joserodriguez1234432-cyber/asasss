// Decompiled with JetBrains decompiler
// Type: Vehicles.JobGiver_SendSlavesToVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

public class JobGiver_SendSlavesToVehicle : ThinkNode_JobGiver
{
  protected virtual Job TryGiveJob(Pawn pawn)
  {
    if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
      return (Job) null;
    Pawn prisoner = this.FindPrisoner(pawn);
    if (prisoner == null)
      return (Job) null;
    this.FindShipToDeposit(pawn, prisoner).handlers.Find((Predicate<VehicleRoleHandler>) (x => x.role.HandlingTypes == HandlingType.None));
    return new Job(JobDefOf.PrepareCaravan_GatherDownedPawns, LocalTargetInfo.op_Implicit((Thing) prisoner))
    {
      count = 1
    };
  }

  private Pawn FindPrisoner(Pawn pawn)
  {
    foreach (Pawn prisoner in ((LordJob_FormAndSendVehicles) LordUtility.GetLord(pawn).LordJob).prisoners)
    {
      if (prisoner != pawn && ((Thing) prisoner).Spawned && ReservationUtility.CanReserveAndReach(pawn, LocalTargetInfo.op_Implicit((Thing) prisoner), (PathEndMode) 2, (Danger) 3, 1, -1, (ReservationLayerDef) null, false))
        return prisoner;
    }
    return (Pawn) null;
  }

  private VehiclePawn FindShipToDeposit(Pawn pawn, Pawn downedPawn)
  {
    return GenCollection.MaxBy<VehiclePawn, int>((IEnumerable<VehiclePawn>) LordUtility.GetLord(pawn).ownedPawns.Where<Pawn>((Func<Pawn, bool>) (x => x is VehiclePawn)).Cast<VehiclePawn>().ToList<VehiclePawn>(), (Func<VehiclePawn, int>) (x => x.VehicleDef.properties.roles.Find((Predicate<VehicleRole>) (y => y.HandlingTypes == HandlingType.None)).Slots));
  }
}
