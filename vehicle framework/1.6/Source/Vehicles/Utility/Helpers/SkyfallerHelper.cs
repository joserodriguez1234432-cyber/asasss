// Decompiled with JetBrains decompiler
// Type: Vehicles.SkyfallerHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public static class SkyfallerHelper
{
  public static Vector3 DrawPos_Accelerate(Vector3 center, int ticks, float angle, float speed)
  {
    float dist = Mathf.Pow((float) ticks, 0.95f) * 1.7f * speed;
    return SkyfallerHelper.PosAtDist(center, dist, angle);
  }

  public static Vector3 DrawPos_ConstantSpeed(Vector3 center, int ticks, float angle, float speed)
  {
    float dist = (float) ticks * speed;
    return SkyfallerHelper.PosAtDist(center, dist, angle);
  }

  public static Vector3 DrawPos_Decelerate(Vector3 center, int ticks, float angle, float speed)
  {
    float dist = (float) (ticks * ticks) * 0.00721f * speed;
    return SkyfallerHelper.PosAtDist(center, dist, angle);
  }

  private static Vector3 PosAtDist(Vector3 center, float dist, float angle)
  {
    return Vector3.op_Addition(center, Vector3.op_Multiply(Vector3Utility.FromAngleFlat(angle - 90f), dist));
  }
}
