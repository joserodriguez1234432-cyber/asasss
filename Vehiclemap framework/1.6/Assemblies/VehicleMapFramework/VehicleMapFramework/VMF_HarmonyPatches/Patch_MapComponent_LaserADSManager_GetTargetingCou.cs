// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_MapComponent_LaserADSManager_GetTargetingCount
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_SRALib")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_MapComponent_LaserADSManager_GetTargetingCount
{
  public static void Postfix(MapComponent __instance, Thing proj, ref int __result)
  {
    Type type = __instance.GetType();
    ReadOnlySpan<VehiclePawnWithMap> readOnlySpan = VehiclePawnWithMapCache.AllVehiclesOnAsReadOnlySpan(__instance.map);
    for (int index = 0; index < readOnlySpan.Length; ++index)
    {
      MapComponent component = readOnlySpan[index].VehicleMap.GetComponent(type);
      if (component != null)
        __result += (int) ModCompat.SRALib.GetTargetingCount.Invoke((object) component, SingleParam.Get((object) proj));
    }
  }
}
