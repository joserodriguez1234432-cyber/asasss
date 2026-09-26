// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleGridsUtility
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using Verse;

#nullable disable
namespace Vehicles;

public static class VehicleGridsUtility
{
  [Obsolete("Call VehicleRegionAndRoomQuery.RegionAt instead.")]
  public static VehicleRegion GetRegion(
    this IntVec3 loc,
    Map map,
    VehicleDef vehicleDef,
    RegionType allowedRegionTypes = 14)
  {
    return VehicleRegionAndRoomQuery.RegionAt(loc, map, vehicleDef, allowedRegionTypes);
  }
}
