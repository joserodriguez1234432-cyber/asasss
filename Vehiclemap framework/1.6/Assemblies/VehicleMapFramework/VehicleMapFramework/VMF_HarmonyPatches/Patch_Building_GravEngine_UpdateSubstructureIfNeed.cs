// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_GravEngine_UpdateSubstructureIfNeeded
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_Odyssey")]
[HarmonyPatch(typeof (Building_GravEngine), "UpdateSubstructureIfNeeded")]
[PatchLevel(Level.Sensitive)]
public static class Patch_Building_GravEngine_UpdateSubstructureIfNeeded
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    CodeMatcher codes = new CodeMatcher(instructions, generator);
    ReplaceType(typeof (SectionLayer_GravshipHull), typeof (SectionLayer_GravshipHullOnVehicle));
    ReplaceType(typeof (SectionLayer_SubstructureProps), typeof (SectionLayer_SubstructurePropsOnVehicle));
    return (IEnumerable<CodeInstruction>) codes.Instructions();

    void ReplaceType(Type type, Type type2)
    {
      codes.MatchStartForward(new CodeMatch[1]
      {
        new CodeMatch(new OpCode?(OpCodes.Ldtoken), (object) type, (string) null)
      });
      Label label;
      codes.CreateLabelWithOffsets(1, ref label);
      LocalBuilder localBuilder;
      codes.DeclareLocal(typeof (VehiclePawnWithMap), ref localBuilder);
      codes.InsertAfterAndAdvance(new CodeInstruction[7]
      {
        CodeInstruction.LoadArgument(0, false),
        new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.g_Thing_Map),
        new CodeInstruction(OpCodes.Ldloca_S, (object) localBuilder),
        new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_IsVehicleMapOf),
        new CodeInstruction(OpCodes.Brfalse_S, (object) label),
        new CodeInstruction(OpCodes.Pop, (object) null),
        new CodeInstruction(OpCodes.Ldtoken, (object) type2)
      });
    }
  }
}
