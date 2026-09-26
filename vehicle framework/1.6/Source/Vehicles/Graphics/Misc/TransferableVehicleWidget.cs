// Decompiled with JetBrains decompiler
// Type: Vehicles.World.TransferableVehicleWidget
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
using Vehicles.Config;
using Vehicles.Rendering;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles.World;

[StaticConstructorOnStartup]
public sealed class TransferableVehicleWidget
{
  private const float TopAreaHeight = 37f;
  private const int ColumnCount = 4;
  private const float CardHeight = 300f;
  private const float LabelHeight = 30f;
  private const float CardIconSize = 150f;
  private const float CardSpacing = 5f;
  private const float CardContentPadding = 5f;
  private const float FirstTransferableY = 6f;
  private const float ExtraSpaceAfterSectionTitle = 5f;
  private static readonly Texture2D PawnIcon = ContentFinder<Texture2D>.Get("UI/Icons/HostilityResponse/Flee", true);
  private static readonly Texture2D EfficiencyHillsIcon = ContentFinder<Texture2D>.Get("UI/Icons/EfficiencyHills", true);
  private static readonly Texture2D EfficiencyRiverIcon = ContentFinder<Texture2D>.Get("UI/Icons/EfficiencyRiver", true);
  private static readonly Texture2D EfficiencyRoadsIcon = ContentFinder<Texture2D>.Get("UI/Icons/EfficiencyRoads", true);
  private static readonly Rect SortersRect = new Rect(0.0f, 0.0f, 350f, 27f);
  private static readonly Color CardColor = new Color(1f, 1f, 1f, 0.04f);
  private static readonly List<TransferableSorterDef> AllSorterDefs = new List<TransferableSorterDef>();
  private static bool showVehicleProps = true;
  private readonly TransferableVehicleWidget.Section vehicleSection;
  private readonly List<TransferableOneWay> pawns;
  private readonly PlanetTile tile;
  private bool transferablesCached;
  private readonly TransferableVehicleWidget.TransferableSorter sortPrimary;
  private readonly TransferableVehicleWidget.TransferableSorter sortSecondary;
  private readonly HashSet<VehicleDef> impassableOnTile = new HashSet<VehicleDef>();
  private Vector2 scrollPosition;

  static TransferableVehicleWidget()
  {
    TransferableVehicleWidget.AllSorterDefs.Add(DefDatabase<TransferableSorterDef>.GetNamed("None", true));
    TransferableVehicleWidget.AllSorterDefs.Add(DefDatabase<TransferableSorterDef>.GetNamed("Name", true));
    TransferableVehicleWidget.AllSorterDefs.Add(TransferableSorterDefOf.MarketValue);
    TransferableVehicleWidget.AllSorterDefs.AddRange((IEnumerable<TransferableSorterDef>) DefDatabase<TransferableVehicleSorterDef>.AllDefsListForReading);
  }

  public TransferableVehicleWidget(
    string title,
    List<TransferableOneWay> vehicles,
    List<TransferableOneWay> pawns,
    PlanetTile tile = default (PlanetTile))
  {
    this.vehicleSection = new TransferableVehicleWidget.Section()
    {
      title = title,
      transferables = vehicles
    };
    this.pawns = pawns;
    this.tile = tile;
    this.sortPrimary = new TransferableVehicleWidget.TransferableSorter(this, (TransferableSorterDef) TransferableVehicleSorterDefOf.Type);
    this.sortSecondary = new TransferableVehicleWidget.TransferableSorter(this, (TransferableSorterDef) TransferableVehicleSorterDefOf.CargoCapacity);
    this.Init();
  }

  private float Height { get; set; } = -1f;

  private bool AnyTransferable
  {
    get
    {
      if (!this.transferablesCached)
        this.CacheTransferables();
      return this.vehicleSection.SortedTransferables.Count > 0;
    }
  }

  private void Init()
  {
    this.transferablesCached = false;
    if (GenList.NullOrEmpty<TransferableOneWay>((IList<TransferableOneWay>) this.vehicleSection.transferables))
      return;
    WorldVehiclePathGrid component = Find.World.GetComponent<WorldVehiclePathGrid>();
    foreach (Transferable transferable in this.vehicleSection.transferables)
    {
      VehicleDef thingDef = transferable.ThingDef as VehicleDef;
      if (!component.Passable(this.tile, thingDef))
        this.impassableOnTile.Add(thingDef);
    }
  }

  private void CacheTransferables()
  {
    this.transferablesCached = true;
    this.vehicleSection.SortedTransferables.Clear();
    this.vehicleSection.SortedTransferables.AddRange((IEnumerable<TransferableOneWay>) this.vehicleSection.transferables.OrderBy<TransferableOneWay, bool>((Func<TransferableOneWay, bool>) (transferableOneWay => !this.CanCaravan(transferableOneWay, out string _))).ThenBy<TransferableOneWay, Transferable>((Func<TransferableOneWay, Transferable>) (transferOneWay => (Transferable) transferOneWay), (IComparer<Transferable>) this.sortPrimary.sorterDef.Comparer).ThenBy<TransferableOneWay, Transferable>((Func<TransferableOneWay, Transferable>) (transferOneWay => (Transferable) transferOneWay), (IComparer<Transferable>) this.sortSecondary.sorterDef.Comparer));
    this.RecalculateHeight();
  }

  private void RecalculateHeight()
  {
    float num = 6f + (float) Mathf.CeilToInt((float) this.vehicleSection.SortedTransferables.Count / 4f) * 300f;
    if (this.vehicleSection.title != null)
      num += 35f;
    this.Height = num;
  }

  public void OnGUI(Rect inRect)
  {
    if (!this.transferablesCached)
      this.CacheTransferables();
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 1);
    try
    {
      TransferableVehicleWidget.DoTransferableSorters(this.sortPrimary.sorterDef, this.sortSecondary.sorterDef, new Action<TransferableSorterDef>(this.sortPrimary.Sort), new Action<TransferableSorterDef>(this.sortSecondary.Sort));
      Rect mainRect = new Rect(((Rect) ref inRect).x, ((Rect) ref inRect).y + 37f, ((Rect) ref inRect).width, ((Rect) ref inRect).height - 37f);
      if (FeatureFlags.IsFeatureEnabled("VehicleCaravanProps"))
      {
        string label = TaggedString.op_Implicit(Translator.Translate("VF_ShowVehicleProperties"));
        float x = Text.CalcSize(label).x;
        Rect rect;
        // ISSUE: explicit constructor call
        ((Rect) ref rect).\u002Ector((float) ((double) ((Rect) ref mainRect).xMax - (double) x - 24.0), ((Rect) ref mainRect).y, x, 37f);
        UIElements.CheckboxLabeled(rect, label, ref TransferableVehicleWidget.showVehicleProps);
      }
      this.FillMainRect(mainRect);
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  private bool CanCaravan(TransferableOneWay transferable, out string disableReason)
  {
    VehicleDef thingDef = ((Transferable) transferable).ThingDef as VehicleDef;
    if (this.impassableOnTile.Contains(thingDef))
    {
      disableReason = "VF_ImpassableBiome";
      return false;
    }
    ICaravanInfo current = CaravanFormation.Current;
    if (current == null || !current.AllowSelectionOfAllVehicles)
    {
      if (!thingDef.canCaravan)
      {
        disableReason = "VF_CaravanDisabled";
        return false;
      }
      if (((Transferable) transferable).AnyThing is VehiclePawn anyThing && !anyThing.CanMove)
      {
        disableReason = "VF_CaravanCantMove";
        return false;
      }
    }
    disableReason = (string) null;
    return true;
  }

  private void FillMainRect(Rect mainRect)
  {
    if (!this.AnyTransferable)
    {
      TextBlock textBlock;
      // ISSUE: explicit constructor call
      ((TextBlock) ref textBlock).\u002Ector((TextAnchor) 1, Color.gray);
      try
      {
        Widgets.Label(mainRect, Translator.Translate("NoneBrackets"));
      }
      finally
      {
        textBlock.Dispose();
      }
    }
    else
    {
      TextBlock textBlock;
      // ISSUE: explicit constructor call
      ((TextBlock) ref textBlock).\u002Ector((GameFont) 1);
      try
      {
        float num1 = 6f;
        float num2 = this.scrollPosition.y - 300f;
        float num3 = this.scrollPosition.y + ((Rect) ref mainRect).height;
        Rect rect1 = new Rect(0.0f, 0.0f, ((Rect) ref mainRect).width - 16f, this.Height);
        Widgets.BeginScrollView(mainRect, ref this.scrollPosition, rect1, true);
        float num4 = ((Rect) ref rect1).width / 4f;
        if (GenList.NullOrEmpty<TransferableOneWay>((IList<TransferableOneWay>) this.vehicleSection.SortedTransferables))
          return;
        if (this.vehicleSection.title != null)
        {
          Widgets.ListSeparator(ref num1, ((Rect) ref rect1).width, this.vehicleSection.title);
          num1 += 5f;
        }
        for (int index = 0; index < this.vehicleSection.SortedTransferables.Count; ++index)
        {
          TransferableOneWay sortedTransferable = this.vehicleSection.SortedTransferables[index];
          if ((double) num1 > (double) num2 && (double) num1 < (double) num3)
          {
            int num5 = index % 4;
            Rect rect2;
            // ISSUE: explicit constructor call
            ((Rect) ref rect2).\u002Ector((float) num5 * num4, num1, num4, 300f);
            Widgets.BeginGroup(rect2);
            rect2 = GenUI.ContractedBy(GenUI.AtZero(rect2), 2.5f);
            Widgets.DrawBoxSolidWithOutline(rect2, TransferableVehicleWidget.CardColor, Widgets.SeparatorLineColor, 1);
            this.DrawCard(GenUI.ContractedBy(rect2, 5f), this.vehicleSection, sortedTransferable);
            Widgets.EndGroup();
          }
          if ((index + 1) % 4 == 0)
            num1 += 300f;
        }
        Widgets.EndScrollView();
      }
      finally
      {
        textBlock.Dispose();
      }
    }
  }

  private void DrawCard(
    Rect rect,
    TransferableVehicleWidget.Section section,
    TransferableOneWay transferable)
  {
    VehiclePawn anyThing = ((Transferable) transferable).AnyThing as VehiclePawn;
    VehicleDef thingDef = ((Transferable) transferable).ThingDef as VehicleDef;
    string disableReason;
    bool flag1 = this.CanCaravan(transferable, out disableReason);
    Rect rect1 = rect;
    ((Rect) ref rect1).height = 150f;
    Rect rect2 = rect1;
    Rect square = rect2.ToSquare();
    string str = ((Entity) anyThing)?.LabelCap ?? TaggedString.op_Implicit(((Def) thingDef).LabelCap);
    bool flag2 = ((Transferable) transferable).CountToTransfer > 0;
    Rect rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector(((Rect) ref rect2).xMax - 24f, ((Rect) ref rect2).y, 24f, 24f);
    Widgets.Checkbox(((Rect) ref rect3).position, ref flag2, 24f, !flag1, false, (Texture2D) null, (Texture2D) null);
    if (!flag1)
      TooltipHandler.TipRegionByKey(rect3, disableReason);
    if (flag2 != ((Transferable) transferable).CountToTransfer > 0)
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
      if (flag2)
      {
        if (anyThing != null)
          Find.WindowStack.Add((Window) new Dialog_AssignSeats(CaravanFormation.Current, this.pawns, transferable));
        else
          ((Transferable) transferable).ForceTo(((Transferable) transferable).GetMaximumToTransfer());
      }
      else
      {
        ((Transferable) transferable).ForceTo(0);
        if (anyThing != null)
        {
          foreach (AssignedSeat assignment in CaravanHelper.assignedSeats.GetAssignments(anyThing))
          {
            AssignedSeat seat = assignment;
            TransferableOneWay transferableOneWay = GenCollection.FirstOrDefault<TransferableOneWay>(this.pawns, (Predicate<TransferableOneWay>) (trnsf => ((Transferable) trnsf).AnyThing == seat.pawn));
            if (transferableOneWay != null && !((Transferable) transferableOneWay).AnyThing.InVehicle())
              ((Transferable) transferableOneWay).ForceTo(0);
          }
          foreach (Pawn pawn1 in anyThing.AllPawnsAboard)
          {
            Pawn pawn = pawn1;
            ((Transferable) GenCollection.FirstOrDefault<TransferableOneWay>(this.pawns, (Predicate<TransferableOneWay>) (trnsf => ((Transferable) trnsf).AnyThing == pawn))).ForceTo(0);
          }
          CaravanHelper.assignedSeats.RemoveAssignments(anyThing);
          CaravanFormation.Current.NotifyTransferablesChanged();
        }
      }
    }
    if (TransferableVehicleWidget.showVehicleProps && FeatureFlags.IsFeatureEnabled("VehicleCaravanProps"))
      TransferableVehicleWidget.DrawSpecialProperties(rect, thingDef, anyThing);
    BlitRequest request = anyThing != null ? BlitRequest.For(anyThing) : BlitRequest.For(thingDef);
    VehicleGui.DrawVehicleOnGUI(square, in request);
    float num = Text.CalcHeight(str, ((Rect) ref rect2).width);
    Rect rect4;
    // ISSUE: explicit constructor call
    ((Rect) ref rect4).\u002Ector(((Rect) ref rect2).x, ((Rect) ref square).yMax - num, ((Rect) ref rect2).width, num);
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(new GameFont?((GameFont) 1), new TextAnchor?((TextAnchor) 1), new bool?(true));
    try
    {
      Widgets.Label(rect4, str);
    }
    finally
    {
      textBlock.Dispose();
    }
    Widgets.DrawLineHorizontal(((Rect) ref rect).x, ((Rect) ref square).yMax, ((Rect) ref rect).width, Widgets.SeparatorLineColor);
    Rect rect5 = rect;
    ((Rect) ref rect5).yMin = ((Rect) ref square).yMax;
    Rect infoRect = GenUI.ContractedBy(rect5, 15f, 0.0f);
    ref Rect local = ref infoRect;
    ((Rect) ref local).yMin = ((Rect) ref local).yMin + 10f;
    TransferableVehicleWidget.DrawVehicleInfo(infoRect, transferable);
  }

  private static void DrawVehicleInfo(Rect infoRect, TransferableOneWay transferable)
  {
    VehiclePawn anyThing = ((Transferable) transferable).AnyThing as VehiclePawn;
    VehicleDef thingDef = ((Transferable) transferable).ThingDef as VehicleDef;
    Rect rect1 = infoRect;
    ((Rect) ref rect1).height = Text.LineHeight;
    Rect rect2 = rect1;
    TransferableVehicleWidget.DrawMoveSpeed(rect2, transferable);
    ref Rect local1 = ref rect2;
    ((Rect) ref local1).y = ((Rect) ref local1).y + (((Rect) ref rect2).height + 2f);
    TransferableVehicleWidget.DrawCargoCapacity(rect2, transferable);
    ref Rect local2 = ref rect2;
    ((Rect) ref local2).y = ((Rect) ref local2).y + (((Rect) ref rect2).height + 2f);
    Tradeability tradeability = thingDef.tradeability;
    if (tradeability == 1 || tradeability == 3)
    {
      TransferableVehicleWidget.DrawMarketValue(rect2, transferable);
      ref Rect local3 = ref rect2;
      ((Rect) ref local3).y = ((Rect) ref local3).y + (((Rect) ref rect2).height + 2f);
    }
    if (anyThing == null)
      return;
    foreach (ThingComp allComp in ((ThingWithComps) anyThing).AllComps)
    {
      if (allComp is VehicleComp vehicleComp)
      {
        float num = vehicleComp.CompStatCard(rect2);
        if ((double) num > 0.0)
        {
          ref Rect local4 = ref rect2;
          ((Rect) ref local4).y = ((Rect) ref local4).y + num;
        }
      }
    }
  }

  private static void DrawSpecialProperties(Rect rect, VehicleDef vehicleDef, VehiclePawn vehicle)
  {
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x + 5f, ((Rect) ref rect).y + 5f, 32f, 32f);
    if (vehicle != null)
    {
      DrawIcon(ref rect1, TransferableVehicleWidget.EfficiencyHillsIcon, vehicle.PawnCountToOperate.ToString());
      DrawIcon(ref rect1, TransferableVehicleWidget.EfficiencyRiverIcon, vehicle.PawnsByHandlingType[HandlingType.None].Count.ToString());
      DrawIcon(ref rect1, TransferableVehicleWidget.EfficiencyRoadsIcon, vehicle.PawnsByHandlingType[HandlingType.None].Count.ToString());
    }
    else
    {
      int num1 = vehicleDef.properties.RoleSeats(HandlingType.Movement);
      int num2 = vehicleDef.properties.TotalSeats - num1;
      DrawIcon(ref rect1, VehicleTex.DraftVehicle, num1.ToString());
      DrawIcon(ref rect1, TransferableVehicleWidget.PawnIcon, num2.ToString());
    }

    static void DrawIcon(ref Rect rect, Texture2D icon, string tooltip)
    {
      GUI.DrawTexture(rect, (Texture) icon);
      TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tooltip));
      ref Rect local = ref rect;
      ((Rect) ref local).y = ((Rect) ref local).y + 37f;
    }
  }

  private static void DrawMoveSpeed(Rect rect, TransferableOneWay trad)
  {
    Widgets.DrawHighlightIfMouseover(rect);
    Rect rect1;
    Rect rect2;
    GenUI.SplitVertically(rect, ((Rect) ref rect).width / 2f, ref rect1, ref rect2);
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 1);
    try
    {
      TooltipHandler.TipRegionByKey(rect, "VF_Caravan_MoveSpeed");
      float num1;
      if (((Transferable) trad).AnyThing is VehiclePawn anyThing)
      {
        num1 = anyThing.statHandler.GetStatValue(VehicleStatDefOf.MoveSpeed) * anyThing.WorldSpeedMultiplier;
      }
      else
      {
        VehicleDef thingDef = ((Transferable) trad).ThingDef as VehicleDef;
        num1 = thingDef.GetStatValueAbstract(VehicleStatDefOf.MoveSpeed) * thingDef.properties.worldSpeedMultiplier;
      }
      float num2 = 0.0f;
      if ((double) num1 > 0.0)
        num2 = 60000f / (float) VehicleCaravanTicksPerMoveUtility.TicksFromMoveSpeed(num1 / 60f);
      using (new TextBlock((TextAnchor) 3))
        Widgets.Label(rect1, VehicleStatDefOf.MoveSpeed.LabelCap);
      using (new TextBlock((TextAnchor) 5))
        Widgets.Label(rect2, $"{num2:0.#} {Translator.Translate("TilesPerDay")}");
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  private static void DrawCargoCapacity(Rect rect, TransferableOneWay trad)
  {
    Widgets.DrawHighlightIfMouseover(rect);
    Rect rect1;
    Rect rect2;
    GenUI.SplitVertically(rect, ((Rect) ref rect).width / 2f, ref rect1, ref rect2);
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 1, (TextAnchor) 5);
    try
    {
      TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(VehicleStatDefOf.CargoCapacity.description));
      using (new TextBlock((TextAnchor) 3))
        Widgets.Label(rect1, VehicleStatDefOf.CargoCapacity.LabelCap);
      float num = ((Transferable) trad).AnyThing is VehiclePawn anyThing ? anyThing.statHandler.GetStatValue(VehicleStatDefOf.CargoCapacity) : (((Transferable) trad).ThingDef as VehicleDef).GetStatValueAbstract(VehicleStatDefOf.CargoCapacity);
      using (new TextBlock((TextAnchor) 5, (double) num > 0.0 ? Color.green : Color.gray))
        Widgets.Label(rect2, GenText.ToStringMassOffset(num));
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  private static void DrawMarketValue(Rect rect, TransferableOneWay trad)
  {
    Widgets.DrawHighlightIfMouseover(rect);
    Rect rect1;
    Rect rect2;
    GenUI.SplitVertically(rect, ((Rect) ref rect).width / 2f, ref rect1, ref rect2);
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 1, (TextAnchor) 5);
    try
    {
      TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(((Def) StatDefOf.MarketValue).description));
      using (new TextBlock((TextAnchor) 3))
        Widgets.Label(rect1, ((Def) StatDefOf.MarketValue).LabelCap);
      using (new TextBlock((TextAnchor) 5))
        Widgets.Label(rect2, GenText.ToStringMoney(((Transferable) trad).AnyThing.MarketValue, (string) null));
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  private static void DoTransferableSorters(
    TransferableSorterDef sorterPrimary,
    TransferableSorterDef sorterSecondary,
    Action<TransferableSorterDef> primarySetter,
    Action<TransferableSorterDef> secondarySetter)
  {
    Widgets.BeginGroup(TransferableVehicleWidget.SortersRect);
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 0);
    try
    {
      Rect rect1 = new Rect(0.0f, 0.0f, 60f, 27f);
      using (new TextBlock((TextAnchor) 3))
        Widgets.Label(rect1, Translator.Translate("SortBy"));
      Rect rect2 = new Rect(((Rect) ref rect1).xMax + 10f, 0.0f, 130f, 27f);
      if (Widgets.ButtonText(rect2, TaggedString.op_Implicit(GenText.Truncate(((Def) sorterPrimary).LabelCap, ((Rect) ref rect2).width - 2f, (Dictionary<string, TaggedString>) null)), true, true, true, new TextAnchor?()))
        OpenSorterChangeFloatMenu(primarySetter);
      ref Rect local = ref rect2;
      ((Rect) ref local).x = ((Rect) ref local).x + (((Rect) ref rect2).width + 10f);
      if (Widgets.ButtonText(rect2, TaggedString.op_Implicit(GenText.Truncate(((Def) sorterSecondary).LabelCap, ((Rect) ref rect2).width - 2f, (Dictionary<string, TaggedString>) null)), true, true, true, new TextAnchor?()))
        OpenSorterChangeFloatMenu(secondarySetter);
      Widgets.EndGroup();
    }
    finally
    {
      textBlock.Dispose();
    }

    static void OpenSorterChangeFloatMenu(Action<TransferableSorterDef> sorterSetter)
    {
      List<FloatMenuOption> floatMenuOptionList = new List<FloatMenuOption>();
      foreach (TransferableSorterDef allSorterDef in TransferableVehicleWidget.AllSorterDefs)
      {
        TransferableSorterDef sorterDef = allSorterDef;
        floatMenuOptionList.Add(new FloatMenuOption(TaggedString.op_Implicit(((Def) sorterDef).LabelCap), (Action) (() => sorterSetter(sorterDef)), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0));
      }
      Find.WindowStack.Add((Window) new FloatMenu(floatMenuOptionList));
    }
  }

  private class TransferableSorter(
    TransferableVehicleWidget widget,
    TransferableSorterDef sorterDef)
  {
    public TransferableSorterDef sorterDef = sorterDef;

    public void Sort(TransferableSorterDef def)
    {
      this.sorterDef = def;
      widget.CacheTransferables();
    }
  }

  private class Section
  {
    public string title;
    public List<TransferableOneWay> transferables;

    public List<TransferableOneWay> SortedTransferables { get; } = new List<TransferableOneWay>();
  }
}
