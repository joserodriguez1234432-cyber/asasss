// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ActionController_UpdateClosestCells
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_MeleeAnimation")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_ActionController_UpdateClosestCells
{
  private static IEnumerable<MethodBase> TargetMethods()
  {
    return (IEnumerable<MethodBase>) AccessToolsExtensions.GetDeclaredMethods(AccessTools.TypeByName("AM.Controller.ActionController")).Where<MethodInfo>((Func<MethodInfo, bool>) (m => m.Name == "UpdateClosestCells"));
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    int index1 = list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Callvirt && CodeInstructionExtensions.OperandIs(c, (MemberInfo) MethodInfoCache.CachedMethodInfo.g_Thing_Position)));
    int index2 = list.FindIndex(index1 + 1, (Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Callvirt && CodeInstructionExtensions.OperandIs(c, (MemberInfo) MethodInfoCache.CachedMethodInfo.g_Thing_Position)));
    list[index2].opcode = OpCodes.Call;
    list[index2].operand = (object) MethodInfoCache.CachedMethodInfo.m_PositionOnAnotherThingMap;
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index2, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[2]
    {
      CodeInstruction.LoadArgument(1, false),
      new CodeInstruction(OpCodes.Ldfld, (object) AccessTools.Field("AM.Controller.Requests.GrappleAttemptRequest:Grappler"))
    }));
    return (IEnumerable<CodeInstruction>) list;
  }
}
