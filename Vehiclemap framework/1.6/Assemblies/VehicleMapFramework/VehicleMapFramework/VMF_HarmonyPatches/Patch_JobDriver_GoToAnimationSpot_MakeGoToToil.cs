// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_JobDriver_GoToAnimationSpot_MakeGoToToil
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_MeleeAnimation")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_JobDriver_GoToAnimationSpot_MakeGoToToil
{
  public static void Postfix(Toil __result)
  {
    __result.AddPreInitAction((Action) (() =>
    {
      Pawn actor = __result.actor;
      LocalTargetInfo target = actor.CurJob.GetTarget((TargetIndex) 1);
      Map mapHeld = ((LocalTargetInfo) ref target).Thing?.MapHeld;
      TargetInfo exitSpot;
      TargetInfo enterSpot;
      List<TraverseSpots> spotsQueue;
      if (mapHeld == null || ((Thing) actor).Map == mapHeld || !actor.CanReach(target, (PathEndMode) 1, (Danger) 3, false, false, (TraverseMode) 0, mapHeld, out exitSpot, out enterSpot, out spotsQueue))
        return;
      JobAcrossMapsUtility.StartGotoDestMapJob(actor, new TargetInfo?(exitSpot), new TargetInfo?(enterSpot), spotsQueue);
    }));
  }
}
