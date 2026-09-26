// Decompiled with JetBrains decompiler
// Type: Vehicles.TurretTop_Artillery
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using SmashTools;
using UnityEngine;
using Vehicles.World;
using Verse;

#nullable disable
namespace Vehicles;

public class TurretTop_Artillery : TurretTop
{
  protected const float IdleTurnDegreesPerTick = 0.26f;
  protected const int IdleTurnDuration = 140;
  protected const int IdleTurnIntervalMin = 150;
  protected const int IdleTurnIntervalMax = 350;
  protected Building_Artillery parentTurret;
  protected float curRotationInt;
  protected int ticksUntilIdleTurn;
  protected int idleTurnTicksLeft;
  protected bool idleTurnClockwise;
  protected Vector3 altitudeDrawLayer;
  protected CompDrawLayerTurret drawLayer;
  protected bool drawLayersDisabled;
  protected Graphic turretGraphic;
  protected GraphicData turretGraphicData;

  public TurretTop_Artillery(Building_Artillery parentTurret)
    : base((Building_Turret) parentTurret)
  {
    this.parentTurret = parentTurret;
  }

  public virtual float CurRotation
  {
    get => this.curRotationInt;
    set => this.curRotationInt = value.ClampAngle();
  }

  public CompDrawLayerTurret DrawLayer
  {
    get
    {
      if (this.drawLayer == null && !this.drawLayersDisabled)
      {
        this.drawLayer = ((ThingWithComps) this.parentTurret).GetComp<CompDrawLayerTurret>();
        if (this.drawLayer == null)
          this.drawLayersDisabled = true;
      }
      return this.drawLayer;
    }
  }

  public GraphicData TurretGraphicData
  {
    get
    {
      if (this.turretGraphicData == null)
        this.turretGraphicData = ((Thing) this.parentTurret).def.building.turretGunDef?.graphicData;
      return this.turretGraphicData;
    }
  }

  public Graphic TurretGraphic
  {
    get
    {
      if (this.turretGraphic == null)
        this.turretGraphic = this.TurretGraphicData?.Graphic.GetColoredVersion(this.TurretGraphicData.shaderType.Shader, ((Thing) this.parentTurret).DrawColor, ((Thing) this.parentTurret).DrawColorTwo);
      return this.turretGraphic;
    }
  }

  public virtual void SetRotationFromOrientation()
  {
    Rot4 rotation = ((Thing) this.parentTurret).Rotation;
    this.CurRotation = ((Rot4) ref rotation).AsAngle;
  }

  public virtual void DrawTurret()
  {
    Vector3 vector3 = Vector3Utility.RotatedBy(new Vector3(((Thing) this.parentTurret).def.building.turretTopOffset.x, 0.0f, ((Thing) this.parentTurret).def.building.turretTopOffset.y), this.CurRotation);
    float turretTopDrawSize = ((Thing) this.parentTurret).def.building.turretTopDrawSize;
    Matrix4x4 matrix4x4 = new Matrix4x4();
    ((Matrix4x4) ref matrix4x4).SetTRS(Vector3.op_Addition(Vector3.op_Addition(((Thing) this.parentTurret).DrawPos, this.altitudeDrawLayer), Altitudes.AltIncVect), GenMath.ToQuat(this.CurRotation + (float) TurretTop.ArtworkRotation), new Vector3(turretTopDrawSize, 1f, turretTopDrawSize));
    Graphics.DrawMesh(MeshPool.plane10, matrix4x4, this.TurretGraphic?.MatAt(((Thing) this.parentTurret).Rotation, (Thing) null) ?? ((Thing) this.parentTurret).def.building.turretTopMat, 0);
    if (this.DrawLayer == null)
      return;
    this.DrawLayer.DrawExtra(Vector3.op_Addition(Vector3.op_Addition(Vector3.op_Addition(((Thing) this.parentTurret).DrawPos, this.altitudeDrawLayer), Altitudes.AltIncVect), vector3), this.CurRotation);
  }

  public virtual void Tick()
  {
    LocalTargetInfo currentTarget = ((Building_Turret) this.parentTurret).CurrentTarget;
    GlobalTargetInfo currentWorldTarget = this.parentTurret.CurrentWorldTarget;
    if (((LocalTargetInfo) ref currentTarget).IsValid)
    {
      IntVec3 cell = ((LocalTargetInfo) ref currentTarget).Cell;
      this.CurRotation = Vector3Utility.AngleFlat(Vector3.op_Subtraction(((IntVec3) ref cell).ToVector3Shifted(), ((Thing) this.parentTurret).DrawPos));
      this.ticksUntilIdleTurn = Rand.RangeInclusive(150, 350);
    }
    else if (((GlobalTargetInfo) ref currentWorldTarget).IsValid)
    {
      this.CurRotation = WorldHelper.TryFindHeading(WorldHelper.GetTilePos(((Thing) this.parentTurret).Map.Tile), ((GlobalTargetInfo) ref currentWorldTarget).WorldObject == null ? Find.WorldGrid.GetTileCenter(((GlobalTargetInfo) ref currentWorldTarget).Tile) : ((GlobalTargetInfo) ref currentWorldTarget).WorldObject.DrawPos);
      this.ticksUntilIdleTurn = Rand.RangeInclusive(150, 350);
    }
    else if (this.ticksUntilIdleTurn > 0)
    {
      --this.ticksUntilIdleTurn;
      if (this.ticksUntilIdleTurn != 0)
        return;
      this.idleTurnClockwise = (double) Rand.Value < 0.5;
      this.idleTurnTicksLeft = 140;
    }
    else
    {
      if (this.idleTurnClockwise)
        this.CurRotation += 0.26f;
      else
        this.CurRotation -= 0.26f;
      --this.idleTurnTicksLeft;
      if (this.idleTurnTicksLeft > 0)
        return;
      this.ticksUntilIdleTurn = Rand.RangeInclusive(150, 350);
    }
  }

  public virtual void PostSpawnSetup()
  {
    this.altitudeDrawLayer = new Vector3(0.0f, ((BuildableDef) ((Thing) this.parentTurret).def.building.turretGunDef).Altitude, 0.0f);
  }
}
