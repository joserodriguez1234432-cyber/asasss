// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_CameraJumper_TryJumpInternal
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (CameraJumper), "TryJumpInternal", new Type[] {typeof (IntVec3), typeof (Map), typeof (CameraJumper.MovementMode)})]
[PatchLevel(Level.Safe)]
public static class Patch_CameraJumper_TryJumpInternal
{
  public static void Prefix(ref IntVec3 cell, ref Map map)
  {
    VehiclePawnWithMap vehicle;
    if (!map.IsVehicleMapOf(out vehicle))
      return;
    if (ModCompat.CompatBase<ModCompat.MultiFloors>.Active)
      vehicle.CurrentLevel = map;
    if (!VehicleMapFramework.VehicleMapFramework.settings.drawPlanet)
      return;
    if (((Thing) vehicle).Spawned)
    {
      map = ((Thing) vehicle).Map;
      cell = cell.ToBaseMapCoord(vehicle);
    }
    else
    {
      cell = cell.ToBaseMapCoord(vehicle);
      Patch_Map_MapUpdate.lastRenderedTick = -1;
    }
  }
}
