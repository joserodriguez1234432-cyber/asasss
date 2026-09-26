// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Pawn_PathFollower_StartPath
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Pawn_PathFollower), "StartPath")]
[PatchLevel(Level.Safe)]
public static class Patch_Pawn_PathFollower_StartPath
{
  public static bool Prefix(LocalTargetInfo dest, PathEndMode peMode, Pawn ___pawn)
  {
    Pawn_JobTracker jobs = ___pawn.jobs;
    if (jobs == null || jobs.curDriver is JobDriverAcrossMaps || jobs.curJob == null)
      return true;
    bool flag = false;
    Map destMap = ((LocalTargetInfo) ref dest).Thing?.MapHeld;
    if (destMap == null)
    {
      flag = true;
      TargetInfo target;
      if (!((Thing) ___pawn).TryGetTargetInfo(out target))
      {
        TargetInfo targetInfo = target = GlobalTargetInfo.op_Explicit(___pawn.CurJob.globalTarget);
        if (!((TargetInfo) ref targetInfo).IsValid)
          goto label_6;
      }
      Map map;
      if (LocalTargetInfo.op_Equality(TargetInfo.op_Explicit(target), dest))
      {
        map = ((TargetInfo) ref target).Map;
        goto label_8;
      }
label_6:
      map = (Map) null;
label_8:
      destMap = map;
    }
    TargetInfo exitSpot;
    TargetInfo enterSpot;
    List<TraverseSpots> spotsQueue;
    if (destMap == null || ((Thing) ___pawn).Map == destMap || !___pawn.CanReach(dest, peMode, (Danger) 3, false, false, (TraverseMode) 0, destMap, out exitSpot, out enterSpot, out spotsQueue))
      return true;
    if (flag)
    {
      ((Thing) ___pawn).RemoveTargetInfo();
      ___pawn.CurJob.globalTarget = GlobalTargetInfo.Invalid;
    }
    JobAcrossMapsUtility.StartGotoDestMapJob(___pawn, new TargetInfo?(exitSpot), new TargetInfo?(enterSpot), spotsQueue);
    return false;
  }
}
