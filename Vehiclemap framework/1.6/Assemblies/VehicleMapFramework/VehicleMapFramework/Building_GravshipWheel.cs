// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Building_GravshipWheel
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public class Building_GravshipWheel : Building, IAttackTarget, ILoadReferenceable
{
  private bool flipped;
  private Rot4? tmpRot;
  private static readonly AccessTools.FieldRef<Thing, Rot4> rotationInt = (AccessTools.FieldRef<Thing, Rot4>) AccessTools.FieldRefAccess<Rot4>(typeof (Thing), nameof (rotationInt));

  public bool CacheMode { get; private set; }

  public Thing Thing => (Thing) this;

  public LocalTargetInfo TargetCurrentlyAimingAt => LocalTargetInfo.Invalid;

  public float TargetPriorityFactor => 0.2f;

  public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
  {
    return !((Thing) this).IsOnVehicleMapOf(out VehiclePawnWithMap _) || !this.ValidFor(Rot4.North);
  }

  public virtual Vector3 DrawPos
  {
    get
    {
      if (!VehicleSectionLayerManager.CacheMode)
        return ((Thing) this).DrawPos;
      this.CacheMode = true;
      Vector3 drawPos = ((Thing) this).DrawPos;
      this.CacheMode = false;
      if (!this.tmpRot.HasValue || !this.ValidFor(Rot4.North))
        return drawPos;
      Vector3 vector3_1 = Vector3.op_Addition(drawPos, Vector3Utility.RotatedBy(new Vector3(0.0f, 0.0f, -0.2f), VehicleSectionLayerManager.RotForPrintCounter));
      Vector3 vector3_2;
      ((Vector3) ref vector3_2).\u002Ector(((Thing) this).DrawSize.x * 0.07f, 0.0f, 0.0f);
      Rot4 rot4 = VehicleSectionLayerManager.RotForPrint;
      if (!((Rot4) ref rot4).IsVertical)
      {
        Rot4? tmpRot = this.tmpRot;
        rot4 = VehicleSectionLayerManager.RotForPrint;
        if ((tmpRot.HasValue ? (Rot4.op_Equality(tmpRot.GetValueOrDefault(), rot4) ? 1 : 0) : 0) == 0)
          goto label_7;
      }
      vector3_2.y -= 9.146342f;
label_7:
      Rot4? tmpRot1 = this.tmpRot;
      rot4 = Rot4.West;
      if ((tmpRot1.HasValue ? (Rot4.op_Equality(tmpRot1.GetValueOrDefault(), rot4) ? 1 : 0) : 0) != 0)
        vector3_2.x = -vector3_2.x;
      return Vector3.op_Addition(vector3_1, vector3_2);
    }
  }

  public CompGravshipFacility CompGravshipFacility
  {
    get
    {
      if (this.\u003CCompGravshipFacility\u003Ek__BackingField == null)
        this.\u003CCompGravshipFacility\u003Ek__BackingField = ((ThingWithComps) this).GetComp<CompGravshipFacility>();
      return this.\u003CCompGravshipFacility\u003Ek__BackingField;
    }
  }

  public virtual IEnumerable<Gizmo> GetGizmos()
  {
    Building_GravshipWheel buildingGravshipWheel = this;
    // ISSUE: reference to a compiler-generated method
    foreach (Gizmo gizmo in buildingGravshipWheel.\u003C\u003En__0())
      yield return gizmo;
    VehiclePawnWithMap vehicle;
    if (!((Thing) buildingGravshipWheel).IsOnVehicleMapOf(out vehicle))
    {
      Command_Toggle gizmo = new Command_Toggle();
      ((Command) gizmo).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VMF_VehicleMode"));
      ((Command) gizmo).icon = (Texture) ContentFinder<Texture2D>.Get("VehicleMapFramework/UI/GravshipVehicleMode", true);
      gizmo.toggleAction = new Action(buildingGravshipWheel.GenerateGravshipVehicle);
      gizmo.isActive = (Func<bool>) (() => false);
      yield return (Gizmo) gizmo;
    }
    else if (((Thing) vehicle).Spawned && ((Def) ((Thing) vehicle).def).HasModExtension<VehicleMapProps_Gravship>())
    {
      Command_Toggle gizmo = new Command_Toggle();
      ((Command) gizmo).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VMF_VehicleMode"));
      ((Command) gizmo).icon = (Texture) ContentFinder<Texture2D>.Get("VehicleMapFramework/UI/GravshipVehicleMode", true);
      gizmo.toggleAction = (Action) (() => this.PlaceGravship(vehicle));
      gizmo.isActive = (Func<bool>) (() => true);
      yield return (Gizmo) gizmo;
    }
    if (vehicle == null || !buildingGravshipWheel.ValidFor(Rot4.North))
    {
      Designator_Build allowedDesignator = BuildCopyCommandUtility.FindAllowedDesignator((BuildableDef) ((Thing) buildingGravshipWheel).def, true);
      Command_FlipBuilding gizmo = new Command_FlipBuilding();
      ((Command) gizmo).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VMF_Flip"));
      gizmo.action = (Action) (() =>
      {
        this.flipped = !this.flipped;
        ((Thing) this).DirtyMapMesh(((Thing) this).Map);
      });
      ((Command) gizmo).icon = allowedDesignator?.ResolvedIcon(((Thing) buildingGravshipWheel).StyleDef);
      ((Command) gizmo).iconProportions = allowedDesignator != null ? ((Command) allowedDesignator).iconProportions : new Vector2();
      ((Command) gizmo).iconDrawScale = allowedDesignator != null ? ((Command) allowedDesignator).iconDrawScale : 0.0f;
      ((Command) gizmo).iconTexCoords = allowedDesignator != null ? ((Command) allowedDesignator).iconTexCoords : new Rect();
      ((Command) gizmo).iconAngle = allowedDesignator != null ? ((Command) allowedDesignator).iconAngle : 0.0f;
      ((Command) gizmo).iconOffset = allowedDesignator != null ? ((Command) allowedDesignator).iconOffset : new Vector2();
      gizmo.commandIcon = ContentFinder<Texture2D>.Get("VehicleMapFramework/UI/FlipIcon", true);
      yield return (Gizmo) gizmo;
    }
  }

  public void GenerateGravshipVehicle()
  {
    AcceptanceReport gravshipVehicle = GravshipVehicleUtility.GenerateGravshipVehicle(this.CompGravshipFacility?.engine, VMF_DefOf.VMF_GravshipVehicleBase);
    if (((AcceptanceReport) ref gravshipVehicle).Accepted)
      return;
    Messages.Message(((AcceptanceReport) ref gravshipVehicle).Reason, MessageTypeDefOf.RejectInput, false);
  }

  public void PlaceGravship(VehiclePawnWithMap vehicle)
  {
    AcceptanceReport acceptanceReport = GravshipVehicleUtility.PlaceGravshipVehicle(this.CompGravshipFacility?.engine, vehicle);
    if (((AcceptanceReport) ref acceptanceReport).Accepted)
      return;
    Messages.Message(((AcceptanceReport) ref acceptanceReport).Reason, MessageTypeDefOf.RejectInput, false);
  }

  public virtual void DeSpawn(DestroyMode mode = 0)
  {
    Building_GravEngine engine = this.CompGravshipFacility?.engine;
    VehiclePawnWithMap vehicle;
    int num = !((Thing) this).IsOnVehicleMapOf(out vehicle) ? 0 : (((Thing) vehicle).Spawned ? 1 : 0);
    base.DeSpawn(mode);
    if (num == 0 || engine == null || GravshipVehicleUtility.GravshipProcessInProgress)
      return;
    AcceptanceReport acceptanceReport1;
    AcceptanceReport acceptanceReport2 = acceptanceReport1 = GravshipVehicleUtility.CheckGravshipVehicleStability(engine, Rot4.North, out CellRect _);
    if (((AcceptanceReport) ref acceptanceReport2).Accepted)
      return;
    Messages.Message(((AcceptanceReport) ref acceptanceReport1).Reason, MessageTypeDefOf.NegativeEvent, true);
    LongEventHandler.QueueLongEvent((Action) (() => GravshipVehicleUtility.PlaceGravshipVehicle(engine, vehicle, true)), TaggedString.op_Implicit(Translator.Translate("VMF_GravshipVehicleDestroyed")), false, (Action<Exception>) null, false, false, (Action) null);
  }

  public virtual void Print(SectionLayer layer)
  {
    IntVec3 position = ((Thing) this).Position;
    this.tmpRot = new Rot4?(((Thing) this).Rotation);
    if (this.flipped)
    {
      ref Rot4 local = ref Building_GravshipWheel.rotationInt.Invoke((Thing) this);
      Rot4 rot4 = this.tmpRot.Value;
      Rot4 opposite = ((Rot4) ref rot4).Opposite;
      local = opposite;
      ((Thing) this).SetPositionDirect(IntVec3.op_Addition(position, IntVec3Utility.RotatedBy(new IntVec3(1 - ((BuildableDef) ((Thing) this).def).Size.x % 2, 0, 1 - ((BuildableDef) ((Thing) this).def).Size.z % 2), this.tmpRot.Value)));
    }
    ((ThingWithComps) this).Print(layer);
    if (this.flipped)
    {
      Building_GravshipWheel.rotationInt.Invoke((Thing) this) = this.tmpRot.Value;
      ((Thing) this).SetPositionDirect(position);
    }
    this.tmpRot = new Rot4?();
  }

  public bool ValidFor(Rot4 rot)
  {
    return Rot4.op_Equality(this.tmpRot ?? ((Thing) this).Rotation, ((Rot4) ref rot).Rotated(this.flipped ? (RotationDirection) 1 : (RotationDirection) 3));
  }

  public virtual void ExposeData()
  {
    base.ExposeData();
    Scribe_Values.Look<bool>(ref this.flipped, "flipped", false, false);
  }
}
