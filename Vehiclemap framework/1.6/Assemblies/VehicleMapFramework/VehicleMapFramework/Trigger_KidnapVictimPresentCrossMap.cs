// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Trigger_KidnapVictimPresentCrossMap
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace VehicleMapFramework;

public class Trigger_KidnapVictimPresentCrossMap : Trigger
{
  private const int CheckInterval = 120;
  private const int MinTicksSinceDamage = 300;

  private TriggerData_PawnCycleInd Data => (TriggerData_PawnCycleInd) this.data;

  public Trigger_KidnapVictimPresentCrossMap()
  {
    this.data = (TriggerData) new TriggerData_PawnCycleInd();
  }

  public virtual bool ActivateOn(Lord lord, TriggerSignal signal)
  {
    if (signal.type == 1 && Find.TickManager.TicksGame % 120 == 0 && Find.TickManager.TicksGame - lord.lastPawnHarmTick > 300)
    {
      if (!(this.data is TriggerData_PawnCycleInd))
        this.data = (TriggerData) new TriggerData_PawnCycleInd();
      TriggerData_PawnCycleInd data = this.Data;
      ++data.pawnCycleInd;
      if (data.pawnCycleInd >= lord.ownedPawns.Count)
        data.pawnCycleInd = 0;
      if (GenCollection.Any<Pawn>(lord.ownedPawns))
      {
        Pawn ownedPawn = lord.ownedPawns[data.pawnCycleInd];
        if (((Thing) ownedPawn).Spawned && !ownedPawn.Downed && ownedPawn.MentalStateDef == null && KidnapToMapVehiclesAIUtility.TryFindGoodKidnapVictim(ownedPawn, 8f, out Pawn _, out TargetInfo _) && !GenAI.InDangerousCombat(ownedPawn))
          return true;
      }
    }
    return false;
  }
}
