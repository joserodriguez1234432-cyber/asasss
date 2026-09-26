// Decompiled with JetBrains decompiler
// Type: Vehicles.Verb_ShootRealistic
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class Verb_ShootRealistic : Verb_Shoot
{
  public ThingWithComps CasterTWC => ((Verb) this).caster as ThingWithComps;

  public VerbProperties_Recoil VerbProps => ((Verb) this).verbProps as VerbProperties_Recoil;

  protected void ThrowDebugText(string text)
  {
    if (!DebugViewSettings.drawShooting)
      return;
    MoteMaker.ThrowText(((Verb) this).caster.DrawPos, ((Verb) this).caster.Map, text, -1f);
  }

  protected void ThrowDebugText(string text, IntVec3 c)
  {
    if (!DebugViewSettings.drawShooting)
      return;
    MoteMaker.ThrowText(((IntVec3) ref c).ToVector3Shifted(), ((Verb) this).caster.Map, text, -1f);
  }

  protected virtual bool CausesTimeSlowdown(GlobalTargetInfo target)
  {
    return ((Verb) this).verbProps.CausesTimeSlowdown;
  }

  protected virtual bool CausesTimeSlowdown(LocalTargetInfo target)
  {
    if (!((Verb) this).verbProps.CausesTimeSlowdown || !((LocalTargetInfo) ref target).HasThing)
      return false;
    Thing thing = ((LocalTargetInfo) ref target).Thing;
    if (thing.def.category != 1 && (thing.def.building == null || !thing.def.building.IsTurret))
      return false;
    bool flag = thing is Pawn pawn && pawn.Downed;
    if (thing.Faction == Faction.OfPlayer && GenHostility.HostileTo(((Verb) this).caster, Faction.OfPlayer))
      return true;
    return ((Verb) this).caster.Faction == Faction.OfPlayer && GenHostility.HostileTo(thing, Faction.OfPlayer) && !flag;
  }

  protected virtual bool TryCastShot()
  {
    (bool success, Vector3 vector3, float angle) = this.TryCastShotInternal();
    if (success)
      this.InitTurretMotes(vector3, angle);
    return success;
  }

  protected virtual (bool success, Vector3 launchPos, float angle) TryCastShotInternal()
  {
    if (((LocalTargetInfo) ref ((Verb) this).currentTarget).HasThing && ((LocalTargetInfo) ref ((Verb) this).currentTarget).Thing.Map != ((Verb) this).caster.Map)
      return (false, Vector3.zero, 0.0f);
    ThingDef projectile1 = ((Verb_LaunchProjectile) this).Projectile;
    if (projectile1 == null)
      return (false, Vector3.zero, 0.0f);
    ShootLine shootLine;
    bool shootLineFromTo = ((Verb) this).TryFindShootLineFromTo(((Verb) this).caster.Position, ((Verb) this).currentTarget, ref shootLine, false);
    if (((Verb) this).verbProps.stopBurstWithoutLos && !shootLineFromTo)
      return (false, Vector3.zero, 0.0f);
    if (((Verb) this).EquipmentSource != null)
      ((Verb) this).EquipmentSource.GetComp<CompChangeableProjectile>()?.Notify_ProjectileLaunched();
    Thing thing1 = ((Verb) this).caster;
    Thing thing2 = (Thing) ((Verb) this).EquipmentSource;
    CompMannable comp = ThingCompUtility.TryGetComp<CompMannable>(((Verb) this).caster);
    if (comp != null && comp.ManningPawn != null)
    {
      thing1 = (Thing) comp.ManningPawn;
      thing2 = ((Verb) this).caster;
    }
    Vector3 drawPos = ((Verb) this).caster.DrawPos;
    float point1 = drawPos.AngleToPoint(((LocalTargetInfo) ref ((Verb) this).currentTarget).CenterVector3);
    Projectile projectile2 = (Projectile) GenSpawn.Spawn(projectile1, ((ShootLine) ref shootLine).Source, ((Verb) this).caster.Map, (WipeMode) 0);
    ProjectilePropertiesDefModExtension modExtension = ((Def) ((Verb) this).caster.def).GetModExtension<ProjectilePropertiesDefModExtension>();
    if (modExtension != null)
      ((ThingWithComps) projectile2).AllComps.Insert(0, (ThingComp) new CompTurretProjectileProperties(this.CasterTWC)
      {
        speed = ((double) modExtension.speed > 0.0 ? modExtension.speed : ((Thing) projectile2).def.projectile.speed),
        hitflags = modExtension.hitFlagDef
      });
    if ((double) ((Verb) this).verbProps.ForcedMissRadius > 0.5)
    {
      float adjustedForcedMiss = VerbUtility.CalculateAdjustedForcedMiss(((Verb) this).verbProps.ForcedMissRadius, IntVec3.op_Subtraction(((LocalTargetInfo) ref ((Verb) this).currentTarget).Cell, ((Verb) this).caster.Position));
      if ((double) adjustedForcedMiss > 0.5)
      {
        int index = Rand.Range(0, GenRadial.NumCellsInRadius(adjustedForcedMiss));
        if (index > 0)
        {
          IntVec3 c = IntVec3.op_Addition(((LocalTargetInfo) ref ((Verb) this).currentTarget).Cell, GenRadial.RadialPattern[index]);
          Vector3 vector3 = Vector3.op_Addition(drawPos, Vector3Utility.RotatedBy(new Vector3(this.VerbProps.shootOffset.x, 0.0f, this.VerbProps.shootOffset.y), point1));
          this.ThrowDebugText("ToRadius");
          this.ThrowDebugText("Rad\nDest", c);
          ProjectileHitFlags projectileHitFlags = (ProjectileHitFlags) 4;
          if (Rand.Chance(0.5f))
            projectileHitFlags = (ProjectileHitFlags) -1;
          if (!((Verb) this).canHitNonTargetPawnsNow)
            projectileHitFlags = (ProjectileHitFlags) (projectileHitFlags & -3);
          projectile2.Launch(thing1, vector3, LocalTargetInfo.op_Implicit(c), ((Verb) this).currentTarget, projectileHitFlags, false, thing2, (ThingDef) null);
          return (true, vector3, point1);
        }
      }
    }
    ShotReport shotReport = ShotReport.HitReportFor(((Verb) this).caster, (Verb) this, ((Verb) this).currentTarget);
    Thing randomCoverToMissInto = ((ShotReport) ref shotReport).GetRandomCoverToMissInto();
    ThingDef def = randomCoverToMissInto?.def;
    if (!Rand.Chance(((ShotReport) ref shotReport).AimOnTargetChance_IgnoringPosture))
    {
      ((ShootLine) ref shootLine).ChangeDestToMissWild(((ShotReport) ref shotReport).AimOnTargetChance_StandardTarget, projectile1.projectile.flyOverhead, ((Verb) this).caster.Map);
      this.ThrowDebugText("ToWild" + (((Verb) this).canHitNonTargetPawnsNow ? "\nchntp" : ""));
      this.ThrowDebugText("Wild\nDest", ((ShootLine) ref shootLine).Dest);
      ProjectileHitFlags projectileHitFlags = (ProjectileHitFlags) 4;
      if (Rand.Chance(0.5f) && ((Verb) this).canHitNonTargetPawnsNow)
        projectileHitFlags = (ProjectileHitFlags) (projectileHitFlags | 2);
      Vector3 vector3 = Vector3.op_Addition(drawPos, Vector3Utility.RotatedBy(new Vector3(this.VerbProps.shootOffset.x, 0.0f, this.VerbProps.shootOffset.y), point1));
      projectile2.Launch(thing1, vector3, LocalTargetInfo.op_Implicit(((ShootLine) ref shootLine).Dest), ((Verb) this).currentTarget, projectileHitFlags, false, thing2, def);
      return (true, vector3, point1);
    }
    if (((LocalTargetInfo) ref ((Verb) this).currentTarget).Thing != null && ((LocalTargetInfo) ref ((Verb) this).currentTarget).Thing.def.category == 1 && !Rand.Chance(((ShotReport) ref shotReport).PassCoverChance))
    {
      this.ThrowDebugText("ToCover" + (((Verb) this).canHitNonTargetPawnsNow ? "\nchntp" : ""));
      this.ThrowDebugText("Cover\nDest", randomCoverToMissInto.Position);
      ProjectileHitFlags projectileHitFlags = (ProjectileHitFlags) 4;
      if (((Verb) this).canHitNonTargetPawnsNow)
        projectileHitFlags = (ProjectileHitFlags) (projectileHitFlags | 2);
      Vector3 vector3 = Vector3.op_Addition(drawPos, Vector3Utility.RotatedBy(new Vector3(this.VerbProps.shootOffset.x, 0.0f, this.VerbProps.shootOffset.y), point1));
      projectile2.Launch(thing1, vector3, LocalTargetInfo.op_Implicit(randomCoverToMissInto), ((Verb) this).currentTarget, projectileHitFlags, false, thing2, def);
      return (true, vector3, point1);
    }
    ProjectileHitFlags projectileHitFlags1 = (ProjectileHitFlags) 1;
    if (((Verb) this).canHitNonTargetPawnsNow)
      projectileHitFlags1 = (ProjectileHitFlags) (projectileHitFlags1 | 2);
    if (!((LocalTargetInfo) ref ((Verb) this).currentTarget).HasThing || ((LocalTargetInfo) ref ((Verb) this).currentTarget).Thing.def.Fillage == 2)
      projectileHitFlags1 = (ProjectileHitFlags) (projectileHitFlags1 | 4);
    this.ThrowDebugText("ToHit" + (((Verb) this).canHitNonTargetPawnsNow ? "\nchntp" : ""));
    float point2;
    Vector3 vector3_1;
    if (((LocalTargetInfo) ref ((Verb) this).currentTarget).Thing != null)
    {
      point2 = drawPos.AngleToPoint(((LocalTargetInfo) ref ((Verb) this).currentTarget).CenterVector3);
      vector3_1 = Vector3.op_Addition(drawPos, Vector3Utility.RotatedBy(new Vector3(this.VerbProps.shootOffset.x, 0.0f, this.VerbProps.shootOffset.y), point2));
      projectile2.Launch(thing1, vector3_1, ((Verb) this).currentTarget, ((Verb) this).currentTarget, projectileHitFlags1, false, thing2, def);
      this.ThrowDebugText("Hit\nDest", ((LocalTargetInfo) ref ((Verb) this).currentTarget).Cell);
    }
    else
    {
      Vector3 pos = drawPos;
      IntVec3 dest = ((ShootLine) ref shootLine).Dest;
      Vector3 vector3Shifted = ((IntVec3) ref dest).ToVector3Shifted();
      point2 = pos.AngleToPoint(vector3Shifted);
      vector3_1 = Vector3.op_Addition(drawPos, Vector3Utility.RotatedBy(new Vector3(this.VerbProps.shootOffset.x, 0.0f, this.VerbProps.shootOffset.y), point2));
      projectile2.Launch(thing1, vector3_1, LocalTargetInfo.op_Implicit(((ShootLine) ref shootLine).Dest), ((Verb) this).currentTarget, projectileHitFlags1, false, thing2, def);
      this.ThrowDebugText("Hit\nDest", ((ShootLine) ref shootLine).Dest);
    }
    return (true, vector3_1, point2);
  }

  public virtual void InitTurretMotes(Vector3 loc, float angle)
  {
    if (GenList.NullOrEmpty<AnimationProperties>((IList<AnimationProperties>) this.VerbProps.motes))
      return;
    foreach (AnimationProperties mote1 in this.VerbProps.motes)
    {
      Vector3 vector3_1 = loc;
      if (GenView.ShouldSpawnMotesAt(loc, ((Verb) this).caster.Map, true))
      {
        try
        {
          float num = Altitudes.AltitudeFor(((BuildableDef) mote1.moteDef).altitudeLayer);
          Vector3 vector3_2 = Vector3.op_Addition(vector3_1, Vector3Utility.RotatedBy(new Vector3(this.VerbProps.shootOffset.x + mote1.offset.x, num + mote1.offset.y, this.VerbProps.shootOffset.y + mote1.offset.z), angle));
          Mote mote2 = (Mote) ThingMaker.MakeThing(mote1.moteDef, (ThingDef) null);
          mote2.exactPosition = vector3_2;
          mote2.exactRotation = ((FloatRange) ref mote1.exactRotation).RandomInRange;
          mote2.instanceColor = mote1.color;
          mote2.rotationRate = mote1.rotationRate;
          mote2.Scale = mote1.scale;
          if (mote2 is MoteThrown moteThrown)
          {
            float angle1 = angle + ((FloatRange) ref mote1.angleThrown).RandomInRange;
            moteThrown.SetVelocity(angle1, ((FloatRange) ref mote1.speedThrown).RandomInRange);
            if (moteThrown is MoteThrownExpand moteThrownExpand)
            {
              if (moteThrownExpand is MoteThrownSlowToSpeed thrownSlowToSpeed)
                thrownSlowToSpeed.SetDecelerationRate(((FloatRange) ref mote1.deceleration).RandomInRange, mote1.fixedAcceleration, angle1);
              moteThrownExpand.growthRate = ((FloatRange) ref mote1.growthRate).RandomInRange;
            }
          }
          if (mote2 is MoteCannonPlume moteCannonPlume)
          {
            moteCannonPlume.cyclesLeft = mote1.cycles;
            moteCannonPlume.animationType = mote1.animationType;
            moteCannonPlume.exactRotation = angle;
          }
          ((Thing) mote2).def = mote1.moteDef;
          ((Thing) mote2).PostMake();
          GenSpawn.Spawn((Thing) mote2, IntVec3Utility.ToIntVec3(vector3_2), ((Verb) this).caster.Map, (WipeMode) 0);
        }
        catch (Exception ex)
        {
          SmashLog.Error($"Failed to spawn mote at {loc}. MoteDef = <field>{((Def) mote1.moteDef)?.defName ?? "Null"}</field> Exception = {ex}");
        }
      }
    }
  }
}
