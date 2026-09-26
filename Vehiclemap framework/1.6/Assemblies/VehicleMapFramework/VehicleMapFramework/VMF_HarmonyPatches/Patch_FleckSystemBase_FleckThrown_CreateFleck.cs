// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_FleckSystemBase_FleckThrown_CreateFleck
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (FleckSystemBase<FleckThrown>), "CreateFleck")]
[PatchLevel(Level.Safe)]
public static class Patch_FleckSystemBase_FleckThrown_CreateFleck
{
  public static void Prefix(
    FleckSystemBase<FleckThrown> __instance,
    ref FleckCreationData creationData)
  {
    VehiclePawnWithMap vehicle;
    if (!((FleckSystem) __instance).parent.parent.IsNonFocusedVehicleMapOf(out vehicle))
      return;
    creationData.spawnPosition = creationData.spawnPosition.ToBaseMapCoord(vehicle);
  }
}
