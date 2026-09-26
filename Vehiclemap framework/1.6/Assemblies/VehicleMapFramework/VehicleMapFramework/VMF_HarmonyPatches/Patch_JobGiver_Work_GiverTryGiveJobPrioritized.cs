// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_JobGiver_Work_GiverTryGiveJobPrioritized
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_JobGiver_Work_GiverTryGiveJobPrioritized
{
  private static MethodInfo TargetMethod()
  {
    return AccessTools.FindIncludingInnerTypes<MethodInfo>(typeof (JobGiver_Work), (Func<Type, MethodInfo>) (t => GenCollection.FirstOrDefault<MethodInfo>(AccessToolsExtensions.GetDeclaredMethods(t), (Predicate<MethodInfo>) (m => m.Name.Contains("<GiverTryGiveJobPrioritized>")))));
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return Patch_JobGiver_Work_Validator.Transpiler(instructions);
  }
}
