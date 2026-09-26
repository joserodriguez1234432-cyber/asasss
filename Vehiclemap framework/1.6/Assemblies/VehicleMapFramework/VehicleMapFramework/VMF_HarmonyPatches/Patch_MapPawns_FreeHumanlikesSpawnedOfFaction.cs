// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_MapPawns_FreeHumanlikesSpawnedOfFaction
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (MapPawns), "FreeHumanlikesSpawnedOfFaction")]
public static class Patch_MapPawns_FreeHumanlikesSpawnedOfFaction
{
  private static readonly CrossMapMapPawnsCache cache = new CrossMapMapPawnsCache(new CrossMapMapPawnsCache.PawnsGetter(Patch_MapPawns_FreeHumanlikesSpawnedOfFaction.FreeHumanlikesSpawnedOfFaction));

  [PatchLevel(Level.Safe)]
  public static void Postfix(ref List<Pawn> __result, Map ___map, Faction faction)
  {
    if (VehiclePawnWithMapCache.AllVehiclesOn(___map).Count == 0)
      return;
    __result = Patch_MapPawns_FreeHumanlikesSpawnedOfFaction.cache.Get(___map, (IEnumerable<Pawn>) __result, faction);
  }

  [PatchLevel(Level.Mandatory)]
  [HarmonyReversePatch]
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static List<Pawn> FreeHumanlikesSpawnedOfFaction(MapPawns instance, Faction faction)
  {
    throw new NotImplementedException();
  }
}
