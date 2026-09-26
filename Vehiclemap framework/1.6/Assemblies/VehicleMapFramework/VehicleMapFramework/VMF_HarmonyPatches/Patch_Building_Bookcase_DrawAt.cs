// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_Bookcase_DrawAt
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Building_Bookcase), "DrawAt")]
[PatchLevel(Level.Sensitive)]
public static class Patch_Building_Bookcase_DrawAt
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    instructions = (IEnumerable<CodeInstruction>) instructions.MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Thing_Rotation, MethodInfoCache.CachedMethodInfo.m_BaseRotationVehicleDraw);
    foreach (CodeInstruction instruction in instructions)
    {
      if (instruction.opcode == OpCodes.Stloc_2 || instruction.opcode == OpCodes.Stloc_3 || instruction.opcode == OpCodes.Stloc_S && ((LocalVariableInfo) instruction.operand).LocalIndex == 4)
      {
        Label label = generator.DefineLabel();
        LocalBuilder vehicle = generator.DeclareLocal(typeof (VehiclePawnWithMap));
        yield return CodeInstruction.LoadArgument(0, false);
        yield return new CodeInstruction(OpCodes.Ldloca_S, (object) vehicle);
        yield return new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_IsOnNonFocusedVehicleMapOf);
        yield return new CodeInstruction(OpCodes.Brfalse_S, (object) label);
        yield return new CodeInstruction(OpCodes.Ldloc_S, (object) vehicle);
        yield return new CodeInstruction(OpCodes.Callvirt, (object) MethodInfoCache.CachedMethodInfo.g_Angle);
        yield return new CodeInstruction(OpCodes.Neg, (object) null);
        yield return PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_RotatedBy);
        yield return CodeInstructionExtensions.WithLabels(instruction, new Label[1]
        {
          label
        });
        vehicle = (LocalBuilder) null;
      }
      else
        yield return instruction;
    }
  }
}
