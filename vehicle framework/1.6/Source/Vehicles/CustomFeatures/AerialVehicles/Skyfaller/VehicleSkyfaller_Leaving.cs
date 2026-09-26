// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleSkyfaller_Leaving
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vehicles.World;
using Verse;

#nullable disable
namespace Vehicles;

public class VehicleSkyfaller_Leaving : VehicleSkyfaller
{
  public IArrivalAction arrivalAction;
  public List<FlightNode> flightPath;
  public bool orderRecon;
  public bool createWorldObject = true;
  private int delayLaunchingTicks;

  [UsedImplicitly]
  [Obsolete("Implemented for Xml Deserialization only. Use VehicleSkyfallerMaker instead.", true)]
  public VehicleSkyfaller_Leaving()
  {
  }

  protected virtual void DrawAt(Vector3 drawLoc, bool flip = false)
  {
    this.launchProtocolDrawPos = this.vehicle.CompVehicleLauncher.launchProtocol.Draw(this.RootPos, 0.0f).drawPos;
  }

  protected override void Tick()
  {
    --this.delayLaunchingTicks;
    if (this.delayLaunchingTicks > 0)
      return;
    base.Tick();
    if (!this.vehicle.CompVehicleLauncher.launchProtocol.FinishedAnimation((VehicleSkyfaller) this))
      return;
    this.LeaveMap();
  }

  protected override void LeaveMap()
  {
    this.vehicle.CompVehicleLauncher.launchProtocol.Release();
    if (!this.createWorldObject)
    {
      base.LeaveMap();
    }
    else
    {
      if (GenCollection.Any<FlightNode>(this.flightPath, (Predicate<FlightNode>) (node =>
      {
        PlanetTile tile = node.Tile;
        return !((PlanetTile) ref tile).Valid;
      })))
      {
        Log.Error("AerialVehicle left the map but has a flight path Tile that is invalid. Removing node from path.");
        this.flightPath.RemoveAll((Predicate<FlightNode>) (node =>
        {
          PlanetTile tile = node.Tile;
          return !((PlanetTile) ref tile).Valid;
        }));
        if (GenList.NullOrEmpty<FlightNode>((IList<FlightNode>) this.flightPath))
          return;
      }
      if (((Verse.Thing) this.vehicle).Faction.IsPlayer)
        Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_AerialVehicleLeft", NamedArgument.op_Implicit(((Entity) this.vehicle).LabelShort))), MessageTypeDefOf.PositiveEvent, true);
      if (this.createWorldObject)
      {
        AerialVehicleInFlight aerialVehicleInFlight = AerialVehicleInFlight.Create(this.vehicle, this.Map.Tile);
        aerialVehicleInFlight.OrderFlyToTiles(this.flightPath, this.arrivalAction);
        if (this.orderRecon)
        {
          FlightPath flightPath1 = aerialVehicleInFlight.flightPath;
          List<FlightNode> flightPath2 = this.flightPath;
          PlanetTile tile = flightPath2[flightPath2.Count - 1].Tile;
          flightPath1.ReconCircleAt(tile);
        }
      }
      this.vehicle.EventRegistry[VehicleEventDefOf.AerialVehicleLeftMap].ExecuteEvents();
      base.LeaveMap();
    }
  }

  public override void SpawnSetup(Map map, bool respawningAfterLoad)
  {
    base.SpawnSetup(map, respawningAfterLoad);
    if (respawningAfterLoad)
      return;
    this.vehicle.CompVehicleLauncher.launchProtocol.Prepare(map, this.Position, this.Rotation);
    this.vehicle.CompVehicleLauncher.launchProtocol.OrderProtocol(LaunchProtocol.LaunchType.Takeoff);
    this.delayLaunchingTicks = this.vehicle.CompVehicleLauncher.launchProtocol.CurAnimationProperties.delayByTicks;
  }

  public override void ExposeData()
  {
    base.ExposeData();
    Scribe_Collections.Look<FlightNode>(ref this.flightPath, "flightPath", (LookMode) 0, Array.Empty<object>());
    Scribe_Values.Look<bool>(ref this.orderRecon, "orderRecon", false, false);
    Scribe_Values.Look<bool>(ref this.createWorldObject, "createWorldObject", true, false);
    Scribe_Values.Look<int>(ref this.delayLaunchingTicks, "delayLaunchingTicks", 0, false);
  }
}
