// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleSkyfaller_Crashing
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools;
using SmashTools.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

public class VehicleSkyfaller_Crashing : VehicleSkyfaller_Arriving
{
  public const float DefaultAngle = -65f;
  private const int RoofHitPreDelay = 15;
  private const int LeaveMapAfterTicks = 220;
  public Rot4 rotCrashing;
  public float shrapnelDirection;
  public int ticksToImpact = 220;

  [UsedImplicitly]
  [Obsolete("Implemented for Xml Deserialization only. Use VehicleSkyfallerMaker instead.", true)]
  public VehicleSkyfaller_Crashing()
  {
  }

  private bool SpawnTimedMotes
  {
    get
    {
      return !Mathf.Approximately(this.def.skyfaller.moteSpawnTime, float.MinValue) && Mathf.Approximately(this.def.skyfaller.moteSpawnTime, this.vehicle.CompVehicleLauncher.launchProtocol.TimeInAnimation);
    }
  }

  public override Vector3 DrawPos
  {
    get
    {
      switch ((int) this.def.skyfaller.movementType)
      {
        case 0:
          return SkyfallerDrawPosUtility.DrawPos_Accelerate(base.DrawPos, this.ticksToImpact, this.angle, this.CurrentSpeed, false, (CompSkyfallerRandomizeDirection) null);
        case 1:
          return SkyfallerDrawPosUtility.DrawPos_ConstantSpeed(base.DrawPos, this.ticksToImpact, this.angle, this.CurrentSpeed, false, (CompSkyfallerRandomizeDirection) null);
        case 2:
          return SkyfallerDrawPosUtility.DrawPos_Decelerate(base.DrawPos, this.ticksToImpact, this.angle, this.CurrentSpeed, false, (CompSkyfallerRandomizeDirection) null);
        default:
          Log.ErrorOnce("SkyfallerMovementType not handled: " + this.def.skyfaller.movementType.ToString(), this.thingIDNumber ^ 1948576711);
          return SkyfallerDrawPosUtility.DrawPos_Accelerate(base.DrawPos, this.ticksToImpact, this.angle, this.CurrentSpeed, false, (CompSkyfallerRandomizeDirection) null);
      }
    }
  }

  protected virtual float CurrentSpeed
  {
    get
    {
      return this.def.skyfaller.speedCurve == null ? this.def.skyfaller.speed : this.def.skyfaller.speedCurve.Evaluate(this.vehicle.CompVehicleLauncher.launchProtocol.TimeInAnimation) * this.def.skyfaller.speed;
    }
  }

  public override void ExposeData()
  {
    base.ExposeData();
    Scribe_Values.Look<Rot4>(ref this.rotCrashing, "rotCrashing", Rot4.East, false);
  }

  protected override void DrawAt(Vector3 drawLoc, bool flip = false)
  {
    float num1 = 0.0f;
    if (this.def.skyfaller.rotateGraphicTowardsDirection)
      num1 = this.angle;
    if (this.def.skyfaller.angleCurve != null)
      this.angle = this.def.skyfaller.angleCurve.Evaluate(this.vehicle.CompVehicleLauncher.launchProtocol.TimeInAnimation);
    if (this.def.skyfaller.rotationCurve != null)
      num1 += this.def.skyfaller.rotationCurve.Evaluate(this.vehicle.CompVehicleLauncher.launchProtocol.TimeInAnimation);
    if (this.def.skyfaller.xPositionCurve != null)
      drawLoc.x += this.def.skyfaller.xPositionCurve.Evaluate(this.vehicle.CompVehicleLauncher.launchProtocol.TimeInAnimation);
    if (this.def.skyfaller.zPositionCurve != null)
      drawLoc.z += this.def.skyfaller.zPositionCurve.Evaluate(this.vehicle.CompVehicleLauncher.launchProtocol.TimeInAnimation);
    VehiclePawn vehicle = this.vehicle;
    ref Vector3 local = ref drawLoc;
    Rot8 rotation1 = (Rot8) this.Rotation;
    double num2 = (double) num1;
    Rot4 rotation2 = this.Rotation;
    double num3 = (double) (((Rot4) ref rotation2).AsInt * 90);
    double rotation3 = num2 + num3;
    vehicle.DrawAt(in local, rotation1, (float) rotation3);
    this.DrawDropSpotShadow();
  }

  protected override void Tick()
  {
    if (this.SpawnTimedMotes)
    {
      CellRect cellRect = GenAdj.OccupiedRect((Verse.Thing) this);
      for (int index = 0; index < ((CellRect) ref cellRect).Area * this.def.skyfaller.motesPerCell; ++index)
        FleckMaker.ThrowDustPuff(((CellRect) ref cellRect).RandomVector3, this.Map, 2f);
    }
    --this.ticksToImpact;
    if (this.ticksToImpact == 15)
      this.HitRoof();
    if (!this.anticipationSoundPlayed && this.def.skyfaller.anticipationSound != null && this.ticksToImpact < this.def.skyfaller.anticipationSoundTicks)
    {
      this.anticipationSoundPlayed = true;
      SoundStarter.PlayOneShot(this.def.skyfaller.anticipationSound, SoundInfo.op_Implicit(new TargetInfo(this.Position, this.Map, false)));
    }
    if (this.ticksToImpact == 0)
    {
      this.Impact();
    }
    else
    {
      if (this.ticksToImpact >= 0)
        return;
      Log.Error("ticksToImpact < 0. Was there an exception? Destroying skyfaller.");
      this.Destroy((DestroyMode) 0);
    }
  }

  protected virtual void Impact()
  {
    if (this.def.skyfaller.CausesExplosion)
    {
      IntVec3 position = this.Position;
      Map map = this.Map;
      double explosionRadius = (double) this.def.skyfaller.explosionRadius;
      DamageDef explosionDamage = this.def.skyfaller.explosionDamage;
      int num = GenMath.RoundRandom((float) this.def.skyfaller.explosionDamage.defaultDamage * this.def.skyfaller.explosionDamageFactor);
      List<Verse.Thing> list = !this.def.skyfaller.damageSpawnedThings ? ((IEnumerable<Verse.Thing>) this.vehicle.inventory.innerContainer).ToList<Verse.Thing>() : (List<Verse.Thing>) null;
      GasType? nullable1 = new GasType?();
      float? nullable2 = new float?();
      float? nullable3 = new float?();
      List<Verse.Thing> thingList = list;
      FloatRange? nullable4 = new FloatRange?();
      GenExplosion.DoExplosion(position, map, (float) explosionRadius, explosionDamage, (Verse.Thing) null, num, -1f, (SoundDef) null, (ThingDef) null, (ThingDef) null, (Verse.Thing) null, (ThingDef) null, 0.0f, 1, nullable1, nullable2, (int) byte.MaxValue, false, (ThingDef) null, 0.0f, 1, 0.0f, false, nullable3, thingList, nullable4, true, 1f, 0.0f, true, (ThingDef) null, 1f, (SimpleCurve) null, (List<IntVec3>) null, (ThingDef) null, (ThingDef) null);
    }
    CellRect cellRect = GenAdj.OccupiedRect((Verse.Thing) this);
    for (int index = 0; index < ((CellRect) ref cellRect).Area * this.def.skyfaller.motesPerCell; ++index)
      FleckMaker.ThrowDustPuff(((CellRect) ref cellRect).RandomVector3, this.Map, 2f);
    if (this.def.skyfaller.MakesShrapnel)
      SkyfallerShrapnelUtility.MakeShrapnel(this.Position, this.Map, this.shrapnelDirection, this.def.skyfaller.shrapnelDistanceFactor, ((IntRange) ref this.def.skyfaller.metalShrapnelCountRange).RandomInRange, ((IntRange) ref this.def.skyfaller.rubbleShrapnelCountRange).RandomInRange, true);
    if ((double) this.def.skyfaller.cameraShake > 0.0 && this.Map == Find.CurrentMap)
      Find.CameraDriver.shaker.DoShake(this.def.skyfaller.cameraShake);
    if (this.def.skyfaller.impactSound != null)
      SoundStarter.PlayOneShot(this.def.skyfaller.impactSound, SoundInfo.InMap(new TargetInfo(this.Position, this.Map, false), (MaintenanceType) 0));
    this.FinalizeLanding();
  }

  protected virtual void HitRoof()
  {
    if (!this.def.skyfaller.hitRoof)
      return;
    CellRect cr = GenAdj.OccupiedRect((Verse.Thing) this);
    if (!((CellRect) ref cr).Cells.Any<IntVec3>((Func<IntVec3, bool>) (x => Ext_Vehicles.IsRoofed(x, this.Map))))
      return;
    RoofDef roof = GridsUtility.GetRoof(((CellRect) ref cr).Cells.First<IntVec3>((Func<IntVec3, bool>) (x => Ext_Vehicles.IsRoofed(x, this.Map))), this.Map);
    if (!SoundDefHelper.NullOrUndefined(roof.soundPunchThrough))
      SoundStarter.PlayOneShot(roof.soundPunchThrough, SoundInfo.op_Implicit(new TargetInfo(this.Position, this.Map, false)));
    CellRect cellRect = ((CellRect) ref cr).ExpandedBy(1);
    cellRect = ((CellRect) ref cellRect).ClipInsideMap(this.Map);
    RoofCollapserImmediate.DropRoofInCells(((CellRect) ref cellRect).Cells.Where<IntVec3>((Func<IntVec3, bool>) (c =>
    {
      if (!GenGrid.InBounds(c, this.Map))
        return false;
      if (((CellRect) ref cr).Contains(c))
        return true;
      if (GridsUtility.GetFirstPawn(c, this.Map) != null)
        return false;
      Building edifice = GridsUtility.GetEdifice(c, this.Map);
      return edifice == null || !((Verse.Thing) edifice).def.holdsRoof;
    })), this.Map, (List<Verse.Thing>) null);
  }

  public virtual void PostMake()
  {
    base.PostMake();
    if (!this.def.skyfaller.MakesShrapnel)
      return;
    this.shrapnelDirection = Rand.Range(0.0f, 360f);
  }

  public override void FinalizeLanding()
  {
    this.vehicle.CompVehicleLauncher.inFlight = false;
    GenSpawn.Spawn((Verse.Thing) this.vehicle, this.Position, this.Map, this.Rotation, (WipeMode) 0, false, false);
    Transform transform = this.vehicle.Transform;
    double angle = (double) this.angle;
    Rot4 rotation = this.Rotation;
    double asAngle = (double) ((Rot4) ref rotation).AsAngle;
    double num = angle + asAngle;
    transform.rotation = (float) num;
    this.vehicle.EventRegistry[VehicleEventDefOf.Repaired].AddSingle("Transform", new Action(this.vehicle.Transform.Reset));
    this.vehicle.DisembarkAll();
    this.vehicle.ignition.Drafted = false;
    this.Destroy((DestroyMode) 0);
  }

  public override void SpawnSetup(Map map, bool respawningAfterLoad)
  {
    base.SpawnSetup(map, respawningAfterLoad);
    if (respawningAfterLoad)
      return;
    this.vehicle.CompVehicleLauncher.launchProtocol.Prepare(map, this.Position, this.Rotation);
    this.vehicle.CompVehicleLauncher.launchProtocol.OrderProtocol(LaunchProtocol.LaunchType.Landing);
    this.delayLandingTicks = this.vehicle.CompVehicleLauncher.launchProtocol.CurAnimationProperties.delayByTicks;
    this.ticksToImpact = ((IntRange) ref this.def.skyfaller.ticksToImpactRange).RandomInRange;
    if (this.def.skyfaller.MakesShrapnel)
    {
      float num = GenMath.PositiveMod(this.shrapnelDirection, 360f);
      if ((double) num < 270.0 && (double) num >= 90.0)
        this.angle = Rand.Range(0.0f, 33f);
      else
        this.angle = Rand.Range(-33f, 0.0f);
    }
    else if (this.def.skyfaller.angleCurve != null)
      this.angle = this.def.skyfaller.angleCurve.Evaluate(0.0f);
    else
      this.angle = -65f;
    this.Rotation = this.rotCrashing;
  }
}
