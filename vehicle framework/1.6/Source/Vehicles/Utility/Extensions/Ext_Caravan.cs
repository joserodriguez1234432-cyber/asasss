// Decompiled with JetBrains decompiler
// Type: Vehicles.World.Ext_Caravan
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Threading;
using Verse;

#nullable disable
namespace Vehicles.World;

[PublicAPI]
public static class Ext_Caravan
{
  public static bool HasVehicle(this Caravan caravan)
  {
    return caravan is VehicleCaravan vehicleCaravan && vehicleCaravan.VehiclesListForReading.Count > 0;
  }

  public static bool HasBoat(this Caravan caravan)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return caravan is VehicleCaravan vehicleCaravan && vehicleCaravan.VehiclesListForReading.Exists(Ext_Caravan.\u003C\u003EO.\u003C0\u003E__IsBoat ?? (Ext_Caravan.\u003C\u003EO.\u003C0\u003E__IsBoat = new Predicate<VehiclePawn>(Ext_Vehicles.IsBoat)));
  }

  public static HashSet<VehicleDef> UniqueVehicleDefsInCaravan(this Caravan caravan)
  {
    HashSet<VehicleDef> vehicleDefSet = new HashSet<VehicleDef>();
    foreach (Pawn pawn in caravan.PawnsListForReading)
    {
      if (pawn is VehiclePawn vehiclePawn)
        vehicleDefSet.Add(vehiclePawn.VehicleDef);
    }
    return vehicleDefSet;
  }

  public static bool ViableForCaravan(this VehicleCaravan vehicleCaravan, VehiclePawn vehicle)
  {
    foreach (VehiclePawn vehiclePawn in vehicleCaravan.VehiclesListForReading)
    {
      if (!GridOwners.World.MatchingReachability(vehiclePawn.VehicleDef, vehicle.VehicleDef))
        return false;
    }
    return true;
  }

  public static List<Pawn> GrabPawnsFromVehicleCaravanSilentFail(this Caravan caravan)
  {
    if (caravan == null || !caravan.HasVehicle())
      return (List<Pawn>) null;
    List<Pawn> pawnList = new List<Pawn>();
    foreach (Pawn pawn in caravan.PawnsListForReading)
    {
      if (pawn is VehiclePawn vehiclePawn)
        pawnList.AddRange((IEnumerable<Pawn>) vehiclePawn.AllPawnsAboard);
      else
        pawnList.Add(pawn);
    }
    return pawnList;
  }

  public static void TransferPawnOrItem(this Caravan caravan, ThingOwner owner, Thing thing)
  {
    if (thing is Pawn)
    {
      owner.TryTransferToContainer(thing, (ThingOwner) caravan.pawns, false);
    }
    else
    {
      Pawn toMoveInventoryTo = CaravanInventoryUtility.FindPawnToMoveInventoryTo(thing, caravan.PawnsListForReading, (List<Pawn>) null, (Pawn) null);
      if (toMoveInventoryTo != null && ((ThingOwner) toMoveInventoryTo.inventory.innerContainer).TryAddOrTransfer(thing, true))
        return;
      Log.Error($"Failed to give item {thing} to caravan {caravan}; item was lost.");
      thing.Destroy((DestroyMode) 0);
    }
  }

  public static void EnsureWorldGridInitialized(this VehicleDef vehicleDef)
  {
    WorldVehiclePathGrid component = Find.World.GetComponent<WorldVehiclePathGrid>();
    if (component[vehicleDef].Enabled)
      return;
    component.RecalculateAllPerceivedPathCostsFor(vehicleDef, CancellationToken.None);
  }

  public static void EnsureWorldGridInitialized(this VehicleCaravan caravan)
  {
    foreach (VehiclePawn vehiclePawn in caravan.VehiclesListForReading)
      vehiclePawn.VehicleDef.EnsureWorldGridInitialized();
  }

  public static void EnsureMapInitialized(this VehicleDef vehicleDef, Map map)
  {
    VehiclePathingSystem cachedMapComponent = map.GetCachedMapComponent<VehiclePathingSystem>();
    if (!cachedMapComponent[vehicleDef].Suspended && cachedMapComponent[vehicleDef].VehiclePathGrid.Enabled)
      return;
    cachedMapComponent.RequestGridsFor(vehicleDef, DeferredGridGeneration.Urgency.Urgent);
  }

  public static void EnsureMapInitialized(List<VehiclePawn> vehicles, Map map)
  {
    VehiclePathingSystem cachedMapComponent = map.GetCachedMapComponent<VehiclePathingSystem>();
    foreach (VehiclePawn vehicle in vehicles)
    {
      VehicleDef vehicleDef = vehicle.VehicleDef;
      if (cachedMapComponent[vehicleDef].Suspended || !cachedMapComponent[vehicleDef].VehiclePathGrid.Enabled)
        cachedMapComponent.RequestGridsFor(vehicleDef, DeferredGridGeneration.Urgency.Urgent);
    }
  }

  public static void EnsureMapInitialized(this VehicleCaravan caravan, Map map)
  {
    Ext_Caravan.EnsureMapInitialized(caravan.VehiclesListForReading, map);
  }
}
