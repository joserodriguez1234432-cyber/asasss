// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_BuildingTargeter_IsValidTarget
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_DefensiveNetwork")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_BuildingTargeter_IsValidTarget
{
  private static IEnumerable<MethodBase> TargetMethods()
  {
    yield return (MethodBase) AccessTools.Method("DNX.Building_TacticalMarkerTower:IsValidTarget", (Type[]) null, (Type[]) null);
    yield return (MethodBase) AccessTools.Method("DNX.Building_SustainedLaserEmitter:IsValidTarget", new Type[2]
    {
      typeof (Pawn),
      typeof (float)
    }, (Type[]) null);
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer((MethodInfoCache.CachedMethodInfo.g_Thing_Map, MethodInfoCache.CachedMethodInfo.m_BaseMap_Thing), (MethodInfoCache.CachedMethodInfo.g_Thing_Position, MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMapSpawned), (MethodInfoCache.CachedMethodInfo.m_GenSight_LineOfSight2, MethodInfoCache.CachedMethodInfo.m_GenSightOnVehicle_LineOfSight2));
  }
}
