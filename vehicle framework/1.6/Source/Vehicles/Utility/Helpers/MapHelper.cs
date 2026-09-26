// Decompiled with JetBrains decompiler
// Type: Vehicles.MapHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using Vehicles.World;
using Verse;

#nullable disable
namespace Vehicles;

public static class MapHelper
{
  public static void UnfogMapFromEdge(Map map, VehicleDef vehicleDef = null)
  {
    IntVec3 intVec3;
    if (!CellFinder.TryFindRandomCellNear(map.Center, map, 30, new Predicate<IntVec3>(Validator), ref intVec3, -1) && !CellFinder.TryFindRandomEdgeCellWith(new Predicate<IntVec3>(Validator), map, 0.0f, ref intVec3) && !CellFinder.TryFindRandomCell(map, new Predicate<IntVec3>(Validator), ref intVec3))
      return;
    FloodFillerFog.FloodUnfog(intVec3, map);

    bool Validator(IntVec3 cellToCheck)
    {
      if (!GenGrid.Standable(cellToCheck, map) || GridsUtility.Roofed(cellToCheck, map))
        return false;
      if (vehicleDef == null)
        return map.reachability.CanReachMapEdge(cellToCheck, TraverseParms.For((TraverseMode) 5, (Danger) 3, false, false, false, true, false));
      VehiclePathingSystem cachedMapComponent = map.GetCachedMapComponent<VehiclePathingSystem>();
      if (cachedMapComponent[vehicleDef].Suspended)
        cachedMapComponent.RequestGridsFor(vehicleDef, DeferredGridGeneration.Urgency.Urgent);
      return cachedMapComponent[vehicleDef].VehicleReachability.CanReachMapEdge(cellToCheck, TraverseParms.For((TraverseMode) 2, (Danger) 3, false, false, false, true, false));
    }
  }

  [Obsolete("Will be removed in 1.7")]
  public static bool AnyVehicleSkyfallersBlockingMap(Map map)
  {
    foreach (Thing allClaimant in map.GetDetachedMapComponent<VehiclePositionManager>().AllClaimants)
    {
      if (allClaimant.ParentHolder is VehicleSkyfaller)
        return true;
    }
    return false;
  }

  public static bool AnyAerialVehiclesInRecon(Map map)
  {
    foreach (AerialVehicleInFlight aerialVehicle in Find.World.GetComponent<VehicleWorldObjectsHolder>().AerialVehicles)
    {
      if (aerialVehicle.flightPath.InRecon && PlanetTile.op_Equality(aerialVehicle.flightPath.Last.Tile, map.Tile) || aerialVehicle.flightPath.ArrivalAction != null && PlanetTile.op_Equality(aerialVehicle.flightPath.Last.Tile, map.Tile))
        return true;
    }
    return false;
  }

  public static bool NonStandableOrVehicleBlocked(
    VehiclePawn vehicle,
    Map map,
    IntVec3 cell,
    Rot4 rot)
  {
    VehiclePawn vehiclePawn = VehicleReservationManager.VehicleInhabitingCells(vehicle.PawnOccupiedCells(cell, rot), map);
    return vehiclePawn != null && vehiclePawn != vehicle || !vehicle.CellRectStandable(map, new IntVec3?(cell), new Rot4?(rot));
  }

  public static bool ImpassableOrVehicleBlocked(
    VehiclePawn vehicle,
    Map map,
    IntVec3 cell,
    Rot4 rot)
  {
    VehiclePawn vehiclePawn = VehicleReservationManager.VehicleInhabitingCells(vehicle.PawnOccupiedCells(cell, rot), map);
    return vehiclePawn != null && vehiclePawn != vehicle || vehicle.LocationRestrictedBySize(map, cell, (Rot8) rot);
  }

  public static VehiclePawn VehicleInPosition(
    VehiclePawn vehicle,
    Map map,
    IntVec3 cell,
    Rot4 rot)
  {
    return VehicleReservationManager.VehicleInhabitingCells(vehicle.PawnOccupiedCells(cell, rot), map);
  }

  public static VehicleSkyfaller VehicleSkyfallerInPosition(
    VehiclePawn vehicle,
    Map map,
    IntVec3 cell,
    Rot4 rot)
  {
    foreach (IntVec3 pawnOccupiedCell in (IEnumerable<IntVec3>) (object) vehicle.PawnOccupiedCells(cell, rot))
    {
      VehicleSkyfaller vehicleSkyfaller = map.thingGrid.ThingAt<VehicleSkyfaller>(pawnOccupiedCell);
      if (vehicleSkyfaller != null)
        return vehicleSkyfaller;
    }
    return (VehicleSkyfaller) null;
  }
}
