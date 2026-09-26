// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_GUI_VehicleMapOffset
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_GUI_VehicleMapOffset
{
  private static IEnumerable<MethodBase> TargetMethods()
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    yield return (MethodBase) (Patch_GUI_VehicleMapOffset.\u003C\u003EO.\u003C0\u003E__RenderMouseoverBracket ?? (Patch_GUI_VehicleMapOffset.\u003C\u003EO.\u003C0\u003E__RenderMouseoverBracket = new Action(GenUI.RenderMouseoverBracket))).Method;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    yield return (MethodBase) (Patch_GUI_VehicleMapOffset.\u003C\u003EO.\u003C1\u003E__RenderHighlightOverSelectableCells ?? (Patch_GUI_VehicleMapOffset.\u003C\u003EO.\u003C1\u003E__RenderHighlightOverSelectableCells = new Action<Designator, List<IntVec3>>(DesignatorUtility.RenderHighlightOverSelectableCells))).Method;
    yield return (MethodBase) AccessTools.Method(typeof (Designator_Cancel), "RenderHighlight", (Type[]) null, (Type[]) null);
    yield return (MethodBase) AccessTools.Method(typeof (CellBoolDrawer), "ActuallyDraw", (Type[]) null, (Type[]) null);
  }

  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    CodeMatcher codeMatcher = new CodeMatcher(instructions, generator);
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Quaternion_identity)
    });
    codeMatcher.InsertAndAdvance(new CodeInstruction[1]
    {
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_ToBaseMapCoord1)
    });
    LocalBuilder localBuilder;
    codeMatcher.DeclareLocal(typeof (VehiclePawnWithMap), ref localBuilder);
    Label label;
    codeMatcher.CreateLabelWithOffsets(1, ref label);
    codeMatcher.InsertAfter(new CodeInstruction[6]
    {
      new CodeInstruction(OpCodes.Ldloca_S, (object) localBuilder),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_FocusedOnVehicleMap),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label),
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_FullAngleQuat),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.o_Quaternion_Multiply)
    });
    return (IEnumerable<CodeInstruction>) codeMatcher.Instructions();
  }
}
