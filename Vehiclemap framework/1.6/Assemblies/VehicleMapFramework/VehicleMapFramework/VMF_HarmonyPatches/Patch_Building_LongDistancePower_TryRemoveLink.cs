// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_LongDistancePower_TryRemoveLink
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_PowerPoles")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_Building_LongDistancePower_TryRemoveLink
{
  public static void Postfix(Building __instance, Building item, bool __result)
  {
    if (!__result)
      return;
    CompPowerPole comp1 = ((ThingWithComps) __instance).GetComp<CompPowerPole>();
    CompPowerPole comp2 = ((ThingWithComps) item).GetComp<CompPowerPole>();
    if (comp1 == null || comp2 == null || comp1.LinkedComp != comp2)
      return;
    comp1.Disconnect();
  }
}
