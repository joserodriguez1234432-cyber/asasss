// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.JobDriver_GotoAcrossMaps
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public class JobDriver_GotoAcrossMaps : JobDriverAcrossMaps
{
  public virtual bool TryMakePreToilReservations(bool errorOnFailed)
  {
    this.DestMap.pawnDestinationReservationManager.Reserve(this.pawn, this.job, ((LocalTargetInfo) ref this.job.targetA).Cell);
    return true;
  }

  protected override IEnumerable<Toil> MakeNewToils()
  {
    JobDriver_GotoAcrossMaps driverGotoAcrossMaps = this;
    // ISSUE: reference to a compiler-generated method
    foreach (Toil toil in driverGotoAcrossMaps.\u003C\u003En__0())
      yield return toil;
    foreach (Toil gotoTarget in driverGotoAcrossMaps.GotoTargetMap((TargetIndex) 1))
      yield return gotoTarget;
    if (((LocalTargetInfo) ref driverGotoAcrossMaps.job.targetA).IsValid)
    {
      LocalTargetInfo target = driverGotoAcrossMaps.job.GetTarget((TargetIndex) 2);
      Toil toil1 = Toils_Goto.GotoCell((TargetIndex) 1, (PathEndMode) 1);
      // ISSUE: reference to a compiler-generated method
      toil1.AddPreTickAction(new Action(driverGotoAcrossMaps.\u003CMakeNewToils\u003Eb__1_0));
      // ISSUE: reference to a compiler-generated method
      ToilFailConditions.FailOn<Toil>(toil1, new Func<bool>(driverGotoAcrossMaps.\u003CMakeNewToils\u003Eb__1_1));
      // ISSUE: reference to a compiler-generated method
      ToilFailConditions.FailOn<Toil>(toil1, new Func<bool>(driverGotoAcrossMaps.\u003CMakeNewToils\u003Eb__1_2));
      // ISSUE: reference to a compiler-generated method
      ToilFailConditions.FailOn<Toil>(toil1, new Func<bool>(driverGotoAcrossMaps.\u003CMakeNewToils\u003Eb__1_3));
      if (((LocalTargetInfo) ref target).IsValid)
      {
        // ISSUE: reference to a compiler-generated method
        toil1.tickAction += new Action(driverGotoAcrossMaps.\u003CMakeNewToils\u003Eb__1_7);
        toil1.handlingFacing = true;
      }
      // ISSUE: reference to a compiler-generated method
      toil1.AddFinishAction(new Action(driverGotoAcrossMaps.\u003CMakeNewToils\u003Eb__1_4));
      yield return toil1;
      Toil toil2 = ToilMaker.MakeToil(nameof (MakeNewToils));
      // ISSUE: reference to a compiler-generated method
      toil2.initAction = new Action(driverGotoAcrossMaps.\u003CMakeNewToils\u003Eb__1_5);
      toil2.defaultCompleteMode = (ToilCompleteMode) 1;
      yield return toil2;
    }
  }

  private void TryExitMap()
  {
    if (this.job.failIfCantJoinOrCreateCaravan && !CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(this.pawn))
      return;
    if (ModsConfig.BiotechActive)
      MechanitorUtility.Notify_PawnGotoLeftMap(this.pawn, ((Thing) this.pawn).BaseMap());
    if (ModsConfig.AnomalyActive && !MetalhorrorUtility.TryPawnExitMap(this.pawn))
      return;
    Pawn pawn = this.pawn;
    CellRect cellRect = CellRect.WholeMap(this.Map.BaseMap());
    Rot4 closestEdge = ((CellRect) ref cellRect).GetClosestEdge(((Thing) this.pawn).Position);
    pawn.ExitMap(true, closestEdge);
  }
}
