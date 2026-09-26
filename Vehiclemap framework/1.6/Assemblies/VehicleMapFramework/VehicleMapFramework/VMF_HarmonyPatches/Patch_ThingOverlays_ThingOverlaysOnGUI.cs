// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ThingOverlays_ThingOverlaysOnGUI
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (ThingOverlays), "ThingOverlaysOnGUI")]
[PatchLevel(Level.Safe)]
public static class Patch_ThingOverlays_ThingOverlaysOnGUI
{
  public static bool Prefix()
  {
    if (Event.current.type != 7)
      return true;
    CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
    Bounds bounds = ((CellRect) ref currentViewRect).ToBounds();
    VehiclePawnWithMap vehicle;
    bool flag = Find.CurrentMap.IsVehicleMapOf(out vehicle);
    foreach (VehiclePawnWithMap vehiclePawnWithMap in flag ? GetVehicles() : (IEnumerable<VehiclePawnWithMap>) VehiclePawnWithMapCache.AllVehiclesOn(Find.CurrentMap))
    {
      if (!flag)
      {
        if (((Bounds) ref bounds).Contains(Vector3Utility.Yto0(((Thing) vehiclePawnWithMap).DrawPos)))
        {
          try
          {
            ((Thing) vehiclePawnWithMap).DrawGUIOverlay();
          }
          catch (Exception ex)
          {
            Log.Error($"Exception drawing ThingOverlay for {vehiclePawnWithMap}: {ex}");
          }
        }
      }
      foreach (Thing thing in vehiclePawnWithMap.CurrentLevel.listerThings.ThingsInGroup((ThingRequestGroup) 38))
      {
        if (((Bounds) ref bounds).Contains(Vector3Utility.Yto0(thing.DrawPos)))
        {
          try
          {
            thing.DrawGUIOverlay();
          }
          catch (Exception ex)
          {
            Log.Error($"Exception drawing ThingOverlay for {thing}: {ex}");
          }
        }
      }
    }
    return !flag;

    IEnumerable<VehiclePawnWithMap> GetVehicles()
    {
      WorldObject orStashedVehicle = vehicle.VehicleCaravanOrStashedVehicle;
      if (orStashedVehicle != null)
      {
        foreach (VehiclePawnWithMap vehicle in VehicleCaravanHelper.get_Vehicles(orStashedVehicle).OfType<VehiclePawnWithMap>())
          yield return vehicle;
      }
      else
        yield return vehicle;
    }
  }
}
