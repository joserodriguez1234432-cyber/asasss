// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_HaulingUtility_TryGetHaulingDestination
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_StackGap")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_HaulingUtility_TryGetHaulingDestination
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    CodeMatcher codeMatcher = new CodeMatcher(instructions, (ILGenerator) null);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls((Patch_HaulingUtility_TryGetHaulingDestination.\u003C\u003EO.\u003C0\u003E__GetSlotGroup ?? (Patch_HaulingUtility_TryGetHaulingDestination.\u003C\u003EO.\u003C0\u003E__GetSlotGroup = new Func<IntVec3, Map, SlotGroup>(StoreUtility.GetSlotGroup))).Method)
    });
    codeMatcher.MatchStartBackwards(new CodeMatch[1]
    {
      new CodeMatch(new OpCode?(OpCodes.Ldarg_2), (object) null, (string) null)
    });
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    codeMatcher.Insert(new CodeInstruction[3]
    {
      CodeInstruction.LoadArgument(0, false),
      CodeInstruction.LoadArgument(1, false),
      new CodeInstruction(OpCodes.Call, (object) (Patch_HaulingUtility_TryGetHaulingDestination.\u003C\u003EO.\u003C1\u003E__TryReplaceMap ?? (Patch_HaulingUtility_TryGetHaulingDestination.\u003C\u003EO.\u003C1\u003E__TryReplaceMap = new \u003C\u003EA\u007B00000008\u007D<Job, Map>(Patch_HaulingUtility_TryGetHaulingDestination.TryReplaceMap))).Method)
    });
    return (IEnumerable<CodeInstruction>) codeMatcher.Instructions();
  }

  private static void TryReplaceMap(Job job, ref Map map)
  {
    Map map1 = job != null ? ((GlobalTargetInfo) ref job.globalTarget).Map : (Map) null;
    if (map1 == null)
      return;
    map = map1;
  }
}
