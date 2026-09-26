// Decompiled with JetBrains decompiler
// Type: Vehicles.World.WITab_AerialVehicle_Health
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles.World;

[StaticConstructorOnStartup]
public class WITab_AerialVehicle_Health : WITab_AerialVehicle
{
  private const float RowHeight = 40f;
  private const float PawnLabelHeight = 18f;
  private const float PawnLabelColumnWidth = 100f;
  private const float SpaceAroundIcon = 4f;
  private const float PawnCapacityColumnWidth = 100f;
  private const float BeCarriedIfSickColumnWidth = 40f;
  private const float IconSize = 24f;
  private static readonly List<PawnCapacityDef> CapacitiesToDisplayTmp = new List<PawnCapacityDef>();
  private Vector2 scrollPosition;
  private float scrollViewHeight;
  private Pawn specificHealthTabForPawn;
  private bool compactMode;

  public WITab_AerialVehicle_Health() => ((InspectTabBase) this).labelKey = "TabCaravanHealth";

  private static List<PawnCapacityDef> CapacitiesToDisplay
  {
    get
    {
      WITab_AerialVehicle_Health.CapacitiesToDisplayTmp.Clear();
      foreach (PawnCapacityDef pawnCapacityDef in DefDatabase<PawnCapacityDef>.AllDefsListForReading)
      {
        if (pawnCapacityDef.showOnCaravanHealthTab)
          WITab_AerialVehicle_Health.CapacitiesToDisplayTmp.Add(pawnCapacityDef);
      }
      GenCollection.SortBy<PawnCapacityDef, int>(WITab_AerialVehicle_Health.CapacitiesToDisplayTmp, new Func<PawnCapacityDef, int>(CapacityDefOrder));
      return WITab_AerialVehicle_Health.CapacitiesToDisplayTmp;

      static int CapacityDefOrder(PawnCapacityDef capacityDef) => capacityDef.listOrder;
    }
  }

  private float SpecificHealthTabWidth
  {
    get
    {
      this.EnsureSpecificHealthTabForPawnValid();
      return !ThingUtility.DestroyedOrNull((Thing) this.specificHealthTabForPawn) ? 630f : 0.0f;
    }
  }

  protected virtual void CloseTab()
  {
    base.CloseTab();
    VehicleTabHelper_Health.Clear();
  }

  protected virtual void FillTab()
  {
    this.EnsureSpecificHealthTabForPawnValid();
    Text.Font = (GameFont) 1;
    Rect scrollOutRect = GenUI.ContractedBy(new Rect(0.0f, 0.0f, ((InspectTabBase) this).size.x, ((InspectTabBase) this).size.y), 10f);
    Rect scrollViewRect;
    // ISSUE: explicit constructor call
    ((Rect) ref scrollViewRect).\u002Ector(0.0f, 0.0f, ((Rect) ref scrollOutRect).width - 16f, this.scrollViewHeight);
    float curY = 0.0f;
    Widgets.BeginScrollView(scrollOutRect, ref this.scrollPosition, scrollViewRect, true);
    this.DoColumnHeaders();
    this.DoRows(ref curY, scrollViewRect, scrollOutRect);
    if (Event.current.type == 8)
      this.scrollViewHeight = curY + 30f;
    Widgets.EndScrollView();
  }

  protected virtual void UpdateSize()
  {
    this.EnsureSpecificHealthTabForPawnValid();
    ((InspectTabBase) this).UpdateSize();
    ((InspectTabBase) this).size = this.GetRawSize(false);
    if ((double) ((InspectTabBase) this).size.x + (double) this.SpecificHealthTabWidth > (double) UI.screenWidth)
    {
      this.compactMode = true;
      ((InspectTabBase) this).size = this.GetRawSize(true);
    }
    else
      this.compactMode = false;
  }

  protected virtual void ExtraOnGUI()
  {
    this.EnsureSpecificHealthTabForPawnValid();
    ((InspectTabBase) this).ExtraOnGUI();
    Pawn localSpecificHealthTabForPawn = this.specificHealthTabForPawn;
    if (localSpecificHealthTabForPawn == null)
      return;
    Rect tabRect = ((InspectTabBase) this).TabRect;
    float specificHealthTabWidth = this.SpecificHealthTabWidth;
    Rect rect = new Rect(((Rect) ref tabRect).xMax - 1f, ((Rect) ref tabRect).yMin, specificHealthTabWidth, ((Rect) ref tabRect).height);
    Find.WindowStack.ImmediateWindow(1439870015, rect, (WindowLayer) 0, (Action) (() =>
    {
      if (ThingUtility.DestroyedOrNull((Thing) localSpecificHealthTabForPawn))
        return;
      HealthCardUtility.DrawPawnHealthCard(new Rect(Vector2.zero, ((Rect) ref rect).size), localSpecificHealthTabForPawn, false, true, (Thing) localSpecificHealthTabForPawn);
      if (!Widgets.CloseButtonFor(GenUI.AtZero(rect)))
        return;
      this.specificHealthTabForPawn = (Pawn) null;
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.TabClose, (Map) null);
    }), true, false, 1f, (Action) null, false);
  }

  private void DoColumnHeaders()
  {
    if (this.compactMode)
      return;
    float num1 = 135f;
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((TextAnchor) 1, Widgets.SeparatorLabelColor);
    try
    {
      Widgets.Label(new Rect(num1, 3f, 100f, 100f), Translator.Translate("Pain"));
      float num2 = num1 + 100f;
      foreach (PawnCapacityDef pawnCapacityDef in WITab_AerialVehicle_Health.CapacitiesToDisplay)
      {
        Widgets.Label(new Rect(num2, 3f, 100f, 100f), GenText.Truncate(((Def) pawnCapacityDef).LabelCap, 100f, (Dictionary<string, TaggedString>) null));
        num2 += 100f;
      }
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  private void DoRows(ref float curY, Rect scrollViewRect, Rect scrollOutRect)
  {
    List<Pawn> pawns = this.Pawns;
    if (this.specificHealthTabForPawn != null && !pawns.Contains(this.specificHealthTabForPawn))
      this.specificHealthTabForPawn = (Pawn) null;
    bool flag1 = false;
    foreach (Pawn p in pawns)
    {
      if (p.IsColonist)
      {
        if (!flag1)
        {
          Widgets.ListSeparator(ref curY, ((Rect) ref scrollViewRect).width, TaggedString.op_Implicit(Translator.Translate("CaravanColonists")));
          flag1 = true;
        }
        this.DoRow(ref curY, scrollViewRect, scrollOutRect, p);
      }
    }
    bool flag2 = false;
    foreach (Pawn p in pawns)
    {
      if (!p.IsColonist)
      {
        if (!flag2)
        {
          Widgets.ListSeparator(ref curY, ((Rect) ref scrollViewRect).width, TaggedString.op_Implicit(ModsConfig.BiotechActive ? Translator.Translate("CaravanPrisonersAnimalsAndMechs") : Translator.Translate("CaravanPrisonersAndAnimals")));
          flag2 = true;
        }
        this.DoRow(ref curY, scrollViewRect, scrollOutRect, p);
      }
    }
  }

  private Vector2 GetRawSize(bool compactMode)
  {
    float num = 100f;
    if (!compactMode)
      num = num + 100f + (float) WITab_AerialVehicle_Health.CapacitiesToDisplay.Count * 100f + 40f;
    Vector2 rawSize;
    rawSize.x = (float) ((double) sbyte.MaxValue + (double) num + 16.0);
    rawSize.y = Mathf.Min(550f, ((InspectTabBase) this).PaneTopY - 30f);
    return rawSize;
  }

  private void DoRow(ref float curY, Rect viewRect, Rect scrollOutRect, Pawn p)
  {
    float num1 = this.scrollPosition.y - 40f;
    float num2 = this.scrollPosition.y + ((Rect) ref scrollOutRect).height;
    if ((double) curY > (double) num1 && (double) curY < (double) num2)
      this.DoRow(new Rect(0.0f, curY, ((Rect) ref viewRect).width, 40f), p);
    curY += 40f;
  }

  private void DoRow(Rect rect, Pawn p)
  {
    Widgets.BeginGroup(rect);
    Rect rowRect = GenUI.AtZero(rect);
    AerialVehicleTabHelper.DoAbandonButton(rowRect, (Thing) p, this.SelAerialVehicle);
    ref Rect local1 = ref rowRect;
    ((Rect) ref local1).width = ((Rect) ref local1).width - 24f;
    Widgets.InfoCardButton(((Rect) ref rowRect).width - 24f, (float) (((double) ((Rect) ref rect).height - 24.0) / 2.0), (Thing) p);
    ref Rect local2 = ref rowRect;
    ((Rect) ref local2).width = ((Rect) ref local2).width - 24f;
    CaravanThingsTabUtility.DoOpenSpecificTabButton(rowRect, p, ref this.specificHealthTabForPawn);
    ref Rect local3 = ref rowRect;
    ((Rect) ref local3).width = ((Rect) ref local3).width - 24f;
    if (Mouse.IsOver(rowRect))
      Widgets.DrawHighlight(rowRect);
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(4f, (float) (((double) ((Rect) ref rect).height - 27.0) / 2.0), 27f, 27f);
    Widgets.ThingIcon(rect1, (Thing) p, 1f, new Rot4?(), false, 1f, false);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).xMax + 4f, 11f, 100f, 18f);
    GenMapUI.DrawPawnLabel(p, rect2, 1f, 100f, (Dictionary<string, string>) null, (GameFont) 1, false, false);
    float xMax = ((Rect) ref rect2).xMax;
    if (!this.compactMode)
    {
      if (p.RaceProps.IsFlesh)
      {
        Rect rect3;
        // ISSUE: explicit constructor call
        ((Rect) ref rect3).\u002Ector(xMax, 0.0f, 100f, 40f);
        this.DoPain(rect3, p);
      }
      float num = xMax + 100f;
      foreach (PawnCapacityDef capacity in WITab_AerialVehicle_Health.CapacitiesToDisplay)
      {
        Rect rect4;
        // ISSUE: explicit constructor call
        ((Rect) ref rect4).\u002Ector(num, 0.0f, 100f, 40f);
        if (p.RaceProps.Humanlike && !capacity.showOnHumanlikes || p.RaceProps.Animal && !capacity.showOnAnimals || p.RaceProps.IsMechanoid && !capacity.showOnMechanoids || !PawnCapacityUtility.BodyCanEverDoCapacity(p.RaceProps.body, capacity))
        {
          num += 100f;
        }
        else
        {
          this.DoCapacity(rect4, p, capacity);
          num += 100f;
        }
      }
    }
    if (p.Downed && !p.ageTracker.CurLifeStage.alwaysDowned)
    {
      TextBlock textBlock;
      // ISSUE: explicit constructor call
      ((TextBlock) ref textBlock).\u002Ector(new Color(1f, 0.0f, 0.0f, 0.5f));
      try
      {
        Widgets.DrawLineHorizontal(0.0f, ((Rect) ref rect).height / 2f, ((Rect) ref rect).width);
      }
      finally
      {
        textBlock.Dispose();
      }
    }
    Widgets.EndGroup();
  }

  private void DoPain(Rect rect, Pawn pawn)
  {
    Pair<string, Color> painLabel = HealthCardUtility.GetPainLabel(pawn);
    if (Mouse.IsOver(rect))
      Widgets.DrawHighlight(rect);
    GUI.color = painLabel.Second;
    Text.Anchor = (TextAnchor) 4;
    Widgets.Label(rect, painLabel.First);
    GUI.color = Color.white;
    Text.Anchor = (TextAnchor) 0;
    if (!Mouse.IsOver(rect))
      return;
    string painTip = HealthCardUtility.GetPainTip(pawn);
    TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(painTip));
  }

  private void DoCapacity(Rect rect, Pawn pawn, PawnCapacityDef capacity)
  {
    Pair<string, Color> efficiencyLabel = HealthCardUtility.GetEfficiencyLabel(pawn, capacity);
    if (Mouse.IsOver(rect))
      Widgets.DrawHighlight(rect);
    GUI.color = efficiencyLabel.Second;
    Text.Anchor = (TextAnchor) 4;
    Widgets.Label(rect, efficiencyLabel.First);
    GUI.color = Color.white;
    Text.Anchor = (TextAnchor) 0;
    if (!Mouse.IsOver(rect))
      return;
    string pawnCapacityTip = HealthCardUtility.GetPawnCapacityTip(pawn, capacity);
    TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(pawnCapacityTip));
  }

  public virtual void Notify_ClearingAllMapsMemory()
  {
    ((InspectTabBase) this).Notify_ClearingAllMapsMemory();
    this.specificHealthTabForPawn = (Pawn) null;
  }

  private void EnsureSpecificHealthTabForPawnValid()
  {
    if (this.specificHealthTabForPawn == null || !((Thing) this.specificHealthTabForPawn).Destroyed && this.SelAerialVehicle.Vehicle.AllPawnsAboard.Contains(this.specificHealthTabForPawn))
      return;
    this.specificHealthTabForPawn = (Pawn) null;
  }
}
