// Decompiled with JetBrains decompiler
// Type: Vehicles.World.WITab_Vehicle_Manifest
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles.World;

public class WITab_Vehicle_Manifest : WITab
{
  private Vector2 scrollPosition;
  private Vector2 thoughtScrollPosition;
  private Vector2 vehicleHealthPanelSize;
  private float scrollViewHeight;
  private Pawn moreDetailsForPawn;
  private readonly List<Pawn> dismountedPawns = new List<Pawn>();
  private bool canUseCargoHold;
  private IVehicleWorldObject cachedSelObject;

  public WITab_Vehicle_Manifest()
  {
    ((InspectTabBase) this).size = new Vector2(520f, 450f);
    ((InspectTabBase) this).labelKey = "VF_TabPassengers";
  }

  public virtual bool IsVisible => true;

  public IVehicleWorldObject VehicleObject => this.SelObject as IVehicleWorldObject;

  private float MoreDetailsWidth
  {
    get
    {
      if (ThingUtility.DestroyedOrNull((Thing) this.moreDetailsForPawn))
        return 0.0f;
      return this.moreDetailsForPawn is VehiclePawn ? this.vehicleHealthPanelSize.x : NeedsCardUtility.GetSize(this.moreDetailsForPawn).x;
    }
  }

  private void RecachePawnLists()
  {
    this.cachedSelObject = this.VehicleObject;
    this.dismountedPawns.Clear();
    this.dismountedPawns.AddRange(this.VehicleObject.DismountedPawns);
    this.canUseCargoHold = this.VehicleObject.Vehicles.NotNullAndAny<VehiclePawn>(new Predicate<VehiclePawn>(HasCargoPawn)) || this.dismountedPawns.Exists(new Predicate<Pawn>(IsCargoPawn));

    static bool HasCargoPawn(VehiclePawn vehicle) => vehicle.AllInventoryPawns.Count > 0;

    static bool IsCargoPawn(Pawn pawn) => pawn.CanBeTransferredToVehiclesCargo();
  }

  public virtual void OnOpen()
  {
    ((InspectTabBase) this).OnOpen();
    this.RecachePawnLists();
  }

  protected virtual void CloseTab()
  {
    base.CloseTab();
    CursorSettings.Reset();
    this.moreDetailsForPawn = (Pawn) null;
  }

  protected virtual void FillTab()
  {
    if (this.cachedSelObject != this.VehicleObject)
      this.RecachePawnLists();
    this.EnsureSpecificNeedsTabForPawnValid();
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 1);
    try
    {
      Rect rect = GenUI.ContractedBy(new Rect(0.0f, 0.0f, ((InspectTabBase) this).size.x, ((InspectTabBase) this).size.y), 10f);
      Rect viewRect = new Rect(0.0f, 0.0f, ((Rect) ref rect).width - 16f, this.scrollViewHeight);
      Widgets.BeginScrollView(rect, ref this.scrollPosition, viewRect, true);
      float num = this.DrawTab(viewRect);
      Widgets.EndScrollView();
      if (!Mouse.IsOver(rect) && !Input.GetMouseButton(0))
      {
        CursorSettings.Reset();
        VehicleTabHelper_Passenger.Clear();
      }
      if (VehicleTabHelper_Passenger.PawnSeatChanged)
        this.RecachePawnLists();
      if (Event.current.type != 8)
        return;
      this.scrollViewHeight = num + 30f;
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  private float DrawTab(Rect viewRect)
  {
    float curY = 0.0f;
    using (new VehicleTabHelper_Passenger.DrawBlock())
    {
      foreach (VehiclePawn vehicle in this.VehicleObject.Vehicles)
      {
        Color baseColor = vehicle != this.moreDetailsForPawn ? Color.white : Color.green;
        Color mouseoverColor = vehicle != this.moreDetailsForPawn ? GenUI.MouseoverColor : new Color(0.0f, 0.5f, 0.0f);
        if (WITab_Vehicle_Manifest.SectionLabel(viewRect, ref curY, ((Entity) vehicle).Label, baseColor, mouseoverColor, CaravanThingsTabUtility.SpecificTabButtonTex))
        {
          if (vehicle == this.moreDetailsForPawn)
          {
            this.moreDetailsForPawn = (Pawn) null;
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.TabClose, (Map) null);
          }
          else
          {
            this.moreDetailsForPawn = (Pawn) vehicle;
            VehicleTabHelper_Health.Init();
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.TabOpen, (Map) null);
          }
        }
        VehicleTabHelper_Passenger.DrawPassengersFor(ref curY, viewRect, this.scrollPosition, vehicle, ref this.moreDetailsForPawn);
        if (this.canUseCargoHold)
          VehicleTabHelper_Passenger.ListPawns(ref curY, viewRect, this.scrollPosition, (IThingHolder) vehicle.inventory, TaggedString.op_Implicit(Translator.Translate("VF_Caravan_Cargo")), vehicle.AllInventoryPawns, ref this.moreDetailsForPawn);
      }
      if (this.VehicleObject.CanDismount)
      {
        WITab_Vehicle_Manifest.SectionLabel(viewRect, ref curY, TaggedString.op_Implicit(Translator.Translate("VF_Caravan_Dismounted")));
        VehicleTabHelper_Passenger.ListPawns(ref curY, viewRect, this.scrollPosition, (IThingHolder) this.VehicleObject, string.Empty, this.dismountedPawns, ref this.moreDetailsForPawn);
      }
      return curY;
    }
  }

  protected virtual void ExtraOnGUI()
  {
    this.EnsureSpecificNeedsTabForPawnValid();
    ((InspectTabBase) this).ExtraOnGUI();
    if (this.moreDetailsForPawn == null)
      return;
    Rect tabRect = ((InspectTabBase) this).TabRect;
    Rect rect = new Rect(((Rect) ref tabRect).xMax - 1f, ((Rect) ref tabRect).yMin, this.MoreDetailsWidth, ((Rect) ref tabRect).height);
    Find.WindowStack.ImmediateWindow(1439870015, rect, (WindowLayer) 0, (Action) (() =>
    {
      if (ThingUtility.DestroyedOrNull((Thing) this.moreDetailsForPawn))
        return;
      Rect rect1 = GenUI.AtZero(rect);
      this.DrawMoreDetailsWindow(rect1);
      if (!Widgets.CloseButtonFor(rect1))
        return;
      this.moreDetailsForPawn = (Pawn) null;
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.TabClose, (Map) null);
    }), true, false, 1f, (Action) null, false);
  }

  private void DrawMoreDetailsWindow(Rect rect)
  {
    if (this.moreDetailsForPawn is VehiclePawn moreDetailsForPawn)
    {
      this.vehicleHealthPanelSize = VehicleTabHelper_Health.Start(moreDetailsForPawn, height: ((Rect) ref rect).height);
      VehicleTabHelper_Health.DrawHealthPanel(moreDetailsForPawn);
      VehicleTabHelper_Health.End();
    }
    else
      NeedsCardUtility.DoNeedsMoodAndThoughts(rect, this.moreDetailsForPawn, ref this.thoughtScrollPosition);
  }

  private static bool SectionLabel(
    Rect viewRect,
    ref float curY,
    string label,
    Texture2D buttonTex = null)
  {
    return WITab_Vehicle_Manifest.SectionLabel(viewRect, ref curY, label, Color.white, GenUI.MouseoverColor, buttonTex);
  }

  private static bool SectionLabel(
    Rect viewRect,
    ref float curY,
    string label,
    Color baseColor,
    Color mouseoverColor,
    Texture2D buttonTex = null)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 2, (TextAnchor) 1);
    try
    {
      bool flag = false;
      Rect rect1 = new Rect(0.0f, curY, ((Rect) ref viewRect).width, Text.CalcSize(label).y);
      Widgets.Label(rect1, GenText.Truncate(label, ((Rect) ref viewRect).width, (Dictionary<string, string>) null));
      Rect rect2 = new Rect(((Rect) ref rect1).width - 24f, curY, 24f, 24f);
      if (Object.op_Inequality((Object) buttonTex, (Object) null) && Widgets.ButtonImageFitted(rect2, buttonTex, baseColor, mouseoverColor))
        flag = true;
      curY += ((Rect) ref rect1).height;
      return flag;
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
    Vector2 vector2;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2).\u002Ector(520f, 0.0f);
    foreach (VehiclePawn vehicle in this.VehicleObject.Vehicles)
    {
      Vector2 size = VehicleTabHelper_Passenger.GetSize(vehicle.AllPawnsAboard.Concat<Pawn>((IEnumerable<Pawn>) vehicle.AllInventoryPawns), ((InspectTabBase) this).PaneTopY);
      vector2.x = Mathf.Max(vector2.x, size.x);
      vector2.y += size.y;
    }
    Vector2 size1 = VehicleTabHelper_Passenger.GetSize(this.VehicleObject.DismountedPawns, ((InspectTabBase) this).PaneTopY);
    vector2.x = Mathf.Max(vector2.x, size1.x);
    vector2.y += size1.y;
    ((InspectTabBase) this).size.x = vector2.x;
    ((InspectTabBase) this).size.y = Mathf.Max(((InspectTabBase) this).size.y, NeedsCardUtility.FullSize.y);
  }

  private void EnsureSpecificNeedsTabForPawnValid()
  {
    Pawn moreDetailsForPawn = this.moreDetailsForPawn;
    if (moreDetailsForPawn == null || ((Thing) moreDetailsForPawn).Destroyed || moreDetailsForPawn is VehiclePawn || this.VehicleObject.DismountedPawns.Contains<Pawn>(this.moreDetailsForPawn))
      return;
    foreach (VehiclePawn vehicle in this.VehicleObject.Vehicles)
    {
      if (vehicle.AllPawnsAboard.Contains(this.moreDetailsForPawn) || vehicle.AllInventoryPawns.Contains(this.moreDetailsForPawn))
        return;
    }
    this.moreDetailsForPawn = (Pawn) null;
  }
}
