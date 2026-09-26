// Decompiled with JetBrains decompiler
// Type: Vehicles.JobDriver_Deploy
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class JobDriver_Deploy : JobDriver
{
  [UsedImplicitly]
  protected VehiclePawn Vehicle
  {
    get
    {
      LocalTargetInfo targetA = this.TargetA;
      return ((LocalTargetInfo) ref targetA).Thing as VehiclePawn;
    }
  }

  public virtual bool TryMakePreToilReservations(bool errorOnFailed)
  {
    CompVehicleTurrets compVehicleTurrets = this.Vehicle.CompVehicleTurrets;
    return compVehicleTurrets != null && compVehicleTurrets.CanDeploy;
  }

  protected virtual IEnumerable<Toil> MakeNewToils()
  {
    // ISSUE: reference to a compiler-generated field
    int num = this.\u003C\u003E1__state;
    JobDriver_Deploy jobDriverDeploy = this;
    if (num != 0)
    {
      if (num != 1)
        return false;
      // ISSUE: reference to a compiler-generated field
      this.\u003C\u003E1__state = -1;
      return false;
    }
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E1__state = -1;
    ToilFailConditions.FailOnDestroyedOrNull<JobDriver_Deploy>(jobDriverDeploy, (TargetIndex) 1);
    ToilFailConditions.FailOn<JobDriver_Deploy>(jobDriverDeploy, (Func<bool>) (() => !((Thing) this.Vehicle).Spawned));
    Toil deployToil = ToilMaker.MakeToil(nameof (MakeNewToils));
    deployToil.initAction = (Action) (() =>
    {
      this.Map.pawnDestinationReservationManager.Reserve((Pawn) this.Vehicle, this.job, ((Thing) this.Vehicle).Position);
      this.Vehicle.vehiclePather.StopDead();
      if (this.Vehicle.CompVehicleTurrets.Deployed)
        this.Vehicle.CompVehicleTurrets.FlagAllTurretsForAlignment();
      if (this.Vehicle.CompVehicleTurrets.Props.deployingSustainer == null)
        return;
      ToilEffects.PlaySustainerOrSound(deployToil, this.Vehicle.CompVehicleTurrets.Props.deployingSustainer, 1f);
    });
    deployToil.tickAction = (Action) (() =>
    {
      if (!this.Vehicle.CompVehicleTurrets.Deployed || this.Vehicle.CompVehicleTurrets.TurretsAligned)
        --this.Vehicle.CompVehicleTurrets.deployTicks;
      if (this.Vehicle.CompVehicleTurrets.deployTicks > 0)
        return;
      this.Vehicle.CompVehicleTurrets.ToggleDeployment();
      this.ReadyForNextToil();
    });
    ToilEffects.WithProgressBar(deployToil, (TargetIndex) 1, (Func<float>) (() => (float) (1.0 - (double) this.Vehicle.CompVehicleTurrets.deployTicks / (double) this.Vehicle.CompVehicleTurrets.DeployTicks)), false, -0.5f, false);
    deployToil.defaultCompleteMode = (ToilCompleteMode) 5;
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E2__current = deployToil;
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E1__state = 1;
    return true;
  }
}
