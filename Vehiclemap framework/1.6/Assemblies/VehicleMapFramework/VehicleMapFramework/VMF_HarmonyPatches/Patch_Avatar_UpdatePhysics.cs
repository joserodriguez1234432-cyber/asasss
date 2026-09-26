// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Avatar_UpdatePhysics
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_PerspectiveShift")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_Avatar_UpdatePhysics
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Find_CurrentMap)
    }).InsertAndAdvance(new CodeInstruction[1]
    {
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_BaseMapOrCaravan_Map)
    }).InsertAfter(new CodeInstruction[1]
    {
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_BaseMapOrCaravan_Map)
    }).InstructionEnumeration();
  }
}
