// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Toils_Bed_GotoBed
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Toils_Bed), "GotoBed")]
[PatchLevel(Level.Safe)]
public static class Patch_Toils_Bed_GotoBed
{
  public static void Postfix(TargetIndex bedIndex, Toil __result)
  {
    __result.endConditions.Insert(0, (Func<JobCondition>) (() =>
    {
      Pawn actor = __result.actor;
      LocalTargetInfo target = actor.CurJob.GetTarget(bedIndex);
      Thing thing = ((LocalTargetInfo) ref target).Thing;
      TargetInfo exitSpot;
      TargetInfo enterSpot;
      List<TraverseSpots> spotsQueue;
      if (((Thing) actor).Map == thing.Map || !actor.CanReach(LocalTargetInfo.op_Implicit(thing), (PathEndMode) 1, (Danger) 3, false, false, (TraverseMode) 0, thing.Map, out exitSpot, out enterSpot, out spotsQueue))
        return (JobCondition) 1;
      JobAcrossMapsUtility.StartGotoDestMapJob(actor, new TargetInfo?(exitSpot), new TargetInfo?(enterSpot), spotsQueue);
      return (JobCondition) 16 /*0x10*/;
    }));
  }
}
