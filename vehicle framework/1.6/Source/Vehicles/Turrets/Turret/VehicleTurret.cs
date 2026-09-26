// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleTurret
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Animations;
using SmashTools.Performance;
using SmashTools.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using Vehicles.Config;
using Vehicles.Rendering;
using Verse;
using Verse.AI;
using Verse.Sound;

#nullable enable
namespace Vehicles;

[PublicAPI]
[StaticConstructorOnStartup]
public class VehicleTurret : 
  IExposable,
  ILoadReferenceable,
  ITweakFields,
  IEventManager<
  #nullable disable
  VehicleTurretEventDef>,
  IMaterialCacheTarget,
  IParallelRenderer,
  IBlitTarget,
  ITransformable
{
  public const int TicksPerOverheatingFrame = 15;
  public const int TicksTillBeginCooldown = 60;
  public const float MaxHeatCapacity = 100f;
  public const int DefaultMaxRange = 9999;
  private static readonly List<IntVec3> ProjectileDestCells = new List<IntVec3>();
  private static readonly List<(Thing, int)> ThingsToTakeReloading = new List<(Thing, int)>();
  public int uniqueID = -1;
  public string parentKey;
  public string key;
  public string groupKey;
  [Unsaved(false)]
  public VehicleTurret reference;
  [TweakField]
  [LoadAlias("turretDef")]
  public VehicleTurretDef def;
  public AutoLoadConfig loadConfig;
  public TargetLock targeting = TargetLock.Thing | TargetLock.Pawn;
  [TweakField(SettingsType = UISettingsType.Checkbox)]
  public bool targetPersists;
  [TweakField(SettingsType = UISettingsType.Checkbox)]
  public bool autoTargeting = true;
  [TweakField(SettingsType = UISettingsType.Checkbox)]
  public bool manualTargeting = true;
  [TweakField(SettingsType = UISettingsType.SliderEnum)]
  public DeploymentType deployment;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector2 angleRestricted = Vector2.zero;
  public float defaultAngleRotated;
  public ComponentRequirement component;
  public string upgradeKey;
  public LocalTargetInfo targetInfo;
  protected float restrictedTheta;
  public ThingDef loadedAmmo;
  public ThingDef savedAmmoType;
  public int shellCount;
  protected bool autoTargetingActive;
  private int reloadTicks;
  private int burstTicks;
  protected int currentFireMode;
  public float currentHeatRate;
  protected bool triggeredCooldown;
  protected int ticksSinceLastShot;
  public bool queuedToFire;
  protected Rot4 parentRotCached;
  protected float parentAngleCached;
  protected int burstsTillWarmup;
  [Unsaved(false)]
  protected float rotationTargeted = float.NaN;
  [Unsaved(false)]
  protected int ticksRotating;
  [Unsaved(false)]
  public VehiclePawn vehicle;
  [Unsaved(false)]
  public VehicleDef vehicleDef;
  [Unsaved(false)]
  public VehicleTurret attachedTo;
  [Unsaved(false)]
  public List<VehicleTurret> childTurrets = new List<VehicleTurret>();
  [Unsaved(false)]
  protected List<VehicleTurret> groupTurrets;
  [Unsaved(false)]
  public TurretRestrictions restrictions;
  [Unsaved(false)]
  public Turret_RecoilTracker recoilTracker;
  [Unsaved(false)]
  public Turret_RecoilTracker[] recoilTrackers;
  public static Func<ThingDef, ThingDef, Def, Vector2, LocalTargetInfo, VehiclePawn, float, float, float, float, object> LaunchProjectileCE;
  public static Func<float, float, Thing, LocalTargetInfo, Vector3, bool, float, float, float, float, Vector2> ProjectileAngleCE;
  public static Func<string, Def> LookupAmmosetCE;
  public static Action<ThingDef, ThingDef, Def, VehicleTurret, float> NotifyShotFiredCE;
  public static Func<ThingDef, Def, float, Tuple<int, float>> LookupProjectileCountAndSpreadCE;
  private static readonly StringBuilder TooltipBuilder = new StringBuilder();
  [TweakField]
  public VehicleTurretRender renderProperties = new VehicleTurretRender();
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector2 aimPieOffset = Vector2.zero;
  [TweakField(SettingsType = UISettingsType.IntegerBox)]
  public int drawLayer = 1;
  public string gizmoLabel;
  [TweakField]
  [AnimationProperty(Name = "Transform")]
  private Transform transform = new Transform();
  [Unsaved(false)]
  private Vehicles.Rendering.PreRenderResults results;
  [Unsaved(false)]
  private List<Vehicles.Rendering.PreRenderResults> subGraphicResults = new List<Vehicles.Rendering.PreRenderResults>();
  [Unsaved(false)]
  private Vector3 rootDrawPosNorth;
  [Unsaved(false)]
  private Vector3 rootDrawPosEast;
  [Unsaved(false)]
  private Vector3 rootDrawPosSouth;
  [Unsaved(false)]
  private Vector3 rootDrawPosWest;
  [Unsaved(false)]
  private Vector3 rootDrawPosNorthEast;
  [Unsaved(false)]
  private Vector3 rootDrawPosSouthEast;
  [Unsaved(false)]
  private Vector3 rootDrawPosSouthWest;
  [Unsaved(false)]
  private Vector3 rootDrawPosNorthWest;
  [Unsaved(false)]
  public Texture2D currentFireIcon;
  [Unsaved(false)]
  private Texture2D gizmoIcon;
  [Unsaved(false)]
  private Texture2D mainMaskTex;
  [Unsaved(false)]
  private Texture2D cachedTexture;
  [Unsaved(false)]
  private Material cachedMaterial;
  [Unsaved(false)]
  private Graphic_Turret cachedGraphic;
  [Unsaved(false)]
  private GraphicDataRGB cachedGraphicData;
  [Unsaved(false)]
  private List<VehicleTurret.TurretDrawData> turretGraphics;
  [Unsaved(false)]
  private RotatingList<Texture2D> overheatIcons;
  [Unsaved(false)]
  private bool selfDirty = true;

  public VehicleTurret()
  {
  }

  public VehicleTurret(VehiclePawn vehicle)
  {
    this.vehicle = vehicle;
    this.vehicleDef = vehicle.VehicleDef;
  }

  public VehicleTurret(VehiclePawn vehicle, VehicleTurret reference)
  {
    this.vehicle = vehicle;
    this.vehicleDef = vehicle.VehicleDef;
    this.uniqueID = Find.UniqueIDsManager.GetNextThingID();
    this.def = reference.def;
    this.gizmoLabel = reference.gizmoLabel;
    this.key = reference.key;
    this.targetPersists = reference.def.turretType != TurretType.Static && reference.targetPersists;
    this.autoTargeting = reference.def.turretType != TurretType.Static && reference.autoTargeting;
    this.manualTargeting = reference.def.turretType != TurretType.Static && reference.manualTargeting;
    this.currentFireMode = 0;
    this.currentFireIcon = this.OverheatIcons.FirstOrDefault<Texture2D>();
    this.ticksSinceLastShot = 0;
    this.burstsTillWarmup = 0;
    this.TurretRotation = reference.defaultAngleRotated;
    this.InitRecoilTrackers();
  }

  string ITweakFields.Label => nameof (VehicleTurret);

  string ITweakFields.Category => TaggedString.op_Implicit(this.def.LabelCap);

  public bool TargetLocked { get; private set; }

  public int PrefireTickCount { get; private set; }

  public int CurrentTurretFiring { get; set; }

  public bool IsManned { get; [UsedImplicitly] protected set; }

  public PawnStatusOnTarget CachedPawnTargetStatus { get; set; }

  public bool IsTargetable
  {
    get
    {
      VehicleTurretDef def = this.def;
      return def != null && def.turretType == TurretType.Rotatable;
    }
  }

  public bool RotationAligned
  {
    get => Mathf.Approximately(this.transform.rotation, this.rotationTargeted);
  }

  public bool TurretRestricted
  {
    get
    {
      TurretRestrictions restrictions = this.restrictions;
      return restrictions != null && restrictions.Disabled;
    }
  }

  public virtual bool TurretDisabled
  {
    get
    {
      return this.TurretRestricted || !this.IsManned || !this.DeploymentSatisfied || this.ComponentDisabled;
    }
  }

  public bool ComponentDisabled
  {
    get
    {
      ComponentRequirement component = this.component;
      if (component != null && !component.MeetsRequirements)
        return true;
      VehicleTurret attachedTo = this.attachedTo;
      return attachedTo != null && attachedTo.ComponentDisabled;
    }
  }

  protected virtual bool TurretTargetValid
  {
    get
    {
      IntVec3 cell = ((LocalTargetInfo) ref this.targetInfo).Cell;
      return ((IntVec3) ref cell).IsValid && !this.TurretDisabled;
    }
  }

  public bool CanAutoTarget => this.autoTargeting || VehicleMod.settings.debug.debugShootAnyTurret;

  public int WarmupTicks => Mathf.CeilToInt(this.def.warmUpTimer * 60f);

  public bool OnCooldown => this.triggeredCooldown;

  public bool CanOverheat
  {
    get
    {
      if (!VehicleMod.settings.main.overheatMechanics)
        return false;
      TurretCooldownProperties cooldown = this.def.cooldown;
      return cooldown != null && (double) cooldown.heatPerShot > 0.0;
    }
  }

  public bool HasAmmo => this.def.ammunition == null || this.shellCount > 0;

  public bool ReadyToFire
  {
    get
    {
      if (!GenText.NullOrEmpty(this.groupKey))
        return GenCollection.Any<VehicleTurret>(this.GroupTurrets, (Predicate<VehicleTurret>) (t => t.burstTicks <= 0 && t.ReloadTicks <= 0 && !t.TurretDisabled));
      return this.burstTicks <= 0 && this.ReloadTicks <= 0 && !this.TurretDisabled;
    }
  }

  public bool FullAuto
  {
    get
    {
      return ((IntRange) ref this.CurrentFireMode.ticksBetweenBursts).TrueMin == this.CurrentFireMode.ticksBetweenShots;
    }
  }

  public int ReloadTicks => this.reloadTicks;

  public EventManager<VehicleTurretEventDef> EventRegistry { get; set; }

  public bool DeploymentSatisfied
  {
    get
    {
      switch (this.deployment)
      {
        case DeploymentType.None:
          return true;
        case DeploymentType.Deployed:
          CompVehicleTurrets compVehicleTurrets1 = this.vehicle.CompVehicleTurrets;
          return compVehicleTurrets1 != null && compVehicleTurrets1.Deployed && !compVehicleTurrets1.Deploying;
        case DeploymentType.Undeployed:
          CompVehicleTurrets compVehicleTurrets2 = this.vehicle.CompVehicleTurrets;
          return compVehicleTurrets2 != null && !compVehicleTurrets2.Deployed && !compVehicleTurrets2.Deploying;
        default:
          throw new NotImplementedException("DeploymentType");
      }
    }
  }

  public int MaxTicks
  {
    get
    {
      float num = this.def.reloadTimer * 60f;
      if (this.def.reloadTimerMultiplierPerCrewCount != null)
      {
        int count = this.vehicle.PawnsByHandlingType[HandlingType.Turret].Count;
        num *= this.def.reloadTimerMultiplierPerCrewCount.Evaluate((float) count);
      }
      return Mathf.CeilToInt(num);
    }
  }

  public ThingDef ProjectileDef => this.loadedAmmo?.projectileWhenLoaded ?? this.def.projectile;

  public List<VehicleTurret> GroupTurrets
  {
    get
    {
      if (this.groupTurrets == null)
      {
        if (GenText.NullOrEmpty(this.groupKey))
          this.groupTurrets = new List<VehicleTurret>(1)
          {
            this
          };
        else
          this.groupTurrets = this.vehicle.CompVehicleTurrets.Turrets.Where<VehicleTurret>((Func<VehicleTurret, bool>) (t => t.groupKey == this.groupKey)).ToList<VehicleTurret>();
      }
      return this.groupTurrets;
    }
  }

  public virtual int MaxShotsCurrentFireMode
  {
    get
    {
      if (!this.FullAuto)
        return ((IntRange) ref this.CurrentFireMode.shotsPerBurst).RandomInRange;
      return !this.CanOverheat ? ((IntRange) ref this.CurrentFireMode.shotsPerBurst).TrueMax * 3 : Mathf.CeilToInt(100f / this.def.cooldown.heatPerShot);
    }
  }

  public int TicksPerShot => this.CurrentFireMode.ticksBetweenShots;

  public float IconAlphaTicked
  {
    get
    {
      return this.ReloadTicks <= 0 ? 1f : (float) ((double) Mathf.PingPong((float) this.ReloadTicks, 25f) / 50.0 + 0.25);
    }
  }

  public Vector3 TurretLocation
  {
    get
    {
      return this.attachedTo != null ? Vector3.op_Addition(((Thing) this.vehicle).DrawPos, this.DrawPosition(this.vehicle.FullRotation)) : Vector3.op_Addition(((Thing) this.vehicle).DrawPos, this.TurretOffset(this.vehicle.FullRotation));
    }
  }

  public float TurretRotation
  {
    get
    {
      if (!this.IsTargetable && this.attachedTo == null)
        return this.defaultAngleRotated + this.vehicle.FullRotation.AsAngle;
      this.UpdateRotationLock();
      this.transform.rotation = this.transform.rotation.ClampAngle();
      return this.attachedTo != null ? this.transform.rotation + this.attachedTo.TurretRotation : this.transform.rotation;
    }
    set => this.transform.rotation = value.ClampAngle();
  }

  public float TurretRotationTargeted
  {
    get => this.rotationTargeted;
    set
    {
      if (Mathf.Approximately(this.rotationTargeted, value))
        return;
      this.rotationTargeted = value.ClampAngle();
    }
  }

  public FireMode CurrentFireMode
  {
    get
    {
      if (this.currentFireMode >= 0 && this.currentFireMode < this.def.fireModes.Count)
        return this.def.fireModes[this.currentFireMode];
      SmashLog.ErrorOnce($"Unable to retrieve fire mode at index {this.currentFireMode}. Outside of bounds for <field>fireModes</field> defined in <field>def</field>. Defaulting to first fireMode.", this.GetHashCode() ^ this.currentFireMode);
      return this.def.fireModes.FirstOrDefault<FireMode>();
    }
    set
    {
      this.currentFireMode = this.def.fireModes.IndexOf(value);
      this.ResetPrefireTimer();
    }
  }

  public bool AutoTarget
  {
    get => this.autoTargetingActive;
    set
    {
      if (!this.CanAutoTarget || value == this.autoTargetingActive)
        return;
      this.autoTargetingActive = value;
      this.UpdateScanEvent();
    }
  }

  public float MaxRange => (double) this.def.maxRange <= 0.0 ? 9999f : this.def.maxRange;

  public float MinRange => this.def.minRange;

  public void Init(VehicleTurret reference)
  {
    this.reference = reference;
    this.groupKey = reference.groupKey;
    this.parentKey = reference.parentKey;
    if (this.loadConfig == null)
      this.loadConfig = new AutoLoadConfig(this);
    this.renderProperties = new VehicleTurretRender(reference.renderProperties);
    if (reference.component != null)
      this.component = ComponentRequirement.CopyFrom(reference.component);
    this.aimPieOffset = reference.aimPieOffset;
    this.angleRestricted = reference.angleRestricted;
    this.restrictedTheta = (float) (int) Mathf.Abs(this.angleRestricted.x - (this.angleRestricted.y + 360f)).ClampAngle();
    this.defaultAngleRotated = reference.defaultAngleRotated;
    this.deployment = reference.deployment;
    this.drawLayer = reference.drawLayer;
    if (reference.def.restrictionType != (System.Type) null)
      this.SetTurretRestriction(reference.def.restrictionType);
    this.ResetAngle();
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      if (this.PropertyBlock != null)
        return;
      MaterialPropertyBlock materialPropertyBlock;
      this.PropertyBlock = materialPropertyBlock = new MaterialPropertyBlock();
    }));
    LongEventHandler.ExecuteWhenFinished(new Action(this.RecacheRootDrawPos));
    this.component?.RegisterEvents(this.vehicle);
    this.UpdateScanEvent();
  }

  public virtual void PostSpawnSetup(bool respawningAfterLoad)
  {
  }

  public void SetTurretRestriction(System.Type type)
  {
    if (!type.IsSubclassOf(typeof (TurretRestrictions)))
    {
      Log.Error("Trying to create TurretRestriction with non-matching type.");
    }
    else
    {
      this.restrictions = (TurretRestrictions) Activator.CreateInstance(type);
      this.restrictions.Init(this.vehicle, this);
    }
  }

  public void RemoveTurretRestriction() => this.restrictions = (TurretRestrictions) null;

  public bool IsDisabled(out string reason)
  {
    if (this.TurretRestricted)
    {
      reason = this.restrictions.DisableReason;
      return true;
    }
    if (!this.DeploymentSatisfied)
    {
      string str;
      switch (this.deployment)
      {
        case DeploymentType.None:
          str = string.Empty;
          break;
        case DeploymentType.Deployed:
          str = TaggedString.op_Implicit(Translator.Translate("VF_MustBeDeployed"));
          break;
        case DeploymentType.Undeployed:
          str = TaggedString.op_Implicit(Translator.Translate("VF_MustBeUndeployed"));
          break;
        default:
          throw new NotImplementedException("DeploymentType");
      }
      reason = str;
      return true;
    }
    if (this.ComponentDisabled)
    {
      for (VehicleTurret vehicleTurret = this; vehicleTurret != null; vehicleTurret = this.attachedTo)
      {
        ComponentRequirement component = vehicleTurret.component;
        if (component != null && !component.MeetsRequirements)
        {
          reason = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_TurretComponentDisabled", NamedArgument.op_Implicit(vehicleTurret.component.Label)));
          return true;
        }
      }
      throw new InvalidOperationException(nameof (IsDisabled));
    }
    reason = (string) null;
    return false;
  }

  public void OnFieldChanged() => this.RecacheRootDrawPos();

  public virtual void RecacheMannedStatus()
  {
    this.IsManned = true;
    if (VehicleMod.settings.debug.debugShootAnyTurret)
      return;
    foreach (VehicleRoleHandler handler in this.vehicle.handlers)
    {
      if ((handler.role.HandlingTypes & HandlingType.Turret) == HandlingType.Turret && (handler.role.TurretIds.Contains(this.key) || handler.role.TurretIds.Contains(this.groupKey)) && !handler.RoleFulfilled)
      {
        this.IsManned = false;
        break;
      }
    }
  }

  public bool GroupsWith(VehicleTurret turret)
  {
    return !GenText.NullOrEmpty(this.groupKey) && this.groupKey == turret.groupKey;
  }

  public static float TurretRotationFor(Rot8 rot, float currentRotation)
  {
    return currentRotation + rot.AsAngle;
  }

  public virtual bool ActivateTimer(bool ignoreTimer = false)
  {
    if (this.ReloadTicks > 0 && !ignoreTimer)
      return false;
    this.reloadTicks = this.MaxTicks;
    this.TargetLocked = false;
    this.StartTicking();
    return true;
  }

  public virtual void ActivateBurstTimer()
  {
    this.burstTicks = ((IntRange) ref this.CurrentFireMode.ticksBetweenBursts).RandomInRange;
    --this.burstsTillWarmup;
    if (this.burstsTillWarmup > 0)
      return;
    this.ResetPrefireTimer();
  }

  public void StartTicking() => this.vehicle.CompVehicleTurrets.QueueTicker(this);

  public void StopTicking() => this.vehicle.CompVehicleTurrets.DequeueTicker(this);

  [Profile]
  public virtual bool Tick()
  {
    bool flag1 = this.TurretCooldownTick();
    bool flag2 = this.TurretReloadTick();
    bool flag3 = this.TurretRotationTick();
    bool flag4 = this.TurretTargeterTick();
    bool flag5 = false;
    if (this.recoilTracker != null)
      flag5 = this.recoilTracker.RecoilTick();
    if (!GenList.NullOrEmpty<Turret_RecoilTracker>((IList<Turret_RecoilTracker>) this.recoilTrackers))
    {
      for (int index = 0; index < this.def.graphics.Count; ++index)
      {
        int num1 = flag5 ? 1 : 0;
        Turret_RecoilTracker recoilTracker = this.recoilTrackers[index];
        int num2 = recoilTracker != null ? (recoilTracker.RecoilTick() ? 1 : 0) : 0;
        flag5 = (num1 | num2) != 0;
      }
    }
    return flag1 | flag2 | flag3 | flag4 | flag5;
  }

  [Profile]
  protected virtual bool TurretCooldownTick()
  {
    if (!this.CanOverheat)
      return false;
    if ((double) this.currentHeatRate > 0.0)
      ++this.ticksSinceLastShot;
    if ((double) this.currentHeatRate > 100.0)
    {
      this.triggeredCooldown = true;
      this.currentHeatRate = 100f;
      this.EventRegistry[VehicleTurretEventDefOf.Cooldown].ExecuteEvents();
    }
    else if ((double) this.currentHeatRate <= 0.0)
    {
      this.currentHeatRate = 0.0f;
      this.triggeredCooldown = false;
      return false;
    }
    if (this.ticksSinceLastShot >= 60)
    {
      float dissipationRate = this.def.cooldown.dissipationRate;
      if (this.triggeredCooldown)
        dissipationRate *= this.def.cooldown.dissipationCapMultiplier;
      this.currentHeatRate -= dissipationRate;
    }
    return true;
  }

  protected virtual bool TurretReloadTick()
  {
    if (((Thing) this.vehicle).Spawned && !this.queuedToFire)
    {
      if (this.ReloadTicks > 0 && !this.OnCooldown)
      {
        --this.reloadTicks;
        return true;
      }
      if (this.burstTicks > 0)
      {
        --this.burstTicks;
        return true;
      }
    }
    return false;
  }

  [Profile]
  protected virtual void ScanForTarget()
  {
    if (!this.AutoTarget)
    {
      Log.ErrorOnce("Scanning for target but auto targeting is disabled.", this.GetHashCode());
      this.UpdateScanEvent();
    }
    else
    {
      LocalTargetInfo targetInfo;
      if (!((Thing) this.vehicle).Spawned || this.queuedToFire || this.TurretDisabled || ((LocalTargetInfo) ref this.targetInfo).IsValid || TurretTargeter.Turret == this || this.ReloadTicks > 0 || !this.HasAmmo || !this.TryGetTarget(out targetInfo, new TargetScanFlags?((TargetScanFlags) 256 /*0x0100*/)))
        return;
      this.AlignToAngleRestricted(this.TurretLocation.AngleToPoint(((LocalTargetInfo) ref targetInfo).Thing.DrawPos));
      this.SetTarget(targetInfo);
    }
  }

  [Profile]
  protected virtual bool TurretRotationTick()
  {
    if (this.ComponentDisabled)
    {
      this.ResetAngle();
      return false;
    }
    bool flag = false;
    if (this.TargetLocked)
    {
      this.AlignToTargetRestricted();
      flag = true;
    }
    if (this.RotationAligned)
    {
      this.ticksRotating = 0;
      return flag;
    }
    if (this.def.autoSnapTargeting)
    {
      this.TurretRotation = this.TurretRotationTargeted;
      return true;
    }
    float rotation = this.transform.rotation;
    float num1 = this.TurretRotationTargeted.ClampAngle();
    float num2 = Mathf.DeltaAngle(rotation, num1);
    if ((double) Mathf.Abs(num2) < (double) this.def.rotationSpeed + 0.10000000149011612)
    {
      this.TurretRotation = num1;
    }
    else
    {
      float num3 = Mathf.Sign(num2);
      float num4 = ((double) this.def.rotationDelta > 0.0 ? Ext_Math.SmoothStep(0.0f, 1f, (float) this.ticksRotating / (this.def.rotationDelta * 60f)) : 1f) * this.def.rotationSpeed * num3;
      this.TurretRotation = rotation + num4;
      ++this.ticksRotating;
    }
    return true;
  }

  [Profile]
  protected virtual bool TurretTargeterTick()
  {
    if (this.TurretTargetValid)
    {
      if (Mathf.Approximately(this.transform.rotation, this.TurretRotationTargeted) && !this.TargetLocked)
      {
        this.TargetLocked = true;
        this.ResetPrefireTimer();
      }
      else if (!this.TurretTargetValid)
      {
        this.SetTarget(LocalTargetInfo.Invalid);
        return TurretTargeter.Turret == this;
      }
      if (this.IsTargetable && !TargetingHelper.TargetMeetsRequirements(this, this.targetInfo, out IntVec3 _))
      {
        this.SetTarget(LocalTargetInfo.Invalid);
        this.TargetLocked = false;
        return TurretTargeter.Turret == this;
      }
      if (this.PrefireTickCount > 0)
      {
        this.TurretRotationTargeted = ((LocalTargetInfo) ref this.targetInfo).HasThing ? this.TurretLocation.AngleToPoint(((LocalTargetInfo) ref this.targetInfo).Thing.DrawPos) : IntVec3Utility.ToIntVec3(this.TurretLocation).AngleToCell(((LocalTargetInfo) ref this.targetInfo).Cell);
        if (this.attachedTo != null)
          this.TurretRotationTargeted -= this.attachedTo.TurretRotation;
        if (this.def.autoSnapTargeting)
          this.TurretRotation = this.TurretRotationTargeted;
        if (this.TargetLocked && this.ReadyToFire)
          --this.PrefireTickCount;
      }
      else if (this.ReadyToFire)
      {
        if (this.IsTargetable && this.RotationAligned && (((LocalTargetInfo) ref this.targetInfo).Pawn == null || !this.CheckTargetInvalid()))
          this.GroupTurrets.ForEach((Action<VehicleTurret>) (t => t.PushTurretToQueue()));
        else if (this.FullAuto && this.queuedToFire)
          this.GroupTurrets.ForEach((Action<VehicleTurret>) (t => t.PushTurretToQueue()));
      }
      return true;
    }
    return this.IsTargetable && TurretTargeter.Turret == this;
  }

  private void UpdateScanEvent()
  {
    this.vehicle.RemoveEvent<VehicleEventDef>(VehicleEventDefOf.ScanShort, new Action(this.ScanForTarget));
    if (!this.autoTargetingActive)
      return;
    string key = this.GetUniqueLoadID() + "::ScanForTarget";
    this.vehicle.AddEvent<VehicleEventDef>(VehicleEventDefOf.ScanShort, new Action(this.ScanForTarget), key);
  }

  public virtual CompVehicleTurrets.TurretData GenerateTurretData()
  {
    return new CompVehicleTurrets.TurretData()
    {
      shots = ((IntRange) ref this.CurrentFireMode.shotsPerBurst).RandomInRange,
      ticksTillShot = 0,
      turret = this
    };
  }

  public virtual void PushTurretToQueue()
  {
    this.ActivateBurstTimer();
    this.vehicle.CompVehicleTurrets.QueueTurret(this.GenerateTurretData());
  }

  public bool TryFindShootLineFromTo(
    IntVec3 root,
    LocalTargetInfo target,
    out ShootLine resultingLine)
  {
    IntVec3 goodDest;
    if (!TargetingHelper.TargetMeetsRequirements(this, target, out goodDest))
    {
      resultingLine = new ShootLine();
      return false;
    }
    resultingLine = new ShootLine(root, goodDest);
    return true;
  }

  public virtual void FireTurret()
  {
    if (!((Thing) this.vehicle).Spawned)
      return;
    float num1 = Vector3.Distance(this.TurretLocation, ((LocalTargetInfo) ref this.targetInfo).CenterVector3);
    LocalTargetInfo targetInfo = this.targetInfo;
    if (this.CurrentTurretFiring >= this.def.projectileShifting.Count)
      this.CurrentTurretFiring = 0;
    Vector3 vector3 = Vector3.op_Addition(this.TurretLocation, Vector3Utility.RotatedBy(new Vector3(!GenList.NullOrEmpty<float>((IList<float>) this.def.projectileShifting) ? this.def.projectileShifting[this.CurrentTurretFiring] : 0.0f, 1f, this.def.projectileOffset), this.TurretRotation));
    if (this.def.ammunition != null)
      this.ConsumeChamberedShot();
    ThingDef projectileDef = this.ProjectileDef;
    if (VehicleTurret.LaunchProjectileCE == null)
    {
      Projectile parent = (Projectile) GenSpawn.Spawn(projectileDef, ((Thing) this.vehicle).Position, ((Thing) this.vehicle).Map, (WipeMode) 0);
      ProjectileHitFlags projectileHitFlags = parent.HitFlags;
      Thing thing1 = (Thing) null;
      LocalTargetInfo localTargetInfo;
      if ((double) this.CurrentFireMode.forcedMissRadius > 0.0)
      {
        int num2 = (double) this.def.maxRange > 0.0 ? GenRadial.NumCellsInRadius(this.CurrentFireMode.forcedMissRadius * (num1 / this.def.maxRange)) : GenRadial.NumCellsInRadius(this.CurrentFireMode.forcedMissRadius);
        localTargetInfo = LocalTargetInfo.op_Implicit(IntVec3.op_Addition(((LocalTargetInfo) ref targetInfo).Cell, GenRadial.RadialPattern[Rand.Range(0, num2)]));
      }
      else
      {
        ShootLine resultingLine;
        if (!this.TryFindShootLineFromTo(IntVec3Utility.ToIntVec3(this.TurretLocation), this.targetInfo, out resultingLine))
          return;
        TurretShotReport turretShotReport = TurretShotReport.HitReportFor(this.vehicle, this, this.targetInfo);
        thing1 = turretShotReport.GetRandomCoverToMissInto();
        if (this.CurrentFireMode.canMiss && !Rand.Chance(turretShotReport.AimOnTargetChanceWithSize))
        {
          ((ShootLine) ref resultingLine).ChangeDestToMissWild(turretShotReport.AimOnTargetChance, projectileDef.projectile.flyOverhead, ((Thing) this.vehicle).Map);
          projectileHitFlags = (ProjectileHitFlags) 4;
          if (Rand.Chance(0.5f))
            projectileHitFlags = (ProjectileHitFlags) (projectileHitFlags | 2);
          localTargetInfo = LocalTargetInfo.op_Implicit(((ShootLine) ref resultingLine).Dest);
        }
        else
        {
          Thing thing2 = ((LocalTargetInfo) ref this.targetInfo).Thing;
          if (thing2 != null)
          {
            ThingDef def = thing2.def;
            if (def != null && def.CanBenefitFromCover && !Rand.Chance(turretShotReport.PassCoverChance))
            {
              projectileHitFlags = (ProjectileHitFlags) 6;
              localTargetInfo = LocalTargetInfo.op_Implicit(thing1);
              goto label_20;
            }
          }
          projectileHitFlags = (ProjectileHitFlags) 3;
          if (!((LocalTargetInfo) ref this.targetInfo).HasThing || ((LocalTargetInfo) ref this.targetInfo).Thing.def.Fillage == 2)
            projectileHitFlags = (ProjectileHitFlags) (projectileHitFlags | 4);
          localTargetInfo = ((LocalTargetInfo) ref this.targetInfo).HasThing ? LocalTargetInfo.op_Implicit(((LocalTargetInfo) ref this.targetInfo).Thing) : LocalTargetInfo.op_Implicit(((ShootLine) ref resultingLine).Dest);
        }
      }
label_20:
      if ((double) this.def.projectileSpeed > 0.0 || this.def.attachProjectileFlag != null)
      {
        CompTurretProjectileProperties comp = new CompTurretProjectileProperties((ThingWithComps) parent)
        {
          speed = (double) this.def.projectileSpeed > 0.0 ? this.def.projectileSpeed : ((Thing) parent).def.projectile.speed,
          hitflags = this.def.attachProjectileFlag
        };
        if (!((ThingWithComps) parent).TryAddComp<CompTurretProjectileProperties>(comp))
          Log.Error($"Failed to attach modified properties to {projectileDef}");
      }
      parent.Launch((Thing) this.vehicle, vector3, localTargetInfo, this.targetInfo, projectileHitFlags, false, (Thing) this.vehicle, thing1?.def);
    }
    else
      this.FireTurretCE(projectileDef, vector3);
    this.EventRegistry[VehicleTurretEventDefOf.ShotFired].ExecuteEvents();
    this.PostTurretFire();
    this.InitTurretMotes(vector3);
  }

  public virtual void PostTurretFire()
  {
    SoundDef shotSound = this.def.shotSound;
    if (shotSound != null)
      SoundStarter.PlayOneShot(shotSound, SoundInfo.op_Implicit(new TargetInfo(((Thing) this.vehicle).Position, ((Thing) this.vehicle).Map, false)));
    this.vehicle.DrawTracker.recoilTracker.Notify_TurretRecoil(this, Ext_Math.RotateAngle(this.TurretRotation, 180f));
    this.recoilTracker?.Notify_TurretRecoil(Ext_Math.RotateAngle(this.TurretRotation, 180f));
    if (!GenList.NullOrEmpty<Turret_RecoilTracker>((IList<Turret_RecoilTracker>) this.recoilTrackers))
    {
      foreach (Turret_RecoilTracker recoilTracker in this.recoilTrackers)
        recoilTracker?.Notify_TurretRecoil(Ext_Math.RotateAngle(this.TurretRotation, 180f));
    }
    this.ticksSinceLastShot = 0;
    if (!this.CanOverheat)
      return;
    this.currentHeatRate += this.def.cooldown.heatPerShot;
  }

  private void InitRecoilTrackers()
  {
    if (this.def.recoil != null)
      this.recoilTracker = new Turret_RecoilTracker(this.def.recoil);
    if (GenList.NullOrEmpty<VehicleTurretRenderData>((IList<VehicleTurretRenderData>) this.def.graphics))
      return;
    this.recoilTrackers = new Turret_RecoilTracker[this.def.graphics.Count];
    for (int index = 0; index < this.def.graphics.Count; ++index)
    {
      RecoilProperties recoil = this.def.graphics[index].recoil;
      if (recoil != null)
        this.recoilTrackers[index] = new Turret_RecoilTracker(recoil);
    }
  }

  public bool AngleBetween(Vector3 position)
  {
    if (Vector2.op_Equality(this.angleRestricted, Vector2.zero))
      return true;
    VehicleTurret attachedTo = this.attachedTo;
    double num1;
    if (attachedTo == null)
    {
      Rot4 rotation = ((Thing) this.vehicle).Rotation;
      num1 = (double) ((Rot4) ref rotation).AsAngle + (double) this.vehicle.Angle;
    }
    else
      num1 = (double) attachedTo.TurretRotation;
    float num2 = (float) num1;
    float num3 = (this.angleRestricted.x + num2).ClampAngle();
    float num4 = (this.angleRestricted.y + num2).ClampAngle();
    float num5 = Vector3Utility.AngleFlat(Vector3.op_Subtraction(position, this.TurretLocation));
    float num6 = (double) num4 - (double) num3 < 0.0 ? (float) ((double) num4 - (double) num3 + 360.0) : num4 - num3;
    return ((double) num5 - (double) num3 < 0.0 ? (double) num5 - (double) num3 + 360.0 : (double) num5 - (double) num3) < (double) num6;
  }

  public bool InRange(LocalTargetInfo target)
  {
    if ((double) this.MinRange == 0.0 && Mathf.Approximately(this.MaxRange, 9999f))
      return true;
    IntVec3 cell = ((LocalTargetInfo) ref target).Cell;
    Vector2 vector2;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2).\u002Ector((float) cell.x, (float) cell.z);
    Vector3 turretLocation = this.TurretLocation;
    float num = Vector2.Distance(new Vector2(turretLocation.x, turretLocation.z), vector2);
    return (double) num >= (double) this.MinRange && (double) num <= (double) this.MaxRange;
  }

  public void AlignToTargetRestricted()
  {
    this.TurretRotationTargeted = ((LocalTargetInfo) ref this.targetInfo).HasThing ? this.TurretLocation.AngleToPoint(((LocalTargetInfo) ref this.targetInfo).Thing.DrawPos) : IntVec3Utility.ToIntVec3(this.TurretLocation).AngleToCell(((LocalTargetInfo) ref this.targetInfo).Cell);
    if (this.attachedTo == null)
      return;
    this.TurretRotationTargeted -= this.attachedTo.TurretRotation;
  }

  public void AlignToAngleRestricted(float angle)
  {
    VehicleTurret attachedTo = this.attachedTo;
    float num = attachedTo != null ? attachedTo.TurretRotation : 0.0f;
    this.TurretRotationTargeted = angle - num;
  }

  public void ReloadIfEmpty()
  {
    if (this.shellCount > 0 && this.loadedAmmo != null || this.def.ammunition == null)
      return;
    ThingDef ammoDef = this.savedAmmoType ?? ((IEnumerable<Thing>) this.vehicle.inventory.innerContainer).Where<Thing>((Func<Thing, bool>) (thing => this.ContainsAmmoDefOrShell(thing.def))).Select<Thing, ThingDef>((Func<Thing, ThingDef>) (thing => thing.def)).Distinct<ThingDef>().FirstOrDefault<ThingDef>();
    if (ammoDef == null)
      return;
    this.Reload(ammoDef);
  }

  public bool ContainsAmmoDefOrShell(ThingDef ammoDef)
  {
    if (this.def.ammunition == null)
      return false;
    ThingDef thingDef = (ThingDef) null;
    if (ammoDef.projectileWhenLoaded != null)
      thingDef = ammoDef.projectileWhenLoaded;
    return this.def.ammunition.Allows(ammoDef) || this.def.ammunition.Allows(thingDef);
  }

  public void Reload() => this.Reload(this.loadedAmmo ?? this.savedAmmoType);

  public void Reload(ThingDef ammoDef)
  {
    this.Reload(ammoDef, ammoDef != this.loadedAmmo && ammoDef != this.savedAmmoType);
  }

  public virtual void Reload(ThingDef ammoDef, bool ignoreTimer)
  {
    if (!this.IsManned || this.ComponentDisabled || (ammoDef == this.savedAmmoType || ammoDef == null) && this.shellCount == this.def.magazineCapacity)
      return;
    if (this.def.ammunition == null)
    {
      this.shellCount = this.def.magazineCapacity;
    }
    else
    {
      if (ammoDef == null)
        ammoDef = this.savedAmmoType;
      if (ammoDef == null || this.loadedAmmo != null && this.shellCount >= this.def.magazineCapacity && this.shellCount > 0 || !this.ReloadInternal(ammoDef))
        return;
      this.ActivateTimer(ignoreTimer);
    }
  }

  public virtual bool AutoReload()
  {
    if (!this.IsManned || this.ComponentDisabled || this.def.ammunition == null)
      return false;
    ThingDef def = GetFirstAmmoType(this)?.def;
    if (def != null)
      return this.ReloadInternal(def);
    Debug.Warning("Failed to auto-reload " + this.def.label);
    return false;

    static Thing GetFirstAmmoType(VehicleTurret turret)
    {
      foreach (Thing firstAmmoType in turret.vehicle.inventory.innerContainer)
      {
        if (turret.def.ammunition.Allows(firstAmmoType) || turret.def.ammunition.Allows(firstAmmoType.def.projectileWhenLoaded))
          return firstAmmoType;
      }
      return (Thing) null;
    }
  }

  public void SetMagazineCount(int count)
  {
    this.shellCount = Mathf.Clamp(count, 0, this.def.magazineCapacity);
  }

  protected bool ReloadInternal([NotNull] ThingDef ammoDef)
  {
    try
    {
      if (!((ThingOwner) this.vehicle.inventory.innerContainer).Contains(ammoDef))
        return false;
      if (ammoDef != this.savedAmmoType)
        this.TryClearChamber();
      int num1 = this.def.magazineCapacity - this.shellCount;
      int num2 = Mathf.CeilToInt((float) num1 * this.def.chargePerAmmoCount);
      int num3 = 0;
      using (new ClearOnDispose<(Thing, int)>((ICollection<(Thing, int)>) VehicleTurret.ThingsToTakeReloading))
      {
        foreach (Thing thing in this.vehicle.inventory.innerContainer)
        {
          if (thing.def == ammoDef)
          {
            int num4 = thing.stackCount - thing.stackCount % Mathf.CeilToInt(this.def.chargePerAmmoCount);
            int num5 = Mathf.Min(num2, num4);
            VehicleTurret.ThingsToTakeReloading.Add((thing, num5));
            num2 -= num5;
            if (num2 <= 0)
              break;
          }
        }
        if ((double) VehicleTurret.ThingsToTakeReloading.Sum<(Thing, int)>((Func<(Thing, int), int>) (pair => pair.Item2)) < (double) this.def.chargePerAmmoCount)
          return false;
        using (new EventDisabler<VehicleEventDef>((IEventControl) this.vehicle.EventRegistry[VehicleEventDefOf.CargoRemoved]))
        {
          for (int index = VehicleTurret.ThingsToTakeReloading.Count - 1; index >= 0; --index)
          {
            if ((double) VehicleTurret.ThingsToTakeReloading.Sum<(Thing, int)>((Func<(Thing, int), int>) (pair => pair.Item2)) >= (double) this.def.chargePerAmmoCount)
            {
              (Thing thing, int count) = VehicleTurret.ThingsToTakeReloading[index];
              num3 += count;
              this.vehicle.TakeFromInventory(thing, count);
              VehicleTurret.ThingsToTakeReloading.RemoveAt(index);
            }
            else
              break;
          }
        }
        if (!Mathf.Approximately((float) num3 % this.def.chargePerAmmoCount, 0.0f))
          Log.Warning($"Taking more than necessary to reload {this}. CountRefilled={num3} CountNeeded={(ValueType) (float) ((double) num1 * (double) this.def.chargePerAmmoCount)}");
        this.loadedAmmo = ammoDef;
        this.shellCount = Mathf.CeilToInt((float) num3 / this.def.chargePerAmmoCount).Clamp(0, this.def.magazineCapacity);
        this.vehicle.EventRegistry[VehicleEventDefOf.CargoRemoved].ExecuteEvents();
        this.EventRegistry[VehicleTurretEventDefOf.Reload].ExecuteEvents();
        SoundDef reloadSound = this.def.reloadSound;
        if (reloadSound != null)
          SoundStarter.PlayOneShot(reloadSound, SoundInfo.op_Implicit(new TargetInfo(((Thing) this.vehicle).Position, ((Thing) this.vehicle).Map, false)));
      }
    }
    catch (Exception ex)
    {
      Log.Error($"Unable to reload Cannon: {this.uniqueID} on Pawn: {((Entity) this.vehicle).LabelShort}. Exception: {ex}");
      return false;
    }
    return true;
  }

  public void ConsumeChamberedShot()
  {
    --this.shellCount;
    if (this.shellCount > 0 || GenCollection.FirstOrFallback<Thing>((IEnumerable<Thing>) this.vehicle.inventory.innerContainer, (Func<Thing, bool>) (x => x.def == this.loadedAmmo), (Thing) null) != null)
      return;
    this.loadedAmmo = (ThingDef) null;
    this.shellCount = 0;
  }

  public virtual void TryClearChamber()
  {
    if (this.loadedAmmo == null || this.shellCount <= 0)
      return;
    using (new EventDisabler<VehicleEventDef>((IEventControl) this.vehicle.EventRegistry[VehicleEventDefOf.CargoAdded]))
    {
      Thing thing = ThingMaker.MakeThing(this.loadedAmmo, (ThingDef) null);
      thing.stackCount = Mathf.CeilToInt((float) this.shellCount * this.def.chargePerAmmoCount);
      if (this.vehicle.AddOrTransfer(thing) <= 0)
        return;
      this.loadedAmmo = (ThingDef) null;
      this.shellCount = 0;
      this.ActivateTimer(true);
    }
  }

  public void CycleFireMode()
  {
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, ((Thing) this.vehicle).Map);
    ++this.currentFireMode;
    if (this.currentFireMode < this.def.fireModes.Count)
      return;
    this.currentFireMode = 0;
  }

  public virtual void SwitchAutoTarget()
  {
    if (this.CanAutoTarget)
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, ((Thing) this.vehicle).Map);
      this.AutoTarget = !this.AutoTarget;
      this.SetTarget(LocalTargetInfo.Invalid);
      if (!this.AutoTarget)
        return;
      this.StartTicking();
    }
    else
      Messages.Message(TaggedString.op_Implicit(Translator.Translate("VF_AutoTargetingDisabled")), MessageTypeDefOf.RejectInput, true);
  }

  public virtual void SetTarget(LocalTargetInfo target)
  {
    this.targetInfo = target;
    this.TargetLocked = false;
    Pawn pawn = ((LocalTargetInfo) ref target).Pawn;
    this.CachedPawnTargetStatus = pawn == null ? PawnStatusOnTarget.None : (!pawn.Downed ? (!pawn.Dead ? PawnStatusOnTarget.Alive : PawnStatusOnTarget.Dead) : PawnStatusOnTarget.Down);
    if (!((LocalTargetInfo) ref this.targetInfo).IsValid)
      return;
    this.StartTicking();
  }

  public virtual bool CheckTargetInvalid(bool resetPrefireTimer = true)
  {
    if (((LocalTargetInfo) ref this.targetInfo).IsValid && (((LocalTargetInfo) ref this.targetInfo).HasThing || this.FullAuto))
    {
      if (((LocalTargetInfo) ref this.targetInfo).Pawn != null)
      {
        if (((LocalTargetInfo) ref this.targetInfo).Pawn.Dead && this.CachedPawnTargetStatus != PawnStatusOnTarget.Dead || ((LocalTargetInfo) ref this.targetInfo).Pawn.Downed && this.CachedPawnTargetStatus != PawnStatusOnTarget.Down)
        {
          this.SetTarget(LocalTargetInfo.Invalid);
          return true;
        }
      }
      else
      {
        Thing thing = ((LocalTargetInfo) ref this.targetInfo).Thing;
        if (thing != null && thing.HitPoints <= 0)
        {
          this.SetTarget(LocalTargetInfo.Invalid);
          return true;
        }
      }
    }
    return false;
  }

  public void ResetAngle() => this.TurretRotationTargeted = this.TurretRotation;

  public void FlagForAlignment()
  {
    this.TurretRotationTargeted = VehicleTurret.TurretRotationFor(this.vehicle.FullRotation, this.defaultAngleRotated.ClampAngle());
  }

  public virtual void ResetPrefireTimer()
  {
    this.PrefireTickCount = this.WarmupTicks;
    this.EventRegistry[VehicleTurretEventDefOf.Warmup].ExecuteEvents();
    this.burstsTillWarmup = this.CurrentFireMode.burstsTillWarmup;
  }

  public void UpdateRotationLock()
  {
    if (this.vehicle == null)
      return;
    if (!((LocalTargetInfo) ref this.targetInfo).IsValid && TurretTargeter.Turret != this && !this.vehicle.CompVehicleTurrets.Deploying)
    {
      float num1 = this.vehicle.Angle - this.parentAngleCached;
      if (this.attachedTo == null)
      {
        Transform transform = this.transform;
        double rotation1 = (double) transform.rotation;
        Rot4 rotation2 = ((Thing) this.vehicle).Rotation;
        double num2 = (double) (90 * (((Rot4) ref rotation2).AsInt - ((Rot4) ref this.parentRotCached).AsInt)) + (double) num1;
        transform.rotation = (float) (rotation1 + num2);
      }
      this.TurretRotationTargeted = this.transform.rotation;
      if (Rot4.op_Inequality(this.parentRotCached, ((Thing) this.vehicle).Rotation))
        this.parentRotCached = ((Thing) this.vehicle).Rotation;
    }
    this.parentAngleCached = this.vehicle.Angle;
  }

  public virtual string GetUniqueLoadID() => "VehicleTurretGroup_" + this.uniqueID.ToString();

  public override string ToString() => $"{this.def}_{this.GetUniqueLoadID()}";

  public virtual IEnumerable<string> ConfigErrors(VehicleDef vehicleDef)
  {
    if (this.def == null)
      yield return "<field>def</field> is a required field for <type>VehicleTurret</type>.".ConvertRichText();
    if (string.IsNullOrEmpty(this.key))
      yield return "<field>key</field> must be included for each <type>VehicleTurret</type>".ConvertRichText();
    if (vehicleDef.GetCompProperties<CompProperties_VehicleTurrets>().turrets.Select<VehicleTurret, string>((Func<VehicleTurret, string>) (x => x.key)).GroupBy<string, string>((Func<string, string>) (y => y)).Where<IGrouping<string, string>>((Func<IGrouping<string, string>, bool>) (y => y.Count<string>() > 1)).Select<IGrouping<string, string>, string>((Func<IGrouping<string, string>, string>) (z => z.Key)).NotNullAndAny<string>())
      yield return "Duplicate turret key " + this.key;
  }

  public virtual void OnDestroy()
  {
    this.AutoTarget = false;
    RGBMaterialPool.Release((IMaterialCacheTarget) this);
    if (GenList.NullOrEmpty<VehicleTurret.TurretDrawData>((IList<VehicleTurret.TurretDrawData>) this.turretGraphics))
      return;
    foreach (IMaterialCacheTarget turretGraphic in this.turretGraphics)
      RGBMaterialPool.Release(turretGraphic);
  }

  public virtual void PostPostLoadInit()
  {
    this.parentRotCached = ((Thing) this.vehicle).Rotation;
    this.parentAngleCached = this.vehicle.Angle;
    if (((LocalTargetInfo) ref this.targetInfo).IsValid)
      this.AlignToTargetRestricted();
    this.InitRecoilTrackers();
  }

  public virtual void ExposeData()
  {
    Scribe_Values.Look<bool>(ref this.autoTargetingActive, "autoTargetingActive", false, false);
    Scribe_Values.Look<int>(ref this.reloadTicks, "reloadTicks", 0, false);
    Scribe_Values.Look<int>(ref this.burstTicks, "burstTicks", 0, false);
    Scribe_Values.Look<int>(ref this.uniqueID, "uniqueID", -1, false);
    Scribe_Values.Look<string>(ref this.key, "key", (string) null, false);
    Scribe_Values.Look<string>(ref this.upgradeKey, "upgradeKey", (string) null, true);
    Scribe_Defs.Look<VehicleTurretDef>(ref this.def, "def");
    Scribe_Deep.Look<Transform>(ref this.transform, "transform", Array.Empty<object>());
    Scribe_Values.Look<bool>(ref this.targetPersists, "targetPersists", false, false);
    Scribe_Values.Look<bool>(ref this.autoTargeting, "autoTargeting", false, false);
    Scribe_Values.Look<bool>(ref this.manualTargeting, "manualTargeting", false, false);
    if (FeatureFlags.IsFeatureEnabled("BetterAutoLoadConfig"))
      Scribe_Deep.Look<AutoLoadConfig>(ref this.loadConfig, "loadConfig", new object[1]
      {
        (object) this
      });
    Scribe_Values.Look<bool>(ref this.queuedToFire, "queuedToFire", false, false);
    Scribe_Values.Look<int>(ref this.currentFireMode, "currentFireMode", 0, false);
    Scribe_Values.Look<float>(ref this.currentHeatRate, "currentHeatRate", 0.0f, false);
    Scribe_Values.Look<bool>(ref this.triggeredCooldown, "triggeredCooldown", false, false);
    Scribe_Values.Look<int>(ref this.ticksSinceLastShot, "ticksSinceLastShot", 0, false);
    Scribe_Values.Look<int>(ref this.burstsTillWarmup, "burstsTillWarmup", 0, false);
    Scribe_Values.Look<float>(ref this.restrictedTheta, "restrictedTheta", (float) (int) Mathf.Abs(this.angleRestricted.x - (this.angleRestricted.y + 360f)).ClampAngle(), false);
    Scribe_Defs.Look<ThingDef>(ref this.loadedAmmo, "loadedAmmo");
    Scribe_Defs.Look<ThingDef>(ref this.savedAmmoType, "savedAmmoType");
    Scribe_Values.Look<int>(ref this.shellCount, "shellCount", 0, false);
    Scribe_Values.Look<string>(ref this.gizmoLabel, "gizmoLabel", (string) null, false);
    Scribe_TargetInfo.Look(ref this.targetInfo, "targetInfo", LocalTargetInfo.Invalid);
  }

  internal void FireTurretCE(ThingDef projectileDef, Vector3 launchPos)
  {
    float num1 = (double) this.def.projectileSpeed > 0.0 ? this.def.projectileSpeed : projectileDef.projectile.speed;
    float num2 = Mathf.Atan2(this.CurrentFireMode.forcedMissRadius, this.MaxRange) * 57.29578f;
    float num3 = num2 * 0.84f;
    float num4 = num2 * 0.16f;
    float num5 = this.def.recoil != null ? this.def.recoil.distanceTotal : 0.0f;
    float num6 = 1f;
    CETurretDataDefModExtension modExtension = this.def.GetModExtension<CETurretDataDefModExtension>();
    if (modExtension != null)
    {
      if ((double) modExtension.speed > 0.0)
        num1 = modExtension.speed;
      if ((double) modExtension.sway >= 0.0)
        num3 = modExtension.sway;
      if ((double) modExtension.spread >= 0.0)
        num4 = modExtension.spread;
      num5 = modExtension.recoil;
      num6 = modExtension.shotHeight;
      if (modExtension._ammoSet == null && modExtension.ammoSet != null)
        modExtension._ammoSet = VehicleTurret.LookupAmmosetCE(modExtension.ammoSet);
    }
    int num7 = 1;
    if (VehicleTurret.LookupProjectileCountAndSpreadCE != null)
    {
      int num8;
      float num9;
      VehicleTurret.LookupProjectileCountAndSpreadCE(this.loadedAmmo, modExtension?._ammoSet, num4).Deconstruct<int, float>(out num8, out num9);
      num7 = num8;
      num4 = num9;
    }
    Vector3 vector3 = Vector3.op_Subtraction(launchPos, ((LocalTargetInfo) ref this.targetInfo).CenterVector3);
    float magnitude = ((Vector3) ref vector3).magnitude;
    Vector2 vector2 = VehicleTurret.ProjectileAngleCE(num1, magnitude, (Thing) this.vehicle, this.targetInfo, new Vector3(launchPos.x, num6, launchPos.z), projectileDef.projectile.flyOverhead, 1f, num3, 0.0f, num5 * (float) this.CurrentTurretFiring);
    float y = vector2.y;
    float num10 = -this.TurretRotation + vector2.x;
    do
    {
      double num11 = (double) Rand.Value * (double) num4;
      double num12 = (double) Rand.Value * Math.PI * 2.0;
      vector2.y = (float) (num11 * Math.Sin(num12));
      vector2.x = (float) (num11 * Math.Cos(num12));
      object obj = VehicleTurret.LaunchProjectileCE(projectileDef, this.loadedAmmo, modExtension?._ammoSet, new Vector2(launchPos.x, launchPos.z), this.targetInfo, this.vehicle, y + vector2.y * ((float) Math.PI / 180f), num10 + vector2.x, num6, num1);
    }
    while (--num7 > 0);
    Action<ThingDef, ThingDef, Def, VehicleTurret, float> notifyShotFiredCe = VehicleTurret.NotifyShotFiredCE;
    if (notifyShotFiredCe == null)
      return;
    notifyShotFiredCe(projectileDef, this.loadedAmmo, modExtension?._ammoSet, this, num5);
  }

  bool IParallelRenderer.IsDirty
  {
    get => this.selfDirty;
    set
    {
      this.selfDirty = value;
      VehicleTurret attachedTo = this.attachedTo;
      if (attachedTo == null)
        return;
      attachedTo.SetDirty();
    }
  }

  public bool GizmoHighlighted { get; set; }

  public MaterialPropertyBlock PropertyBlock { get; private set; }

  public bool ShouldDraw
  {
    get
    {
      if (this.component != null && !this.component.MeetsRequirements)
        return false;
      return this.attachedTo == null || this.attachedTo.ShouldDraw;
    }
  }

  public int MaterialCount => 1;

  public string Name => $"{this.def}_{this.key}_{((Thing) this.vehicle)?.ThingID ?? "Def"}";

  public bool NoGraphic => this.def.graphicData == null;

  public float DrawLayerOffset => (float) this.drawLayer * 0.00365853682f;

  public Transform Transform => this.transform;

  public PatternDef PatternDef
  {
    get
    {
      if (this.NoGraphic)
        return PatternDefOf.Default;
      if (this.vehicle == null)
        return GenCollection.TryGetValue<string, PatternData>((IReadOnlyDictionary<string, PatternData>) VehicleMod.settings.vehicles.defaultGraphics, ((Def) this.vehicleDef).defName, (PatternData) this.vehicleDef.graphicData)?.patternDef ?? PatternDefOf.Default;
      return this.def.matchParentColor ? this.vehicle?.PatternDef ?? PatternDefOf.Default : this.GraphicData.pattern;
    }
  }

  public Texture2D FireIcon
  {
    get
    {
      if (Find.TickManager.TicksGame % 15 == 0)
        this.currentFireIcon = this.OverheatIcons.Next;
      return this.currentFireIcon;
    }
  }

  protected RotatingList<Texture2D> OverheatIcons
  {
    get
    {
      if (GenList.NullOrEmpty<Texture2D>((IList<Texture2D>) this.overheatIcons))
        this.overheatIcons = TexData.FireIcons.ToRotatingList<Texture2D>();
      return this.overheatIcons;
    }
  }

  public virtual Material Material
  {
    get
    {
      if (!Object.op_Implicit((Object) this.cachedMaterial))
        this.ResolveGraphics(this.vehicle);
      return this.cachedMaterial;
    }
  }

  public virtual Texture2D Texture
  {
    get
    {
      if (GenText.NullOrEmpty(this.GraphicData.texPath))
        return (Texture2D) null;
      if (!Object.op_Implicit((Object) this.cachedTexture))
        this.cachedTexture = ContentFinder<Texture2D>.Get(this.GraphicData.texPath, true);
      return this.cachedTexture;
    }
  }

  public virtual Texture2D MainMaskTexture
  {
    get
    {
      if (GenText.NullOrEmpty(this.GraphicData.texPath))
        return (Texture2D) null;
      if (!Object.op_Implicit((Object) this.mainMaskTex))
        this.mainMaskTex = ContentFinder<Texture2D>.Get(this.GraphicData.texPath + Graphic_Turret.TurretMaskSuffix, true);
      return this.mainMaskTex;
    }
  }

  public virtual GraphicDataRGB GraphicData
  {
    get
    {
      if (this.cachedGraphicData == null)
        this.ResolveGraphics(this.vehicle);
      return this.cachedGraphicData;
    }
  }

  public virtual Graphic_Turret Graphic
  {
    get
    {
      if (this.cachedGraphic == null)
        this.ResolveGraphics(this.vehicle);
      return this.cachedGraphic;
    }
  }

  public virtual List<VehicleTurret.TurretDrawData> TurretGraphics => this.turretGraphics;

  public virtual Texture2D GizmoIcon
  {
    get
    {
      if (!Object.op_Implicit((Object) this.gizmoIcon))
      {
        if (!string.IsNullOrEmpty(this.def.gizmoIconTexPath))
          this.gizmoIcon = ContentFinder<Texture2D>.Get(this.def.gizmoIconTexPath, true);
        else if (this.NoGraphic)
        {
          this.gizmoIcon = BaseContent.BadTex;
        }
        else
        {
          this.gizmoIcon = this.Texture;
          if (!Object.op_Implicit((Object) this.gizmoIcon))
            this.gizmoIcon = BaseContent.BadTex;
        }
      }
      return this.gizmoIcon;
    }
  }

  public virtual IEnumerable<VehicleTurret.SubGizmo> SubGizmos
  {
    get
    {
      VehicleTurret turret = this;
      if (turret.def.magazineCapacity > 0)
      {
        if (turret.def.ammunition != null)
          yield return VehicleTurret.SubGizmo_RemoveAmmo(turret);
        yield return VehicleTurret.SubGizmo_ReloadFromInventory(turret);
      }
      yield return VehicleTurret.SubGizmo_FireMode(turret);
      if (turret.autoTargeting)
        yield return VehicleTurret.SubGizmo_AutoTarget(turret);
    }
  }

  public virtual void DynamicDrawPhaseAt(
    DrawPhase phase,
    in TransformData transformData,
    bool forceDraw = false)
  {
    if (this.NoGraphic)
      return;
    switch ((int) phase)
    {
      case 0:
        for (int index = 0; index < 4; ++index)
          ((Verse.Graphic) this.Graphic).MeshAt(new Rot4(index));
        if (!GenList.NullOrEmpty<VehicleTurret.TurretDrawData>((IList<VehicleTurret.TurretDrawData>) this.TurretGraphics))
        {
          foreach (VehicleTurret.TurretDrawData turretGraphic in this.TurretGraphics)
          {
            for (int index = 0; index < 4; ++index)
              ((Verse.Graphic) turretGraphic.graphic).MeshAt(new Rot4(index));
          }
        }
        if (GenList.NullOrEmpty<VehicleTurret>((IList<VehicleTurret>) this.childTurrets))
          break;
        using (List<VehicleTurret>.Enumerator enumerator = this.childTurrets.GetEnumerator())
        {
          while (enumerator.MoveNext())
            enumerator.Current.DynamicDrawPhaseAt(phase, in transformData, forceDraw);
          break;
        }
      case 1:
        this.ParallelPreRenderResultsRecursive(ref transformData, this.TurretRotation, 0.0f, forceDraw);
        break;
      case 2:
        if (!this.results.valid)
        {
          float rotation = this.defaultAngleRotated + transformData.orientation.AsAngle;
          this.ParallelPreRenderResultsRecursive(ref transformData, rotation, 0.0f, forceDraw);
        }
        this.Draw();
        this.results = new Vehicles.Rendering.PreRenderResults();
        this.subGraphicResults.Clear();
        break;
      default:
        throw new NotImplementedException("DrawPhase");
    }
  }

  private void ParallelPreRenderResultsRecursive(
    [RequiresLocation, In] ref TransformData transformData,
    float rotation,
    float parentRotation,
    bool forceDraw = false)
  {
    this.results = this.ParallelPreRenderResults(ref transformData, rotation, parentRotation, forceDraw);
    this.AddSubGraphicParallelPreRenderResults(ref transformData, this.subGraphicResults, rotation, parentRotation);
    if (GenList.NullOrEmpty<VehicleTurret>((IList<VehicleTurret>) this.childTurrets))
      return;
    foreach (VehicleTurret childTurret in this.childTurrets)
      childTurret.ParallelPreRenderResultsRecursive(ref transformData, childTurret.TurretRotation, rotation, forceDraw);
  }

  protected virtual Vehicles.Rendering.PreRenderResults ParallelPreRenderResults(
    [RequiresLocation, In] ref TransformData transformData,
    float rotation,
    float parentRotation,
    bool forceDraw = false)
  {
    if (this.NoGraphic || !this.ShouldDraw && !forceDraw)
      return new Vehicles.Rendering.PreRenderResults()
      {
        valid = true,
        draw = false
      };
    Vehicles.Rendering.PreRenderResults preRenderResults = new Vehicles.Rendering.PreRenderResults()
    {
      valid = true,
      draw = true
    };
    float num = transformData.rotation + rotation;
    float rotation1 = transformData.rotation + parentRotation;
    Vector3 vector3 = Vector3.op_Addition(transformData.position, this.DrawPosition(transformData.orientation, rotation1));
    VehiclePawn vehicle1 = this.vehicle;
    if (vehicle1 != null && ((Thing) vehicle1).Spawned)
    {
      Turret_RecoilTracker recoilTracker = this.recoilTracker;
      if (recoilTracker != null && (double) recoilTracker.Recoil > 0.0)
        vector3 = Vector3.op_Addition(vector3, Vector3.zero.PointFromAngle(this.recoilTracker.Recoil, this.recoilTracker.Angle));
    }
    preRenderResults.position = vector3;
    VehiclePawn vehicle2 = this.vehicle;
    if (vehicle2 != null && ((Thing) vehicle2).Spawned)
    {
      AltitudeLayer? altLayerSpawned = this.def.graphicData.altLayerSpawned;
      if (altLayerSpawned.HasValue)
      {
        AltitudeLayer valueOrDefault = altLayerSpawned.GetValueOrDefault();
        preRenderResults.position.y = Altitudes.AltitudeFor(valueOrDefault);
        preRenderResults.position.y += this.Graphic.DrawOffset(Rot4.North).y;
      }
    }
    preRenderResults.quaternion = GenMath.ToQuat(num);
    preRenderResults.mesh = ((Verse.Graphic) this.Graphic).MeshAt((Rot4) transformData.orientation);
    preRenderResults.material = this.Material;
    return preRenderResults;
  }

  protected virtual void AddSubGraphicParallelPreRenderResults(
    [RequiresLocation, In] ref TransformData transformData,
    List<Vehicles.Rendering.PreRenderResults> outList,
    float rotation,
    float parentRotation)
  {
    Vehicles.Rendering.PreRenderResults results = this.results;
    if (results.valid && !results.draw || GenList.NullOrEmpty<VehicleTurret.TurretDrawData>((IList<VehicleTurret.TurretDrawData>) this.TurretGraphics))
      return;
    for (int index = 0; index < this.TurretGraphics.Count; ++index)
    {
      Vehicles.Rendering.PreRenderResults preRenderResults = new Vehicles.Rendering.PreRenderResults()
      {
        valid = true,
        draw = true
      };
      VehicleTurret.TurretDrawData turretGraphic = this.TurretGraphics[index];
      Turret_RecoilTracker recoilTracker1 = this.recoilTrackers[index];
      float parentRotation1 = transformData.rotation + rotation;
      float rotation1 = transformData.rotation + parentRotation;
      Vector3 vector3_1 = Vector3.op_Addition(transformData.position, turretGraphic.DrawOffset(transformData.orientation, parentRotation1, rotation1));
      Vector3 vector3_2 = Vector3.zero;
      Vector3 vector3_3 = Vector3.zero;
      if (recoilTracker1 != null && (double) recoilTracker1.Recoil > 0.0)
        vector3_2 = Vector3.zero.PointFromAngle(recoilTracker1.Recoil, recoilTracker1.Angle);
      Turret_RecoilTracker recoilTracker2 = this.attachedTo?.recoilTracker;
      if (recoilTracker2 != null && (double) recoilTracker2.Recoil > 0.0)
        vector3_3 = Vector3.zero.PointFromAngle(this.attachedTo.recoilTracker.Recoil, this.attachedTo.recoilTracker.Angle);
      preRenderResults.position = Vector3.op_Addition(Vector3.op_Addition(vector3_1, vector3_2), vector3_3);
      VehiclePawn vehicle = this.vehicle;
      if (vehicle != null && ((Thing) vehicle).Spawned)
      {
        AltitudeLayer? altLayerSpawned = turretGraphic.graphicData.altLayerSpawned;
        if (altLayerSpawned.HasValue)
        {
          AltitudeLayer valueOrDefault = altLayerSpawned.GetValueOrDefault();
          preRenderResults.position.y = Altitudes.AltitudeFor(valueOrDefault);
          preRenderResults.position.y += this.Graphic.DrawOffset(Rot4.North).y;
        }
      }
      if (this.vehicle.Transform != null)
      {
        Transform transform = this.vehicle.Transform;
        ref Vector3 local = ref preRenderResults.position;
        local = Vector3.op_Addition(local, transform.position);
      }
      preRenderResults.quaternion = GenMath.ToQuat(parentRotation1);
      preRenderResults.mesh = ((Verse.Graphic) turretGraphic.graphic).MeshAt((Rot4) transformData.orientation);
      preRenderResults.material = turretGraphic.graphic.MatAt(Rot4.North, (Thing) null);
      outList.Add(preRenderResults);
    }
  }

  protected virtual void Draw()
  {
    if (!this.results.draw)
      return;
    Graphics.DrawMesh(this.results.mesh, this.results.position, this.results.quaternion, this.results.material, 0);
    if (this.subGraphicResults != null)
    {
      foreach (Vehicles.Rendering.PreRenderResults subGraphicResult in this.subGraphicResults)
        Graphics.DrawMesh(subGraphicResult.mesh, subGraphicResult.position, subGraphicResult.quaternion, subGraphicResult.material, 0);
    }
    if (!GenList.NullOrEmpty<VehicleTurret>((IList<VehicleTurret>) this.childTurrets))
    {
      foreach (VehicleTurret childTurret in this.childTurrets)
        childTurret.Draw();
    }
    if (!((Thing) this.vehicle).Spawned)
      return;
    this.DrawTargeter();
    this.DrawAimPie();
  }

  public Vector3 DrawPosition(Rot8 rot)
  {
    Rot8 rot1 = rot;
    if (this.attachedTo != null)
      rot1 = Rot8.North;
    Graphic_Turret graphic = this.Graphic;
    Vector3 vector3_1 = graphic != null ? graphic.DrawOffset((Rot4) rot1) : Vector3.zero;
    Vector2 vector2 = this.renderProperties.OffsetFor(rot1);
    Vector2 coord;
    // ISSUE: explicit constructor call
    ((Vector2) ref coord).\u002Ector(vector3_1.x + vector2.x, vector3_1.z + vector2.y);
    float theta = InheritedRotation(this);
    coord = coord.RotatePointClockwise(theta);
    if (this.attachedTo != null)
    {
      Vector3 vector3_2 = this.attachedTo.DrawPosition(rot);
      coord.x += vector3_2.x;
      coord.y += vector3_2.z;
    }
    return new Vector3(coord.x, vector3_1.y + this.DrawLayerOffset, coord.y);

    static float InheritedRotation(VehicleTurret turret)
    {
      VehiclePawn vehicle = turret.vehicle;
      // ISSUE: explicit non-virtual call
      float num = vehicle != null ? __nonvirtual (vehicle.Transform).rotation : 0.0f;
      for (VehicleTurret attachedTo = turret.attachedTo; attachedTo != null; attachedTo = attachedTo.attachedTo)
        num += attachedTo.transform.rotation;
      return num;
    }
  }

  private Vector3 DrawPosition(Rot8 rot, float rotation)
  {
    Rot8 rot1 = rot;
    if (this.attachedTo != null)
      rot1 = Rot8.North;
    Graphic_Turret graphic = this.Graphic;
    Vector3 vector3_1 = graphic != null ? graphic.DrawOffset((Rot4) rot1) : Vector3.zero;
    Vector2 vector2 = this.renderProperties.OffsetFor(rot1);
    Vector2 coord;
    // ISSUE: explicit constructor call
    ((Vector2) ref coord).\u002Ector(vector3_1.x + vector2.x, vector3_1.z + vector2.y);
    coord = coord.RotatePointClockwise(rotation);
    if (this.attachedTo != null)
    {
      Vector3 vector3_2 = this.attachedTo.DrawPosition(rot);
      coord.x += vector3_2.x;
      coord.y += vector3_2.z;
    }
    return new Vector3(coord.x, vector3_1.y + this.DrawLayerOffset, coord.y);
  }

  public Rect ScaleUIRectFor(VehicleDef vehicleDef, Rect rect, Rot8 rot, float iconScale = 1f)
  {
    GraphicDataRGB graphicData = this.def.graphicData;
    Vector2 vector2_1 = vehicleDef.ScaleDrawRatio((Verse.GraphicData) graphicData, (Rot4) rot, ((Rect) ref rect).size, iconScale);
    Vector2 vector2_2;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_2).\u002Ector(graphicData.drawSize.x, graphicData.drawSize.y);
    Vector2 vector2_3;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_3).\u002Ector(vector2_1.x / vector2_2.x, vector2_1.y / vector2_2.y);
    Vector3 vector3 = graphicData.DrawOffsetForRot((Rot4) rot);
    Vector2 vector2_4;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_4).\u002Ector(vector3.x * vector2_3.x, -vector3.z * vector2_3.y);
    Vector2 vector2_5 = this.renderProperties.OffsetFor(rot);
    Vector2 vector2_6;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_6).\u002Ector(vector2_5.x * vector2_3.x, -vector2_5.y * vector2_3.y);
    Vector2 vector2_7 = Vector2.op_Addition(Vector2.op_Addition(((Rect) ref rect).center, vector2_4), vector2_6);
    if (this.attachedTo != null)
    {
      Rect rect1 = this.attachedTo.ScaleUIRectFor(vehicleDef, rect, rot, iconScale);
      vector2_7 = Vector2.op_Addition(vector2_7, Vector2.op_Subtraction(((Rect) ref rect1).center, ((Rect) ref rect).center));
    }
    return new Rect(Vector2.op_Subtraction(vector2_7, Vector2.op_Multiply(vector2_1, 0.5f)), vector2_1);
  }

  (int width, int height) IBlitTarget.TextureSize(in BlitRequest request)
  {
    return !Object.op_Implicit((Object) this.Texture) ? (0, 0) : (((UnityEngine.Texture) this.Texture).width, ((UnityEngine.Texture) this.Texture).height);
  }

  IEnumerable<SmashTools.Rendering.RenderData> IBlitTarget.GetRenderData(
    Rect rect,
    BlitRequest request)
  {
    VehicleTurret vehicleTurret = this;
    if (!vehicleTurret.NoGraphic)
    {
      VehicleDef vehicleDef = vehicleTurret.vehicleDef ?? request.vehicleDef;
      Rect turretRect = request.iconFrame ? rect : VehicleGraphics.TurretRect(rect, vehicleDef, vehicleTurret, request.rot);
      yield return GetRenderDataFor(vehicleTurret, (IMaterialCacheTarget) vehicleTurret, turretRect, request, vehicleTurret.Graphic);
      if (!GenList.NullOrEmpty<VehicleTurret.TurretDrawData>((IList<VehicleTurret.TurretDrawData>) vehicleTurret.TurretGraphics))
      {
        foreach (VehicleTurret.TurretDrawData turretGraphic in vehicleTurret.TurretGraphics)
          yield return GetRenderDataFor(vehicleTurret, (IMaterialCacheTarget) turretGraphic, turretRect, request, turretGraphic.graphic);
      }
    }

    static SmashTools.Rendering.RenderData GetRenderDataFor(
      VehicleTurret turret,
      IMaterialCacheTarget target,
      Rect turretRect,
      BlitRequest request,
      Graphic_Turret graphic)
    {
      bool flag = graphic.Shader.SupportsRGBMaskTex();
      Material material = flag ? graphic.MatAtFull(Rot8.North) : (Material) null;
      if (graphic.Shader.SupportsRGBMaskTex())
        material = RGBMaterialPool.GetUi(target, (Rot4) request.rot);
      if (flag && turret.def.matchParentColor)
        RGBMaterialPool.SetProperties(target, request.patternData, new Func<Rot8, Texture2D>(((Graphic_Rgb) graphic).TexAt), new Func<Rot8, Texture2D>(((Graphic_Rgb) graphic).MaskAt));
      float asAngle = request.rot.AsAngle;
      if (!request.iconFrame)
        asAngle += turret.defaultAngleRotated;
      return new SmashTools.Rendering.RenderData(turretRect, (UnityEngine.Texture) graphic.TexAt(Rot8.North), material, target.PropertyBlock, graphic.DrawOffset(Rot4.North).y + turret.DrawLayerOffset, asAngle);
    }
  }

  protected virtual void DrawTargeter()
  {
    if (!this.GizmoHighlighted && TurretTargeter.Turret != this)
      return;
    switch (this.def.turretType)
    {
      case TurretType.Rotatable:
        if (Mathf.Approximately(this.restrictedTheta, 0.0f))
        {
          if ((double) this.MaxRange < 9999.0)
            GenDraw.DrawCircleOutline(this.results.position, this.MaxRange);
          if ((double) this.MinRange <= 0.0)
            break;
          GenDraw.DrawCircleOutline(this.results.position, this.MaxRange);
          break;
        }
        Vector3 position = this.results.position;
        Vector2 angleRestricted = this.angleRestricted;
        double minRange = (double) this.MinRange;
        double maxRange = (double) this.MaxRange;
        double restrictedTheta = (double) this.restrictedTheta;
        VehicleTurret attachedTo = this.attachedTo;
        double rotation = attachedTo != null ? (double) attachedTo.TurretRotation : (double) this.vehicle.FullRotation.AsAngle;
        VehicleTurret.DrawAngleLines(position, angleRestricted, (float) minRange, (float) maxRange, (float) restrictedTheta, (float) rotation);
        break;
      case TurretType.Static:
        if (!GenText.NullOrEmpty(this.groupKey))
        {
          using (List<VehicleTurret>.Enumerator enumerator = this.GroupTurrets.GetEnumerator())
          {
            while (enumerator.MoveNext())
            {
              VehicleTurret current = enumerator.Current;
              Vector3 vector3 = current.TurretLocation.PointFromAngle(current.MaxRange, current.TurretRotation);
              float num = Vector3.Distance(current.TurretLocation, vector3);
              GenDraw.DrawRadiusRing(IntVec3Utility.ToIntVec3(vector3), current.CurrentFireMode.forcedMissRadius * (num / current.def.maxRange));
            }
            break;
          }
        }
        Vector3 vector3_1 = this.TurretLocation.PointFromAngle(this.MaxRange, this.TurretRotation);
        float num1 = Vector3.Distance(this.TurretLocation, vector3_1);
        GenDraw.DrawRadiusRing(IntVec3Utility.ToIntVec3(vector3_1), this.CurrentFireMode.forcedMissRadius * (num1 / this.def.maxRange));
        break;
      default:
        throw new NotImplementedException("turretType");
    }
  }

  public static void DrawAngleLines(
    Vector3 position,
    Vector2 restrictedAngle,
    float minRange,
    float maxRange,
    float theta,
    float rotation = 0.0f)
  {
    Vector3 vector3_1 = position.PointFromAngle(minRange, restrictedAngle.x + rotation);
    Vector3 vector3_2 = position.PointFromAngle(minRange, restrictedAngle.y + rotation);
    Vector3 vector3_3 = position.PointFromAngle(maxRange, restrictedAngle.x + rotation);
    Vector3 vector3_4 = position.PointFromAngle(maxRange, restrictedAngle.y + rotation);
    GenDraw.DrawLineBetween(vector3_1, vector3_3);
    GenDraw.DrawLineBetween(vector3_2, vector3_4);
    if ((double) minRange > 0.0)
    {
      GenDraw.DrawLineBetween(position, vector3_1, (SimpleColor) 1, 0.2f);
      GenDraw.DrawLineBetween(position, vector3_2, (SimpleColor) 1, 0.2f);
    }
    float x = restrictedAngle.x;
    Vector3 vector3_5 = vector3_1;
    Vector3 vector3_6 = vector3_3;
    for (int index = 0; (double) index < (double) theta + 1.0; ++index)
    {
      Vector3 vector3_7 = position.PointFromAngle(maxRange, x + (float) index + rotation);
      GenDraw.DrawLineBetween(vector3_6, vector3_7);
      vector3_6 = vector3_7;
      if ((double) minRange > 0.0)
      {
        Vector3 vector3_8 = position.PointFromAngle(minRange, x + (float) index + rotation);
        GenDraw.DrawLineBetween(vector3_5, vector3_8, (SimpleColor) 1, 0.2f);
        vector3_5 = vector3_8;
      }
    }
  }

  protected virtual void DrawAimPie()
  {
    if (!this.TargetLocked || !this.ReadyToFire || Find.Selector.SingleSelectedThing != this.vehicle)
      return;
    double num1;
    if (((LocalTargetInfo) ref this.targetInfo).Thing == null)
    {
      IntVec3 intVec3 = IntVec3.op_Subtraction(((LocalTargetInfo) ref this.targetInfo).Cell, IntVec3Utility.ToIntVec3(this.TurretLocation));
      num1 = (double) ((IntVec3) ref intVec3).AngleFlat;
    }
    else
      num1 = (double) Vector3Utility.AngleFlat(Vector3.op_Subtraction(((LocalTargetInfo) ref this.targetInfo).Thing.DrawPos, this.TurretLocation));
    float num2 = (float) num1;
    GenDraw.DrawAimPieRaw(Vector3.op_Addition(this.TurretLocation, Vector3Utility.RotatedBy(new Vector3(this.aimPieOffset.x, 0.03658537f, this.aimPieOffset.y), this.TurretRotation)), num2, (int) ((double) this.PrefireTickCount * 0.5));
  }

  public virtual void ResolveGraphics(VehiclePawn vehicle, bool forceRegen = false)
  {
    this.ResolveGraphics(vehicle.patternData, forceRegen);
  }

  public virtual void ResolveGraphics(VehicleDef vehicleDef, bool forceRegen = false)
  {
    this.ResolveGraphics(GenCollection.TryGetValue<string, PatternData>((IReadOnlyDictionary<string, PatternData>) VehicleMod.settings.vehicles.defaultGraphics, ((Def) vehicleDef).defName, (PatternData) vehicleDef.graphicData), forceRegen);
  }

  public virtual void ResolveGraphics(PatternData patternData, bool forceRegen = false)
  {
    if (this.NoGraphic)
      return;
    if (this.cachedGraphicData == null | forceRegen)
    {
      this.cachedGraphic = VehicleTurret.GenerateGraphicData((IMaterialCacheTarget) this, this, this.def.graphicData, patternData, ref this.cachedGraphicData);
      this.cachedMaterial = (Material) null;
      if (!GenList.NullOrEmpty<VehicleTurretRenderData>((IList<VehicleTurretRenderData>) this.def.graphics))
        this.SetLayerGraphics(patternData);
      this.SetDirty();
    }
    if (!(!Object.op_Implicit((Object) this.cachedMaterial) | forceRegen))
      return;
    this.cachedMaterial = this.cachedGraphic?.MatAt((Rot4) Rot8.North, (Thing) this.vehicle);
  }

  private void SetLayerGraphics(PatternData patternData)
  {
    if (GenList.NullOrEmpty<VehicleTurret.TurretDrawData>((IList<VehicleTurret.TurretDrawData>) this.turretGraphics))
    {
      if (this.turretGraphics == null)
        this.turretGraphics = new List<VehicleTurret.TurretDrawData>();
      foreach (VehicleTurretRenderData graphic in this.def.graphics)
        this.turretGraphics.Add(new VehicleTurret.TurretDrawData(this, graphic));
    }
    for (int index = 0; index < this.def.graphics.Count; ++index)
    {
      VehicleTurretRenderData graphic = this.def.graphics[index];
      this.TurretGraphics[index].Set(graphic.graphicData, patternData);
    }
  }

  private static Graphic_Turret GenerateGraphicData(
    IMaterialCacheTarget cacheTarget,
    VehicleTurret turret,
    GraphicDataRGB copyGraphicData,
    PatternData patternData,
    ref GraphicDataRGB cachedGraphicData)
  {
    cachedGraphicData = new GraphicDataRGB();
    cachedGraphicData.CopyFrom((GraphicDataLayered) copyGraphicData);
    if (ShaderUtility.SupportsMaskTex(cachedGraphicData.shaderType.Shader) || cachedGraphicData.shaderType.Shader.SupportsRGBMaskTex())
    {
      if (turret.def.matchParentColor)
        cachedGraphicData.CopyDrawData((GraphicDataRGB) patternData);
      else
        cachedGraphicData.CopyDrawData(copyGraphicData);
    }
    Graphic_Turret graphic;
    if (cachedGraphicData.shaderType != null && cachedGraphicData.shaderType.Shader.SupportsRGBMaskTex())
    {
      RGBMaterialPool.CacheMaterialsFor(cacheTarget, patternData.patternDef);
      cachedGraphicData.Init(cacheTarget);
      graphic = (Graphic_Turret) cachedGraphicData.Graphic;
      RGBMaterialPool.SetProperties(cacheTarget, (PatternData) cachedGraphicData, new Func<Rot8, Texture2D>(((Graphic_Rgb) graphic).TexAt), new Func<Rot8, Texture2D>(((Graphic_Rgb) graphic).MaskAt));
    }
    else
      graphic = (Graphic_Turret) ((Verse.GraphicData) cachedGraphicData).Graphic;
    return graphic;
  }

  public virtual void InitTurretMotes(Vector3 loc)
  {
    if (GenList.NullOrEmpty<AnimationProperties>((IList<AnimationProperties>) this.def.motes))
      return;
    foreach (AnimationProperties mote1 in this.def.motes)
    {
      Vector3 vector3_1 = loc;
      if (GenView.ShouldSpawnMotesAt(loc, ((Thing) this.vehicle).Map, true))
      {
        try
        {
          float num = Altitudes.AltitudeFor(((BuildableDef) mote1.moteDef).altitudeLayer);
          Vector3 vector3_2 = Vector3Utility.RotatedBy(mote1.offset, this.TurretRotation);
          Vector3 vector3_3 = Vector3.op_Addition(vector3_1, new Vector3(vector3_2.x, num + vector3_2.y, vector3_2.z));
          Mote mote2 = (Mote) ThingMaker.MakeThing(mote1.moteDef, (ThingDef) null);
          mote2.instanceColor = mote1.color;
          mote2.rotationRate = mote1.rotationRate;
          mote2.Scale = mote1.scale;
          ((Thing) mote2).def = mote1.moteDef;
          GenSpawn.Spawn((Thing) mote2, IntVec3Utility.ToIntVec3(vector3_3), ((Thing) this.vehicle).Map, (WipeMode) 0);
          mote2.exactPosition = vector3_3;
          mote2.exactRotation = ((FloatRange) ref mote1.exactRotation).RandomInRange;
          if (!(mote2 is MoteThrown moteThrown))
          {
            if (mote2 is MoteCannonPlume moteCannonPlume)
            {
              moteCannonPlume.cyclesLeft = mote1.cycles;
              moteCannonPlume.animationType = mote1.animationType;
              moteCannonPlume.exactRotation = this.TurretRotation;
            }
          }
          else
          {
            float angle = this.TurretRotation + ((FloatRange) ref mote1.angleThrown).RandomInRange;
            if (moteThrown is MoteThrownExpand moteThrownExpand)
            {
              if (moteThrownExpand is MoteThrownSlowToSpeed thrownSlowToSpeed)
                thrownSlowToSpeed.SetDecelerationRate(((FloatRange) ref mote1.deceleration).RandomInRange, mote1.fixedAcceleration, angle);
              moteThrownExpand.growthRate = ((FloatRange) ref mote1.growthRate).RandomInRange;
            }
            moteThrown.SetVelocity(angle, ((FloatRange) ref mote1.speedThrown).RandomInRange);
          }
        }
        catch (Exception ex)
        {
          SmashLog.Error($"Failed to spawn mote at {loc}. MoteDef = <field>{((Def) mote1.moteDef)?.defName ?? "Null"}</field> Exception = {ex}");
        }
      }
    }
  }

  public void RecacheRootDrawPos()
  {
    if (this.GraphicData == null)
      return;
    this.rootDrawPosNorth = RootOffset(this, Rot8.North);
    this.rootDrawPosEast = RootOffset(this, Rot8.East);
    this.rootDrawPosSouth = RootOffset(this, Rot8.South);
    this.rootDrawPosWest = RootOffset(this, Rot8.West);
    this.rootDrawPosNorthEast = RootOffset(this, Rot8.NorthEast);
    this.rootDrawPosSouthEast = RootOffset(this, Rot8.SouthEast);
    this.rootDrawPosSouthWest = RootOffset(this, Rot8.SouthWest);
    this.rootDrawPosNorthWest = RootOffset(this, Rot8.NorthWest);

    static Vector3 RootOffset(VehicleTurret turret, Rot8 rot)
    {
      Vector2 vector2 = turret.renderProperties.OffsetFor(rot);
      Graphic_Turret graphic = turret.Graphic;
      Vector3 vector3 = graphic != null ? graphic.DrawOffset((Rot4) rot) : Vector3.zero;
      return new Vector3(vector3.x + vector2.x, vector3.y + turret.DrawLayerOffset, vector3.z + vector2.y);
    }
  }

  public Vector3 TurretOffset(Rot8 rot)
  {
    switch (rot.AsInt)
    {
      case 0:
        return this.rootDrawPosNorth;
      case 1:
        return this.rootDrawPosEast;
      case 2:
        return this.rootDrawPosSouth;
      case 3:
        return this.rootDrawPosWest;
      case 4:
        return this.rootDrawPosNorthEast;
      case 5:
        return this.rootDrawPosSouthEast;
      case 6:
        return this.rootDrawPosSouthWest;
      case 7:
        return this.rootDrawPosNorthWest;
      default:
        throw new NotImplementedException("Invalid Rot8");
    }
  }

  public static VehicleTurret.SubGizmo SubGizmo_RemoveAmmo(VehicleTurret turret)
  {
    Action<Rect> drawGizmo = (Action<Rect>) (rect =>
    {
      if (turret.loadedAmmo != null)
      {
        TextBlock textBlock;
        // ISSUE: explicit constructor call
        ((TextBlock) ref textBlock).\u002Ector(new Color(GUI.color.r, GUI.color.g, GUI.color.b, turret.IconAlphaTicked));
        try
        {
          Widgets.DrawTextureFitted(rect, (UnityEngine.Texture) ((BuildableDef) turret.loadedAmmo).uiIcon, 1f, 1f);
        }
        finally
        {
          textBlock.Dispose();
        }
        Rect rect1;
        // ISSUE: explicit constructor call
        ((Rect) ref rect1).\u002Ector(rect);
        string stringSafe = Gen.ToStringSafe<int>(((IEnumerable<Thing>) turret.vehicle.inventory.innerContainer).Where<Thing>((Func<Thing, bool>) (td => td.def == turret.loadedAmmo)).Select<Thing, int>((Func<Thing, int>) (t => t.stackCount)).Sum());
        ref Rect local1 = ref rect1;
        ((Rect) ref local1).y = ((Rect) ref local1).y + ((Rect) ref rect1).height / 2f;
        ref Rect local2 = ref rect1;
        ((Rect) ref local2).x = ((Rect) ref local2).x + (((Rect) ref rect1).width - Verse.Text.CalcSize(stringSafe).x);
        Widgets.Label(rect1, stringSafe);
      }
      else
      {
        if (!turret.def.genericAmmo)
          return;
        ThingFilter ammunition = turret.def.ammunition;
        if (ammunition == null || ammunition.AllowedDefCount <= 0)
          return;
        ThingDef thingDef = turret.def.ammunition.AllowedThingDefs.FirstOrDefault<ThingDef>();
        Widgets.DrawTextureFitted(rect, (UnityEngine.Texture) ((BuildableDef) thingDef).uiIcon, 1f, 1f);
        Rect rect2;
        // ISSUE: explicit constructor call
        ((Rect) ref rect2).\u002Ector(rect);
        string stringSafe = Gen.ToStringSafe<int>(((IEnumerable<Thing>) turret.vehicle.inventory.innerContainer).Where<Thing>((Func<Thing, bool>) (td => td.def == turret.def.ammunition.AllowedThingDefs.FirstOrDefault<ThingDef>())).Select<Thing, int>((Func<Thing, int>) (t => t.stackCount)).Sum());
        ref Rect local3 = ref rect2;
        ((Rect) ref local3).y = ((Rect) ref local3).y + ((Rect) ref rect2).height / 2f;
        ref Rect local4 = ref rect2;
        ((Rect) ref local4).x = ((Rect) ref local4).x + (((Rect) ref rect2).width - Verse.Text.CalcSize(stringSafe).x);
        Widgets.Label(rect2, stringSafe);
      }
    });
    Func<bool> canClick = (Func<bool>) (() => turret.shellCount > 0);
    Action onClick = (Action) (() =>
    {
      turret.TryClearChamber();
      SoundStarter.PlayOneShot(SoundDefOf.Artillery_ShellLoaded, SoundInfo.op_Implicit(new TargetInfo(((Thing) turret.vehicle).Position, ((Thing) turret.vehicle).Map, false)));
    });
    TaggedString? labelCap = ((Def) turret.loadedAmmo)?.LabelCap;
    string tooltip = labelCap.HasValue ? TaggedString.op_Implicit(labelCap.GetValueOrDefault()) : (string) null;
    return new VehicleTurret.SubGizmo(drawGizmo, canClick, onClick, tooltip);
  }

  public static VehicleTurret.SubGizmo SubGizmo_ReloadFromInventory(VehicleTurret turret)
  {
    return new VehicleTurret.SubGizmo((Action<Rect>) (rect => Widgets.DrawTextureFitted(rect, (UnityEngine.Texture) VehicleTex.ReloadIcon, 1f, 1f)), (Func<bool>) (() => true), (Action) (() =>
    {
      if (turret.def.ammunition == null)
        turret.Reload();
      else if (turret.def.genericAmmo)
      {
        if (!((ThingOwner) turret.vehicle.inventory.innerContainer).Contains(turret.def.ammunition.AllowedThingDefs.FirstOrDefault<ThingDef>()))
          Messages.Message(TaggedString.op_Implicit(Translator.Translate("VF_NoAmmoAvailable")), MessageTypeDefOf.RejectInput, true);
        else
          turret.Reload(turret.def.ammunition.AllowedThingDefs.FirstOrDefault<ThingDef>());
      }
      else
      {
        List<FloatMenuOption> floatMenuOptionList = new List<FloatMenuOption>();
        List<ThingDef> list = ((IEnumerable<Thing>) turret.vehicle.inventory.innerContainer).Where<Thing>((Func<Thing, bool>) (d => turret.ContainsAmmoDefOrShell(d.def))).Select<Thing, ThingDef>((Func<Thing, ThingDef>) (t => t.def)).Distinct<ThingDef>().ToList<ThingDef>();
        for (int index = list.Count - 1; index >= 0; --index)
        {
          ThingDef ammo = list[index];
          floatMenuOptionList.Add(new FloatMenuOption(TaggedString.op_Implicit(((Def) list[index]).LabelCap), (Action) (() => turret.Reload(ammo)), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0));
        }
        if (floatMenuOptionList.Count == 0)
        {
          FloatMenuOption floatMenuOption = new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("VF_VehicleTurrets_NoAmmoToReload")), (Action) null, (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
          {
            Disabled = true
          };
          floatMenuOptionList.Add(floatMenuOption);
        }
        Find.WindowStack.Add((Window) new FloatMenu(floatMenuOptionList));
      }
    }), TaggedString.op_Implicit(Translator.Translate("VF_ReloadVehicleTurret")));
  }

  public static VehicleTurret.SubGizmo SubGizmo_FireMode(VehicleTurret turret)
  {
    return new VehicleTurret.SubGizmo((Action<Rect>) (rect => Widgets.DrawTextureFitted(rect, (UnityEngine.Texture) turret.CurrentFireMode.Icon, 1f, 1f)), (Func<bool>) (() => turret.def.fireModes.Count > 1), new Action(turret.CycleFireMode), turret.CurrentFireMode.label);
  }

  public static VehicleTurret.SubGizmo SubGizmo_AutoTarget(VehicleTurret turret)
  {
    return new VehicleTurret.SubGizmo((Action<Rect>) (rect =>
    {
      Widgets.DrawTextureFitted(rect, (UnityEngine.Texture) VehicleTex.AutoTargetIcon, 1f, 1f);
      Rect rect1;
      // ISSUE: explicit constructor call
      ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x + ((Rect) ref rect).width / 2f, ((Rect) ref rect).y + ((Rect) ref rect).height / 2f, ((Rect) ref rect).width / 2f, ((Rect) ref rect).height / 2f);
      GUI.DrawTexture(rect1, turret.AutoTarget ? (UnityEngine.Texture) Widgets.CheckboxOnTex : (UnityEngine.Texture) Widgets.CheckboxOffTex);
    }), (Func<bool>) (() => turret.CanAutoTarget), new Action(turret.SwitchAutoTarget), AutoTargetingTooltip());

    string AutoTargetingTooltip()
    {
      VehicleTurret.TooltipBuilder.Clear();
      VehicleTurret.TooltipBuilder.AppendLine(TaggedString.op_Implicit(Translator.Translate("VF_ToggleAutoTargeting")));
      VehicleTurret.TooltipBuilder.AppendLine();
      VehicleTurret.TooltipBuilder.AppendLine();
      StringBuilder tooltipBuilder = VehicleTurret.TooltipBuilder;
      TaggedString taggedString = TranslatorFormattedStringExtensions.Translate("VF_ToggleAutoTargetingDesc", NamedArgumentUtility.Named((object) GenText.UncapitalizeFirst(turret.AutoTarget ? Translator.TranslateSimple("On") : Translator.TranslateSimple("Off")), "ONOFF"));
      string str1 = ((TaggedString) ref taggedString).Resolve();
      tooltipBuilder.AppendLine(str1);
      string str2 = VehicleTurret.TooltipBuilder.ToString();
      VehicleTurret.TooltipBuilder.Clear();
      return str2;
    }
  }

  [NoProfiling]
  public (Texture2D mainTex, Texture2D maskTex) GetTextures(Rot8 rot)
  {
    throw new NotImplementedException();
  }

  public record SubGizmo
  {
    public readonly Action<Rect> drawGizmo;
    public readonly Func<bool> canClick;
    public readonly Action onClick;
    public readonly string tooltip;

    public SubGizmo(Action<Rect> drawGizmo, Func<bool> canClick, Action onClick, string tooltip)
    {
      this.drawGizmo = drawGizmo;
      this.canClick = canClick;
      this.onClick = onClick;
      this.tooltip = tooltip;
    }

    public bool IsValid => this.onClick != null;

    [CompilerGenerated]
    protected virtual bool PrintMembers(
    #nullable enable
    StringBuilder builder)
    {
      RuntimeHelpers.EnsureSufficientExecutionStack();
      builder.Append("drawGizmo = ");
      builder.Append((object) this.drawGizmo);
      builder.Append(", canClick = ");
      builder.Append((object) this.canClick);
      builder.Append(", onClick = ");
      builder.Append((object) this.onClick);
      builder.Append(", tooltip = ");
      builder.Append((object) this.tooltip);
      builder.Append(", IsValid = ");
      builder.Append(this.IsValid.ToString());
      return true;
    }

    [CompilerGenerated]
    public override int GetHashCode()
    {
      return (((EqualityComparer<System.Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<Action<Rect>>.Default.GetHashCode(this.drawGizmo)) * -1521134295 + EqualityComparer<Func<bool>>.Default.GetHashCode(this.canClick)) * -1521134295 + EqualityComparer<Action>.Default.GetHashCode(this.onClick)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.tooltip);
    }

    [CompilerGenerated]
    public virtual bool Equals(VehicleTurret.SubGizmo? other)
    {
      if ((object) this == (object) other)
        return true;
      return (object) other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<Action<Rect>>.Default.Equals(this.drawGizmo, other.drawGizmo) && EqualityComparer<Func<bool>>.Default.Equals(this.canClick, other.canClick) && EqualityComparer<Action>.Default.Equals(this.onClick, other.onClick) && EqualityComparer<string>.Default.Equals(this.tooltip, other.tooltip);
    }

    [CompilerGenerated]
    protected SubGizmo(VehicleTurret.SubGizmo original)
    {
      this.drawGizmo = original.drawGizmo;
      this.canClick = original.canClick;
      this.onClick = original.onClick;
      this.tooltip = original.tooltip;
    }
  }

  public class TurretDrawData : IMaterialCacheTarget
  {
    private readonly 
    #nullable disable
    VehicleTurret turret;
    public Graphic_Turret graphic;
    public GraphicDataRGB graphicData;
    public VehicleTurretRenderData renderData;

    public TurretDrawData(VehicleTurret turret, VehicleTurretRenderData renderData)
    {
      this.turret = turret;
      this.renderData = renderData;
    }

    public int MaterialCount => 1;

    public PatternDef PatternDef => this.turret.PatternDef;

    public string Name
    {
      get
      {
        return $"{this.turret.def}_{this.turret.key}_{((Thing) this.turret.vehicle)?.ThingID ?? "Def"}";
      }
    }

    public MaterialPropertyBlock PropertyBlock { get; } = new MaterialPropertyBlock();

    public void Set(GraphicDataRGB copyFrom, PatternData patternData)
    {
      this.graphic = VehicleTurret.GenerateGraphicData((IMaterialCacheTarget) this, this.turret, copyFrom, patternData, ref this.graphicData);
    }

    public Vector3 DrawOffset(Rot8 rot, float parentRotation, float rotation)
    {
      Rot8 rot1 = rot;
      if (this.turret.attachedTo != null)
        rot1 = Rot8.North;
      Graphic_Turret graphic = this.graphic;
      Vector3 vector3_1 = graphic != null ? graphic.DrawOffset((Rot4) rot1) : Vector3.zero;
      Vector2 vector2_1 = new Vector2(vector3_1.x, vector3_1.z).RotatePointClockwise(parentRotation);
      Vector2 vector2_2 = this.turret.renderProperties.OffsetFor(rot1);
      Vector2 coord;
      // ISSUE: explicit constructor call
      ((Vector2) ref coord).\u002Ector(vector2_1.x + vector2_2.x, vector2_1.y + vector2_2.y);
      Vector2 vector2_3 = coord.RotatePointClockwise(rotation);
      if (this.turret.attachedTo != null)
      {
        Vector3 vector3_2 = this.turret.attachedTo.DrawPosition(rot);
        vector2_3.x += vector3_2.x;
        vector2_3.y += vector3_2.z;
      }
      return new Vector3(vector2_3.x, vector3_1.y + this.turret.DrawLayerOffset, vector2_3.y);
    }

    public override string ToString()
    {
      return $"TurretDrawData_{this.turret.key}_({this.graphicData.texPath})";
    }
  }
}
