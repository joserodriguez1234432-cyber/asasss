// Decompiled with JetBrains decompiler
// Type: Vehicles.ProjectileHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

public static class ProjectileHelper
{
  private static LinearCurve distanceBySpeed = new LinearCurve()
  {
    new CurvePoint(0.0f, 3f),
    new CurvePoint(10f, 6f),
    new CurvePoint(60f, 10f),
    new CurvePoint(100f, 20f),
    new CurvePoint(200f, 30f),
    new CurvePoint(400f, 50f)
  };

  public static bool DeflectProjectile(Projectile projectile, VehiclePawn deflectedOff)
  {
    SoundStarter.PlayOneShot(SoundDefOf.MetalHitImportant, SoundInfo.op_Implicit((Thing) deflectedOff));
    Quaternion exactRotation = projectile.ExactRotation;
    float angle = (((Quaternion) ref exactRotation).eulerAngles.y + (float) Rand.Range(-10, 10)).ClampAngle();
    float speed = ((Thing) projectile).def.projectile.speed;
    float num = ProjectileHelper.distanceBySpeed.Evaluate(speed);
    FloatRange floatRange;
    // ISSUE: explicit constructor call
    ((FloatRange) ref floatRange).\u002Ector(num * 0.9f, num * 1.1f);
    Projectile_Deflected projectileDeflected = new Projectile_Deflected();
    ((Thing) projectileDeflected).def = ((Thing) projectile).def;
    ((Thing) projectileDeflected).SetStuffDirect(((Thing) projectile).Stuff);
    ((Thing) projectileDeflected).PostMake();
    ((Thing) projectileDeflected).PostPostMake();
    projectileDeflected.Deflect(projectile, (Thing) deflectedOff, ((FloatRange) ref floatRange).RandomInRange, angle, Mathf.Sqrt(num));
    return true;
  }
}
