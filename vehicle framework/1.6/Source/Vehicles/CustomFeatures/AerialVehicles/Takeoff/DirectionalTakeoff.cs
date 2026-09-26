// Decompiled with JetBrains decompiler
// Type: Vehicles.DirectionalTakeoff
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class DirectionalTakeoff : LaunchProtocol
{
  [GraphEditable(Category = "Takeoff")]
  public DirectionalProtocolProperties launchProperties;
  [GraphEditable(Category = "Landing")]
  public DirectionalProtocolProperties landingProperties;

  public DirectionalTakeoff()
  {
  }

  public DirectionalTakeoff(DirectionalTakeoff reference, VehiclePawn vehicle)
    : base((LaunchProtocol) reference, vehicle)
  {
    this.launchProperties = reference.launchProperties;
    this.landingProperties = reference.landingProperties;
  }

  protected override int TotalTicks_Takeoff => this.LaunchProperties.maxTicks;

  protected override int TotalTicks_Landing => this.LandingProperties.maxTicks;

  public override LaunchProtocolProperties CurAnimationProperties
  {
    get
    {
      switch (this.launchType)
      {
        case LaunchProtocol.LaunchType.Landing:
          return this.LandingProperties;
        case LaunchProtocol.LaunchType.Takeoff:
          return this.LaunchProperties;
        default:
          throw new NotImplementedException(nameof (CurAnimationProperties));
      }
    }
  }

  public override LaunchProtocolProperties LandingProperties
  {
    get
    {
      Rot4 rotation = ((Thing) this.vehicle).Rotation;
      return !((Rot4) ref rotation).IsHorizontal ? this.landingProperties.vertical : this.landingProperties.horizontal;
    }
  }

  public override LaunchProtocolProperties LaunchProperties
  {
    get
    {
      Rot4 rotation = ((Thing) this.vehicle).Rotation;
      return !((Rot4) ref rotation).IsHorizontal ? this.launchProperties.vertical : this.launchProperties.horizontal;
    }
  }

  public override bool LaunchRestricted
  {
    get
    {
      return ((Thing) this.vehicle).Spawned && this.LaunchProperties.restriction != null && !this.LaunchProperties.restriction.CanStartProtocol(this.vehicle, ((Thing) this.vehicle).Map, ((Thing) this.vehicle).Position, ((Thing) this.vehicle).Rotation);
    }
  }

  public override LaunchProtocolProperties GetProperties(
    LaunchProtocol.LaunchType launchType,
    Rot4 rot)
  {
    if (launchType == LaunchProtocol.LaunchType.Landing)
      return ((Rot4) ref rot).IsHorizontal ? this.landingProperties.horizontal : this.landingProperties.vertical;
    if (launchType == LaunchProtocol.LaunchType.Takeoff)
      return ((Rot4) ref rot).IsHorizontal ? this.launchProperties.horizontal : this.launchProperties.vertical;
    throw new NotImplementedException();
  }

  public override bool LandingRestricted(Map map, IntVec3 position, Rot4 rotation)
  {
    return this.LandingProperties.restriction != null && !this.LandingProperties.restriction.CanStartProtocol(this.vehicle, map, position, rotation);
  }

  public override bool FinishedAnimation(VehicleSkyfaller skyfaller)
  {
    return this.TicksPassed >= this.CurAnimationProperties.maxTicks;
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
      int num1 = Ext_Math.Sign(Rot4.op_Inequality(this.LaunchProperties.flipHorizontal, ((Thing) this.vehicle).Rotation));
      int num2 = Ext_Math.Sign(Rot4.op_Inequality(this.LaunchProperties.flipVertical, ((Thing) this.vehicle).Rotation));
      Vector2 t = this.LaunchProperties.offsetCurve.EvaluateT(this.TimeInAnimation);
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
    DirectionalTakeoff directionalTakeoff = reference as DirectionalTakeoff;
    this.launchProperties = directionalTakeoff.launchProperties;
    this.landingProperties = directionalTakeoff.landingProperties;
  }
}
