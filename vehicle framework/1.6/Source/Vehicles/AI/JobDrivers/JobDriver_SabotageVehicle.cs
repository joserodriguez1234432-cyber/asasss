// Decompiled with JetBrains decompiler
// Type: Vehicles.JobDriver_SabotageVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class JobDriver_SabotageVehicle : JobDriver_WorkVehicle
{
  private const int ChargeBaseTicks = 60;
  private const int MaxChargeTicks = 600;
  private const int ExplosionDamage = 100;
  private const float ArmorPenetration = 2f;

  protected override JobDef JobDef => JobDefOf_Vehicles.SabotageVehicle;

  protected override StatDef Stat => StatDefOf.ConstructionSpeed;

  protected override float TotalWork => this.Vehicle.GetStatValue(VehicleStatDefOf.WorkToSabotage);

  protected override void WorkComplete(Pawn actor)
  {
    this.AttachExplosive(actor);
    actor.jobs.EndCurrentJob((JobCondition) 2, true, true);
  }

  private void AttachExplosive(Pawn culprit)
  {
    this.Vehicle.vehiclePather.StopDead();
    VehiclePawn vehicle = this.Vehicle;
    IntVec3 position = ((Thing) this.Vehicle).Position;
    IntVec2 toIntVec2 = ((IntVec3) ref position).ToIntVec2;
    IntVec2 cell = VehicleStatHandler.AdjustFromVehiclePosition(vehicle, toIntVec2);
    IntVec2 size = ((BuildableDef) this.Vehicle.VehicleDef).Size;
    int radius = Mathf.Min(size.x, size.z);
    this.Vehicle.AddTimedExplosion(new TimedExplosion.Data(cell, Mathf.Min(60 * ((IntVec2) ref size).Area, 600), radius, DamageDefOf.Bomb, 100, 2f));
  }
}
