// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleRoleHandler
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[UsedImplicitly]
public class VehicleRoleHandler : 
  IExposable,
  ILoadReferenceable,
  IThingHolderPawnOverlayer,
  IThingHolderWithDrawnPawn,
  IThingHolder,
  IParallelRenderer,
  IComparable<VehicleRoleHandler>
{
  public const float ComfortInsideCargo = 0.15f;
  public ThingOwner<Pawn> thingOwner;
  private string roleKey;
  public VehicleRole role;
  public int uniqueID = -1;
  public VehiclePawn vehicle;

  public VehicleRoleHandler()
  {
    this.thingOwner = new ThingOwner<Pawn>((IThingHolder) this, false, (LookMode) 2, true);
  }

  public VehicleRoleHandler(VehiclePawn vehicle)
    : this()
  {
    this.uniqueID = VehicleIdManager.Instance.GetNextHandlerId();
    this.vehicle = vehicle;
  }

  public VehicleRoleHandler(VehiclePawn vehicle, VehicleRole role)
    : this(vehicle)
  {
    this.role = new VehicleRole(role);
    this.roleKey = role.key;
  }

  bool IParallelRenderer.IsDirty { get; set; }

  public IThingHolder ParentHolder => (IThingHolder) this.vehicle;

  Rot4 IThingHolderPawnOverlayer.PawnRotation
  {
    get
    {
      PawnOverlayRenderer pawnRenderer = this.role.PawnRenderer;
      return pawnRenderer == null ? Rot4.South : pawnRenderer.RotFor(this.vehicle.FullRotation);
    }
  }

  float IThingHolderWithDrawnPawn.HeldPawnDrawPos_Y
  {
    get
    {
      return ((Thing) this.vehicle).DrawPos.y + this.role.PawnRenderer.LayerFor(this.vehicle.FullRotation);
    }
  }

  float IThingHolderWithDrawnPawn.HeldPawnBodyAngle
  {
    get => this.role.PawnRenderer.AngleFor(this.vehicle.FullRotation);
  }

  PawnPosture IThingHolderWithDrawnPawn.HeldPawnPosture => (PawnPosture) 7;

  bool IThingHolderPawnOverlayer.ShowBody => this.role.PawnRenderer.showBody;

  public bool RequiredForMovement => (this.role.HandlingTypes & HandlingType.Movement) != 0;

  public bool RoleFulfilled
  {
    get
    {
      if (this.role == null || ((ThingOwner) this.thingOwner).Count < this.role.SlotsToOperate)
        return false;
      int num = 0;
      foreach (Pawn pawn in this.thingOwner)
      {
        if (this.CanOperateRole(pawn))
          ++num;
      }
      return num >= this.role.SlotsToOperate;
    }
  }

  public bool AreSlotsAvailable
  {
    get => this.role != null && ((ThingOwner) this.thingOwner).Count < this.role.Slots;
  }

  public bool AreSlotsAvailableAndReservable
  {
    get
    {
      if (((Thing) this.vehicle).Map == null)
        return this.AreSlotsAvailable;
      return this.AreSlotsAvailable && ((Thing) this.vehicle).Map.GetCachedMapComponent<VehicleReservationManager>().CanReserve<VehicleRoleHandler, VehicleHandlerReservation>(this.vehicle, (Pawn) null, this);
    }
  }

  public void DoTick() => ((ThingOwner) this.thingOwner).DoTick();

  public void DynamicDrawPhaseAt(DrawPhase phase, in TransformData transformData, bool forceDraw = false)
  {
    foreach (Pawn pawn in this.thingOwner)
    {
      Rot4 rot4 = this.role.PawnRenderer.RotFor(transformData.orientation);
      Vector3 vector3 = this.role.PawnRenderer.DrawOffsetFor(transformData.orientation);
      pawn.Drawer.renderer.DynamicDrawPhaseAt(phase, Vector3.op_Addition(transformData.position, vector3), new Rot4?(rot4), true);
    }
  }

  public bool CanOperateRole(Pawn pawn)
  {
    return VehicleRoleHandler.CanOperateRole(pawn, this.role.HandlingTypes) && ((Thing) pawn).Faction == ((Thing) this.vehicle).Faction;
  }

  public static bool CanOperateRole(Pawn pawn, HandlingType handlingType)
  {
    return handlingType == HandlingType.None || ((handlingType & HandlingType.Turret) == HandlingType.None || !pawn.WorkTagIsDisabled((WorkTags) 8)) && pawn.RaceProps.ToolUser && !pawn.Downed && !pawn.Dead && !pawn.InMentalState && !pawn.IsPrisoner && !pawn.IsColonyMech && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Consciousness);
  }

  public override string ToString() => this.roleKey;

  public string GetUniqueLoadID() => $"VehicleHandler_{this.uniqueID}";

  public void GetChildHolders(List<IThingHolder> outChildren)
  {
    ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, (IList<Thing>) this.GetDirectlyHeldThings());
  }

  public ThingOwner GetDirectlyHeldThings() => (ThingOwner) this.thingOwner;

  public void ExposeData()
  {
    Scribe_Values.Look<int>(ref this.uniqueID, "uniqueID", -1, false);
    Scribe_References.Look<VehiclePawn>(ref this.vehicle, "vehicle", true);
    Scribe_Values.Look<string>(ref this.roleKey, "role", (string) null, true);
    if (Scribe.mode == 1)
    {
      ThingOwner<Pawn> thingOwner = this.thingOwner;
      Pawn pawn = this.thingOwner.InnerListForReading.FirstOrDefault<Pawn>();
      int num = (pawn != null ? (WorldPawnsUtility.IsWorldPawn(pawn) ? 1 : 0) : 0) != 0 ? 3 : 2;
      ((ThingOwner) thingOwner).contentsLookMode = (LookMode) num;
    }
    Scribe_Deep.Look<ThingOwner<Pawn>>(ref this.thingOwner, "thingOwner", new object[1]
    {
      (object) this
    });
    if (Scribe.mode != 3)
      return;
    this.role = this.vehicle.VehicleDef.CreateRole(this.roleKey);
    if (this.role != null)
      return;
    Log.Error($"Unable to load role={this.roleKey}. Creating empty role to avoid game-breaking issues.");
    if (this.role != null)
      return;
    this.role = new VehicleRole()
    {
      key = this.roleKey + "_INVALID",
      label = this.roleKey + " (INVALID)"
    };
  }

  int IComparable<VehicleRoleHandler>.CompareTo(VehicleRoleHandler other)
  {
    if (other == null)
      return -1;
    int priority = GetPriority(this.role.HandlingTypes);
    return GetPriority(other.role.HandlingTypes).CompareTo(priority);

    static int GetPriority(HandlingType type)
    {
      int priority = 0;
      if ((type & HandlingType.Movement) != HandlingType.None)
        priority += 10;
      if ((type & HandlingType.Turret) != HandlingType.None)
        ++priority;
      return priority;
    }
  }

  void IParallelRenderer.DynamicDrawPhaseAt(
    DrawPhase phase,
    in TransformData transformData,
    bool forceDraw = false)
  {
    this.DynamicDrawPhaseAt(phase, in transformData, forceDraw);
  }
}
