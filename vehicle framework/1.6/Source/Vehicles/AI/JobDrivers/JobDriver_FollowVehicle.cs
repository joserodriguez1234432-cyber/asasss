// Decompiled with JetBrains decompiler
// Type: Vehicles.JobDriver_FollowVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class JobDriver_FollowVehicle : JobDriver
{
  private VehiclePawn Vehicle
  {
    get
    {
      LocalTargetInfo target = this.job.GetTarget((TargetIndex) 1);
      return ((LocalTargetInfo) ref target).Thing as VehiclePawn;
    }
  }

  public virtual bool TryMakePreToilReservations(bool errorOnFailed) => true;

  public virtual void Notify_Starting()
  {
    base.Notify_Starting();
    if ((double) this.job.followRadius > 0.0)
      return;
    Log.Error($"Follow radius is <= 0. pawn=\"{Gen.ToStringSafe<Pawn>(this.pawn)}\" vehicle=\"{Gen.ToStringSafe<VehiclePawn>(this.Vehicle)}\"");
    this.job.followRadius = 10f;
  }

  public virtual bool IsContinuation(Job job)
  {
    return LocalTargetInfo.op_Equality(this.job.GetTarget((TargetIndex) 1), job.GetTarget((TargetIndex) 1));
  }

  protected virtual IEnumerable<Toil> MakeNewToils()
  {
    JobDriver_FollowVehicle driverFollowVehicle = this;
    ToilFailConditions.FailOnDespawnedOrNull<JobDriver_FollowVehicle>(driverFollowVehicle, (TargetIndex) 1);
    float radius = driverFollowVehicle.job.followRadius;
    if ((double) radius <= 0.0 || (double) radius <= (double) ((BuildableDef) driverFollowVehicle.Vehicle.VehicleDef).Size.z / 2.0)
      radius = (float) ((BuildableDef) driverFollowVehicle.Vehicle.VehicleDef).Size.z * 1.5f;
    yield return new Toil()
    {
      tickAction = (Action) (() =>
      {
        IntVec3 position = ((Thing) this.pawn).Position;
        if (((IntVec3) ref position).InHorDistOf(this.Vehicle.FollowerCell, radius) && RegionTraverser.WithinRegions(((Thing) this.pawn).Position, this.Vehicle.FollowerCell, this.Map, 2, TraverseParms.For(this.pawn, (Danger) 3, (TraverseMode) 0, false, false, false, true), (RegionType) 14))
          return;
        if (!ReachabilityUtility.CanReach(this.pawn, LocalTargetInfo.op_Implicit(this.Vehicle.FollowerCell), (PathEndMode) 2, (Danger) 3, false, false, (TraverseMode) 0))
        {
          this.EndJobWith((JobCondition) 4);
        }
        else
        {
          if (this.pawn.pather.Moving && !LocalTargetInfo.op_Inequality(this.pawn.pather.Destination, LocalTargetInfo.op_Implicit(this.Vehicle.FollowerCell)))
            return;
          this.pawn.pather.StartPath(LocalTargetInfo.op_Implicit(this.Vehicle.FollowerCell), (PathEndMode) 2);
        }
      }),
      defaultCompleteMode = (ToilCompleteMode) 5
    };
  }
}
