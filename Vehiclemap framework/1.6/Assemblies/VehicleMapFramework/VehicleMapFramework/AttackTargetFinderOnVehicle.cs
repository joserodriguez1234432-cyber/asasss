// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.AttackTargetFinderOnVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace VehicleMapFramework;

public static class AttackTargetFinderOnVehicle
{
  private const float FriendlyFireScoreOffsetPerHumanlikeOrMechanoid = 18f;
  private const float FriendlyFireScoreOffsetPerAnimal = 7f;
  private const float FriendlyFireScoreOffsetPerNonPawn = 10f;
  private const float FriendlyFireScoreOffsetSelf = 40f;
  private static readonly List<IAttackTarget> tmpTargets = new List<IAttackTarget>(128 /*0x80*/);
  private static readonly List<IAttackTarget> validTargets = new List<IAttackTarget>();
  private static readonly List<Pair<IAttackTarget, float>> availableShootingTargets = new List<Pair<IAttackTarget, float>>();
  private static readonly List<float> tmpTargetScores = new List<float>();
  private static readonly List<bool> tmpCanShootAtTarget = new List<bool>();
  private static readonly List<IntVec3> tempDestList = new List<IntVec3>();
  private static readonly List<IntVec3> tempSourceList = new List<IntVec3>();

  public static IAttackTarget BestAttackTarget(
    IAttackTargetSearcher searcher,
    TargetScanFlags flags,
    Predicate<Thing> validator = null,
    float minDist = 0.0f,
    float maxDist = 9999f,
    IntVec3 locus = default (IntVec3),
    float maxTravelRadiusFromLocus = 3.40282347E+38f,
    bool canBashDoors = false,
    bool canTakeTargetsCloserThanEffectiveMinRange = true,
    bool canBashFences = false,
    bool onlyRanged = false)
  {
    Thing searcherThing = searcher.Thing;
    Pawn searcherPawn = searcher as Pawn;
    Verb verb = searcher.CurrentEffectiveVerb;
    if (verb == null)
    {
      Log.Error($"BestAttackTarget with {Gen.ToStringSafe<IAttackTargetSearcher>(searcher)} who has no attack verb.");
      return (IAttackTarget) null;
    }
    bool onlyTargetMachines = !ModCompat.CombatExtended && VerbUtility.IsEMP(verb);
    float minDistSquared = minDist * minDist;
    float num1 = maxTravelRadiusFromLocus + verb.verbProps.range;
    float maxLocusDistSquared = num1 * num1;
    Func<IntVec3, bool> losValidator = (Func<IntVec3, bool>) null;
    if ((flags & 128 /*0x80*/) != null)
      losValidator = (Func<IntVec3, bool>) (vec3 => !GenGrid.InBounds(vec3, searcherThing.BaseMap()) || !GasUtility.AnyGas(vec3, searcherThing.BaseMap(), (GasType) 0));
    Predicate<IAttackTarget> innerValidator = (Predicate<IAttackTarget>) (t =>
    {
      Thing thing = t.Thing;
      Map map = thing.BaseMap();
      if (t == searcher)
        return false;
      if ((double) minDistSquared > 0.0)
      {
        IntVec3 intVec3 = IntVec3.op_Subtraction(VehicleMapUtility.get_PositionOnBaseMapSpawned(searcherThing), VehicleMapUtility.get_PositionOnBaseMapSpawned(thing));
        if ((double) ((IntVec3) ref intVec3).LengthHorizontalSquared < (double) minDistSquared)
          return false;
      }
      if (!canTakeTargetsCloserThanEffectiveMinRange)
      {
        float num2 = verb.verbProps.EffectiveMinRange(LocalTargetInfo.op_Implicit(thing), searcherThing);
        if ((double) num2 > 0.0)
        {
          IntVec3 intVec3 = IntVec3.op_Subtraction(VehicleMapUtility.get_PositionOnBaseMapSpawned(searcherThing), VehicleMapUtility.get_PositionOnBaseMapSpawned(thing));
          if ((double) ((IntVec3) ref intVec3).LengthHorizontalSquared < (double) num2 * (double) num2)
            return false;
        }
      }
      if ((double) maxTravelRadiusFromLocus < 9999.0)
      {
        IntVec3 intVec3 = IntVec3.op_Subtraction(VehicleMapUtility.get_PositionOnBaseMapSpawned(thing), locus);
        if ((double) ((IntVec3) ref intVec3).LengthHorizontalSquared > (double) maxLocusDistSquared)
          return false;
      }
      if (!GenHostility.HostileTo(searcherThing, thing) || validator != null && !validator(thing))
        return false;
      Pawn pawn2 = searcherPawn;
      Lord lord = pawn2 != null ? LordUtility.GetLord(pawn2) : (Lord) null;
      if (lord != null && !lord.LordJob.ValidateAttackTarget(searcherPawn, thing))
        return false;
      if ((flags & 512 /*0x0200*/) != null)
      {
        RoofDef roof = GridsUtility.GetRoof(VehicleMapUtility.get_PositionOnBaseMapSpawned(thing), map);
        if (roof != null && roof.isThickRoof)
          return false;
      }
      if ((flags & 3) != null)
      {
        if (losValidator != null && (!losValidator(VehicleMapUtility.get_PositionOnBaseMapSpawned(searcherThing)) || !losValidator(VehicleMapUtility.get_PositionOnBaseMapSpawned(thing))))
          return false;
        if (!searcherThing.CanSee(thing, losValidator))
        {
          if (t is Pawn)
          {
            if ((flags & 1) != null)
              return false;
          }
          else if ((flags & 2) != null)
            return false;
        }
      }
      if (((flags & 32 /*0x20*/) != null || (flags & 256 /*0x0100*/) != null) && t.ThreatDisabled(searcher) || (flags & 256 /*0x0100*/) != null && !AttackTargetFinderOnVehicle.IsAutoTargetable(t) || (flags & 64 /*0x40*/) != null && !GenHostility.IsActiveThreatTo(t, searcher.Thing.Faction, true, false) || onlyTargetMachines && t is Pawn pawn3 && pawn3.RaceProps.IsFlesh || (flags & 16 /*0x10*/) != null && FireUtility.IsBurning(thing))
        return false;
      RaceProperties race = searcherThing.def.race;
      if (race != null && race.intelligence >= 2)
      {
        CompExplosive comp = ThingCompUtility.TryGetComp<CompExplosive>(thing);
        if (comp != null && comp.wickStarted)
          return false;
      }
      IntVec2 size = thing.def.size;
      if (size.x == 1 && size.z == 1)
      {
        if (GridsUtility.Fogged(VehicleMapUtility.get_PositionOnBaseMapSpawned(thing), map))
          return false;
      }
      else
      {
        CellRect cellRect = thing.MovedOccupiedRect();
        foreach (IntVec3 intVec3 in cellRect)
        {
          if (GridsUtility.Fogged(intVec3, map))
            return false;
        }
      }
      return true;
    });
    Pawn pawn4 = searcherPawn;
    Ability ability;
    if (pawn4 == null)
    {
      ability = (Ability) null;
    }
    else
    {
      Pawn_AbilityTracker abilities = pawn4.abilities;
      ability = abilities != null ? GenCollection.FirstOrDefault<Ability>(abilities.AllAbilitiesForReading, (Predicate<Ability>) (a => a is Ability_GrapplingHook)) : (Ability) null;
    }
    bool flag = ability != null;
    if (AttackTargetFinderOnVehicle.HasRangedAttack(searcher) | onlyRanged | flag && (searcherPawn == null || !searcherPawn.InAggroMentalState))
    {
      AttackTargetFinderOnVehicle.tmpTargets.Clear();
      AttackTargetFinderOnVehicle.tmpTargets.AddRange(searcherThing.Map.BaseMapAndVehicleMaps(false).SelectMany<Map, IAttackTarget>((Func<Map, IEnumerable<IAttackTarget>>) (m => (IEnumerable<IAttackTarget>) m.attackTargetsCache.GetPotentialTargetsFor(searcher))));
      AttackTargetFinderOnVehicle.validTargets.Clear();
      for (int index = 0; index < AttackTargetFinderOnVehicle.tmpTargets.Count; ++index)
      {
        IAttackTarget tmpTarget = AttackTargetFinderOnVehicle.tmpTargets[index];
        IntVec3 onBaseMapSpawned = VehicleMapUtility.get_PositionOnBaseMapSpawned(tmpTarget.Thing);
        if (((IntVec3) ref onBaseMapSpawned).InHorDistOf(VehicleMapUtility.get_PositionOnBaseMapSpawned(searcherThing), maxDist) && innerValidator(tmpTarget))
          AttackTargetFinderOnVehicle.validTargets.Add(tmpTarget);
      }
      if (AttackTargetFinderOnVehicle.validTargets.Count == 0)
        return (IAttackTarget) null;
      IAttackTarget shootingTargetByScore = AttackTargetFinderOnVehicle.GetRandomShootingTargetByScore(AttackTargetFinderOnVehicle.validTargets, searcher, verb);
      if (shootingTargetByScore != null || searcher is Building_Turret || searcher is Pawn && searcherPawn.CurJobDef == JobDefOf.ManTurret)
        return shootingTargetByScore;
      return !flag && ((flags & 8) != null || (flags & 4) != null) ? (IAttackTarget) GenClosestCrossMap.ClosestThing_Global(VehicleMapUtility.get_PositionOnBaseMapSpawned(searcher.Thing), (IEnumerable) AttackTargetFinderOnVehicle.validTargets, maxDist, (Predicate<Thing>) (t => AttackTargetFinderOnVehicle.CanReach(searcher.Thing, t, canBashDoors, canBashFences))) : (IAttackTarget) GenClosestCrossMap.ClosestThing_Global(VehicleMapUtility.get_PositionOnBaseMapSpawned(searcher.Thing), (IEnumerable) AttackTargetFinderOnVehicle.validTargets, maxDist);
    }
    PawnDuty duty = searcherPawn?.mindState.duty;
    if (duty != null && (double) duty.radius > 0.0 && !searcherPawn.InMentalState)
    {
      Predicate<IAttackTarget> oldValidator = innerValidator;
      innerValidator = (Predicate<IAttackTarget>) (t =>
      {
        if (!oldValidator(t))
          return false;
        IntVec3 onBaseMapSpawned = VehicleMapUtility.get_PositionOnBaseMapSpawned(t.Thing);
        return ((IntVec3) ref onBaseMapSpawned).InHorDistOf(searcherPawn.mindState.duty.focus.CellOnBaseMap(), searcherPawn.mindState.duty.radius);
      });
    }
    Predicate<IAttackTarget> oldValidator2 = innerValidator;
    innerValidator = (Predicate<IAttackTarget>) (t => oldValidator2(t) && !AttackTargetFinderOnVehicle.ShouldIgnoreNoncombatant(searcherThing, t, flags));
    return (IAttackTarget) GenClosestCrossMap.ClosestThingReachable(searcherThing.Position, searcherThing.Map, ThingRequest.ForGroup((ThingRequestGroup) 16 /*0x10*/), (PathEndMode) 2, TraverseParms.For(searcherPawn, (Danger) 3, (TraverseMode) 0, canBashDoors, false, canBashFences, true), maxDist, (Predicate<Thing>) (x => innerValidator((IAttackTarget) x)), searchRegionsMax: (double) maxDist > 800.0 ? -1 : 40);
  }

  private static bool ShouldIgnoreNoncombatant(
    Thing searcherThing,
    IAttackTarget t,
    TargetScanFlags flags)
  {
    if (!(t is Pawn end) || PawnUtility.IsCombatant(end))
      return false;
    return (flags & 1024 /*0x0400*/) != null || !GenSightOnVehicle.LineOfSightThingToThing(searcherThing, (Thing) end);
  }

  private static bool CanReach(
    Thing searcher,
    Thing target,
    bool canBashDoors,
    bool canBashFences)
  {
    if (searcher is Pawn pawn)
    {
      if (!pawn.CanReach(LocalTargetInfo.op_Implicit(target), (PathEndMode) 2, (Danger) 2, canBashDoors, canBashFences, (TraverseMode) 0, target.Map))
        return false;
    }
    else
    {
      TraverseMode traverseMode = canBashDoors ? (TraverseMode) 1 : (TraverseMode) 2;
      if (!CrossMapReachabilityUtility.CanReach(searcher.Map, searcher.Position, LocalTargetInfo.op_Implicit(target), (PathEndMode) 2, TraverseParms.For(traverseMode, (Danger) 3, false, false, false, true, false), target.Map))
        return false;
    }
    return true;
  }

  private static IAttackTarget FindBestReachableMeleeTarget(
    Predicate<IAttackTarget> validator,
    Pawn searcherPawn,
    float maxTargDist,
    bool canBashDoors,
    bool canBashFences)
  {
    maxTargDist = Mathf.Min(maxTargDist, 30f);
    IAttackTarget reachableTarget = (IAttackTarget) null;
    ((Thing) searcherPawn).Map.floodFiller.FloodFill(((Thing) searcherPawn).Position, (Predicate<IntVec3>) (x =>
    {
      if (!GenGrid.WalkableBy(x, ((Thing) searcherPawn).Map, searcherPawn) || (double) IntVec3Utility.DistanceToSquared(x, ((Thing) searcherPawn).Position) > (double) maxTargDist * (double) maxTargDist)
        return false;
      Building edifice = GridsUtility.GetEdifice(x, ((Thing) searcherPawn).Map);
      return (edifice == null || (canBashDoors || !(edifice is Building_Door buildingDoor2) || buildingDoor2.CanPhysicallyPass(searcherPawn)) && (canBashFences || !((Thing) edifice).def.IsFence || !((Thing) searcherPawn).def.race.FenceBlocked)) && !PawnUtility.AnyPawnBlockingPathAt(x, searcherPawn, true, false, false, false);
    }), (Func<IntVec3, bool>) (x =>
    {
      for (int index = 0; index < 8; ++index)
      {
        IntVec3 x1 = IntVec3.op_Addition(x, GenAdj.AdjacentCells[index]);
        if (GenGrid.InBounds(x1, ((Thing) searcherPawn).Map))
        {
          IAttackTarget iattackTarget = bestTargetOnCell(x1);
          if (iattackTarget != null)
          {
            reachableTarget = iattackTarget;
            break;
          }
        }
      }
      return reachableTarget != null;
    }), int.MaxValue, false, (IEnumerable<IntVec3>) null);
    return reachableTarget;

    IAttackTarget bestTargetOnCell(IntVec3 x)
    {
      List<Thing> thingList = GridsUtility.GetThingList(x, ((Thing) searcherPawn).Map);
      for (int index = 0; index < thingList.Count; ++index)
      {
        Thing thing = thingList[index];
        if (thing is IAttackTarget iattackTarget1 && validator(iattackTarget1) && ReachabilityImmediate.CanReachImmediate(x, LocalTargetInfo.op_Implicit(thing), ((Thing) searcherPawn).Map, (PathEndMode) 2, searcherPawn) && (ReachabilityImmediate.CanReachImmediate(searcherPawn, LocalTargetInfo.op_Implicit(thing), (PathEndMode) 2) || ((Thing) searcherPawn).Map.attackTargetReservationManager.CanReserve(searcherPawn, iattackTarget1)))
          return iattackTarget1;
      }
      return (IAttackTarget) null;
    }
  }

  private static bool HasRangedAttack(IAttackTargetSearcher t)
  {
    Verb currentEffectiveVerb = t.CurrentEffectiveVerb;
    return currentEffectiveVerb != null && !currentEffectiveVerb.verbProps.IsMeleeAttack;
  }

  private static bool CanShootAtFromCurrentPosition(
    IAttackTarget target,
    IAttackTargetSearcher searcher,
    Verb verb)
  {
    return verb != null && verb.CanHitTargetFrom(VehicleMapUtility.get_PositionOnBaseMapSpawned(searcher.Thing), LocalTargetInfo.op_Implicit(target.Thing));
  }

  private static IAttackTarget GetRandomShootingTargetByScore(
    List<IAttackTarget> targets,
    IAttackTargetSearcher searcher,
    Verb verb)
  {
    Pair<IAttackTarget, float> pair;
    return !GenCollection.TryRandomElementByWeight<Pair<IAttackTarget, float>>((IEnumerable<Pair<IAttackTarget, float>>) AttackTargetFinderOnVehicle.GetAvailableShootingTargetsByScore(targets, searcher, verb), (Func<Pair<IAttackTarget, float>, float>) (x => x.Second), ref pair) ? (IAttackTarget) null : pair.First;
  }

  private static List<Pair<IAttackTarget, float>> GetAvailableShootingTargetsByScore(
    List<IAttackTarget> rawTargets,
    IAttackTargetSearcher searcher,
    Verb verb)
  {
    AttackTargetFinderOnVehicle.availableShootingTargets.Clear();
    if (rawTargets.Count == 0)
      return AttackTargetFinderOnVehicle.availableShootingTargets;
    AttackTargetFinderOnVehicle.tmpTargetScores.Clear();
    AttackTargetFinderOnVehicle.tmpCanShootAtTarget.Clear();
    float num1 = 0.0f;
    IAttackTarget iattackTarget = (IAttackTarget) null;
    for (int index = 0; index < rawTargets.Count; ++index)
    {
      AttackTargetFinderOnVehicle.tmpTargetScores.Add(float.MinValue);
      AttackTargetFinderOnVehicle.tmpCanShootAtTarget.Add(false);
      if (rawTargets[index] != searcher)
      {
        bool flag = AttackTargetFinderOnVehicle.CanShootAtFromCurrentPosition(rawTargets[index], searcher, verb);
        AttackTargetFinderOnVehicle.tmpCanShootAtTarget[index] = flag;
        if (flag)
        {
          float shootingTargetScore = AttackTargetFinderOnVehicle.GetShootingTargetScore(rawTargets[index], searcher, verb);
          AttackTargetFinderOnVehicle.tmpTargetScores[index] = shootingTargetScore;
          if (iattackTarget == null || (double) shootingTargetScore > (double) num1)
          {
            iattackTarget = rawTargets[index];
            num1 = shootingTargetScore;
          }
        }
      }
    }
    if ((double) num1 < 1.0)
    {
      if (iattackTarget != null)
        AttackTargetFinderOnVehicle.availableShootingTargets.Add(new Pair<IAttackTarget, float>(iattackTarget, 1f));
    }
    else
    {
      float num2 = num1 - 30f;
      for (int index = 0; index < rawTargets.Count; ++index)
      {
        if (rawTargets[index] != searcher && AttackTargetFinderOnVehicle.tmpCanShootAtTarget[index])
        {
          float tmpTargetScore = AttackTargetFinderOnVehicle.tmpTargetScores[index];
          if ((double) tmpTargetScore >= (double) num2)
          {
            float num3 = Mathf.InverseLerp(num1 - 30f, num1, tmpTargetScore);
            AttackTargetFinderOnVehicle.availableShootingTargets.Add(new Pair<IAttackTarget, float>(rawTargets[index], num3));
          }
        }
      }
    }
    return AttackTargetFinderOnVehicle.availableShootingTargets;
  }

  private static float GetShootingTargetScore(
    IAttackTarget target,
    IAttackTargetSearcher searcher,
    Verb verb)
  {
    IntVec3 intVec3 = IntVec3.op_Subtraction(VehicleMapUtility.get_PositionOnBaseMapSpawned(target.Thing), VehicleMapUtility.get_PositionOnBaseMapSpawned(searcher.Thing));
    float num1 = (float) (60.0 - (double) Mathf.Min(((IntVec3) ref intVec3).LengthHorizontal, 40f));
    if (LocalTargetInfo.op_Equality(target.TargetCurrentlyAimingAt, LocalTargetInfo.op_Implicit(searcher.Thing)))
      num1 += 10f;
    if (LocalTargetInfo.op_Equality(searcher.LastAttackedTarget, LocalTargetInfo.op_Implicit(target.Thing)) && Find.TickManager.TicksGame - searcher.LastAttackTargetTick <= 300)
      num1 += 40f;
    float num2 = num1 - CoverUtility.CalculateOverallBlockChance(LocalTargetInfo.op_Implicit(target.Thing.Position), searcher.Thing.PositionOnAnotherThingMap(target.Thing), target.Thing.Map) * 10f;
    if (target is Pawn target1)
    {
      num2 -= AttackTargetFinderOnVehicle.NonCombatantScore((Thing) target1);
      if ((double) verb.verbProps.ai_TargetHasRangedAttackScoreOffset != 0.0 && target1.CurrentEffectiveVerb != null && target1.CurrentEffectiveVerb.verbProps.Ranged)
        num2 += verb.verbProps.ai_TargetHasRangedAttackScoreOffset;
      if (target1.Downed)
        num2 -= 50f;
    }
    return (num2 + AttackTargetFinderOnVehicle.FriendlyFireBlastRadiusTargetScoreOffset(target, searcher, verb) + AttackTargetFinderOnVehicle.FriendlyFireConeTargetScoreOffset(target, searcher, verb)) * target.TargetPriorityFactor;
  }

  private static float NonCombatantScore(Thing target)
  {
    if (!(target is Pawn pawn))
      return 0.0f;
    if (!PawnUtility.IsCombatant(pawn))
      return 50f;
    return DevelopmentalStageExtensions.Juvenile(pawn.DevelopmentalStage) ? 25f : 0.0f;
  }

  private static float FriendlyFireBlastRadiusTargetScoreOffset(
    IAttackTarget target,
    IAttackTargetSearcher searcher,
    Verb verb)
  {
    float num = AttackTargetFinderOnVehicle.FriendlyFireBlastRadiusTargetScoreOffset(target, searcher, target.Thing.Map, verb);
    if (target.Thing.Map != searcher.Thing.Map)
      num += AttackTargetFinderOnVehicle.FriendlyFireBlastRadiusTargetScoreOffset(target, searcher, searcher.Thing.Map, verb);
    return num;
  }

  private static float FriendlyFireBlastRadiusTargetScoreOffset(
    IAttackTarget target,
    IAttackTargetSearcher searcher,
    Map map,
    Verb verb)
  {
    if ((double) verb.verbProps.ai_AvoidFriendlyFireRadius <= 0.0)
      return 0.0f;
    IntVec3 start = target.Thing.Map == map ? target.Thing.Position : target.Thing.PositionOnAnotherThingMap(searcher.Thing);
    int num1 = GenRadial.NumCellsInRadius(verb.verbProps.ai_AvoidFriendlyFireRadius);
    float num2 = 0.0f;
    ModCompat.AsAboveSoBelow.TargetBand? targetBand1 = ModCompat.AsAboveSoBelow.GetTargetBand(searcher.Thing);
    ModCompat.AsAboveSoBelow.TargetBand? targetBand2 = ModCompat.AsAboveSoBelow.GetTargetBand(target.Thing);
    for (int index1 = 0; index1 < num1; ++index1)
    {
      IntVec3 end = IntVec3.op_Addition(start, GenRadial.RadialPattern[index1]);
      if (GenGrid.InBounds(end, map))
      {
        bool flag = true;
        List<Thing> thingList = GridsUtility.GetThingList(end, map);
        for (int index2 = 0; index2 < thingList.Count; ++index2)
        {
          if (thingList[index2] is IAttackTarget && thingList[index2] != target)
          {
            if (flag)
            {
              if (GenSightOnVehicle.LineOfSight(start, end, map, targetBand1, targetBand2, true))
                flag = false;
              else
                break;
            }
            float num3 = thingList[index2] != searcher ? (!(thingList[index2] is Pawn) ? 10f : (thingList[index2].def.race.Animal ? 7f : 18f)) : 40f;
            if (GenHostility.HostileTo(searcher.Thing, thingList[index2]))
              num2 += num3 * 0.6f;
            else
              num2 -= num3;
          }
        }
      }
    }
    return num2;
  }

  private static float FriendlyFireConeTargetScoreOffset(
    IAttackTarget target,
    IAttackTargetSearcher searcher,
    Verb verb)
  {
    Pawn pawn = searcher.Thing as Pawn;
    if (pawn == null || pawn.RaceProps.intelligence < 1 || pawn.RaceProps.IsMechanoid || !(verb is Verb_Shoot verbShoot))
      return 0.0f;
    ThingDef defaultProjectile = ((Verb) verbShoot).verbProps.defaultProjectile;
    if (defaultProjectile == null || defaultProjectile.projectile.flyOverhead)
      return 0.0f;
    ShotReport report = ShotReport.HitReportFor((Thing) pawn, verb, LocalTargetInfo.op_Implicit((Thing) target));
    double forcedMissRadius = (double) verb.verbProps.ForcedMissRadius;
    ShootLine shootLine1 = ((ShotReport) ref report).ShootLine;
    IntVec3 dest1 = ((ShootLine) ref shootLine1).Dest;
    ShootLine shootLine2 = ((ShotReport) ref report).ShootLine;
    IntVec3 source1 = ((ShootLine) ref shootLine2).Source;
    IntVec3 intVec3 = IntVec3.op_Subtraction(dest1, source1);
    float num1 = Mathf.Max(VerbUtility.CalculateAdjustedForcedMiss((float) forcedMissRadius, intVec3), 1.5f);
    ShootLine shootLine3 = ((ShotReport) ref report).ShootLine;
    IEnumerable<IntVec3> intVec3s = GenRadial.RadialCellsAround(((ShootLine) ref shootLine3).Dest, num1, true).Select<IntVec3, ShootLine>((Func<IntVec3, ShootLine>) (dest =>
    {
      ShootLine shootLine4 = ((ShotReport) ref report).ShootLine;
      return new ShootLine(((ShootLine) ref shootLine4).Source, dest);
    })).SelectMany<ShootLine, IntVec3>((Func<ShootLine, IEnumerable<IntVec3>>) (line => GenCollection.Concat<IntVec3>(((ShootLine) ref line).Points(), ((ShootLine) ref line).Dest).TakeWhile<IntVec3>(new Func<IntVec3, bool>(func)))).Distinct<IntVec3>();
    float num2 = 0.0f;
    foreach (IntVec3 original in intVec3s)
    {
      shootLine3 = ((ShotReport) ref report).ShootLine;
      IntVec3 source2 = ((ShootLine) ref shootLine3).Source;
      float num3 = VerbUtility.InterceptChanceFactorFromDistance(((IntVec3) ref source2).ToVector3Shifted(), original);
      if ((double) num3 > 0.0)
      {
        IEnumerable<Thing> first = searcher.Thing.Map.thingGrid.ThingsAt(original.ToThingMapCoord(searcher.Thing));
        if (searcher.Thing.Map != target.Thing.Map)
          first = first.Concat<Thing>(target.Thing.Map.thingGrid.ThingsAt(original.ToThingMapCoord(target.Thing)));
        foreach (Thing thing in first)
        {
          if (thing is IAttackTarget && thing != target)
          {
            float num4 = (thing != searcher ? (!(thing is Pawn) ? 10f : (thing.def.race.Animal ? 7f : 18f)) : 40f) * num3;
            float num5 = !GenHostility.HostileTo(searcher.Thing, thing) ? num4 * -1f : num4 * 0.6f;
            num2 += num5;
          }
        }
      }
    }
    return num2;

    bool func(IntVec3 pos) => pos.CanBeSeenOverOnVehicle(((Thing) pawn).BaseMap());
  }

  public static bool CanSee(this Thing seer, Thing target, Func<IntVec3, bool> validator = null)
  {
    if (seer.Map == target.Map)
      return AttackTargetFinder.CanSee(seer, target, validator);
    IntVec3 onBaseMapSpawned1 = VehicleMapUtility.get_PositionOnBaseMapSpawned(seer);
    IntVec3 onBaseMapSpawned2 = VehicleMapUtility.get_PositionOnBaseMapSpawned(target);
    ModCompat.AsAboveSoBelow.TargetBand? targetBand1 = ModCompat.AsAboveSoBelow.GetTargetBand(seer);
    ModCompat.AsAboveSoBelow.TargetBand? targetBand2 = ModCompat.AsAboveSoBelow.GetTargetBand(target);
    Map map = seer.BaseMap();
    if (!GenGrid.InBounds(onBaseMapSpawned1, map) || !GenGrid.InBounds(onBaseMapSpawned2, map))
      return false;
    AttackTargetFinderOnVehicle.tempDestList.Clear();
    ShootLeanUtilityOnVehicle.CalcShootableCellsOf(AttackTargetFinderOnVehicle.tempDestList, target, onBaseMapSpawned1, targetBand1, targetBand2);
    for (int index = 0; index < AttackTargetFinderOnVehicle.tempDestList.Count; ++index)
    {
      if (GenSightOnVehicle.LineOfSight(onBaseMapSpawned1, AttackTargetFinderOnVehicle.tempDestList[index].ToThingBaseMapCoord(target), map, targetBand1, targetBand2, true, validator))
        return true;
    }
    ShootLeanUtilityOnVehicle.LeanShootingSourcesFromTo(seer.Position, onBaseMapSpawned2, seer.Map, AttackTargetFinderOnVehicle.tempSourceList, targetBand1, targetBand2);
    for (int index1 = 0; index1 < AttackTargetFinderOnVehicle.tempSourceList.Count; ++index1)
    {
      for (int index2 = 0; index2 < AttackTargetFinderOnVehicle.tempDestList.Count; ++index2)
      {
        if (GenSightOnVehicle.LineOfSight(AttackTargetFinderOnVehicle.tempSourceList[index1].ToThingBaseMapCoord(seer), AttackTargetFinderOnVehicle.tempDestList[index2].ToThingBaseMapCoord(target), map, targetBand1, targetBand2, true, validator))
          return true;
      }
    }
    return false;
  }

  public static void DebugDrawAttackTargetScores_Update()
  {
    if (!(Find.Selector.SingleSelectedThing is IAttackTargetSearcher singleSelectedThing) || singleSelectedThing.Thing.Map != Find.CurrentMap)
      return;
    Verb currentEffectiveVerb = singleSelectedThing.CurrentEffectiveVerb;
    if (currentEffectiveVerb == null)
      return;
    AttackTargetFinderOnVehicle.tmpTargets.Clear();
    List<Thing> thingList = singleSelectedThing.Thing.Map.listerThings.ThingsInGroup((ThingRequestGroup) 16 /*0x10*/);
    for (int index = 0; index < thingList.Count; ++index)
      AttackTargetFinderOnVehicle.tmpTargets.Add((IAttackTarget) thingList[index]);
    List<Pair<IAttackTarget, float>> shootingTargetsByScore = AttackTargetFinderOnVehicle.GetAvailableShootingTargetsByScore(AttackTargetFinderOnVehicle.tmpTargets, singleSelectedThing, currentEffectiveVerb);
    for (int index = 0; index < shootingTargetsByScore.Count; ++index)
      GenDraw.DrawLineBetween(singleSelectedThing.Thing.DrawPos, shootingTargetsByScore[index].First.Thing.DrawPos);
  }

  public static void DebugDrawAttackTargetScores_OnGUI()
  {
    if (!(Find.Selector.SingleSelectedThing is IAttackTargetSearcher singleSelectedThing) || singleSelectedThing.Thing.Map != Find.CurrentMap)
      return;
    Verb currentEffectiveVerb = singleSelectedThing.CurrentEffectiveVerb;
    if (currentEffectiveVerb == null)
      return;
    List<Thing> thingList = singleSelectedThing.Thing.Map.listerThings.ThingsInGroup((ThingRequestGroup) 16 /*0x10*/);
    Text.Anchor = (TextAnchor) 4;
    Text.Font = (GameFont) 0;
    for (int index = 0; index < thingList.Count; ++index)
    {
      Thing target = thingList[index];
      if (target != singleSelectedThing)
      {
        string str;
        Color red;
        if (!AttackTargetFinderOnVehicle.CanShootAtFromCurrentPosition((IAttackTarget) target, singleSelectedThing, currentEffectiveVerb))
        {
          str = "out of range";
          red = Color.red;
        }
        else
        {
          str = AttackTargetFinderOnVehicle.GetShootingTargetScore((IAttackTarget) target, singleSelectedThing, currentEffectiveVerb).ToString("F0");
          // ISSUE: explicit constructor call
          ((Color) ref red).\u002Ector(0.25f, 1f, 0.25f);
        }
        GenMapUI.DrawThingLabel(UI.MapToUIPosition(target.DrawPos), str, red);
      }
    }
    Text.Anchor = (TextAnchor) 0;
    Text.Font = (GameFont) 1;
  }

  public static void DebugDrawNonCombatantTimer_OnGUI()
  {
    List<Thing> thingList = Find.CurrentMap.listerThings.ThingsInGroup((ThingRequestGroup) 12);
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(new GameFont?((GameFont) 0), new TextAnchor?((TextAnchor) 4), new bool?(false));
    try
    {
      foreach (Thing thing in thingList)
      {
        if (thing is Pawn pawn && pawn.mindState != null)
        {
          int lastCombatantTick = pawn.mindState.lastCombatantTick;
          Vector2 uiPosition = UI.MapToUIPosition(((Thing) pawn).DrawPos);
          if (PawnUtility.IsCombatant(pawn))
          {
            int num = lastCombatantTick + 3600 - Find.TickManager.TicksGame;
            if (PawnUtility.IsPermanentCombatant(pawn) || num == 3600)
              GenMapUI.DrawThingLabel(uiPosition, "combatant", Color.red);
            else
              GenMapUI.DrawThingLabel(uiPosition, $"combatant {num}", Color.red);
          }
          else
            GenMapUI.DrawThingLabel(uiPosition, "non-combatant", Color.green);
        }
      }
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  public static bool IsAutoTargetable(IAttackTarget target)
  {
    CompCanBeDormant comp1 = ThingCompUtility.TryGetComp<CompCanBeDormant>(target.Thing);
    if (comp1 != null && !comp1.Awake)
      return false;
    CompInitiatable comp2 = ThingCompUtility.TryGetComp<CompInitiatable>(target.Thing);
    return comp2 == null || comp2.Initiated;
  }

  public static IAttackTarget CompareTarget(
    IAttackTarget target1,
    IAttackTarget target2,
    IAttackTargetSearcher searcher)
  {
    if (target1 == null)
      return target2;
    if (target2 == null)
      return target1;
    float targetPriorityFactor1 = target1.TargetPriorityFactor;
    float targetPriorityFactor2 = target2.TargetPriorityFactor;
    if ((double) targetPriorityFactor1 < (double) targetPriorityFactor2)
      return target2;
    if ((double) targetPriorityFactor1 > (double) targetPriorityFactor2)
      return target1;
    IntVec3 intVec3_1 = IntVec3.op_Subtraction(target1.Thing.Position, searcher.Thing.Position);
    int horizontalSquared1 = ((IntVec3) ref intVec3_1).LengthHorizontalSquared;
    IntVec3 intVec3_2 = IntVec3.op_Subtraction(VehicleMapUtility.get_PositionOnBaseMap(target2.Thing), VehicleMapUtility.get_PositionOnBaseMap(searcher.Thing));
    int horizontalSquared2 = ((IntVec3) ref intVec3_2).LengthHorizontalSquared;
    return horizontalSquared1 <= horizontalSquared2 ? target1 : target2;
  }
}
