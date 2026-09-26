// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleTweener
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class VehicleTweener
{
  private const float SpringTightness = 0.09f;
  private VehiclePawn vehicle;
  private Vector3 tweenedPos = Vector3.zero;
  private Vector3 lastTickSpringPos;
  private int lastDrawFrame = -1;

  public VehicleTweener(VehiclePawn vehicle) => this.vehicle = vehicle;

  public Vector3 TweenedPos => this.tweenedPos;

  public Vector3 LastTickTweenedVelocity
  {
    get => Vector3.op_Subtraction(this.TweenedPos, this.lastTickSpringPos);
  }

  public void PreDrawPosCalculation()
  {
    if (this.lastDrawFrame == RealTime.frameCount)
      return;
    if (this.lastDrawFrame < RealTime.frameCount - 1)
    {
      this.ResetTweenedPosToRoot();
    }
    else
    {
      this.lastTickSpringPos = this.tweenedPos;
      float tickRateMultiplier = Find.TickManager.TickRateMultiplier;
      if ((double) tickRateMultiplier < 5.0)
      {
        Vector3 vector3 = Vector3.op_Subtraction(this.TweenedPosRoot(), this.tweenedPos);
        float num = (float) (0.090000003576278687 * ((double) RealTime.deltaTime * 60.0 * (double) tickRateMultiplier));
        if ((double) RealTime.deltaTime > 0.05000000074505806)
          num = Mathf.Min(num, 1f);
        this.tweenedPos = Vector3.op_Addition(this.tweenedPos, Vector3.op_Multiply(vector3, num));
      }
      else
        this.tweenedPos = this.TweenedPosRoot();
    }
    this.lastDrawFrame = RealTime.frameCount;
  }

  public void ResetTweenedPosToRoot()
  {
    this.tweenedPos = this.TweenedPosRoot();
    this.lastTickSpringPos = this.tweenedPos;
  }

  private Vector3 TweenedPosRoot()
  {
    if (!((Thing) this.vehicle).Spawned || this.vehicle.vehiclePather == null)
      return this.vehicle.TrueCenter();
    float num = this.MovedPercent();
    return Vector3.op_Addition(Vector3.op_Multiply(this.vehicle.TrueCenter(this.vehicle.vehiclePather.nextCell), num), Vector3.op_Multiply(this.vehicle.TrueCenter(), 1f - num));
  }

  public float MovedPercent()
  {
    return this.vehicle.vehiclePather == null || !this.vehicle.vehiclePather.Moving || this.vehicle.vehiclePather.BuildingBlockingNextPathCell() != null ? 0.0f : (float) (1.0 - (double) this.vehicle.vehiclePather.nextCellCostLeft / (double) this.vehicle.vehiclePather.nextCellCostTotal);
  }
}
