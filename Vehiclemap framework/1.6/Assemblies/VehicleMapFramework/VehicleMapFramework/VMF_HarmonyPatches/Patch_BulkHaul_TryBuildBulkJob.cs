// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_BulkHaul_TryBuildBulkJob
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

[HarmonyPatchCategory("VMF_Patches_HaulersDream")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_BulkHaul_TryBuildBulkJob
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Map)
    }).InsertAndAdvance(new CodeInstruction[1]
    {
      CodeInstruction.LoadArgument(1, false)
    }).Set(OpCodes.Call, (object) (Patch_BulkHaul_TryBuildBulkJob.\u003C\u003EO.\u003C0\u003E__ThingMapOrPawnMap ?? (Patch_BulkHaul_TryBuildBulkJob.\u003C\u003EO.\u003C0\u003E__ThingMapOrPawnMap = new Func<Pawn, Thing, Map>(Patch_BulkHaul_TryBuildBulkJob.ThingMapOrPawnMap))).Method).InstructionEnumeration();
  }

  private static Map ThingMapOrPawnMap(Pawn pawn, Thing primary)
  {
    return primary.Map ?? ((Thing) pawn).Map;
  }
}
