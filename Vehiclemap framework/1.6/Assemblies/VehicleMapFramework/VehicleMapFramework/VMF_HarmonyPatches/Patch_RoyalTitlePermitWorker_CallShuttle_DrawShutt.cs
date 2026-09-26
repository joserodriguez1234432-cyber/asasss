// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_RoyalTitlePermitWorker_CallShuttle_DrawShuttleGhost
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

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (RoyalTitlePermitWorker_CallShuttle), "DrawShuttleGhost")]
[PatchLevel(Level.Sensitive)]
public static class Patch_RoyalTitlePermitWorker_CallShuttle_DrawShuttleGhost
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    list.Insert(list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Call && CodeInstructionExtensions.OperandIs(c, (MemberInfo) MethodInfoCache.CachedMethodInfo.g_Quaternion_identity))), PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_FocusedDrawPosOffset));
    return (IEnumerable<CodeInstruction>) list;
  }
}
