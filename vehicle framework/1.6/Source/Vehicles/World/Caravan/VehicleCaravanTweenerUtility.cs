// Decompiled with JetBrains decompiler
// Type: Vehicles.World.VehicleCaravanTweenerUtility
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld.Planet;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.World;

public static class VehicleCaravanTweenerUtility
{
  private const float BaseRadius = 0.15f;
  private const float BaseDistToCollide = 0.2f;

  public static Vector3 PatherTweenedPosRoot(VehicleCaravan caravan)
  {
    WorldGrid worldGrid = Find.WorldGrid;
    if (!((WorldObject) caravan).Spawned)
      return worldGrid.GetTileCenter(((WorldObject) caravan).Tile);
    if (!caravan.vehiclePather.Moving)
      return worldGrid.GetTileCenter(((WorldObject) caravan).Tile);
    float num1 = caravan.vehiclePather.IsNextTilePassable() ? (float) (1.0 - (double) caravan.vehiclePather.nextTileCostLeft / (double) caravan.vehiclePather.nextTileCostTotal) : 0.0f;
    int num2 = !PlanetTile.op_Equality(caravan.vehiclePather.NextTile, ((WorldObject) caravan).Tile) || caravan.vehiclePather.previousTileForDrawingIfInDoubt == -1 ? PlanetTile.op_Implicit(((WorldObject) caravan).Tile) : caravan.vehiclePather.previousTileForDrawingIfInDoubt;
    return Vector3.op_Addition(Vector3.op_Multiply(worldGrid.GetTileCenter(caravan.vehiclePather.NextTile), num1), Vector3.op_Multiply(worldGrid.GetTileCenter(PlanetTile.op_Implicit(num2)), 1f - num1));
  }

  public static Vector3 CaravanCollisionPosOffsetFor(VehicleCaravan caravan)
  {
    if (!((WorldObject) caravan).Spawned)
      return Vector3.zero;
    bool flag = ((WorldObject) caravan).Spawned && caravan.vehiclePather.Moving;
    float num1 = 0.15f * Find.WorldGrid.AverageTileSize;
    if (!flag || PlanetTile.op_Equality(caravan.vehiclePather.NextTile, caravan.vehiclePather.Destination))
    {
      PlanetTile planetTile = flag ? caravan.vehiclePather.NextTile : ((WorldObject) caravan).Tile;
      int caravansCount;
      int caravansWithLowerIdCount;
      VehicleCaravanTweenerUtility.GetCaravansStandingAtOrAboutToStandAt(PlanetTile.op_Implicit(planetTile), out caravansCount, out caravansWithLowerIdCount, caravan);
      return caravansCount == 0 ? Vector3.zero : WorldRendererUtility.ProjectOnQuadTangentialToPlanet(Find.WorldGrid.GetTileCenter(planetTile), Vector2.op_Multiply(GenGeo.RegularPolygonVertexPosition(caravansCount, caravansWithLowerIdCount, 0.0f), num1));
    }
    if (!VehicleCaravanTweenerUtility.DrawPosCollides(caravan))
      return Vector3.zero;
    Rand.PushState();
    Rand.Seed = ((WorldObject) caravan).ID;
    float num2 = Rand.Range(0.0f, 360f);
    Rand.PopState();
    Vector2 vector2 = Vector2.op_Multiply(new Vector2(Mathf.Cos(num2), Mathf.Sin(num2)), num1);
    return WorldRendererUtility.ProjectOnQuadTangentialToPlanet(VehicleCaravanTweenerUtility.PatherTweenedPosRoot(caravan), vector2);
  }

  private static void GetCaravansStandingAtOrAboutToStandAt(
    int tile,
    out int caravansCount,
    out int caravansWithLowerIdCount,
    VehicleCaravan forCaravan)
  {
    caravansCount = 0;
    caravansWithLowerIdCount = 0;
    foreach (Caravan caravan in Find.WorldObjects.Caravans)
    {
      if (caravan is VehicleCaravan vehicleCaravan)
      {
        if (PlanetTile.op_Inequality(((WorldObject) vehicleCaravan).Tile, PlanetTile.op_Implicit(tile)))
        {
          if (!vehicleCaravan.vehiclePather.Moving || PlanetTile.op_Inequality(vehicleCaravan.vehiclePather.NextTile, vehicleCaravan.vehiclePather.Destination) || PlanetTile.op_Inequality(vehicleCaravan.vehiclePather.Destination, PlanetTile.op_Implicit(tile)))
            continue;
        }
        else if (vehicleCaravan.vehiclePather.Moving)
          continue;
        ++caravansCount;
        if (((WorldObject) caravan).ID < ((WorldObject) forCaravan).ID)
          ++caravansWithLowerIdCount;
      }
    }
  }

  private static bool DrawPosCollides(VehicleCaravan caravan)
  {
    Vector3 vector3 = VehicleCaravanTweenerUtility.PatherTweenedPosRoot(caravan);
    float num = Find.WorldGrid.AverageTileSize * 0.2f;
    foreach (Caravan caravan1 in Find.WorldObjects.Caravans)
    {
      if (caravan1 is VehicleCaravan caravan2 && caravan2 != caravan && (double) Vector3.Distance(vector3, VehicleCaravanTweenerUtility.PatherTweenedPosRoot(caravan2)) < (double) num)
        return true;
    }
    return false;
  }
}
