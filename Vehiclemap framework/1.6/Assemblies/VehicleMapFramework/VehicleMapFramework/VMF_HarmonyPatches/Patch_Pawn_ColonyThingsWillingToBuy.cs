// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Pawn_ColonyThingsWillingToBuy
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Pawn), "ColonyThingsWillingToBuy")]
[PatchLevel(Level.Safe)]
public static class Patch_Pawn_ColonyThingsWillingToBuy
{
  public static IEnumerable<Thing> Postfix(
    IEnumerable<Thing> values,
    Pawn playerNegotiator,
    Pawn __instance)
  {
    IEnumerator<Thing> enumerator;
    if (values != null)
    {
      enumerator = values.GetEnumerator();
      while (enumerator.MoveNext())
        yield return enumerator.Current;
      enumerator = (IEnumerator<Thing>) null;
    }
    HashSet<Map> mapSet = ((Thing) __instance).Map.BaseMapAndVehicleMaps(false);
    Map departMap = ((Thing) __instance).Map;
    CrossMapReachabilityUtility.DepartMapGlobal = departMap;
    try
    {
      foreach (Map map in mapSet)
      {
        ((Thing) __instance).VirtualMapTransfer(map);
        enumerator = __instance.trader.ColonyThingsWillingToBuy(playerNegotiator).GetEnumerator();
        while (enumerator.MoveNext())
          yield return enumerator.Current;
        enumerator = (IEnumerator<Thing>) null;
      }
    }
    finally
    {
      ((Thing) __instance).VirtualMapTransfer(departMap);
      CrossMapReachabilityUtility.DepartMapGlobal = (Map) null;
    }
  }
}
