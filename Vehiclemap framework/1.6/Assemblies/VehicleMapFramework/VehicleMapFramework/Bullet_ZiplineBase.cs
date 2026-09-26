// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Bullet_ZiplineBase
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using UnityEngine;
using VehicleMapFramework.VMF_HarmonyPatches;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public abstract class Bullet_ZiplineBase : Bullet, IZiplineEnd
{
  public Verb_LaunchZipline launchVerb;

  public CustomZipline.ZipLineData ZipLineData { get; set; }

  public virtual int UpdateRateTicks
  {
    get
    {
      int updateRateTicks = ((Projectile) this).UpdateRateTicks;
      return updateRateTicks == 1 || VehicleMapUtility.get_BaseMapOrCaravan(Find.CurrentMap) != VehicleMapUtility.get_BaseMapOrCaravan((Thing) this) ? updateRateTicks : 1;
    }
  }

  protected Quaternion ExactRotationOrigin
  {
    get
    {
      Quaternion exactRotationOrigin = ((Projectile) this).ExactRotation;
      VehiclePawnWithMap vehicle;
      if (((Thing) this).IsOnNonFocusedVehicleMapOf(out vehicle))
        exactRotationOrigin = Quaternion.op_Multiply(exactRotationOrigin, Quaternion.AngleAxis(-VehicleMapUtility.get_FullAngle((VehiclePawn) vehicle), Vector3.up));
      return exactRotationOrigin;
    }
  }

  protected abstract Vector3 ExactDestination { get; }

  protected float ArcHeightFactor
  {
    get
    {
      float arcHeightFactor = ((Thing) this).def.projectile.arcHeightFactor;
      float num = GenGeo.MagnitudeHorizontalSquared(Vector3.op_Subtraction(((Projectile) this).destination, ((Projectile) this).origin));
      if ((double) arcHeightFactor * (double) arcHeightFactor > (double) num * 0.20000000298023224 * 0.20000000298023224)
        arcHeightFactor = Mathf.Sqrt(num) * 0.2f;
      return arcHeightFactor;
    }
  }

  public virtual void Launch(
    Thing _launcher,
    Vector3 _origin,
    LocalTargetInfo _usedTarget,
    LocalTargetInfo _intendedTarget,
    ProjectileHitFlags hitFlags,
    bool _preventFriendlyFire = false,
    Thing _equipment = null,
    ThingDef _targetCoverDef = null)
  {
    VehiclePawnWithMap vehicle;
    if (((Thing) this).IsOnNonFocusedVehicleMapOf(out vehicle))
      _origin = _origin.ToVehicleMapCoord(vehicle);
    ((Projectile) this).Launch(_launcher, _origin, _usedTarget, _intendedTarget, hitFlags, _preventFriendlyFire, _equipment, _targetCoverDef);
    ((Projectile) this).destination = this.ExactDestination;
    if (this is Bullet_ZiplineEnd)
      ((Projectile) this).origin = Vector3.op_Addition(((Projectile) this).origin, Quaternion.op_Multiply(this.ExactRotationOrigin, Vector3.op_Multiply(Vector3.forward, this.ZipLineData.LauncherOffset + ((Thing) this).DrawSize.y / 2f)));
    ((Projectile) this).ticksToImpact = Mathf.CeilToInt(((Projectile) this).StartingTicksToImpact);
    if (((Projectile) this).ticksToImpact < 1)
      ((Projectile) this).ticksToImpact = 1;
    ((Projectile) this).lifetime = ((Projectile) this).ticksToImpact;
  }

  public abstract void DrawZipline(Vector3 drawLoc);

  public virtual void SpawnSetup(Map map, bool respawningAfterLoad)
  {
    ((ThingWithComps) this).SpawnSetup(map, respawningAfterLoad);
    Verb_LaunchZipline launchVerb = this.launchVerb;
    if (launchVerb == null)
      return;
    launchVerb.ziplineEnd = (Thing) this;
  }

  protected virtual void TickInterval(int delta)
  {
    ((Projectile) this).destination = this.ExactDestination;
    if (!VehicleMapUtility.get_IsOnNonFocusedVehicleMap((Thing) this) || ((Projectile) this).landed)
    {
      ((Projectile) this).TickInterval(delta);
    }
    else
    {
      Rect rect;
      // ISSUE: explicit constructor call
      ((Rect) ref rect).\u002Ector(Vector2.zero, Patch_Map_MapUpdate.MeshSize);
      if (!((Rect) ref rect).Contains(Vector2Utility.ToVector2(((Thing) this).DrawPos)))
      {
        ((Thing) this).Destroy((DestroyMode) 0);
      }
      else
      {
        ((Projectile) this).lifetime = ((Projectile) this).lifetime - delta;
        ((Projectile) this).ticksToImpact = ((Projectile) this).ticksToImpact - delta;
        IntVec3 intVec3 = IntVec3Utility.ToIntVec3(((Projectile) this).ExactPosition);
        if (GenGrid.InBounds(intVec3, ((Thing) this).Map))
          ((Thing) this).Position = intVec3;
        if (((Projectile) this).ticksToImpact > 0)
          return;
        ((Projectile) this).ImpactSomething();
      }
    }
  }

  protected virtual void ImpactSomething()
  {
    ((Projectile) this).Impact(((LocalTargetInfo) ref ((Projectile) this).intendedTarget).Thing, false);
  }

  public virtual void Destroy(DestroyMode mode = 0)
  {
    ((ThingWithComps) this).Destroy(mode);
    Verb_LaunchZipline launchVerb = this.launchVerb;
    if (launchVerb == null)
      return;
    launchVerb.ziplineEnd = (Thing) null;
  }

  public virtual void ExposeData()
  {
    ((Projectile) this).ExposeData();
    Scribe_References.Look<Verb_LaunchZipline>(ref this.launchVerb, "LaunchVerb", false);
    if (Scribe.mode != 4)
      return;
    CustomZipline modExtension = ((Def) ((Thing) this).def).GetModExtension<CustomZipline>();
    if (modExtension == null)
      return;
    this.ZipLineData = modExtension.zipLineData;
  }
}
