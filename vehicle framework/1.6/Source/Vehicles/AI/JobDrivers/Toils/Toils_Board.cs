// Decompiled with JetBrains decompiler
// Type: Vehicles.Toils_Board
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

internal class Toils_Board
{
  public static Toil BoardVehicle(Pawn pawn)
  {
    Toil toil = new Toil();
    toil.initAction = (Action) (() =>
    {
      LocalTargetInfo target = toil.actor.jobs.curJob.GetTarget((TargetIndex) 1);
      VehiclePawn thing = ((LocalTargetInfo) ref target).Thing as VehiclePawn;
      if (LordUtility.GetLord(pawn)?.LordJob is LordJob_FormAndSendVehicles lordJob2)
      {
        AssignedSeat vehicleAssigned = lordJob2.GetVehicleAssigned(pawn);
        vehicleAssigned.Vehicle.TryAddPawn(pawn, vehicleAssigned.handler);
      }
      else
      {
        thing.BoardPawn(pawn);
        Toils_Board.ThrowAppropriateHistoryEvent(thing.VehicleDef.type, toil.actor);
      }
    });
    toil.defaultCompleteMode = (ToilCompleteMode) 1;
    return toil;
  }

  private static void ThrowAppropriateHistoryEvent(VehicleType type, Pawn pawn)
  {
    if (!ModsConfig.IdeologyActive)
      return;
    switch (type)
    {
      case VehicleType.Sea:
        Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf_Vehicles.VF_BoardedSeaVehicle, NamedArgumentUtility.Named((object) pawn, HistoryEventArgsNames.Doer)), true);
        break;
      case VehicleType.Air:
        Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf_Vehicles.VF_BoardedAirVehicle, NamedArgumentUtility.Named((object) pawn, HistoryEventArgsNames.Doer)), true);
        break;
      case VehicleType.Land:
        Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf_Vehicles.VF_BoardedLandVehicle, NamedArgumentUtility.Named((object) pawn, HistoryEventArgsNames.Doer)), true);
        break;
      case VehicleType.Universal:
        Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf_Vehicles.VF_BoardedUniversalVehicle, NamedArgumentUtility.Named((object) pawn, HistoryEventArgsNames.Doer)), true);
        break;
      default:
        throw new NotImplementedException("VehicleType");
    }
  }
}
