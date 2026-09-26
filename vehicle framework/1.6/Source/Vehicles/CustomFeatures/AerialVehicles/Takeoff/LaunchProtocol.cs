// Decompiled with JetBrains decompiler
// Type: Vehicles.LaunchProtocol
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
using Vehicles.Compatibility;
using Vehicles.World;
using Verse;

#nullable disable
namespace Vehicles;

[StaticConstructorOnStartup]
public abstract class LaunchProtocol : IExposable
{
  protected VehiclePawn vehicle;
  protected bool drawOverlays = true;
  protected bool drawMotes = true;
  protected int ticksPassed;
  protected float effectsToThrow;
  protected LaunchProtocol.LaunchType launchType;
  private Map map;
  protected IntVec3 position = IntVec3.Invalid;
  protected List<Graphic>[] cachedOverlayGraphics;
  protected List<GraphicDataLayered>[] cachedOverlayGraphicDatas;
  protected Material cachedShadowMaterial;
  private static MaterialPropertyBlock shadowPropertyBlock;
  protected int maxFlightNodes = int.MaxValue;

  static LaunchProtocol()
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() => LaunchProtocol.shadowPropertyBlock = new MaterialPropertyBlock()));
  }

  public LaunchProtocol()
  {
  }

  public LaunchProtocol(LaunchProtocol reference, VehiclePawn vehicle)
  {
    this.vehicle = vehicle;
    this.maxFlightNodes = reference.maxFlightNodes;
  }

  public VehiclePawn Vehicle => this.vehicle;

  public Vector3 DrawPos { get; protected set; }

  public IntVec3 Position => this.position;

  public float Angle { get; protected set; }

  public int TicksPassed => this.ticksPassed;

  public Map Map => this.map ?? ((Thing) this.vehicle).Map;

  protected abstract int TotalTicks_Takeoff { get; }

  protected abstract int TotalTicks_Landing { get; }

  public abstract LaunchProtocolProperties CurAnimationProperties { get; }

  public abstract LaunchProtocolProperties LandingProperties { get; }

  public abstract LaunchProtocolProperties LaunchProperties { get; }

  public virtual int MaxFlightNodes => this.maxFlightNodes;

  public virtual float TimeInAnimation
  {
    get
    {
      int maxTicks = this.CurAnimationProperties.maxTicks;
      return maxTicks <= 0 ? 0.0f : (float) this.ticksPassed / (float) maxTicks;
    }
  }

  public virtual IEnumerable<AnimationDriver> Animations
  {
    get
    {
      LaunchProtocol launchProtocol = this;
      yield return new AnimationDriver("Takeoff", new Func<int, int>(launchProtocol.AnimationEditorTick_Takeoff), new AnimationDriver.AnimationDrawer(launchProtocol.Draw), launchProtocol.TotalTicks_Takeoff, new Action(launchProtocol.\u003Cget_Animations\u003Eb__47_0));
      yield return new AnimationDriver("Landing", new Func<int, int>(launchProtocol.AnimationEditorTick_Landing), new AnimationDriver.AnimationDrawer(launchProtocol.Draw), launchProtocol.TotalTicks_Landing, new Action(launchProtocol.\u003Cget_Animations\u003Eb__47_1));
    }
  }

  protected virtual Material ShadowMaterial
  {
    get
    {
      if (Object.op_Equality((Object) this.cachedShadowMaterial, (Object) null) && !GenText.NullOrEmpty(this.vehicle.CompVehicleLauncher.Props.shadow))
        this.cachedShadowMaterial = MaterialPool.MatFrom(this.vehicle.CompVehicleLauncher.Props.shadow, ShaderDatabase.Transparent);
      return this.cachedShadowMaterial;
    }
  }

  public virtual string FailLaunchMessage
  {
    get => TaggedString.op_Implicit(Translator.Translate("VF_AerialVehicleLaunchNotValid"));
  }

  public virtual bool CanLaunchNow
  {
    get
    {
      return (!((Thing) this.vehicle).Spawned || !Ext_Vehicles.IsRoofed(((Thing) this.vehicle).Position, ((Thing) this.vehicle).Map)) && !this.LaunchRestricted;
    }
  }

  public virtual bool LaunchRestricted
  {
    get
    {
      return this.LaunchProperties.restriction != null && !this.LaunchProperties.restriction.CanStartProtocol(this.vehicle, ((Thing) this.vehicle).Map, ((Thing) this.vehicle).Position, ((Thing) this.vehicle).Rotation);
    }
  }

  public virtual bool LandingRestricted(Map map, IntVec3 position, Rot4 rotation) => false;

  public abstract LaunchProtocolProperties GetProperties(
    LaunchProtocol.LaunchType launchType,
    Rot4 rot);

  public abstract bool FinishedAnimation(VehicleSkyfaller skyfaller);

  public (Vector3 drawPos, float rotation) Draw(Vector3 drawPos, float rotation)
  {
    (Vector3, float) valueTuple = (drawPos, rotation);
    DynamicShadowData shadowData = DynamicShadowData.CreateFrom(this.vehicle);
    switch (this.launchType)
    {
      case LaunchProtocol.LaunchType.Landing:
        // ISSUE: explicit reference operation
        ref Vector3 local1 = @valueTuple.Item1;
        // ISSUE: explicit reference operation
        ref float local2 = @valueTuple.Item2;
        (Vector3 drawPos, float rotation, DynamicShadowData shadowData) tuple1 = this.AnimateLanding(valueTuple.Item1, valueTuple.Item2, shadowData);
        Vector3 drawPos1 = tuple1.drawPos;
        local1 = drawPos1;
        local2 = tuple1.rotation;
        shadowData = tuple1.shadowData;
        break;
      case LaunchProtocol.LaunchType.Takeoff:
        // ISSUE: explicit reference operation
        ref Vector3 local3 = @valueTuple.Item1;
        // ISSUE: explicit reference operation
        ref float local4 = @valueTuple.Item2;
        (Vector3 drawPos, float rotation, DynamicShadowData shadowData) tuple2 = this.AnimateTakeoff(valueTuple.Item1, valueTuple.Item2, shadowData);
        Vector3 drawPos2 = tuple2.drawPos;
        local3 = drawPos2;
        local4 = tuple2.rotation;
        shadowData = tuple2.shadowData;
        break;
    }
    valueTuple.Item1.y = Altitudes.AltitudeFor((AltitudeLayer) 30);
    Rot8 rot = (Rot8) (this.CurAnimationProperties.forcedRotation ?? ((Thing) this.vehicle).Rotation);
    this.vehicle.DrawAt(valueTuple.Item1, rot, valueTuple.Item2);
    (this.DrawPos, this.Angle) = valueTuple;
    if (VehicleMod.settings.main.aerialVehicleEffects)
      this.DrawOverlays(valueTuple.Item1, valueTuple.Item2);
    if (!shadowData.Invalid)
    {
      Color white = Color.white;
      white.a = shadowData.alpha;
      IntVec3 intVec3 = ((IntVec3) ref this.position).IsValid || !((Thing) this.vehicle).Spawned ? this.position : ((Thing) this.vehicle).Position;
      this.DrawShadow(((IntVec3) ref intVec3).ToVector3Shifted(), shadowData.width, shadowData.height, white);
    }
    return valueTuple;
  }

  private void DrawShadow(Vector3 drawPos, float width, float height, Color color)
  {
    Material shadowMaterial = this.ShadowMaterial;
    if (shadowMaterial == null)
      return;
    Vector3 pos = drawPos;
    if (!this.CurAnimationProperties.lockShadowX)
      pos.x = this.DrawPos.x;
    if (!this.CurAnimationProperties.lockShadowZ)
      pos.z = this.DrawPos.z;
    this.DrawShadow(pos, this.CurAnimationProperties.forcedRotation ?? ((Thing) this.vehicle).Rotation, shadowMaterial, width, height, color);
  }

  private void DrawShadow(
    Vector3 pos,
    Rot4 rot,
    Material material,
    float width,
    float height,
    Color color)
  {
    pos.y = Altitudes.AltitudeFor((AltitudeLayer) 13);
    Vector3 vector3;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3).\u002Ector(width, 1f, height);
    LaunchProtocol.shadowPropertyBlock.SetColor(ShaderPropertyIDs.Color, color);
    Matrix4x4 matrix4x4 = new Matrix4x4();
    ((Matrix4x4) ref matrix4x4).SetTRS(pos, ((Rot4) ref rot).AsQuat, vector3);
    Graphics.DrawMesh(MeshPool.plane10Back, matrix4x4, material, 0, (Camera) null, 0, LaunchProtocol.shadowPropertyBlock);
  }

  protected virtual (Vector3 drawPos, float rotation, DynamicShadowData shadowData) AnimateLanding(
    Vector3 drawPos,
    float rotation,
    DynamicShadowData shadowData)
  {
    return (drawPos, rotation, shadowData);
  }

  protected virtual (Vector3 drawPos, float rotation, DynamicShadowData shadowData) AnimateTakeoff(
    Vector3 drawPos,
    float rotation,
    DynamicShadowData shadowData)
  {
    return (drawPos, rotation, shadowData);
  }

  protected virtual int AnimationEditorTick_Landing(int ticksPassed)
  {
    this.ticksPassed = ticksPassed;
    this.TickMotes();
    return 0;
  }

  protected virtual int AnimationEditorTick_Takeoff(int ticksPassed)
  {
    this.ticksPassed = ticksPassed;
    this.TickMotes();
    return 0;
  }

  protected virtual void DrawOverlays(Vector3 drawPos, float rotation)
  {
    if (!this.drawOverlays || GenList.NullOrEmpty<GraphicDataLayered>((IList<GraphicDataLayered>) this.CurAnimationProperties.additionalTextures))
      return;
    for (int index = 0; index < this.CurAnimationProperties.additionalTextures.Count; ++index)
    {
      GraphicDataLayered additionalTexture = this.CurAnimationProperties.additionalTextures[index];
      if (additionalTexture.Graphic is Graphic_Animate graphic)
        graphic.DrawWorkerAnimated(drawPos, Rot4.North, this.ticksPassed, rotation);
      else
        additionalTexture.Graphic.DrawWorker(drawPos, Rot4.North, (ThingDef) null, (Thing) null, rotation);
    }
  }

  protected virtual void TickMotes()
  {
    if (!GenList.NullOrEmpty<FleckData>((IList<FleckData>) this.CurAnimationProperties.fleckData))
    {
      foreach (FleckData fleckData in this.CurAnimationProperties.fleckData)
      {
        if (fleckData.runOutOfStep || (double) this.TimeInAnimation > 0.0 && (double) this.TimeInAnimation < 1.0)
          this.effectsToThrow = this.TryThrowFleck(fleckData, this.TimeInAnimation, this.effectsToThrow);
      }
    }
    if (GenList.NullOrEmpty<FleckOneShot>((IList<FleckOneShot>) this.CurAnimationProperties.fleckOneShots))
      return;
    foreach (FleckOneShot fleckOneShot in this.CurAnimationProperties.fleckOneShots)
    {
      if (fleckOneShot.emitAtTick == this.TicksPassed)
        this.ThrowFleck(fleckOneShot);
    }
  }

  public void Tick()
  {
    switch (this.launchType)
    {
      case LaunchProtocol.LaunchType.Landing:
        this.TickLanding();
        break;
      case LaunchProtocol.LaunchType.Takeoff:
        this.TickTakeoff();
        break;
    }
  }

  protected virtual void TickLanding()
  {
    ++this.ticksPassed;
    this.TickEvents();
    if (!VehicleMod.settings.main.aerialVehicleEffects)
      return;
    this.TickMotes();
  }

  protected virtual void TickTakeoff()
  {
    ++this.ticksPassed;
    this.TickEvents();
    if (!VehicleMod.settings.main.aerialVehicleEffects)
      return;
    this.TickMotes();
  }

  protected virtual void TickEvents()
  {
    if (GenList.NullOrEmpty<AnimationEvent<LaunchProtocol>>((IList<AnimationEvent<LaunchProtocol>>) this.CurAnimationProperties.events))
      return;
    for (int index = 0; index < this.CurAnimationProperties.events.Count; ++index)
    {
      AnimationEvent<LaunchProtocol> animationEvent = this.CurAnimationProperties.events[index];
      try
      {
        if (animationEvent.EventFrame(this.TimeInAnimation))
          animationEvent.method.Invoke((object) null, this);
      }
      catch (Exception ex)
      {
        Log.Error($"Exception thrown ticking animation event {animationEvent?.method} for {this.Vehicle}.\nException={ex}");
      }
    }
  }

  private static void SetMoteStatus(LaunchProtocol launchProtocol, bool active)
  {
    launchProtocol.drawMotes = active;
  }

  private static void SetOverlayStatus(LaunchProtocol launchProtocol, bool active)
  {
    launchProtocol.drawOverlays = active;
  }

  private static void SetComponentHealth(LaunchProtocol launchProtocol, string key, float health)
  {
    launchProtocol.vehicle.statHandler.SetComponentHealth(key, health);
  }

  public virtual void Prepare(Map map, IntVec3 position, Rot4 rot)
  {
    this.map = map;
    this.position = position;
    ((Thing) this.vehicle).Rotation = rot;
  }

  public virtual void Release()
  {
    this.map = (Map) null;
    this.position = IntVec3.Invalid;
  }

  public virtual void SetTickCount(int ticks) => this.ticksPassed = ticks;

  protected virtual void PreAnimationSetup()
  {
    this.ticksPassed = 0;
    this.TickEvents();
  }

  protected virtual float TryThrowFleck(FleckData fleckData, float t, float count)
  {
    float num1 = fleckData.frequency.Evaluate(t);
    count += num1 / 60f;
    int num2 = Mathf.FloorToInt(count);
    count -= (float) num2;
    for (int index = 0; index < num2; ++index)
      this.ThrowFleck(fleckData, t);
    return count;
  }

  public void ThrowFleck(FleckData fleckData, float t)
  {
    LinearCurve size1 = fleckData.size;
    float size2 = size1 != null ? size1.Evaluate(t) : 1f;
    float? airTime = fleckData.airTime?.Evaluate(t);
    float? speed = fleckData.speed?.Evaluate(t);
    float? rotationRate = fleckData.rotationRate?.Evaluate(t);
    float randomInRange = ((FloatRange) ref fleckData.angle).RandomInRange;
    Vector3 pos = ((IntVec3) ref this.position).ToVector3Shifted();
    if (!fleckData.lockFleckX)
      pos.x = this.DrawPos.x;
    if (!fleckData.lockFleckZ)
      pos.z = this.DrawPos.z;
    if (fleckData.drawOffset != null)
      pos = pos.PointFromAngle(fleckData.drawOffset.Evaluate(t), randomInRange);
    if (!fleckData.xFleckPositionCurve.NullOrEmpty())
      pos.x += fleckData.xFleckPositionCurve.Evaluate(t);
    if (!fleckData.zFleckPositionCurve.NullOrEmpty())
      pos.z += fleckData.zFleckPositionCurve.Evaluate(t);
    Vector3 loc = Vector3.op_Addition(pos, fleckData.originOffset);
    if (fleckData.originOffsetRange != null)
    {
      Vector3 from = fleckData.originOffsetRange.from;
      Vector3 to = fleckData.originOffsetRange.to;
      float num1 = from.x;
      if ((double) from.x != (double) to.x)
        num1 = Rand.Range(from.x, to.x);
      float num2 = from.y;
      if ((double) from.y != (double) to.y)
        num2 = Rand.Range(from.y, to.y);
      float num3 = from.z;
      if ((double) from.z != (double) to.z)
        num3 = Rand.Range(from.z, to.z);
      loc = Vector3.op_Addition(loc, new Vector3(num1, num2, num3));
    }
    loc.y = Altitudes.AltitudeFor(fleckData.def.altitudeLayer);
    LaunchProtocol.ThrowFleck(fleckData.def, loc, this.Map, size2, airTime, new float?(randomInRange), speed, rotationRate);
  }

  public void ThrowFleck(FleckOneShot fleckOneShot)
  {
    ref FloatRange? local1 = ref fleckOneShot.size;
    FloatRange valueOrDefault;
    double num1;
    if (!local1.HasValue)
    {
      num1 = 1.0;
    }
    else
    {
      valueOrDefault = local1.GetValueOrDefault();
      num1 = (double) ((FloatRange) ref valueOrDefault).RandomInRange;
    }
    float size = (float) num1;
    ref FloatRange? local2 = ref fleckOneShot.airTime;
    float? nullable1;
    if (!local2.HasValue)
    {
      nullable1 = new float?();
    }
    else
    {
      valueOrDefault = local2.GetValueOrDefault();
      nullable1 = new float?(((FloatRange) ref valueOrDefault).RandomInRange);
    }
    float? airTime = nullable1;
    ref FloatRange? local3 = ref fleckOneShot.speed;
    float? nullable2;
    if (!local3.HasValue)
    {
      nullable2 = new float?();
    }
    else
    {
      valueOrDefault = local3.GetValueOrDefault();
      nullable2 = new float?(((FloatRange) ref valueOrDefault).RandomInRange);
    }
    float? speed = nullable2;
    ref FloatRange? local4 = ref fleckOneShot.rotationRate;
    float? nullable3;
    if (!local4.HasValue)
    {
      nullable3 = new float?();
    }
    else
    {
      valueOrDefault = local4.GetValueOrDefault();
      nullable3 = new float?(((FloatRange) ref valueOrDefault).RandomInRange);
    }
    float? rotationRate = nullable3;
    float randomInRange = ((FloatRange) ref fleckOneShot.angle).RandomInRange;
    Vector3 loc = ((IntVec3) ref this.position).ToVector3Shifted();
    if (!fleckOneShot.lockFleckX)
      loc.x = this.DrawPos.x;
    if (!fleckOneShot.lockFleckZ)
      loc.z = this.DrawPos.z;
    loc = Vector3.op_Addition(loc, fleckOneShot.originOffset);
    if (fleckOneShot.originOffsetRange != null)
    {
      Vector3 from = fleckOneShot.originOffsetRange.from;
      Vector3 to = fleckOneShot.originOffsetRange.to;
      float num2 = from.x;
      if ((double) from.x != (double) to.x)
        num2 = Rand.Range(from.x, to.x);
      float num3 = from.y;
      if ((double) from.y != (double) to.y)
        num3 = Rand.Range(from.y, to.y);
      float num4 = from.z;
      if ((double) from.z != (double) to.z)
        num4 = Rand.Range(from.z, to.z);
      loc = Vector3.op_Addition(loc, new Vector3(num2, num3, num4));
    }
    loc.y = Altitudes.AltitudeFor(fleckOneShot.def.altitudeLayer);
    LaunchProtocol.ThrowFleck(fleckOneShot.def, loc, this.Map, size, airTime, new float?(randomInRange), speed, rotationRate);
  }

  public static void ThrowFleck(
    FleckDef fleckDef,
    Vector3 loc,
    Map map,
    float size,
    float? airTime,
    float? angle,
    float? speed,
    float? rotationRate)
  {
    Rand.PushState();
    try
    {
      FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc, map, fleckDef, size);
      if (rotationRate.HasValue)
        dataStatic.rotationRate = rotationRate.Value * ((double) Rand.Value < 0.5 ? 1f : -1f);
      if (speed.HasValue)
        dataStatic.velocitySpeed = speed.Value;
      if (angle.HasValue)
        dataStatic.velocityAngle = angle.Value;
      if (airTime.HasValue)
        dataStatic.airTimeLeft = new float?(airTime.Value);
      map.flecks.CreateFleck(dataStatic);
    }
    finally
    {
      Rand.PopState();
    }
  }

  public virtual void OrderProtocol(LaunchProtocol.LaunchType launchType)
  {
    this.launchType = launchType;
    this.PreAnimationSetup();
  }

  public virtual IEnumerable<ArrivalOption> GetArrivalOptions(GlobalTargetInfo target)
  {
    if (((GlobalTargetInfo) ref target).WorldObject == null || this.vehicle.CompVehicleLauncher.SpaceFlight || ((Def) ((GlobalTargetInfo) ref target).WorldObject.def).GetModExtension<SpaceObjectDefModExtension>() == null)
    {
      MapParent mapParent = ((GlobalTargetInfo) ref target).WorldObject as MapParent;
      if (mapParent != null)
      {
        if (mapParent != null && ((WorldObject) mapParent).Spawned && mapParent.HasMap && !EnterCooldownCompUtility.EnterCooldownBlocksEntering(mapParent))
          yield return new ArrivalOption(TranslatorFormattedStringExtensions.Translate("LandInExistingMap", NamedArgument.op_Implicit(((WorldObject) mapParent).Label)), (Action<TargetData<GlobalTargetInfo>>) (targetData =>
          {
            Current.Game.CurrentMap = mapParent.Map;
            CameraJumper.TryHideWorld();
            LandingTargeter.Instance.BeginTargeting(this.vehicle, mapParent.Map, (Action<LocalTargetInfo, Rot4>) ((landingCell, rot) => LaunchProtocol.StartTargetingLocalMap(this.vehicle, targetData, mapParent, landingCell, rot)), (Func<LocalTargetInfo, bool>) (targetInfo => GenGrid.InBounds(((LocalTargetInfo) ref targetInfo).Cell, mapParent.Map) && !Ext_Vehicles.IsRoofRestricted(this.vehicle.VehicleDef, ((LocalTargetInfo) ref targetInfo).Cell, mapParent.Map)), allowRotating: this.vehicle.VehicleDef.rotatable);
          }));
        else if (!mapParent.HasMap)
        {
          if (mapParent is Settlement settlement)
          {
            if (settlement.Visitable)
            {
              yield return new ArrivalOption(TranslatorFormattedStringExtensions.Translate("VisitSettlement", NamedArgument.op_Implicit(((WorldObject) settlement).Label)), (IArrivalAction) new ArrivalAction_VisitSettlement(this.vehicle));
              if (FloatMenuAcceptanceReport.op_Implicit(ArrivalAction_Trade.CanTradeWith(this.vehicle, settlement)))
                yield return new ArrivalOption(TranslatorFormattedStringExtensions.Translate("TradeWith", NamedArgument.op_Implicit(((WorldObject) settlement).Label)), (IArrivalAction) new ArrivalAction_Trade(this.vehicle));
              if (FloatMenuAcceptanceReport.op_Implicit(ArrivalAction_OfferGifts.CanOfferGiftsTo(this.vehicle, settlement)))
                yield return new ArrivalOption(TranslatorFormattedStringExtensions.Translate("OfferGifts", NamedArgument.op_Implicit(((WorldObject) settlement).Label)), (IArrivalAction) new ArrivalAction_OfferGifts(this.vehicle));
            }
            if (FloatMenuAcceptanceReport.op_Implicit(ArrivalAction_AttackSettlement.CanAttack(this.vehicle, settlement)))
            {
              if (this.vehicle.CompVehicleLauncher.ControlInFlight)
                yield return new ArrivalOption(TranslatorFormattedStringExtensions.Translate("VF_AttackAndTargetLanding", NamedArgument.op_Implicit(((WorldObject) settlement).Label)), (IArrivalAction) new ArrivalAction_AttackSettlement(this.vehicle, AerialVehicleArrivalModeDefOf.TargetedLanding));
              yield return new ArrivalOption(TranslatorFormattedStringExtensions.Translate("AttackAndDropAtEdge", NamedArgument.op_Implicit(((WorldObject) settlement).Label)), (IArrivalAction) new ArrivalAction_AttackSettlement(this.vehicle, AerialVehicleArrivalModeDefOf.EdgeDrop));
              yield return new ArrivalOption(TranslatorFormattedStringExtensions.Translate("AttackAndDropInCenter", NamedArgument.op_Implicit(((WorldObject) settlement).Label)), (IArrivalAction) new ArrivalAction_AttackSettlement(this.vehicle, AerialVehicleArrivalModeDefOf.CenterDrop));
            }
          }
          else if (AerialVehicleCompatibility.CanLandIn(mapParent))
          {
            if (this.vehicle.CompVehicleLauncher.ControlInFlight)
              yield return new ArrivalOption(TranslatorFormattedStringExtensions.Translate("VF_LandVehicleTargetedLanding", NamedArgument.op_Implicit(((WorldObject) mapParent).Label)), (IArrivalAction) new ArrivalAction_LoadMap(this.vehicle, AerialVehicleArrivalModeDefOf.TargetedLanding));
            yield return new ArrivalOption(TranslatorFormattedStringExtensions.Translate("VF_LandVehicleEdge", NamedArgument.op_Implicit(((WorldObject) mapParent).Label)), (IArrivalAction) new ArrivalAction_LoadMap(this.vehicle, AerialVehicleArrivalModeDefOf.EdgeDrop));
            yield return new ArrivalOption(TranslatorFormattedStringExtensions.Translate("VF_LandVehicleCenter", NamedArgument.op_Implicit(((WorldObject) mapParent).Label)), (IArrivalAction) new ArrivalAction_LoadMap(this.vehicle, AerialVehicleArrivalModeDefOf.CenterDrop));
          }
          settlement = (Settlement) null;
        }
      }
      else if (WorldVehiclePathGrid.Instance.Passable(((GlobalTargetInfo) ref target).Tile, this.vehicle.VehicleDef))
        yield return new ArrivalOption(Translator.Translate("FormCaravanHere"), (IArrivalAction) new ArrivalAction_LandToCaravan(this.vehicle));
    }
  }

  [PublicAPI]
  public static void StartTargetingLocalMap(
    VehiclePawn vehicle,
    TargetData<GlobalTargetInfo> targetData,
    MapParent mapParent,
    LocalTargetInfo landingCell,
    Rot4 rot)
  {
    if (((Thing) vehicle).Spawned)
    {
      vehicle.CompVehicleLauncher.Launch(targetData, (IArrivalAction) new ArrivalAction_LandToCell(vehicle, mapParent, ((LocalTargetInfo) ref landingCell).Cell, rot));
    }
    else
    {
      vehicle.GetOrMakeAerialVehicle().OrderFlyToTiles(targetData.targets.Select<GlobalTargetInfo, FlightNode>((Func<GlobalTargetInfo, FlightNode>) (target => new FlightNode(target))).ToList<FlightNode>(), (IArrivalAction) new ArrivalAction_LandToCell(vehicle, mapParent, ((LocalTargetInfo) ref landingCell).Cell, rot));
      vehicle.CompVehicleLauncher.inFlight = true;
      CameraJumper.TryShowWorld();
    }
  }

  public virtual void ResolveProperties(LaunchProtocol reference)
  {
    int length = ((IEnumerable<string>) Enum.GetNames(typeof (LaunchProtocol.LaunchType))).Count<string>();
    this.cachedOverlayGraphicDatas = new List<GraphicDataLayered>[length];
    this.cachedOverlayGraphics = new List<Graphic>[length];
  }

  public virtual void ExposeData()
  {
    Scribe_References.Look<VehiclePawn>(ref this.vehicle, "vehicle", true);
    Scribe_Values.Look<int>(ref this.ticksPassed, "ticksPassed", 0, false);
    Scribe_Values.Look<float>(ref this.effectsToThrow, "effectsToThrow", 0.0f, false);
    Scribe_Values.Look<bool>(ref this.drawOverlays, "drawOverlays", true, false);
    Scribe_Values.Look<bool>(ref this.drawMotes, "drawMotes", true, false);
    Scribe_Values.Look<LaunchProtocol.LaunchType>(ref this.launchType, "launchType", LaunchProtocol.LaunchType.Landing, false);
    Scribe_Values.Look<int>(ref this.maxFlightNodes, "maxFlightNodes", int.MaxValue, false);
    Scribe_References.Look<Map>(ref this.map, "map", false);
    Scribe_Values.Look<IntVec3>(ref this.position, "position", IntVec3.Invalid, false);
  }

  public enum LaunchType
  {
    Landing,
    Takeoff,
  }
}
