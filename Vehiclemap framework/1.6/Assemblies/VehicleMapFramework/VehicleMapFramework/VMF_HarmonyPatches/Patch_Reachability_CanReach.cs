// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Reachability_CanReach
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Reachability), "CanReach", new Type[] {typeof (IntVec3), typeof (LocalTargetInfo), typeof (PathEndMode), typeof (TraverseParms)})]
public static class Patch_Reachability_CanReach
{
  [PatchLevel(Level.Safe)]
  public static bool Prefix(
    ref IntVec3 start,
    LocalTargetInfo dest,
    PathEndMode peMode,
    TraverseParms traverseParams,
    Map ___map,
    ref bool __result)
  {
    if (CrossMapReachabilityUtility.working)
      return true;
    Pawn pawn = traverseParams.pawn;
    Map map1 = CrossMapReachabilityUtility.DestMapGlobal;
    if (map1 == null)
    {
      Map destMap = CrossMapReachabilityUtility.get_DestMap(pawn);
      if (destMap == null)
      {
        Map mapHeld = ((LocalTargetInfo) ref dest).Thing?.MapHeld;
        if (mapHeld == null)
        {
          TargetInfo target;
          if (!((Thing) pawn).IsTargeting(dest, out target))
          {
            Lord lord = pawn != null ? LordUtility.GetLord(pawn) : (Lord) null;
            map1 = lord == null || !(lord.LordJob is LordJob_Ritual) ? ___map : lord.Map;
          }
          else
            map1 = ((TargetInfo) ref target).Map;
        }
        else
          map1 = mapHeld;
      }
      else
        map1 = destMap;
    }
    Map map2 = map1;
    if (map2 == null)
      return true;
    Map map3 = CrossMapReachabilityUtility.DepartMapGlobal;
    IntVec3? departPosition;
    if (map3 == null)
    {
      if (pawn != null)
      {
        IntVec3 intVec3 = start;
        departPosition = CrossMapReachabilityUtility.get_DepartPosition(pawn);
        if ((departPosition.HasValue ? (IntVec3.op_Equality(intVec3, departPosition.GetValueOrDefault()) ? 1 : 0) : 0) != 0 || IntVec3.op_Equality(start, ((Thing) pawn).Position))
        {
          map3 = CrossMapReachabilityUtility.get_DepartMap(pawn) ?? ___map;
          goto label_15;
        }
      }
      map3 = ___map;
    }
label_15:
    Map map4 = map3;
    if (map4 == null || map4 == map2 && map4 == ___map)
      return true;
    ref bool local = ref __result;
    Map departMap = map4;
    departPosition = CrossMapReachabilityUtility.get_DepartPosition(pawn);
    IntVec3 root = departPosition ?? start;
    LocalTargetInfo dest1 = dest;
    PathEndMode peMode1 = peMode;
    TraverseParms traverseParms = traverseParams;
    Map destMap1 = map2;
    int num = CrossMapReachabilityUtility.CanReach(departMap, root, dest1, peMode1, traverseParms, destMap1) ? 1 : 0;
    local = num != 0;
    return false;
  }

  [PatchLevel(Level.Cautious)]
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Map)
    }).SetInstruction(PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_BaseMapOrCaravan_Thing)).MatchStartForward(new CodeMatch[1]
    {
      new CodeMatch(new OpCode?(OpCodes.Beq_S), (object) null, (string) null)
    }).Insert(new CodeInstruction[1]
    {
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_BaseMapOrCaravan_Map)
    }).InstructionEnumeration();
  }
}
