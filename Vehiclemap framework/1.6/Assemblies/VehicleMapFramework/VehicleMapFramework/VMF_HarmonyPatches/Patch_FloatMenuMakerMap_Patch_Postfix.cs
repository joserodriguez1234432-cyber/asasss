// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_FloatMenuMakerMap_Patch_Postfix
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_RimWorldOfMagic")]
[HarmonyPatch]
[PatchLevel(Level.Cautious)]
public static class Patch_FloatMenuMakerMap_Patch_Postfix
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    CodeMatcher codeMatcher = new CodeMatcher(instructions, (ILGenerator) null);
    codeMatcher.MatchStartForward(new CodeMatch[2]
    {
      CodeMatch.IsLdarg(new int?(2)),
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Map)
    });
    codeMatcher.Repeat((Action<CodeMatcher>) (c => c.Opcode = OpCodes.Ldarg_0), (Action<string>) null);
    return (IEnumerable<CodeInstruction>) codeMatcher.Instructions();
  }
}
