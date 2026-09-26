// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleReachabilityImmediate
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public static class VehicleReachabilityImmediate
{
  public static bool CanReachImmediateVehicle(
    IntVec3 start,
    LocalTargetInfo target,
    Map map,
    VehicleDef vehicleDef,
    PathEndMode peMode)
  {
    if (!((LocalTargetInfo) ref target).IsValid)
      return false;
    target = TargetInfo.op_Explicit(GenPathVehicles.ResolvePathMode(vehicleDef, map, ((LocalTargetInfo) ref target).ToTargetInfo(map), ref peMode));
    if (!((LocalTargetInfo) ref target).HasThing || ((LocalTargetInfo) ref target).Thing.def.size.x == 1 && ((LocalTargetInfo) ref target).Thing.def.size.z == 1)
    {
      if (IntVec3.op_Equality(start, ((LocalTargetInfo) ref target).Cell))
        return true;
    }
    else if (GenAdj.IsInside(start, ((LocalTargetInfo) ref target).Thing))
      return true;
    return peMode == 2 && TouchPathEndModeUtilityVehicles.IsAdjacentOrInsideAndAllowedToTouch(start, target, map, vehicleDef);
  }

  public static bool CanReachImmediateVehicle(
    this VehiclePawn vehicle,
    LocalTargetInfo target,
    PathEndMode peMode)
  {
    return ((Thing) vehicle).Spawned && VehicleReachabilityImmediate.CanReachImmediateVehicle(((Thing) vehicle).Position, target, ((Thing) vehicle).Map, vehicle.VehicleDef, peMode);
  }

  public static bool CanReachImmediateNonLocalVehicle(
    this VehiclePawn vehicle,
    TargetInfo target,
    PathEndMode peMode)
  {
    return ((Thing) vehicle).Spawned && (((TargetInfo) ref target).Map == null || ((TargetInfo) ref target).Map == ((Thing) vehicle).Map) && vehicle.CanReachImmediateVehicle(TargetInfo.op_Explicit(target), peMode);
  }

  public static bool CanReachImmediateVehicle(
    IntVec3 start,
    CellRect rect,
    Map map,
    PathEndMode peMode,
    VehiclePawn vehicle)
  {
    IntVec3 intVec3 = ((CellRect) ref rect).ClosestCellTo(start);
    return VehicleReachabilityImmediate.CanReachImmediateVehicle(start, LocalTargetInfo.op_Implicit(intVec3), map, vehicle.VehicleDef, peMode);
  }
}
