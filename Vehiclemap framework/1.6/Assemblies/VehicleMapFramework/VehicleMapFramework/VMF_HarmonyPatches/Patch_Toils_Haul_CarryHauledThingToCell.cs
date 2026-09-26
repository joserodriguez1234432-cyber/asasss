// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Toils_Haul_CarryHauledThingToCell
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_Toils_Haul_CarryHauledThingToCell
{
  private static IEnumerable<MethodBase> TargetMethods()
  {
    return (IEnumerable<MethodBase>) AccessTools.InnerTypes(typeof (Toils_Haul)).Select<Type, List<MethodInfo>>((Func<Type, List<MethodInfo>>) (t => AccessToolsExtensions.GetDeclaredMethods(t))).Select<List<MethodInfo>, MethodInfo>((Func<List<MethodInfo>, MethodInfo>) (methods => GenCollection.FirstOrDefault<MethodInfo>(methods, (Predicate<MethodInfo>) (m =>
    {
      if (m.Name.Contains("DupeValidator"))
        return true;
      if (!m.Name.Contains("<CarryHauledThingToCell>"))
        return false;
      // ISSUE: object of a compiler-generated type is created
      return m.GetMethodBody().LocalVariables.Select<LocalVariableInfo, Type>((Func<LocalVariableInfo, Type>) (l => l.LocalType)).SequenceEqual<Type>((IEnumerable<Type>) new \u003C\u003Ez__ReadOnlyArray<Type>(new Type[4]
      {
        typeof (Pawn),
        typeof (IntVec3),
        typeof (CompPushable),
        typeof (LocalTargetInfo)
      }));
    })))).Where<MethodInfo>((Func<MethodInfo, bool>) (method => method != (MethodInfo) null));
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Thing_Map, MethodInfoCache.CachedMethodInfo.m_TargetMapOrPawnMap);
  }
}
