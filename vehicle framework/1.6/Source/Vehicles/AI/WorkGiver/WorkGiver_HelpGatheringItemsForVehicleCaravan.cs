// Decompiled with JetBrains decompiler
// Type: Vehicles.WorkGiver_HelpGatheringItemsForVehicleCaravan
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

public class WorkGiver_HelpGatheringItemsForVehicleCaravan : WorkGiver
{
  private static JobDef JobDef => JobDefOf_Vehicles.PrepareCaravan_GatheringVehicle;

  public virtual Job NonScanJob(Pawn pawn)
  {
    foreach (Lord lord in ((Thing) pawn).Map.lordManager.lords)
    {
      if (lord.LordJob is LordJob_FormAndSendVehicles lordJob && lordJob.GatherItemsNow)
      {
        List<TransferableOneWay> caravanTransferables = GatherItemsForVehicleCaravanUtility.GetCaravanTransferables(lord);
        Thing thingToPack = JobDriver_LoadVehicle.FindThingToPack(pawn, WorkGiver_HelpGatheringItemsForVehicleCaravan.JobDef, caravanTransferables, lord);
        if (thingToPack != null && WorkGiver_HelpGatheringItemsForVehicleCaravan.AnyReachableCarrierOrColonist(pawn, lord))
        {
          Job job = JobMaker.MakeJob(WorkGiver_HelpGatheringItemsForVehicleCaravan.JobDef, LocalTargetInfo.op_Implicit(thingToPack));
          job.lord = lord;
          return job;
        }
      }
    }
    return (Job) null;
  }

  private static bool AnyReachableCarrierOrColonist(Pawn forPawn, Lord lord)
  {
    foreach (Pawn ownedPawn in lord.ownedPawns)
    {
      if (ownedPawn is VehiclePawn carrier && !ForbidUtility.IsForbidden((Thing) carrier, forPawn) && GatherItemsForVehicleCaravanUtility.IsUsableCarrier((Pawn) carrier, forPawn) && ReachabilityUtility.CanReach(forPawn, LocalTargetInfo.op_Implicit((Thing) carrier), (PathEndMode) 2, (Danger) 3, false, false, (TraverseMode) 0))
        return true;
    }
    return false;
  }
}
