// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_PlaceWorker_GravshipThruster_DrawGhost
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_Odyssey")]
[HarmonyPatch(typeof (PlaceWorker_GravshipThruster), "DrawGhost")]
[PatchLevel(Level.Sensitive)]
public static class Patch_PlaceWorker_GravshipThruster_DrawGhost
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    CodeMatcher codeMatcher = new CodeMatcher(instructions, generator);
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.m_GenDraw_DrawFieldEdges1)
    });
    Label label1;
    codeMatcher.CreateLabel(ref label1);
    Label label2;
    codeMatcher.DefineLabel(ref label2);
    codeMatcher.InsertAndAdvance(new CodeInstruction[6]
    {
      CodeInstruction.LoadArgument(5, false),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label2),
      CodeInstruction.LoadArgument(5, false),
      new CodeInstruction(OpCodes.Callvirt, (object) MethodInfoCache.CachedMethodInfo.g_Thing_Map),
      new CodeInstruction(OpCodes.Br_S, (object) label1),
      CodeInstructionExtensions.WithLabels(new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.g_VehicleMapUtility_CurrentMap), new Label[1]
      {
        label2
      })
    });
    codeMatcher.Operand = (object) MethodInfoCache.CachedMethodInfo.m_GenDrawOnVehicle_DrawFieldEdges1;
    return (IEnumerable<CodeInstruction>) codeMatcher.Instructions();
  }
}
