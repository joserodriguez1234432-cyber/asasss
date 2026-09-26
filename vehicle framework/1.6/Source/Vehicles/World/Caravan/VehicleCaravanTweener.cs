// Decompiled with JetBrains decompiler
// Type: Vehicles.World.VehicleCaravanTweener
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using UnityEngine;

#nullable disable
namespace Vehicles.World;

public class VehicleCaravanTweener
{
  private const float SpringTightness = 0.09f;
  private readonly VehicleCaravan caravan;
  private Vector3 tweenedPos = Vector3.zero;
  private Vector3 lastTickSpringPos;

  public VehicleCaravanTweener(VehicleCaravan caravan) => this.caravan = caravan;

  public Vector3 TweenedPos => this.tweenedPos;

  public Vector3 LastTickTweenedVelocity
  {
    get => Vector3.op_Subtraction(this.TweenedPos, this.lastTickSpringPos);
  }

  private Vector3 TweenedPosRoot
  {
    get
    {
      return Vector3.op_Addition(VehicleCaravanTweenerUtility.PatherTweenedPosRoot(this.caravan), VehicleCaravanTweenerUtility.CaravanCollisionPosOffsetFor(this.caravan));
    }
  }

  public void TweenerTick()
  {
    this.lastTickSpringPos = this.tweenedPos;
    this.tweenedPos = Vector3.op_Addition(this.tweenedPos, Vector3.op_Multiply(Vector3.op_Subtraction(this.TweenedPosRoot, this.tweenedPos), 0.09f));
  }

  public void ResetTweenedPosToRoot()
  {
    this.tweenedPos = this.TweenedPosRoot;
    this.lastTickSpringPos = this.tweenedPos;
  }
}
