// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.ZiplineEnd
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class ZiplineEnd : ThingWithComps, IZiplineEnd
{
  public Verb_LaunchZipline launchVerb;
  public float rotation;

  public CustomZipline.ZipLineData ZipLineData { get; set; }

  public virtual void SpawnSetup(Map map, bool respawningAfterLoad)
  {
    base.SpawnSetup(map, respawningAfterLoad);
    if (this.launchVerb == null)
      return;
    this.launchVerb.ziplineEnd = (Thing) this;
    if (((Verb) this.launchVerb).caster is Building_TurretGunForcedTargetOnly caster)
    {
      ((Thing) caster).RemoveTargetInfo();
      caster.ForcedTarget = LocalTargetInfo.op_Implicit((Thing) this);
    }
    if (!(this.launchVerb.Ability is Ability_GrapplingHook ability))
      return;
    ability.OnHit(this);
  }

  protected virtual void TickInterval(int delta)
  {
    base.TickInterval(delta);
    Verb_LaunchZipline launchVerb = this.launchVerb;
    if (launchVerb != null)
    {
      Thing caster = ((Verb) launchVerb).caster;
      if (caster != null && caster.SpawnedOrAnyParentSpawned && this.launchVerb.ziplineEnd == this)
        return;
    }
    base.Destroy((DestroyMode) 0);
  }

  public virtual void Destroy(DestroyMode mode = 0)
  {
    ZiplineEnd.ReturnZipline(this.launchVerb);
    base.Destroy(mode);
  }

  public virtual void Notify_MyMapRemoved()
  {
    this.launchVerb.ziplineEnd = (Thing) null;
    base.Notify_MyMapRemoved();
  }

  public virtual void Print(SectionLayer layer)
  {
    ((Thing) this).Graphic.Print(layer, (Thing) this, this.rotation);
    foreach (ThingComp allComp in this.AllComps)
      allComp.PostPrintOnto(layer);
  }

  protected virtual void DrawAt(Vector3 drawLoc, bool flip = false)
  {
    if (((Thing) this).def.drawerType == 1)
    {
      Verb_LaunchZipline launchVerb = this.launchVerb;
      if (launchVerb != null)
      {
        Thing caster = ((Verb) launchVerb).caster;
        if (caster != null && caster.Spawned)
        {
          this.rotation = Vector3Utility.AngleFlat(Vector3.op_Subtraction(drawLoc, ((Verb) this.launchVerb).caster.DrawPos));
          ((Thing) this).Graphic.Draw(drawLoc, Rot4.North, (Thing) this, this.rotation);
        }
      }
    }
    this.Comps_DrawAt(drawLoc, flip);
    this.Comps_PostDraw();
    SilhouetteUtility.DrawGraphicSilhouette((Thing) this, drawLoc);
    this.DrawZipline(drawLoc);
  }

  public void DrawZipline(Vector3 drawLoc)
  {
    float rotation = this.rotation;
    ZiplineEnd.DrawZipline(drawLoc, rotation, this.launchVerb, this.ZipLineData);
  }

  public static void DrawZipline(
    Vector3 drawLoc,
    float rotation,
    Verb_LaunchZipline launchVerb,
    CustomZipline.ZipLineData ziplineData)
  {
    Thing spawnedParentOrMe = ((Verb) launchVerb)?.caster.SpawnedParentOrMe;
    if (spawnedParentOrMe == null)
      return;
    Vector3 vector3_1 = Vector3.op_Addition(drawLoc, Vector3Utility.RotatedBy(Vector3.op_Multiply(Vector3.back, ziplineData.ZiplineEndOffset), rotation));
    Vector3 vector3_2 = spawnedParentOrMe.DrawPos;
    Vector2? turretTopOffset = spawnedParentOrMe.def.building?.turretTopOffset;
    if (turretTopOffset.HasValue)
    {
      Vector2 vector2 = turretTopOffset.GetValueOrDefault();
      VehiclePawnWithMap vehicle;
      if (spawnedParentOrMe.IsOnNonFocusedVehicleMapOf(out vehicle))
        vector2 = Vector2Utility.RotatedBy(vector2, -vehicle.Angle + vehicle.Transform.rotation);
      vector3_2 = Vector3.op_Addition(vector3_2, Vector2Utility.ToVector3(vector2));
    }
    Vector3 vector3_3 = Vector3.op_Addition(vector3_2, Vector3Utility.RotatedBy(Vector3.op_Multiply(Vector3.forward, ziplineData.LauncherOffset), Vector3Utility.AngleFlat(Vector3.op_Subtraction(vector3_1, vector3_2))));
    float num = Mathf.Max(vector3_1.y, vector3_3.y) - 0.03658537f;
    GenDrawOnVehicle.DrawLineBetweenInstanced(Vector3Utility.WithY(vector3_1, num), Vector3Utility.WithY(vector3_3, num), ziplineData.ZiplineMat, ziplineData.ZiplineWidth);
  }

  public static void ReturnZipline(Verb_LaunchZipline launchVerb)
  {
    Thing spawnedParentOrMe = ((Verb) launchVerb)?.caster.SpawnedParentOrMe;
    if (spawnedParentOrMe == null || !(launchVerb.ziplineEnd is IZiplineEnd ziplineEnd1))
      return;
    Thing ziplineEnd2 = launchVerb.ziplineEnd;
    IntVec3 onBaseMapSpawned = VehicleMapUtility.get_PositionOnBaseMapSpawned(ziplineEnd2);
    Bullet_ZiplineEndReturn ziplineEndReturn = (Bullet_ZiplineEndReturn) ThingMaker.MakeThing(ziplineEnd1.ZipLineData.ZiplineReturnDef, (ThingDef) null);
    ziplineEndReturn.launchVerb = launchVerb;
    ziplineEndReturn.ZipLineData = ziplineEnd1.ZipLineData;
    GenSpawn.Spawn((Thing) ziplineEndReturn, onBaseMapSpawned, VehicleMapUtility.get_GroundMap(ziplineEnd2), (WipeMode) 0);
    ((Projectile) ziplineEndReturn).Launch(spawnedParentOrMe, ziplineEnd2.DrawPos, LocalTargetInfo.op_Implicit(spawnedParentOrMe), LocalTargetInfo.op_Implicit(spawnedParentOrMe), (ProjectileHitFlags) 1, false, (Thing) null, (ThingDef) null);
  }

  public virtual void ExposeData()
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
