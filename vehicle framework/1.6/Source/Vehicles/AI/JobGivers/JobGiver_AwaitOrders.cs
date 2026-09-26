// Decompiled with JetBrains decompiler
// Type: Vehicles.JobGiver_AwaitOrders
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class JobGiver_AwaitOrders : ThinkNode_JobGiver
{
  private int overrideExpiryInterval = -1;

  public virtual ThinkNode DeepCopy(bool resolve = true)
  {
    JobGiver_AwaitOrders giverAwaitOrders = (JobGiver_AwaitOrders) base.DeepCopy(resolve);
    giverAwaitOrders.overrideExpiryInterval = this.overrideExpiryInterval;
    return (ThinkNode) giverAwaitOrders;
  }

  protected virtual Job TryGiveJob(Pawn pawn)
  {
    VehiclePawn vehiclePawn = pawn as VehiclePawn;
    if (vehiclePawn.vehiclePather.Moving)
      vehiclePawn.vehiclePather.StopDead();
    return new Job(JobDefOf_Vehicles.IdleVehicle, LocalTargetInfo.op_Implicit((Thing) vehiclePawn))
    {
      checkOverrideOnExpire = true,
      expiryInterval = this.overrideExpiryInterval > 0 ? this.overrideExpiryInterval : 180
    };
  }
}
