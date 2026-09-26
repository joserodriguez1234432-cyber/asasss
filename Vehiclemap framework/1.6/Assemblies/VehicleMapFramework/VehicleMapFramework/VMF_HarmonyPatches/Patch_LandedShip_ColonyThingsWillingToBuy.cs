// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_LandedShip_ColonyThingsWillingToBuy
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

[HarmonyPatchCategory("VMF_Patches_TraderShips")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_LandedShip_ColonyThingsWillingToBuy
{
  private static bool working;

  public static IEnumerable<Thing> Postfix(
    IEnumerable<Thing> values,
    Pawn playerNegotiator,
    ITrader __instance)
  {
    if (values != null)
    {
      IEnumerator<Thing> enumerator = values.GetEnumerator();
      while (enumerator.MoveNext())
        yield return enumerator.Current;
      enumerator = (IEnumerator<Thing>) null;
    }
    if (!Patch_LandedShip_ColonyThingsWillingToBuy.working)
    {
      HashSet<Map> mapSet = ((Thing) playerNegotiator).Map.BaseMapAndVehicleMaps(false);
      Map departMap = ((Thing) playerNegotiator).Map;
      CrossMapReachabilityUtility.DepartMapGlobal = departMap;
      try
      {
        Patch_LandedShip_ColonyThingsWillingToBuy.working = true;
        foreach (Map map in mapSet)
        {
          ((Thing) playerNegotiator).VirtualMapTransfer(map);
          List<Thing>.Enumerator enumerator = __instance.ColonyThingsWillingToBuy(playerNegotiator).ToList<Thing>().GetEnumerator();
          while (enumerator.MoveNext())
            yield return enumerator.Current;
          enumerator = new List<Thing>.Enumerator();
        }
      }
      finally
      {
        Patch_LandedShip_ColonyThingsWillingToBuy.working = false;
        ((Thing) playerNegotiator).VirtualMapTransfer(departMap);
        CrossMapReachabilityUtility.DepartMapGlobal = (Map) null;
      }
    }
  }
}
