// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ReservationManager_Reserve
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (ReservationManager), "Reserve")]
public static class Patch_ReservationManager_Reserve
{
  [PatchLevel(Level.Safe)]
  public static void Prefix(
    ref ReservationManager __instance,
    Map ___map,
    Pawn claimant,
    Job job,
    LocalTargetInfo target)
  {
    Map map;
    if (!Patch_ReservationManager_Reserve.ShouldReplace(___map, claimant, target, false, out map, job))
      return;
    __instance = map.reservationManager;
  }

  public static bool ShouldReplace(
    Map ___map,
    Pawn claimant,
    LocalTargetInfo target,
    bool allowSameMap,
    out Map map,
    Job job = null)
  {
    ref Map local = ref map;
    Map map1 = ((LocalTargetInfo) ref target).Thing?.MapHeld;
    if (map1 == null)
    {
      Map targetMap = TargetMapUtility.get_TargetMap((Thing) claimant);
      if (targetMap == null)
      {
        if (job == null || !LocalTargetInfo.op_Equality(GlobalTargetInfo.op_Explicit(job.globalTarget), target))
        {
          Lord lord = LordUtility.GetLord(claimant);
          map1 = lord == null || !(lord.LordJob is LordJob_Ritual) ? (Map) null : lord.Map;
        }
        else
          map1 = ((GlobalTargetInfo) ref job.globalTarget).Map;
      }
      else
        map1 = targetMap;
    }
    local = map1;
    if (map == null)
      return false;
    return allowSameMap || ___map != map;
  }

  [PatchLevel(Level.Sensitive)]
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    CodeMatcher codeMatcher = new CodeMatcher(instructions, (ILGenerator) null);
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Map)
    });
    codeMatcher.Repeat((Action<CodeMatcher>) (c =>
    {
      c.InstructionAt(-1).opcode = OpCodes.Ldarg_0;
      c.Opcode = OpCodes.Ldfld;
      c.Operand = (object) AccessTools.Field(typeof (ReservationManager), "map");
    }), (Action<string>) null);
    return (IEnumerable<CodeInstruction>) codeMatcher.Instructions();
  }
}
