// Decompiled with JetBrains decompiler
// Type: Vehicles.World.AerialVehicleTabHelper
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
public static class AerialVehicleTabHelper
{
  public const float MassColumnWidth = 60f;
  public const float SpaceAroundIcon = 4f;
  public const float SpecificTabButtonSize = 24f;
  public const float AbandonButtonSize = 24f;
  public const float AbandonSpecificCountButtonSize = 24f;
  private const float RowHeight = 30f;
  private const float LabelColumnWidth = 300f;

  public static void DoRows(
    Vector2 size,
    List<TransferableImmutable> things,
    AerialVehicleInFlight aerialVehicle,
    ref Vector2 scrollPosition,
    ref float scrollViewHeight)
  {
    Text.Font = (GameFont) 1;
    Rect scrollOutRect = GenUI.ContractedBy(new Rect(0.0f, 0.0f, size.x, size.y), 10f);
    Rect viewRect;
    // ISSUE: explicit constructor call
    ((Rect) ref viewRect).\u002Ector(0.0f, 0.0f, ((Rect) ref scrollOutRect).width - 16f, scrollViewHeight);
    Widgets.BeginScrollView(scrollOutRect, ref scrollPosition, viewRect, true);
    float curY = 0.0f;
    Widgets.ListSeparator(ref curY, ((Rect) ref viewRect).width, TaggedString.op_Implicit(Translator.Translate("CaravanItems")));
    if (GenCollection.Any<TransferableImmutable>(things))
    {
      for (int index = 0; index < things.Count; ++index)
        AerialVehicleTabHelper.DoRow(ref curY, viewRect, scrollOutRect, scrollPosition, things[index], aerialVehicle);
    }
    else
      Widgets.NoneLabel(ref curY, ((Rect) ref viewRect).width, (string) null);
    if (Event.current.type == 8)
      scrollViewHeight = curY + 30f;
    Widgets.EndScrollView();
  }

  public static Vector2 GetSize(List<TransferableImmutable> things, float paneTopY, bool doNeeds = true)
  {
    float num = 300f + 24f + 60f;
    Vector2 size;
    size.x = (float) (103.0 + (double) num + 16.0);
    size.y = Mathf.Min(550f, paneTopY - 30f);
    return size;
  }

  private static void DoRow(
    ref float curY,
    Rect viewRect,
    Rect scrollOutRect,
    Vector2 scrollPosition,
    TransferableImmutable thing,
    AerialVehicleInFlight aerialVehicle)
  {
    float num1 = scrollPosition.y - 30f;
    float num2 = scrollPosition.y + ((Rect) ref scrollOutRect).height;
    if ((double) curY > (double) num1 && (double) curY < (double) num2)
      AerialVehicleTabHelper.DoRow(new Rect(0.0f, curY, ((Rect) ref viewRect).width, 30f), thing, aerialVehicle);
    curY += 30f;
  }

  private static void DoRow(
    Rect rect,
    TransferableImmutable thing,
    AerialVehicleInFlight aerialVehicle)
  {
    Widgets.BeginGroup(rect);
    Rect rowRect = GenUI.AtZero(rect);
    if (thing.TotalStackCount != 1)
      AerialVehicleTabHelper.DoAbandonSpecificCountButton(rowRect, thing, aerialVehicle);
    ref Rect local1 = ref rowRect;
    ((Rect) ref local1).width = ((Rect) ref local1).width - 24f;
    Widgets.InfoCardButton(((Rect) ref rowRect).width - 24f, (float) (((double) ((Rect) ref rect).height - 24.0) / 2.0), ((Transferable) thing).AnyThing);
    ref Rect local2 = ref rowRect;
    ((Rect) ref local2).width = ((Rect) ref local2).width - 24f;
    Rect rect1 = rowRect;
    ((Rect) ref rect1).xMin = ((Rect) ref rect1).xMax - 60f;
    CaravanThingsTabUtility.DrawMass(thing, rect1);
    ref Rect local3 = ref rowRect;
    ((Rect) ref local3).width = ((Rect) ref local3).width - 60f;
    Widgets.DrawHighlightIfMouseover(rowRect);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(4f, (float) (((double) ((Rect) ref rect).height - 27.0) / 2.0), 27f, 27f);
    Widgets.ThingIcon(rect2, ((Transferable) thing).AnyThing, 1f, new Rot4?(), false, 1f, false);
    Rect rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector(((Rect) ref rect2).xMax + 4f, 0.0f, 300f, 30f);
    Text.Anchor = (TextAnchor) 3;
    Text.WordWrap = false;
    Widgets.Label(rect3, GenText.Truncate(thing.LabelCapWithTotalStackCount, ((Rect) ref rect3).width, (Dictionary<string, string>) null));
    Text.Anchor = (TextAnchor) 0;
    Text.WordWrap = true;
    Widgets.EndGroup();
  }

  public static void DoAbandonButton(
    Rect rowRect,
    Thing thing,
    AerialVehicleInFlight aerialVehicle)
  {
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(((Rect) ref rowRect).width - 24f, (float) (((double) ((Rect) ref rowRect).height - 24.0) / 2.0), 24f, 24f);
    if (Widgets.ButtonImage(rect, CaravanThingsTabUtility.AbandonButtonTex, true, (string) null))
      AerialVehicleAbandonOrBanishHelper.TryAbandonOrBanishViaInterface(thing, aerialVehicle);
    if (!Mouse.IsOver(rect))
      return;
    TooltipHandler.TipRegion(rect, (Func<string>) (() => AerialVehicleAbandonOrBanishHelper.GetAbandonOrBanishButtonTooltip(thing, false)), Gen.HashCombineInt(thing.GetHashCode(), 1383004931));
  }

  public static void DoAbandonButton(
    Rect rowRect,
    TransferableImmutable transferable,
    AerialVehicleInFlight aerialVehicle)
  {
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(((Rect) ref rowRect).width - 24f, (float) (((double) ((Rect) ref rowRect).height - 24.0) / 2.0), 24f, 24f);
    if (Widgets.ButtonImage(rect, CaravanThingsTabUtility.AbandonButtonTex, true, (string) null))
      AerialVehicleAbandonOrBanishHelper.TryAbandonOrBanishViaInterface(transferable, aerialVehicle);
    if (!Mouse.IsOver(rect))
      return;
    TooltipHandler.TipRegion(rect, (Func<string>) (() => AerialVehicleAbandonOrBanishHelper.GetAbandonOrBanishButtonTooltip(transferable, false)), Gen.HashCombineInt(transferable.GetHashCode(), 8476546));
  }

  public static void DoAbandonSpecificCountButton(
    Rect rowRect,
    Thing thing,
    AerialVehicleInFlight aerialVehicle)
  {
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(((Rect) ref rowRect).width - 24f, (float) (((double) ((Rect) ref rowRect).height - 24.0) / 2.0), 24f, 24f);
    if (Widgets.ButtonImage(rect, CaravanThingsTabUtility.AbandonSpecificCountButtonTex, true, (string) null))
      AerialVehicleAbandonOrBanishHelper.TryAbandonSpecificCountViaInterface(thing, aerialVehicle);
    if (!Mouse.IsOver(rect))
      return;
    TooltipHandler.TipRegion(rect, (Func<string>) (() => AerialVehicleAbandonOrBanishHelper.GetAbandonOrBanishButtonTooltip(thing, true)), Gen.HashCombineInt(thing.GetHashCode(), 1163428609));
  }

  public static void DoAbandonSpecificCountButton(
    Rect rowRect,
    TransferableImmutable transferable,
    AerialVehicleInFlight aerialVehicle)
  {
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(((Rect) ref rowRect).width - 24f, (float) (((double) ((Rect) ref rowRect).height - 24.0) / 2.0), 24f, 24f);
    if (Widgets.ButtonImage(rect, CaravanThingsTabUtility.AbandonSpecificCountButtonTex, true, (string) null))
      AerialVehicleAbandonOrBanishHelper.TryAbandonSpecificCountViaInterface(transferable, aerialVehicle);
    if (!Mouse.IsOver(rect))
      return;
    TooltipHandler.TipRegion(rect, (Func<string>) (() => AerialVehicleAbandonOrBanishHelper.GetAbandonOrBanishButtonTooltip(transferable, true)), Gen.HashCombineInt(transferable.GetHashCode(), 1163428609));
  }

  public static void DoOpenSpecificTabButton(Rect rowRect, Pawn p, ref Pawn specificTabForPawn)
  {
    Color color1 = p == specificTabForPawn ? CaravanThingsTabUtility.OpenedSpecificTabButtonColor : Color.white;
    Color color2 = p == specificTabForPawn ? CaravanThingsTabUtility.OpenedSpecificTabButtonMouseoverColor : GenUI.MouseoverColor;
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(((Rect) ref rowRect).width - 24f, (float) (((double) ((Rect) ref rowRect).height - 24.0) / 2.0), 24f, 24f);
    if (Widgets.ButtonImage(rect, CaravanThingsTabUtility.SpecificTabButtonTex, color1, color2, true, (string) null))
    {
      if (p == specificTabForPawn)
      {
        specificTabForPawn = (Pawn) null;
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.TabClose, (Map) null);
      }
      else
      {
        specificTabForPawn = p;
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.TabOpen, (Map) null);
      }
    }
    TooltipHandler.TipRegionByKey(rect, "OpenSpecificTabButtonTip");
    GUI.color = Color.white;
  }

  public static void DoOpenSpecificTabButtonInvisible(
    Rect rect,
    Pawn pawn,
    ref Pawn specificTabForPawn)
  {
    if (!Widgets.ButtonInvisible(rect, true))
      return;
    specificTabForPawn = pawn != specificTabForPawn ? pawn : (Pawn) null;
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.TabClose, (Map) null);
  }

  public static void DrawMass(TransferableImmutable transferable, Rect rect)
  {
    float mass = 0.0f;
    for (int index = 0; index < transferable.things.Count; ++index)
      mass += StatExtension.GetStatValue(transferable.things[index], StatDefOf.Mass, true, -1) * (float) transferable.things[index].stackCount;
    AerialVehicleTabHelper.DrawMass(mass, rect);
  }

  public static void DrawMass(Thing thing, Rect rect)
  {
    AerialVehicleTabHelper.DrawMass(StatExtension.GetStatValue(thing, StatDefOf.Mass, true, -1) * (float) thing.stackCount, rect);
  }

  private static void DrawMass(float mass, Rect rect)
  {
    GUI.color = TransferableOneWayWidget.ItemMassColor;
    Text.Anchor = (TextAnchor) 3;
    Text.WordWrap = false;
    Widgets.Label(rect, GenText.ToStringMass(mass));
    Text.WordWrap = true;
    Text.Anchor = (TextAnchor) 0;
    GUI.color = Color.white;
  }
}
