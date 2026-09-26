// Decompiled with JetBrains decompiler
// Type: Vehicles.LordToil_AssaultColonyArmored
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

public class LordToil_AssaultColonyArmored : LordToil
{
  public virtual bool ForceHighStoryDanger => true;

  public virtual bool AllowSatisfyLongNeeds => false;

  public virtual void Init()
  {
    base.Init();
    LessonAutoActivator.TeachOpportunity(ConceptDefOf.Drafting, (OpportunityType) 2);
  }

  public virtual void UpdateAllDuties()
  {
    foreach (Pawn ownedPawn in this.lord.ownedPawns)
    {
      if (ownedPawn is VehiclePawn vehiclePawn)
        vehiclePawn.mindState.duty = new PawnDuty(DutyDefOf_Vehicles.VF_RangedAggressive);
      else
        ownedPawn.mindState.duty = new PawnDuty(DutyDefOf.Follow);
    }
  }
}
