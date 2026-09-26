// Decompiled with JetBrains decompiler
// Type: Vehicles.World.FlightPath
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.World;

[PublicAPI]
public class FlightPath : IExposable
{
  private IArrivalAction arrivalAction;
  private List<FlightNode> nodes = new List<FlightNode>();
  private List<PlanetTile> reconTiles = new List<PlanetTile>();
  private AerialVehicleInFlight aerialVehicle;
  private bool circling;
  private bool currentlyInRecon;

  public FlightPath(AerialVehicleInFlight aerialVehicle) => this.aerialVehicle = aerialVehicle;

  public List<FlightNode> Path => this.nodes;

  public FlightNode First => this.nodes[0];

  public FlightNode Last
  {
    get
    {
      List<FlightNode> nodes = this.nodes;
      return nodes[nodes.Count - 1];
    }
  }

  public int Count => this.nodes.Count;

  public bool Empty => GenList.NullOrEmpty<FlightNode>((IList<FlightNode>) this.nodes);

  public FlightNode this[int index] => this.nodes[index];

  public bool Circling => this.circling;

  public bool InRecon => this.currentlyInRecon;

  public IArrivalAction ArrivalAction => this.arrivalAction;

  public float TotalDistance { get; private set; }

  public float DistanceLeft
  {
    get
    {
      float distanceLeft = 0.0f;
      Vector3 source = ((WorldObject) this.aerialVehicle).DrawPos;
      foreach (FlightNode node in this.nodes)
      {
        Vector3 tilePos = WorldHelper.GetTilePos(node.Tile);
        distanceLeft += Ext_Math.SphericalDistance(source, tilePos);
        source = tilePos;
      }
      return distanceLeft;
    }
  }

  public void VerifyFlightPath() => this.First.RecalculateCenter();

  public void RecacheCenters()
  {
    if (GenList.NullOrEmpty<FlightNode>((IList<FlightNode>) this.nodes))
      return;
    for (int index = 0; index < this.nodes.Count; ++index)
      this.nodes[index].RecalculateCenter();
  }

  public void AddNode(PlanetTile tile)
  {
    this.nodes.Add(new FlightNode(tile));
    this.RecalculateDistance();
  }

  public void PushCircleAt(PlanetTile tile)
  {
    this.reconTiles.Clear();
    Ext_World.GetTileNeighbors(tile, this.reconTiles, this.aerialVehicle.Vehicle.CompVehicleLauncher.ReconDistance, new Vector3?(((WorldObject) this.aerialVehicle).DrawPos));
    foreach (PlanetTile reconTile in this.reconTiles)
      this.nodes.Insert(0, new FlightNode(reconTile));
    this.circling = true;
  }

  public void ReconCircleAt(PlanetTile tile)
  {
    if (PlanetTile.op_Equality(this.Last.Tile, tile))
      GenCollection.Pop<FlightNode>(this.nodes);
    this.reconTiles.Clear();
    Ext_World.GetTileNeighbors(tile, this.reconTiles, this.aerialVehicle.Vehicle.CompVehicleLauncher.ReconDistance, new Vector3?(((WorldObject) this.aerialVehicle).DrawPos));
    foreach (PlanetTile reconTile in this.reconTiles)
      this.nodes.Add(new FlightNode(reconTile));
    this.circling = true;
    this.aerialVehicle.recon = true;
    this.nodes.Add(new FlightNode(tile));
    this.aerialVehicle.GenerateMapForRecon(tile);
  }

  public void ConsumeNode(bool haltCircle = false)
  {
    FlightNode target = this.nodes.PopAt<FlightNode>(0);
    this.aerialVehicle.Tile = target.Tile;
    this.currentlyInRecon = this.reconTiles.Contains(this.aerialVehicle.Tile);
    if (this.circling & haltCircle)
    {
      PlanetTile tile = this.Last.Tile;
      this.ResetPath();
      this.AddNode(tile);
    }
    else if (this.nodes.Count <= 1 && this.circling)
    {
      if (this.aerialVehicle.recon)
        this.ReconCircleAt(this.First.Tile);
      else
        this.PushCircleAt(this.First.Tile);
    }
    if (!GenList.NullOrEmpty<FlightNode>((IList<FlightNode>) this.nodes))
      return;
    this.arrivalAction?.Arrived((GlobalTargetInfo) target);
  }

  public void ResetPath()
  {
    this.nodes.Clear();
    this.reconTiles.Clear();
    this.circling = false;
    this.aerialVehicle.recon = false;
    this.currentlyInRecon = false;
    this.TotalDistance = 0.0f;
  }

  public void NewPath(FlightPath flightPath)
  {
    this.ResetPath();
    this.nodes.AddRange((IEnumerable<FlightNode>) flightPath.Path);
    this.arrivalAction = flightPath.arrivalAction;
    this.RecalculateDistance();
  }

  public void NewPath(List<FlightNode> path, IArrivalAction arrivalAction)
  {
    this.ResetPath();
    this.nodes.AddRange((IEnumerable<FlightNode>) path);
    this.arrivalAction = arrivalAction;
    this.RecalculateDistance();
  }

  private void RecalculateDistance()
  {
    this.TotalDistance = 0.0f;
    if (GenList.NullOrEmpty<FlightNode>((IList<FlightNode>) this.nodes))
      return;
    FlightNode node1 = this.nodes[0];
    for (int index = 1; index < this.nodes.Count; ++index)
    {
      FlightNode node2 = this.nodes[index];
      this.TotalDistance += Ext_Math.SphericalDistance(WorldHelper.GetTilePos(node1.Tile), WorldHelper.GetTilePos(node2.Tile));
    }
  }

  public void ExposeData()
  {
    Scribe_Collections.Look<FlightNode>(ref this.nodes, "nodes", (LookMode) 0, Array.Empty<object>());
    Scribe_Deep.Look<IArrivalAction>(ref this.arrivalAction, "arrivalAction", Array.Empty<object>());
    Scribe_Collections.Look<PlanetTile>(ref this.reconTiles, "reconTiles", (LookMode) 0, Array.Empty<object>());
    Scribe_References.Look<AerialVehicleInFlight>(ref this.aerialVehicle, "aerialVehicle", false);
    Scribe_Values.Look<bool>(ref this.circling, "circling", false, false);
    Scribe_Values.Look<bool>(ref this.currentlyInRecon, "currentlyInRecon", false, false);
    if (Scribe.mode != 2)
      return;
    this.RecalculateDistance();
  }

  public static void DrawPath(Vector3 start, Vector3 end, Material material)
  {
    int num1 = Mathf.CeilToInt(Ext_Math.SphericalDistance(start, end) * 100f / 5f);
    start = Vector3.op_Addition(start, Vector3.op_Multiply(((Vector3) ref start).normalized, 0.05f));
    end = Vector3.op_Addition(end, Vector3.op_Multiply(((Vector3) ref end).normalized, 0.05f));
    Vector3 vector3_1 = start;
    for (int index = 1; index <= num1; ++index)
    {
      float num2 = (float) index / (float) num1;
      Vector3 vector3_2 = Vector3.Slerp(start, end, num2);
      GenDraw.DrawWorldLineBetween(vector3_1, vector3_2, material, 0.5f);
      vector3_1 = vector3_2;
    }
  }
}
