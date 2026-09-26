// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_CompPowerPlantWind_PostDraw
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (CompPowerPlantWind), "PostDraw")]
[PatchLevel(Level.Cautious)]
public static class Patch_CompPowerPlantWind_PostDraw
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(((MethodBase) MethodInfoCache.CachedMethodInfo.g_Thing_Rotation, (MethodBase) MethodInfoCache.CachedMethodInfo.m_BaseFullRotation_Thing), ((MethodBase) MethodInfoCache.CachedMethodInfo.g_Rot4_FacingCell, (MethodBase) MethodInfoCache.CachedMethodInfo.g_Rot8_FacingCell), ((MethodBase) MethodInfoCache.CachedMethodInfo.g_Rot4_RighthandCell, (MethodBase) MethodInfoCache.CachedMethodInfo.m_Rot8Utility_RighthandCell), ((MethodBase) MethodInfoCache.CachedMethodInfo.m_Rot4_Rotate, (MethodBase) MethodInfoCache.CachedMethodInfo.m_Rot8_Rotate), ((MethodBase) MethodInfoCache.CachedMethodInfo.g_Rot4_AsQuat, (MethodBase) MethodInfoCache.CachedMethodInfo.m_Rot8_AsQuatRef), ((MethodBase) MethodInfoCache.CachedMethodInfo.m_IntVec3_ToVector3, (MethodBase) MethodInfoCache.CachedMethodInfo.m_Rot8Utility_ToFundVector3));
  }
}
