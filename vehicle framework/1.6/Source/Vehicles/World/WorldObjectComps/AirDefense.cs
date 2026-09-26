// Decompiled with JetBrains decompiler
// Type: Vehicles.World.AirDefense
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vehicles.Rendering;
using Verse;

#nullable disable
namespace Vehicles.World;

public class AirDefense : IExposable, ILoadReferenceable
{
  protected const int TickRareInterval = 250;
  protected const int TickLongInterval = 2000;
  protected const float ConeFadeInterval = 0.005f;
  public int defenseBuildings = 3;
  public AntiAircraftDef antiAircraftDef;
  public WorldObject parent;
  protected AntiAircraftWorker worker;
  public float angle;
  protected Mesh coneMesh;
  protected Vector3 centerTile = Vector3.zero;
  public HashSet<AerialVehicleInFlight> activeTargets = new HashSet<AerialVehicleInFlight>();
  protected int cooldownTimer;
  protected int uniqueId = -1;
  public int searchDirection = 1;
  protected float spotlightAlpha = 1f;

  public AirDefense()
  {
  }

  public AirDefense(WorldObject parent)
  {
    this.parent = parent;
    this.uniqueId = VehicleIdManager.Instance.GetNextAirDefenseId();
    this.antiAircraftDef = AntiAircraftDefOf.FlakProjectile;
    if (parent.Faction == Faction.OfPlayerSilentFail)
      return;
    this.searchDirection = Rand.Chance(0.5f) ? 1 : -1;
  }

  public virtual float Arc => (float) this.antiAircraftDef.properties.arc;

  public virtual float MaxDistance => this.antiAircraftDef.properties.distance + 0.5f;

  public AerialVehicleInFlight CurrentTarget => this.Worker.CurrentTarget;

  public Vector3 CenterTile
  {
    get
    {
      if (Vector3.op_Equality(this.centerTile, Vector3.zero))
        this.centerTile = Find.WorldGrid.GetTileCenter(this.parent.Tile);
      return this.centerTile;
    }
  }

  public virtual AntiAircraftWorker Worker
  {
    get
    {
      if (this.worker == null)
        this.worker = (AntiAircraftWorker) Activator.CreateInstance(this.antiAircraftDef.antiAircraftWorker, (object) this, (object) this.antiAircraftDef);
      return this.worker;
    }
  }

  public Mesh SpotlightMesh
  {
    get
    {
      if (this.coneMesh == null)
        this.coneMesh = RenderHelper.NewConeMesh(Find.WorldGrid.AverageTileSize, this.antiAircraftDef.properties.arc);
      return this.coneMesh;
    }
  }

  public virtual void DrawSpotlightOverlay()
  {
    if (this.Worker.ShouldDrawSearchLight)
    {
      if ((double) this.spotlightAlpha < 1.0)
        this.spotlightAlpha = Mathf.Clamp01(this.spotlightAlpha + 0.005f);
      float maxDistance = this.MaxDistance;
      Vector3 centerTile = this.CenterTile;
      Vector3 normalized = ((Vector3) ref centerTile).normalized;
      Quaternion quaternion = Quaternion.op_Multiply(Quaternion.LookRotation(Vector3.Cross(normalized, Vector3.up), normalized), Quaternion.Euler(0.0f, this.angle - 90f, 0.0f));
      Vector3 vector3;
      // ISSUE: explicit constructor call
      ((Vector3) ref vector3).\u002Ector(maxDistance, 1f, maxDistance);
      Matrix4x4 matrix4x4 = new Matrix4x4();
      ((Matrix4x4) ref matrix4x4).SetTRS(Vector3.op_Addition(this.CenterTile, Vector3.op_Multiply(normalized, 0.025f)), quaternion, vector3);
      int worldLayer = WorldCameraManager.WorldLayer;
    }
    else
    {
      if ((double) this.spotlightAlpha <= 0.0)
        return;
      this.spotlightAlpha = Mathf.Clamp01(this.spotlightAlpha - 0.005f);
    }
  }

  public virtual void Attack()
  {
    this.Worker.Tick();
    if (Find.TickManager.TicksGame % 250 == 0)
      this.Worker.TickRare();
    if (Find.TickManager.TicksGame % 2000 != 0)
      return;
    this.Worker.TickLong();
  }

  public string GetUniqueLoadID() => $"AirDefense_{this.GetType()}_{this.uniqueId}";

  public virtual void ExposeData()
  {
    Scribe_References.Look<WorldObject>(ref this.parent, "parent", false);
    Scribe_Values.Look<int>(ref this.uniqueId, "uniqueId", 0, false);
    Scribe_Values.Look<int>(ref this.cooldownTimer, "cooldownTimer", 0, false);
    Scribe_Values.Look<int>(ref this.defenseBuildings, "defenseBuildings", 0, false);
    Scribe_Defs.Look<AntiAircraftDef>(ref this.antiAircraftDef, "antiAircraftDef");
    Scribe_Collections.Look<AerialVehicleInFlight>(ref this.activeTargets, "activeTargets", (LookMode) 3);
    if (Scribe.mode != 4)
      return;
    if (this.activeTargets == null)
      this.activeTargets = new HashSet<AerialVehicleInFlight>();
    this.antiAircraftDef = AntiAircraftDefOf.FlakProjectile;
  }
}
