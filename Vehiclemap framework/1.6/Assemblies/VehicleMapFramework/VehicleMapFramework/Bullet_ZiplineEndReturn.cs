// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Bullet_ZiplineEndReturn
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class Bullet_ZiplineEndReturn : Bullet_ZiplineBase
{
  public virtual Quaternion ExactRotation
  {
    get
    {
      return Quaternion.op_Multiply(((Projectile) this).ExactRotation, Quaternion.AngleAxis(180f, Vector3.up));
    }
  }

  protected override Vector3 ExactDestination
  {
    get
    {
      Verb_LaunchZipline launchVerb = this.launchVerb;
      if (launchVerb != null)
      {
        Thing caster = ((Verb) launchVerb).caster;
        if (caster != null && caster.Spawned)
        {
          Vector3 drawPos = ((Verb) this.launchVerb).caster.DrawPos;
          BuildingProperties building = ((Projectile) this).launcher.def.building;
          Vector3 vector3 = building != null ? Vector2Utility.ToVector3(building.turretTopOffset) : Vector3.zero;
          VehiclePawnWithMap vehicle;
          if (((Projectile) this).launcher.IsOnNonFocusedVehicleMapOf(out vehicle) && !VehicleMapUtility.get_IsOnNonFocusedVehicleMap((Thing) this))
            vector3 = Vector3Utility.RotatedBy(vector3, -vehicle.Angle + vehicle.Transform.rotation);
          ((Projectile) this).destination = Vector3.op_Addition(Vector3.op_Addition(drawPos, vector3), Quaternion.op_Multiply(((Projectile) this).ExactRotation, Vector3.op_Multiply(Vector3.forward, this.ZipLineData.LauncherOffset + ((Thing) this).DrawSize.y / 2f)));
        }
      }
      VehiclePawnWithMap vehicle1;
      return !((Thing) this).IsOnNonFocusedVehicleMapOf(out vehicle1) ? ((Projectile) this).destination : ((Projectile) this).destination.ToVehicleMapCoord(vehicle1);
    }
  }

  protected virtual void Impact(Thing hitThing, bool blockedByShield = false)
  {
    if (blockedByShield)
      return;
    ((Thing) this).Destroy((DestroyMode) 0);
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
    Scribe_References.Look<Verb_LaunchZipline>(ref this.launchVerb, "launchVerb", false);
    if (Scribe.mode != 4)
      return;
    CustomZipline modExtension = ((Def) ((Verb) this.launchVerb)?.verbProps?.defaultProjectile)?.GetModExtension<CustomZipline>();
    if (modExtension == null)
      return;
    this.ZipLineData = modExtension.zipLineData;
  }
}
