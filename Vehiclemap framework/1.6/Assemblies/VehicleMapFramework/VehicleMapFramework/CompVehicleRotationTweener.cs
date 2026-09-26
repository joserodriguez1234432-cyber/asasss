// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompVehicleRotationTweener
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using SmashTools;
using SmashTools.Rendering;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompVehicleRotationTweener : VehicleComp
{
  private float tweenedAngle;
  private float curVelocity;

  private float TargetAngle
  {
    get
    {
      VehiclePath curPath = this.Vehicle.vehiclePather.curPath;
      if (curPath != null && curPath.NodesLeft > 2)
      {
        IntVec3 intVec3_1 = curPath.Peek(1);
        IntVec3 intVec3_2 = IntVec3.op_Subtraction(((Thing) this.Vehicle).Position, intVec3_1);
        if (((IntVec3) ref intVec3_2).LengthManhattan < ((BuildableDef) ((Thing) this.Vehicle).def).Size.z)
        {
          intVec3_2 = IntVec3.op_Subtraction(curPath.Peek(2), intVec3_1);
          return ((IntVec3) ref intVec3_2).AngleFlat;
        }
      }
      Rot8 fullRotation = this.Vehicle.FullRotation;
      return ((Rot8) ref fullRotation).AsAngle;
    }
  }

  public virtual void CompTick()
  {
    float targetAngle = this.TargetAngle;
    if (Mathf.Approximately(this.tweenedAngle, targetAngle))
      return;
    this.tweenedAngle = Mathf.SmoothDampAngle(this.tweenedAngle, targetAngle, ref this.curVelocity, 0.5f);
    Transform transform = this.Vehicle.Transform;
    double tweenedAngle = (double) this.tweenedAngle;
    Rot8 fullRotation = this.Vehicle.FullRotation;
    double asAngle = (double) ((Rot8) ref fullRotation).AsAngle;
    double num = tweenedAngle - asAngle;
    transform.rotation = (float) num;
  }

  public virtual void OnDeSpawn() => this.tweenedAngle = 0.0f;
}
