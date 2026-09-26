// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleCaravanIncidentUtility
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Vehicles.World;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class VehicleCaravanIncidentUtility
{
  public static int CalculateIncidentMapSize(
    List<VehiclePawn> caravanVehicles,
    List<VehiclePawnWithMap> enemyVehicles)
  {
    List<VehiclePawn> list = GenCollection.ConcatIfNotNull<VehiclePawn>((IEnumerable<VehiclePawn>) caravanVehicles, (IEnumerable<VehiclePawn>) enemyVehicles).ToList<VehiclePawn>();
    int num = list.Select<VehiclePawn, int>((Func<VehiclePawn, int>) (v => ((Thing) v).def.size.x)).Concat<int>(list.Select<VehiclePawn, int>((Func<VehiclePawn, int>) (v => ((Thing) v).def.size.z))).Max();
    return Mathf.Clamp(Mathf.RoundToInt(Mathf.Sqrt((float) Mathf.RoundToInt((float) (list.Count * 300 * num)))), 75, 200);
  }

  public static Map SetupCaravanAttackMap(
    VehicleCaravan caravan,
    List<Pawn> enemies,
    bool sendLetterIfRelatedPawns,
    WorldObjectDef mapParent,
    CaravanEnterMode enterMode = 1)
  {
    if (caravan.Vehicles.FirstOrDefault<VehiclePawn>() == null)
      return (Map) null;
    int incidentMapSize = VehicleCaravanIncidentUtility.CalculateIncidentMapSize(caravan.VehiclesListForReading, (List<VehiclePawnWithMap>) null);
    Map generateMapForIncident = CaravanIncidentUtility.GetOrGenerateMapForIncident((Caravan) caravan, new IntVec3(incidentMapSize, 1, incidentMapSize), mapParent);
    if (generateMapForIncident == null)
      return (Map) null;
    VehicleCaravan vehicleCaravan = caravan;
    Map map = generateMapForIncident;
    EnterMapUtilityVehicles.SpawnParams spawnParams;
    // ISSUE: explicit constructor call
    ((EnterMapUtilityVehicles.SpawnParams) ref spawnParams).\u002Ector(enterMode);
    spawnParams.draftColonists = true;
    ref EnterMapUtilityVehicles.SpawnParams local = ref spawnParams;
    EnterMapUtilityVehicles.EnterMap(vehicleCaravan, map, ref local);
    IntVec3 intVec3_1 = enterMode == 1 ? generateMapForIncident.Center : CellFinder.RandomEdgeCell(generateMapForIncident);
    for (int index = 0; index < enemies.Count; ++index)
    {
      IntVec3 intVec3_2 = CellFinder.RandomSpawnCellForPawnNear(intVec3_1, generateMapForIncident, 4);
      GenSpawn.Spawn((Thing) enemies[index], intVec3_2, generateMapForIncident, Rot4.Random, (WipeMode) 0, false, false);
    }
    if (sendLetterIfRelatedPawns)
      PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send((IEnumerable<Pawn>) enemies, TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("LetterRelatedPawnsGroupGeneric", NamedArgument.op_Implicit(Faction.OfPlayer.def.pawnsPlural))), LetterDefOf.NeutralEvent, true, true);
    return generateMapForIncident;
  }

  public static Map SetupCaravanAttackMap(
    VehicleCaravan caravan,
    List<VehiclePawnWithMap> vehicles,
    List<Pawn> enemies,
    bool sendLetterIfRelatedPawns,
    WorldObjectDef mapParent,
    CaravanEnterMode enterMode = 1)
  {
    try
    {
      VehiclePawn vehiclePawn = caravan.Vehicles.FirstOrDefault<VehiclePawn>();
      if (vehiclePawn == null)
        return (Map) null;
      int incidentMapSize = VehicleCaravanIncidentUtility.CalculateIncidentMapSize(caravan.VehiclesListForReading, vehicles);
      Map generateMapForIncident = CaravanIncidentUtility.GetOrGenerateMapForIncident((Caravan) caravan, new IntVec3(incidentMapSize, 1, incidentMapSize), mapParent);
      if (generateMapForIncident == null)
        return (Map) null;
      EnterMapUtilityVehicles.EnterMap(caravan, generateMapForIncident, ref new EnterMapUtilityVehicles.SpawnParams(enterMode)
      {
        draftColonists = true
      });
      Map map = generateMapForIncident;
      List<VehiclePawnWithMap> vehicles1 = vehicles;
      List<Pawn> enemies1 = enemies;
      CellRect cellRect = CellRect.WholeMap(generateMapForIncident);
      Rot4 closestEdge = ((CellRect) ref cellRect).GetClosestEdge(((Thing) vehiclePawn).Position);
      Rot4? edge = new Rot4?(((Rot4) ref closestEdge).Opposite);
      VehicleCaravanIncidentUtility.SpawnEnemies(map, vehicles1, enemies1, edge);
      if (sendLetterIfRelatedPawns)
        PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send((IEnumerable<Pawn>) enemies, TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("LetterRelatedPawnsGroupGeneric", NamedArgument.op_Implicit(Faction.OfPlayer.def.pawnsPlural))), LetterDefOf.NeutralEvent, true, true);
      return generateMapForIncident;
    }
    catch (Exception ex)
    {
      vehicles.ForEach((Action<VehiclePawnWithMap>) (v => ((Thing) v).Destroy((DestroyMode) 0)));
      VMF_Log.Error($"Error within SetupCaravanAttackMap: {ex}");
      return (Map) null;
    }
  }

  public static void SpawnEnemies(
    Map map,
    List<VehiclePawnWithMap> vehicles,
    List<Pawn> enemies,
    Rot4? edge = null)
  {
    GenCollection.SortBy<VehiclePawnWithMap, float>(vehicles, (Func<VehiclePawnWithMap, float>) (v =>
    {
      CompNpcVehicleMap compNpcVehicleMap = v.CompNpcVehicleMap;
      return compNpcVehicleMap == null ? 0.0f : compNpcVehicleMap.Props.pawnCountWeight;
    }));
    int[] numArray = PawnAllocation();
    int index = 0;
    VehiclePathingSystem mapping = ComponentCache.GetCachedMapComponent<VehiclePathingSystem>(map);
    for (int index1 = 0; index1 < vehicles.Count; ++index1)
    {
      VehiclePawnWithMap vehicle = vehicles[index1];
      int count = numArray[index1];
      Map vehicleMap = vehicle.VehicleMap;
      if (vehicle.CompNpcVehicleMap != null)
      {
        vehicle.CompNpcVehicleMap.SetParams(count);
        PrefabUtility.SpawnPrefab(vehicle.CompNpcVehicleMap.Params.prefabDef, vehicleMap, vehicleMap.Center, Rot4.North, ((Thing) vehicle).Faction, (List<Thing>) null, (Func<PrefabThingData, Tuple<ThingDef, ThingDef>>) null, (Action<Thing>) (thing =>
        {
          if (!(thing is ThingWithComps thingWithComps2))
            return;
          CompPowerBattery compPowerBattery;
          if (ThingCompUtility.TryGetComp<CompPowerBattery>(thingWithComps2, ref compPowerBattery))
            compPowerBattery.SetStoredEnergyPct(1f);
          CompDrawAdditionalGraphicsOpacity additionalGraphicsOpacity;
          if (!ThingCompUtility.TryGetComp<CompDrawAdditionalGraphicsOpacity>(thingWithComps2, ref additionalGraphicsOpacity))
            return;
          additionalGraphicsOpacity.Opacity = 0.5f;
        }), false);
        vehicle.Resize();
      }
      if (SpawnVehicle(vehicle))
        LongEventHandler.ExecuteWhenFinished((Action) (() =>
        {
          List<VehiclePawn> allClaimants = ComponentCache.GetDetachedMapComponent<VehiclePositionManager>(vehicle.VehicleMap).AllClaimants;
          for (int index2 = index; index < index2 + count; ++index)
          {
            Pawn enemy = enemies[index];
            if (vehicle.SeatsAvailable > 0)
            {
              if (!vehicle.TryAddPawn(enemy))
                VMF_Log.Error($"Unable to add {enemy} to {vehicle} during raid generation.");
              else
                continue;
            }
            VehiclePawn vehiclePawn = GenCollection.FirstOrDefault<VehiclePawn>(allClaimants, (Predicate<VehiclePawn>) (v => v.SeatsAvailable > 0));
            if (vehiclePawn != null)
            {
              if (!vehiclePawn.TryAddPawn(enemy))
              {
                VMF_Log.Error($"Unable to add {enemy} to {vehicle} during raid generation.");
              }
              else
              {
                if (vehiclePawn != null)
                {
                  CompVehicleTurrets compVehicleTurrets = vehiclePawn.CompVehicleTurrets;
                  if (compVehicleTurrets != null && compVehicleTurrets.CanDeploy && !compVehicleTurrets.Deployed)
                  {
                    vehiclePawn.CompVehicleTurrets.ToggleDeployment();
                    continue;
                  }
                  continue;
                }
                continue;
              }
            }
            IntVec3 intVec3 = CellFinderExtended.RandomSpawnCellForPawnNear(vehicleMap.Center, vehicleMap, enemy, (Predicate<IntVec3>) (_ => true), false, 4);
            GenSpawn.Spawn((Thing) enemy, intVec3, vehicleMap, Rot4.South, (WipeMode) 0, false, false);
          }
        }));
    }

    int[] PawnAllocation()
    {
      int[] numArray = new int[vehicles.Count];
      if (vehicles.Count == 0)
        return numArray;
      int count = enemies.Count;
      int num1 = count;
      float num2 = vehicles.Sum<VehiclePawnWithMap>((Func<VehiclePawnWithMap, float>) (v =>
      {
        CompNpcVehicleMap compNpcVehicleMap = v.CompNpcVehicleMap;
        return compNpcVehicleMap == null ? 0.0f : compNpcVehicleMap.Props.pawnCountWeight;
      }));
      if ((double) num2 == 0.0)
        num2 = 1f;
      for (int index = 0; index < vehicles.Count; ++index)
      {
        CompNpcVehicleMap compNpcVehicleMap = vehicles[index].CompNpcVehicleMap;
        int num3 = Mathf.FloorToInt((compNpcVehicleMap != null ? compNpcVehicleMap.Props.pawnCountWeight : 0.0f) / num2 * (float) count);
        numArray[index] = num3;
        num1 -= num3;
      }
      for (int index = 0; index < num1; ++index)
        ++numArray[index % vehicles.Count];
      return numArray;
    }

    bool SpawnVehicle(VehiclePawnWithMap vehicle)
    {
      Rot4 rot = edge ?? Rot4.Random;
      ((Thing) vehicle).Rotation = rot;
      VehiclePathingSystem.VehiclePathData vehiclePathData = mapping[vehicle.VehicleDef];
      if (!vehiclePathData.VehiclePathGrid.Enabled)
        vehiclePathData.VehiclePathGrid.RecalculateAllPerceivedPathCosts();
      if (!vehiclePathData.VehicleRegionAndRoomUpdater.Enabled)
        vehiclePathData.VehicleRegionAndRoomUpdater.Init();
      IntVec3 root;
      if (VehicleCaravanIncidentUtility.TryFindNearEdgeCell(map, vehicle.VehicleDef, rot, out root))
        return GenSpawn.Spawn((Thing) vehicle, root, map, rot, (WipeMode) 0, false, false) != null;
      VMF_Log.Error($"Unable to find spawn position for vehicle {vehicle}");
      ((Thing) vehicle).Destroy((DestroyMode) 0);
      return false;
    }
  }

  private static bool TryFindNearEdgeCell(
    Map map,
    VehicleDef vehicleDef,
    Rot4 rot,
    out IntVec3 root)
  {
    if (!CellFinderExtended.TryFindRandomEdgeCellWith(new Predicate<IntVec3>(Validator), map, rot, vehicleDef, CellFinder.EdgeRoadChance_Hostile, ref root))
      return false;
    root = CellFinderExtended.RandomClosewalkCellNear(root, map, vehicleDef, 5, (Predicate<IntVec3>) null);
    return true;

    bool Validator(IntVec3 cell)
    {
      return GenGridVehicles.Standable(cell, vehicleDef, map) && !GridsUtility.Fogged(cell, map);
    }
  }

  public static bool ValidThreatVehicle(
    VehicleDef vehicleDef,
    VehicleCategory category,
    PawnsArrivalModeDef arrivalModeDef,
    Faction faction,
    float points)
  {
    bool flag = GenTypes.SameOrSubclassOf<VehiclePawnWithMap>(((ThingDef) vehicleDef).thingClass) && ((ThingDef) vehicleDef).HasComp<CompNpcVehicleMap>() && RaidInjectionHelper.ValidRaiderVehicle(vehicleDef, category, arrivalModeDef, faction, points);
    if (flag)
    {
      VehicleMapProps_Unique modExtension = ((Def) vehicleDef).GetModExtension<VehicleMapProps_Unique>();
      flag = modExtension == null || modExtension.baseDef == null;
    }
    return flag && UniqueVehicleUtility.AllowGenerate(vehicleDef);
  }

  public static bool ValidSeaThreatVehicle(
    VehicleDef vehicleDef,
    VehicleCategory category,
    PawnsArrivalModeDef arrivalModeDef,
    Faction faction,
    float points)
  {
    bool flag = GenTypes.SameOrSubclassOf<VehiclePawnWithMap>(((ThingDef) vehicleDef).thingClass) && ((ThingDef) vehicleDef).HasComp<CompNpcVehicleMap>();
    if (flag)
    {
      VehicleMapProps_Unique modExtension = ((Def) vehicleDef).GetModExtension<VehicleMapProps_Unique>();
      flag = modExtension == null || modExtension.baseDef == null;
    }
    if (!flag || !UniqueVehicleUtility.AllowGenerate(vehicleDef) || vehicleDef.type != null || (vehicleDef.vehicleCategory & category) != category || (double) vehicleDef.combatPower > (double) points || faction.def.techLevel < ((ThingDef) vehicleDef).techLevel || (vehicleDef.enabled & 2) == null || vehicleDef.npcProperties == null)
      return false;
    return vehicleDef.npcProperties.raidParams == null || vehicleDef.npcProperties.raidParams.Allows(faction, arrivalModeDef);
  }
}
