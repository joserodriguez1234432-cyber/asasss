// Decompiled with JetBrains decompiler
// Type: Vehicles.LordToil_PrepareCaravan_GatherAnimalsForVehicles
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

public class LordToil_PrepareCaravan_GatherAnimalsForVehicles(IntVec3 destinationPoint) : 
  LordToil_PrepareCaravan_GatherAnimals(destinationPoint),
  IDebugLordMeetingPoint
{
  public IntVec3 MeetingPoint => ((LordToil_PrepareCaravan_RopeAnimals) this).destinationPoint;

  public virtual void UpdateAllDuties()
  {
    for (int index = 0; index < ((LordToil) this).lord.ownedPawns.Count; ++index)
    {
      Pawn ownedPawn = ((LordToil) this).lord.ownedPawns[index];
      if (ownedPawn.IsColonist || AnimalPenUtility.NeedsToBeManagedByRope(ownedPawn))
      {
        ownedPawn.mindState.duty = ((LordToil_PrepareCaravan_RopeAnimals) this).MakeRopeDuty();
        ownedPawn.mindState.duty.ropeeLimit = ((LordToil_PrepareCaravan_RopeAnimals) this).ropeeLimit;
      }
      else
        ownedPawn.mindState.duty = !(ownedPawn is VehiclePawn) ? new PawnDuty(DutyDefOf.PrepareCaravan_Wait, LocalTargetInfo.op_Implicit(((LordToil_PrepareCaravan_RopeAnimals) this).destinationPoint), -1f) : new PawnDuty(DutyDefOf_Vehicles.PrepareVehicleCaravan_WaitVehicle);
    }
  }
}
