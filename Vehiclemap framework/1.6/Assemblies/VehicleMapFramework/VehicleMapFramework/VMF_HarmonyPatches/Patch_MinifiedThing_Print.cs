// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_MinifiedThing_Print
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

[HarmonyPatch(typeof (MinifiedThing), "Print")]
[PatchLevel(Level.Sensitive)]
public static class Patch_MinifiedThing_Print
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    CodeMatcher codeMatcher = new CodeMatcher(instructions, (ILGenerator) null);
    MethodInfo method = Printer_Plane.PrintPlane.Method;
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(method)
    });
    codeMatcher.MatchStartBackwards(new CodeMatch[1]
    {
      new CodeMatch((Func<CodeInstruction, bool>) (c => c.opcode == OpCodes.Ldloc_1), (string) null)
    });
    codeMatcher.InsertAfterAndAdvance(new CodeInstruction[1]
    {
      CodeInstruction.LoadArgument(0, false)
    });
    codeMatcher.Advance(1);
    codeMatcher.SetInstruction(new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_PrintExtraRotation));
    codeMatcher.End();
    codeMatcher.MatchStartBackwards(new CodeMatch[1]
    {
      CodeMatch.Calls(method)
    });
    codeMatcher.MatchStartBackwards(new CodeMatch[1]
    {
      new CodeMatch((Func<CodeInstruction, bool>) (c => c.opcode == OpCodes.Ldloc_S && ((LocalVariableInfo) c.operand).LocalType == typeof (Material)), (string) null)
    });
    codeMatcher.InsertAfterAndAdvance(new CodeInstruction[1]
    {
      CodeInstruction.LoadArgument(0, false)
    });
    codeMatcher.Advance(1);
    codeMatcher.SetInstruction(new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_PrintExtraRotation));
    return (IEnumerable<CodeInstruction>) codeMatcher.Instructions();
  }
}
