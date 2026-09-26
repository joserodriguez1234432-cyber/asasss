// Decompiled with JetBrains decompiler
// Type: Vehicles.World.WorldGridOwners
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.World;

public class WorldGridOwners : GridOwnerList<WorldGridOwners.PathConfig>
{
  protected override bool CanTransferOwnershipTo(VehicleDef vehicleDef)
  {
    throw new NotSupportedException();
  }

  protected override void GenerateConfigs()
  {
    this.configs = new WorldGridOwners.PathConfig[DefDatabase<VehicleDef>.DefCount];
    foreach (VehicleDef vehicleDef in DefDatabase<VehicleDef>.AllDefsListForReading)
      this.configs[vehicleDef.DefIndex] = new WorldGridOwners.PathConfig(vehicleDef);
  }

  public bool MatchingReachability(VehicleDef vehicleDef, VehicleDef otherVehicleDef)
  {
    return ((IPathConfig) this.configs[vehicleDef.DefIndex]).MatchesReachability((IPathConfig) this.configs[otherVehicleDef.DefIndex]);
  }

  public readonly struct PathConfig : IPathConfig
  {
    private readonly VehicleDef vehicleDef;
    private readonly DefaultImpassable defaultWorldImpassable;
    private readonly SimpleDictionary<BiomeDef, float> customBiomeCosts;
    private readonly SimpleDictionary<Hilliness, float> customHillinessCosts;
    private readonly SimpleDictionary<RiverDef, float> customRiverCosts;

    internal PathConfig(VehicleDef vehicleDef)
    {
      this.vehicleDef = vehicleDef;
      this.defaultWorldImpassable = vehicleDef.properties.defaultImpassable & (DefaultImpassable.Biomes | DefaultImpassable.Rivers | DefaultImpassable.Hilliness);
      this.customBiomeCosts = vehicleDef.properties.customBiomeCosts;
      this.customHillinessCosts = vehicleDef.properties.customHillinessCosts;
      this.customRiverCosts = vehicleDef.properties.customRiverCosts;
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
      return other is WorldGridOwners.PathConfig pathConfig && this.defaultWorldImpassable == pathConfig.defaultWorldImpassable && MatchingValues<BiomeDef>(this.customBiomeCosts, pathConfig.customBiomeCosts) && MatchingValues<Hilliness>(this.customHillinessCosts, pathConfig.customHillinessCosts) && MatchingValues<RiverDef>(this.customRiverCosts, pathConfig.customRiverCosts);

      static bool MatchingValues<T>(SimpleDictionary<T, float> lhs, SimpleDictionary<T, float> rhs)
      {
        T obj;
        float num1;
        foreach (KeyValuePair<T, float> lh in (Dictionary<T, float>) lhs)
        {
          lh.Deconstruct(ref obj, ref num1);
          T key = obj;
          float num2 = num1;
          float num3;
          if (!rhs.TryGetValue(key, out num3) || Mathf.Approximately(num2, 1000f) == Mathf.Approximately(num3, 1000f))
            return false;
        }
        foreach (KeyValuePair<T, float> rh in (Dictionary<T, float>) rhs)
        {
          rh.Deconstruct(ref obj, ref num1);
          T key = obj;
          float num4 = num1;
          float num5;
          if (!lhs.TryGetValue(key, out num5) || Mathf.Approximately(num4, 1000f) == Mathf.Approximately(num5, 1000f))
            return false;
        }
        return true;
      }
    }
  }
}
