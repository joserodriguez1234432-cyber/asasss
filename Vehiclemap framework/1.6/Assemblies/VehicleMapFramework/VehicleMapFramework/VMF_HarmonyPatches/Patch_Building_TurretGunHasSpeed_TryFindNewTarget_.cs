// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_TurretGunHasSpeed_TryFindNewTarget_Delegate
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

[HarmonyPatchCategory("VMF_Patches_SRALib")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_Building_TurretGunHasSpeed_TryFindNewTarget_Delegate
{
  private static IEnumerable<MethodBase> TargetMethods()
  {
    return (IEnumerable<MethodBase>) LinqUtility.get_NonNull<MethodInfo>(ModCompat.SRALib.Building_TurretGunHasSpeed.Select<Type, MethodInfo>((Func<Type, MethodInfo>) (t => AccessTools.FindIncludingInnerTypes<MethodInfo>(t, (Func<Type, MethodInfo>) (t2 => GenCollection.FirstOrDefault<MethodInfo>(AccessToolsExtensions.GetDeclaredMethods(t2), (Predicate<MethodInfo>) (m => m.Name.Contains("<TryFindNewTarget>"))))))));
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Thing_Position, MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMap);
  }
}
