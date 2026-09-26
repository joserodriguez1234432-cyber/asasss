// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_JobDriver_Board_MakeNewToils
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using Vehicles;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (JobDriver_Board), "MakeNewToils")]
[PatchLevel(Level.Safe)]
public static class Patch_JobDriver_Board_MakeNewToils
{
  public static readonly Func<LordJob_FormAndSendVehicles, Pawn, AssignedSeat> GetAssignedSeat;

  public static IEnumerable<Toil> Postfix(IEnumerable<Toil> values)
  {
    foreach (Toil toil1 in values)
    {
      Toil toil = toil1;
      if (toil.debugName == "GotoThing")
      {
        Action oldAction = toil.initAction;
        toil.initAction = (Action) (() =>
        {
          Pawn actor = toil.actor;
          if (LordUtility.GetLord(actor)?.LordJob is LordJob_FormAndSendVehicles lordJob2 && Patch_JobDriver_Board_MakeNewToils.GetAssignedSeat(lordJob2, actor).handler?.role is VehicleRoleBuildable role2)
          {
            ThingWithComps parent = role2.upgradeComp?.parent;
            if (ToilFailConditions.DespawnedOrNull(LocalTargetInfo.op_Implicit((Thing) parent), actor))
              actor.jobs.EndCurrentJob((JobCondition) 4, true, false);
            else
              actor.pather.StartPath(LocalTargetInfo.op_Implicit((Thing) parent), (PathEndMode) 2);
          }
          else
            oldAction();
        });
      }
      yield return toil;
    }
  }

  static Patch_JobDriver_Board_MakeNewToils()
  {
    MethodInfo methodInfo = AccessTools.Method(typeof (LordJob_FormAndSendVehicles), nameof (GetAssignedSeat), (Type[]) null, (Type[]) null);
    if ((object) methodInfo == null)
      methodInfo = AccessTools.Method(typeof (LordJob_FormAndSendVehicles), "GetVehicleAssigned", (Type[]) null, (Type[]) null);
    Patch_JobDriver_Board_MakeNewToils.GetAssignedSeat = AccessTools.MethodDelegate<Func<LordJob_FormAndSendVehicles, Pawn, AssignedSeat>>(methodInfo, (object) null, true, (Type[]) null);
  }
}
