// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patches_DebugTools
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[StaticConstructorOnStartupPriority(200)]
public static class Patches_DebugTools
{
  static Patches_DebugTools()
  {
    if (!VehicleMapFramework.VehicleMapFramework.settings.debugToolPatches)
      return;
    Patches_DebugTools.ApplyPatches();
  }

  public static void ApplyPatches(bool unpatch = false)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo transpiler = (Patches_DebugTools.\u003C\u003EO.\u003C0\u003E__Transpiler ?? (Patches_DebugTools.\u003C\u003EO.\u003C0\u003E__Transpiler = new Func<IEnumerable<CodeInstruction>, IEnumerable<CodeInstruction>>(Patches_DebugTools.Transpiler))).Method;
    Patch(AccessTools.FindIncludingInnerTypes<MethodInfo>(typeof (DebugActionNode), (Func<Type, MethodInfo>) (t => GenCollection.FirstOrDefault<MethodInfo>(AccessToolsExtensions.GetDeclaredMethods(t), (Predicate<MethodInfo>) (m => m.Name.Contains("<Enter>"))))));
    foreach (MethodInfo declaredMethod in AccessToolsExtensions.GetDeclaredMethods(typeof (DebugToolsSpawning)))
      Patch(declaredMethod);
    foreach (Type innerType in AccessTools.InnerTypes(typeof (DebugToolsSpawning)))
    {
      foreach (MethodInfo declaredMethod in AccessToolsExtensions.GetDeclaredMethods(innerType))
        Patch(declaredMethod);
    }
    foreach (MethodInfo declaredMethod in AccessToolsExtensions.GetDeclaredMethods(typeof (DebugToolsGeneral)))
      Patch(declaredMethod);
    foreach (MethodInfo declaredMethod in AccessToolsExtensions.GetDeclaredMethods(typeof (DebugToolsPawns)))
      Patch(declaredMethod);
    foreach (MethodInfo method in GenCollection.Concat<Type>(AccessTools.InnerTypes(typeof (PrefabUtility)), typeof (PrefabUtility)).SelectMany<Type, MethodInfo>((Func<Type, IEnumerable<MethodInfo>>) (t => (IEnumerable<MethodInfo>) AccessToolsExtensions.GetDeclaredMethods(t))))
      Patch(method);

    void Patch(MethodInfo method)
    {
      if (method.IsGenericMethod || method.ContainsGenericParameters)
        return;
      if (unpatch)
        VMF_Harmony.Instance.Unpatch((MethodBase) method, transpiler);
      else
        VMF_Harmony.Instance.Patch((MethodBase) method, (HarmonyMethod) null, (HarmonyMethod) null, HarmonyMethod.op_Implicit(transpiler), (HarmonyMethod) null);
    }
  }

  private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Find_CurrentMap, MethodInfoCache.CachedMethodInfo.g_VehicleMapUtility_CurrentMap);
  }
}
