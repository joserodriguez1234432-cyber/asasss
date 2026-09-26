// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.ScenPart_StartOnMapVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class ScenPart_StartOnMapVehicle : ScenPart
{
  public virtual void GenerateIntoMap(Map map)
  {
    if (Find.GameInitData == null)
      return;
    List<Thing> source = new List<Thing>();
    foreach (ScenPart allPart in Find.Scenario.AllParts)
      source.AddRange(allPart.PlayerStartingThings());
    foreach (Pawn startingAndOptionalPawn in Find.GameInitData.startingAndOptionalPawns)
    {
      foreach (ThingDefCount thingDefCount in Find.GameInitData.startingPossessions[startingAndOptionalPawn])
        source.Add(StartingPawnUtility.GenerateStartingPossession(thingDefCount));
    }
    VehiclePawnWithMap vehicle = source.OfType<VehiclePawnWithMap>().FirstOrDefault<VehiclePawnWithMap>();
    if (vehicle == null)
      return;
    source.Remove((Thing) vehicle);
    ((Thing) vehicle).SetFactionDirect(Faction.OfPlayer);
    IntVec3 intVec3 = CellFinderExtended.RandomSpawnCellForPawnNear(map.Center, map, (Pawn) vehicle, (Predicate<IntVec3>) (c => Ext_Vehicles.CellRectStandable((VehiclePawn) vehicle, map, new IntVec3?(c), new Rot4?())), false, 4);
    GenSpawn.Spawn((Thing) vehicle, intVec3, map, (WipeMode) 0);
    List<List<Thing>> list2 = new List<List<Thing>>();
    foreach (Pawn startingAndOptionalPawn in Find.GameInitData.startingAndOptionalPawns)
      list2.Add(new List<Thing>(1)
      {
        (Thing) startingAndOptionalPawn
      });
    int index = 0;
    foreach (Thing thing in source)
    {
      if (thing.def.CanHaveFaction)
        thing.SetFactionDirect(Faction.OfPlayer);
      list2[index].Add(thing);
      ++index;
      if (index >= list2.Count)
        index = 0;
    }
    Map vehicleMap = vehicle.VehicleMap;
    LongEventHandler.ExecuteWhenFinished((Action) (() => DropPodUtility.DropThingGroupsNear(vehicle.VehicleMap.Center, vehicle.VehicleMap, list2, 110, true, true, true, true, false, false, (Faction) null)));
  }
}
