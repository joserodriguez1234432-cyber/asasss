// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_LongDistancePower_CanLinkTo
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_PowerPoles")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_Building_LongDistancePower_CanLinkTo
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Thing_Position, MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMap);
  }

  public static void Postfix(Building __instance, Building other, ref bool __result)
  {
    CompPowerPole compPowerPole;
    CompPowerPole other1;
    __result = ((__result ? 1 : 0) & (((Thing) __instance).Map == ((Thing) other).Map ? 1 : (!ThingCompUtility.TryGetComp<CompPowerPole>((ThingWithComps) __instance, ref compPowerPole) || !ThingCompUtility.TryGetComp<CompPowerPole>((ThingWithComps) other, ref other1) ? 0 : (compPowerPole.CanLinkTo((CompPowerNetLink) other1) ? 1 : 0)))) != 0;
  }
}
