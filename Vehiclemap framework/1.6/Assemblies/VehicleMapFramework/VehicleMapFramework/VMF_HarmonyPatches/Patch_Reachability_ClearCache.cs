// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Reachability_ClearCache
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Reachability), "ClearCache")]
[PatchLevel(Level.Sensitive)]
public static class Patch_Reachability_ClearCache
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(AccessTools.Method(typeof (ReachabilityCache), "Clear", (Type[]) null, (Type[]) null))
    }).InsertAfter(new CodeInstruction[4]
    {
      CodeInstruction.LoadArgument(0, false),
      CodeInstruction.LoadField(typeof (Reachability), "map", false),
      new CodeInstruction(OpCodes.Ldc_I4_0, (object) null),
      PatchHelper.get_CallInstruction(CrossMapReachabilityCache.ClearCacheFor.Method)
    }).InstructionEnumeration();
  }
}
