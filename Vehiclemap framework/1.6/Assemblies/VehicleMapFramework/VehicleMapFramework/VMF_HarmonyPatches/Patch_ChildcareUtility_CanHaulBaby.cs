// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ChildcareUtility_CanHaulBaby
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Cautious)]
public static class Patch_ChildcareUtility_CanHaulBaby
{
  private static IEnumerable<MethodBase> TargetMethods()
  {
    yield return (MethodBase) AccessTools.Method(typeof (ChildcareUtility), "CanHaulBaby", (Type[]) null, (Type[]) null);
    yield return (MethodBase) AccessTools.Method(typeof (ChildcareUtility), "CanHaulToMom", (Type[]) null, (Type[]) null);
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer((MethodInfoCache.CachedMethodInfo.g_Thing_Map, MethodInfoCache.CachedMethodInfo.m_BaseMapOrCaravan_Thing), (MethodInfoCache.CachedMethodInfo.g_Thing_MapHeld, MethodInfoCache.CachedMethodInfo.m_MapHeldBaseMapOrCaravan));
  }
}
