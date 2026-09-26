// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.JobGiver_BoardMapVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using SmashTools;
using System.Collections.Generic;
using Vehicles;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public class JobGiver_BoardMapVehicle : ThinkNode_JobGiver
{
  protected virtual Job TryGiveJob(Pawn pawn)
  {
    VehiclePawnWithMap vehicle;
    if (!((Thing) pawn).IsOnVehicleMapOf(out vehicle) || ((Thing) pawn).Faction != ((Thing) vehicle).Faction || vehicle.HasEnoughOperators)
      return (Job) null;
    Map map = ((Thing) vehicle).Map;
    VehicleReservationManager cachedMapComponent = map != null ? ComponentCache.GetCachedMapComponent<VehicleReservationManager>(map) : (VehicleReservationManager) null;
    foreach (VehicleRoleHandler handler in vehicle.Handlers)
    {
      if (handler.AreSlotsAvailableAndReservable && CanOperateRole(pawn, handler.role.HandlingTypes) && handler.RequiredForMovement)
      {
        ThingWithComps thingWithComps = handler.role is VehicleRoleBuildable role ? role.upgradeComp.parent : (ThingWithComps) vehicle;
        TargetInfo exitSpot;
        TargetInfo enterSpot;
        List<TraverseSpots> spotsQueue;
        if (pawn.CanReach(LocalTargetInfo.op_Implicit((Thing) thingWithComps), (PathEndMode) 2, (Danger) 3, false, false, (TraverseMode) 0, ((Thing) thingWithComps).Map, out exitSpot, out enterSpot, out spotsQueue))
        {
          Job jobAcrossMaps = JobMaker.MakeJob(VMF_DefOf.VMF_BoardAcrossMaps, LocalTargetInfo.op_Implicit((Thing) thingWithComps)).SetSpotsToJobAcrossMaps(pawn, new TargetInfo?(exitSpot), new TargetInfo?(enterSpot), spotsQueue);
          vehicle.GiveLoadJob(pawn, handler);
          cachedMapComponent?.Reserve<VehicleRoleHandler, VehicleHandlerReservation>((VehiclePawn) vehicle, pawn, jobAcrossMaps, handler);
          return jobAcrossMaps;
        }
      }
    }
    return (Job) null;

    static bool CanOperateRole(Pawn pawn, HandlingType handlingType)
    {
      return handlingType == null || ((handlingType & 2) == null || pawn.IsPlayerControlled && !pawn.WorkTagIsDisabled((WorkTags) 8)) && pawn.RaceProps.ToolUser && !pawn.Downed && !pawn.Dead && (!pawn.IsPlayerControlled || !pawn.InMentalState) && !pawn.IsPrisoner && !pawn.IsColonyMech && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Consciousness);
    }
  }
}
