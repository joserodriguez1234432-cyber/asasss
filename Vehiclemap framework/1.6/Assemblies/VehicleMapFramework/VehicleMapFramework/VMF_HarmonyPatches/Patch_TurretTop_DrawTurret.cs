// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_TurretTop_DrawTurret
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (TurretTop), "DrawTurret")]
[PatchLevel(Level.Sensitive)]
public static class Patch_TurretTop_DrawTurret
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    LocalBuilder localBuilder1;
    Label label1;
    Label label2;
    LocalBuilder localBuilder2;
    return new CodeMatcher(instructions, generator).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.m_RotatedBy)
    }).DeclareLocal(typeof (VehiclePawnWithMap), ref localBuilder1).CreateLabel(ref label1).InsertAndAdvance(new CodeInstruction[8]
    {
      CodeInstruction.LoadArgument(0, false),
      CodeInstruction.LoadField(typeof (TurretTop), "parentTurret", false),
      new CodeInstruction(OpCodes.Ldloca_S, (object) localBuilder1),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_IsOnNonFocusedVehicleMapOf),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label1),
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder1),
      PatchHelper.get_CallvirtInstruction(MethodInfoCache.CachedMethodInfo.g_Angle),
      new CodeInstruction(OpCodes.Sub, (object) null)
    }).MatchStartForward(new CodeMatch[1]
    {
      new CodeMatch((Func<CodeInstruction, bool>) (c => c.opcode == OpCodes.Stloc_S && ((LocalVariableInfo) c.operand).LocalType == typeof (Quaternion)), (string) null)
    }).CreateLabel(ref label2).DeclareLocal(typeof (LocalTargetInfo), ref localBuilder2).Insert(new CodeInstruction[12]
    {
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder1),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label2),
      CodeInstruction.LoadArgument(0, false),
      CodeInstruction.LoadField(typeof (TurretTop), "parentTurret", false),
      PatchHelper.get_CallvirtInstruction(AccessTools.PropertyGetter(typeof (Building_Turret), "CurrentTarget")),
      new CodeInstruction(OpCodes.Stloc_S, (object) localBuilder2),
      new CodeInstruction(OpCodes.Ldloca_S, (object) localBuilder2),
      PatchHelper.get_CallInstruction(AccessTools.PropertyGetter(typeof (LocalTargetInfo), "IsValid")),
      new CodeInstruction(OpCodes.Brtrue_S, (object) label2),
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder1),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_FullAngleQuat),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.o_Quaternion_Multiply)
    }).InstructionEnumeration();
  }
}
