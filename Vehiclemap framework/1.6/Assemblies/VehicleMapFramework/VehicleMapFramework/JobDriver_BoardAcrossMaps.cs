// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.JobDriver_BoardAcrossMaps
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using SmashTools.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using VehicleMapFramework.VMF_HarmonyPatches;
using Vehicles;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace VehicleMapFramework;

[StaticConstructorOnStartup]
public class JobDriver_BoardAcrossMaps : JobDriverAcrossMaps
{
  public virtual bool TryMakePreToilReservations(bool errorOnFailed) => true;

  protected override IEnumerable<Toil> MakeNewToils()
  {
    JobDriver_BoardAcrossMaps driverBoardAcrossMaps = this;
    ToilFailConditions.FailOnDespawnedOrNull<JobDriver_BoardAcrossMaps>(driverBoardAcrossMaps, (TargetIndex) 1);
    ToilFailConditions.FailOnForbidden<JobDriver_BoardAcrossMaps>(driverBoardAcrossMaps, (TargetIndex) 1);
    foreach (Toil gotoTarget in driverBoardAcrossMaps.GotoTargetMap((TargetIndex) 1))
      yield return gotoTarget;
    yield return Toils_Goto.GotoThing((TargetIndex) 1, (PathEndMode) 2, false);
    yield return JobDriver_BoardAcrossMaps.BoardVehicle(driverBoardAcrossMaps.pawn);
  }

  private static Toil BoardVehicle(Pawn pawnBoarding)
  {
    Toil toil = new Toil();
    toil.initAction = (Action) (() =>
    {
      LocalTargetInfo target1 = pawnBoarding.jobs.curJob.GetTarget((TargetIndex) 1);
      Thing target = ((LocalTargetInfo) ref target1).Thing;
      if (!(target is VehiclePawn vehiclePawn2))
      {
        VehiclePawnWithMap vehicle;
        if (!target.IsOnVehicleMapOf(out vehicle))
        {
          VMF_Log.Error("TargetA of JobDriver_BoardAcrossMaps must be VehiclePawn or on vehicle map.");
          return;
        }
        vehiclePawn2 = (VehiclePawn) vehicle;
      }
      if (LordUtility.GetLord(pawnBoarding)?.LordJob is LordJob_FormAndSendVehicles lordJob2)
      {
        AssignedSeat assignedSeat = Patch_JobDriver_Board_MakeNewToils.GetAssignedSeat(lordJob2, pawnBoarding);
        assignedSeat.Vehicle.TryAddPawn(pawnBoarding, assignedSeat.handler);
      }
      else
      {
        vehiclePawn2.BoardPawn(pawnBoarding);
        JobDriver_BoardAcrossMaps.ThrowAppropriateHistoryEvent(vehiclePawn2.VehicleDef.type, toil.actor);
      }
      VehicleRoleHandlerBuildable handlerBuildable = vehiclePawn2.handlers.OfType<VehicleRoleHandlerBuildable>().FirstOrDefault<VehicleRoleHandlerBuildable>((Func<VehicleRoleHandlerBuildable, bool>) (h => h.role is VehicleRoleBuildable role2 && role2.upgradeComp.parent == target));
      if (handlerBuildable == null)
        return;
      ParallelRenderer.SetDirty((IParallelRenderer) handlerBuildable);
    });
    toil.defaultCompleteMode = (ToilCompleteMode) 1;
    return toil;
  }

  private static void ThrowAppropriateHistoryEvent(VehicleType type, Pawn pawn)
  {
    if (!ModsConfig.IdeologyActive)
      return;
    switch ((int) type)
    {
      case 0:
        Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf_Vehicles.VF_BoardedSeaVehicle, NamedArgumentUtility.Named((object) pawn, HistoryEventArgsNames.Doer)), true);
        break;
      case 1:
        Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf_Vehicles.VF_BoardedAirVehicle, NamedArgumentUtility.Named((object) pawn, HistoryEventArgsNames.Doer)), true);
        break;
      case 2:
        Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf_Vehicles.VF_BoardedLandVehicle, NamedArgumentUtility.Named((object) pawn, HistoryEventArgsNames.Doer)), true);
        break;
      case 3:
        Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf_Vehicles.VF_BoardedUniversalVehicle, NamedArgumentUtility.Named((object) pawn, HistoryEventArgsNames.Doer)), true);
        break;
    }
  }
}
