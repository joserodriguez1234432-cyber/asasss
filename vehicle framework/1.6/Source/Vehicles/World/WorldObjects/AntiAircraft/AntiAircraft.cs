// Decompiled with JetBrains decompiler
// Type: Vehicles.World.AntiAircraft
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.World;

public abstract class AntiAircraft : DynamicDrawnWorldObject
{
  protected float transition;
  protected float speedPctPerTick;
  protected Vector3 directionFacing;
  protected Vector3 destination;
  protected Vector3 source;
  protected AerialVehicleInFlight target;
  protected WorldObject firedFrom;
  protected Graphic explosionGraphic;
  protected int explosionFrame;

  public virtual Vector3 Destination => this.destination;

  public virtual Vector3 DrawPos => Vector3.Slerp(this.source, this.destination, this.transition);

  public AntiAircraftDef AADef => this.def as AntiAircraftDef;

  public Graphic ExplosionGraphic
  {
    get
    {
      if (this.explosionGraphic == null)
        this.explosionGraphic = this.AADef.explosionGraphic?.Graphic;
      return this.explosionGraphic;
    }
  }

  public abstract void Initialize(
    WorldObject firedFrom,
    AerialVehicleInFlight target,
    Vector3 source);

  public virtual void Draw()
  {
    if (WorldObjectSelectionUtility.HiddenBehindTerrainNow((WorldObject) this))
      return;
    float averageTileSize = Find.WorldGrid.AverageTileSize;
    float num1 = (float) (1.0 + (double) ExpandableWorldObjectsUtility.TransitionPct((WorldObject) this) * (double) Find.WorldCameraDriver.AltitudePercent * 35.0);
    bool flag = this.explosionFrame >= 0 && this.ExplosionGraphic != null;
    float num2 = flag ? Mathf.Max(this.AADef.explosionGraphic.drawSize.x, this.AADef.explosionGraphic.drawSize.y) : this.AADef.drawSizeMultiplier;
    Vector3 drawPos = base.DrawPos;
    Vector3 normalized = ((Vector3) ref drawPos).normalized;
    Quaternion quaternion = Quaternion.op_Multiply(Quaternion.LookRotation(Vector3.Cross(normalized, this.directionFacing), normalized), Quaternion.Euler(0.0f, 90f, 0.0f));
    Vector3 vector3;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3).\u002Ector(averageTileSize * 0.7f * num1 * num2, 5f, averageTileSize * 0.7f * num1 * num2);
    Matrix4x4 matrix4x4 = new Matrix4x4();
    ((Matrix4x4) ref matrix4x4).SetTRS(Vector3.op_Addition(base.DrawPos, normalized), quaternion, vector3);
    int worldLayer = WorldCameraManager.WorldLayer;
    if (flag)
    {
      if (this.explosionGraphic is Graphic_Animate explosionGraphic)
        Graphics.DrawMesh(MeshPool.plane10, matrix4x4, explosionGraphic.MatAt(Rot4.North, this.explosionFrame), worldLayer);
      else
        Graphics.DrawMesh(MeshPool.plane10, matrix4x4, this.explosionGraphic.MatAt(Rot4.North, (Thing) null), worldLayer);
    }
    else
      Graphics.DrawMesh(MeshPool.plane10, matrix4x4, this.Material, worldLayer);
  }

  protected virtual void Tick()
  {
    base.Tick();
    this.transition += this.speedPctPerTick;
    if ((double) this.transition < 1.0)
      return;
    if (this.explosionFrame < 0 && this.ExplosionGraphic != null)
    {
      this.explosionFrame = this.AADef.framesForExplosion;
    }
    else
    {
      --this.explosionFrame;
      if (this.explosionFrame >= 0)
        return;
      base.Destroy();
    }
  }

  public virtual void Destroy()
  {
    if (Rand.Chance(this.AADef.accuracy))
    {
      AerialVehicleInFlight target = this.target;
      if ((target != null ? (target.Vehicle.CompVehicleLauncher.inFlight ? 1 : 0) : 0) != 0)
      {
        CellRect cellRect = GenAdj.OccupiedRect((Thing) this.target.Vehicle);
        IntVec3 randomCell = ((CellRect) ref cellRect).RandomCell;
        this.target.TakeDamage(new DamageInfo(DamageDefOf.Bomb, this.AADef.damage, 0.0f, -1f, (Thing) null, (BodyPartRecord) null, (ThingDef) null, (DamageInfo.SourceCategory) 0, (Thing) null, true, true, (QualityCategory) 2, true, false), ((IntVec3) ref randomCell).ToIntVec2);
      }
    }
    base.Destroy();
  }

  protected virtual void InitializeFacing()
  {
    Vector3 vector3 = Vector3.op_Subtraction(base.DrawPos, this.destination);
    this.directionFacing = ((Vector3) ref vector3).normalized;
  }

  public virtual void ExposeData()
  {
    base.ExposeData();
    Scribe_Values.Look<float>(ref this.transition, "transition", 0.0f, false);
    Scribe_Values.Look<float>(ref this.speedPctPerTick, "speedPctPerTick", 0.0f, false);
    Scribe_Values.Look<Vector3>(ref this.directionFacing, "directionFacing", new Vector3(), false);
    Scribe_Values.Look<Vector3>(ref this.destination, "destination", new Vector3(), false);
    Scribe_Values.Look<Vector3>(ref this.source, "source", new Vector3(), false);
    Scribe_References.Look<AerialVehicleInFlight>(ref this.target, "target", false);
    Scribe_References.Look<WorldObject>(ref this.firedFrom, "firedFrom", false);
    Scribe_Values.Look<int>(ref this.explosionFrame, "explosionFrame", 0, false);
  }
}
