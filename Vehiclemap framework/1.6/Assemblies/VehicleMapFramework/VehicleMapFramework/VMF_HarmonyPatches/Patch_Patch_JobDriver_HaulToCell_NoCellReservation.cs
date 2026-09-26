// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Patch_JobDriver_HaulToCell_NoCellReservation_Prefix
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_HaulersDream")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_Patch_JobDriver_HaulToCell_NoCellReservation_Prefix
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
      CodeInstruction.LoadArgument(0, false)
    }).Set(OpCodes.Call, (object) (Patch_Patch_JobDriver_HaulToCell_NoCellReservation_Prefix.\u003C\u003EO.\u003C0\u003E__TargetMap ?? (Patch_Patch_JobDriver_HaulToCell_NoCellReservation_Prefix.\u003C\u003EO.\u003C0\u003E__TargetMap = new Func<Pawn, JobDriver, Map>(Patch_Patch_JobDriver_HaulToCell_NoCellReservation_Prefix.TargetMap))).Method).InstructionEnumeration();
  }

  private static Map TargetMap(Pawn pawn, JobDriver driver)
  {
    Job job = driver.job;
    return (job != null ? ((GlobalTargetInfo) ref job.globalTarget).Map : (Map) null) ?? TargetMapUtility.get_TargetMapOrThingMap((Thing) pawn);
  }
}
