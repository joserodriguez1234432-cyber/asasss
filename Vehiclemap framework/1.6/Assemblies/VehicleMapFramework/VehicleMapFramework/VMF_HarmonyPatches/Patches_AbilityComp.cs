// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patches_AbilityComp
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patches_AbilityComp
{
  private static IEnumerable<MethodBase> TargetMethods()
  {
    return ((IEnumerable<MethodBase>) GenTypes.AllSubclasses(typeof (AbilityComp)).Append<Type>(typeof (GenClamor)).SelectMany<Type, MethodInfo>((Func<Type, IEnumerable<MethodInfo>>) (type => (IEnumerable<MethodInfo>) AccessToolsExtensions.GetDeclaredMethods(type)))).WhereCallsMethod((MethodBase) MethodInfoCache.CachedMethodInfo.g_Thing_Position, (MethodBase) MethodInfoCache.CachedMethodInfo.g_LocalTargetInfo_Cell, (MethodBase) MethodInfoCache.CachedMethodInfo.g_Thing_Map, (MethodBase) MethodInfoCache.CachedMethodInfo.g_Thing_MapHeld, (MethodBase) MethodInfoCache.CachedMethodInfo.m_OccupiedRect, (MethodBase) MethodInfoCache.CachedMethodInfo.m_BreadthFirstTraverse);
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    if (UnitTestDetector.IsTestingContext)
      return instructions;
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(((MethodBase) MethodInfoCache.CachedMethodInfo.g_Thing_Position, (MethodBase) MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMapSpawned), ((MethodBase) MethodInfoCache.CachedMethodInfo.m_OccupiedRect, (MethodBase) MethodInfoCache.CachedMethodInfo.m_MovedOccupiedRect), ((MethodBase) MethodInfoCache.CachedMethodInfo.g_LocalTargetInfo_Cell, (MethodBase) MethodInfoCache.CachedMethodInfo.m_CellOnBaseMapSpawned), ((MethodBase) MethodInfoCache.CachedMethodInfo.g_Thing_Map, (MethodBase) MethodInfoCache.CachedMethodInfo.m_BaseMap_Thing), ((MethodBase) MethodInfoCache.CachedMethodInfo.g_Thing_MapHeld, (MethodBase) MethodInfoCache.CachedMethodInfo.m_MapHeldBaseMap), ((MethodBase) MethodInfoCache.CachedMethodInfo.m_BreadthFirstTraverse, (MethodBase) MethodInfoCache.CachedMethodInfo.m_BreadthFirstTraverseAcrossMaps));
  }
}
