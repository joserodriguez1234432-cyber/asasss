// Decompiled with JetBrains decompiler
// Type: Vehicles.ThinkNode_ConditionalVehicleState
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class ThinkNode_ConditionalVehicleState : ThinkNode_Conditional
{
  private bool? canMove;
  private bool? canTakeoff;
  private bool? hasPassengers;

  public virtual ThinkNode DeepCopy(bool resolve = true)
  {
    ThinkNode_ConditionalVehicleState conditionalVehicleState = (ThinkNode_ConditionalVehicleState) base.DeepCopy(resolve);
    conditionalVehicleState.canMove = this.canMove;
    conditionalVehicleState.canTakeoff = this.canTakeoff;
    conditionalVehicleState.hasPassengers = this.hasPassengers;
    return (ThinkNode) conditionalVehicleState;
  }

  protected virtual bool Satisfied(Pawn pawn)
  {
    if (!(pawn is VehiclePawn vehiclePawn) || this.canTakeoff.HasValue && vehiclePawn.CompVehicleLauncher != null && vehiclePawn.CompVehicleLauncher.CanLaunchWithCargoCapacity(out string _) != this.canTakeoff.Value || this.hasPassengers.HasValue && vehiclePawn.AllPawnsAboard.Count > 0 != this.hasPassengers.Value)
      return false;
    return !this.canMove.HasValue || vehiclePawn.CanMove == this.canMove.Value;
  }
}
