// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Projectile_CanHit
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

[HarmonyPatch(typeof (Projectile), "CanHit")]
[PatchLevel(Level.Sensitive)]
public static class Patch_Projectile_CanHit
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Map)
    }).Set(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_BaseMap_Thing).MatchStartForward(new CodeMatch[3]
    {
      new CodeMatch(new OpCode?(OpCodes.Ldarg_0), (object) null, (string) null),
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Map),
      CodeMatch.Calls((Patch_Projectile_CanHit.\u003C\u003EO.\u003C0\u003E__ThingCovered ?? (Patch_Projectile_CanHit.\u003C\u003EO.\u003C0\u003E__ThingCovered = new Func<Thing, Map, bool>(CoverUtility.ThingCovered))).Method)
    }).SetOpcodeAndAdvance(OpCodes.Ldarg_1).SetOpcodeAndAdvance(OpCodes.Callvirt).InstructionEnumeration();
  }
}
