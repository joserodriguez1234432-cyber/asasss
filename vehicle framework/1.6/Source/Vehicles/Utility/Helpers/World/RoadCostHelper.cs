// Decompiled with JetBrains decompiler
// Type: Vehicles.RoadCostHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public static class RoadCostHelper
{
  public static float GetRoadMovementDifficultyMultiplier(
    List<VehiclePawn> vehicles,
    int fromTile,
    int toTile,
    StringBuilder explanation = null)
  {
    List<SurfaceTile.RoadLink> roads = Find.WorldGrid.Surface[fromTile].Roads;
    if (roads == null)
    {
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      return RoadCostHelper.MaxRoadMultiplier<VehiclePawn>(vehicles, RoadCostHelper.\u003C\u003EO.\u003C0\u003E__VehicleOffRoadMultiplier ?? (RoadCostHelper.\u003C\u003EO.\u003C0\u003E__VehicleOffRoadMultiplier = new Func<VehiclePawn, float>(RoadCostHelper.VehicleOffRoadMultiplier)));
    }
    if (toTile == -1)
      toTile = PlanetTile.op_Implicit(Find.WorldGrid.FindMostReasonableAdjacentTileForDisplayedPathCost(PlanetTile.op_Implicit(fromTile)));
    for (int index = 0; index < roads.Count; ++index)
    {
      if (PlanetTile.op_Equality(roads[index].neighbor, PlanetTile.op_Implicit(toTile)))
      {
        float difficultyMultiplier = RoadCostHelper.GetRoadMovementDifficultyMultiplier(vehicles, roads[index].road);
        if (explanation != null)
        {
          if (explanation.Length > 0)
            explanation.AppendLine();
          explanation.Append($"{((Def) roads[index].road).LabelCap}: {GenText.ToStringPercent(difficultyMultiplier)}");
        }
        return difficultyMultiplier;
      }
    }
    return 1f;
  }

  public static float GetRoadMovementDifficultyMultiplier(
    List<VehicleDef> vehicleDefs,
    int fromTile,
    int toTile,
    StringBuilder explanation = null)
  {
    List<SurfaceTile.RoadLink> roads = Find.WorldGrid.Surface[fromTile].Roads;
    if (roads == null)
    {
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      return RoadCostHelper.MaxRoadMultiplier<VehicleDef>(vehicleDefs, RoadCostHelper.\u003C\u003EO.\u003C1\u003E__VehicleDefOffRoadMultiplier ?? (RoadCostHelper.\u003C\u003EO.\u003C1\u003E__VehicleDefOffRoadMultiplier = new Func<VehicleDef, float>(RoadCostHelper.VehicleDefOffRoadMultiplier)));
    }
    if (toTile == -1)
      toTile = PlanetTile.op_Implicit(Find.WorldGrid.FindMostReasonableAdjacentTileForDisplayedPathCost(PlanetTile.op_Implicit(fromTile)));
    for (int index = 0; index < roads.Count; ++index)
    {
      if (PlanetTile.op_Equality(roads[index].neighbor, PlanetTile.op_Implicit(toTile)))
      {
        float difficultyMultiplier = RoadCostHelper.GetRoadMovementDifficultyMultiplier(vehicleDefs, roads[index].road);
        if (explanation != null)
        {
          if (explanation.Length > 0)
            explanation.AppendLine();
          explanation.Append($"{((Def) roads[index].road).LabelCap}: {GenText.ToStringPercent(difficultyMultiplier)}");
        }
        return difficultyMultiplier;
      }
    }
    return 1f;
  }

  private static float MaxRoadMultiplier<T>(List<T> list, Func<T, float> selector)
  {
    return Mathf.Clamp(list.Max<T>(selector), 0.01f, 100f);
  }

  public static float VehicleOffRoadMultiplier(VehiclePawn vehicle)
  {
    float num = RoadCostHelper.VehicleDefOffRoadMultiplier(vehicle.VehicleDef);
    return Mathf.Clamp(vehicle.statHandler.GetStatOffset(VehicleStatUpgradeCategoryDefOf.OffRoadMultiplier, num), 0.01f, 10f);
  }

  public static float VehicleDefOffRoadMultiplier(VehicleDef vehicleDef)
  {
    return SettingsCache.TryGetValue<float>(vehicleDef, typeof (VehicleProperties), "offRoadMultiplier", vehicleDef.properties.offRoadMultiplier);
  }

  public static float GetRoadMovementDifficultyMultiplier(
    List<VehicleDef> vehicleDefs,
    RoadDef roadDef)
  {
    float difficultyMultiplier = roadDef.movementCostMultiplier;
    bool flag = false;
    foreach (VehicleDef vehicleDef in vehicleDefs)
    {
      float num;
      if (vehicleDef.properties.customRoadCosts.TryGetValue(roadDef, out num) && (!flag || (double) num < (double) difficultyMultiplier))
      {
        flag = true;
        difficultyMultiplier = num;
      }
    }
    return difficultyMultiplier;
  }

  public static float GetRoadMovementDifficultyMultiplier(
    List<VehiclePawn> vehicles,
    RoadDef roadDef)
  {
    float difficultyMultiplier = roadDef.movementCostMultiplier;
    bool flag = false;
    foreach (VehiclePawn vehicle in vehicles)
    {
      float num;
      if (vehicle.VehicleDef.properties.customRoadCosts.TryGetValue(roadDef, out num) && (!flag || (double) num < (double) difficultyMultiplier))
      {
        flag = true;
        difficultyMultiplier = num;
      }
    }
    return difficultyMultiplier;
  }
}
