// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.GravshipVehicleUtility
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public static class GravshipVehicleUtility
{
  public static bool placingGravshipVehicle;
  private static readonly Func<IntVec3, Map, AcceptanceReport> IsValidCell = (Func<IntVec3, Map, AcceptanceReport>) AccessTools.Method(typeof (Designator_MoveGravship), nameof (IsValidCell), (Type[]) null, (Type[]) null).CreateDelegate(typeof (Func<IntVec3, Map, AcceptanceReport>));
  private static readonly Action<Def, Type, HashSet<ushort>> GiveShortHash = (Action<Def, Type, HashSet<ushort>>) AccessTools.Method(typeof (ShortHashGiver), nameof (GiveShortHash), (Type[]) null, (Type[]) null).CreateDelegate(typeof (Action<Def, Type, HashSet<ushort>>));
  private static readonly Dictionary<Type, HashSet<ushort>> takenHashesPerDeftype = AccessTools.StaticFieldRefAccess<Dictionary<Type, HashSet<ushort>>>(typeof (ShortHashGiver), nameof (takenHashesPerDeftype));
  private static readonly Func<WorldComponent_GravshipController, Building_GravEngine, Gravship> RemoveGravshipFromMap = (Func<WorldComponent_GravshipController, Building_GravEngine, Gravship>) AccessTools.Method(typeof (WorldComponent_GravshipController), nameof (RemoveGravshipFromMap), (Type[]) null, (Type[]) null).CreateDelegate(typeof (Func<WorldComponent_GravshipController, Building_GravEngine, Gravship>));
  public static readonly Func<WorldComponent_GravshipController, Gravship, IntVec3, Map, Building_GravEngine> PlaceGravship = (Func<WorldComponent_GravshipController, Gravship, IntVec3, Map, Building_GravEngine>) AccessTools.Method(typeof (WorldComponent_GravshipController), nameof (PlaceGravship), (Type[]) null, (Type[]) null).CreateDelegate(typeof (Func<WorldComponent_GravshipController, Gravship, IntVec3, Map, Building_GravEngine>));

  public static bool GravshipProcessInProgress
  {
    get
    {
      return GravshipUtility.generatingGravship || GravshipPlacementUtility.placingGravship || GravshipVehicleUtility.placingGravshipVehicle;
    }
  }

  public static void PlaceGravshipVehicleUnSpawned(
    Building_GravEngine engine,
    IntVec3 loc,
    Rot4 rot,
    VehiclePawnWithMap vehicle,
    bool forced = false)
  {
    if (!ModsConfig.OdysseyActive || GravshipVehicleUtility.GravshipProcessInProgress)
      return;
    bool spawned = ((Thing) engine).Spawned;
    bool destroyed = ((Thing) engine).Destroyed;
    MinifiedThing spawnedParentOrMe = ((Thing) engine).SpawnedParentOrMe as MinifiedThing;
    if (!spawned)
    {
      ((Thing) engine).ForceSetStateToUnspawned();
      ((Thing) engine).stackCount = 1;
      GenSpawn.Spawn((Thing) engine, loc, vehicle.VehicleMap, rot, (WipeMode) 0, false, false);
    }
    GravshipVehicleUtility.PlaceGravshipVehicle(engine, vehicle, forced);
    if (destroyed)
      ((Thing) engine).Destroy((DestroyMode) 0);
    else if (!spawned)
      ((Entity) engine).DeSpawn((DestroyMode) 0);
    if (spawnedParentOrMe == null)
      return;
    spawnedParentOrMe.InnerThing = (Thing) engine;
  }

  public static AcceptanceReport PlaceGravshipVehicle(
    Building_GravEngine engine,
    VehiclePawnWithMap vehicle,
    bool forced = false)
  {
    if (!ModsConfig.OdysseyActive || GravshipVehicleUtility.GravshipProcessInProgress)
      return AcceptanceReport.op_Implicit(false);
    if (engine == null)
      return AcceptanceReport.op_Implicit(Translator.Translate("CannotLaunchNoEngine"));
    Rot8 fullRotation = vehicle.FullRotation;
    if (((Rot8) ref fullRotation).IsDiagonal && !forced)
      return AcceptanceReport.op_Implicit(TranslatorFormattedStringExtensions.Translate("VMF_CannotSetDownDiagonal", NamedArgument.op_Implicit(((Entity) vehicle).LabelCap)));
    GravshipVehicleUtility.placingGravshipVehicle = true;
    Gravship gravship1 = Current.Game.Gravship;
    try
    {
      Map map = ((Thing) vehicle).Map;
      foreach (IntVec3 original in engine.AllConnectedSubstructure)
      {
        AcceptanceReport acceptanceReport = GravshipVehicleUtility.IsValidCell(original.ToBaseMapCoord(vehicle), map);
        if (!((AcceptanceReport) ref acceptanceReport).Accepted)
        {
          if (!forced)
            return acceptanceReport;
          TerrainGrid terrainGrid = vehicle.VehicleMap.terrainGrid;
          if (terrainGrid.CanRemoveFoundationAt(original))
            terrainGrid.RemoveFoundation(original, true);
          CollectionExtensions.DoIf<IntVec3>(GridsUtility.GetThingList(original, vehicle.VehicleMap).SelectMany<Thing, IntVec3>((Func<Thing, IEnumerable<IntVec3>>) (t => (IEnumerable<IntVec3>) (object) GenAdj.OccupiedRect(t))).Distinct<IntVec3>(), new Func<IntVec3, bool>(terrainGrid.CanRemoveFoundationAt), (Action<IntVec3>) (intVec3 => terrainGrid.RemoveFoundation(intVec3, true)));
        }
      }
      vehicle.DisembarkAll();
      Rot4 rotation = ((Thing) vehicle).Rotation;
      List<(IntVec3, float, float)> list = vehicle.VehicleMap.regionGrid.AllRooms.Where<Room>((Func<Room, bool>) (r => !r.ExposedToSpace && r.AnyPassable)).Select<Room, (IntVec3, float, float)>((Func<Room, (IntVec3, float, float)>) (r => (r.Cells.FirstOrDefault<IntVec3>().ToBaseMapCoord(vehicle), r.Temperature, r.Vacuum))).ToList<(IntVec3, float, float)>();
      Gravship gravship2 = GravshipVehicleUtility.RemoveGravshipFromMap((WorldComponent_GravshipController) null, engine);
      gravship2.Rotation = rotation;
      IntVec3 baseMapCoord = gravship2.originalPosition.ToBaseMapCoord(vehicle);
      Building_GravEngine buildingGravEngine = GravshipVehicleUtility.PlaceGravship((WorldComponent_GravshipController) null, gravship2, baseMapCoord, map);
      if (!((Thing) vehicle).Destroyed)
        ((Thing) vehicle).Destroy((DestroyMode) 0);
      foreach ((IntVec3, float, float) tuple in list)
      {
        Room room = GridsUtility.GetRoom(tuple.Item1, map);
        if (room != null && !room.ExposedToSpace && room.AnyPassable)
        {
          room.Temperature = tuple.Item2;
          room.Vacuum = tuple.Item3;
        }
      }
    }
    finally
    {
      Current.Game.Gravship = gravship1;
      GravshipVehicleUtility.placingGravshipVehicle = false;
    }
    return AcceptanceReport.op_Implicit(true);
  }

  public static AcceptanceReport GenerateGravshipVehicle(
    Building_GravEngine engine,
    VehicleDef baseDef,
    bool checkStability = true)
  {
    if (!ModsConfig.OdysseyActive || GravshipVehicleUtility.GravshipProcessInProgress)
      return AcceptanceReport.op_Implicit(false);
    if (engine == null || !((Thing) engine).Spawned)
      return AcceptanceReport.op_Implicit(Translator.Translate("CannotLaunchNoEngine"));
    Map map = ((Thing) engine).Map;
    CompGravshipFacility gravshipFacility = GenCollection.FirstOrDefault<CompGravshipFacility>(engine.GravshipComponents, (Predicate<CompGravshipFacility>) (c => c is CompPilotConsole));
    if (gravshipFacility == null || !((CompFacility) gravshipFacility).CanBeActive)
      return AcceptanceReport.op_Implicit(Translator.Translate("PilotConsoleInaccessible"));
    Rot4 rotation = ((Thing) ((ThingComp) gravshipFacility).parent).Rotation;
    Rot4 rotCounter = ((Rot4) ref rotation).IsHorizontal ? ((Rot4) ref rotation).Opposite : rotation;
    CellRect wheelsRect;
    AcceptanceReport acceptanceReport = GravshipVehicleUtility.CheckGravshipVehicleStability(engine, rotation, out wheelsRect);
    if (checkStability && !((AcceptanceReport) ref acceptanceReport).Accepted)
      return AcceptanceReport.op_Implicit(((AcceptanceReport) ref acceptanceReport).Reason);
    HashSet<IntVec3> cells = engine.ValidSubstructure;
    if (!((IEnumerable<IntVec3>) (object) GenAdj.OccupiedRect((Thing) engine)).All<IntVec3>(new Func<IntVec3, bool>(cells.Contains)))
      return AcceptanceReport.op_Implicit(Translator.Translate("CannotLaunchNoEngine"));
    VehiclePawnWithMap vehiclePawnWithMap;
    if (cells.Any<IntVec3>((Func<IntVec3, bool>) (c => GridsUtility.TryGetFirstThing<VehiclePawnWithMap>(c, map, ref vehiclePawnWithMap))))
      return AcceptanceReport.op_Implicit(Translator.Translate("VMF_ContainsMapVehicle"));
    Thing thing = (Thing) null;
    if (cells.Any<IntVec3>((Func<IntVec3, bool>) (c => GenCollection.Any<Thing>(GridsUtility.GetThingList(c, map), (Predicate<Thing>) (t =>
    {
      List<PlaceWorker> placeWorkers = ((BuildableDef) (thing = t).def).PlaceWorkers;
      return placeWorkers != null && GenCollection.Any<PlaceWorker>(placeWorkers, (Predicate<PlaceWorker>) (p => p is PlaceWorker_ForbidOnVehicle));
    })))))
      return AcceptanceReport.op_Implicit(TranslatorFormattedStringExtensions.Translate("VMF_ContainsForbidOnVehicle", NamedArgument.op_Implicit(thing?.LabelCapNoCount)));
    CellRect first = CellRect.FromCellList((IEnumerable<IntVec3>) cells);
    if (CellRect.op_Inequality(wheelsRect, CellRect.Empty))
      first = ((CellRect) ref first).Encapsulate(wheelsRect);
    IEnumerable<IntVec3> source = ((IEnumerable<IntVec3>) (object) first).Except<IntVec3>((IEnumerable<IntVec3>) cells);
    VehiclePathGrid pathGrid = ComponentCache.GetCachedMapComponent<VehiclePathingSystem>(map)[baseDef].VehiclePathGrid;
    Func<IntVec3, bool> predicate = (Func<IntVec3, bool>) (c => !pathGrid.WalkableFast(c));
    List<IntVec3> list1 = source.Where<IntVec3>(predicate).ToList<IntVec3>();
    if (GenCollection.Any<IntVec3>(list1))
    {
      CollectionExtensions.Do<IntVec3>((IEnumerable<IntVec3>) list1, (Action<IntVec3>) (c => map.debugDrawer.FlashCell(c, 0.5f, (string) null, 50)));
      return AcceptanceReport.op_Implicit(Translator.Translate("VMF_RectContainsImpassable"));
    }
    IntVec3 min = ((CellRect) ref first).GetCorner(((Rot4) ref rotation).Opposite, true);
    VehicleMapProps_Gravship mapPropsGravship = new VehicleMapProps_Gravship();
    mapPropsGravship.baseDef = baseDef;
    IntVec2 intVec2;
    if (!((Rot4) ref rotation).IsHorizontal)
    {
      intVec2 = ((CellRect) ref first).Size;
    }
    else
    {
      IntVec2 size = ((CellRect) ref first).Size;
      intVec2 = ((IntVec2) ref size).Rotated();
    }
    mapPropsGravship.size = intVec2;
    mapPropsGravship.offset = new Vector3(0.0f, 0.0f, 0.25f);
    mapPropsGravship.outOfBoundsCells = ((IEnumerable<IntVec3>) (object) first).Except<IntVec3>((IEnumerable<IntVec3>) cells).Select<IntVec3, IntVec2>((Func<IntVec3, IntVec2>) (c =>
    {
      IntVec3 intVec3 = IntVec3Utility.RotatedBy(IntVec3.op_Subtraction(c, min), rotCounter);
      return ((IntVec3) ref intVec3).ToIntVec2;
    })).ToList<IntVec2>();
    VehicleMapProps_Gravship props = mapPropsGravship;
    Gravship gravship1 = Current.Game.Gravship;
    try
    {
      VehiclePawnWithMap vehiclePawn1 = (VehiclePawnWithMap) VehicleSpawner.GenerateVehicle(GravshipVehicleUtility.GenerateGravshipVehicleDef(props), Faction.OfPlayer);
      if (vehiclePawn1?.VehicleMap == null)
        return AcceptanceReport.op_Implicit(false);
      List<(IntVec3, float, float)> list2 = map.regionGrid.AllRooms.Where<Room>((Func<Room, bool>) (r => r.Cells.Any<IntVec3>(new Func<IntVec3, bool>(cells.Contains)))).Where<Room>((Func<Room, bool>) (r => !r.ExposedToSpace && r.AnyPassable)).Select<Room, (IntVec3, float, float)>((Func<Room, (IntVec3, float, float)>) (r => (r.Cells.FirstOrDefault<IntVec3>(), r.Temperature, r.Vacuum))).ToList<(IntVec3, float, float)>();
      Gravship gravship2;
      try
      {
        gravship2 = GravshipVehicleUtility.RemoveGravshipFromMap(Find.World.GetComponent<WorldComponent_GravshipController>(), engine);
      }
      catch (Exception ex)
      {
        VMF_Log.Error($"Error while generating gravship.\n{ex}");
        GravshipUtility.generatingGravship = false;
        return AcceptanceReport.WasRejected;
      }
      if (ModCompat.CompatBase<ModCompat.MultiFloors>.Active)
        ModCompat.MultiFloors.RevalidateLaunchSiteState(map);
      Thing thing1 = (Thing) null;
      try
      {
        thing1 = GenSpawn.Spawn((Thing) vehiclePawn1, ((CellRect) ref first).CenterCell, map, rotation, (WipeMode) 0, false, false);
      }
      catch (Exception ex)
      {
        VMF_Log.Error("Error while spawning gravship vehicle.\n" + ex.Message);
      }
      if (thing1 == null)
      {
        Building_GravEngine buildingGravEngine1 = GravshipVehicleUtility.PlaceGravship((WorldComponent_GravshipController) null, gravship2, gravship2.originalPosition, map);
      }
      gravship2.Rotation = rotCounter;
      IntVec3 intVec3_1 = IntVec3.op_Subtraction(gravship2.originalPosition, min);
      LongEventHandler.ExecuteWhenFinished((Action) (() =>
      {
        TransformData transformData;
        // ISSUE: explicit constructor call
        ((TransformData) ref transformData).\u002Ector(((Thing) vehiclePawn1).DrawPos, vehiclePawn1.FullRotation, vehiclePawn1.Transform.rotation);
        vehiclePawn1.cachedDrawPos = ((Graphic_Rgb) vehiclePawn1.VehicleGraphic).ParallelGetPreRenderResults(ref transformData, false, (Thing) vehiclePawn1, 0.0f).position;
      }));
      FrameDelay.DelayOne<(Gravship, IntVec3, Rot4, VehiclePawnWithMap)>((Action<(Gravship, IntVec3, Rot4, VehiclePawnWithMap)>) (state =>
      {
        Building_GravEngine buildingGravEngine2 = GravshipVehicleUtility.PlaceGravship((WorldComponent_GravshipController) null, state.gravship, IntVec3.op_Addition(IntVec3Utility.RotatedBy(state.minOffset, state.rotCounter), IntVec3.NorthEast), state.vehiclePawn.VehicleMap);
        FrameDelay.DelayOne<VehiclePawnWithMap>((Action<VehiclePawnWithMap>) (vehiclePawn =>
        {
          ((ThingComp) vehiclePawn.CompFueledTravel)?.CompTick();
          vehiclePawn.VehicleMap.mapDrawer.RegenerateLayerNow(typeof (SectionLayer_LightingOnVehicle));
        }), state.vehiclePawn);
      }), (gravship2, intVec3_1, rotCounter, vehiclePawn1));
      Area_BuildRoof buildRoof1 = map.areaManager.BuildRoof;
      IEnumerable<IntVec3> activeCells = ((Area) buildRoof1).ActiveCells;
      Area_BuildRoof buildRoof2 = vehiclePawn1.VehicleMap.areaManager.BuildRoof;
      HashSet<IntVec3> second = cells;
      foreach (IntVec3 intVec3_2 in activeCells.Intersect<IntVec3>((IEnumerable<IntVec3>) second))
      {
        ((Area) buildRoof1)[intVec3_2] = false;
        ((Area) buildRoof2)[IntVec3.op_Addition(IntVec3Utility.RotatedBy(IntVec3.op_Subtraction(intVec3_2, min), rotCounter), IntVec3.NorthEast)] = true;
      }
      foreach ((IntVec3, float, float) tuple in list2)
      {
        Room room = GridsUtility.GetRoom(IntVec3.op_Addition(IntVec3Utility.RotatedBy(IntVec3.op_Subtraction(tuple.Item1, min), rotCounter), IntVec3.NorthEast), vehiclePawn1.VehicleMap);
        if (room != null && !room.ExposedToSpace && room.AnyPassable)
        {
          room.Temperature = tuple.Item2;
          room.Vacuum = tuple.Item3;
        }
      }
    }
    finally
    {
      Current.Game.Gravship = gravship1;
    }
    return AcceptanceReport.op_Implicit(true);
  }

  public static VehicleDef GenerateGravshipVehicleDef(
    VehicleMapProps_Gravship props,
    UniqueVehicleManager manager = null)
  {
    if (!ModsConfig.OdysseyActive)
      return (VehicleDef) null;
    VehicleDef vehicleDef = (manager ?? Current.Game.GetComponent<UniqueVehicleManager>()).ClaimUniqueVehicleDef(props.baseDef);
    if (!GenText.NullOrEmpty(props.defName))
      ScribeMetaHeaderUtility.modListChanged = true;
    ((Def) vehicleDef).label = TaggedString.op_Implicit(Translator.Translate("Gravship"));
    ((ThingDef) vehicleDef).size = props.size;
    vehicleDef.graphicData = new GraphicDataRGB();
    ((GraphicDataLayered) vehicleDef.graphicData).CopyFrom((GraphicDataLayered) props.baseDef.graphicData);
    ((GraphicData) vehicleDef.graphicData).texPath = "VehicleMapFramework/ClearTex";
    ((GraphicData) vehicleDef.graphicData).drawSize = ((IntVec2) ref props.size).ToVector2();
    ((Def) vehicleDef).modContentPack = VehicleMapFramework.VehicleMapFramework.mod.Content;
    ((Def) vehicleDef).modExtensions = new List<DefModExtension>(1)
    {
      (DefModExtension) props
    };
    UniqueVehicleUtility.ReinitializeComponents(vehicleDef);
    LongEventHandler.ExecuteWhenFinished((Action) (() => ((GraphicDataLayered) vehicleDef.graphicData).Init((IMaterialCacheTarget) vehicleDef)));
    return vehicleDef;
  }

  public static AcceptanceReport CheckGravshipVehicleStability(
    Building_GravEngine engine,
    Rot4 rot,
    out CellRect wheelsRect)
  {
    HashSet<IntVec3> cells = engine.ValidSubstructure;
    List<Building_GravshipWheel> list = engine.GravshipComponents.Select<CompGravshipFacility, ThingWithComps>((Func<CompGravshipFacility, ThingWithComps>) (c => ((ThingComp) c).parent)).OfType<Building_GravshipWheel>().Where<Building_GravshipWheel>((Func<Building_GravshipWheel, bool>) (w => w.ValidFor(rot))).Where<Building_GravshipWheel>((Func<Building_GravshipWheel, bool>) (w =>
    {
      Thing wallAttachedTo = GenConstruct.GetWallAttachedTo((Thing) w);
      return wallAttachedTo != null && ((IEnumerable<IntVec3>) (object) GenAdj.OccupiedRect(wallAttachedTo)).Any<IntVec3>(new Func<IntVec3, bool>(cells.Contains));
    })).Where<Building_GravshipWheel>((Func<Building_GravshipWheel, bool>) (w => !((IEnumerable<IntVec3>) (object) GenAdj.OccupiedRect((Thing) w)).Intersect<IntVec3>((IEnumerable<IntVec3>) cells).Any<IntVec3>())).ToList<Building_GravshipWheel>();
    if (GenCollection.Empty<Building_GravshipWheel>(list))
    {
      wheelsRect = CellRect.Empty;
      return AcceptanceReport.op_Implicit(TranslatorFormattedStringExtensions.Translate("VMF_WheelsUnstable", NamedArgument.op_Implicit(0)));
    }
    IEnumerable<IntVec3> intVec3s = list.SelectMany<Building_GravshipWheel, IntVec3>((Func<Building_GravshipWheel, IEnumerable<IntVec3>>) (w => (IEnumerable<IntVec3>) (object) GenAdj.OccupiedRect((Thing) w)));
    wheelsRect = CellRect.FromCellList(intVec3s);
    CellRect cellRect1 = CellRect.FromCellList((IEnumerable<IntVec3>) cells);
    CellRect cellRect2 = wheelsRect;
    ((CellRect) ref cellRect2).ClipInsideRect(cellRect1);
    if (list.Count >= 3 && (double) ((CellRect) ref cellRect2).Area / (double) ((CellRect) ref cellRect1).Area >= 0.5)
      return AcceptanceReport.op_Implicit(true);
    CollectionExtensions.Do<IntVec3>((IEnumerable<IntVec3>) (object) cellRect2, (Action<IntVec3>) (c => ((Thing) engine).Map.debugDrawer.FlashCell(c, 0.25f, (string) null, 5)));
    return AcceptanceReport.op_Implicit(TranslatorFormattedStringExtensions.Translate("VMF_WheelsUnstable", NamedArgument.op_Implicit(list.Count)));
  }
}
