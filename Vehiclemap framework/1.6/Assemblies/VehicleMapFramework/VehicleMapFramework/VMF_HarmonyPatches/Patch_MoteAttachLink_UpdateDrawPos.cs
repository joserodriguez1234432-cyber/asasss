// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_MoteAttachLink_UpdateDrawPos
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (MoteAttachLink), "UpdateDrawPos")]
[PatchLevel(Level.Sensitive)]
public static class Patch_MoteAttachLink_UpdateDrawPos
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    int index = list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Call && CodeInstructionExtensions.OperandIs(c, (MemberInfo) MethodInfoCache.CachedMethodInfo.m_IntVec3_ToVector3Shifted))) + 1;
    LocalBuilder localBuilder = generator.DeclareLocal(typeof (VehiclePawnWithMap));
    Label label = generator.DefineLabel();
    list[index].labels.Add(label);
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[8]
    {
      CodeInstruction.LoadArgument(0, false),
      CodeInstruction.LoadField(typeof (MoteAttachLink), "targetInt", true),
      new CodeInstruction(OpCodes.Call, (object) AccessTools.PropertyGetter(typeof (TargetInfo), "Map")),
      new CodeInstruction(OpCodes.Ldloca, (object) localBuilder),
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_IsNonFocusedVehicleMapOf),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label),
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder),
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_ToBaseMapCoord2)
    }));
    return (IEnumerable<CodeInstruction>) list;
  }
}
