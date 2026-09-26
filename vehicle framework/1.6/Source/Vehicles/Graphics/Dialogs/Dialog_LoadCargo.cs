// Decompiled with JetBrains decompiler
// Type: Vehicles.Dialog_LoadCargo
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
namespace Vehicles;

public class Dialog_LoadCargo : Window
{
  private VehiclePawn vehicle;
  private List<TransferableOneWay> transferables = new List<TransferableOneWay>();
  private TransferableOneWayWidget itemsTransfer;
  private bool massUsageDirty;
  private float cachedMassUsage;
  private readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);
  private static readonly List<Pair<float, Color>> MassColor = new List<Pair<float, Color>>()
  {
    new Pair<float, Color>(0.1f, Color.green),
    new Pair<float, Color>(0.75f, Color.yellow),
    new Pair<float, Color>(1f, new Color(1f, 0.6f, 0.0f))
  };

  public Dialog_LoadCargo(VehiclePawn vehicle)
    : base((IWindowDrawing) null)
  {
    this.vehicle = vehicle;
    this.closeOnAccept = true;
    this.closeOnCancel = true;
    this.forcePause = false;
    this.absorbInputAroundWindow = true;
  }

  public virtual Vector2 InitialSize => new Vector2(1024f, (float) UI.screenHeight);

  public float MassUsage
  {
    get
    {
      if (this.massUsageDirty)
      {
        this.massUsageDirty = false;
        this.cachedMassUsage = CollectionsMassCalculator.MassUsageTransferables(this.transferables, (IgnorePawnsInventoryMode) 2, true, false);
        this.cachedMassUsage += MassUtility.GearAndInventoryMass((Pawn) this.vehicle);
      }
      return this.cachedMassUsage;
    }
  }

  public float MassCapacity => this.vehicle.GetStatValue(VehicleStatDefOf.CargoCapacity);

  public virtual void PostOpen()
  {
    base.PostOpen();
    this.massUsageDirty = true;
    this.CalculateAndRecacheTransferables();
  }

  public virtual void PostClose() => base.PostClose();

  public virtual void DoWindowContents(Rect inRect)
  {
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(0.0f, 0.0f, ((Rect) ref inRect).width, 35f);
    Text.Font = (GameFont) 2;
    Text.Anchor = (TextAnchor) 4;
    Widgets.Label(rect1, ((Entity) this.vehicle).LabelShortCap);
    Text.Font = (GameFont) 1;
    Text.Anchor = (TextAnchor) 0;
    this.DrawCargoNumbers(new Rect(12f, 35f, ((Rect) ref inRect).width - 24f, 40f));
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref inRect).width - 225f, 35f, 225f, 40f);
    bool showAllCargoItems = VehicleMod.settings.showAllCargoItems;
    string str = TaggedString.op_Implicit(Translator.Translate("VF_ShowAllItemsOnMap"));
    Widgets.Label(rect2, str);
    ref Rect local1 = ref rect2;
    ((Rect) ref local1).x = ((Rect) ref local1).x + (Text.CalcSize(str).x + 20f);
    Widgets.Checkbox(new Vector2(((Rect) ref rect2).x, ((Rect) ref rect2).y), ref VehicleMod.settings.showAllCargoItems, 24f, false, false, (Texture2D) null, (Texture2D) null);
    if (showAllCargoItems != VehicleMod.settings.showAllCargoItems)
      this.CalculateAndRecacheTransferables();
    ref Rect local2 = ref inRect;
    ((Rect) ref local2).yMin = ((Rect) ref local2).yMin + 60f;
    Widgets.DrawMenuSection(inRect);
    inRect = GenUI.ContractedBy(inRect, 17f);
    Widgets.BeginGroup(inRect);
    Rect rect3 = GenUI.AtZero(inRect);
    this.BottomButtons(rect3);
    Rect rect4 = rect3;
    ref Rect local3 = ref rect4;
    ((Rect) ref local3).yMax = ((Rect) ref local3).yMax - 76f;
    bool flag;
    this.itemsTransfer.OnGUI(rect4, ref flag);
    if (flag)
      this.CountToTransferChanged();
    Widgets.EndGroup();
  }

  public void BottomButtons(Rect rect)
  {
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector((float) ((double) ((Rect) ref rect).width / 2.0 - (double) this.BottomButtonSize.x / 2.0), (float) ((double) ((Rect) ref rect).height - 55.0 - 17.0), this.BottomButtonSize.x, this.BottomButtonSize.y);
    if (Widgets.ButtonText(rect1, TaggedString.op_Implicit(Translator.Translate("AcceptButton")), true, true, true, new TextAnchor?()))
    {
      this.vehicle.cargoToLoad = this.transferables.Where<TransferableOneWay>((Func<TransferableOneWay, bool>) (t => ((Transferable) t).CountToTransfer > 0)).ToList<TransferableOneWay>();
      ((Thing) this.vehicle).Map.GetCachedMapComponent<VehicleReservationManager>().RegisterLister(this.vehicle, "LoadVehicle");
      this.Close(true);
    }
    if (Widgets.ButtonText(new Rect(((Rect) ref rect1).x - 10f - this.BottomButtonSize.x, ((Rect) ref rect1).y, this.BottomButtonSize.x, this.BottomButtonSize.y), TaggedString.op_Implicit(Translator.Translate("ResetButton")), true, true, true, new TextAnchor?()))
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_Low, (Map) null);
      this.CalculateAndRecacheTransferables();
    }
    if (Widgets.ButtonText(new Rect(((Rect) ref rect1).xMax + 10f, ((Rect) ref rect1).y, this.BottomButtonSize.x, this.BottomButtonSize.y), TaggedString.op_Implicit(Translator.Translate("CancelButton")), true, true, true, new TextAnchor?()))
      this.Close(true);
    if (!Prefs.DevMode)
      return;
    float num1 = 200f;
    float num2 = this.BottomButtonSize.y / 2f;
    if (Widgets.ButtonText(new Rect(0.0f, (float) ((double) ((Rect) ref rect).height - 55.0 - 17.0), num1, num2), "Dev: Pack Instantly", true, true, true, new TextAnchor?()))
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, (Map) null);
      for (int index = 0; index < this.transferables.Count; ++index)
        TransferableUtility.Transfer(this.transferables[index].things, ((Transferable) this.transferables[index]).CountToTransfer, (Action<Thing, IThingHolder>) ((thing, originalHolder) => this.vehicle.AddOrTransfer(thing)));
      this.Close(false);
    }
    if (!Widgets.ButtonText(new Rect(0.0f, (float) ((double) ((Rect) ref rect).height - 55.0 - 17.0) + num2, num1, num2), "Dev: Select everything", true, true, true, new TextAnchor?()))
      return;
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, (Map) null);
    this.SetToSendEverything();
  }

  public void DrawCargoNumbers(Rect rect)
  {
    Color color1 = (double) this.MassUsage <= (double) this.MassCapacity ? ((double) this.MassCapacity != 0.0 ? GenUI.LerpColor(Dialog_LoadCargo.MassColor, this.MassUsage / this.MassCapacity) : Color.grey) : Color.red;
    Color color2 = GUI.color;
    GUI.color = color1;
    string str = $"{Translator.Translate("Mass")}: {this.MassUsage}/{this.MassCapacity}";
    Widgets.Label(rect, str);
    GUI.color = color2;
  }

  private void AddToTransferables(Thing t, bool setToTransferMax = false)
  {
    TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching<TransferableOneWay>(t, this.transferables, (TransferAsOneMode) 1);
    if (transferableOneWay == null)
    {
      transferableOneWay = new TransferableOneWay();
      this.transferables.Add(transferableOneWay);
    }
    if (transferableOneWay.things.Contains(t))
    {
      Log.Error("Tried to add the same thing twice to TransferableOneWay: " + t?.ToString());
    }
    else
    {
      transferableOneWay.things.Add(t);
      if (!setToTransferMax)
        return;
      ((Transferable) transferableOneWay).AdjustTo(((Transferable) transferableOneWay).CountToTransfer + t.stackCount);
    }
  }

  private void CalculateAndRecacheTransferables()
  {
    this.transferables = new List<TransferableOneWay>();
    this.AddItemsToTransferables();
    this.itemsTransfer = new TransferableOneWayWidget((IEnumerable<TransferableOneWay>) this.transferables, (string) null, (string) null, (string) null, true, (IgnorePawnsInventoryMode) 1, false, (Func<float>) (() => this.MassCapacity - this.MassUsage), 0.0f, false, new PlanetTile?(), false, false, false, false, false, false, false, false, false, false);
    this.CountToTransferChanged();
  }

  private void AddItemsToTransferables()
  {
    List<Thing> thingList = CaravanFormingUtility.AllReachableColonyItems(((Thing) this.vehicle).Map, VehicleMod.settings.showAllCargoItems, false, false);
    for (int index = 0; index < thingList.Count; ++index)
      this.AddToTransferables(thingList[index]);
  }

  private void SetToSendEverything()
  {
    for (int index = 0; index < this.transferables.Count; ++index)
      ((Transferable) this.transferables[index]).AdjustTo(((Transferable) this.transferables[index]).GetMaximumToTransfer());
    this.CountToTransferChanged();
  }

  private void CountToTransferChanged() => this.massUsageDirty = true;
}
