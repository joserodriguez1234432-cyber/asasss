// Decompiled with JetBrains decompiler
// Type: Vehicles.JobGiver_PrepareVehicleCaravan_RopeAnimals
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

[Obsolete("Incomplete")]
public class JobGiver_PrepareVehicleCaravan_RopeAnimals : JobGiver_PrepareCaravan_CollectPawns
{
  protected virtual JobDef RopeJobDef => JobDefOf_Vehicles.RopeAnimalToVehicle;

  protected virtual Job TryGiveJob(Pawn pawn)
  {
    LocalTargetInfo focus = pawn.mindState.duty.focus;
    VehiclePawn pawn1 = ((LocalTargetInfo) ref focus).Pawn as VehiclePawn;
    Pawn ropee = !pawn.roping.IsRopingOthers ? this.FindAnimalNeedingRoping(pawn) : pawn.roping.Ropees[0];
    if (ropee == null || pawn1 == null)
      return (Job) null;
    IntVec3 intVec3 = pawn1.SurroundingCells.FirstOrDefault<IntVec3>((Func<IntVec3, bool>) (cell => ((IntVec3) ref cell).IsValid && GenGrid.WalkableBy(cell, ((Thing) ropee).Map, ropee) && GenGrid.WalkableBy(cell, ((Thing) pawn).Map, pawn)));
    Job job = JobMaker.MakeJob(((JobGiver_PrepareCaravan_RopePawns) this).RopeJobDef, LocalTargetInfo.op_Implicit((Thing) ropee), focus, LocalTargetInfo.op_Implicit(intVec3));
    job.lord = LordUtility.GetLord(pawn);
    ((JobGiver_PrepareCaravan_RopePawns) this).DecorateJob(job);
    return job;
  }

  protected virtual Pawn FindAnimalNeedingRoping(Pawn pawn)
  {
    foreach (Pawn ownedPawn in LordUtility.GetLord(pawn).ownedPawns)
    {
      if (((JobGiver_PrepareCaravan_RopePawns) this).AnimalNeedsGathering(pawn, ownedPawn))
        return ownedPawn;
    }
    return (Pawn) null;
  }
}
