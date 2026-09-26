// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_AnimalPenUtility_GetHitchingPostAnimalShouldBeTakenTo
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (AnimalPenUtility), "GetHitchingPostAnimalShouldBeTakenTo")]
[PatchLevel(Level.Sensitive)]
public static class Patch_AnimalPenUtility_GetHitchingPostAnimalShouldBeTakenTo
{
  private static readonly HashSet<Building> tmpBuildings = new HashSet<Building>();

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.LoadsField(AccessTools.Field(typeof (ListerBuildings), "allBuildingsHitchingPosts"), false)
    }).InsertAfter(new CodeInstruction[2]
    {
      CodeInstruction.LoadArgument(1, false),
      PatchHelper.get_CallInstruction((Patch_AnimalPenUtility_GetHitchingPostAnimalShouldBeTakenTo.\u003C\u003EO.\u003C0\u003E__AddHitchingPosts ?? (Patch_AnimalPenUtility_GetHitchingPostAnimalShouldBeTakenTo.\u003C\u003EO.\u003C0\u003E__AddHitchingPosts = new Func<HashSet<Building>, Pawn, HashSet<Building>>(Patch_AnimalPenUtility_GetHitchingPostAnimalShouldBeTakenTo.AddHitchingPosts))).Method)
    }).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Position)
    }).Advance(1).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Position)
    }).Repeat((Action<CodeMatcher>) (matcher => matcher.Set(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMap)), (Action<string>) null).InstructionEnumeration();
  }

  private static HashSet<Building> AddHitchingPosts(HashSet<Building> hashSet, Pawn animal)
  {
    Patch_AnimalPenUtility_GetHitchingPostAnimalShouldBeTakenTo.tmpBuildings.Clear();
    GenCollection.AddRange<Building>(Patch_AnimalPenUtility_GetHitchingPostAnimalShouldBeTakenTo.tmpBuildings, hashSet);
    GenCollection.AddRange<Building>(Patch_AnimalPenUtility_GetHitchingPostAnimalShouldBeTakenTo.tmpBuildings, ((Thing) animal).Map.BaseMapAndVehicleMaps(false).SelectMany<Map, Building>((Func<Map, IEnumerable<Building>>) (m => (IEnumerable<Building>) m.listerBuildings.allBuildingsHitchingPosts)));
    return Patch_AnimalPenUtility_GetHitchingPostAnimalShouldBeTakenTo.tmpBuildings;
  }
}
