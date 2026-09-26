// Decompiled with JetBrains decompiler
// Type: Vehicles.GenGridVehicles
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using Verse;

#nullable disable
namespace Vehicles;

public static class GenGridVehicles
{
  public static bool Walkable(this IntVec3 cell, VehicleDef vehicleDef, Map map)
  {
    return map != null && MapComponentCache<VehiclePathingSystem>.GetComponent(map)[vehicleDef].VehiclePathGrid.Walkable(cell);
  }

  public static bool Walkable(
    this IntVec3 cell,
    VehicleDef vehicleDef,
    VehiclePathingSystem mapping)
  {
    return mapping[vehicleDef].VehiclePathGrid.Walkable(cell);
  }

  public static bool StandableUnknown(this IntVec3 cell, Pawn pawn, Map map)
  {
    return pawn is VehiclePawn vehicle ? cell.Standable(vehicle, map) : GenGrid.Standable(cell, map);
  }

  public static bool Standable(this IntVec3 cell, VehiclePawn vehicle, Map map)
  {
    if (!MapComponentCache<VehiclePathingSystem>.GetComponent(map)[vehicle.VehicleDef].VehiclePathGrid.Walkable(cell))
      return false;
    foreach (Thing thing in map.thingGrid.ThingsListAt(cell))
    {
      if (thing != vehicle && ((BuildableDef) thing.def).passability != null)
        return false;
    }
    return true;
  }

  public static bool Standable(this IntVec3 cell, VehicleDef vehicleDef, Map map)
  {
    if (!MapComponentCache<VehiclePathingSystem>.GetComponent(map)[vehicleDef].VehiclePathGrid.Walkable(cell))
      return false;
    foreach (Thing thing in map.thingGrid.ThingsListAt(cell))
    {
      if (((BuildableDef) thing.def).passability != null)
        return false;
    }
    return true;
  }

  public static bool ImpassableForVehicles(this ThingDef thingDef)
  {
    return ((BuildableDef) thingDef).passability == 2 || thingDef.IsFence || GenTypes.SameOrSubclassOf(thingDef.thingClass, typeof (Building_Door));
  }
}
