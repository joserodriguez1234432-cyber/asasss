// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_JobDriver_HaulToContainer_TryReplaceWithFrame
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (JobDriver_HaulToContainer), "TryReplaceWithFrame")]
[PatchLevel(Level.Cautious)]
public static class Patch_JobDriver_HaulToContainer_TryReplaceWithFrame
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
      CodeInstruction.LoadLocal(0, false)
    }).Set(OpCodes.Call, (object) (Patch_JobDriver_HaulToContainer_TryReplaceWithFrame.\u003C\u003EO.\u003C0\u003E__ThingMapOrPawnMap ?? (Patch_JobDriver_HaulToContainer_TryReplaceWithFrame.\u003C\u003EO.\u003C0\u003E__ThingMapOrPawnMap = new Func<Pawn, Thing, Map>(Patch_JobDriver_HaulToContainer_TryReplaceWithFrame.ThingMapOrPawnMap))).Method).InstructionEnumeration();
  }

  private static Map ThingMapOrPawnMap(Pawn pawn, Thing t) => t.Map ?? ((Thing) pawn).Map;
}
