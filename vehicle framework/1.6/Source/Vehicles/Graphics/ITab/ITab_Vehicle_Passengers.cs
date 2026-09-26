// Decompiled with JetBrains decompiler
// Type: Vehicles.ITab_Vehicle_Passengers
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

public class ITab_Vehicle_Passengers : ITab
{
  public const float PawnRowHeight = 50f;
  public const float WindowWidth = 520f;
  public const float WindowHeight = 450f;
  private static List<Need> tmpNeeds = new List<Need>();
  private Vector2 scrollPosition;
  private Vector2 thoughtScrollPosition;
  private float scrollViewHeight;
  private VehicleRoleHandler editingPawnOverlayRenderer;
  private Pawn specificNeedsTabForPawn;

  public ITab_Vehicle_Passengers()
  {
    ((InspectTabBase) this).size = new Vector2(520f, 450f);
    ((InspectTabBase) this).labelKey = "VF_TabPassengers";
  }

  public VehiclePawn Vehicle => this.SelPawn as VehiclePawn;

  private float SpecificNeedsTabWidth
  {
    get
    {
      return !ThingUtility.DestroyedOrNull((Thing) this.specificNeedsTabForPawn) ? NeedsCardUtility.GetSize(this.specificNeedsTabForPawn).x : 0.0f;
    }
  }

  public virtual bool IsVisible => !this.Vehicle.beached;

  protected virtual void CloseTab()
  {
    base.CloseTab();
    CursorSettings.Reset();
  }

  protected virtual void FillTab()
  {
    this.EnsureSpecificNeedsTabForPawnValid();
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 1);
    try
    {
      Rect rect = GenUI.ContractedBy(new Rect(0.0f, 0.0f, ((InspectTabBase) this).size.x, ((InspectTabBase) this).size.y), 10f);
      Rect viewRect = new Rect(0.0f, 0.0f, ((Rect) ref rect).width - 16f, this.scrollViewHeight);
      float curY = 0.0f;
      Widgets.BeginScrollView(rect, ref this.scrollPosition, viewRect, true);
      using (new VehicleTabHelper_Passenger.DrawBlock())
      {
        VehicleTabHelper_Passenger.DrawPassengersFor(ref curY, viewRect, this.scrollPosition, this.Vehicle, ref this.specificNeedsTabForPawn);
        VehicleTabHelper_Passenger.ListPawns(ref curY, viewRect, this.scrollPosition, (IThingHolder) this.Vehicle.inventory, TaggedString.op_Implicit(Translator.Translate("VF_Caravan_Cargo")), this.Vehicle.AllInventoryPawns, ref this.specificNeedsTabForPawn);
      }
      Widgets.EndScrollView();
      if (!Mouse.IsOver(rect) && !Input.GetMouseButton(0))
      {
        CursorSettings.Reset();
        VehicleTabHelper_Passenger.Clear();
      }
      if (Event.current.type != 8)
        return;
      this.scrollViewHeight = curY + 30f;
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  protected virtual void UpdateSize()
  {
    this.EnsureSpecificNeedsTabForPawnValid();
    ((InspectTabBase) this).UpdateSize();
    ((InspectTabBase) this).size = VehicleTabHelper_Passenger.GetSize(this.Vehicle.AllPawnsAboard.Concat<Pawn>((IEnumerable<Pawn>) this.Vehicle.AllInventoryPawns), ((InspectTabBase) this).PaneTopY);
    ((InspectTabBase) this).size.y = Mathf.Max(((InspectTabBase) this).size.y, NeedsCardUtility.FullSize.y);
  }

  protected virtual void ExtraOnGUI()
  {
    this.EnsureSpecificNeedsTabForPawnValid();
    ((InspectTabBase) this).ExtraOnGUI();
    if (this.specificNeedsTabForPawn != null)
    {
      Rect tabRect = ((InspectTabBase) this).TabRect;
      float specificNeedsTabWidth = this.SpecificNeedsTabWidth;
      Rect rect = new Rect(((Rect) ref tabRect).xMax - 1f, ((Rect) ref tabRect).yMin, specificNeedsTabWidth, ((Rect) ref tabRect).height);
      Find.WindowStack.ImmediateWindow(1439870015, rect, (WindowLayer) 0, (Action) (() =>
      {
        if (ThingUtility.DestroyedOrNull((Thing) this.specificNeedsTabForPawn))
          return;
        NeedsCardUtility.DoNeedsMoodAndThoughts(GenUI.AtZero(rect), this.specificNeedsTabForPawn, ref this.thoughtScrollPosition);
        if (!Widgets.CloseButtonFor(GenUI.AtZero(rect)))
          return;
        this.specificNeedsTabForPawn = (Pawn) null;
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.TabClose, (Map) null);
      }), true, false, 1f, (Action) null, false);
    }
    else
    {
      if (this.editingPawnOverlayRenderer == null)
        return;
      double num = (double) ((InspectTabBase) this).size.x + 1.0;
      Rect tabRect = ((InspectTabBase) this).TabRect;
      double yMin = (double) ((Rect) ref tabRect).yMin;
      double y = (double) ((InspectTabBase) this).size.y;
      Rect pawnOverlayRect = new Rect((float) num, (float) yMin, 600f, (float) y);
      Find.WindowStack.ImmediateWindow(this.editingPawnOverlayRenderer.role.GetHashCode() ^ ((object) this.Vehicle).GetHashCode(), pawnOverlayRect, (WindowLayer) 0, (Action) (() =>
      {
        if (this.editingPawnOverlayRenderer == null || ThingUtility.DestroyedOrNull((Thing) this.editingPawnOverlayRenderer.vehicle))
          return;
        Rect rect = GenUI.ContractedBy(new Rect(0.0f, 0.0f, ((Rect) ref pawnOverlayRect).width, ((Rect) ref pawnOverlayRect).height), 5f);
        this.editingPawnOverlayRenderer.role.PawnRenderer.RenderEditor(rect);
        if (!Widgets.CloseButtonFor(rect))
          return;
        this.editingPawnOverlayRenderer = (VehicleRoleHandler) null;
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.TabClose, (Map) null);
      }), true, false, 1f, (Action) null, false);
    }
  }

  public virtual void Notify_ClearingAllMapsMemory()
  {
    ((InspectTabBase) this).Notify_ClearingAllMapsMemory();
    this.specificNeedsTabForPawn = (Pawn) null;
  }

  private void EnsureSpecificNeedsTabForPawnValid()
  {
    Pawn specificNeedsTabForPawn = this.specificNeedsTabForPawn;
    if (specificNeedsTabForPawn == null || ((Thing) specificNeedsTabForPawn).Destroyed)
      return;
    if (!this.Vehicle.AllPawnsAboard.Contains(this.specificNeedsTabForPawn) && !this.Vehicle.AllInventoryPawns.Contains(this.specificNeedsTabForPawn))
      this.specificNeedsTabForPawn = (Pawn) null;
    if (this.editingPawnOverlayRenderer == null || this.specificNeedsTabForPawn == null && ((ThingOwner) this.editingPawnOverlayRenderer.thingOwner).Count != 0)
      return;
    this.editingPawnOverlayRenderer = (VehicleRoleHandler) null;
  }
}
