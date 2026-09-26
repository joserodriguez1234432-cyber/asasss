// Decompiled with JetBrains decompiler
// Type: Vehicles.Building_Artillery
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vehicles.World;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

[StaticConstructorOnStartup]
public class Building_Artillery : Building_TurretGun
{
  public const float WorldObjectOffsetPercent = 0.1f;
  public const float MaxMapDistance = 500f;
  private static readonly Material AimPieMaterial = SolidColorMaterials.SimpleSolidColorMaterial(new Color(1f, 1f, 1f, 0.3f), false);
  protected GlobalTargetInfo worldTarget = GlobalTargetInfo.Invalid;
  protected GlobalTargetInfo forcedWorldTarget = GlobalTargetInfo.Invalid;
  protected GlobalTargetInfo lastAttackedWorldTarget;
  protected int lastAttackedWorldTargetTick;

  public Building_Artillery()
  {
    this.top = (TurretTop) Activator.CreateInstance(typeof (TurretTop_Artillery), (object) this);
  }

  protected TurretTop_Artillery TopArtillery => this.top as TurretTop_Artillery;

  public virtual Verb ActiveVerb { get; protected set; }

  public virtual Verb AttackVerb => this.ActiveVerb ?? base.AttackVerb;

  public bool WarmingUp => this.burstWarmupTicksLeft > 0;

  public bool PlayerControlled
  {
    get
    {
      return (((Thing) this).Faction == Faction.OfPlayer || this.MannedByColonist) && !this.MannedByNonColonist;
    }
  }

  public bool CanToggleHoldFire => this.PlayerControlled;

  public bool IsMortar => ((Thing) this).def.building.IsMortar;

  public bool IsMortarOrProjectileFliesOverhead
  {
    get
    {
      return VerbUtility.ProjectileFliesOverhead(((Building_Turret) this).AttackVerb) || this.IsMortar;
    }
  }

  public bool CanExtractShell
  {
    get
    {
      if (!this.PlayerControlled)
        return false;
      CompChangeableProjectile comp = ThingCompUtility.TryGetComp<CompChangeableProjectile>(this.gun);
      return comp != null && comp.Loaded;
    }
  }

  public bool MannedByColonist
  {
    get
    {
      return this.mannableComp != null && this.mannableComp.ManningPawn != null && ((Thing) this.mannableComp.ManningPawn).Faction == Faction.OfPlayer;
    }
  }

  public bool MannedByNonColonist
  {
    get
    {
      return this.mannableComp != null && this.mannableComp.ManningPawn != null && ((Thing) this.mannableComp.ManningPawn).Faction != Faction.OfPlayer;
    }
  }

  public virtual GlobalTargetInfo CurrentWorldTarget => this.worldTarget;

  public virtual IEnumerable<Gizmo> GetGizmos()
  {
    Building_Artillery buildingArtillery = this;
    // ISSUE: reference to a compiler-generated method
    foreach (Gizmo gizmo in buildingArtillery.\u003C\u003En__0())
      yield return gizmo;
    Command_Action gizmo1 = new Command_Action();
    ((Command) gizmo1).defaultLabel = buildingArtillery.ActiveVerb.ToString();
    // ISSUE: reference to a compiler-generated method
    gizmo1.action = new Action(buildingArtillery.\u003CGetGizmos\u003Eb__34_0);
    yield return (Gizmo) gizmo1;
    Command_Action gizmo2 = new Command_Action();
    ((Command) gizmo2).defaultLabel = "Reset Target";
    // ISSUE: reference to a compiler-generated method
    gizmo2.action = new Action(buildingArtillery.\u003CGetGizmos\u003Eb__34_1);
    yield return (Gizmo) gizmo2;
  }

  protected virtual void DrawAt(Vector3 drawLoc, bool flip = false)
  {
    this.TopArtillery.DrawTurret();
    if (((Thing) this).def.drawerType == 1)
      ((Thing) this).DrawAt(drawLoc, false);
    else
      ((ThingWithComps) this).Comps_PostDraw();
  }

  protected virtual void Tick()
  {
    base.Tick();
    if (this.TopArtillery is TurretTop_Recoiled topArtillery)
      topArtillery.RecoilTick();
    if (!this.Active || this.mannableComp != null && !this.mannableComp.MannedNow || ((Building_Turret) this).IsStunned || !((Thing) this).Spawned || ((Building_Turret) this).AttackVerb.state == 1)
      return;
    this.TopArtillery.Tick();
  }

  protected virtual void ExtractShell()
  {
    GenPlace.TryPlaceThing(ThingCompUtility.TryGetComp<CompChangeableProjectile>(this.gun).RemoveShell(), ((Thing) this).Position, ((Thing) this).Map, (ThingPlaceMode) 1, (Action<Thing, int>) null, (Predicate<IntVec3>) null, new Rot4?(), 1);
  }

  protected virtual void ResetForcedTarget()
  {
    ((Building_Turret) this).forcedTarget = LocalTargetInfo.Invalid;
    this.forcedWorldTarget = GlobalTargetInfo.Invalid;
    this.burstWarmupTicksLeft = 0;
    if (this.burstCooldownTicksLeft > 0)
      return;
    this.TryStartShootSomething(false);
  }

  protected virtual void ResetCurrentTarget()
  {
    this.currentTargetInt = LocalTargetInfo.Invalid;
    this.worldTarget = GlobalTargetInfo.Invalid;
    this.burstWarmupTicksLeft = 0;
  }

  public virtual void NotifyTargetInRange(GlobalTargetInfo target) => this.worldTarget = target;

  public virtual void NotifyTargetOutOfRange(GlobalTargetInfo target)
  {
    if (!GlobalTargetInfo.op_Equality(this.worldTarget, target))
      return;
    this.ResetCurrentTarget();
  }

  protected void OnAttackedTarget(GlobalTargetInfo target)
  {
    this.lastAttackedWorldTargetTick = Find.TickManager.TicksGame;
    this.lastAttackedWorldTarget = target;
  }

  protected virtual void BeginBurst()
  {
    GlobalTargetInfo currentWorldTarget = this.CurrentWorldTarget;
    if (((GlobalTargetInfo) ref currentWorldTarget).IsValid && ((Building_Turret) this).AttackVerb is Verb_ShootWorldRecoiled attackVerb)
      attackVerb.TryStartCastOn(this.CurrentWorldTarget, this.TopArtillery.CurRotation, canHitNonTargetPawns: false);
    LocalTargetInfo currentTarget = ((Building_Turret) this).CurrentTarget;
    if (!((LocalTargetInfo) ref currentTarget).IsValid)
      return;
    base.BeginBurst();
  }

  protected virtual void TryStartShootSomething(bool canBeginBurstImmediately)
  {
    if (this.progressBarEffecter != null)
    {
      this.progressBarEffecter.Cleanup();
      this.progressBarEffecter = (Effecter) null;
    }
    if (!((Thing) this).Spawned || (bool) AccessTools.Field(typeof (Building_TurretGun), "holdFire")?.GetValue((object) this) && this.CanToggleHoldFire || VerbUtility.ProjectileFliesOverhead(((Building_Turret) this).AttackVerb) && ((Thing) this).Map.roofGrid.Roofed(((Thing) this).Position) || !((Building_Turret) this).AttackVerb.Available())
    {
      this.ResetCurrentTarget();
    }
    else
    {
      bool flag = ((LocalTargetInfo) ref this.currentTargetInt).IsValid || ((GlobalTargetInfo) ref this.worldTarget).IsValid;
      if (((LocalTargetInfo) ref ((Building_Turret) this).forcedTarget).IsValid)
        this.currentTargetInt = ((Building_Turret) this).forcedTarget;
      else
        this.currentTargetInt = this.TryFindNewTarget();
      if (((GlobalTargetInfo) ref this.forcedWorldTarget).IsValid)
        this.worldTarget = this.forcedWorldTarget;
      if (!flag && (((LocalTargetInfo) ref this.currentTargetInt).IsValid || ((GlobalTargetInfo) ref this.worldTarget).IsValid))
        SoundStarter.PlayOneShot(SoundDefOf.TurretAcquireTarget, SoundInfo.op_Implicit(new TargetInfo(((Thing) this).Position, ((Thing) this).Map, false)));
      if (!((LocalTargetInfo) ref this.currentTargetInt).IsValid && !((GlobalTargetInfo) ref this.worldTarget).IsValid)
      {
        this.ResetCurrentTarget();
      }
      else
      {
        float randomInRange = ((FloatRange) ref ((Thing) this).def.building.turretBurstWarmupTime).RandomInRange;
        if ((double) randomInRange > 0.0)
          this.burstWarmupTicksLeft = GenTicks.SecondsToTicks(randomInRange);
        else if (canBeginBurstImmediately)
          this.BeginBurst();
        else
          this.burstWarmupTicksLeft = 1;
      }
    }
  }

  public virtual void DrawExtraSelectionOverlays()
  {
    float range = ((Building_Turret) this).AttackVerb.verbProps.range;
    if ((double) range < 90.0)
      GenDraw.DrawRadiusRing(((Thing) this).Position, range);
    float num1 = ((Building_Turret) this).AttackVerb.verbProps.EffectiveMinRange(true);
    if ((double) num1 < 90.0 && (double) num1 > 0.10000000149011612)
      GenDraw.DrawRadiusRing(((Thing) this).Position, num1);
    if (this.WarmingUp)
    {
      int index = (int) ((double) this.burstWarmupTicksLeft * 0.5);
      float num2 = (float) ((Thing) this).def.size.x * 0.5f;
      LocalTargetInfo currentTarget = ((Building_Turret) this).CurrentTarget;
      if (!((LocalTargetInfo) ref currentTarget).IsValid)
      {
        GlobalTargetInfo currentWorldTarget = this.CurrentWorldTarget;
        if (!((GlobalTargetInfo) ref currentWorldTarget).IsValid)
          goto label_10;
      }
      Vector2 zero = Vector2.zero;
      if (((Building_Turret) this).AttackVerb.verbProps is VerbProperties_Animated verbProps)
      {
        Vector3 vector3 = Vector3Utility.RotatedBy(new Vector3(verbProps.shootOffset.x, 0.0f, verbProps.shootOffset.y), this.TopArtillery.CurRotation);
        zero.x = vector3.x;
        zero.y = vector3.z;
      }
      Vector3 vector3_1 = Vector3.op_Addition(Vector3.op_Addition(((Thing) this).DrawPos, new Vector3(zero.x, num2, zero.y)), Vector3.op_Multiply(Quaternion.op_Multiply(Quaternion.AngleAxis(this.TopArtillery.CurRotation, Vector3.up), Vector3.forward), 0.8f));
      Graphics.DrawMesh(MeshPool.pies[index], vector3_1, Quaternion.AngleAxis((float) ((double) this.TopArtillery.CurRotation + (double) (index / 2) - 90.0), Vector3.up), Building_Artillery.AimPieMaterial, 0);
    }
label_10:
    if (!((LocalTargetInfo) ref ((Building_Turret) this).forcedTarget).IsValid || ((LocalTargetInfo) ref ((Building_Turret) this).forcedTarget).HasThing && !((LocalTargetInfo) ref ((Building_Turret) this).forcedTarget).Thing.Spawned)
      return;
    Vector3 vector3_2;
    if (((LocalTargetInfo) ref ((Building_Turret) this).forcedTarget).HasThing)
    {
      vector3_2 = GenThing.TrueCenter(((LocalTargetInfo) ref ((Building_Turret) this).forcedTarget).Thing);
    }
    else
    {
      IntVec3 cell = ((LocalTargetInfo) ref ((Building_Turret) this).forcedTarget).Cell;
      vector3_2 = ((IntVec3) ref cell).ToVector3Shifted();
    }
    Vector3 vector3_3 = GenThing.TrueCenter((Thing) this);
    vector3_2.y = Altitudes.AltitudeFor((AltitudeLayer) 39);
    vector3_3.y = vector3_2.y;
    GenDraw.DrawLineBetween(vector3_3, vector3_2, Building_TurretGun.ForcedTargetLineMat, 0.2f);
  }

  public virtual void DeSpawn(DestroyMode mode = 0) => base.DeSpawn(mode);

  public virtual void Destroy(DestroyMode mode = 0) => ((Building) this).Destroy(mode);

  public virtual void SpawnSetup(Map map, bool respawningAfterLoad)
  {
    base.SpawnSetup(map, respawningAfterLoad);
    if (!respawningAfterLoad)
    {
      this.TopArtillery.SetRotationFromOrientation();
      this.burstCooldownTicksLeft = GenTicks.SecondsToTicks(((Thing) this).def.building.turretInitialCooldownTime);
    }
    this.ActiveVerb = base.AttackVerb;
    this.TopArtillery.PostSpawnSetup();
  }

  public static IEnumerable<Vector3> RandomWorldPosition(PlanetTile tile, int numPositions)
  {
    List<PlanetTile> neighborTiles = new List<PlanetTile>();
    Find.WorldGrid.GetTileNeighbors(tile, neighborTiles);
    WorldObject tileObject = WorldHelper.WorldObjectAt(tile);
    for (int i = 0; i < numPositions; ++i)
    {
      PlanetTile tile1 = GenCollection.RandomElement<PlanetTile>((IEnumerable<PlanetTile>) neighborTiles);
      float num = Rand.Range(0.0f, 0.1f);
      WorldObject worldObject = WorldHelper.WorldObjectAt(tile);
      bool spaceObject;
      yield return Vector3.Slerp(WorldHelper.GetTilePos(tile, tileObject, out spaceObject), WorldHelper.GetTilePos(tile1, worldObject, out spaceObject), num);
    }
  }
}
