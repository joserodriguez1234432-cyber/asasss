// Decompiled with JetBrains decompiler
// Type: Vehicles.Rendering.UIHelper
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
using Vehicles.World;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles.Rendering;

public static class UIHelper
{
  public static void CreateVehicleCaravanTransferableWidgets(
    List<TransferableOneWay> transferables,
    out TransferableOneWayWidget pawnsTransfer,
    out TransferableVehicleWidget vehiclesTransfer,
    out TransferableOneWayWidget itemsTransfer,
    string thingCountTip,
    IgnorePawnsInventoryMode ignorePawnInventoryMass,
    Func<float> availableMassGetter,
    bool ignoreSpawnedCorpseGearAndInventoryMass,
    PlanetTile tile,
    bool playerPawnsReadOnly = false)
  {
    pawnsTransfer = new TransferableOneWayWidget((IEnumerable<TransferableOneWay>) null, (string) null, (string) null, thingCountTip, true, ignorePawnInventoryMass, false, availableMassGetter, 0.0f, ignoreSpawnedCorpseGearAndInventoryMass, new PlanetTile?(tile), true, true, true, false, true, false, playerPawnsReadOnly, false, false, false);
    UIHelper.AddVehicleAndPawnSections(pawnsTransfer, out vehiclesTransfer, transferables, tile);
    itemsTransfer = new TransferableOneWayWidget(transferables.Where<TransferableOneWay>((Func<TransferableOneWay, bool>) (t => ((Transferable) t).ThingDef.category != 1)), (string) null, (string) null, thingCountTip, true, ignorePawnInventoryMass, false, availableMassGetter, 0.0f, ignoreSpawnedCorpseGearAndInventoryMass, new PlanetTile?(tile), true, false, false, true, false, true, false, false, false, false);
  }

  private static void AddVehicleAndPawnSections(
    TransferableOneWayWidget pawnWidget,
    out TransferableVehicleWidget vehicleWidget,
    List<TransferableOneWay> transferables,
    PlanetTile tile)
  {
    IEnumerable<TransferableOneWay> source = transferables.Where<TransferableOneWay>((Func<TransferableOneWay, bool>) (t => ((Transferable) t).ThingDef.category == 1));
    List<TransferableOneWay> vehicles = new List<TransferableOneWay>();
    List<TransferableOneWay> pawns = new List<TransferableOneWay>();
    foreach (TransferableOneWay transferable in transferables)
    {
      if (((Transferable) transferable).ThingDef.category == 1)
      {
        Thing anyThing = ((Transferable) transferable).AnyThing;
        if (!(anyThing is VehiclePawn))
        {
          if (anyThing is Pawn pawn && pawn.IsFreeColonist)
            pawns.Add(transferable);
        }
        else
          vehicles.Add(transferable);
      }
    }
    vehicleWidget = new TransferableVehicleWidget(TaggedString.op_Implicit(Translator.Translate("VF_Vehicles")), vehicles, pawns, tile);
    pawnWidget.AddSection(TaggedString.op_Implicit(Translator.Translate("ColonistsSection")), source.Where<TransferableOneWay>((Func<TransferableOneWay, bool>) (t => ((Transferable) t).AnyThing is Pawn anyThing1 && anyThing1.IsFreeColonist)));
    pawnWidget.AddSection(TaggedString.op_Implicit(Translator.Translate("PrisonersSection")), source.Where<TransferableOneWay>((Func<TransferableOneWay, bool>) (t => ((Transferable) t).AnyThing is Pawn anyThing2 && anyThing2.IsPrisoner)));
    pawnWidget.AddSection(TaggedString.op_Implicit(Translator.Translate("CaptureSection")), source.Where<TransferableOneWay>((Func<TransferableOneWay, bool>) (t => ((Transferable) t).AnyThing is Pawn anyThing3 && anyThing3.Downed && CaravanUtility.ShouldAutoCapture(anyThing3, Faction.OfPlayer))));
    pawnWidget.AddSection(TaggedString.op_Implicit(Translator.Translate("AnimalsSection")), source.Where<TransferableOneWay>((Func<TransferableOneWay, bool>) (t => ((Transferable) t).AnyThing is Pawn anyThing4 && anyThing4.RaceProps.Animal)));
  }

  public static bool DrawPagination(Rect rect, ref int pageNumber, int pageCount)
  {
    bool flag = false;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y, ((Rect) ref rect).height, ((Rect) ref rect).height);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect).x + ((Rect) ref rect).width - ((Rect) ref rect).height, ((Rect) ref rect).y, ((Rect) ref rect).height, ((Rect) ref rect).height);
    ref Rect local1 = ref rect;
    ((Rect) ref local1).xMin = ((Rect) ref local1).xMin + ((Rect) ref rect1).width;
    ref Rect local2 = ref rect;
    ((Rect) ref local2).xMax = ((Rect) ref local2).xMax - ((Rect) ref rect2).width;
    if (Widgets.ButtonImage(rect1, VehicleTex.LeftArrow, true, (string) null))
    {
      flag = true;
      pageNumber = (--pageNumber).Clamp(1, pageCount);
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.PageChange, (Map) null);
    }
    if (Widgets.ButtonImage(rect2, VehicleTex.RightArrow, true, (string) null))
    {
      flag = true;
      pageNumber = (++pageNumber).Clamp(1, pageCount);
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.PageChange, (Map) null);
    }
    float num1 = ((Rect) ref rect).width - ((Rect) ref rect).height * 2f;
    int num2 = Mathf.CeilToInt(num1 / 1.5f / ((Rect) ref rect).height);
    int num3 = Mathf.FloorToInt((float) num2 / 2f);
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 1, (TextAnchor) 4);
    try
    {
      float num4 = (float) ((double) ((Rect) ref rect).x + (double) ((Rect) ref rect).height + (double) num1 / 2.0);
      Rect rect3 = new Rect(num4, ((Rect) ref rect).y, ((Rect) ref rect).height, ((Rect) ref rect).height);
      Widgets.ButtonText(rect3, pageNumber.ToString(), false, false, true, new TextAnchor?());
      Text.Font = (GameFont) 0;
      int num5 = 1;
      int num6 = pageNumber + 1;
      while (num6 <= pageNumber + num3 && num6 <= pageCount)
      {
        ((Rect) ref rect3).x = num4 + num1 / (float) num2 * (float) num5;
        if (Widgets.ButtonText(rect3, num6.ToString(), false, true, true, new TextAnchor?()))
        {
          flag = true;
          pageNumber = num6;
          SoundStarter.PlayOneShotOnCamera(SoundDefOf.PageChange, (Map) null);
        }
        ++num6;
        ++num5;
      }
      int num7 = 1;
      int num8 = pageNumber - 1;
      while (num8 >= pageNumber - num3 && num8 >= 1)
      {
        ((Rect) ref rect3).x = num4 - num1 / (float) num2 * (float) num7;
        if (Widgets.ButtonText(rect3, num8.ToString(), false, true, true, new TextAnchor?()))
        {
          flag = true;
          pageNumber = num8;
          SoundStarter.PlayOneShotOnCamera(SoundDefOf.PageChange, (Map) null);
        }
        --num8;
        ++num7;
      }
      return flag;
    }
    finally
    {
      textBlock.Dispose();
    }
  }
}
