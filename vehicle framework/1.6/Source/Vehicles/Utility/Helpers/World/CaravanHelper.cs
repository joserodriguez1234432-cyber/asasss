// Decompiled with JetBrains decompiler
// Type: Vehicles.CaravanHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Vehicles.World;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

[PublicAPI]
public static class CaravanHelper
{
  private static readonly HashSet<PlanetTile> AvailableExitTiles = new HashSet<PlanetTile>();
  private static readonly List<PlanetTile> NeighborTiles = new List<PlanetTile>();
  public static VehicleAssignment assignedSeats = new VehicleAssignment();
  private static int pawnsBeingAdded;

  public static bool AbleToEmbark(List<Pawn> pawns)
  {
    return CaravanHelper.HasEnoughSpacePawns(pawns) && CaravanHelper.HasEnoughPawnsToEmbark(pawns);
  }

  public static bool AbleToEmbark(Caravan caravan)
  {
    List<Pawn> pawns = new List<Pawn>();
    foreach (Pawn pawn in caravan.PawnsListForReading)
    {
      if (pawn is VehiclePawn vehiclePawn)
        pawns.AddRange((IEnumerable<Pawn>) vehiclePawn.AllPawnsAboard);
      pawns.Add(pawn);
    }
    return CaravanHelper.AbleToEmbark(pawns);
  }

  public static PlanetTile BestExitTileToGoTo(
    List<VehicleDef> vehicleDefs,
    PlanetTile destinationTile,
    Map from)
  {
    PlanetTile planetTile1 = PlanetTile.op_Implicit(-1);
    using (WorldPath path = Find.World.GetComponent<WorldVehiclePathfinder>().FindPath(from.Tile, destinationTile, vehicleDefs))
    {
      if (path.Found && path.NodesLeftCount >= 2)
      {
        List<PlanetTile> nodesReversed = path.NodesReversed;
        planetTile1 = nodesReversed[nodesReversed.Count - 2];
      }
      if (!((PlanetTile) ref planetTile1).Valid)
        return CaravanHelper.RandomBestExitTileFrom(vehicleDefs, from);
      float num1 = 0.0f;
      PlanetTile planetTile2 = PlanetTile.Invalid;
      foreach (PlanetTile planetTile3 in CaravanHelper.AvailableExitTilesAt(vehicleDefs, from))
      {
        if (PlanetTile.op_Equality(planetTile3, planetTile1))
          return planetTile3;
        float num2 = GenGeo.MagnitudeHorizontalSquared(Vector3.op_Subtraction(Find.WorldGrid.GetTileCenter(planetTile3), Find.WorldGrid.GetTileCenter(planetTile1)));
        if (!((PlanetTile) ref planetTile2).Valid || (double) num2 < (double) num1)
        {
          planetTile2 = planetTile3;
          num1 = num2;
        }
      }
      return planetTile2;
    }
  }

  public static PlanetTile RandomBestExitTileFrom(List<VehicleDef> vehicleDefs, Map map)
  {
    Tile tileInfo = map.TileInfo;
    List<PlanetTile> options = CaravanHelper.AvailableExitTilesAt(vehicleDefs, map);
    if (GenList.NullOrEmpty<PlanetTile>((IList<PlanetTile>) options))
      return PlanetTile.Invalid;
    if (!(tileInfo is SurfaceTile surfaceTile))
      return GenCollection.RandomElement<PlanetTile>((IEnumerable<PlanetTile>) options);
    List<SurfaceTile.RoadLink> roads = surfaceTile.Roads;
    int bestRoadIndex = -1;
    for (int index = 0; index < roads.Count; ++index)
    {
      SurfaceTile.RoadLink roadLink = roads[index];
      if (options.Contains(roadLink.neighbor) && (bestRoadIndex == -1 || roadLink.road.priority > roads[bestRoadIndex].road.priority))
        bestRoadIndex = index;
    }
    return bestRoadIndex == -1 ? GenCollection.RandomElement<PlanetTile>((IEnumerable<PlanetTile>) options) : GenCollection.RandomElement<SurfaceTile.RoadLink>(roads.Where<SurfaceTile.RoadLink>((Func<SurfaceTile.RoadLink, bool>) (roadLink => options.Contains(roadLink.neighbor) && roadLink.road == roads[bestRoadIndex].road))).neighbor;
  }

  public static List<PlanetTile> AvailableExitTilesAt(List<VehicleDef> vehicleDefs, Map map)
  {
    try
    {
      PlanetTile currentTileID = map.Tile;
      WorldGrid grid = Find.World.grid;
      grid.GetTileNeighbors(currentTileID, CaravanHelper.NeighborTiles);
      VehicleDef largestVehicleDef = GenCollection.MaxBy<VehicleDef, int>((IEnumerable<VehicleDef>) vehicleDefs, (Func<VehicleDef, int>) (vehicleDef => ((BuildableDef) vehicleDef).Size.z));
      foreach (PlanetTile neighborTile in CaravanHelper.NeighborTiles)
      {
        PlanetTile tile = neighborTile;
        if (!vehicleDefs.Exists((Predicate<VehicleDef>) (vehicleDef => !Find.World.GetComponent<WorldVehiclePathGrid>().Passable(tile, vehicleDef))))
        {
          Rot4 exitDir1;
          Rot4 exitDir2;
          CaravanExitMapUtility.GetExitMapEdges(grid, currentTileID, tile, ref exitDir1, ref exitDir2);
          IntVec3 result;
          if (!Rot4.op_Equality(exitDir1, Rot4.Invalid) && CellFinderExtended.TryFindRandomEdgeCellWith(new Predicate<IntVec3>(CellValidator), map, exitDir1, largestVehicleDef, CellFinder.EdgeRoadChance_Ignore, out result) || !Rot4.op_Equality(exitDir2, Rot4.Invalid) && CellFinderExtended.TryFindRandomEdgeCellWith(new Predicate<IntVec3>(CellValidator), map, exitDir2, largestVehicleDef, CellFinder.EdgeRoadChance_Ignore, out result))
            CaravanHelper.AvailableExitTiles.Add(tile);
        }
      }
      List<PlanetTile> list = CaravanHelper.AvailableExitTiles.ToList<PlanetTile>();
      GenCollection.SortBy<PlanetTile, float>(list, (Func<PlanetTile, float>) (tile => grid.GetHeadingFromTo(currentTileID, tile)));
      return list;
    }
    finally
    {
      CaravanHelper.AvailableExitTiles.Clear();
      CaravanHelper.NeighborTiles.Clear();
    }

    bool CellValidator(IntVec3 cell)
    {
      foreach (VehicleDef vehicleDef in vehicleDefs)
      {
        if (!cell.Walkable(vehicleDef, map) || GridsUtility.Fogged(cell, map))
          return false;
      }
      return true;
    }
  }

  private static bool HasEnoughSpacePawns(List<Pawn> pawns)
  {
    int num = 0;
    foreach (Pawn pawn in pawns)
    {
      if (pawn is VehiclePawn vehiclePawn)
        num += vehiclePawn.TotalSeats;
    }
    return GenCollection.Count<Pawn>(pawns, (Predicate<Pawn>) (pawn => !(pawn is VehiclePawn))) <= num;
  }

  private static bool HasEnoughPawnsToEmbark(List<Pawn> pawns)
  {
    int num = 0;
    foreach (Pawn pawn in pawns)
    {
      if (pawn is VehiclePawn vehiclePawn)
        num += vehiclePawn.PawnCountToOperate;
    }
    return GenCollection.Count<Pawn>(pawns, (Predicate<Pawn>) (pawn => !(pawn is VehiclePawn))) >= num;
  }

  public static IEnumerable<Pawn> AllSendablePawnsInVehicles(
    Map map,
    bool allowEvenIfDowned = false,
    bool allowEvenIfInMentalState = false,
    bool allowEvenIfPrisonerNotSecure = false,
    bool allowCapturableDownedPawns = false,
    bool allowLodgers = false)
  {
    foreach (Pawn pawn in (IEnumerable<Pawn>) map.mapPawns.AllPawnsSpawned)
    {
      if (pawn is VehiclePawn vehiclePawn && ((Thing) vehiclePawn).Faction == Faction.OfPlayer)
      {
        List<Pawn>.Enumerator enumerator = vehiclePawn.AllPawnsAboard.GetEnumerator();
        while (enumerator.MoveNext())
        {
          Pawn current = enumerator.Current;
          bool flag1 = allowEvenIfDowned || !current.Downed;
          bool flag2 = allowEvenIfInMentalState || !current.InMentalState;
          bool flag3 = ((Thing) current).Faction == Faction.OfPlayer || current.IsPrisonerOfColony || allowCapturableDownedPawns && current.Downed && !current.mindState.WillJoinColonyIfRescued && CaravanUtility.ShouldAutoCapture(current, Faction.OfPlayer);
          bool flag4 = !QuestUtility.IsQuestLodger(current) | allowLodgers;
          bool flag5 = allowEvenIfPrisonerNotSecure || !current.IsPrisoner || current.guest.PrisonerIsSecure;
          bool flag6 = LordUtility.GetLord(current) == null || LordUtility.GetLord(current).LordJob is LordJob_VoluntarilyJoinable || LordUtility.GetLord(current).LordJob.IsCaravanSendable;
          if (((!(flag1 & flag2 & flag3) || !current.RaceProps.allowedOnCaravan ? 0 : (!QuestUtility.IsQuestHelper(current) ? 1 : 0)) & (flag4 ? 1 : 0) & (flag5 ? 1 : 0) & (flag6 ? 1 : 0)) != 0)
            yield return current;
        }
        enumerator = new List<Pawn>.Enumerator();
      }
    }
  }

  public static void StashVehicles(VehicleCaravan caravan)
  {
    Find.WindowStack.Add((Window) new Dialog_StashVehicle(caravan));
  }

  public static void BoardAllAssignedPawns()
  {
    foreach (AssignedSeat assignedSeat in CaravanHelper.assignedSeats.AllAssignments.Values)
      assignedSeat.Vehicle.TryAddPawn(assignedSeat.pawn, assignedSeat.handler);
    CaravanHelper.assignedSeats.Clear();
  }

  public static bool CanStartCaravan([NotNull] List<Pawn> pawns)
  {
    int num1 = 0;
    int num2 = 0;
    int num3 = 0;
    foreach (Pawn pawn in pawns)
    {
      if (pawn is VehiclePawn vehiclePawn)
      {
        num1 += vehiclePawn.SeatsAvailable;
        num3 += vehiclePawn.PawnCountToOperate - vehiclePawn.PawnsByHandlingType[HandlingType.Movement].Count;
      }
      else if (pawn.IsColonistPlayerControlled && !pawn.Downed && !pawn.Dead)
        ++num2;
    }
    bool flag1 = pawns.Exists((Predicate<Pawn>) (pawn => ((Thing) pawn).IsBoat())) && num2 > num1;
    bool flag2 = num2 < num3;
    if (flag1)
      Messages.Message(TaggedString.op_Implicit(Translator.Translate("VF_CaravanMustHaveEnoughSpaceOnShip")), MessageTypeDefOf.RejectInput, false);
    if (flag2)
      Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_CaravanMustHaveEnoughPawnsToOperate", NamedArgument.op_Implicit(num3))), MessageTypeDefOf.RejectInput, false);
    return !flag1 && !flag2;
  }

  public static bool IsFormingCaravanShipHelper(Pawn pawn)
  {
    Lord lord = LordUtility.GetLord(pawn);
    return lord != null && lord.LordJob is LordJob_FormAndSendVehicles;
  }

  public static VehicleCaravan ExitMapAndCreateVehicleCaravan(
    IEnumerable<Pawn> pawns,
    Faction faction,
    PlanetTile exitFromTile,
    PlanetTile directionTile,
    PlanetTile destinationTile,
    bool sendMessage = true)
  {
    if (!GenWorldClosest.TryFindClosestPassableTile(exitFromTile, ref exitFromTile))
    {
      Log.Error("Could not find any passable tile for a new caravan.");
      return (VehicleCaravan) null;
    }
    if (Find.World.Impassable(directionTile))
      directionTile = exitFromTile;
    List<Pawn> pawnList = new List<Pawn>();
    Map map = (Map) null;
    foreach (Pawn pawn in pawns)
    {
      if (!pawn.InVehicle())
        pawnList.Add(pawn);
      CaravanHelper.AddVehicleCaravanExitTaleIfShould(pawn);
      if (map == null)
        map = ((Thing) pawn).MapHeld;
    }
    VehicleCaravan vehicleCaravan = CaravanHelper.MakeVehicleCaravan((IEnumerable<Pawn>) pawnList, faction, exitFromTile, false);
    Rot4 rot4 = map != null ? Find.WorldGrid.GetRotFromTo(exitFromTile, directionTile) : Rot4.Invalid;
    foreach (Pawn pawn in pawnList)
      pawn.ExitMap(false, rot4);
    foreach (Pawn pawn in vehicleCaravan.pawns)
    {
      if (!WorldPawnsUtility.IsWorldPawn(pawn))
        Find.WorldPawns.PassToWorld(pawn, (PawnDiscardDecideMode) 0);
    }
    if (map != null)
    {
      map.Parent.Notify_CaravanFormed((Caravan) vehicleCaravan);
      map.retainedCaravanData.Notify_CaravanFormed((Caravan) vehicleCaravan);
    }
    if (!vehicleCaravan.vehiclePather.Moving && PlanetTile.op_Inequality(((WorldObject) vehicleCaravan).Tile, directionTile))
    {
      vehicleCaravan.vehiclePather.StartPath(directionTile, (CaravanArrivalAction) null, true);
      vehicleCaravan.vehiclePather.nextTileCostLeft /= 2f;
      vehicleCaravan.vehicleTweener.ResetTweenedPosToRoot();
    }
    if (((PlanetTile) ref destinationTile).Valid)
    {
      List<FloatMenuOption> floatMenuOptionList = FloatMenuMakerWorld.ChoicesAtFor(destinationTile, (Caravan) vehicleCaravan);
      if (floatMenuOptionList.NotNullAndAny<FloatMenuOption>((Predicate<FloatMenuOption>) (floatOpt => !floatOpt.Disabled)))
        floatMenuOptionList.First<FloatMenuOption>((Func<FloatMenuOption, bool>) (floatOpt => !floatOpt.Disabled)).action();
      else
        vehicleCaravan.vehiclePather.StartPath(destinationTile, (CaravanArrivalAction) null, true);
    }
    if (sendMessage)
    {
      TaggedString taggedString1 = TranslatorFormattedStringExtensions.Translate("MessageFormedCaravan", NamedArgument.op_Implicit(vehicleCaravan.Name));
      TaggedString taggedString2 = ((TaggedString) ref taggedString1).CapitalizeFirst();
      if (vehicleCaravan.vehiclePather.Moving && vehicleCaravan.vehiclePather.ArrivalAction != null)
        taggedString2 = TaggedString.op_Addition(taggedString2, TaggedString.op_Addition(TaggedString.op_Addition(TaggedString.op_Addition(TaggedString.op_Addition(" ", Translator.Translate("MessageFormedCaravan_Orders")), ": "), vehicleCaravan.vehiclePather.ArrivalAction.Label), "."));
      Messages.Message(TaggedString.op_Implicit(taggedString2), LookTargets.op_Implicit((WorldObject) vehicleCaravan), MessageTypeDefOf.TaskCompletion, true);
    }
    return vehicleCaravan;
  }

  public static bool OpportunistcallyCreatedAerialVehicle(VehiclePawn vehicle, int tile)
  {
    if (Find.World.GetComponent<WorldVehiclePathGrid>().Passable(PlanetTile.op_Implicit(tile), vehicle.VehicleDef) || vehicle.GetCachedComp<CompVehicleLauncher>() == null)
      return false;
    AerialVehicleInFlight.Create(vehicle, PlanetTile.op_Implicit(tile));
    if (((Thing) vehicle).Spawned)
    {
      vehicle.jobs.StopAll(false, true);
      ((Entity) vehicle).DeSpawn((DestroyMode) 0);
    }
    return true;
  }

  public static PlanetTile FindRandomStartingTileBasedOnExitDir(
    VehiclePawn vehicle,
    int tileID,
    Rot4 exitDir)
  {
    List<PlanetTile> planetTileList = new List<PlanetTile>();
    List<PlanetTile> source = new List<PlanetTile>();
    WorldVehiclePathGrid vehiclePathGrid = WorldVehiclePathGrid.Instance;
    Find.WorldGrid.GetTileNeighbors(PlanetTile.op_Implicit(tileID), source);
    foreach (PlanetTile tile in source)
    {
      if (vehiclePathGrid.Passable(tile, vehicle.VehicleDef) && (!((Rot4) ref exitDir).IsValid || !Rot4.op_Inequality(Find.WorldGrid.GetRotFromTo(PlanetTile.op_Implicit(tileID), tile), exitDir)))
        planetTileList.Add(tile);
    }
    PlanetTile planetTile;
    return GenCollection.TryRandomElement<PlanetTile>((IEnumerable<PlanetTile>) planetTileList, ref planetTile) || GenCollection.TryRandomElement<PlanetTile>(source.Where<PlanetTile>((Func<PlanetTile, bool>) (pt =>
    {
      if (!vehiclePathGrid.Passable(pt, vehicle.VehicleDef))
        return false;
      Rot4 rotFromTo = Find.WorldGrid.GetRotFromTo(PlanetTile.op_Implicit(tileID), pt);
      if ((Rot4.op_Equality(exitDir, Rot4.North) || Rot4.op_Equality(exitDir, Rot4.South)) && (Rot4.op_Equality(rotFromTo, Rot4.East) || Rot4.op_Equality(rotFromTo, Rot4.West)))
        return true;
      if (!Rot4.op_Equality(exitDir, Rot4.East) && !Rot4.op_Equality(exitDir, Rot4.West))
        return false;
      return Rot4.op_Equality(rotFromTo, Rot4.North) || Rot4.op_Equality(rotFromTo, Rot4.South);
    })), ref planetTile) || GenCollection.TryRandomElement<PlanetTile>(source.Where<PlanetTile>((Func<PlanetTile, bool>) (tile => vehiclePathGrid.Passable(tile, vehicle.VehicleDef))), ref planetTile) ? planetTile : PlanetTile.op_Implicit(tileID);
  }

  public static VehicleCaravan SwapToVehicleCaravan(Caravan caravan)
  {
    if (!GenCollection.ContainsAny<Pawn>((IList<Pawn>) caravan.PawnsListForReading, new Func<Pawn, bool>(IsVehicle)))
    {
      Trace.Fail("Unable to swap to VehicleCaravan. Caravan has no vehicles in pawn list.");
      return (VehicleCaravan) null;
    }
    VehiclePawn vehiclePawn1 = (VehiclePawn) null;
    List<Pawn> pawns = new List<Pawn>();
    foreach (Pawn pawn in caravan.pawns.InnerListForReading)
    {
      if (pawn is VehiclePawn vehiclePawn2)
        vehiclePawn1 = vehiclePawn2;
      else
        pawns.Add(pawn);
    }
    PlanetTile tile = ((WorldObject) caravan).Tile;
    caravan.RemoveAllPawns();
    ((WorldObject) caravan).Destroy();
    RoleHelper.Distribute(new List<VehiclePawn>(1)
    {
      vehiclePawn1
    }, pawns);
    Pawn pawn1 = (Pawn) vehiclePawn1;
    List<Pawn> pawnList = pawns;
    int index1 = 0;
    Pawn[] items = new Pawn[1 + pawnList.Count];
    items[index1] = pawn1;
    int index2 = index1 + 1;
    foreach (Pawn pawn2 in pawnList)
    {
      items[index2] = pawn2;
      ++index2;
    }
    // ISSUE: object of a compiler-generated type is created
    return CaravanHelper.MakeVehicleCaravan((IEnumerable<Pawn>) new \u003C\u003Ez__ReadOnlyArray<Pawn>(items), Faction.OfPlayer, tile, true);

    static bool IsVehicle(Pawn pawn) => pawn is VehiclePawn;
  }

  [MustUseReturnValue]
  public static VehicleCaravan MakeVehicleCaravan(
    IEnumerable<Pawn> pawns,
    Faction faction,
    PlanetTile startingTile,
    bool addToWorldPawnsIfNotAlready)
  {
    if (!((PlanetTile) ref startingTile).Valid & addToWorldPawnsIfNotAlready)
      Log.Warning("Tried to create a caravan but chose not to spawn a caravan but pass pawns to world. This can cause bugs because pawns can be discarded.");
    VehicleCaravan caravan1 = (VehicleCaravan) WorldObjectMaker.MakeWorldObject(WorldObjectDefOfVehicles.VehicleCaravan);
    if (((PlanetTile) ref startingTile).Valid)
      ((WorldObject) caravan1).Tile = startingTile;
    ((WorldObject) caravan1).SetFaction(faction);
    if (((PlanetTile) ref startingTile).Valid)
      Find.WorldObjects.Add((WorldObject) caravan1);
    foreach (Pawn filterOutPassenger in VehicleFilter.FilterOutPassengers(pawns))
    {
      if (filterOutPassenger.Dead)
      {
        Trace.Fail($"Tried to form caravan with dead pawn {filterOutPassenger}. Removing...");
      }
      else
      {
        filterOutPassenger.GetVehicle()?.RemovePawn(filterOutPassenger);
        Caravan caravan2 = CaravanUtility.GetCaravan((Thing) filterOutPassenger);
        if (caravan2 != null)
          caravan1.TransferPawnOrItem((ThingOwner) caravan2.pawns, (Thing) filterOutPassenger);
        else
          caravan1.AddPawn(filterOutPassenger, addToWorldPawnsIfNotAlready);
        if (addToWorldPawnsIfNotAlready && !WorldPawnsUtility.IsWorldPawn(filterOutPassenger))
          Find.WorldPawns.PassToWorld(filterOutPassenger, (PawnDiscardDecideMode) 0);
      }
    }
    caravan1.Name = CaravanNameGenerator.GenerateCaravanName((Caravan) caravan1);
    caravan1.SetUniqueId(Find.UniqueIDsManager.GetNextCaravanID());
    caravan1.PostInit();
    return caravan1;
  }

  public static List<Pawn> GrabPawnsFromMapPawnsInVehicle(List<Pawn> pawns)
  {
    List<VehiclePawn> list = pawns.Where<Pawn>((Func<Pawn, bool>) (x => ((Thing) x).Faction == Faction.OfPlayer && x is VehiclePawn)).Cast<VehiclePawn>().ToList<VehiclePawn>();
    return list.Count == 0 ? pawns.Where<Pawn>((Func<Pawn, bool>) (x => ((Thing) x).Faction == Faction.OfPlayer && x.RaceProps.Humanlike)).ToList<Pawn>() : GenCollection.RandomElement<VehiclePawn>((IEnumerable<VehiclePawn>) list).AllCapablePawns;
  }

  public static float CapacityLeft(LordJob_FormAndSendVehicles lordJob)
  {
    float num1 = CollectionsMassCalculator.MassUsageTransferables(lordJob.transferables, (IgnorePawnsInventoryMode) 1, false, false);
    List<ThingCount> thingCounts = new List<ThingCount>();
    foreach (Pawn ownedPawn in ((LordJob) lordJob).lord.ownedPawns)
      thingCounts.Add(new ThingCount((Thing) ownedPawn, ((Thing) ownedPawn).stackCount, false));
    float num2 = num1 + CollectionsMassCalculator.MassUsage(thingCounts, (IgnorePawnsInventoryMode) 1, false, false);
    float num3 = CaravanInfoHelper.Capacity(thingCounts);
    thingCounts.Clear();
    return num3 - num2;
  }

  public static VehiclePawn UsableVehicleWithTheMostFreeSpace(Pawn pawn)
  {
    IEnumerable<Pawn> pawns = !CaravanFormingUtility.IsFormingCaravan(pawn) ? (IEnumerable<Pawn>) ((Thing) pawn).Map.mapPawns.SpawnedPawnsInFaction(((Thing) pawn).Faction) : (IEnumerable<Pawn>) LordUtility.GetLord(pawn).ownedPawns;
    VehiclePawn vehiclePawn1 = (VehiclePawn) null;
    float num1 = 0.0f;
    foreach (Pawn pawn1 in pawns)
    {
      if (pawn1 is VehiclePawn vehiclePawn2 && vehiclePawn2 != pawn && (pawn.InVehicleCaravan() || ReachabilityUtility.CanReach(pawn, LocalTargetInfo.op_Implicit((Thing) vehiclePawn2), (PathEndMode) 2, (Danger) 3, false, false, (TraverseMode) 0)))
      {
        float num2 = MassUtility.FreeSpace(pawn1);
        if (vehiclePawn1 == null || (double) num2 > (double) num1)
        {
          vehiclePawn1 = vehiclePawn2;
          num1 = num2;
        }
      }
    }
    return vehiclePawn1;
  }

  public static void CountPawnsBeingTraded(List<Tradeable> ___cachedTradeables)
  {
    int num = 0;
    foreach (Tradeable cachedTradeable in ___cachedTradeables)
    {
      if (((Transferable) cachedTradeable).AnyThing is Pawn anyThing && anyThing.RaceProps.Humanlike)
      {
        if (cachedTradeable.ActionToDo == 1)
          num += ((Transferable) cachedTradeable).CountToTransfer;
        else if (cachedTradeable.ActionToDo == 2)
          num -= ((Transferable) cachedTradeable).CountToTransfer;
      }
    }
    CaravanHelper.pawnsBeingAdded = num;
  }

  public static bool CanFitInVehicle(AerialVehicleInFlight aerialVehicle)
  {
    if (TradeSession.Active)
      return aerialVehicle.Vehicle.SeatsAvailable - CaravanHelper.pawnsBeingAdded > 0;
    Log.Warning("Improper use of CanFitInVehicle which should only operate during TradeSessions.");
    return true;
  }

  public static void DoItemsListForVehicle(
    Rect inRect,
    ref float curY,
    ref List<Thing> tmpSingleThing,
    ITab_Pawn_FormingCaravan instance)
  {
    LordJob_FormAndSendVehicles lordJob = (LordJob_FormAndSendVehicles) LordUtility.GetLord(Find.Selector.SingleSelectedThing as Pawn).LordJob;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(0.0f, curY, (float) (((double) ((Rect) ref inRect).width - 10.0) / 2.0), ((Rect) ref inRect).height);
    float num1 = 0.0f;
    Widgets.BeginGroup(rect1);
    Widgets.ListSeparator(ref num1, ((Rect) ref rect1).width, TaggedString.op_Implicit(Translator.Translate("ItemsToLoad")));
    bool flag1 = false;
    foreach (TransferableOneWay transferable in lordJob.transferables)
    {
      if (((Transferable) transferable).CountToTransfer > 0 && ((Transferable) transferable).HasAnyThing)
      {
        flag1 = true;
        MethodInfo methodInfo = AccessTools.Method(typeof (ITab_Pawn_FormingCaravan), "DoThingRow", (System.Type[]) null, (System.Type[]) null);
        object[] parameters = new object[5]
        {
          (object) ((Transferable) transferable).ThingDef,
          (object) ((Transferable) transferable).CountToTransfer,
          (object) transferable.things,
          (object) ((Rect) ref rect1).width,
          (object) num1
        };
        methodInfo.Invoke((object) instance, parameters);
        num1 = (float) parameters[4];
      }
    }
    if (!flag1)
      Widgets.NoneLabel(ref num1, ((Rect) ref rect1).width, (string) null);
    Widgets.EndGroup();
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector((float) (((double) ((Rect) ref inRect).width + 10.0) / 2.0), curY, (float) (((double) ((Rect) ref inRect).width - 10.0) / 2.0), ((Rect) ref inRect).height);
    float num2 = 0.0f;
    Widgets.BeginGroup(rect2);
    Widgets.ListSeparator(ref num2, ((Rect) ref rect2).width, TaggedString.op_Implicit(Translator.Translate("LoadedItems")));
    bool flag2 = false;
    foreach (Pawn ownedPawn in ((LordJob) lordJob).lord.ownedPawns)
    {
      if (!ownedPawn.inventory.UnloadEverything)
      {
        foreach (Thing thing in ownedPawn.inventory.innerContainer)
        {
          flag2 = true;
          tmpSingleThing.Clear();
          tmpSingleThing.Add(thing);
          MethodInfo methodInfo = AccessTools.Method(typeof (ITab_Pawn_FormingCaravan), "DoThingRow", (System.Type[]) null, (System.Type[]) null);
          object[] parameters = new object[5]
          {
            (object) thing.def,
            (object) thing.stackCount,
            (object) tmpSingleThing,
            (object) ((Rect) ref rect2).width,
            (object) num2
          };
          methodInfo.Invoke((object) instance, parameters);
          num2 = (float) parameters[4];
        }
      }
    }
    if (!flag2)
      Widgets.NoneLabel(ref num2, ((Rect) ref rect1).width, (string) null);
    Widgets.EndGroup();
    curY += Mathf.Max(num1, num2);
  }

  public static void AddVehicleCaravanExitTaleIfShould(Pawn pawn)
  {
    Pawn pawn1 = pawn;
    if (pawn is VehiclePawn vehiclePawn)
      pawn1 = GenCollection.FirstOrFallback<Pawn>((IEnumerable<Pawn>) vehiclePawn.AllPawnsAboard, pawn);
    if (!((Thing) pawn1).Spawned || !pawn1.IsFreeColonist)
      return;
    if (((Thing) pawn1).Map.IsPlayerHome)
    {
      TaleRecorder.RecordTale(TaleDefOf.CaravanFormed, new object[1]
      {
        (object) pawn1
      });
    }
    else
    {
      if (!GenHostility.AnyHostileActiveThreatToPlayer(((Thing) pawn1).Map, false, false))
        return;
      TaleRecorder.RecordTale(TaleDefOf.CaravanFled, new object[1]
      {
        (object) pawn1
      });
    }
  }

  public static Caravan FindCaravanToJoinForAllowingVehicles(Pawn pawn)
  {
    if (((Thing) pawn).Faction != Faction.OfPlayer && pawn.HostFaction != Faction.OfPlayer)
      return (Caravan) null;
    if (!((Thing) pawn).Spawned)
      return (Caravan) null;
    if (pawn is VehiclePawn vehicle1)
    {
      if (!vehicle1.CanReachVehicleMapEdge())
        return (Caravan) null;
    }
    else if (!ReachabilityUtility.CanReachMapEdge(pawn))
      return (Caravan) null;
    List<PlanetTile> planetTileList = new List<PlanetTile>();
    PlanetTile tile = ((Thing) pawn).Map.Tile;
    Find.WorldGrid.GetTileNeighbors(tile, planetTileList);
    planetTileList.Add(tile);
    foreach (Caravan caravan in Find.WorldObjects.Caravans)
    {
      if (planetTileList.Contains(((WorldObject) caravan).Tile) && caravan.autoJoinable)
      {
        if (pawn is VehiclePawn vehicle2 && caravan is VehicleCaravan vehicleCaravan && !vehicleCaravan.ViableForCaravan(vehicle2))
          return (Caravan) null;
        if (pawn.HostFaction == null)
        {
          if (((WorldObject) caravan).Faction == ((Thing) pawn).Faction)
            return caravan;
        }
        else if (((WorldObject) caravan).Faction == pawn.HostFaction)
          return caravan;
      }
    }
    return (Caravan) null;
  }

  public static AerialVehicleInFlight FindAerialVehicleToJoinForAllowingVehicles(Pawn pawn)
  {
    if (((Thing) pawn).Faction != Faction.OfPlayer && pawn.HostFaction != Faction.OfPlayer)
      return (AerialVehicleInFlight) null;
    if (!((Thing) pawn).Spawned)
      return (AerialVehicleInFlight) null;
    if (pawn is VehiclePawn)
      return (AerialVehicleInFlight) null;
    if (!ReachabilityUtility.CanReachMapEdge(pawn))
      return (AerialVehicleInFlight) null;
    foreach (AerialVehicleInFlight allowingVehicles in Find.World.GetComponent<VehicleWorldObjectsHolder>().AerialVehicles.Where<AerialVehicleInFlight>((Func<AerialVehicleInFlight, bool>) (aerialVehicle => PlanetTile.op_Equality(aerialVehicle.Tile, ((Thing) pawn).Map.Tile))).ToList<AerialVehicleInFlight>())
    {
      if (pawn.HostFaction == null && allowingVehicles.Faction == ((Thing) pawn).Faction || allowingVehicles.Faction == pawn.HostFaction)
        return allowingVehicles;
    }
    return (AerialVehicleInFlight) null;
  }
}
