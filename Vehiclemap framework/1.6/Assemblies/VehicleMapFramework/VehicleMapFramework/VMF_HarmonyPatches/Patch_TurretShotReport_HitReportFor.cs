// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_TurretShotReport_HitReportFor
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (TurretShotReport), "HitReportFor")]
[PatchLevel(Level.Mandatory)]
public static class Patch_TurretShotReport_HitReportFor
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    MethodInfo methodInfo = AccessTools.PropertyGetter(typeof (LocalTargetInfo), "Thing");
    LocalBuilder localBuilder1;
    LocalBuilder localBuilder2;
    Label label1;
    Label label2;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return (IEnumerable<CodeInstruction>) new CodeMatcher(instructions, generator).Reset(true).DeclareLocal(typeof (Thing), ref localBuilder1).DeclareLocal(typeof (Map), ref localBuilder2).CreateLabel(ref label1).DefineLabel(ref label2).Insert(new CodeInstruction[11]
    {
      CodeInstruction.LoadArgument(2, true),
      new CodeInstruction(OpCodes.Call, (object) methodInfo),
      new CodeInstruction(OpCodes.Stloc_S, (object) localBuilder1),
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder1),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label1),
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder1),
      new CodeInstruction(OpCodes.Callvirt, (object) MethodInfoCache.CachedMethodInfo.g_Thing_Map),
      new CodeInstruction(OpCodes.Br_S, (object) label2),
      CodeInstructionExtensions.WithLabels(CodeInstruction.LoadArgument(0, false), new Label[1]
      {
        label1
      }),
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_BaseMap_Thing),
      CodeInstructionExtensions.WithLabels(new CodeInstruction(OpCodes.Stloc_S, (object) localBuilder2), new Label[1]
      {
        label2
      })
    }).MatchStartForward(new CodeMatch[2]
    {
      new CodeMatch(new OpCode?(OpCodes.Ldloc_0), (object) null, (string) null),
      CodeMatch.Calls((Patch_TurretShotReport_HitReportFor.\u003C\u003EO.\u003C0\u003E__CalculateCoverGiverSet ?? (Patch_TurretShotReport_HitReportFor.\u003C\u003EO.\u003C0\u003E__CalculateCoverGiverSet = new Func<LocalTargetInfo, IntVec3, Map, List<CoverInfo>>(CoverUtility.CalculateCoverGiverSet))).Method)
    }).Set(OpCodes.Ldloc_S, (object) localBuilder2).InstructionEnumeration().MethodReplacer((MethodInfoCache.CachedMethodInfo.g_Thing_Map, MethodInfoCache.CachedMethodInfo.m_BaseMap_Thing), (MethodInfoCache.CachedMethodInfo.g_LocalTargetInfo_Cell, MethodInfoCache.CachedMethodInfo.m_CellOnBaseMap), (MethodInfoCache.CachedMethodInfo.m_Roofed2, MethodInfoCache.CachedMethodInfo.m_RoofedAcrossMaps2));
  }
}
