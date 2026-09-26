// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_JobGiver_Lord_TryGiveJob
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_JobGiver_Lord_TryGiveJob
{
  private static IEnumerable<MethodBase> TargetMethods()
  {
    yield return (MethodBase) AccessTools.Method(typeof (JobGiver_SpectateDutySpectateRect), "TryGiveJob", (Type[]) null, (Type[]) null);
    yield return (MethodBase) AccessTools.Method(typeof (JobGiver_GotoTravelDestination), "TryGiveJob", (Type[]) null, (Type[]) null);
  }

  public static void Prefix(Pawn pawn, ref VirtualTeleporter? __state)
  {
    Map mapHeld = ((Thing) pawn).MapHeld;
    Map map = LordUtility.GetLord(pawn)?.Map;
    if (mapHeld == null || map == null || mapHeld == map)
      return;
    __state = new VirtualTeleporter?(new VirtualTeleporter((Thing) pawn, map, setDepartMap: true));
  }

  public static void Finalizer(Pawn pawn, VirtualTeleporter? __state, ref Job __result)
  {
    if (!__state.HasValue)
      return;
    TargetInfo exitSpot;
    TargetInfo enterSpot;
    List<TraverseSpots> spotsQueue;
    if (__result != null && pawn.CanReach(__result.targetA, (PathEndMode) 2, (Danger) 3, false, false, (TraverseMode) 0, ((Thing) pawn).Map, out exitSpot, out enterSpot, out spotsQueue))
      __result = JobAcrossMapsUtility.GotoDestMapJob(pawn, new TargetInfo?(exitSpot), new TargetInfo?(enterSpot), spotsQueue, __result);
    __state.Value.Dispose();
  }
}
