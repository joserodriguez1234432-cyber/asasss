// Decompiled with JetBrains decompiler
// Type: Vehicles.VehiclePawn
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Animations;
using SmashTools.Performance;
using SmashTools.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Vehicles.Compatibility;
using Vehicles.Rendering;
using Vehicles.World;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class VehiclePawn : 
  Pawn,
  IInspectable,
  IThingHolderTickable,
  IThingHolder,
  IAnimationTarget,
  IAnimator,
  IAnimationObject,
  ITransformable,
  IEventManager<VehicleEventDef>,
  IMaterialCacheTarget
{
  private static readonly AccessTools.FieldRef<Pawn_DraftController, bool> DraftedIntFieldRef = AccessTools.FieldRefAccess<Pawn_DraftController, bool>(AccessTools.Field(typeof (Pawn_DraftController), "draftedInt"));
  [Unsaved(false)]
  public VehicleAI vehicleAI;
  public VehiclePathFollower vehiclePather;
  public VehicleIgnitionController ignition;
  public SharedJob sharedJob;
  private bool fishing;
  private static readonly AccessTools.FieldRef<TransferableOneWay, int> CountToTransferFieldRef = AccessTools.FieldRefAccess<TransferableOneWay, int>("countToTransfer");
  public List<TransferableOneWay> cargoToLoad;
  [Unsaved(false)]
  private bool fetchedCompVehicleTurrets;
  [Unsaved(false)]
  private bool fetchedCompFuel;
  [Unsaved(false)]
  private bool fetchedCompUpgradeTree;
  [Unsaved(false)]
  private bool fetchedCompVehicleLauncher;
  [Unsaved(false)]
  private CompVehicleTurrets compVehicleTurrets;
  [Unsaved(false)]
  private CompFueledTravel compFuel;
  [Unsaved(false)]
  private CompUpgradeTree compUpgradeTree;
  [Unsaved(false)]
  private CompVehicleLauncher compVehicleLauncher;
  [Unsaved(false)]
  private SelfOrderingList<ThingComp> cachedComps = new SelfOrderingList<ThingComp>();
  [Unsaved(false)]
  private List<ThingComp> compTickers = new List<ThingComp>();
  internal List<ThingComp> deactivatedComps = new List<ThingComp>();
  internal List<ActivatableThingComp> activatableComps = new List<ActivatableThingComp>();
  internal List<System.Type> deactivatedCompTypes = new List<System.Type>();
  private List<AssignedSeat> boardingAssignments = new List<AssignedSeat>();
  public List<VehicleRoleHandler> handlers = new List<VehicleRoleHandler>();
  public bool beached;
  [TweakField]
  public VehicleStatHandler statHandler;
  public VehicleMovementStatus movementStatus = VehicleMovementStatus.Online;
  [AnimationProperty]
  [TweakField]
  private VehicleDrawTracker drawTracker;
  public PatternData patternData;
  private RetextureDef retextureDef;
  private float angle;
  private bool reverse;
  [TweakField]
  [AnimationProperty(Name = "Transform")]
  private readonly Transform transform = new Transform();
  public AnimationManager animator;
  private Graphic_Vehicle graphic;
  public PatternData patternToPaint;
  private bool crashLanded;
  private Command_Toggle fishToggle;
  public const int HoursIdleToAlert = 2;
  public const int TicksTillAlert = 5000;
  public const int MaxTickInterval = 250;
  [Unsaved(false)]
  public VehicleSustainers sustainers;
  private int ticksSinceBoarded;
  private List<TimedExplosion> explosives = new List<TimedExplosion>();

  public EventManager<VehicleEventDef> EventRegistry { get; set; }

  public bool Drafted => this.ignition.Drafted;

  public VehicleDef VehicleDef => ((Verse.Thing) this).def as VehicleDef;

  public int AverageSkillOfCapablePawns(SkillDef skill)
  {
    if (this.AllCapablePawns.Count == 0)
      return 0;
    int num = 0;
    foreach (Pawn allCapablePawn in this.AllCapablePawns)
    {
      SkillRecord skill1 = allCapablePawn.skills?.GetSkill(skill);
      if (skill1 != null)
        num += skill1.Level;
    }
    return num / this.AllCapablePawns.Count;
  }

  private void InitializeVehicle()
  {
    List<VehicleRoleHandler> handlers = this.handlers;
    if (handlers != null && handlers.Count > 0)
      return;
    if (this.cargoToLoad == null)
      this.cargoToLoad = new List<TransferableOneWay>();
    if (this.boardingAssignments == null)
      this.boardingAssignments = new List<AssignedSeat>();
    if (!GenList.NullOrEmpty<VehicleRole>((IList<VehicleRole>) this.VehicleDef.properties.roles))
    {
      foreach (VehicleRole role in this.VehicleDef.properties.roles)
        this.handlers.Add(new VehicleRoleHandler(this, role));
    }
    this.handlers.Sort();
    this.RecacheComponents();
    this.RecacheMovementPermissions();
  }

  public virtual void PostMapInit() => this.vehiclePather.TryResumePathingAfterLoading();

  public virtual void PostGenerationSetup()
  {
    this.ignition = new VehicleIgnitionController(this);
    this.ageTracker.AgeBiologicalTicks = 0L;
    this.ageTracker.AgeChronologicalTicks = 0L;
    this.ageTracker.BirthAbsTicks = 0L;
    this.statHandler.InitializeComponents();
    this.RegisterEvents();
    this.InitializeVehicle();
    this.RegenerateUnsavedComponents();
    UnityThread.ExecuteOnMainThread(new Action(this.DrawTracker.overlayRenderer.Init));
    if (((Verse.Thing) this).Faction == Faction.OfPlayer || this.VehicleDef.npcProperties == null)
      return;
    this.GenerateInventory();
  }

  private void GenerateInventory()
  {
    if (this.VehicleDef.npcProperties?.raidParams?.inventory == null)
      return;
    foreach (PawnInventoryOption pawnInventoryOption in this.VehicleDef.npcProperties.raidParams.inventory)
    {
      foreach (Verse.Thing thing in pawnInventoryOption.GenerateThings())
        ((ThingOwner) this.inventory.innerContainer).TryAdd(thing, true);
    }
  }

  private void RecacheAlerts() => this.ticksSinceBoarded = 0;

  private void UpdateDraftController()
  {
    if (this.drafter == null || this.ignition == null)
      return;
    VehiclePawn.DraftedIntFieldRef.Invoke(this.drafter) = this.ignition.Drafted;
  }

  [Conditional("ANIMATOR")]
  private void UpdateDraftAnimationProperty()
  {
    this.animator?.SetBool(PropertyIds.IgnitionOn, this.ignition.Drafted);
  }

  public void RegisterEvents()
  {
    if (this.EventRegistry != null && this.EventRegistry.Initialized<VehicleEventDef>())
      return;
    this.FillEventsDef<VehicleEventDef>();
    ((ThingOwner) this.inventory.innerContainer).OnContentsChanged += new Action(this.RecacheInventoryPawns);
    this.AddEvent<VehicleEventDef>(VehicleEventDefOf.CargoAdded, new Action(this.statHandler.MarkAllDirty));
    this.AddEvent<VehicleEventDef>(VehicleEventDefOf.CargoRemoved, new Action(this.statHandler.MarkAllDirty));
    this.AddEvent<VehicleEventDef>(VehicleEventDefOf.PawnEntered, new Action(this.RecachePawnCount), new Action(this.RecacheAlerts));
    this.AddEvent<VehicleEventDef>(VehicleEventDefOf.PawnExited, new Action(this.vehiclePather.RecalculatePermissions), new Action(this.RecachePawnCount));
    this.AddEvent<VehicleEventDef>(VehicleEventDefOf.PawnRemoved, new Action(this.vehiclePather.RecalculatePermissions), new Action(this.RecachePawnCount));
    this.AddEvent<VehicleEventDef>(VehicleEventDefOf.PawnChangedSeats, new Action(this.vehiclePather.RecalculatePermissions), new Action(this.RecachePawnCount));
    this.AddEvent<VehicleEventDef>(VehicleEventDefOf.PawnKilled, new Action(this.vehiclePather.RecalculatePermissions), new Action(this.RecachePawnCount));
    this.AddEvent<VehicleEventDef>(VehicleEventDefOf.PawnCapacitiesDirty, new Action(this.vehiclePather.RecalculatePermissions));
    this.AddEvent<VehicleEventDef>(VehicleEventDefOf.MoveStart, new Action(this.RecacheAlerts));
    this.AddEvent<VehicleEventDef>(VehicleEventDefOf.MoveStop, new Action(this.RecacheAlerts));
    this.AddEvent<VehicleEventDef>(VehicleEventDefOf.IgnitionOn, new Action(this.vehiclePather.RecalculatePermissions), new Action(this.RecacheAlerts), new Action(this.UpdateDraftController));
    this.AddEvent<VehicleEventDef>(VehicleEventDefOf.IgnitionOff, new Action(this.vehiclePather.RecalculatePermissions), new Action(this.UpdateDraftController));
    this.AddEvent<VehicleEventDef>(VehicleEventDefOf.HealthChanged, new Action(this.vehiclePather.RecalculatePermissions));
    this.AddEvent<VehicleEventDef>(VehicleEventDefOf.DamageTaken, new Action(this.statHandler.MarkAllDirty), new Action(this.Notify_TookDamage));
    this.AddEvent<VehicleEventDef>(VehicleEventDefOf.Repaired, new Action(this.statHandler.MarkAllDirty));
    this.AddEvent<VehicleEventDef>(VehicleEventDefOf.OutOfFuel, (Action) (() =>
    {
      if (!((Verse.Thing) this).Spawned)
        return;
      this.vehiclePather.PatherFailed();
      this.ignition.Drafted = false;
    }));
    this.AddEvent<VehicleEventDef>(VehicleEventDefOf.ScanRare, new Action(this.statHandler.MarkAllDirty));
    this.AddEvent<VehicleEventDef>(VehicleEventDefOf.UpgradeCompleted, new Action(this.ResetRenderStatus), new Action(this.RecacheMovementPermissions));
    this.AddEvent<VehicleEventDef>(VehicleEventDefOf.UpgradeRefundCompleted, new Action(this.ResetRenderStatus), new Action(this.RecacheMovementPermissions));
    if (!GenDictionary.NullOrEmpty<VehicleEventDef, List<DynamicDelegate<VehiclePawn>>>((Dictionary<VehicleEventDef, List<DynamicDelegate<VehiclePawn>>>) this.VehicleDef.events))
    {
      foreach (KeyValuePair<VehicleEventDef, List<DynamicDelegate<VehiclePawn>>> keyValuePair in (Dictionary<VehicleEventDef, List<DynamicDelegate<VehiclePawn>>>) this.VehicleDef.events)
      {
        VehicleEventDef vehicleEventDef;
        List<DynamicDelegate<VehiclePawn>> dynamicDelegateList1;
        keyValuePair.Deconstruct(ref vehicleEventDef, ref dynamicDelegateList1);
        VehicleEventDef @event = vehicleEventDef;
        List<DynamicDelegate<VehiclePawn>> dynamicDelegateList2 = dynamicDelegateList1;
        if (!GenList.NullOrEmpty<DynamicDelegate<VehiclePawn>>((IList<DynamicDelegate<VehiclePawn>>) dynamicDelegateList2))
        {
          foreach (DynamicDelegate<VehiclePawn> dynamicDelegate in dynamicDelegateList2)
          {
            DynamicDelegate<VehiclePawn> method = dynamicDelegate;
            this.AddEvent<VehicleEventDef>(@event, (Action) (() => method.Invoke((object) null, this)));
          }
        }
      }
    }
    if (!GenList.NullOrEmpty<StatCache.EventLister>((IList<StatCache.EventLister>) this.VehicleDef.statEvents))
    {
      foreach (StatCache.EventLister statEvent in this.VehicleDef.statEvents)
      {
        StatCache.EventLister eventLister = statEvent;
        foreach (VehicleEventDef eventDef in eventLister.eventDefs)
          this.AddEvent<VehicleEventDef>(eventDef, (Action) (() => this.statHandler.MarkStatDirty(eventLister.statDef)));
      }
    }
    if (!GenList.NullOrEmpty<VehicleSoundEventEntry<VehicleEventDef>>((IList<VehicleSoundEventEntry<VehicleEventDef>>) this.VehicleDef.soundOneShotsOnEvent))
    {
      foreach (VehicleSoundEventEntry<VehicleEventDef> vehicleSoundEventEntry in this.VehicleDef.soundOneShotsOnEvent)
      {
        VehicleSoundEventEntry<VehicleEventDef> soundEventEntry = vehicleSoundEventEntry;
        this.AddEvent<VehicleEventDef>(soundEventEntry.key, (Action) (() => this.PlayOneShotOnVehicle<VehicleEventDef>(soundEventEntry)), soundEventEntry.removalKey);
      }
    }
    if (!GenList.NullOrEmpty<VehicleSustainerEventEntry<VehicleEventDef>>((IList<VehicleSustainerEventEntry<VehicleEventDef>>) this.VehicleDef.soundSustainersOnEvent))
    {
      foreach (VehicleSustainerEventEntry<VehicleEventDef> sustainerEventEntry in this.VehicleDef.soundSustainersOnEvent)
      {
        VehicleSustainerEventEntry<VehicleEventDef> soundEventEntry = sustainerEventEntry;
        this.AddEvent<VehicleEventDef>(soundEventEntry.start, (Action) (() => this.StartSustainerOnVehicle<VehicleEventDef>(soundEventEntry)), soundEventEntry.removalKey);
        this.AddEvent<VehicleEventDef>(soundEventEntry.stop, (Action) (() => this.StopSustainerOnVehicle<VehicleEventDef>(soundEventEntry)), soundEventEntry.removalKey);
      }
    }
    foreach (ThingComp allComp in ((ThingWithComps) this).AllComps)
    {
      if (allComp is VehicleComp vehicleComp)
        vehicleComp.EventRegistration();
    }
  }

  protected virtual void PostLoad()
  {
    this.RegisterEvents();
    this.RegenerateUnsavedComponents();
    this.RecacheComponents();
    this.RecachePawnCount();
    this.RecacheMovementPermissions();
    this.statHandler.MarkAllDirty();
    this.animator?.PostLoad();
    UnityThread.ExecuteOnMainThread(new Action(this.DrawTracker.overlayRenderer.Init));
    foreach (ThingComp allComp in ((ThingWithComps) this).AllComps)
    {
      if (allComp is VehicleComp vehicleComp)
        vehicleComp.PostLoad();
    }
  }

  protected virtual void RegenerateUnsavedComponents()
  {
    this.vehicleAI = new VehicleAI(this);
    this.drawTracker = new VehicleDrawTracker(this);
    if (this.sustainers != null)
      return;
    this.sustainers = new VehicleSustainers(this);
  }

  public virtual void SpawnSetup(Map map, bool respawningAfterLoad)
  {
    this.RegisterEvents();
    base.SpawnSetup(map, respawningAfterLoad);
    if (this.PropertyBlock == null)
      LongEventHandler.ExecuteWhenFinished((Action) (() => this.PropertyBlock = new MaterialPropertyBlock()));
    this.ReleaseSustainerTarget();
    this.EventRegistry[VehicleEventDefOf.Spawned].ExecuteEvents();
    if (this.Drafted)
      this.EventRegistry[VehicleEventDefOf.IgnitionOn].ExecuteEvents();
    if (this.sharedJob == null)
      this.sharedJob = new SharedJob();
    if (!respawningAfterLoad)
      this.vehiclePather.ResetToCurrentPosition();
    if (((Verse.Thing) this).Faction != Faction.OfPlayer)
    {
      this.ignition.Drafted = true;
      CompVehicleTurrets compVehicleTurrets = this.CompVehicleTurrets;
      if (compVehicleTurrets != null)
      {
        foreach (VehicleTurret turret in (IEnumerable<VehicleTurret>) compVehicleTurrets.Turrets)
        {
          turret.autoTargeting = true;
          turret.AutoTarget = true;
        }
      }
    }
    this.RecachePawnCount();
    this.RecacheMovementPermissions();
    this.UpdateRotationAndAngle();
    this.UpdateDraftController();
    this.DrawTracker.Notify_Spawned();
    this.InitializeHitbox();
    ((Verse.Thing) this).Map.GetCachedMapComponent<VehiclePathingSystem>().RequestGridsFor(this);
    ((Verse.Thing) this).Map.GetCachedMapComponent<ListerVehiclesRepairable>().NotifyVehicleSpawned(this);
    this.ResetRenderStatus();
    UnityThread.ExecuteOnMainThread(new Action(this.ReclaimPosition));
    if (respawningAfterLoad)
      return;
    UnityThread.ExecuteOnMainThread(new Action(Find.GameEnder.CheckOrUpdateGameOver));
  }

  public virtual void ExposeData()
  {
    if (Scribe.mode == 4)
      this.PostLoad();
    base.ExposeData();
    Scribe_Deep.Look<VehiclePathFollower>(ref this.vehiclePather, "vehiclePather", new object[1]
    {
      (object) this
    });
    Scribe_Deep.Look<VehicleIgnitionController>(ref this.ignition, "ignition", new object[1]
    {
      (object) this
    });
    Scribe_Deep.Look<VehicleStatHandler>(ref this.statHandler, "statHandler", new object[1]
    {
      (object) this
    });
    Scribe_Deep.Look<SharedJob>(ref this.sharedJob, "sharedJob", Array.Empty<object>());
    Scribe_Deep.Look<AnimationManager>(ref this.animator, "animator", new object[2]
    {
      (object) this,
      (object) this.VehicleDef.drawProperties.controller
    });
    Scribe_Values.Look<float>(ref this.angle, "angle", 0.0f, false);
    Scribe_Values.Look<bool>(ref this.reverse, "reverse", false, false);
    Scribe_Values.Look<bool>(ref this.crashLanded, "crashLanded", false, false);
    Scribe_Deep.Look<PatternData>(ref this.patternData, "patternData", Array.Empty<object>());
    Scribe_Defs.Look<RetextureDef>(ref this.retextureDef, "retextureDef");
    Scribe_Deep.Look<PatternData>(ref this.patternToPaint, "patternToPaint", Array.Empty<object>());
    Scribe_Values.Look<VehicleMovementStatus>(ref this.movementStatus, "movementStatus", VehicleMovementStatus.Online, false);
    Scribe_Values.Look<bool>(ref this.fishing, "fishing", false, false);
    Scribe_Collections.Look<TransferableOneWay>(ref this.cargoToLoad, "cargoToLoad", (LookMode) 2, Array.Empty<object>());
    Scribe_Collections.Look<VehicleRoleHandler>(ref this.handlers, "handlers", (LookMode) 2, Array.Empty<object>());
    Scribe_Collections.Look<AssignedSeat>(ref this.boardingAssignments, "boardingAssignments", (LookMode) 2, Array.Empty<object>());
    if (this.activatableComps == null)
      this.activatableComps = new List<ActivatableThingComp>();
    LoadSaveMode mode = Scribe.mode;
    if (mode != 2)
    {
      if (mode == 4)
      {
        this.CompUpgradeTree?.ReloadUnlocks();
        this.UpdateDraftController();
      }
    }
    else
    {
      using (new StatDirtyDisabler(this))
      {
        this.RecacheComponents();
        this.CompUpgradeTree?.ReactivateComps();
        this.LoadVarsActivatableComps();
      }
    }
    if (!GenList.NullOrEmpty<ThingComp>((IList<ThingComp>) this.deactivatedComps))
    {
      foreach (ThingComp deactivatedComp in this.deactivatedComps)
        deactivatedComp.PostExposeData();
    }
    if (VehicleMod.settings.main.useCustomShaders)
      return;
    this.patternData = new PatternData(this.VehicleDef.graphicData.color, this.VehicleDef.graphicData.colorTwo, this.VehicleDef.graphicData.colorThree, PatternDefOf.Default, Vector2.zero, 0.0f);
    this.retextureDef = (RetextureDef) null;
    this.patternToPaint = (PatternData) null;
  }

  public bool IsFishing => this.fishing;

  public virtual bool DeconstructibleBy(Faction faction)
  {
    return DebugSettings.godMode || ((Verse.Thing) this).Faction == faction;
  }

  public virtual AcceptanceReport ClaimableBy(Faction faction)
  {
    if (!((Verse.Thing) this).def.Claimable)
      return AcceptanceReport.op_Implicit(false);
    if (((Verse.Thing) this).Faction != null)
    {
      if (((Verse.Thing) this).Faction == faction)
        return AcceptanceReport.op_Implicit(false);
      if (faction == Faction.OfPlayer)
      {
        if (((Verse.Thing) this).Faction == Faction.OfInsects)
        {
          if (HiveUtility.AnyHivePreventsClaiming((Verse.Thing) this))
            return AcceptanceReport.op_Implicit(false);
        }
        else if (((Verse.Thing) this).Faction == Faction.OfMechanoids || ((Verse.Thing) this).Spawned && AnyHostileToolUserOfFaction(((Verse.Thing) this).Faction))
          return AcceptanceReport.op_Implicit(false);
      }
    }
    else if (((Verse.Thing) this).Spawned && ((Verse.Thing) this).Map.ParentFaction != null && ((Verse.Thing) this).Map.ParentFaction != Faction.OfPlayer && ((Verse.Thing) this).Map.ParentFaction.def.humanlikeFaction && AnyHostileToolUserOfFaction(((Verse.Thing) this).Map.ParentFaction))
      return AcceptanceReport.op_Implicit(false);
    return AcceptanceReport.op_Implicit(true);

    bool AnyHostileToolUserOfFaction(Faction ofFaction)
    {
      if (!((Verse.Thing) this).Spawned)
        return false;
      foreach (Pawn pawn in ((Verse.Thing) this).Map.mapPawns.SpawnedPawnsInFaction(ofFaction))
      {
        if (pawn.RaceProps.ToolUser && GenHostility.IsPotentialThreat((IAttackTarget) pawn))
          return true;
      }
      return false;
    }
  }

  internal bool IsThreatToAttackTargetSearcher(IAttackTargetSearcher attackTargetSearcher)
  {
    if (this.AllPawnsAboard.Count > 0)
      return true;
    foreach (ThingComp allComp in ((ThingWithComps) this).AllComps)
    {
      if (allComp is VehicleComp vehicleComp && vehicleComp.IsThreat(attackTargetSearcher))
        return true;
    }
    return false;
  }

  public void ReclaimPosition()
  {
    ((Verse.Thing) this).Map.GetDetachedMapComponent<VehiclePositionManager>().ClaimPosition(this);
  }

  public void Notify_Teleported(bool endCurrentJob = true, bool resetTweenedPos = true)
  {
    if (resetTweenedPos)
      this.DrawTracker.tweener.ResetTweenedPosToRoot();
    this.vehiclePather.Notify_Teleported();
    double angle = (double) this.CalculateAngle();
    if (!endCurrentJob || this.jobs == null || this.jobs.curJob == null)
      return;
    this.jobs.EndCurrentJob((JobCondition) 16 /*0x10*/, true, true);
  }

  public virtual AcceptanceReport CanDraft()
  {
    foreach (ThingComp allComp in ((ThingWithComps) this).AllComps)
    {
      if (allComp is VehicleComp vehicleComp)
      {
        AcceptanceReport acceptanceReport = vehicleComp.CanDraft();
        if (!((AcceptanceReport) ref acceptanceReport).Accepted)
          return acceptanceReport;
      }
    }
    return !VehicleMod.settings.debug.debugDraftAnyVehicle && !this.HasEnoughOperators ? AcceptanceReport.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_NotEnoughToOperate", NamedArgument.op_Implicit((Verse.Thing) this))) : AcceptanceReport.op_Implicit(true);
  }

  public int TotalAllowedFor(JobDef jobDef)
  {
    if (!VehicleMod.settings.main.multiplePawnsPerJob)
      return 1;
    foreach (VehicleJobLimitations vehicleJobLimitation in this.VehicleDef.properties.vehicleJobLimitations)
    {
      if (vehicleJobLimitation.defName == ((Def) jobDef).defName)
        return vehicleJobLimitation.maxWorkers;
    }
    return 1;
  }

  public void BeachShip()
  {
    this.movementStatus = VehicleMovementStatus.Offline;
    this.beached = true;
  }

  public void RemoveBeachedStatus()
  {
    this.movementStatus = VehicleMovementStatus.Online;
    this.beached = false;
  }

  public IntVec3 FollowerCell { get; protected set; }

  public virtual void ExitMap(bool allowedToJoinOrCreateCaravan, Rot4 exitDir)
  {
    base.ExitMap(allowedToJoinOrCreateCaravan, exitDir);
    foreach (Pawn pawn in this.AllPawnsAboard)
      LordUtility.GetLord(pawn)?.Notify_PawnLost(pawn, (PawnLostCondition) 6, new DamageInfo?());
  }

  public int AddOrTransfer([NotNull] Verse.Thing thing)
  {
    return this.AddOrTransfer(thing, thing.stackCount);
  }

  public int AddOrTransfer([NotNull] Verse.Thing thing, int count)
  {
    if (thing is Pawn pawn)
    {
      if (((Verse.Thing) pawn).Spawned)
        ((Entity) pawn).DeSpawn((DestroyMode) 0);
      if (WorldPawnsUtility.IsWorldPawn(pawn))
        Find.WorldPawns.RemovePawn(pawn);
    }
    int num = ((ThingOwner) this.inventory.innerContainer).TryAddOrTransfer(thing, count, true);
    this.EventRegistry[VehicleEventDefOf.CargoAdded].ExecuteEvents();
    if (pawn != null)
      this.EventRegistry[VehicleEventDefOf.PawnEntered].ExecuteEvents();
    TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatchingDesperate(thing, this.cargoToLoad, (TransferAsOneMode) 1);
    if (transferableOneWay != null)
    {
      VehiclePawn.CountToTransferFieldRef.Invoke(transferableOneWay) = ((Transferable) transferableOneWay).CountToTransfer - count;
      if (((Transferable) transferableOneWay).CountToTransfer <= 0)
        this.cargoToLoad.Remove(transferableOneWay);
    }
    return num;
  }

  public Verse.Thing TakeFromInventory(Verse.Thing thing)
  {
    return this.inventory.innerContainer.Take(thing, thing.stackCount);
  }

  public Verse.Thing TakeFromInventory(Verse.Thing thing, int count)
  {
    Verse.Thing fromInventory = this.inventory.innerContainer.Take(thing, count);
    this.EventRegistry[VehicleEventDefOf.CargoRemoved].ExecuteEvents();
    return fromInventory;
  }

  public void RecalculateFollowerCell()
  {
    IntVec3 intVec3 = IntVec3.Invalid;
    if (((Verse.Thing) this).Map != null)
    {
      Rot8 fullRotation = this.FullRotation;
      for (int index = 0; index < 8; ++index)
      {
        intVec3 = this.CalculateOffset(fullRotation);
        if (!GenGrid.InBounds(intVec3, ((Verse.Thing) this).Map))
          fullRotation.Rotate((RotationDirection) 1);
        else
          break;
      }
    }
    this.FollowerCell = intVec3;
  }

  private IntVec3 CalculateOffset(Rot8 rot)
  {
    int z = ((BuildableDef) this.VehicleDef).Size.z;
    int num1 = Mathf.CeilToInt((float) z * Mathf.Cos(0.7853982f));
    int num2 = Mathf.CeilToInt((float) z * Mathf.Sin(0.7853982f));
    IntVec3 positionHeld = ((Verse.Thing) this).PositionHeld;
    switch (rot.AsByte)
    {
      case 0:
        return new IntVec3(positionHeld.x, positionHeld.y, positionHeld.z - z);
      case 1:
        return new IntVec3(positionHeld.x - z, positionHeld.y, positionHeld.z);
      case 2:
        return new IntVec3(positionHeld.x, positionHeld.y, positionHeld.z + z);
      case 3:
        return new IntVec3(positionHeld.x + z, positionHeld.y, positionHeld.z);
      case 4:
        return new IntVec3(positionHeld.x - num1, positionHeld.y, positionHeld.z - num2);
      case 5:
        return new IntVec3(positionHeld.x - num1, positionHeld.y, positionHeld.z + num2);
      case 6:
        return new IntVec3(positionHeld.x + num1, positionHeld.y, positionHeld.z + num2);
      case 7:
        return new IntVec3(positionHeld.x + num1, positionHeld.y, positionHeld.z - num2);
      default:
        throw new NotImplementedException();
    }
  }

  public float PawnCollisionMultiplier
  {
    get
    {
      float num = SettingsCache.TryGetValue<float>(this.VehicleDef, typeof (VehicleProperties), "pawnCollisionMultiplier", this.VehicleDef.properties.pawnCollisionMultiplier);
      return this.statHandler.GetStatOffset(VehicleStatUpgradeCategoryDefOf.PawnCollisionMultiplier, num);
    }
  }

  public float PawnCollisionRecoilMultiplier
  {
    get
    {
      float num = SettingsCache.TryGetValue<float>(this.VehicleDef, typeof (VehicleProperties), "pawnCollisionRecoilMultiplier", this.VehicleDef.properties.pawnCollisionRecoilMultiplier);
      return this.statHandler.GetStatOffset(VehicleStatUpgradeCategoryDefOf.PawnCollisionRecoilMultiplier, num);
    }
  }

  public void CheckForCollisions(float moveSpeed)
  {
    CellRect cellRect = GenAdj.OccupiedRect((Verse.Thing) this);
    foreach (IntVec3 intVec3 in cellRect)
    {
      if (((Verse.Thing) this).Map.thingGrid.ThingAt(intVec3, (ThingCategory) 1) is Pawn pawn1 && !(pawn1 is VehiclePawn) && (FactionUtility.HostileTo(((Verse.Thing) pawn1).Faction, ((Verse.Thing) this).Faction) || ((Verse.Thing) this).Faction == null || Rand.Chance(VehicleDamager.FriendlyFireChance(this, pawn1))))
      {
        (float pawnDamage, float vehicleDamage) = VehiclePawn.CalculateImpactDamage(pawn1, this, moveSpeed);
        VehicleRoleHandler vehicleRoleHandler = this.GetHandlers(HandlingType.Movement).FirstOrDefault<VehicleRoleHandler>((Func<VehicleRoleHandler, bool>) (handler => ((ThingOwner) handler.thingOwner).Any));
        Pawn pawn = (vehicleRoleHandler != null ? vehicleRoleHandler.thingOwner.InnerListForReading.First<Pawn>() : (Pawn) null) ?? (Pawn) this;
        IntVec3 position = ((Verse.Thing) pawn1).Position;
        ((Verse.Thing) pawn1).TakeDamage(new DamageInfo(DamageDefOf.Blunt, pawnDamage, 0.0f, -1f, (Verse.Thing) pawn, (BodyPartRecord) null, (ThingDef) null, (DamageInfo.SourceCategory) 0, (Verse.Thing) null, true, true, (QualityCategory) 2, true, false));
        this.TryTakeDamage(new DamageInfo(DamageDefOf.Blunt, vehicleDamage, 0.0f, -1f, (Verse.Thing) pawn1, (BodyPartRecord) null, (ThingDef) null, (DamageInfo.SourceCategory) 0, (Verse.Thing) null, false, true, (QualityCategory) 2, true, false), position, out DamageWorker.DamageResult _);
      }
    }
  }

  public static (float pawnDamage, float vehicleDamage) CalculateImpactDamage(
    Pawn pawn,
    VehiclePawn vehicle,
    float velocity)
  {
    float statValue1 = vehicle.GetStatValue(VehicleStatDefOf.Mass);
    float baseBodySize = pawn.RaceProps.baseBodySize;
    float num1 = (float) (0.5 * (double) statValue1 * ((double) velocity * (double) velocity));
    float num2 = (float) ((double) vehicle.PawnCollisionMultiplier * (double) num1 / 100.0) * baseBodySize;
    float statValue2 = StatExtension.GetStatValue((Verse.Thing) vehicle, StatDefOf.ArmorRating_Blunt, true, -1);
    float num3 = (float) ((double) vehicle.PawnCollisionRecoilMultiplier * (2.0 - (double) statValue2) * (double) num1 * (double) baseBodySize / 200.0);
    return (num2, num3);
  }

  public CompVehicleTurrets CompVehicleTurrets
  {
    get
    {
      if (!this.fetchedCompVehicleTurrets)
      {
        this.compVehicleTurrets = this.GetCachedComp<CompVehicleTurrets>();
        this.fetchedCompVehicleTurrets = true;
      }
      return this.compVehicleTurrets;
    }
  }

  public CompFueledTravel CompFueledTravel
  {
    get
    {
      if (!this.fetchedCompFuel)
      {
        this.compFuel = this.GetCachedComp<CompFueledTravel>();
        this.fetchedCompFuel = true;
      }
      return this.compFuel;
    }
  }

  public CompUpgradeTree CompUpgradeTree
  {
    get
    {
      if (!this.fetchedCompUpgradeTree)
      {
        this.compUpgradeTree = this.GetCachedComp<CompUpgradeTree>();
        this.fetchedCompUpgradeTree = true;
      }
      return this.compUpgradeTree;
    }
  }

  public CompVehicleLauncher CompVehicleLauncher
  {
    get
    {
      if (!this.fetchedCompVehicleLauncher)
      {
        this.compVehicleLauncher = this.GetCachedComp<CompVehicleLauncher>();
        this.fetchedCompVehicleLauncher = true;
      }
      return this.compVehicleLauncher;
    }
  }

  public void AddComp(ThingComp thingComp)
  {
    ((ThingWithComps) this).AllComps.Add(thingComp);
    this.RecacheComponents();
  }

  public bool RemoveComp(ThingComp thingComp)
  {
    bool flag = ((ThingWithComps) this).AllComps.Remove(thingComp);
    if (flag)
      this.RecacheComponents();
    return flag;
  }

  public void ActivateComp(ThingComp comp)
  {
    ActivatableThingComp activatableThingComp = GenCollection.FirstOrDefault<ActivatableThingComp>(this.activatableComps, (Predicate<ActivatableThingComp>) (activatableComp => activatableComp.Type == comp.GetType()));
    if (activatableThingComp == null)
    {
      activatableThingComp = new ActivatableThingComp(this);
      activatableThingComp.Init(comp);
      this.activatableComps.Add(activatableThingComp);
    }
    ++activatableThingComp.Owners;
  }

  public void DeactivateComp(ThingComp comp)
  {
    foreach (ActivatableThingComp activatableComp in this.activatableComps)
    {
      if (activatableComp.Type == comp.GetType())
      {
        --activatableComp.Owners;
        break;
      }
    }
  }

  public T GetCachedComp<T>() where T : ThingComp
  {
    for (int index = 0; index < this.cachedComps.Count; ++index)
    {
      if (this.cachedComps[index] is T cachedComp)
      {
        this.cachedComps.Touch(index);
        return cachedComp;
      }
    }
    return default (T);
  }

  public ThingComp GetComp(System.Type type)
  {
    foreach (ThingComp allComp in ((ThingWithComps) this).AllComps)
    {
      if (GenTypes.SameOrSubclassOf(allComp.GetType(), type))
        return allComp;
    }
    return (ThingComp) null;
  }

  public ThingComp GetDeactivatedComp(System.Type type)
  {
    foreach (ThingComp deactivatedComp in this.deactivatedComps)
    {
      if (GenTypes.SameOrSubclassOf(deactivatedComp.GetType(), type))
        return deactivatedComp;
    }
    return (ThingComp) null;
  }

  protected virtual void RecacheComponents()
  {
    this.fetchedCompVehicleTurrets = false;
    this.fetchedCompFuel = false;
    this.fetchedCompUpgradeTree = false;
    this.fetchedCompVehicleLauncher = false;
    this.cachedComps.Clear();
    if (!GenList.NullOrEmpty<ThingComp>((IList<ThingComp>) ((ThingWithComps) this).AllComps))
      this.cachedComps.AddRange((IEnumerable<ThingComp>) ((ThingWithComps) this).AllComps);
    this.RecacheCompTickers();
  }

  private void RecacheCompTickers()
  {
    this.compTickers.Clear();
    foreach (ThingComp allComp in ((ThingWithComps) this).AllComps)
    {
      if (!(allComp is VehicleComp vehicleComp) || !vehicleComp.TickByRequest)
        this.compTickers.Add(allComp);
    }
  }

  private void LoadVarsActivatableComps()
  {
    if (this.CompUpgradeTree == null || GenList.NullOrEmpty<ActivatableThingComp>((IList<ActivatableThingComp>) this.activatableComps))
      return;
    foreach (ActivatableThingComp activatableComp in this.activatableComps)
    {
      if (activatableComp.Comp == null)
      {
        Log.Error("Unable to load variables from " + (activatableComp.Type?.Name ?? "NULL"));
        break;
      }
      activatableComp.Comp.PostExposeData();
    }
  }

  public List<VehicleRoleHandler> Handlers => this.handlers;

  public List<VehicleRoleHandler> OccupiedHandlers { get; private set; } = new List<VehicleRoleHandler>();

  public List<Pawn> AllPawnsAboard { get; private set; } = new List<Pawn>();

  public List<Pawn> AllColonistsAboard { get; private set; } = new List<Pawn>();

  public List<Pawn> AllInventoryPawns { get; private set; } = new List<Pawn>();

  public Dictionary<HandlingType, List<Pawn>> PawnsByHandlingType { get; private set; } = new Dictionary<HandlingType, List<Pawn>>()
  {
    [HandlingType.None] = new List<Pawn>(),
    [HandlingType.Movement] = new List<Pawn>(),
    [HandlingType.Turret] = new List<Pawn>()
  };

  public int PawnCountToOperate
  {
    get
    {
      int pawnCountToOperate = 0;
      foreach (VehicleRoleHandler handler in this.handlers)
      {
        if ((handler.role.HandlingTypes & HandlingType.Movement) != HandlingType.None)
          pawnCountToOperate += handler.role.SlotsToOperate;
      }
      return pawnCountToOperate;
    }
  }

  public int PawnCountToOperateLeft
  {
    get => this.PawnCountToOperate - this.PawnsByHandlingType[HandlingType.Movement].Count;
  }

  [Obsolete("Use HasEnoughOperators instead. Will be removed in 1.7")]
  public bool CanMoveWithOperators => this.HasEnoughOperators;

  public bool HasEnoughOperators
  {
    get
    {
      if ((this.MovementPermissions & VehiclePermissions.Autonomous) != VehiclePermissions.None || VehicleMod.settings.debug.debugDraftAnyVehicle)
        return true;
      foreach (VehicleRoleHandler handler in this.handlers)
      {
        if ((handler.role.HandlingTypes & HandlingType.Movement) != HandlingType.None && !handler.RoleFulfilled)
          return false;
      }
      return true;
    }
  }

  public List<Pawn> Passengers => this.PawnsByHandlingType[HandlingType.None];

  public List<Pawn> AllCapablePawns
  {
    get
    {
      List<Pawn> allCapablePawns = new List<Pawn>();
      List<VehicleRoleHandler> handlers = this.handlers;
      if (handlers != null && handlers.Count > 0)
      {
        foreach (VehicleRoleHandler handler in this.handlers)
        {
          foreach (Pawn pawn in handler.thingOwner)
          {
            if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
              allCapablePawns.Add(pawn);
          }
        }
      }
      return allCapablePawns;
    }
  }

  public int SeatsAvailable
  {
    get
    {
      int seatsAvailable = 0;
      foreach (VehicleRoleHandler handler in this.handlers)
        seatsAvailable += handler.role.Slots - ((ThingOwner) handler.thingOwner).Count;
      return seatsAvailable;
    }
  }

  public int TotalSeats
  {
    get
    {
      int totalSeats = 0;
      foreach (VehicleRoleHandler handler in this.handlers)
        totalSeats += handler.role.Slots;
      return totalSeats;
    }
  }

  private void RecacheInventoryPawns()
  {
    this.AllInventoryPawns.Clear();
    foreach (Verse.Thing thing in this.inventory.innerContainer)
    {
      if (thing is Pawn pawn)
        this.AllInventoryPawns.Add(pawn);
    }
  }

  public void RecachePawnCount()
  {
    GenList.ClearValueLists<HandlingType, Pawn>(this.PawnsByHandlingType);
    this.OccupiedHandlers.Clear();
    this.AllPawnsAboard.Clear();
    this.AllColonistsAboard.Clear();
    foreach (VehicleRoleHandler handler in this.handlers)
    {
      if (((ThingOwner) handler.thingOwner).Any)
      {
        this.OccupiedHandlers.Add(handler);
        foreach (Pawn pawn in handler.thingOwner)
        {
          this.AllPawnsAboard.Add(pawn);
          if (pawn.IsColonist)
            this.AllColonistsAboard.Add(pawn);
          if (handler.role.HandlingTypes == HandlingType.None)
          {
            this.PawnsByHandlingType[HandlingType.None].Add(pawn);
          }
          else
          {
            TryAddToCache(pawn, handler.role.HandlingTypes, HandlingType.Movement, this.PawnsByHandlingType);
            TryAddToCache(pawn, handler.role.HandlingTypes, HandlingType.Turret, this.PawnsByHandlingType);
          }
        }
      }
    }

    static void TryAddToCache(
      Pawn pawn,
      HandlingType value,
      HandlingType mask,
      Dictionary<HandlingType, List<Pawn>> cache)
    {
      if ((value & mask) != mask)
        return;
      cache[mask].Add(pawn);
    }
  }

  public void AddRole(VehicleRole role)
  {
    role.ResolveReferences(this.VehicleDef);
    this.handlers.Add(new VehicleRoleHandler(this, role));
    this.handlers.Sort();
    this.ResetRenderStatus();
  }

  public void RemoveRole(VehicleRole role)
  {
    this.DisembarkAll();
    for (int index = this.handlers.Count - 1; index >= 0; --index)
    {
      VehicleRoleHandler handler = this.handlers[index];
      if (handler.role.key == role.key)
      {
        this.DrawTracker.RemoveRenderer((IParallelRenderer) handler);
        this.handlers.RemoveAt(index);
      }
    }
  }

  public void RemoveRole(string roleKey)
  {
    this.DisembarkAll();
    for (int index = this.handlers.Count - 1; index >= 0; --index)
    {
      VehicleRoleHandler handler = this.handlers[index];
      if (handler.role.key == roleKey)
      {
        this.DrawTracker.RemoveRenderer((IParallelRenderer) handler);
        this.handlers.RemoveAt(index);
      }
    }
  }

  [Pure]
  public VehicleRoleHandler GetHandler(string roleKey)
  {
    foreach (VehicleRoleHandler handler in this.handlers)
    {
      if (handler.role.key == roleKey)
        return handler;
    }
    return (VehicleRoleHandler) null;
  }

  [Pure]
  public IEnumerable<VehicleRoleHandler> GetHandlers(HandlingType handlingTypeFlag)
  {
    return handlingTypeFlag == HandlingType.None ? this.handlers.Where<VehicleRoleHandler>((Func<VehicleRoleHandler, bool>) (handler => handler.role.HandlingTypes == HandlingType.None)) : this.handlers.Where<VehicleRoleHandler>((Func<VehicleRoleHandler, bool>) (handler => (handler.role.HandlingTypes & handlingTypeFlag) == handlingTypeFlag));
  }

  [Pure]
  public VehicleRoleHandler GetAnyAvailableHandler()
  {
    foreach (VehicleRoleHandler handler in this.handlers)
    {
      if (handler.AreSlotsAvailableAndReservable)
        return handler;
    }
    return (VehicleRoleHandler) null;
  }

  [Pure]
  [Obsolete("Use overload with pawn for permissions check.")]
  public VehicleRoleHandler GetNextAvailableHandler(HandlingType handlingTypeFlag)
  {
    foreach (VehicleRoleHandler handler in this.handlers)
    {
      if (handlingTypeFlag == HandlingType.None)
      {
        if (handler.role.HandlingTypes == HandlingType.None || handler.AreSlotsAvailableAndReservable)
          return handler;
      }
      else if ((handler.role.HandlingTypes & handlingTypeFlag) == handlingTypeFlag && handler.AreSlotsAvailableAndReservable)
        return handler;
    }
    return (VehicleRoleHandler) null;
  }

  [Pure]
  public VehicleRoleHandler GetNextAvailableHandler(Pawn pawn, HandlingType handlingTypeFlag)
  {
    foreach (VehicleRoleHandler handler in this.handlers)
    {
      if (handlingTypeFlag == HandlingType.None)
      {
        if (handler.role.HandlingTypes == HandlingType.None || handler.AreSlotsAvailableAndReservable)
          return handler;
      }
      else if (handler.CanOperateRole(pawn) && (handler.role.HandlingTypes & handlingTypeFlag) == handlingTypeFlag && handler.AreSlotsAvailableAndReservable)
        return handler;
    }
    return (VehicleRoleHandler) null;
  }

  [Pure]
  public VehicleRoleHandler GetHighestPriorityAvailableHandler()
  {
    foreach (VehicleRoleHandler availableHandler in (IEnumerable<VehicleRoleHandler>) this.handlers.OrderBy<VehicleRoleHandler, VehicleRoleHandler>((Func<VehicleRoleHandler, VehicleRoleHandler>) (handler => handler)))
    {
      if (availableHandler.AreSlotsAvailableAndReservable)
        return availableHandler;
    }
    return (VehicleRoleHandler) null;
  }

  public void GiveLoadJob(Pawn pawn, VehicleRoleHandler handler)
  {
    if (this.boardingAssignments.Count > 0)
    {
      AssignedSeat assignedSeat = GenCollection.FirstOrDefault<AssignedSeat>(this.boardingAssignments, (Predicate<AssignedSeat>) (assignment => assignment.pawn == pawn));
      if (assignedSeat != null)
      {
        assignedSeat.handler = handler;
        return;
      }
    }
    this.boardingAssignments.Add(new AssignedSeat(pawn, handler));
  }

  public bool BoardPawn(Pawn pawn)
  {
    if (this.boardingAssignments.Count > 0)
    {
      AssignedSeat assignedSeat = GenCollection.FirstOrDefault<AssignedSeat>(this.boardingAssignments, (Predicate<AssignedSeat>) (assignment => assignment.pawn == pawn));
      if (assignedSeat != null)
      {
        if (WorldPawnsUtility.IsWorldPawn(pawn))
        {
          Log.Error("Tried boarding vehicle with world pawn. Use Notify_BoardedCaravan instead.");
          return false;
        }
        if (!this.TryAddPawn(pawn, assignedSeat.handler))
          return false;
        this.boardingAssignments.Remove(assignedSeat);
        return true;
      }
    }
    return false;
  }

  public bool TryAddPawn(Pawn pawn)
  {
    if (pawn.ShouldAlwaysTransferToVehiclesCargo())
    {
      this.AddOrTransfer((Verse.Thing) pawn);
      return true;
    }
    if (GenList.NullOrEmpty<VehicleRoleHandler>((IList<VehicleRoleHandler>) this.handlers))
      return false;
    foreach (VehicleRoleHandler handler in this.handlers)
    {
      if ((handler.role.HandlingTypes == HandlingType.None || handler.CanOperateRole(pawn)) && this.TryAddPawn(pawn, handler))
        return true;
    }
    return false;
  }

  public bool TryAddPawn(Pawn pawn, VehicleRoleHandler handler)
  {
    VehicleReservationManager reservationManager = (VehicleReservationManager) null;
    if (((Verse.Thing) this).Spawned)
    {
      reservationManager = ((Verse.Thing) this).Map.GetCachedMapComponent<VehicleReservationManager>();
      if (!reservationManager.ReservedBy<VehicleRoleHandler, VehicleHandlerReservation>(this, pawn, handler) && !handler.AreSlotsAvailable)
        return false;
    }
    if (!handler.AreSlotsAvailable)
      return false;
    if (((Verse.Thing) pawn).Spawned)
      ((Entity) pawn).DeSpawn((DestroyMode) 0);
    if (WorldPawnsUtility.IsWorldPawn(pawn))
      Find.WorldPawns.RemovePawn(pawn);
    bool flag = true;
    if (!((ThingOwner) handler.thingOwner).TryAddOrTransfer((Verse.Thing) pawn, false) && ((Verse.Thing) pawn).holdingOwner != null)
      flag = ((Verse.Thing) pawn).holdingOwner.TryTransferToContainer((Verse.Thing) pawn, (ThingOwner) handler.thingOwner, true);
    reservationManager?.ReleaseAllClaimedBy(pawn);
    if (flag)
      this.EventRegistry[VehicleEventDefOf.PawnEntered].ExecuteEvents();
    this.GetVehicleCaravan()?.RecacheVehicles();
    return flag;
  }

  public bool RemovePawn(Pawn pawn)
  {
    foreach (VehicleRoleHandler handler in this.handlers)
    {
      if (this.TryRemovePawn(pawn, handler))
        return true;
    }
    return ((ThingOwner) this.inventory.innerContainer).Remove((Verse.Thing) pawn);
  }

  public bool TryRemovePawn(Pawn pawn, VehicleRoleHandler handler)
  {
    if (!((ThingOwner) handler.thingOwner).Remove((Verse.Thing) pawn))
      return false;
    this.EventRegistry[VehicleEventDefOf.PawnRemoved].ExecuteEvents();
    this.GetVehicleCaravan()?.RecacheVehicles();
    if (((Verse.Thing) this).Spawned)
      ((Verse.Thing) this).Map.GetCachedMapComponent<VehicleReservationManager>().ReleaseAllClaimedBy(pawn);
    return true;
  }

  public void DisembarkPawn(Pawn pawn)
  {
    VehicleCaravan vehicleCaravan = this.GetVehicleCaravan();
    if (vehicleCaravan != null)
    {
      this.RemovePawn(pawn);
      vehicleCaravan.AddPawn(pawn, true);
      Find.WorldPawns.PassToWorld(pawn, (PawnDiscardDecideMode) 0);
    }
    else
    {
      if (!this.RemovePawn(pawn))
        return;
      this.SpawnPawnNearVehicle(pawn);
      this.EventRegistry[VehicleEventDefOf.PawnExited].ExecuteEvents();
    }
  }

  public void DisembarkAllFromInventory()
  {
    VehicleCaravan vehicleCaravan = this.GetVehicleCaravan();
    if (vehicleCaravan != null)
    {
      for (int index = ((ThingOwner) this.inventory.innerContainer).Count - 1; index >= 0; --index)
      {
        if (this.inventory.innerContainer[index] is Pawn pawn)
        {
          ((ThingOwner) this.inventory.innerContainer).RemoveAt(index);
          vehicleCaravan.AddPawn(pawn, true);
          Find.WorldPawns.PassToWorld(pawn, (PawnDiscardDecideMode) 0);
          this.EventRegistry[VehicleEventDefOf.PawnExited].ExecuteEvents();
        }
      }
    }
    else if (((Verse.Thing) this).Spawned)
    {
      using (new EventDisabler<VehicleEventDef>((IEventControl) this.EventRegistry[VehicleEventDefOf.PawnExited]))
      {
        for (int index = ((ThingOwner) this.inventory.innerContainer).Count - 1; index >= 0; --index)
        {
          if (this.inventory.innerContainer[index] is Pawn pawn)
            this.DisembarkPawn(pawn);
        }
      }
      this.EventRegistry[VehicleEventDefOf.PawnExited].ExecuteEvents();
    }
    else
    {
      SmashTools.Trace.Fail("Disembarking from vehicle when it is not spawned or in a caravan.");
      for (int index = ((ThingOwner) this.inventory.innerContainer).Count - 1; index >= 0; --index)
      {
        if (this.inventory.innerContainer[index] is Pawn pawn)
        {
          ((ThingOwner) this.inventory.innerContainer).RemoveAt(index);
          Find.WorldPawns.PassToWorld(pawn, (PawnDiscardDecideMode) 0);
          this.EventRegistry[VehicleEventDefOf.PawnRemoved].ExecuteEvents();
        }
      }
    }
  }

  public void DisembarkAll()
  {
    VehicleCaravan vehicleCaravan = this.GetVehicleCaravan();
    if (vehicleCaravan != null)
    {
      foreach (VehicleRoleHandler handler in this.handlers)
      {
        int count = ((ThingOwner) handler.thingOwner).Count;
        while (--count >= 0)
        {
          Pawn pawn = handler.thingOwner[count];
          ((ThingOwner) handler.thingOwner).Remove((Verse.Thing) pawn);
          vehicleCaravan.AddPawn(pawn, true);
          Find.WorldPawns.PassToWorld(pawn, (PawnDiscardDecideMode) 0);
          this.EventRegistry[VehicleEventDefOf.PawnExited].ExecuteEvents();
        }
      }
    }
    else if (((Verse.Thing) this).Spawned)
    {
      using (new EventDisabler<VehicleEventDef>((IEventControl) this.EventRegistry[VehicleEventDefOf.PawnExited]))
      {
        for (int index = this.AllPawnsAboard.Count - 1; index >= 0; --index)
          this.DisembarkPawn(this.AllPawnsAboard[index]);
      }
      this.EventRegistry[VehicleEventDefOf.PawnExited].ExecuteEvents();
    }
    else
    {
      SmashTools.Trace.Fail("Disembarking from vehicle when it is not spawned or in a caravan.");
      foreach (VehicleRoleHandler handler in this.handlers)
      {
        int count = ((ThingOwner) handler.thingOwner).Count;
        while (--count >= 0)
        {
          Pawn pawn = handler.thingOwner[count];
          this.TryRemovePawn(pawn, handler);
          Find.WorldPawns.PassToWorld(pawn, (PawnDiscardDecideMode) 0);
        }
      }
    }
  }

  internal VehicleComponent HighlightedComponent { get; set; }

  public VehiclePermissions MovementPermissions { get; private set; }

  public bool CanMove
  {
    get
    {
      return (double) this.GetStatValue(VehicleStatDefOf.MoveSpeed) > 0.10000000149011612 && (this.MovementPermissions & VehiclePermissions.Mobile) != VehiclePermissions.None && this.movementStatus == VehicleMovementStatus.Online;
    }
  }

  public bool CanMoveFinal => this.CanMove && this.HasEnoughOperators;

  public CellRect Hitbox { get; private set; }

  public bool CanFish
  {
    get
    {
      foreach (VehicleRoleHandler handler in this.handlers)
      {
        foreach (Pawn pawn in handler.thingOwner)
        {
          SkillRecord skill = pawn.skills?.GetSkill(SkillDefOf.Animals);
          if (skill != null && !skill.TotallyDisabled)
            return true;
        }
      }
      return false;
    }
  }

  public float WorldSpeedMultiplier
  {
    get
    {
      float num = SettingsCache.TryGetValue<float>(this.VehicleDef, typeof (VehicleProperties), "worldSpeedMultiplier", this.VehicleDef.properties.worldSpeedMultiplier);
      return this.statHandler.GetStatOffset(VehicleStatUpgradeCategoryDefOf.WorldSpeedMultiplier, num);
    }
  }

  public IEnumerable<IntVec3> SurroundingCells
  {
    get
    {
      CellRect cellRect = GenAdj.OccupiedRect((Verse.Thing) this);
      cellRect = ((CellRect) ref cellRect).ExpandedBy(1);
      return ((CellRect) ref cellRect).EdgeCells;
    }
  }

  public float GetStatValue(VehicleStatDef statDef) => this.statHandler.GetStatValue(statDef);

  private void RecacheMovementPermissions()
  {
    this.MovementPermissions = VehiclePermissions.Mobile | VehiclePermissions.Autonomous;
    if (Mathf.Approximately(this.VehicleDef.GetStatValueAbstract(VehicleStatDefOf.MoveSpeed), 0.0f))
      this.MovementPermissions &= ~VehiclePermissions.Mobile;
    foreach (VehicleRoleHandler handler in this.handlers)
    {
      if ((handler.role.HandlingTypes & HandlingType.Movement) != HandlingType.None)
      {
        this.MovementPermissions &= ~VehiclePermissions.Autonomous;
        break;
      }
    }
  }

  public IEnumerable<IntVec3> InhabitedCells(int expandedBy = 0)
  {
    return this.InhabitedCellsProjected(((Verse.Thing) this).Position, this.FullRotation, expandedBy);
  }

  public IEnumerable<IntVec3> InhabitedCellsProjected(
    IntVec3 projectedCell,
    Rot8 rot,
    int expandedBy = 0)
  {
    bool maxSizePossible = !rot.IsValid;
    CellRect cellRect = this.VehicleRect(projectedCell, (Rot4) rot, maxSizePossible);
    cellRect = ((CellRect) ref cellRect).ExpandedBy(expandedBy);
    return ((CellRect) ref cellRect).Cells;
  }

  private void InitializeHitbox()
  {
    this.Hitbox = this.VehicleRect(IntVec3.Zero, Rot4.North);
    this.statHandler.InitializeHitboxCells();
  }

  public virtual void Notify_TookDamage()
  {
    if (!((Verse.Thing) this).Spawned)
      return;
    this.animator?.SetBool(PropertyIds.Disabled, this.CanMove);
    ((Verse.Thing) this).Map.GetCachedMapComponent<ListerVehiclesRepairable>().NotifyVehicleTookDamage(this);
  }

  public bool TryTakeDamage(
    DamageInfo dinfo,
    IntVec3 position,
    out DamageWorker.DamageResult result)
  {
    result = new DamageWorker.DamageResult();
    CellRect cellRect = GenAdj.OccupiedRect((Verse.Thing) this);
    if (!((CellRect) ref cellRect).Contains(position))
      return false;
    IntVec2 cell;
    // ISSUE: explicit constructor call
    ((IntVec2) ref cell).\u002Ector(position.x - ((Verse.Thing) this).Position.x, position.z - ((Verse.Thing) this).Position.z);
    result = this.TakeDamage(dinfo, cell);
    return true;
  }

  public virtual DamageWorker.DamageResult TakeDamage(DamageInfo dinfo, IntVec2 cell)
  {
    this.statHandler.TakeDamage(dinfo, cell);
    return new DamageWorker.DamageResult();
  }

  public virtual void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
  {
    this.statHandler.TakeDamage(dinfo);
    absorbed = true;
  }

  public bool TryDamageObstructions()
  {
    if (this.CellRectStandable(((Verse.Thing) this).Map, new IntVec3?(((Verse.Thing) this).Position), new Rot4?(((Verse.Thing) this).Rotation)))
      return false;
    List<Verse.Thing> thingList = SimplePool<List<Verse.Thing>>.Get();
    CellRect cellRect = GenAdj.OccupiedRect((Verse.Thing) this);
    foreach (IntVec3 intVec3_1 in cellRect)
    {
      thingList.AddRange((IEnumerable<Verse.Thing>) ((Verse.Thing) this).Map.thingGrid.ThingsListAt(intVec3_1));
      foreach (Verse.Thing thing in thingList)
      {
        if (thing.def.useHitPoints && (double) thing.def.fillPercent > 0.15000000596046448)
        {
          float num = (float) thing.HitPoints / 10f;
          IntVec3 intVec3_2 = IntVec3.op_Subtraction(((Verse.Thing) this).Position, thing.Position);
          this.TakeDamage(new DamageInfo(DamageDefOf.Blunt, num, 0.0f, -1f, (Verse.Thing) null, (BodyPartRecord) null, (ThingDef) null, (DamageInfo.SourceCategory) 0, (Verse.Thing) null, true, true, (QualityCategory) 2, true, false), ((IntVec3) ref intVec3_2).ToIntVec2);
          thing.Destroy((DestroyMode) 2);
        }
      }
      thingList.Clear();
    }
    thingList.Clear();
    SimplePool<List<Verse.Thing>>.Return(thingList);
    return true;
  }

  public virtual void DeSpawn(DestroyMode mode = 0)
  {
    this.vehiclePather.StopDead();
    ((Verse.Thing) this).Map.GetDetachedMapComponent<VehiclePositionManager>().ReleaseClaimed(this);
    VehicleReservationManager cachedMapComponent = ((Verse.Thing) this).Map.GetCachedMapComponent<VehicleReservationManager>();
    cachedMapComponent.ClearReservedFor(this);
    cachedMapComponent.RemoveAllListerFor(this);
    this.cargoToLoad.Clear();
    ((Verse.Thing) this).Map.GetCachedMapComponent<ListerVehiclesRepairable>().NotifyVehicleDespawned(this);
    this.EventRegistry[VehicleEventDefOf.Despawned].ExecuteEvents();
    if (!GenList.NullOrEmpty<ThingComp>((IList<ThingComp>) this.cachedComps))
    {
      for (int index = 0; index < this.cachedComps.Count; ++index)
      {
        if (this.cachedComps[index] is VehicleComp cachedComp)
          cachedComp.OnDeSpawn();
      }
    }
    base.DeSpawn(mode);
    this.SoundCleanup();
  }

  public virtual void Destroy(DestroyMode mode = 0)
  {
    if (this.AllPawnsAboard.Count > 0)
      this.DisembarkAll();
    this.sustainers.EndAll();
    this.EventRegistry?[VehicleEventDefOf.Destroyed].ExecuteEvents();
    RGBMaterialPool.Release((IMaterialCacheTarget) this);
    this.graphic = (Graphic_Vehicle) null;
    if (!GenList.NullOrEmpty<ThingComp>((IList<ThingComp>) this.cachedComps))
    {
      for (int index = 0; index < this.cachedComps.Count; ++index)
      {
        if (this.cachedComps[index] is VehicleComp cachedComp)
          cachedComp.OnDestroy();
      }
    }
    base.Destroy(mode);
    Find.GameEnder.CheckOrUpdateGameOver();
    if (!Find.WorldPawns.Contains((Pawn) this))
      return;
    Find.WorldPawns.RemoveAndDiscardPawnViaGC((Pawn) this);
  }

  public virtual void DestroyPawns(DestroyMode mode = 0)
  {
    for (int index = this.AllPawnsAboard.Count - 1; index >= 0; --index)
    {
      Pawn pawn = this.AllPawnsAboard[index];
      this.AllPawnsAboard.RemoveAt(index);
      ((Verse.Thing) pawn).Destroy(mode);
    }
    for (int index = ((ThingOwner) this.inventory.innerContainer).Count - 1; index >= 0; --index)
    {
      if (this.inventory.innerContainer[index] is Pawn pawn)
      {
        ((ThingOwner) this.inventory.innerContainer).RemoveAt(index);
        ((Verse.Thing) pawn).Destroy(mode);
      }
    }
  }

  public virtual void DestroyVehicleAndPawns(DestroyMode mode = 0)
  {
    this.DestroyPawns(mode);
    ((Verse.Thing) this).Destroy(mode);
  }

  public virtual void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
  {
    this.Kill(dinfo, (DestroyMode) 2);
  }

  public virtual void Kill(DamageInfo? dinfo, DestroyMode destroyMode = 2, bool spawnWreckage = false)
  {
    IntVec3 positionHeld = ((Verse.Thing) this).PositionHeld;
    Rot4 rotation = ((Verse.Thing) this).Rotation;
    Map map = ((Verse.Thing) this).Map;
    bool spawned = ((Verse.Thing) this).Spawned;
    ThingDef buildDef = (ThingDef) this.VehicleDef.buildDef;
    if (Current.ProgramState == 2)
      Find.Storyteller.Notify_PawnEvent((Pawn) this, (AdaptationEvent) 1, new DamageInfo?());
    if (dinfo.HasValue)
    {
      DamageInfo valueOrDefault = dinfo.GetValueOrDefault();
      if (((DamageInfo) ref valueOrDefault).Instigator is Pawn instigator)
        RecordsUtility.Notify_PawnKilled((Pawn) this, instigator);
    }
    if (LordUtility.GetLord((Pawn) this) != null)
      LordUtility.GetLord((Pawn) this).Notify_PawnLost((Pawn) this, (PawnLostCondition) 3, dinfo);
    if (spawned)
    {
      this.DropAndForbidEverything(false, false);
      if (destroyMode != 2)
      {
        if (destroyMode == 4)
          SoundStarter.PlayOneShot(SoundDefOf.Building_Deconstructed, SoundInfo.op_Implicit(new TargetInfo(((Verse.Thing) this).Position, map, false)));
      }
      else
        this.DoDestroyEffects(map);
    }
    VehicleBuilding vehicleBuilding = (VehicleBuilding) ThingMaker.MakeThing(buildDef, (ThingDef) null);
    ((Verse.Thing) vehicleBuilding).SetFactionDirect(((Verse.Thing) this).Faction);
    vehicleBuilding.vehicle = this;
    ((Verse.Thing) vehicleBuilding).HitPoints = ((Verse.Thing) vehicleBuilding).MaxHitPoints / 10;
    this.meleeVerbs.Notify_PawnKilled();
    if (!spawned)
      return;
    if (map.terrainGrid.TerrainAt(positionHeld) == TerrainDefOf.WaterOceanDeep || map.terrainGrid.TerrainAt(positionHeld) == TerrainDefOf.WaterDeep)
    {
      StringBuilder stringBuilder = new StringBuilder();
      bool flag = false;
      foreach (Pawn pawn in this.AllPawnsAboard)
      {
        if (HealthHelper.AttemptToDrown(pawn))
        {
          flag = true;
          stringBuilder.AppendLine(((Entity) pawn).LabelCap);
        }
        else
          this.DisembarkPawn(pawn);
      }
      string str = TaggedString.op_Implicit(flag ? TranslatorFormattedStringExtensions.Translate("VF_BoatSunkDesc", NamedArgument.op_Implicit(((Entity) this).LabelShort)) : TranslatorFormattedStringExtensions.Translate("VF_BoatSunkWithPawnsDesc", NamedArgument.op_Implicit(((Entity) this).LabelShort), NamedArgument.op_Implicit(stringBuilder.ToString())));
      Find.LetterStack.ReceiveLetter(Translator.Translate("VF_BoatSunk"), TaggedString.op_Implicit(str), LetterDefOf.NegativeEvent, LookTargets.op_Implicit(new TargetInfo(((Verse.Thing) this).Position, map, false)), (Faction) null, (Quest) null, (List<ThingDef>) null, (string) null, 0, true);
      ((Verse.Thing) this).Destroy((DestroyMode) 2);
    }
    else
    {
      ((Verse.Thing) this).Destroy((DestroyMode) 2);
      if (!spawnWreckage)
        return;
      GenSpawn.Spawn((Verse.Thing) vehicleBuilding, positionHeld, map, rotation, (WipeMode) 1, false, false);
    }
  }

  public virtual void Notify_DamageImpact(VehicleComponent.DamageResult damageResult)
  {
    if (!((Verse.Thing) this).Spawned)
      return;
    EffecterDef effecterDef1;
    switch (damageResult.penetration)
    {
      case VehicleComponent.Penetration.NonPenetrated:
        effecterDef1 = this.VehicleDef.BodyType.nonPenetrationEffect;
        break;
      case VehicleComponent.Penetration.Deflected:
        effecterDef1 = ((DamageInfo) ref damageResult.damageInfo).Def == DamageDefOf.Bullet ? this.VehicleDef.BodyType.deflectionEffectBullet : this.VehicleDef.BodyType.deflectionEffect;
        break;
      case VehicleComponent.Penetration.Diminished:
        effecterDef1 = this.VehicleDef.BodyType.diminishedEffect;
        break;
      case VehicleComponent.Penetration.Penetrated:
        effecterDef1 = this.VehicleDef.BodyType.damageEffecter;
        break;
      case VehicleComponent.Penetration.Electrified:
        effecterDef1 = this.VehicleDef.BodyType.electrifiedEffect;
        break;
      default:
        throw new NotImplementedException("Unhandled Penetration result.");
    }
    EffecterDef effecterDef2 = effecterDef1;
    if (effecterDef2 != null && (this.health.deflectionEffecter == null || this.health.deflectionEffecter.def != effecterDef2))
    {
      if (this.health.deflectionEffecter != null)
      {
        this.health.deflectionEffecter.Cleanup();
        this.health.deflectionEffecter = (Effecter) null;
      }
      this.health.deflectionEffecter = effecterDef2.Spawn();
    }
    IntVec2 intVec2 = damageResult.cell.MirrorRotatedBy(((Verse.Thing) this).Rotation, ((BuildableDef) this.VehicleDef).Size);
    IntVec3 intVec3;
    // ISSUE: explicit constructor call
    ((IntVec3) ref intVec3).\u002Ector(((Verse.Thing) this).Position.x + intVec2.x, 0, ((Verse.Thing) this).Position.z + intVec2.z);
    Effecter deflectionEffecter = this.health.deflectionEffecter;
    if (deflectionEffecter != null)
    {
      TargetInfo targetInfo1 = new TargetInfo(intVec3, ((Verse.Thing) this).Map, false);
      Verse.Thing instigator = ((DamageInfo) ref damageResult.damageInfo).Instigator;
      TargetInfo targetInfo2 = instigator != null ? TargetInfo.op_Implicit(instigator) : new TargetInfo(intVec3, ((Verse.Thing) this).Map, false);
      deflectionEffecter.Trigger(targetInfo1, targetInfo2, -1);
    }
    this.PlayImpactSound(damageResult);
  }

  protected virtual void DoDestroyEffects(Map map)
  {
    if (this.VehicleDef.buildDef.building.destroyEffecter != null)
    {
      Effecter effecter = this.VehicleDef.buildDef.building.destroyEffecter.Spawn(((Verse.Thing) this).Position, map, 1f);
      effecter.Trigger(new TargetInfo(((Verse.Thing) this).Position, map, false), TargetInfo.Invalid, -1);
      effecter.Cleanup();
    }
    else
    {
      SoundDef destroySound = this.GetDestroySound();
      if (destroySound != null)
        SoundStarter.PlayOneShot(destroySound, SoundInfo.op_Implicit(new TargetInfo(((Verse.Thing) this).Position, map, false)));
      CellRect cellRect = GenAdj.OccupiedRect((Verse.Thing) this);
      foreach (IntVec3 intVec3 in cellRect)
      {
        int num = this.VehicleDef.buildDef.building.isNaturalRock ? 1 : Rand.RangeInclusive(3, 5);
        for (int index = 0; index < num; ++index)
          FleckMaker.ThrowDustPuffThick(((IntVec3) ref intVec3).ToVector3Shifted(), map, Rand.Range(1.5f, 2f), Color.white);
      }
      if (Find.CurrentMap != map)
        return;
      float num1 = this.VehicleDef.buildDef.building.destroyShakeAmount;
      if ((double) num1 < 0.0)
      {
        SimpleCurve amountPerAreaCurve = this.VehicleDef.buildDef.shakeAmountPerAreaCurve;
        double num2;
        if (amountPerAreaCurve == null)
        {
          num2 = 0.0;
        }
        else
        {
          IntVec2 size = ((BuildableDef) this.VehicleDef.buildDef).Size;
          num2 = (double) amountPerAreaCurve.Evaluate((float) ((IntVec2) ref size).Area);
        }
        num1 = (float) num2;
      }
      CompLifespan cachedComp = this.GetCachedComp<CompLifespan>();
      if (cachedComp != null && cachedComp.age >= cachedComp.Props.lifespanTicks)
        return;
      Find.CameraDriver.shaker.DoShake(num1);
    }
  }

  public virtual SoundDef GetDestroySound()
  {
    if (!SoundDefHelper.NullOrUndefined(this.VehicleDef.buildDef.building.destroySound))
      return this.VehicleDef.buildDef.building.destroySound;
    if (GenList.NullOrEmpty<ThingDefCountClass>((IList<ThingDefCountClass>) ((BuildableDef) this.VehicleDef.buildDef).CostList) || !((BuildableDef) this.VehicleDef.buildDef).CostList[0].thingDef.IsStuff || GenList.NullOrEmpty<StuffCategoryDef>((IList<StuffCategoryDef>) ((BuildableDef) this.VehicleDef.buildDef).CostList[0].thingDef.stuffProps.categories))
      return (SoundDef) null;
    StuffCategoryDef category = ((BuildableDef) this.VehicleDef.buildDef).CostList[0].thingDef.stuffProps.categories[0];
    switch ((int) this.VehicleDef.buildDef.building.buildingSizeCategory)
    {
      case 0:
        IntVec2 size = ((BuildableDef) this.VehicleDef.buildDef).Size;
        int area = ((IntVec2) ref size).Area;
        if (area <= 1 && !SoundDefHelper.NullOrUndefined(category.destroySoundSmall))
          return category.destroySoundSmall;
        if (area <= 4 && !SoundDefHelper.NullOrUndefined(category.destroySoundMedium))
          return category.destroySoundMedium;
        if (!SoundDefHelper.NullOrUndefined(category.destroySoundLarge))
          return category.destroySoundLarge;
        break;
      case 1:
        if (!SoundDefHelper.NullOrUndefined(category.destroySoundSmall))
          return category.destroySoundSmall;
        break;
      case 2:
        if (!SoundDefHelper.NullOrUndefined(category.destroySoundMedium))
          return category.destroySoundMedium;
        break;
      case 3:
        if (!SoundDefHelper.NullOrUndefined(category.destroySoundLarge))
          return category.destroySoundLarge;
        break;
    }
    return (SoundDef) null;
  }

  public float CachedAngle { get; set; }

  public bool NorthSouthRotation
  {
    get
    {
      if (this.VehicleGraphic.EastDiagonalRotated && (this.FullRotation == Rot8.NorthEast || this.FullRotation == Rot8.SouthEast))
        return true;
      if (!this.VehicleGraphic.WestDiagonalRotated)
        return false;
      return this.FullRotation == Rot8.NorthWest || this.FullRotation == Rot8.SouthWest;
    }
  }

  public bool CanPaintNow => this.patternToPaint != null;

  public bool Nameable
  {
    get
    {
      return SettingsCache.TryGetValue<bool>(this.VehicleDef, typeof (VehicleDef), "nameable", this.VehicleDef.nameable);
    }
  }

  public virtual Vector3 DrawPos
  {
    get => Vector3.op_Addition(this.DrawTracker.DrawPos, this.transform.position);
  }

  public (Vector3 drawPos, float rotation) DrawData => (((Verse.Thing) this).DrawPos, this.Angle);

  public ThingWithComps Thing => (ThingWithComps) this;

  public RetextureDef Retexture => this.retextureDef;

  public MaterialPropertyBlock PropertyBlock { get; private set; }

  ModContentPack IAnimator.ModContentPack => ((Def) this.VehicleDef).modContentPack;

  AnimationManager IAnimator.Manager => this.animator;

  string IAnimationObject.ObjectId => nameof (VehiclePawn);

  public Transform Transform => this.transform;

  public float Angle
  {
    get
    {
      return !VehicleMod.settings.main.allowDiagonalRendering || !this.VehicleDef.properties.diagonalRotation ? 0.0f : this.angle;
    }
    set
    {
      if (Mathf.Approximately(value, this.angle))
        return;
      this.angle = this.Reverse ? -value : value;
    }
  }

  public Rot8 FullRotation
  {
    get
    {
      return !this.VehicleDef.graphicData.drawRotated ? Rot8.North : new Rot8(((Verse.Thing) this).Rotation, this.Angle);
    }
    set
    {
      if (value == this.FullRotation)
        return;
      ((Verse.Thing) this).Rotation = (Rot4) value;
      this.Angle = 0.0f;
      if (value == Rot8.NorthEast || value == Rot8.SouthWest)
      {
        this.Angle = -45f;
      }
      else
      {
        if (!(value == Rot8.SouthEast) && !(value == Rot8.NorthWest))
          return;
        this.Angle = 45f;
      }
    }
  }

  public bool Reverse
  {
    get => this.reverse;
    set
    {
      this.vehiclePather.StopDead();
      this.reverse = value;
    }
  }

  [Obsolete("Vehicles should call DrawTracker instead of the vanilla implementation", true)]
  public VehicleDrawTracker Drawer => this.DrawTracker;

  public VehicleDrawTracker DrawTracker => this.drawTracker;

  public Graphic_Vehicle VehicleGraphic
  {
    get
    {
      if (this.graphic == null)
        this.graphic = this.GenerateGraphic();
      return this.graphic;
    }
  }

  public int MaterialCount => 8;

  public PatternDef PatternDef => this.Pattern;

  string IMaterialCacheTarget.Name => $"{this.VehicleDef}_{this}";

  public virtual Color DrawColor
  {
    get => (Color?) this.Pattern?.properties?.colorOne ?? this.patternData.color;
    set => this.patternData.color = value;
  }

  public Color DrawColorTwo
  {
    get => (Color?) this.Pattern?.properties?.colorTwo ?? this.patternData.colorTwo;
    set => this.patternData.colorTwo = value;
  }

  public Color DrawColorThree
  {
    get => (Color?) this.Pattern?.properties?.colorThree ?? this.patternData.colorThree;
    set => this.patternData.colorThree = value;
  }

  public Vector2 Displacement
  {
    get => this.patternData.displacement;
    set => this.patternData.displacement = value;
  }

  public float Tiles
  {
    get => this.patternData.tiles;
    set => this.patternData.tiles = value;
  }

  public PatternDef Pattern
  {
    get
    {
      return this.patternData.patternDef ?? GenCollection.TryGetValue<string, PatternData>((IReadOnlyDictionary<string, PatternData>) VehicleMod.settings.vehicles.defaultGraphics, ((Def) this.VehicleDef).defName, (PatternData) this.VehicleGraphic.DataRgb)?.patternDef ?? PatternDefOf.Default;
    }
    set => this.patternData.patternDef = value;
  }

  public IEnumerable<AnimationDriver> Animations
  {
    get
    {
      if (this.CompVehicleLauncher != null)
      {
        foreach (AnimationDriver animation in this.compVehicleLauncher.Animations)
          yield return animation;
      }
    }
  }

  internal void SetRotationInt(Rot4 value, ref Rot4 rotationInt)
  {
    if (this.Reverse)
      value = ((Rot4) ref value).Opposite;
    if (Rot4.op_Equality(rotationInt, value))
      return;
    Rot4 rotation = ((Verse.Thing) this).Rotation;
    if (((Verse.Thing) this).Spawned)
    {
      CellRect cellRect = this.OccupiedRectShifted(IntVec2.Zero, new Rot4?(value));
      if (!((CellRect) ref cellRect).InBounds(((Verse.Thing) this).Map))
        return;
      RegionListersUpdater.DeregisterInRegions((Verse.Thing) this, ((Verse.Thing) this).Map);
      ((Verse.Thing) this).Map.thingGrid.Deregister((Verse.Thing) this, false);
      ((Verse.Thing) this).Map.coverGrid.DeRegister((Verse.Thing) this);
    }
    rotationInt = value;
    if (!((Verse.Thing) this).Spawned)
      return;
    ((Verse.Thing) this).Map.thingGrid.Register((Verse.Thing) this);
    ((Verse.Thing) this).Map.coverGrid.Register((Verse.Thing) this);
    RegionListersUpdater.RegisterInRegions((Verse.Thing) this, ((Verse.Thing) this).Map);
    this.ReclaimPosition();
    foreach (IntVec3 intVec3 in this.OccupiedRectShifted(IntVec2.Zero, new Rot4?(rotation)).AllCellsNoRepeat(this.OccupiedRectShifted(IntVec2.Zero, new Rot4?(rotationInt))))
      ((Verse.Thing) this).Map.pathing.RecalculatePerceivedPathCostAt(intVec3);
  }

  public Vector3 TrueCenter() => this.TrueCenter(((Verse.Thing) this).Position);

  public Vector3 TrueCenter(IntVec3 cell, float? altitude = null)
  {
    float num1 = (float) ((double) altitude ?? (double) ((BuildableDef) this.VehicleDef).Altitude);
    Vector3 shiftedWithAltitude = ((IntVec3) ref cell).ToVector3ShiftedWithAltitude(num1);
    IntVec2 size = ((BuildableDef) this.VehicleDef).Size;
    Rot8 rotation = (Rot8) ((Verse.Thing) this).Rotation;
    if (size.x != 1 || size.z != 1)
    {
      if (rotation.IsHorizontal)
      {
        ref int local1 = ref size.x;
        ref int local2 = ref size.z;
        int z = size.z;
        int x = size.x;
        local1 = z;
        int num2 = x;
        local2 = num2;
      }
      switch (rotation.AsInt)
      {
        case 0:
        case 2:
          if (size.x % 2 == 0)
            shiftedWithAltitude.x += 0.5f;
          if (size.z % 2 == 0)
          {
            shiftedWithAltitude.z += 0.5f;
            break;
          }
          break;
        case 1:
        case 3:
          if (size.x % 2 == 0)
            shiftedWithAltitude.x += 0.5f;
          if (size.z % 2 == 0)
          {
            shiftedWithAltitude.z -= 0.5f;
            break;
          }
          break;
      }
    }
    return shiftedWithAltitude;
  }

  public virtual void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
  {
    if (this.AnimationLocked())
      return;
    this.DrawTracker.DynamicDrawPhaseAt(phase, Vector3.op_Addition(this.transform.position, drawLoc), this.FullRotation, this.transform.rotation);
    if (phase != 2)
      return;
    if (this.HighlightedComponent != null)
      this.statHandler.DrawHitbox(this.HighlightedComponent);
    ((ThingWithComps) this).Comps_PostDraw();
  }

  protected virtual void DrawAt(Vector3 drawLoc, bool flip = false)
  {
    Log.ErrorOnce("Calling DrawAt instead of DynamicDrawPhaseAt", ((object) this).GetHashCode());
    this.DrawAt(in drawLoc, this.FullRotation, this.transform.rotation);
  }

  public virtual void DrawAt(in Vector3 drawLoc, Rot8 rot, float rotation)
  {
    this.DrawTracker.DynamicDrawPhaseAt((DrawPhase) 2, in drawLoc, rot, rotation);
  }

  public void ProcessPostTickVisuals(int ticksPassed, CellRect viewRect)
  {
    if (!((Verse.Thing) this).Suspended && ((Verse.Thing) this).Spawned && Current.ProgramState != 2 || ((CellRect) ref viewRect).Contains(((Verse.Thing) this).Position))
      this.DrawTracker.ProcessPostTickVisuals(ticksPassed);
    this.rotationTracker.ProcessPostTickVisuals(ticksPassed);
  }

  public void ResetRenderStatus()
  {
    foreach (IParallelRenderer handler in this.handlers)
      this.DrawTracker.RemoveRenderer(handler);
    foreach (VehicleRoleHandler handler in this.handlers)
    {
      if (handler.role.PawnRenderer != null)
        this.DrawTracker.AddRenderer((IParallelRenderer) handler);
    }
  }

  public virtual void Notify_ColorChanged()
  {
    this.ResetMaterialProperties();
    this.EventRegistry[VehicleEventDefOf.ColorChanged].ExecuteEvents();
    ((ThingWithComps) this).Notify_ColorChanged();
  }

  public void ResetGraphic() => this.graphic = this.GenerateGraphic();

  private void ResetMaterialProperties()
  {
    if (!UnityData.IsInMainThread)
      return;
    RGBMaterialPool.SetProperties((IMaterialCacheTarget) this, this.patternData);
    foreach (ThingComp allComp in ((ThingWithComps) this).AllComps)
    {
      if (allComp is VehicleComp vehicleComp)
        ((ThingComp) vehicleComp).Notify_ColorChanged();
    }
  }

  private Graphic_Vehicle GenerateGraphic()
  {
    if (((Verse.Thing) this).Destroyed && !GenList.NullOrEmpty<Material>((IList<Material>) RGBMaterialPool.GetAll((IMaterialCacheTarget) this)))
    {
      Log.Error($"Reinitializing RGB Materials but {this} has already been destroyed and the cache was not cleared for this entry. This may result in a memory leak.");
      RGBMaterialPool.Release((IMaterialCacheTarget) this);
    }
    GraphicDataRGB graphicDataRgb = new GraphicDataRGB();
    graphicDataRgb.CopyFrom((GraphicDataLayered) (this.retextureDef?.graphicData ?? this.VehicleDef.graphicData));
    graphicDataRgb.color = this.patternData.color;
    graphicDataRgb.colorTwo = this.patternData.colorTwo;
    graphicDataRgb.colorThree = this.patternData.colorThree;
    graphicDataRgb.tiles = this.patternData.tiles;
    graphicDataRgb.displacement = this.patternData.displacement;
    graphicDataRgb.pattern = this.patternData.patternDef;
    Graphic_Vehicle graphic;
    if (graphicDataRgb.shaderType.Shader.SupportsRGBMaskTex())
    {
      RGBMaterialPool.CacheMaterialsFor((IMaterialCacheTarget) this);
      GraphicDatabaseRGB.Remove((IMaterialCacheTarget) this);
      graphicDataRgb.Init((IMaterialCacheTarget) this);
      graphic = graphicDataRgb.Graphic as Graphic_Vehicle;
      RGBMaterialPool.SetProperties((IMaterialCacheTarget) this, this.patternData, new Func<Rot8, Texture2D>(((Graphic_Rgb) graphic).TexAt), new Func<Rot8, Texture2D>(((Graphic_Rgb) graphic).MaskAt));
    }
    else
      graphic = ((GraphicData) graphicDataRgb).Graphic as Graphic_Vehicle;
    for (int newRot = 0; newRot < 8; ++newRot)
      graphic.MeshAtFull(new Rot8(newRot));
    return graphic;
  }

  public void UpdateRotationAndAngle()
  {
    this.UpdateRotation();
    this.UpdateAngle();
  }

  public void UpdateRotation()
  {
    if (IntVec3.op_Equality(this.vehiclePather.nextCell, ((Verse.Thing) this).Position))
      return;
    if (!this.VehicleDef.rotatable)
    {
      ((Verse.Thing) this).Rotation = ((BuildableDef) this.VehicleDef).defaultPlacingRot;
    }
    else
    {
      IntVec3 intVec3 = IntVec3.op_Subtraction(this.vehiclePather.nextCell, ((Verse.Thing) this).Position);
      int x = intVec3.x;
      ((Verse.Thing) this).Rotation = x > 0 ? Rot4.East : (x < 0 ? Rot4.West : (intVec3.z <= 0 ? Rot4.South : Rot4.North));
    }
  }

  public void UpdateAngle()
  {
    if (!this.vehiclePather.Moving)
      return;
    IntVec3 intVec3 = IntVec3.op_Subtraction(this.vehiclePather.nextCell, ((Verse.Thing) this).Position);
    int x = intVec3.x;
    int num;
    if (x <= 0)
    {
      if (x < 0)
      {
        int z = intVec3.z;
        if (z >= 0)
        {
          if (z > 0)
          {
            num = 45;
            goto label_13;
          }
        }
        else
        {
          num = -45;
          goto label_13;
        }
      }
    }
    else
    {
      int z = intVec3.z;
      if (z <= 0)
      {
        if (z < 0)
        {
          num = 45;
          goto label_13;
        }
      }
      else
      {
        num = -45;
        goto label_13;
      }
    }
    num = 0;
label_13:
    this.angle = (float) num;
  }

  public virtual void DrawGUIOverlay()
  {
  }

  public virtual void DrawExtraSelectionOverlays()
  {
    base.DrawExtraSelectionOverlays();
    VehiclePath curPath = this.vehiclePather.curPath;
    if (curPath != null && curPath.NodesLeft > 0)
      this.vehiclePather.curPath.DrawPath(this);
    RenderHelper.DrawLinesBetweenTargets(this, this.jobs.curJob, this.jobs.jobQueue);
    if (GenList.NullOrEmpty<TransferableOneWay>((IList<TransferableOneWay>) this.cargoToLoad))
      return;
    foreach (TransferableOneWay transferableOneWay in this.cargoToLoad)
    {
      if (((Transferable) transferableOneWay).HasAnyThing)
        GenDraw.DrawLineBetween(((Verse.Thing) this).DrawPos, ((Transferable) transferableOneWay).AnyThing.DrawPos);
    }
  }

  public virtual IEnumerable<Gizmo> GetGizmos()
  {
    VehiclePawn vehicle = this;
    if (((Verse.Thing) vehicle).Faction == Faction.OfPlayer || DebugSettings.ShowDevGizmos)
    {
      IEnumerator<Gizmo> enumerator1;
      if ((vehicle.MovementPermissions & VehiclePermissions.Mobile) != VehiclePermissions.None)
      {
        enumerator1 = vehicle.ignition.GetGizmos().GetEnumerator();
        while (enumerator1.MoveNext())
          yield return enumerator1.Current;
        enumerator1 = (IEnumerator<Gizmo>) null;
      }
      if (DebugSettings.ShowDevGizmos && ((Verse.Thing) vehicle).Spawned && !vehicle.pather.Moving)
      {
        Command_Action gizmo = new Command_Action();
        ((Command) gizmo).defaultLabel = "Teleport";
        gizmo.action = (Action) (() =>
        {
          Targeter targeter = Find.Targeter;
          TargetingParameters targetingParameters = new TargetingParameters();
          targetingParameters.canTargetLocations = true;
          targetingParameters.canTargetPawns = false;
          targetingParameters.canTargetBuildings = false;
          Action<LocalTargetInfo> action1 = (Action<LocalTargetInfo>) (target =>
          {
            ((Verse.Thing) this).Position = ((LocalTargetInfo) ref target).Cell;
            this.Notify_Teleported();
          });
          Action<LocalTargetInfo> action2 = (Action<LocalTargetInfo>) (target =>
          {
            Color color = LandingTargeter.GhostDrawerColor(Validator(((LocalTargetInfo) ref target).Cell) ? LandingTargeter.PositionState.Valid : LandingTargeter.PositionState.Invalid);
            GhostDrawer.DrawGhostThing(((LocalTargetInfo) ref target).Cell, (Rot4) this.FullRotation, (ThingDef) this.VehicleDef.buildDef, ((BuildableDef) this.VehicleDef.buildDef).graphic, color, (AltitudeLayer) 26, (Verse.Thing) null, true, (ThingDef) null);
          });
          Func<LocalTargetInfo, bool> func = (Func<LocalTargetInfo, bool>) (target => Validator(((LocalTargetInfo) ref target).Cell));
          targeter.BeginTargeting(targetingParameters, action1, action2, func, (Pawn) null, (Action) null, (Texture2D) null, true, (Action<LocalTargetInfo>) null, (Action<LocalTargetInfo>) null);
        });
        yield return (Gizmo) gizmo;
      }
      CompUpgradeTree compUpgradeTree = vehicle.CompUpgradeTree;
      bool upgrading = compUpgradeTree != null && compUpgradeTree.Upgrading;
      if (!GenList.NullOrEmpty<TransferableOneWay>((IList<TransferableOneWay>) vehicle.cargoToLoad))
      {
        Command_Action gizmo = new Command_Action();
        ((Command) gizmo).defaultLabel = TaggedString.op_Implicit(Translator.Translate("DesignatorCancel"));
        ((Command) gizmo).icon = (Texture) vehicle.VehicleDef.CancelCargoIcon;
        ((Command) gizmo).hotKey = KeyBindingDefOf.Designator_Cancel;
        gizmo.action = (Action) (() =>
        {
          ((Verse.Thing) this).Map.GetCachedMapComponent<VehicleReservationManager>().RemoveLister(this, "LoadVehicle");
          this.cargoToLoad.Clear();
        });
        yield return (Gizmo) gizmo;
      }
      else
      {
        Command_Action commandAction = new Command_Action();
        ((Command) commandAction).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VF_LoadCargo"));
        ((Command) commandAction).icon = (Texture) vehicle.VehicleDef.LoadCargoIcon;
        ((Command) commandAction).hotKey = KeyBindingDefOf.Misc2;
        commandAction.action = (Action) (() => Find.WindowStack.Add((Window) new Dialog_LoadCargo(this)));
        Command_Action gizmo = commandAction;
        if (upgrading)
          ((Gizmo) gizmo).Disable(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_DisabledByVehicleUpgrading", NamedArgument.op_Implicit(((Entity) vehicle).LabelCap))));
        yield return (Gizmo) gizmo;
      }
      if (FishingCompatibility.Active && SettingsCache.TryGetValue<bool>(vehicle.VehicleDef, typeof (VehicleProperties), "canFish", vehicle.VehicleDef.properties.canFish))
      {
        if (vehicle.fishToggle == null)
        {
          VehiclePawn vehiclePawn = vehicle;
          Command_Toggle commandToggle = new Command_Toggle();
          ((Command) commandToggle).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VF_StartFishing"));
          ((Command) commandToggle).defaultDesc = TaggedString.op_Implicit(Translator.Translate("VF_StartFishingDesc"));
          ((Command) commandToggle).icon = (Texture) VehicleTex.FishingIcon;
          commandToggle.isActive = (Func<bool>) (() => this.fishing);
          commandToggle.toggleAction = (Action) (() =>
          {
            this.fishing = !this.fishing;
            SoundStarter.PlayOneShotOnCamera(this.fishing ? SoundDefOf.Checkbox_TurnedOn : SoundDefOf.Checkbox_TurnedOff, (Map) null);
          });
          vehiclePawn.fishToggle = commandToggle;
        }
        ((Gizmo) vehicle.fishToggle).Disabled = false;
        ((Gizmo) vehicle.fishToggle).disabledReason = (string) null;
        if (!vehicle.CanFish)
        {
          vehicle.fishing = false;
          ((Gizmo) vehicle.fishToggle).Disable(TaggedString.op_Implicit(Translator.Translate("VF_NoFishermenTooltip")));
        }
        if (!FishingCompatibility.CanFishAt(vehicle, ((Verse.Thing) vehicle).Position))
        {
          vehicle.fishing = false;
          ((Gizmo) vehicle.fishToggle).Disable(TaggedString.op_Implicit(Translator.Translate("VF_NoFishAtSpot")));
        }
        yield return (Gizmo) vehicle.fishToggle;
      }
      Command_Action commandAction1 = new Command_Action();
      ((Command) commandAction1).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VF_HaulPawnToVehicle"));
      ((Command) commandAction1).icon = (Texture) VehicleTex.HaulPawnToVehicle;
      commandAction1.action = (Action) (() =>
      {
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
        HaulTargeter.BeginTargeting(new TargetingParameters()
        {
          canTargetPawns = true,
          canTargetBuildings = false,
          neverTargetHostileFaction = true,
          canTargetItems = false,
          thingCategory = (ThingCategory) 1,
          validator = (Predicate<TargetInfo>) (target =>
          {
            if (!((TargetInfo) ref target).HasThing || !(((TargetInfo) ref target).Thing is Pawn thing2) || thing2 is VehiclePawn)
              return false;
            return ((Verse.Thing) thing2).Faction == Faction.OfPlayer || thing2.IsColonist || thing2.IsColonyMech || thing2.IsSlaveOfColony || thing2.IsPrisonerOfColony;
          })
        }, (Action<LocalTargetInfo>) (target =>
        {
          if (((LocalTargetInfo) ref target).Thing is Pawn thing5 && !thing5.Downed)
          {
            VehicleRoleHandler handler = thing5.IsColonistPlayerControlled ? this.GetAnyAvailableHandler() : this.GetNextAvailableHandler(thing5, HandlingType.None);
            this.PromptToBoardVehicle(thing5, handler);
          }
          else
          {
            Verse.Thing thing6 = ((LocalTargetInfo) ref target).Thing;
            thing6.CancelTransferToAnyOtherVehicle(this);
            thing6.TransferToVehicle(this);
            TransferableOneWay transferableOneWay = new TransferableOneWay()
            {
              things = new List<Verse.Thing>(1) { thing6 }
            };
            ((Transferable) transferableOneWay).AdjustTo(thing6.stackCount);
            this.cargoToLoad.Add(transferableOneWay);
            ((Verse.Thing) this).Map.GetCachedMapComponent<VehicleReservationManager>().RegisterLister(this, "LoadVehicle");
          }
        }), this);
      });
      Command_Action gizmo1 = commandAction1;
      if (upgrading)
        ((Gizmo) gizmo1).Disable(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_DisabledByVehicleUpgrading", NamedArgument.op_Implicit(((Entity) vehicle).LabelCap))));
      yield return (Gizmo) gizmo1;
      if (!vehicle.Drafted)
      {
        Command_Action commandAction2 = new Command_Action();
        ((Command) commandAction2).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VF_DisembarkAllPawns"));
        ((Command) commandAction2).icon = (Texture) VehicleTex.UnloadAll;
        commandAction2.action = new Action(vehicle.DisembarkAll);
        ((Command) commandAction2).hotKey = KeyBindingDefOf.Misc2;
        Command_Action gizmo2 = commandAction2;
        bool exitBlocked = !vehicle.SurroundingCells.NotNullAndAny<IntVec3>((Predicate<IntVec3>) (cell => GenGrid.Walkable(cell, ((Verse.Thing) this).Map)));
        if (exitBlocked)
          ((Gizmo) gizmo2).Disable(TaggedString.op_Implicit(Translator.Translate("VF_DisembarkNoExit")));
        yield return (Gizmo) gizmo2;
        List<VehicleRoleHandler>.Enumerator enumerator2 = vehicle.handlers.GetEnumerator();
        while (enumerator2.MoveNext())
        {
          VehicleRoleHandler handler = enumerator2.Current;
          for (int i = 0; i < ((ThingOwner) handler.thingOwner).Count; ++i)
          {
            Pawn currentPawn = handler.thingOwner.InnerListForReading[i];
            Command_ActionPawnDrawer actionPawnDrawer = new Command_ActionPawnDrawer();
            ((Command) actionPawnDrawer).defaultLabel = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_DisembarkSinglePawn", NamedArgument.op_Implicit(((Entity) currentPawn).LabelShort)));
            ((Command) actionPawnDrawer).groupable = false;
            actionPawnDrawer.pawn = currentPawn;
            actionPawnDrawer.action = (Action) (() => this.DisembarkPawn(currentPawn));
            Command_ActionPawnDrawer gizmo3 = actionPawnDrawer;
            if (exitBlocked)
              ((Gizmo) gizmo3).Disable(TaggedString.op_Implicit(Translator.Translate("VF_DisembarkNoExit")));
            yield return (Gizmo) gizmo3;
          }
          handler = (VehicleRoleHandler) null;
        }
        enumerator2 = new List<VehicleRoleHandler>.Enumerator();
      }
      LordJob_FormAndSendVehicles formCaravanLordJob = LordUtility.GetLord((Pawn) vehicle)?.LordJob as LordJob_FormAndSendVehicles;
      if (formCaravanLordJob != null)
      {
        Command_Action gizmo4 = new Command_Action();
        ((Command) gizmo4).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VF_ForceLeaveCaravan"));
        ((Command) gizmo4).defaultDesc = TaggedString.op_Implicit(Translator.Translate("VF_ForceLeaveCaravanDesc"));
        ((Command) gizmo4).icon = (Texture) TexData.CaravanIcon;
        ((Command) gizmo4).activateSound = SoundDefOf.Tick_Low;
        gizmo4.action = (Action) (() =>
        {
          formCaravanLordJob.ForceCaravanLeave();
          Messages.Message(TaggedString.op_Implicit(Translator.Translate("VF_ForceLeaveConfirmation")), MessageTypeDefOf.TaskCompletion, true);
        });
        yield return (Gizmo) gizmo4;
        Command_Action gizmo5 = new Command_Action();
        ((Command) gizmo5).defaultLabel = TaggedString.op_Implicit(Translator.Translate("CommandCancelFormingCaravan"));
        ((Command) gizmo5).defaultDesc = TaggedString.op_Implicit(Translator.Translate("CommandCancelFormingCaravanDesc"));
        ((Command) gizmo5).icon = (Texture) TexCommand.ClearPrioritizedWork;
        ((Command) gizmo5).activateSound = SoundDefOf.Tick_Low;
        gizmo5.action = (Action) (() => CaravanFormingUtility.StopFormingCaravan(((LordJob) formCaravanLordJob).lord));
        ((Command) gizmo5).hotKey = KeyBindingDefOf.Designator_Cancel;
        yield return (Gizmo) gizmo5;
      }
      foreach (ThingComp allComp in ((ThingWithComps) vehicle).AllComps)
      {
        enumerator1 = allComp.CompGetGizmosExtra().GetEnumerator();
        while (enumerator1.MoveNext())
          yield return enumerator1.Current;
        enumerator1 = (IEnumerator<Gizmo>) null;
      }
      if (DebugSettings.ShowDevGizmos && ((Verse.Thing) vehicle).Spawned)
      {
        Command_Action gizmo6 = new Command_Action();
        ((Command) gizmo6).defaultLabel = "Destroy Component";
        gizmo6.action = (Action) (() =>
        {
          List<FloatMenuOption> floatMenuOptionList = new List<FloatMenuOption>();
          foreach (VehicleComponent component1 in this.statHandler.components)
          {
            VehicleComponent component = component1;
            floatMenuOptionList.Add(new FloatMenuOption(component.props.label, (Action) (() =>
            {
              component.TakeDamage(this, new DamageInfo(DamageDefOf.Vaporize, float.MaxValue, 0.0f, -1f, (Verse.Thing) null, (BodyPartRecord) null, (ThingDef) null, (DamageInfo.SourceCategory) 0, (Verse.Thing) null, true, true, (QualityCategory) 2, true, false), true);
              this.Notify_TookDamage();
            }), (MenuOptionPriority) 4, (Action<Rect>) null, (Verse.Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0));
          }
          if (floatMenuOptionList.Count <= 0)
            return;
          Find.WindowStack.Add((Window) new FloatMenu(floatMenuOptionList));
        });
        yield return (Gizmo) gizmo6;
        Command_Action gizmo7 = new Command_Action();
        ((Command) gizmo7).defaultLabel = "Damage Component";
        gizmo7.action = (Action) (() =>
        {
          List<FloatMenuOption> floatMenuOptionList = new List<FloatMenuOption>();
          foreach (VehicleComponent component2 in this.statHandler.components)
          {
            VehicleComponent component = component2;
            floatMenuOptionList.Add(new FloatMenuOption(component.props.label, (Action) (() =>
            {
              component.TakeDamage(this, new DamageInfo(DamageDefOf.Vaporize, component.Health * Rand.Range(0.1f, 1f), 0.0f, -1f, (Verse.Thing) null, (BodyPartRecord) null, (ThingDef) null, (DamageInfo.SourceCategory) 0, (Verse.Thing) null, true, true, (QualityCategory) 2, true, false), true);
              this.Notify_TookDamage();
            }), (MenuOptionPriority) 4, (Action<Rect>) null, (Verse.Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0));
          }
          if (floatMenuOptionList.Count <= 0)
            return;
          Find.WindowStack.Add((Window) new FloatMenu(floatMenuOptionList));
        });
        yield return (Gizmo) gizmo7;
        Command_Action gizmo8 = new Command_Action();
        ((Command) gizmo8).defaultLabel = "Explode Component";
        gizmo8.action = (Action) (() =>
        {
          List<FloatMenuOption> floatMenuOptionList = new List<FloatMenuOption>();
          foreach (VehicleComponent component3 in this.statHandler.components)
          {
            VehicleComponent component = component3;
            Reactor_Explosive reactorExplosive = component.props.GetReactor<Reactor_Explosive>();
            if (reactorExplosive != null)
              floatMenuOptionList.Add(new FloatMenuOption(component.props.label, (Action) (() => reactorExplosive.SpawnExploder(this, component)), (MenuOptionPriority) 4, (Action<Rect>) null, (Verse.Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0));
          }
          if (GenList.NullOrEmpty<FloatMenuOption>((IList<FloatMenuOption>) floatMenuOptionList))
            return;
          Find.WindowStack.Add((Window) new FloatMenu(floatMenuOptionList));
        });
        yield return (Gizmo) gizmo8;
        Command_Action gizmo9 = new Command_Action();
        ((Command) gizmo9).defaultLabel = "Heal All Components";
        gizmo9.action = (Action) (() =>
        {
          this.statHandler.components.ForEach((Action<VehicleComponent>) (c => c.HealComponent(float.MaxValue)));
          ((Verse.Thing) this).Map.GetCachedMapComponent<ListerVehiclesRepairable>().NotifyVehicleRepaired(this);
        });
        yield return (Gizmo) gizmo9;
        Command_Action gizmo10 = new Command_Action();
        ((Command) gizmo10).defaultLabel = "Recache All Stats";
        gizmo10.action = new Action(vehicle.statHandler.MarkAllDirty);
        yield return (Gizmo) gizmo10;
        Command_Action gizmo11 = new Command_Action();
        ((Command) gizmo11).defaultLabel = "Give Random Pawn MentalState";
        gizmo11.action = (Action) (() =>
        {
          Pawn pawn;
          if (!GenCollection.TryRandomElement<Pawn>((IEnumerable<Pawn>) this.AllPawnsAboard, ref pawn))
            return;
          foreach (MentalStateDef mentalStateDef in DefDatabase<MentalStateDef>.AllDefsListForReading)
          {
            if (pawn.mindState.mentalStateHandler.TryStartMentalState(mentalStateDef, "testing", false, false, false, (Pawn) null, false, false, false))
              break;
            Log.Warning($"Failed to execute {mentalStateDef} inside vehicles.");
          }
        });
        yield return (Gizmo) gizmo11;
        Command_Action gizmo12 = new Command_Action();
        ((Command) gizmo12).defaultLabel = "Down Random Pawn";
        gizmo12.action = (Action) (() => GenCollection.RandomElementWithFallback<Pawn>((IEnumerable<Pawn>) this.AllPawnsAboard, (Pawn) null)?.health.GetOrAddHediff(HediffDefOf.RegenerationComa, (BodyPartRecord) null, new DamageInfo?(), (DamageWorker.DamageResult) null));
        yield return (Gizmo) gizmo12;
        Command_Action gizmo13 = new Command_Action();
        ((Command) gizmo13).defaultLabel = "Kill Random Pawn";
        gizmo13.action = (Action) (() => ((Verse.Thing) GenCollection.RandomElementWithFallback<Pawn>((IEnumerable<Pawn>) this.AllPawnsAboard, (Pawn) null))?.Kill(new DamageInfo?(), (Hediff) null));
        yield return (Gizmo) gizmo13;
        Command_Action gizmo14 = new Command_Action();
        ((Command) gizmo14).defaultLabel = "Flash OccupiedRect";
        gizmo14.action = (Action) (() =>
        {
          if (this.vehiclePather.Moving)
          {
            IntVec3 from = ((Verse.Thing) this).Position;
            Rot8 rot = this.FullRotation;
            HashSet<IntVec3> intVec3Set = new HashSet<IntVec3>();
            foreach (IntVec3 node in (IEnumerable<IntVec3>) this.vehiclePather.curPath.Nodes)
            {
              if (IntVec3.op_Inequality(from, node))
                rot = Rot8.DirectionFromCells(from, node);
              if (!rot.IsValid)
                rot = Rot8.North;
              CellRect cellRect = this.VehicleRect(node, (Rot4) rot);
              foreach (IntVec3 cell in ((CellRect) ref cellRect).Cells)
              {
                if (GenGrid.InBounds(cell, ((Verse.Thing) this).Map) && intVec3Set.Add(cell))
                  ((Verse.Thing) this).Map.debugDrawer.FlashCell(cell, 0.95f, (string) null, 180);
              }
              from = node;
            }
          }
          else
          {
            CellRect cellRect1 = GenAdj.OccupiedRect((Verse.Thing) this);
            foreach (IntVec3 intVec3 in cellRect1)
            {
              if (GenGrid.InBounds(intVec3, ((Verse.Thing) this).Map))
                ((Verse.Thing) this).Map.debugDrawer.FlashCell(intVec3, 0.95f, (string) null, 180);
            }
            CellRect cellRect2 = this.VehicleRect();
            foreach (IntVec3 intVec3 in cellRect2)
            {
              if (GenGrid.InBounds(intVec3, ((Verse.Thing) this).Map))
                ((Verse.Thing) this).Map.debugDrawer.FlashCell(intVec3, 0.0f, (string) null, 180);
            }
          }
        });
        yield return (Gizmo) gizmo14;
        if (vehicle.animator != null && vehicle.CompVehicleLauncher != null)
        {
          Command_Action gizmo15 = new Command_Action();
          ((Command) gizmo15).defaultLabel = "Toggle Loitering";
          gizmo15.action = (Action) (() =>
          {
            this.CompVehicleLauncher.loiter = !this.CompVehicleLauncher.loiter;
            this.animator.SetBool(PropertyIds.Loiter, this.CompVehicleLauncher.loiter);
          });
          yield return (Gizmo) gizmo15;
        }
      }
    }

    bool Validator(IntVec3 cell)
    {
      VehiclePositionManager detachedMapComponent = ((Verse.Thing) this).Map.GetDetachedMapComponent<VehiclePositionManager>();
      CellRect cellRect = this.PawnOccupiedCells(cell, ((Verse.Thing) this).Rotation);
      foreach (IntVec3 cell1 in cellRect)
      {
        if (!GenGrid.InBounds(cell1, ((Verse.Thing) this).Map) || !cell1.Walkable(this.VehicleDef, ((Verse.Thing) this).Map) || detachedMapComponent.PositionClaimed(cell1))
          return false;
      }
      return true;
    }
  }

  public virtual IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
  {
    VehiclePawn vehicle = this;
    if (selPawn != null && ((Verse.Thing) selPawn).Faction == ((Verse.Thing) vehicle).Faction && selPawn.RaceProps.ToolUser && !(selPawn is VehiclePawn) && ReservationUtility.CanReserveAndReach(selPawn, LocalTargetInfo.op_Implicit((Verse.Thing) vehicle), (PathEndMode) 4, (Danger) 3, 1, -1, (ReservationLayerDef) null, false) && vehicle.movementStatus != VehicleMovementStatus.Offline)
    {
      if (!vehicle.IdeoAllowsBoarding(selPawn))
      {
        yield return new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("VF_CantEnterVehicle_IdeoligionForbids")), (Action) null, (MenuOptionPriority) 4, (Action<Rect>) null, (Verse.Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0);
      }
      else
      {
        foreach (ThingComp allComp in ((ThingWithComps) vehicle).AllComps)
        {
          if (allComp is VehicleComp vehicleComp)
          {
            IEnumerator<FloatMenuOption> enumerator = vehicleComp.CompFloatMenuOptions().GetEnumerator();
            while (enumerator.MoveNext())
              yield return enumerator.Current;
            enumerator = (IEnumerator<FloatMenuOption>) null;
          }
        }
        VehicleReservationManager reservationManager = ((Verse.Thing) vehicle).Map.GetCachedMapComponent<VehicleReservationManager>();
        foreach (VehicleRoleHandler handler1 in vehicle.handlers)
        {
          VehicleRoleHandler handler = handler1;
          if (handler.AreSlotsAvailableAndReservable)
          {
            bool flag = handler.CanOperateRole(selPawn);
            VehicleHandlerReservation reservation = reservationManager.GetReservation<VehicleHandlerReservation>(vehicle);
            int num = reservation != null ? reservation.ClaimantsOnHandler(handler) : 0;
            yield return new FloatMenuOption(TaggedString.op_Implicit(flag ? TranslatorFormattedStringExtensions.Translate("VF_BoardVehicle", NamedArgument.op_Implicit(handler.role.label), NamedArgument.op_Implicit((handler.role.Slots - (((ThingOwner) handler.thingOwner).Count + num)).ToString())) : TranslatorFormattedStringExtensions.Translate("VF_BoardVehicleGroupFail", NamedArgument.op_Implicit(handler.role.label), NamedArgument.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_BoardFailureNonCombatant", NamedArgument.op_Implicit(((Entity) selPawn).LabelShort))))), (Action) (() => this.PromptToBoardVehicle(selPawn, handler)), (MenuOptionPriority) 4, (Action<Rect>) null, (Verse.Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
            {
              Disabled = !flag
            };
          }
        }
      }
    }
  }

  public void PromptToBoardVehicle(Pawn pawn, VehicleRoleHandler handler)
  {
    if (handler == null)
    {
      Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_HandlerNotEnoughRoom", NamedArgument.op_Implicit((Verse.Thing) pawn), NamedArgument.op_Implicit((Verse.Thing) this))), MessageTypeDefOf.RejectInput, false);
    }
    else
    {
      Job job = new Job(JobDefOf_Vehicles.Board, LocalTargetInfo.op_Implicit((Verse.Thing) this));
      this.GiveLoadJob(pawn, handler);
      pawn.jobs.TryTakeOrderedJob(job, new JobTag?((JobTag) 6), false);
      if (!((Verse.Thing) pawn).Spawned)
        return;
      ((Verse.Thing) this).Map.GetCachedMapComponent<VehicleReservationManager>().Reserve<VehicleRoleHandler, VehicleHandlerReservation>(this, pawn, pawn.CurJob, handler);
    }
  }

  public bool IdeoAllowsBoarding(Pawn selPawn)
  {
    if (!ModsConfig.IdeologyActive)
      return true;
    bool flag;
    switch (this.VehicleDef.type)
    {
      case VehicleType.Sea:
        flag = IdeoUtility.DoerWillingToDo(HistoryEventDefOf_Vehicles.VF_BoardSeaVehicle, selPawn);
        break;
      case VehicleType.Air:
        flag = IdeoUtility.DoerWillingToDo(HistoryEventDefOf_Vehicles.VF_BoardAirVehicle, selPawn);
        break;
      case VehicleType.Land:
        flag = IdeoUtility.DoerWillingToDo(HistoryEventDefOf_Vehicles.VF_BoardLandVehicle, selPawn);
        break;
      case VehicleType.Universal:
        flag = IdeoUtility.DoerWillingToDo(HistoryEventDefOf_Vehicles.VF_BoardUniversalVehicle, selPawn);
        break;
      default:
        flag = true;
        break;
    }
    return flag;
  }

  public void ChangeColor()
  {
    Dialog_VehiclePainter.OpenColorPicker(this, (Dialog_VehiclePainter.SaveColor) ((colorOne, colorTwo, colorThree, patternDef, displacement, tiles) =>
    {
      this.patternToPaint = new PatternData(colorOne, colorTwo, colorThree, patternDef, displacement, tiles);
      if (!DebugSettings.godMode)
        return;
      this.SetColor();
    }));
  }

  public void SetRetexture(RetextureDef retextureDef)
  {
    VehiclePawn.SetRetextureInternal(this, retextureDef);
  }

  private static void SetRetextureInternal(VehiclePawn vehicle, RetextureDef retextureDef)
  {
    vehicle.retextureDef = retextureDef;
    vehicle.ResetGraphic();
    ((Verse.Thing) vehicle).Notify_ColorChanged();
  }

  public void Rename()
  {
    if (!this.Nameable)
      return;
    Find.WindowStack.Add((Window) new Dialog_GiveVehicleName(this));
  }

  public void SetColor()
  {
    if (!this.CanPaintNow)
      return;
    this.patternData.Copy(this.patternToPaint);
    ((Verse.Thing) this).DrawColor = this.patternData.color;
    this.DrawColorTwo = this.patternData.colorTwo;
    this.DrawColorThree = this.patternData.colorThree;
    ((Verse.Thing) this).Notify_ColorChanged();
    this.patternToPaint = (PatternData) null;
  }

  public virtual float DoInspectPaneButtons(float x)
  {
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(x, 0.0f, 30f, 30f);
    float num = 0.0f;
    if (this.Nameable)
    {
      ref Rect local = ref rect;
      ((Rect) ref local).x = ((Rect) ref local).x - ((Rect) ref rect).width;
      num += ((Rect) ref rect).width;
      TooltipHandler.TipRegionByKey(rect, "VF_RenameVehicleTooltip");
      if (Widgets.ButtonImage(rect, TexData.Rename, true, (string) null))
        this.Rename();
    }
    if (VehicleMod.settings.main.useCustomShaders && this.VehicleGraphic.Shader.SupportsRGBMaskTex())
    {
      ref Rect local = ref rect;
      ((Rect) ref local).x = ((Rect) ref local).x - ((Rect) ref rect).width;
      num += ((Rect) ref rect).width;
      TooltipHandler.TipRegionByKey(rect, "VF_RecolorTooltip");
      if (Widgets.ButtonImage(rect, VehicleTex.Recolor, true, (string) null))
        this.ChangeColor();
    }
    if (DebugSettings.ShowDevGizmos)
    {
      ref Rect local = ref rect;
      ((Rect) ref local).x = ((Rect) ref local).x - ((Rect) ref rect).width;
      num += ((Rect) ref rect).width;
      if (Widgets.ButtonImage(rect, VehicleTex.Settings, true, (string) null))
      {
        List<FloatMenuOption> floatMenuOptionList = new List<FloatMenuOption>();
        floatMenuOptionList.Add(new FloatMenuOption("Tweak Values", (Action) (() => Find.WindowStack.Add((Window) new EditWindow_TweakFields((Verse.Thing) this))), (MenuOptionPriority) 4, (Action<Rect>) null, (Verse.Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0));
        if (this.CompVehicleLauncher != null)
          floatMenuOptionList.Add(new FloatMenuOption("Open in Graph Editor", new Action(this.OpenInAnimator), (MenuOptionPriority) 4, (Action<Rect>) null, (Verse.Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0));
        if (floatMenuOptionList.Count > 0)
          Find.WindowStack.Add((Window) new FloatMenu(floatMenuOptionList));
        else
          Messages.Message($"{this} doesn't have any configuration options available.", MessageTypeDefOf.RejectInput, false);
      }
    }
    return num;
  }

  public void OpenInAnimator()
  {
    Find.WindowStack.Add((Window) new Dialog_GraphEditor((IAnimationTarget) this)
    {
      LogReport = VehicleMod.settings.debug.debugLogging
    });
  }

  public void OpenInNewAnimator()
  {
    Find.WindowStack.Add((Window) new Dialog_AnimationEditor((IAnimator) this));
  }

  public void MultiplePawnFloatMenuOptions(List<Pawn> pawns)
  {
    List<FloatMenuOption> options = new List<FloatMenuOption>();
    if (GenCollection.Any<Pawn>(pawns, (Predicate<Pawn>) (pawn => !this.IdeoAllowsBoarding(pawn))))
    {
      options.Add(new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("VF_CantEnterVehicle_IdeoligionForbids")), (Action) null, (MenuOptionPriority) 4, (Action<Rect>) null, (Verse.Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0));
    }
    else
    {
      bool flag = this.HasRoomFor(pawns);
      FloatMenuOption floatMenuOption = new FloatMenuOption(TaggedString.op_Implicit(flag ? TranslatorFormattedStringExtensions.Translate("VF_BoardVehicleGroup", NamedArgument.op_Implicit(((Entity) this).LabelShort)) : TranslatorFormattedStringExtensions.Translate("VF_BoardVehicleGroupFail", NamedArgument.op_Implicit(((Entity) this).LabelShort), NamedArgument.op_Implicit(Translator.Translate("VF_BoardFailureTooMany")))), new Action(OrderPawns), (MenuOptionPriority) 4, (Action<Rect>) null, (Verse.Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
      {
        Disabled = !flag
      };
      options.Add(floatMenuOption);
    }
    FloatMenuMulti floatMenuMulti = new FloatMenuMulti(options, pawns, (Pawn) this, ((Entity) pawns[0]).LabelCap, UI.MouseMapPosition());
    floatMenuMulti.givesColonistOrders = true;
    Find.WindowStack.Add((Window) floatMenuMulti);

    void OrderPawns()
    {
      CellRect cellRect = GenAdj.OccupiedRect((Verse.Thing) this);
      List<IntVec3> list = ((CellRect) ref cellRect).Cells.ToList<IntVec3>();
      foreach (Pawn pawn in pawns)
      {
        if (!list.Contains(((Verse.Thing) pawn).Position))
        {
          VehicleRoleHandler handler1 = pawn.IsColonistPlayerControlled ? this.GetAnyAvailableHandler() : GenCollection.FirstOrDefault<VehicleRoleHandler>(this.handlers, (Predicate<VehicleRoleHandler>) (handler => handler.AreSlotsAvailableAndReservable && handler.role.HandlingTypes == HandlingType.None));
          this.PromptToBoardVehicle(pawn, handler1);
        }
      }
    }
  }

  public ISustainerTarget SustainerTarget { get; private set; }

  public void SetSustainerTarget(ISustainerTarget sustainerTarget)
  {
    this.SustainerTarget = sustainerTarget;
  }

  public void ReleaseSustainerTarget()
  {
    this.sustainers.EndAll();
    this.SustainerTarget = (ISustainerTarget) null;
  }

  public virtual void SoundCleanup()
  {
    if (this.sustainers == null)
      return;
    this.sustainers.EndAll();
  }

  [AnimationEvent]
  private void PlaySound(SoundDef soundDef)
  {
    if (!((Verse.Thing) this).Spawned)
      return;
    SoundStarter.PlayOneShot(soundDef, SoundInfo.op_Implicit((Verse.Thing) this));
  }

  [AnimationEvent]
  private void PlaySustainer(SoundDef soundDef)
  {
    if (!((Verse.Thing) this).Spawned)
      return;
    this.sustainers.Spawn(this, soundDef);
  }

  [AnimationEvent]
  private void EndSustainer(SoundDef soundDef) => this.sustainers.EndAll(soundDef);

  [AnimationEvent]
  private void EndAllSustainers() => this.sustainers.EndAll();

  public bool IdlePawnsInVehicle => this.ticksSinceBoarded >= 5000 && this.AllPawnsAboard.Count > 0;

  public virtual bool Suspended => false;

  public int AttachedExplosives => this.explosives.Count;

  bool IThingHolderTickable.ShouldTickContents => false;

  protected virtual int MaxTickIntervalRate => 250;

  public virtual int UpdateRateTicks
  {
    get
    {
      return this.AllPawnsAboard.Count == 0 && this.compTickers.Count == 0 ? 250 : base.UpdateRateTicks;
    }
  }

  public void AddTimedExplosion(TimedExplosion exploder)
  {
    this.explosives.Add(exploder);
    this.DrawTracker.AddRenderer((IParallelRenderer) exploder);
  }

  public TimedExplosion AddTimedExplosion(
    TimedExplosion.Data explosionData,
    DrawOffsets drawOffsets = null)
  {
    TimedExplosion exploder = new TimedExplosion(this, explosionData, drawOffsets);
    this.AddTimedExplosion(exploder);
    return exploder;
  }

  [Profile]
  protected virtual void Tick()
  {
    this.BaseTickOptimized();
    this.TickAllComps();
    if (((Verse.Thing) this).Faction == Faction.OfPlayer)
      return;
    this.vehicleAI?.AITick();
  }

  public bool RequestTickStart<T>(T comp) where T : ThingComp
  {
    if (this.compTickers.Contains((ThingComp) comp))
      return false;
    this.compTickers.Add((ThingComp) comp);
    return true;
  }

  public bool RequestTickStop<T>(T comp) where T : ThingComp
  {
    return this.compTickers.Remove((ThingComp) comp);
  }

  private void TickExplosives()
  {
    for (int index = this.explosives.Count - 1; index >= 0; --index)
    {
      TimedExplosion explosive = this.explosives[index];
      if (!explosive.Tick())
      {
        this.explosives.Remove(explosive);
        this.DrawTracker.RemoveRenderer((IParallelRenderer) explosive);
      }
    }
  }

  protected virtual void TickAllComps()
  {
    for (int index = this.compTickers.Count - 1; index >= 0; --index)
      this.compTickers[index].CompTick();
  }

  public virtual void TickRare()
  {
    base.TickRare();
    this.EventRegistry[VehicleEventDefOf.ScanRare].ExecuteEvents();
  }

  private void TickShort() => this.EventRegistry[VehicleEventDefOf.ScanShort].ExecuteEvents();

  [Profile]
  protected virtual void TickInterval(int delta)
  {
    if (this.cachedComps != null)
    {
      for (int index = 0; index < this.cachedComps.Count; ++index)
        this.cachedComps[index].CompTickInterval(delta);
    }
    this.ageTracker.AgeTickInterval(delta);
    this.records.RecordsTickInterval(delta);
    if (!WorldPawnsUtility.IsWorldPawn((Pawn) this))
      this.jobs.JobTrackerTickInterval(delta);
    if (!((Verse.Thing) this).Spawned || this.vehiclePather.Moving || this.ticksSinceBoarded >= 5000)
      return;
    this.ticksSinceBoarded += delta;
  }

  [Profile]
  private void TickHandlers()
  {
    foreach (VehicleRoleHandler occupiedHandler in this.OccupiedHandlers)
      occupiedHandler.DoTick();
  }

  [Profile]
  protected virtual void BaseTickOptimized()
  {
    if (Gen.IsHashIntervalTick((Verse.Thing) this, 60))
      this.TickShort();
    if (Gen.IsHashIntervalTick((Verse.Thing) this, 250))
      ((Entity) this).TickRare();
    this.sustainers.Tick();
    this.TickHandlers();
    if (((Verse.Thing) this).Spawned)
    {
      this.vehiclePather.PatherTick();
      this.stances.StanceTrackerTick();
      if (!this.Drafted && !this.fishing)
      {
        CompVehicleTurrets compVehicleTurrets = this.CompVehicleTurrets;
        if (compVehicleTurrets == null || !compVehicleTurrets.Deploying)
          goto label_9;
      }
      this.jobs.JobTrackerTick();
      if (this.vehiclePather.Moving)
        this.fishing = false;
label_9:
      this.TickExplosives();
    }
    ((ThingOwner) this.inventory.innerContainer).DoTick();
    this.inventory.InventoryTrackerTick();
  }
}
