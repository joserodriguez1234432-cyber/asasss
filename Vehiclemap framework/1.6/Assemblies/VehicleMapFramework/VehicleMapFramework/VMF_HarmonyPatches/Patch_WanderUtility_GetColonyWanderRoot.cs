// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_WanderUtility_GetColonyWanderRoot
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (WanderUtility), "GetColonyWanderRoot")]
[PatchLevel(Level.Cautious)]
public static class Patch_WanderUtility_GetColonyWanderRoot
{
  public static List<Pawn> FreeColonistsSpawned(MapPawns instance)
  {
    return Patch_MapPawns_FreeHumanlikesSpawnedOfFaction.FreeHumanlikesSpawnedOfFaction(instance, Faction.OfPlayer);
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(AccessTools.PropertyGetter(typeof (MapPawns), "FreeColonistsSpawned"), (Patch_WanderUtility_GetColonyWanderRoot.\u003C\u003EO.\u003C0\u003E__FreeColonistsSpawned ?? (Patch_WanderUtility_GetColonyWanderRoot.\u003C\u003EO.\u003C0\u003E__FreeColonistsSpawned = new Func<MapPawns, List<Pawn>>(Patch_WanderUtility_GetColonyWanderRoot.FreeColonistsSpawned))).Method);
  }
}
