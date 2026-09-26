// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Caravan_ColonyThingsWillingToBuy
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Vehicles;
using Vehicles.World;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Caravan), "ColonyThingsWillingToBuy")]
[PatchLevel(Level.Safe)]
public static class Patch_Caravan_ColonyThingsWillingToBuy
{
  public static IEnumerable<Thing> Postfix(IEnumerable<Thing> values, Pawn playerNegotiator)
  {
    Caravan caravan = CaravanUtility.GetCaravan((Thing) playerNegotiator);
    IEnumerable<VehiclePawnWithMap> source;
    if (caravan == null)
    {
      source = (IEnumerable<VehiclePawnWithMap>) null;
    }
    else
    {
      List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
      source = pawnsListForReading != null ? pawnsListForReading.OfType<VehiclePawnWithMap>() : (IEnumerable<VehiclePawnWithMap>) null;
    }
    if (source == null)
    {
      VehicleCaravan vehicleCaravan = Ext_Vehicles.GetVehicleCaravan(playerNegotiator);
      if (vehicleCaravan == null)
      {
        source = (IEnumerable<VehiclePawnWithMap>) null;
      }
      else
      {
        IEnumerable<VehiclePawn> vehicles1 = vehicleCaravan.Vehicles;
        source = vehicles1 != null ? vehicles1.OfType<VehiclePawnWithMap>() : (IEnumerable<VehiclePawnWithMap>) null;
      }
    }
    List<VehiclePawnWithMap> vehicles = source != null ? source.ToList<VehiclePawnWithMap>() : (List<VehiclePawnWithMap>) null;
    if (values != null)
    {
      foreach (Thing thing in values)
        yield return thing;
    }
    if (!VehicleMapFramework.VehicleMapFramework.settings.includeMapThings)
    {
      if (!GenList.NullOrEmpty<VehiclePawnWithMap>((IList<VehiclePawnWithMap>) vehicles))
      {
        foreach (Thing thing in vehicles.SelectMany<VehiclePawnWithMap, Thing>((Func<VehiclePawnWithMap, IEnumerable<Thing>>) (vehicle => vehicle.ColonyThingsWillingToBuyOnVehicle((ITrader) playerNegotiator))))
          yield return thing;
      }
      else if (playerNegotiator is VehiclePawnWithMap vehicle1)
      {
        foreach (Thing thing in vehicle1.ColonyThingsWillingToBuyOnVehicle((ITrader) playerNegotiator))
          yield return thing;
      }
    }
  }
}
