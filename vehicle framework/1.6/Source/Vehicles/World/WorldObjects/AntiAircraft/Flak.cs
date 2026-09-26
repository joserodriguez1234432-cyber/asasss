// Decompiled with JetBrains decompiler
// Type: Vehicles.World.Flak
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld.Planet;
using SmashTools;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.World;

public class Flak : AntiAircraft
{
  protected const float Spread = 0.1f;
  private const float MinSpeed = 0.01f;

  public override void Initialize(
    WorldObject firedFrom,
    AerialVehicleInFlight target,
    Vector3 source)
  {
    this.target = target;
    Vector3 vector3;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3).\u002Ector(Rand.Range(-0.1f, 0.1f), Rand.Range(-0.1f, 0.1f), Rand.Range(-0.1f, 0.1f));
    this.destination = Vector3.op_Subtraction(this.target.DrawPosAhead(50), vector3);
    this.source = source;
    this.firedFrom = firedFrom;
    this.speedPctPerTick = Mathf.Max(1f / 1000f / Ext_Math.SphericalDistance(this.source, this.destination) * (float) Rand.Range(40, 70), 0.01f);
    this.InitializeFacing();
    this.explosionFrame = -1;
  }
}
