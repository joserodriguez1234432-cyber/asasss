// Decompiled with JetBrains decompiler
// Type: Vehicles.AsyncPathFindAction
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools.Performance;
using System;
using System.Threading;
using Verse;

#nullable disable
namespace Vehicles;

public class AsyncPathFindAction : AsyncAction
{
  private VehiclePawn vehicle;
  private CancellationToken token;

  public override bool IsValid
  {
    get
    {
      if (this.token.IsCancellationRequested)
        return false;
      VehiclePawn vehicle = this.vehicle;
      if (vehicle != null && ((Thing) vehicle).Spawned)
      {
        VehiclePathFollower vehiclePather = vehicle.vehiclePather;
        if (vehiclePather != null && vehiclePather.Moving)
          return vehiclePather.RequestStatus == VehiclePathFollower.PathRequestStatus.Calculating;
      }
      return false;
    }
  }

  public void Set(VehiclePawn vehicle, in CancellationToken token)
  {
    this.vehicle = vehicle;
    this.token = token;
  }

  public override void Invoke() => this.vehicle.vehiclePather.GeneratePath(this.token);

  public override void ReturnToPool()
  {
    this.vehicle = (VehiclePawn) null;
    AsyncPool<AsyncPathFindAction>.Return(this);
  }

  public override void ExceptionThrown(Exception ex) => this.vehicle.vehiclePather.PatherFailed();
}
