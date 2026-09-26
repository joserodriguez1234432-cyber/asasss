// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.TargetingHelperOnVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public static class TargetingHelperOnVehicle
{
  private static readonly List<IAttackTarget> tmpTargets = new List<IAttackTarget>();

  public static IAttackTarget BestAttackTarget(
    VehicleTurret turret,
    TargetScanFlags flags,
    Predicate<Thing> validator = null,
    float minDist = 0.0f,
    float maxDist = 9999f,
    IntVec3 locus = default (IntVec3),
    float maxTravelRadiusFromLocus = 3.40282347E+38f,
    bool canTakeTargetsCloserThanEffectiveMinRange = true)
  {
    VehiclePawn searcherPawn = turret.vehicle;
    float minDistSquared = minDist * minDist;
    float num = maxTravelRadiusFromLocus + turret.MaxRange;
    float maxLocusDistSquared = num * num;
    IntVec3 searcherPosOnBaseMap = VehicleMapUtility.get_PositionOnBaseMap((Thing) searcherPawn);
    Map baseMap = ((Thing) searcherPawn).BaseMap();
    Func<IntVec3, bool> losValidator = (Func<IntVec3, bool>) null;
    if (((Enum) (object) flags).HasFlag((Enum) (object) (TargetScanFlags) 128 /*0x80*/))
      losValidator = (Func<IntVec3, bool>) (pos => GasUtility.AnyGas(pos, ((Thing) searcherPawn).BaseMap(), (GasType) 0));
    TargetingHelperOnVehicle.tmpTargets.Clear();
    HashSet<Map> source = ((Thing) searcherPawn).Map.BaseMapAndVehicleMaps(false);
    TargetingHelperOnVehicle.tmpTargets.AddRange(source.SelectMany<Map, IAttackTarget>((Func<Map, IEnumerable<IAttackTarget>>) (m => (IEnumerable<IAttackTarget>) m.attackTargetsCache.GetPotentialTargetsFor((IAttackTargetSearcher) searcherPawn))));
    bool flag = false;
    for (int index = 0; index < TargetingHelperOnVehicle.tmpTargets.Count; ++index)
    {
      IAttackTarget tmpTarget = TargetingHelperOnVehicle.tmpTargets[index];
      IntVec3 positionOnBaseMap = VehicleMapUtility.get_PositionOnBaseMap(tmpTarget.Thing);
      ShootLine shootLine;
      if (((IntVec3) ref positionOnBaseMap).InHorDistOf(searcherPosOnBaseMap, maxDist) && innerValidator(tmpTarget) && turret.TryFindShootLineFromTo(searcherPosOnBaseMap, new LocalTargetInfo(tmpTarget.Thing), ref shootLine))
      {
        flag = true;
        break;
      }
    }
    IAttackTarget iattackTarget;
    if (flag)
    {
      TargetingHelperOnVehicle.tmpTargets.RemoveAll((Predicate<IAttackTarget>) (x =>
      {
        IntVec3 positionOnBaseMap = VehicleMapUtility.get_PositionOnBaseMap(x.Thing);
        return !((IntVec3) ref positionOnBaseMap).InHorDistOf(searcherPosOnBaseMap, maxDist) || !innerValidator(x);
      }));
      iattackTarget = TargetingHelperOnVehicle.GetRandomShootingTargetByScore(turret, TargetingHelperOnVehicle.tmpTargets, searcherPawn);
    }
    else
    {
      ShootLine shootLine;
      Predicate<Thing> validator1 = (flags & 8) == null || (flags & 4) != null ? (Predicate<Thing>) (t => innerValidator((IAttackTarget) t)) : (Predicate<Thing>) (t => innerValidator((IAttackTarget) t) && turret.TryFindShootLineFromTo(VehicleMapUtility.get_PositionOnBaseMap((Thing) searcherPawn), new LocalTargetInfo(t), ref shootLine));
      iattackTarget = (IAttackTarget) GenClosestCrossMap.ClosestThing_Global(VehicleMapUtility.get_PositionOnBaseMap((Thing) searcherPawn), (IEnumerable) TargetingHelperOnVehicle.tmpTargets, maxDist, validator1);
    }
    TargetingHelperOnVehicle.tmpTargets.Clear();
    return iattackTarget;

    bool innerValidator(IAttackTarget t)
    {
      Thing thing = t.Thing;
      IntVec3 positionOnBaseMap = VehicleMapUtility.get_PositionOnBaseMap(thing);
      if (t == searcherPawn)
        return false;
      IntVec3 intVec3_1;
      if ((double) minDistSquared > 0.0)
      {
        intVec3_1 = IntVec3.op_Subtraction(searcherPosOnBaseMap, positionOnBaseMap);
        if ((double) ((IntVec3) ref intVec3_1).LengthHorizontalSquared < (double) minDistSquared)
          return false;
      }
      if (!canTakeTargetsCloserThanEffectiveMinRange)
      {
        float minRange = turret.MinRange;
        if ((double) minRange > 0.0)
        {
          intVec3_1 = IntVec3.op_Subtraction(VehicleMapUtility.get_PositionOnBaseMap((Thing) turret.vehicle), positionOnBaseMap);
          if ((double) ((IntVec3) ref intVec3_1).LengthHorizontalSquared < (double) minRange * (double) minRange)
            return false;
        }
      }
      if ((double) maxTravelRadiusFromLocus < 9999.0)
      {
        intVec3_1 = IntVec3.op_Subtraction(positionOnBaseMap, locus);
        if ((double) ((IntVec3) ref intVec3_1).LengthHorizontalSquared > (double) maxLocusDistSquared)
          return false;
      }
      if (!GenHostility.HostileTo((Thing) searcherPawn, thing) || validator != null && !validator(thing))
        return false;
      if ((flags & 3) != null)
      {
        if (losValidator != null && (!losValidator(searcherPosOnBaseMap) || !losValidator(positionOnBaseMap)))
          return false;
        if (!((Thing) searcherPawn).CanSee(thing, losValidator))
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
      if (((flags & 32 /*0x20*/) != null || (flags & 256 /*0x0100*/) != null) && t.ThreatDisabled((IAttackTargetSearcher) searcherPawn) || (flags & 256 /*0x0100*/) != null && !AttackTargetFinder.IsAutoTargetable(t) || (flags & 64 /*0x40*/) != null && !GenHostility.IsActiveThreatTo(t, ((Thing) searcherPawn).Faction, true, false) || (flags & 16 /*0x10*/) != null && FireUtility.IsBurning(thing))
        return false;
      IntVec2 size = thing.def.size;
      if (size.x == 1 && size.z == 1)
      {
        if (GridsUtility.Fogged(positionOnBaseMap, baseMap))
          return false;
      }
      else
      {
        bool flag = false;
        CellRect cellRect = GenAdj.OccupiedRect(thing);
        foreach (IntVec3 intVec3_2 in cellRect)
        {
          if (!GridsUtility.Fogged(intVec3_2, baseMap))
          {
            flag = true;
            break;
          }
        }
        if (!flag)
          return false;
      }
      return true;
    }
  }

  private static IAttackTarget GetRandomShootingTargetByScore(
    VehicleTurret turret,
    List<IAttackTarget> targets,
    VehiclePawn searcher)
  {
    Pair<IAttackTarget, float> pair;
    return !GenCollection.TryRandomElementByWeight<Pair<IAttackTarget, float>>((IEnumerable<Pair<IAttackTarget, float>>) TargetingHelperOnVehicle.GetAvailableShootingTargetsByScore(turret, targets, searcher), (Func<Pair<IAttackTarget, float>, float>) (x => x.Second), ref pair) ? (IAttackTarget) null : pair.First;
  }

  public static List<Pair<IAttackTarget, float>> GetAvailableShootingTargetsByScore(
    VehicleTurret turret,
    List<IAttackTarget> rawTargets,
    VehiclePawn searcher)
  {
    List<Pair<IAttackTarget, float>> shootingTargetsByScore = new List<Pair<IAttackTarget, float>>();
    List<float> floatList = new List<float>();
    List<bool> boolList = new List<bool>();
    if (rawTargets.Count == 0)
      return shootingTargetsByScore;
    floatList.Clear();
    boolList.Clear();
    float num1 = 0.0f;
    IAttackTarget iattackTarget = (IAttackTarget) null;
    for (int index = 0; index < rawTargets.Count; ++index)
    {
      floatList.Add(float.MinValue);
      boolList.Add(false);
      if (rawTargets[index] != searcher)
      {
        ShootLine shootLine;
        bool shootLineFromTo = turret.TryFindShootLineFromTo(VehicleMapUtility.get_PositionOnBaseMap((Thing) searcher), new LocalTargetInfo(VehicleMapUtility.get_PositionOnBaseMap(rawTargets[index].Thing)), ref shootLine);
        boolList[index] = shootLineFromTo;
        if (shootLineFromTo)
        {
          float shootingTargetScore = TargetingHelperOnVehicle.GetShootingTargetScore(rawTargets[index], (IAttackTargetSearcher) searcher);
          floatList[index] = shootingTargetScore;
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
        shootingTargetsByScore.Add(new Pair<IAttackTarget, float>(iattackTarget, 1f));
    }
    else
    {
      float num2 = num1 - 30f;
      for (int index = 0; index < rawTargets.Count; ++index)
      {
        if (rawTargets[index] != searcher && boolList[index])
        {
          float num3 = floatList[index];
          if ((double) num3 >= (double) num2)
          {
            float num4 = Mathf.InverseLerp(num1 - 30f, num1, num3);
            shootingTargetsByScore.Add(new Pair<IAttackTarget, float>(rawTargets[index], num4));
          }
        }
      }
    }
    return shootingTargetsByScore;
  }

  private static float GetShootingTargetScore(IAttackTarget target, IAttackTargetSearcher searcher)
  {
    IntVec3 intVec3 = IntVec3.op_Subtraction(VehicleMapUtility.get_PositionOnBaseMap(target.Thing), VehicleMapUtility.get_PositionOnBaseMap(searcher.Thing));
    float num1 = (float) (60.0 - (double) Mathf.Min(((IntVec3) ref intVec3).LengthHorizontal, 40f));
    if (LocalTargetInfo.op_Equality(target.TargetCurrentlyAimingAt, LocalTargetInfo.op_Implicit(searcher.Thing)))
      num1 += 10f;
    if (LocalTargetInfo.op_Equality(searcher.LastAttackedTarget, LocalTargetInfo.op_Implicit(target.Thing)) && Find.TickManager.TicksGame - searcher.LastAttackTargetTick <= 300)
      num1 += 40f;
    float num2 = num1 - CoverUtility.CalculateOverallBlockChance(LocalTargetInfo.op_Implicit(target.Thing.Position), searcher.Thing.PositionOnAnotherThingMap(target.Thing), target.Thing.Map) * 10f;
    if (target is Pawn pawn && pawn.RaceProps.Animal && ((Thing) pawn).Faction != null && !PawnUtility.IsFighting(pawn))
      num2 -= 50f;
    return num2 * target.TargetPriorityFactor;
  }
}
