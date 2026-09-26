// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_MapPawns_AllPawnsSpawned
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyBefore(new string[] {"SmashPhil.VehicleFramework"})]
[HarmonyPatchCategory("VehicleMapFramework.EarlyPatches")]
[HarmonyPatch]
[PatchLevel(Level.Mandatory)]
public static class Patch_MapPawns_AllPawnsSpawned
{
  private static readonly CrossMapMapPawnsCache cache = new CrossMapMapPawnsCache((CrossMapMapPawnsCache.PawnsGetter) ((instance, _) => Patch_MapPawns_AllPawnsSpawned.AllPawnsSpawned(instance)));

  public static void Postfix(ref IReadOnlyList<Pawn> __result, Map ___map)
  {
    if (VehiclePawnWithMapCache.AllVehiclesOn(___map).Count == 0)
      return;
    __result = (IReadOnlyList<Pawn>) Patch_MapPawns_AllPawnsSpawned.cache.Get(___map, (IEnumerable<Pawn>) __result);
  }

  [HarmonyReversePatch]
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static List<Pawn> AllPawnsSpawned(MapPawns instance)
  {
    throw new NotImplementedException();
  }
}
