// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleRoleHandlerBuildable
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Rendering;
using System.Linq;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class VehicleRoleHandlerBuildable : 
  VehicleRoleHandler,
  IExposable,
  IThingHolderWithDrawnPawn,
  IThingHolder,
  IParallelRenderer
{
  private static readonly AccessTools.FieldRef<VehicleRoleHandler, string> roleKey = AccessTools.FieldRefAccess<VehicleRoleHandler, string>(nameof (roleKey));

  float IThingHolderWithDrawnPawn.HeldPawnDrawPos_Y
  {
    get
    {
      Rot8 rot8 = !(this.role is VehicleRoleBuildable role) ? this.vehicle.FullRotation : ((Thing) role.upgradeComp.parent).BaseFullRotation();
      return ((Thing) this.vehicle).DrawPos.y + Altitudes.AltitudeFor((AltitudeLayer) 17).YOffset() + this.role.PawnRenderer.LayerFor(rot8);
    }
  }

  float IThingHolderWithDrawnPawn.HeldPawnBodyAngle
  {
    get
    {
      return this.role.PawnRenderer.AngleFor(!(this.role is VehicleRoleBuildable role) ? this.vehicle.FullRotation : ((Thing) role.upgradeComp.parent).BaseFullRotation()) + this.vehicle.Transform.rotation;
    }
  }

  PawnPosture IThingHolderWithDrawnPawn.HeldPawnPosture => (PawnPosture) 7;

  void IParallelRenderer.DynamicDrawPhaseAt(
    DrawPhase phase,
    in TransformData transformData,
    bool forceDraw)
  {
    this.DynamicDrawPhaseAt(phase, in transformData, forceDraw);
  }

  public void DynamicDrawPhaseAt(DrawPhase phase, in TransformData transformData, bool forceDraw = false)
  {
    foreach (Pawn pawn in this.thingOwner)
    {
      Rot4 rot4 = this.role.PawnRenderer.RotFor(transformData.orientation);
      Vector3 vector3 = Vector3Utility.RotatedBy(this.role.PawnRenderer.DrawOffsetFor(transformData.orientation), transformData.rotation.FlipAngle(this.vehicle));
      pawn.Drawer.renderer.DynamicDrawPhaseAt(phase, Vector3.op_Addition(transformData.position, vector3), new Rot4?(rot4), true);
    }
  }

  public VehicleRoleHandlerBuildable()
  {
    if (this.thingOwner != null)
      return;
    this.thingOwner = new ThingOwner<Pawn>((IThingHolder) this, false, (LookMode) 2, true);
  }

  public VehicleRoleHandlerBuildable(VehiclePawn vehicle)
    : this()
  {
    this.uniqueID = VehicleIdManager.Instance.GetNextHandlerId();
    this.vehicle = vehicle;
  }

  public VehicleRoleHandlerBuildable(VehiclePawn vehicle, VehicleRoleBuildable role)
    : this(vehicle)
  {
    this.role = (VehicleRole) role;
    VehicleRoleHandlerBuildable.roleKey.Invoke((VehicleRoleHandler) this) = role.key;
  }

  public void ExposeData()
  {
    Scribe_Values.Look<int>(ref this.uniqueID, "uniqueID", -1, false);
    Scribe_References.Look<VehiclePawn>(ref this.vehicle, "vehicle", true);
    Scribe_Values.Look<string>(ref VehicleRoleHandlerBuildable.roleKey.Invoke((VehicleRoleHandler) this), "role", (string) null, true);
    if (Scribe.mode == 1)
    {
      ThingOwner<Pawn> thingOwner = this.thingOwner;
      Pawn pawn = this.thingOwner.InnerListForReading.FirstOrDefault<Pawn>();
      int num = pawn == null || !WorldPawnsUtility.IsWorldPawn(pawn) ? 2 : 3;
      ((ThingOwner) thingOwner).contentsLookMode = (LookMode) num;
    }
    Scribe_Deep.Look<ThingOwner<Pawn>>(ref this.thingOwner, "thingOwner", new object[1]
    {
      (object) this
    });
    if (Scribe.mode != 3)
      return;
    this.role = new VehicleRole()
    {
      key = VehicleRoleHandlerBuildable.roleKey.Invoke((VehicleRoleHandler) this) + "_INVALID",
      label = VehicleRoleHandlerBuildable.roleKey.Invoke((VehicleRoleHandler) this) + " (INVALID)"
    };
    this.role.AddUpgrade(new VehicleUpgrade.RoleUpgrade()
    {
      key = this.role.key,
      label = this.role.label,
      handlingTypes = new HandlingType?((HandlingType) 1)
    });
  }
}
