// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_LandingTargeter_TargeterUpdate
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (LandingTargeter), "TargeterUpdate")]
[PatchLevel(Level.Safe)]
public static class Patch_LandingTargeter_TargeterUpdate
{
  public static void Postfix()
  {
    if (Command_FocusVehicleMap.FocusLockedVehicle != null)
      return;
    Command_FocusVehicleMap.FocusedVehicle = (VehiclePawnWithMap) null;
    VehiclePawnWithMap vehicle;
    if (!UI.MouseMapPosition().TryGetVehicleMap(Find.CurrentMap, out vehicle, VehicleMapFlag.None))
      return;
    Command_FocusVehicleMap.FocusedVehicle = vehicle;
  }
}
