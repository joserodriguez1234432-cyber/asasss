// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.GenStep_MapVehicleThreat
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class GenStep_MapVehicleThreat : GenStep
{
  public virtual int SeedPart => 167961163;

  protected virtual bool ValidRaiderVehicle(
    VehicleDef vehicleDef,
    VehicleCategory category,
    PawnsArrivalModeDef arrivalModeDef,
    Faction faction,
    float points)
  {
    return VehicleCaravanIncidentUtility.ValidThreatVehicle(vehicleDef, category, arrivalModeDef, faction, points);
  }

  protected virtual List<VehiclePawnWithMap> GenerateVehicles(Faction faction, SitePart sitePart)
  {
    float generatePawnGroup = faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat, (PawnGroupMakerParms) null);
    float points = Mathf.Max(sitePart.parms.points, generatePawnGroup);
    List<VehicleDef> list1 = DefDatabase<VehicleDef>.AllDefs.Where<VehicleDef>((Func<VehicleDef, bool>) (vehicleDef => this.ValidRaiderVehicle(vehicleDef, (VehicleCategory) 4, (PawnsArrivalModeDef) null, faction, points))).ToList<VehicleDef>();
    List<VehiclePawnWithMap> list2 = MapVehicleGroupMakerUtility.GenerateVehicles(faction, points, IncidentWorker_Ambush_EnemyMapVehicle.VehicleCountByPointsCurve, list1).ToList<VehiclePawnWithMap>();
    points = Mathf.Max(points - list2.Sum<VehiclePawnWithMap>((Func<VehiclePawnWithMap, float>) (v => v.VehicleDef.combatPower)), generatePawnGroup);
    return list2;
  }

  protected virtual List<Pawn> GeneratePawns(Faction faction, SitePart sitePart)
  {
    return PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms()
    {
      groupKind = PawnGroupKindDefOf.Combat,
      tile = ((WorldObject) sitePart.site).Tile,
      faction = faction,
      points = Mathf.Max(sitePart.parms.points, faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat, (PawnGroupMakerParms) null))
    }, true).ToList<Pawn>();
  }

  public virtual void Generate(Map map, GenStepParams parms)
  {
    Faction faction1 = ((WorldObject) parms.sitePart.site).Faction;
    Faction faction2 = faction1 == null || faction1.IsPlayer ? Find.FactionManager.RandomEnemyFaction(false, false, false, (TechLevel) 0) : ((WorldObject) parms.sitePart.site).Faction;
    VehicleCaravanIncidentUtility.SpawnEnemies(map, this.GenerateVehicles(faction2, parms.sitePart), this.GeneratePawns(faction2, parms.sitePart));
  }
}
