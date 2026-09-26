// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_SelectionHelper_MultiSelectClicker
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (SelectionHelper), "MultiSelectClicker")]
[PatchLevel(Level.Safe)]
public static class Patch_SelectionHelper_MultiSelectClicker
{
  public static bool Prefix(ref bool __result)
  {
    if (!UI.MouseMapPosition().TryGetVehicleMap(Find.CurrentMap, out VehiclePawnWithMap _, VehicleMapFlag.None))
      return true;
    __result = false;
    return false;
  }
}
