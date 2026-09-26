// Decompiled with JetBrains decompiler
// Type: Vehicles.JobGiver_GotoTravelDestinationVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System;
using System.Linq;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class JobGiver_GotoTravelDestinationVehicle : JobGiver_GotoTravelDestination
{
  protected virtual Job TryGiveJob(Pawn pawn)
  {
    if (!(pawn is VehiclePawn vehicle))
      return (Job) null;
    IntVec3 cell1 = ((LocalTargetInfo) ref pawn.mindState.duty.focus).Cell;
    if (IntVec3.op_Equality(((Thing) vehicle).Position, cell1))
      return (Job) null;
    if (!vehicle.CanReachVehicle(LocalTargetInfo.op_Implicit(cell1), (PathEndMode) 2, PawnUtility.ResolveMaxDanger(pawn, this.maxDanger), (TraverseMode) 0))
      return (Job) null;
    Job job = new Job(JobDefOf.Goto, LocalTargetInfo.op_Implicit(cell1))
    {
      locomotionUrgency = (LocomotionUrgency) 3,
      expiryInterval = this.jobMaxDuration
    };
    if (vehicle.InhabitedCellsProjected(cell1, Rot8.Invalid).Any<IntVec3>((Func<IntVec3, bool>) (cell => ((Thing) pawn).Map.exitMapGrid.IsExitCell(cell))))
      job.exitMapOnArrival = true;
    return job;
  }
}
