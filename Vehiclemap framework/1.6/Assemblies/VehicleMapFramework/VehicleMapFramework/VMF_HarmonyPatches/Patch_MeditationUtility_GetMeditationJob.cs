// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_MeditationUtility_GetMeditationJob
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_Royalty")]
[HarmonyPatch(typeof (MeditationUtility), "GetMeditationJob")]
[PatchLevel(Level.Safe)]
public static class Patch_MeditationUtility_GetMeditationJob
{
  public static void Postfix(Pawn pawn, bool forJoy, ref Job __result)
  {
    float initialScore = __result != null ? Patch_MeditationUtility_GetMeditationJob.GetMeditationScore(__result.targetA, __result.targetC, pawn, pawn.ownership.OwnedRoom) : 0.0f;
    foreach (Map mapAndVehicleMap in ((Thing) pawn).Map.BaseMapAndVehicleMaps(false))
    {
      using (new VirtualTeleporter((Thing) pawn, mapAndVehicleMap, new IntVec3?(((Thing) pawn).PositionOnAnotherMap(mapAndVehicleMap)), true))
      {
        float score;
        Job meditationJob = Patch_MeditationUtility_GetMeditationJob.GetMeditationJob(pawn, forJoy, initialScore, out score);
        if (meditationJob != null)
        {
          if ((double) score > (double) initialScore)
          {
            TargetInfo exitSpot;
            TargetInfo enterSpot;
            List<TraverseSpots> spotsQueue;
            if (pawn.CanReach(meditationJob.targetA, (PathEndMode) 1, DangerUtility.NormalMaxDanger(pawn), false, false, (TraverseMode) 0, mapAndVehicleMap, out exitSpot, out enterSpot, out spotsQueue))
            {
              initialScore = score;
              __result = JobAcrossMapsUtility.GotoDestMapJob(pawn, new TargetInfo?(exitSpot), new TargetInfo?(enterSpot), spotsQueue, meditationJob);
            }
          }
        }
      }
    }
  }

  private static Job GetMeditationJob(Pawn pawn, bool forJoy, float initialScore, out float score)
  {
    MeditationSpotAndFocus meditationSpot = Patch_MeditationUtility_GetMeditationJob.FindMeditationSpot(pawn, initialScore, out score);
    if (!((MeditationSpotAndFocus) ref meditationSpot).IsValid || !ReservationUtility.CanReserveAndReach(pawn, meditationSpot.spot, (PathEndMode) 1, DangerUtility.NormalMaxDanger(pawn), 1, -1, (ReservationLayerDef) null, false))
      return (Job) null;
    Job meditationJob;
    if (((LocalTargetInfo) ref meditationSpot.focus).Thing is Building_Throne thing)
    {
      meditationJob = JobMaker.MakeJob(JobDefOf.Reign, LocalTargetInfo.op_Implicit((Thing) thing), LocalTargetInfo.op_Implicit((Thing) null), LocalTargetInfo.op_Implicit((Thing) thing));
    }
    else
    {
      JobDef jobDef = JobDefOf.Meditate;
      if (forJoy && ModsConfig.IdeologyActive)
      {
        Ideo ideo = pawn.Ideo;
        if (ideo != null && ideo.foundation is IdeoFoundation_Deity foundation && GenCollection.Any<IdeoFoundation_Deity.Deity>(foundation.DeitiesListForReading))
          jobDef = JobDefOf.MeditatePray;
      }
      meditationJob = JobMaker.MakeJob(jobDef, meditationSpot.spot, LocalTargetInfo.op_Implicit((Thing) null), meditationSpot.focus);
    }
    meditationJob.ignoreJoyTimeAssignment = !forJoy;
    return meditationJob;
  }

  private static MeditationSpotAndFocus FindMeditationSpot(
    Pawn pawn,
    float initialScore,
    out float score)
  {
    score = initialScore;
    LocalTargetInfo localTargetInfo1 = LocalTargetInfo.Invalid;
    LocalTargetInfo localTargetInfo2 = LocalTargetInfo.Invalid;
    if (!ModLister.CheckRoyalty("Psyfocus"))
      return new MeditationSpotAndFocus(localTargetInfo1, localTargetInfo2);
    Room ownedRoom = pawn.ownership.OwnedRoom;
    foreach (LocalTargetInfo meditationSpotCandidate in Patch_MeditationUtility_GetMeditationJob.AllMeditationSpotCandidates(pawn))
    {
      if (MeditationUtility.SafeEnvironmentalConditions(pawn, ((LocalTargetInfo) ref meditationSpotCandidate).Cell, ((Thing) pawn).Map) && GenGrid.Standable(((LocalTargetInfo) ref meditationSpotCandidate).Cell, ((Thing) pawn).Map) && !ForbidUtility.IsForbidden(((LocalTargetInfo) ref meditationSpotCandidate).Cell, pawn))
      {
        LocalTargetInfo focus = ((LocalTargetInfo) ref meditationSpotCandidate).Thing is Building_Throne ? LocalTargetInfo.op_Implicit(((LocalTargetInfo) ref meditationSpotCandidate).Thing) : MeditationUtility.BestFocusAt(meditationSpotCandidate, pawn);
        float meditationScore = Patch_MeditationUtility_GetMeditationJob.GetMeditationScore(meditationSpotCandidate, focus, pawn, ownedRoom);
        if ((double) meditationScore > (double) score)
        {
          localTargetInfo1 = meditationSpotCandidate;
          localTargetInfo2 = focus;
          score = meditationScore;
        }
      }
    }
    return new MeditationSpotAndFocus(localTargetInfo1, localTargetInfo2);
  }

  private static float GetMeditationScore(
    LocalTargetInfo spot,
    LocalTargetInfo focus,
    Pawn pawn,
    Room ownedRoom)
  {
    float meditationScore = 1f / Mathf.Max((float) IntVec3Utility.DistanceToSquared(((LocalTargetInfo) ref spot).Cell, ((Thing) pawn).Position), 0.1f);
    if (pawn.HasPsylink && ((LocalTargetInfo) ref focus).IsValid)
      meditationScore += StatExtension.GetStatValueForPawn(((LocalTargetInfo) ref focus).Thing, StatDefOf.MeditationFocusStrength, pawn, true) * 100f;
    Room room = GridsUtility.GetRoom(((LocalTargetInfo) ref spot).Cell, ((Thing) pawn).Map);
    if (room != null && ownedRoom == room)
      ++meditationScore;
    if (((LocalTargetInfo) ref spot).Thing is Building thing && AssignableUtility.GetAssignedPawn(thing) == pawn)
      meditationScore += ((Thing) thing).def == ThingDefOf.MeditationSpot ? 200f : 100f;
    if (room != null && ModsConfig.IdeologyActive && room.Role == RoomRoleDefOf.WorshipRoom)
    {
      meditationScore += 100f;
      foreach (Thing andAdjacentThing in room.ContainedAndAdjacentThings)
        meditationScore += StatExtension.GetStatValue(andAdjacentThing, StatDefOf.StyleDominance, true, -1);
    }
    return meditationScore;
  }

  private static IEnumerable<LocalTargetInfo> AllMeditationSpotCandidates(
    Pawn pawn,
    bool allowFallbackSpots = true)
  {
    bool flag = false;
    if (pawn.royalty != null && pawn.royalty.AllTitlesInEffectForReading.Count > 0 && !pawn.IsPrisonerOfColony)
    {
      Building_Throne bestUsableThrone = RoyalTitleUtility.FindBestUsableThrone(pawn);
      if (bestUsableThrone != null)
      {
        yield return LocalTargetInfo.op_Implicit((Thing) bestUsableThrone);
        flag = true;
      }
    }
    if (!pawn.IsPrisonerOfColony)
    {
      IEnumerator<Building> enumerator = ((Thing) pawn).Map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.MeditationSpot).Where<Building>((Func<Building, bool>) (s => MeditationUtility.IsValidMeditationBuildingForPawn(s, pawn))).GetEnumerator();
      while (enumerator.MoveNext())
      {
        yield return LocalTargetInfo.op_Implicit((Thing) enumerator.Current);
        flag = true;
      }
      enumerator = (IEnumerator<Building>) null;
    }
    if (!flag && allowFallbackSpots)
    {
      List<Thing>.Enumerator enumerator1 = ((Thing) pawn).Map.listerThings.ThingsInGroup((ThingRequestGroup) 62).GetEnumerator();
      while (enumerator1.MoveNext())
      {
        Thing current = enumerator1.Current;
        if (current.def != ThingDefOf.Wall)
        {
          Room room = RegionAndRoomQuery.GetRoom(current, (RegionType) 15);
          if ((room == null || MeditationUtility.CanUseRoomToMeditate(room, pawn)) && (double) StatExtension.GetStatValueForPawn(current, StatDefOf.MeditationFocusStrength, pawn, true) > 0.0)
          {
            LocalTargetInfo localTargetInfo = MeditationUtility.MeditationSpotForFocus(current, pawn, (Func<IntVec3, bool>) null);
            if (((LocalTargetInfo) ref localTargetInfo).IsValid)
              yield return localTargetInfo;
          }
        }
      }
      enumerator1 = new List<Thing>.Enumerator();
      Building_Bed ownedBed = pawn.ownership.OwnedBed;
      Room room1 = ownedBed != null ? RegionAndRoomQuery.GetRoom((Thing) ownedBed, (RegionType) 15) : (Room) null;
      IEnumerator<LocalTargetInfo> enumerator2;
      if (room1 != null && !room1.PsychologicallyOutdoors && ReservationUtility.CanReserveAndReach(pawn, LocalTargetInfo.op_Implicit((Thing) ownedBed), (PathEndMode) 1, DangerUtility.NormalMaxDanger(pawn), 1, -1, (ReservationLayerDef) null, false))
      {
        enumerator2 = MeditationUtility.FocusSpotsInTheRoom(pawn, room1).GetEnumerator();
        while (enumerator2.MoveNext())
          yield return enumerator2.Current;
        enumerator2 = (IEnumerator<LocalTargetInfo>) null;
      }
      foreach (Room usableWorshipRoom in MeditationUtility.UsableWorshipRooms(pawn))
      {
        enumerator2 = MeditationUtility.FocusSpotsInTheRoom(pawn, usableWorshipRoom).GetEnumerator();
        while (enumerator2.MoveNext())
        {
          LocalTargetInfo current = enumerator2.Current;
          if (ReachabilityUtility.CanReach(pawn, current, (PathEndMode) 2, DangerUtility.NormalMaxDanger(pawn), false, false, (TraverseMode) 0))
            yield return current;
        }
        enumerator2 = (IEnumerator<LocalTargetInfo>) null;
      }
    }
  }
}
