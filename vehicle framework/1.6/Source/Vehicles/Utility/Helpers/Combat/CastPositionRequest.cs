// Decompiled with JetBrains decompiler
// Type: Vehicles.CastPositionRequest
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using Verse;

#nullable disable
namespace Vehicles;

public readonly struct CastPositionRequest
{
  public readonly VehiclePawn vehicle;
  public readonly LocalTargetInfo target;
  public readonly float maxRange;
  public readonly float positionDistance;
  public readonly float maxRangeFromLocus;
  public readonly int maxRegions;
  public readonly float range;
  public readonly IntVec3 locus;
  public readonly IntVec3? preferredCastPosition;
  public readonly Func<IntVec3, bool> validator;

  public CastPositionRequest(
    VehiclePawn vehicle,
    LocalTargetInfo target,
    Func<IntVec3, bool> validator = null)
  {
    this.maxRange = 0.0f;
    this.positionDistance = 0.0f;
    this.maxRangeFromLocus = 0.0f;
    this.maxRegions = 0;
    this.locus = new IntVec3();
    this.preferredCastPosition = new IntVec3?();
    this.vehicle = vehicle;
    this.target = target;
    this.validator = validator;
    this.range = vehicle.CompVehicleTurrets.OptimalDistance;
  }

  public CastPositionRequest(
    VehiclePawn vehicle,
    LocalTargetInfo target,
    IntVec3? preferredCastPosition,
    Func<IntVec3, bool> validator = null)
    : this(vehicle, target, validator)
  {
    this.preferredCastPosition = preferredCastPosition;
  }

  public CastPositionRequest(
    VehiclePawn vehicle,
    LocalTargetInfo target,
    IntVec3? preferredCastPosition,
    float maxRange,
    float positionDistance,
    float maxRangeFromLocus,
    Func<IntVec3, bool> validator = null)
    : this(vehicle, target, validator)
  {
    this.maxRange = maxRange;
    this.positionDistance = positionDistance;
    this.maxRangeFromLocus = maxRangeFromLocus;
  }

  public CastPositionRequest(
    VehiclePawn vehicle,
    LocalTargetInfo target,
    IntVec3? preferredCastPosition,
    float maxRangeFromCaster,
    float maxRangeFromTarget,
    float maxRangeFromLocus,
    int maxRegions,
    Func<IntVec3, bool> validator = null)
    : this(vehicle, target, preferredCastPosition, maxRangeFromCaster, maxRangeFromTarget, maxRangeFromLocus, validator)
  {
    this.maxRegions = maxRegions;
  }

  public bool HasThing
  {
    get
    {
      LocalTargetInfo target = this.target;
      return ((LocalTargetInfo) ref target).HasThing;
    }
  }

  public static CastPositionRequest For(VehiclePawn vehicle, Thing target)
  {
    VehicleNPCProperties npcProperties = vehicle.VehicleDef.npcProperties;
    return new CastPositionRequest(vehicle, LocalTargetInfo.op_Implicit(target), new IntVec3?(((Thing) vehicle).Position), npcProperties.targetAcquireRadius, vehicle.CompVehicleTurrets.OptimalDistance, npcProperties.targetAcquireRadius);
  }
}
