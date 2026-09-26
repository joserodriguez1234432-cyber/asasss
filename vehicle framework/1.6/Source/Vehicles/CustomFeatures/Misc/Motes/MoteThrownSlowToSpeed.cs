// Decompiled with JetBrains decompiler
// Type: Vehicles.MoteThrownSlowToSpeed
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class MoteThrownSlowToSpeed : MoteThrownExpand
{
  public Vector3 deceleration;
  public Vector3 minDeceleration;
  protected bool xNeg;
  protected bool zNeg;
  protected bool velocityXNeg;
  protected bool velocityZNeg;

  public Vector3 DecelerationLerp
  {
    get
    {
      Vector3 vector3 = Vector3.op_Multiply(this.deceleration, Mathf.Pow(Mathf.Clamp01(((Thing) this).def.mote.Lifespan - ((Mote) this).AgeSecs), 2.5f));
      Vector3 decelerationLerp;
      ((Vector3) ref decelerationLerp).\u002Ector(this.xNeg ? Mathf.Min(this.minDeceleration.x, vector3.x) : Mathf.Max(this.minDeceleration.x, vector3.x), vector3.y, this.zNeg ? Mathf.Min(this.minDeceleration.z, vector3.z) : Mathf.Max(this.minDeceleration.z, vector3.z));
      return decelerationLerp;
    }
  }

  public void SetDecelerationRate(float rate, float fixedAcceleration, float angle)
  {
    this.deceleration = Vector3.op_Multiply(Quaternion.op_Multiply(Quaternion.AngleAxis(angle, Vector3.up), Vector3.forward), rate);
    this.minDeceleration = Vector3.op_Multiply(Quaternion.op_Multiply(Quaternion.AngleAxis(angle, Vector3.up), Vector3.forward), fixedAcceleration);
    this.xNeg = (double) this.deceleration.x < 0.0;
    this.zNeg = (double) this.deceleration.z < 0.0;
    this.velocityXNeg = (double) this.velocity.x < 0.0;
    this.velocityZNeg = (double) this.velocity.z < 0.0;
  }

  protected virtual void TimeInterval(float deltaTime)
  {
    base.TimeInterval(deltaTime);
    this.velocity = Vector3.op_Addition(this.velocity, Vector3.op_Multiply(this.DecelerationLerp, deltaTime));
    this.velocity.x = this.velocityXNeg ? Mathf.Min(0.0f, this.velocity.x) : Mathf.Max(0.0f, this.velocity.x);
    this.velocity.z = this.velocityZNeg ? Mathf.Min(0.0f, this.velocity.z) : Mathf.Max(0.0f, this.velocity.z);
  }
}
