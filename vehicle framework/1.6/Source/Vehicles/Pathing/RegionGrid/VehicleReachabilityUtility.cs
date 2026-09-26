// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleReachabilityUtility
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System.Collections.Generic;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public static class VehicleReachabilityUtility
{
  public static bool CanReachVehicle(
    this VehiclePawn vehicle,
    LocalTargetInfo dest,
    PathEndMode peMode,
    Danger maxDanger,
    TraverseMode mode = 0)
  {
    if (IntVec3.op_Equality(((LocalTargetInfo) ref dest).Cell, ((Thing) vehicle).Position))
      return true;
    return ((Thing) vehicle).Spawned && MapComponentCache<VehiclePathingSystem>.GetComponent(((Thing) vehicle).Map)[vehicle.VehicleDef].VehicleReachability.CanReachVehicle(((Thing) vehicle).Position, dest, peMode, TraverseParms.For((Pawn) vehicle, maxDanger, mode, false, false, false, true));
  }

  public static bool CanReachVehicleNonLocal(
    this VehiclePawn vehicle,
    TargetInfo dest,
    PathEndMode peMode,
    Danger maxDanger,
    TraverseMode mode = 0)
  {
    if (IntVec3.op_Equality(((TargetInfo) ref dest).Cell, ((Thing) vehicle).Position))
      return true;
    return ((Thing) vehicle).Spawned && MapComponentCache<VehiclePathingSystem>.GetComponent(((Thing) vehicle).Map)[vehicle.VehicleDef].VehicleReachability.CanReachVehicleNonLocal(((Thing) vehicle).Position, dest, peMode, TraverseParms.For((Pawn) vehicle, maxDanger, mode, false, false, false, true));
  }

  public static bool CanReachVehicleMapEdge(this VehiclePawn vehicle)
  {
    return ((Thing) vehicle).Spawned && MapComponentCache<VehiclePathingSystem>.GetComponent(((Thing) vehicle).Map)[vehicle.VehicleDef].VehicleReachability.CanReachMapEdge(((Thing) vehicle).Position, TraverseParms.For((Pawn) vehicle, (Danger) 3, (TraverseMode) 0, false, false, false, true));
  }

  public static void ClearCacheFor(VehiclePawn vehicle)
  {
    List<Map> maps = Find.Maps;
    for (int index = 0; index < maps.Count; ++index)
      maps[index].GetCachedMapComponent<VehiclePathingSystem>()[vehicle.VehicleDef].VehicleReachability.ClearCacheFor(vehicle);
  }
}
