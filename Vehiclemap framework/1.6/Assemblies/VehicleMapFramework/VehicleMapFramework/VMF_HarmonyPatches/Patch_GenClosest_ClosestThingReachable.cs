// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_GenClosest_ClosestThingReachable
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (GenClosest), "ClosestThingReachable")]
public static class Patch_GenClosest_ClosestThingReachable
{
  public static bool forceCrossMap;

  [HarmonyReversePatch]
  [PatchLevel(Level.Mandatory)]
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static Thing ClosestThingReachableOriginal(
    IntVec3 root,
    Map map,
    ThingRequest thingReq,
    PathEndMode peMode,
    TraverseParms traverseParams,
    float maxDistance,
    Predicate<Thing> validator,
    IEnumerable<Thing> customGlobalSearchSet,
    int searchRegionsMin,
    int searchRegionsMax,
    bool forceAllowGlobalSearch,
    RegionType traversableRegionTypes,
    bool ignoreEntirelyForbiddenRegions,
    bool lookInHaulSources)
  {
    throw new NotImplementedException();
  }

  [PatchLevel(Level.Safe)]
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

  [PatchLevel(Level.Safe)]
  public static void Postfix(
    IntVec3 root,
    Map map,
    ThingRequest thingReq,
    PathEndMode peMode,
    TraverseParms traverseParams,
    float maxDistance,
    Predicate<Thing> validator,
    IEnumerable<Thing> customGlobalSearchSet,
    int searchRegionsMin,
    int searchRegionsMax,
    bool forceAllowGlobalSearch,
    RegionType traversableRegionTypes,
    bool ignoreEntirelyForbiddenRegions,
    bool lookInHaulSources,
    ref Thing __result)
  {
    if (!VehicleMapUtility.get_CrossMapContext(map))
      return;
    Pawn pawn = traverseParams.pawn;
    if (pawn == null)
      return;
    if (!Patch_GenClosest_ClosestThingReachable.forceCrossMap && pawn != null)
    {
      Faction faction = ((Thing) pawn).Faction;
      if (faction != null && !faction.IsPlayer)
        return;
    }
    customGlobalSearchSet = customGlobalSearchSet != null ? customGlobalSearchSet.Where<Thing>((Func<Thing, bool>) (t => t.Map != map)) : (IEnumerable<Thing>) null;
    if (__result != null)
      return;
    __result = GenClosestCrossMap.ClosestThingReachable(root, map, thingReq, peMode, traverseParams, maxDistance, validator, customGlobalSearchSet, searchRegionsMin, searchRegionsMax, forceAllowGlobalSearch, traversableRegionTypes, ignoreEntirelyForbiddenRegions, lookInHaulSources);
  }
}
