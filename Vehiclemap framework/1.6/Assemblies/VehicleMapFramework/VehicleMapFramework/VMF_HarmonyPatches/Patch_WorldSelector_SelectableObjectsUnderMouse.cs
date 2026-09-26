// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_WorldSelector_SelectableObjectsUnderMouse
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_WorldSelector_SelectableObjectsUnderMouse
{
  public static void Postfix(IEnumerable<WorldObject> __result)
  {
    if (!(__result is List<WorldObject> worldObjectList))
      return;
    worldObjectList.RemoveAll((Predicate<WorldObject>) (w =>
    {
      if (w is MapParent_Vehicle mapParentVehicle2)
      {
        VehiclePawnWithMap vehicle = mapParentVehicle2.vehicle;
        if (vehicle != null)
          return ((Thing) vehicle).Spawned;
      }
      return false;
    }));
  }
}
