// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.JobGiver_ExitWithMapVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public class JobGiver_ExitWithMapVehicle : ThinkNode_JobGiver
{
  private const int VehicleWaitForPawnTicks = 300;

  protected virtual Job TryGiveJob(Pawn pawn)
  {
    VehiclePawnWithMap vehicle;
    if (((Thing) pawn).IsOnVehicleMapOf(out vehicle) && ((Thing) vehicle).Faction == ((Thing) pawn).Faction)
      return (Job) null;
    Map groundMap = VehicleMapUtility.get_GroundMap((Thing) pawn);
    IntVec3 positionOnBaseMap = VehicleMapUtility.get_PositionOnBaseMap((Thing) pawn);
    foreach (VehiclePawnWithMap vehiclePawnWithMap in (IEnumerable<VehiclePawnWithMap>) VehiclePawnWithMapCache.AllVehiclesOn(groundMap).Where<VehiclePawnWithMap>((Func<VehiclePawnWithMap, bool>) (v => ((Thing) v).Faction == ((Thing) pawn).Faction)).OrderBy<VehiclePawnWithMap, int>((Func<VehiclePawnWithMap, int>) (v =>
    {
      IntVec3 intVec3 = IntVec3.op_Subtraction(positionOnBaseMap, ((Thing) v).Position);
      return ((IntVec3) ref intVec3).LengthHorizontalSquared;
    })))
    {
      TargetInfo exitSpot;
      TargetInfo enterSpot;
      List<TraverseSpots> spotsQueue;
      if (CrossMapReachabilityUtility.CanReachToMap(((Thing) pawn).Position, ((Thing) pawn).Map, TraverseParms.For(pawn, (Danger) 3, (TraverseMode) 0, false, false, false, true), vehiclePawnWithMap.VehicleMap, out exitSpot, out enterSpot, out spotsQueue))
      {
        if (((Pawn) vehiclePawnWithMap).CurJobDef != JobDefOf.Wait_MaintainPosture)
          PawnUtility.ForceWait((Pawn) vehiclePawnWithMap, 300, (Thing) null, true, false);
        return JobAcrossMapsUtility.GotoDestMapJob(pawn, new TargetInfo?(exitSpot), new TargetInfo?(enterSpot), spotsQueue);
      }
    }
    TargetInfo exitSpot2 = TargetInfo.Invalid;
    TargetInfo enterSpot2 = TargetInfo.Invalid;
    List<TraverseSpots> spotsQueue2 = (List<TraverseSpots>) null;
    IntVec3 spot;
    if (!TryFindBestExitSpot(out spot))
      return (Job) null;
    Job jobAcrossMaps = JobMaker.MakeJob(VMF_DefOf.VMF_GotoAcrossMaps, LocalTargetInfo.op_Implicit(spot)).SetSpotsToJobAcrossMaps(pawn, new TargetInfo?(exitSpot2), new TargetInfo?(enterSpot2), spotsQueue2);
    jobAcrossMaps.exitMapOnArrival = true;
    return jobAcrossMaps;

    bool TryFindBestExitSpot(out IntVec3 spot)
    {
      int num1 = 0;
      for (int index = 0; index < 30; ++index)
      {
        IntVec3 intVec3_1;
        int num2 = CellFinder.TryFindRandomCellNear(positionOnBaseMap, groundMap, num1, (Predicate<IntVec3>) null, ref intVec3_1, -1) ? 1 : 0;
        num1 += 4;
        if (num2 != 0)
        {
          int num3 = intVec3_1.x;
          IntVec3 intVec3_2;
          // ISSUE: explicit constructor call
          ((IntVec3) ref intVec3_2).\u002Ector(0, 0, intVec3_1.z);
          if (groundMap.Size.z - intVec3_1.z < num3)
          {
            num3 = groundMap.Size.z - intVec3_1.z;
            // ISSUE: explicit constructor call
            ((IntVec3) ref intVec3_2).\u002Ector(intVec3_1.x, 0, groundMap.Size.z - 1);
          }
          if (groundMap.Size.x - intVec3_1.x < num3)
          {
            num3 = groundMap.Size.x - intVec3_1.x;
            // ISSUE: explicit constructor call
            ((IntVec3) ref intVec3_2).\u002Ector(groundMap.Size.x - 1, 0, intVec3_1.z);
          }
          if (intVec3_1.z < num3)
          {
            // ISSUE: explicit constructor call
            ((IntVec3) ref intVec3_2).\u002Ector(intVec3_1.x, 0, 0);
          }
          if (GenGrid.Standable(intVec3_2, groundMap) && CrossMapReachabilityUtility.CanReach(((Thing) pawn).Map, ((Thing) pawn).Position, LocalTargetInfo.op_Implicit(intVec3_2), (PathEndMode) 1, TraverseParms.For(pawn, (Danger) 3, (TraverseMode) 0, false, false, false, false), groundMap, out exitSpot2, out enterSpot2, out spotsQueue2))
          {
            spot = intVec3_2;
            return true;
          }
        }
      }
      spot = ((Thing) pawn).Position;
      return false;
    }
  }
}
