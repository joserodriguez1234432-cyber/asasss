// Decompiled with JetBrains decompiler
// Type: Vehicles.World.AerialVehicleInFlight
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
using System.Linq;
using UnityEngine;
using Vehicles.Rendering;
using Verse;

#nullable disable
namespace Vehicles.World;

[PublicAPI]
[StaticConstructorOnStartup]
public class AerialVehicleInFlight : 
  DynamicDrawnWorldObject,
  IVehicleWorldObject,
  IThingHolder,
  IThingHolderTickable,
  IThingHolderEvents<VehiclePawn>,
  ILauncher,
  ITargeterSource<GlobalTargetInfo, ArrivalOption>
{
  private static readonly Texture2D ViewQuestCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/ViewQuest", true);
  public const float ReconFlightSpeed = 5f;
  public const float TransitionTakeoff = 0.025f;
  public const float PctPerTick = 0.001f;
  public const int TicksPerValidateFlightPath = 60;
  private VehiclePawn vehicle;
  public ThingOwner<VehiclePawn> innerContainer;
  public FlightPath flightPath;
  private Gizmo_RefuelableFuelTravel fuelGizmo;
  internal float transition;
  public float elevation;
  public bool recon;
  private float speedPctPerTick;
  public Vector3 position;
  private Material material;

  [Obsolete("This constructor is requierd for Xml Deserialization, use AerialVehicleInFlight::Create instead")]
  public AerialVehicleInFlight()
  {
    this.innerContainer = new ThingOwner<VehiclePawn>((IThingHolder) this, false, (LookMode) 3, true);
  }

  public virtual string Label => ((Entity) this.vehicle)?.Label;

  public VehiclePawn Vehicle => this.vehicle;

  bool IThingHolderTickable.ShouldTickContents => false;

  public virtual bool IsPlayerControlled => ((Thing) this.vehicle).Faction == Faction.OfPlayer;

  public float Elevation => 0.0f;

  public float ElevationChange { get; protected set; }

  protected virtual Rot8 FullRotation => Rot8.North;

  protected virtual float RotatorSpeeds => 59f;

  public bool Flying => this.vehicle.CompVehicleLauncher.inFlight;

  public bool CanDismount => false;

  Vector3 ILauncher.Origin => base.DrawPos;

  bool ITargeterSource<GlobalTargetInfo, ArrivalOption>.TargeterValid
  {
    get
    {
      if (this.Destroyed)
        return false;
      VehiclePawn vehicle = this.Vehicle;
      return vehicle != null && !((Thing) vehicle).Spawned && !((Thing) vehicle).Destroyed;
    }
  }

  public virtual Vector3 DrawPos
  {
    get
    {
      if (GenList.NullOrEmpty<FlightNode>((IList<FlightNode>) this.flightPath.Path))
        return WorldHelper.GetTilePos(this.Tile);
      Vector3 center = this.flightPath.First.GetCenter(this);
      return Vector3.op_Equality(this.position, center) ? this.position : Vector3.Slerp(this.position, center, this.transition);
    }
  }

  public IEnumerable<VehiclePawn> Vehicles
  {
    get
    {
      yield return this.vehicle;
    }
  }

  public IEnumerable<Pawn> DismountedPawns
  {
    get
    {
      yield break;
    }
  }

  public virtual Material Material
  {
    get
    {
      if (!Object.op_Implicit((Object) this.material))
        this.material = MaterialPool.MatFrom(GenCollection.TryGetValue<VehicleDef, string>((IReadOnlyDictionary<VehicleDef, string>) VehicleTex.CachedTextureIconPaths, this.vehicle.VehicleDef, "UI/Icons/DefaultVehicleIcon"), ShaderDatabase.WorldOverlayTransparentLit, this.Faction.Color, 3550);
      return this.material;
    }
  }

  public virtual void Initialize() => this.position = base.DrawPos;

  public virtual Vector3 DrawPosAhead(int ticksAhead)
  {
    return Vector3.Slerp(this.position, this.flightPath.First.GetCenter(this), this.transition + this.speedPctPerTick * (float) ticksAhead);
  }

  public virtual void Draw()
  {
    if (WorldObjectSelectionUtility.HiddenBehindTerrainNow((WorldObject) this))
      return;
    WorldHelper.DrawQuadTangentialToPlanet(base.DrawPos, 0.7f * Find.WorldGrid.AverageTileSize, 0.015f, base.Material);
  }

  public virtual IEnumerable<Gizmo> GetGizmos()
  {
    AerialVehicleInFlight aerialVehicleInFlight = this;
    // ISSUE: reference to a compiler-generated method
    foreach (Gizmo gizmo in aerialVehicleInFlight.\u003C\u003En__0())
      yield return gizmo;
    if (aerialVehicleInFlight.ShowRelatedQuests)
    {
      foreach (Quest quest1 in Find.QuestManager.QuestsListForReading)
      {
        Quest quest = quest1;
        if (!quest.hidden && !quest.Historical && !quest.dismissed && quest.QuestLookTargets.Contains<GlobalTargetInfo>(GlobalTargetInfo.op_Implicit((WorldObject) aerialVehicleInFlight)))
        {
          Command_Action gizmo = new Command_Action();
          ((Command) gizmo).defaultLabel = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("CommandViewQuest", NamedArgument.op_Implicit(quest.name)));
          ((Command) gizmo).defaultDesc = TaggedString.op_Implicit(Translator.Translate("CommandViewQuestDesc"));
          ((Command) gizmo).icon = (Texture) AerialVehicleInFlight.ViewQuestCommandTex;
          gizmo.action = (Action) (() =>
          {
            Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests, true);
            ((MainTabWindow_Quests) MainButtonDefOf.Quests.TabWindow).Select(quest);
          });
          yield return (Gizmo) gizmo;
        }
      }
    }
    if (aerialVehicleInFlight.IsPlayerControlled)
    {
      if (aerialVehicleInFlight.vehicle.CompFueledTravel != null)
      {
        if (aerialVehicleInFlight.fuelGizmo == null)
          aerialVehicleInFlight.fuelGizmo = new Gizmo_RefuelableFuelTravel(aerialVehicleInFlight.vehicle.CompFueledTravel, false);
        yield return (Gizmo) aerialVehicleInFlight.fuelGizmo;
        if (DebugSettings.ShowDevGizmos)
        {
          foreach (Gizmo devModeGizmo in aerialVehicleInFlight.vehicle.CompFueledTravel.DevModeGizmos())
            yield return devModeGizmo;
        }
      }
      if (aerialVehicleInFlight.vehicle.CompVehicleLauncher.ControlInFlight)
      {
        Command_Action commandAction = new Command_Action();
        ((Command) commandAction).defaultLabel = TaggedString.op_Implicit(Translator.Translate("CommandLaunchGroup"));
        ((Command) commandAction).defaultDesc = TaggedString.op_Implicit(Translator.Translate("CommandLaunchGroupDesc"));
        ((Command) commandAction).icon = (Texture) TexData.LaunchCommandTex;
        ((Gizmo) commandAction).alsoClickIfOtherInGroupClicked = false;
        commandAction.action = new Action(aerialVehicleInFlight.StartTargeting);
        Command_Action gizmo = commandAction;
        string disableReason;
        if (!aerialVehicleInFlight.vehicle.CompVehicleLauncher.CanLaunchWithCargoCapacity(out disableReason))
        {
          ((Gizmo) gizmo).Disabled = true;
          ((Gizmo) gizmo).disabledReason = disableReason;
        }
        yield return (Gizmo) gizmo;
      }
      if (DebugSettings.ShowDevGizmos)
      {
        Command_Action gizmo1 = new Command_Action();
        ((Command) gizmo1).defaultLabel = "Debug: Land at Nearest Player Settlement";
        // ISSUE: reference to a compiler-generated method
        gizmo1.action = new Action(aerialVehicleInFlight.\u003CGetGizmos\u003Eb__53_1);
        yield return (Gizmo) gizmo1;
        Command_Action gizmo2 = new Command_Action();
        ((Command) gizmo2).defaultLabel = "Debug: Initiate Crash Event";
        // ISSUE: reference to a compiler-generated method
        gizmo2.action = new Action(aerialVehicleInFlight.\u003CGetGizmos\u003Eb__53_2);
        yield return (Gizmo) gizmo2;
      }
    }
  }

  private void StartTargeting()
  {
    CameraJumper.TryJump(CameraJumper.GetWorldTarget(GlobalTargetInfo.op_Implicit((WorldObject) this)), (CameraJumper.MovementMode) 0);
    Find.WorldSelector.ClearSelection();
    new WorldTargeter<ArrivalOption>((ITargeterSource<GlobalTargetInfo, ArrivalOption>) this, this.Vehicle.CompFueledTravel != null ? (ITargeterUpdate<GlobalTargetInfo>) new FuelTargetUpdater(this.Vehicle, (ILauncher) this) : (ITargeterUpdate<GlobalTargetInfo>) null)
    {
      TargetTexture = TexData.TargeterMouseAttachment
    }.Start();
  }

  protected virtual void Tick()
  {
    base.Tick();
    if (!this.vehicle.CompVehicleLauncher.inFlight)
      return;
    this.SpendFuel();
    CompFueledTravel compFueledTravel = this.vehicle.CompFueledTravel;
    if ((compFueledTravel != null ? ((double) compFueledTravel.Fuel <= 0.0 ? 1 : 0) : 0) != 0)
      this.InitiateCrashEvent((WorldObject) null, TaggedString.op_Implicit(Translator.Translate("VF_IncidentCrashedSiteReason_OutOfFuel")));
    this.MoveForward();
  }

  public virtual void SpendFuel()
  {
    if (this.vehicle.CompFueledTravel == null || (this.vehicle.CompFueledTravel.FuelCondition & FuelConsumptionCondition.Flying) == (FuelConsumptionCondition) 0)
      return;
    this.vehicle.CompFueledTravel.ConsumeFuel(this.vehicle.CompFueledTravel.ConsumptionRatePerTick * this.vehicle.CompVehicleLauncher.FuelConsumptionWorldMultiplier);
  }

  public virtual void TakeDamage(DamageInfo damageInfo, IntVec2 cell)
  {
    this.vehicle.TakeDamage(damageInfo, cell);
  }

  public void InitiateCrashEvent(WorldObject culprit = null, params string[] reasons)
  {
    this.vehicle.CompVehicleLauncher.inFlight = false;
    this.Tile = PlanetTile.op_Implicit(WorldHelper.GetNearestTile(base.DrawPos));
    this.ResetPosition(base.DrawPos);
    this.flightPath.ResetPath();
    AirDefensePositionTracker.DeregisterAerialVehicle(this);
    IncidentWorker_ShuttleDowned.Execute(this, reasons, culprit);
  }

  public virtual void MoveForward()
  {
    if (this.flightPath.Empty)
    {
      Log.Error($"{this} in flight with empty FlightPath.  Grounding to current Tile.");
      if (this.Destroyed)
        return;
      this.ArriveAtTile(this.Tile);
      this.SwitchToCaravan();
    }
    else
    {
      this.transition += this.speedPctPerTick;
      if ((double) this.transition < 1.0)
        return;
      if (((Thing) this.vehicle).Faction.IsPlayer && this.flightPath.Count == 1)
        Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_AerialVehicleArrived", NamedArgument.op_Implicit(((Entity) this.vehicle).LabelShort))), MessageTypeDefOf.NeutralEvent, true);
      this.ArriveAtTile(this.flightPath.First.Tile);
      this.flightPath.ConsumeNode(!this.recon);
      if (this.Destroyed)
        return;
      this.InitializeNextFlight(base.DrawPos);
    }
  }

  private void ResetPosition(Vector3 position)
  {
    this.position = position;
    this.transition = 0.0f;
  }

  public void SwitchToCaravan()
  {
    bool flag = Find.WorldSelector.SelectedObjects.Contains((WorldObject) this);
    ((ThingOwner) this.innerContainer).Remove((Thing) this.vehicle);
    // ISSUE: object of a compiler-generated type is created
    VehicleCaravan vehicleCaravan = CaravanHelper.MakeVehicleCaravan((IEnumerable<Pawn>) new \u003C\u003Ez__ReadOnlySingleElementList<Pawn>((Pawn) this.vehicle), ((Thing) this.vehicle).Faction, this.Tile, true);
    if (!this.Destroyed)
      this.ClearAndDestroy();
    if (!flag)
      return;
    Find.WorldSelector.Select((WorldObject) vehicleCaravan, false);
  }

  private void InitializeNextFlight(Vector3 origin)
  {
    this.vehicle.CompVehicleLauncher.inFlight = true;
    this.ResetPosition(origin);
    this.SetSpeed();
  }

  private void SetSpeed()
  {
    Vector3 center = this.flightPath.First.GetCenter(this);
    if (Vector3.op_Equality(this.position, center))
      this.speedPctPerTick = 1f;
    else
      this.speedPctPerTick = 1f / 1000f / Mathf.Clamp(Ext_Math.SphericalDistance(this.position, center), 1E-05f, float.MaxValue) * (this.recon ? 5f : this.vehicle.CompVehicleLauncher.FlightSpeed).Clamp(0.0f, 99999f);
  }

  TargetValidation ITargeterSource<GlobalTargetInfo, ArrivalOption>.CanTarget(
    GlobalTargetInfo target)
  {
    return this.vehicle == null || this.Destroyed ? TargetValidation.Failed : this.vehicle.CompVehicleLauncher.CanTarget(target);
  }

  TargeterResult ITargeterSource<GlobalTargetInfo, ArrivalOption>.Select(GlobalTargetInfo target)
  {
    return this.vehicle.CompVehicleLauncher.Select(target);
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

  public void Launch(TargetData<GlobalTargetInfo> targetData, IArrivalAction arrivalAction)
  {
    this.OrderFlyToTiles(targetData.targets.Select<GlobalTargetInfo, FlightNode>((Func<GlobalTargetInfo, FlightNode>) (target => new FlightNode(target))).ToList<FlightNode>(), arrivalAction);
  }

  IEnumerable<ArrivalOption> ILauncher.OptionsAt(GlobalTargetInfo target)
  {
    return this.vehicle.CompVehicleLauncher.OptionsAt(target);
  }

  public void ArriveAtTile(PlanetTile tile)
  {
    this.Tile = tile;
    this.ResetPosition(base.DrawPos);
    this.vehicle.CompVehicleLauncher.inFlight = false;
    AirDefensePositionTracker.DeregisterAerialVehicle(this);
  }

  private void ResumePathPostLoad()
  {
    this.OrderFlyToTiles(this.flightPath.Path.ToList<FlightNode>(), this.flightPath.ArrivalAction);
  }

  public void OrderFlyToTiles(List<FlightNode> flightPath, [NotNull] IArrivalAction arrivalAction)
  {
    if (GenCollection.Any<FlightNode>(flightPath, (Predicate<FlightNode>) (node =>
    {
      PlanetTile tile = node.Tile;
      return !((PlanetTile) ref tile).Valid;
    })))
      throw new ArgumentException("Invalid tiles in flight path.", nameof (flightPath));
    Vector3 drawPos = base.DrawPos;
    this.flightPath.NewPath(flightPath, arrivalAction);
    this.InitializeNextFlight(drawPos);
    this.vehicle.EventRegistry[VehicleEventDefOf.AerialVehicleOrdered].ExecuteEvents();
  }

  public virtual void DrawExtraSelectionOverlays()
  {
    base.DrawExtraSelectionOverlays();
    this.DrawFlightPath();
  }

  private void DrawFlightPath()
  {
    if (this.flightPath.Path.Count > 1)
    {
      Vector3 start = base.DrawPos;
      for (int index = 0; index < this.flightPath.Path.Count; ++index)
      {
        Vector3 center = this.flightPath[index].GetCenter(this);
        FlightPath.DrawPath(start, center, TexData.WorldLineMatWhite);
        start = center;
      }
      FlightPath.DrawPath(start, this.flightPath.Last.GetCenter(this), TexData.WorldLineMatWhite);
    }
    else
    {
      if (this.flightPath.Path.Count != 1)
        return;
      FlightPath.DrawPath(base.DrawPos, this.flightPath.First.GetCenter(this), TexData.WorldLineMatWhite);
    }
  }

  public void SetCircle(PlanetTile tile) => this.flightPath.PushCircleAt(tile);

  public void GenerateMapForRecon(PlanetTile tile)
  {
    if (!this.flightPath.InRecon)
      return;
    MapParent mapParent = Find.WorldObjects.MapParentAt(tile);
    if (mapParent == null || mapParent.HasMap)
      return;
    LongEventHandler.QueueLongEvent((Action) (() =>
    {
      Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(tile, (WorldObjectDef) null, (IEnumerable<GenStepWithParams>) null);
      TaggedString taggedString1 = Translator.Translate("LetterLabelCaravanEnteredEnemyBase");
      TaggedString taggedString2 = TranslatorFormattedStringExtensions.Translate("LetterTransportPodsLandedInEnemyBase", NamedArgument.op_Implicit(((WorldObject) mapParent).Label));
      TaggedString taggedString3 = ((TaggedString) ref taggedString2).CapitalizeFirst();
      if (mapParent is Settlement settlement2)
        SettlementUtility.AffectRelationsOnAttacked((MapParent) settlement2, ref taggedString3);
      if (!mapParent.HasMap)
      {
        Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
        PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter((IEnumerable<Pawn>) orGenerateMap.mapPawns.AllPawns, ref taggedString1, ref taggedString3, TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("LetterRelatedPawnsInMapWherePlayerLanded", NamedArgument.op_Implicit(Faction.OfPlayer.def.pawnsPlural))), true, true);
      }
      Find.LetterStack.ReceiveLetter(taggedString1, taggedString3, LetterDefOf.NeutralEvent, LookTargets.op_Implicit((Thing) this.vehicle), ((WorldObject) mapParent).Faction, (Quest) null, (List<ThingDef>) null, (string) null, 0, true);
      Current.Game.CurrentMap = orGenerateMap;
      CameraJumper.TryHideWorld();
    }), "GeneratingMap", false, (Action<Exception>) null, true, false, (Action) null);
  }

  public virtual void PostMake()
  {
    base.PostMake();
    this.flightPath = new FlightPath(this);
  }

  public void ClearAndDestroy()
  {
    this.vehicle = (VehiclePawn) null;
    ((ThingOwner) this.innerContainer).Clear();
    base.Destroy();
  }

  public virtual void Destroy()
  {
    base.Destroy();
    VehiclePawn vehicle = this.vehicle;
    if (vehicle == null || ((Thing) vehicle).Destroyed)
      return;
    ThingOwner<VehiclePawn> innerContainer = this.innerContainer;
    if (innerContainer != null && !((ThingOwner) innerContainer).Any)
    {
      Trace.Fail($"Trying to destroy {this.vehicle} but it's not inside the aerial vehicle.");
    }
    else
    {
      this.vehicle.DestroyVehicleAndPawns((DestroyMode) 0);
      ((ThingOwner) this.innerContainer).Clear();
    }
  }

  public virtual void ExposeData()
  {
    base.ExposeData();
    Scribe_References.Look<VehiclePawn>(ref this.vehicle, "vehicle", true);
    Scribe_Deep.Look<FlightPath>(ref this.flightPath, "flightPath", new object[1]
    {
      (object) this
    });
    Scribe_Values.Look<float>(ref this.transition, "transition", 0.0f, false);
    Scribe_Values.Look<Vector3>(ref this.position, "position", new Vector3(), false);
    Scribe_Values.Look<bool>(ref this.recon, "recon", false, false);
    if (Scribe.mode != 4)
      return;
    ((ThingOwner) this.innerContainer).TryAdd((Thing) this.vehicle, false);
    if (this.flightPath == null || GenList.NullOrEmpty<FlightNode>((IList<FlightNode>) this.flightPath.Path))
      return;
    this.ResumePathPostLoad();
  }

  public virtual void SpawnSetup()
  {
    base.SpawnSetup();
    this.vehicle.RegisterEvents();
  }

  void IThingHolder.GetChildHolders(List<IThingHolder> outChildren)
  {
  }

  ThingOwner IThingHolder.GetDirectlyHeldThings() => (ThingOwner) this.innerContainer;

  public static AerialVehicleInFlight Create(VehiclePawn vehicle, PlanetTile tile)
  {
    AerialVehicleInFlight aerialVehicleInFlight = (AerialVehicleInFlight) WorldObjectMaker.MakeWorldObject(WorldObjectDefOfVehicles.AerialVehicle);
    aerialVehicleInFlight.vehicle = vehicle;
    aerialVehicleInFlight.Tile = tile;
    aerialVehicleInFlight.SetFaction(((Thing) vehicle).Faction);
    aerialVehicleInFlight.Initialize();
    ((ThingOwner) aerialVehicleInFlight.innerContainer).TryAddOrTransfer((Thing) vehicle, false);
    Find.WorldObjects.Add((WorldObject) aerialVehicleInFlight);
    if (!WorldPawnsUtility.IsWorldPawn((Pawn) vehicle))
      Find.WorldPawns.PassToWorld((Pawn) vehicle, (PawnDiscardDecideMode) 0);
    foreach (Pawn pawn in vehicle.AllPawnsAboard)
    {
      if (WorldPawnsUtility.IsWorldPawn(pawn))
        Find.WorldPawns.RemovePawn(pawn);
    }
    return aerialVehicleInFlight;
  }

  void IThingHolderEvents<VehiclePawn>.Notify_ItemAdded(VehiclePawn vehicle)
  {
  }

  void IThingHolderEvents<VehiclePawn>.Notify_ItemRemoved(VehiclePawn vehicle)
  {
  }
}
