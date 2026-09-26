// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_DesignationDragger_DraggerOnGUI
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

[HarmonyBefore(new string[] {"com.cheatereater.designationstooltip"})]
[HarmonyPatch(typeof (DesignationDragger), "DraggerOnGUI")]
[PatchLevel(Level.Mandatory)]
public static class Patch_DesignationDragger_DraggerOnGUI
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator,
    MethodBase original)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    ConstructorInfo c_Vector3 = AccessTools.Constructor(typeof (Vector3), new Type[3]
    {
      typeof (float),
      typeof (float),
      typeof (float)
    }, false);
    int num1 = list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Call && CodeInstructionExtensions.OperandIs(c, (MemberInfo) c_Vector3))) + 1;
    int localIndex = original.GetMethodBody().LocalVariables.First<LocalVariableInfo>((Func<LocalVariableInfo, bool>) (l => l.LocalType == typeof (Vector3))).LocalIndex;
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(num1, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[5]
    {
      CodeInstruction.LoadLocal(localIndex, false),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_ToBaseMapCoord1),
      new CodeInstruction(OpCodes.Ldc_R4, (object) 0.0f),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_Vector3Utility_WithY),
      CodeInstruction.StoreLocal(localIndex)
    }));
    int num2 = list.FindIndex(num1, (Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Newobj && CodeInstructionExtensions.OperandIs(c, (MemberInfo) c_Vector3))) + 1;
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(num2, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[3]
    {
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_ToBaseMapCoord1),
      new CodeInstruction(OpCodes.Ldc_R4, (object) 0.0f),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_Vector3Utility_WithY)
    }));
    MethodInfo m_Widgets_DrawBox = Widgets.DrawBox.Method;
    int index1 = list.FindIndex(num2, (Predicate<CodeInstruction>) (c => CodeInstructionExtensions.Calls(c, m_Widgets_DrawBox)));
    MethodInfo method1 = VMF_Widgets.DrawBoxRotated.Method;
    Label label1 = generator.DefineLabel();
    Label label2 = generator.DefineLabel();
    list[index1].operand = (object) method1;
    list[index1].labels.Add(label2);
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index1, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[6]
    {
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.g_FocusedVehicle),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label1),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.g_FocusedVehicle),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_ExtraAngle),
      new CodeInstruction(OpCodes.Br_S, (object) label2),
      CodeInstructionExtensions.WithLabels(new CodeInstruction(OpCodes.Ldc_R4, (object) 0.0f), new Label[1]
      {
        label1
      })
    }));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo m_Widgets_DrawNumberOnMap = (Patch_DesignationDragger_DraggerOnGUI.\u003C\u003EO.\u003C2\u003E__DrawNumberOnMap ?? (Patch_DesignationDragger_DraggerOnGUI.\u003C\u003EO.\u003C2\u003E__DrawNumberOnMap = new Action<Vector2, int, Color>(Widgets.DrawNumberOnMap))).Method;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo method2 = (Patch_DesignationDragger_DraggerOnGUI.\u003C\u003EO.\u003C3\u003E__ConvertToVehicleMap ?? (Patch_DesignationDragger_DraggerOnGUI.\u003C\u003EO.\u003C3\u003E__ConvertToVehicleMap = new Func<Vector2, Vector2>(Patch_DesignationDragger_DraggerOnGUI.ConvertToVehicleMap))).Method;
    int index2 = list.FindIndex(index1, (Predicate<CodeInstruction>) (c => CodeInstructionExtensions.Calls(c, m_Widgets_DrawNumberOnMap))) - 3;
    list.Insert(index2, PatchHelper.get_CallInstruction(method2));
    int index3 = list.FindIndex(index2 + 5, (Predicate<CodeInstruction>) (c => CodeInstructionExtensions.Calls(c, m_Widgets_DrawNumberOnMap))) - 3;
    list.Insert(index3, PatchHelper.get_CallInstruction(method2));
    int index4 = list.FindIndex(index3 + 5, (Predicate<CodeInstruction>) (c => CodeInstructionExtensions.Calls(c, m_Widgets_DrawNumberOnMap)));
    int lastIndex = list.FindLastIndex(index4, (Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Ldarg_0));
    list.Insert(lastIndex, PatchHelper.get_CallInstruction(method2));
    return (IEnumerable<CodeInstruction>) list;
  }

  private static Vector2 ConvertToVehicleMap(Vector2 screenPos)
  {
    screenPos.y = (float) UI.screenHeight - screenPos.y;
    return UI.MapToUIPosition(Vector3Utility.Yto0(UI.UIToMapPosition(screenPos).ToBaseMapCoord()));
  }
}
