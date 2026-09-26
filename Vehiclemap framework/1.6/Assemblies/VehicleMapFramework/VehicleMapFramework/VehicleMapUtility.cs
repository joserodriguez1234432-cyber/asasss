// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleMapUtility
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using VehicleMapFramework.VMF_HarmonyPatches;
using Vehicles;
using Vehicles.World;
using Verse;
using Verse.AI.Group;

#nullable disable
namespace VehicleMapFramework;

[PublicAPI]
public static class VehicleMapUtility
{
  public const float YCompress = 39.9999962f;
  public const float YOffsetBase = 0.000914634264f;
  internal static readonly VFVersionalPatchAttribute VFLatestRelease = new VFVersionalPatchAttribute("1.6.2144", (ComparisonType) 1);
  internal static readonly VFVersionalPatchAttribute VFUnstableRelease = new VFVersionalPatchAttribute("1.6.2144", (ComparisonType) 3);
  private static readonly List<Thing> tmpThingList = new List<Thing>();
  private static readonly List<Building> tmpBuildingList = new List<Building>();
  private static readonly AccessTools.FieldRef<RoofGrid, Map> roofGrid_map = AccessTools.FieldRefAccess<RoofGrid, Map>("map");
  private static readonly SimpleCurve PointsPerWealthCurve = AccessTools.StaticFieldRefAccess<SimpleCurve>(typeof (StorytellerUtility), nameof (PointsPerWealthCurve));
  private static readonly SimpleCurve PointsPerColonistByWealthCurve = AccessTools.StaticFieldRefAccess<SimpleCurve>(typeof (StorytellerUtility), nameof (PointsPerColonistByWealthCurve));
  private static readonly SimpleCurve PointsFactorForColonyMechsCurve = AccessTools.StaticFieldRefAccess<SimpleCurve>(typeof (StorytellerUtility), nameof (PointsFactorForColonyMechsCurve));
  private static readonly SimpleCurve PointsFactorForColonySubhumanCurve = AccessTools.StaticFieldRefAccess<SimpleCurve>(typeof (StorytellerUtility), nameof (PointsFactorForColonySubhumanCurve));
  private static readonly SimpleCurve PointsFactorForPawnAgeYearsCurve = AccessTools.StaticFieldRefAccess<SimpleCurve>(typeof (StorytellerUtility), nameof (PointsFactorForPawnAgeYearsCurve));

  public static Map CurrentMap
  {
    get
    {
      return Command_FocusVehicleMap.FocusedVehicle == null ? Find.CurrentMap : Command_FocusVehicleMap.FocusedVehicle.CurrentLevel;
    }
  }

  [ContractAnnotation("=> true, vehicle:notnull; => false, vehicle:null")]
  public static bool FocusedOnVehicleMap([CanBeNull] out VehiclePawnWithMap vehicle)
  {
    if (Command_FocusVehicleMap.FocusedVehicle == null)
      return Find.CurrentMap.IsNonFocusedVehicleMapOf(out vehicle);
    vehicle = Command_FocusVehicleMap.FocusedVehicle;
    return true;
  }

  private static Vector3 MapPivot(Map map)
  {
    if (!ModCompat.CompatBase<ModCompat.AsAboveSoBelow>.Active)
    {
      CellRect cellRect = CellRect.WholeMap(map);
      return ((CellRect) ref cellRect).CenterVector3;
    }
    CellRect cellRect1 = ModCompat.AsAboveSoBelow.RectOfBand(map, ModCompat.AsAboveSoBelow.CurrentBand(map));
    return ((CellRect) ref cellRect1).CenterVector3;
  }

  private static Vector3 MapPivot(Map map, IntVec3 bandSource)
  {
    CellRect band;
    if (ModCompat.CompatBase<ModCompat.AsAboveSoBelow>.Active && ModCompat.AsAboveSoBelow.TryBandRectOf(map, bandSource, out band))
      return ((CellRect) ref band).CenterVector3;
    CellRect cellRect = CellRect.WholeMap(map);
    return ((CellRect) ref cellRect).CenterVector3;
  }

  public static CellRect ClipInsideVehicleMap(ref this CellRect cellRect, Map map)
  {
    VehiclePawnWithMap vehicle;
    return map.IsVehicleMapOf(out vehicle) ? (cellRect = CellRect.WholeMap(vehicle.VehicleMap)) : ((CellRect) ref cellRect).ClipInsideMap(map);
  }

  public static Matrix4x4 ToBaseMapCoord(this Matrix4x4 matrix, VehiclePawnWithMap vehicle)
  {
    Vector3 original = GenMath.Position(matrix);
    ((Matrix4x4) ref matrix).SetColumn(3, Vector4.op_Implicit(Vector3Utility.WithY(original.ToBaseMapCoord(vehicle), original.y)));
    return matrix;
  }

  public static Vector3 OffsetFor(VehiclePawnWithMap vehicle)
  {
    return Vector3Utility.RotatedBy(VehicleMapUtility.OffsetFor(vehicle, vehicle.FullRotation), vehicle.Transform.rotation);
  }

  public static Vector3 OffsetFor(VehiclePawnWithMap vehicle, Rot8 rot)
  {
    Vector3 zero = Vector3.zero;
    VehicleMapProps vehicleMap = vehicle.VehicleMapProps;
    if (vehicleMap == null)
      return zero;
    Vector3 vector3_1;
    switch (((Rot8) ref rot).AsByte)
    {
      case 0:
        vector3_1 = OffsetNorth();
        break;
      case 1:
        vector3_1 = vehicleMap.offsetEast ?? (!vehicleMap.offsetWest.HasValue ? (vehicleMap.offsetEast = vehicleMap.offsetWest = new Vector3?(vehicleMap.offset)) : (vehicleMap.offsetEast = new Vector3?(Ext_Unity.MirrorHorizontal(vehicleMap.offsetWest.Value)))).Value;
        break;
      case 2:
        vector3_1 = OffsetSouth();
        break;
      case 3:
        vector3_1 = vehicleMap.offsetWest ?? (!vehicleMap.offsetEast.HasValue ? (vehicleMap.offsetWest = vehicleMap.offsetEast = new Vector3?(vehicleMap.offset)) : (vehicleMap.offsetWest = new Vector3?(Ext_Unity.MirrorHorizontal(vehicleMap.offsetEast.Value)))).Value;
        break;
      case 4:
        VehicleMapProps vehicleMapProps1 = vehicleMap;
        Vector3 valueOrDefault1 = vehicleMapProps1.offsetNorthEast.GetValueOrDefault();
        Vector3 vector3_2;
        if (!vehicleMapProps1.offsetNorthEast.HasValue)
        {
          VehicleMapProps vehicleMapProps2 = vehicleMap;
          Vector3 valueOrDefault2 = vehicleMapProps2.offsetNorthWest.GetValueOrDefault();
          Vector3 vector3_3;
          if (!vehicleMapProps2.offsetNorthWest.HasValue)
          {
            Vector3 vector3_4 = Vector3Utility.RotatedBy(OffsetNorth(), -45f);
            vehicleMapProps2.offsetNorthWest = new Vector3?(vector3_4);
            vector3_3 = vector3_4;
          }
          else
            vector3_3 = valueOrDefault2;
          Vector3 vector3_5 = Ext_Unity.MirrorHorizontal(vector3_3);
          vehicleMapProps1.offsetNorthEast = new Vector3?(vector3_5);
          vector3_2 = vector3_5;
        }
        else
          vector3_2 = valueOrDefault1;
        vector3_1 = vector3_2;
        break;
      case 5:
        VehicleMapProps vehicleMapProps3 = vehicleMap;
        Vector3 valueOrDefault3 = vehicleMapProps3.offsetSouthEast.GetValueOrDefault();
        Vector3 vector3_6;
        if (!vehicleMapProps3.offsetSouthEast.HasValue)
        {
          VehicleMapProps vehicleMapProps4 = vehicleMap;
          Vector3 valueOrDefault4 = vehicleMapProps4.offsetSouthWest.GetValueOrDefault();
          Vector3 vector3_7;
          if (!vehicleMapProps4.offsetSouthWest.HasValue)
          {
            Vector3 vector3_8 = Vector3Utility.RotatedBy(OffsetSouth(), 45f);
            vehicleMapProps4.offsetSouthWest = new Vector3?(vector3_8);
            vector3_7 = vector3_8;
          }
          else
            vector3_7 = valueOrDefault4;
          Vector3 vector3_9 = Ext_Unity.MirrorHorizontal(vector3_7);
          vehicleMapProps3.offsetSouthEast = new Vector3?(vector3_9);
          vector3_6 = vector3_9;
        }
        else
          vector3_6 = valueOrDefault3;
        vector3_1 = vector3_6;
        break;
      case 6:
        VehicleMapProps vehicleMapProps5 = vehicleMap;
        Vector3 valueOrDefault5 = vehicleMapProps5.offsetSouthWest.GetValueOrDefault();
        Vector3 vector3_10;
        if (!vehicleMapProps5.offsetSouthWest.HasValue)
        {
          VehicleMapProps vehicleMapProps6 = vehicleMap;
          Vector3 valueOrDefault6 = vehicleMapProps6.offsetSouthEast.GetValueOrDefault();
          Vector3 vector3_11;
          if (!vehicleMapProps6.offsetSouthEast.HasValue)
          {
            Vector3 vector3_12 = Vector3Utility.RotatedBy(OffsetSouth(), -45f);
            vehicleMapProps6.offsetSouthEast = new Vector3?(vector3_12);
            vector3_11 = vector3_12;
          }
          else
            vector3_11 = valueOrDefault6;
          Vector3 vector3_13 = Ext_Unity.MirrorHorizontal(vector3_11);
          vehicleMapProps5.offsetSouthWest = new Vector3?(vector3_13);
          vector3_10 = vector3_13;
        }
        else
          vector3_10 = valueOrDefault5;
        vector3_1 = vector3_10;
        break;
      case 7:
        VehicleMapProps vehicleMapProps7 = vehicleMap;
        Vector3 valueOrDefault7 = vehicleMapProps7.offsetNorthWest.GetValueOrDefault();
        Vector3 vector3_14;
        if (!vehicleMapProps7.offsetNorthWest.HasValue)
        {
          VehicleMapProps vehicleMapProps8 = vehicleMap;
          Vector3 valueOrDefault8 = vehicleMapProps8.offsetNorthEast.GetValueOrDefault();
          Vector3 vector3_15;
          if (!vehicleMapProps8.offsetNorthEast.HasValue)
          {
            Vector3 vector3_16 = Vector3Utility.RotatedBy(OffsetNorth(), 45f);
            vehicleMapProps8.offsetNorthEast = new Vector3?(vector3_16);
            vector3_15 = vector3_16;
          }
          else
            vector3_15 = valueOrDefault8;
          Vector3 vector3_17 = Ext_Unity.MirrorHorizontal(vector3_15);
          vehicleMapProps7.offsetNorthWest = new Vector3?(vector3_17);
          vector3_14 = vector3_17;
        }
        else
          vector3_14 = valueOrDefault7;
        vector3_1 = vector3_14;
        break;
      default:
        vector3_1 = zero;
        break;
    }
    return vector3_1;

    Vector3 OffsetNorth()
    {
      return vehicleMap.offsetNorth ?? (!vehicleMap.offsetSouth.HasValue ? (vehicleMap.offsetNorth = vehicleMap.offsetSouth = new Vector3?(vehicleMap.offset)) : (vehicleMap.offsetNorth = new Vector3?(Ext_Unity.MirrorVertical(vehicleMap.offsetSouth.Value)))).Value;
    }

    Vector3 OffsetSouth()
    {
      return vehicleMap.offsetSouth ?? (!vehicleMap.offsetNorth.HasValue ? (vehicleMap.offsetSouth = vehicleMap.offsetNorth = new Vector3?(vehicleMap.offset)) : (vehicleMap.offsetNorth = new Vector3?(Ext_Unity.MirrorVertical(vehicleMap.offsetNorth.Value)))).Value;
    }
  }

  public static IntVec3 HitboxToMapCell(VehiclePawnWithMap vehicle)
  {
    return IntVec3.op_Subtraction(IntVec3.op_Division(vehicle.MapSize, 2), IntVec3Utility.ToIntVec3(VehicleMapUtility.OffsetFor(vehicle, Rot8.North)));
  }

  public static IntVec2 MapCellToHitbox(VehiclePawnWithMap vehicle)
  {
    IntVec3 intVec3 = IntVec3.op_Subtraction(IntVec3Utility.ToIntVec3(VehicleMapUtility.OffsetFor(vehicle, Rot8.North)), IntVec3.op_Division(vehicle.MapSize, 2));
    return ((IntVec3) ref intVec3).ToIntVec2;
  }

  public static float PrintExtraRotation(Thing thing)
  {
    float num1 = 0.0f;
    if (thing.IsOnVehicleMapOf(out VehiclePawnWithMap _))
    {
      double num2 = (double) num1;
      Rot4 rotForPrint = VehicleSectionLayerManager.RotForPrint;
      double asAngle = (double) ((Rot4) ref rotForPrint).AsAngle;
      num1 = (float) (num2 - asAngle);
    }
    return num1;
  }

  public static Map BaseMap(this Zone zone)
  {
    VehiclePawnWithMap vehicle;
    return zone.Map.IsVehicleMapOf(out vehicle) && ((Thing) vehicle).Spawned ? ((Thing) vehicle).Map : zone.Map;
  }

  public static IntVec3 PositionOnBaseMap(this IHaulDestination dest)
  {
    VehiclePawnWithMap vehicle;
    return !dest.Map.IsVehicleMapOf(out vehicle) ? dest.Position : dest.Position.ToBaseMapCoord(vehicle);
  }

  public static TargetInfo ToBaseMapTargetInfo(ref LocalTargetInfo target, Map map)
  {
    if (!((LocalTargetInfo) ref target).IsValid)
      return TargetInfo.Invalid;
    return ((LocalTargetInfo) ref target).Thing == null ? new TargetInfo(target.CellOnBaseMap(), map, false) : new TargetInfo(((LocalTargetInfo) ref target).Thing);
  }

  public static IntVec3 CellOnAnotherThingMap(this LocalTargetInfo target, Thing another)
  {
    if (((LocalTargetInfo) ref target).HasThing)
      return ((LocalTargetInfo) ref target).Thing.PositionOnAnotherThingMap(another);
    VehiclePawnWithMap vehicle;
    return !another.IsOnVehicleMapOf(out vehicle) ? ((LocalTargetInfo) ref target).Cell : ((LocalTargetInfo) ref target).Cell.ToVehicleMapCoord(vehicle);
  }

  public static IntVec3 CellOnAnotherMap(this IntVec3 cell, Map another)
  {
    VehiclePawnWithMap vehicle;
    return !another.IsVehicleMapOf(out vehicle) ? cell : cell.ToVehicleMapCoord(vehicle);
  }

  public static int HalfLength(this VehicleDef vehicleDef) => ((ThingDef) vehicleDef).size.z / 2;

  public static Rot4 RotForVehicleDraw(this Rot8 rot)
  {
    if (!((Rot8) ref rot).IsDiagonal)
      return Rot8.op_Implicit(rot);
    return !Rot8.op_Equality(rot, Rot8.NorthEast) && !Rot8.op_Equality(rot, Rot8.NorthWest) ? Rot4.South : Rot4.North;
  }

  public static IntVec2 BaseRotatedSize(Thing thing)
  {
    Rot4 rot4 = thing.BaseRotation();
    return ((Rot4) ref rot4).IsHorizontal ? new IntVec2(thing.def.size.z, thing.def.size.x) : thing.def.size;
  }

  public static float VehicleMapMass(VehiclePawnWithMap vehicle)
  {
    float num = CollectionsMassCalculator.MassUsage<Thing>(vehicle.VehicleMap.listerThings.AllThings, (IgnorePawnsInventoryMode) 3, true, false);
    if (ModCompat.CompatBase<ModCompat.MultiFloors>.Active)
      num += ModCompat.MultiFloors.GetOtherLevels(vehicle.VehicleMap).Sum<Map>((Func<Map, float>) (map => CollectionsMassCalculator.MassUsage<Thing>(map.listerThings.AllThings, (IgnorePawnsInventoryMode) 3, true, false)));
    return num;
  }

  public static Vector3 RotateForPrintNegate(Vector3 vector)
  {
    Vector3 vector3 = vector;
    Rot4 rotForPrint = VehicleSectionLayerManager.RotForPrint;
    double num = -(double) ((Rot4) ref rotForPrint).AsAngle;
    return Vector3Utility.RotatedBy(vector3, (float) num);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TryGetVehicleMap(this IntVec3 c, Map map, out VehiclePawnWithMap vehicle)
  {
    vehicle = MapComponentCache<VehicleMapGrid>.GetComponent(map).VehicleAt(c);
    return vehicle != null;
  }

  public static void SetTRSOnVehicle(
    ref Matrix4x4 matrix,
    Vector3 pos,
    Quaternion q,
    Vector3 s,
    Thing thing)
  {
    VehiclePawnWithMap vehicle;
    if (thing.IsOnNonFocusedVehicleMapOf(out vehicle))
    {
      Rot8 fullRotation = vehicle.FullRotation;
      float num = ((Rot8) ref fullRotation).AsAngle + vehicle.Transform.rotation;
      matrix = Matrix4x4.TRS(Ext_Math.RotatePoint(pos, GenThing.TrueCenter(thing), -num), Quaternion.op_Multiply(q, VehicleMapUtility.get_FullAngleQuat((VehiclePawn) vehicle)), s);
    }
    else
      matrix = Matrix4x4.TRS(pos, q, s);
  }

  public static Vector3 SelectedDrawPosOffset(Vector3 original, IntVec3 center)
  {
    VehiclePawnWithMap vehicle = (VehiclePawnWithMap) null;
    return !GenCollection.Any<object>(Find.Selector.SelectedObjects, (Predicate<object>) (o => o is Thing thing && IntVec3.op_Equality(thing.Position, center) && thing.IsOnNonFocusedVehicleMapOf(out vehicle))) ? original : Vector3Utility.WithY(original.ToBaseMapCoord(vehicle), Altitudes.AltitudeFor((AltitudeLayer) 39));
  }

  public static Vector3 FocusedDrawPosOffset(Vector3 original)
  {
    VehiclePawnWithMap vehicle;
    return !VehicleMapUtility.FocusedOnVehicleMap(out vehicle) ? original : Vector3Utility.WithY(original.ToBaseMapCoord(vehicle), Altitudes.AltitudeFor((AltitudeLayer) 39));
  }

  public static Vector3 FocusedOrSelectedDrawPosOffset(Vector3 original, IntVec3 center)
  {
    Thing thing;
    if ((thing = Find.Selector.SelectedObjects.OfType<Thing>().FirstOrDefault<Thing>((Func<Thing, bool>) (t => IntVec3.op_Equality(t.Position, center)))) != null)
    {
      VehiclePawnWithMap vehicle;
      if (thing.IsOnNonFocusedVehicleMapOf(out vehicle))
        return Vector3Utility.WithY(original.ToBaseMapCoord(vehicle), Altitudes.AltitudeFor((AltitudeLayer) 39));
    }
    else
    {
      VehiclePawnWithMap vehicle;
      if (VehicleMapUtility.FocusedOnVehicleMap(out vehicle))
        return Vector3Utility.WithY(original.ToBaseMapCoord(vehicle), Altitudes.AltitudeFor((AltitudeLayer) 39));
    }
    return original;
  }

  public static IEnumerable<Thing> ColonyThingsWillingToBuyOnVehicle(
    this VehiclePawnWithMap vehicle,
    ITrader trader)
  {
    Map map = vehicle.VehicleMap;
    IEnumerator<Thing> enumerator1 = map.listerThings.AllThings.Where<Thing>((Func<Thing, bool>) (x =>
    {
      if (x.def.category != 2 || !TradeUtility.PlayerSellableNow(x, trader) || GridsUtility.Fogged(x.Position, map))
        return false;
      return ((Area) map.areaManager.Home)[x.Position] || StoreUtility.IsInAnyStorage(x);
    })).GetEnumerator();
    while (enumerator1.MoveNext())
      yield return enumerator1.Current;
    enumerator1 = (IEnumerator<Thing>) null;
    if (ModsConfig.BiotechActive)
    {
      IEnumerator<Genepack> enumerator2 = map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.GeneBank).Select<Building, CompGenepackContainer>((Func<Building, CompGenepackContainer>) (item2 => ThingCompUtility.TryGetComp<CompGenepackContainer>((Thing) item2))).Where<CompGenepackContainer>((Func<CompGenepackContainer, bool>) (compGenepackContainer => compGenepackContainer != null)).Select<CompGenepackContainer, List<Genepack>>((Func<CompGenepackContainer, List<Genepack>>) (compGenepackContainer => compGenepackContainer.ContainedGenepacks)).SelectMany<List<Genepack>, Genepack>((Func<List<Genepack>, IEnumerable<Genepack>>) (containedGenepacks => (IEnumerable<Genepack>) containedGenepacks)).GetEnumerator();
      while (enumerator2.MoveNext())
        yield return (Thing) enumerator2.Current;
      enumerator2 = (IEnumerator<Genepack>) null;
    }
    foreach (IThingHolder ithingHolder in map.listerBuildings.AllColonistBuildingsOfType<IHaulSource>())
    {
      enumerator1 = ((IEnumerable<Thing>) ithingHolder.GetDirectlyHeldThings()).GetEnumerator();
      while (enumerator1.MoveNext())
        yield return enumerator1.Current;
      enumerator1 = (IEnumerator<Thing>) null;
    }
    if ((!(trader is Pawn pawn) || LordUtility.GetLord(pawn) != null) && !((Thing) vehicle).Spawned)
    {
      foreach (Thing thing in TradeUtility.AllSellableColonyPawns(map, true).Where<Pawn>((Func<Pawn, bool>) (x => !x.Downed)))
        yield return thing;
    }
  }

  public static bool ShouldRotatedOnVehicle(this ThingDef tDef)
  {
    return (double) tDef.fillPercent > 0.25 || IntVec2.op_Inequality(((BuildableDef) tDef).Size, IntVec2.One) || !(((BuildableDef) tDef).graphic is Graphic_Single) && !(((BuildableDef) tDef).graphic is Graphic_Collection) || tDef.hasInteractionCell || tDef.drawerType == 2 || tDef.drawerType == 3 || tDef.size.x != tDef.size.z;
  }

  public static List<Thing> GetThingListAcrossMaps(this IntVec3 c, Map map)
  {
    VehicleMapUtility.tmpThingList.Clear();
    List<Thing> collection = GenGrid.InBounds(c, map) ? map.thingGrid.ThingsListAtFast(c) : VehicleMapUtility.tmpThingList;
    VehiclePawnWithMap vehicle1;
    if (map.IsVehicleMapOf(out vehicle1))
    {
      VehicleMapUtility.tmpThingList.AddRange((IEnumerable<Thing>) collection);
      IntVec3 baseMapCoord = c.ToBaseMapCoord(vehicle1);
      if (((Thing) vehicle1).Spawned)
      {
        Map map1 = ((Thing) vehicle1).Map;
        VehicleMapUtility.tmpThingList.AddRange((IEnumerable<Thing>) GridsUtility.GetThingList(baseMapCoord, map1));
        VehiclePawnWithMap vehicle2;
        if (baseMapCoord.TryGetVehicleMap(map1, out vehicle2) && vehicle1 != vehicle2)
        {
          IntVec3 vehicleMapCoord = baseMapCoord.ToVehicleMapCoord(vehicle2);
          if (GenGrid.InBounds(vehicleMapCoord, vehicle2.VehicleMap))
            VehicleMapUtility.tmpThingList.AddRange((IEnumerable<Thing>) vehicle2.VehicleMap.thingGrid.ThingsListAtFast(vehicleMapCoord));
        }
        return VehicleMapUtility.tmpThingList;
      }
      foreach (Map mapAndVehicleMap in map.BaseMapAndVehicleMaps(false))
      {
        VehiclePawnWithMap vehicle3;
        if (mapAndVehicleMap.IsVehicleMapOf(out vehicle3))
        {
          IntVec3 vehicleMapCoord = baseMapCoord.ToVehicleMapCoord(vehicle3);
          if (GenGrid.InBounds(vehicleMapCoord, mapAndVehicleMap))
            VehicleMapUtility.tmpThingList.AddRange((IEnumerable<Thing>) mapAndVehicleMap.thingGrid.ThingsListAtFast(vehicleMapCoord));
        }
        else if (GenGrid.InBounds(baseMapCoord, mapAndVehicleMap))
          VehicleMapUtility.tmpThingList.AddRange((IEnumerable<Thing>) mapAndVehicleMap.thingGrid.ThingsListAtFast(baseMapCoord));
      }
      return VehicleMapUtility.tmpThingList;
    }
    VehiclePawnWithMap vehicle4;
    if (c.TryGetVehicleMap(map, out vehicle4))
    {
      IntVec3 vehicleMapCoord = c.ToVehicleMapCoord(vehicle4);
      Map vehicleMap = vehicle4.VehicleMap;
      if (GenGrid.InBounds(vehicleMapCoord, vehicleMap))
      {
        VehicleMapUtility.tmpThingList.AddRange((IEnumerable<Thing>) collection);
        VehicleMapUtility.tmpThingList.AddRange((IEnumerable<Thing>) vehicleMap.thingGrid.ThingsListAtFast(vehicleMapCoord));
        return VehicleMapUtility.tmpThingList;
      }
    }
    return collection;
  }

  public static List<Building> AddColonistBuildingList(
    List<Building> allBuildingsColonist,
    Thing instance)
  {
    HashSet<Map> mapSet = instance.Map.BaseMapAndVehicleMaps(false);
    if (GenCollection.NullOrEmpty<Map>(mapSet))
      return allBuildingsColonist;
    VehicleMapUtility.tmpBuildingList.Clear();
    VehicleMapUtility.tmpBuildingList.AddRange((IEnumerable<Building>) allBuildingsColonist);
    foreach (Map map in mapSet)
      VehicleMapUtility.tmpBuildingList.AddRange((IEnumerable<Building>) map.listerBuildings.allBuildingsColonist);
    return VehicleMapUtility.tmpBuildingList;
  }

  public static bool RoofedAcrossMaps(RoofGrid roofGrid, IntVec3 c)
  {
    return c.RoofedAcrossMaps(VehicleMapUtility.roofGrid_map.Invoke(roofGrid));
  }

  public static float DefaultThreatPointsNowForMapVehicles(IIncidentTarget target)
  {
    VehiclePawnWithMap vehicle;
    List<Pawn> list;
    if (target is Map map && map.IsVehicleMapOf(out vehicle))
    {
      WorldObject orStashedVehicle = vehicle.VehicleCaravanOrStashedVehicle;
      if (!(orStashedVehicle is VehicleCaravan vehicleCaravan))
      {
        if (orStashedVehicle is StashedVehicle stashedVehicle)
        {
          list = ((IEnumerable<Pawn>) stashedVehicle.Vehicles).ToList<Pawn>();
        }
        else
        {
          if (!((Thing) vehicle).Spawned)
            return 0.0f;
          target = (IIncidentTarget) ((Thing) vehicle).Map;
          list = ((Thing) vehicle).Map.PlayerPawnsForStoryteller.ToList<Pawn>();
        }
      }
      else
      {
        target = (IIncidentTarget) vehicleCaravan;
        list = ((Caravan) vehicleCaravan).PlayerPawnsForStoryteller.ToList<Pawn>();
      }
    }
    else
      list = target.PlayerPawnsForStoryteller.ToList<Pawn>();
    float wealthForStoryteller = target.PlayerWealthForStoryteller;
    wealthForStoryteller += list.OfType<VehiclePawnWithMap>().Sum<VehiclePawnWithMap>((Func<VehiclePawnWithMap, float>) (v => v.VehicleMap.PlayerWealthForStoryteller));
    double num1 = (double) VehicleMapUtility.PointsPerWealthCurve.Evaluate(wealthForStoryteller);
    float num2 = 0.0f;
    PawnsFactor((IEnumerable<Pawn>) list);
    double num3 = (double) num2;
    double num4 = num1 + num3;
    FloatRange randomFactorRange = target.IncidentPointsRandomFactorRange;
    double randomInRange = (double) ((FloatRange) ref randomFactorRange).RandomInRange;
    return Mathf.Clamp((float) (num4 * randomInRange) * Mathf.Lerp(1f, Find.StoryWatcher.watcherAdaptation.TotalThreatPointsFactor, Find.Storyteller.difficulty.adaptationEffectFactor) * Find.Storyteller.difficulty.threatScale * Find.Storyteller.def.pointsFactorFromDaysPassed.Evaluate((float) GenDate.DaysPassedSinceSettle), StorytellerUtility.GlobalPointsMin(), 10000f);

    void PawnsFactor(IEnumerable<Pawn> pawnsEnumerable)
    {
      foreach (Pawn pawns in pawnsEnumerable)
      {
        if (!QuestUtility.IsQuestLodger(pawns))
        {
          float num1 = 0.0f;
          if (pawns.IsFreeColonist)
          {
            num1 = VehicleMapUtility.PointsPerColonistByWealthCurve.Evaluate(wealthForStoryteller);
          }
          else
          {
            if (pawns.IsAnimal && ((Thing) pawns).Faction == Faction.OfPlayer && !pawns.Downed)
            {
              AcceptanceReport train = pawns.training.CanAssignToTrain(TrainableDefOf.Release);
              if (((AcceptanceReport) ref train).Accepted)
              {
                num1 = 0.08f * pawns.kindDef.combatPower;
                if (target is Caravan)
                {
                  num1 *= 0.7f;
                  goto label_13;
                }
                goto label_13;
              }
            }
            if (pawns.IsColonyMech && !pawns.Downed)
              num1 = pawns.kindDef.combatPower * VehicleMapUtility.PointsFactorForColonyMechsCurve.Evaluate(wealthForStoryteller);
            else if (pawns.IsSubhuman)
              num1 = pawns.kindDef.combatPower * VehicleMapUtility.PointsFactorForColonySubhumanCurve.Evaluate(wealthForStoryteller);
          }
label_13:
          if (pawns is VehiclePawnWithMap vehiclePawnWithMap)
          {
            num1 += VehicleMapUtility.PointsPerWealthCurve.Evaluate(vehiclePawnWithMap.VehicleMap.PlayerWealthForStoryteller);
            PawnsFactor(vehiclePawnWithMap.VehicleMap.PlayerPawnsForStoryteller);
          }
          if (pawns is VehiclePawn vehiclePawn)
            PawnsFactor((IEnumerable<Pawn>) vehiclePawn.AllPawnsAboard);
          if ((double) num1 > 0.0)
          {
            if (((Thing) pawns).ParentHolder is Building_CryptosleepCasket)
              num1 *= 0.3f;
            float num2 = Mathf.Lerp(num1, num1 * pawns.health.summaryHealth.SummaryHealthPercent, 0.65f);
            if (pawns.IsSlaveOfColony)
              num2 *= 0.75f;
            if (ModsConfig.BiotechActive && pawns.RaceProps.Humanlike)
              num2 *= VehicleMapUtility.PointsFactorForPawnAgeYearsCurve.Evaluate(pawns.ageTracker.AgeBiologicalYearsFloat);
            num2 += num2;
          }
        }
      }
    }
  }

  public static bool get_IsVehicleMap(Map map) => map.IsVehicleMapOf(out VehiclePawnWithMap _);

  public static bool get_IsNonFocusedVehicleMap(Map map)
  {
    return map.IsNonFocusedVehicleMapOf(out VehiclePawnWithMap _);
  }

  public static bool get_CrossMapContext(Map map)
  {
    if (map == null)
      return false;
    return VehicleMapUtility.get_IsVehicleMap(map) || VehiclePawnWithMapCache.AllVehiclesOn(map).Count != 0;
  }

  public static Map get_GroundMap(Map map) => map.BaseMap();

  public static object get_BaseMapOrCaravan(Map map)
  {
    VehiclePawnWithMap vehicle;
    if (!map.IsVehicleMapOf(out vehicle))
      return (object) map;
    return !((Thing) vehicle).Spawned ? (object) vehicle.VehicleCaravanOrStashedVehicle : (object) ((Thing) vehicle).Map;
  }

  [UsedImplicitly]
  [ContractAnnotation("=> true, vehicle:notnull; => false, vehicle:null")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsVehicleMapOf(this Map map, [CanBeNull] out VehiclePawnWithMap vehicle)
  {
    MapParent_Vehicle cachedVehicle = VehicleMapParentsComponent.GetCachedVehicle(map);
    if (cachedVehicle != null)
    {
      vehicle = cachedVehicle.vehicle;
      return vehicle != null;
    }
    vehicle = (VehiclePawnWithMap) null;
    return false;
  }

  [ContractAnnotation("=> true, vehicle:notnull; => false, vehicle:null")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNonFocusedVehicleMapOf(this Map map, [CanBeNull] out VehiclePawnWithMap vehicle)
  {
    if (map.IsVehicleMapOf(out vehicle) && (VehicleMapFramework.VehicleMapFramework.settings.drawPlanet || Find.CurrentMap != vehicle.VehicleMap))
      return true;
    vehicle = (VehiclePawnWithMap) null;
    return false;
  }

  [UsedImplicitly]
  public static IEnumerable<Map> BaseMapAndVehicleMaps(this Map map)
  {
    return (IEnumerable<Map>) map.BaseMapAndVehicleMaps(true);
  }

  public static HashSet<Map> BaseMapAndVehicleMaps(this Map map, bool includeItself)
  {
    VehiclePawnWithMapCache cachedMapComponent = map != null ? ComponentCache.GetCachedMapComponent<VehiclePawnWithMapCache>(map) : (VehiclePawnWithMapCache) null;
    if (cachedMapComponent == null)
      return new HashSet<Map>();
    ref (int, HashSet<Map>, HashSet<Map>) local = ref cachedMapComponent.cachedBaseMapAndVehicleMaps;
    if (local.Item1 == GenTicks.TicksGame)
      return !includeItself ? local.Item3 : local.Item2;
    local.Item1 = GenTicks.TicksGame;
    local.Item2.Clear();
    local.Item3.Clear();
    Map map1 = map.BaseMap();
    if (map1 == null)
      return local.Item2;
    local.Item2.Add(map);
    if (ModCompat.CompatBase<ModCompat.MultiFloors>.Active && ModCompat.MultiFloors.GroundMap(map) != map)
      return !includeItself ? local.Item3 : local.Item2;
    if (map1 != map)
      local.Item3.Add(map1);
    VehiclePawnWithMap vehicle;
    if (map1.IsVehicleMapOf(out vehicle))
    {
      WorldObject orStashedVehicle = vehicle.VehicleCaravanOrStashedVehicle;
      if (orStashedVehicle != null)
      {
        using (IEnumerator<VehiclePawn> enumerator = VehicleCaravanHelper.get_Vehicles(orStashedVehicle).GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            VehiclePawn current = enumerator.Current;
            if (vehicle != current && current is VehiclePawnWithMap vehiclePawnWithMap)
              local.Item3.Add(vehiclePawnWithMap.VehicleMap);
          }
          goto label_30;
        }
      }
    }
    foreach (VehiclePawnWithMap vehiclePawnWithMap in VehiclePawnWithMapCache.AllVehiclesOn(map1))
    {
      if (vehiclePawnWithMap.VehicleMap != map)
        local.Item3.Add(vehiclePawnWithMap.VehicleMap);
    }
label_30:
    GenCollection.AddRange<Map>(local.Item2, local.Item3);
    return !includeItself ? local.Item3 : local.Item2;
  }

  public static IEnumerable<Map> VehicleMapsOnMap(this Map map)
  {
    VehiclePawnWithMap vehicle;
    if (map.IsVehicleMapOf(out vehicle))
    {
      WorldObject orStashedVehicle = vehicle.VehicleCaravanOrStashedVehicle;
      if (orStashedVehicle != null)
      {
        foreach (VehiclePawn vehicle1 in VehicleCaravanHelper.get_Vehicles(orStashedVehicle))
        {
          if (vehicle != vehicle1 && vehicle1 is VehiclePawnWithMap vehiclePawnWithMap)
            yield return vehiclePawnWithMap.VehicleMap;
        }
      }
    }
    else
    {
      foreach (VehiclePawnWithMap vehiclePawnWithMap in VehiclePawnWithMapCache.AllVehiclesOn(map))
        yield return vehiclePawnWithMap.VehicleMap;
    }
  }

  public static void VehicleMapsOnMap(this Map map, List<Map> list)
  {
    VehiclePawnWithMap vehicle1;
    if (map.IsVehicleMapOf(out vehicle1))
    {
      WorldObject orStashedVehicle = vehicle1.VehicleCaravanOrStashedVehicle;
      if (orStashedVehicle == null)
        return;
      foreach (VehiclePawn vehicle2 in VehicleCaravanHelper.get_Vehicles(orStashedVehicle))
      {
        if (vehicle1 != vehicle2 && vehicle2 is VehiclePawnWithMap vehiclePawnWithMap)
          list.Add(vehiclePawnWithMap.VehicleMap);
      }
    }
    else
    {
      ReadOnlySpan<VehiclePawnWithMap> readOnlySpan = VehiclePawnWithMapCache.AllVehiclesOnAsReadOnlySpan(map);
      for (int index = 0; index < readOnlySpan.Length; ++index)
      {
        VehiclePawnWithMap vehiclePawnWithMap = readOnlySpan[index];
        list.Add(vehiclePawnWithMap.VehicleMap);
      }
    }
  }

  [UsedImplicitly]
  public static Map BaseMap(this Map map)
  {
    VehiclePawnWithMap vehicle;
    return map.IsVehicleMapOf(out vehicle) && ((Thing) vehicle).Spawned ? ((Thing) vehicle).Map : map;
  }

  public static CellRect BoundsRect(this Map map, int contractedBy = 0)
  {
    VehiclePawnWithMap vehicle;
    if (!ModCompat.CompatBase<ModCompat.AsAboveSoBelow>.Active || !map.IsVehicleMapOf(out vehicle))
      return GenGrid.BoundsRect(map, contractedBy);
    IntVec3 mapSize = vehicle.MapSize;
    return new CellRect(contractedBy, contractedBy, mapSize.x - contractedBy * 2, mapSize.z - contractedBy * 2);
  }

  public static bool get_IsOnVehicleMap(Thing thing)
  {
    return thing.IsOnVehicleMapOf(out VehiclePawnWithMap _);
  }

  public static bool get_IsOnNonFocusedVehicleMap(Thing thing)
  {
    return thing.IsOnNonFocusedVehicleMapOf(out VehiclePawnWithMap _);
  }

  public static Map get_GroundMap(Thing thing) => thing.BaseMap();

  public static object get_BaseMapOrCaravan(Thing thing)
  {
    VehiclePawnWithMap vehicle;
    if (!thing.IsOnVehicleMapOf(out vehicle))
      return (object) thing.Map;
    return !((Thing) vehicle).Spawned ? (object) vehicle.VehicleCaravanOrStashedVehicle : (object) ((Thing) vehicle).Map;
  }

  public static object get_MapHeldBaseMapOrCaravan(Thing thing)
  {
    Map mapHeld = thing.MapHeld;
    VehiclePawnWithMap vehicle;
    if (!mapHeld.IsVehicleMapOf(out vehicle))
      return (object) mapHeld;
    return !((Thing) vehicle).Spawned ? (object) vehicle.VehicleCaravanOrStashedVehicle : (object) ((Thing) vehicle).Map;
  }

  public static bool IsOnVehicleMapOf(this Thing thing, out VehiclePawnWithMap vehicle)
  {
    if (thing != null)
      return thing.Map.IsVehicleMapOf(out vehicle);
    vehicle = (VehiclePawnWithMap) null;
    return false;
  }

  public static bool IsOnNonFocusedVehicleMapOf(this Thing thing, out VehiclePawnWithMap vehicle)
  {
    if (thing != null)
      return thing.Map.IsNonFocusedVehicleMapOf(out vehicle);
    vehicle = (VehiclePawnWithMap) null;
    return false;
  }

  public static Map BaseMap(this Thing thing)
  {
    VehiclePawnWithMap vehicle;
    return thing.IsOnVehicleMapOf(out vehicle) && ((Thing) vehicle).Spawned ? ((Thing) vehicle).Map : thing.Map;
  }

  public static Map MapHeldBaseMap(this Thing thing) => thing.MapHeld.BaseMap();

  public static IntVec3 get_PositionOnBaseMap(Thing thing)
  {
    VehiclePawnWithMap vehicle;
    if (!thing.IsOnVehicleMapOf(out vehicle))
      return thing.Position;
    VehiclePawnWithMapCache component = MapComponentCache<VehiclePawnWithMapCache>.GetComponent(thing.Map);
    IntVec3 baseMapCoord;
    if (component.cachedPosOnBaseMap.TryGetValue(thing, out baseMapCoord))
      return baseMapCoord;
    baseMapCoord = thing.Position.ToBaseMapCoord(vehicle);
    component.cachedPosOnBaseMap[thing] = baseMapCoord;
    return baseMapCoord;
  }

  public static IntVec3 get_PositionOnBaseMapSpawned(Thing thing)
  {
    VehiclePawnWithMap vehicle;
    if (!thing.IsOnVehicleMapOf(out vehicle) || !((Thing) vehicle).Spawned)
      return thing.Position;
    VehiclePawnWithMapCache component = MapComponentCache<VehiclePawnWithMapCache>.GetComponent(thing.Map);
    IntVec3 onBaseMapSpawned;
    if (component.cachedPosOnBaseMap.TryGetValue(thing, out onBaseMapSpawned))
      return onBaseMapSpawned;
    IntVec3 baseMapCoord = thing.Position.ToBaseMapCoord(vehicle);
    component.cachedPosOnBaseMap[thing] = baseMapCoord;
    return baseMapCoord;
  }

  public static IntVec3 get_PositionHeldOnBaseMap(Thing thing)
  {
    if (thing.Spawned)
      return VehicleMapUtility.get_PositionOnBaseMap(thing);
    IntVec3 intVec3_1 = IntVec3.Invalid;
    for (IThingHolder parentHolder = thing.ParentHolder; parentHolder != null; parentHolder = parentHolder.ParentHolder)
    {
      IntVec3 intVec3_2;
      switch (parentHolder)
      {
        case Thing thing1:
          IntVec3 positionOnBaseMap1 = VehicleMapUtility.get_PositionOnBaseMap(thing1);
          if (((IntVec3) ref positionOnBaseMap1).IsValid)
          {
            intVec3_2 = VehicleMapUtility.get_PositionOnBaseMap(thing1);
            break;
          }
          goto default;
        case ThingComp thingComp:
          IntVec3 positionOnBaseMap2 = VehicleMapUtility.get_PositionOnBaseMap((Thing) thingComp.parent);
          if (((IntVec3) ref positionOnBaseMap2).IsValid)
          {
            intVec3_2 = VehicleMapUtility.get_PositionOnBaseMap((Thing) thingComp.parent);
            break;
          }
          goto default;
        default:
          intVec3_2 = intVec3_1;
          break;
      }
      intVec3_1 = intVec3_2;
    }
    return !((IntVec3) ref intVec3_1).IsValid ? VehicleMapUtility.get_PositionOnBaseMap(thing) : intVec3_1;
  }

  public static IntVec3 get_PositionHeldOnBaseMapSpawned(Thing thing)
  {
    if (thing.Spawned)
      return VehicleMapUtility.get_PositionOnBaseMapSpawned(thing);
    IntVec3 intVec3_1 = IntVec3.Invalid;
    for (IThingHolder parentHolder = thing.ParentHolder; parentHolder != null; parentHolder = parentHolder.ParentHolder)
    {
      IntVec3 intVec3_2;
      switch (parentHolder)
      {
        case Thing thing1:
          IntVec3 onBaseMapSpawned1 = VehicleMapUtility.get_PositionOnBaseMapSpawned(thing1);
          if (((IntVec3) ref onBaseMapSpawned1).IsValid)
          {
            intVec3_2 = VehicleMapUtility.get_PositionOnBaseMapSpawned(thing1);
            break;
          }
          goto default;
        case ThingComp thingComp:
          IntVec3 onBaseMapSpawned2 = VehicleMapUtility.get_PositionOnBaseMapSpawned((Thing) thingComp.parent);
          if (((IntVec3) ref onBaseMapSpawned2).IsValid)
          {
            intVec3_2 = VehicleMapUtility.get_PositionOnBaseMapSpawned((Thing) thingComp.parent);
            break;
          }
          goto default;
        default:
          intVec3_2 = intVec3_1;
          break;
      }
      intVec3_1 = intVec3_2;
    }
    return !((IntVec3) ref intVec3_1).IsValid ? VehicleMapUtility.get_PositionOnBaseMapSpawned(thing) : intVec3_1;
  }

  public static IntVec3 PositionOnAnotherMap(this Thing thing, Map map)
  {
    VehiclePawnWithMap vehicle;
    return !map.IsVehicleMapOf(out vehicle) ? VehicleMapUtility.get_PositionOnBaseMap(thing) : VehicleMapUtility.get_PositionOnBaseMap(thing).ToVehicleMapCoord(vehicle);
  }

  public static IntVec3 PositionOnAnotherThingMap(this Thing thing, Thing another)
  {
    VehiclePawnWithMap vehicle;
    return !another.IsOnVehicleMapOf(out vehicle) ? VehicleMapUtility.get_PositionOnBaseMap(thing) : ModCompat.AsAboveSoBelow.TranslateToThingBand(VehicleMapUtility.get_PositionOnBaseMap(thing).ToVehicleMapCoord(vehicle), another);
  }

  public static Rot4 BaseRotation(this Thing thing)
  {
    VehiclePawnWithMap vehicle;
    if (!thing.IsOnNonFocusedVehicleMapOf(out vehicle))
      return thing.Rotation;
    Rot4 rotation = thing.Rotation;
    int asInt1 = ((Rot4) ref rotation).AsInt;
    rotation = ((Thing) vehicle).Rotation;
    int asInt2 = ((Rot4) ref rotation).AsInt;
    return new Rot4(asInt1 + asInt2);
  }

  public static Rot4 BaseRotationSpawned(this Thing thing)
  {
    VehiclePawnWithMap vehicle;
    if (!thing.IsOnNonFocusedVehicleMapOf(out vehicle) || !((Thing) vehicle).Spawned)
      return thing.Rotation;
    Rot4 rotation = thing.Rotation;
    int asInt1 = ((Rot4) ref rotation).AsInt;
    rotation = ((Thing) vehicle).Rotation;
    int asInt2 = ((Rot4) ref rotation).AsInt;
    return new Rot4(asInt1 + asInt2);
  }

  public static Rot4 BaseRotationVehicleDraw(this Thing thing)
  {
    VehiclePawnWithMap vehicle;
    if (!thing.IsOnNonFocusedVehicleMapOf(out vehicle))
      return thing.Rotation;
    Rot4 rot4 = thing.Rotation;
    int asInt1 = ((Rot4) ref rot4).AsInt;
    rot4 = vehicle.FullRotation.RotForVehicleDraw();
    int asInt2 = ((Rot4) ref rot4).AsInt;
    return new Rot4(asInt1 + asInt2);
  }

  public static Rot8 BaseFullRotation(this Thing thing)
  {
    VehiclePawnWithMap vehicle;
    return thing.IsOnNonFocusedVehicleMapOf(out vehicle) ? new Rot8(thing.Rotation).Rotated(vehicle.FullRotation) : Rot8.op_Implicit(thing.Rotation);
  }

  public static Rot8 BaseFullRotationSpawned(this Thing thing)
  {
    VehiclePawnWithMap vehicle;
    return thing.IsOnNonFocusedVehicleMapOf(out vehicle) && ((Thing) vehicle).Spawned ? new Rot8(thing.Rotation).Rotated(vehicle.FullRotation) : Rot8.op_Implicit(thing.Rotation);
  }

  public static Rot4 BaseFullRotationAsRot4(this Thing thing)
  {
    return thing.BaseFullRotation().AsRot4Force();
  }

  public static Rot8 BaseFullRotationDoor(this Thing thing)
  {
    VehiclePawnWithMap vehicle;
    if (!thing.IsOnNonFocusedVehicleMapOf(out vehicle))
      return Rot8.op_Implicit(thing.Rotation);
    Rot8 rot8 = new Rot8(thing.Rotation).Rotated(vehicle.FullRotation);
    return ((Rot8) ref rot8).FacingCell.z >= 0 ? rot8 : ((Rot8) ref rot8).Opposite;
  }

  public static bool TryGetDrawPos(this Thing thing, ref Vector3 result)
  {
    if (VehicleSectionLayerManager.CacheMode)
    {
      if (thing.def.category == 2 && StoreUtility.GetSlotGroup(thing)?.parent is Building_Hatch)
      {
        result = Vector3.negativeInfinity;
        return true;
      }
      if (!(thing is Building_GravshipWheel buildingGravshipWheel) || buildingGravshipWheel.CacheMode)
        return false;
      result = thing.DrawPos;
      return true;
    }
    Map map = thing.Map;
    VehiclePawnWithMap vehicle;
    if (!map.IsNonFocusedVehicleMapOf(out vehicle) || VehiclePawnWithMapCache.CacheMode)
      return false;
    VehiclePawnWithMapCache component = MapComponentCache<VehiclePawnWithMapCache>.GetComponent(map);
    if (!component.cachedDrawPos.TryGetValue(thing, out result))
    {
      try
      {
        VehiclePawnWithMapCache.CacheMode = true;
        result = thing.DrawPos.ToBaseMapCoord(vehicle);
        if (ModCompat.CompatBase<ModCompat.AsAboveSoBelow>.Active)
        {
          MapComponent mapComponent = ModCompat.AsAboveSoBelow.CompOf(map);
          if (mapComponent != null && ModCompat.AsAboveSoBelow.Banded(mapComponent) && ModCompat.AsAboveSoBelow.CurrentBand(map) > ModCompat.AsAboveSoBelow.BandOf(mapComponent, thing.Position))
            result.y = Altitudes.AltitudeFor((AltitudeLayer) 2).YOffsetFull(vehicle);
        }
        component.cachedDrawPos[thing] = result;
      }
      finally
      {
        VehiclePawnWithMapCache.CacheMode = false;
      }
    }
    return true;
  }

  public static void VirtualMapTransfer(this Thing thing, Map map)
  {
    if (thing == null || map == null)
      return;
    VirtualTeleporter.mapIndexOrState.Invoke(thing) = (sbyte) map.Index;
  }

  public static void VirtualMapTransfer(this Thing thing, Map map, IntVec3 c)
  {
    if (thing == null)
      return;
    if (map != null)
      VirtualTeleporter.mapIndexOrState.Invoke(thing) = (sbyte) map.Index;
    thing.SetPositionDirect(c);
  }

  public static CellRect MovedOccupiedDrawRect(this Thing thing)
  {
    Vector2 drawSize = thing.DrawSize;
    return GenAdj.OccupiedRect(VehicleMapUtility.get_PositionOnBaseMap(thing), thing.BaseRotation(), new IntVec2(Mathf.CeilToInt(drawSize.x), Mathf.CeilToInt(drawSize.y)));
  }

  public static Rot4 RotationForPrint(this Thing thing)
  {
    Rot4 rot = thing.Rotation;
    if (Rot4.op_Inequality(VehicleSectionLayerManager.RotForPrint, Rot4.North))
    {
      if (thing.def.size.x == thing.def.size.z && !thing.def.rotatable)
      {
        GraphicData graphicData = thing.def.graphicData;
        if ((graphicData != null ? (graphicData.drawRotated ? 1 : 0) : 0) == 0 || !(thing.Graphic is Graphic_Multi) || SameMaterialByRot())
          goto label_4;
      }
      ref Rot4 local = ref rot;
      int asInt1 = ((Rot4) ref local).AsInt;
      Rot4 rotForPrint = VehicleSectionLayerManager.RotForPrint;
      int asInt2 = ((Rot4) ref rotForPrint).AsInt;
      ((Rot4) ref local).AsInt = asInt1 + asInt2;
    }
label_4:
    return rot;

    bool SameMaterialByRot()
    {
      Graphic graphic = thing.Graphic;
      Rot4 rot4;
      ref Rot4 local = ref rot4;
      int asInt1 = ((Rot4) ref rot).AsInt;
      Rot4 rotForPrint = VehicleSectionLayerManager.RotForPrint;
      int asInt2 = ((Rot4) ref rotForPrint).AsInt;
      int num = asInt1 + asInt2;
      // ISSUE: explicit constructor call
      ((Rot4) ref local).\u002Ector(num);
      return graphic != null && Object.op_Equality((Object) graphic.MatAt(rot, thing), (Object) graphic.MatAt(rot4, thing)) && Vector3.op_Equality(graphic.DrawOffset(rot), graphic.DrawOffset(rot4));
    }
  }

  public static CellRect MovedOccupiedRect(this Thing thing)
  {
    IntVec2 size = thing.def.size;
    return GenAdj.OccupiedRect(VehicleMapUtility.get_PositionOnBaseMap(thing), thing.BaseRotation(), new IntVec2(Mathf.CeilToInt((float) size.x), Mathf.CeilToInt((float) size.z)));
  }

  public static Map get_LordMapOrMapHeld(Pawn pawn)
  {
    return LordUtility.GetLord(pawn)?.Map ?? ((Thing) pawn).MapHeld;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float YOffsetFull(this float original, VehiclePawnWithMap vehicle)
  {
    return (float) ((double) original / 39.999996185302734 + (double) vehicle.cachedDrawPos.y + 0.00091463426360860467);
  }

  public static float FlipAngle(this float original, VehiclePawn vehicle)
  {
    return !VehicleMapUtility.VFLatestRelease.Available || !((Graphic) vehicle.VehicleGraphic).WestFlipped || !Rot4.op_Equality(((Thing) vehicle).BaseRotation(), Rot4.West) ? original : -original;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float YOffset(this float original)
  {
    return (float) ((double) original / 39.999996185302734 + 0.00091463426360860467);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 YOffset(this Vector3 original)
  {
    return Vector3Utility.WithY(original, original.y.YOffset());
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 YOffsetFull(this Vector3 original, VehiclePawnWithMap vehicle)
  {
    return Vector3Utility.WithY(original, original.y.YOffsetFull(vehicle));
  }

  public static Vector3 ToVehicleMapCoord(this Vector3 original)
  {
    if (Command_FocusVehicleMap.FocusedVehicle != null)
      return original.ToVehicleMapCoord(Command_FocusVehicleMap.FocusedVehicle);
    VehiclePawnWithMap vehicle;
    return VehicleMapFramework.VehicleMapFramework.settings.drawPlanet && Find.CurrentMap.IsVehicleMapOf(out VehiclePawnWithMap _) && UI.MouseMapPosition().TryGetVehicleMap(Find.CurrentMap, out vehicle) ? original.ToVehicleMapCoord(vehicle) : original;
  }

  public static Vector3 ToVehicleMapCoord(this Vector3 original, VehiclePawnWithMap vehicle)
  {
    Vector3 vector3_1 = Vector3.op_Addition(vehicle.cachedDrawPos, VehicleMapUtility.OffsetFor(vehicle));
    Vector3 vector3_2 = VehicleMapUtility.MapPivot(vehicle.VehicleMap);
    return Vector3.op_Addition(Vector3Utility.RotatedBy(Vector3.op_Subtraction(original, vector3_1), -VehicleMapUtility.get_FullAngle((VehiclePawn) vehicle)), vector3_2);
  }

  public static Vector3 ToNonFocusedThingMapCoord(this Vector3 original, Thing thing)
  {
    VehiclePawnWithMap vehicle;
    return !thing.IsOnNonFocusedVehicleMapOf(out vehicle) ? original : original.ToVehicleMapCoord(vehicle);
  }

  public static Vector3 ToBaseMapCoord(this Vector3 original)
  {
    if (Command_FocusVehicleMap.FocusedVehicle != null)
      return Vector3Utility.WithY(original.ToBaseMapCoord(Command_FocusVehicleMap.FocusedVehicle), original.y);
    VehiclePawnWithMap vehicle;
    return VehicleMapFramework.VehicleMapFramework.settings.drawPlanet && Find.CurrentMap.IsVehicleMapOf(out VehiclePawnWithMap _) && UI.MouseMapPosition().TryGetVehicleMap(Find.CurrentMap, out vehicle) ? Vector3Utility.WithY(original.ToBaseMapCoord(vehicle), original.y) : original;
  }

  public static Vector3 ToBaseMapCoord(this Vector3 original, Map map)
  {
    VehiclePawnWithMap vehicle;
    return !map.IsNonFocusedVehicleMapOf(out vehicle) ? original : original.ToBaseMapCoord(vehicle);
  }

  public static Vector3 ToBaseMapCoord(this Vector3 original, VehiclePawnWithMap vehicle)
  {
    Vector3 cachedDrawPos = vehicle.cachedDrawPos;
    Vector3 vector3 = VehicleMapUtility.MapPivot(vehicle.VehicleMap, IntVec3Utility.ToIntVec3(original));
    return Vector3.op_Addition(Vector3.op_Addition(Vector3Utility.RotatedBy(Vector3.op_Subtraction(original.YOffset(), vector3), VehicleMapUtility.get_FullAngle((VehiclePawn) vehicle)), cachedDrawPos), VehicleMapUtility.OffsetFor(vehicle));
  }

  public static Vector3 ToBaseMapCoord(this Vector3 original, VehiclePawnWithMap vehicle, Rot8 rot)
  {
    Vector3 cachedDrawPos = vehicle.cachedDrawPos;
    Vector3 vector3 = VehicleMapUtility.MapPivot(vehicle.VehicleMap, IntVec3Utility.ToIntVec3(original));
    return Vector3.op_Addition(Vector3.op_Addition(Vector3Utility.RotatedBy(Vector3.op_Subtraction(original.YOffset(), vector3), ((Rot8) ref rot).AsAngle), cachedDrawPos), VehicleMapUtility.OffsetFor(vehicle, rot));
  }

  public static bool TryGetVehicleMap(
    this Vector3 original,
    Map map,
    out VehiclePawnWithMap vehicle,
    VehicleMapFlag flag = VehicleMapFlag.StructureCells)
  {
    vehicle = (VehiclePawnWithMap) null;
    if (map == null)
      return false;
    VehiclePawnWithMap vehicle1;
    bool flag1 = map.IsVehicleMapOf(out vehicle1);
    WorldObject orStashedVehicle = vehicle1?.VehicleCaravanOrStashedVehicle;
    if (flag1 && orStashedVehicle == null && VehicleMapFramework.VehicleMapFramework.settings.drawPlanet && original.TryGetVehicleMap(vehicle1, flag))
    {
      vehicle = vehicle1;
      return true;
    }
    IEnumerable<VehiclePawnWithMap> vehiclePawnWithMaps = flag1 ? VehicleCaravanHelper.get_Vehicles(orStashedVehicle).OfType<VehiclePawnWithMap>() : (IEnumerable<VehiclePawnWithMap>) VehiclePawnWithMapCache.AllVehiclesOn(map);
    float num1 = float.MaxValue;
    foreach (VehiclePawnWithMap vehicle2 in vehiclePawnWithMaps)
    {
      if (original.TryGetVehicleMap(vehicle2, flag))
      {
        float num2 = GenGeo.MagnitudeHorizontalSquared(Vector3.op_Subtraction(vehicle2.cachedDrawPos, original));
        if ((double) num2 < (double) num1)
        {
          num1 = num2;
          vehicle = vehicle2;
        }
      }
    }
    return vehicle != null;
  }

  public static bool TryGetVehicleMap(
    this Vector3 original,
    VehiclePawnWithMap vehicle,
    VehicleMapFlag flag = VehicleMapFlag.StructureCells)
  {
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(0.0f, 0.0f, (float) vehicle.MapSize.x, (float) vehicle.MapSize.z);
    Vector3 vehicleMapCoordLocal = ToVehicleMapCoordLocal(original, vehicle);
    if (!((Rect) ref rect).Contains(new Vector2(vehicleMapCoordLocal.x, vehicleMapCoordLocal.z)))
      return false;
    IntVec3 intVec3 = IntVec3Utility.ToIntVec3(vehicleMapCoordLocal);
    if (!GenGrid.InBounds(intVec3, vehicle.VehicleMap))
      return false;
    if (!vehicle.ImpassableCellGrid[intVec3])
      return true;
    bool flag1 = vehicle.EmptyStructureGrid[intVec3];
    bool flag2 = vehicle.ExpandableGrid[intVec3];
    bool flag3 = vehicle.OutOfBoundsGrid[intVec3];
    return (flag & VehicleMapFlag.StructureCells) > VehicleMapFlag.None && !flag1 && !flag2 && !flag3 || (flag & VehicleMapFlag.ExpandableCells) > VehicleMapFlag.None & flag2 || (flag & VehicleMapFlag.OutOfBoundsCells) > VehicleMapFlag.None & flag3;

    static Vector3 ToVehicleMapCoordLocal(Vector3 o, VehiclePawnWithMap v)
    {
      Vector3 vector3_1 = Vector3.op_Addition(v.cachedDrawPos, VehicleMapUtility.OffsetFor(v));
      IntVec3 mapSize = v.MapSize;
      Vector3 vector3_2;
      // ISSUE: explicit constructor call
      ((Vector3) ref vector3_2).\u002Ector((float) mapSize.x / 2f, 0.0f, (float) mapSize.z / 2f);
      return Vector3.op_Addition(Vector3Utility.RotatedBy(Vector3.op_Subtraction(o, vector3_1), -VehicleMapUtility.get_FullAngle((VehiclePawn) v)), vector3_2);
    }
  }

  public static Vector3 ToThingBaseMapCoord(this Vector3 original, Thing thing)
  {
    VehiclePawnWithMap vehicle;
    return !thing.IsOnVehicleMapOf(out vehicle) ? original : original.ToBaseMapCoord(vehicle);
  }

  public static IntVec3 ToBaseMapCoord(this IntVec3 original, VehiclePawnWithMap vehicle)
  {
    Vector3 cachedExactPos = vehicle.cachedExactPos;
    Vector3 vector3 = VehicleMapUtility.MapPivot(vehicle.VehicleMap, original);
    return IntVec3Utility.ToIntVec3(Vector3.op_Addition(Vector3.op_Addition(Vector3Utility.RotatedBy(Vector3.op_Subtraction(((IntVec3) ref original).ToVector3Shifted(), vector3), VehicleMapUtility.get_FullAngle((VehiclePawn) vehicle)), cachedExactPos), VehicleMapUtility.OffsetFor(vehicle)));
  }

  public static IntVec3 ToBaseMapCoord(this IntVec3 original, Map map)
  {
    VehiclePawnWithMap vehicle;
    return !map.IsVehicleMapOf(out vehicle) ? original : original.ToBaseMapCoord(vehicle);
  }

  public static IntVec3 ToVehicleMapCoord(this IntVec3 original, VehiclePawnWithMap vehicle)
  {
    Vector3 vector3_1 = Vector3.op_Addition(vehicle.cachedExactPos, VehicleMapUtility.OffsetFor(vehicle));
    IntVec3 mapSize = vehicle.MapSize;
    Vector3 vector3_2;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3_2).\u002Ector((float) mapSize.x / 2f, 0.0f, (float) mapSize.z / 2f);
    return IntVec3Utility.ToIntVec3(Vector3.op_Addition(Vector3Utility.RotatedBy(Vector3.op_Subtraction(((IntVec3) ref original).ToVector3Shifted(), vector3_1), -VehicleMapUtility.get_FullAngle((VehiclePawn) vehicle)), vector3_2));
  }

  public static IntVec3 ToThingMapCoord(this IntVec3 original, Thing thing)
  {
    return original.ToVehicleMapCoord(thing.Map);
  }

  public static IntVec3 ToVehicleMapCoord(this IntVec3 original, Map map)
  {
    VehiclePawnWithMap vehicle;
    return !map.IsVehicleMapOf(out vehicle) ? original : original.ToVehicleMapCoord(vehicle);
  }

  public static IntVec3 ToThingBaseMapCoord(this IntVec3 original, Thing thing)
  {
    VehiclePawnWithMap vehicle;
    return !thing.IsOnVehicleMapOf(out vehicle) ? original : original.ToBaseMapCoord(vehicle);
  }

  public static IntVec2 ToHitCell(this IntVec3 original, VehiclePawnWithMap vehicle)
  {
    IntVec3 intVec3 = IntVec3Utility.ToIntVec3(Vector3.op_Subtraction(((IntVec3) ref original).ToVector3Shifted(), VehicleMapUtility.OffsetFor(vehicle, Rot8.North)));
    return ((IntVec3) ref intVec3).ToIntVec2;
  }

  public static IntVec3 ClosestEdgeCell(this IntVec3 original, VehiclePawnWithMap vehicle)
  {
    if (vehicle.CachedMapEdgeCells.Count == 0)
      return IntVec3.Invalid;
    IntVec3 vehicleMapCoord = original.ToVehicleMapCoord(vehicle);
    CellRect validMapRect = vehicle.ValidMapRect;
    CellRect cellRect = ((CellRect) ref validMapRect).ExpandedBy(1);
    IntVec3 intVec3_1 = ((CellRect) ref cellRect).ClosestCellTo(vehicleMapCoord);
    IntVec3 intVec3_2 = IntVec3.op_Subtraction(((CellRect) ref cellRect).GetCorner(Rot4.North, true), ((CellRect) ref cellRect).GetCorner(Rot4.South, true));
    float lengthHorizontal = ((IntVec3) ref intVec3_2).LengthHorizontal;
    IntRange indexRange;
    IntVec3[] intVec3Array = GenRadialDirectional.PatternFor(vehicleMapCoord, vehicle.ValidMapRect, 0.0f, lengthHorizontal, out indexRange);
    for (int min = indexRange.min; min < indexRange.max; ++min)
    {
      IntVec3 intVec3_3 = IntVec3.op_Addition(intVec3_1, intVec3Array[min]);
      if (vehicle.CachedMapEdgeCells.Contains(intVec3_3))
        return intVec3_3;
    }
    return IntVec3.Invalid;
  }

  public static IntVec3 ClosestMapEdgeCell(this IntVec3 original, VehiclePawnWithMap vehicle)
  {
    if (vehicle.CachedMapEdgeCells.Count == 0)
      return IntVec3.Invalid;
    IntVec3 vehicleMapCoord = original.ToVehicleMapCoord(vehicle);
    CellRect validMapRect = vehicle.ValidMapRect;
    CellRect cellRect = ((CellRect) ref validMapRect).ExpandedBy(1);
    IntVec3 intVec3_1 = ((CellRect) ref cellRect).ClosestCellTo(vehicleMapCoord);
    if (IntVec3.op_Equality(vehicleMapCoord, intVec3_1) || vehicle.CachedMapEdgeCells.Contains(intVec3_1))
      return intVec3_1;
    IntVec3 intVec3_2 = IntVec3.op_Subtraction(((CellRect) ref cellRect).GetCorner(Rot4.North, true), ((CellRect) ref cellRect).GetCorner(Rot4.South, true));
    float lengthHorizontal = ((IntVec3) ref intVec3_2).LengthHorizontal;
    IntRange indexRange;
    IntVec3[] intVec3Array = GenRadialDirectional.PatternFor(vehicleMapCoord, vehicle.ValidMapRect, 0.0f, lengthHorizontal, out indexRange);
    for (int min = indexRange.min; min < indexRange.max; ++min)
    {
      IntVec3 intVec3_3 = IntVec3.op_Addition(intVec3_1, intVec3Array[min]);
      if (vehicle.CachedMapEdgeCells.Contains(intVec3_3))
        return intVec3_3;
    }
    return IntVec3.Invalid;
  }

  public static IntVec3 ClosestWalkableEdgeCell(
    this IntVec3 original,
    VehiclePawnWithMap vehicle,
    int districtID = -1)
  {
    if (vehicle.CachedWalkableMapEdgeCells.Count == 0)
      return IntVec3.Invalid;
    IntVec3 vehicleMapCoord = original.ToVehicleMapCoord(vehicle);
    CellRect validMapRect = vehicle.ValidMapRect;
    CellRect cellRect = ((CellRect) ref validMapRect).ExpandedBy(1);
    IntVec3 key1 = ((CellRect) ref cellRect).ClosestCellTo(vehicleMapCoord);
    District district;
    if (IntVec3.op_Equality(vehicleMapCoord, key1) || vehicle.CachedWalkableMapEdgeCells.TryGetValue(key1, out district) && (districtID == -1 || district.ID == districtID))
      return key1;
    IntVec3 intVec3 = IntVec3.op_Subtraction(((CellRect) ref cellRect).GetCorner(Rot4.North, true), ((CellRect) ref cellRect).GetCorner(Rot4.South, true));
    float lengthHorizontal = ((IntVec3) ref intVec3).LengthHorizontal;
    IntRange indexRange;
    IntVec3[] intVec3Array = GenRadialDirectional.PatternFor(vehicleMapCoord, vehicle.ValidMapRect, 0.0f, lengthHorizontal, out indexRange);
    for (int min = indexRange.min; min < indexRange.max; ++min)
    {
      IntVec3 key2 = IntVec3.op_Addition(key1, intVec3Array[min]);
      if (vehicle.CachedWalkableMapEdgeCells.TryGetValue(key2, out district) && (districtID == -1 || district.ID == districtID))
        return key2;
    }
    return IntVec3.Invalid;
  }

  public static IntVec3 CellOnBaseMap(ref this LocalTargetInfo target)
  {
    return !((LocalTargetInfo) ref target).HasThing ? ((LocalTargetInfo) ref target).Cell : VehicleMapUtility.get_PositionOnBaseMap(((LocalTargetInfo) ref target).Thing);
  }

  public static IntVec3 CellOnBaseMapSpawned(ref this LocalTargetInfo target)
  {
    return !((LocalTargetInfo) ref target).HasThing ? ((LocalTargetInfo) ref target).Cell : VehicleMapUtility.get_PositionOnBaseMapSpawned(((LocalTargetInfo) ref target).Thing);
  }

  public static IntVec3 get_CellOnGroundMap(TargetInfo target)
  {
    if (((TargetInfo) ref target).HasThing)
      return VehicleMapUtility.get_PositionOnBaseMap(((TargetInfo) ref target).Thing);
    VehiclePawnWithMap vehicle;
    return !((TargetInfo) ref target).Map.IsVehicleMapOf(out vehicle) ? ((TargetInfo) ref target).Cell : ((TargetInfo) ref target).Cell.ToBaseMapCoord(vehicle);
  }

  public static Vector3 get_CenterVector3OnGroundMap(TargetInfo target)
  {
    if (((TargetInfo) ref target).HasThing)
    {
      if (((TargetInfo) ref target).Thing.Spawned)
        return ((TargetInfo) ref target).Thing.DrawPos;
      Vector3? drawPosHeld = ((TargetInfo) ref target).Thing.DrawPosHeld;
      if (drawPosHeld.HasValue)
        return drawPosHeld.GetValueOrDefault();
      IntVec3 position = ((TargetInfo) ref target).Thing.Position;
      return ((IntVec3) ref position).ToVector3Shifted().ToThingBaseMapCoord(((TargetInfo) ref target).Thing);
    }
    IntVec3 cell = ((TargetInfo) ref target).Cell;
    if (!((IntVec3) ref cell).IsValid)
      return new Vector3();
    cell = ((TargetInfo) ref target).Cell;
    return ((IntVec3) ref cell).ToVector3Shifted().ToBaseMapCoord(((TargetInfo) ref target).Map);
  }

  public static IntVec3 CellOnBaseMap(ref this TargetInfo target)
  {
    if (((TargetInfo) ref target).HasThing)
      return VehicleMapUtility.get_PositionOnBaseMap(((TargetInfo) ref target).Thing);
    VehiclePawnWithMap vehicle;
    return !((TargetInfo) ref target).Map.IsVehicleMapOf(out vehicle) ? ((TargetInfo) ref target).Cell : ((TargetInfo) ref target).Cell.ToBaseMapCoord(vehicle);
  }

  public static IntVec3 CellOnBaseMapSpawned(ref this TargetInfo target)
  {
    if (((TargetInfo) ref target).HasThing)
      return VehicleMapUtility.get_PositionOnBaseMapSpawned(((TargetInfo) ref target).Thing);
    VehiclePawnWithMap vehicle;
    return !((TargetInfo) ref target).Map.IsVehicleMapOf(out vehicle) || !((Thing) vehicle).Spawned ? ((TargetInfo) ref target).Cell : ((TargetInfo) ref target).Cell.ToBaseMapCoord(vehicle);
  }

  public static Map BaseMap(ref this TargetInfo target) => ((TargetInfo) ref target).Map.BaseMap();

  public static IntVec3 CellOnBaseMap(ref this GlobalTargetInfo target)
  {
    VehiclePawnWithMap vehicle;
    return !((GlobalTargetInfo) ref target).Map.IsVehicleMapOf(out vehicle) ? ((GlobalTargetInfo) ref target).Cell : ((GlobalTargetInfo) ref target).Cell.ToBaseMapCoord(vehicle);
  }

  public static IntVec3 CellOnBaseMapSpawned(ref this GlobalTargetInfo target)
  {
    VehiclePawnWithMap vehicle;
    return !((GlobalTargetInfo) ref target).Map.IsVehicleMapOf(out vehicle) || !((Thing) vehicle).Spawned ? ((GlobalTargetInfo) ref target).Cell : ((GlobalTargetInfo) ref target).Cell.ToBaseMapCoord(vehicle);
  }

  public static Map BaseMap(ref this GlobalTargetInfo target)
  {
    return ((GlobalTargetInfo) ref target).Map.BaseMap();
  }

  public static Rot4 DirectionToInsideMap(this IntVec3 c, VehiclePawnWithMap vehicle)
  {
    CellRect validMapRect = vehicle.ValidMapRect;
    Rot4 closestEdge = ((CellRect) ref validMapRect).GetClosestEdge(c);
    return ((Rot4) ref closestEdge).Opposite;
  }

  public static Rot8 BaseFullDirectionToInsideMap(this IntVec3 c, VehiclePawnWithMap vehicle)
  {
    Rot4 insideMap = c.DirectionToInsideMap(vehicle);
    return Find.CurrentMap != vehicle.VehicleMap || VehicleMapFramework.VehicleMapFramework.settings.drawPlanet ? new Rot8(insideMap).Rotated(vehicle.FullRotation) : Rot8.op_Implicit(insideMap);
  }

  public static float get_FullAngle(VehiclePawn vehicle)
  {
    Rot8 fullRotation = vehicle.FullRotation;
    return Ext_Math.RotateAngle(((Rot8) ref fullRotation).AsAngle, vehicle.Transform.rotation);
  }

  public static Quaternion get_FullAngleQuat(VehiclePawn vehicle)
  {
    return Quaternion.AngleAxis(VehicleMapUtility.get_FullAngle(vehicle), Vector3.up);
  }

  public static float get_ExtraAngle(VehiclePawn vehicle)
  {
    double fullAngle = (double) VehicleMapUtility.get_FullAngle(vehicle);
    Rot4 rot4 = vehicle.FullRotation.RotForVehicleDraw();
    double asAngle = (double) ((Rot4) ref rot4).AsAngle;
    return Mathf.Repeat((float) (fullAngle - asAngle), 360f);
  }

  public static int HalfLength(this VehiclePawn vehicle) => vehicle.VehicleDef.HalfLength();

  public static bool TryGetFullRotation(this VehiclePawn vehicle, ref Rot8 rot)
  {
    Map map = ((Thing) vehicle).Map;
    if (!map.IsNonFocusedVehicleMapOf(out VehiclePawnWithMap _))
      return false;
    VehiclePawnWithMapCache component = MapComponentCache<VehiclePawnWithMapCache>.GetComponent(map);
    if (!component.cachedFullRot.TryGetValue(vehicle, out rot))
    {
      rot = vehicle.BaseFullRotation();
      component.cachedFullRot[vehicle] = rot;
    }
    return true;
  }

  public static Rot8 BaseFullRotation(this VehiclePawn vehicle)
  {
    if (!((GraphicData) vehicle.VehicleDef.graphicData).drawRotated)
      return Rot8.North;
    Rot8 rot;
    // ISSUE: explicit constructor call
    ((Rot8) ref rot).\u002Ector(((Thing) vehicle).Rotation, vehicle.Angle);
    VehiclePawnWithMap vehicle1;
    if (((Thing) vehicle).IsOnNonFocusedVehicleMapOf(out vehicle1))
      rot = rot.Rotated(vehicle1.FullRotation);
    return rot;
  }

  public static Pawn GetFirstPawnAcrossMaps(this IntVec3 c, Map map)
  {
    foreach (Thing thingListAcrossMap in c.GetThingListAcrossMaps(map))
    {
      if (thingListAcrossMap is Pawn firstPawnAcrossMaps)
        return firstPawnAcrossMaps;
    }
    return (Pawn) null;
  }

  public static Thing GetCoverOnThingMap(this IntVec3 c, Map map, Thing thing)
  {
    Map mapHeld = thing?.MapHeld;
    if (mapHeld == null)
      return GridsUtility.GetCover(c, map);
    IntVec3 baseMapCoord = c.ToBaseMapCoord(mapHeld);
    return !GenGrid.InBounds(baseMapCoord, mapHeld) ? GridsUtility.GetCover(c, map) : GridsUtility.GetCover(baseMapCoord, mapHeld);
  }

  public static bool RoofedAcrossMaps(this IntVec3 c, Map map)
  {
    if (GridsUtility.Roofed(c, map))
      return true;
    VehiclePawnWithMap vehicle1;
    if (map.IsVehicleMapOf(out vehicle1) && ((Thing) vehicle1).Spawned)
      return GridsUtility.Roofed(c.ToBaseMapCoord(vehicle1), ((Thing) vehicle1).Map);
    VehiclePawnWithMap vehicle2 = ComponentCache.GetCachedMapComponent<VehicleMapGrid>(map).VehicleAt(c);
    return vehicle2 != null && GridsUtility.Roofed(c.ToVehicleMapCoord(vehicle2), vehicle2.VehicleMap);
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u0024BB8179112D74E5BE27AC2ADC29C08E52
  {
    [ExtensionMarker("<M>$D90418F0C67860308C33845B4AA8491F")]
    public bool IsVehicleMap
    {
      [ExtensionMarker("<M>$D90418F0C67860308C33845B4AA8491F")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$D90418F0C67860308C33845B4AA8491F")]
    public bool IsNonFocusedVehicleMap
    {
      [ExtensionMarker("<M>$D90418F0C67860308C33845B4AA8491F")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$D90418F0C67860308C33845B4AA8491F")]
    public bool CrossMapContext
    {
      [ExtensionMarker("<M>$D90418F0C67860308C33845B4AA8491F")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$D90418F0C67860308C33845B4AA8491F")]
    public Map GroundMap
    {
      [ExtensionMarker("<M>$D90418F0C67860308C33845B4AA8491F")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$D90418F0C67860308C33845B4AA8491F")]
    public object BaseMapOrCaravan
    {
      [ExtensionMarker("<M>$D90418F0C67860308C33845B4AA8491F")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$D90418F0C67860308C33845B4AA8491F")]
    [UsedImplicitly]
    [ContractAnnotation("=> true, vehicle:notnull; => false, vehicle:null")]
    public bool IsVehicleMapOf([CanBeNull] out VehiclePawnWithMap vehicle)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$D90418F0C67860308C33845B4AA8491F")]
    [ContractAnnotation("=> true, vehicle:notnull; => false, vehicle:null")]
    public bool IsNonFocusedVehicleMapOf([CanBeNull] out VehiclePawnWithMap vehicle)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$D90418F0C67860308C33845B4AA8491F")]
    [UsedImplicitly]
    public IEnumerable<Map> BaseMapAndVehicleMaps() => throw new NotSupportedException();

    [ExtensionMarker("<M>$D90418F0C67860308C33845B4AA8491F")]
    public HashSet<Map> BaseMapAndVehicleMaps(bool includeItself)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$D90418F0C67860308C33845B4AA8491F")]
    public IEnumerable<Map> VehicleMapsOnMap() => throw new NotSupportedException();

    [ExtensionMarker("<M>$D90418F0C67860308C33845B4AA8491F")]
    public void VehicleMapsOnMap(List<Map> list) => throw new NotSupportedException();

    [ExtensionMarker("<M>$D90418F0C67860308C33845B4AA8491F")]
    [UsedImplicitly]
    public Map BaseMap() => throw new NotSupportedException();

    [ExtensionMarker("<M>$D90418F0C67860308C33845B4AA8491F")]
    public CellRect BoundsRect(int contractedBy = 0) => throw new NotSupportedException();

    [SpecialName]
    public static class \u003CM\u003E\u0024D90418F0C67860308C33845B4AA8491F
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(Map map)
      {
      }
    }
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u00249EA7376D1A13FE36E57A42DEEFA9C1DE
  {
    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public bool IsOnVehicleMap
    {
      [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public bool IsOnNonFocusedVehicleMap
    {
      [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public Map GroundMap
    {
      [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public object BaseMapOrCaravan
    {
      [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public object MapHeldBaseMapOrCaravan
    {
      [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public bool IsOnVehicleMapOf(out VehiclePawnWithMap vehicle)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public bool IsOnNonFocusedVehicleMapOf(out VehiclePawnWithMap vehicle)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public Map BaseMap() => throw new NotSupportedException();

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public Map MapHeldBaseMap() => throw new NotSupportedException();

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public IntVec3 PositionOnBaseMap
    {
      [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public IntVec3 PositionOnBaseMapSpawned
    {
      [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public IntVec3 PositionHeldOnBaseMap
    {
      [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public IntVec3 PositionHeldOnBaseMapSpawned
    {
      [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public IntVec3 PositionOnAnotherMap(Map map) => throw new NotSupportedException();

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public IntVec3 PositionOnAnotherThingMap(Thing another) => throw new NotSupportedException();

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public Rot4 BaseRotation() => throw new NotSupportedException();

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public Rot4 BaseRotationSpawned() => throw new NotSupportedException();

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public Rot4 BaseRotationVehicleDraw() => throw new NotSupportedException();

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public Rot8 BaseFullRotation() => throw new NotSupportedException();

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public Rot8 BaseFullRotationSpawned() => throw new NotSupportedException();

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public Rot4 BaseFullRotationAsRot4() => throw new NotSupportedException();

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public Rot8 BaseFullRotationDoor() => throw new NotSupportedException();

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public bool TryGetDrawPos(ref Vector3 result) => throw new NotSupportedException();

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public void VirtualMapTransfer(Map map) => throw new NotSupportedException();

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public void VirtualMapTransfer(Map map, IntVec3 c) => throw new NotSupportedException();

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public CellRect MovedOccupiedDrawRect() => throw new NotSupportedException();

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public Rot4 RotationForPrint() => throw new NotSupportedException();

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public CellRect MovedOccupiedRect() => throw new NotSupportedException();

    [SpecialName]
    public static class \u003CM\u003E\u0024DFCC9EE29BE0341F99924D3A409FC2EE
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(Thing thing)
      {
      }
    }
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u00242F59771236D7C1AA57ADCD68358D448A
  {
    [ExtensionMarker("<M>$E15D13A036DCD41255EBD6266F8556E9")]
    public Map LordMapOrMapHeld
    {
      [ExtensionMarker("<M>$E15D13A036DCD41255EBD6266F8556E9")] get
      {
        throw new NotSupportedException();
      }
    }

    [SpecialName]
    public static class \u003CM\u003E\u0024E15D13A036DCD41255EBD6266F8556E9
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(Pawn pawn)
      {
      }
    }
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u0024464CF13CF05F23E774A9D567340C67D6
  {
    [ExtensionMarker("<M>$AB6E3698B79D9C19BEF3884BAD75C407")]
    public float YOffsetFull(VehiclePawnWithMap vehicle) => throw new NotSupportedException();

    [ExtensionMarker("<M>$AB6E3698B79D9C19BEF3884BAD75C407")]
    public float FlipAngle(VehiclePawn vehicle) => throw new NotSupportedException();

    [ExtensionMarker("<M>$AB6E3698B79D9C19BEF3884BAD75C407")]
    public float YOffset() => throw new NotSupportedException();

    [SpecialName]
    public static class \u003CM\u003E\u0024AB6E3698B79D9C19BEF3884BAD75C407
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(float original)
      {
      }
    }
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u00248F1CC08547E77D955A6D73192FA545FB
  {
    [ExtensionMarker("<M>$C8412A4FB1A615DB4C07F140ED8D0CE4")]
    public Vector3 YOffset() => throw new NotSupportedException();

    [ExtensionMarker("<M>$C8412A4FB1A615DB4C07F140ED8D0CE4")]
    public Vector3 YOffsetFull(VehiclePawnWithMap vehicle) => throw new NotSupportedException();

    [ExtensionMarker("<M>$C8412A4FB1A615DB4C07F140ED8D0CE4")]
    public Vector3 ToVehicleMapCoord() => throw new NotSupportedException();

    [ExtensionMarker("<M>$C8412A4FB1A615DB4C07F140ED8D0CE4")]
    public Vector3 ToVehicleMapCoord(VehiclePawnWithMap vehicle)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$C8412A4FB1A615DB4C07F140ED8D0CE4")]
    public Vector3 ToNonFocusedThingMapCoord(Thing thing) => throw new NotSupportedException();

    [ExtensionMarker("<M>$C8412A4FB1A615DB4C07F140ED8D0CE4")]
    public Vector3 ToBaseMapCoord() => throw new NotSupportedException();

    [ExtensionMarker("<M>$C8412A4FB1A615DB4C07F140ED8D0CE4")]
    public Vector3 ToBaseMapCoord(Map map) => throw new NotSupportedException();

    [ExtensionMarker("<M>$C8412A4FB1A615DB4C07F140ED8D0CE4")]
    public Vector3 ToBaseMapCoord(VehiclePawnWithMap vehicle) => throw new NotSupportedException();

    [ExtensionMarker("<M>$C8412A4FB1A615DB4C07F140ED8D0CE4")]
    public Vector3 ToBaseMapCoord(VehiclePawnWithMap vehicle, Rot8 rot)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$C8412A4FB1A615DB4C07F140ED8D0CE4")]
    public bool TryGetVehicleMap(Map map, out VehiclePawnWithMap vehicle, VehicleMapFlag flag = VehicleMapFlag.StructureCells)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$C8412A4FB1A615DB4C07F140ED8D0CE4")]
    public bool TryGetVehicleMap(VehiclePawnWithMap vehicle, VehicleMapFlag flag = VehicleMapFlag.StructureCells)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$C8412A4FB1A615DB4C07F140ED8D0CE4")]
    public Vector3 ToThingBaseMapCoord(Thing thing) => throw new NotSupportedException();

    [SpecialName]
    public static class \u003CM\u003E\u0024C8412A4FB1A615DB4C07F140ED8D0CE4
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(Vector3 original)
      {
      }
    }
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u0024EBA7D8B16BF040CED4E3DDA13865FC4E
  {
    [ExtensionMarker("<M>$6D0C93A67972F3D343D61AB150EA0096")]
    public IntVec3 ToBaseMapCoord(VehiclePawnWithMap vehicle) => throw new NotSupportedException();

    [ExtensionMarker("<M>$6D0C93A67972F3D343D61AB150EA0096")]
    public IntVec3 ToBaseMapCoord(Map map) => throw new NotSupportedException();

    [ExtensionMarker("<M>$6D0C93A67972F3D343D61AB150EA0096")]
    public IntVec3 ToVehicleMapCoord(VehiclePawnWithMap vehicle)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$6D0C93A67972F3D343D61AB150EA0096")]
    public IntVec3 ToThingMapCoord(Thing thing) => throw new NotSupportedException();

    [ExtensionMarker("<M>$6D0C93A67972F3D343D61AB150EA0096")]
    public IntVec3 ToVehicleMapCoord(Map map) => throw new NotSupportedException();

    [ExtensionMarker("<M>$6D0C93A67972F3D343D61AB150EA0096")]
    public IntVec3 ToThingBaseMapCoord(Thing thing) => throw new NotSupportedException();

    [ExtensionMarker("<M>$6D0C93A67972F3D343D61AB150EA0096")]
    public IntVec2 ToHitCell(VehiclePawnWithMap vehicle) => throw new NotSupportedException();

    [ExtensionMarker("<M>$6D0C93A67972F3D343D61AB150EA0096")]
    public IntVec3 ClosestEdgeCell(VehiclePawnWithMap vehicle) => throw new NotSupportedException();

    [ExtensionMarker("<M>$6D0C93A67972F3D343D61AB150EA0096")]
    public IntVec3 ClosestMapEdgeCell(VehiclePawnWithMap vehicle)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$6D0C93A67972F3D343D61AB150EA0096")]
    public IntVec3 ClosestWalkableEdgeCell(VehiclePawnWithMap vehicle, int districtID = -1)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$CDF12A6F7EBB5A4C3C551878406C614D")]
    public Rot4 DirectionToInsideMap(VehiclePawnWithMap vehicle)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$CDF12A6F7EBB5A4C3C551878406C614D")]
    public Rot8 BaseFullDirectionToInsideMap(VehiclePawnWithMap vehicle)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$CDF12A6F7EBB5A4C3C551878406C614D")]
    public Pawn GetFirstPawnAcrossMaps(Map map) => throw new NotSupportedException();

    [ExtensionMarker("<M>$CDF12A6F7EBB5A4C3C551878406C614D")]
    public Thing GetCoverOnThingMap(Map map, Thing thing) => throw new NotSupportedException();

    [ExtensionMarker("<M>$CDF12A6F7EBB5A4C3C551878406C614D")]
    public bool RoofedAcrossMaps(Map map) => throw new NotSupportedException();

    [SpecialName]
    public static class \u003CM\u003E\u00246D0C93A67972F3D343D61AB150EA0096
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(IntVec3 original)
      {
      }
    }

    [SpecialName]
    public static class \u003CM\u003E\u0024CDF12A6F7EBB5A4C3C551878406C614D
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(IntVec3 c)
      {
      }
    }
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u0024A5FFD86A62DD169AB93042D5339B86EC
  {
    [ExtensionMarker("<M>$D8731CB47C54380BE0760618C446BD86")]
    public IntVec3 CellOnBaseMap() => throw new NotSupportedException();

    [ExtensionMarker("<M>$D8731CB47C54380BE0760618C446BD86")]
    public IntVec3 CellOnBaseMapSpawned() => throw new NotSupportedException();

    [SpecialName]
    public static class \u003CM\u003E\u0024D8731CB47C54380BE0760618C446BD86
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(ref LocalTargetInfo target)
      {
      }
    }
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u0024606A6B43D1C7BFA7AA067F3A3E342434
  {
    [ExtensionMarker("<M>$B1465CC191C073B1F306C381E3B5D02D")]
    public IntVec3 CellOnGroundMap
    {
      [ExtensionMarker("<M>$B1465CC191C073B1F306C381E3B5D02D")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$B1465CC191C073B1F306C381E3B5D02D")]
    public Vector3 CenterVector3OnGroundMap
    {
      [ExtensionMarker("<M>$B1465CC191C073B1F306C381E3B5D02D")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$7A2A64A06EB569B5DE28B5DE2694F7AC")]
    public IntVec3 CellOnBaseMap() => throw new NotSupportedException();

    [ExtensionMarker("<M>$7A2A64A06EB569B5DE28B5DE2694F7AC")]
    public IntVec3 CellOnBaseMapSpawned() => throw new NotSupportedException();

    [ExtensionMarker("<M>$7A2A64A06EB569B5DE28B5DE2694F7AC")]
    public Map BaseMap() => throw new NotSupportedException();

    [SpecialName]
    public static class \u003CM\u003E\u0024B1465CC191C073B1F306C381E3B5D02D
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(TargetInfo target)
      {
      }
    }

    [SpecialName]
    public static class \u003CM\u003E\u00247A2A64A06EB569B5DE28B5DE2694F7AC
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(ref TargetInfo target)
      {
      }
    }
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u00242E00BFEA7D9EF812AFBA6F25EF212A42
  {
    [ExtensionMarker("<M>$F18D84E0BB10420C435CCEDDF6A212AF")]
    public IntVec3 CellOnBaseMap() => throw new NotSupportedException();

    [ExtensionMarker("<M>$F18D84E0BB10420C435CCEDDF6A212AF")]
    public IntVec3 CellOnBaseMapSpawned() => throw new NotSupportedException();

    [ExtensionMarker("<M>$F18D84E0BB10420C435CCEDDF6A212AF")]
    public Map BaseMap() => throw new NotSupportedException();

    [SpecialName]
    public static class \u003CM\u003E\u0024F18D84E0BB10420C435CCEDDF6A212AF
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(ref GlobalTargetInfo target)
      {
      }
    }
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u0024ABFF6B6B8A5941EB4E8CC8D86BA457C5
  {
    [ExtensionMarker("<M>$F9A44A31E7799B8454B27FC6F4A26CDD")]
    public float FullAngle
    {
      [ExtensionMarker("<M>$F9A44A31E7799B8454B27FC6F4A26CDD")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$F9A44A31E7799B8454B27FC6F4A26CDD")]
    public Quaternion FullAngleQuat
    {
      [ExtensionMarker("<M>$F9A44A31E7799B8454B27FC6F4A26CDD")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$F9A44A31E7799B8454B27FC6F4A26CDD")]
    public float ExtraAngle
    {
      [ExtensionMarker("<M>$F9A44A31E7799B8454B27FC6F4A26CDD")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$F9A44A31E7799B8454B27FC6F4A26CDD")]
    public int HalfLength() => throw new NotSupportedException();

    [ExtensionMarker("<M>$F9A44A31E7799B8454B27FC6F4A26CDD")]
    public bool TryGetFullRotation(ref Rot8 rot) => throw new NotSupportedException();

    [ExtensionMarker("<M>$F9A44A31E7799B8454B27FC6F4A26CDD")]
    public Rot8 BaseFullRotation() => throw new NotSupportedException();

    [SpecialName]
    public static class \u003CM\u003E\u0024F9A44A31E7799B8454B27FC6F4A26CDD
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(VehiclePawn vehicle)
      {
      }
    }
  }
}
