// Decompiled with JetBrains decompiler
// Type: Vehicles.TouchPathEndModeUtilityVehicles
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public static class TouchPathEndModeUtilityVehicles
{
  public static bool IsCornerTouchAllowed(
    int cornerX,
    int cornerZ,
    int adjCardinal1X,
    int adjCardinal1Z,
    int adjCardinal2X,
    int adjCardinal2Z,
    Map map,
    VehicleDef vehicleDef)
  {
    Building building = map.edificeGrid[new IntVec3(cornerX, 0, cornerZ)];
    if (building != null && TouchPathEndModeUtilityVehicles.MakesOccupiedCellsAlwaysReachableDiagonally(((Thing) building).def))
      return true;
    IntVec3 loc1;
    // ISSUE: explicit constructor call
    ((IntVec3) ref loc1).\u002Ector(adjCardinal1X, 0, adjCardinal1Z);
    IntVec3 loc2;
    // ISSUE: explicit constructor call
    ((IntVec3) ref loc2).\u002Ector(adjCardinal2X, 0, adjCardinal2Z);
    VehiclePathingSystem cachedMapComponent = map.GetCachedMapComponent<VehiclePathingSystem>();
    if (cachedMapComponent[vehicleDef].VehiclePathGrid.Walkable(loc1) && GridsUtility.GetDoor(loc1, map) == null)
      return true;
    return cachedMapComponent[vehicleDef].VehiclePathGrid.Walkable(loc2) && GridsUtility.GetDoor(loc2, map) == null;
  }

  public static bool MakesOccupiedCellsAlwaysReachableDiagonally(ThingDef def)
  {
    ThingDef thingDef = !def.IsFrame ? def : def.entityDefToBuild as ThingDef;
    return thingDef != null && thingDef.CanInteractThroughCorners;
  }

  public static bool IsAdjacentCornerAndNotAllowed(
    IntVec3 cell,
    IntVec3 BL,
    IntVec3 TL,
    IntVec3 TR,
    IntVec3 BR,
    Map map,
    VehicleDef vehicleDef)
  {
    if (IntVec3.op_Equality(cell, BL) && !TouchPathEndModeUtilityVehicles.IsCornerTouchAllowed(BL.x + 1, BL.z + 1, BL.x + 1, BL.z, BL.x, BL.z + 1, map, vehicleDef) || IntVec3.op_Equality(cell, TL) && !TouchPathEndModeUtilityVehicles.IsCornerTouchAllowed(TL.x + 1, TL.z - 1, TL.x + 1, TL.z, TL.x, TL.z - 1, map, vehicleDef) || IntVec3.op_Equality(cell, TR) && !TouchPathEndModeUtilityVehicles.IsCornerTouchAllowed(TR.x - 1, TR.z - 1, TR.x - 1, TR.z, TR.x, TR.z - 1, map, vehicleDef))
      return true;
    return IntVec3.op_Equality(cell, BR) && !TouchPathEndModeUtilityVehicles.IsCornerTouchAllowed(BR.x - 1, BR.z + 1, BR.x - 1, BR.z, BR.x, BR.z + 1, map, vehicleDef);
  }

  public static void AddAllowedAdjacentRegions(
    LocalTargetInfo dest,
    TraverseParms traverseParams,
    Map map,
    VehicleDef vehicleDef,
    List<VehicleRegion> regions)
  {
    VehiclePathingSystem cachedMapComponent = map.GetCachedMapComponent<VehiclePathingSystem>();
    IntVec3 BL;
    IntVec3 TL;
    IntVec3 TR;
    IntVec3 BR;
    GenAdj.GetAdjacentCorners(dest, ref BL, ref TL, ref TR, ref BR);
    if (!((LocalTargetInfo) ref dest).HasThing || ((LocalTargetInfo) ref dest).Thing.def.size.x == 1 && ((LocalTargetInfo) ref dest).Thing.def.size.z == 1)
    {
      IntVec3 cell1 = ((LocalTargetInfo) ref dest).Cell;
      for (int index = 0; index < 8; ++index)
      {
        IntVec3 cell2 = IntVec3.op_Addition(GenAdj.AdjacentCells[index], cell1);
        if (GenGrid.InBounds(cell2, map) && !TouchPathEndModeUtilityVehicles.IsAdjacentCornerAndNotAllowed(cell2, BL, TL, TR, BR, map, vehicleDef))
        {
          VehicleRegion vehicleRegion = VehicleRegionAndRoomQuery.RegionAt(cell2, cachedMapComponent, vehicleDef);
          if (vehicleRegion != null && vehicleRegion.Allows(traverseParams))
            regions.Add(vehicleRegion);
        }
      }
    }
    else
    {
      List<IntVec3> intVec3List = GenAdjFast.AdjacentCells8Way(dest);
      for (int index = 0; index < intVec3List.Count; ++index)
      {
        if (GenGrid.InBounds(intVec3List[index], map) && !TouchPathEndModeUtilityVehicles.IsAdjacentCornerAndNotAllowed(intVec3List[index], BL, TL, TR, BR, map, vehicleDef))
        {
          VehicleRegion vehicleRegion = VehicleRegionAndRoomQuery.RegionAt(intVec3List[index], cachedMapComponent, vehicleDef);
          if (vehicleRegion != null && vehicleRegion.Allows(traverseParams))
            regions.Add(vehicleRegion);
        }
      }
    }
  }

  public static bool IsAdjacentOrInsideAndAllowedToTouch(
    IntVec3 root,
    LocalTargetInfo target,
    Map map,
    VehicleDef vehicleDef)
  {
    IntVec3 BL;
    IntVec3 TL;
    IntVec3 TR;
    IntVec3 BR;
    GenAdj.GetAdjacentCorners(target, ref BL, ref TL, ref TR, ref BR);
    return GenAdj.AdjacentTo8WayOrInside(root, target) && !TouchPathEndModeUtilityVehicles.IsAdjacentCornerAndNotAllowed(root, BL, TL, TR, BR, map, vehicleDef);
  }
}
