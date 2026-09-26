// Decompiled with JetBrains decompiler
// Type: Vehicles.VTOLTakeoff
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class VTOLTakeoff : DefaultTakeoff
{
  protected int ticksPassedVertical;
  protected float effectsToThrowVertical;

  public VTOLTakeoff()
  {
  }

  public VTOLTakeoff(VTOLTakeoff reference, VehiclePawn vehicle)
    : base((DefaultTakeoff) reference, vehicle)
  {
  }

  public VerticalProtocolProperties LandingProperties_VTOL
  {
    get => this.landingProperties as VerticalProtocolProperties;
  }

  public VerticalProtocolProperties LaunchProperties_VTOL
  {
    get => this.launchProperties as VerticalProtocolProperties;
  }

  public VerticalProtocolProperties CurAnimationProperties_Vertical
  {
    get => this.CurAnimationProperties as VerticalProtocolProperties;
  }

  protected override int TotalTicks_Landing
  {
    get => base.TotalTicks_Landing + this.LandingProperties_VTOL.maxTicksVertical;
  }

  protected override int TotalTicks_Takeoff
  {
    get => base.TotalTicks_Takeoff + this.LaunchProperties_VTOL.maxTicksVertical;
  }

  public virtual float TimeInAnimationVTOL
  {
    get
    {
      int maxTicksVertical = this.CurAnimationProperties_Vertical.maxTicksVertical;
      return maxTicksVertical <= 0 ? 0.0f : (float) this.ticksPassedVertical / (float) maxTicksVertical;
    }
  }

  public override bool FinishedAnimation(VehicleSkyfaller skyfaller)
  {
    return this.ticksPassedVertical >= this.CurAnimationProperties_Vertical.maxTicksVertical && base.FinishedAnimation(skyfaller);
  }

  protected override int AnimationEditorTick_Takeoff(int ticksPassed)
  {
    int remaining;
    this.ticksPassedVertical = ticksPassed.Take(this.LaunchProperties_VTOL.maxTicksVertical, out remaining);
    return base.AnimationEditorTick_Takeoff(remaining);
  }

  protected override int AnimationEditorTick_Landing(int ticksPassed)
  {
    ticksPassed = base.AnimationEditorTick_Landing(ticksPassed);
    int remaining;
    this.ticksPassedVertical = ticksPassed.Take(this.LandingProperties_VTOL.maxTicksVertical, out remaining);
    return remaining;
  }

  protected override (Vector3 drawPos, float rotation, DynamicShadowData shadowData) AnimateLanding(
    Vector3 drawPos,
    float rotation,
    DynamicShadowData shadowData)
  {
    if (!this.LandingProperties_VTOL.rotationVerticalCurve.NullOrEmpty())
      rotation += this.LandingProperties_VTOL.rotationVerticalCurve.Evaluate(this.TimeInAnimationVTOL);
    if (!this.LandingProperties_VTOL.zPositionVerticalCurve.NullOrEmpty())
      drawPos.z += this.LandingProperties_VTOL.zPositionVerticalCurve.Evaluate(this.TimeInAnimationVTOL);
    if (!this.LandingProperties_VTOL.xPositionVerticalCurve.NullOrEmpty())
      drawPos.x += this.LandingProperties_VTOL.xPositionVerticalCurve.Evaluate(this.TimeInAnimationVTOL);
    if (!this.LandingProperties_VTOL.offsetVerticalCurve.NullOrEmpty())
    {
      Vector2 t = this.LandingProperties_VTOL.offsetVerticalCurve.EvaluateT(this.TimeInAnimationVTOL);
      drawPos = Vector3.op_Addition(drawPos, new Vector3(t.x, 0.0f, t.y));
    }
    if (this.LandingProperties_VTOL.renderShadow)
    {
      if (!this.LandingProperties_VTOL.shadowSizeXVerticalCurve.NullOrEmpty())
        shadowData.width = this.LandingProperties_VTOL.shadowSizeXVerticalCurve.Evaluate(this.TimeInAnimationVTOL);
      if (!this.LandingProperties_VTOL.shadowSizeZVerticalCurve.NullOrEmpty())
        shadowData.height = this.LandingProperties_VTOL.shadowSizeZVerticalCurve.Evaluate(this.TimeInAnimationVTOL);
      if (!this.LandingProperties_VTOL.shadowAlphaVerticalCurve.NullOrEmpty())
        shadowData.alpha = this.LandingProperties_VTOL.shadowAlphaVerticalCurve.Evaluate(this.TimeInAnimationVTOL);
    }
    return base.AnimateLanding(drawPos, rotation, shadowData);
  }

  protected override (Vector3 drawPos, float rotation, DynamicShadowData shadowData) AnimateTakeoff(
    Vector3 drawPos,
    float rotation,
    DynamicShadowData shadowData)
  {
    if (!this.LaunchProperties_VTOL.rotationVerticalCurve.NullOrEmpty())
      rotation += this.LaunchProperties_VTOL.rotationVerticalCurve.Evaluate(this.TimeInAnimationVTOL);
    if (!this.LaunchProperties_VTOL.zPositionVerticalCurve.NullOrEmpty())
      drawPos.z += this.LaunchProperties_VTOL.zPositionVerticalCurve.Evaluate(this.TimeInAnimationVTOL);
    if (!this.LaunchProperties_VTOL.xPositionVerticalCurve.NullOrEmpty())
      drawPos.x += this.LaunchProperties_VTOL.xPositionVerticalCurve.Evaluate(this.TimeInAnimationVTOL);
    if (!this.LaunchProperties_VTOL.offsetVerticalCurve.NullOrEmpty())
    {
      Vector2 t = this.LaunchProperties_VTOL.offsetVerticalCurve.EvaluateT(this.TimeInAnimationVTOL);
      drawPos = Vector3.op_Addition(drawPos, new Vector3(t.x, 0.0f, t.y));
    }
    if (this.LaunchProperties_VTOL.renderShadow)
    {
      if (!this.LaunchProperties_VTOL.shadowSizeXVerticalCurve.NullOrEmpty())
        shadowData.width = this.LaunchProperties_VTOL.shadowSizeXVerticalCurve.Evaluate(this.TimeInAnimationVTOL);
      if (!this.LaunchProperties_VTOL.shadowSizeZVerticalCurve.NullOrEmpty())
        shadowData.height = this.LaunchProperties_VTOL.shadowSizeZVerticalCurve.Evaluate(this.TimeInAnimationVTOL);
      if (!this.LaunchProperties_VTOL.shadowAlphaVerticalCurve.NullOrEmpty())
        shadowData.alpha = this.LaunchProperties_VTOL.shadowAlphaVerticalCurve.Evaluate(this.TimeInAnimationVTOL);
    }
    return base.AnimateTakeoff(drawPos, rotation, shadowData);
  }

  protected override void TickMotes()
  {
    FleckData fleckDataVertical = this.CurAnimationProperties_Vertical.fleckDataVertical;
    if (fleckDataVertical != null && (fleckDataVertical.runOutOfStep || (double) this.TimeInAnimationVTOL > 0.0 && (double) this.TimeInAnimationVTOL < 1.0))
      this.effectsToThrowVertical = this.TryThrowFleck(fleckDataVertical, this.TimeInAnimationVTOL, this.effectsToThrowVertical);
    base.TickMotes();
  }

  protected override void TickLanding()
  {
    if (this.ticksPassed < this.landingProperties.maxTicks)
    {
      base.TickLanding();
    }
    else
    {
      ++this.ticksPassedVertical;
      this.TickMotes();
    }
  }

  protected override void TickTakeoff()
  {
    if (this.ticksPassedVertical >= this.LaunchProperties_VTOL.maxTicksVertical)
    {
      base.TickTakeoff();
    }
    else
    {
      ++this.ticksPassedVertical;
      this.TickMotes();
    }
  }

  protected override void TickEvents()
  {
    base.TickEvents();
    if (GenList.NullOrEmpty<AnimationEvent<LaunchProtocol>>((IList<AnimationEvent<LaunchProtocol>>) this.CurAnimationProperties_Vertical.eventsVertical))
      return;
    for (int index = 0; index < this.CurAnimationProperties_Vertical.eventsVertical.Count; ++index)
    {
      AnimationEvent<LaunchProtocol> animationEvent = this.CurAnimationProperties_Vertical.eventsVertical[index];
      if (animationEvent.EventFrame(this.TimeInAnimationVTOL))
        animationEvent.method.Invoke((object) null, (LaunchProtocol) this);
    }
  }

  protected override void PreAnimationSetup()
  {
    base.PreAnimationSetup();
    this.ticksPassedVertical = 0;
    this.effectsToThrowVertical = 0.0f;
  }

  public override void ExposeData()
  {
    base.ExposeData();
    Scribe_Values.Look<int>(ref this.ticksPassedVertical, "ticksPassedVertical", 0, false);
    Scribe_Values.Look<float>(ref this.effectsToThrowVertical, "effectsToThrowVertical", 0.0f, false);
  }
}
