// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_CompBuildingExtraRenderer_PostPrintOnto
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_ExosuitFramework")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_CompBuildingExtraRenderer_PostPrintOnto
{
  private static MethodBase TargetMethod()
  {
    return (MethodBase) AccessTools.FindIncludingInnerTypes<MethodInfo>(GenTypes.GetTypeInAnyAssembly("Exosuit.CompBuildingExtraRenderer", "Exosuit"), (Func<Type, MethodInfo>) (t => GenCollection.FirstOrDefault<MethodInfo>(AccessToolsExtensions.GetDeclaredMethods(t), (Predicate<MethodInfo>) (m => m.Name.Contains("<PostPrintOnto>")))));
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.LoadsConstant(0.0)
    }).Set(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_PrintExtraRotation).Insert(new CodeInstruction[1]
    {
      new CodeInstruction(OpCodes.Dup, (object) null)
    }).InstructionEnumeration();
  }
}
