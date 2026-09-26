// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Verb_ShootBeam_GetBeamHitNeighbourCells
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

[HarmonyPatch]
[PatchLevel(Level.Cautious)]
public static class Patch_Verb_ShootBeam_GetBeamHitNeighbourCells
{
  private static MethodBase TargetMethod()
  {
    return (MethodBase) AccessTools.FindIncludingInnerTypes<MethodInfo>(typeof (Verb_ShootBeam), (Func<Type, MethodInfo>) (t => t.Name.Contains("<GetBeamHitNeighbourCells>") ? AccessTools.Method(t, "MoveNext", (Type[]) null, (Type[]) null) : (MethodInfo) null));
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer((MethodInfoCache.CachedMethodInfo.g_Thing_Map, MethodInfoCache.CachedMethodInfo.m_BaseMap_Thing), (MethodInfoCache.CachedMethodInfo.m_GenSight_LineOfSight1, MethodInfoCache.CachedMethodInfo.m_GenSightOnVehicle_LineOfSight1));
  }
}
