// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_Door_DrawMovers
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Building_Door), "DrawMovers")]
public static class Patch_Building_Door_DrawMovers
{
  [PatchLevel(Level.Safe)]
  public static void Prefix(ref float altitude, Building_Door __instance)
  {
    VehiclePawnWithMap vehicle;
    if (!((Thing) __instance).IsOnNonFocusedVehicleMapOf(out vehicle))
      return;
    altitude = altitude.YOffsetFull(vehicle);
  }

  [PatchLevel(Level.Sensitive)]
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    CodeMatch codeMatch = CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Rot4_AsQuat);
    LocalBuilder vehicle;
    return (IEnumerable<CodeInstruction>) PatchHelper.AddExtraAngle(new CodeMatcher(instructions, generator).MatchStartForward(new CodeMatch[1]
    {
      codeMatch
    }).Advance(1), out vehicle).MatchStartForward(new CodeMatch[1]
    {
      codeMatch
    }).Advance(1).AddExtraAngle(vehicle).InstructionEnumeration().MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Thing_Rotation, MethodInfoCache.CachedMethodInfo.m_BaseRotationVehicleDraw);
  }
}
