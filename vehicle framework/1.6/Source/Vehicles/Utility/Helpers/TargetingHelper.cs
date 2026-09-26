// Decompiled with JetBrains decompiler
// Type: Vehicles.TargetingHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public static class TargetingHelper
{
  public static bool TryGetTarget(
    this VehicleTurret turret,
    out LocalTargetInfo targetInfo,
    TargetScanFlags? additionalFlags = null)
  {
    targetInfo = LocalTargetInfo.Invalid;
    TargetScanFlags flags = turret.def.targetScanFlags;
    if (additionalFlags.HasValue)
      flags = flags | additionalFlags.Value;
    Thing thing1 = (Thing) TargetingHelper.BestAttackTarget(turret, flags, (Predicate<Thing>) (thing => TargetingHelper.TargetMeetsRequirements(turret, LocalTargetInfo.op_Implicit(thing), out IntVec3 _)), locus: new IntVec3(), canTakeTargetsCloserThanEffectiveMinRange: false);
    if (thing1 == null)
      return false;
    targetInfo = new LocalTargetInfo(thing1);
    return true;
  }

  private static IAttackTarget BestAttackTarget(
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
    Func<IntVec3, bool> losValidator = (Func<IntVec3, bool>) null;
    if ((flags & 128 /*0x80*/) != null)
      losValidator = (Func<IntVec3, bool>) (pos => GasUtility.AnyGas(pos, ((Thing) searcherPawn).Map, (GasType) 0));
    Predicate<IAttackTarget> innerValidator = (Predicate<IAttackTarget>) (t =>
    {
      Thing thing = t.Thing;
      if (t == searcherPawn)
        return false;
      if ((double) minDistSquared > 0.0)
      {
        IntVec3 intVec3 = IntVec3.op_Subtraction(((Thing) searcherPawn).Position, thing.Position);
        if ((double) ((IntVec3) ref intVec3).LengthHorizontalSquared < (double) minDistSquared)
          return false;
      }
      IntVec3 intVec3_1;
      if (!canTakeTargetsCloserThanEffectiveMinRange)
      {
        float minRange = turret.MinRange;
        if ((double) minRange > 0.0)
        {
          intVec3_1 = IntVec3.op_Subtraction(((Thing) turret.vehicle).Position, thing.Position);
          if ((double) ((IntVec3) ref intVec3_1).LengthHorizontalSquared < (double) minRange * (double) minRange)
            return false;
        }
      }
      if ((double) maxTravelRadiusFromLocus < 9999.0)
      {
        intVec3_1 = IntVec3.op_Subtraction(thing.Position, locus);
        if ((double) ((IntVec3) ref intVec3_1).LengthHorizontalSquared > (double) maxLocusDistSquared)
          return false;
      }
      if (!GenHostility.HostileTo((Thing) searcherPawn, thing) || validator != null && !validator(thing))
        return false;
      if ((flags & 3) != null)
      {
        if (losValidator != null && (!losValidator(((Thing) searcherPawn).Position) || !losValidator(thing.Position)))
          return false;
        if (!AttackTargetFinder.CanSee((Thing) searcherPawn, thing, losValidator))
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
      if (((flags & 32 /*0x20*/) != null || (flags & 256 /*0x0100*/) != null) && t.ThreatDisabled((IAttackTargetSearcher) searcherPawn) || (flags & 256 /*0x0100*/) != null && !AttackTargetFinder.IsAutoTargetable(t) || (flags & 64 /*0x40*/) != null && !GenHostility.IsActiveThreatTo(t, ((Thing) searcherPawn).Faction, true, false))
        return false;
      Pawn pawn = t as Pawn;
      if ((flags & 16 /*0x10*/) != null && FireUtility.IsBurning(thing))
        return false;
      if (thing.def.size.x == 1 && thing.def.size.z == 1)
      {
        if (GridsUtility.Fogged(thing.Position, thing.Map))
          return false;
      }
      else
      {
        bool flag = false;
        CellRect cellRect = GenAdj.OccupiedRect(thing);
        foreach (IntVec3 intVec3_2 in cellRect)
        {
          if (!GridsUtility.Fogged(intVec3_2, thing.Map))
          {
            flag = true;
            break;
          }
        }
        if (!flag)
          return false;
      }
      return true;
    });
    List<IAttackTarget> list = ((Thing) searcherPawn).Map.attackTargetsCache.GetPotentialTargetsFor((IAttackTargetSearcher) searcherPawn).ToList<IAttackTarget>();
    bool flag1 = false;
    for (int index = 0; index < list.Count; ++index)
    {
      IAttackTarget iattackTarget = list[index];
      IntVec3 position = iattackTarget.Thing.Position;
      if (((IntVec3) ref position).InHorDistOf(((Thing) searcherPawn).Position, maxDist) && innerValidator(iattackTarget) && turret.TryFindShootLineFromTo(((Thing) searcherPawn).Position, new LocalTargetInfo(iattackTarget.Thing), out ShootLine _))
      {
        flag1 = true;
        break;
      }
    }
    IAttackTarget iattackTarget1;
    if (flag1)
    {
      list.RemoveAll((Predicate<IAttackTarget>) (x =>
      {
        IntVec3 position = x.Thing.Position;
        return !((IntVec3) ref position).InHorDistOf(((Thing) searcherPawn).Position, maxDist) || !innerValidator(x);
      }));
      iattackTarget1 = TargetingHelper.GetRandomShootingTargetByScore(turret, list, searcherPawn);
    }
    else
    {
      Predicate<Thing> predicate = (flags & 8) == null || (flags & 4) != null ? (Predicate<Thing>) (t => innerValidator((IAttackTarget) t)) : (Predicate<Thing>) (t => innerValidator((IAttackTarget) t) && turret.TryFindShootLineFromTo(((Thing) searcherPawn).Position, new LocalTargetInfo(t), out ShootLine _));
      iattackTarget1 = (IAttackTarget) GenClosest.ClosestThing_Global(((Thing) searcherPawn).Position, (IEnumerable) list, maxDist, predicate, (Func<Thing, float>) null, false);
    }
    list.Clear();
    return iattackTarget1;
  }

  private static IAttackTarget GetRandomShootingTargetByScore(
    VehicleTurret turret,
    List<IAttackTarget> targets,
    VehiclePawn searcher)
  {
    Pair<IAttackTarget, float> pair;
    return GenCollection.TryRandomElementByWeight<Pair<IAttackTarget, float>>((IEnumerable<Pair<IAttackTarget, float>>) TargetingHelper.GetAvailableShootingTargetsByScore(turret, targets, searcher), (Func<Pair<IAttackTarget, float>, float>) (x => x.Second), ref pair) ? pair.First : (IAttackTarget) null;
  }

  private static List<Pair<IAttackTarget, float>> GetAvailableShootingTargetsByScore(
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
        bool shootLineFromTo = turret.TryFindShootLineFromTo(((Thing) searcher).Position, new LocalTargetInfo(rawTargets[index].Thing), out ShootLine _);
        boolList[index] = shootLineFromTo;
        if (shootLineFromTo)
        {
          float shootingTargetScore = TargetingHelper.GetShootingTargetScore(rawTargets[index], (IAttackTargetSearcher) searcher);
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
    IntVec3 intVec3 = IntVec3.op_Subtraction(target.Thing.Position, searcher.Thing.Position);
    float num1 = (float) (60.0 - (double) Mathf.Min(((IntVec3) ref intVec3).LengthHorizontal, 40f));
    if (LocalTargetInfo.op_Equality(target.TargetCurrentlyAimingAt, LocalTargetInfo.op_Implicit(searcher.Thing)))
      num1 += 10f;
    if (LocalTargetInfo.op_Equality(searcher.LastAttackedTarget, LocalTargetInfo.op_Implicit(target.Thing)) && Find.TickManager.TicksGame - searcher.LastAttackTargetTick <= 300)
      num1 += 40f;
    float num2 = num1 - CoverUtility.CalculateOverallBlockChance(LocalTargetInfo.op_Implicit(target.Thing.Position), searcher.Thing.Position, searcher.Thing.Map) * 10f;
    if (target is Pawn pawn && pawn.RaceProps.Animal && ((Thing) pawn).Faction != null && !PawnUtility.IsFighting(pawn))
      num2 -= 50f;
    return num2 * target.TargetPriorityFactor;
  }

  public static bool TargetMeetsRequirements(
    VehicleTurret turret,
    LocalTargetInfo target,
    out IntVec3 goodDest)
  {
    return TargetingHelper.TargetMeetsRequirements(turret, ((Thing) turret.vehicle).Position, target, out goodDest);
  }

  public static bool TargetMeetsRequirements(
    VehicleTurret turret,
    IntVec3 root,
    LocalTargetInfo target,
    out IntVec3 goodDest)
  {
    goodDest = ((LocalTargetInfo) ref target).Cell;
    if (LocalTargetInfo.op_Equality(target, LocalTargetInfo.op_Implicit((Thing) turret.vehicle)))
      return false;
    Map map = ((Thing) turret.vehicle).Map;
    if (map == null || !turret.InRange(target) || !turret.AngleBetween(((LocalTargetInfo) ref target).CenterVector3))
      return false;
    if (turret.ProjectileDef.projectile.flyOverhead)
      return !GridsUtility.Roofed(root, map);
    if (((LocalTargetInfo) ref target).HasThing && !TargetingHelper.TargetValidator(turret, map, target))
      return false;
    TargetScanFlags targetScanFlags = turret.def.targetScanFlags;
    if (!((LocalTargetInfo) ref target).HasThing || (targetScanFlags & 3) != 3 && ((targetScanFlags & 1) != 1 || !(((LocalTargetInfo) ref target).Thing is Pawn)) && ((targetScanFlags & 2) != 2 || ((LocalTargetInfo) ref target).Thing is Pawn))
      return GenSight.LineOfSight(root, ((LocalTargetInfo) ref target).Cell, map);
    Thing thing1 = ((LocalTargetInfo) ref target).Thing;
    if (thing1 != null && (!thing1.Spawned || thing1.Destroyed) || !AttackTargetFinder.CanSee((Thing) turret.vehicle, ((LocalTargetInfo) ref target).Thing, (Func<IntVec3, bool>) (cell => TargetingHelper.LOSValidator(turret, map, target, cell))))
      return false;
    Thing thing2 = ((LocalTargetInfo) ref target).Thing;
    bool flag;
    if (thing2 != null)
    {
      ThingDef def = thing2.def;
      if (def != null)
      {
        IntVec2 size = def.size;
        if (size.x > 1 || size.z > 1)
        {
          flag = true;
          goto label_16;
        }
      }
    }
    flag = false;
label_16:
    if (flag)
    {
      CellRect cellRect = GenAdj.OccupiedRect(((LocalTargetInfo) ref target).Thing);
      foreach (IntVec3 intVec3 in cellRect)
      {
        if (IntVec3.op_Inequality(intVec3, ((LocalTargetInfo) ref target).Thing.Position) && GenSight.LineOfSightToEdges(root, intVec3, map, ((LocalTargetInfo) ref target).Thing.def.Fillage == 2, (Func<IntVec3, bool>) null))
        {
          goodDest = intVec3;
          return true;
        }
      }
    }
    return true;
  }

  private static bool LOSValidator(
    VehicleTurret turret,
    Map map,
    LocalTargetInfo target,
    IntVec3 cell)
  {
    TargetScanFlags targetScanFlags = turret.def.targetScanFlags;
    return ((targetScanFlags & 128 /*0x80*/) != 128 /*0x80*/ || TargetingHelper.LOSThroughGas(map, cell)) && ((targetScanFlags & 512 /*0x0200*/) != 512 /*0x0200*/ || TargetingHelper.LOSUnderRoof(map, cell));
  }

  private static bool TargetValidator(VehicleTurret turret, Map map, LocalTargetInfo target)
  {
    TargetScanFlags targetScanFlags = turret.def.targetScanFlags;
    return ((targetScanFlags & 32 /*0x20*/) != 32 /*0x20*/ || TargetingHelper.LOSHasThreat(turret.vehicle, ((LocalTargetInfo) ref target).Thing)) && ((targetScanFlags & 64 /*0x40*/) != 64 /*0x40*/ || TargetingHelper.LOSHasActiveThreat(turret.vehicle, ((LocalTargetInfo) ref target).Thing)) && ((targetScanFlags & 256 /*0x0100*/) != 256 /*0x0100*/ || TargetingHelper.LOSIsAutoTargetable(((LocalTargetInfo) ref target).Thing)) && ((targetScanFlags & 16 /*0x10*/) != 16 /*0x10*/ || TargetingHelper.LOSIsNonBurning(((LocalTargetInfo) ref target).Thing));
  }

  private static bool LOSThroughGas(Map map, IntVec3 cell)
  {
    return !GasUtility.AnyGas(cell, map, (GasType) 0);
  }

  private static bool LOSUnderRoof(Map map, IntVec3 cell)
  {
    RoofDef roof = GridsUtility.GetRoof(cell, map);
    return roof == null || !roof.isThickRoof;
  }

  private static bool LOSHasThreat(VehiclePawn vehicle, Thing thing)
  {
    return thing is IAttackTarget iattackTarget && !iattackTarget.ThreatDisabled((IAttackTargetSearcher) vehicle);
  }

  private static bool LOSHasActiveThreat(VehiclePawn vehicle, Thing thing)
  {
    return thing is IAttackTarget iattackTarget && GenHostility.IsActiveThreatTo(iattackTarget, ((Thing) vehicle).Faction, true, false);
  }

  private static bool LOSIsAutoTargetable(Thing thing)
  {
    return thing is IAttackTarget iattackTarget && AttackTargetFinder.IsAutoTargetable(iattackTarget);
  }

  private static bool LOSIsNonBurning(Thing thing)
  {
    return thing == null || !FireUtility.IsBurning(thing);
  }
}
