// Decompiled with JetBrains decompiler
// Type: Vehicles.JobDriver_RepairVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class JobDriver_RepairVehicle : JobDriver_WorkVehicle
{
  public const float TicksForRepair = 60f;
  private const float VanillaSkillAmount = 0.05f;

  protected override JobDef JobDef => JobDefOf_Vehicles.RepairVehicle;

  protected override float TotalWork => 60f;

  protected override StatDef Stat => StatDefOf.ConstructionSpeed;

  protected override SkillDef Skill => SkillDefOf.Construction;

  protected override float SkillAmount => 0.05f;

  protected override void WorkComplete(Pawn actor)
  {
    if (!GenCollection.Any<VehicleComponent>(this.Vehicle.statHandler.ComponentsPrioritized, (Predicate<VehicleComponent>) (c => (double) c.HealthPercent < 1.0)))
    {
      MapComponentCache<ListerVehiclesRepairable>.GetComponent(((Thing) this.Vehicle).Map).NotifyVehicleRepaired(this.Vehicle);
      actor.records.Increment(RecordDefOf.ThingsRepaired);
      actor.jobs.EndCurrentJob((JobCondition) 2, true, true);
    }
    else
    {
      this.ResetWork();
      GenCollection.FirstOrDefault<VehicleComponent>(this.Vehicle.statHandler.ComponentsPrioritized, (Predicate<VehicleComponent>) (c => (double) c.HealthPercent < 1.0)).HealComponent(this.Vehicle.GetStatValue(VehicleStatDefOf.RepairRate));
      this.Vehicle.Transform.rotation = 0.0f;
      if (this.Vehicle.VehicleDef.graphicData.drawRotated)
        return;
      ((Thing) this.Vehicle).Rotation = ((BuildableDef) this.Vehicle.VehicleDef).defaultPlacingRot;
    }
  }
}
