// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Pawn_JobTracker_StartJob
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

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Pawn_JobTracker), "StartJob")]
public static class Patch_Pawn_JobTracker_StartJob
{
  [PatchLevel(Level.Sensitive)]
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    MethodInfo from = AccessTools.Method(typeof (Job), "MakeDriver", (Type[]) null, (Type[]) null);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo method = (Patch_Pawn_JobTracker_StartJob.\u003C\u003EO.\u003C0\u003E__MakeOrGetDriver ?? (Patch_Pawn_JobTracker_StartJob.\u003C\u003EO.\u003C0\u003E__MakeOrGetDriver = new Func<Job, Pawn, JobDriver>(Patch_Pawn_JobTracker_StartJob.MakeOrGetDriver))).Method;
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(from, method);
  }

  private static JobDriver MakeOrGetDriver(Job curJob, Pawn driverPawn)
  {
    return GenTypes.SameOrSubclassOf(curJob.def.driverClass, typeof (JobDriverAcrossMaps)) || curJob.jobGiver?.GetType() == typeof (JobDriver_GotoDestMap.ThinkNode_JobFromGotoDestMap) ? curJob.GetCachedDriver(driverPawn) : curJob.MakeDriver(driverPawn);
  }

  [PatchLevel(Level.Safe)]
  public static void Prefix(
    Pawn_JobTracker __instance,
    Pawn ___pawn,
    int ___jobsGivenThisTick,
    Job newJob,
    JobCondition lastJobEndCondition)
  {
    if ((lastJobEndCondition & 24) != null)
      ((Thing) ___pawn).RemoveTargetInfo();
    if (___jobsGivenThisTick <= 9)
      return;
    WorkGiverDef workGiverDef = newJob?.workGiverDef;
    if (workGiverDef == null)
      return;
    VMF_Log.Warning("A \"10 jobs in one tick\" error is about to occur. " + (VehicleMapFramework.VehicleMapFramework.settings.crossMapJobProtect ? $"Disable cross-map job support for WorkGiver: {((Def) workGiverDef).defName} and temporarily enable job logging." : $"Likely to be the cause WorkGiver: {((Def) workGiverDef).defName}. Temporarily enable job logging."));
    if (!__instance.debugLog)
    {
      __instance.debugLog = true;
      FrameDelay.DelayOne<Pawn_JobTracker>((Action<Pawn_JobTracker>) (instance => instance.debugLog = false), __instance);
    }
    if (!VehicleMapFramework.VehicleMapFramework.settings.crossMapJobProtect)
      return;
    GenCollection.AddUnique<WorkGiverDef>(JobAcrossMapsUtility.DisabledCrossMapWorkGiverDefs, workGiverDef);
  }
}
