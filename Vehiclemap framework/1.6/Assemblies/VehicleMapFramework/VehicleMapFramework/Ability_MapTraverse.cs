// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Ability_MapTraverse
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Linq;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public abstract class Ability_MapTraverse : Ability
{
  protected Ability_MapTraverse()
  {
  }

  protected Ability_MapTraverse(Pawn pawn)
    : base(pawn)
  {
  }

  protected Ability_MapTraverse(Pawn pawn, Precept sourcePrecept)
    : base(pawn, sourcePrecept)
  {
  }

  protected Ability_MapTraverse(Pawn pawn, AbilityDef def)
    : base(pawn, def)
  {
  }

  protected Ability_MapTraverse(Pawn pawn, Precept sourcePrecept, AbilityDef def)
    : base(pawn, sourcePrecept, def)
  {
  }

  private bool ValidAOEAffectedTarget(Thing target)
  {
    return this.verb.targetParams.CanTarget(TargetInfo.op_Implicit(target), (ITargetingSource) null) && !GridsUtility.Fogged(target) && this.EffectComps.All<CompAbilityEffect>((Func<CompAbilityEffect, bool>) (t => t.Valid(LocalTargetInfo.op_Implicit(target), false)));
  }

  public virtual bool TryFindCastPosition(
    TargetInfo destination,
    out TargetInfo castSpot,
    out TargetInfo targSpot)
  {
    return this.TryFindCastPositionFromTo(new TargetInfo(((Thing) this.pawn).Position, ((Thing) this.pawn).Map, false), destination, out castSpot, out targSpot);
  }

  public virtual bool TryFindCastPositionFromTo(
    TargetInfo from,
    TargetInfo to,
    out TargetInfo castSpot,
    out TargetInfo targSpot,
    int districtID = -1)
  {
    castSpot = TargetInfo.Invalid;
    targSpot = TargetInfo.Invalid;
    Map map1 = ((TargetInfo) ref from).Map;
    Map map2 = ((TargetInfo) ref to).Map;
    if (map1 == null || map1 == map2)
      return false;
    VehiclePawnWithMap vehicle1;
    bool flag1 = map2.IsVehicleMapOf(out vehicle1);
    float minRange = this.verb.verbProps.EffectiveMinRange(true);
    float effectiveRange = this.verb.EffectiveRange;
    VehiclePawnWithMap vehicle2;
    IntVec3 intVec3_1;
    if (map1.IsVehicleMapOf(out vehicle2))
    {
      intVec3_1 = VehicleMapUtility.get_CellOnGroundMap(to).ClosestWalkableEdgeCell(vehicle2);
      if (((IntVec3) ref intVec3_1).IsValid & flag1)
      {
        IntVec3 vehicleMapCoord = intVec3_1.ToBaseMapCoord(vehicle2).ToVehicleMapCoord(vehicle1);
        CellRect validMapRect = vehicle1.ValidMapRect;
        IntVec3 intVec3_2 = IntVec3.op_Subtraction(((CellRect) ref validMapRect).ClosestCellTo(vehicleMapCoord), vehicleMapCoord);
        if ((double) ((IntVec3) ref intVec3_2).LengthHorizontalSquared > (double) effectiveRange * (double) effectiveRange)
          return false;
      }
    }
    else if (flag1)
    {
      intVec3_1 = VehicleMapUtility.get_CellOnGroundMap(from).ClosestWalkableEdgeCell(vehicle1);
      if (((IntVec3) ref intVec3_1).IsValid)
      {
        intVec3_1 = intVec3_1.ToBaseMapCoord(vehicle1);
        IntVec3 vehicleMapCoord = intVec3_1.ToVehicleMapCoord(vehicle1);
        CellRect validMapRect = vehicle1.ValidMapRect;
        IntVec3 intVec3_3 = IntVec3.op_Subtraction(((CellRect) ref validMapRect).ClosestCellTo(vehicleMapCoord), vehicleMapCoord);
        if ((double) ((IntVec3) ref intVec3_3).LengthHorizontalSquared > (double) effectiveRange * (double) effectiveRange)
          return false;
      }
    }
    else
      intVec3_1 = IntVec3.Invalid;
    if (!((IntVec3) ref intVec3_1).IsValid)
      return false;
    IntVec3 original = CrossMapRCellFinder.GoodDestNearFromTo(((TargetInfo) ref from).Cell, intVec3_1, this.pawn, map1, reserve: false, radius: effectiveRange);
    if (!((IntVec3) ref original).IsValid)
      return false;
    IntVec3 baseMapCoord = original.ToBaseMapCoord(map1);
    IntVec3 vehicleMapCoord1 = baseMapCoord.ToVehicleMapCoord(map2);
    TargetingParameters targetParams = this.verb.targetParams;
    bool canTargetLocations = targetParams.canTargetLocations;
    bool flag2 = targetParams.canTargetPawns || targetParams.canTargetBuildings || targetParams.canTargetItems || targetParams.canTargetPlants || targetParams.canTargetSelf || targetParams.canTargetFires;
    Map targetMap = TargetMapUtility.get_TargetMap((Thing) this.pawn);
    bool flag3 = targetMap != map2;
    if (flag3)
      TargetMapUtility.set_TargetMap((Thing) this.pawn, map2);
    CellRect to1 = flag1 ? vehicle1.ValidMapRect : map2.BoundsRect(1);
    IntRange indexRange;
    IntVec3[] intVec3Array = GenRadialDirectional.PatternFor(vehicleMapCoord1, to1, minRange, effectiveRange, out indexRange);
    for (int min = indexRange.min; min < indexRange.max; ++min)
    {
      IntVec3 intVec3_4 = IntVec3.op_Addition(vehicleMapCoord1, intVec3Array[min]);
      if (((CellRect) ref to1).Contains(intVec3_4))
      {
        if (canTargetLocations && this.verb.ValidateTarget(LocalTargetInfo.op_Implicit(intVec3_4), false) && this.verb.CanHitTargetFrom(baseMapCoord, LocalTargetInfo.op_Implicit(intVec3_4)))
        {
          if (districtID != -1)
          {
            District district = RegionAndRoomQuery.DistirctAtFast(intVec3_4, map2, (RegionType) 14);
            if ((district != null ? (district.ID == districtID ? 1 : 0) : 0) == 0)
              goto label_25;
          }
          castSpot = new TargetInfo(original, map1, false);
          targSpot = new TargetInfo(intVec3_4, map2, false);
          if (flag3)
            TargetMapUtility.set_TargetMap((Thing) this.pawn, targetMap);
          return true;
        }
label_25:
        if (flag2)
        {
          foreach (Thing target in map2.thingGrid.ThingsListAtFast(intVec3_4))
          {
            if (this.ValidAOEAffectedTarget(target))
            {
              if (districtID != -1)
              {
                District district = RegionAndRoomQuery.DistirctAtFast(intVec3_4, map2, (RegionType) 14);
                if ((district != null ? (district.ID == districtID ? 1 : 0) : 0) == 0)
                  continue;
              }
              castSpot = new TargetInfo(original, map1, false);
              targSpot = TargetInfo.op_Implicit(target);
              if (flag3)
                TargetMapUtility.set_TargetMap((Thing) this.pawn, targetMap);
              return true;
            }
          }
        }
      }
    }
    if (flag3)
      TargetMapUtility.set_TargetMap((Thing) this.pawn, targetMap);
    return false;
  }
}
