// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.LordToil_KidnapToMapVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System.Collections.Generic;
using Vehicles;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace VehicleMapFramework;

public class LordToil_KidnapToMapVehicle : LordToil_KidnapCover
{
  protected virtual DutyDef DutyDef => VMF_DefOf.VMF_Kidnap;

  protected virtual DutyDef DutyDefVehicle => DutyDefOf_Vehicles.VF_RangedAggressive;

  protected virtual DutyDef AssaultDutyDef => DutyDefOf.AssaultColony;

  public virtual void UpdateAllDuties()
  {
    List<Thing> thingList = (List<Thing>) null;
    for (int index = 0; index < ((LordToil) this).lord.ownedPawns.Count; ++index)
    {
      Pawn ownedPawn = ((LordToil) this).lord.ownedPawns[index];
      if (((Thing) ownedPawn).Spawned)
      {
        Thing thing = (Thing) null;
        if (ownedPawn is VehiclePawn vehiclePawn)
          ((Pawn) vehiclePawn).mindState.duty = new PawnDuty(this.DutyDefVehicle);
        else if (!((LordToil_DoOpportunisticTaskOrCover) this).cover || ownedPawn.RaceProps.Humanlike && ((LordToil_DoOpportunisticTaskOrCover) this).TryFindGoodOpportunisticTaskTarget(ownedPawn, ref thing, thingList) && !GenAI.InDangerousCombat(ownedPawn))
        {
          if (ownedPawn.mindState.duty == null || ownedPawn.mindState.duty.def != ((LordToil_DoOpportunisticTaskOrCover) this).DutyDef)
          {
            ownedPawn.mindState.duty = new PawnDuty(((LordToil_DoOpportunisticTaskOrCover) this).DutyDef);
            ownedPawn.jobs.EndCurrentJob((JobCondition) 16 /*0x10*/, true, true);
          }
          if (thingList == null)
            thingList = new List<Thing>();
          thingList.Add(thing);
        }
        else
          ownedPawn.mindState.duty = new PawnDuty(this.AssaultDutyDef);
      }
    }
  }

  protected virtual bool TryFindGoodOpportunisticTaskTarget(
    Pawn pawn,
    out Thing target,
    List<Thing> alreadyTakenTargets)
  {
    if (pawn.mindState.duty != null && pawn.mindState.duty.def == ((LordToil_DoOpportunisticTaskOrCover) this).DutyDef && pawn.carryTracker.CarriedThing is Pawn)
    {
      target = pawn.carryTracker.CarriedThing;
      return true;
    }
    Pawn victim;
    int num = KidnapToMapVehiclesAIUtility.TryFindGoodKidnapVictim(pawn, 8f, out victim, out TargetInfo _, alreadyTakenTargets) ? 1 : 0;
    target = (Thing) victim;
    return num != 0;
  }
}
