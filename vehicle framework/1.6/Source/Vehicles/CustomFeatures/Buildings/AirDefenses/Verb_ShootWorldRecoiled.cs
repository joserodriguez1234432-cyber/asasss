// Decompiled with JetBrains decompiler
// Type: Vehicles.Verb_ShootWorldRecoiled
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles.World;
using Verse;

#nullable disable
namespace Vehicles;

public class Verb_ShootWorldRecoiled : Verb_ShootRecoiled
{
  protected GlobalTargetInfo target = GlobalTargetInfo.Invalid;
  protected float heading;

  public bool IsTargeting => ((GlobalTargetInfo) ref this.target).IsValid;

  public void ResetWorldTarget() => this.target = GlobalTargetInfo.Invalid;

  public virtual bool CanHitTarget(GlobalTargetInfo target)
  {
    return ((Verb) this).caster != null && ((Verb) this).caster.Spawned && !((Verb) this).ApparelPreventsShooting();
  }

  public virtual bool TryStartCastOn(
    GlobalTargetInfo castTarg,
    float heading,
    bool surpriseAttack = false,
    bool canHitNonTargetPawns = true)
  {
    if (((Verb) this).caster == null)
    {
      Log.Error($"Verb {((Verb) this).GetUniqueLoadID()} needs caster to work (possibly lost during saving/loading).");
      return false;
    }
    if (!((Verb) this).caster.Spawned || ((Verb) this).state == 1 || !this.CanHitTarget(castTarg))
      return false;
    this.heading = heading;
    IntVec3 intVec3 = ((Verb) this).caster.Position.PointFromAngle(500f, this.heading);
    if (this.CausesTimeSlowdown(castTarg))
      Find.TickManager.slower.SignalForceNormalSpeed();
    ((Verb) this).surpriseAttack = surpriseAttack;
    ((Verb) this).canHitNonTargetPawnsNow = canHitNonTargetPawns;
    this.target = castTarg;
    if (((Verb) this).CasterIsPawn && (double) ((Verb) this).verbProps.warmupTime > 0.0)
    {
      ((Verb) this).CasterPawn.Drawer.Notify_WarmingCastAlongLine(new ShootLine(((Verb) this).caster.Position, intVec3), ((Verb) this).caster.Position);
      ((Verb) this).CasterPawn.stances.SetStance((Stance) new Stance_Warmup(GenTicks.SecondsToTicks(((Verb) this).verbProps.warmupTime * StatExtension.GetStatValue((Thing) ((Verb) this).CasterPawn, StatDefOf.AimingDelayFactor, true, -1)), LocalTargetInfo.op_Implicit(intVec3), (Verb) this));
    }
    else
      ((Verb) this).WarmupComplete();
    return true;
  }

  protected override (bool success, Vector3 launchPos, float angle) TryCastShotInternal()
  {
    IntVec3 intVec3 = ((Verb) this).caster.Position.PointFromAngle(500f, this.heading);
    if (!((GlobalTargetInfo) ref this.target).HasWorldObject && !((GlobalTargetInfo) ref this.target).HasThing)
      return (false, Vector3.zero, 0.0f);
    ThingDef projectile1 = ((Verb_LaunchProjectile) this).Projectile;
    if (projectile1 == null)
      return (false, Vector3.zero, 0.0f);
    ShootLine shootLine;
    bool shootLineFromTo = ((Verb) this).TryFindShootLineFromTo(((Verb) this).caster.Position, LocalTargetInfo.op_Implicit(intVec3), ref shootLine, false);
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
    Projectile projectile2 = (Projectile) GenSpawn.Spawn(projectile1, ((Verb) this).caster.Position, ((Verb) this).caster.Map, (WipeMode) 0);
    if (GenList.NullOrEmpty<ThingComp>((IList<ThingComp>) ((ThingWithComps) projectile2).AllComps))
      AccessTools.Field(typeof (ThingWithComps), "comps").SetValue((object) projectile2, (object) new List<ThingComp>());
    ((ThingWithComps) projectile2).AllComps.Add((ThingComp) new CompProjectileExitMap(this.CasterTWC)
    {
      airDefenseDef = AntiAircraftDefOf.FlakProjectile,
      target = (((GlobalTargetInfo) ref this.target).WorldObject as AerialVehicleInFlight),
      spawnPos = Building_Artillery.RandomWorldPosition(((Verb) this).caster.Map.Tile, 1).FirstOrDefault<Vector3>()
    });
    ProjectilePropertiesDefModExtension modExtension = ((Def) ((Verb) this).caster.def).GetModExtension<ProjectilePropertiesDefModExtension>();
    if (modExtension != null)
      ((ThingWithComps) projectile2).AllComps.Add((ThingComp) new CompTurretProjectileProperties(this.CasterTWC)
      {
        speed = ((double) modExtension.speed > 0.0 ? modExtension.speed : ((Thing) projectile2).def.projectile.speed),
        hitflags = (CustomHitFlags) null
      });
    this.ThrowDebugText("ToHit" + (((Verb) this).canHitNonTargetPawnsNow ? "\nchntp" : ""));
    Vector3 vector3 = Vector3.op_Addition(drawPos, Vector3Utility.RotatedBy(new Vector3(this.VerbProps.shootOffset.x, 0.0f, this.VerbProps.shootOffset.y), this.heading));
    projectile2.Launch(thing1, vector3, LocalTargetInfo.op_Implicit(intVec3), LocalTargetInfo.op_Implicit(intVec3), (ProjectileHitFlags) 1, false, thing2, (ThingDef) null);
    this.ThrowDebugText("Hit\nDest", ((ShootLine) ref shootLine).Dest);
    return (true, vector3, this.heading);
  }
}
