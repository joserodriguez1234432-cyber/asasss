// Decompiled with JetBrains decompiler
// Type: Vehicles.World.WinterPathingHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.World;

public static class WinterPathingHelper
{
  private const float MaxTempForWinterOffset = 5f;

  public static float GetWinterPercent(PlanetTile tile, int? ticksAbs = null)
  {
    Vector2 vector2 = Find.WorldGrid.LongLatOf(tile);
    int num1 = ticksAbs ?? GenTicks.TicksAbs;
    float num2;
    float num3;
    float num4;
    float num5;
    float num6;
    float num7;
    SeasonUtility.GetSeason(GenDate.YearPercent((long) num1, vector2.x), vector2.y, ref num2, ref num3, ref num4, ref num5, ref num6, ref num7);
    return (num5 + num7) * Mathf.InverseLerp(5f, 0.0f, GenTemperature.GetTemperatureFromSeasonAtTile(num1, tile));
  }

  public static float GetCurrentWinterMovementDifficultyOffset(
    List<VehiclePawn> vehicles,
    PlanetTile tile,
    StringBuilder explanation = null)
  {
    float finalCost = WorldVehiclePathGrid.Instance.WinterPercentAt(tile);
    if ((double) finalCost <= 0.0099999997764825821)
      return 0.0f;
    float num = WinterPathingHelper.HighestWinterOffset(vehicles);
    float difficultyOffset = finalCost * num;
    if (explanation != null)
      WinterPathingHelper.WinterExplanation(explanation, finalCost);
    return difficultyOffset;
  }

  public static float GetCurrentWinterMovementDifficultyOffset(
    List<VehicleDef> vehicleDefs,
    int tile,
    StringBuilder explanation = null)
  {
    float finalCost = WorldVehiclePathGrid.Instance.WinterPercentAt(PlanetTile.op_Implicit(tile));
    if ((double) finalCost <= 0.0099999997764825821)
      return 0.0f;
    float num = WinterPathingHelper.HighestWinterOffset(vehicleDefs);
    float difficultyOffset = finalCost * num;
    if (explanation != null)
      WinterPathingHelper.WinterExplanation(explanation, finalCost);
    return difficultyOffset;
  }

  private static void WinterExplanation(StringBuilder explanation, float finalCost)
  {
    explanation.AppendLine();
    explanation.Append($"{Translator.Translate("Winter")}: {GenText.ToStringWithSign(finalCost, "0.#")}");
  }

  public static float GetCurrentWinterMovementDifficultyFor(
    VehicleDef vehicleDef,
    PlanetTile tile,
    StringBuilder explanation = null)
  {
    float finalCost = WorldVehiclePathGrid.Instance.WinterPercentAt(tile);
    if ((double) finalCost <= 0.0099999997764825821)
      return 0.0f;
    float num = SettingsCache.TryGetValue<float>(vehicleDef, typeof (VehicleProperties), "winterCost", vehicleDef.properties.winterCost);
    float movementDifficultyFor = finalCost * num;
    if (explanation != null)
      WinterPathingHelper.WinterExplanation(explanation, finalCost);
    return movementDifficultyFor;
  }

  private static float HighestWinterOffset(List<VehicleDef> vehicleDefs)
  {
    float num1 = 0.01f;
    foreach (VehicleDef vehicleDef in vehicleDefs)
    {
      float num2 = SettingsCache.TryGetValue<float>(vehicleDef, typeof (VehicleProperties), "winterCost", vehicleDef.properties.winterCost);
      if ((double) num2 > (double) num1)
        num1 = num2;
    }
    return num1;
  }

  private static float HighestWinterOffset(List<VehiclePawn> vehicles)
  {
    float num1 = 0.01f;
    foreach (VehiclePawn vehicle in vehicles)
    {
      float num2 = SettingsCache.TryGetValue<float>(vehicle.VehicleDef, typeof (VehicleProperties), "winterCost", vehicle.VehicleDef.properties.winterCost);
      float statOffset = vehicle.statHandler.GetStatOffset(VehicleStatUpgradeCategoryDefOf.WinterCostMultiplier, num2);
      if ((double) statOffset > (double) num1)
        num1 = statOffset;
    }
    return num1;
  }
}
