// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_JobSystem
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using SmashTools.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

internal class Patch_JobSystem : IPatchCategory
{
  private static bool startingErrorRecoverJob;

  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (JobUtility), "TryStartErrorRecoverJob", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_JobSystem), "VehicleErrorRecoverJob", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (JobGiver_Wander), "TryGiveJob", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_JobSystem), "VehiclesDontWander", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Pawn_JobTracker), "CheckForJobOverride", (System.Type[]) null, (System.Type[]) null), transpiler: new HarmonyMethod(typeof (Patch_JobSystem), "NoOverrideDamageTakenTranspiler", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Property(typeof (JobDriver_PrepareCaravan_GatherItems), "Transferables").GetGetMethod(true), new HarmonyMethod(typeof (Patch_JobSystem), "TransferablesVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (ThingRequest), "Accepts", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_JobSystem), "AcceptsVehicleRefuelable", (System.Type[]) null));
  }

  public static bool VehicleErrorRecoverJob(
    Pawn pawn,
    string message,
    Exception exception = null,
    JobDriver concreteDriver = null)
  {
    if (!(pawn is VehiclePawn))
      return true;
    if (exception != null)
      message += $"\n{exception}";
    Log.Error(message);
    if (pawn.jobs != null)
    {
      if (pawn.jobs.curJob != null)
        pawn.jobs.EndCurrentJob((JobCondition) 64 /*0x40*/, false, true);
      if (Patch_JobSystem.startingErrorRecoverJob)
      {
        Log.Error($"An error occurred while starting an error recover job. We have to stop now to avoid infinite recursion. This means that the vehicle is now jobless which can cause further bugs. vehicle={pawn}");
        return false;
      }
      Patch_JobSystem.startingErrorRecoverJob = true;
      try
      {
        if (pawn.jobs.jobQueue.Count > 0)
        {
          Job job = pawn.jobs.jobQueue.Dequeue().job;
          pawn.jobs.StartJob(job, (JobCondition) 2, (ThinkNode) null, false, true, (ThinkTreeDef) null, new JobTag?(), false, false, new bool?(), false, true, false);
        }
        else
          pawn.jobs.StartJob(new Job(JobDefOf_Vehicles.IdleVehicle, -1, false), (JobCondition) 4, (ThinkNode) null, false, true, (ThinkTreeDef) null, new JobTag?(), false, false, new bool?(), false, true, false);
        Patch_JobSystem.startingErrorRecoverJob = false;
      }
      catch
      {
        Log.Error($"An error occurred when trying to recover the job for {pawn}. Unable to assign idle recovery job.");
      }
      finally
      {
        Patch_JobSystem.startingErrorRecoverJob = false;
      }
    }
    return false;
  }

  public static bool VehiclesDontWander(Pawn pawn, ref Job __result)
  {
    if (!(pawn is VehiclePawn))
      return true;
    __result = new Job(JobDefOf_Vehicles.IdleVehicle);
    return false;
  }

  public static IEnumerable<CodeInstruction> NoOverrideDamageTakenTranspiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator ilg)
  {
    List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
    for (int i = 0; i < instructionList.Count; ++i)
    {
      CodeInstruction instruction = instructionList[i];
      if (instruction.opcode == OpCodes.Stloc_1)
      {
        yield return instruction;
        instruction = instructionList[++i];
        Label label = ilg.DefineLabel();
        Label retlabel = ilg.DefineLabel();
        yield return new CodeInstruction(OpCodes.Ldloc_0, (object) null);
        yield return new CodeInstruction(OpCodes.Brfalse, (object) retlabel);
        yield return new CodeInstruction(OpCodes.Ldloca_S, (object) 1);
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Property(typeof (ThinkResult), "IsValid").GetGetMethod());
        yield return new CodeInstruction(OpCodes.Brtrue, (object) label);
        yield return new CodeInstruction(OpCodes.Ret, (object) null)
        {
          labels = new List<Label>() { retlabel }
        };
        instruction.labels.Add(label);
      }
      yield return instruction;
      instruction = (CodeInstruction) null;
    }
  }

  public static bool TransferablesVehicle(
    JobDriver_PrepareCaravan_GatherItems __instance,
    ref List<TransferableOneWay> __result)
  {
    if (!(((JobDriver) __instance).job.lord.LordJob is LordJob_FormAndSendVehicles))
      return true;
    __result = ((LordJob_FormAndSendCaravan) ((JobDriver) __instance).job.lord.LordJob).transferables;
    return false;
  }

  public static void AcceptsVehicleRefuelable(Thing t, ref bool __result, ThingRequest __instance)
  {
    if (!(t is VehiclePawn vehiclePawn) || __instance.group != 19)
      return;
    __result = vehiclePawn.CompFueledTravel != null;
  }
}
