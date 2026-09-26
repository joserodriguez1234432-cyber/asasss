// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_JobDriver_FoodDeliver_MakeNewToils
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

[HarmonyPatch(typeof (JobDriver_FoodDeliver), "MakeNewToils")]
[PatchLevel(Level.Safe)]
public static class Patch_JobDriver_FoodDeliver_MakeNewToils
{
  public static IEnumerable<Toil> Postfix(IEnumerable<Toil> values, Job ___job)
  {
    bool found = false;
    foreach (Toil toil1 in values)
    {
      Toil toil = toil1;
      if (toil.debugName == "MakeNewToils" && !found)
      {
        found = true;
        toil.AddPreInitAction((Action) (() =>
        {
          TargetInfo exitSpot;
          TargetInfo enterSpot;
          List<TraverseSpots> spotsQueue;
          if (!((LocalTargetInfo) ref ___job.targetB).HasThing || ((Thing) toil.actor).Map == ((LocalTargetInfo) ref ___job.targetB).Thing.MapHeld || !toil.actor.CanReach(___job.targetB, (PathEndMode) 2, (Danger) 3, false, false, (TraverseMode) 0, ((LocalTargetInfo) ref ___job.targetB).Thing.MapHeld, out exitSpot, out enterSpot, out spotsQueue))
            return;
          JobAcrossMapsUtility.StartGotoDestMapJob(toil.actor, new TargetInfo?(exitSpot), new TargetInfo?(enterSpot), spotsQueue);
        }));
      }
      yield return toil;
    }
  }
}
