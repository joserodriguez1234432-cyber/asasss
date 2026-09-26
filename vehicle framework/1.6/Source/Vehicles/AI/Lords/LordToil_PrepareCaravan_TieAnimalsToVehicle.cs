// Decompiled with JetBrains decompiler
// Type: Vehicles.LordToil_PrepareCaravan_TieAnimalsToVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

public class LordToil_PrepareCaravan_TieAnimalsToVehicle : LordToil, IDebugLordMeetingPoint
{
  protected IntVec3 meetingPoint;
  protected RotatingList<VehiclePawn> vehicles;

  public LordToil_PrepareCaravan_TieAnimalsToVehicle(IntVec3 meetingPoint)
  {
    this.meetingPoint = meetingPoint;
  }

  public IntVec3 MeetingPoint => this.meetingPoint;

  public VehiclePawn NextVehicle
  {
    get
    {
      if (GenList.NullOrEmpty<VehiclePawn>((IList<VehiclePawn>) this.vehicles))
        this.vehicles = this.lord.ownedPawns.Where<Pawn>((Func<Pawn, bool>) (pawn => pawn is VehiclePawn)).Cast<VehiclePawn>().ToRotatingList<VehiclePawn>();
      return this.vehicles.Next;
    }
  }

  public virtual void UpdateAllDuties()
  {
    for (int index = 0; index < this.lord.ownedPawns.Count; ++index)
    {
      Pawn ownedPawn = this.lord.ownedPawns[index];
      if (ownedPawn.IsColonist || AnimalPenUtility.NeedsToBeManagedByRope(ownedPawn))
      {
        ownedPawn.mindState.duty = new PawnDuty(DutyDefOf_Vehicles.PrepareVehicleCaravan_RopeAnimalsToVehicle, LocalTargetInfo.op_Implicit((Thing) this.NextVehicle), -1f);
        ownedPawn.mindState.duty.ropeeLimit = new int?(int.MaxValue);
      }
      else
        ownedPawn.mindState.duty = !(ownedPawn is VehiclePawn) ? new PawnDuty(DutyDefOf.PrepareCaravan_Wait, LocalTargetInfo.op_Implicit(this.meetingPoint), -1f) : new PawnDuty(DutyDefOf_Vehicles.PrepareVehicleCaravan_WaitVehicle);
    }
  }

  public virtual void LordToilTick()
  {
    if (Find.TickManager.TicksGame % 100 != 0)
      return;
    bool flag = true;
    foreach (Pawn ownedPawn in this.lord.ownedPawns)
    {
      if (AnimalPenUtility.NeedsToBeManagedByRope(ownedPawn))
      {
        Pawn_RopeTracker roping = ownedPawn.roping;
        Pawn pawn;
        if (roping == null)
        {
          pawn = (Pawn) null;
        }
        else
        {
          LocalTargetInfo ropedTo = roping.RopedTo;
          pawn = ((LocalTargetInfo) ref ropedTo).Pawn;
        }
        if (!(pawn is VehiclePawn vehiclePawn) || LordUtility.GetLord((Pawn) vehiclePawn) != this.lord)
        {
          flag = false;
          break;
        }
      }
    }
    if (!flag)
      return;
    this.lord.ReceiveMemo("AllAnimalsTiedDown");
  }
}
