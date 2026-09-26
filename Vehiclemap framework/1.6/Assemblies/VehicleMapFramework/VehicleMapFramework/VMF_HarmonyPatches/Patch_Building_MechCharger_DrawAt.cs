// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_MechCharger_DrawAt
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

[HarmonyPatch(typeof (Building_MechCharger), "DrawAt")]
[PatchLevel(Level.Sensitive)]
public static class Patch_Building_MechCharger_DrawAt
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    LocalBuilder localBuilder1 = generator.DeclareLocal(typeof (Rot4));
    FieldInfo f_rotation = AccessTools.Field(typeof (GenDraw.FillableBarRequest), "rotation");
    int index1 = list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Stfld && CodeInstructionExtensions.OperandIs(c, (MemberInfo) f_rotation)));
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index1, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[2]
    {
      new CodeInstruction(OpCodes.Stloc_S, (object) localBuilder1),
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder1)
    }));
    int index2 = list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Call && CodeInstructionExtensions.OperandIs(c, (MemberInfo) MethodInfoCache.CachedMethodInfo.m_IntVec3_ToVector3Shifted))) + 1;
    LocalBuilder localBuilder2 = generator.DeclareLocal(typeof (VehiclePawnWithMap));
    Label label = generator.DefineLabel();
    list[index2].labels.Add(label);
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index2, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[6]
    {
      CodeInstruction.LoadArgument(0, false),
      new CodeInstruction(OpCodes.Ldloca_S, (object) localBuilder2),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_IsOnNonFocusedVehicleMapOf),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label),
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder2),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_ToBaseMapCoord2)
    }));
    return (IEnumerable<CodeInstruction>) list.MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Thing_Rotation, MethodInfoCache.CachedMethodInfo.m_BaseFullRotation_Thing);
  }
}
