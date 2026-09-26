// Decompiled with JetBrains decompiler
// Type: Vehicles.World.FlightNode
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld.Planet;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.World;

[PublicAPI]
public class FlightNode : IExposable
{
  private PlanetTile tile;
  private Vector3 origin;
  private bool spaceObject;

  public PlanetTile Tile => this.tile;

  public WorldObject WorldObject { get; private set; }

  public FlightNode()
  {
  }

  public FlightNode(PlanetTile tile)
  {
    this.tile = tile;
    this.WorldObject = WorldHelper.WorldObjectAt(tile);
    this.origin = WorldHelper.GetTilePos(tile, this.WorldObject, out this.spaceObject);
  }

  public FlightNode(GlobalTargetInfo target)
  {
    this.tile = ((GlobalTargetInfo) ref target).Tile;
    if (((GlobalTargetInfo) ref target).HasWorldObject)
    {
      this.WorldObject = ((GlobalTargetInfo) ref target).WorldObject;
      this.tile = this.WorldObject.Tile;
    }
    this.origin = WorldHelper.GetTilePos(this.tile, this.WorldObject, out this.spaceObject);
  }

  public Vector3 GetCenter(AerialVehicleInFlight aerialVehicle)
  {
    return this.WorldObject != null && this.WorldObject != aerialVehicle ? this.WorldObject.DrawPos : this.origin;
  }

  public void RecalculateCenter()
  {
    if (!this.spaceObject)
      return;
    this.origin = WorldHelper.GetTilePos(this.tile, this.WorldObject, out bool _);
  }

  public static implicit operator GlobalTargetInfo(FlightNode node)
  {
    return node.WorldObject != null ? new GlobalTargetInfo(node.WorldObject) : new GlobalTargetInfo(node.Tile);
  }

  public void ExposeData()
  {
    Scribe_Values.Look<PlanetTile>(ref this.tile, "tile", new PlanetTile(), false);
    if (Scribe.mode != 4)
      return;
    this.WorldObject = WorldHelper.WorldObjectAt(this.tile);
    this.origin = WorldHelper.GetTilePos(this.tile, this.WorldObject, out this.spaceObject);
  }
}
