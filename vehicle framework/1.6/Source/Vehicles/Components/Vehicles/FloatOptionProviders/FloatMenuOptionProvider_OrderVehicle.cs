// Decompiled with JetBrains decompiler
// Type: Vehicles.FloatMenuOptionProvider_OrderVehicle
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

public class FloatMenuOptionProvider_OrderVehicle : FloatMenuOptionProvider_Vehicle
{
  private static readonly List<VehiclePawn> MultiSelectVehicles = new List<VehiclePawn>();

  protected override bool SelectedVehicleValid(VehiclePawn vehicle, FloatMenuContext context)
  {
    return vehicle.CanMoveFinal && vehicle.ignition.Drafted;
  }

  protected virtual FloatMenuOption GetSingleOption(FloatMenuContext context)
  {
    FloatMenuOption singleOption = (FloatMenuOption) null;
    IntVec3 clickCell = context.ClickedCell;
    if (context.IsMultiselect)
    {
      using (new ClearOnDispose<VehiclePawn>((ICollection<VehiclePawn>) FloatMenuOptionProvider_OrderVehicle.MultiSelectVehicles))
      {
        foreach (Pawn validSelectedPawn in context.ValidSelectedPawns)
        {
          if (validSelectedPawn is VehiclePawn vehicle)
          {
            AcceptanceReport acceptanceReport = FloatMenuOptionProvider_OrderVehicle.VehicleCanGoto(vehicle, clickCell);
            if (((AcceptanceReport) ref acceptanceReport).Accepted)
              FloatMenuOptionProvider_OrderVehicle.MultiSelectVehicles.Add(vehicle);
          }
        }
        if (FloatMenuOptionProvider_OrderVehicle.MultiSelectVehicles.Count == 0)
          return (FloatMenuOption) null;
        singleOption = new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("GoHere")), (Action) (() => VehicleOrientationController.StartOrienting(FloatMenuOptionProvider_OrderVehicle.MultiSelectVehicles, clickCell, clickCell)), (MenuOptionPriority) 1, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0);
      }
    }
    else
    {
      VehiclePawn vehicle = context.FirstSelectedPawn as VehiclePawn;
      if (vehicle == null)
        return (FloatMenuOption) null;
      IntVec3 result;
      if (!PathingHelper.TryFindNearestStandableCell(vehicle, clickCell, out result))
        return (FloatMenuOption) null;
      foreach (ThingComp allComp in ((ThingWithComps) vehicle).AllComps)
      {
        if (allComp is VehicleComp vehicleComp)
        {
          AcceptanceReport acceptanceReport = vehicleComp.CanMove(context);
          if (!((AcceptanceReport) ref acceptanceReport).Accepted)
          {
            Messages.Message(((AcceptanceReport) ref acceptanceReport).Reason, MessageTypeDefOf.RejectInput, true);
            return (FloatMenuOption) null;
          }
        }
      }
      AcceptanceReport acceptanceReport1 = FloatMenuOptionProvider_OrderVehicle.VehicleCanGoto(vehicle, result);
      if (!((AcceptanceReport) ref acceptanceReport1).Accepted)
        return new FloatMenuOption(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_CannotMoveToCell", NamedArgument.op_Implicit(((Entity) vehicle).LabelCap))), (Action) null, (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0);
      singleOption = new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("GoHere")), (Action) (() => VehicleOrientationController.StartOrienting(vehicle, result, clickCell)), (MenuOptionPriority) 1, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0);
    }
    singleOption.isGoto = true;
    singleOption.autoTakeable = true;
    singleOption.autoTakeablePriority = 10f;
    return singleOption;
  }

  public static AcceptanceReport VehicleCanGoto(VehiclePawn vehicle, IntVec3 gotoLoc)
  {
    return !vehicle.CanReachVehicle(LocalTargetInfo.op_Implicit(gotoLoc), (PathEndMode) 1, (Danger) 3, (TraverseMode) 0) ? AcceptanceReport.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_CannotMoveToCell", NamedArgument.op_Implicit(((Entity) vehicle).LabelCap))) : AcceptanceReport.WasAccepted;
  }

  internal static void PawnGotoAction(
    IntVec3 clickCell,
    VehiclePawn vehicle,
    IntVec3 gotoLoc,
    Rot8 rot)
  {
    bool flag1;
    if (IntVec3.op_Equality(((Thing) vehicle).Position, gotoLoc))
    {
      flag1 = true;
      vehicle.FullRotation = rot;
      if (vehicle.CurJobDef == JobDefOf.Goto)
        vehicle.jobs.EndCurrentJob((JobCondition) 2, true, true);
    }
    else if (vehicle.CurJobDef == JobDefOf.Goto && IntVec3.op_Equality(((LocalTargetInfo) ref vehicle.CurJob.targetA).Cell, gotoLoc))
    {
      flag1 = true;
    }
    else
    {
      Job job = new Job(JobDefOf.Goto, LocalTargetInfo.op_Implicit(gotoLoc));
      CellRect cellRect = CellRect.WholeMap(((Thing) vehicle).Map);
      bool flag2 = ((CellRect) ref cellRect).IsOnEdge(clickCell, 3);
      if (((Thing) vehicle).Map.exitMapGrid.IsExitCell(clickCell) | vehicle.InhabitedCellsProjected(clickCell, rot).NotNullAndAny<IntVec3>((Predicate<IntVec3>) (cell => GenGrid.InBounds(cell, ((Thing) vehicle).Map) && ((Thing) vehicle).Map.exitMapGrid.IsExitCell(cell))))
        job.exitMapOnArrival = true;
      else if (((((Thing) vehicle).Map.IsPlayerHome ? 0 : (!((Thing) vehicle).Map.exitMapGrid.MapUsesExitGrid ? 1 : 0)) & (flag2 ? 1 : 0)) != 0)
      {
        FormCaravanComp component = ((WorldObject) ((Thing) vehicle).Map.Parent).GetComponent<FormCaravanComp>();
        if (component != null && MessagesRepeatAvoider.MessageShowAllowed($"MessagePlayerTriedToLeaveMapViaExitGrid-{((Thing) vehicle).Map.uniqueID}", 60f))
          Messages.Message(TaggedString.op_Implicit(component.CanFormOrReformCaravanNow ? Translator.Translate("MessagePlayerTriedToLeaveMapViaExitGrid_CanReform") : Translator.Translate("MessagePlayerTriedToLeaveMapViaExitGrid_CantReform")), LookTargets.op_Implicit((WorldObject) ((Thing) vehicle).Map.Parent), MessageTypeDefOf.RejectInput, false);
      }
      flag1 = vehicle.jobs.TryTakeOrderedJob(job, new JobTag?((JobTag) 0), false);
      if (flag1)
        vehicle.vehiclePather.SetEndRotation(rot);
    }
    if (!flag1)
      return;
    FleckMaker.Static(gotoLoc, ((Thing) vehicle).Map, FleckDefOf.FeedbackGoto, 1f);
  }
}
