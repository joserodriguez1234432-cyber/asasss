// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.MapVehicleGroupMakerUtility
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public static class MapVehicleGroupMakerUtility
{
  public const float MinPointsToGenerateVehicles = 50f;

  public static IEnumerable<VehiclePawnWithMap> GenerateVehicles(
    Faction faction,
    float points,
    LinearCurve vehicleCountCurve,
    List<VehicleDef> availableDefs)
  {
    VehicleRaiderDefModExtension modExtension = ((Def) faction.def).GetModExtension<VehicleRaiderDefModExtension>();
    float vehicleBudget = (float) ((modExtension != null ? (double) modExtension.pointMultiplier : 1.0) * (double) points / 2.0);
    if ((double) vehicleBudget > 0.0)
    {
      vehicleBudget = Mathf.Max(vehicleBudget, 50f);
      int vehicleCount = Mathf.FloorToInt(vehicleCountCurve.Evaluate(points));
      if (vehicleCount > 0 && availableDefs.Count > 0)
      {
        for (int i = 0; i < vehicleCount; ++i)
        {
          float budget = vehicleBudget;
          VehicleDef vehicleDef1;
          if (GenCollection.TryRandomElementByWeight<VehicleDef>(availableDefs.Where<VehicleDef>((Func<VehicleDef, bool>) (vehicleDef => (double) vehicleDef.combatPower <= (double) budget)), (Func<VehicleDef, float>) (vehicleDef => vehicleDef.combatPower), ref vehicleDef1))
          {
            vehicleBudget -= vehicleDef1.combatPower;
            points = Mathf.Max(points - vehicleDef1.combatPower, 10f);
            yield return (VehiclePawnWithMap) VehicleSpawner.GenerateVehicle(vehicleDef1, faction);
          }
        }
      }
    }
  }
}
