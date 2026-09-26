// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_WorkGiver_ConstructDeliverResources_ResourceDeliverJobFor_Delegate
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
public static class Patch_WorkGiver_ConstructDeliverResources_ResourceDeliverJobFor_Delegate
{
  private static MethodBase TargetMethod()
  {
    Type[] fields = new Type[1]{ typeof (Thing) };
    Type[] args = new Type[1]{ typeof (Thing) };
    return (MethodBase) AccessTools.FindIncludingInnerTypes<MethodInfo>(typeof (WorkGiver_ConstructDeliverResources), (Func<Type, MethodInfo>) (t => !AccessToolsExtensions.GetDeclaredFields(t).Select<FieldInfo, Type>((Func<FieldInfo, Type>) (f => f.FieldType)).SequenceEqual<Type>((IEnumerable<Type>) fields) ? (MethodInfo) null : GenCollection.FirstOrDefault<MethodInfo>(AccessToolsExtensions.GetDeclaredMethods(t), (Predicate<MethodInfo>) (m => ((IEnumerable<ParameterInfo>) m.GetParameters()).Select<ParameterInfo, Type>((Func<ParameterInfo, Type>) (p => p.ParameterType)).SequenceEqual<Type>((IEnumerable<Type>) args) && m.Name.Contains("<ResourceDeliverJobFor>")))));
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Thing_Position, MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMap);
  }
}
