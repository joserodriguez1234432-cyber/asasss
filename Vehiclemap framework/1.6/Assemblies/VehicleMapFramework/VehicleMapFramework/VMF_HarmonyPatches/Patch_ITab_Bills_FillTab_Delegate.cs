// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ITab_Bills_FillTab_Delegate
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
[HarmonyAfter(new string[] {"GestaltEngine.Mod"})]
public static class Patch_ITab_Bills_FillTab_Delegate
{
  private static MethodBase TargetMethod()
  {
    return (MethodBase) AccessTools.FindIncludingInnerTypes<MethodInfo>(typeof (ITab_Bills), (Func<Type, MethodInfo>) (t => GenCollection.FirstOrDefault<MethodInfo>(AccessToolsExtensions.GetDeclaredMethods(t), (Predicate<MethodInfo>) (m => m.Name.Contains("<FillTab>") && PatchHelper.ReadMethodBodyWrapper((MethodBase) m).Any<KeyValuePair<OpCode, object>>((Func<KeyValuePair<OpCode, object>, bool>) (i => MethodInfoCache.CachedMethodInfo.g_Thing_Map.Equals(i.Value)))))));
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Thing_Map, MethodInfoCache.CachedMethodInfo.m_BaseMap_Thing);
  }
}
