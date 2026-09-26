// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleSkyfaller
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

[StaticConstructorOnStartup]
public abstract class VehicleSkyfaller : 
  Verse.Thing,
  IThingHolderTickable,
  IThingHolder,
  IRoofCollapseAlert,
  ISustainerTarget
{
  protected static MaterialPropertyBlock shadowPropertyBlock = new MaterialPropertyBlock();
  public float angle;
  protected Vector3 launchProtocolDrawPos;
  protected Material cachedShadowMaterial;
  protected bool anticipationSoundPlayed;
  public VehiclePawn vehicle;
  private readonly ThingOwner<VehiclePawn> innerContainer = new ThingOwner<VehiclePawn>();

  public virtual Vector3 DrawPos => this.launchProtocolDrawPos;

  protected Vector3 RootPos => this.vehicle.TrueCenter(this.Position, new float?(base.DrawPos.y));

  public ThingWithComps Thing => (ThingWithComps) this.vehicle;

  bool IThingHolderTickable.ShouldTickContents => true;

  TargetInfo ISustainerTarget.Target => TargetInfo.op_Implicit((Verse.Thing) this);

  MaintenanceType ISustainerTarget.MaintenanceType => (MaintenanceType) 1;

  private Material ShadowMaterial
  {
    get
    {
      if (this.cachedShadowMaterial == null && !GenText.NullOrEmpty(this.def.skyfaller.shadow))
        this.cachedShadowMaterial = MaterialPool.MatFrom(this.def.skyfaller.shadow, ShaderDatabase.Transparent);
      return this.cachedShadowMaterial;
    }
  }

  protected virtual void Tick() => this.vehicle.CompVehicleLauncher.launchProtocol.Tick();

  protected virtual void LeaveMap() => this.Destroy((DestroyMode) 0);

  public virtual void DeSpawn(DestroyMode mode = 0)
  {
    ((ThingOwner) this.innerContainer).Remove((Verse.Thing) this.vehicle);
    if (!((Verse.Thing) this.vehicle).Spawned)
      this.Map.GetDetachedMapComponent<VehiclePositionManager>().ReleaseClaimed(this.vehicle);
    base.DeSpawn(mode);
    this.vehicle.ReleaseSustainerTarget();
  }

  protected virtual void DrawDropSpotShadow()
  {
    Material shadowMaterial = this.ShadowMaterial;
    if (shadowMaterial == null)
      return;
    VehicleSkyfaller.DrawDropSpotShadow(base.DrawPos, this.Rotation, shadowMaterial, this.def.skyfaller.shadowSize, this.vehicle.CompVehicleLauncher.launchProtocol.TicksPassed);
  }

  public static void DrawDropSpotShadow(
    Vector3 center,
    Rot4 rot,
    Material material,
    Vector2 shadowSize,
    int ticksToLand)
  {
    if (((Rot4) ref rot).IsHorizontal)
      Gen.Swap<float>(ref shadowSize.x, ref shadowSize.y);
    ticksToLand = Mathf.Max(ticksToLand, 0);
    Vector3 vector3_1 = center;
    vector3_1.y = Altitudes.AltitudeFor((AltitudeLayer) 13);
    float num = (float) (1.0 + (double) ticksToLand / 100.0);
    Vector3 vector3_2;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3_2).\u002Ector(num * shadowSize.x, 1f, num * shadowSize.y);
    Color white = Color.white;
    if (ticksToLand > 150)
      white.a = Mathf.InverseLerp(200f, 150f, (float) ticksToLand);
    VehicleSkyfaller.shadowPropertyBlock.SetColor(ShaderPropertyIDs.Color, white);
    Matrix4x4 matrix4x4 = new Matrix4x4();
    ((Matrix4x4) ref matrix4x4).SetTRS(vector3_1, ((Rot4) ref rot).AsQuat, vector3_2);
    Graphics.DrawMesh(MeshPool.plane10Back, matrix4x4, material, 0, (Camera) null, 0, VehicleSkyfaller.shadowPropertyBlock);
  }

  private void PackVehicle()
  {
    if (((Verse.Thing) this.vehicle).Spawned)
      ((Entity) this.vehicle).DeSpawn((DestroyMode) 0);
    ((ThingOwner) this.innerContainer).TryAddOrTransfer((Verse.Thing) this.vehicle, true);
    this.Map.GetDetachedMapComponent<VehiclePositionManager>().ClaimPosition(this.vehicle, this.Position, this.Rotation);
    Find.GameEnder.CheckOrUpdateGameOver();
  }

  public virtual void SpawnSetup(Map map, bool respawningAfterLoad)
  {
    base.SpawnSetup(map, respawningAfterLoad);
    this.launchProtocolDrawPos = this.RootPos;
    if (WorldPawnsUtility.IsWorldPawn((Pawn) this.vehicle))
    {
      Find.WorldPawns.RemovePawn((Pawn) this.vehicle);
      foreach (Pawn pawn in this.vehicle.AllPawnsAboard)
      {
        if (WorldPawnsUtility.IsWorldPawn(pawn))
          Find.WorldPawns.RemovePawn(pawn);
      }
    }
    this.vehicle.SetSustainerTarget((ISustainerTarget) this);
    this.vehicle.ResetRenderStatus();
    this.PackVehicle();
  }

  public virtual void ExposeData()
  {
    base.ExposeData();
    Scribe_Values.Look<float>(ref this.angle, "angle", 0.0f, false);
    Scribe_Deep.Look<VehiclePawn>(ref this.vehicle, "vehicle", Array.Empty<object>());
  }

  RoofCollapseResponse IRoofCollapseAlert.Notify_OnBeforeRoofCollapse() => (RoofCollapseResponse) 0;

  void IThingHolder.GetChildHolders(List<IThingHolder> outChildren)
  {
  }

  ThingOwner IThingHolder.GetDirectlyHeldThings() => (ThingOwner) this.innerContainer;
}
