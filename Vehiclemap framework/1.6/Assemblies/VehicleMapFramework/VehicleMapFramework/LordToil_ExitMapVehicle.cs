// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.LordToil_ExitMapVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using Vehicles;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace VehicleMapFramework;

public class LordToil_ExitMapVehicle(
  LocomotionUrgency locomotion = 0,
  bool canDig = false,
  bool interruptCurrentJob = false) : LordToil_ExitMap(locomotion, canDig, interruptCurrentJob)
{
  public virtual DutyDef ExitDuty => VMF_DefOf.VMF_ExitMapWithMapVehicle;

  protected virtual DutyDef ExitDutyVehicle => VMF_DefOf.VMF_ExitMapBest;

  public virtual void UpdateAllDuties()
  {
    LordToilData_ExitMap data = this.Data;
    for (int index = 0; index < ((LordToil) this).lord.ownedPawns.Count; ++index)
    {
      Pawn ownedPawn = ((LordToil) this).lord.ownedPawns[index];
      PawnDuty pawnDuty = new PawnDuty(ownedPawn is VehiclePawn ? this.ExitDutyVehicle : base.ExitDuty)
      {
        locomotion = data.locomotion,
        canDig = data.canDig
      };
      ownedPawn.mindState.duty = pawnDuty;
      if (this.Data.interruptCurrentJob && ownedPawn.jobs.curJob != null)
        ownedPawn.jobs.EndCurrentJob((JobCondition) 16 /*0x10*/, true, true);
    }
  }
}
