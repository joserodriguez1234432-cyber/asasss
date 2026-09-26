// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_VehicleOrientationController_Init
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (VehicleOrientationController), "Init")]
[PatchLevel(Level.Safe)]
public static class Patch_VehicleOrientationController_Init
{
  public static void Postfix(List<VehiclePawn> vehicles, ref IntVec3 ___start, ref IntVec3 ___end)
  {
    VehiclePawnWithMap vehicle;
    if (vehicles.All<VehiclePawn>((Func<VehiclePawn, bool>) (p => p is VehiclePawnWithMap)) || !UI.MouseMapPosition().TryGetVehicleMap(Find.CurrentMap, out vehicle, VehicleMapFlag.None))
      return;
    ___start = ___start.ToBaseMapCoord(vehicle);
    ___end = ___end.ToBaseMapCoord(vehicle);
  }
}
