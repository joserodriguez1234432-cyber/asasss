// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_SupportedDoor_DrawAt
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Building_SupportedDoor), "DrawAt")]
[PatchLevel(Level.Sensitive)]
public static class Patch_Building_SupportedDoor_DrawAt
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    FieldInfo f_Vector3_y = AccessTools.Field(typeof (Vector3), "y");
    int num = 0;
    foreach (CodeInstruction instruction in instructions)
    {
      if (CodeInstructionExtensions.StoresField(instruction, f_Vector3_y))
      {
        Label label = generator.DefineLabel();
        LocalBuilder vehicle = generator.DeclareLocal(typeof (VehiclePawnWithMap));
        yield return CodeInstruction.LoadArgument(0, false);
        yield return new CodeInstruction(OpCodes.Ldloca_S, (object) vehicle);
        yield return PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_IsOnNonFocusedVehicleMapOf);
        yield return new CodeInstruction(OpCodes.Brfalse_S, (object) label);
        yield return new CodeInstruction(OpCodes.Ldloc_S, (object) vehicle);
        yield return PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_YOffsetFull);
        yield return CodeInstructionExtensions.WithLabels(instruction, new Label[1]
        {
          label
        });
        vehicle = (LocalBuilder) null;
      }
      else if (CodeInstructionExtensions.Calls(instruction, MethodInfoCache.CachedMethodInfo.g_Thing_Rotation) && num < 2)
      {
        ++num;
        yield return PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_BaseRotationVehicleDraw);
      }
      else
        yield return instruction;
    }
  }
}
