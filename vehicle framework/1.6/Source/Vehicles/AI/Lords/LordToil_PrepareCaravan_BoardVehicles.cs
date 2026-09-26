// Decompiled with JetBrains decompiler
// Type: Vehicles.LordToil_PrepareCaravan_BoardVehicles
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

public class LordToil_PrepareCaravan_BoardVehicles : LordToil, IDebugLordMeetingPoint
{
  private readonly IntVec3 meetingPoint;

  public LordToil_PrepareCaravan_BoardVehicles(IntVec3 meetingPoint)
  {
    this.meetingPoint = meetingPoint;
  }

  public IntVec3 MeetingPoint => this.meetingPoint;

  public virtual float? CustomWakeThreshold => new float?(0.5f);

  public virtual bool AllowRestingInBed => false;

  public virtual void UpdateAllDuties()
  {
    foreach (Pawn ownedPawn in this.lord.ownedPawns)
    {
      if (!ownedPawn.InVehicle())
      {
        Pawn_MindState mindState = ownedPawn.mindState;
        PawnDuty pawnDuty;
        if (!(ownedPawn is VehiclePawn))
          pawnDuty = new PawnDuty(DutyDefOf_Vehicles.PrepareVehicleCaravan_BoardVehicle)
          {
            locomotion = (LocomotionUrgency) 3
          };
        else
          pawnDuty = new PawnDuty(DutyDefOf_Vehicles.PrepareVehicleCaravan_WaitVehicle);
        mindState.duty = pawnDuty;
      }
    }
  }

  public virtual void LordToilTick()
  {
    if (Find.TickManager.TicksGame % 200 != 0)
      return;
    int count = this.lord.ownedPawns.Count;
    while (--count >= 0)
    {
      Pawn ownedPawn = this.lord.ownedPawns[count];
      if (!ownedPawn.InVehicle())
      {
        AssignedSeat vehicleAssigned = (this.lord.LordJob as LordJob_FormAndSendVehicles).GetVehicleAssigned(ownedPawn);
        if (vehicleAssigned?.handler != null && !vehicleAssigned.Vehicle.AllPawnsAboard.Contains(ownedPawn))
          return;
      }
    }
    this.lord.ReceiveMemo("AllPawnsOnboard");
  }
}
