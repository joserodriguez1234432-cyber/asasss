// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_WorkGiver_DoBill_TryFindBestIngredientsHelper
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (WorkGiver_DoBill), "TryFindBestIngredientsHelper")]
[PatchLevel(Level.Sensitive)]
public static class Patch_WorkGiver_DoBill_TryFindBestIngredientsHelper
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    int index = list.FindIndex((Predicate<CodeInstruction>) (c => CodeInstructionExtensions.Calls(c, MethodInfoCache.CachedMethodInfo.g_Thing_Map)));
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[2]
    {
      new CodeInstruction(OpCodes.Pop, (object) null),
      CodeInstruction.LoadArgument(4, false)
    }));
    return (IEnumerable<CodeInstruction>) list.MethodReplacer(MethodInfoCache.CachedMethodInfo.m_BreadthFirstTraverse, MethodInfoCache.CachedMethodInfo.m_BreadthFirstTraverseAcrossMaps);
  }
}
