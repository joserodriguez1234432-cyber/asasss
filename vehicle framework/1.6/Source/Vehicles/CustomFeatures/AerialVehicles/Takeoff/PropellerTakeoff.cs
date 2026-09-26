// Decompiled with JetBrains decompiler
// Type: Vehicles.PropellerTakeoff
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class PropellerTakeoff : VTOLTakeoff
{
  public const float DefaultAccelerationRate = 0.65f;
  public const float MinRotationStep = 0.1f;
  public const float MaxRotationStep = 59f;
  protected int ticksPassedPropeller;
  protected float effectsToThrowPropeller;

  public PropellerTakeoff()
  {
  }

  public PropellerTakeoff(PropellerTakeoff reference, VehiclePawn vehicle)
    : base((VTOLTakeoff) reference, vehicle)
  {
  }

  public PropellerProtocolProperties LandingProperties_Propeller
  {
    get => this.landingProperties as PropellerProtocolProperties;
  }

  public PropellerProtocolProperties LaunchProperties_Propeller
  {
    get => this.launchProperties as PropellerProtocolProperties;
  }

  public PropellerProtocolProperties CurAnimationProperties_Propeller
  {
    get => this.CurAnimationProperties as PropellerProtocolProperties;
  }

  protected override int TotalTicks_Landing
  {
    get => base.TotalTicks_Landing + this.LandingProperties_Propeller.maxTicksPropeller;
  }

  protected override int TotalTicks_Takeoff
  {
    get => base.TotalTicks_Takeoff + this.LaunchProperties_Propeller.maxTicksPropeller;
  }

  public virtual float TimeInAnimationPropeller
  {
    get
    {
      int maxTicksPropeller = this.CurAnimationProperties_Propeller.maxTicksPropeller;
      return maxTicksPropeller <= 0 ? 0.0f : (float) this.ticksPassedPropeller / (float) maxTicksPropeller;
    }
  }

  protected virtual float RotationRate(float t)
  {
    return this.CurAnimationProperties_Propeller.angularVelocityPropeller.Evaluate(t);
  }

  protected override void TickMotes()
  {
    FleckData fleckDataPropeller = this.CurAnimationProperties_Propeller.fleckDataPropeller;
    if (fleckDataPropeller != null && (fleckDataPropeller.runOutOfStep || (double) this.TimeInAnimationPropeller > 0.0 && (double) this.TimeInAnimationPropeller < 1.0))
      this.effectsToThrowPropeller = this.TryThrowFleck(fleckDataPropeller, this.TimeInAnimationPropeller, this.effectsToThrowPropeller);
    base.TickMotes();
  }

  protected override void TickLanding()
  {
    if (this.ticksPassedVertical < this.LandingProperties_Propeller.maxTicksVertical)
    {
      base.TickLanding();
    }
    else
    {
      ++this.ticksPassedPropeller;
      this.TickMotes();
    }
    this.vehicle.DrawTracker.overlayRenderer.SetAcceleration(this.RotationRate(this.TimeInAnimationPropeller));
  }

  protected override void TickTakeoff()
  {
    if (this.ticksPassedPropeller >= this.LaunchProperties_Propeller.maxTicksPropeller)
    {
      base.TickTakeoff();
    }
    else
    {
      ++this.ticksPassedPropeller;
      this.TickMotes();
    }
    this.vehicle.DrawTracker.overlayRenderer.SetAcceleration(this.RotationRate(this.TimeInAnimationPropeller));
  }

  protected override void TickEvents()
  {
    base.TickEvents();
    if (GenList.NullOrEmpty<AnimationEvent<LaunchProtocol>>((IList<AnimationEvent<LaunchProtocol>>) this.CurAnimationProperties_Propeller.eventsPropeller))
      return;
    for (int index = 0; index < this.CurAnimationProperties_Propeller.eventsPropeller.Count; ++index)
    {
      AnimationEvent<LaunchProtocol> animationEvent = this.CurAnimationProperties_Propeller.eventsPropeller[index];
      if (animationEvent.EventFrame(this.TimeInAnimationPropeller))
        animationEvent.method.Invoke((object) null, (LaunchProtocol) this);
    }
  }

  protected override int AnimationEditorTick_Landing(int ticksPassed)
  {
    ticksPassed = base.AnimationEditorTick_Landing(ticksPassed);
    int remaining;
    this.ticksPassedPropeller = ticksPassed.Take(this.LandingProperties_Propeller.maxTicksPropeller, out remaining);
    this.vehicle.DrawTracker.overlayRenderer.SetAcceleration(this.RotationRate(this.TimeInAnimationPropeller));
    return remaining;
  }

  protected override int AnimationEditorTick_Takeoff(int ticksPassed)
  {
    int remaining;
    this.ticksPassedPropeller = ticksPassed.Take(this.LaunchProperties_Propeller.maxTicksPropeller, out remaining);
    this.vehicle.DrawTracker.overlayRenderer.SetAcceleration(this.RotationRate(this.TimeInAnimationPropeller));
    return base.AnimationEditorTick_Takeoff(remaining);
  }

  protected override (Vector3 drawPos, float rotation, DynamicShadowData shadowData) AnimateLanding(
    Vector3 drawPos,
    float rotation,
    DynamicShadowData shadowData)
  {
    if (!this.LandingProperties_Propeller.rotationPropellerCurve.NullOrEmpty())
      rotation += this.LandingProperties_Propeller.rotationPropellerCurve.Evaluate(this.TimeInAnimationPropeller);
    if (!this.LandingProperties_Propeller.zPositionPropellerCurve.NullOrEmpty())
      drawPos.z += this.LandingProperties_Propeller.zPositionPropellerCurve.Evaluate(this.TimeInAnimationPropeller);
    if (!this.LandingProperties_Propeller.xPositionPropellerCurve.NullOrEmpty())
      drawPos.x += this.LandingProperties_Propeller.xPositionPropellerCurve.Evaluate(this.TimeInAnimationPropeller);
    if (!this.LandingProperties_Propeller.offsetPropellerCurve.NullOrEmpty())
    {
      Vector2 t = this.LandingProperties_Propeller.offsetPropellerCurve.EvaluateT(this.TimeInAnimationPropeller);
      drawPos = Vector3.op_Addition(drawPos, new Vector3(t.x, 0.0f, t.y));
    }
    if (this.LandingProperties_Propeller.renderShadow)
    {
      if (!this.LandingProperties_Propeller.shadowSizeXPropellerCurve.NullOrEmpty())
        shadowData.width = this.LandingProperties_Propeller.shadowSizeXPropellerCurve.Evaluate(this.TimeInAnimationPropeller);
      if (!this.LandingProperties_Propeller.shadowSizeZPropellerCurve.NullOrEmpty())
        shadowData.height = this.LandingProperties_Propeller.shadowSizeZPropellerCurve.Evaluate(this.TimeInAnimationPropeller);
      if (!this.LandingProperties_Propeller.shadowAlphaPropellerCurve.NullOrEmpty())
        shadowData.alpha = this.LandingProperties_Propeller.shadowAlphaPropellerCurve.Evaluate(this.TimeInAnimationPropeller);
    }
    return base.AnimateLanding(drawPos, rotation, shadowData);
  }

  protected override (Vector3 drawPos, float rotation, DynamicShadowData shadowData) AnimateTakeoff(
    Vector3 drawPos,
    float rotation,
    DynamicShadowData shadowData)
  {
    if (!this.LaunchProperties_Propeller.rotationPropellerCurve.NullOrEmpty())
      rotation += this.LaunchProperties_Propeller.rotationPropellerCurve.Evaluate(this.TimeInAnimationPropeller);
    if (!this.LaunchProperties_Propeller.zPositionPropellerCurve.NullOrEmpty())
      drawPos.z += this.LaunchProperties_Propeller.zPositionPropellerCurve.Evaluate(this.TimeInAnimationPropeller);
    if (!this.LaunchProperties_Propeller.xPositionPropellerCurve.NullOrEmpty())
      drawPos.x += this.LaunchProperties_Propeller.xPositionPropellerCurve.Evaluate(this.TimeInAnimationPropeller);
    if (!this.LaunchProperties_Propeller.offsetPropellerCurve.NullOrEmpty())
    {
      Vector2 t = this.LaunchProperties_Propeller.offsetPropellerCurve.EvaluateT(this.TimeInAnimationPropeller);
      drawPos = Vector3.op_Addition(drawPos, new Vector3(t.x, 0.0f, t.y));
    }
    if (this.LaunchProperties_Propeller.renderShadow)
    {
      if (!this.LaunchProperties_Propeller.shadowSizeXPropellerCurve.NullOrEmpty())
        shadowData.width = this.LaunchProperties_Propeller.shadowSizeXPropellerCurve.Evaluate(this.TimeInAnimationPropeller);
      if (!this.LaunchProperties_Propeller.shadowSizeZPropellerCurve.NullOrEmpty())
        shadowData.height = this.LaunchProperties_Propeller.shadowSizeZPropellerCurve.Evaluate(this.TimeInAnimationPropeller);
      if (!this.LaunchProperties_Propeller.shadowAlphaPropellerCurve.NullOrEmpty())
        shadowData.alpha = this.LaunchProperties_Propeller.shadowAlphaPropellerCurve.Evaluate(this.TimeInAnimationPropeller);
    }
    return base.AnimateTakeoff(drawPos, rotation, shadowData);
  }

  public override bool FinishedAnimation(VehicleSkyfaller skyfaller)
  {
    return this.ticksPassedPropeller >= this.CurAnimationProperties_Propeller.maxTicksPropeller && base.FinishedAnimation(skyfaller);
  }

  protected override void PreAnimationSetup()
  {
    base.PreAnimationSetup();
    this.ticksPassedPropeller = 0;
    this.effectsToThrowPropeller = 0.0f;
  }

  public override void ExposeData()
  {
    base.ExposeData();
    Scribe_Values.Look<int>(ref this.ticksPassedPropeller, "ticksPassedPropeller", 0, false);
    Scribe_Values.Look<float>(ref this.effectsToThrowPropeller, "effectsToThrowPropeller", 0.0f, false);
  }
}
