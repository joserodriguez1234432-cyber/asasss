// Decompiled with JetBrains decompiler
// Type: Vehicles.AerialVehicleArrivalModeWorker_EdgeDrop
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System;
using Verse;

#nullable disable
namespace Vehicles;

public class AerialVehicleArrivalModeWorker_EdgeDrop : AerialVehicleArrivalModeWorker
{
  public override void VehicleArrived(VehiclePawn vehicle, LaunchProtocol launchProtocol, Map map)
  {
    Rot4 vehicleRotation = (Rot4?) launchProtocol.LandingProperties?.forcedRotation ?? Rot4.Random;
    IntVec2 size = ((BuildableDef) vehicle.VehicleDef).Size;
    IntVec3 result;
    bool randomEdgeCell = CellFinderExtended.TryFindRandomEdgeCell(((Rot4) ref vehicleRotation).Opposite, map, (Predicate<IntVec3>) (cell =>
    {
      vehicle.ClampToMap(cell, map, 1);
      return !MapHelper.NonStandableOrVehicleBlocked(vehicle, Current.Game.CurrentMap, cell, vehicleRotation);
    }), size.x > size.z ? size.x : size.z, out result);
    if (!randomEdgeCell)
      randomEdgeCell = CellFinderExtended.TryFindRandomEdgeCell(((Rot4) ref vehicleRotation).Opposite, map, (Predicate<IntVec3>) (cell =>
      {
        vehicle.ClampToMap(cell, map, 1);
        return !MapHelper.ImpassableOrVehicleBlocked(vehicle, Current.Game.CurrentMap, cell, vehicleRotation);
      }), size.x > size.z ? size.x : size.z, out result);
    if (!randomEdgeCell)
    {
      AerialVehicleArrivalModeDefOf.TargetedLanding.Worker.VehicleArrived(vehicle, launchProtocol, map);
    }
    else
    {
      IntVec3 map1 = vehicle.ClampToMap(result, map, 1);
      GenSpawn.Spawn((Thing) VehicleSkyfallerMaker.MakeSkyfaller(vehicle.CompVehicleLauncher.Props.skyfallerIncoming, vehicle), map1, map, vehicleRotation, (WipeMode) 0, false, false);
    }
  }

  public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
  {
    Map target = (Map) parms.target;
    if (!parms.raidArrivalModeForQuickMilitaryAid)
      parms.podOpenDelay = 520;
    parms.spawnRotation = Rot4.Random;
    if (!((IntVec3) ref parms.spawnCenter).IsValid)
    {
      bool flag1 = parms.faction == Faction.OfMechanoids;
      bool flag2 = parms.faction != null && FactionUtility.HostileTo(parms.faction, Faction.OfPlayer);
      if (Rand.Chance(0.4f) && !flag1 && target.listerBuildings.ColonistsHaveBuildingWithPowerOn(ThingDefOf.OrbitalTradeBeacon))
        parms.spawnCenter = DropCellFinder.TradeDropSpot(target);
      else if (!DropCellFinder.TryFindRaidDropCenterClose(ref parms.spawnCenter, target, !flag1 & flag2, !flag1, true, -1))
      {
        parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeDrop;
        return parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms);
      }
    }
    return true;
  }
}
