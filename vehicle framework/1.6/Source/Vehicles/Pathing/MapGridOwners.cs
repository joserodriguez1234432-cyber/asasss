// Decompiled with JetBrains decompiler
// Type: Vehicles.MapGridOwners
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class MapGridOwners : GridOwnerList<MapGridOwners.PathConfig>
{
  private readonly VehiclePathingSystem mapping;

  public MapGridOwners(VehiclePathingSystem mapping) => this.mapping = mapping;

  protected override bool CanTransferOwnershipTo(VehicleDef vehicleDef)
  {
    return this.mapping[vehicleDef].VehiclePathGrid.Enabled;
  }

  protected override void GenerateConfigs()
  {
    this.configs = new MapGridOwners.PathConfig[DefDatabase<VehicleDef>.DefCount];
    foreach (VehicleDef vehicleDef in DefDatabase<VehicleDef>.AllDefsListForReading)
      this.configs[vehicleDef.DefIndex] = new MapGridOwners.PathConfig(vehicleDef);
  }

  public readonly struct PathConfig : IPathConfig
  {
    private readonly VehicleDef vehicleDef;
    private readonly HashSet<ThingDef> impassableThingDefs;
    private readonly HashSet<TerrainDef> impassableTerrain;
    private readonly int size;
    private readonly DefaultImpassable defaultMapImpassable;

    internal PathConfig(VehicleDef vehicleDef)
    {
      this.vehicleDef = vehicleDef;
      this.size = Mathf.Min(((BuildableDef) vehicleDef).Size.x, ((BuildableDef) vehicleDef).Size.z);
      this.defaultMapImpassable = vehicleDef.properties.defaultImpassable & (DefaultImpassable.Terrain | DefaultImpassable.Things);
      this.impassableThingDefs = vehicleDef.properties.customThingCosts.Where<KeyValuePair<ThingDef, int>>((Func<KeyValuePair<ThingDef, int>, bool>) (kvp => kvp.Value >= 10000)).Select<KeyValuePair<ThingDef, int>, ThingDef>((Func<KeyValuePair<ThingDef, int>, ThingDef>) (kvp => kvp.Key)).ToHashSet<ThingDef>();
      this.impassableTerrain = vehicleDef.properties.customTerrainCosts.Where<KeyValuePair<TerrainDef, int>>((Func<KeyValuePair<TerrainDef, int>, bool>) (kvp => kvp.Value >= 10000)).Select<KeyValuePair<TerrainDef, int>, TerrainDef>((Func<KeyValuePair<TerrainDef, int>, TerrainDef>) (kvp => kvp.Key)).ToHashSet<TerrainDef>();
    }

    bool IPathConfig.UsesRegions
    {
      get
      {
        return !Mathf.Approximately(this.vehicleDef.GetStatValueAbstract(VehicleStatDefOf.MoveSpeed), 0.0f);
      }
    }

    bool IPathConfig.MatchesReachability(IPathConfig other)
    {
      return other is MapGridOwners.PathConfig pathConfig && this.size == pathConfig.size && this.defaultMapImpassable == pathConfig.defaultMapImpassable && this.impassableThingDefs.SetEquals((IEnumerable<ThingDef>) pathConfig.impassableThingDefs) && this.impassableTerrain.SetEquals((IEnumerable<TerrainDef>) pathConfig.impassableTerrain);
    }
  }
}
