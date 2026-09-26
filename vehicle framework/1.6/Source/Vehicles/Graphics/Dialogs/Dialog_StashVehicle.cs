// Decompiled with JetBrains decompiler
// Type: Vehicles.World.Dialog_StashVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles.World;

public class Dialog_StashVehicle : Window
{
  private const float TitleRectHeight = 35f;
  private const float BottomAreaHeight = 55f;
  private readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);
  private VehicleCaravan caravan;
  private List<TransferableOneWay> transferables = new List<TransferableOneWay>();
  private TransferableOneWayWidget itemsTransfer;
  private bool sourceMassUsageDirty = true;
  private float cachedSourceMassUsage;
  private bool sourceMassCapacityDirty = true;
  private float cachedSourceMassCapacity;
  private string cachedSourceMassCapacityExplanation;
  private bool sourceTilesPerDayDirty = true;
  private float cachedSourceTilesPerDay;
  private string cachedSourceTilesPerDayExplanation;
  private bool sourceDaysWorthOfFoodDirty = true;
  private (float days, float tillRot) cachedSourceDaysWorthOfFood;
  private bool sourceForagedFoodPerDayDirty = true;
  private (ThingDef, float) cachedSourceForagedFoodPerDay;
  private string cachedSourceForagedFoodPerDayExplanation;
  private bool sourceVisibilityDirty = true;
  private float cachedSourceVisibility;
  private string cachedSourceVisibilityExplanation;
  private bool destMassUsageDirty = true;
  private float cachedDestMassUsage;
  private bool destMassCapacityDirty = true;
  private float cachedDestMassCapacity;
  private string cachedDestMassCapacityExplanation;
  private bool destTilesPerDayDirty = true;
  private float cachedDestTilesPerDay;
  private string cachedDestTilesPerDayExplanation;
  private bool destDaysWorthOfFoodDirty = true;
  private Pair<float, float> cachedDestDaysWorthOfFood;
  private bool destForagedFoodPerDayDirty = true;
  private (ThingDef food, float perDay) cachedDestForagedFoodPerDay;
  private string cachedDestForagedFoodPerDayExplanation;
  private bool destVisibilityDirty = true;
  private float cachedDestVisibility;
  private string cachedDestVisibilityExplanation;
  private bool ticksToArriveDirty = true;
  private int cachedTicksToArrive;

  public Dialog_StashVehicle(VehicleCaravan caravan)
    : base((IWindowDrawing) null)
  {
    this.caravan = caravan;
    this.forcePause = true;
    this.absorbInputAroundWindow = true;
  }

  public virtual Vector2 InitialSize => new Vector2(1024f, (float) UI.screenHeight);

  protected virtual float Margin => 0.0f;

  private BiomeDef Biome => ((WorldObject) this.caravan).Biome;

  private IEnumerable<Pawn> PawnsEmbarking
  {
    get
    {
      foreach (Pawn pawn in this.caravan.PawnsListForReading)
      {
        if (!((Thing) pawn).IsBoat())
          yield return pawn;
      }
    }
  }

  private float SourceMassUsage
  {
    get
    {
      if (this.sourceMassUsageDirty)
      {
        this.sourceMassUsageDirty = false;
        this.cachedSourceMassUsage = CollectionsMassCalculator.MassUsageTransferables(this.transferables, (IgnorePawnsInventoryMode) 0, false, false);
      }
      return this.cachedSourceMassUsage;
    }
  }

  private float SourceMassCapacity
  {
    get
    {
      if (this.sourceMassCapacityDirty)
      {
        this.sourceMassCapacityDirty = false;
        StringBuilder stringBuilder = new StringBuilder();
        float num = 0.0f;
        foreach (Pawn pawn in this.PawnsEmbarking)
        {
          if (MassUtility.CanEverCarryAnything(pawn))
            num += MassUtility.Capacity(pawn, stringBuilder);
        }
        this.cachedSourceMassCapacity = num;
        this.cachedSourceMassCapacityExplanation = stringBuilder.ToString();
      }
      return this.cachedSourceMassCapacity;
    }
  }

  private float SourceTilesPerDay
  {
    get
    {
      if (this.sourceTilesPerDayDirty)
      {
        this.sourceTilesPerDayDirty = false;
        StringBuilder stringBuilder = new StringBuilder();
        this.cachedSourceTilesPerDay = TilesPerDayCalculator.ApproxTilesPerDay(CaravanTicksPerMoveUtility.GetTicksPerMove(this.PawnsEmbarking.ToList<Pawn>(), this.SourceMassUsage, this.SourceMassCapacity, false, stringBuilder), ((WorldObject) this.caravan).Tile, PlanetTile.op_Implicit(-1), stringBuilder, (string) null, false);
        this.cachedSourceTilesPerDayExplanation = stringBuilder.ToString();
      }
      return this.cachedSourceTilesPerDay;
    }
  }

  private (float days, float tillRot) SourceDaysWorthOfFood
  {
    get
    {
      if (this.sourceDaysWorthOfFoodDirty)
      {
        this.sourceDaysWorthOfFoodDirty = false;
        this.cachedSourceDaysWorthOfFood = (DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(this.transferables, ((WorldObject) this.caravan).Tile, (IgnorePawnsInventoryMode) 3, ((WorldObject) this.caravan).Faction, (WorldPath) null, 0.0f, 3300), DaysUntilRotCalculator.ApproxDaysUntilRotLeftAfterTransfer(this.transferables, ((WorldObject) this.caravan).Tile, (IgnorePawnsInventoryMode) 0, (WorldPath) null, 0.0f, 3300));
      }
      return this.cachedSourceDaysWorthOfFood;
    }
  }

  private (ThingDef food, float perDay) SourceForagedFoodPerDay
  {
    get
    {
      if (this.sourceForagedFoodPerDayDirty)
      {
        this.sourceForagedFoodPerDayDirty = false;
        StringBuilder stringBuilder = new StringBuilder();
        this.cachedSourceForagedFoodPerDay = ForagedFoodPerDayCalculator.ForagedFoodPerDay(this.PawnsEmbarking.ToList<Pawn>(), this.Biome, Faction.OfPlayer, true, false, stringBuilder);
        this.cachedSourceForagedFoodPerDayExplanation = stringBuilder.ToString();
      }
      return this.cachedSourceForagedFoodPerDay;
    }
  }

  private float SourceVisibility
  {
    get
    {
      if (this.sourceVisibilityDirty)
      {
        this.sourceVisibilityDirty = false;
        StringBuilder stringBuilder = new StringBuilder();
        this.cachedSourceVisibility = CaravanVisibilityCalculator.Visibility(this.PawnsEmbarking, true, stringBuilder);
        this.cachedSourceVisibilityExplanation = stringBuilder.ToString();
      }
      return this.cachedSourceVisibility;
    }
  }

  private float DestMassUsage
  {
    get
    {
      if (this.destMassUsageDirty)
      {
        this.destMassUsageDirty = false;
        this.cachedDestMassUsage = CollectionsMassCalculator.MassUsageTransferables(this.transferables, (IgnorePawnsInventoryMode) 0, false, false);
      }
      return this.cachedDestMassUsage;
    }
  }

  private float DestMassCapacity
  {
    get
    {
      if (this.destMassCapacityDirty)
      {
        this.destMassCapacityDirty = false;
        StringBuilder stringBuilder = new StringBuilder();
        this.cachedDestMassCapacity = CollectionsMassCalculator.CapacityTransferables(this.transferables, stringBuilder);
        this.cachedDestMassCapacityExplanation = stringBuilder.ToString();
      }
      return this.cachedDestMassCapacity;
    }
  }

  private float DestTilesPerDay
  {
    get
    {
      if (this.destTilesPerDayDirty)
      {
        this.destTilesPerDayDirty = false;
        StringBuilder stringBuilder = new StringBuilder();
        this.cachedDestTilesPerDay = TilesPerDayCalculator.ApproxTilesPerDay(this.transferables, this.DestMassUsage, this.DestMassCapacity, ((WorldObject) this.caravan).Tile, !this.caravan.vehiclePather.Moving ? PlanetTile.Invalid : this.caravan.vehiclePather.NextTile, false, stringBuilder);
        this.cachedDestTilesPerDayExplanation = stringBuilder.ToString();
      }
      return this.cachedDestTilesPerDay;
    }
  }

  private Pair<float, float> DestDaysWorthOfFood
  {
    get
    {
      if (this.destDaysWorthOfFoodDirty)
      {
        this.destDaysWorthOfFoodDirty = false;
        float num1;
        float num2;
        if (this.caravan.vehiclePather.Moving)
        {
          num1 = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(this.transferables, ((WorldObject) this.caravan).Tile, (IgnorePawnsInventoryMode) 0, ((WorldObject) this.caravan).Faction, this.caravan.vehiclePather.curPath, this.caravan.vehiclePather.nextTileCostLeft, this.caravan.TicksPerMove);
          num2 = DaysUntilRotCalculator.ApproxDaysUntilRot(this.transferables, ((WorldObject) this.caravan).Tile, (IgnorePawnsInventoryMode) 0, this.caravan.vehiclePather.curPath, this.caravan.vehiclePather.nextTileCostLeft, this.caravan.TicksPerMove);
        }
        else
        {
          num1 = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(this.transferables, ((WorldObject) this.caravan).Tile, (IgnorePawnsInventoryMode) 0, ((WorldObject) this.caravan).Faction, (WorldPath) null, 0.0f, 3300);
          num2 = DaysUntilRotCalculator.ApproxDaysUntilRot(this.transferables, ((WorldObject) this.caravan).Tile, (IgnorePawnsInventoryMode) 0, (WorldPath) null, 0.0f, 3300);
        }
        this.cachedDestDaysWorthOfFood = new Pair<float, float>(num1, num2);
      }
      return this.cachedDestDaysWorthOfFood;
    }
  }

  private (ThingDef food, float perDay) DestForagedFoodPerDay
  {
    get
    {
      if (this.destForagedFoodPerDayDirty)
      {
        this.destForagedFoodPerDayDirty = false;
        StringBuilder stringBuilder = new StringBuilder();
        this.cachedDestForagedFoodPerDay = ForagedFoodPerDayCalculator.ForagedFoodPerDay(this.transferables, this.Biome, Faction.OfPlayer, stringBuilder);
        this.cachedDestForagedFoodPerDayExplanation = stringBuilder.ToString();
      }
      return this.cachedDestForagedFoodPerDay;
    }
  }

  private float DestVisibility
  {
    get
    {
      if (this.destVisibilityDirty)
      {
        this.destVisibilityDirty = false;
        StringBuilder stringBuilder = new StringBuilder();
        this.cachedDestVisibility = CaravanVisibilityCalculator.Visibility(this.transferables, stringBuilder);
        this.cachedDestVisibilityExplanation = stringBuilder.ToString();
      }
      return this.cachedDestVisibility;
    }
  }

  private int TicksToArrive
  {
    get
    {
      if (!this.caravan.vehiclePather.Moving)
        return 0;
      if (this.ticksToArriveDirty)
      {
        this.ticksToArriveDirty = false;
        this.cachedTicksToArrive = CaravanArrivalTimeEstimator.EstimatedTicksToArrive((Caravan) this.caravan, false);
      }
      return this.cachedTicksToArrive;
    }
  }

  public virtual void PostOpen()
  {
    base.PostOpen();
    this.CalculateAndRecacheTransferables();
  }

  public virtual void DoWindowContents(Rect inRect)
  {
    TextBlock textBlock1;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock1).\u002Ector((GameFont) 2, (TextAnchor) 4);
    try
    {
      Rect rect;
      // ISSUE: explicit constructor call
      ((Rect) ref rect).\u002Ector(0.0f, 0.0f, ((Rect) ref inRect).width, 35f);
      Widgets.Label(rect, Translator.Translate("VF_DockToCaravan"));
    }
    finally
    {
      textBlock1.Dispose();
    }
    TextBlock textBlock2;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock2).\u002Ector((GameFont) 1, (TextAnchor) 0);
    try
    {
      CaravanUIUtility.DrawCaravanInfo(new CaravanUIUtility.CaravanInfo(this.SourceMassUsage, this.SourceMassCapacity, this.cachedSourceMassCapacityExplanation, this.SourceTilesPerDay, this.cachedSourceTilesPerDayExplanation, this.SourceDaysWorthOfFood, this.SourceForagedFoodPerDay, this.cachedSourceForagedFoodPerDayExplanation, this.SourceVisibility, this.cachedSourceVisibilityExplanation, -1f, -1f, (string) null), new CaravanUIUtility.CaravanInfo?(), ((WorldObject) this.caravan).Tile, !this.caravan.vehiclePather.Moving ? new int?() : new int?(this.TicksToArrive), -9999f, new Rect(12f, 35f, ((Rect) ref inRect).width - 24f, 40f), true, (string) null, false, true);
      ref Rect local1 = ref inRect;
      ((Rect) ref local1).yMin = ((Rect) ref local1).yMin + 119f;
      Widgets.DrawMenuSection(inRect);
      TabDrawer.DrawTabs<TabRecord>(inRect, new List<TabRecord>(1)
      {
        new TabRecord(TaggedString.op_Implicit(Translator.Translate("ItemsTab")), (Action) null, true)
      }, 200f);
      inRect = GenUI.ContractedBy(inRect, 17f);
      Widgets.BeginGroup(inRect);
      Rect rect1 = GenUI.AtZero(inRect);
      this.DoBottomButtons(rect1);
      Rect rect2 = rect1;
      ref Rect local2 = ref rect2;
      ((Rect) ref local2).yMax = ((Rect) ref local2).yMax - 59f;
      bool flag;
      this.itemsTransfer.OnGUI(rect2, ref flag);
      if (flag)
        this.CountToTransferChanged();
      Widgets.EndGroup();
    }
    finally
    {
      textBlock2.Dispose();
    }
  }

  private void AddToTransferables(Thing t)
  {
    TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching<TransferableOneWay>(t, this.transferables, (TransferAsOneMode) 1);
    if (transferableOneWay == null)
    {
      transferableOneWay = new TransferableOneWay();
      this.transferables.Add(transferableOneWay);
    }
    transferableOneWay.things.Add(t);
  }

  private void DoBottomButtons(Rect rect)
  {
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector((float) ((double) ((Rect) ref rect).width / 2.0 - (double) this.BottomButtonSize.x / 2.0), ((Rect) ref rect).height - 55f, this.BottomButtonSize.x, this.BottomButtonSize.y);
    if (Widgets.ButtonText(rect1, TaggedString.op_Implicit(Translator.Translate("AcceptButton")), true, false, true, new TextAnchor?()) && this.TransferPawns())
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, (Map) null);
      this.Close(false);
    }
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).x - 10f - this.BottomButtonSize.x, ((Rect) ref rect1).y, this.BottomButtonSize.x, this.BottomButtonSize.y);
    if (Widgets.ButtonText(rect2, TaggedString.op_Implicit(Translator.Translate("ResetButton")), true, false, false, new TextAnchor?()))
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_Low, (Map) null);
      this.CalculateAndRecacheTransferables();
    }
    Rect rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector(((Rect) ref rect1).xMax + 10f, ((Rect) ref rect1).y, this.BottomButtonSize.x, this.BottomButtonSize.y);
    if (!Widgets.ButtonText(rect3, TaggedString.op_Implicit(Translator.Translate("CancelButton")), true, false, true, new TextAnchor?()))
      return;
    this.Close(true);
  }

  private void CalculateAndRecacheTransferables()
  {
    this.transferables = new List<TransferableOneWay>();
    this.AddItemsToTransferables();
    this.CreateCaravanItemsWidget(this.transferables, out this.itemsTransfer, TaggedString.op_Implicit(Translator.Translate("SplitCaravanThingCountTip")), (IgnorePawnsInventoryMode) 0, (Func<float>) (() => this.DestMassCapacity - this.DestMassUsage), false, PlanetTile.op_Implicit(((WorldObject) this.caravan).Tile));
    this.CountToTransferChanged();
  }

  private bool TransferPawns()
  {
    return StashedVehicle.Create(this.caravan, out Caravan _, this.transferables) != null;
  }

  private void CreateCaravanItemsWidget(
    List<TransferableOneWay> transferables,
    out TransferableOneWayWidget itemsTransfer,
    string thingCountTip,
    IgnorePawnsInventoryMode ignorePawnInventoryMass,
    Func<float> availableMassGetter,
    bool ignoreSpawnedCorpsesGearAndInventoryMass,
    int tile,
    bool playerPawnsReadOnly = false)
  {
    itemsTransfer = new TransferableOneWayWidget((IEnumerable<TransferableOneWay>) transferables, (string) null, (string) null, thingCountTip, true, ignorePawnInventoryMass, false, availableMassGetter, 0.0f, ignoreSpawnedCorpsesGearAndInventoryMass, new PlanetTile?(PlanetTile.op_Implicit(tile)), true, false, false, true, false, true, false, false, false, false);
  }

  private bool CheckForErrors(List<Pawn> pawns)
  {
    if (pawns.NotNullAndAny<Pawn>((Predicate<Pawn>) (x => CaravanUtility.IsOwner(x, Faction.OfPlayer) && !x.Downed)))
      return true;
    Messages.Message(TaggedString.op_Implicit(Translator.Translate("CaravanMustHaveAtLeastOneColonist")), LookTargets.op_Implicit((WorldObject) this.caravan), MessageTypeDefOf.RejectInput, false);
    return false;
  }

  private void AddItemsToTransferables()
  {
    foreach (Pawn pawn in this.caravan.VehiclesListForReading)
    {
      foreach (Thing t in pawn.inventory.innerContainer)
        this.AddToTransferables(t);
    }
  }

  private void CountToTransferChanged()
  {
    this.sourceMassUsageDirty = true;
    this.sourceMassCapacityDirty = true;
    this.sourceTilesPerDayDirty = true;
    this.sourceDaysWorthOfFoodDirty = true;
    this.sourceForagedFoodPerDayDirty = true;
    this.sourceVisibilityDirty = true;
    this.destMassUsageDirty = true;
    this.destMassCapacityDirty = true;
    this.destTilesPerDayDirty = true;
    this.destDaysWorthOfFoodDirty = true;
    this.destForagedFoodPerDayDirty = true;
    this.destVisibilityDirty = true;
    this.ticksToArriveDirty = true;
  }
}
