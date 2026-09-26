// Decompiled with JetBrains decompiler
// Type: Vehicles.CompVehicleLauncher
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Targeting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Vehicles.World;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
[StaticConstructorOnStartup]
[HeaderTitle(Label = "CompVehicleLauncher")]
public class CompVehicleLauncher : 
  VehicleComp,
  ILauncher,
  ITargeterSource<GlobalTargetInfo, ArrivalOption>
{
  private static readonly List<ArrivalOption> ArrivalOptions = new List<ArrivalOption>();
  private static readonly SimpleCurve ClimbRateCurve;
  [GraphEditable]
  public LaunchProtocol launchProtocol;
  public float fuelEfficiencyWorldModifier;
  public float flightSpeedModifier;
  public bool? signalJammer;
  public float rateOfClimbModifier;
  public int maxAltitudeModifier;
  public int landingAltitudeModifier;
  private CompVehicleLauncher.DeploymentTimer timer;
  public bool loiter;
  public bool inFlight;
  private Command_ActionHighlighter takeoffCommand;

  public bool AnyLeftToLoad
  {
    get
    {
      return !GenList.NullOrEmpty<TransferableOneWay>((IList<TransferableOneWay>) this.Vehicle.cargoToLoad);
    }
  }

  public CompProperties_VehicleLauncher Props => this.props as CompProperties_VehicleLauncher;

  bool ITargeterSource<GlobalTargetInfo, ArrivalOption>.TargeterValid
  {
    get => ((Thing) this.Vehicle).Spawned && !((Thing) this.Vehicle).Destroyed;
  }

  public PlanetTile Tile
  {
    get
    {
      if (((Thing) this.Vehicle).Spawned)
        return ((Thing) this.Vehicle).Map.Tile;
      return !(((Thing) this.Vehicle).ParentHolder is WorldObject parentHolder) ? PlanetTile.Invalid : parentHolder.Tile;
    }
  }

  public Vector3 Origin
  {
    get
    {
      return ((Thing) this.Vehicle).ParentHolder is ILauncher parentHolder ? parentHolder.Origin : WorldHelper.GetTilePos(this.Tile);
    }
  }

  public int MaxLaunchDistance => this.FixedMaxDistance <= 0 ? int.MaxValue : this.FixedMaxDistance;

  public float FlightSpeed
  {
    get => this.flightSpeedModifier + this.Vehicle.GetStatValue(VehicleStatDefOf.FlightSpeed);
  }

  public float FuelConsumptionWorldMultiplier
  {
    get
    {
      return this.fuelEfficiencyWorldModifier + SettingsCache.TryGetValue<float>(this.Vehicle.VehicleDef, typeof (CompProperties_VehicleLauncher), "fuelConsumptionWorldMultiplier", this.Props.fuelConsumptionWorldMultiplier);
    }
  }

  public int FixedMaxDistance
  {
    get
    {
      return SettingsCache.TryGetValue<int>(this.Vehicle.VehicleDef, typeof (CompProperties_VehicleLauncher), "fixedLaunchDistanceMax", this.Props.fixedLaunchDistanceMax);
    }
  }

  public float RateOfClimb
  {
    get
    {
      return this.rateOfClimbModifier + SettingsCache.TryGetValue<float>(this.Vehicle.VehicleDef, typeof (CompProperties_VehicleLauncher), "rateOfClimb", this.Props.rateOfClimb);
    }
  }

  public int MaxAltitude
  {
    get
    {
      return this.maxAltitudeModifier + SettingsCache.TryGetValue<int>(this.Vehicle.VehicleDef, typeof (CompProperties_VehicleLauncher), "maxAltitude", this.Props.maxAltitude);
    }
  }

  public int LandingAltitude
  {
    get
    {
      return this.landingAltitudeModifier + SettingsCache.TryGetValue<int>(this.Vehicle.VehicleDef, typeof (CompProperties_VehicleLauncher), "landingAltitude", this.Props.landingAltitude);
    }
  }

  public bool ControlInFlight
  {
    get
    {
      return SettingsCache.TryGetValue<bool>(this.Vehicle.VehicleDef, typeof (CompProperties_VehicleLauncher), "controlInFlight", this.Props.controlInFlight);
    }
  }

  public int ReconDistance
  {
    get
    {
      return SettingsCache.TryGetValue<int>(this.Vehicle.VehicleDef, typeof (CompProperties_VehicleLauncher), "reconDistance", this.Props.reconDistance);
    }
  }

  public bool SpaceFlight
  {
    get
    {
      return SettingsCache.TryGetValue<bool>(this.Vehicle.VehicleDef, typeof (CompProperties_VehicleLauncher), "spaceFlight", this.Props.spaceFlight);
    }
  }

  public bool SignalJammer
  {
    get
    {
      return this.signalJammer ?? SettingsCache.TryGetValue<bool>(this.Vehicle.VehicleDef, typeof (CompProperties_VehicleLauncher), "signalJammer", this.Props.signalJammer);
    }
  }

  public override bool TickByRequest => true;

  public override IEnumerable<AnimationDriver> Animations => this.launchProtocol.Animations;

  public IEnumerable<VehicleTurret> StrafeTurrets
  {
    get
    {
      CompVehicleLauncher compVehicleLauncher = this;
      if (compVehicleLauncher.Vehicle.CompVehicleTurrets == null)
      {
        Log.Error("Cannot retrieve StrafeTurrets with no CompVehicleTurrets comp.");
      }
      else
      {
        foreach (VehicleTurret strafeTurret in compVehicleLauncher.Vehicle.CompVehicleTurrets.Turrets.Where<VehicleTurret>(new Func<VehicleTurret, bool>(compVehicleLauncher.\u003Cget_StrafeTurrets\u003Eb__50_0)))
          yield return strafeTurret;
      }
    }
  }

  public void SetTimedDeployment()
  {
    this.timer.Reset(this.Props.deployTicks);
    this.StartTicking();
  }

  public virtual IEnumerable<Gizmo> CompGetGizmosExtra()
  {
    CompVehicleLauncher compVehicleLauncher = this;
    // ISSUE: reference to a compiler-generated method
    foreach (Gizmo gizmo in compVehicleLauncher.\u003C\u003En__0())
      yield return gizmo;
    if (compVehicleLauncher.launchProtocol == null)
    {
      Log.ErrorOnce($"No launch protocols for {compVehicleLauncher.Vehicle}. At least 1 must be included in order to initiate takeoff.", ((Thing) compVehicleLauncher.Vehicle).thingIDNumber);
    }
    else
    {
      ((Gizmo) compVehicleLauncher.takeoffCommand).Disabled = false;
      if (((Thing) compVehicleLauncher.Vehicle).Spawned && compVehicleLauncher.launchProtocol.LaunchProperties.restriction != null)
      {
        // ISSUE: reference to a compiler-generated method
        compVehicleLauncher.takeoffCommand.mouseOver = new Action(compVehicleLauncher.\u003CCompGetGizmosExtra\u003Eb__52_0);
      }
      string disableReason;
      if (!compVehicleLauncher.CanLaunchWithCargoCapacity(out disableReason))
        ((Gizmo) compVehicleLauncher.takeoffCommand).Disable(disableReason);
      yield return (Gizmo) compVehicleLauncher.takeoffCommand;
    }
  }

  public bool CanLaunchWithCargoCapacity(out string disableReason)
  {
    disableReason = (string) null;
    if (((Thing) this.Vehicle).Spawned)
    {
      if (this.Vehicle.vehiclePather.Moving)
        disableReason = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_CannotLaunchWhileMoving", NamedArgument.op_Implicit(((Entity) this.Vehicle).LabelShort)));
      else if (Ext_Vehicles.IsRoofed(((Thing) this.Vehicle).Position, ((Thing) this.Vehicle).Map))
        disableReason = TaggedString.op_Implicit(Translator.Translate("CommandLaunchGroupFailUnderRoof"));
    }
    if ((this.Vehicle.MovementPermissions & VehiclePermissions.Mobile) != VehiclePermissions.None)
    {
      if (!this.Vehicle.CanMoveFinal)
        disableReason = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_CannotLaunchImmobile", NamedArgument.op_Implicit(((Entity) this.Vehicle).LabelShort)));
      else if ((double) this.Vehicle.Angle != 0.0)
        disableReason = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_CannotLaunchRotated", NamedArgument.op_Implicit(((Entity) this.Vehicle).LabelShort)));
    }
    else
    {
      float statValue = this.Vehicle.GetStatValue(VehicleStatDefOf.CargoCapacity);
      if ((double) MassUtility.InventoryMass((Pawn) this.Vehicle) > (double) statValue)
        disableReason = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_CannotLaunchOverEncumbered", NamedArgument.op_Implicit(((Entity) this.Vehicle).LabelShort)));
    }
    if ((!this.Vehicle.HasEnoughOperators || this.Vehicle.PawnCountToOperateLeft > 0) && !VehicleMod.settings.debug.debugDraftAnyVehicle)
    {
      disableReason = TaggedString.op_Implicit(Translator.Translate("VF_NotEnoughToOperate"));
    }
    else
    {
      CompFueledTravel compFueledTravel = this.Vehicle.CompFueledTravel;
      if (compFueledTravel != null && compFueledTravel.EmptyTank)
        disableReason = TaggedString.op_Implicit(Translator.Translate("VF_LaunchOutOfFuel"));
      else if ((double) this.FlightSpeed <= 0.0)
        disableReason = TaggedString.op_Implicit(Translator.Translate("VF_NoFlightSpeed"));
    }
    if (!this.launchProtocol.CanLaunchNow)
      disableReason = this.launchProtocol.FailLaunchMessage;
    return GenText.NullOrEmpty(disableReason);
  }

  public virtual string CompInspectStringExtra()
  {
    if (!this.Vehicle.HasEnoughOperators || !this.AnyLeftToLoad)
      return TaggedString.op_Implicit(Translator.Translate("ReadyForLaunch"));
    TaggedString taggedString1 = (object) Translator.Translate("NotReadyForLaunch");
    TaggedString taggedString2 = Translator.Translate("TransportPodInGroupHasSomethingLeftToLoad");
    TaggedString taggedString3 = (object) ((TaggedString) ref taggedString2).CapitalizeFirst();
    return $"{taggedString1}: {taggedString3}.";
  }

  public float FuelNeededToLaunchAtDist(PlanetTile destination)
  {
    return this.FuelNeededToLaunchAtDist(this.Origin, destination);
  }

  public float FuelNeededToLaunchAtDist(Vector3 origin, PlanetTile destination)
  {
    return this.FuelNeededToLaunchAtDist(Ext_Math.SphericalDistance(origin, WorldHelper.GetTilePos(destination)));
  }

  public float FuelNeededToLaunchAtDist(float tileDistance)
  {
    return this.Vehicle.CompFueledTravel == null ? 0.0f : this.Vehicle.CompFueledTravel.ConsumptionRatePerTickRaw * this.FuelConsumptionWorldMultiplier / (1f / 1000f / tileDistance * this.FlightSpeed);
  }

  private void StartChoosingDestination()
  {
    if (this.AnyLeftToLoad)
    {
      // ISSUE: method pointer
      Find.WindowStack.Add((Window) Dialog_MessageBox.CreateConfirmation(TranslatorFormattedStringExtensions.Translate("ConfirmSendNotCompletelyLoadedLaunchable", NamedArgument.op_Implicit(((Thing) this.Vehicle).LabelCapNoCount)), new Action((object) this, __methodptr(\u003CStartChoosingDestination\u003Eg__ConfirmStart\u007C58_0)), false, (string) null, (WindowLayer) 1));
    }
    else
      ConfirmStart();

    void ConfirmStart()
    {
      CameraJumper.TryJump(CameraJumper.GetWorldTarget(GlobalTargetInfo.op_Implicit((Thing) this.Vehicle)), (CameraJumper.MovementMode) 0);
      Find.WorldSelector.ClearSelection();
      new WorldTargeter<ArrivalOption>((ITargeterSource<GlobalTargetInfo, ArrivalOption>) this, this.Vehicle.CompFueledTravel != null ? (ITargeterUpdate<GlobalTargetInfo>) new FuelTargetUpdater(this.Vehicle, (ILauncher) this) : (ITargeterUpdate<GlobalTargetInfo>) null)
      {
        TargetTexture = TexData.TargeterMouseAttachment
      }.Start();
    }
  }

  public void Launch(TargetData<GlobalTargetInfo> targetData, IArrivalAction arrivalAction)
  {
    this.Vehicle.CompVehicleLauncher.inFlight = true;
    this.Vehicle.CompVehicleLauncher.launchProtocol.OrderProtocol(LaunchProtocol.LaunchType.Takeoff);
    VehicleSkyfaller_Leaving skyfallerLeaving = (VehicleSkyfaller_Leaving) VehicleSkyfallerMaker.MakeSkyfaller(this.Props.skyfallerLeaving, this.Vehicle);
    skyfallerLeaving.arrivalAction = arrivalAction;
    skyfallerLeaving.flightPath = targetData.targets.Select<GlobalTargetInfo, FlightNode>((Func<GlobalTargetInfo, FlightNode>) (target => new FlightNode(target))).ToList<FlightNode>();
    GenSpawn.Spawn((Thing) skyfallerLeaving, ((Thing) this.Vehicle).Position, ((Thing) this.Vehicle).Map, this.Vehicle.CompVehicleLauncher.launchProtocol.CurAnimationProperties.forcedRotation ?? ((Thing) this.Vehicle).Rotation, (WipeMode) 0, false, false);
    CameraJumper.TryHideWorld();
    this.Vehicle.EventRegistry[VehicleEventDefOf.AerialVehicleLaunch].ExecuteEvents();
  }

  public IEnumerable<ArrivalOption> OptionsAt(GlobalTargetInfo target)
  {
    return this.launchProtocol.GetArrivalOptions(target);
  }

  public TargetValidation CanTarget(GlobalTargetInfo target)
  {
    if (!((GlobalTargetInfo) ref target).IsValid)
      return TargetValidation.Failed;
    TaggedString failReason;
    if (!this.CanReach(target, out failReason) || !this.HasEnoughFuel(target, out failReason))
      return TargetValidation.Failed with
      {
        Tooltip = failReason
      };
    return TargetValidation.Success with
    {
      Tooltip = FloatMenuTooltip()
    };

    TaggedString FloatMenuTooltip()
    {
      using (new ClearOnDispose<ArrivalOption>((ICollection<ArrivalOption>) CompVehicleLauncher.ArrivalOptions))
      {
        CompVehicleLauncher.ArrivalOptions.AddRange(this.launchProtocol.GetArrivalOptions(target));
        int count = CompVehicleLauncher.ArrivalOptions.Count;
        if (count > 0)
        {
          if (count == 1)
          {
            FloatMenuAcceptanceReport acceptanceReport = CompVehicleLauncher.ArrivalOptions[0].AcceptanceReport;
            if (!((FloatMenuAcceptanceReport) ref acceptanceReport).Accepted)
              GUI.color = TexData.RedReadable;
            return CompVehicleLauncher.ArrivalOptions[0].label;
          }
          return ((GlobalTargetInfo) ref target).WorldObject is MapParent worldObject ? TranslatorFormattedStringExtensions.Translate("ClickToSeeAvailableOrders_WorldObject", NamedArgument.op_Implicit(((WorldObject) worldObject).LabelCap)) : Translator.Translate("ClickToSeeAvailableOrders_Empty");
        }
        if (count == 0)
          return TaggedString.op_Implicit((string) null);
        throw new InvalidOperationException("ArrivalOptions");
      }
    }
  }

  public TargeterResult Select(GlobalTargetInfo target)
  {
    List<ArrivalOption> list = this.OptionsAt(target).ToList<ArrivalOption>();
    return list.Count == 0 ? TargeterResult.Reject : TargeterResult.Accept<ArrivalOption>(list);
  }

  void ITargeterSource<GlobalTargetInfo, ArrivalOption>.OnTargetingFinished(
    TargetData<GlobalTargetInfo> targetData,
    ArrivalOption arrivalOption)
  {
    if (arrivalOption.continueWith != null)
      arrivalOption.continueWith(targetData);
    else
      this.Launch(targetData, arrivalOption.arrivalAction);
  }

  private bool HasEnoughFuel(GlobalTargetInfo target, out TaggedString failReason)
  {
    failReason = TaggedString.op_Implicit((string) null);
    float launchAtDist = this.FuelNeededToLaunchAtDist(((GlobalTargetInfo) ref target).Tile);
    if (this.Vehicle.CompFueledTravel != null && (double) launchAtDist > (double) this.Vehicle.CompFueledTravel.Fuel)
    {
      failReason = TaggedString.op_Implicit(ColoredText.Colorize(Translator.Translate("VF_NotEnoughFuel"), TexData.RedReadable));
      return false;
    }
    if (this.Vehicle.CompFueledTravel != null && ((GlobalTargetInfo) ref target).IsValid && (double) this.Vehicle.CompVehicleLauncher.FuelNeededToLaunchAtDist(this.Origin, ((GlobalTargetInfo) ref target).Tile) > (double) this.Vehicle.CompFueledTravel.Fuel - (double) launchAtDist)
      failReason = TaggedString.op_Implicit(ColoredText.Colorize(Translator.Translate("VF_NoFuelReturnTrip"), TexData.YellowReadable));
    return true;
  }

  private bool CanReach(GlobalTargetInfo target, out TaggedString failReason)
  {
    failReason = TaggedString.op_Implicit((string) null);
    if (!((GlobalTargetInfo) ref target).IsValid)
    {
      failReason = Translator.Translate("MessageTransportPodsDestinationIsInvalid");
      return false;
    }
    if (((GlobalTargetInfo) ref target).HasWorldObject)
    {
      if (!this.SpaceFlight && ((Def) ((GlobalTargetInfo) ref target).WorldObject.def).GetModExtension<SpaceObjectDefModExtension>() != null)
      {
        failReason = TranslatorFormattedStringExtensions.Translate("VF_NoSpaceFlight", NamedArgument.op_Implicit(((Entity) this.Vehicle).LabelCap));
        return false;
      }
      if (ModsConfig.OdysseyActive && ((GlobalTargetInfo) ref target).WorldObject.RequiresSignalJammerToReach && !this.SignalJammer)
      {
        failReason = Translator.Translate("TransportPodDestinationRequiresSignalJammer");
        return false;
      }
    }
    float tileDistance = Ext_Math.SphericalDistance(this.Origin, WorldHelper.GetTilePos(((GlobalTargetInfo) ref target).Tile));
    float launchAtDist = this.FuelNeededToLaunchAtDist(tileDistance);
    if ((double) tileDistance <= (double) this.MaxLaunchDistance)
    {
      CompFueledTravel compFueledTravel = this.Vehicle.CompFueledTravel;
      if (compFueledTravel == null || (double) launchAtDist <= (double) compFueledTravel.Fuel)
        return true;
    }
    failReason = Translator.Translate("TransportPodDestinationBeyondMaximumRange");
    return false;
  }

  public override void PostLoad()
  {
    this.ResolveProtocolProperties();
    Command_ActionHighlighter actionHighlighter = new Command_ActionHighlighter();
    ((Command) actionHighlighter).defaultLabel = TaggedString.op_Implicit(Translator.Translate("CommandLaunchGroup"));
    ((Command) actionHighlighter).defaultDesc = TaggedString.op_Implicit(Translator.Translate("CommandLaunchGroupDesc"));
    ((Command) actionHighlighter).icon = (Texture) TexData.LaunchCommandTex;
    ((Gizmo) actionHighlighter).alsoClickIfOtherInGroupClicked = false;
    actionHighlighter.action = new Action(this.StartChoosingDestination);
    this.takeoffCommand = actionHighlighter;
  }

  public override void PostGeneration()
  {
    base.PostGeneration();
    this.InitLaunchProtocol();
    Command_ActionHighlighter actionHighlighter = new Command_ActionHighlighter();
    ((Command) actionHighlighter).defaultLabel = TaggedString.op_Implicit(Translator.Translate("CommandLaunchGroup"));
    ((Command) actionHighlighter).defaultDesc = TaggedString.op_Implicit(Translator.Translate("CommandLaunchGroupDesc"));
    ((Command) actionHighlighter).icon = (Texture) TexData.LaunchCommandTex;
    ((Gizmo) actionHighlighter).alsoClickIfOtherInGroupClicked = false;
    actionHighlighter.action = new Action(this.StartChoosingDestination);
    this.takeoffCommand = actionHighlighter;
  }

  protected virtual void ResolveProtocolProperties()
  {
    this.launchProtocol.ResolveProperties(this.Props.launchProtocol);
  }

  private void InitLaunchProtocol()
  {
    if (this.Props.launchProtocol == null)
    {
      Log.Error("Vehicle has null launchProtocol.");
    }
    else
    {
      if (this.launchProtocol != null)
        return;
      this.launchProtocol = (LaunchProtocol) Activator.CreateInstance(this.Props.launchProtocol.GetType(), (object) this.Props.launchProtocol, (object) this.Vehicle);
    }
  }

  public virtual void CompTick()
  {
    this.timer.Tick(this.Vehicle);
    if (!this.timer.Expired)
      return;
    this.StopTicking();
  }

  public virtual void PostSpawnSetup(bool respawningAfterLoad)
  {
    base.PostSpawnSetup(respawningAfterLoad);
    this.inFlight = false;
    if (respawningAfterLoad)
      return;
    this.fuelEfficiencyWorldModifier = 0.0f;
  }

  public virtual void PostExposeData()
  {
    base.PostExposeData();
    Scribe_Deep.Look<LaunchProtocol>(ref this.launchProtocol, "launchProtocol", Array.Empty<object>());
    Scribe_Values.Look<float>(ref this.flightSpeedModifier, "flightSpeedModifier", 0.0f, false);
    Scribe_Values.Look<float>(ref this.fuelEfficiencyWorldModifier, "fuelEfficiencyWorldModifier", 0.0f, false);
    Scribe_Values.Look<bool?>(ref this.signalJammer, "signalJammer", new bool?(), false);
    Scribe_Values.Look<float>(ref this.rateOfClimbModifier, "rateOfClimbModifier", 0.0f, false);
    Scribe_Values.Look<int>(ref this.maxAltitudeModifier, "maxAltitudeModifier", 0, false);
    Scribe_Values.Look<int>(ref this.landingAltitudeModifier, "landingAltitudeModifier", 0, false);
    Scribe_Values.Look<bool>(ref this.inFlight, "inFlight", false, false);
    Scribe_Values.Look<bool>(ref this.loiter, "loiter", false, false);
    Scribe_Values.Look<CompVehicleLauncher.DeploymentTimer>(ref this.timer, "timer", new CompVehicleLauncher.DeploymentTimer(), false);
  }

  static CompVehicleLauncher()
  {
    SimpleCurve simpleCurve = new SimpleCurve();
    simpleCurve.Add(new CurvePoint(0.0f, -5f), true);
    simpleCurve.Add(new CurvePoint(0.15f, -2.5f), true);
    simpleCurve.Add(new CurvePoint(0.25f, -1f), true);
    simpleCurve.Add(new CurvePoint(0.35f, -0.25f), true);
    simpleCurve.Add(new CurvePoint(0.45f, 0.0f), true);
    simpleCurve.Add(new CurvePoint(0.5f, 0.25f), true);
    simpleCurve.Add(new CurvePoint(0.75f, 0.45f), true);
    simpleCurve.Add(new CurvePoint(0.8f, 0.75f), true);
    simpleCurve.Add(new CurvePoint(0.9f, 0.95f), true);
    simpleCurve.Add(new CurvePoint(1f, 1f), true);
    CompVehicleLauncher.ClimbRateCurve = simpleCurve;
  }

  [PublicAPI]
  public struct DeploymentTimer(int ticksLeft, bool enabled)
  {
    private int ticksLeft = ticksLeft;
    private bool enabled = enabled;

    public static CompVehicleLauncher.DeploymentTimer Default
    {
      get => new CompVehicleLauncher.DeploymentTimer(0, false);
    }

    public bool Expired => !this.enabled || this.ticksLeft <= 0;

    public void Reset(int delayDeploymentTicks)
    {
      this.ticksLeft = delayDeploymentTicks + Mathf.RoundToInt(VehicleMod.settings.main.delayDeployOnLanding * 60f);
      this.enabled = true;
    }

    public void Tick(VehiclePawn vehicle)
    {
      --this.ticksLeft;
      if (!this.enabled || this.ticksLeft > 0)
        return;
      this.enabled = false;
      vehicle.DisembarkAll();
    }

    public static CompVehicleLauncher.DeploymentTimer FromString(string entry)
    {
      entry = entry.TrimStart('(').TrimEnd(')');
      string[] strArray = entry.Split(',', StringSplitOptions.None);
      try
      {
        CultureInfo invariantCulture = CultureInfo.InvariantCulture;
        return new CompVehicleLauncher.DeploymentTimer(Convert.ToInt32(strArray[0], (IFormatProvider) invariantCulture), Convert.ToBoolean(strArray[1], (IFormatProvider) invariantCulture));
      }
      catch (Exception ex)
      {
        SmashLog.Error($"{entry} is not a valid <struct>DeploymentTimer</struct> format. Exception: {ex}");
        return CompVehicleLauncher.DeploymentTimer.Default;
      }
    }

    public override string ToString() => $"({this.ticksLeft},{this.enabled})";
  }
}
