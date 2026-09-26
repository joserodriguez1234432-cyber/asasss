// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Pawn_JobTracker_DetermineNextJob
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Pawn_JobTracker), "DetermineNextJob")]
[PatchLevel(Level.Mandatory)]
public static class Patch_Pawn_JobTracker_DetermineNextJob
{
  public static void Prefix(Pawn ___pawn, bool ignoreQueue)
  {
    if (!ignoreQueue && ((IEnumerable<QueuedJob>) ___pawn.jobs.jobQueue).Any<QueuedJob>())
      return;
    ((Thing) ___pawn).RemoveTargetInfo();
  }
}
