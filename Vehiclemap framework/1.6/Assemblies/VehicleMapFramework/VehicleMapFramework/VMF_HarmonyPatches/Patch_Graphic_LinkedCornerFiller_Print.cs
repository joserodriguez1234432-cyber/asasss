// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Graphic_LinkedCornerFiller_Print
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Graphic_LinkedCornerFiller), "Print")]
[PatchLevel(Level.Sensitive)]
public static class Patch_Graphic_LinkedCornerFiller_Print
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    FieldInfo f_Altitudes_AltIncVect = AccessTools.Field(typeof (Altitudes), "AltIncVect");
    int num1 = list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Ldsfld && CodeInstructionExtensions.OperandIs(c, (MemberInfo) f_Altitudes_AltIncVect))) - 1;
    list.Insert(num1, new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_RotateForPrintNegate));
    ConstructorInfo c_Vector3 = AccessTools.Constructor(typeof (Vector3), new Type[3]
    {
      typeof (float),
      typeof (float),
      typeof (float)
    }, false);
    int num2 = list.FindIndex(num1, (Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Newobj && CodeInstructionExtensions.OperandIs(c, (MemberInfo) c_Vector3))) + 1;
    list.Insert(num2, new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_RotateForPrintNegate));
    int index = list.FindIndex(num2, (Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Brtrue));
    object operand = list[index].operand;
    LocalBuilder localBuilder = generator.DeclareLocal(typeof (VehiclePawnWithMap));
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index + 1, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[4]
    {
      CodeInstruction.LoadArgument(2, false),
      new CodeInstruction(OpCodes.Ldloca, (object) localBuilder),
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_IsOnVehicleMapOf),
      new CodeInstruction(OpCodes.Brtrue, operand)
    }));
    return (IEnumerable<CodeInstruction>) list;
  }
}
