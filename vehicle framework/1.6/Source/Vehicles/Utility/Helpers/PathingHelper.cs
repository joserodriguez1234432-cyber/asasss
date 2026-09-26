// Decompiled with JetBrains decompiler
// Type: Vehicles.PathingHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

[StaticConstructorOnStartup]
public static class PathingHelper
{
  private const string AllowTerrainWithTag = "PassableVehicles";
  private const string DisallowTerrainWithTag = "ImpassableVehicles";
  public static readonly Dictionary<ThingDef, List<VehicleDef>> regionEffectors = new Dictionary<ThingDef, List<VehicleDef>>();
  public static readonly Dictionary<string, Dictionary<string, int>> allTerrainCostsByTag = new Dictionary<string, Dictionary<string, int>>();
  internal static AccessTools.FieldRef<RegionAndRoomUpdater, bool> regionAndRoomUpdaterWorking = (AccessTools.FieldRef<RegionAndRoomUpdater, bool>) AccessTools.FieldRefAccess<bool>(typeof (RegionAndRoomUpdater), "working");

  public static bool IsRegionEffector(VehicleDef vehicleDef, ThingDef thingDef)
  {
    List<VehicleDef> vehicleDefList;
    return PathingHelper.regionEffectors.TryGetValue(thingDef, out vehicleDefList) && vehicleDefList.Contains(vehicleDef);
  }

  public static bool ShouldCreateRegions(VehicleDef vehicleDef)
  {
    return !Mathf.Approximately(vehicleDef.GetStatValueAbstract(VehicleStatDefOf.MoveSpeed), 0.0f);
  }

  public static Thing FirstBlockingBuilding(VehiclePawn vehicle, VehiclePath path)
  {
    if (!path.Found)
      return (Thing) null;
    IReadOnlyList<IntVec3> nodes = path.Nodes;
    if (nodes.Count == 1)
      return (Thing) null;
    VehicleDef vehicleDef = vehicle.VehicleDef;
    for (int index = nodes.Count - 2; index >= 0; --index)
    {
      Building edifice = GridsUtility.GetEdifice(nodes[index], ((Thing) vehicle).Map);
      if (edifice != null && (((Thing) edifice).def.IsFence && !vehicleDef.race.CanPassFences || PathingHelper.IsRegionEffector(vehicleDef, ((Thing) edifice).def)))
        return (Thing) edifice;
    }
    return (Thing) null;
  }

  public static bool TryGetStandableCell(VehiclePawn vehicle, IntVec3 cell)
  {
    int num = GenRadial.NumCellsInRadius(2.9f);
    for (int index = 0; index < num; ++index)
    {
      IntVec3 cell1 = IntVec3.op_Addition(GenRadial.RadialPattern[index], cell);
      if (cell1.Standable(vehicle, ((Thing) vehicle).Map) && (!VehicleMod.settings.main.fullVehiclePathing || vehicle.DrivableRectOnCell(cell1)))
        return IntVec3.op_Inequality(cell1, ((Thing) vehicle).Position) && !vehicle.beached;
    }
    return false;
  }

  public static void LoadTerrainDefaults()
  {
    foreach (TerrainDef key in DefDatabase<TerrainDef>.AllDefsListForReading)
    {
      if (key.tags.NotNullAndAny<string>((Predicate<string>) (tag => tag == "PassableVehicles")))
      {
        foreach (VehicleDef vehicleDef in DefDatabase<VehicleDef>.AllDefsListForReading)
          vehicleDef.properties.customTerrainCosts[key] = 1;
      }
      else if (key.tags.NotNullAndAny<string>((Predicate<string>) (tag => tag == "ImpassableVehicles")))
      {
        foreach (VehicleDef vehicleDef in DefDatabase<VehicleDef>.AllDefsListForReading)
          vehicleDef.properties.customTerrainCosts[key] = 10000;
      }
    }
  }

  public static void LoadTerrainTagCosts()
  {
    List<TerrainDef> defsListForReading = DefDatabase<TerrainDef>.AllDefsListForReading;
    foreach (KeyValuePair<string, Dictionary<string, int>> keyValuePair1 in PathingHelper.allTerrainCostsByTag)
    {
      VehicleDef named = DefDatabase<VehicleDef>.GetNamed(keyValuePair1.Key, true);
      foreach (KeyValuePair<string, int> keyValuePair2 in keyValuePair1.Value)
      {
        string terrainTag = keyValuePair2.Key;
        int num = keyValuePair2.Value;
        foreach (TerrainDef key in defsListForReading.Where<TerrainDef>((Func<TerrainDef, bool>) (td => td.tags.NotNullAndAny<string>((Predicate<string>) (tag => tag == terrainTag)))).ToList<TerrainDef>())
          named.properties.customTerrainCosts[key] = num;
      }
    }
  }

  public static void LoadDefModExtensionCosts<T>(
    Func<VehicleDef, Dictionary<T, int>> dictFromVehicle)
    where T : Def
  {
    foreach (T key in DefDatabase<T>.AllDefsListForReading)
    {
      CustomCostDefModExtension modExtension = key.GetModExtension<CustomCostDefModExtension>();
      if (modExtension != null)
      {
        if ((object) key is VehicleDef)
        {
          Debug.Warning($"Attempting to set custom path cost for {key.defName} when vehicles should not be pathing over other vehicles to begin with. Please do not add this DefModExtension to VehicleDefs.");
        }
        else
        {
          List<VehicleDef> vehicleDefList = modExtension.vehicles;
          if (GenList.NullOrEmpty<VehicleDef>((IList<VehicleDef>) vehicleDefList))
            vehicleDefList = DefDatabase<VehicleDef>.AllDefsListForReading;
          foreach (VehicleDef vehicleDef in vehicleDefList)
            dictFromVehicle(vehicleDef)[key] = Mathf.RoundToInt(modExtension.cost);
        }
      }
    }
  }

  public static void LoadDefModExtensionCosts<T>(
    Func<VehicleDef, Dictionary<T, float>> dictFromVehicle)
    where T : Def
  {
    foreach (T key in DefDatabase<T>.AllDefsListForReading)
    {
      CustomCostDefModExtension modExtension = key.GetModExtension<CustomCostDefModExtension>();
      if (modExtension != null)
      {
        if ((object) key is VehicleDef)
        {
          Debug.Warning($"Attempting to set custom path cost for {key.defName} when vehicles should not be pathing over other vehicles to begin with. Please do not add this DefModExtension to VehicleDefs.");
        }
        else
        {
          List<VehicleDef> vehicleDefList = modExtension.vehicles;
          if (GenList.NullOrEmpty<VehicleDef>((IList<VehicleDef>) vehicleDefList))
            vehicleDefList = DefDatabase<VehicleDef>.AllDefsListForReading;
          foreach (VehicleDef vehicleDef in vehicleDefList)
            dictFromVehicle(vehicleDef)[key] = modExtension.cost;
        }
      }
    }
  }

  public static void CacheVehicleRegionEffecters()
  {
    foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefsListForReading)
      PathingHelper.RegisterRegionEffecter(thingDef);
  }

  public static void RegisterRegionEffecter(ThingDef thingDef)
  {
    PathingHelper.regionEffectors[thingDef] = new List<VehicleDef>();
    foreach (VehicleDef moveableVehicleDef in VehicleHarmony.AllMoveableVehicleDefs)
    {
      int num;
      if (moveableVehicleDef.properties.customThingCosts.TryGetValue(thingDef, out num))
      {
        if (num < 0 || num >= 10000)
          PathingHelper.regionEffectors[thingDef].Add(moveableVehicleDef);
      }
      else if (thingDef.AffectsRegions)
        PathingHelper.regionEffectors[thingDef].Add(moveableVehicleDef);
    }
  }

  public static void ThingAffectingRegionsStateChange(Thing thing, Map map, bool spawned)
  {
    List<VehicleDef> vehicleDefs;
    if (!PathingHelper.regionEffectors.TryGetValue(thing.def, out vehicleDefs) || GenList.NullOrEmpty<VehicleDef>((IList<VehicleDef>) vehicleDefs))
      return;
    VehiclePathingSystem component = MapComponentCache<VehiclePathingSystem>.GetComponent(map);
    if (component.ThreadAvailable)
    {
      CellRect cellRect = GenAdj.OccupiedRect(thing);
      AsyncRegionAction action = AsyncPool<AsyncRegionAction>.Get();
      action.Set(component, vehicleDefs, cellRect, spawned);
      component.dedicatedThread.Enqueue((AsyncAction) action);
    }
    else
    {
      CellRect occupiedRect = GenAdj.OccupiedRect(thing);
      if (spawned)
        PathingHelper.ThingInRegionSpawned(occupiedRect, component, vehicleDefs);
      else
        PathingHelper.ThingInRegionDespawned(occupiedRect, component, vehicleDefs);
    }
  }

  internal static void ThingInRegionSpawned(
    CellRect occupiedRect,
    VehiclePathingSystem mapping,
    List<VehicleDef> vehicleDefs)
  {
    foreach (VehicleDef vehicleDef in vehicleDefs)
    {
      mapping[vehicleDef].VehiclePathGrid.RecalculatePerceivedPathCostUnderRect(occupiedRect);
      if (mapping.GridOwners.IsOwner(vehicleDef))
      {
        mapping[vehicleDef].VehicleRegionDirtyer.NotifyThingAffectingRegionsSpawned(occupiedRect);
        mapping[vehicleDef].VehicleReachability.ClearCache();
      }
    }
  }

  internal static void ThingInRegionDespawned(
    CellRect occupiedRect,
    VehiclePathingSystem mapping,
    List<VehicleDef> vehicleDefs)
  {
    foreach (VehicleDef vehicleDef in vehicleDefs)
    {
      mapping[vehicleDef].VehiclePathGrid.RecalculatePerceivedPathCostUnderRect(occupiedRect);
      if (mapping.GridOwners.IsOwner(vehicleDef))
      {
        mapping[vehicleDef].VehicleRegionDirtyer.NotifyThingAffectingRegionsDespawned(occupiedRect);
        mapping[vehicleDef].VehicleReachability.ClearCache();
      }
    }
  }

  public static void ThingAffectingRegionsOrientationChanged(Thing thing, Map map)
  {
    List<VehicleDef> vehicleDefs;
    if (!PathingHelper.regionEffectors.TryGetValue(thing.def, out vehicleDefs) || GenList.NullOrEmpty<VehicleDef>((IList<VehicleDef>) vehicleDefs))
      return;
    VehiclePathingSystem component = MapComponentCache<VehiclePathingSystem>.GetComponent(map);
    if (component.ThreadAvailable)
    {
      AsyncReachabilityCacheAction action = AsyncPool<AsyncReachabilityCacheAction>.Get();
      action.Set(component, vehicleDefs);
      component.dedicatedThread.Enqueue((AsyncAction) action);
    }
    else
      PathingHelper.ThingInRegionOrientationChanged(component, vehicleDefs);
  }

  private static void ThingInRegionOrientationChanged(
    VehiclePathingSystem mapping,
    List<VehicleDef> vehicleDefs)
  {
    foreach (VehicleDef vehicleDef in vehicleDefs)
    {
      if (mapping.GridOwners.IsOwner(vehicleDef))
        mapping[vehicleDef].VehicleReachability.ClearCache();
    }
  }

  public static void RecalculateAllPerceivedPathCosts(Map map)
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      VehiclePathingSystem component = MapComponentCache<VehiclePathingSystem>.GetComponent(map);
      if (!component.GridOwners.AnyOwners)
        return;
      PathingHelper.RecalculateAllPerceivedPathCosts(component);
    }));
  }

  private static void RecalculateAllPerceivedPathCosts(VehiclePathingSystem mapping)
  {
    foreach (IntVec3 allCell in mapping.map.AllCells)
    {
      foreach (VehicleDef allOwner in mapping.GridOwners.AllOwners)
        mapping[allOwner].VehiclePathGrid.RecalculatePerceivedPathCostAt(allCell);
    }
  }

  public static void RecalculatePerceivedPathCostAt(IntVec3 cell, Map map)
  {
    VehiclePathingSystem component = MapComponentCache<VehiclePathingSystem>.GetComponent(map);
    if (!component.GridOwners.AnyOwners)
      return;
    if (component.ThreadAvailable)
    {
      AsyncPathingAction action = AsyncPool<AsyncPathingAction>.Get();
      action.Set(component, cell);
      component.dedicatedThread.Enqueue((AsyncAction) action);
    }
    else
      PathingHelper.RecalculatePerceivedPathCostAtFor(component, cell);
  }

  internal static void RecalculatePerceivedPathCostAtFor(VehiclePathingSystem mapping, IntVec3 cell)
  {
    foreach (VehicleDef moveableVehicleDef in VehicleHarmony.AllMoveableVehicleDefs)
    {
      VehiclePathingSystem.VehiclePathData vehiclePathData = mapping[moveableVehicleDef];
      if (vehiclePathData.VehiclePathGrid.Enabled)
        vehiclePathData.VehiclePathGrid.RecalculatePerceivedPathCostAt(cell);
    }
  }

  public static bool VehicleImpassableInCell(Map map, IntVec3 cell)
  {
    VehiclePawn vehiclePawn = map.GetDetachedMapComponent<VehiclePositionManager>().ClaimedBy(cell);
    if (vehiclePawn != null)
    {
      VehicleDef vehicleDef = vehiclePawn.VehicleDef;
      if (vehicleDef != null)
        return ((BuildableDef) vehicleDef).passability == 2;
    }
    return false;
  }

  public static bool VehicleImpassableInCell(Map map, int x, int z)
  {
    return PathingHelper.VehicleImpassableInCell(map, new IntVec3(x, 0, z));
  }

  public static bool TryFindNearestStandableCell(
    VehiclePawn vehicle,
    IntVec3 cell,
    out IntVec3 result,
    float radius = -1f)
  {
    if ((double) radius < 0.0)
      radius = (float) (Mathf.Min(((BuildableDef) vehicle.VehicleDef).Size.x, ((BuildableDef) vehicle.VehicleDef).Size.z) * 2);
    int num = GenRadial.NumCellsInRadius(radius);
    result = IntVec3.Invalid;
    for (int index = 0; index < num; ++index)
    {
      IntVec3 cell1 = IntVec3.op_Addition(GenRadial.RadialPattern[index], cell);
      if (cell1.Standable(vehicle, ((Thing) vehicle).Map) && (!VehicleMod.settings.main.fullVehiclePathing || vehicle.DrivableRectOnCell(cell1, Ext_Vehicles.DestinationHitboxReq.AnyRotation)))
      {
        if (IntVec3.op_Equality(cell1, ((Thing) vehicle).Position) || vehicle.beached)
        {
          result = cell1;
          return true;
        }
        if (PathingHelper.AnyVehicleBlockingPathAt(cell1, vehicle) == null && vehicle.CanReachVehicle(LocalTargetInfo.op_Implicit(cell1), (PathEndMode) 1, (Danger) 3, (TraverseMode) 0))
        {
          result = cell1;
          return true;
        }
      }
    }
    return false;
  }

  public static float CalculateAngle(this VehiclePawn vehicle)
  {
    if (vehicle == null)
      return 0.0f;
    if (vehicle.vehiclePather.Moving)
    {
      IntVec3 intVec3 = IntVec3.op_Subtraction(vehicle.vehiclePather.nextCell, ((Thing) vehicle).Position);
      vehicle.Angle = intVec3.x <= 0 || intVec3.z <= 0 ? (intVec3.x <= 0 || intVec3.z >= 0 ? (intVec3.x >= 0 || intVec3.z >= 0 ? (intVec3.x >= 0 || intVec3.z <= 0 ? 0.0f : 45f) : -45f) : 45f) : -45f;
    }
    return vehicle.Angle;
  }

  public static VehiclePawn AnyVehicleBlockingPathAt(IntVec3 cell, VehiclePawn vehicle)
  {
    List<Thing> thingList = GridsUtility.GetThingList(cell, ((Thing) vehicle).Map);
    if (GenList.NullOrEmpty<Thing>((IList<Thing>) thingList))
      return (VehiclePawn) null;
    float num = Ext_Map.Distance(((Thing) vehicle).Position, cell);
    foreach (Thing thing in thingList)
    {
      if (thing is VehiclePawn vehiclePawn && vehiclePawn != vehicle && ((double) num < 20.0 || !vehiclePawn.vehiclePather.Moving))
        return vehiclePawn;
    }
    return (VehiclePawn) null;
  }

  public static void ExitMapForVehicle(VehiclePawn vehicle, Job job)
  {
    if (job.failIfCantJoinOrCreateCaravan && !CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow((Pawn) vehicle))
      return;
    VehiclePawn vehicle1 = vehicle;
    CellRect cellRect = CellRect.WholeMap(((Thing) vehicle).Map);
    Rot4 closestEdge = ((CellRect) ref cellRect).GetClosestEdge(((Thing) vehicle).Position);
    PathingHelper.ExitMap(vehicle1, true, closestEdge);
  }

  public static void ExitMap(VehiclePawn vehicle, bool allowedToJoinOrCreateCaravan, Rot4 exitDir)
  {
    if (WorldPawnsUtility.IsWorldPawn((Pawn) vehicle))
    {
      Log.Warning($"Called ExitMap() on world pawn {vehicle}");
    }
    else
    {
      vehicle.Ideo?.Notify_MemberLost((Pawn) vehicle, ((Thing) vehicle).Map);
      if (allowedToJoinOrCreateCaravan && CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow((Pawn) vehicle))
      {
        CaravanExitMapUtility.ExitMapAndJoinOrCreateCaravan((Pawn) vehicle, exitDir);
      }
      else
      {
        LordUtility.GetLord((Pawn) vehicle)?.Notify_PawnLost((Pawn) vehicle, (PawnLostCondition) 6, new DamageInfo?());
        if (vehicle.carryTracker != null && vehicle.carryTracker.CarriedThing != null)
        {
          if (vehicle.carryTracker.CarriedThing is Pawn carriedThing)
          {
            if (((Thing) vehicle).Faction != null && ((Thing) vehicle).Faction != ((Thing) carriedThing).Faction)
            {
              ((Thing) vehicle).Faction.kidnapped.Kidnap(carriedThing, (Pawn) vehicle);
            }
            else
            {
              if (!vehicle.teleporting)
                ((ThingOwner) vehicle.carryTracker.innerContainer).Remove((Thing) carriedThing);
              carriedThing.ExitMap(false, exitDir);
            }
          }
          else
            vehicle.carryTracker.CarriedThing.Destroy((DestroyMode) 0);
          if (!vehicle.teleporting || carriedThing == null)
            ((ThingOwner) vehicle.carryTracker.innerContainer).Clear();
        }
        bool flag = !CaravanUtility.IsCaravanMember((Pawn) vehicle) && !vehicle.teleporting && !PawnUtility.IsTravelingInTransportPodWorldObject((Pawn) vehicle) && (!vehicle.IsPrisoner || ((Thing) vehicle).ParentHolder == null || ((Thing) vehicle).ParentHolder is CompShuttle || vehicle.guest != null && vehicle.guest.Released);
        if (((Thing) vehicle).Faction != null)
          ((Thing) vehicle).Faction.Notify_MemberExitedMap((Pawn) vehicle, flag);
        if (((Thing) vehicle).Faction == Faction.OfPlayer && vehicle.IsSlave && vehicle.SlaveFaction != null && vehicle.SlaveFaction != Faction.OfPlayer && vehicle.guest.Released)
          vehicle.SlaveFaction.Notify_MemberExitedMap((Pawn) vehicle, flag);
        if (((Thing) vehicle).Spawned)
          ((Entity) vehicle).DeSpawn((DestroyMode) 0);
        vehicle.inventory.UnloadEverything = false;
        if (flag)
        {
          vehicle.vehiclePather.StopDead();
          vehicle.jobs.StopAll(false, true);
          vehicle.VerifyReservations((Job) null);
        }
        Find.WorldPawns.PassToWorld((Pawn) vehicle, (PawnDiscardDecideMode) 0);
        foreach (Thing thing in vehicle.inventory.innerContainer)
        {
          if (thing is Pawn pawn && !WorldPawnsUtility.IsWorldPawn(pawn))
            Find.WorldPawns.PassToWorld(pawn, (PawnDiscardDecideMode) 0);
        }
        QuestUtility.SendQuestTargetSignals(((Thing) vehicle).questTags, "LeftMap", NamedArgumentUtility.Named((object) vehicle, "SUBJECT"));
        Find.FactionManager.Notify_PawnLeftMap((Pawn) vehicle);
        Find.IdeoManager.Notify_PawnLeftMap((Pawn) vehicle);
      }
    }
  }
}
