// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Rendering_DrawSelectionBracketsVehicles
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using SmashTools.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch]
[PatchLevel(Level.Mandatory)]
public static class Patch_Rendering_DrawSelectionBracketsVehicles
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    LocalBuilder localBuilder1;
    Label label1;
    CodeMatcher label2 = new CodeMatcher(instructions, generator).MatchEndForward(new CodeMatch[2]
    {
      CodeMatch.LoadsField(AccessTools.Field(typeof (Transform), "rotation"), false),
      new CodeMatch(new OpCode?(OpCodes.Add), (object) null, (string) null)
    }).DeclareLocal(typeof (VehiclePawnWithMap), ref localBuilder1).CreateLabel(ref label1);
    LocalBuilder localBuilder2 = label2.Instructions().Select<CodeInstruction, object>((Func<CodeInstruction, object>) (i => i.operand)).OfType<LocalBuilder>().FirstOrDefault<LocalBuilder>((Func<LocalBuilder, bool>) (l => l.LocalType == typeof (VehiclePawn)));
    int localIndex = localBuilder2 != null ? localBuilder2.LocalIndex : 0;
    Label label3;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return label2.InsertAndAdvance(new CodeInstruction[6]
    {
      CodeInstruction.LoadLocal(localIndex, false),
      new CodeInstruction(OpCodes.Ldloca_S, (object) localBuilder1),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_IsOnNonFocusedVehicleMapOf),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label1),
      CodeInstruction.LoadLocal(localIndex, false),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_FlipAngle)
    }).CreateLabelWithOffsets(1, ref label3).InsertAfter(new CodeInstruction[5]
    {
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder1),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label3),
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder1),
      PatchHelper.get_CallvirtInstruction(MethodInfoCache.CachedMethodInfo.g_Angle),
      new CodeInstruction(OpCodes.Add, (object) null)
    }).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(AccessTools.PropertyGetter(typeof (Thing), "RotatedSize"))
    }).Set(OpCodes.Call, (object) (Patch_Rendering_DrawSelectionBracketsVehicles.\u003C\u003EO.\u003C0\u003E__BaseRotatedSize ?? (Patch_Rendering_DrawSelectionBracketsVehicles.\u003C\u003EO.\u003C0\u003E__BaseRotatedSize = new Func<Thing, IntVec2>(VehicleMapUtility.BaseRotatedSize))).Method).InstructionEnumeration();
  }
}
