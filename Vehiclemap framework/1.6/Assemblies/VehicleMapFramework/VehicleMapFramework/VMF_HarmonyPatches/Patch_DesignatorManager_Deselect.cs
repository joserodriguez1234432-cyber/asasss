// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_DesignatorManager_Deselect
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (DesignatorManager), "Deselect")]
[PatchLevel(Level.Safe)]
public static class Patch_DesignatorManager_Deselect
{
  public static void Postfix()
  {
    if (Command_FocusVehicleMap.FocusLockedVehicle != null)
      return;
    Command_FocusVehicleMap.FocusedVehicle = (VehiclePawnWithMap) null;
  }
}
