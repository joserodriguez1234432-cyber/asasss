// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_JobGiver_FollowRoper_TryGiveJob
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (JobGiver_FollowRoper), "TryGiveJob")]
[PatchLevel(Level.Safe)]
public static class Patch_JobGiver_FollowRoper_TryGiveJob
{
  public static void Postfix(Pawn pawn, ref Job __result)
  {
    Pawn ropedByPawn;
    if (__result != null || !((ropedByPawn = pawn?.roping?.RopedByPawn)?.jobs?.curDriver is JobDriver_GotoDestMap curDriver) || !(curDriver.nextJob.GetCachedDriver(ropedByPawn) is JobDriver_RopeToDestination))
      return;
    IntVec3 cell = ((LocalTargetInfo) ref curDriver.nextJob.targetB).Cell;
    if (!((IntVec3) ref cell).IsValid || !((Thing) ropedByPawn).Spawned || !ReachabilityUtility.CanReach(pawn, LocalTargetInfo.op_Implicit((Thing) ropedByPawn), (PathEndMode) 2, (Danger) 3, false, false, (TraverseMode) 0))
      return;
    Job job = JobMaker.MakeJob(JobDefOf.FollowRoper, LocalTargetInfo.op_Implicit((Thing) ropedByPawn));
    job.expiryInterval = 140;
    job.checkOverrideOnExpire = true;
    __result = job;
  }
}
