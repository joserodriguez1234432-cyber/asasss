// Decompiled with JetBrains decompiler
// Type: Vehicles.JobDriver_Board
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System.Collections.Generic;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class JobDriver_Board : JobDriver
{
  public virtual bool TryMakePreToilReservations(bool errorOnFailed) => true;

  protected virtual IEnumerable<Toil> MakeNewToils()
  {
    JobDriver_Board jobDriverBoard = this;
    ToilFailConditions.FailOnDespawnedNullOrForbidden<JobDriver_Board>(jobDriverBoard, (TargetIndex) 1);
    ToilFailConditions.FailOnDowned<JobDriver_Board>(jobDriverBoard, (TargetIndex) 1);
    yield return Toils_Goto.GotoThing((TargetIndex) 1, (PathEndMode) 2, false);
    yield return Toils_Board.BoardVehicle(jobDriverBoard.pawn);
  }
}
