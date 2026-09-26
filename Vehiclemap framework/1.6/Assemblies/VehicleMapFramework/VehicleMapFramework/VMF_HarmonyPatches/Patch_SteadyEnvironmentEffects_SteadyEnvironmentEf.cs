// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_SteadyEnvironmentEffects_SteadyEnvironmentEffectsTick
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (SteadyEnvironmentEffects), "SteadyEnvironmentEffectsTick")]
[PatchLevel(Level.Sensitive)]
public static class Patch_SteadyEnvironmentEffects_SteadyEnvironmentEffectsTick
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo m_CeilToInt = (Patch_SteadyEnvironmentEffects_SteadyEnvironmentEffectsTick.\u003C\u003EO.\u003C0\u003E__CeilToInt ?? (Patch_SteadyEnvironmentEffects_SteadyEnvironmentEffectsTick.\u003C\u003EO.\u003C0\u003E__CeilToInt = new Func<float, int>(Mathf.CeilToInt))).Method;
    int index = list.FindIndex((Predicate<CodeInstruction>) (c => CodeInstructionExtensions.Calls(c, m_CeilToInt)));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    list[index].operand = (object) (Patch_SteadyEnvironmentEffects_SteadyEnvironmentEffectsTick.\u003C\u003EO.\u003C1\u003E__ChanceToInt ?? (Patch_SteadyEnvironmentEffects_SteadyEnvironmentEffectsTick.\u003C\u003EO.\u003C1\u003E__ChanceToInt = new Func<float, Map, int>(Patch_SteadyEnvironmentEffects_SteadyEnvironmentEffectsTick.ChanceToInt))).Method;
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[2]
    {
      CodeInstruction.LoadArgument(0, false),
      CodeInstruction.LoadField(typeof (SteadyEnvironmentEffects), "map", false)
    }));
    return (IEnumerable<CodeInstruction>) list;
  }

  public static int ChanceToInt(float chance, Map map)
  {
    if (!map.IsVehicleMapOf(out VehiclePawnWithMap _))
      return Mathf.CeilToInt(chance);
    int num = Mathf.FloorToInt(chance);
    chance -= (float) num;
    if (Rand.Chance(chance))
      ++num;
    return num;
  }
}
