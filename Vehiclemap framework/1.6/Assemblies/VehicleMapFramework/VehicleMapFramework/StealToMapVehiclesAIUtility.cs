// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.StealToMapVehiclesAIUtility
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public static class StealToMapVehiclesAIUtility
{
  private const float MinMarketValueToTake = 320f;
  private static readonly List<Thing> tmpToSteal = new List<Thing>();

  public static bool TryFindBestItemToSteal(
    Pawn thief,
    float maxDist,
    out Thing item,
    out TargetInfo to,
    List<Thing> disallowed = null)
  {
    if (!thief.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
    {
      item = (Thing) null;
      to = TargetInfo.Invalid;
      return false;
    }
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    item = GenClosestCrossMap.ClosestThing_Regionwise_ReachablePrioritized(((Thing) thief).Position, ((Thing) thief).Map, ThingRequest.ForGroup((ThingRequestGroup) 21), (PathEndMode) 3, TraverseParms.For((TraverseMode) 2, (Danger) 2, false, false, false, true, false), maxDist, new Predicate<Thing>(Predicate), StealToMapVehiclesAIUtility.\u003C\u003EO.\u003C0\u003E__GetValue ?? (StealToMapVehiclesAIUtility.\u003C\u003EO.\u003C0\u003E__GetValue = new Func<Thing, float>(StealAIUtility.GetValue)), 15, 15);
    if (item != null && (double) StealAIUtility.GetValue(item) < 320.0)
      item = (Thing) null;
    if (item == null)
    {
      to = TargetInfo.Invalid;
      return false;
    }
    if (!KidnapToMapVehiclesAIUtility.TryFindPlaceSpot(thief, item, maxDist, out to))
      return false;
    TargetMapUtility.set_TargetInfo((Thing) thief, to);
    return true;

    bool Predicate(Thing t)
    {
      return t.Map.ParentFaction != ((Thing) thief).Faction && ReservationUtility.CanReserve(thief, LocalTargetInfo.op_Implicit(t), 1, -1, (ReservationLayerDef) null, false) && (disallowed == null || !disallowed.Contains(t)) && t.def.stealable && !FireUtility.IsBurning(t);
    }
  }

  public static float TotalMarketValueAround(List<Pawn> pawns)
  {
    float num = 0.0f;
    StealToMapVehiclesAIUtility.tmpToSteal.Clear();
    for (int index = 0; index < pawns.Count; ++index)
    {
      Thing thing;
      if (((Thing) pawns[index]).Spawned && StealToMapVehiclesAIUtility.TryFindBestItemToSteal(pawns[index], 7f, out thing, out TargetInfo _, StealToMapVehiclesAIUtility.tmpToSteal))
      {
        num += StealAIUtility.GetValue(thing);
        StealToMapVehiclesAIUtility.tmpToSteal.Add(thing);
      }
    }
    StealToMapVehiclesAIUtility.tmpToSteal.Clear();
    return num;
  }
}
