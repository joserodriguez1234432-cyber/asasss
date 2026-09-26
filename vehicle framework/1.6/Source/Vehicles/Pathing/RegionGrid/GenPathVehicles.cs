// Decompiled with JetBrains decompiler
// Type: Vehicles.GenPathVehicles
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public static class GenPathVehicles
{
  public static TargetInfo ResolvePathMode(
    VehicleDef vehicleDef,
    Map map,
    TargetInfo dest,
    ref PathEndMode peMode)
  {
    if (((TargetInfo) ref dest).HasThing && ((TargetInfo) ref dest).Thing.Spawned)
    {
      // ISSUE: cast to a reference type
      // ISSUE: explicit reference operation
      ^(sbyte&) ref peMode = (sbyte) 2;
      return dest;
    }
    // ISSUE: cast to a reference type
    // ISSUE: explicit reference operation
    if (^(byte&) ref peMode == (byte) 4)
    {
      if (!((TargetInfo) ref dest).HasThing)
        Log.Error($"Pathed to cell {dest.ToString()} with PathEndMode.InteractionCell.");
      // ISSUE: cast to a reference type
      // ISSUE: explicit reference operation
      ^(sbyte&) ref peMode = (sbyte) 1;
      return new TargetInfo(((TargetInfo) ref dest).Thing.InteractionCell, ((TargetInfo) ref dest).Thing.Map, false);
    }
    // ISSUE: cast to a reference type
    // ISSUE: explicit reference operation
    if (^(byte&) ref peMode == (byte) 3)
    {
      // ISSUE: cast to a reference type
      // ISSUE: explicit reference operation
      ^(sbyte&) ref peMode = (sbyte) GenPathVehicles.ResolveClosestTouchPathMode(vehicleDef, map, ((TargetInfo) ref dest).Cell);
    }
    return dest;
  }

  public static PathEndMode ResolveClosestTouchPathMode(
    VehicleDef vehicleDef,
    Map map,
    IntVec3 target)
  {
    return GenPathVehicles.ShouldNotEnterCell(vehicleDef, map, target) ? (PathEndMode) 2 : (PathEndMode) 1;
  }

  private static bool ShouldNotEnterCell(VehicleDef vehicleDef, Map map, IntVec3 dest)
  {
    return map.GetCachedMapComponent<VehiclePathingSystem>()[vehicleDef].VehiclePathGrid.PerceivedPathCostAt(dest) > 30 || !dest.Walkable(vehicleDef, map);
  }
}
