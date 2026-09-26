// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_TryFindShootLineFromTo_Base_Patch_Prefix
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_RimWorldOfMagic")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_TryFindShootLineFromTo_Base_Patch_Prefix
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    CodeMatcher codeMatcher = new CodeMatcher(instructions, (ILGenerator) null);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo method = (Patch_TryFindShootLineFromTo_Base_Patch_Prefix.\u003C\u003EO.\u003C0\u003E__CanReachImmediate ?? (Patch_TryFindShootLineFromTo_Base_Patch_Prefix.\u003C\u003EO.\u003C0\u003E__CanReachImmediate = new Func<IntVec3, LocalTargetInfo, Map, PathEndMode, Pawn, bool>(ReachabilityImmediate.CanReachImmediate))).Method;
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(method)
    });
    codeMatcher.MatchStartBackwards(new CodeMatch[1]
    {
      CodeMatch.IsLdarg(new int?(1))
    });
    codeMatcher.InsertAfter(new CodeInstruction[3]
    {
      CodeInstruction.LoadArgument(0, false),
      CodeInstruction.LoadField(typeof (Verb), "caster", false),
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_ToThingMapCoord)
    });
    return (IEnumerable<CodeInstruction>) codeMatcher.Instructions().MethodReplacer(MethodInfoCache.CachedMethodInfo.g_LocalTargetInfo_Cell, MethodInfoCache.CachedMethodInfo.m_CellOnBaseMap);
  }
}
