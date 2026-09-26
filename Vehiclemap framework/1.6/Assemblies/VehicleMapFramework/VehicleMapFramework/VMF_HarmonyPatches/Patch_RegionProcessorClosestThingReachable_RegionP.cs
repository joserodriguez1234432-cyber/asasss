// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_RegionProcessorClosestThingReachable_RegionProcessor
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

[HarmonyPatch(typeof (RegionProcessorClosestThingReachable), "RegionProcessor")]
[HarmonyPriority(400)]
[PatchLevel(Level.Mandatory)]
public static class Patch_RegionProcessorClosestThingReachable_RegionProcessor
{
  [HarmonyReversePatch]
  public static bool RegionProcessorBaseMapCoord(
    this RegionProcessorClosestThingReachable instance,
    Region reg)
  {
    Transpiler((IEnumerable<CodeInstruction>) null);
    throw new NotImplementedException();

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
      MethodInfo from = AccessTools.Method(typeof (RegionProcessorClosestThingReachable), "ProcessThing", (Type[]) null, (Type[]) null);
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      MethodInfo method = (Patch_RegionProcessorClosestThingReachable_RegionProcessor.\u003C\u003EO.\u003C0\u003E__ProcessThing ?? (Patch_RegionProcessorClosestThingReachable_RegionProcessor.\u003C\u003EO.\u003C0\u003E__ProcessThing = new Action<RegionProcessorClosestThingReachable, Region, Thing>(Patch_RegionProcessorClosestThingReachable_ProcessThing.ProcessThing))).Method;
      return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(from, method);
    }
  }
}
