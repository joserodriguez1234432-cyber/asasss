// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_OrbitalTradeBeacon_AllPowered
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Building_OrbitalTradeBeacon), "AllPowered")]
[PatchLevel(Level.Safe)]
public static class Patch_Building_OrbitalTradeBeacon_AllPowered
{
  public static IEnumerable<Building_OrbitalTradeBeacon> Postfix(
    IEnumerable<Building_OrbitalTradeBeacon> values,
    Map map)
  {
    foreach (Building_OrbitalTradeBeacon orbitalTradeBeacon in values)
      yield return orbitalTradeBeacon;
    foreach (Building_OrbitalTradeBeacon orbitalTradeBeacon in map.BaseMapAndVehicleMaps(false).SelectMany<Map, Building_OrbitalTradeBeacon>((Func<Map, IEnumerable<Building_OrbitalTradeBeacon>>) (m => m.listerBuildings.AllBuildingsColonistOfClass<Building_OrbitalTradeBeacon>().Where<Building_OrbitalTradeBeacon>((Func<Building_OrbitalTradeBeacon, bool>) (b =>
    {
      CompPowerTrader comp = ((ThingWithComps) b).GetComp<CompPowerTrader>();
      return comp == null || comp.PowerOn;
    })))))
      yield return orbitalTradeBeacon;
  }
}
