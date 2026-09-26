// Decompiled with JetBrains decompiler
// Type: Vehicles.Projectile_Deflected
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using SmashTools;
using System;
using System.Reflection;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[StaticConstructorOnStartup]
public class Projectile_Deflected : Projectile
{
  private Projectile projectile;
  private static MethodInfo impactMethod = AccessTools.Method(typeof (Projectile), "Impact", (System.Type[]) null, (System.Type[]) null);
  private static Material shadowMaterialRef = (Material) AccessTools.Field(typeof (Projectile), "shadowMaterial").GetValue((object) null);

  public virtual float ArmorPenetration => this.projectile.ArmorPenetration;

  public virtual int DamageAmount => this.projectile.DamageAmount;

  public virtual Material DrawMat => this.projectile.DrawMat;

  public float DeflectedArcHeight => 0.0f;

  protected virtual void DrawAt(Vector3 drawLoc, bool flip = false)
  {
    float height = this.DeflectedArcHeight * GenMath.InverseParabola(this.DistanceCoveredFraction);
    drawLoc = Vector3.op_Addition(drawLoc, Vector3.op_Multiply(Vector3.forward, height));
    if ((double) ((Thing) this.projectile).def.projectile.shadowSize > 0.0)
      this.DrawShadow(drawLoc, height);
    Graphics.DrawMesh(MeshPool.GridPlane(((Thing) this.projectile).def.graphicData.drawSize), drawLoc, this.ExactRotation, base.DrawMat, 0);
    ((ThingWithComps) this).Comps_PostDraw();
  }

  private void DrawShadow(Vector3 drawLoc, float height)
  {
    if (Object.op_Equality((Object) Projectile_Deflected.shadowMaterialRef, (Object) null))
      return;
    float num = ((Thing) this.projectile).def.projectile.shadowSize * Mathf.Lerp(1f, 0.6f, height);
    Vector3 vector3_1;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3_1).\u002Ector(num, 1f, num);
    Vector3 vector3_2;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3_2).\u002Ector(0.0f, -0.01f, 0.0f);
    Matrix4x4 matrix4x4 = new Matrix4x4();
    ((Matrix4x4) ref matrix4x4).SetTRS(Vector3.op_Addition(drawLoc, vector3_2), Quaternion.identity, vector3_1);
    Graphics.DrawMesh(MeshPool.plane10, matrix4x4, Projectile_Deflected.shadowMaterialRef, 0);
  }

  public void Deflect(
    Projectile projectile,
    Thing deflectedOff,
    float distance,
    float angle,
    float arcHeight = 0.0f)
  {
    this.projectile = projectile;
    Vector3 exactPosition = projectile.ExactPosition;
    Vector3 vector3 = exactPosition.PointFromAngle(distance, angle);
    Map map = ((Thing) projectile).Map;
    ((Entity) projectile).DeSpawn((DestroyMode) 0);
    GenSpawn.Spawn((Thing) this, IntVec3Utility.ToIntVec3(exactPosition), map, (WipeMode) 0);
    this.Launch(deflectedOff, exactPosition, LocalTargetInfo.op_Implicit(IntVec3Utility.ToIntVec3(vector3)), LocalTargetInfo.op_Implicit(IntVec3Utility.ToIntVec3(vector3)), (ProjectileHitFlags) 1, false, (Thing) null, (ThingDef) null);
  }

  protected virtual void Impact(Thing hitThing, bool blockedByShield = false)
  {
    GenSpawn.Spawn((Thing) this.projectile, ((Thing) this).Position, ((Thing) this).Map, (WipeMode) 0);
    Projectile_Deflected.impactMethod.Invoke((object) this.projectile, new object[2]
    {
      (object) hitThing,
      (object) blockedByShield
    });
    ((Thing) this).Destroy((DestroyMode) 0);
  }

  public virtual void ExposeData()
  {
    base.ExposeData();
    Scribe_Deep.Look<Projectile>(ref this.projectile, "projectile", Array.Empty<object>());
  }
}
