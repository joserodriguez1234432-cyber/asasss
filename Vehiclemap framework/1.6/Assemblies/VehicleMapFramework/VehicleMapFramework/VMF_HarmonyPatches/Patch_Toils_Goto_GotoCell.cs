// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Toils_Goto_GotoCell
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

[HarmonyPatch(typeof (Toils_Goto), "GotoCell", new Type[] {typeof (IntVec3), typeof (PathEndMode)})]
[PatchLevel(Level.Safe)]
public static class Patch_Toils_Goto_GotoCell
{
  public static void Postfix(IntVec3 cell, PathEndMode peMode, Toil __result)
  {
    __result.AddPreInitAction((Action) (() =>
    {
      Pawn actor = __result.actor;
      Job curJob = actor.CurJob;
      LocalTargetInfo dest3 = GenCollection.FirstOrFallback<LocalTargetInfo>(GenCollection.ConcatIfNotNull<LocalTargetInfo>(GenCollection.ConcatIfNotNull<LocalTargetInfo>((IEnumerable<LocalTargetInfo>) new LocalTargetInfo[3]
      {
        curJob.targetA,
        curJob.targetB,
        curJob.targetC
      }, (IEnumerable<LocalTargetInfo>) curJob.targetQueueA), (IEnumerable<LocalTargetInfo>) curJob.targetQueueB), (Func<LocalTargetInfo, bool>) (t =>
      {
        if (!((LocalTargetInfo) ref t).HasThing)
          return false;
        if (IntVec3.op_Equality(((LocalTargetInfo) ref t).Cell, cell))
          return true;
        return ((LocalTargetInfo) ref t).Thing.Spawned && IntVec3.op_Equality(((LocalTargetInfo) ref t).Thing.InteractionCell, cell);
      }), LocalTargetInfo.Invalid);
      TargetInfo exitSpot;
      TargetInfo enterSpot;
      List<TraverseSpots> spotsQueue;
      if (!((LocalTargetInfo) ref dest3).IsValid || ((Thing) actor).Map == ((LocalTargetInfo) ref dest3).Thing.MapHeld || !actor.CanReach(dest3, peMode, (Danger) 3, false, false, (TraverseMode) 0, ((LocalTargetInfo) ref dest3).Thing.MapHeld, out exitSpot, out enterSpot, out spotsQueue))
        return;
      JobAcrossMapsUtility.StartGotoDestMapJob(actor, new TargetInfo?(exitSpot), new TargetInfo?(enterSpot), spotsQueue);
    }));
  }
}
