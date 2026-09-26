// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patches_Designator_ZoneAdd_MakeNewZone
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

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patches_Designator_ZoneAdd_MakeNewZone
{
  private static IEnumerable<MethodBase> TargetMethods()
  {
    return ((IEnumerable<MethodBase>) GenTypes.AllSubclasses(typeof (Designator_ZoneAdd)).Select<Type, MethodInfo>((Func<Type, MethodInfo>) (type => AccessTools.DeclaredMethod(type, "MakeNewZone", (Type[]) null, (Type[]) null)))).WhereCallsMethod((MethodBase) MethodInfoCache.CachedMethodInfo.g_Find_CurrentMap);
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Find_CurrentMap, MethodInfoCache.CachedMethodInfo.g_VehicleMapUtility_CurrentMap);
  }
}
