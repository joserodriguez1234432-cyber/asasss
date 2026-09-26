// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_OrbitalTradeBeacon_TradeableCellsAround
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Building_OrbitalTradeBeacon), "TradeableCellsAround")]
[PatchLevel(Level.Safe)]
public static class Patch_Building_OrbitalTradeBeacon_TradeableCellsAround
{
  public static void Postfix(Map map, List<IntVec3> __result)
  {
    __result.RemoveAll((Predicate<IntVec3>) (c => !GenGrid.InBounds(c, map)));
  }
}
