// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Verb_LaunchZipline
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using SmashTools;
using System;
using UnityEngine;
using VehicleMapFramework.VMF_HarmonyPatches;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class Verb_LaunchZipline : Verb_LaunchProjectile, IAbilityVerb
{
  public Thing ziplineEnd;
  private Ability ability;

  public Ability Ability
  {
    get => this.ability;
    set => this.ability = value;
  }

  public virtual bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
  {
    Map map = ((LocalTargetInfo) ref target).Thing?.Map ?? TargetMapUtility.get_TargetMap(((Verb) this).caster) ?? (((Verb) this).caster is Pawn caster ? caster.mindState?.enemyTarget?.Map : (Map) null) ?? ((Verb) this).caster.Map;
    if (this.Ability == null && ((Verb) this).caster.Map == map)
    {
      if (showMessages)
        Messages.Message(TaggedString.op_Implicit(Translator.Translate("VMF_MustShotAtAnotherMap")), MessageTypeDefOf.RejectInput, false);
      return false;
    }
    return ((Verb) this).ValidateTarget(target, showMessages) && GenGrid.Walkable(((LocalTargetInfo) ref target).Cell, map);
  }

  public virtual bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
  {
    Thing thing = ((LocalTargetInfo) ref targ).Thing;
    return thing == null || thing != ((Verb) this).caster ? (((LocalTargetInfo) ref targ).Pawn == null || !InvisibilityUtility.IsPsychologicallyInvisible(((LocalTargetInfo) ref targ).Pawn) || !GenHostility.HostileTo(((Verb) this).caster, (Thing) ((LocalTargetInfo) ref targ).Pawn)) && !((Verb) this).ApparelPreventsShooting() && ((Verb) this).TryFindShootLineFromToOnVehicle(root, targ, out ShootLine _) : ((Verb) this).targetParams.canTargetSelf;
  }

  protected virtual bool TryCastShot()
  {
    ThingDef projectile1 = this.Projectile;
    if (projectile1 == null)
      return false;
    LocalTargetInfo currentTarget = ((Verb) this).currentTarget;
    Map map = ((LocalTargetInfo) ref currentTarget).Thing?.Map ?? TargetMapUtility.get_TargetMap(((Verb) this).caster) ?? (((Verb) this).caster is Pawn caster ? caster.mindState?.enemyTarget?.Map : (Map) null) ?? ((Verb) this).caster.Map;
    ShootLine resultingLine;
    bool lineFromToOnVehicle = ((Verb) this).TryFindShootLineFromToOnVehicle(VehicleMapUtility.get_PositionOnBaseMap(((Verb) this).caster), currentTarget, out resultingLine);
    if (((Verb) this).verbProps.stopBurstWithoutLos && !lineFromToOnVehicle)
      return false;
    if (((Verb) this).EquipmentSource != null)
    {
      ((Verb) this).EquipmentSource.GetComp<CompChangeableProjectile>()?.Notify_ProjectileLaunched();
      ((CompApparelVerbOwner) ((Verb) this).EquipmentSource.GetComp<CompApparelVerbOwner_Charged>())?.UsedOnce();
    }
    ((Verb) this).lastShotTick = Find.TickManager.TicksGame;
    Thing thing1 = ((Verb) this).caster;
    Thing thing2 = (Thing) ((Verb) this).EquipmentSource;
    CompMannable comp = ThingCompUtility.TryGetComp<CompMannable>(((Verb) this).caster);
    if (comp?.ManningPawn != null)
    {
      thing1 = (Thing) comp.ManningPawn;
      thing2 = ((Verb) this).caster;
    }
    Vector3 drawPos = ((Verb) this).caster.DrawPos;
    BuildingProperties building = ((Verb) this).caster.def.building;
    Vector3 vector3_1 = building != null ? Vector2Utility.ToVector3(building.turretTopOffset) : Vector3.zero;
    VehiclePawnWithMap vehicle;
    if (((Verb) this).caster.IsOnNonFocusedVehicleMapOf(out vehicle))
      vector3_1 = Vector3Utility.RotatedBy(vector3_1, -vehicle.Angle + vehicle.Transform.rotation);
    Vector3 vector3_2 = Vector3.op_Addition(drawPos, vector3_1);
    Projectile projectile2 = (Projectile) ThingMaker.MakeThing(projectile1, (ThingDef) null);
    if (projectile2 is Bullet_ZiplineEnd bulletZiplineEnd)
    {
      bulletZiplineEnd.launchVerb = this;
      bulletZiplineEnd.destMap = map;
      CustomZipline modExtension = ((Def) ((Thing) bulletZiplineEnd).def).GetModExtension<CustomZipline>();
      if (modExtension != null)
        bulletZiplineEnd.ZipLineData = modExtension.zipLineData;
    }
    GenSpawn.Spawn((Thing) projectile2, ((ShootLine) ref resultingLine).Source, VehicleMapUtility.get_GroundMap(((Verb) this).caster), (WipeMode) 0);
    if ((double) ((Verb) this).verbProps.ForcedMissRadius > 0.5)
    {
      float forcedMissRadius = ((Verb) this).verbProps.ForcedMissRadius;
      if (thing1 is Pawn pawn)
        forcedMissRadius *= ((Verb) this).verbProps.GetForceMissFactorFor(thing2, pawn);
      float adjustedForcedMiss = VerbUtility.CalculateAdjustedForcedMiss(forcedMissRadius, IntVec3.op_Subtraction(((LocalTargetInfo) ref currentTarget).Cell.ToBaseMapCoord(map), VehicleMapUtility.get_PositionOnBaseMap(((Verb) this).caster)));
      if ((double) adjustedForcedMiss > 0.5)
      {
        IntVec3 forcedMissTarget = this.GetForcedMissTarget(adjustedForcedMiss);
        if (IntVec3.op_Inequality(forcedMissTarget, ((LocalTargetInfo) ref currentTarget).Cell))
        {
          ProjectileHitFlags projectileHitFlags = (ProjectileHitFlags) 4;
          if (Rand.Chance(0.5f))
            projectileHitFlags = (ProjectileHitFlags) -1;
          if (!((Verb) this).canHitNonTargetPawnsNow)
            projectileHitFlags = (ProjectileHitFlags) (projectileHitFlags & -3);
          projectile2.Launch(thing1, vector3_2, LocalTargetInfo.op_Implicit(forcedMissTarget), currentTarget, projectileHitFlags, ((Verb) this).preventFriendlyFire, thing2, (ThingDef) null);
          return true;
        }
      }
    }
    ShotReport shotReport = ShotReport.HitReportFor(((Verb) this).caster, (Verb) this, currentTarget);
    Thing randomCoverToMissInto = ((ShotReport) ref shotReport).GetRandomCoverToMissInto();
    ThingDef def = randomCoverToMissInto?.def;
    if (((Verb) this).verbProps.canGoWild && !Rand.Chance(((ShotReport) ref shotReport).AimOnTargetChance_IgnoringPosture))
    {
      ProjectileProperties projectile3 = ((Thing) projectile2)?.def?.projectile;
      bool flag = projectile3 != null && projectile3.flyOverhead;
      ((ShootLine) ref resultingLine).ChangeDestToMissWild(((ShotReport) ref shotReport).AimOnTargetChance_StandardTarget, flag, ((Verb) this).caster.BaseMap());
      ProjectileHitFlags projectileHitFlags = (ProjectileHitFlags) 4;
      if (Rand.Chance(0.5f) && ((Verb) this).canHitNonTargetPawnsNow)
        projectileHitFlags = (ProjectileHitFlags) (projectileHitFlags | 2);
      projectile2.Launch(thing1, vector3_2, LocalTargetInfo.op_Implicit(((ShootLine) ref resultingLine).Dest), currentTarget, projectileHitFlags, ((Verb) this).preventFriendlyFire, thing2, def);
      return true;
    }
    if (((LocalTargetInfo) ref currentTarget).Thing != null && ((LocalTargetInfo) ref currentTarget).Thing.def.CanBenefitFromCover && !Rand.Chance(((ShotReport) ref shotReport).PassCoverChance))
    {
      ProjectileHitFlags projectileHitFlags = (ProjectileHitFlags) 4;
      if (((Verb) this).canHitNonTargetPawnsNow)
        projectileHitFlags = (ProjectileHitFlags) (projectileHitFlags | 2);
      projectile2.Launch(thing1, vector3_2, LocalTargetInfo.op_Implicit(randomCoverToMissInto), currentTarget, projectileHitFlags, ((Verb) this).preventFriendlyFire, thing2, def);
      return true;
    }
    projectile2.Launch(thing1, vector3_2, ((LocalTargetInfo) ref currentTarget).Thing != null ? currentTarget : LocalTargetInfo.op_Implicit(((ShootLine) ref resultingLine).Dest), currentTarget, (ProjectileHitFlags) 1, ((Verb) this).preventFriendlyFire, thing2, def);
    return true;
  }

  public virtual void DrawHighlight(LocalTargetInfo target)
  {
    Thing caster = ((Verb) this).caster;
    if (caster != null && !caster.Spawned)
      return;
    Map targetMapOrThingMap = TargetMapUtility.get_TargetMapOrThingMap(((Verb) this).caster);
    if (((LocalTargetInfo) ref target).IsValid && JumpUtility.ValidJumpTarget(((Verb) this).caster, targetMapOrThingMap, ((LocalTargetInfo) ref target).Cell))
      GenDraw.DrawTargetHighlightWithLayer(Patch_Verb_Jump_DrawHighlight.CenterVector3Offset(ref target, (Verb) this), (AltitudeLayer) 39);
    Map baseMap = VehicleMapUtility.get_GroundMap(((Verb) this).caster);
    GenDraw.DrawRadiusRing(((Verb) this).caster.Position, ((Verb) this).EffectiveRange, Color.white, (Func<IntVec3, bool>) (c =>
    {
      if (!GenSightOnVehicle.LineOfSight(VehicleMapUtility.get_PositionOnBaseMap(((Verb) this).caster), c, baseMap, false))
        return false;
      if (JumpUtility.ValidJumpTarget(((Verb) this).caster, baseMap, c))
        return true;
      if (GenGrid.InBounds(c, baseMap))
      {
        VehiclePawnWithMap vehicle = ComponentCache.GetCachedMapComponent<VehicleMapGrid>(baseMap).VehicleAt(c);
        if (vehicle != null)
          return JumpUtility.ValidJumpTarget(((Verb) this).caster, vehicle.VehicleMap, c.ToVehicleMapCoord(vehicle));
      }
      return false;
    }));
  }

  public virtual void OnGUI(LocalTargetInfo target)
  {
    if (!((LocalTargetInfo) ref target).IsValid)
      return;
    if (((Verb) this).CanHitTarget(target) && JumpUtility.ValidJumpTarget(((Verb) this).caster, TargetMapUtility.get_TargetMapOrThingMap(((Verb) this).caster), ((LocalTargetInfo) ref target).Cell))
      ((Verb) this).OnGUI(target);
    else
      GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
  }

  public virtual void ExposeData()
  {
    ((Verb) this).ExposeData();
    Scribe_References.Look<Ability>(ref this.ability, "ability", false);
  }
}
