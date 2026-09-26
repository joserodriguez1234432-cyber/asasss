// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Toils_Rope_GotoRopeAttachmentInteractionCell
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

[HarmonyPatch(typeof (Toils_Rope), "GotoRopeAttachmentInteractionCell")]
[PatchLevel(Level.Safe)]
public static class Patch_Toils_Rope_GotoRopeAttachmentInteractionCell
{
  public static void Postfix(Toil __result, TargetIndex ropeeIndex)
  {
    __result.AddPreInitAction((Action) (() =>
    {
      Pawn actor = __result.actor;
      LocalTargetInfo target = actor.CurJob.GetTarget(ropeeIndex);
      TargetInfo exitSpot;
      TargetInfo enterSpot;
      List<TraverseSpots> spotsQueue;
      if (!(((LocalTargetInfo) ref target).Thing is Pawn thing2) || ((Thing) actor).Map == ((Thing) thing2).Map || !actor.CanReach(LocalTargetInfo.op_Implicit((Thing) thing2), (PathEndMode) 2, (Danger) 3, false, false, (TraverseMode) 0, ((Thing) thing2).Map, out exitSpot, out enterSpot, out spotsQueue))
        return;
      JobAcrossMapsUtility.StartGotoDestMapJob(actor, new TargetInfo?(exitSpot), new TargetInfo?(enterSpot), spotsQueue);
    }));
  }
}
