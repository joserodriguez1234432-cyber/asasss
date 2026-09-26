// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Verb_AlignedTrackingShot_CanEngage
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_DefensiveNetwork")]
[HarmonyPatch]
[PatchLevel(Level.Cautious)]
public static class Patch_Verb_AlignedTrackingShot_CanEngage
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(((MethodBase) MethodInfoCache.CachedMethodInfo.g_Thing_Map, (MethodBase) MethodInfoCache.CachedMethodInfo.m_BaseMap_Thing), ((MethodBase) MethodInfoCache.CachedMethodInfo.g_Thing_Position, (MethodBase) MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMapSpawned), ((MethodBase) MethodInfoCache.CachedMethodInfo.g_LocalTargetInfo_Cell, (MethodBase) MethodInfoCache.CachedMethodInfo.m_CellOnBaseMapSpawned), ((MethodBase) MethodInfoCache.CachedMethodInfo.m_GenSight_LineOfSight2, (MethodBase) MethodInfoCache.CachedMethodInfo.m_GenSightOnVehicle_LineOfSight2));
  }
}
