// Decompiled with JetBrains decompiler
// Type: Vehicles.World.VehicleCaravanPathingHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld.Planet;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.World;

[PublicAPI]
public static class VehicleCaravanPathingHelper
{
  private const int CacheDuration = 100;
  private const int MaxIterations = 10000;
  private static readonly List<VehicleCaravanPathingHelper.TileEstimate> TmpTicksToArrive = new List<VehicleCaravanPathingHelper.TileEstimate>();
  private static int cacheTicks = -1;
  private static VehicleCaravan cachedForCaravan;
  private static int cachedForDest = -1;
  private static int cachedResult = -1;

  public static bool ShouldRestAt(VehicleCaravan caravan, in PlanetTile tile)
  {
    if (!((WorldObject) caravan).Spawned || !caravan.needs.AnyPawnsNeedRest || !CaravanNightRestUtility.RestingNowAt(((WorldObject) caravan).Tile) || !VehicleCaravanPathingHelper.ShouldRestAt(caravan.VehiclesListForReading, in tile))
      return false;
    return !caravan.vehiclePather.Moving || !Caravan_PathFollower.IsValidFinalPushDestination(caravan.vehiclePather.Destination) || PlanetTile.op_Inequality(caravan.vehiclePather.NextTile, caravan.vehiclePather.Destination) || Mathf.CeilToInt(caravan.vehiclePather.nextTileCostLeft) > 10000;
  }

  public static bool ShouldRestAt(List<VehiclePawn> vehicles, in PlanetTile tile)
  {
    bool flag = true;
    foreach (VehiclePawn vehicle in vehicles)
      flag &= (vehicle.MovementPermissions & VehiclePermissions.Autonomous) != 0;
    return !flag && CaravanNightRestUtility.RestingNowAt(tile);
  }

  public static bool ShouldRestAt(List<VehicleDef> vehicleDefs, in PlanetTile tile)
  {
    bool flag = true;
    foreach (VehicleDef vehicleDef in vehicleDefs)
      flag &= (vehicleDef.MovementPermissions & VehiclePermissions.Autonomous) != 0;
    return !flag && CaravanNightRestUtility.RestingNowAt(tile);
  }

  public static int EstimatedTicksToArrive([NotNull] VehicleCaravan caravan, bool allowCaching)
  {
    if (allowCaching && caravan == VehicleCaravanPathingHelper.cachedForCaravan && PlanetTile.op_Equality(caravan.vehiclePather.Destination, PlanetTile.op_Implicit(VehicleCaravanPathingHelper.cachedForDest)) && Find.TickManager.TicksGame - VehicleCaravanPathingHelper.cacheTicks < 100)
      return VehicleCaravanPathingHelper.cachedResult;
    PlanetTile to = PlanetTile.Invalid;
    int arrive = 0;
    if (((WorldObject) caravan).Spawned && caravan.vehiclePather.Moving && caravan.vehiclePather.curPath != null)
    {
      to = caravan.vehiclePather.Destination;
      arrive = VehicleCaravanPathingHelper.EstimatedTicksToArrive(caravan.VehiclesListForReading.Select<VehiclePawn, VehicleDef>((Func<VehiclePawn, VehicleDef>) (vehicle => vehicle.VehicleDef)).ToList<VehicleDef>(), ((WorldObject) caravan).Tile, in to, caravan.vehiclePather.curPath, caravan.vehiclePather.nextTileCostLeft, caravan.TicksPerMove, Find.TickManager.TicksAbs);
    }
    if (allowCaching)
    {
      VehicleCaravanPathingHelper.cacheTicks = Find.TickManager.TicksGame;
      VehicleCaravanPathingHelper.cachedForCaravan = caravan;
      VehicleCaravanPathingHelper.cachedForDest = PlanetTile.op_Implicit(to);
      VehicleCaravanPathingHelper.cachedResult = arrive;
    }
    return arrive;
  }

  public static int EstimatedTicksToArrive(
    [NotNull] VehicleCaravan caravan,
    in PlanetTile from,
    in PlanetTile to)
  {
    using (WorldPath path = Find.World.GetComponent<WorldVehiclePathfinder>().FindPath(from, to, caravan))
      return !path.Found ? 0 : VehicleCaravanPathingHelper.EstimatedTicksToArrive(caravan.VehiclesListForReading.UniqueVehicleDefsInList(), in from, in to, path, 0.0f, caravan.TicksPerMove, Find.TickManager.TicksAbs);
  }

  public static int EstimatedTicksToArrive(
    List<VehicleDef> vehicleDefs,
    in PlanetTile from,
    in PlanetTile to,
    WorldPath path,
    float nextTileCostLeft,
    int caravanTicksPerMove,
    int curTicksAbs)
  {
    List<VehicleCaravanPathingHelper.TileEstimate> list;
    using (GlobalObjectPool.Get<VehicleCaravanPathingHelper.TileEstimate>(out list))
    {
      VehicleCaravanPathingHelper.EstimatedTicksToArriveToEvery(vehicleDefs, in from, in to, path, nextTileCostLeft, caravanTicksPerMove, curTicksAbs, list);
      return VehicleCaravanPathingHelper.EstimatedTicksToArrive(to, list);
    }
  }

  private static void EstimatedTicksToArriveToEvery(
    List<VehicleDef> vehicleDefs,
    in PlanetTile from,
    in PlanetTile to,
    WorldPath path,
    float nextTileCostLeft,
    int caravanTicksPerMove,
    int curTicksAbs,
    List<VehicleCaravanPathingHelper.TileEstimate> outTicksToArrive)
  {
    outTicksToArrive.Clear();
    outTicksToArrive.Add(new VehicleCaravanPathingHelper.TileEstimate(from, 0));
    if (PlanetTile.op_Equality(from, to))
    {
      outTicksToArrive.Add(new VehicleCaravanPathingHelper.TileEstimate(to, 0));
    }
    else
    {
      int num1 = 0;
      int nextTile = PlanetTile.op_Implicit(from);
      int num2 = 0;
      int num3 = 19999;
      int num4 = 60000 - num3;
      int num5 = 0;
      int num6;
      if (VehicleCaravanPathingHelper.ShouldRestAt(vehicleDefs, in from) && CaravanNightRestUtility.WouldBeRestingAt(from, (long) curTicksAbs))
      {
        if (VehicleCaravan_PathFollower.IsValidFinalPushDestination(to) && (PlanetTile.op_Equality(path.Peek(0), to) || (double) nextTileCostLeft <= 0.0 && path.NodesLeftCount >= 2 && PlanetTile.op_Equality(path.Peek(1), to)))
        {
          int num7 = Mathf.CeilToInt(VehicleCaravanPathingHelper.GetCostToMove(vehicleDefs, nextTileCostLeft, PlanetTile.op_Equality(path.Peek(0), to), curTicksAbs, num1, caravanTicksPerMove, PlanetTile.op_Implicit(from), PlanetTile.op_Implicit(to)) / 1f);
          if (num7 <= 10000)
          {
            int ticksToArrive = num1 + num7;
            outTicksToArrive.Add(new VehicleCaravanPathingHelper.TileEstimate(to, ticksToArrive));
            return;
          }
        }
        num1 += CaravanNightRestUtility.LeftRestTicksAt(from, (long) curTicksAbs);
        num6 = num4;
      }
      else
        num6 = CaravanNightRestUtility.LeftNonRestTicksAt(from, (long) curTicksAbs);
      for (int index = 0; index < 10000; ++index)
      {
        if (num5 <= 0)
        {
          if (PlanetTile.op_Equality(PlanetTile.op_Implicit(nextTile), to))
          {
            outTicksToArrive.Add(new VehicleCaravanPathingHelper.TileEstimate(to, num1));
            return;
          }
          bool firstInPath = num2 == 0;
          int curTile = nextTile;
          nextTile = PlanetTile.op_Implicit(path.Peek(num2));
          ++num2;
          outTicksToArrive.Add(new VehicleCaravanPathingHelper.TileEstimate(PlanetTile.op_Implicit(curTile), num1));
          num5 = Mathf.CeilToInt(VehicleCaravanPathingHelper.GetCostToMove(vehicleDefs, nextTileCostLeft, firstInPath, curTicksAbs, num1, caravanTicksPerMove, curTile, nextTile));
        }
        if (num6 < num5)
        {
          int num8 = num1 + num6;
          num5 -= num6;
          if (PlanetTile.op_Equality(PlanetTile.op_Implicit(nextTile), to) && num5 <= 10000 && Caravan_PathFollower.IsValidFinalPushDestination(to))
          {
            int ticksToArrive = num8 + num5;
            outTicksToArrive.Add(new VehicleCaravanPathingHelper.TileEstimate(to, ticksToArrive));
            return;
          }
          num1 = num8 + num3;
          num6 = num4;
        }
        else
        {
          num1 += num5;
          num6 -= num5;
          num5 = 0;
        }
      }
      Log.ErrorOnce("Could not calculate estimated ticks to arrive. Too many iterations.", 1837451324);
      outTicksToArrive.Add(new VehicleCaravanPathingHelper.TileEstimate(to, num1));
    }
  }

  private static float GetCostToMove(
    List<VehicleDef> vehicleDefs,
    float initialNextTileCostLeft,
    bool firstInPath,
    int initialTicksAbs,
    int curResult,
    int caravanTicksPerMove,
    int curTile,
    int nextTile)
  {
    if (firstInPath)
      return initialNextTileCostLeft;
    int num = initialTicksAbs + curResult;
    return (float) VehicleCaravan_PathFollower.CostToMove(vehicleDefs, caravanTicksPerMove, PlanetTile.op_Implicit(curTile), PlanetTile.op_Implicit(nextTile), new int?(num));
  }

  private static int EstimatedTicksToArrive(
    PlanetTile destinationTile,
    List<VehicleCaravanPathingHelper.TileEstimate> estimatedTicksToArriveToEvery)
  {
    if (!((PlanetTile) ref destinationTile).Valid)
      return 0;
    foreach (VehicleCaravanPathingHelper.TileEstimate tileEstimate in estimatedTicksToArriveToEvery)
    {
      if (PlanetTile.op_Equality(destinationTile, tileEstimate.tile))
        return tileEstimate.ticksToArrive;
    }
    return 0;
  }

  private readonly struct TileEstimate(PlanetTile tile, int ticksToArrive)
  {
    public readonly PlanetTile tile = tile;
    public readonly int ticksToArrive = ticksToArrive;
  }
}
