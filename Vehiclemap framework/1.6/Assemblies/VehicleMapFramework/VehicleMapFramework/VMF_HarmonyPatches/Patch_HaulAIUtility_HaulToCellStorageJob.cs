// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_HaulAIUtility_HaulToCellStorageJob
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (HaulAIUtility), "HaulToCellStorageJob")]
public static class Patch_HaulAIUtility_HaulToCellStorageJob
{
  [PatchLevel(Level.Cautious)]
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Thing_Map, MethodInfoCache.CachedMethodInfo.m_TargetMapOrPawnMap);
  }

  [HarmonyBefore(new string[] {"Andromeda.StackGap"})]
  [PatchLevel(Level.Safe)]
  public static void Postfix(Pawn p, IntVec3 storeCell, Job __result)
  {
    Map map;
    if (!((Thing) p).TryGetTargetMap(out map) || __result == null)
      return;
    __result.globalTarget = new GlobalTargetInfo(storeCell, map, false);
  }
}
