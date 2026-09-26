// Decompiled with JetBrains decompiler
// Type: Vehicles.World.EnterMapUtilityVehicles
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Verse;

#nullable disable
namespace Vehicles.World;

[PublicAPI]
public static class EnterMapUtilityVehicles
{
  private static bool SettleMapCellValidator(Map map, IntVec3 cell, VehicleDef vehicleDef)
  {
    VehicleRoom vehicleRoom = VehicleRegionAndRoomQuery.RoomAt(cell, map, vehicleDef);
    return vehicleRoom != null && vehicleRoom.CellCount >= 600;
  }

  [Obsolete("Use EnterMap instead.", true)]
  public static void EnterAndSpawn(
    VehicleCaravan caravan,
    Map map,
    CaravanEnterMode enterMode,
    CaravanDropInventoryMode dropInventoryMode = 0,
    bool draftColonists = false,
    Predicate<IntVec3> extraValidator = null)
  {
    if (enterMode == null)
    {
      Log.Error($"VehicleCaravan {caravan} tried to enter map {map} with no enter mode. Defaulting to edge.");
      enterMode = (CaravanEnterMode) 1;
    }
    IntVec3 enterCellVehicle = EnterMapUtilityVehicles.GetEnterCellVehicle(caravan, map, enterMode, extraValidator);
    Rot4 rot4;
    if (enterMode != 1)
    {
      rot4 = Rot4.North;
    }
    else
    {
      CellRect cellRect = CellRect.WholeMap(map);
      rot4 = ((CellRect) ref cellRect).GetClosestEdge(enterCellVehicle);
    }
    Rot4 edge = rot4;
    EnterMapUtilityVehicles.SpawnCaravanPawns(caravan, map, enterCellVehicle, edge, draftColonists);
  }

  [Obsolete("Deprecated", true)]
  public static IntVec3 GetEnterCellVehicle(
    VehicleCaravan caravan,
    Map map,
    CaravanEnterMode enterMode,
    Predicate<IntVec3> extraCellValidator)
  {
    caravan.EnsureMapInitialized(map);
    switch ((int) enterMode)
    {
      case 1:
        return EnterMapUtilityVehicles.FindNearEdgeCell(map, caravan.LeadVehicle.VehicleDef, ((WorldObject) caravan).Faction, new EnterMapUtilityVehicles.SpawnParams(enterMode));
      case 2:
        return EnterMapUtilityVehicles.FindCenterCell(map, caravan.LeadVehicle.VehicleDef, new EnterMapUtilityVehicles.SpawnParams(enterMode));
      default:
        throw new NotImplementedException("CaravanEnterMode");
    }
  }

  public static void EnterMap(
    VehicleCaravan caravan,
    Map map,
    in EnterMapUtilityVehicles.SpawnParams spawnParams)
  {
    if (spawnParams.enterMode == null)
      Trace.Fail($"VehicleCaravan {caravan} tried to enter map {map} with no enter mode. Defaulting to edge.");
    IntVec3 enterCellVehicle = EnterMapUtilityVehicles.GetEnterCellVehicle(caravan, map, in spawnParams);
    Rot4 rot4;
    if (spawnParams.enterMode != 1)
    {
      rot4 = Rot4.North;
    }
    else
    {
      CellRect cellRect = CellRect.WholeMap(map);
      rot4 = ((CellRect) ref cellRect).GetClosestEdge(enterCellVehicle);
    }
    Rot4 edge = rot4;
    EnterMapUtilityVehicles.SpawnCaravanPawns(caravan, map, enterCellVehicle, edge, spawnParams.draftColonists);
  }

  private static IntVec3 GetEnterCellVehicle(
    VehicleCaravan caravan,
    Map map,
    in EnterMapUtilityVehicles.SpawnParams spawnParams)
  {
    caravan.EnsureMapInitialized(map);
    CaravanEnterMode enterMode = spawnParams.enterMode;
    if (enterMode <= 1)
      return EnterMapUtilityVehicles.FindNearEdgeCell(map, caravan.LeadVehicle.VehicleDef, ((WorldObject) caravan).Faction, spawnParams);
    if (enterMode == 2)
      return EnterMapUtilityVehicles.FindCenterCell(map, caravan.LeadVehicle.VehicleDef, spawnParams);
    throw new NotImplementedException("CaravanEnterMode");
  }

  [Obsolete("Method signature changed, patch SpawnCaravanPawns instead.")]
  private static void SpawnVehicles(
    VehicleCaravan caravan,
    List<Pawn> pawns,
    Map map,
    IntVec3 enterCell,
    Rot4 edge,
    bool draftColonists)
  {
  }

  private static void SpawnCaravanPawns(
    VehicleCaravan caravan,
    Map map,
    IntVec3 enterCell,
    Rot4 edge,
    bool draftColonists)
  {
    using (new VehicleCaravan.RecacheDisabler(caravan))
    {
      bool waterEntry = caravan.HasBoat();
      List<Pawn> list;
      using (GlobalObjectPool.Get<Pawn>(out list))
      {
        list.AddRange((IEnumerable<Pawn>) caravan.pawns);
        foreach (Pawn pawn1 in list)
        {
          Pawn pawn = pawn1;
          IntVec3 spawnPoint = CellFinderExtended.RandomSpawnCellForPawnNear(enterCell, map, pawn, (Predicate<IntVec3>) (cell => cell.StandableUnknown(pawn, map)), waterEntry);
          IntVec3 map1 = pawn.ClampToMap(spawnPoint, map, 2);
          GenSpawn.Spawn((Thing) pawn, map1, map, ((Rot4) ref edge).Opposite, (WipeMode) 0, false, false);
          if (!((Thing) pawn).Spawned)
          {
            Trace.Fail($"Unable to spawn {pawn} in map. Sending back to caravan.");
            if (!caravan.ContainsPawn(pawn))
              caravan.AddPawn(pawn, true);
          }
          else
          {
            if (pawn.IsColonist && !pawn.InMentalState)
              pawn.drafter.Drafted = draftColonists;
            if (pawn is VehiclePawn vehiclePawn)
            {
              vehiclePawn.Angle = 0.0f;
              vehiclePawn.ignition.Drafted = draftColonists;
            }
          }
        }
        EnterMapUtilityVehicles.SpawnVehicles(caravan, list, map, enterCell, edge, draftColonists);
      }
    }
    if (((ThingOwner) caravan.pawns).Count != 0)
      return;
    ((WorldObject) caravan).Destroy();
  }

  private static Rot4 CalculateEdgeToSpawnBoatOn(Map map)
  {
    Rot4 edgeToSpawnBoatOn = Find.World.CoastDirectionAt(map.Tile);
    if (((Rot4) ref edgeToSpawnBoatOn).IsValid)
      return edgeToSpawnBoatOn;
    SurfaceTile surfaceTile = Find.WorldGrid.Surface[map.Tile];
    if (surfaceTile == null || GenList.NullOrEmpty<SurfaceTile.RiverLink>((IList<SurfaceTile.RiverLink>) surfaceTile.Rivers))
      return Rot4.Invalid;
    float num = Find.WorldGrid.GetHeadingFromTo(map.Tile, surfaceTile.Rivers.OrderBy<SurfaceTile.RiverLink, int>((Func<SurfaceTile.RiverLink, int>) (link => link.river.degradeThreshold)).First<SurfaceTile.RiverLink>().neighbor).ClampAngle();
    if ((double) num < 45.0)
      return Rot4.South;
    if ((double) num < 135.0)
      return Rot4.East;
    if ((double) num < 225.0)
      return Rot4.North;
    if ((double) num < 315.0)
      return Rot4.West;
    throw new ArgumentException("ClampAndWrap did not return valid 0:360 value");
  }

  private static IntVec3 FindCenterCell(
    Map map,
    VehicleDef vehicleDef,
    EnterMapUtilityVehicles.SpawnParams spawnParams)
  {
    IntVec3 centerCell;
    if (RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((Predicate<IntVec3>) (cell => Validator(map, vehicleDef, cell, in spawnParams)), map, ref centerCell))
      return centerCell;
    Log.Warning("Could not find any valid cell.");
    return CellFinder.RandomCell(map);

    static bool Validator(
      Map map,
      VehicleDef vehicleDef,
      IntVec3 cell,
      in EnterMapUtilityVehicles.SpawnParams spawnParams)
    {
      return (spawnParams.extraCellValidator == null || spawnParams.extraCellValidator(cell, map, vehicleDef)) && cell.Standable(vehicleDef, map) && !GridsUtility.Fogged(cell, map) && map.reachability.CanReachMapEdge(cell, TraverseParms.For((TraverseMode) 2, (Danger) 3, false, false, false, true, false));
    }
  }

  private static IntVec3 FindNearEdgeCell(
    Map map,
    VehicleDef vehicleDef,
    Faction faction,
    EnterMapUtilityVehicles.SpawnParams spawnParams)
  {
    Rot4 rot = Rot4.Random;
    if (vehicleDef.type == VehicleType.Sea)
      rot = EnterMapUtilityVehicles.CalculateEdgeToSpawnBoatOn(map);
    for (EnterMapUtilityVehicles.RoadPreference preference = EnterMapUtilityVehicles.RoadPreferenceFor(faction); preference > EnterMapUtilityVehicles.RoadPreference.Invalid; preference--)
    {
      IntVec3 foundCell;
      if (TryFindCellWithBestPreference(out foundCell))
        return foundCell;

      bool TryFindCellWithBestPreference(out IntVec3 foundCell)
      {
        foundCell = IntVec3.Invalid;
        return EnterMapUtilityVehicles.TryFindNearEdgeCell(map, vehicleDef, rot, preference, spawnParams, out foundCell) || EnterMapUtilityVehicles.TryFindNearEdgeCell(map, vehicleDef, ((Rot4) ref rot).Opposite, preference, spawnParams, out foundCell) || EnterMapUtilityVehicles.TryFindNearEdgeCell(map, vehicleDef, ((Rot4) ref rot).Rotated((RotationDirection) 1), preference, spawnParams, out foundCell) || EnterMapUtilityVehicles.TryFindNearEdgeCell(map, vehicleDef, ((Rot4) ref rot).Rotated((RotationDirection) 3), preference, spawnParams, out foundCell);
      }
    }
    Log.Warning("Could not find any valid edge cell.");
    return CellFinder.RandomCell(map);
  }

  private static bool TryFindNearEdgeCell(
    Map map,
    VehicleDef vehicleDef,
    Rot4 rot,
    EnterMapUtilityVehicles.RoadPreference roadPref,
    EnterMapUtilityVehicles.SpawnParams spawnParams,
    out IntVec3 root)
  {
    Faction hostFaction = map.ParentFaction;
    if (CellFinderExtended.TryFindRandomEdgeCellWith(new Predicate<IntVec3>(OptimalSpot), map, rot, vehicleDef, CellFinder.EdgeRoadChance_Always, out root))
      return true;
    if (!CellFinderExtended.TryFindRandomEdgeCellWith(new Predicate<IntVec3>(MinimalValidator), map, rot, vehicleDef, CellFinder.EdgeRoadChance_Always, out root))
      return false;
    root = CellFinderExtended.RandomClosewalkCellNear(root, map, vehicleDef, 5);
    return true;

    bool MinimalValidator(IntVec3 cell)
    {
      return cell.Standable(vehicleDef, map) && !GridsUtility.Fogged(cell, map) && (spawnParams.extraCellValidator == null || spawnParams.extraCellValidator(cell, map, vehicleDef));
    }

    bool OptimalSpot(IntVec3 cell)
    {
      if (!cell.Standable(vehicleDef, map) || GridsUtility.Fogged(cell, map) || spawnParams.extraCellValidator != null && !spawnParams.extraCellValidator(cell, map, vehicleDef) || !EnterMapUtilityVehicles.AllowsPreference(map, cell, roadPref))
        return false;
      VehiclePathingSystem.VehiclePathData vehiclePathData = map.GetCachedMapComponent<VehiclePathingSystem>()[vehicleDef];
      if (hostFaction != null && vehiclePathData.VehicleReachability.CanReachBase(cell, vehicleDef))
        return true;
      return hostFaction == null && vehiclePathData.VehicleReachability.CanReachBiggestMapEdgeRoom(cell);
    }
  }

  private static EnterMapUtilityVehicles.RoadPreference RoadPreferenceFor(Faction faction)
  {
    return !FactionUtility.HostileTo(faction, Faction.OfPlayer) ? EnterMapUtilityVehicles.RoadPreference.Prioritize : EnterMapUtilityVehicles.RoadPreference.None;
  }

  private static bool AllowsPreference(
    Map map,
    IntVec3 cell,
    EnterMapUtilityVehicles.RoadPreference roadPref)
  {
    if (roadPref == EnterMapUtilityVehicles.RoadPreference.NoAvoidal)
      return !map.areaManager.Get<Area_RoadAvoidal>()[cell];
    return roadPref != EnterMapUtilityVehicles.RoadPreference.Prioritize || map.areaManager.Get<Area_Road>()[cell];
  }

  private enum RoadPreference
  {
    Invalid,
    None,
    NoAvoidal,
    Prioritize,
  }

  public struct SpawnParams
  {
    public required CaravanEnterMode enterMode;
    public CaravanDropInventoryMode dropInventoryMode;
    public bool draftColonists;
    public EnterMapUtilityVehicles.SpawnParams.SpawnCellValidator extraCellValidator;

    [SetsRequiredMembers]
    public SpawnParams(CaravanEnterMode enterMode)
    {
      this.extraCellValidator = (EnterMapUtilityVehicles.SpawnParams.SpawnCellValidator) null;
      this.enterMode = (CaravanEnterMode) 1;
      this.dropInventoryMode = (CaravanDropInventoryMode) 0;
      this.draftColonists = false;
      this.enterMode = enterMode;
    }

    public delegate bool SpawnCellValidator(IntVec3 cell, Map map, VehicleDef vehicleDef);
  }
}
