// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Pawn_JobTracker_DrawLinesBetweenTargets
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Pawn_JobTracker), "DrawLinesBetweenTargets")]
[PatchLevel(Level.Sensitive)]
public static class Patch_Pawn_JobTracker_DrawLinesBetweenTargets
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    int index = list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Callvirt && CodeInstructionExtensions.OperandIs(c, (MemberInfo) MethodInfoCache.CachedMethodInfo.g_Thing_Position)));
    list.RemoveRange(index, 4);
    list.Insert(index, PatchHelper.get_CallvirtInstruction(AccessTools.PropertyGetter(typeof (Pawn), "DrawPos")));
    MethodInfo g_CenterVector3 = AccessTools.PropertyGetter(typeof (LocalTargetInfo), "CenterVector3");
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo m_CenterVector3VehicleOffset = (Patch_Pawn_JobTracker_DrawLinesBetweenTargets.\u003C\u003EO.\u003C0\u003E__CenterVector3VehicleOffset ?? (Patch_Pawn_JobTracker_DrawLinesBetweenTargets.\u003C\u003EO.\u003C0\u003E__CenterVector3VehicleOffset = new \u003C\u003EF\u007B00000001\u007D<LocalTargetInfo, Pawn, Vector3>(Patch_Pawn_JobTracker_DrawLinesBetweenTargets.CenterVector3VehicleOffset))).Method;
    foreach (CodeInstruction code in list)
    {
      if (code.opcode == OpCodes.Call && CodeInstructionExtensions.OperandIs(code, (MemberInfo) g_CenterVector3))
      {
        yield return CodeInstruction.LoadArgument(0, false);
        yield return CodeInstruction.LoadField(typeof (Pawn_JobTracker), "pawn", false);
        code.operand = (object) m_CenterVector3VehicleOffset;
      }
      yield return code;
    }
  }

  public static Vector3 CenterVector3VehicleOffset(ref LocalTargetInfo targ, Pawn pawn)
  {
    if (((LocalTargetInfo) ref targ).HasThing)
    {
      if (((LocalTargetInfo) ref targ).Thing.Spawned)
        return ((LocalTargetInfo) ref targ).Thing.DrawPos;
      if (((LocalTargetInfo) ref targ).Thing.SpawnedOrAnyParentSpawned)
        return ((LocalTargetInfo) ref targ).Thing.SpawnedParentOrMe.DrawPos;
      IntVec3 position = ((LocalTargetInfo) ref targ).Thing.Position;
      return ((IntVec3) ref position).ToVector3Shifted();
    }
    IntVec3 cell = ((LocalTargetInfo) ref targ).Cell;
    if (!((IntVec3) ref cell).IsValid)
      return new Vector3();
    Map map1;
    if (((Thing) pawn).TryGetTargetMap(out map1) && pawn.stances.curStance is Stance_Busy)
    {
      cell = ((LocalTargetInfo) ref targ).Cell;
      return ((IntVec3) ref cell).ToVector3Shifted().ToBaseMapCoord(map1);
    }
    Job curJob = pawn.CurJob;
    Map map2 = curJob != null ? ((GlobalTargetInfo) ref curJob.globalTarget).Map : (Map) null;
    if (map2 != null)
    {
      cell = ((LocalTargetInfo) ref targ).Cell;
      return ((IntVec3) ref cell).ToVector3Shifted().ToBaseMapCoord(map2);
    }
    if (pawn.CurJob?.GetCachedDriver(pawn) is JobDriverAcrossMaps cachedDriver)
    {
      VehiclePawnWithMap vehicle;
      if (cachedDriver.DestMap.IsNonFocusedVehicleMapOf(out vehicle))
      {
        cell = ((LocalTargetInfo) ref targ).Cell;
        return ((IntVec3) ref cell).ToVector3Shifted().ToBaseMapCoord(vehicle);
      }
    }
    else
    {
      VehiclePawnWithMap vehicle;
      bool flag1 = ((Thing) pawn).IsOnNonFocusedVehicleMapOf(out vehicle);
      if (flag1)
      {
        bool flag2;
        if (pawn.stances.curStance is Stance_Busy curStance)
        {
          switch (curStance.verb)
          {
            case Verb_Jump _:
            case Verb_CastAbilityJump _:
              flag2 = true;
              goto label_20;
          }
        }
        flag2 = false;
label_20:
        flag1 = !flag2;
      }
      if (flag1)
      {
        cell = ((LocalTargetInfo) ref targ).Cell;
        return ((IntVec3) ref cell).ToVector3Shifted().ToBaseMapCoord(vehicle);
      }
    }
    cell = ((LocalTargetInfo) ref targ).Cell;
    return ((IntVec3) ref cell).ToVector3Shifted();
  }
}
