// Decompiled with JetBrains decompiler
// Type: Vehicles.TurretTop_Recoiled
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class TurretTop_Recoiled(Building_RecoiledTurret parentTurret) : TurretTop_Artillery((Building_Artillery) parentTurret)
{
  public float curRecoil;
  protected float targetRecoil;
  protected float recoilStep;
  protected bool recoilingBack;

  protected virtual Building_RecoiledTurret ParentTurret
  {
    get => this.parentTurret as Building_RecoiledTurret;
  }

  public VerbProperties_Recoil VerbProps
  {
    get
    {
      if (((Building_Turret) this.parentTurret).AttackVerb.verbProps is VerbProperties_Recoil verbProps)
        return verbProps;
      SmashLog.Error("Unable to retrieve <type>VerbProperties_Recoil</type> for recoiled turret. <property>AttackVerb</property> must have a VerbProperty of type <type>VerbProperties_Recoil</type>");
      return (VerbProperties_Recoil) null;
    }
  }

  public override void DrawTurret()
  {
    Vector3 pos = Vector3Utility.RotatedBy(new Vector3(((Thing) this.parentTurret).def.building.turretTopOffset.x, 0.0f, ((Thing) this.parentTurret).def.building.turretTopOffset.y), this.CurRotation);
    Vector3 vector3 = pos.PointFromAngle(-this.curRecoil, this.CurRotation);
    float turretTopDrawSize = ((Thing) this.parentTurret).def.building.turretTopDrawSize;
    Matrix4x4 matrix4x4 = new Matrix4x4();
    ((Matrix4x4) ref matrix4x4).SetTRS(Vector3.op_Addition(Vector3.op_Addition(Vector3.op_Addition(((Thing) this.parentTurret).DrawPos, this.altitudeDrawLayer), Altitudes.AltIncVect), vector3), GenMath.ToQuat(this.CurRotation + (float) TurretTop.ArtworkRotation), new Vector3(turretTopDrawSize, 1f, turretTopDrawSize));
    Graphics.DrawMesh(MeshPool.plane10, matrix4x4, this.TurretGraphic?.MatAt(((Thing) this.parentTurret).Rotation, (Thing) null) ?? ((Thing) this.parentTurret).def.building.turretTopMat, 0);
    if (this.DrawLayer == null)
      return;
    this.DrawLayer.DrawExtra(Vector3.op_Addition(Vector3.op_Addition(Vector3.op_Addition(((Thing) this.parentTurret).DrawPos, this.altitudeDrawLayer), Altitudes.AltIncVect), pos), this.CurRotation);
  }

  public virtual void RecoilTick()
  {
    if ((double) this.targetRecoil <= 0.0)
      return;
    if (this.recoilingBack)
    {
      this.curRecoil += this.recoilStep;
      if ((double) this.curRecoil >= (double) this.targetRecoil)
        this.curRecoil = this.targetRecoil;
    }
    else
    {
      this.curRecoil -= this.recoilStep * this.VerbProps.recoil.speedMultiplierPostRecoil;
      if ((double) this.curRecoil <= 0.0)
        this.ResetRecoilVars();
    }
    if ((double) this.curRecoil < (double) this.targetRecoil)
      return;
    this.recoilingBack = false;
  }

  public void Notify_TurretRecoil()
  {
    this.targetRecoil = this.VerbProps.recoil.distanceTotal;
    this.recoilStep = this.VerbProps.recoil.distancePerTick;
    this.curRecoil = 0.0f;
    this.recoilingBack = true;
  }

  private void ResetRecoilVars()
  {
    this.curRecoil = 0.0f;
    this.targetRecoil = 0.0f;
    this.recoilStep = 0.0f;
  }
}
