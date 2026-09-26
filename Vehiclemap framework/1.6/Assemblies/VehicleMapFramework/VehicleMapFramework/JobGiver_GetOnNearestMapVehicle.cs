// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.JobGiver_GetOnNearestMapVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

[UsedImplicitly]
public class JobGiver_GetOnNearestMapVehicle : ThinkNode_JobGiver
{
  public bool allowEnemyVehicle;

  protected virtual Job TryGiveJob(Pawn pawn)
  {
    VehiclePawnWithMap vehicle;
    if (((Thing) pawn).IsOnVehicleMapOf(out vehicle) && (this.allowEnemyVehicle || ((Thing) vehicle).Faction == ((Thing) pawn).Faction))
      return (Job) null;
    Map groundMap = VehicleMapUtility.get_GroundMap((Thing) pawn);
    IntVec3 positionOnBaseMap = VehicleMapUtility.get_PositionOnBaseMap((Thing) pawn);
    IEnumerable<VehiclePawnWithMap> source = (IEnumerable<VehiclePawnWithMap>) VehiclePawnWithMapCache.AllVehiclesOn(groundMap);
    if (!this.allowEnemyVehicle)
      source = source.Where<VehiclePawnWithMap>((Func<VehiclePawnWithMap, bool>) (v => ((Thing) v).Faction == ((Thing) pawn).Faction));
    foreach (VehiclePawnWithMap vehiclePawnWithMap in (IEnumerable<VehiclePawnWithMap>) source.OrderBy<VehiclePawnWithMap, int>((Func<VehiclePawnWithMap, int>) (v =>
    {
      IntVec3 intVec3 = IntVec3.op_Subtraction(positionOnBaseMap, ((Thing) v).Position);
      return ((IntVec3) ref intVec3).LengthHorizontalSquared;
    })))
    {
      TargetInfo exitSpot;
      TargetInfo enterSpot;
      List<TraverseSpots> spotsQueue;
      if (CrossMapReachabilityUtility.CanReachToMap(((Thing) pawn).Position, ((Thing) pawn).Map, TraverseParms.For(pawn, (Danger) 3, (TraverseMode) 0, false, false, false, true), vehiclePawnWithMap.VehicleMap, out exitSpot, out enterSpot, out spotsQueue))
        return JobAcrossMapsUtility.GotoDestMapJob(pawn, new TargetInfo?(exitSpot), new TargetInfo?(enterSpot), spotsQueue);
    }
    return (Job) null;
  }
}
