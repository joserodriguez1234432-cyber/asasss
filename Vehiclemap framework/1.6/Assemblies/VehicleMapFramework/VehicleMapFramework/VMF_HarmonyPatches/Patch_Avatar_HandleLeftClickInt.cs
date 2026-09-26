// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Avatar_HandleLeftClickInt
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_PerspectiveShift")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_Avatar_HandleLeftClickInt
{
  public static void Prefix(
    Pawn ___pawn,
    ref (VirtualTeleporter?, Command_FocusVehicleMap.FocusVehicle?) __state)
  {
    if (!((Thing) ___pawn).Spawned)
      return;
    Vector3 original1 = UI.MouseMapPosition();
    VehiclePawnWithMap vehicle;
    Map map = original1.TryGetVehicleMap(Find.CurrentMap, out vehicle, VehicleMapFlag.None) ? vehicle.VehicleMap : Find.CurrentMap;
    if (((Thing) ___pawn).Map != map)
    {
      IntVec3 original2 = VehicleMapUtility.get_PositionOnBaseMap((Thing) ___pawn);
      if (vehicle != null)
      {
        if (!GenGrid.InBounds(original1.ToVehicleMapCoord(vehicle), vehicle.VehicleMap))
          return;
        original2 = original2.ToVehicleMapCoord(vehicle);
      }
      CrossMapReachabilityUtility.set_DepartMap(___pawn, map);
      __state.Item1 = new VirtualTeleporter?(new VirtualTeleporter((Thing) ___pawn, map, new IntVec3?(original2)));
    }
    if (vehicle == null)
      return;
    __state.Item2 = new Command_FocusVehicleMap.FocusVehicle?(new Command_FocusVehicleMap.FocusVehicle(vehicle));
  }

  public static void Finalizer(
    Pawn ___pawn,
    (VirtualTeleporter?, Command_FocusVehicleMap.FocusVehicle?) __state)
  {
    ___pawn.RemoveDepartMap();
    // ISSUE: explicit reference operation
    ref VirtualTeleporter? local1 = @__state.Item1;
    if (local1.HasValue)
      local1.GetValueOrDefault().Dispose();
    // ISSUE: explicit reference operation
    ref Command_FocusVehicleMap.FocusVehicle? local2 = @__state.Item2;
    if (!local2.HasValue)
      return;
    local2.GetValueOrDefault().Dispose();
  }
}
