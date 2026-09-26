// Decompiled with JetBrains decompiler
// Type: Vehicles.DefaultTakeoff
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class DefaultTakeoff : LaunchProtocol
{
  [GraphEditable(Category = "Takeoff")]
  public LaunchProtocolProperties launchProperties;
  [GraphEditable(Category = "Landing")]
  public LaunchProtocolProperties landingProperties;

  public DefaultTakeoff()
  {
  }

  public DefaultTakeoff(DefaultTakeoff reference, VehiclePawn vehicle)
    : base((LaunchProtocol) reference, vehicle)
  {
    this.landingProperties = reference.landingProperties;
    this.launchProperties = reference.launchProperties;
  }

  protected override int TotalTicks_Takeoff => this.launchProperties.maxTicks;

  protected override int TotalTicks_Landing => this.landingProperties.maxTicks;

  public override LaunchProtocolProperties CurAnimationProperties
  {
    get
    {
      return this.launchType != LaunchProtocol.LaunchType.Landing ? this.launchProperties : this.landingProperties;
    }
  }

  public override LaunchProtocolProperties LandingProperties => this.landingProperties;

  public override LaunchProtocolProperties LaunchProperties => this.launchProperties;

  public override LaunchProtocolProperties GetProperties(
    LaunchProtocol.LaunchType launchType,
    Rot4 rot)
  {
    if (launchType == LaunchProtocol.LaunchType.Landing)
      return this.LandingProperties;
    if (launchType == LaunchProtocol.LaunchType.Takeoff)
      return this.LaunchProperties;
    throw new NotImplementedException();
  }

  public override bool FinishedAnimation(VehicleSkyfaller skyfaller)
  {
    return this.ticksPassed >= this.CurAnimationProperties.maxTicks;
  }

  protected override int AnimationEditorTick_Landing(int ticksPassed)
  {
    int remaining;
    this.ticksPassed = ticksPassed.Take(this.landingProperties.maxTicks, out remaining);
    this.TickMotes();
    return remaining;
  }

  protected override int AnimationEditorTick_Takeoff(int ticksPassed)
  {
    this.ticksPassed = ticksPassed;
    this.TickMotes();
    return 0;
  }

  protected override (Vector3 drawPos, float rotation, DynamicShadowData shadowData) AnimateLanding(
    Vector3 drawPos,
    float rotation,
    DynamicShadowData shadowData)
  {
    if (!this.LandingProperties.rotationCurve.NullOrEmpty())
    {
      int num = Ext_Math.Sign(Rot4.op_Inequality(this.LandingProperties.flipRotation, ((Thing) this.vehicle).Rotation));
      rotation += this.LandingProperties.rotationCurve.Evaluate(this.TimeInAnimation) * (float) num;
    }
    if (!this.LandingProperties.xPositionCurve.NullOrEmpty())
    {
      int num = Ext_Math.Sign(Rot4.op_Inequality(this.LandingProperties.flipHorizontal, ((Thing) this.vehicle).Rotation));
      drawPos.x += this.LandingProperties.xPositionCurve.Evaluate(this.TimeInAnimation) * (float) num;
    }
    if (!this.LandingProperties.zPositionCurve.NullOrEmpty())
    {
      int num = Ext_Math.Sign(Rot4.op_Inequality(this.LandingProperties.flipVertical, ((Thing) this.vehicle).Rotation));
      drawPos.z += this.LandingProperties.zPositionCurve.Evaluate(this.TimeInAnimation) * (float) num;
    }
    if (!this.LandingProperties.offsetCurve.NullOrEmpty())
    {
      Vector2 t = this.LandingProperties.offsetCurve.EvaluateT(this.TimeInAnimation);
      int num1 = Ext_Math.Sign(Rot4.op_Inequality(this.LandingProperties.flipHorizontal, ((Thing) this.vehicle).Rotation));
      int num2 = Ext_Math.Sign(Rot4.op_Inequality(this.LandingProperties.flipVertical, ((Thing) this.vehicle).Rotation));
      drawPos = Vector3.op_Addition(drawPos, new Vector3(t.x * (float) num1, 0.0f, t.y * (float) num2));
    }
    if (this.LandingProperties.renderShadow)
    {
      if (!this.LandingProperties.shadowSizeXCurve.NullOrEmpty())
        shadowData.width = this.LandingProperties.shadowSizeXCurve.Evaluate(this.TimeInAnimation);
      if (!this.LandingProperties.shadowSizeZCurve.NullOrEmpty())
        shadowData.height = this.LandingProperties.shadowSizeZCurve.Evaluate(this.TimeInAnimation);
      if (!this.LandingProperties.shadowAlphaCurve.NullOrEmpty())
        shadowData.alpha = this.LandingProperties.shadowAlphaCurve.Evaluate(this.TimeInAnimation);
    }
    return base.AnimateLanding(drawPos, rotation, shadowData);
  }

  protected override (Vector3 drawPos, float rotation, DynamicShadowData shadowData) AnimateTakeoff(
    Vector3 drawPos,
    float rotation,
    DynamicShadowData shadowData)
  {
    if (!this.LaunchProperties.rotationCurve.NullOrEmpty())
    {
      int num = Ext_Math.Sign(Rot4.op_Inequality(this.LaunchProperties.flipRotation, ((Thing) this.vehicle).Rotation));
      rotation += this.LaunchProperties.rotationCurve.Evaluate(this.TimeInAnimation) * (float) num;
    }
    if (!this.LaunchProperties.xPositionCurve.NullOrEmpty())
    {
      int num = Ext_Math.Sign(Rot4.op_Inequality(this.LaunchProperties.flipHorizontal, ((Thing) this.vehicle).Rotation));
      drawPos.x += this.LaunchProperties.xPositionCurve.Evaluate(this.TimeInAnimation) * (float) num;
    }
    if (!this.LaunchProperties.zPositionCurve.NullOrEmpty())
    {
      int num = Ext_Math.Sign(Rot4.op_Inequality(this.LaunchProperties.flipVertical, ((Thing) this.vehicle).Rotation));
      drawPos.z += this.LaunchProperties.zPositionCurve.Evaluate(this.TimeInAnimation) * (float) num;
    }
    if (!this.LaunchProperties.offsetCurve.NullOrEmpty())
    {
      Vector2 t = this.LaunchProperties.offsetCurve.EvaluateT(this.TimeInAnimation);
      int num1 = Ext_Math.Sign(Rot4.op_Inequality(this.LaunchProperties.flipHorizontal, ((Thing) this.vehicle).Rotation));
      int num2 = Ext_Math.Sign(Rot4.op_Inequality(this.LaunchProperties.flipVertical, ((Thing) this.vehicle).Rotation));
      drawPos = Vector3.op_Addition(drawPos, new Vector3(t.x * (float) num1, 0.0f, t.y * (float) num2));
    }
    if (this.LaunchProperties.renderShadow)
    {
      if (!this.LaunchProperties.shadowSizeXCurve.NullOrEmpty())
        shadowData.width = this.LaunchProperties.shadowSizeXCurve.Evaluate(this.TimeInAnimation);
      if (!this.LaunchProperties.shadowSizeZCurve.NullOrEmpty())
        shadowData.height = this.LaunchProperties.shadowSizeZCurve.Evaluate(this.TimeInAnimation);
      if (!this.LaunchProperties.shadowAlphaCurve.NullOrEmpty())
        shadowData.alpha = this.LaunchProperties.shadowAlphaCurve.Evaluate(this.TimeInAnimation);
    }
    return base.AnimateTakeoff(drawPos, rotation, shadowData);
  }

  public override void ResolveProperties(LaunchProtocol reference)
  {
    base.ResolveProperties(reference);
    DefaultTakeoff defaultTakeoff = reference as DefaultTakeoff;
    this.launchProperties = defaultTakeoff.launchProperties;
    this.landingProperties = defaultTakeoff.landingProperties;
  }
}
