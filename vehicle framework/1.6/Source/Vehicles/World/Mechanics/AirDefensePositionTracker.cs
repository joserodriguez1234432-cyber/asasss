// Decompiled with JetBrains decompiler
// Type: Vehicles.World.AirDefensePositionTracker
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.World;

public class AirDefensePositionTracker(RimWorld.Planet.World world) : WorldComponentTemp(world)
{
  public const float RotationRate = 0.35f;
  private static Dictionary<AerialVehicleInFlight, List<AirDefense>> searchingDefenses = new Dictionary<AerialVehicleInFlight, List<AirDefense>>();
  private static HashSet<AirDefense> defensesToDraw = new HashSet<AirDefense>();
  public static Dictionary<WorldObject, AirDefense> airDefenseCache = new Dictionary<WorldObject, AirDefense>();
  private static List<WorldObject> saveableWorldObjects;
  private static List<AirDefense> saveableAirDefenses;

  public override void WorldComponentUpdate()
  {
    if (!VehicleMod.settings.main.airDefenses)
      return;
    foreach (AirDefense airDefense in AirDefensePositionTracker.defensesToDraw)
      airDefense.DrawSpotlightOverlay();
  }

  public override void WorldComponentTick()
  {
    if (!VehicleMod.settings.main.airDefenses)
      return;
    foreach (KeyValuePair<AerialVehicleInFlight, List<AirDefense>> searchingDefense in AirDefensePositionTracker.searchingDefenses)
    {
      AerialVehicleInFlight key = searchingDefense.Key;
      for (int index = searchingDefense.Value.Count - 1; index >= 0; --index)
      {
        AirDefense airDefense = searchingDefense.Value.ElementAt<AirDefense>(index);
        bool flag = (double) Ext_Math.SphericalDistance(airDefense.parent.DrawPos, ((WorldObject) key).DrawPos) <= (double) airDefense.MaxDistance;
        if (airDefense.CurrentTarget != key)
        {
          airDefense.angle = (airDefense.angle + 0.35f * (float) airDefense.searchDirection).ClampAngle();
          float point = airDefense.parent.DrawPos.AngleToPoint(((WorldObject) key).DrawPos);
          if (flag && (double) Mathf.Abs(point - airDefense.angle) <= (double) airDefense.Arc / 2.0)
            airDefense.activeTargets.Add(key);
        }
        else
        {
          float heading = WorldHelper.TryFindHeading(airDefense.parent.DrawPos, ((WorldObject) airDefense.CurrentTarget).DrawPos);
          int num = (double) heading < (double) airDefense.angle ? -2 : 2;
          if ((double) Mathf.Abs(heading - airDefense.angle) < 1.0 || (double) Mathf.Abs(heading - airDefense.angle) > 359.0)
          {
            airDefense.angle = heading;
            airDefense.Attack();
          }
          else
            airDefense.angle = (airDefense.angle + 0.35f * (float) num).ClampAngle();
          if (!flag)
            airDefense.activeTargets.Remove(key);
        }
      }
    }
  }

  public override void FinalizeInit()
  {
    if (AirDefensePositionTracker.searchingDefenses == null)
      AirDefensePositionTracker.searchingDefenses = new Dictionary<AerialVehicleInFlight, List<AirDefense>>();
    if (AirDefensePositionTracker.airDefenseCache != null)
      return;
    AirDefensePositionTracker.airDefenseCache = new Dictionary<WorldObject, AirDefense>();
  }

  public static void RegisterAerialVehicle(
    AerialVehicleInFlight aerialVehicle,
    List<AirDefense> newDefenses)
  {
    List<AirDefense> oldDefenses = new List<AirDefense>();
    List<AirDefense> collection;
    if (AirDefensePositionTracker.searchingDefenses.TryGetValue(aerialVehicle, out collection))
    {
      oldDefenses.AddRange((IEnumerable<AirDefense>) collection);
      collection.Clear();
      collection.AddRange((IEnumerable<AirDefense>) newDefenses);
    }
    else
      AirDefensePositionTracker.searchingDefenses.Add(aerialVehicle, newDefenses);
    AirDefensePositionTracker.defensesToDraw.RemoveWhere((Predicate<AirDefense>) (d => oldDefenses.Contains(d)));
    foreach (AirDefense newDefense in newDefenses)
    {
      if (AirDefensePositionTracker.defensesToDraw.Add(newDefense) && !oldDefenses.Contains(newDefense))
        newDefense.angle = (float) ((double) (Find.TickManager.TicksGame + newDefense.parent.GetHashCode()) * 0.34999999403953552 % 360.0);
    }
  }

  public static void DeregisterAerialVehicle(AerialVehicleInFlight aerialVehicle)
  {
    AirDefensePositionTracker.searchingDefenses.Remove(aerialVehicle);
    AirDefensePositionTracker.RecacheAirDefenseDrawers();
  }

  public static void RecacheAirDefenseDrawers()
  {
    AirDefensePositionTracker.defensesToDraw.Clear();
    foreach (KeyValuePair<AerialVehicleInFlight, List<AirDefense>> searchingDefense in AirDefensePositionTracker.searchingDefenses)
    {
      foreach (AirDefense airDefense in searchingDefense.Value)
        AirDefensePositionTracker.defensesToDraw.Add(airDefense);
    }
  }

  public static List<AirDefense> GetNearbyObjects(
    AerialVehicleInFlight aerialVehicle,
    float speedPctPerTick)
  {
    List<AirDefense> nearbyObjects = new List<AirDefense>();
    float num1 = (speedPctPerTick * 100f).RoundTo(1f / 1000f);
    Vector3 vector3_1 = ((WorldObject) aerialVehicle).DrawPos;
    for (int index = 0; index < aerialVehicle.flightPath.Path.Count; ++index)
    {
      Vector3 tileCenter = Find.WorldGrid.GetTileCenter(PlanetTile.op_Implicit(PlanetTile.op_Implicit(aerialVehicle.flightPath[index].Tile)));
      Vector3 vector3_2 = vector3_1;
      for (float num2 = 0.0f; (double) num2 < 1.0; num2 += num1)
      {
        Vector3 source = Vector3.Slerp(vector3_2, tileCenter, num2);
        foreach (KeyValuePair<WorldObject, AirDefense> keyValuePair in AirDefensePositionTracker.airDefenseCache)
        {
          if ((double) Ext_Math.SphericalDistance(source, keyValuePair.Key.DrawPos) < (double) keyValuePair.Value.MaxDistance)
            nearbyObjects.Add(keyValuePair.Value);
        }
      }
      vector3_1 = tileCenter;
    }
    return nearbyObjects;
  }

  public override void ExposeData()
  {
    base.ExposeData();
    Scribe_Collections.Look<WorldObject, AirDefense>(ref AirDefensePositionTracker.airDefenseCache, "airDefenseCache", (LookMode) 3, (LookMode) 2, ref AirDefensePositionTracker.saveableWorldObjects, ref AirDefensePositionTracker.saveableAirDefenses, true, false, false);
  }
}
