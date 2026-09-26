// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleRegionTypeUtility
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using Verse;

#nullable disable
namespace Vehicles;

public static class VehicleRegionTypeUtility
{
  public static RegionType GetExpectedRegionType(
    IntVec3 cell,
    VehiclePathingSystem mapping,
    VehicleDef vehicleDef)
  {
    if (!mapping[vehicleDef].VehiclePathGrid.Walkable(cell))
      return (RegionType) 0;
    return !VehicleRegionTypeUtility.VerifyCardinalCellSpace(cell, mapping, vehicleDef) ? (RegionType) 0 : (RegionType) 2;
  }

  private static bool VerifyCardinalCellSpace(
    IntVec3 cell,
    VehiclePathingSystem mapping,
    VehicleDef vehicleDef)
  {
    return vehicleDef.FullRectWalkable(mapping, cell, Rot4.North) || vehicleDef.FullRectWalkable(mapping, cell, Rot4.South) || vehicleDef.FullRectWalkable(mapping, cell, Rot4.East) || vehicleDef.FullRectWalkable(mapping, cell, Rot4.West);
  }
}
