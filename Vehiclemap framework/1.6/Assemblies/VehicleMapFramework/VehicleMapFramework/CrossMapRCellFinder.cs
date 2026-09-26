// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CrossMapRCellFinder
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public static class CrossMapRCellFinder
{
  public static IntVec3 BestOrderedGotoDestNear(
    IntVec3 root,
    Pawn searcher,
    Predicate<IntVec3> cellValidator,
    bool reachable,
    Map map)
  {
    if (map == null)
      return IntVec3.Invalid;
    if (IsGoodDest(root))
      return root;
    int index = 1;
    IntVec3 intVec3 = new IntVec3();
    float num1 = -1000f;
    bool flag = false;
    int num2 = GenRadial.NumCellsInRadius(30f);
    do
    {
      IntVec3 c = IntVec3.op_Addition(root, GenRadial.RadialPattern[index]);
      if (IsGoodDest(c))
      {
        float num3 = CoverUtility.TotalSurroundingCoverScore(c, map);
        if ((double) num3 > (double) num1)
        {
          num1 = num3;
          intVec3 = c;
          flag = true;
        }
      }
      if (index >= 8 & flag)
        return intVec3;
      ++index;
    }
    while (index < num2);
    return ((Thing) searcher).Position;

    bool IsGoodDest(IntVec3 c)
    {
      if (!CrossMapRCellFinder.IsGoodDestinationFor(c, searcher, map, false) || cellValidator != null && !cellValidator(c) || !map.pawnDestinationReservationManager.CanReserve(c, searcher, true) || reachable && !searcher.CanReach(LocalTargetInfo.op_Implicit(c), (PathEndMode) 1, (Danger) 3, false, false, (TraverseMode) 0, map))
        return false;
      List<Thing> thingList = GridsUtility.GetThingList(c, map);
      for (int index = 0; index < thingList.Count; ++index)
      {
        if (thingList[index] is Pawn pawn && pawn != searcher && pawn.RaceProps.Humanlike && (((Thing) searcher).Faction == Faction.OfPlayer && ((Thing) pawn).Faction == ((Thing) searcher).Faction || ((Thing) searcher).Faction != Faction.OfPlayer && ((Thing) pawn).Faction != Faction.OfPlayer))
          return false;
      }
      return true;
    }
  }

  public static IntVec3 GoodDestNearFromTo(
    IntVec3 from,
    IntVec3 to,
    Pawn searcher,
    Map map,
    Predicate<IntVec3> cellValidator = null,
    bool reachable = true,
    bool reserve = true,
    float radius = 30f)
  {
    if (map == null)
      return IntVec3.Invalid;
    if (IsGoodDest(to))
      return to;
    int index = 1;
    int num = GenRadial.NumCellsInRadius(radius);
    do
    {
      IntVec3 c = IntVec3.op_Addition(to, GenRadial.RadialPattern[index]);
      if (IsGoodDest(c))
        return c;
      ++index;
    }
    while (index < num);
    return IntVec3.Invalid;

    bool IsGoodDest(IntVec3 c)
    {
      if (!CrossMapRCellFinder.IsGoodDestinationFor(c, searcher, map, false) || cellValidator != null && !cellValidator(c) || reserve && !map.pawnDestinationReservationManager.CanReserve(c, searcher, true) || reachable && !map.reachability.CanReach(from, LocalTargetInfo.op_Implicit(c), (PathEndMode) 1, (TraverseMode) 0, (Danger) 3))
        return false;
      List<Thing> thingList = GridsUtility.GetThingList(c, map);
      for (int index = 0; index < thingList.Count; ++index)
      {
        if (thingList[index] is Pawn pawn && pawn != searcher && pawn.RaceProps.Humanlike && (((Thing) searcher).Faction == Faction.OfPlayer && ((Thing) pawn).Faction == ((Thing) searcher).Faction || ((Thing) searcher).Faction != Faction.OfPlayer && ((Thing) pawn).Faction != Faction.OfPlayer))
          return false;
      }
      return true;
    }
  }

  private static bool IsGoodDestination(IntVec3 c, Map map, bool careAboutDanger)
  {
    if (!GenGrid.Standable(c, map))
      return false;
    return !careAboutDanger || !GridsUtility.GetTerrain(c, map).dangerous;
  }

  private static bool IsGoodDestinationFor(IntVec3 c, Pawn pawn, Map map, bool careAboutDanger)
  {
    if (!CrossMapRCellFinder.IsGoodDestination(c, map, careAboutDanger) || !GenGrid.WalkableBy(c, map, pawn))
      return false;
    if (!GenGrid.Standable(c, map))
    {
      Building_Door door = GridsUtility.GetDoor(c, map);
      if (door == null || !door.CanPhysicallyPass(pawn))
        return false;
    }
    if (ForbidUtility.IsForbidden(c, pawn) || careAboutDanger && DangerUtility.GetDangerFor(c, pawn, map) == 3 || careAboutDanger && PawnUtility.KnownDangerAt(c, map, pawn))
      return false;
    return !careAboutDanger || !VacuumConcernTo(c);

    bool VacuumConcernTo(IntVec3 cell)
    {
      return pawn.ConcernedByVacuum && (double) VacuumUtility.GetVacuum(cell, map) >= 0.5;
    }
  }

  public static bool TryFindGoodAdjacentSpotToTouch(
    Pawn toucher,
    Thing touchee,
    out IntVec3 result)
  {
    IntVec3 intVec3_1 = IntVec3.Invalid;
    int num = int.MaxValue;
    Map map = touchee.MapHeld ?? ((Thing) toucher).Map;
    IntVec3 intVec3_2 = ((Thing) toucher).PositionOnAnotherThingMap(touchee);
    foreach (IntVec3 c in GenAdj.CellsAdjacent8Way(touchee))
    {
      if (CrossMapRCellFinder.IsGoodDestinationFor(c, toucher, map, true) && toucher.CanReach(LocalTargetInfo.op_Implicit(c), (PathEndMode) 1, (Danger) 3, false, false, (TraverseMode) 0, map) && ReachabilityImmediate.CanReachImmediate(c, LocalTargetInfo.op_Implicit(touchee), ((Thing) toucher).Map, (PathEndMode) 2, toucher))
      {
        if (IntVec3.op_Equality(intVec3_2, c) && map == ((Thing) toucher).Map)
        {
          intVec3_1 = c;
          break;
        }
        int squared = IntVec3Utility.DistanceToSquared(intVec3_2, c);
        if (squared < num || GridsUtility.GetTerrain(intVec3_1, map).avoidWander && !GridsUtility.GetTerrain(c, map).avoidWander || GridsUtility.GetFirstThing<Building_Trap>(intVec3_1, map) != null && GridsUtility.GetFirstThing<Building_Trap>(c, map) == null)
        {
          num = squared;
          intVec3_1 = c;
        }
      }
    }
    if (((IntVec3) ref intVec3_1).IsValid)
    {
      result = intVec3_1;
      return true;
    }
    foreach (IntVec3 intVec3_3 in GenCollection.InRandomOrder<IntVec3>(GenAdj.CellsAdjacent8Way(touchee), (IList<IntVec3>) null))
    {
      if (GenGrid.WalkableBy(intVec3_3, map, toucher) && toucher.CanReach(LocalTargetInfo.op_Implicit(intVec3_3), (PathEndMode) 1, (Danger) 3, false, false, (TraverseMode) 0, map))
      {
        result = intVec3_3;
        return true;
      }
    }
    result = touchee.Position;
    return false;
  }
}
