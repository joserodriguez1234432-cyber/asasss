// Decompiled with JetBrains decompiler
// Type: Vehicles.LordToil_PrepareCaravan_GatherDownedPawnsVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

public class LordToil_PrepareCaravan_GatherDownedPawnsVehicle : LordToil, IDebugLordMeetingPoint
{
  private readonly IntVec3 meetingPoint;

  public LordToil_PrepareCaravan_GatherDownedPawnsVehicle(IntVec3 meetingPoint)
  {
    this.meetingPoint = meetingPoint;
  }

  public IntVec3 MeetingPoint => this.meetingPoint;

  public virtual float? CustomWakeThreshold => new float?(0.5f);

  public virtual bool AllowRestingInBed => false;

  public virtual void UpdateAllDuties()
  {
    foreach (Pawn ownedPawn in this.lord.ownedPawns)
      ownedPawn.mindState.duty = !ownedPawn.IsColonist ? (!(ownedPawn is VehiclePawn) ? new PawnDuty(DutyDefOf.PrepareCaravan_Wait, LocalTargetInfo.op_Implicit(this.meetingPoint), -1f) : new PawnDuty(DutyDefOf_Vehicles.PrepareVehicleCaravan_WaitVehicle)) : new PawnDuty(DutyDefOf_Vehicles.PrepareVehicleCaravan_GatherDownedPawns);
  }

  public virtual void LordToilTick()
  {
    if (Find.TickManager.TicksGame % 100 != 0)
      return;
    List<Pawn> downedPawns = ((LordJob_FormAndSendCaravan) this.lord.LordJob).downedPawns;
    if (this.CheckMemo(downedPawns))
      return;
    foreach (VehiclePawn vehicle1 in ((LordJob_FormAndSendVehicles) this.lord.LordJob).vehicles)
    {
      VehiclePawn vehicle = vehicle1;
      downedPawns.RemoveAll((Predicate<Pawn>) (pawn => vehicle.AllPawnsAboard.Contains(pawn)));
    }
    this.CheckMemo(downedPawns);
  }

  private bool CheckMemo(List<Pawn> pawns)
  {
    if (!GenList.NullOrEmpty<Pawn>((IList<Pawn>) pawns))
      return false;
    this.lord.ReceiveMemo("AllDownedPawnsGathered");
    return true;
  }
}
