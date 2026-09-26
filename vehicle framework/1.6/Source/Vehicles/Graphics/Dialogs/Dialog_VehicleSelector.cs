// Decompiled with JetBrains decompiler
// Type: Vehicles.World.Dialog_VehicleSelector
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
using Vehicles.Rendering;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles.World;

public class Dialog_VehicleSelector : Window
{
  private const int Columns = 4;
  private const float ButtonPadding = 10f;
  private const float RowHeight = 90f;
  private static readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);
  private readonly List<VehiclePawn> availableVehicles;
  private readonly List<VehicleDef> availableVehicleDefs;
  private readonly HashSet<VehicleDef> selectedDefs = new HashSet<VehicleDef>();
  private readonly HashSet<VehiclePawn> selectedVehicles = new HashSet<VehiclePawn>();
  private Vector2 scrollPosition;
  private bool showVehicleDefs;

  public Dialog_VehicleSelector()
    : base((IWindowDrawing) null)
  {
    this.absorbInputAroundWindow = true;
    this.doCloseX = true;
    this.forcePause = true;
    this.availableVehicles = Find.Maps.Select<Map, VehiclePositionManager>((Func<Map, VehiclePositionManager>) (map => map.GetDetachedMapComponent<VehiclePositionManager>())).SelectMany<VehiclePositionManager, VehiclePawn>((Func<VehiclePositionManager, IEnumerable<VehiclePawn>>) (positionManager => positionManager.AllClaimants.Where<VehiclePawn>((Func<VehiclePawn, bool>) (vehicle => vehicle.VehicleDef.canCaravan)))).ToList<VehiclePawn>();
    this.availableVehicleDefs = DefDatabase<VehicleDef>.AllDefsListForReading.Where<VehicleDef>((Func<VehicleDef, bool>) (vehicleDef => vehicleDef.canCaravan)).ToList<VehicleDef>();
    this.RecalculateHeight();
  }

  public virtual Vector2 InitialSize
  {
    get => new Vector2((float) UI.screenWidth / 2f, (float) UI.screenHeight / 1.5f);
  }

  private VehicleType SelectedType
  {
    get
    {
      if (!this.showVehicleDefs)
      {
        VehiclePawn vehiclePawn = this.selectedVehicles.FirstOrDefault<VehiclePawn>();
        return vehiclePawn == null ? VehicleType.Universal : vehiclePawn.VehicleDef.type;
      }
      VehicleDef vehicleDef = this.selectedDefs.FirstOrDefault<VehicleDef>();
      return vehicleDef == null ? VehicleType.Universal : vehicleDef.type;
    }
  }

  private float ViewRectHeight { get; set; }

  private bool NoVehiclesSelected
  {
    get => !this.showVehicleDefs ? this.selectedVehicles.Count == 0 : this.selectedDefs.Count == 0;
  }

  private void RecalculateHeight()
  {
    this.ViewRectHeight = (float) Mathf.CeilToInt((this.showVehicleDefs ? (float) this.availableVehicleDefs.Count : (float) this.availableVehicles.Count) / 4f) * 90f;
  }

  public virtual void DoWindowContents(Rect inRect)
  {
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(0.0f, 0.0f, ((Rect) ref inRect).width, 35f);
    Text.Font = (GameFont) 2;
    Text.Anchor = (TextAnchor) 4;
    Widgets.Label(rect1, Translator.Translate("VF_SelectVehiclesForPlanner"));
    Text.Font = (GameFont) 1;
    Text.Anchor = (TextAnchor) 0;
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(rect1);
    if (UIElements.ClickableLabel(rect2, TaggedString.op_Implicit(this.showVehicleDefs ? Translator.Translate("VF_RoutePlannerToggleVehicleDefs") : Translator.Translate("VF_RoutePlannerToggleVehicles")), Color.grey, Color.white, (GameFont) 2))
    {
      this.showVehicleDefs = !this.showVehicleDefs;
      this.selectedDefs.Clear();
      this.selectedVehicles.Clear();
      this.RecalculateHeight();
    }
    ref Rect local = ref inRect;
    ((Rect) ref local).yMin = ((Rect) ref local).yMin + ((Rect) ref rect1).height;
    Widgets.DrawMenuSection(inRect);
    Rect rect3 = GenUI.ContractedBy(new Rect(inRect), 1f);
    ((Rect) ref rect3).yMax = (float) ((double) ((Rect) ref inRect).height - 10.0 - 5.0);
    this.DrawVehicleSelect(rect3);
    this.DoBottomButtons(GenUI.AtZero(inRect));
  }

  private void DrawVehicleSelect(Rect rect)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((TextAnchor) 3);
    try
    {
      Rect rect1 = new Rect(0.0f, ((Rect) ref rect).yMin, ((Rect) ref rect).width - 16f, this.ViewRectHeight);
      Widgets.BeginScrollView(rect, ref this.scrollPosition, rect1, true);
      this.DrawVehicles(rect);
      Widgets.EndScrollView();
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  private void DrawVehicles(Rect rect)
  {
    int num1 = this.showVehicleDefs ? this.availableVehicleDefs.Count : this.availableVehicles.Count;
    float num2 = (float) ((double) ((Rect) ref rect).width - 16.0 - 5.0);
    float num3 = num2 / 4f;
    float num4 = 30f;
    int index1 = 0;
    while (index1 < num1)
    {
      float num5 = 5f;
      Rect rect1;
      // ISSUE: explicit constructor call
      ((Rect) ref rect1).\u002Ector(num5, num4, num2, 90f);
      for (int index2 = 0; index2 < 4 && index1 < num1; ++index2)
      {
        Rect iconRect;
        // ISSUE: explicit constructor call
        ((Rect) ref iconRect).\u002Ector(num5, ((Rect) ref rect1).y + 5f, 90f, 90f);
        Rect rowRect;
        // ISSUE: explicit constructor call
        ((Rect) ref rowRect).\u002Ector(((Rect) ref iconRect).xMax, ((Rect) ref iconRect).y, num3 - ((Rect) ref iconRect).width, 90f);
        ref Rect local = ref rowRect;
        ((Rect) ref local).xMin = ((Rect) ref local).xMin + 5f;
        if (this.showVehicleDefs)
        {
          VehicleDef availableVehicleDef = this.availableVehicleDefs[index1];
          this.DrawVehicleDef(rowRect, iconRect, availableVehicleDef);
        }
        else
        {
          VehiclePawn availableVehicle = this.availableVehicles[index1];
          this.DrawVehicle(rowRect, iconRect, availableVehicle);
        }
        num5 = ((Rect) ref rowRect).xMax;
        ++index1;
      }
      num4 += 90f;
    }
  }

  private void DrawVehicle(Rect rowRect, Rect iconRect, VehiclePawn vehicle)
  {
    Vector2 drawSize = vehicle.VehicleDef.graphicData.drawSize;
    ref float local1 = ref drawSize.x;
    ref float local2 = ref drawSize.y;
    float y = drawSize.y;
    float x = drawSize.x;
    local1 = y;
    double num = (double) x;
    local2 = (float) num;
    VehicleGui.DrawVehicleOnGUI(iconRect, BlitRequest.For(vehicle));
    Widgets.Label(rowRect, ((Entity) vehicle).LabelShortCap);
    bool flag1 = this.selectedVehicles.Contains(vehicle);
    Vector2 vector2;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2).\u002Ector(((Rect) ref rowRect).xMax - 24f, ((Rect) ref rowRect).y + 5f);
    bool flag2 = this.SelectedType != VehicleType.Universal && vehicle.VehicleDef.type != this.SelectedType;
    Widgets.Checkbox(vector2, ref flag1, 24f, flag2, false, (Texture2D) null, (Texture2D) null);
    if (flag1 == this.selectedVehicles.Contains(vehicle))
      return;
    if (flag1)
      this.selectedVehicles.Add(vehicle);
    else
      this.selectedVehicles.Remove(vehicle);
  }

  private void DrawVehicleDef(Rect rowRect, Rect iconRect, VehicleDef vehicleDef)
  {
    Vector2 drawSize = vehicleDef.graphicData.drawSize;
    ref float local1 = ref drawSize.x;
    ref float local2 = ref drawSize.y;
    float y = drawSize.y;
    float x = drawSize.x;
    local1 = y;
    double num = (double) x;
    local2 = (float) num;
    VehicleGui.DrawVehicleOnGUI(iconRect, BlitRequest.For(vehicleDef));
    Widgets.Label(rowRect, ((Def) vehicleDef).LabelCap);
    bool flag1 = this.selectedDefs.Contains(vehicleDef);
    Vector2 vector2;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2).\u002Ector(((Rect) ref rowRect).xMax - 24f, ((Rect) ref rowRect).y + 5f);
    bool flag2 = this.SelectedType != VehicleType.Universal && vehicleDef.type != this.SelectedType;
    Widgets.Checkbox(vector2, ref flag1, 24f, flag2, false, (Texture2D) null, (Texture2D) null);
    if (flag1 == this.selectedDefs.Contains(vehicleDef))
      return;
    if (flag1)
      this.selectedDefs.Add(vehicleDef);
    else
      this.selectedDefs.Remove(vehicleDef);
  }

  private void DoBottomButtons(Rect rect)
  {
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector((float) ((double) ((Rect) ref rect).width - (double) Dialog_VehicleSelector.BottomButtonSize.x - 10.0 - 15.0), ((Rect) ref rect).height - 10f, Dialog_VehicleSelector.BottomButtonSize.x, Dialog_VehicleSelector.BottomButtonSize.y);
    if (Widgets.ButtonText(rect1, TaggedString.op_Implicit(Translator.Translate("VF_StartVehicleRoutePlanner")), true, true, true, new TextAnchor?()))
    {
      if (this.NoVehiclesSelected)
      {
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.ClickReject, (Map) null);
        return;
      }
      VehicleRoutePlanner component = Find.World.GetComponent<VehicleRoutePlanner>();
      if (this.showVehicleDefs)
      {
        component.Start(this.selectedDefs.ToList<VehicleDef>());
      }
      else
      {
        List<Pawn> list = this.selectedVehicles.Cast<Pawn>().ToList<Pawn>();
        component.Start(new VehicleCaravanInfo(list, CollectionsMassCalculator.MassUsage<Pawn>(list, (IgnorePawnsInventoryMode) 3, false, false), this.selectedVehicles.Sum<VehiclePawn>((Func<VehiclePawn, float>) (vehicle => vehicle.GetStatValue(VehicleStatDefOf.CargoCapacity))), PlanetTile.Invalid));
      }
      this.Close(true);
    }
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector((float) ((double) ((Rect) ref rect1).x - (double) Dialog_VehicleSelector.BottomButtonSize.x - 10.0), ((Rect) ref rect1).y, Dialog_VehicleSelector.BottomButtonSize.x, Dialog_VehicleSelector.BottomButtonSize.y);
    if (!Widgets.ButtonText(rect2, TaggedString.op_Implicit(Translator.Translate("CancelButton")), true, true, true, new TextAnchor?()))
      return;
    Find.World.GetComponent<VehicleRoutePlanner>().Stop();
    this.Close(true);
  }
}
