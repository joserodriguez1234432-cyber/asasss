// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Patch_JobTracker_ABLocalizeJobLines_Prefix
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_AsAboveSoBelow")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_Patch_JobTracker_ABLocalizeJobLines_Prefix
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(ModCompat.AsAboveSoBelow.LocalizeForPawn.Method)
    }).SetOperandAndAdvance((object) (Patch_Patch_JobTracker_ABLocalizeJobLines_Prefix.\u003C\u003EO.\u003C0\u003E__LocalizeForPawnIfNotOnVehicleMap ?? (Patch_Patch_JobTracker_ABLocalizeJobLines_Prefix.\u003C\u003EO.\u003C0\u003E__LocalizeForPawnIfNotOnVehicleMap = new Func<Pawn, Vector3, Vector3>(Patch_Patch_JobTracker_ABLocalizeJobLines_Prefix.LocalizeForPawnIfNotOnVehicleMap))).Method).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(ModCompat.AsAboveSoBelow.LocalizeForPawn.Method)
    }).Repeat((Action<CodeMatcher>) (c => c.Advance(-1).RemoveInstruction().SetOperandAndAdvance((object) (Patch_Patch_JobTracker_ABLocalizeJobLines_Prefix.\u003C\u003EO.\u003C1\u003E__LocalizeForPawnTarget ?? (Patch_Patch_JobTracker_ABLocalizeJobLines_Prefix.\u003C\u003EO.\u003C1\u003E__LocalizeForPawnTarget = new \u003C\u003EF\u007B00000008\u007D<Pawn, LocalTargetInfo, Vector3>(Patch_Patch_JobTracker_ABLocalizeJobLines_Prefix.LocalizeForPawnTarget))).Method)), (Action<string>) null).Reset(true).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Map)
    }).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Map)
    }).Repeat((Action<CodeMatcher>) (c => c.Set(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_BaseMapOrCaravan_Thing)), (Action<string>) null).InstructionEnumeration();
  }

  private static Vector3 LocalizeForPawnIfNotOnVehicleMap(Pawn pawn, Vector3 world)
  {
    return !VehicleMapUtility.get_IsOnNonFocusedVehicleMap((Thing) pawn) ? ModCompat.AsAboveSoBelow.LocalizeForPawn(pawn, world) : world;
  }

  private static Vector3 LocalizeForPawnTarget(Pawn pawn, ref LocalTargetInfo target)
  {
    int num;
    if (!VehicleMapUtility.get_IsOnNonFocusedVehicleMap(((LocalTargetInfo) ref target).Thing) && (!(pawn.stances.curStance is Stance_Busy) || !VehicleMapUtility.get_IsNonFocusedVehicleMap(TargetMapUtility.get_TargetMap((Thing) pawn))))
    {
      Job curJob = pawn.CurJob;
      if (curJob != null)
      {
        GlobalTargetInfo globalTarget = curJob.globalTarget;
        Map map = ((GlobalTargetInfo) ref globalTarget).Map;
        if (map != null && VehicleMapUtility.get_IsNonFocusedVehicleMap(map))
          goto label_7;
      }
      if (pawn.CurJob?.GetCachedDriver(pawn) is JobDriverAcrossMaps cachedDriver)
      {
        Map destMap = cachedDriver.DestMap;
        if (destMap != null)
        {
          num = VehicleMapUtility.get_IsNonFocusedVehicleMap(destMap) ? 1 : 0;
          goto label_8;
        }
      }
      num = 0;
      goto label_8;
    }
label_7:
    num = 1;
label_8:
    bool flag1 = num != 0;
    if (!flag1)
    {
      bool flag2 = VehicleMapUtility.get_IsOnNonFocusedVehicleMap((Thing) pawn);
      if (flag2)
      {
        bool flag3;
        if (pawn.stances.curStance is Stance_Busy curStance)
        {
          switch (curStance.verb)
          {
            case Verb_Jump _:
            case Verb_CastAbilityJump _:
              flag3 = true;
              goto label_14;
          }
        }
        flag3 = false;
label_14:
        flag2 = !flag3;
      }
      flag1 = flag2;
    }
    return flag1 ? Patch_Pawn_JobTracker_DrawLinesBetweenTargets.CenterVector3VehicleOffset(ref target, pawn) : ModCompat.AsAboveSoBelow.LocalizeForPawn(pawn, ((LocalTargetInfo) ref target).CenterVector3);
  }
}
