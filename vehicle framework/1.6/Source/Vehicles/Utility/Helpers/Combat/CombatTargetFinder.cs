// Decompiled with JetBrains decompiler
// Type: Vehicles.CombatTargetFinder
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public static class CombatTargetFinder
{
  public static Thing FindAttackTarget(
    VehiclePawn vehicle,
    TargetScanFlags scanFlags,
    Func<Thing, bool> validator = null,
    float minDistance = 0.0f,
    float maxDistance = 3.40282347E+38f,
    IntVec3? locus = null,
    float maxTravelRadiusFromLocus = 3.40282347E+38f,
    bool onlyRanged = false)
  {
    return GenClosest.ClosestThingReachable(((Thing) vehicle).Position, ((Thing) vehicle).Map, ThingRequest.ForGroup((ThingRequestGroup) 16 /*0x10*/), (PathEndMode) 2, TraverseParms.For((Pawn) vehicle, (Danger) 3, (TraverseMode) 0, false, false, false, true), maxDistance, (Predicate<Thing>) (target => (validator == null || validator(target)) && !CombatTargetFinder.ShouldIgnoreNonCombatant(vehicle, target, scanFlags)), (IEnumerable<Thing>) null, 0, (double) maxDistance > 800.0 ? -1 : 40, false, (RegionType) 14, false, false);
  }

  private static bool ShouldIgnoreNonCombatant(
    VehiclePawn vehicle,
    Thing thing,
    TargetScanFlags scanFlags)
  {
    if (!(thing is Pawn pawn) || PawnUtility.IsCombatant(pawn))
      return false;
    return (scanFlags & 1024 /*0x0400*/) != null || !GenSight.LineOfSightToThing(((Thing) vehicle).Position, (Thing) pawn, ((Thing) vehicle).Map, false, (Func<IntVec3, bool>) null);
  }
}
