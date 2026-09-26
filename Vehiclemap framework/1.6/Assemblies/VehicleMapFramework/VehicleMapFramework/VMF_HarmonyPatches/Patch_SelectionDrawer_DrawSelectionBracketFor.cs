// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_SelectionDrawer_DrawSelectionBracketFor
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VehicleMapFramework.LatePatches")]
[HarmonyPatch(typeof (SelectionDrawer), "DrawSelectionBracketFor")]
[HarmonyAfter(new string[] {"owlchemist.smartfarming", "Helixien.ReGrowthCore"})]
[PatchLevel(Level.Sensitive)]
public static class Patch_SelectionDrawer_DrawSelectionBracketFor
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    int index1 = list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Stloc_S && ((LocalVariableInfo) c.operand).LocalIndex == 9));
    LocalBuilder localBuilder = generator.DeclareLocal(typeof (VehiclePawnWithMap));
    Label label1 = generator.DefineLabel();
    list[index1].labels.Add(label1);
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index1, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[8]
    {
      CodeInstruction.LoadLocal(2, false),
      new CodeInstruction(OpCodes.Ldloca_S, (object) localBuilder),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_IsOnNonFocusedVehicleMapOf),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label1),
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_FullAngle),
      new CodeInstruction(OpCodes.Conv_I4, (object) null),
      new CodeInstruction(OpCodes.Add, (object) null)
    }));
    int index2 = list.FindIndex(index1, (Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Stloc_S && ((LocalVariableInfo) c.operand).LocalIndex == 18));
    Label label2 = generator.DefineLabel();
    list[index2].labels.Add(label2);
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index2, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[8]
    {
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label2),
      CodeInstruction.LoadLocal(2, false),
      PatchHelper.get_CallvirtInstruction(MethodInfoCache.CachedMethodInfo.g_Thing_DrawPos),
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_FullAngle),
      new CodeInstruction(OpCodes.Neg, (object) null),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_RotatePoint)
    }));
    MethodInfo m_DrawFieldEdges = ModCompat.CompatBase<ModCompat.SmartFarming>.Active ? AccessTools.Method(ModCompat.SmartFarming.MapComponent_SmartFarming, "DrawFieldEdges", (Type[]) null, (Type[]) null) : MethodInfoCache.CachedMethodInfo.m_GenDraw_DrawFieldEdges1;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo methodInfo = ModCompat.SmartFarming.SmartFarmingActive ? (Patch_SelectionDrawer_DrawSelectionBracketFor.\u003C\u003EO.\u003C0\u003E__DrawFieldEdgesSF ?? (Patch_SelectionDrawer_DrawSelectionBracketFor.\u003C\u003EO.\u003C0\u003E__DrawFieldEdgesSF = new Action<List<IntVec3>, Zone, Map>(GenDrawOnVehicle.DrawFieldEdgesSF))).Method : (ModCompat.SmartFarming.ReGrowthActive ? (Patch_SelectionDrawer_DrawSelectionBracketFor.\u003C\u003EO.\u003C1\u003E__DrawFieldEdgesRG ?? (Patch_SelectionDrawer_DrawSelectionBracketFor.\u003C\u003EO.\u003C1\u003E__DrawFieldEdgesRG = new Action<List<IntVec3>, int, Zone, Map>(GenDrawOnVehicle.DrawFieldEdgesRG))).Method : MethodInfoCache.CachedMethodInfo.m_GenDrawOnVehicle_DrawFieldEdges1);
    int index3 = list.FindIndex((Predicate<CodeInstruction>) (c => CodeInstructionExtensions.Calls(c, m_DrawFieldEdges)));
    list[index3].operand = (object) methodInfo;
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index3, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[2]
    {
      CodeInstruction.LoadLocal(0, false),
      PatchHelper.get_CallvirtInstruction(MethodInfoCache.CachedMethodInfo.g_Zone_Map)
    }));
    int index4 = list.FindIndex(index3 + 3, (Predicate<CodeInstruction>) (c => CodeInstructionExtensions.Calls(c, MethodInfoCache.CachedMethodInfo.m_GenDraw_DrawFieldEdges1)));
    list[index4].operand = (object) MethodInfoCache.CachedMethodInfo.m_GenDrawOnVehicle_DrawFieldEdges1;
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index4, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[2]
    {
      CodeInstruction.LoadLocal(1, false),
      PatchHelper.get_CallvirtInstruction(AccessTools.PropertyGetter(typeof (Plan), "Map"))
    }));
    return (IEnumerable<CodeInstruction>) list;
  }
}
