// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Comp_DrakkenLaserDrill_Attack_DoSomething_II_Delegate
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_DrakkenLaserDrill")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_Comp_DrakkenLaserDrill_Attack_DoSomething_II_Delegate
{
  private static IEnumerable<MethodBase> TargetMethods()
  {
    return (IEnumerable<MethodBase>) AccessTools.InnerTypes(GenTypes.GetTypeInAnyAssembly("MYDE_DrakkenLaserDrill.Comp_DrakkenLaserDrill_Attack", "MYDE_DrakkenLaserDrill")).SelectMany<Type, MethodInfo>((Func<Type, IEnumerable<MethodInfo>>) (t => (IEnumerable<MethodInfo>) AccessToolsExtensions.GetDeclaredMethods(t))).Where<MethodInfo>((Func<MethodInfo, bool>) (m => m.Name.Contains("<DoSomething_II>")));
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return UnitTestDetector.IsTestingContext ? instructions : (IEnumerable<CodeInstruction>) instructions.MethodReplacer((MethodInfoCache.CachedMethodInfo.g_LocalTargetInfo_Cell, MethodInfoCache.CachedMethodInfo.m_CellOnBaseMapSpawned), (MethodInfoCache.CachedMethodInfo.g_TargetInfo_Cell, MethodInfoCache.CachedMethodInfo.m_CellOnBaseMapSpawned_TargetInfo), (MethodInfoCache.CachedMethodInfo.g_Thing_Position, MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMapSpawned));
  }
}
