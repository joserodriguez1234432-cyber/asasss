// Decompiled with JetBrains decompiler
// Type: Vehicles.CompFueledTravel
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Vehicles.Rendering;
using Vehicles.World;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

[PublicAPI]
[StaticConstructorOnStartup]
[HeaderTitle(Label = "CompFueledTravel")]
public class CompFueledTravel : VehicleComp, IRefundable
{
  private const float FuelPerLeak = 1f;
  private const float TicksPerLeakCheck = 120f;
  private const float MaxTicksPerLeak = 400f;
  private const float EfficiencyTickMultiplier = 1.66666669E-05f;
  internal const float EfficiencyIdleMultiplier = 0.5f;
  private const float CellOffsetIntVec3ToVector3 = 0.5f;
  private const float TicksToCharge = 120f;
  private static readonly List<Thing> FuelToConsume = new List<Thing>();
  private static readonly Texture2D ElectricPowerTex = ContentFinder<Texture2D>.Get("UI/Overlays/NeedsPower", true);
  private static readonly MethodInfo PowerNetMethod = AccessTools.Method(typeof (PowerNet), "ChangeStoredEnergy", (System.Type[]) null, (System.Type[]) null);
  public bool allowAutoRefuel = true;
  private float fuel;
  private float targetFuelPercent = 1f;
  private bool terminateMotes;
  private Vector3 motePosition;
  private float offsetX;
  private float offsetZ;
  private Action<float> changeStoredEnergy;
  private CompPower connectedPower;
  private bool postLoadReconnect;
  private readonly Gizmo_RefuelableFuelTravel refuelGizmo;

  public CompFueledTravel() => this.refuelGizmo = new Gizmo_RefuelableFuelTravel(this, false);

  private List<(VehicleComponent component, Reactor_FuelLeak fuelLeak)> FuelComponents { get; set; }

  public bool FuelLeaking { get; private set; }

  public CompProperties_FueledTravel Props => this.props as CompProperties_FueledTravel;

  public override bool TickByRequest => true;

  public float Fuel => this.fuel;

  public float FuelPercent => this.Fuel / this.FuelCapacity;

  public bool EmptyTank => (double) this.Fuel <= 0.0;

  public bool FullTank => Mathf.Approximately(this.fuel, this.TargetFuelLevel);

  public int FuelCountToFull => Mathf.CeilToInt(this.TargetFuelLevel - this.Fuel);

  private int FuelToEject => Mathf.FloorToInt(this.Fuel - this.TargetFuelLevel);

  public bool CanEjectFuel => this.FuelToEject > 0;

  public float TargetFuelPercent
  {
    get => this.targetFuelPercent;
    set => this.targetFuelPercent = value;
  }

  public float TargetFuelLevel => this.targetFuelPercent * this.FuelCapacity;

  private float FuelPercentOfTarget
  {
    get => (double) this.TargetFuelLevel != 0.0 ? this.fuel / this.TargetFuelLevel : 0.0f;
  }

  public float ConsumptionRatePerTickRaw => this.FuelEfficiency * 1.66666669E-05f;

  public float ConsumptionRatePerTick
  {
    get
    {
      VehiclePawn vehicle = this.Vehicle;
      if (vehicle != null && ((Thing) vehicle).Spawned)
      {
        VehiclePathFollower vehiclePather = vehicle.vehiclePather;
        if (vehiclePather != null && !vehiclePather.Moving && (this.FuelCondition & FuelConsumptionCondition.Drafted) != (FuelConsumptionCondition) 0 && (this.FuelCondition & FuelConsumptionCondition.Moving) != (FuelConsumptionCondition) 0)
          return this.ConsumptionRatePerTickRaw * 0.5f;
      }
      return this.ConsumptionRatePerTickRaw;
    }
  }

  public float ConsumptionRateWorldPerTick
  {
    get => this.ConsumptionRatePerTick * this.Props.fuelConsumptionWorldMultiplier;
  }

  public FuelConsumptionCondition FuelCondition => this.Props.fuelConsumptionCondition;

  public bool ShouldAutoRefuelNow
  {
    get
    {
      return (double) this.FuelPercentOfTarget <= (double) this.Props.autoRefuelPercent && !this.FullTank && (double) this.TargetFuelLevel > 0.0 && this.ShouldAutoRefuelNowIgnoringFuelPct;
    }
  }

  private bool ShouldAutoRefuelNowIgnoringFuelPct
  {
    get
    {
      return this.allowAutoRefuel && !this.Vehicle.Drafted && !FireUtility.IsBurning((Thing) this.Vehicle) && ((Thing) this.parent).Map.designationManager.DesignationOn((Thing) this.Vehicle, DesignationDefOf_Vehicles.DisassembleVehicle) == null;
    }
  }

  public bool Charging
  {
    get
    {
      return this.connectedPower != null && !this.FullTank && (double) this.connectedPower.PowerNet.CurrentStoredEnergy() > (double) this.Props.chargeRate;
    }
  }

  public IEnumerable<(ThingDef thingDef, float count)> Refunds
  {
    get
    {
      if (!this.Props.ElectricPowered)
        yield return (this.Props.fuelType, this.Fuel);
    }
  }

  protected virtual float ChargeRate
  {
    get
    {
      float num = SettingsCache.TryGetValue<float>(this.Vehicle.VehicleDef, typeof (CompProperties_FueledTravel), "chargeRate", this.Props.chargeRate);
      return this.Vehicle.statHandler.GetStatOffset(VehicleStatUpgradeCategoryDefOf.ChargeRate, num);
    }
  }

  protected virtual float DischargeRate
  {
    get
    {
      float num = SettingsCache.TryGetValue<float>(this.Vehicle.VehicleDef, typeof (CompProperties_FueledTravel), "dischargeRate", this.Props.dischargeRate);
      return this.Vehicle.statHandler.GetStatOffset(VehicleStatUpgradeCategoryDefOf.DischargeRate, num);
    }
  }

  public virtual float FuelEfficiency
  {
    get
    {
      float num = SettingsCache.TryGetValue<float>(this.Vehicle.VehicleDef, typeof (CompProperties_FueledTravel), "fuelConsumptionRate", this.Props.fuelConsumptionRate);
      return this.Vehicle.statHandler.GetStatOffset(VehicleStatUpgradeCategoryDefOf.FuelConsumptionRate, num);
    }
  }

  public virtual float FuelCapacity
  {
    get
    {
      float num = (float) SettingsCache.TryGetValue<int>(this.Vehicle.VehicleDef, typeof (CompProperties_FueledTravel), "fuelCapacity", this.Props.fuelCapacity);
      return this.Vehicle.statHandler.GetStatOffset(VehicleStatUpgradeCategoryDefOf.FuelCapacity, num);
    }
  }

  private bool ShouldConsumeNow
  {
    get
    {
      if (this.EmptyTank || !((Thing) this.Vehicle).Spawned)
        return false;
      return this.ConsumeWhenDrafted || this.ConsumeWhenMoving || this.ConsumeAlways;
    }
  }

  private bool ConsumeAlways => (this.FuelCondition & FuelConsumptionCondition.Always) != 0;

  private bool ConsumeWhenDrafted
  {
    get
    {
      return ((Thing) this.Vehicle).Spawned && (this.FuelCondition & FuelConsumptionCondition.Drafted) != (FuelConsumptionCondition) 0 && this.Vehicle.Drafted;
    }
  }

  private bool ConsumeWhenMoving
  {
    get
    {
      if ((this.FuelCondition & FuelConsumptionCondition.Moving) != (FuelConsumptionCondition) 0)
      {
        if (((Thing) this.Vehicle).Spawned && this.Vehicle.vehiclePather.Moving)
          return true;
        VehicleCaravan vehicleCaravan = this.Vehicle.GetVehicleCaravan();
        if (vehicleCaravan != null && vehicleCaravan.vehiclePather.MovingNow)
          return true;
      }
      return false;
    }
  }

  public virtual Thing ClosestFuelAvailable(Pawn pawn)
  {
    return this.Props.ElectricPowered ? (Thing) null : GenClosest.ClosestThingReachable(((Thing) pawn).Position, ((Thing) pawn).Map, ThingRequest.ForDef(this.Props.fuelType), (PathEndMode) 3, TraverseParms.For(pawn, (Danger) 3, (TraverseMode) 0, false, false, false, true), 9999f, new Predicate<Thing>(Validator), (IEnumerable<Thing>) null, 0, -1, false, (RegionType) 14, false, false);

    bool Validator(Thing thing)
    {
      return !ForbidUtility.IsForbidden(thing, pawn) && ReservationUtility.CanReserve(pawn, LocalTargetInfo.op_Implicit(thing), 1, -1, (ReservationLayerDef) null, false) && thing.def == this.Props.fuelType;
    }
  }

  public override AcceptanceReport CanMove(FloatMenuContext context)
  {
    return this.EmptyTank ? AcceptanceReport.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_OutOfFuel", NamedArgument.op_Implicit((Thing) this.Vehicle))) : AcceptanceReport.op_Implicit(true);
  }

  public override AcceptanceReport CanDraft()
  {
    return this.EmptyTank ? AcceptanceReport.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_OutOfFuel", NamedArgument.op_Implicit((Thing) this.Vehicle))) : AcceptanceReport.op_Implicit(true);
  }

  public virtual void Refuel(List<Thing> fuelThings)
  {
    int amount;
    for (int fuelCountToFull = this.FuelCountToFull; fuelCountToFull > 0 && fuelThings.Count > 0; fuelCountToFull -= amount)
    {
      Thing thing = GenCollection.Pop<Thing>(fuelThings);
      amount = Mathf.Min(fuelCountToFull, thing.stackCount);
      this.Refuel((float) amount);
      thing.SplitOff(amount).Destroy((DestroyMode) 0);
    }
  }

  public void ConsumeFuelFromInventory(int count)
  {
    this.Refuel((float) count);
    foreach (Thing thing1 in CompFueledTravel.AllFuelFromInventory(this.Vehicle))
    {
      Thing thing2 = thing1.SplitOff(Mathf.Min(count, thing1.stackCount));
      count -= thing2.stackCount;
      if (count <= 0)
        break;
    }
  }

  public static IEnumerable<Thing> AllFuelFromInventory(VehiclePawn vehicle)
  {
    CompProperties_FueledTravel props = vehicle.CompFueledTravel.Props;
    VehicleCaravan vehicleCaravan = vehicle.GetVehicleCaravan();
    if (vehicleCaravan != null)
    {
      foreach (Thing allThing in vehicleCaravan.AllThings)
      {
        if (allThing.def == props.fuelType)
          yield return allThing;
      }
    }
    else if (((Thing) vehicle).Spawned)
    {
      foreach (Thing thing in vehicle.inventory.innerContainer.InnerListForReading)
      {
        if (thing.def == props.fuelType)
          yield return thing;
      }
    }
  }

  public virtual void Refuel(float amount)
  {
    if ((double) amount <= 0.0)
      throw new ArgumentException("Refuel amount must be greater than 0.", nameof (amount));
    this.fuel = Mathf.Clamp(this.fuel + amount, 0.0f, this.FuelCapacity);
    this.Vehicle.EventRegistry?[VehicleEventDefOf.Refueled].ExecuteEvents();
  }

  private void RefuelHalfway()
  {
    this.ConsumeFuel(float.MaxValue);
    this.Refuel(this.FuelCapacity / 2f);
  }

  public virtual void ConsumeFuel(float amount)
  {
    if (Mathf.Approximately(this.fuel, 0.0f))
      return;
    this.fuel = Mathf.Clamp(this.fuel - amount, 0.0f, this.FuelCapacity);
    if (!Mathf.Approximately(this.fuel, 0.0f))
      return;
    this.Vehicle.EventRegistry[VehicleEventDefOf.OutOfFuel].ExecuteEvents();
  }

  public virtual void ConsumeFuelWorld()
  {
    if ((double) this.fuel <= 0.0)
      return;
    float rateWorldPerTick = this.ConsumptionRateWorldPerTick;
    if (!this.Vehicle.GetVehicleCaravan().vehiclePather.Moving)
      rateWorldPerTick *= 0.5f;
    this.fuel -= rateWorldPerTick;
    if ((double) this.fuel > 0.0)
      return;
    this.fuel = 0.0f;
    this.Vehicle.EventRegistry[VehicleEventDefOf.OutOfFuel].ExecuteEvents();
  }

  public void EjectFuel()
  {
    int fuelToEject = this.FuelToEject;
    while (fuelToEject > 0)
    {
      Thing thing = ThingMaker.MakeThing(this.Props.fuelType, (ThingDef) null);
      thing.stackCount = Mathf.Min(fuelToEject, this.Props.fuelType.stackLimit);
      fuelToEject -= thing.stackCount;
      this.fuel -= (float) thing.stackCount;
      GenPlace.TryPlaceThing(thing, ((Thing) this.parent).Position, ((Thing) this.parent).Map, (ThingPlaceMode) 1, (Action<Thing, int>) null, (Predicate<IntVec3>) null, new Rot4?(), 1);
      ForbidUtility.SetForbidden(thing, true, true);
    }
    if ((double) this.fuel != 0.0)
      return;
    this.Vehicle.EventRegistry[VehicleEventDefOf.OutOfFuel].ExecuteEvents();
  }

  public virtual void PostDraw()
  {
    base.PostDraw();
    if (!this.EmptyTank)
      return;
    ((Thing) this.parent).Map.overlayDrawer.DrawOverlay((Thing) this.parent, this.Props.ElectricPowered ? (OverlayTypes) 1 : (OverlayTypes) 128 /*0x80*/);
  }

  public override float CompStatCard(Rect rect)
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
      float rateWorldPerTick = this.ConsumptionRateWorldPerTick;
      float num = 0.0f;
      if ((double) rateWorldPerTick > 0.0)
        num = rateWorldPerTick * 60000f;
      using (new TextBlock((TextAnchor) 3))
        Widgets.Label(rect1, this.Props.GizmoLabel);
      using (new TextBlock((TextAnchor) 5))
      {
        string str = TaggedString.op_Implicit(Translator.Translate("VF_PerDay"));
        float x = Verse.Text.CalcSize(str).x;
        Rect rect3 = rect2;
        ((Rect) ref rect3).xMin = ((Rect) ref rect3).xMax - x;
        Widgets.Label(rect3, str);
        Rect rect4 = rect2;
        ((Rect) ref rect4).width = ((Rect) ref rect2).height;
        Rect rect5 = rect4;
        ((Rect) ref rect5).x = ((Rect) ref rect3).x - ((Rect) ref rect5).width;
        if (this.Props.ElectricPowered)
        {
          GUI.DrawTexture(rect5, (Texture) CompFueledTravel.ElectricPowerTex);
        }
        else
        {
          Widgets.DefIcon(rect5, (Def) this.Props.fuelType, (ThingDef) null, 1f, (ThingStyleDef) null, false, new Color?(), (Material) null, new int?(), 1f);
          TooltipHandler.TipRegion(rect5, TipSignal.op_Implicit(((Def) this.Props.fuelType).LabelCap));
        }
        ((Rect) ref rect2).xMax = ((Rect) ref rect5).xMin;
        Widgets.Label(rect2, $"{num:0.#}");
      }
      return Verse.Text.LineHeight;
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  public virtual IEnumerable<Gizmo> CompGetGizmosExtra()
  {
    foreach (Gizmo gizmo in base.CompGetGizmosExtra())
      yield return gizmo;
    if (Find.Selector.SelectedObjects.Count == 1)
      yield return (Gizmo) this.refuelGizmo;
    if (DebugSettings.ShowDevGizmos)
    {
      foreach (Gizmo devModeGizmo in this.DevModeGizmos())
        yield return devModeGizmo;
    }
  }

  public override IEnumerable<Gizmo> CompCaravanGizmos()
  {
    CompFueledTravel refuelable = this;
    yield return (Gizmo) new Gizmo_RefuelableFuelTravel(refuelable, true);
    if (DebugSettings.ShowDevGizmos)
    {
      Command_Action commandAction1 = new Command_Action();
      ((Command) commandAction1).defaultLabel = $"Vehicle Dev: [{((Entity) refuelable.Vehicle).Label}] Set fuel to 0.";
      // ISSUE: reference to a compiler-generated method
      commandAction1.action = new Action(refuelable.\u003CCompCaravanGizmos\u003Eb__102_0);
      yield return (Gizmo) commandAction1;
      Command_Action commandAction2 = new Command_Action();
      ((Command) commandAction2).defaultLabel = $"Vehicle Dev: [{((Entity) refuelable.Vehicle).Label}] Set fuel to max.";
      // ISSUE: reference to a compiler-generated method
      commandAction2.action = new Action(refuelable.\u003CCompCaravanGizmos\u003Eb__102_1);
      yield return (Gizmo) commandAction2;
    }
  }

  public virtual IEnumerable<Gizmo> DevModeGizmos()
  {
    CompFueledTravel compFueledTravel = this;
    Command_Action commandAction1 = new Command_Action();
    ((Command) commandAction1).defaultLabel = "Debug: Set fuel to 0";
    // ISSUE: reference to a compiler-generated method
    commandAction1.action = new Action(compFueledTravel.\u003CDevModeGizmos\u003Eb__103_0);
    yield return (Gizmo) commandAction1;
    Command_Action commandAction2 = new Command_Action();
    ((Command) commandAction2).defaultLabel = "Debug: Set fuel to half";
    commandAction2.action = new Action(compFueledTravel.RefuelHalfway);
    yield return (Gizmo) commandAction2;
    Command_Action commandAction3 = new Command_Action();
    ((Command) commandAction3).defaultLabel = "Debug: Set fuel to max";
    // ISSUE: reference to a compiler-generated method
    commandAction3.action = new Action(compFueledTravel.\u003CDevModeGizmos\u003Eb__103_1);
    yield return (Gizmo) commandAction3;
    Command_Action commandAction4 = new Command_Action();
    ((Command) commandAction4).defaultLabel = "Debug: Set fuel to 99.99%";
    // ISSUE: reference to a compiler-generated method
    commandAction4.action = new Action(compFueledTravel.\u003CDevModeGizmos\u003Eb__103_2);
    yield return (Gizmo) commandAction4;
  }

  public virtual IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
  {
    yield return new FloatMenuOption(Translator.Translate("Refuel").ToString(), (Action) (() => selPawn.jobs.TryTakeOrderedJob(new Job(JobDefOf_Vehicles.RefuelVehicle, LocalTargetInfo.op_Implicit((Thing) this.parent), LocalTargetInfo.op_Implicit(this.ClosestFuelAvailable(selPawn))), new JobTag?((JobTag) 6), false)), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0);
  }

  public override void CompCaravanInspectString(StringBuilder stringBuilder)
  {
    if (!this.EmptyTank)
      return;
    stringBuilder.AppendLine(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_OutOfFuel", NamedArgument.op_Implicit((Thing) this.Vehicle))));
  }

  private void RevalidateConsumptionStatus()
  {
    if (this.ShouldConsumeNow || this.Charging || this.FuelLeaking)
      this.StartTicking();
    else
      this.StopTicking();
  }

  protected void ChangeStoredEnergy(float extra)
  {
    if (this.changeStoredEnergy == null && this.connectedPower.PowerNet != null)
      this.changeStoredEnergy = AccessTools.MethodDelegate<Action<float>>(CompFueledTravel.PowerNetMethod, (object) this.connectedPower.PowerNet, false, new System.Type[1]
      {
        typeof (float)
      });
    Action<float> changeStoredEnergy = this.changeStoredEnergy;
    if (changeStoredEnergy == null)
      return;
    changeStoredEnergy(extra);
  }

  [Profile]
  public virtual void CompTick()
  {
    if (this.FuelLeaking)
      this.LeakTick();
    if (!this.ShouldConsumeNow)
      return;
    this.ConsumeFuel(this.ConsumptionRatePerTick);
    if (!this.terminateMotes && !GenList.NullOrEmpty<OffsetMote>((IList<OffsetMote>) this.Props.motesGenerated) && Find.TickManager.TicksGame % this.Props.ticksToSpawnMote == 0)
      this.DrawMotes();
    if (this.EmptyTank && !VehicleMod.settings.debug.debugDraftAnyVehicle)
      this.Vehicle.ignition.Drafted = false;
    if (!this.Props.ElectricPowered)
      return;
    if (!this.Charging)
    {
      this.ConsumeFuel(Mathf.Min(this.DischargeRate * 1.66666669E-05f, this.Fuel));
    }
    else
    {
      if ((double) Find.TickManager.TicksGame % 120.0 != 0.0)
        return;
      this.ChangeStoredEnergy(-this.ChargeRate);
      this.Refuel(this.ChargeRate);
    }
  }

  private void LeakTick()
  {
    foreach ((VehicleComponent component, Reactor_FuelLeak fuelLeak) in this.FuelComponents)
    {
      if (Find.TickManager.TicksGame % CompFueledTravel.TicksPerLeak(component.HealthPercent, fuelLeak.healthPercent, fuelLeak.rate) == 0)
      {
        this.ConsumeFuel(1f);
        if (((Thing) this.Vehicle).Spawned && this.Props.leakDef != null && !this.EmptyTank)
        {
          IntVec2 intVec2 = GenCollection.RandomElementWithFallback<IntVec2>((IEnumerable<IntVec2>) component.props.hitbox.cells, IntVec2.Zero);
          IntVec3 intVec3;
          // ISSUE: explicit constructor call
          ((IntVec3) ref intVec3).\u002Ector(((Thing) this.Vehicle).Position.x + intVec2.x, 0, ((Thing) this.Vehicle).Position.z + intVec2.z);
          FilthMaker.TryMakeFilth(intVec3, ((Thing) this.Vehicle).Map, this.Props.leakDef, 1, (FilthSourceFlags) 0, true);
        }
      }
    }
  }

  public static int TicksPerLeak(float healthPercent, float fuelLeakPercent, FloatRange leakRate)
  {
    if ((double) fuelLeakPercent <= 0.0)
      return -1;
    float num1 = (float) (((double) fuelLeakPercent - (double) healthPercent) * (1.0 / (double) fuelLeakPercent));
    if ((double) num1 < 0.0)
      return -1;
    float num2 = Mathf.Lerp(leakRate.min, leakRate.max, num1);
    return (double) num2 <= 0.0 ? -1 : Mathf.CeilToInt(60f / num2);
  }

  public virtual void CompTickRare()
  {
    base.CompTickRare();
    this.RevalidateConsumptionStatus();
    if (!((Thing) this.Vehicle).Spawned)
      return;
    VehicleReservationManager cachedMapComponent = ((Thing) this.Vehicle).Map.GetCachedMapComponent<VehicleReservationManager>();
    if (!this.FullTank)
      cachedMapComponent.RegisterLister(this.Vehicle, "Refuel");
    else
      cachedMapComponent.RemoveLister(this.Vehicle, "Refuel");
    if (this.CanEjectFuel)
      cachedMapComponent.RegisterLister(this.Vehicle, "RemoveFuel");
    else
      cachedMapComponent.RemoveLister(this.Vehicle, "RemoveFuel");
    if (!Mathf.Approximately(this.Props.ambientHeat, 0.0f))
      GenTemperature.PushHeat((Thing) this.Vehicle, this.Props.ambientHeat);
    if (!this.Vehicle.vehiclePather.Moving)
      return;
    this.DisconnectPower();
  }

  public override void OnDeSpawn() => this.DisconnectPower();

  public bool TryConnectPower()
  {
    if (!this.Props.ElectricPowered)
      return false;
    this.Vehicle.RequestTickStart<CompFueledTravel>(this);
    foreach (IntVec3 inhabitedCell in this.Vehicle.InhabitedCells(1))
    {
      Thing thing = ((Thing) this.Vehicle).Map.thingGrid.ThingAt(inhabitedCell, (ThingCategory) 3);
      CompPower comp = thing != null ? ThingCompUtility.TryGetComp<CompPower>(thing) : (CompPower) null;
      if (comp != null && comp.TransmitsPowerNow)
      {
        this.connectedPower = comp;
        return true;
      }
    }
    return false;
  }

  public void DisconnectPower()
  {
    this.connectedPower = (CompPower) null;
    this.changeStoredEnergy = (Action<float>) null;
  }

  protected virtual void DrawMotes()
  {
    if (GenList.NullOrEmpty<OffsetMote>((IList<OffsetMote>) this.Props.motesGenerated))
      return;
    foreach (OffsetMote offsetMote in this.Props.motesGenerated)
    {
      for (int index = 0; index < offsetMote.NumTimesSpawned; ++index)
      {
        try
        {
          Vector2 vector2 = VehicleGraphics.VehicleDrawOffset(this.Vehicle.FullRotation, offsetMote.xOffset, offsetMote.zOffset);
          this.offsetX = vector2.x;
          this.offsetZ = vector2.y;
          this.motePosition = new Vector3((float) ((double) ((Thing) this.parent).Position.x + (double) this.offsetX + 0.5), (float) ((Thing) this.parent).Position.y, (float) ((double) ((Thing) this.parent).Position.z + (double) this.offsetZ + 0.5));
          MoteThrown mote = (MoteThrown) ThingMaker.MakeThing(this.Props.moteDisplayed, (ThingDef) null);
          ((Mote) mote).exactPosition = this.motePosition;
          ((Mote) mote).Scale = 1f;
          ((Mote) mote).rotationRate = 15f;
          float valueOrDefault = offsetMote.predeterminedAngleVector.GetValueOrDefault();
          float num = offsetMote.windAffected ? Rand.Range(0.5f, 3.5f) * ((Thing) this.Vehicle).Map.windManager.WindSpeed : offsetMote.moteThrownSpeed;
          mote.SetVelocity(valueOrDefault, num);
          MoteGenerator.ThrowMote(IntVec3Utility.ToIntVec3(this.motePosition), ((Thing) this.parent).Map, mote);
        }
        catch (Exception ex)
        {
          Log.Error($"Exception thrown while trying to display {((Def) this.Props.moteDisplayed).defName}.\n{ex}");
          this.terminateMotes = true;
          return;
        }
      }
    }
  }

  private void RevalidateFuelLeakage()
  {
    if (GenList.NullOrEmpty<(VehicleComponent, Reactor_FuelLeak)>((IList<(VehicleComponent, Reactor_FuelLeak)>) this.FuelComponents))
      return;
    bool fuelLeaking = this.FuelLeaking;
    this.FuelLeaking = false;
    foreach ((VehicleComponent component, Reactor_FuelLeak fuelLeak) fuelComponent in this.FuelComponents)
      this.FuelLeaking |= (double) fuelComponent.component.HealthPercent <= (double) fuelComponent.fuelLeak.healthPercent;
    if (this.FuelLeaking == fuelLeaking)
      return;
    this.RevalidateConsumptionStatus();
  }

  public override void EventRegistration()
  {
    this.FuelComponents = new List<(VehicleComponent, Reactor_FuelLeak)>();
    foreach (VehicleComponent vehicleComponent in this.Vehicle.statHandler.components.Where<VehicleComponent>((Func<VehicleComponent, bool>) (component => component.props.HasReactor<Reactor_FuelLeak>())))
    {
      if (vehicleComponent.props.HasReactor<Reactor_FuelLeak>())
        this.FuelComponents.Add((vehicleComponent, vehicleComponent.props.GetReactor<Reactor_FuelLeak>()));
    }
    this.Vehicle.AddEvent<VehicleEventDef>(VehicleEventDefOf.MoveStart, new Action(this.RevalidateConsumptionStatus));
    this.Vehicle.AddEvent<VehicleEventDef>(VehicleEventDefOf.MoveStop, new Action(this.RevalidateConsumptionStatus));
    this.Vehicle.AddEvent<VehicleEventDef>(VehicleEventDefOf.OutOfFuel, new Action(this.RevalidateConsumptionStatus));
    this.Vehicle.AddEvent<VehicleEventDef>(VehicleEventDefOf.Refueled, new Action(this.RevalidateConsumptionStatus));
    this.Vehicle.AddEvent<VehicleEventDef>(VehicleEventDefOf.IgnitionOn, new Action(this.RevalidateConsumptionStatus));
    this.Vehicle.AddEvent<VehicleEventDef>(VehicleEventDefOf.IgnitionOff, new Action(this.RevalidateConsumptionStatus));
    this.Vehicle.AddEvent<VehicleEventDef>(VehicleEventDefOf.HealthChanged, new Action(this.RevalidateFuelLeakage));
  }

  public override void PostGeneration()
  {
    base.PostGeneration();
    this.targetFuelPercent = 1f;
    if (((Thing) this.Vehicle).Faction == Faction.OfPlayer)
      return;
    this.Refuel(this.FuelCapacity * Rand.Range(0.45f, 0.85f));
  }

  public virtual void PostSpawnSetup(bool respawningAfterLoad)
  {
    base.PostSpawnSetup(respawningAfterLoad);
    this.RevalidateConsumptionStatus();
    if (!this.postLoadReconnect)
      return;
    this.TryConnectPower();
  }

  public virtual void PostExposeData()
  {
    base.PostExposeData();
    Scribe_Values.Look<bool>(ref this.allowAutoRefuel, "allowAutoRefuel", true, false);
    Scribe_Values.Look<float>(ref this.fuel, "fuel", 0.0f, false);
    Scribe_Values.Look<float>(ref this.targetFuelPercent, "targetFuelPercent", 1f, false);
    if (Scribe.mode == 1)
      this.postLoadReconnect = this.Charging;
    Scribe_Values.Look<bool>(ref this.postLoadReconnect, "postLoadReconnect", false, false);
  }
}
