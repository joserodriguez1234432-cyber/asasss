// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_WG_AbilityVerb_QuickJump_DoJump
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_ExosuitFramework")]
[HarmonyPatch]
[HarmonyPatch(new Type[] {typeof (Pawn), typeof (Map), typeof (LocalTargetInfo), typeof (LocalTargetInfo), typeof (bool), typeof (bool)})]
public static class Patch_WG_AbilityVerb_QuickJump_DoJump
{
  [PatchLevel(Level.Safe)]
  public static void Prefix(Pawn pawn, Map targetMap, ref LocalTargetInfo currentTarget)
  {
    if (!VehicleMapUtility.get_IsOnNonFocusedVehicleMap((Thing) pawn))
      return;
    IntVec3 positionOnBaseMap = VehicleMapUtility.get_PositionOnBaseMap((Thing) pawn);
    currentTarget = LocalTargetInfo.op_Implicit(new IntVec3(positionOnBaseMap.x, positionOnBaseMap.y, Math.Min(positionOnBaseMap.z + 25, CellRect.WholeMap(targetMap).maxZ)));
  }

  [PatchLevel(Level.Cautious)]
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer((MethodInfoCache.CachedMethodInfo.g_Thing_Position, MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMap), (MethodInfoCache.CachedMethodInfo.g_LocalTargetInfo_Cell, MethodInfoCache.CachedMethodInfo.m_CellOnBaseMap), (MethodInfoCache.CachedMethodInfo.g_Thing_Map, MethodInfoCache.CachedMethodInfo.m_BaseMap_Thing));
  }
}
