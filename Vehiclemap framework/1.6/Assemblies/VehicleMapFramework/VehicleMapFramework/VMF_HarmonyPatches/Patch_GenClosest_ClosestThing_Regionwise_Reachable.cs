// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_GenClosest_ClosestThing_Regionwise_ReachablePrioritized
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (GenClosest), "ClosestThing_Regionwise_ReachablePrioritized")]
[PatchLevel(Level.Safe)]
public static class Patch_GenClosest_ClosestThing_Regionwise_ReachablePrioritized
{
  public static void Prefix(ref IntVec3 root, ref Map map, TraverseParms traverseParams)
  {
    if (!VehicleMapUtility.get_CrossMapContext(map))
      return;
    Pawn pawn = traverseParams.pawn;
    if (pawn == null || !IntVec3.op_Equality(root, ((Thing) pawn).Position) || map != ((Thing) pawn).Map)
      return;
    map = CrossMapReachabilityUtility.get_DepartMap(pawn) ?? map;
    root = CrossMapReachabilityUtility.get_DepartPosition(pawn) ?? root;
  }

  public static void Postfix(
    IntVec3 root,
    Map map,
    ThingRequest thingReq,
    PathEndMode peMode,
    TraverseParms traverseParams,
    float maxDistance,
    Predicate<Thing> validator,
    Func<Thing, float> priorityGetter,
    int minRegions,
    int maxRegions,
    bool lookInHaulSources,
    ref Thing __result)
  {
    Pawn pawn = traverseParams.pawn;
    bool flag;
    if (pawn != null)
    {
      Faction faction = ((Thing) pawn).Faction;
      if (faction == null || faction.IsPlayer)
      {
        flag = false;
        goto label_4;
      }
    }
    flag = true;
label_4:
    if (flag || __result != null)
      return;
    __result = GenClosestCrossMap.ClosestThing_Regionwise_ReachablePrioritized(root, map, thingReq, peMode, traverseParams, maxDistance, validator, priorityGetter, minRegions, maxRegions, lookInHaulSources);
  }
}
