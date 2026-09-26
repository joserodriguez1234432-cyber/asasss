// Decompiled with JetBrains decompiler
// Type: Vehicles.AerialVehicleArrivalModeWorker_TargetedDrop
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using Verse;

#nullable disable
namespace Vehicles;

public class AerialVehicleArrivalModeWorker_TargetedDrop : AerialVehicleArrivalModeWorker
{
  public override void VehicleArrived(VehiclePawn vehicle, LaunchProtocol launchProtocol, Map map)
  {
    CameraJumper.TryJump(map.Center, map, (CameraJumper.MovementMode) 0);
    LandingTargeter.Instance.BeginTargetingAndFocusMap(vehicle, map, (Action<LocalTargetInfo, Rot4>) ((target, rot) => GenSpawn.Spawn((Thing) VehicleSkyfallerMaker.MakeSkyfaller(vehicle.CompVehicleLauncher.Props.skyfallerIncoming, vehicle), ((LocalTargetInfo) ref target).Cell, map, rot, (WipeMode) 0, false, false)), allowRotating: vehicle.VehicleDef.rotatable, forcedTargeting: true);
    Find.GameEnder.CheckOrUpdateGameOver();
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
