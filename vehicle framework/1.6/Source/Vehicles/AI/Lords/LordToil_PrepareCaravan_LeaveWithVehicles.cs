// Decompiled with JetBrains decompiler
// Type: Vehicles.LordToil_PrepareCaravan_LeaveWithVehicles
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

public class LordToil_PrepareCaravan_LeaveWithVehicles : LordToil, IDebugLordMeetingPoint
{
  private readonly IntVec3 exitSpot;

  public LordToil_PrepareCaravan_LeaveWithVehicles(IntVec3 exitSpot) => this.exitSpot = exitSpot;

  public IntVec3 MeetingPoint => this.exitSpot;

  public virtual bool AllowSatisfyLongNeeds => false;

  public virtual float? CustomWakeThreshold => new float?(0.5f);

  public virtual bool AllowRestingInBed => false;

  public virtual bool AllowSelfTend => false;

  public virtual void Init()
  {
    base.Init();
    foreach (Pawn ownedPawn in this.lord.ownedPawns)
      ownedPawn.roping?.BreakAllRopes();
  }

  public virtual void UpdateAllDuties()
  {
    RotatingList<VehiclePawn> rotatingList = this.lord.ownedPawns.Where<Pawn>((Func<Pawn, bool>) (p => p is VehiclePawn)).Cast<VehiclePawn>().ToRotatingList<VehiclePawn>();
    foreach (Pawn ownedPawn in this.lord.ownedPawns)
    {
      if (!ownedPawn.InVehicle())
      {
        if (ownedPawn is VehiclePawn vehiclePawn)
        {
          vehiclePawn.ignition.Drafted = true;
          ownedPawn.mindState.duty = new PawnDuty(DutyDefOf_Vehicles.TravelOrWaitVehicle, LocalTargetInfo.op_Implicit(this.exitSpot), -1f)
          {
            locomotion = (LocomotionUrgency) 3
          };
          ownedPawn.jobs.EndCurrentJob((JobCondition) 16 /*0x10*/, true, true);
        }
        else
        {
          VehiclePawn next = rotatingList.Next;
          ownedPawn.mindState.duty = new PawnDuty(DutyDefOf_Vehicles.FollowVehicle, LocalTargetInfo.op_Implicit((Thing) next), (float) ((BuildableDef) next.VehicleDef).Size.z * 1.5f)
          {
            locomotion = (LocomotionUrgency) 4
          };
        }
      }
    }
  }

  public virtual void LordToilTick()
  {
    if (Find.TickManager.TicksGame % 100 != 0)
      return;
    ExitMapUtility.CheckArrived(this.lord, this.lord.ownedPawns, this.exitSpot, "ReadyToExitMap", new Predicate<Pawn>(CheckPawnArrived), new Predicate<Pawn>(PawnCanMove));

    static bool CheckPawnArrived(Pawn pawn) => !pawn.InVehicle();

    static bool PawnCanMove(Pawn pawn)
    {
      return !(pawn is VehiclePawn vehiclePawn) || vehiclePawn.CanMoveFinal;
    }
  }
}
