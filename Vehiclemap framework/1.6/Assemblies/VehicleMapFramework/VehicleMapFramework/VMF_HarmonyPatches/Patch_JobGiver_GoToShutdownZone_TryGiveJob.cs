// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_JobGiver_GoToShutdownZone_TryGiveJob
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System.Collections.Generic;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_WVCWorkModes")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_JobGiver_GoToShutdownZone_TryGiveJob
{
  private static bool working;

  public static void Postfix(ThinkNode_JobGiver __instance, Pawn pawn, ref Job __result)
  {
    if (__result != null)
      return;
    if (Patch_JobGiver_GoToShutdownZone_TryGiveJob.working)
      return;
    try
    {
      Patch_JobGiver_GoToShutdownZone_TryGiveJob.working = true;
      CrossMapReachabilityUtility.set_DepartMap(pawn, ((Thing) pawn).Map);
      foreach (Map mapAndVehicleMap in ((Thing) pawn).Map.BaseMapAndVehicleMaps(false))
      {
        using (new VirtualTeleporter((Thing) pawn, mapAndVehicleMap))
        {
          ThinkResult thinkResult = ((ThinkNode) __instance).TryIssueJobPackage(pawn, new JobIssueParams());
          Job job = ((ThinkResult) ref thinkResult).Job;
          if (job != null)
          {
            TargetInfo exitSpot;
            TargetInfo enterSpot;
            List<TraverseSpots> spotsQueue;
            if (pawn.CanReach(job.targetA, (PathEndMode) 1, (Danger) 2, false, false, (TraverseMode) 0, mapAndVehicleMap, out exitSpot, out enterSpot, out spotsQueue))
            {
              __result = JobAcrossMapsUtility.GotoDestMapJob(pawn, new TargetInfo?(exitSpot), new TargetInfo?(enterSpot), spotsQueue, job);
              break;
            }
          }
        }
      }
      pawn.RemoveDepartMap();
    }
    finally
    {
      Patch_JobGiver_GoToShutdownZone_TryGiveJob.working = false;
    }
  }
}
