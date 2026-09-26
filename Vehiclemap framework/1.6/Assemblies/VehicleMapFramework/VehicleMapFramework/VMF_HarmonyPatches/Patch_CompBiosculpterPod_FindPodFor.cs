// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_CompBiosculpterPod_FindPodFor
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (CompBiosculpterPod), "FindPodFor")]
[PatchLevel(Level.Sensitive)]
public static class Patch_CompBiosculpterPod_FindPodFor
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[2]
    {
      new CodeMatch(new OpCode?(OpCodes.Ldarg_0), (object) null, (string) null),
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Map)
    }).SetOpcodeAndAdvance(OpCodes.Ldarg_1).Set(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.g_Thing_MapHeld).InstructionEnumeration();
  }
}
