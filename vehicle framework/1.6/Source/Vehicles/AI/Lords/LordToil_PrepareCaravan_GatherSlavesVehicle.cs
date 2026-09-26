// Decompiled with JetBrains decompiler
// Type: Vehicles.LordToil_PrepareCaravan_GatherSlavesVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

public class LordToil_PrepareCaravan_GatherSlavesVehicle : LordToil
{
  private IntVec3 meetingPoint;

  public LordToil_PrepareCaravan_GatherSlavesVehicle(IntVec3 meetingPoint)
  {
    this.meetingPoint = meetingPoint;
  }

  public virtual float? CustomWakeThreshold => new float?(0.5f);

  public virtual bool AllowRestingInBed => false;

  public virtual void UpdateAllDuties()
  {
    foreach (Pawn ownedPawn in this.lord.ownedPawns)
      ownedPawn.mindState.duty = !(ownedPawn is VehiclePawn) ? (ownedPawn.RaceProps.Animal || ownedPawn.IsColonist ? new PawnDuty(DutyDefOf.PrepareCaravan_Wait, LocalTargetInfo.op_Implicit(this.meetingPoint), -1f) : new PawnDuty(DutyDefOf_Vehicles.PrepareVehicleCaravan_SendSlavesToVehicle, LocalTargetInfo.op_Implicit(this.meetingPoint), -1f)) : new PawnDuty(DutyDefOf_Vehicles.PrepareVehicleCaravan_WaitVehicle);
  }

  public virtual void LordToilTick()
  {
    if (Find.TickManager.TicksGame % 100 != 0 || this.lord.ownedPawns.Where<Pawn>((Func<Pawn, bool>) (v => !(v is VehiclePawn))).ToList<Pawn>().NotNullAndAny<Pawn>((Predicate<Pawn>) (x => !x.IsColonist && x.RaceProps.Humanlike && ((Thing) x).Spawned)))
      return;
    this.lord.ReceiveMemo("AllSlavesGathered");
  }
}
