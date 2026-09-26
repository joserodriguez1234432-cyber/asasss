// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_VehicleTurret_RotationAligned
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Vehicles;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_VehicleTurret_RotationAligned
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    FieldInfo f_rotationTargeted = AccessTools.Field(typeof (VehicleTurret), "rotationTargeted");
    MethodInfo g_RotationTargeted = AccessTools.PropertyGetter(typeof (VehicleTurret), "TurretRotationTargeted");
    return Transpilers.Manipulator(instructions, (Func<CodeInstruction, bool>) (c => CodeInstructionExtensions.OperandIs(c, (MemberInfo) f_rotationTargeted)), (Action<CodeInstruction>) (c =>
    {
      c.opcode = OpCodes.Callvirt;
      c.operand = (object) g_RotationTargeted;
    }));
  }
}
