// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompGroundToVehicleAccess
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompGroundToVehicleAccess : CompVehicleEnterSpot
{
  protected override bool Available
  {
    get
    {
      VehiclePawnWithMap vehicle;
      if (!((Thing) this.parent).IsOnVehicleMapOf(out vehicle))
        return false;
      IntVec3 position = ((Thing) this.parent).Position;
      Rot4 rot4 = ((Thing) this.parent).Rotation;
      rot4 = ((Rot4) ref rot4).Opposite;
      IntVec3 asIntVec3 = ((Rot4) ref rot4).AsIntVec3;
      IntVec3 intVec3 = IntVec3.op_Addition(position, asIntVec3);
      if (!GenGrid.InBounds(intVec3, vehicle.VehicleMap))
        return false;
      if (vehicle.OutOfBoundsGrid[intVec3])
        return true;
      return vehicle.ExpandableGrid[intVec3] && vehicle.ImpassableCellGrid[intVec3];
    }
  }

  public override bool ShouldOffsetOnEdge => true;

  protected override TargetInfo AccessSpot
  {
    get
    {
      IntVec3 intVec3 = CrossMapReachabilityUtility.EnterVehiclePosition(TargetInfo.op_Implicit((Thing) this.parent));
      return !((IntVec3) ref intVec3).IsValid ? TargetInfo.Invalid : new TargetInfo(intVec3, VehicleMapUtility.get_GroundMap((Thing) this.parent), false);
    }
  }

  public override float MovePerTick(Pawn pawn) => 0.3f / pawn.TicksPerMoveCardinal;
}
