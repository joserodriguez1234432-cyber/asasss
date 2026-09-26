// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_JumpUtility_CanHitTargetFrom
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (JumpUtility), "CanHitTargetFrom")]
[PatchLevel(Level.Sensitive)]
public static class Patch_JumpUtility_CanHitTargetFrom
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.MethodReplacer(((MethodBase) MethodInfoCache.CachedMethodInfo.g_Thing_Map, (MethodBase) MethodInfoCache.CachedMethodInfo.m_BaseMap_Thing), ((MethodBase) MethodInfoCache.CachedMethodInfo.g_Thing_Position, (MethodBase) MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMap), ((MethodBase) MethodInfoCache.CachedMethodInfo.g_LocalTargetInfo_Cell, (MethodBase) MethodInfoCache.CachedMethodInfo.m_TargetCellOnBaseMap), ((MethodBase) MethodInfoCache.CachedMethodInfo.m_GenSight_LineOfSight1, (MethodBase) MethodInfoCache.CachedMethodInfo.m_GenSightOnVehicle_LineOfSight1)).ToList<CodeInstruction>();
    list.Insert(list.FindIndex((Predicate<CodeInstruction>) (c => CodeInstructionExtensions.Calls(c, MethodInfoCache.CachedMethodInfo.m_TargetCellOnBaseMap))), CodeInstruction.LoadArgument(0, false));
    return (IEnumerable<CodeInstruction>) list;
  }
}
