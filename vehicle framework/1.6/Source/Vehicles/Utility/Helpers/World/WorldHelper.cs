// Decompiled with JetBrains decompiler
// Type: Vehicles.World.WorldHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.World;

public static class WorldHelper
{
  private static readonly List<Thing> InventoryItems = new List<Thing>();
  private static readonly BFS<PlanetTile> WorldTileBfs = new BFS<PlanetTile>();
  private static readonly Dictionary<PlanetTile, float> TileWeights = new Dictionary<PlanetTile, float>();
  private static readonly List<PlanetTile> CandidateTiles = new List<PlanetTile>();

  public static List<Thing> AllInventoryItems(AerialVehicleInFlight aerialVehicle)
  {
    WorldHelper.InventoryItems.Clear();
    foreach (Pawn pawn in aerialVehicle.Vehicle.AllPawnsAboard)
      WorldHelper.InventoryItems.AddRange((IEnumerable<Thing>) pawn.inventory.innerContainer);
    WorldHelper.InventoryItems.AddRange((IEnumerable<Thing>) aerialVehicle.Vehicle.inventory.innerContainer);
    return WorldHelper.InventoryItems;
  }

  public static float RiverCostAt(int tile, VehiclePawn vehicle)
  {
    RiverDef river = GenCollection.MaxBy<SurfaceTile.RiverLink, float>((IEnumerable<SurfaceTile.RiverLink>) Find.WorldGrid[tile].Rivers, (Func<SurfaceTile.RiverLink, float>) (r => r.river.widthOnWorld)).river;
    return GenCollection.TryGetValue<RiverDef, float>((IReadOnlyDictionary<RiverDef, float>) vehicle.VehicleDef.properties.customRiverCosts, river, 1000f);
  }

  public static SurfaceTile.RiverLink BiggestRiverOnTile(List<SurfaceTile.RiverLink> list)
  {
    return GenCollection.MaxBy<SurfaceTile.RiverLink, float>((IEnumerable<SurfaceTile.RiverLink>) list, (Func<SurfaceTile.RiverLink, float>) (riverlink => ModSettingsHelper.RiverSizeWithMultiplier(riverlink.river)));
  }

  public static bool VehicleBiggerThanRiver(VehicleDef vehicleDef, RiverDef riverDef)
  {
    return !GenDictionary.NullOrEmpty<RiverDef, float>((Dictionary<RiverDef, float>) vehicleDef.properties.customRiverCosts) && (double) ModSettingsHelper.RiverSizeWithMultiplier(riverDef) / 2.0 < (double) ((BuildableDef) vehicleDef).Size.x;
  }

  public static float TryFindHeading(Vector3 source, Vector3 target)
  {
    return Find.WorldGrid.GetHeadingFromTo(source, target);
  }

  public static WorldObject WorldObjectAt(PlanetTile tile)
  {
    foreach (WorldObject allWorldObject in Find.WorldObjects.AllWorldObjects)
    {
      if (PlanetTile.op_Equality(allWorldObject.Tile, tile))
        return allWorldObject;
    }
    return (WorldObject) null;
  }

  public static (WorldObject sourceObject, WorldObject destObject) WorldObjectsAt(
    PlanetTile source,
    PlanetTile destination)
  {
    WorldObject worldObject1 = (WorldObject) null;
    WorldObject worldObject2 = (WorldObject) null;
    List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
    for (int index = 0; index < allWorldObjects.Count && (worldObject1 == null || worldObject2 == null); ++index)
    {
      WorldObject worldObject3 = allWorldObjects[index];
      if (PlanetTile.op_Equality(worldObject3.Tile, source))
        worldObject1 = worldObject3;
      if (PlanetTile.op_Equality(worldObject3.Tile, destination))
        worldObject2 = worldObject3;
    }
    return (worldObject1, worldObject2);
  }

  public static Vector3 GetTilePos(PlanetTile tile)
  {
    WorldObject worldObject = WorldHelper.WorldObjectAt(tile);
    return WorldHelper.GetTilePos(tile, worldObject, out bool _);
  }

  public static Vector3 GetTilePos(PlanetTile tile, out bool spaceObject)
  {
    WorldObject worldObject = WorldHelper.WorldObjectAt(tile);
    return WorldHelper.GetTilePos(tile, worldObject, out spaceObject);
  }

  public static Vector3 GetTilePos(PlanetTile tile, WorldObject worldObject, out bool spaceObject)
  {
    spaceObject = false;
    if (!((PlanetTile) ref tile).Valid)
      return Vector3.zero;
    Vector3 tilePos = Find.WorldGrid.GetTileCenter(tile);
    if (worldObject != null && ((Def) worldObject.def).HasModExtension<SpaceObjectDefModExtension>())
    {
      spaceObject = true;
      tilePos = worldObject.DrawPos;
    }
    return tilePos;
  }

  public static float GetTileDistance(PlanetTile source, PlanetTile destination)
  {
    (WorldObject worldObject1, WorldObject worldObject2) = WorldHelper.WorldObjectsAt(source, destination);
    bool spaceObject;
    return Ext_Math.SphericalDistance(WorldHelper.GetTilePos(source, worldObject1, out spaceObject), WorldHelper.GetTilePos(destination, worldObject2, out spaceObject));
  }

  public static PlanetTile BestGotoDestForVehicle(VehicleCaravan caravan, PlanetTile tile)
  {
    if (CaravanReachable(tile))
      return tile;
    PlanetTile planetTile;
    GenWorldClosest.TryFindClosestTile(tile, new Predicate<PlanetTile>(CaravanReachable), ref planetTile, 50, true);
    return planetTile;

    bool CaravanReachable(PlanetTile planetTile)
    {
      return caravan.UniqueVehicleDefsInCaravan().All<VehicleDef>((Func<VehicleDef, bool>) (vehicleDef => WorldVehiclePathGrid.Instance.Passable(planetTile, vehicleDef))) && WorldVehiclePathGrid.Instance.reachability.CanReach(caravan, planetTile);
    }
  }

  public static Pawn FindBestNegotiator(
    this VehiclePawn vehicle,
    Faction faction = null,
    TraderKindDef trader = null)
  {
    Predicate<Pawn> pawnValidator = (Predicate<Pawn>) null;
    if (faction != null)
      pawnValidator = (Predicate<Pawn>) (p =>
      {
        AcceptanceReport acceptanceReport = FactionUtility.CanTradeWith(p, faction, trader);
        return ((AcceptanceReport) ref acceptanceReport).Accepted;
      });
    return vehicle.FindPawnWithBestStat(StatDefOf.TradePriceImprovement, pawnValidator);
  }

  public static Pawn FindPawnWithBestStat(
    this VehiclePawn vehicle,
    StatDef stat,
    Predicate<Pawn> pawnValidator)
  {
    Pawn pawnWithBestStat = (Pawn) null;
    float num = -1f;
    foreach (Pawn pawn in vehicle.AllPawnsAboard)
    {
      if (!pawn.Dead && !pawn.Downed && !pawn.InMentalState && CaravanUtility.IsOwner(pawn, ((Thing) vehicle).Faction) && !stat.Worker.IsDisabledFor((Thing) pawn) && (pawnValidator == null || pawnValidator(pawn)))
      {
        float statValue = StatExtension.GetStatValue((Thing) pawn, stat, true, -1);
        if (pawnWithBestStat == null || (double) statValue > (double) num)
        {
          pawnWithBestStat = pawn;
          num = statValue;
        }
      }
    }
    return pawnWithBestStat;
  }

  public static Pawn FindBestNegotiator(
    VehicleCaravan caravan,
    Faction faction = null,
    TraderKindDef trader = null)
  {
    Predicate<Pawn> predicate = (Predicate<Pawn>) null;
    if (faction != null)
      predicate = (Predicate<Pawn>) (p =>
      {
        AcceptanceReport acceptanceReport = FactionUtility.CanTradeWith(p, faction, trader);
        return ((AcceptanceReport) ref acceptanceReport).Accepted;
      });
    return BestCaravanPawnUtility.FindPawnWithBestStat((Caravan) caravan, StatDefOf.TradePriceImprovement, predicate);
  }

  public static int GetNearestTile(Vector3 worldCoord)
  {
    for (int nearestTile = 0; nearestTile < Find.WorldGrid.TilesCount; ++nearestTile)
    {
      Vector3 tileCenter = Find.WorldGrid.GetTileCenter(PlanetTile.op_Implicit(nearestTile));
      if ((double) Ext_Math.SphericalDistance(worldCoord, tileCenter) <= 0.75)
        return nearestTile;
    }
    return -1;
  }

  public static PlanetTile AdjustSettlement(PlanetTile tile)
  {
    if (((PlanetTile) ref tile).LayerDef.isSpace || ((PlanetTile) ref tile).Tile.IsCoastal)
      return tile;
    if (((PlanetTile) ref tile).Tile.OnSurface && ((PlanetTile) ref tile).Tile is SurfaceTile tile1)
    {
      List<SurfaceTile.RoadLink> roads = tile1.Roads;
      if (roads != null && roads.Count > 0)
        goto label_6;
    }
    if (((PlanetTile) ref tile).Tile is SurfaceTile tile2)
    {
      List<SurfaceTile.RiverLink> rivers = tile2.Rivers;
      if (rivers != null && rivers.Count > 0)
        goto label_6;
    }
    using (new ClearOnDispose<PlanetTile>((ICollection<PlanetTile>) WorldHelper.CandidateTiles))
    {
      Ext_World.Bfs(tile, new Action<PlanetTile>(ProcessTile), VehicleMod.settings.main.adjustSettlementRadius, new Func<PlanetTile, bool>(CanEnter));
      if (GenList.NullOrEmpty<PlanetTile>((IList<PlanetTile>) WorldHelper.CandidateTiles))
        return tile;
      PlanetTile to = GenCollection.RandomElementByWeightWithFallback<PlanetTile>((IEnumerable<PlanetTile>) WorldHelper.CandidateTiles, new Func<PlanetTile, float>(GetTileWeight), tile);
      if (DebugProperties.Debug)
        DebugHelper.DebugAddSettlementOrigin(tile, to);
      return to;
    }
label_6:
    return tile;

    static float GetTileWeight(PlanetTile currentTile)
    {
      return GenCollection.TryGetValue<PlanetTile, float>((IReadOnlyDictionary<PlanetTile, float>) WorldHelper.TileWeights, currentTile, 0.0f);
    }

    static bool CanEnter(PlanetTile currentTile)
    {
      WorldObject worldObject;
      return Find.WorldGrid[currentTile].PrimaryBiome.canBuildBase && Find.WorldGrid[currentTile].PrimaryBiome.implemented && Find.WorldGrid[currentTile].hilliness != 5 && !Find.WorldObjects.AnySettlementBaseAtOrAdjacent(currentTile, ref worldObject) && SettleInEmptyTileUtility.CanCreateMapAt(currentTile, false);
    }

    static void ProcessTile(PlanetTile currentTile)
    {
      WorldHelper.CandidateTiles.Add(currentTile);
      if (WorldHelper.TileWeights.ContainsKey(currentTile))
        return;
      float num = TileWeightAt(currentTile);
      WorldHelper.TileWeights[currentTile] = num;
    }

    static float TileWeightAt(PlanetTile currentTile)
    {
      float num = 0.0f;
      if (((PlanetTile) ref currentTile).Tile.IsCoastal)
        num += VehicleMod.settings.main.adjustCoastWeight;
      if (((PlanetTile) ref currentTile).Tile.OnSurface && ((PlanetTile) ref currentTile).Tile is SurfaceTile tile3 && !GenList.NullOrEmpty<SurfaceTile.RiverLink>((IList<SurfaceTile.RiverLink>) tile3.Rivers))
        num += (float) tile3.Rivers.Count * VehicleMod.settings.main.adjustRiverWeight;
      return num;
    }
  }

  public static Matrix4x4 GetWorldQuadAt(
    Vector3 pos,
    float size,
    float altOffset,
    bool counterClockwise = false)
  {
    Vector3 normalized = ((Vector3) ref pos).normalized;
    Vector3 vector3_1 = !counterClockwise ? normalized : Vector3.op_UnaryNegation(normalized);
    Quaternion quaternion = Quaternion.LookRotation(Vector3.Cross(vector3_1, Vector3.up), vector3_1);
    Vector3 vector3_2;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3_2).\u002Ector(size, 1f, size);
    Matrix4x4 worldQuadAt = new Matrix4x4();
    ((Matrix4x4) ref worldQuadAt).SetTRS(Vector3.op_Addition(pos, Vector3.op_Multiply(normalized, altOffset)), quaternion, vector3_2);
    return worldQuadAt;
  }

  public static void DrawQuadTangentialToPlanet(
    Vector3 pos,
    float size,
    float altOffset,
    Material material,
    bool counterClockwise = false,
    bool useSkyboxLayer = false,
    MaterialPropertyBlock propertyBlock = null)
  {
    if (Object.op_Equality((Object) material, (Object) null))
    {
      Log.Warning("Tried to draw quad with null material.");
    }
    else
    {
      Vector3 normalized = ((Vector3) ref pos).normalized;
      Vector3 vector3_1 = !counterClockwise ? normalized : Vector3.op_UnaryNegation(normalized);
      Quaternion quaternion = Quaternion.op_Multiply(Quaternion.LookRotation(Vector3.Cross(vector3_1, Vector3.up), vector3_1), Quaternion.Euler(0.0f, -90f, 0.0f));
      Vector3 vector3_2;
      // ISSUE: explicit constructor call
      ((Vector3) ref vector3_2).\u002Ector(size, 1f, size);
      Matrix4x4 matrix4x4 = new Matrix4x4();
      ((Matrix4x4) ref matrix4x4).SetTRS(Vector3.op_Addition(pos, Vector3.op_Multiply(normalized, altOffset)), quaternion, vector3_2);
      int num = useSkyboxLayer ? WorldCameraManager.WorldSkyboxLayer : WorldCameraManager.WorldLayer;
      if (propertyBlock != null)
        Graphics.DrawMesh(MeshPool.plane10, matrix4x4, material, num, (Camera) null, 0, propertyBlock);
      else
        Graphics.DrawMesh(MeshPool.plane10, matrix4x4, material, num);
    }
  }
}
