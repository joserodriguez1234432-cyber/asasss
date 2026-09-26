// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_TradeShip_ColonyThingsWillingToBuy
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (TradeShip), "ColonyThingsWillingToBuy")]
[PatchLevel(Level.Safe)]
public static class Patch_TradeShip_ColonyThingsWillingToBuy
{
  public static IEnumerable<Thing> Postfix(IEnumerable<Thing> values, Pawn playerNegotiator)
  {
    List<Thing> list = values.ToList<Thing>();
    foreach (Map mapAndVehicleMap in ((Thing) playerNegotiator).Map.BaseMapAndVehicleMaps(false))
      list.AddRange((IEnumerable<Thing>) TradeUtility.AllSellableColonyPawns(mapAndVehicleMap, false));
    return (IEnumerable<Thing>) list;
  }
}
