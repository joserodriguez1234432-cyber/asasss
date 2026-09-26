// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_VehiclePawn_Notify_Teleported
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Vehicles;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (VehiclePawn), "Notify_Teleported")]
[PatchLevel(Level.Safe)]
public static class Patch_VehiclePawn_Notify_Teleported
{
  public static void Postfix(VehiclePawn __instance)
  {
    if (!(__instance is VehiclePawnWithMap vehiclePawnWithMap))
      return;
    CrossMapReachabilityCache.ClearCacheFor(vehiclePawnWithMap.VehicleMap);
  }
}
