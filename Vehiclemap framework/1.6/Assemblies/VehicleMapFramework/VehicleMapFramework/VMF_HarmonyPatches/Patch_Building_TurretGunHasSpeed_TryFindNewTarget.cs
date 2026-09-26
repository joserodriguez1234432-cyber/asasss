// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_TurretGunHasSpeed_TryFindNewTarget
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_SRALib")]
[HarmonyPatch]
[PatchLevel(Level.Cautious)]
public static class Patch_Building_TurretGunHasSpeed_TryFindNewTarget
{
  private static IEnumerable<MethodBase> TargetMethods()
  {
    return (IEnumerable<MethodBase>) LinqUtility.get_NonNull<MethodInfo>(ModCompat.SRALib.Building_TurretGunHasSpeed.Select<Type, MethodInfo>((Func<Type, MethodInfo>) (t => AccessTools.DeclaredMethod(t, "TryFindNewTarget", (Type[]) null, (Type[]) null))));
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return instructions.AddAllBuildingsColonistForThingInstance();
  }
}
