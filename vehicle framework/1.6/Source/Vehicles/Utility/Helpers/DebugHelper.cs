// Decompiled with JetBrains decompiler
// Type: Vehicles.DebugHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace Vehicles;

public static class DebugHelper
{
  public static readonly DebugHelper.PathDebugData<DebugRegionType> Local = new DebugHelper.PathDebugData<DebugRegionType>();
  public static readonly DebugHelper.PathDebugData<WorldPathingDebugType> World = new DebugHelper.PathDebugData<WorldPathingDebugType>();
  internal static List<WorldPath> debugLines = new List<WorldPath>();

  public static bool AnyDebugSettings
  {
    get => DebugHelper.Local.DebugType != DebugRegionType.None || DebugHelper.World.DebugType != 0;
  }

  static DebugHelper() => GameEvent.OnWorldRemoved += new Action(DebugHelper.debugLines.Clear);

  public static void FillArea(CellRect rect, Map map, ThingDef thingDef)
  {
    ThingDef thingDef1 = ((BuildableDef) thingDef).MadeFromStuff ? GenStuff.DefaultStuffFor((BuildableDef) thingDef) : (ThingDef) null;
    ((CellRect) ref rect).ClipInsideMap(map);
    foreach (IntVec3 intVec3 in rect)
      GenSpawn.Spawn(ThingMaker.MakeThing(thingDef, thingDef1), intVec3, map, (WipeMode) 0);
  }

  public static void FillEdge(CellRect rect, Map map, ThingDef thingDef)
  {
    ThingDef thingDef1 = ((BuildableDef) thingDef).MadeFromStuff ? GenStuff.DefaultStuffFor((BuildableDef) thingDef) : (ThingDef) null;
    ((CellRect) ref rect).ClipInsideMap(map);
    foreach (IntVec3 edgeCell in ((CellRect) ref rect).EdgeCells)
      GenSpawn.Spawn(ThingMaker.MakeThing(thingDef, thingDef1), edgeCell, map, (WipeMode) 0);
  }

  public static void DestroyArea(CellRect rect, Map map, TerrainDef replaceTerrain = null)
  {
    Thing.allowDestroyNonDestroyable = true;
    try
    {
      ((CellRect) ref rect).ClipInsideMap(map);
      foreach (IntVec3 cell in rect)
        DebugHelper.DestroyCell(cell, map);
      if (replaceTerrain == null)
        return;
      foreach (IntVec3 intVec3 in rect)
        map.terrainGrid.SetTerrain(intVec3, replaceTerrain);
    }
    finally
    {
      Thing.allowDestroyNonDestroyable = false;
    }
  }

  public static void DestroyCell(IntVec3 cell, Map map, TerrainDef replaceTerrain = null)
  {
    map.roofGrid.SetRoof(cell, (RoofDef) null);
    foreach (Thing thing in GridsUtility.GetThingList(cell, map).ToList<Thing>())
      thing.Destroy((DestroyMode) 0);
    if (replaceTerrain == null)
      return;
    map.terrainGrid.SetTerrain(cell, replaceTerrain);
  }

  public static void DebugAddSettlementOrigin(PlanetTile from, PlanetTile to)
  {
    PeaceTalks peaceTalks = (PeaceTalks) WorldObjectMaker.MakeWorldObject(WorldObjectDefOfVehicles.DebugSettlement);
    ((WorldObject) peaceTalks).Tile = from;
    ((WorldObject) peaceTalks).SetFaction(Faction.OfMechanoids);
    Find.WorldObjects.Add((WorldObject) peaceTalks);
    if (!DebugProperties.DrawPaths)
      return;
    DebugHelper.debugLines.Add(((PlanetTile) ref from).Layer.Pather.FindPath(from, to, (Caravan) null, (Func<float, bool>) null));
  }

  public static List<Toggle> DebugToggles<T>(
    VehicleDef vehicleDef,
    DebugHelper.PathDebugData<T> debugData)
    where T : Enum
  {
    List<Toggle> toggleList = new List<Toggle>();
    if (Enum.GetUnderlyingType(typeof (T)) != typeof (int))
    {
      Log.Error($"Cannot generate DebugToggles for enum type {typeof (T)}. Must be int32 to avoid overflow.");
      return toggleList;
    }
    foreach (T obj in Enum.GetValues(typeof (T)))
    {
      T @enum = obj;
      bool flags = typeof (T).IsDefined(typeof (FlagsAttribute), false);
      if (!flags || Convert.ToInt32((object) (T) @enum) != 0)
      {
        Toggle toggle = new Toggle(@enum.ToString(), (Func<bool>) (() =>
        {
          if (debugData.VehicleDef != vehicleDef)
            return false;
          return !flags ? debugData.DebugType.Equals((object) @enum) : debugData.DebugType.HasFlag((Enum) @enum);
        }), (Action<bool>) (value =>
        {
          debugData.VehicleDef = vehicleDef;
          if (flags)
          {
            debugData.DebugType = (T) Enum.ToObject(typeof (T), value ? Convert.ToInt32((object) debugData.DebugType) | Convert.ToInt32((object) @enum) : Convert.ToInt32((object) debugData.DebugType) & ~Convert.ToInt32((object) @enum));
          }
          else
          {
            if (!value)
              return;
            debugData.DebugType = @enum;
          }
        }));
        toggleList.Add(toggle);
      }
    }
    return toggleList;
  }

  public static void DebugDrawVehicleRegion(Map map)
  {
    if (DebugHelper.Local.VehicleDef == null)
      return;
    map.GetCachedMapComponent<VehiclePathingSystem>()[DebugHelper.Local.VehicleDef].VehicleRegionGrid.DebugDraw(DebugHelper.Local.DebugType);
  }

  public static void DebugDrawVehiclePathCostsOverlay(Map map)
  {
    if (DebugHelper.Local.VehicleDef == null)
      return;
    map.GetCachedMapComponent<VehiclePathingSystem>()[DebugHelper.Local.VehicleDef].VehicleRegionGrid.DebugOnGUI(DebugHelper.Local.DebugType);
  }

  public class PathDebugData<T> where T : Enum
  {
    public VehicleDef VehicleDef { get; set; }

    public T DebugType { get; set; }
  }

  public readonly struct DestroyAreaScope(Map map, CellRect rect) : IDisposable
  {
    void IDisposable.Dispose() => DebugHelper.DestroyArea(rect, map);
  }
}
