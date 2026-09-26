// Decompiled with JetBrains decompiler
// Type: Vehicles.CompVehicleTurrets
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools;
using SmashTools.Performance;
using SmashTools.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles.Rendering;
using Verse;
using Verse.AI;
using Verse.Sound;

#nullable disable
namespace Vehicles;

[PublicAPI]
[HeaderTitle(Label = "CompVehicleTurrets")]
public class CompVehicleTurrets : VehicleAIComp, IRefundable
{
  private static readonly HashSet<string> DuplicateKeySet = new HashSet<string>();
  private List<CompVehicleTurrets.TurretData> turretQueue = new List<CompVehicleTurrets.TurretData>();
  private bool deployed;
  internal int deployTicks;
  private Dictionary<VehicleTurret, int> turretQuotas = new Dictionary<VehicleTurret, int>();
  private List<CompVehicleTurrets.BackupTurretQuota> backupQuotas = new List<CompVehicleTurrets.BackupTurretQuota>();
  [TweakField]
  private List<VehicleTurret> turrets = new List<VehicleTurret>();
  [Unsaved(false)]
  private readonly List<VehicleTurret> tickers = new List<VehicleTurret>();
  private List<VehicleTurret> tmpListTurrets = new List<VehicleTurret>();
  private List<int> tmpListTurretQuota = new List<int>();
  private Command_Toggle deployToggle;
  private readonly List<Command_Turret> turretGizmos = new List<Command_Turret>();

  public float MinRange { get; private set; }

  public float MaxRange { get; private set; }

  public bool CanDeploy { get; private set; }

  public bool Deployed => this.deployed;

  public int DeployTicks
  {
    get
    {
      return Mathf.RoundToInt(SettingsCache.TryGetValue<float>(this.Vehicle.VehicleDef, typeof (CompProperties_VehicleTurrets), "deployTime", this.Props.deployTime) * 60f);
    }
  }

  public bool Deploying => this.Vehicle.jobs.curJob?.def == JobDefOf_Vehicles.DeployVehicle;

  private bool ShouldStopTicking => this.tickers.Count == 0;

  public CompProperties_VehicleTurrets Props => (CompProperties_VehicleTurrets) this.props;

  public IReadOnlyList<VehicleTurret> Turrets => (IReadOnlyList<VehicleTurret>) this.turrets;

  public IEnumerable<(ThingDef thingDef, float count)> Refunds
  {
    get
    {
      foreach (VehicleTurret turret in this.turrets)
        yield return (turret.loadedAmmo, (float) turret.shellCount * turret.def.chargePerAmmoCount);
    }
  }

  public bool TurretsAligned
  {
    get
    {
      foreach (VehicleTurret turret in this.turrets)
      {
        if ((turret.deployment == DeploymentType.Deployed && this.Deployed || turret.deployment == DeploymentType.Undeployed && !this.Deployed) && !turret.RotationAligned)
          return false;
      }
      return true;
    }
  }

  public float OptimalDistance
  {
    get => this.MaxRange * this.Vehicle.VehicleDef.npcProperties.targetPositionRadiusPercent;
  }

  public void FlagAllTurretsForAlignment()
  {
    foreach (VehicleTurret turret in this.turrets)
    {
      if (turret.deployment != DeploymentType.None)
      {
        if (TurretTargeter.Turret == turret)
          TurretTargeter.Instance.StopTargeting(true);
        if (!Mathf.Approximately(turret.TurretRotation, turret.defaultAngleRotated))
        {
          turret.SetTarget(LocalTargetInfo.Invalid);
          turret.FlagForAlignment();
          turret.StartTicking();
        }
      }
    }
  }

  public void SetQuotaLevel(VehicleTurret turret, int level)
  {
    this.turretQuotas[turret] = level;
    this.RecacheTurretAutoLoading(turret);
  }

  public int GetQuotaLevel(VehicleTurret turret)
  {
    int quotaLevel;
    if (!this.turretQuotas.TryGetValue(turret, out quotaLevel))
      quotaLevel = Mathf.CeilToInt(turret.def.autoRefuelProportion * (float) turret.def.magazineCapacity * turret.def.chargePerAmmoCount);
    return quotaLevel;
  }

  public VehicleTurret GetTurret(string key)
  {
    foreach (VehicleTurret turret in this.turrets)
    {
      if (turret.key == key)
        return turret;
    }
    return (VehicleTurret) null;
  }

  public override void OnDestroy()
  {
    foreach (VehicleTurret turret in this.turrets)
      turret.OnDestroy();
  }

  public override void PostLoad()
  {
    if (this.turrets == null)
      this.turrets = new List<VehicleTurret>();
    this.RecacheTurretPermissions();
    foreach (VehicleTurret turret in this.turrets)
      turret.PostPostLoadInit();
  }

  private void RecacheGizmos()
  {
    this.turretGizmos.Clear();
    if (this.CanDeploy)
      this.deployToggle = new Command_Toggle()
      {
        toggleAction = (Action) (() =>
        {
          this.Vehicle.jobs.StartJob(new Job(JobDefOf_Vehicles.DeployVehicle, LocalTargetInfo.op_Implicit((Thing) this.Vehicle)), (JobCondition) 16 /*0x10*/, (ThinkNode) null, false, true, (ThinkTreeDef) null, new JobTag?(), false, false, new bool?(), false, true, false);
          this.deployTicks = this.DeployTicks;
        }),
        isActive = (Func<bool>) (() => this.Deployed)
      };
    HashSet<string> stringSet = new HashSet<string>();
    foreach (VehicleTurret turret in this.turrets)
    {
      switch (turret.def.turretType)
      {
        case TurretType.Rotatable:
          if (turret.manualTargeting)
          {
            this.turretGizmos.Add(this.GetRotatableTurretGizmo(turret));
            continue;
          }
          continue;
        case TurretType.Static:
          if (GenText.NullOrEmpty(turret.groupKey) || !stringSet.Contains(turret.groupKey))
          {
            this.turretGizmos.Add(this.GetStaticTurretGizmo(turret));
            continue;
          }
          continue;
        default:
          throw new NotImplementedException("TurretType");
      }
    }
  }

  private Command_Turret GetStaticTurretGizmo(VehicleTurret turret)
  {
    Command_CooldownAction commandCooldownAction = new Command_CooldownAction();
    commandCooldownAction.vehicle = this.Vehicle;
    commandCooldownAction.turret = turret;
    commandCooldownAction.defaultLabel = !string.IsNullOrEmpty(turret.gizmoLabel) ? turret.gizmoLabel : TaggedString.op_Implicit(turret.def.LabelCap);
    commandCooldownAction.icon = (Texture) turret.GizmoIcon;
    commandCooldownAction.iconDrawScale = turret.def.gizmoIconScale;
    commandCooldownAction.canReload = this.turrets.All<VehicleTurret>((Func<VehicleTurret, bool>) (t => t.def.ammunition != null));
    Command_CooldownAction staticTurretGizmo = commandCooldownAction;
    if (!string.IsNullOrEmpty(turret.def.gizmoDescription))
      staticTurretGizmo.defaultDesc = turret.def.gizmoDescription;
    return (Command_Turret) staticTurretGizmo;
  }

  private Command_Turret GetRotatableTurretGizmo(VehicleTurret turret)
  {
    Command_TargeterCooldownAction targeterCooldownAction = new Command_TargeterCooldownAction();
    targeterCooldownAction.vehicle = this.Vehicle;
    targeterCooldownAction.turret = turret;
    targeterCooldownAction.defaultLabel = !string.IsNullOrEmpty(turret.gizmoLabel) ? turret.gizmoLabel : TaggedString.op_Implicit(turret.def.LabelCap);
    targeterCooldownAction.icon = (Texture) turret.GizmoIcon;
    targeterCooldownAction.iconDrawScale = turret.def.gizmoIconScale;
    Command_TargeterCooldownAction rotatableTurretGizmo = targeterCooldownAction;
    if (!string.IsNullOrEmpty(turret.def.gizmoDescription))
      rotatableTurretGizmo.defaultDesc = turret.def.gizmoDescription;
    rotatableTurretGizmo.targetingParams = new TargetingParameters()
    {
      canTargetLocations = true
    };
    return (Command_Turret) rotatableTurretGizmo;
  }

  public virtual IEnumerable<Gizmo> CompGetGizmosExtra()
  {
    CompVehicleTurrets compVehicleTurrets1 = this;
    if (((Thing) compVehicleTurrets1.Vehicle).Faction == Faction.OfPlayer || DebugSettings.ShowDevGizmos)
    {
      CompUpgradeTree compUpgradeTree = compVehicleTurrets1.Vehicle.CompUpgradeTree;
      bool upgrading = compUpgradeTree != null && compUpgradeTree.Upgrading;
      if (compVehicleTurrets1.CanDeploy)
      {
        ((Gizmo) compVehicleTurrets1.deployToggle).Disabled = false;
        ((Gizmo) compVehicleTurrets1.deployToggle).disabledReason = (string) null;
        ((Command) compVehicleTurrets1.deployToggle).icon = compVehicleTurrets1.Deployed ? (Texture) VehicleTex.UndeployVehicle : (Texture) VehicleTex.DeployVehicle;
        ((Command) compVehicleTurrets1.deployToggle).defaultLabel = TaggedString.op_Implicit(compVehicleTurrets1.Deployed ? Translator.Translate("VF_Undeploy") : Translator.Translate("VF_Deploy"));
        ((Command) compVehicleTurrets1.deployToggle).defaultDesc = TaggedString.op_Implicit(Translator.Translate("VF_DeployDescription"));
        if (!compVehicleTurrets1.Vehicle.CanMoveFinal || compVehicleTurrets1.Deploying || compVehicleTurrets1.Vehicle.vehiclePather.Moving)
          ((Gizmo) compVehicleTurrets1.deployToggle).Disable((string) null);
        if (upgrading)
          ((Gizmo) compVehicleTurrets1.deployToggle).Disable(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_DisabledByVehicleUpgrading", NamedArgument.op_Implicit(((Entity) compVehicleTurrets1.Vehicle).LabelCap))));
        yield return (Gizmo) compVehicleTurrets1.deployToggle;
      }
      foreach (Command_Turret turretGizmo in compVehicleTurrets1.turretGizmos)
      {
        CompVehicleTurrets compVehicleTurrets = compVehicleTurrets1;
        ((Gizmo) turretGizmo).Disabled = false;
        ((Gizmo) turretGizmo).disabledReason = (string) null;
        VehicleTurret turret = turretGizmo.turret;
        foreach (VehicleRoleHandler vehicleRoleHandler in compVehicleTurrets1.Vehicle.GetHandlers(HandlingType.Turret).Where<VehicleRoleHandler>((Func<VehicleRoleHandler, bool>) (handler => GenList.NotNullAndContains<string>((IList<string>) handler.role.TurretIds, turret.key))))
        {
          if (((ThingOwner) vehicleRoleHandler.thingOwner).Count < vehicleRoleHandler.role.SlotsToOperate && !VehicleMod.settings.debug.debugShootAnyTurret)
          {
            ((Gizmo) turretGizmo).Disable(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_NotEnoughCrew", NamedArgument.op_Implicit(((Entity) compVehicleTurrets1.Vehicle).LabelShort), NamedArgument.op_Implicit(vehicleRoleHandler.role.label))));
            break;
          }
        }
        string reason;
        if (turret.IsDisabled(out reason))
          ((Gizmo) turretGizmo).Disable(reason);
        if (upgrading)
          ((Gizmo) turretGizmo).Disable(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_DisabledByVehicleUpgrading", NamedArgument.op_Implicit(((Entity) compVehicleTurrets1.Vehicle).LabelCap))));
        yield return (Gizmo) turretGizmo;
        if (DebugSettings.ShowDevGizmos)
        {
          Command_Action commandAction = new Command_Action();
          ((Command) commandAction).defaultLabel = "Full Refill: " + turret.gizmoLabel;
          commandAction.action = (Action) (() => compVehicleTurrets.DevModeReloadTurret(turret));
          yield return (Gizmo) commandAction;
        }
      }
    }
  }

  public override AcceptanceReport CanMove(FloatMenuContext context)
  {
    return this.Deploying || this.Deployed ? AcceptanceReport.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_VehicleImmobileDeployed", NamedArgument.op_Implicit((Thing) this.Vehicle))) : AcceptanceReport.op_Implicit(true);
  }

  public override AcceptanceReport CanDraft()
  {
    return this.Deploying ? AcceptanceReport.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_VehicleUnableToMove", NamedArgument.op_Implicit((Thing) this.Vehicle))) : AcceptanceReport.op_Implicit(true);
  }

  public void QueueTicker(VehicleTurret turret)
  {
    if (this.tickers.Contains(turret))
      return;
    this.tickers.Add(turret);
    this.StartTicking();
  }

  public void DequeueTicker(VehicleTurret turret)
  {
    this.tickers.Remove(turret);
    if (!this.ShouldStopTicking)
      return;
    this.StopTicking();
  }

  public void QueueTurret(CompVehicleTurrets.TurretData turretData)
  {
    turretData.turret.queuedToFire = true;
    this.turretQueue.Add(turretData);
    turretData.turret.EventRegistry[VehicleTurretEventDefOf.Queued].ExecuteEvents();
  }

  public void DequeueTurret(CompVehicleTurrets.TurretData turretData)
  {
    turretData.turret.queuedToFire = false;
    this.turretQueue.RemoveAll((Predicate<CompVehicleTurrets.TurretData>) (td => td.turret == turretData.turret));
    turretData.turret.EventRegistry[VehicleTurretEventDefOf.Dequeued].ExecuteEvents();
  }

  private void ResolveTurretQueue()
  {
    for (int index = this.turretQueue.Count - 1; index >= 0; --index)
    {
      CompVehicleTurrets.TurretData turret = this.turretQueue[index];
      try
      {
        if (!((LocalTargetInfo) ref turret.turret.targetInfo).IsValid || !turret.turret.HasAmmo)
          this.DequeueTurret(turret);
        else if (!turret.CanTarget)
        {
          turret.turret.SetTarget(LocalTargetInfo.Invalid);
          this.DequeueTurret(turret);
        }
        else
        {
          this.turretQueue[index].turret.AlignToTargetRestricted();
          if (this.turretQueue[index].ticksTillShot <= 0)
          {
            turret.turret.FireTurret();
            ++turret.turret.CurrentTurretFiring;
            --turret.shots;
            turret.ticksTillShot = turret.turret.TicksPerShot;
            if (!turret.turret.OnCooldown && turret.shots != 0)
            {
              if (turret.turret.HasAmmo)
                continue;
            }
            if (turret.turret.targetPersists)
            {
              turret.turret.CheckTargetInvalid();
            }
            else
            {
              Thing thing = ((LocalTargetInfo) ref turret.turret.targetInfo).Thing;
              if (thing != null)
              {
                if ((turret.turret.targeting & TargetLock.Thing) == (TargetLock) 0 || thing is Pawn && (turret.turret.targeting & TargetLock.Pawn) == (TargetLock) 0)
                  turret.turret.SetTarget(LocalTargetInfo.Invalid);
              }
              else if ((turret.turret.targeting & TargetLock.Cell) == (TargetLock) 0)
                turret.turret.SetTarget(LocalTargetInfo.Invalid);
            }
            if (!turret.turret.HasAmmo)
              turret.turret.Reload();
            this.DequeueTurret(turret);
          }
          else
            --turret.ticksTillShot;
        }
      }
      catch (Exception ex)
      {
        turret.turret.SetTarget(LocalTargetInfo.Invalid);
        this.DequeueTurret(turret);
        Log.Error($"Exception thrown while shooting turret {turret.turret}. Removing from " + $"queue to resolve issue temporarily.{Environment.NewLine}Exception={ex}");
      }
    }
  }

  private void DevModeReloadTurret(VehicleTurret turret)
  {
    if (turret.def.ammunition == null)
    {
      turret.Reload();
    }
    else
    {
      ThingDef ammoDef = turret.def.ammunition.AllowedThingDefs.FirstOrDefault<ThingDef>();
      if (ammoDef != null)
      {
        Thing thing = ThingMaker.MakeThing(ammoDef, (ThingDef) null);
        int num = Mathf.CeilToInt((this.Vehicle.GetStatValue(VehicleStatDefOf.CargoCapacity) - MassUtility.InventoryMass((Pawn) this.Vehicle)) / StatExtension.GetStatValueAbstract((BuildableDef) ammoDef, StatDefOf.Mass, (ThingDef) null));
        thing.stackCount = Mathf.Min(ammoDef.stackLimit, num);
        this.Vehicle.AddOrTransfer(thing);
        turret.Reload(ammoDef);
      }
      else
        Log.Error($"Unable to reload {turret} through DevMode, no AllowedThingDefs in ammunition list.");
    }
  }

  [Profile]
  public virtual void CompTick()
  {
    base.CompTick();
    if (!((Thing) this.Vehicle).Spawned)
      return;
    this.ResolveTurretQueue();
    for (int index = this.tickers.Count - 1; index >= 0; --index)
    {
      VehicleTurret ticker = this.tickers[index];
      if ((!this.Vehicle.stances.stunner.Stunned || !ticker.def.empDisables) && !ticker.Tick())
        this.DequeueTicker(ticker);
    }
    if (!this.ShouldStopTicking)
      return;
    this.StopTicking();
  }

  public override void AIAutoCheck()
  {
    foreach (VehicleTurret turret in this.turrets)
    {
      if (turret.shellCount < Mathf.CeilToInt((float) turret.def.magazineCapacity / 4f) && (!turret.TargetLocked || turret.shellCount <= 0))
        turret.AutoReload();
    }
  }

  public override bool IsThreat(IAttackTargetSearcher searcher)
  {
    if (!GenList.NullOrEmpty<VehicleTurret>((IList<VehicleTurret>) this.turrets))
    {
      foreach (VehicleTurret turret in this.turrets)
      {
        if (!turret.TurretDisabled)
          return true;
      }
    }
    return false;
  }

  public void ToggleDeployment()
  {
    this.deployed = !this.deployed;
    this.deployTicks = 0;
    if (this.deployed)
    {
      SoundDef deploySound = this.Props.deploySound;
      if (deploySound != null)
        SoundStarter.PlayOneShot(deploySound, SoundInfo.op_Implicit((Thing) this.Vehicle));
      this.Vehicle.EventRegistry[VehicleEventDefOf.Deployed].ExecuteEvents();
    }
    else
    {
      SoundDef undeploySound = this.Props.undeploySound;
      if (undeploySound != null)
        SoundStarter.PlayOneShot(undeploySound, SoundInfo.op_Implicit((Thing) this.Vehicle));
      this.Vehicle.EventRegistry[VehicleEventDefOf.Undeployed].ExecuteEvents();
    }
  }

  public virtual void PostDeSpawn(Map map, DestroyMode mode = 0)
  {
    base.PostDeSpawn(map, mode);
    for (int index = this.tickers.Count - 1; index >= 0; --index)
      this.DequeueTicker(this.tickers[index]);
  }

  public override void PostGeneration()
  {
    this.CreateTurretInstances();
    if (((Thing) this.Vehicle).Faction == Faction.OfPlayer)
      return;
    this.FillMagazineCapacity();
  }

  public override void Notify_ColorChanged()
  {
    foreach (VehicleTurret turret in this.turrets)
      turret.ResolveGraphics(this.Vehicle.patternData, true);
  }

  public override void EventRegistration()
  {
    this.Vehicle.AddEvent<VehicleEventDef>(VehicleEventDefOf.PawnEntered, new Action(this.RecacheTurretPermissions), new Action(this.ReloadAllTurretsIfEmpty));
    this.Vehicle.AddEvent<VehicleEventDef>(VehicleEventDefOf.PawnExited, new Action(this.RecacheTurretPermissions));
    this.Vehicle.AddEvent<VehicleEventDef>(VehicleEventDefOf.CargoAdded, new Action(this.ReloadAllTurretsIfEmpty));
    this.Vehicle.AddEvent<VehicleEventDef>(VehicleEventDefOf.PawnChangedSeats, new Action(this.RecacheTurretPermissions), new Action(this.ReloadAllTurretsIfEmpty));
    this.Vehicle.AddEvent<VehicleEventDef>(VehicleEventDefOf.PawnKilled, new Action(this.RecacheTurretPermissions));
    this.Vehicle.AddEvent<VehicleEventDef>(VehicleEventDefOf.PawnCapacitiesDirty, new Action(this.RecacheTurretPermissions));
    ((ThingOwner) this.Vehicle.inventory.innerContainer).OnContentsChanged += new Action(this.RecacheAllTurretAutoLoading);
    foreach (VehicleTurret turret in this.turrets)
      this.RegisterEventsFor(turret);
  }

  private void RegisterEventsFor(VehicleTurret turret)
  {
    turret.FillEventsDef<VehicleTurretEventDef>();
  }

  private void CreateTurretInstances()
  {
    if (GenList.NullOrEmpty<VehicleTurret>((IList<VehicleTurret>) this.Props.turrets))
      return;
    foreach (VehicleTurret turret in this.Props.turrets)
    {
      try
      {
        this.CopyAndAddTurret(turret);
      }
      catch (Exception ex)
      {
        Log.Error($"Exception thrown while attempting to generate {turret.def.label} {$"for {((Entity) this.Vehicle).Label}. Exception=\"{ex}\""}");
      }
    }
    this.CheckDuplicateKeys();
  }

  internal void CheckDuplicateKeys()
  {
    using (new ClearOnDispose<string>((ICollection<string>) CompVehicleTurrets.DuplicateKeySet))
    {
      foreach (VehicleTurret turret in this.turrets)
      {
        if (!CompVehicleTurrets.DuplicateKeySet.Add(turret.key))
          Log.Warning($"Duplicate VehicleTurret key {turret.key}. These must be unique.");
      }
    }
  }

  public VehicleTurret CopyAndAddTurret(VehicleTurret reference, string upgradeKey = null)
  {
    VehicleTurret instance = (VehicleTurret) Activator.CreateInstance(reference.GetType(), (object) this.Vehicle, (object) reference);
    this.SetDefaults(instance);
    instance.Init(reference);
    this.AddTurret(instance, upgradeKey);
    return instance;
  }

  private void SetDefaults(VehicleTurret turret)
  {
    turret.SetTarget(LocalTargetInfo.Invalid);
    turret.ResetAngle();
    this.RegisterEventsFor(turret);
  }

  public void AddTurret(VehicleTurret turret, string upgradeKey)
  {
    turret.upgradeKey = upgradeKey;
    this.turrets.Add(turret);
    this.RevalidateTurrets();
    if (((Thing) this.Vehicle).Spawned)
      LongEventHandler.ExecuteWhenFinished(new Action(this.RecacheGizmos));
    VehicleTurret vehicleTurret = turret;
    if (vehicleTurret.loadConfig == null)
      vehicleTurret.loadConfig = new AutoLoadConfig(turret);
    if (!GenList.NullOrEmpty<CompVehicleTurrets.BackupTurretQuota>((IList<CompVehicleTurrets.BackupTurretQuota>) this.backupQuotas))
    {
      for (int index = 0; index < this.backupQuotas.Count; ++index)
      {
        CompVehicleTurrets.BackupTurretQuota backupQuota = this.backupQuotas[index];
        if (backupQuota.key == turret.key && backupQuota.upgradeKey == turret.upgradeKey)
        {
          this.SetQuotaLevel(turret, backupQuota.config);
          this.backupQuotas.RemoveAt(index);
          break;
        }
      }
    }
    List<UpgradeState> outList;
    if (upgradeKey != null && this.Vehicle.CompUpgradeTree != null && this.Vehicle.CompUpgradeTree.TryGetStates(turret.key, out outList))
    {
      foreach (UpgradeState upgradeState in outList)
      {
        if (!GenList.NullOrEmpty<UpgradeState.Setting>((IList<UpgradeState.Setting>) upgradeState.settings))
        {
          foreach (UpgradeState.Setting setting in upgradeState.settings)
          {
            if (setting is UpgradeSetting_Turret upgradeSettingTurret && upgradeSettingTurret.turretKey == turret.key)
              upgradeSettingTurret.Unlocked(this.Vehicle, false);
          }
        }
      }
    }
    this.CacheBoundaries();
  }

  public bool RemoveTurret(string key)
  {
    for (int index = this.turrets.Count - 1; index >= 0; --index)
    {
      VehicleTurret turret = this.turrets[index];
      if (turret.key == key)
        return this.RemoveTurret(turret);
    }
    return false;
  }

  public bool RemoveTurret(VehicleTurret turret)
  {
    turret.TryClearChamber();
    int num;
    if (this.turretQuotas.TryGetValue(turret, out num))
    {
      this.backupQuotas.Add(new CompVehicleTurrets.BackupTurretQuota()
      {
        key = turret.key,
        upgradeKey = turret.upgradeKey,
        config = num
      });
      this.turretQuotas.Remove(turret);
    }
    turret.OnDestroy();
    bool flag = this.turrets.Remove(turret);
    this.TryDeregisterRenderer(turret);
    if (((Thing) this.Vehicle).Spawned)
      LongEventHandler.ExecuteWhenFinished(new Action(this.RecacheGizmos));
    return flag;
  }

  private void TryRegisterRenderer(VehicleTurret turret)
  {
    if (turret.NoGraphic || turret.attachedTo != null)
      return;
    this.Vehicle.DrawTracker.AddRenderer((IParallelRenderer) turret);
  }

  private void TryDeregisterRenderer(VehicleTurret turret)
  {
    if (turret.NoGraphic)
      return;
    this.Vehicle.DrawTracker.RemoveRenderer((IParallelRenderer) turret);
  }

  public void RevalidateTurrets()
  {
    this.ResolveAllTurretChildren();
    this.RecacheDeployment();
    this.RecacheTurretPermissions();
  }

  private void ResolveAllTurretChildren()
  {
    foreach (VehicleTurret turret in this.turrets)
    {
      turret.attachedTo = (VehicleTurret) null;
      turret.childTurrets.Clear();
      this.TryDeregisterRenderer(turret);
      this.ResolveTurretChildren(turret);
    }
    foreach (VehicleTurret turret in this.turrets)
      this.TryRegisterRenderer(turret);
  }

  private void ResolveTurretChildren(VehicleTurret turret)
  {
    turret.childTurrets = new List<VehicleTurret>();
    if (string.IsNullOrEmpty(turret.parentKey))
      return;
    foreach (VehicleTurret turret1 in this.turrets)
    {
      if (turret1.key == turret.parentKey)
      {
        turret.attachedTo = turret1;
        if (turret1.attachedTo == turret || turret == turret1)
        {
          Log.Error("Recursive turret attachments detected, this is not allowed. Disconnecting turret from parent.");
          turret.attachedTo = (VehicleTurret) null;
        }
        else
          turret1.childTurrets.Add(turret);
      }
    }
  }

  private void InitTurrets()
  {
    for (int index = this.turrets.Count - 1; index >= 0; --index)
    {
      VehicleTurret turret = this.turrets[index];
      VehicleTurret turretReference = this.FindTurretReference(turret);
      if (turretReference != null)
      {
        this.InitTurret(turret, turretReference);
      }
      else
      {
        Log.Error($"Unable to find reference turret for key {turret.key}. Turrets can only be added if they exist in the vehicle's upgrade tree or in its initial turrets.");
        this.turrets.Remove(turret);
      }
    }
  }

  private VehicleTurret FindTurretReference(VehicleTurret turret)
  {
    if (!GenText.NullOrEmpty(turret.upgradeKey) && this.Vehicle.CompUpgradeTree != null)
    {
      foreach (UpgradeNode node in this.Vehicle.CompUpgradeTree.Props.def.nodes)
      {
        if (!GenList.NullOrEmpty<Upgrade>((IList<Upgrade>) node.upgrades))
        {
          foreach (Upgrade upgrade in node.upgrades)
          {
            VehicleTurret result;
            if (upgrade is TurretUpgrade turretUpgrade && CompVehicleTurrets.MatchingTurret(turret.key, turretUpgrade.turrets, out result))
              return result;
          }
        }
      }
    }
    VehicleTurret result1;
    if (CompVehicleTurrets.MatchingTurret(turret.key, this.Props.turrets, out result1))
      return result1;
    if (this.Vehicle.CompUpgradeTree != null)
    {
      Log.Warning($"Unable to locate {turret.key} in CompProperties with null upgradeKey. Sweeping UpgradeTree for any matching turret.");
      foreach (UpgradeNode node in this.Vehicle.CompUpgradeTree.Props.def.nodes)
      {
        if (!GenList.NullOrEmpty<Upgrade>((IList<Upgrade>) node.upgrades))
        {
          foreach (Upgrade upgrade in node.upgrades)
          {
            VehicleTurret result2;
            if (upgrade is TurretUpgrade turretUpgrade && CompVehicleTurrets.MatchingTurret(turret.key, turretUpgrade.turrets, out result2))
              return result2;
          }
        }
      }
    }
    return (VehicleTurret) null;
  }

  private static bool MatchingTurret(
    string key,
    List<VehicleTurret> turrets,
    out VehicleTurret result)
  {
    result = (VehicleTurret) null;
    if (GenList.NullOrEmpty<VehicleTurret>((IList<VehicleTurret>) turrets))
      return false;
    foreach (VehicleTurret turret in turrets)
    {
      if (turret.key == key)
      {
        result = turret;
        return true;
      }
    }
    return false;
  }

  private void InitTurret(VehicleTurret turret, VehicleTurret reference)
  {
    turret.Init(reference);
    this.ResolveTurretChildren(turret);
    this.QueueTicker(turret);
  }

  private void FillMagazineCapacity()
  {
    foreach (VehicleTurret turret in this.turrets)
      turret.SetMagazineCount(int.MaxValue);
  }

  private void CacheBoundaries()
  {
    if (GenList.NullOrEmpty<VehicleTurret>((IList<VehicleTurret>) this.turrets))
      return;
    this.MinRange = this.turrets.Min<VehicleTurret>((Func<VehicleTurret, float>) (turret => turret.MinRange));
    this.MaxRange = this.turrets.Min<VehicleTurret>((Func<VehicleTurret, float>) (turret => turret.MaxRange));
  }

  public void RecacheDeployment()
  {
    this.CanDeploy = (double) SettingsCache.TryGetValue<float>(this.Vehicle.VehicleDef, typeof (CompProperties_VehicleTurrets), "deployTime", this.Props.deployTime) > 0.0;
  }

  public void RecacheTurretPermissions()
  {
    foreach (VehicleTurret turret in this.turrets)
      turret.RecacheMannedStatus();
  }

  public void ReloadAllTurretsIfEmpty()
  {
    foreach (VehicleTurret turret in this.turrets)
      turret.ReloadIfEmpty();
  }

  private void RecacheTurretComponents()
  {
    foreach (VehicleTurret turret in this.turrets)
      turret.component?.RecacheComponent(this.Vehicle);
  }

  private void RecacheAllTurretAutoLoading()
  {
    foreach (VehicleTurret turret in this.turrets)
      this.RecacheTurretAutoLoading(turret);
  }

  private void RecacheTurretAutoLoading(VehicleTurret turret)
  {
    if (!((Thing) this.Vehicle).Spawned)
      return;
    int num = GenCollection.TryGetValue<VehicleTurret, int>((IReadOnlyDictionary<VehicleTurret, int>) this.turretQuotas, turret, 0);
    if (num <= 0 || QuotaInCargo(this.Vehicle, turret) >= num)
      ((Thing) this.Vehicle).Map.GetCachedMapComponent<VehicleReservationManager>().RemoveLister(this.Vehicle, "LoadVehicleForTurret");
    else
      ((Thing) this.Vehicle).Map.GetCachedMapComponent<VehicleReservationManager>().RegisterLister(this.Vehicle, "LoadVehicleForTurret");

    static int QuotaInCargo(VehiclePawn vehicle, VehicleTurret turret)
    {
      int num = 0;
      foreach (Thing thing in vehicle.inventory.innerContainer)
      {
        if (turret.def.ammunition != null && turret.def.ammunition.Allows(thing) || turret.def.genericAmmo && turret.def.projectile == thing.def)
          num += thing.stackCount;
      }
      return num;
    }
  }

  public virtual void PostSpawnSetup(bool respawningAfterLoad)
  {
    base.PostSpawnSetup(respawningAfterLoad);
    try
    {
      this.RevalidateTurrets();
      this.RecacheTurretComponents();
      foreach (VehicleTurret turret in this.turrets)
        turret.PostSpawnSetup(respawningAfterLoad);
      this.CacheBoundaries();
      if (!respawningAfterLoad)
      {
        foreach (VehicleTurret turret in this.turrets)
          this.SetQuotaLevel(turret, this.GetQuotaLevel(turret));
      }
      LongEventHandler.ExecuteWhenFinished(new Action(this.RecacheGizmos));
    }
    catch (Exception ex)
    {
      Log.Error($"Exception caught while initializing turrets in PostSpawnSetup.\nException={ex}");
    }
  }

  public virtual void PostExposeData()
  {
    base.PostExposeData();
    Scribe_Values.Look<bool>(ref this.deployed, "deployed", false, false);
    Scribe_Values.Look<int>(ref this.deployTicks, "deployTicks", 0, false);
    Scribe_Collections.Look<VehicleTurret>(ref this.turrets, "turrets", (LookMode) 2, new object[1]
    {
      (object) this.Vehicle
    });
    Scribe_Collections.Look<CompVehicleTurrets.TurretData>(ref this.turretQueue, "turretQueue", (LookMode) 3, Array.Empty<object>());
    Scribe_Collections.Look<VehicleTurret, int>(ref this.turretQuotas, "turretQuotas", (LookMode) 3, (LookMode) 1, ref this.tmpListTurrets, ref this.tmpListTurretQuota, true, false, false);
    Scribe_Collections.Look<CompVehicleTurrets.BackupTurretQuota>(ref this.backupQuotas, "backupQuotas", (LookMode) 2, Array.Empty<object>());
    if (this.turrets == null)
      this.turrets = new List<VehicleTurret>();
    if (this.turretQueue == null)
      this.turretQueue = new List<CompVehicleTurrets.TurretData>();
    if (this.turretQuotas == null)
      this.turretQuotas = new Dictionary<VehicleTurret, int>();
    if (this.backupQuotas == null)
      this.backupQuotas = new List<CompVehicleTurrets.BackupTurretQuota>();
    if (Scribe.mode != 4)
      return;
    this.InitTurrets();
    LongEventHandler.ExecuteWhenFinished(new Action(this.RecacheAllTurretAutoLoading));
  }

  public class TurretData : IExposable
  {
    public int shots;
    public int ticksTillShot;
    public VehicleTurret turret;

    public TurretData()
    {
    }

    public TurretData(int shots, int ticksTillShot, VehicleTurret turret)
    {
      this.shots = shots;
      this.ticksTillShot = ticksTillShot;
      this.turret = turret;
    }

    public bool CanTarget
    {
      get
      {
        if (this.turret.TurretRestricted || this.turret.OnCooldown)
          return false;
        if (this.turret.IsManned)
          return true;
        return VehicleMod.settings.debug.debugShootAnyTurret && FactionUtility.IsPlayerSafe(((Thing) this.turret.vehicle).Faction);
      }
    }

    public void ExposeData()
    {
      Scribe_Values.Look<int>(ref this.shots, "shots", 0, false);
      Scribe_Values.Look<int>(ref this.ticksTillShot, "ticksTillShot", 0, false);
      Scribe_References.Look<VehicleTurret>(ref this.turret, "turret", false);
    }
  }

  [UsedImplicitly]
  public struct BackupTurretQuota : IExposable
  {
    public string key;
    public string upgradeKey;
    public int config;

    public void ExposeData()
    {
      Scribe_Values.Look<string>(ref this.key, "key", (string) null, false);
      Scribe_Values.Look<string>(ref this.upgradeKey, "upgradeKey", (string) null, false);
      Scribe_Values.Look<int>(ref this.config, "config", 0, false);
    }
  }
}
