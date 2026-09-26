// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.KidnapToMapVehiclesAIUtility
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using VehicleMapFramework.VMF_HarmonyPatches;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public class KidnapToMapVehiclesAIUtility
{
  public static bool TryFindGoodKidnapVictim(
    Pawn kidnapper,
    float maxDist,
    out Pawn victim,
    out TargetInfo to,
    List<Thing> disallowed = null)
  {
    if (!kidnapper.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
    {
      victim = (Pawn) null;
      to = TargetInfo.Invalid;
      return false;
    }
    Patch_GenClosest_ClosestThingReachable.forceCrossMap = true;
    victim = (Pawn) GenClosest.ClosestThingReachable(((Thing) kidnapper).Position, ((Thing) kidnapper).Map, ThingRequest.ForGroup((ThingRequestGroup) 12), (PathEndMode) 1, TraverseParms.For(kidnapper, (Danger) 2, (TraverseMode) 2, false, false, false, true), maxDist, new Predicate<Thing>(Validator), (IEnumerable<Thing>) null, 0, -1, false, (RegionType) 14, false, false);
    Patch_GenClosest_ClosestThingReachable.forceCrossMap = false;
    if (victim == null)
    {
      to = TargetInfo.Invalid;
      return false;
    }
    if (!KidnapToMapVehiclesAIUtility.TryFindPlaceSpot(kidnapper, (Thing) victim, maxDist, out to))
      return false;
    TargetMapUtility.set_TargetInfo((Thing) kidnapper, to);
    return true;

    bool Validator(Thing t)
    {
      if (!(t is Pawn pawn) || ((Thing) pawn).Map.ParentFaction == ((Thing) kidnapper).Faction || !pawn.RaceProps.Humanlike || !pawn.Downed || ((Thing) pawn).Faction != Faction.OfPlayer || !FactionUtility.HostileTo(((Thing) pawn).Faction, ((Thing) kidnapper).Faction) || !ReservationUtility.CanReserve(kidnapper, LocalTargetInfo.op_Implicit((Thing) pawn), 1, -1, (ReservationLayerDef) null, false) || disallowed != null && disallowed.Contains((Thing) pawn))
        return false;
      return !ModsConfig.AnomalyActive || !pawn.IsSubhuman;
    }
  }

  public static bool TryFindPlaceSpot(Pawn kidnapper, Thing t, float maxDist, out TargetInfo spot)
  {
    IntVec3 positionOnBaseMap = VehicleMapUtility.get_PositionOnBaseMap(t);
    foreach (VehiclePawnWithMap vehiclePawnWithMap in (IEnumerable<VehiclePawnWithMap>) VehiclePawnWithMapCache.AllVehiclesOn(VehicleMapUtility.get_GroundMap((Thing) kidnapper)).Where<VehiclePawnWithMap>((Func<VehiclePawnWithMap, bool>) (v => ((Thing) v).Faction == ((Thing) kidnapper).Faction)).OrderBy<VehiclePawnWithMap, int>((Func<VehiclePawnWithMap, int>) (v =>
    {
      IntVec3 intVec3 = IntVec3.op_Subtraction(positionOnBaseMap, ((Thing) v).Position);
      return ((IntVec3) ref intVec3).LengthHorizontalSquared;
    })))
    {
      VehiclePawnWithMap vehicle = vehiclePawnWithMap;
      CellRect cellRect1 = CellRect.SingleCell(positionOnBaseMap.ToVehicleMapCoord(vehicle));
      CellRect cellRect2 = ((CellRect) ref cellRect1).ExpandedBy((int) maxDist);
      CellRect cellRect3 = ((CellRect) ref cellRect2).Encapsulate(vehicle.ValidMapRect);
      if (!((CellRect) ref cellRect3).IsEmpty)
      {
        TraverseParms parms = TraverseParms.For(kidnapper, (Danger) 3, (TraverseMode) 0, false, false, false, true);
        IntVec3 intVec3;
        if (CrossMapReachabilityUtility.CanReachToMap(t.Position, t.Map, parms, vehicle.VehicleMap) && CellFinder.TryFindRandomCellInsideWith(cellRect3, new Predicate<IntVec3>(Validator), ref intVec3))
        {
          spot = new TargetInfo(intVec3, vehicle.VehicleMap, false);
          return true;
        }
      }
      // ISSUE: variable of a compiler-generated type
      KidnapToMapVehiclesAIUtility.\u003C\u003Ec__DisplayClass1_0 cDisplayClass10;

      bool Validator(IntVec3 c)
      {
        Map vehicleMap = vehicle.VehicleMap;
        // ISSUE: reference to a compiler-generated method
        return !c.IsForbidden(kidnapper, vehicleMap) && ((BuildableDef) GridsUtility.GetTerrain(c, vehicleMap)).passability != 2 && StoreAcrossMapsUtility.NoStorageBlockersIn(c, vehicleMap, t) && kidnapper.CanReserveNew(LocalTargetInfo.op_Implicit(c), vehicleMap) && !FireUtility.ContainsStaticFire(c, vehicleMap) && !GenCollection.Any<Thing>(GridsUtility.GetThingList(c, vehicleMap), closure_0 ?? (closure_0 = new Predicate<Thing>(cDisplayClass10.\u003CTryFindPlaceSpot\u003Eb__3))) && CrossMapReachabilityUtility.CanReach(t.Map, t.Position, LocalTargetInfo.op_Implicit(c), (PathEndMode) 3, TraverseParms.For(kidnapper, (Danger) 3, (TraverseMode) 0, false, false, false, true), vehicleMap);
      }
    }
    spot = TargetInfo.Invalid;
    return false;
  }
}
