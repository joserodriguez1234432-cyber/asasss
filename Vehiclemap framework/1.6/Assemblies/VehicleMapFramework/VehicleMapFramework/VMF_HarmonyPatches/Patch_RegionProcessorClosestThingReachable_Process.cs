// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_RegionProcessorClosestThingReachable_ProcessThing
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (RegionProcessorClosestThingReachable), "ProcessThing")]
[HarmonyPriority(600)]
[PatchLevel(Level.Mandatory)]
public static class Patch_RegionProcessorClosestThingReachable_ProcessThing
{
  [HarmonyReversePatch]
  public static void ProcessThing(
    RegionProcessorClosestThingReachable instance,
    Region reg,
    Thing t)
  {
    Transpiler((IEnumerable<CodeInstruction>) null);
    throw new NotImplementedException();

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
      return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Thing_PositionHeld, MethodInfoCache.CachedMethodInfo.m_PositionHeldOnBaseMap);
    }
  }
}
