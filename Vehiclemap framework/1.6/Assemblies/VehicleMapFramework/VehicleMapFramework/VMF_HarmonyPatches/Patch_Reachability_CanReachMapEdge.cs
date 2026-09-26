// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Reachability_CanReachMapEdge
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Reachability), "CanReachMapEdge", new Type[] {typeof (IntVec3), typeof (TraverseParms)})]
[PatchLevel(Level.Cautious)]
public static class Patch_Reachability_CanReachMapEdge
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) Patch_Reachability_CanReach.Transpiler(instructions).MethodReplacer(MethodInfoCache.CachedMethodInfo.m_BreadthFirstTraverse, MethodInfoCache.CachedMethodInfo.m_BreadthFirstTraverseAcrossMaps);
  }
}
