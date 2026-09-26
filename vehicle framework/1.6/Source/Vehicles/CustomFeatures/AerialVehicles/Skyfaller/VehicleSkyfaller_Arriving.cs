// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleSkyfaller_Arriving
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

public class VehicleSkyfaller_Arriving : VehicleSkyfaller
{
  private const int NotificationSquishInterval = 60;
  public int delayLandingTicks;
  private bool punchedRoof;
  public Rot4 rotatePostLanding = Rot4.Invalid;
  private static CompProperties_VehicleLauncher vehicleLauncherProps;

  [UsedImplicitly]
  [Obsolete("Implemented for Xml Deserialization only. Use VehicleSkyfallerMaker instead.", true)]
  public VehicleSkyfaller_Arriving()
  {
  }

  public bool LandingSpotOccupied { get; private set; }

  public Rot4 LandingRotation
  {
    get => !((Rot4) ref this.rotatePostLanding).IsValid ? this.Rotation : this.rotatePostLanding;
  }

  public CompProperties_VehicleLauncher VehicleLauncherProps
  {
    get
    {
      if (VehicleSkyfaller_Arriving.vehicleLauncherProps == null)
        VehicleSkyfaller_Arriving.vehicleLauncherProps = this.vehicle.VehicleDef.GetCompProperties<CompProperties_VehicleLauncher>();
      return VehicleSkyfaller_Arriving.vehicleLauncherProps;
    }
  }

  protected virtual void DrawAt(Vector3 drawLoc, bool flip = false)
  {
    this.launchProtocolDrawPos = this.vehicle.CompVehicleLauncher.launchProtocol.Draw(this.RootPos, 0.0f).drawPos;
    this.DrawLandingGhost();
  }

  protected override void Tick()
  {
    base.Tick();
    if (this.vehicle.CompVehicleLauncher.launchProtocol.FinishedAnimation((VehicleSkyfaller) this))
    {
      --this.delayLandingTicks;
      if (this.delayLandingTicks <= 0 && GenGrid.InBounds(this.Position, this.Map))
      {
        this.FinalizeLanding();
        return;
      }
    }
    else if (!this.punchedRoof && (double) this.vehicle.CompVehicleLauncher.launchProtocol.TimeInAnimation >= (double) this.VehicleLauncherProps.animationPunchAt)
      this.TryHitRoof();
    if (Find.TickManager.TicksGame % 60 != 0 || this.Map == null)
      return;
    this.LandingSpotOccupied = false;
    foreach (Verse.Thing thing in this.Map.thingGrid.ThingsAt(this.Position))
      this.LandingSpotOccupied |= thing is VehiclePawn;
    CellRect cellRect = this.vehicle.PawnOccupiedCells(this.Position, this.LandingRotation);
    foreach (IntVec3 cell in cellRect)
      VehicleDamager.NotifyNearbyPawnsOfDangerousPosition(this.Map, this.vehicle, cell);
  }

  public virtual void FinalizeLanding()
  {
    this.vehicle.CompVehicleLauncher.launchProtocol.Release();
    this.vehicle.CompVehicleLauncher.inFlight = false;
    if (VehicleReservationManager.AnyVehicleInhabitingCells(this.vehicle.PawnOccupiedCells(this.Position, this.LandingRotation), this.Map))
    {
      GenExplosion.DoExplosion(this.Position, this.Map, (float) Mathf.Max(((BuildableDef) this.vehicle.VehicleDef).Size.x, ((BuildableDef) this.vehicle.VehicleDef).Size.z), DamageDefOf.Bomb, (Verse.Thing) this.vehicle, -1, -1f, (SoundDef) null, (ThingDef) null, (ThingDef) null, (Verse.Thing) null, (ThingDef) null, 0.0f, 1, new GasType?(), new float?(), (int) byte.MaxValue, false, (ThingDef) null, 0.0f, 1, 0.0f, false, new float?(), (List<Verse.Thing>) null, new FloatRange?(), true, 1f, 0.0f, true, (ThingDef) null, 1f, (SimpleCurve) null, (List<IntVec3>) null, (ThingDef) null, (ThingDef) null);
    }
    else
    {
      GenSpawn.Spawn((Verse.Thing) this.vehicle, this.Position, this.Map, this.LandingRotation, (WipeMode) 0, false, false);
      this.vehicle.TryDamageObstructions();
      if (VehicleMod.settings.main.deployOnLanding)
        this.vehicle.CompVehicleLauncher.SetTimedDeployment();
    }
    this.Destroy((DestroyMode) 0);
  }

  private void DrawLandingGhost()
  {
    if (!VehicleMod.settings.main.drawLandingGhost)
      return;
    GhostDrawer.DrawGhostThing(this.Position, this.LandingRotation, (ThingDef) this.vehicle.VehicleDef, (Graphic) this.vehicle.VehicleGraphic, this.LandingSpotOccupied ? LandingTargeter.GhostOccupiedColor : VehicleGhostUtility.whiteGhostColor, (AltitudeLayer) 26, (Verse.Thing) this.vehicle, true, (ThingDef) null);
  }

  protected virtual void TryHitRoof()
  {
    this.punchedRoof = true;
    CellRect cellRect = GenAdj.OccupiedRect(this.Position, this.LandingRotation, ((BuildableDef) this.vehicle.VehicleDef).Size);
    if (!((IEnumerable<IntVec3>) (object) cellRect).Any<IntVec3>((Func<IntVec3, bool>) (cell => Ext_Vehicles.IsRoofed(cell, this.Map))))
      return;
    RoofDef roof = GridsUtility.GetRoof(((CellRect) ref cellRect).Cells.First<IntVec3>((Func<IntVec3, bool>) (cell => Ext_Vehicles.IsRoofed(cell, this.Map))), this.Map);
    if (!SoundDefHelper.NullOrUndefined(roof.soundPunchThrough))
      SoundStarter.PlayOneShot(roof.soundPunchThrough, SoundInfo.op_Implicit(new TargetInfo(this.Position, this.Map, false)));
    CellRect cellRect1 = ((CellRect) ref cellRect).ExpandedBy(1);
    CellRect cellRect2 = ((CellRect) ref cellRect1).ClipInsideMap(this.Map);
    RoofCollapserImmediate.DropRoofInCells(((CellRect) ref cellRect2).Cells.Where<IntVec3>((Func<IntVec3, bool>) (c =>
    {
      if (!GenGrid.InBounds(c, this.Map))
        return false;
      if (((CellRect) ref cellRect).Contains(c))
        return true;
      if (GridsUtility.GetFirstPawn(c, this.Map) != null)
        return false;
      Building edifice = GridsUtility.GetEdifice(c, this.Map);
      return edifice == null || !((Verse.Thing) edifice).def.holdsRoof;
    })), this.Map, (List<Verse.Thing>) null);
    foreach (IntVec3 intVec3 in cellRect)
    {
      IntVec2 cell;
      // ISSUE: explicit constructor call
      ((IntVec2) ref cell).\u002Ector(intVec3.x - this.Position.x, intVec3.z - this.Position.z);
      this.vehicle.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 5f, 0.0f, -1f, (Verse.Thing) null, (BodyPartRecord) null, (ThingDef) null, (DamageInfo.SourceCategory) 0, (Verse.Thing) null, true, true, (QualityCategory) 2, true, false), cell);
    }
  }

  public override void SpawnSetup(Map map, bool respawningAfterLoad)
  {
    base.SpawnSetup(map, respawningAfterLoad);
    if (respawningAfterLoad)
      return;
    map.GetCachedMapComponent<VehiclePathingSystem>().RequestGridsFor(this.vehicle.VehicleDef, DeferredGridGeneration.Urgency.Urgent);
    this.vehicle.CompVehicleLauncher.launchProtocol.Prepare(map, this.Position, this.Rotation);
    this.vehicle.CompVehicleLauncher.launchProtocol.OrderProtocol(LaunchProtocol.LaunchType.Landing);
    this.delayLandingTicks = this.vehicle.CompVehicleLauncher.launchProtocol.CurAnimationProperties.delayByTicks;
  }

  public override void ExposeData()
  {
    base.ExposeData();
    Scribe_Values.Look<int>(ref this.delayLandingTicks, "delayLandingTicks", 0, false);
    Scribe_Values.Look<bool>(ref this.punchedRoof, "punchedRoof", false, false);
  }
}
