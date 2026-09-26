// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_FireProperties_GetAimOnTargetChance
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_AvoidFriendlyFire")]
[HarmonyPatch]
[PatchLevel(Level.Cautious)]
public static class Patch_FireProperties_GetAimOnTargetChance
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer((MethodInfoCache.CachedMethodInfo.g_Thing_Position, MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMap), ((Patch_FireProperties_GetAimOnTargetChance.\u003C\u003EO.\u003C0\u003E__Roofed ?? (Patch_FireProperties_GetAimOnTargetChance.\u003C\u003EO.\u003C0\u003E__Roofed = new Func<IntVec3, Map, bool>(GridsUtility.Roofed))).Method, (Patch_FireProperties_GetAimOnTargetChance.\u003C\u003EO.\u003C1\u003E__RoofedAcrossMaps ?? (Patch_FireProperties_GetAimOnTargetChance.\u003C\u003EO.\u003C1\u003E__RoofedAcrossMaps = new Func<IntVec3, Map, bool>(VehicleMapUtility.RoofedAcrossMaps))).Method), ((Patch_FireProperties_GetAimOnTargetChance.\u003C\u003EO.\u003C2\u003E__CanBeSeenOver ?? (Patch_FireProperties_GetAimOnTargetChance.\u003C\u003EO.\u003C2\u003E__CanBeSeenOver = new Func<IntVec3, Map, bool>(GenGrid.CanBeSeenOver))).Method, (Patch_FireProperties_GetAimOnTargetChance.\u003C\u003EO.\u003C3\u003E__CanBeSeenOverOnVehicle ?? (Patch_FireProperties_GetAimOnTargetChance.\u003C\u003EO.\u003C3\u003E__CanBeSeenOverOnVehicle = new Func<IntVec3, Map, bool>(GenSightOnVehicle.CanBeSeenOverOnVehicle))).Method));
  }
}
