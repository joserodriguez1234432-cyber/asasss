// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleIgnitionController
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
using Verse.AI;

#nullable disable
namespace Vehicles;

public class VehicleIgnitionController : IExposable
{
  private bool drafted;
  private VehiclePawn vehicle;
  private Command_Toggle draftCommand;

  public VehicleIgnitionController(VehiclePawn vehicle) => this.vehicle = vehicle;

  public bool Drafted
  {
    get => this.drafted;
    set
    {
      if (value == this.Drafted || value && !this.CanDraft())
        return;
      this.CheckForFailedPathing(value);
      if (this.vehicle.jobs != null && (value || CaravanFormingUtility.IsFormingCaravan((Pawn) this.vehicle)))
        this.vehicle.jobs.SetFormingCaravanTick(true);
      this.drafted = value;
      if (value)
      {
        this.vehicle.GetCachedComp<CompCanBeDormant>()?.WakeUp();
        this.vehicle.EventRegistry[VehicleEventDefOf.IgnitionOn].ExecuteEvents();
      }
      else
        this.vehicle.EventRegistry[VehicleEventDefOf.IgnitionOff].ExecuteEvents();
    }
  }

  private string DraftGizmoLabel
  {
    get
    {
      if (!this.Drafted)
        return this.vehicle.VehicleDef.draftLabel;
      return this.vehicle.vehiclePather.Moving ? TaggedString.op_Implicit(Translator.Translate("VF_StopVehicle")) : TaggedString.op_Implicit(Translator.Translate("VF_UndraftVehicle"));
    }
  }

  private string DraftGizmoDescription
  {
    get
    {
      if (!this.Drafted)
        return TaggedString.op_Implicit(Translator.Translate("VF_DraftVehicleDesc"));
      return this.vehicle.vehiclePather.Moving ? TaggedString.op_Implicit(Translator.Translate("VF_StopVehicleDesc")) : TaggedString.op_Implicit(Translator.Translate("VF_UndraftVehicleDesc"));
    }
  }

  private bool CanDraft()
  {
    AcceptanceReport acceptanceReport = this.vehicle.CanDraft();
    if (!((AcceptanceReport) ref acceptanceReport).Accepted)
    {
      Messages.Message(((AcceptanceReport) ref acceptanceReport).Reason, MessageTypeDefOf.RejectInput, true);
      return false;
    }
    if (((Thing) this.vehicle).Spawned)
      ((Thing) this.vehicle).Map.GetCachedMapComponent<VehicleReservationManager>().ClearReservedFor(this.vehicle);
    return true;
  }

  private void CheckForFailedPathing(bool value)
  {
    if (!value && this.vehicle.vehiclePather.curPath != null)
      this.vehicle.vehiclePather.PatherFailed();
    if (value)
      return;
    this.vehicle.jobs.ClearQueuedJobs(true);
    if (this.vehicle.jobs.curJob != null && this.vehicle.jobs.IsCurrentJobPlayerInterruptible())
      this.vehicle.jobs.EndCurrentJob((JobCondition) 16 /*0x10*/, true, true);
    this.vehicle.vehiclePather.PatherFailed();
  }

  public IEnumerable<Gizmo> GetGizmos()
  {
    VehicleIgnitionController ignitionController1 = this;
    if (ignitionController1.draftCommand == null)
    {
      VehicleIgnitionController ignitionController2 = ignitionController1;
      Command_Toggle commandToggle = new Command_Toggle();
      ((Command) commandToggle).hotKey = KeyBindingDefOf.Command_ColonistDraft;
      // ISSUE: reference to a compiler-generated method
      commandToggle.isActive = new Func<bool>(ignitionController1.\u003CGetGizmos\u003Eb__13_0);
      // ISSUE: reference to a compiler-generated method
      commandToggle.toggleAction = new Action(ignitionController1.\u003CGetGizmos\u003Eb__13_1);
      ignitionController2.draftCommand = commandToggle;
    }
    ((Gizmo) ignitionController1.draftCommand).Disabled = false;
    ((Command) ignitionController1.draftCommand).defaultLabel = ignitionController1.DraftGizmoLabel;
    ((Command) ignitionController1.draftCommand).defaultDesc = ignitionController1.DraftGizmoDescription;
    ((Command) ignitionController1.draftCommand).icon = !ignitionController1.Drafted || !ignitionController1.vehicle.vehiclePather.Moving ? (Texture) VehicleTex.DraftVehicle : (Texture) VehicleTex.HaltVehicle;
    if (!ignitionController1.Drafted)
    {
      ((Command) ignitionController1.draftCommand).defaultLabel = ignitionController1.vehicle.VehicleDef.draftLabel;
      AcceptanceReport acceptanceReport = ignitionController1.vehicle.CanDraft();
      if (!((AcceptanceReport) ref acceptanceReport).Accepted)
        ((Gizmo) ignitionController1.draftCommand).Disable(((AcceptanceReport) ref acceptanceReport).Reason);
      if (!ignitionController1.vehicle.CanMove)
        ((Gizmo) ignitionController1.draftCommand).Disable(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_VehicleUnableToMove", NamedArgument.op_Implicit((Thing) ignitionController1.vehicle))));
    }
    ((Command) ignitionController1.draftCommand).tutorTag = ignitionController1.Drafted ? "Undraft" : "Draft";
    yield return (Gizmo) ignitionController1.draftCommand;
  }

  void IExposable.ExposeData()
  {
    Scribe_Values.Look<bool>(ref this.drafted, "drafted", false, false);
    Scribe_References.Look<VehiclePawn>(ref this.vehicle, "vehicle", false);
  }
}
