// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_X2_JobGiver_Return2BaseRoom_TryIssueJobPackage
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_MiscRobots")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_X2_JobGiver_Return2BaseRoom_TryIssueJobPackage
{
  public static bool Prefix(ThinkNode __instance, Pawn pawn, ref ThinkResult __result)
  {
    Type x2AiRobot = ModCompat.MiscRobots.X2_AIRobot;
    if (((object) x2AiRobot != null ? (!x2AiRobot.IsAssignableFrom(pawn.GetType()) ? 1 : 0) : 1) != 0)
      return true;
    Building rechargeStation = ModCompat.MiscRobots.rechargeStation.Invoke(pawn);
    if (((Thing) pawn).Map == ((Thing) rechargeStation)?.Map)
      return true;
    if (ThingUtility.DestroyedOrNull((Thing) pawn) || !((Thing) pawn).Spawned || ThingUtility.DestroyedOrNull((Thing) rechargeStation) || !((Thing) rechargeStation).Spawned)
    {
      __result = ThinkResult.NoJob;
      return false;
    }
    Room room1 = GridsUtility.GetRoom(((Thing) rechargeStation).Position, ((Thing) rechargeStation).Map);
    Room room2 = GridsUtility.GetRoom(((Thing) pawn).Position, ((Thing) pawn).Map);
    if (room1 == room2)
    {
      __result = ThinkResult.NoJob;
      return false;
    }
    Map mapRecharge = ((Thing) rechargeStation).Map;
    IntVec3 posRecharge = ((Thing) rechargeStation).Position;
    TargetInfo exitSpot = TargetInfo.Invalid;
    TargetInfo enterSpot = TargetInfo.Invalid;
    List<TraverseSpots> spotsQueue = (List<TraverseSpots>) null;
    IntVec3 intVec3_1 = room1.Cells.Where<IntVec3>((Func<IntVec3, bool>) (c =>
    {
      if (GenGrid.Standable(c, mapRecharge) && !ForbidUtility.IsForbidden(c, pawn))
      {
        IntVec3 intVec3_2 = c;
        if (((IntVec3) ref intVec3_2).InHorDistOf(posRecharge, 5f))
          return pawn.CanReach(LocalTargetInfo.op_Implicit(c), (PathEndMode) 1, (Danger) 2, false, false, (TraverseMode) 0, ((Thing) rechargeStation).Map, out exitSpot, out enterSpot, out spotsQueue);
      }
      return false;
    })).FirstOrDefault<IntVec3>();
    if (IntVec3.op_Equality(intVec3_1, IntVec3.Invalid))
    {
      __result = ThinkResult.NoJob;
      return false;
    }
    Job job = JobMaker.MakeJob(VMF_DefOf.VMF_GotoAcrossMaps, LocalTargetInfo.op_Implicit(intVec3_1));
    job.locomotionUrgency = (LocomotionUrgency) 1;
    job.SetSpotsToJobAcrossMaps(pawn, new TargetInfo?(exitSpot), new TargetInfo?(enterSpot), spotsQueue);
    __result = new ThinkResult(job, __instance, new JobTag?((JobTag) 0), false);
    return false;
  }
}
