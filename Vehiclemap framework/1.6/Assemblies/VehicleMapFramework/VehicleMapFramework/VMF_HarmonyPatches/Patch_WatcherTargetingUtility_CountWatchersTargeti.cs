// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_WatcherTargetingUtility_CountWatchersTargeting
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_DefensiveNetwork")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_WatcherTargetingUtility_CountWatchersTargeting
{
  private static bool working;

  public static void Postfix(Map map, Thing target, Building ignored, ref int __result)
  {
    if (Patch_WatcherTargetingUtility_CountWatchersTargeting.working)
      return;
    Patch_WatcherTargetingUtility_CountWatchersTargeting.working = true;
    try
    {
      foreach (Map mapAndVehicleMap in map.BaseMapAndVehicleMaps(false))
        __result += (int) ModCompat.DefensiveNetwork.CountWatchersTargeting.Invoke((object) null, VehicleMapFramework.Params<(object, object, object)>.Get(((object) mapAndVehicleMap, (object) target, (object) ignored)));
    }
    finally
    {
      Patch_WatcherTargetingUtility_CountWatchersTargeting.working = false;
    }
  }
}
