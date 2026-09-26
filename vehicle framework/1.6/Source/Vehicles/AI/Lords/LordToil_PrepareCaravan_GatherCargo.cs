// Decompiled with JetBrains decompiler
// Type: Vehicles.LordToil_PrepareCaravan_GatherCargo
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

public class LordToil_PrepareCaravan_GatherCargo : LordToil, IDebugLordMeetingPoint
{
  private IntVec3 meetingPoint;

  public LordToil_PrepareCaravan_GatherCargo(IntVec3 meetingPoint)
  {
    this.meetingPoint = meetingPoint;
  }

  public IntVec3 MeetingPoint => this.meetingPoint;

  public virtual float? CustomWakeThreshold => new float?(0.5f);

  public virtual bool AllowRestingInBed => false;

  public virtual void UpdateAllDuties()
  {
    foreach (Pawn ownedPawn in this.lord.ownedPawns)
      ownedPawn.mindState.duty = !ownedPawn.IsColonist ? (!(ownedPawn is VehiclePawn) ? new PawnDuty(DutyDefOf.PrepareCaravan_Wait, LocalTargetInfo.op_Implicit(this.meetingPoint), -1f) : new PawnDuty(DutyDefOf_Vehicles.PrepareVehicleCaravan_WaitVehicle)) : new PawnDuty(DutyDefOf_Vehicles.PrepareVehicleCaravan_GatherItems);
  }

  public virtual void LordToilTick()
  {
    base.LordToilTick();
    if (Find.TickManager.TicksGame % 120 != 0)
      return;
    bool flag = true;
    foreach (Pawn pawn in this.lord.ownedPawns.Where<Pawn>((Func<Pawn, bool>) (x => !x.Downed && !x.Dead)).ToList<Pawn>())
    {
      if (pawn.IsColonist && pawn.mindState.lastJobTag != 8)
      {
        flag = false;
        break;
      }
    }
    if (flag)
    {
      foreach (Pawn pawn in (IEnumerable<Pawn>) this.Map.mapPawns.AllPawnsSpawned)
      {
        if (pawn.CurJob != null && pawn.jobs.curDriver is JobDriver_PrepareVehicleCaravan_GatheringItems && pawn.CurJob.lord == this.lord)
        {
          flag = false;
          break;
        }
      }
    }
    if (!flag)
      return;
    this.lord.ReceiveMemo("AllItemsGathered");
  }
}
