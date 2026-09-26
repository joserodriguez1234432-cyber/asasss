// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Graphic_ABLink_Print
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_AsAboveSoBelow")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_Graphic_ABLink_Print
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(AccessTools.Method(typeof (Graphic), "DrawOffset", (Type[]) null, (Type[]) null))
    }).InsertAfter(new CodeInstruction[1]
    {
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_RotateForPrintNegate)
    }).InstructionEnumeration().MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Thing_Rotation, MethodInfoCache.CachedMethodInfo.m_RotationForPrint);
  }
}
