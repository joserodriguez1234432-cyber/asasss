// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleTabHelper_Passenger
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

[PublicAPI]
public static class VehicleTabHelper_Passenger
{
  private const float PawnRowHeight = 50f;
  private const float PawnRowPadding = 4f;
  private const float ThingIconSize = 27f;
  public const float PawnExtraButtonSize = 24f;
  private const float LabelWidth = 100f;
  private static readonly List<Need> TmpNeeds = new List<Need>();
  private static Pawn draggedPawn;
  private static IThingHolder transferToHolder;
  private static Pawn hoveringOverPawn;
  private static bool overDropSpot;
  private static bool pawnSeatChanged;
  private static bool drawing;
  private static VehicleTabHelper_Passenger.MouseState mouseState;
  private static VehicleTabHelper_Passenger.MouseState desiredMouseState;

  public static bool PawnSeatChanged => VehicleTabHelper_Passenger.pawnSeatChanged;

  public static void Start()
  {
    if (VehicleTabHelper_Passenger.drawing)
    {
      Trace.Fail("VehicleTabHelper_Passenger is not re-entrant and can only be called after the previous draw cycle has finished.");
    }
    else
    {
      VehicleTabHelper_Passenger.drawing = true;
      VehicleTabHelper_Passenger.overDropSpot = false;
      VehicleTabHelper_Passenger.pawnSeatChanged = false;
      VehicleTabHelper_Passenger.desiredMouseState = VehicleTabHelper_Passenger.MouseState.None;
    }
  }

  public static void End()
  {
    VehicleTabHelper_Passenger.HandleDragEvent();
    if (!VehicleTabHelper_Passenger.overDropSpot)
      VehicleTabHelper_Passenger.transferToHolder = (IThingHolder) null;
    if (VehicleTabHelper_Passenger.desiredMouseState != VehicleTabHelper_Passenger.mouseState)
    {
      VehicleTabHelper_Passenger.mouseState = VehicleTabHelper_Passenger.desiredMouseState;
      switch (VehicleTabHelper_Passenger.mouseState)
      {
        case VehicleTabHelper_Passenger.MouseState.None:
          CursorSettings.Reset();
          break;
        case VehicleTabHelper_Passenger.MouseState.HoverOver:
          CursorSettings.SetCursor(CursorSettings.Type.OpenHand);
          break;
        case VehicleTabHelper_Passenger.MouseState.Dragging:
          CursorSettings.SetCursor(CursorSettings.Type.CloseHand);
          break;
      }
    }
    VehicleTabHelper_Passenger.drawing = false;
  }

  public static void Clear() => VehicleTabHelper_Passenger.draggedPawn = (Pawn) null;

  public static void DrawPassengersFor(
    ref float curY,
    Rect viewRect,
    Vector2 scrollPos,
    VehiclePawn vehicle,
    ref Pawn moreDetailsForPawn)
  {
    foreach (VehicleRoleHandler handler in vehicle.handlers)
    {
      List<Pawn> innerListForReading = handler.thingOwner.InnerListForReading;
      VehicleTabHelper_Passenger.overDropSpot |= VehicleTabHelper_Passenger.ListPawns(ref curY, viewRect, scrollPos, (IThingHolder) handler, handler.role.label, innerListForReading, ref moreDetailsForPawn);
    }
  }

  public static bool ListPawns(
    ref float curY,
    Rect viewRect,
    Vector2 scrollPos,
    IThingHolder holder,
    string label,
    List<Pawn> pawns,
    ref Pawn moreDetailsForPawn)
  {
    bool flag = false;
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(0.0f, curY, ((Rect) ref viewRect).width - 48f, (float) (25.0 + 50.0 * (double) pawns.Count));
    if (VehicleTabHelper_Passenger.draggedPawn != null && Mouse.IsOver(rect) && ((Thing) VehicleTabHelper_Passenger.draggedPawn).ParentHolder != holder)
    {
      VehicleTabHelper_Passenger.transferToHolder = holder;
      flag = true;
      Widgets.DrawHighlight(rect);
    }
    Widgets.ListSeparator(ref curY, ((Rect) ref viewRect).width, label);
    foreach (Pawn pawn in pawns)
    {
      if (VehicleTabHelper_Passenger.DoRow(curY, viewRect, scrollPos, pawn, ref moreDetailsForPawn, VehicleTabHelper_Passenger.draggedPawn == null))
        VehicleTabHelper_Passenger.hoveringOverPawn = pawn;
      curY += 50f;
    }
    return flag;
  }

  public static bool DoRow(
    float curY,
    Rect viewRect,
    Vector2 scrollPos,
    Pawn pawn,
    ref Pawn moreDetailsForPawn,
    bool highlight)
  {
    float num1 = scrollPos.y - 50f;
    float num2 = scrollPos.y + 450f;
    bool flag1 = pawn == VehicleTabHelper_Passenger.draggedPawn;
    if (!flag1 && ((double) curY <= (double) num1 || (double) curY >= (double) num2))
      return false;
    float num3 = flag1 ? Event.current.mousePosition.y - 25f : curY;
    float num4 = flag1 ? Event.current.mousePosition.x - 63.5f : 0.0f;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(num4, num3, ((Rect) ref viewRect).width, 50f);
    Widgets.BeginGroup(rect1);
    Rect rowRect = GenUI.AtZero(rect1);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(0.0f, 0.0f, 131f, 50f);
    bool flag2 = Mouse.IsOver(rect2);
    if (VehicleTabHelper_Passenger.draggedPawn == null & flag2)
    {
      VehicleTabHelper_Passenger.desiredMouseState = Ext_Enum.Max<VehicleTabHelper_Passenger.MouseState>(VehicleTabHelper_Passenger.desiredMouseState, VehicleTabHelper_Passenger.MouseState.HoverOver);
      if (Event.current.type == null && Event.current.button == 0)
      {
        VehicleTabHelper_Passenger.draggedPawn = pawn;
        Event.current.Use();
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
      }
    }
    if (VehicleTabHelper_Passenger.draggedPawn != null)
      VehicleTabHelper_Passenger.desiredMouseState = Ext_Enum.Max<VehicleTabHelper_Passenger.MouseState>(VehicleTabHelper_Passenger.desiredMouseState, VehicleTabHelper_Passenger.MouseState.Dragging);
    Widgets.InfoCardButton(((Rect) ref rowRect).width - 24f, (float) (((double) ((Rect) ref rect1).height - 24.0) / 2.0), (Thing) pawn);
    ref Rect local1 = ref rowRect;
    ((Rect) ref local1).width = ((Rect) ref local1).width - 24f;
    if (!pawn.Dead)
    {
      VehicleTabHelper_Passenger.OpenSpecificTabButton(rowRect, pawn, ref moreDetailsForPawn);
      ref Rect local2 = ref rowRect;
      ((Rect) ref local2).width = ((Rect) ref local2).width - 24f;
    }
    if (highlight)
      Widgets.DrawHighlightIfMouseover(rect2);
    Rect rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector(4f, (float) (((double) ((Rect) ref rect1).height - 27.0) / 2.0), 27f, 27f);
    Widgets.ThingIcon(rect3, (Thing) pawn, 1f, new Rot4?(), false, 1f, false);
    Rect rect4;
    // ISSUE: explicit constructor call
    ((Rect) ref rect4).\u002Ector(((Rect) ref rect3).xMax + 4f, 16f, 100f, 18f);
    GenMapUI.DrawPawnLabel(pawn, rect4, 1f, 100f, (Dictionary<string, string>) null, (GameFont) 1, false, false);
    using (new ClearOnDispose<Need>((ICollection<Need>) VehicleTabHelper_Passenger.TmpNeeds))
    {
      foreach (Need allNeed in pawn.needs.AllNeeds)
      {
        if (allNeed.def.showForCaravanMembers)
          VehicleTabHelper_Passenger.TmpNeeds.Add(allNeed);
      }
      PawnNeedsUIUtility.SortInDisplayOrder(VehicleTabHelper_Passenger.TmpNeeds);
      float xMax = ((Rect) ref rect4).xMax;
      foreach (Need tmpNeed in VehicleTabHelper_Passenger.TmpNeeds)
      {
        Rect rect5;
        // ISSUE: explicit constructor call
        ((Rect) ref rect5).\u002Ector(xMax, 0.0f, 100f, 50f);
        tmpNeed.DrawOnGUI(rect5, int.MaxValue, 10f, false, true, new Rect?(), true);
        xMax = ((Rect) ref rect5).xMax;
      }
      if (pawn.Downed)
      {
        TextBlock textBlock;
        // ISSUE: explicit constructor call
        ((TextBlock) ref textBlock).\u002Ector(new Color(1f, 0.0f, 0.0f, 0.5f));
        try
        {
          Widgets.DrawLineHorizontal(0.0f, ((Rect) ref rect1).height / 2f, ((Rect) ref rect1).width);
        }
        finally
        {
          textBlock.Dispose();
        }
      }
      Widgets.EndGroup();
      return flag2 && !flag1;
    }
  }

  private static void OpenSpecificTabButton(Rect rowRect, Pawn pawn, ref Pawn moreDetailsForPawn)
  {
    Color color1 = pawn != moreDetailsForPawn ? Color.white : Color.green;
    Color color2 = pawn != moreDetailsForPawn ? GenUI.MouseoverColor : new Color(0.0f, 0.5f, 0.0f);
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(((Rect) ref rowRect).width - 24f, (float) (((double) ((Rect) ref rowRect).height - 24.0) / 2.0), 24f, 24f);
    if (Widgets.ButtonImage(rect, CaravanThingsTabUtility.SpecificTabButtonTex, color1, color2, true, (string) null))
    {
      if (pawn == moreDetailsForPawn)
      {
        moreDetailsForPawn = (Pawn) null;
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.TabClose, (Map) null);
      }
      else
      {
        moreDetailsForPawn = pawn;
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.TabOpen, (Map) null);
      }
    }
    TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(Translator.Translate("OpenSpecificTabButtonTip")));
  }

  public static void HandleDragEvent()
  {
    if (VehicleTabHelper_Passenger.draggedPawn == null || Event.current.type != 1 || Event.current.button != 0)
      return;
    if (VehicleTabHelper_Passenger.transferToHolder == null)
    {
      VehicleTabHelper_Passenger.Clear();
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.ClickReject, (Map) null);
    }
    else
    {
      try
      {
        if (VehicleTabHelper_Passenger.transferToHolder is VehicleRoleHandler transferToHolder && !transferToHolder.AreSlotsAvailable)
        {
          if (VehicleTabHelper_Passenger.hoveringOverPawn != null && ((Thing) VehicleTabHelper_Passenger.draggedPawn).ParentHolder is VehicleRoleHandler parentHolder1 && parentHolder1 != VehicleTabHelper_Passenger.transferToHolder && transferToHolder.CanOperateRole(VehicleTabHelper_Passenger.draggedPawn) && parentHolder1.CanOperateRole(VehicleTabHelper_Passenger.hoveringOverPawn))
          {
            parentHolder1.thingOwner.Swap<Pawn>(transferToHolder.thingOwner, VehicleTabHelper_Passenger.draggedPawn, VehicleTabHelper_Passenger.hoveringOverPawn);
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
            transferToHolder.vehicle.EventRegistry[VehicleEventDefOf.PawnChangedSeats].ExecuteEvents();
          }
          else
            Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_HandlerNotEnoughRoom", NamedArgument.op_Implicit((Thing) VehicleTabHelper_Passenger.draggedPawn), NamedArgument.op_Implicit(transferToHolder.role.label))), MessageTypeDefOf.RejectInput, true);
        }
        else
        {
          if (((Thing) VehicleTabHelper_Passenger.draggedPawn).ParentHolder == VehicleTabHelper_Passenger.transferToHolder)
            return;
          switch (VehicleTabHelper_Passenger.transferToHolder)
          {
            case VehicleRoleHandler vehicleRoleHandler:
              if (!vehicleRoleHandler.CanOperateRole(VehicleTabHelper_Passenger.draggedPawn))
              {
                bool flag = (vehicleRoleHandler.role.HandlingTypes & HandlingType.Movement) == HandlingType.None;
                MessageTypeDef messageTypeDef = flag ? MessageTypeDefOf.CautionInput : MessageTypeDefOf.RejectInput;
                Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_IncapableStatusForRole", NamedArgument.op_Implicit(((Entity) VehicleTabHelper_Passenger.draggedPawn).LabelShortCap))), messageTypeDef, true);
                if (!flag)
                  return;
                break;
              }
              break;
            case Pawn_InventoryTracker _:
              if (!VehicleTabHelper_Passenger.draggedPawn.CanBeTransferredToVehiclesCargo())
              {
                Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_CannotAddToCargo", NamedArgument.op_Implicit(((Entity) VehicleTabHelper_Passenger.draggedPawn).LabelShortCap))), MessageTypeDefOf.RejectInput, true);
                return;
              }
              break;
          }
          IThingHolder parentHolder2 = ((Thing) VehicleTabHelper_Passenger.draggedPawn).ParentHolder;
          if (!VehicleTabHelper_Passenger.transferToHolder.GetDirectlyHeldThings().TryAddOrTransfer((Thing) VehicleTabHelper_Passenger.draggedPawn, false))
          {
            Log.Warning($"Unable to add {VehicleTabHelper_Passenger.draggedPawn} to {VehicleTabHelper_Passenger.transferToHolder}.");
          }
          else
          {
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
            VehicleTabHelper_Passenger.OnPawnChangedSeats(parentHolder2);
            VehicleTabHelper_Passenger.draggedPawn.GetVehicleCaravan()?.RecacheVehicles();
          }
        }
      }
      finally
      {
        VehicleTabHelper_Passenger.draggedPawn = (Pawn) null;
      }
    }
  }

  private static void OnPawnChangedSeats(IThingHolder previousHolder)
  {
    VehiclePawn vehicle1 = GetVehicle(previousHolder);
    VehiclePawn vehicle2 = GetVehicle(VehicleTabHelper_Passenger.transferToHolder);
    VehicleTabHelper_Passenger.pawnSeatChanged = true;
    if (vehicle1 != null && vehicle2 == null)
    {
      if (!((Thing) VehicleTabHelper_Passenger.draggedPawn).Spawned && !WorldPawnsUtility.IsWorldPawn(VehicleTabHelper_Passenger.draggedPawn))
        Find.WorldPawns.PassToWorld(VehicleTabHelper_Passenger.draggedPawn, (PawnDiscardDecideMode) 0);
      vehicle1.EventRegistry[VehicleEventDefOf.PawnExited].ExecuteEvents();
    }
    if (vehicle2 == null)
      return;
    if (vehicle1 == vehicle2)
    {
      vehicle2.EventRegistry[VehicleEventDefOf.PawnChangedSeats].ExecuteEvents();
    }
    else
    {
      if (!((Thing) VehicleTabHelper_Passenger.draggedPawn).Spawned && WorldPawnsUtility.IsWorldPawn(VehicleTabHelper_Passenger.draggedPawn))
        Find.WorldPawns.RemovePawn(VehicleTabHelper_Passenger.draggedPawn);
      vehicle1?.EventRegistry[VehicleEventDefOf.PawnExited].ExecuteEvents();
      vehicle2.EventRegistry[VehicleEventDefOf.PawnEntered].ExecuteEvents();
    }

    static VehiclePawn GetVehicle(IThingHolder holder)
    {
      VehiclePawn vehicle;
      switch (holder)
      {
        case VehicleRoleHandler vehicleRoleHandler:
          vehicle = vehicleRoleHandler.vehicle;
          break;
        case Pawn_InventoryTracker inventoryTracker:
          vehicle = inventoryTracker.pawn as VehiclePawn;
          break;
        default:
          vehicle = (VehiclePawn) null;
          break;
      }
      return vehicle;
    }
  }

  public static Vector2 GetSize(IEnumerable<Pawn> pawns, float paneTopY, bool doNeeds = true)
  {
    float num1 = 100f;
    if (doNeeds)
      num1 += (float) VehicleTabHelper_Passenger.MaxNeedsCount(pawns) * 100f;
    float num2 = num1 + 24f;
    Vector2 size;
    size.x = (float) (100.0 + (double) num2 + 16.0 + 3.0);
    size.y = Mathf.Min(450f, paneTopY - 30f);
    return size;
  }

  private static int MaxNeedsCount(IEnumerable<Pawn> pawns)
  {
    int num = 0;
    List<Need> needList = new List<Need>();
    foreach (Pawn pawn in pawns)
    {
      if (pawn.needs != null)
      {
        foreach (Need allNeed in pawn.needs.AllNeeds)
        {
          if (allNeed.def.showForCaravanMembers)
            needList.Add(allNeed);
        }
        num = Mathf.Max(num, needList.Count);
        needList.Clear();
      }
    }
    return num;
  }

  private enum MouseState
  {
    None,
    HoverOver,
    Dragging,
  }

  [StructLayout(LayoutKind.Sequential, Size = 1)]
  public readonly struct DrawBlock : IDisposable
  {
    public DrawBlock() => VehicleTabHelper_Passenger.Start();

    void IDisposable.Dispose() => VehicleTabHelper_Passenger.End();
  }
}
