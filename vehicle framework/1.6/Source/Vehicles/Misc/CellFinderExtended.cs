// Decompiled with JetBrains decompiler
// Type: Vehicles.CellFinderExtended
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
using System.Linq;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

[PublicAPI]
public static class CellFinderExtended
{
  private static List<IntVec3> mapEdgeCells;
  private static IntVec3 mapEdgeCellsSize;

  public static bool TryFindRandomEdgeCell(
    Rot4 dir,
    Map map,
    Predicate<IntVec3> validator,
    int offset,
    out IntVec3 result)
  {
    List<IntVec3> list1;
    if (!((Rot4) ref dir).IsValid)
    {
      CellRect cellRect1 = CellRect.WholeMap(map);
      CellRect cellRect2 = ((CellRect) ref cellRect1).ContractedBy(offset);
      list1 = ((CellRect) ref cellRect2).EdgeCells.ToList<IntVec3>();
    }
    else
    {
      CellRect cellRect3 = CellRect.WholeMap(map);
      CellRect cellRect4 = ((CellRect) ref cellRect3).ContractedBy(offset);
      list1 = ((CellRect) ref cellRect4).GetEdgeCells(dir).ToList<IntVec3>();
    }
    List<IntVec3> list2 = list1;
    while (list2.Count > 0)
    {
      IntVec3 intVec3 = list2.PopRandom<IntVec3>();
      if (validator(intVec3))
      {
        result = intVec3;
        return true;
      }
    }
    Log.Warning($"Failed to find edge cell at {((Rot4) ref dir).AsInt}");
    result = CellFinder.RandomEdgeCell(map);
    return false;
  }

  public static bool TryFindRandomCenterCell(
    Map map,
    Predicate<IntVec3> validator,
    out IntVec3 result,
    bool allowRoofed = false)
  {
    Faction hostFaction = map.ParentFaction ?? Faction.OfPlayer;
    List<Thing> list = map.mapPawns.FreeHumanlikesSpawnedOfFaction(hostFaction).Cast<Thing>().ToList<Thing>();
    if (hostFaction == Faction.OfPlayer)
      list.AddRange((IEnumerable<Thing>) map.listerBuildings.allBuildingsColonist);
    else
      list.AddRange(map.listerThings.ThingsInGroup((ThingRequestGroup) 10).Where<Thing>((Func<Thing, bool>) (thing => thing.Faction == hostFaction)));
    float num = 65f;
    for (int index = 0; index < 300; ++index)
    {
      IntVec3 cell;
      CellFinder.TryFindRandomCellNear(map.Center, map, 30, validator, ref cell, -1);
      if (validator(cell) && !GridsUtility.Fogged(cell, map) && (allowRoofed || !Ext_Vehicles.IsRoofed(cell, map)))
      {
        num -= 0.2f;
        bool flag = false;
        foreach (Thing thing in list)
        {
          IntVec3 intVec3 = IntVec3.op_Subtraction(cell, thing.Position);
          if ((double) ((IntVec3) ref intVec3).LengthHorizontalSquared < (double) num * (double) num)
          {
            flag = true;
            break;
          }
        }
        if (!flag && map.reachability.CanReachFactionBase(cell, hostFaction))
        {
          result = cell;
          return true;
        }
      }
    }
    result = IntVec3.Invalid;
    return false;
  }

  public static IntVec3 MiddleEdgeCell(Rot4 dir, Map map, Pawn pawn, Predicate<IntVec3> validator)
  {
    CellRect cellRect1 = CellRect.WholeMap(map);
    List<IntVec3> list = ((CellRect) ref cellRect1).GetEdgeCells(dir).ToList<IntVec3>();
    bool flag = Rot4.op_Inequality(Find.World.CoastDirectionAt(map.Tile), dir) && !GenList.NullOrEmpty<SurfaceTile.RiverLink>((IList<SurfaceTile.RiverLink>) Find.WorldGrid[map.Tile.tileId].Rivers);
    int extraOffset = ((Thing) pawn).def.size.z / 2 > 4 ? ((Thing) pawn).def.size.z / 2 + 1 : 4;
    int num = list.Count / 2;
label_10:
    for (int index = 0; index < 10000; ++index)
    {
      IntVec3 map1 = pawn.ClampToMap(CellFinder.RandomEdgeCell(dir, map), map, extraOffset);
      CellRect cellRect2 = pawn.PawnOccupiedCells(map1, ((Rot4) ref dir).Opposite);
      using (CellRect.Enumerator enumerator = ((CellRect) ref cellRect2).GetEnumerator())
      {
        IntVec3 current;
        do
        {
          do
          {
            if (((CellRect.Enumerator) ref enumerator).MoveNext())
            {
              current = ((CellRect.Enumerator) ref enumerator).Current;
              if (!validator(current))
                goto label_10;
            }
            else
              goto label_8;
          }
          while (!flag);
        }
        while (RiverSpawnValidator(current));
        continue;
      }
label_8:
      return map1;
    }
    Log.Warning("Running secondary spawn cell check for boats");
    for (int index = 0; list.Count > 0 && index < list.Count / 2; ++index)
    {
      if (index > list.Count)
      {
        Log.Warning("List of Cells almost went out of bounds. Report to Boats mod author - Smash Phil");
        break;
      }
      IntVec3 map2 = pawn.ClampToMap(list[num + index], map, extraOffset);
      CellRect cellRect3 = pawn.PawnOccupiedCells(map2, ((Rot4) ref dir).Opposite);
      using (CellRect.Enumerator enumerator = ((CellRect) ref cellRect3).GetEnumerator())
      {
        IntVec3 current;
        do
        {
          if (((CellRect.Enumerator) ref enumerator).MoveNext())
            current = ((CellRect.Enumerator) ref enumerator).Current;
          else
            goto label_19;
        }
        while (validator(current));
        goto label_20;
      }
label_19:
      return map2;
label_20:
      IntVec3 map3 = pawn.ClampToMap(list[num - index], map, extraOffset);
      CellRect cellRect4 = pawn.PawnOccupiedCells(map3, ((Rot4) ref dir).Opposite);
      using (CellRect.Enumerator enumerator = ((CellRect) ref cellRect4).GetEnumerator())
      {
        IntVec3 current;
        do
        {
          if (((CellRect.Enumerator) ref enumerator).MoveNext())
            current = ((CellRect.Enumerator) ref enumerator).Current;
          else
            goto label_25;
        }
        while (validator(current));
        continue;
      }
label_25:
      return map3;
    }
    Log.Error("Could not find valid edge cell to spawn boats on. This could be due to the Boat being too large to spawn on the coast of a Mountainous Map.");
    return pawn.ClampToMap(CellFinder.RandomEdgeCell(dir, map), map, extraOffset);

    bool RiverSpawnValidator(IntVec3 x) => map.terrainGrid.TerrainAt(x).IsRiver;
  }

  public static bool TryFindRandomReachableCellNear(
    IntVec3 root,
    Map map,
    VehicleDef vehicleDef,
    float radius,
    TraverseParms traverseParms,
    Predicate<IntVec3> extraValidator,
    Predicate<VehicleRegion> regionValidator,
    out IntVec3 result,
    int maxRegions = 999999)
  {
    // ISSUE: unable to decompile the method.
  }

  public static bool TryFindRandomCellInRegion(
    this VehicleRegion region,
    Predicate<IntVec3> validator,
    out IntVec3 result)
  {
    for (int index = 0; index < 10; ++index)
    {
      result = region.RandomCell;
      if (validator == null || validator(result))
        return true;
    }
    List<IntVec3> list;
    using (GlobalObjectPool.Get<IntVec3>(out list))
    {
      list.AddRange(region.Cells);
      GenList.Shuffle<IntVec3>((IList<IntVec3>) list);
      foreach (IntVec3 intVec3 in list)
      {
        if (validator == null || validator(intVec3))
        {
          result = intVec3;
          return true;
        }
      }
      result = region.RandomCell;
      return false;
    }
  }

  public static IntVec3 RandomClosewalkCellNear(
    IntVec3 root,
    Map map,
    VehicleDef vehicleDef,
    int radius,
    Predicate<IntVec3> validator = null)
  {
    IntVec3 result;
    return CellFinderExtended.TryRandomClosewalkCellNear(root, map, vehicleDef, radius, out result, validator) ? result : root;
  }

  public static bool TryRandomClosewalkCellNear(
    IntVec3 root,
    Map map,
    VehicleDef vehicleDef,
    int radius,
    out IntVec3 result,
    Predicate<IntVec3> validator = null)
  {
    return CellFinderExtended.TryFindRandomReachableCellNear(root, map, vehicleDef, (float) radius, TraverseParms.For((TraverseMode) 2, (Danger) 3, false, false, false, true, false), validator, (Predicate<VehicleRegion>) null, out result);
  }

  public static IntVec3 RandomSpawnCellForPawnNear(
    IntVec3 root,
    Map map,
    Pawn pawn,
    Predicate<IntVec3> validator,
    bool waterEntry = false,
    int firstTryWithRadius = 4)
  {
    VehiclePawn vehicle = pawn as VehiclePawn;
    if (vehicle != null)
    {
      if (validator(root) && GridsUtility.GetFirstPawn(root, map) == null && vehicle.CellRectStandable(map, new IntVec3?(root)))
        return root;
      int radius1 = firstTryWithRadius;
      IntVec3 result;
      for (int index = 0; index < 3; ++index)
      {
        if (waterEntry)
        {
          if (CellFinderExtended.TryFindRandomReachableCellNear(root, map, vehicle.VehicleDef, (float) radius1, TraverseParms.For((TraverseMode) 2, (Danger) 3, false, false, false, true, false), (Predicate<IntVec3>) (cell => validator(cell) && vehicle.CellRectStandable(map, new IntVec3?(cell)) && (GridsUtility.Fogged(root, map) || !GridsUtility.Fogged(cell, map)) && GridsUtility.GetFirstPawn(cell, map) == null), (Predicate<VehicleRegion>) null, out result))
            return result;
        }
        else if (CellFinder.TryFindRandomReachableNearbyCell(root, map, (float) radius1, TraverseParms.For((TraverseMode) 2, (Danger) 3, false, false, false, true, false), (Predicate<IntVec3>) (cell => validator(cell) && vehicle.CellRectStandable(map, new IntVec3?(cell)) && (GridsUtility.Fogged(root, map) || !GridsUtility.Fogged(cell, map)) && GridsUtility.GetFirstPawn(cell, map) == null), (Predicate<Region>) null, ref result, 999999))
          return result;
        radius1 *= 2;
      }
      for (int radius2 = firstTryWithRadius + 1; !CellFinderExtended.TryRandomClosewalkCellNear(root, map, vehicle.VehicleDef, radius2, out result); radius2 *= 2)
      {
        if (radius2 > map.Size.x / 2 && radius2 > map.Size.z / 2)
          return root;
      }
      return result;
    }
    IntVec3 intVec3;
    return CellFinder.TryFindRandomSpawnCellForPawnNear(root, map, ref intVec3, firstTryWithRadius, validator) ? intVec3 : root;
  }

  public static bool TryFindRandomEdgeCellWith(
    Predicate<IntVec3> validator,
    Map map,
    Rot4 exitDir,
    VehicleDef largestVehicleDef,
    float roadChance,
    out IntVec3 result)
  {
    result = IntVec3.Invalid;
    if (Rand.Chance(roadChance))
    {
      CellFinderExtended.CacheAndShuffleMapEdgeCells(map);
      Area_Road areaRoad = map.areaManager.Get<Area_Road>();
      foreach (IntVec3 mapEdgeCell in CellFinderExtended.mapEdgeCells)
      {
        IntVec3 intVec3 = mapEdgeCell.PadForHitbox(map, largestVehicleDef);
        if (areaRoad[intVec3] && validator(intVec3))
        {
          result = intVec3;
          return true;
        }
      }
      foreach (IntVec3 roadEdgeTile in map.roadInfo.roadEdgeTiles)
      {
        IntVec3 intVec3 = roadEdgeTile.PadForHitbox(map, largestVehicleDef);
        if (validator(intVec3))
        {
          result = intVec3;
          return true;
        }
      }
    }
    for (int index = 0; index < 100; ++index)
    {
      result = CellFinder.RandomEdgeCell(map).PadForHitbox(map, largestVehicleDef);
      if (validator(result))
        return true;
    }
    CellFinderExtended.CacheAndShuffleMapEdgeCells(map);
    foreach (IntVec3 mapEdgeCell in CellFinderExtended.mapEdgeCells)
    {
      try
      {
        if (validator(mapEdgeCell))
        {
          result = mapEdgeCell;
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error($"CellFinderExtended.TryFindRandomEdgeCellWith threw exception while validating {mapEdgeCell}. Exception={ex}");
      }
    }
    result = IntVec3.Invalid;
    return false;
  }

  public static bool TryFindRandomExitSpot(
    VehiclePawn vehicle,
    out IntVec3 dest,
    TraverseMode mode = 0)
  {
    Map map = ((Thing) vehicle).Map;
    VehiclePathingSystem.VehiclePathData pathData = map.GetCachedMapComponent<VehiclePathingSystem>()[vehicle.VehicleDef];
    Danger maxDanger = (Danger) 2;
    for (int index = 0; index < 40; ++index)
    {
      if (index > 15)
        maxDanger = (Danger) 3;
      IntVec3 cell1 = CellFinder.RandomCell(map);
      Rot4 random = Rot4.Random;
      switch (((Rot4) ref random).AsInt)
      {
        case 0:
          cell1.x = 0;
          break;
        case 1:
          cell1.x = map.Size.x - 1;
          break;
        case 2:
          cell1.z = 0;
          break;
        case 3:
          cell1.z = map.Size.z - 1;
          break;
      }
      IntVec3 cell2 = cell1.PadForHitbox(map, vehicle.VehicleDef);
      if (Validator(cell2))
      {
        dest = cell2;
        return true;
      }
    }
    dest = ((Thing) vehicle).Position;
    return false;

    bool Validator(IntVec3 cell)
    {
      IntVec3 intVec3 = cell.PadForHitbox(map, vehicle);
      return pathData.VehicleReachability.CanReachVehicle(((Thing) vehicle).Position, LocalTargetInfo.op_Implicit(intVec3), (PathEndMode) 1, mode, maxDanger);
    }
  }

  public static bool TryFindBestExitSpot(VehiclePawn vehicle, out IntVec3 cell, TraverseMode mode = 0)
  {
    cell = IntVec3.Invalid;
    Map map = ((Thing) vehicle).Map;
    VehiclePathingSystem.VehiclePathData pathData = map.GetCachedMapComponent<VehiclePathingSystem>()[vehicle.VehicleDef];
    if (!pathData.VehicleReachability.CanReachMapEdge(((Thing) vehicle).Position, TraverseParms.For((Pawn) vehicle, (Danger) 3, (TraverseMode) 0, false, false, false, true)))
      return false;
    int num1 = 0;
    for (int index = 0; index < 100; ++index)
    {
      num1 += 4;
      IntVec3 intVec3;
      if (CellFinder.TryFindRandomCellNear(((Thing) vehicle).Position, map, num1, (Predicate<IntVec3>) null, ref intVec3, -1))
      {
        int num2 = intVec3.x;
        cell = new IntVec3(0, 0, intVec3.z).PadForHitbox(map, vehicle.VehicleDef);
        if (((Thing) vehicle).Map.Size.z - intVec3.z < num2)
        {
          num2 = map.Size.z - intVec3.z;
          cell = new IntVec3(intVec3.x, 0, map.Size.z - 1).PadForHitbox(map, vehicle.VehicleDef);
        }
        if (map.Size.x - intVec3.x < num2)
        {
          num2 = map.Size.x - intVec3.x;
          cell = new IntVec3(map.Size.x - 1, 0, intVec3.z).PadForHitbox(map, vehicle.VehicleDef);
        }
        if (intVec3.z < num2)
          cell = new IntVec3(intVec3.x, 0, 0).PadForHitbox(map, vehicle.VehicleDef);
        if (cell.Standable(vehicle, map) && pathData.VehicleReachability.CanReachVehicle(((Thing) vehicle).Position, LocalTargetInfo.op_Implicit(cell), (PathEndMode) 1, mode, (Danger) 3))
          return true;
      }
    }
    for (int index = 0; index < 4; ++index)
    {
      if (CellFinderExtended.TryFindRandomEdgeCellWith(new Predicate<IntVec3>(Validator), map, new Rot4(index), vehicle.VehicleDef, 0.0f, out cell))
        return true;
    }
    cell = IntVec3.Invalid;
    return false;

    bool Validator(IntVec3 cell)
    {
      return pathData.VehicleReachability.CanReachVehicle(((Thing) vehicle).Position, LocalTargetInfo.op_Implicit(cell.PadForHitbox(map, vehicle)), (PathEndMode) 1, TraverseParms.For((Pawn) vehicle, (Danger) 3, (TraverseMode) 0, false, false, false, true));
    }
  }

  public static bool TryRadialSearchForCell(
    IntVec3 cell,
    Map map,
    float radius,
    Predicate<IntVec3> validator,
    out IntVec3 result)
  {
    result = IntVec3.Invalid;
    int num = GenRadial.NumCellsInRadius(radius);
    for (int index = 0; index < num; ++index)
    {
      IntVec3 intVec3 = IntVec3.op_Addition(GenRadial.RadialPattern[index], cell);
      if (GenGrid.InBounds(intVec3, map) && validator(intVec3))
      {
        result = intVec3;
        return true;
      }
    }
    return false;
  }

  private static void CacheAndShuffleMapEdgeCells(Map map)
  {
    if (GenList.NullOrEmpty<IntVec3>((IList<IntVec3>) CellFinderExtended.mapEdgeCells) || IntVec3.op_Inequality(map.Size, CellFinderExtended.mapEdgeCellsSize))
    {
      CellFinderExtended.mapEdgeCellsSize = map.Size;
      CellRect cellRect = CellRect.WholeMap(map);
      CellFinderExtended.mapEdgeCells = ((CellRect) ref cellRect).EdgeCells.ToList<IntVec3>();
    }
    GenList.Shuffle<IntVec3>((IList<IntVec3>) CellFinderExtended.mapEdgeCells);
  }
}
