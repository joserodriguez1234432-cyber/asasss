// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_FloatMenuOptionProvider_WorkGivers_GetWorkGiverOption
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

[HarmonyPatch(typeof (FloatMenuOptionProvider_WorkGivers), "GetWorkGiverOption")]
[PatchLevel(Level.Safe)]
public static class Patch_FloatMenuOptionProvider_WorkGivers_GetWorkGiverOption
{
  public static void Prefix(
    Pawn pawn,
    WorkGiverDef workGiver,
    LocalTargetInfo target,
    FloatMenuContext context,
    ref VirtualTeleporter? __state)
  {
    if (JobAcrossMapsUtility.NoNeedVirtualMapTransfer(((Thing) pawn).Map, context.map, workGiver) || !pawn.CanReach(target, (PathEndMode) 2, (Danger) 3, false, false, (TraverseMode) 0, context.map))
      return;
    __state = new VirtualTeleporter?(new VirtualTeleporter((Thing) pawn, context.map, new IntVec3?(((LocalTargetInfo) ref target).Cell)));
  }

  public static void Finalizer(
    Pawn pawn,
    WorkGiverDef workGiver,
    LocalTargetInfo target,
    FloatMenuContext context,
    VirtualTeleporter? __state,
    FloatMenuOption __result)
  {
    if (!__state.HasValue)
      return;
    __state.Value.Dispose();
    TargetInfo exitSpot;
    TargetInfo enterSpot;
    List<TraverseSpots> spotsQueue;
    if (__result == null || __result.Disabled || __result.action == null || !JobAcrossMapsUtility.NeedWrapGotoDestMapJob(workGiver.Worker as WorkGiver_Scanner) || !pawn.CanReach(target, (PathEndMode) 2, (Danger) 3, false, false, (TraverseMode) 0, context.map, out exitSpot, out enterSpot, out spotsQueue))
      return;
    __result.action = (Action) (() => JobAcrossMapsUtility.StartGotoDestMapJob(pawn, new TargetInfo?(exitSpot), new TargetInfo?(enterSpot), spotsQueue)) + __result.action;
  }
}
