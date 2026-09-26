// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Trigger_HighValueThingsAroundCrossMap
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using Verse;
using Verse.AI.Group;

#nullable disable
namespace VehicleMapFramework;

public class Trigger_HighValueThingsAroundCrossMap : Trigger
{
  private const int CheckInterval = 120;
  private const int MinTicksSinceDamage = 300;

  public virtual bool ActivateOn(Lord lord, TriggerSignal signal)
  {
    return signal.type == 1 && Find.TickManager.TicksGame % 120 == 0 && !TutorSystem.TutorialMode && Find.TickManager.TicksGame - lord.lastPawnHarmTick > 300 && (double) StealToMapVehiclesAIUtility.TotalMarketValueAround(lord.ownedPawns) > (double) StealAIUtility.StartStealingMarketValueThreshold(lord);
  }
}
