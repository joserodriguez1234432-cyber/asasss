// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleDamager
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public static class VehicleDamager
{
  private const float FleeAngleIncrement = 22.5f;
  private static readonly int PawnNotifyCellCount = GenRadial.NumCellsInRadius(4.5f);
  private static readonly FloatRange[] FleeAngleRanges;

  public static void NotifyNearbyPawnsOfDangerousPosition(
    Map map,
    VehiclePawn vehicle,
    IntVec3 cell)
  {
    for (int index = 0; index < VehicleDamager.PawnNotifyCellCount; ++index)
    {
      IntVec3 intVec3 = IntVec3.op_Addition(cell, GenRadial.RadialPattern[index]);
      if (GenGrid.InBounds(intVec3, map))
      {
        foreach (Thing thing in GridsUtility.GetThingList(intVec3, map))
        {
          if (thing is Pawn pawn && GenSight.LineOfSight(cell, ((Thing) pawn).Position, map, true, (Func<IntVec3, bool>) null, 0, 0))
            VehicleDamager.Notify_DangerousPosition(pawn, vehicle, cell);
        }
      }
    }
  }

  private static void Notify_DangerousPosition(Pawn pawn, VehiclePawn vehicle, IntVec3 cell)
  {
    float distance;
    IntVec3 cell1;
    if (!VehicleDamager.ShouldFleeDangerZone(pawn, vehicle, out distance) || !RCellFinder.TryFindDirectFleeDestination(cell, distance * 1.9f, pawn, ref cell1))
      return;
    VehicleDamager.ForcePawnFlee(pawn, cell1);
  }

  public static float FriendlyFireChance(VehiclePawn vehicle, Pawn pawn)
  {
    float num1 = 1f;
    if (((Thing) pawn).Faction != ((Thing) vehicle).Faction)
      num1 = 0.5f;
    float num2 = num1;
    float num3;
    switch (VehicleMod.settings.main.friendlyFire)
    {
      case VehicleTracksFriendlyFire.None:
        num3 = 0.0f;
        break;
      case VehicleTracksFriendlyFire.Vanilla:
        num3 = Find.Storyteller.difficulty.friendlyFireChanceFactor;
        break;
      case VehicleTracksFriendlyFire.Custom:
        num3 = VehicleMod.settings.main.friendlyFireChance;
        break;
      default:
        throw new NotImplementedException("VehicleTracksFriendlyFire");
    }
    return num2 * num3;
  }

  public static void Notify_DangerousVehiclePath(this Pawn pawn, VehiclePawn vehicle)
  {
    float distance;
    if (!VehicleDamager.ShouldFleeDangerZone(pawn, vehicle, out distance))
      return;
    Rot8 opposite = vehicle.FullRotation.Opposite;
    Rot8 rot8_1 = opposite.Rotated((RotationDirection) 1);
    Rot8 rot8_2 = opposite.Rotated((RotationDirection) 3);
    IntVec3 result;
    if (!VehicleDamager.TryFindDirectFleeDestination(((Thing) vehicle).Position, distance, pawn, out result, opposite, rot8_1, rot8_2))
      return;
    VehicleDamager.ForcePawnFlee(pawn, result);
  }

  private static bool ShouldFleeDangerZone(Pawn pawn, VehiclePawn vehicle, out float distance)
  {
    distance = 5f;
    if (pawn is VehiclePawn || !((Thing) vehicle).Spawned || !((Thing) pawn).Spawned || pawn.Downed || pawn.Dead || pawn.InMentalState || (double) VehicleDamager.FriendlyFireChance(vehicle, pawn) == 0.0 || PawnUtility.PlayerForcedJobNowOrSoon(pawn) || pawn.RaceProps.intelligence < 1 || pawn.RaceProps.IsMechanoid || pawn.RaceProps.Insect || pawn.IsMutant)
      return false;
    distance *= (float) ((BuildableDef) vehicle.VehicleDef).Size.x;
    PawnFleeSettingsDefModExtension modExtension = ((Def) pawn.kindDef).GetModExtension<PawnFleeSettingsDefModExtension>();
    if (modExtension != null)
    {
      if (!modExtension.shouldFlee)
        return false;
      if ((double) modExtension.fleeDistance > 0.0)
        distance = modExtension.fleeDistance;
    }
    return true;
  }

  private static void ForcePawnFlee(Pawn pawn, IntVec3 cell)
  {
    pawn.jobs.EndCurrentJob((JobCondition) 16 /*0x10*/, true, true);
    Job job = JobMaker.MakeJob(JobDefOf.Goto, LocalTargetInfo.op_Implicit(cell));
    job.locomotionUrgency = (LocomotionUrgency) 4;
    if (!pawn.jobs.TryTakeOrderedJob(job, new JobTag?((JobTag) 0), false))
      return;
    MoteMaker.MakeColonistActionOverlay(pawn, ThingDefOf.Mote_ColonistFleeing);
  }

  private static bool TryFindDirectFleeDestination(
    IntVec3 root,
    float dist,
    Pawn pawn,
    out IntVec3 result,
    params Rot8[] excludeDirections)
  {
    List<Rot8> rot8List1 = new List<Rot8>(8)
    {
      Rot8.North,
      Rot8.NorthEast,
      Rot8.East,
      Rot8.SouthEast,
      Rot8.South,
      Rot8.SouthWest,
      Rot8.West,
      Rot8.NorthWest
    };
    if (!GenList.NullOrEmpty<Rot8>((IList<Rot8>) excludeDirections))
    {
      foreach (Rot8 excludeDirection in excludeDirections)
        rot8List1.Remove(excludeDirection);
    }
    Rand.PushState();
    try
    {
      for (int index = 0; index < 30; ++index)
      {
        List<Rot8> rot8List2 = rot8List1;
        Rot4 rotation = ((Thing) pawn).Rotation;
        Rot8 opposite = (Rot8) ((Rot4) ref rotation).Opposite;
        Rot8 rot8 = GenCollection.RandomElementWithFallback<Rot8>((IEnumerable<Rot8>) rot8List2, opposite);
        if (VehicleDamager.ImmediatelyWalkable(root, VehicleDamager.FleeAngleRanges[rot8.AsIntClockwise], dist, pawn, out result))
          return true;
      }
    }
    finally
    {
      Rand.PopState();
    }
    Region region = RegionAndRoomQuery.GetRegion((Thing) pawn, (RegionType) 14);
    for (int index = 0; index < 30; ++index)
    {
      IntVec3 randomCell = CellFinder.RandomRegionNear(region, 15, TraverseParms.For(pawn, (Danger) 3, (TraverseMode) 0, false, false, false, true), (Predicate<Region>) null, (Pawn) null, (RegionType) 14).RandomCell;
      if (GenGrid.Walkable(randomCell, ((Thing) pawn).Map))
      {
        IntVec3 intVec3 = IntVec3.op_Subtraction(root, randomCell);
        if ((double) ((IntVec3) ref intVec3).LengthHorizontalSquared > (double) dist * (double) dist)
        {
          using (PawnPath pathNow = ((Thing) pawn).Map.pathFinder.FindPathNow(((Thing) pawn).Position, LocalTargetInfo.op_Implicit(randomCell), pawn, new PathFinderCostTuning?(), (PathEndMode) 1))
          {
            if (PawnPathUtility.TryFindCellAtIndex(pathNow, (int) dist + 3, ref result))
              return true;
          }
        }
      }
    }
    result = ((Thing) pawn).Position;
    return false;
  }

  private static bool ImmediatelyWalkable(
    IntVec3 root,
    FloatRange angleRange,
    float distance,
    Pawn pawn,
    out IntVec3 result)
  {
    IntVec3 intVec3 = IntVec3.FromVector3(Vector3.op_Multiply(Quaternion.op_Multiply(Quaternion.AngleAxis(((FloatRange) ref angleRange).RandomInRange, Vector3.up), Vector3.forward), distance));
    result = IntVec3.op_Addition(root, intVec3);
    if (GenGrid.Walkable(result, ((Thing) pawn).Map) && IntVec3Utility.DistanceToSquared(result, ((Thing) pawn).Position) < IntVec3Utility.DistanceToSquared(result, root) && GenSight.LineOfSight(root, result, ((Thing) pawn).Map, true, (Func<IntVec3, bool>) null, 0, 0))
    {
      if (VehicleMod.settings.debug.debugDrawFleePoint)
      {
        ((Thing) pawn).Map.debugDrawer.FlashCell(result, 0.5f, (string) null, 50);
        ((Thing) pawn).Map.debugDrawer.FlashLine(((Thing) pawn).Position, result, 50, (SimpleColor) 2);
      }
      return true;
    }
    if (VehicleMod.settings.debug.debugDrawFleePoint && GenGrid.InBounds(result, ((Thing) pawn).Map))
    {
      ((Thing) pawn).Map.debugDrawer.FlashCell(result, 0.0f, (string) null, 50);
      ((Thing) pawn).Map.debugDrawer.FlashLine(((Thing) pawn).Position, result, 50, (SimpleColor) 1);
    }
    return false;
  }

  static VehicleDamager()
  {
    FloatRange[] floatRangeArray = new FloatRange[8]
    {
      new FloatRange(337.5f, 22.5f),
      null,
      null,
      null,
      null,
      null,
      null,
      null
    };
    Rot8 northEast = Rot8.NorthEast;
    double num1 = (double) northEast.AsAngle - 22.5;
    northEast = Rot8.NorthEast;
    double num2 = (double) northEast.AsAngle + 22.5;
    floatRangeArray[1] = new FloatRange((float) num1, (float) num2);
    Rot8 east = Rot8.East;
    double num3 = (double) east.AsAngle - 22.5;
    east = Rot8.East;
    double num4 = (double) east.AsAngle + 22.5;
    floatRangeArray[2] = new FloatRange((float) num3, (float) num4);
    Rot8 southEast = Rot8.SouthEast;
    double num5 = (double) southEast.AsAngle - 22.5;
    southEast = Rot8.SouthEast;
    double num6 = (double) southEast.AsAngle + 22.5;
    floatRangeArray[3] = new FloatRange((float) num5, (float) num6);
    Rot8 south = Rot8.South;
    double num7 = (double) south.AsAngle - 22.5;
    south = Rot8.South;
    double num8 = (double) south.AsAngle + 22.5;
    floatRangeArray[4] = new FloatRange((float) num7, (float) num8);
    Rot8 southWest = Rot8.SouthWest;
    double num9 = (double) southWest.AsAngle - 22.5;
    southWest = Rot8.SouthWest;
    double num10 = (double) southWest.AsAngle + 22.5;
    floatRangeArray[5] = new FloatRange((float) num9, (float) num10);
    Rot8 west = Rot8.West;
    double num11 = (double) west.AsAngle - 22.5;
    west = Rot8.West;
    double num12 = (double) west.AsAngle + 22.5;
    floatRangeArray[6] = new FloatRange((float) num11, (float) num12);
    Rot8 northWest = Rot8.NorthWest;
    double num13 = (double) northWest.AsAngle - 22.5;
    northWest = Rot8.NorthWest;
    double num14 = (double) northWest.AsAngle + 22.5;
    floatRangeArray[7] = new FloatRange((float) num13, (float) num14);
    VehicleDamager.FleeAngleRanges = floatRangeArray;
  }
}
