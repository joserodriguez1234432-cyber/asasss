// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_MouseoverReadout_MouseoverReadoutOnGUI
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (MouseoverReadout), "MouseoverReadoutOnGUI")]
[PatchLevel(Level.Safe)]
public static class Patch_MouseoverReadout_MouseoverReadoutOnGUI
{
  public static void PrefixCommon(
    ref (sbyte, Command_FocusVehicleMap.FocusVehicle)? __state)
  {
    VehiclePawnWithMap vehicle = Command_FocusVehicleMap.FocusedVehicle;
    if (vehicle == null && !UI.MouseMapPosition().TryGetVehicleMap(Find.CurrentMap, out vehicle))
      return;
    __state = new (sbyte, Command_FocusVehicleMap.FocusVehicle)?((Current.Game.currentMapIndex, new Command_FocusVehicleMap.FocusVehicle(vehicle)));
    Current.Game.currentMapIndex = (sbyte) vehicle.CurrentLevel.Index;
  }

  public static void Prefix(
    ref (sbyte, Command_FocusVehicleMap.FocusVehicle)? __state)
  {
    if (Event.current.type != 7 || Find.MainTabsRoot.OpenTab != null)
      return;
    Patch_MouseoverReadout_MouseoverReadoutOnGUI.PrefixCommon(ref __state);
  }

  public static void Finalizer(
    (sbyte, Command_FocusVehicleMap.FocusVehicle)? __state)
  {
    if (!__state.HasValue)
      return;
    Current.Game.currentMapIndex = __state.Value.Item1;
    __state.Value.Item2.Dispose();
  }
}
