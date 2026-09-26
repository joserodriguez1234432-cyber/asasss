// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_WASDGameComponent_GetImportantThing_Delegate
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Reflection;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_WASDedPawn")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_WASDGameComponent_GetImportantThing_Delegate
{
  private static MethodBase TargetMethod()
  {
    return (MethodBase) AccessTools.FindIncludingInnerTypes<MethodInfo>(GenTypes.GetTypeInAnyAssembly("wasdedPawn.WASDGameComponent", "wasdedPawn"), (Func<Type, MethodInfo>) (t => GenCollection.FirstOrDefault<MethodInfo>(AccessToolsExtensions.GetDeclaredMethods(t), (Predicate<MethodInfo>) (m => m.Name.Contains("<GetImportantThing>")))));
  }

  public static void Postfix(Thing t, ref int __result)
  {
    if (!(t is VehiclePawnWithMap))
      return;
    __result = 2;
  }
}
