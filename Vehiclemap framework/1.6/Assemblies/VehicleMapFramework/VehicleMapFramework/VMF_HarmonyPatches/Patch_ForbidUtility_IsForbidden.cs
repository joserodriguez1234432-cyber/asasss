// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ForbidUtility_IsForbidden
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (ForbidUtility), "IsForbidden", new Type[] {typeof (Thing), typeof (Pawn)})]
[PatchLevel(Level.Sensitive)]
public static class Patch_ForbidUtility_IsForbidden
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.m_IsForbidden)
    }).InsertAndAdvance(new CodeInstruction[1]
    {
      CodeInstruction.LoadArgument(0, false)
    }).SetOperandAndAdvance((object) MethodInfoCache.CachedMethodInfo.m_CrossMapIsForbidden1).InstructionEnumeration();
  }
}
