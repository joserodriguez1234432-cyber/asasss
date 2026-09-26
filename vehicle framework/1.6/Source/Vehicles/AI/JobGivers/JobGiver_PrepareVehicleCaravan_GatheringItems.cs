// Decompiled with JetBrains decompiler
// Type: Vehicles.JobGiver_PrepareVehicleCaravan_GatheringItems
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

public class JobGiver_PrepareVehicleCaravan_GatheringItems : ThinkNode_JobGiver
{
  private static JobDef JobDef => JobDefOf_Vehicles.PrepareCaravan_GatheringVehicle;

  protected virtual Job TryGiveJob(Pawn pawn)
  {
    if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
      return (Job) null;
    Lord lord = LordUtility.GetLord(pawn);
    List<TransferableOneWay> caravanTransferables = GatherItemsForVehicleCaravanUtility.GetCaravanTransferables(lord);
    Thing thingToPack = JobDriver_LoadVehicle.FindThingToPack(pawn, JobGiver_PrepareVehicleCaravan_GatheringItems.JobDef, caravanTransferables, lord);
    if (thingToPack == null)
      return (Job) null;
    return new Job(JobGiver_PrepareVehicleCaravan_GatheringItems.JobDef, LocalTargetInfo.op_Implicit(thingToPack))
    {
      lord = lord
    };
  }
}
