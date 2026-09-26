// Decompiled with JetBrains decompiler
// Type: Vehicles.World.Dialog_AssignSeats
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
namespace Vehicles.World;

public class Dialog_AssignSeats : Window
{
  private const float ButtonWidth = 120f;
  private const float ButtonHeight = 30f;
  private const float EntryButtonWidth = 90f;
  private const float RowHeight = 30f;
  private const float RowPadding = 5f;
  private readonly ICaravanInfo parent;
  private readonly VehiclePawn vehicle;
  private Pawn draggedPawn;
  private Vector2 draggedItemPosOffset;
  private static Vector2 dialogPawnsScrollPos;
  private static Vector2 dialogPawnsAssignedScrollPos;
  private readonly List<Pawn> removalList = new List<Pawn>();
  private readonly VehicleAssignment assigner = new VehicleAssignment();
  private readonly List<TransferableOneWay> transferablePawns = new List<TransferableOneWay>();
  private readonly List<Pawn> pawns;
  private readonly HashSet<Pawn> insideVehicle;
  private readonly TransferableOneWay vehicleTransferable;
  private readonly RoleAssignment prioritizer = new RoleAssignment();

  public Dialog_AssignSeats(
    ICaravanInfo parent,
    List<TransferableOneWay> pawns,
    TransferableOneWay vehicleTransferable)
    : base((IWindowDrawing) null)
  {
    this.parent = parent;
    this.vehicle = ((Transferable) vehicleTransferable).AnyThing as VehiclePawn;
    this.insideVehicle = this.vehicle.AllPawnsAboard.ToHashSet<Pawn>();
    this.vehicleTransferable = vehicleTransferable;
    Dialog_AssignSeats.GetTransferablePawns(pawns, this.vehicle, this.transferablePawns);
    this.pawns = this.transferablePawns.Select<TransferableOneWay, Pawn>((Func<TransferableOneWay, Pawn>) (pawn => ((Transferable) pawn).AnyThing as Pawn)).OrderBy<Pawn, Pawn>((Func<Pawn, Pawn>) (p => p), (IComparer<Pawn>) new Dialog_AssignSeats.TransferablePawnComparer()).ToList<Pawn>();
    this.absorbInputAroundWindow = true;
    this.closeOnCancel = true;
    Dialog_AssignSeats.dialogPawnsScrollPos = Vector2.zero;
    Dialog_AssignSeats.dialogPawnsAssignedScrollPos = Vector2.zero;
    foreach (VehicleRoleHandler handler in this.vehicle.handlers)
    {
      foreach (Pawn pawn in handler.thingOwner)
        this.assigner.SetAssignment(new AssignedSeat(pawn, handler));
    }
    List<AssignedSeat> assignments = CaravanHelper.assignedSeats.GetAssignments(this.vehicle);
    if (GenList.NullOrEmpty<AssignedSeat>((IList<AssignedSeat>) assignments))
      return;
    foreach (AssignedSeat assignment in assignments)
    {
      if (assignment.Vehicle == this.vehicle)
        this.assigner.SetAssignment(assignment);
    }
  }

  private List<AssignedSeat> Assignments => this.assigner.GetAssignments(this.vehicle);

  public virtual Vector2 InitialSize => new Vector2(900f, (float) UI.screenHeight / 1.85f);

  private int PreAssignedCount(VehicleRoleHandler handler)
  {
    int num = 0;
    foreach (AssignedSeat assignment in this.Assignments)
    {
      if (assignment.handler == handler)
        ++num;
    }
    return num;
  }

  public virtual void DoWindowContents(Rect rect)
  {
    this.DrawVehicleMenu(rect);
    this.DoBottomButtons(rect);
  }

  private void DrawVehicleMenu(Rect rect)
  {
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(rect);
    ((Rect) ref rect1).y = ((Rect) ref rect).y + 25f;
    ((Rect) ref rect1).height = (float) ((double) ((Rect) ref rect).height - 25.0 - 33.0);
    Rect rect2 = rect1;
    Widgets.DrawMenuSection(rect2);
    float num1 = (float) ((double) ((Rect) ref rect).width / 2.0 - 5.0);
    Rect rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y, num1, ((Rect) ref rect2).height - 15f);
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(rect3);
    ((Rect) ref rect1).x = (float) ((double) ((Rect) ref rect3).x + (double) ((Rect) ref rect3).width + 10.0);
    Rect rect4 = rect1;
    if (this.draggedPawn != null)
    {
      float num2 = Event.current.mousePosition.x - this.draggedItemPosOffset.x;
      float num3 = Event.current.mousePosition.y - this.draggedItemPosOffset.y;
      float num4 = ((Rect) ref rect3).width - 1f;
      Rect rect5;
      // ISSUE: explicit constructor call
      ((Rect) ref rect5).\u002Ector(num2, num3, num4, 30f);
      Dialog_AssignSeats.DrawPawnRow(rect5, this.draggedPawn, (string) null);
    }
    this.DrawPawns(rect3);
    UIElements.DrawLineVerticalGrey((float) ((double) ((Rect) ref rect3).x + (double) ((Rect) ref rect3).width + 5.0), ((Rect) ref rect2).y, ((Rect) ref rect2).height);
    this.DrawAssignees(rect4);
    if (Event.current.type != 1 || Event.current.button != 0)
      return;
    this.draggedPawn = (Pawn) null;
  }

  [MustUseReturnValue]
  private static bool DrawPawnRow(Rect rect, Pawn pawn, string label)
  {
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y, 30f, 30f);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).x + 30f, ((Rect) ref rect).y + 5f, ((Rect) ref rect).width - 1f, 30f);
    Widgets.Label(rect2, ((Entity) pawn).LabelCap);
    Widgets.ThingIcon(rect1, (Thing) pawn, 1f, new Rot4?(), false, 1f, false);
    Rect rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector((float) ((double) ((Rect) ref rect).x + (double) ((Rect) ref rect2).width - 100.0), ((Rect) ref rect).y + 5f, 90f, 20f);
    if (GenText.NullOrEmpty(label))
      return false;
    bool flag = Widgets.ButtonText(rect3, label, true, true, true, new TextAnchor?());
    if (flag)
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
    return flag;
  }

  private void DrawPawns(Rect rect)
  {
    Widgets.Label(rect, Translator.Translate("VF_Colonists"));
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x, (float) ((double) ((Rect) ref rect).y + 30.0 + 5.0), ((Rect) ref rect).width - 1f, 30f);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(rect1);
    ((Rect) ref rect2).x = ((Rect) ref rect).x;
    ((Rect) ref rect2).height = ((Rect) ref rect).height;
    Rect rect3 = rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(rect3);
    ((Rect) ref rect2).width = ((Rect) ref rect3).width - 17f;
    ((Rect) ref rect2).height = 35f * (float) this.pawns.Count;
    Rect rect4 = rect2;
    Widgets.BeginScrollView(rect3, ref Dialog_AssignSeats.dialogPawnsScrollPos, rect4, true);
    foreach (Pawn pawn1 in this.pawns)
    {
      Pawn pawn = pawn1;
      if (!this.assigner.IsAssigned(pawn))
      {
        Rect rect5;
        // ISSUE: explicit constructor call
        ((Rect) ref rect5).\u002Ector((float) ((double) ((Rect) ref rect1).x + (double) ((Rect) ref rect1).width - 100.0), ((Rect) ref rect1).y + 5f, 90f, 20f);
        if (Mouse.IsOver(rect1) && !Mouse.IsOver(rect5))
        {
          Widgets.DrawHighlight(rect1);
          if (Event.current.type == null && Event.current.button == 0)
          {
            this.draggedPawn = pawn;
            this.draggedItemPosOffset = Vector2.op_Subtraction(Event.current.mousePosition, Vector2.op_Addition(((Rect) ref rect1).position, new Vector2(30f, 0.0f)));
            Event.current.Use();
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
          }
        }
        if (this.draggedPawn != pawn && Dialog_AssignSeats.DrawPawnRow(rect1, pawn, TaggedString.op_Implicit(Translator.Translate("VF_AddToRole"))))
        {
          VehicleRoleHandler handler1 = GenCollection.FirstOrDefault<VehicleRoleHandler>(this.vehicle.handlers, (Predicate<VehicleRoleHandler>) (handler => handler.CanOperateRole(pawn) && MatchingHandler(handler))) ?? GenCollection.FirstOrDefault<VehicleRoleHandler>(this.vehicle.handlers, (Predicate<VehicleRoleHandler>) (handler => !handler.RequiredForMovement && MatchingHandler(handler)));
          if (handler1 != null)
          {
            bool flag = true;
            if (!handler1.CanOperateRole(pawn))
            {
              flag = (handler1.role.HandlingTypes & HandlingType.Movement) == HandlingType.None;
              MessageTypeDef messageTypeDef = flag ? MessageTypeDefOf.CautionInput : MessageTypeDefOf.RejectInput;
              Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_IncapableStatusForRole", NamedArgument.op_Implicit(((Entity) pawn).LabelShortCap))), messageTypeDef, true);
            }
            if (flag)
              this.assigner.SetAssignment(new AssignedSeat(pawn, handler1));
          }
        }
        ref Rect local = ref rect1;
        ((Rect) ref local).y = ((Rect) ref local).y + 35f;
      }
    }
    Widgets.EndScrollView();

    bool MatchingHandler(VehicleRoleHandler handler)
    {
      int num = 0;
      foreach (AssignedSeat assignment in this.Assignments)
      {
        if (assignment.handler.role == handler.role)
          ++num;
      }
      return num < handler.role.Slots;
    }
  }

  private void DrawAssignees(Rect rect)
  {
    Widgets.Label(rect, Translator.Translate("VF_Assigned"));
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x, (float) ((double) ((Rect) ref rect).y + 30.0 + 5.0), ((Rect) ref rect).width - 1f, 30f);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(rect1);
    ((Rect) ref rect2).x = ((Rect) ref rect).x;
    ((Rect) ref rect2).height = ((Rect) ref rect).height;
    Rect rect3 = rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(rect3);
    ((Rect) ref rect2).width = ((Rect) ref rect3).width - 17f;
    ((Rect) ref rect2).height = (float) ((double) (this.vehicle.handlers.Count + this.Assignments.Count) * 30.0 + (double) GenCollection.Count<VehicleRoleHandler>(this.vehicle.handlers, (Predicate<VehicleRoleHandler>) (handler => handler.AreSlotsAvailableAndReservable)) * 30.0 + 5.0);
    Rect rect4 = rect2;
    Widgets.BeginScrollView(rect3, ref Dialog_AssignSeats.dialogPawnsAssignedScrollPos, rect4, true);
    foreach (VehicleRoleHandler handler in this.vehicle.handlers)
    {
      int num1 = this.PreAssignedCount(handler);
      Color label2Color = handler.role.RequiredForCaravan ? (num1 < handler.role.SlotsToOperate ? Color.red : (num1 == handler.role.Slots ? Color.grey : Color.white)) : (num1 == handler.role.Slots ? Color.grey : Color.white);
      UIElements.LabelUnderlined(rect1, handler.role.label, $"({handler.role.Slots - num1})", Color.white, label2Color, Color.white);
      ref Rect local1 = ref rect1;
      ((Rect) ref local1).y = ((Rect) ref local1).y + 30f;
      Rect rect5;
      // ISSUE: explicit constructor call
      ((Rect) ref rect5).\u002Ector(((Rect) ref rect1).x, ((Rect) ref rect1).y, ((Rect) ref rect1).width, (float) (30.0 + 30.0 * (double) num1));
      int num2 = handler.role.Slots - num1;
      if (num2 > 0 && Mouse.IsOver(rect5) && this.draggedPawn != null && Event.current.type == 1 && Event.current.button == 0)
      {
        bool flag = true;
        if (!handler.CanOperateRole(this.draggedPawn))
        {
          flag = (handler.role.HandlingTypes & HandlingType.Movement) == HandlingType.None;
          MessageTypeDef messageTypeDef = flag ? MessageTypeDefOf.CautionInput : MessageTypeDefOf.RejectInput;
          Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_IncapableStatusForRole", NamedArgument.op_Implicit(((Entity) this.draggedPawn).LabelShortCap))), messageTypeDef, true);
        }
        if (flag)
          this.assigner.SetAssignment(new AssignedSeat(this.draggedPawn, handler));
      }
      foreach (AssignedSeat assignment in this.Assignments)
      {
        if (assignment.handler.role == handler.role)
        {
          if (Dialog_AssignSeats.DrawPawnRow(rect1, assignment.pawn, TaggedString.op_Implicit(this.insideVehicle.Contains(assignment.pawn) ? TaggedString.op_Implicit((string) null) : Translator.Translate("VF_RemoveFromRole"))))
            this.removalList.Add(assignment.pawn);
          ref Rect local2 = ref rect1;
          ((Rect) ref local2).y = ((Rect) ref local2).y + 30f;
        }
      }
      foreach (Pawn removal in this.removalList)
        this.assigner.RemoveAssignment(removal);
      this.removalList.Clear();
      if (num2 > 0)
      {
        if (this.draggedPawn != null)
        {
          if (Mouse.IsOver(rect1))
            Widgets.DrawHighlight(rect1);
          else
            Widgets.DrawHighlight(rect1, 0.5f);
        }
        ref Rect local3 = ref rect1;
        ((Rect) ref local3).y = ((Rect) ref local3).y + 30f;
      }
    }
    Widgets.EndScrollView();
  }

  private void DoBottomButtons(Rect rect)
  {
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).yMax - 30f, 120f, 30f);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect).xMax - 120f, ((Rect) ref rect).yMax - 30f, 120f, 30f);
    if (Widgets.ButtonText(rect1, TaggedString.op_Implicit(Translator.Translate("CancelButton")), true, true, true, new TextAnchor?()))
      this.Close(true);
    Rect rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector(((Rect) ref rect).center.x, ((Rect) ref rect).height - 30f, 120f, 30f);
    if (Widgets.ButtonText(rect3, TaggedString.op_Implicit(Translator.Translate("VF_AutoAssign")), true, true, true, new TextAnchor?()))
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
      this.AutoAssign();
    }
    ref Rect local = ref rect3;
    ((Rect) ref local).x = ((Rect) ref local).x - 120f;
    if (Widgets.ButtonText(rect3, TaggedString.op_Implicit(Translator.Translate("VF_ClearSeats")), true, true, true, new TextAnchor?()))
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
      this.assigner.RemoveAll((Predicate<Pawn>) (pawn => !this.vehicle.AllPawnsAboard.Contains(pawn)));
    }
    if (!Widgets.ButtonText(rect2, TaggedString.op_Implicit(Translator.Translate("Confirm")), true, true, true, new TextAnchor?()))
      return;
    string failReason;
    if (!this.FinalizeSeats(out failReason))
      Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_AssignFailure", NamedArgument.op_Implicit(failReason))), MessageTypeDefOf.RejectInput, true);
    else
      this.Close(true);
  }

  private void AutoAssign()
  {
    this.prioritizer.Set(this.pawns.Where<Pawn>((Func<Pawn, bool>) (pawn => !this.assigner.IsAssigned(pawn) && !this.vehicle.AllPawnsAboard.Contains(pawn))));
    foreach (VehicleRoleHandler handler in (IEnumerable<VehicleRoleHandler>) this.vehicle.handlers.OrderBy<VehicleRoleHandler, VehicleRoleHandler>((Func<VehicleRoleHandler, VehicleRoleHandler>) (handler => handler)))
    {
      int num = handler.role.SlotsToOperate - this.PreAssignedCount(handler);
      for (int index = 0; index < num; ++index)
      {
        Pawn pawn;
        if (!this.prioritizer.TryPull(handler, out pawn))
          return;
        if (pawn != null && handler.AreSlotsAvailable)
          this.assigner.SetAssignment(new AssignedSeat(pawn, handler));
      }
    }
    foreach (VehicleRoleHandler handler in (IEnumerable<VehicleRoleHandler>) this.vehicle.handlers.OrderBy<VehicleRoleHandler, VehicleRoleHandler>((Func<VehicleRoleHandler, VehicleRoleHandler>) (handler => handler)))
    {
      int num = handler.role.Slots - this.PreAssignedCount(handler);
      for (int index = 0; index < num; ++index)
      {
        Pawn pawn;
        if (!this.prioritizer.TryPull(handler, out pawn))
          return;
        if (pawn != null && handler.AreSlotsAvailable)
          this.assigner.SetAssignment(new AssignedSeat(pawn, handler));
      }
    }
  }

  private bool FinalizeSeats(out string failReason)
  {
    failReason = string.Empty;
    foreach (VehicleRoleHandler handler in this.vehicle.handlers)
    {
      if (handler.role.RequiredForCaravan && this.PreAssignedCount(handler) < handler.role.SlotsToOperate)
      {
        failReason = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_CantAssignVehicle", NamedArgument.op_Implicit(((Entity) this.vehicle).LabelCap)));
        return false;
      }
    }
    try
    {
      foreach (AssignedSeat assignment in CaravanHelper.assignedSeats.GetAssignments(this.vehicle))
      {
        AssignedSeat seat = assignment;
        ((Transferable) GenCollection.FirstOrDefault<TransferableOneWay>(this.transferablePawns, (Predicate<TransferableOneWay>) (transferable => ((Transferable) transferable).AnyThing == seat.pawn)))?.ForceTo(0);
      }
      foreach (Pawn key in this.assigner.AllAssignments.Keys)
      {
        Pawn pawn = key;
        ((Transferable) GenCollection.FirstOrDefault<TransferableOneWay>(this.transferablePawns, (Predicate<TransferableOneWay>) (transferable => ((Transferable) transferable).AnyThing == pawn)))?.ForceTo(1);
      }
      CaravanHelper.assignedSeats.SetAssignments(this.vehicle, this.Assignments);
      ((Transferable) this.vehicleTransferable).AdjustTo(this.Assignments.Count > 0 ? ((Transferable) this.vehicleTransferable).GetMaximumToTransfer() : 0);
      this.parent.NotifyTransferablesChanged();
    }
    catch (Exception ex)
    {
      Log.Error($"Failed to finalize assigning vehicle seats.\n{ex}");
      failReason = ex.Message;
      return false;
    }
    return true;
  }

  private static void GetTransferablePawns(
    List<TransferableOneWay> pawns,
    VehiclePawn vehicle,
    List<TransferableOneWay> outList)
  {
    outList.Clear();
    foreach (TransferableOneWay pawn in pawns)
    {
      if (((Transferable) pawn).AnyThing is Pawn anyThing)
      {
        if (!CaravanHelper.assignedSeats.IsAssigned(anyThing) || CaravanHelper.assignedSeats.GetAssignment(anyThing).Vehicle == vehicle)
        {
          outList.Add(pawn);
        }
        else
        {
          foreach (VehicleRoleHandler handler in vehicle.handlers)
          {
            if (((ThingOwner) handler.thingOwner).Contains((Thing) anyThing))
            {
              outList.Add(pawn);
              break;
            }
          }
        }
      }
    }
  }

  private class TransferablePawnComparer : Comparer<Pawn>
  {
    public override int Compare(Pawn lhs, Pawn rhs)
    {
      if (lhs == null && rhs != null)
        return 1;
      if (lhs != null && rhs == null)
        return -1;
      if (lhs == rhs)
        return 0;
      int num = PawnPriority(lhs).CompareTo(PawnPriority(rhs));
      if (num != 0)
        return num;
      if (lhs.Name == null)
        return 1;
      return rhs.Name == null ? -1 : string.Compare(lhs.Name.ToStringFull, rhs.Name.ToStringFull, StringComparison.OrdinalIgnoreCase);

      static int PawnPriority(Pawn pawn)
      {
        Dialog_AssignSeats.TransferablePawnComparer.SemanticOrdering semanticOrdering;
        if (pawn != null)
        {
          if (!pawn.IsColonist)
          {
            if (!pawn.IsSlaveOfColony)
            {
              if (!pawn.IsColonyMech)
              {
                if (pawn.IsAnimal)
                {
                  semanticOrdering = Dialog_AssignSeats.TransferablePawnComparer.SemanticOrdering.Animal;
                  goto label_10;
                }
              }
              else
              {
                semanticOrdering = Dialog_AssignSeats.TransferablePawnComparer.SemanticOrdering.ColonyMech;
                goto label_10;
              }
            }
            else
            {
              semanticOrdering = Dialog_AssignSeats.TransferablePawnComparer.SemanticOrdering.SlaveOfColony;
              goto label_10;
            }
          }
          else
          {
            semanticOrdering = Dialog_AssignSeats.TransferablePawnComparer.SemanticOrdering.Colonist;
            goto label_10;
          }
        }
        semanticOrdering = Dialog_AssignSeats.TransferablePawnComparer.SemanticOrdering.Undefined;
label_10:
        return (int) semanticOrdering;
      }
    }

    private enum SemanticOrdering
    {
      Colonist,
      SlaveOfColony,
      ColonyMech,
      Animal,
      Undefined,
    }
  }
}
