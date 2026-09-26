// Decompiled with JetBrains decompiler
// Type: Vehicles.VehiclePathGrid
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public sealed class VehiclePathGrid : VehicleGridManager
{
  public const int ImpassableCost = 10000;
  public int[] innerArray;

  public VehiclePathGrid(VehiclePathingSystem mapping, VehicleDef vehicleDef)
    : base(mapping, vehicleDef)
  {
    this.innerArray = new int[((CellIndices) ref mapping.map.cellIndices).NumGridCells];
  }

  public bool Enabled { get; private set; }

  public void Release()
  {
    this.Enabled = false;
    if (!this.mapping.GridOwners.IsOwner(this.createdFor) || this.mapping.GridOwners.TryForfeitOwnership(this.createdFor))
      return;
    this.mapping[this.createdFor].VehicleRegionAndRoomUpdater.Release();
  }

  public override void PostInit()
  {
  }

  public bool Walkable(IntVec3 loc)
  {
    try
    {
      return GenGrid.InBounds(loc, this.mapping.map) && this.WalkableFast(loc);
    }
    catch (Exception ex)
    {
      Log.Error($"Mapping: {this.mapping == null} Map: {this.mapping?.map == null} CellInd: " + $"{!this.mapping?.map?.cellIndices.HasValue} Info: {this.mapping?.map?.info}Exception: {ex}");
      Log.Error("StackTrace: " + StackTraceUtility.ExtractStackTrace());
    }
    return false;
  }

  public bool WalkableFast(IntVec3 loc)
  {
    return this.WalkableFast(((CellIndices) ref this.mapping.map.cellIndices).CellToIndex(loc));
  }

  public bool WalkableFast(int x, int z)
  {
    return this.WalkableFast(((CellIndices) ref this.mapping.map.cellIndices).CellToIndex(x, z));
  }

  public bool WalkableFast(int index) => this.innerArray[index] < 10000;

  public int PerceivedPathCostAt(IntVec3 loc)
  {
    return this.innerArray[((CellIndices) ref this.mapping.map.cellIndices).CellToIndex(loc)];
  }

  public void RecalculatePerceivedPathCostUnderRect(CellRect cellRect)
  {
    for (int minZ = cellRect.minZ; minZ <= cellRect.maxZ; ++minZ)
    {
      for (int minX = cellRect.minX; minX <= cellRect.maxX; ++minX)
      {
        IntVec3 cell;
        // ISSUE: explicit constructor call
        ((IntVec3) ref cell).\u002Ector(minX, 0, minZ);
        this.RecalculatePerceivedPathCostAt(cell);
      }
    }
  }

  public void RecalculatePerceivedPathCostAt(IntVec3 cell)
  {
    if (!GenGrid.InBounds(cell, this.mapping.map))
      return;
    bool flag1 = this.WalkableFast(cell);
    StringBuilder stringBuilder = (StringBuilder) null;
    if (VehicleMod.settings.debug.debugPathCostChanges)
      stringBuilder = new StringBuilder();
    int num = this.CalculatedCostAt(cell, stringBuilder);
    Interlocked.Exchange(ref this.innerArray[((CellIndices) ref this.mapping.map.cellIndices).CellToIndex(cell)], num);
    stringBuilder?.Append($"WalkableNew: {this.WalkableFast(cell)} WalkableOld: {flag1}");
    bool flag2 = this.WalkableFast(cell) != flag1;
    if (VehicleMod.settings.debug.debugPathCostChanges)
      Debug.Message(Gen.ToStringSafe<StringBuilder>(stringBuilder));
    if (!flag2 || this.mapping[this.createdFor].Suspended)
      return;
    this.mapping[this.createdFor].VehicleReachability.ClearCache();
    this.mapping[this.createdFor].VehicleRegionDirtyer.NotifyWalkabilityChanged(cell);
  }

  public void RecalculateAllPerceivedPathCosts()
  {
    this.Enabled = true;
    foreach (IntVec3 allCell in this.mapping.map.AllCells)
      this.RecalculatePerceivedPathCostAt(allCell);
  }

  public int CalculatedCostAt(IntVec3 cell, StringBuilder stringBuilder = null)
  {
    return VehiclePathGrid.CalculatePathCostFor(this.createdFor, this.mapping.map, cell, stringBuilder);
  }

  [Profile]
  public static int CalculatePathCostFor(
    VehicleDef vehicleDef,
    Map map,
    IntVec3 cell,
    StringBuilder stringBuilder = null)
  {
    stringBuilder?.AppendLine($"Starting calculation for {vehicleDef} at {cell}.");
    int pathCost = 0;
    try
    {
      TerrainDef terrainDef = map.terrainGrid.TerrainAt(cell);
      if (terrainDef == null)
      {
        stringBuilder?.AppendLine($"Unable to retrieve terrain at {cell}.");
        return 10000;
      }
      if (!VehiclePathGrid.PassableTerrainCost(vehicleDef, terrainDef, out pathCost, stringBuilder))
        return 10000;
      ThingGrid thingGrid = map.thingGrid;
      lock (thingGrid)
      {
        List<Thing> thingList = thingGrid.ThingsListAt(cell);
        stringBuilder?.AppendLine("Starting ThingList check.");
        if (!GenList.NullOrEmpty<Thing>((IList<Thing>) thingList))
        {
          int num1 = 0;
          foreach (Thing thing in thingList)
          {
            if (thing != null && thing.Spawned && !thing.Destroyed && !(thing is VehiclePawn))
            {
              int num2 = VehiclePathGrid.ThingCostOf(vehicleDef, thing.def, stringBuilder);
              stringBuilder?.AppendLine($"thingPathCost: {num2}");
              if (num2 > num1)
                num1 = num2;
            }
          }
          pathCost += num1;
        }
      }
      WeatherBuildupCategory category = map.snowGrid.GetCategory(cell);
      int val;
      if (!vehicleDef.properties.customWeatherCosts.TryGetValue(category, out val))
        val = WeatherBuildupUtility.MovementTicksAddOn(category);
      val = val.Clamp(0, 450);
      stringBuilder?.AppendLine($"weatherPathCost: {val}");
      pathCost += val;
      stringBuilder?.AppendLine($"final cost: {pathCost}");
    }
    catch (Exception ex)
    {
      Log.Error($"Exception thrown while recalculating cost for {vehicleDef} at {cell}.\nException={ex}");
      Log.Error($"Calculated Cost Report:\n{stringBuilder}\nProps={vehicleDef?.properties == null} " + $"Terrain={vehicleDef?.properties?.customTerrainCosts == null} Snow: " + $"{vehicleDef?.properties?.customWeatherCosts == null}");
    }
    return pathCost;
  }

  public static int ThingCostOf(
    VehicleDef vehicleDef,
    ThingDef thingDef,
    StringBuilder stringBuilder = null)
  {
    int pathCost;
    if (vehicleDef.properties.customThingCosts.TryGetValue(thingDef, out pathCost))
    {
      if (pathCost >= 10000)
      {
        stringBuilder?.AppendLine($"thingPathCost is impassable: {pathCost}");
        return 10000;
      }
    }
    else
    {
      if ((vehicleDef.properties.defaultImpassable & DefaultImpassable.Things) != DefaultImpassable.None)
      {
        stringBuilder?.AppendLine($"thingPathCost is impassable: {pathCost}");
        return 10000;
      }
      if (thingDef.ImpassableForVehicles())
      {
        stringBuilder?.AppendLine($"thingDef is impassable: {pathCost}");
        return 10000;
      }
      pathCost = ((BuildableDef) thingDef).pathCost;
    }
    return pathCost;
  }

  public static bool PassableTerrainCost(
    VehicleDef vehicleDef,
    TerrainDef terrainDef,
    out int pathCost,
    StringBuilder stringBuilder = null)
  {
    pathCost = VehiclePathGrid.TerrainCostAt(vehicleDef, terrainDef, stringBuilder);
    return pathCost < 10000;
  }

  public static int TerrainCostAt(
    VehicleDef vehicleDef,
    TerrainDef terrainDef,
    StringBuilder stringBuilder = null)
  {
    int num1 = ((BuildableDef) terrainDef).pathCost;
    stringBuilder?.AppendLine($"Starting Terrain check. Default Cost = {num1}");
    int num2;
    if (vehicleDef.properties.customTerrainCosts.TryGetValue(terrainDef, out num2))
    {
      stringBuilder?.AppendLine($"custom terrain cost: {num2}");
      num1 = num2;
    }
    else
    {
      if (((BuildableDef) terrainDef).passability == 2)
      {
        stringBuilder?.AppendLine($"terrainDef impassable: {10000}");
        return 10000;
      }
      if ((vehicleDef.properties.defaultImpassable & DefaultImpassable.Terrain) != DefaultImpassable.None)
      {
        stringBuilder?.AppendLine("defaultTerrain is impassable and no custom pathCost was found.");
        return 10000;
      }
    }
    return num1;
  }
}
