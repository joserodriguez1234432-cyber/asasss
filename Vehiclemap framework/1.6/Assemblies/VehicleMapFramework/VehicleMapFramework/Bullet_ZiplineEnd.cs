// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Bullet_ZiplineEnd
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.Sound;

#nullable disable
namespace VehicleMapFramework;

public class Bullet_ZiplineEnd : Bullet_ZiplineBase
{
  public Map destMap;

  protected override Vector3 ExactDestination
  {
    get
    {
      IntVec3 cell = ((LocalTargetInfo) ref ((Projectile) this).intendedTarget).Cell;
      Vector3 original = ((IntVec3) ref cell).ToVector3Shifted();
      if (this.destMap != null)
        original = original.ToBaseMapCoord(this.destMap);
      VehiclePawnWithMap vehicle;
      if (((Thing) this).IsOnNonFocusedVehicleMapOf(out vehicle))
        original = original.ToVehicleMapCoord(vehicle);
      return original;
    }
  }

  public override void Destroy(DestroyMode mode = 0)
  {
    base.Destroy(mode);
    if (this.destMap != null)
    {
      VehiclePawnWithMap vehicle;
      if (this.destMap.IsVehicleMapOf(out vehicle))
        SoundHelper.PlayImpactSound((VehiclePawn) vehicle, new VehicleComponent.DamageResult()
        {
          penetration = (VehicleComponent.Penetration) 3,
          cell = ((LocalTargetInfo) ref ((Projectile) this).intendedTarget).Cell.ToHitCell(vehicle)
        });
      else
        SoundStarter.PlayOneShot(SoundDefOf.BulletImpact_Ground, SoundInfo.op_Implicit(((LocalTargetInfo) ref ((Projectile) this).intendedTarget).ToTargetInfo(this.destMap)));
    }
    else
      SoundStarter.PlayOneShot(SoundDefOf.BulletImpact_Ground, SoundInfo.op_Implicit(((LocalTargetInfo) ref ((Projectile) this).intendedTarget).ToTargetInfo(((Thing) this).Map)));
  }

  protected virtual void Impact(Thing hitThing, bool blockedByShield = false)
  {
    if (blockedByShield || hitThing != ((LocalTargetInfo) ref ((Projectile) this).intendedTarget).Thing)
    {
      ZiplineEnd.ReturnZipline(this.launchVerb);
    }
    else
    {
      ((Thing) this).Destroy((DestroyMode) 0);
      if (this.destMap == null)
        return;
      ZiplineEnd ziplineEnd = (ZiplineEnd) ThingMaker.MakeThing(this.ZipLineData.ZiplineEndDef, (ThingDef) null);
      ziplineEnd.launchVerb = this.launchVerb;
      Quaternion exactRotation = ((Projectile) this).ExactRotation;
      ziplineEnd.rotation = ((Quaternion) ref exactRotation).eulerAngles.y;
      ziplineEnd.ZipLineData = this.ZipLineData;
      GenSpawn.Spawn((Thing) ziplineEnd, ((LocalTargetInfo) ref ((Projectile) this).intendedTarget).Cell, this.destMap, (WipeMode) 0);
    }
  }

  protected virtual void DrawAt(Vector3 drawLoc, bool flip = false)
  {
    ((Projectile) this).DrawAt(drawLoc, flip);
    this.DrawZipline(drawLoc);
  }

  public override void DrawZipline(Vector3 drawLoc)
  {
    float num = this.ArcHeightFactor * GenMath.InverseParabola(((Projectile) this).DistanceCoveredFractionArc);
    Vector3 drawLoc1 = Vector3.op_Addition(drawLoc, Vector3.op_Multiply(Vector3.forward, num));
    Quaternion exactRotation = ((Projectile) this).ExactRotation;
    double y = (double) ((Quaternion) ref exactRotation).eulerAngles.y;
    Verb_LaunchZipline launchVerb = this.launchVerb;
    CustomZipline.ZipLineData zipLineData = this.ZipLineData;
    ZiplineEnd.DrawZipline(drawLoc1, (float) y, launchVerb, zipLineData);
  }

  public override void ExposeData()
  {
    base.ExposeData();
    Scribe_References.Look<Map>(ref this.destMap, "destMap", false);
  }
}
