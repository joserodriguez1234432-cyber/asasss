// Decompiled with JetBrains decompiler
// Type: Vehicles.ITab_Vehicle_Cargo
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

public class ITab_Vehicle_Cargo : ITab_Airdrop_Container
{
  public ITab_Vehicle_Cargo()
  {
    ((InspectTabBase) this).size = new Vector2(460f, 450f);
    ((InspectTabBase) this).labelKey = "VF_TabCargo";
  }

  public override bool IsVisible => !this.Vehicle.beached;

  protected override string InventoryLabelKey => "VF_Cargo";

  protected override bool AllowDropping => true;

  private VehiclePawn Vehicle
  {
    get
    {
      return this.SelPawn is VehiclePawn selPawn ? selPawn : throw new InvalidOperationException("Cargo tab on non-pawn ship " + this.SelThing?.ToString());
    }
  }

  protected override void DrawAdditionalRows(ref float curY, Rect rect)
  {
    foreach (TransferableOneWay transferableOneWay in this.Vehicle.cargoToLoad)
    {
      if (((Transferable) transferableOneWay).AnyThing != null && ((Transferable) transferableOneWay).CountToTransfer > 0 && !((ThingOwner) this.Vehicle.inventory.innerContainer).Contains(((Transferable) transferableOneWay).AnyThing))
        this.DrawThingRow(ref curY, ((Rect) ref rect).width, ((Transferable) transferableOneWay).AnyThing, new int?(((Transferable) transferableOneWay).CountToTransfer), missingFromInventory: true);
    }
  }

  protected override void DrawHeader(ref float curY, float width)
  {
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(0.0f, curY, width, 22f);
    float num1 = MassUtility.GearAndInventoryMass((Pawn) this.Vehicle);
    float statValue = this.Vehicle.GetStatValue(VehicleStatDefOf.CargoCapacity);
    Widgets.Label(rect1, TranslatorFormattedStringExtensions.Translate("MassCarried", NamedArgument.op_Implicit(num1.ToString("0.##")), NamedArgument.op_Implicit(statValue.ToString("0.##"))));
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).xMax - 24f, curY, 24f, 24f);
    if (this.AllowDropping && this.Inventory.Any)
    {
      TooltipHandler.TipRegion(rect2, TipSignal.op_Implicit(Translator.Translate("EjectAll")));
      if (Widgets.ButtonImageFitted(rect2, TexData.Drop))
      {
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, (Map) null);
        this.InterfaceDropAll();
      }
    }
    curY += 22f;
    Rect rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector(0.0f, curY, width, 22f);
    float num2 = 0.0f;
    foreach (Thing thing in (IEnumerable<Thing>) this.Inventory)
      num2 += thing.MarketValue * (float) thing.stackCount;
    Widgets.Label(rect3, $"{GenText.CapitalizeFirst(((Def) StatDefOf.MarketValue).label, (Def) StatDefOf.MarketValue)}: {num2.ToString("F0")}");
    curY += 22f;
  }

  protected override bool InterfaceDrop(Thing thing)
  {
    bool flag = base.InterfaceDrop(thing);
    if (flag)
      this.Vehicle.EventRegistry[VehicleEventDefOf.CargoRemoved].ExecuteEvents();
    return flag;
  }

  protected override bool InterfaceDropAll()
  {
    bool flag = base.InterfaceDropAll();
    if (flag)
      this.Vehicle.EventRegistry[VehicleEventDefOf.CargoRemoved].ExecuteEvents();
    return flag;
  }
}
