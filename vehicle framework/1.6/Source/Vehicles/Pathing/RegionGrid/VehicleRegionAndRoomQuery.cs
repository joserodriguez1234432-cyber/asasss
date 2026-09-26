// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleRegionAndRoomQuery
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using Verse;

#nullable disable
namespace Vehicles;

public static class VehicleRegionAndRoomQuery
{
  public static VehicleRegion RegionAt(
    IntVec3 cell,
    Map map,
    VehicleDef vehicleDef,
    RegionType allowedRegionTypes = 14)
  {
    return VehicleRegionAndRoomQuery.RegionAt(cell, map.GetCachedMapComponent<VehiclePathingSystem>(), vehicleDef, allowedRegionTypes);
  }

  public static VehicleRegion RegionAt(
    IntVec3 cell,
    VehiclePathingSystem mapping,
    VehicleDef vehicleDef,
    RegionType allowedRegionTypes = 14)
  {
    if (!GenGrid.InBounds(cell, mapping.map))
      return (VehicleRegion) null;
    VehicleRegion validRegionAt = mapping[vehicleDef].VehicleRegionGrid.GetValidRegionAt(cell);
    return validRegionAt != null && (allowedRegionTypes & validRegionAt.type) == validRegionAt.type ? validRegionAt : (VehicleRegion) null;
  }

  public static VehicleRegion GetRegion(
    this Thing thing,
    VehicleDef vehicleDef,
    RegionType allowedRegiontypes = 14)
  {
    if (!thing.Spawned)
      return (VehicleRegion) null;
    return thing.Spawned ? VehicleRegionAndRoomQuery.RegionAt(thing.Position, thing.Map, vehicleDef, allowedRegiontypes) : (VehicleRegion) null;
  }

  public static VehicleRoom RoomAt(
    IntVec3 cell,
    Map map,
    VehicleDef vehicleDef,
    RegionType allowedRegionTypes = 14)
  {
    return VehicleRegionAndRoomQuery.RegionAt(cell, map, vehicleDef, allowedRegionTypes)?.Room;
  }

  public static VehicleRoom RoomAtFast(
    IntVec3 cell,
    Map map,
    VehicleDef vehicleDef,
    RegionType allowedRegionTypes = 14)
  {
    VehicleRegion validRegionAt = map.GetCachedMapComponent<VehiclePathingSystem>()[vehicleDef].VehicleRegionGrid?.GetValidRegionAt(cell);
    return validRegionAt != null && (validRegionAt.type & allowedRegionTypes) != null ? validRegionAt.Room : (VehicleRoom) null;
  }
}
