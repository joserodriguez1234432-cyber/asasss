// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleSkyfaller_Strafe
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

public class VehicleSkyfaller_Strafe : VehicleSkyfaller_FlyOver
{
  protected bool shotsFired;
  protected List<CompVehicleTurrets.TurretData> turrets = new List<CompVehicleTurrets.TurretData>();
  protected Dictionary<VehicleTurret, int> shotsFromTurret = new Dictionary<VehicleTurret, int>();
  private List<VehicleTurret> turretsTmp;
  private List<int> shotsTmp;

  [UsedImplicitly]
  [Obsolete("Implemented for Xml Deserialization only. Use VehicleSkyfallerMaker instead.", true)]
  public VehicleSkyfaller_Strafe()
  {
  }

  protected float StrafeAreaDistance
  {
    get
    {
      return Vector3.Distance(((IntVec3) ref this.start).ToVector3Shifted(), ((IntVec3) ref this.end).ToVector3Shifted());
    }
  }

  protected virtual Vector3 Target(VehicleTurret turret)
  {
    float distance = this.StrafeAreaDistance * (float) this.shotsFromTurret[turret] / (float) turret.MaxShotsCurrentFireMode;
    Vector3 vector3 = ((IntVec3) ref this.start).ToVector3Shifted().PointFromAngle(distance, this.angle);
    Vector2 zero = Vector2.zero;
    return new Vector3(vector3.x + zero.x, vector3.y + (float) turret.drawLayer, vector3.z + zero.y);
  }

  protected virtual Vector3 TurretLocation(VehicleTurret turret)
  {
    float num = 0.0f;
    if (turret.attachedTo != null)
      num = turret.attachedTo.TurretRotation;
    Vector3 distanceAtMin = this.DistanceAtMin;
    Vector2 zero = Vector2.zero;
    return new Vector3(distanceAtMin.x + zero.x, distanceAtMin.y + (float) turret.drawLayer, distanceAtMin.z + zero.y);
  }

  protected virtual void TurretTick()
  {
    if (GenList.NullOrEmpty<CompVehicleTurrets.TurretData>((IList<CompVehicleTurrets.TurretData>) this.turrets))
      return;
    for (int index = 0; index < this.turrets.Count; ++index)
    {
      CompVehicleTurrets.TurretData turret1 = this.turrets[index];
      VehicleTurret turret = turret1.turret;
      if (!turret.HasAmmo && !VehicleMod.settings.debug.debugShootAnyTurret)
      {
        this.turrets.Remove(turret1);
        this.shotsFired = GenList.NullOrEmpty<CompVehicleTurrets.TurretData>((IList<CompVehicleTurrets.TurretData>) this.turrets);
      }
      else if (turret.OnCooldown)
      {
        turret.SetTarget(LocalTargetInfo.Invalid);
        this.turrets.Remove(turret1);
        this.shotsFired = GenList.NullOrEmpty<CompVehicleTurrets.TurretData>((IList<CompVehicleTurrets.TurretData>) this.turrets);
      }
      else
      {
        this.turrets[index].turret.AlignToTargetRestricted();
        if (this.turrets[index].ticksTillShot <= 0)
        {
          this.FireTurret(turret);
          ++this.shotsFromTurret[turret];
          ++turret.CurrentTurretFiring;
          --turret1.shots;
          turret1.ticksTillShot = turret.TicksPerShot;
          if (turret.OnCooldown || turret1.shots == 0 || turret.def.ammunition != null && turret.shellCount <= 0)
          {
            turret.SetTarget(LocalTargetInfo.Invalid);
            this.turrets.RemoveAll((Predicate<CompVehicleTurrets.TurretData>) (t => t.turret == turret));
            this.shotsFired = GenList.NullOrEmpty<CompVehicleTurrets.TurretData>((IList<CompVehicleTurrets.TurretData>) this.turrets);
            continue;
          }
        }
        else
          --turret1.ticksTillShot;
        this.turrets[index] = turret1;
      }
    }
  }

  protected virtual void FireTurret(VehicleTurret turret)
  {
    float num1 = turret.def.projectileShifting.NotNullAndAny<float>() ? turret.def.projectileShifting[turret.CurrentTurretFiring] : 0.0f;
    Vector3 origin = Vector3.op_Addition(this.TurretLocation(turret), new Vector3(num1, 1f, turret.def.projectileOffset));
    Vector3 vector3 = this.Target(turret);
    float num2 = Vector3.Distance(this.TurretLocation(turret), vector3);
    IntVec3 intVec3 = IntVec3.op_Addition(IntVec3Utility.ToIntVec3(vector3), GenRadial.RadialPattern[Rand.Range(0, GenRadial.NumCellsInRadius(turret.CurrentFireMode.forcedMissRadius * (num2 / turret.def.maxRange)))]);
    if (turret.CurrentTurretFiring >= turret.def.projectileShifting.Count)
      turret.CurrentTurretFiring = 0;
    ThingDef projectileDef = turret.def.ammunition == null || turret.def.genericAmmo ? turret.def.projectile : turret.loadedAmmo?.projectileWhenLoaded;
    try
    {
      float speedTilesPerTick = projectileDef.projectile.SpeedTilesPerTick;
      if ((double) turret.def.projectileSpeed > 0.0)
        speedTilesPerTick = turret.def.projectileSpeed;
      GenSpawn.Spawn((Verse.Thing) ProjectileSkyfallerMaker.WrapProjectile(SkyfallerDefOf.ProjectileSkyfaller, projectileDef, (Verse.Thing) this, origin, ((IntVec3) ref intVec3).ToVector3Shifted(), speedTilesPerTick), ((IntVec3) ref intVec3).ClampInsideMap(this.Map), this.Map, (WipeMode) 0);
      if (turret.def.ammunition != null)
        turret.ConsumeChamberedShot();
      if (turret.def.shotSound != null)
        SoundStarter.PlayOneShot(turret.def.shotSound, SoundInfo.op_Implicit(new TargetInfo(this.Position, this.Map, false)));
      turret.PostTurretFire();
    }
    catch (Exception ex)
    {
      Log.Error($"Exception when firing Cannon: {turret.def.LabelCap} on Pawn: {((Entity) this.vehicle).LabelCap}. Exception: {ex}");
    }
  }

  protected override void Tick()
  {
    this.TurretTick();
    if (!this.shotsFired)
      return;
    base.Tick();
  }

  public override void SpawnSetup(Map map, bool respawningAfterLoad)
  {
    base.SpawnSetup(map, respawningAfterLoad);
    if (this.vehicle.CompVehicleLauncher == null || respawningAfterLoad)
      return;
    foreach (VehicleTurret strafeTurret in this.vehicle.CompVehicleLauncher.StrafeTurrets)
    {
      CompVehicleTurrets.TurretData turretData = strafeTurret.GenerateTurretData();
      turretData.shots = turretData.turret.MaxShotsCurrentFireMode;
      this.turrets.Add(turretData);
      this.shotsFromTurret.Add(strafeTurret, 0);
    }
  }

  public override void ExposeData()
  {
    base.ExposeData();
    Scribe_Values.Look<bool>(ref this.shotsFired, "shotsFired", false, false);
    Scribe_Collections.Look<CompVehicleTurrets.TurretData>(ref this.turrets, "turrets", (LookMode) 0, Array.Empty<object>());
    Scribe_Collections.Look<VehicleTurret, int>(ref this.shotsFromTurret, "shotsFromTurret", (LookMode) 3, (LookMode) 1, ref this.turretsTmp, ref this.shotsTmp, true, false, false);
  }
}
