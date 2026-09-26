// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_WorkGiver_HaulToInventory_PotentialWorkThingsGlobal
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_PickUpAndHaul")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_WorkGiver_HaulToInventory_PotentialWorkThingsGlobal
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    CodeMatcher codeMatcher = new CodeMatcher(instructions, (ILGenerator) null);
    FieldInfo fieldInfo = AccessTools.Field("PickUpAndHaul.WorkGiver_HaulToInventory+ThingPositionComparer:rootCell");
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.StoresField(fieldInfo)
    });
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    codeMatcher.Insert(new CodeInstruction[2]
    {
      CodeInstruction.LoadArgument(1, false),
      PatchHelper.get_CallInstruction((Patch_WorkGiver_HaulToInventory_PotentialWorkThingsGlobal.\u003C\u003EO.\u003C0\u003E__ToBaseMapCoord ?? (Patch_WorkGiver_HaulToInventory_PotentialWorkThingsGlobal.\u003C\u003EO.\u003C0\u003E__ToBaseMapCoord = new Func<IntVec3, Pawn, IntVec3>(Patch_WorkGiver_HaulToInventory_PotentialWorkThingsGlobal.ToBaseMapCoord))).Method)
    });
    return (IEnumerable<CodeInstruction>) codeMatcher.Instructions();
  }

  public static IntVec3 ToBaseMapCoord(IntVec3 c, Pawn pawn)
  {
    return c.ToBaseMapCoord(CrossMapReachabilityUtility.get_DepartMap(pawn) ?? ((Thing) pawn).Map);
  }
}
