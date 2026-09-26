// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.GenSightOnVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public static class GenSightOnVehicle
{
  public static bool LineOfSight(
    IntVec3 start,
    IntVec3 end,
    Map map,
    bool skipFirstCell,
    Func<IntVec3, bool> validator = null,
    int halfXOffset = 0,
    int halfZOffset = 0)
  {
    return GenSightOnVehicle.LineOfSight(start, end, map, new ModCompat.AsAboveSoBelow.TargetBand?(), new ModCompat.AsAboveSoBelow.TargetBand?(), skipFirstCell, validator, halfXOffset, halfZOffset);
  }

  public static bool LineOfSight(
    IntVec3 start,
    IntVec3 end,
    Map map,
    ModCompat.AsAboveSoBelow.TargetBand? sourceBand,
    ModCompat.AsAboveSoBelow.TargetBand? targetBand,
    bool skipFirstCell = false,
    Func<IntVec3, bool> validator = null,
    int halfXOffset = 0,
    int halfZOffset = 0)
  {
    VehiclePawnWithMap vehicle;
    if (map.IsVehicleMapOf(out vehicle))
    {
      if (!((Thing) vehicle).Spawned)
        return GenSightOnVehicle.LineOfSightVehicleToVehicle(start, end, map, sourceBand, targetBand, skipFirstCell, validator, halfXOffset, halfZOffset);
      start = start.ToBaseMapCoord(vehicle);
      end = end.ToBaseMapCoord(vehicle);
      map = ((Thing) vehicle).Map;
    }
    if (!GenGrid.InBounds(start, map) || !GenGrid.InBounds(end, map))
      return false;
    bool flag = start.x != end.x ? start.x < end.x : start.z < end.z;
    int num1 = Mathf.Abs(end.x - start.x);
    int num2 = Mathf.Abs(end.z - start.z);
    int x = start.x;
    int z = start.z;
    int num3 = 1 + num1 + num2;
    int num4 = end.x > start.x ? 1 : -1;
    int num5 = end.z > start.z ? 1 : -1;
    int num6 = num1 * 4;
    int num7 = num2 * 4;
    int num8 = num6 + halfXOffset * 2;
    int num9 = num7 + halfZOffset * 2;
    int num10 = num8 / 2 - num9 / 2;
    IntVec3 c = new IntVec3();
    for (; num3 > 1; --num3)
    {
      c.x = x;
      c.z = z;
      if ((!skipFirstCell || !IntVec3.op_Equality(c, start)) && (!c.CanBeSeenOverOnVehicleFast(map, sourceBand, targetBand) || validator != null && !validator(c)))
        return false;
      if (num10 > 0 || num10 == 0 & flag)
      {
        x += num4;
        num10 -= num9;
      }
      else
      {
        z += num5;
        num10 += num8;
      }
    }
    return true;
  }

  public static bool LineOfSightVehicleToVehicle(
    IntVec3 start,
    IntVec3 end,
    Map map,
    ModCompat.AsAboveSoBelow.TargetBand? sourceBand,
    ModCompat.AsAboveSoBelow.TargetBand? targetBand,
    bool skipFirstCell = false,
    Func<IntVec3, bool> validator = null,
    int halfXOffset = 0,
    int halfZOffset = 0)
  {
    VehiclePawnWithMap vehicle1;
    if (((IntVec3) ref start).ToVector3Shifted().TryGetVehicleMap(map, out vehicle1) && !LOS(vehicle1))
      return false;
    VehiclePawnWithMap vehicle2;
    return !((IntVec3) ref end).ToVector3Shifted().TryGetVehicleMap(map, out vehicle2) || LOS(vehicle2);

    bool LOS(VehiclePawnWithMap v)
    {
      IntVec3 c1 = start.ToVehicleMapCoord(v);
      IntVec3 c2 = end.ToVehicleMapCoord(v);
      IntVec3 targetBand = c1.TranslateToTargetBand(v.VehicleMap, sourceBand, targetBand);
      if (IntVec3.op_Inequality(c1, targetBand))
      {
        c1 = targetBand;
        c2 = c2.TranslateToTargetBand(v.VehicleMap, sourceBand, targetBand);
      }
      bool flag = c1.x == c2.x ? c1.z < c2.z : c1.x < c2.x;
      int num1 = Mathf.Abs(c2.x - c1.x);
      int num2 = Mathf.Abs(c2.z - c1.z);
      int x = c1.x;
      int z = c1.z;
      int num3 = 1 + num1 + num2;
      int num4 = c2.x > c1.x ? 1 : -1;
      int num5 = c2.z > c1.z ? 1 : -1;
      int num6 = num1 * 4;
      int num7 = num2 * 4;
      int num8 = num6 + halfXOffset * 2;
      int num9 = num7 + halfZOffset * 2;
      int num10 = num8 / 2 - num9 / 2;
      IntVec3 intVec3 = new IntVec3();
      for (; num3 > 1; --num3)
      {
        intVec3.x = x;
        intVec3.z = z;
        if (GenGrid.InBounds(intVec3, v.VehicleMap) && (!skipFirstCell || !IntVec3.op_Equality(intVec3, c1)) && (!GenGrid.CanBeSeenOverFast(intVec3, v.VehicleMap) || validator != null && !validator(intVec3)))
          return false;
        if (num10 > 0 || num10 == 0 & flag)
        {
          x += num4;
          num10 -= num9;
        }
        else
        {
          z += num5;
          num10 += num8;
        }
      }
      return true;
    }
  }

  public static bool LineOfSightThingToTarget(
    Thing thing,
    LocalTargetInfo target,
    bool skipFirstCell = false,
    Func<IntVec3, bool> validator = null)
  {
    return GenSightOnVehicle.LineOfSight(VehicleMapUtility.get_PositionOnBaseMapSpawned(thing), target.CellOnBaseMapSpawned(), thing.BaseMap(), ModCompat.AsAboveSoBelow.GetTargetBand(thing), ModCompat.AsAboveSoBelow.GetTargetBand(((LocalTargetInfo) ref target).Thing), skipFirstCell, validator);
  }

  public static bool LineOfSightThingToThing(
    Thing start,
    Thing end,
    bool skipFirstCell = false,
    Func<IntVec3, bool> validator = null)
  {
    return GenSightOnVehicle.LineOfSight(VehicleMapUtility.get_PositionOnBaseMapSpawned(start), VehicleMapUtility.get_PositionOnBaseMapSpawned(end), start.BaseMap(), ModCompat.AsAboveSoBelow.GetTargetBand(start), ModCompat.AsAboveSoBelow.GetTargetBand(end), skipFirstCell, validator);
  }

  public static bool LineOfSightToThing(
    IntVec3 start,
    Thing t,
    Map map,
    bool skipFirstCell = false,
    Func<IntVec3, bool> validator = null)
  {
    return GenSightOnVehicle.LineOfSightToThing(start, t, map, new ModCompat.AsAboveSoBelow.TargetBand?(), skipFirstCell, validator);
  }

  public static bool LineOfSightToThing(
    IntVec3 start,
    Thing t,
    Map map,
    ModCompat.AsAboveSoBelow.TargetBand? sourceBand,
    bool skipFirstCell = false,
    Func<IntVec3, bool> validator = null)
  {
    bool flag = false;
    VehiclePawnWithMap vehicle;
    if (map.IsVehicleMapOf(out vehicle) && ((Thing) vehicle).Spawned)
    {
      start = start.ToBaseMapCoord(vehicle);
      map = ((Thing) vehicle).Map;
      flag = true;
    }
    ModCompat.AsAboveSoBelow.TargetBand? targetBand = ModCompat.AsAboveSoBelow.GetTargetBand(t);
    return !IntVec2.op_Equality(t.def.size, IntVec2.One) ? ((IEnumerable<IntVec3>) (object) GenAdj.OccupiedRect(t)).Select<IntVec3, IntVec3>((Func<IntVec3, IntVec3>) (end => !flag ? end : end.ToBaseMapCoord(vehicle))).Any<IntVec3>((Func<IntVec3, bool>) (end2 => GenSightOnVehicle.LineOfSight(start, end2, map, sourceBand, targetBand, skipFirstCell, validator))) : GenSightOnVehicle.LineOfSight(start, VehicleMapUtility.get_PositionOnBaseMapSpawned(t), map, sourceBand, targetBand, skipFirstCell, validator);
  }

  public static bool LineOfSight(IntVec3 start, IntVec3 end, Map map)
  {
    return GenSightOnVehicle.LineOfSight(start, end, map, CellRect.SingleCell(start), CellRect.SingleCell(end));
  }

  public static bool LineOfSight(
    IntVec3 start,
    IntVec3 end,
    Map map,
    CellRect startRect,
    CellRect endRect,
    Func<IntVec3, bool> validator = null)
  {
    return GenSightOnVehicle.LineOfSight(start, end, map, startRect, endRect, new ModCompat.AsAboveSoBelow.TargetBand?(), new ModCompat.AsAboveSoBelow.TargetBand?(), validator);
  }

  public static bool LineOfSight(
    IntVec3 start,
    IntVec3 end,
    Map map,
    CellRect startRect,
    CellRect endRect,
    ModCompat.AsAboveSoBelow.TargetBand? sourceBand,
    ModCompat.AsAboveSoBelow.TargetBand? targetBand,
    Func<IntVec3, bool> validator = null)
  {
    VehiclePawnWithMap vehicle;
    if (map.IsVehicleMapOf(out vehicle))
    {
      if (!((Thing) vehicle).Spawned)
        return GenSightOnVehicle.LineOfSightVehicleToVehicle(start, end, map, sourceBand, targetBand, validator: validator);
      start = start.ToBaseMapCoord(vehicle);
      end = end.ToBaseMapCoord(vehicle);
      map = ((Thing) vehicle).Map;
    }
    if (!GenGrid.InBounds(start, map) || !GenGrid.InBounds(end, map))
      return false;
    bool flag = start.x != end.x ? start.x < end.x : start.z < end.z;
    int num1 = Mathf.Abs(end.x - start.x);
    int num2 = Mathf.Abs(end.z - start.z);
    int x = start.x;
    int z = start.z;
    int num3 = 1 + num1 + num2;
    int num4 = end.x > start.x ? 1 : -1;
    int num5 = end.z > start.z ? 1 : -1;
    int num6 = num1 - num2;
    int num7 = num1 * 2;
    int num8 = num2 * 2;
    IntVec3 c = new IntVec3();
    for (; num3 > 1; --num3)
    {
      c.x = x;
      c.z = z;
      if (((CellRect) ref endRect).Contains(c))
        return true;
      if (!((CellRect) ref startRect).Contains(c) && (!c.CanBeSeenOverOnVehicleFast(map, sourceBand, targetBand) || validator != null && !validator(c)))
        return false;
      if (num6 > 0 || num6 == 0 & flag)
      {
        x += num4;
        num6 -= num8;
      }
      else
      {
        z += num5;
        num6 += num7;
      }
    }
    return true;
  }

  public static bool LineOfSightToEdges(
    IntVec3 start,
    IntVec3 end,
    Map map,
    bool skipFirstCell = false,
    Func<IntVec3, bool> validator = null)
  {
    return GenSightOnVehicle.LineOfSightToEdges(start, end, map, new ModCompat.AsAboveSoBelow.TargetBand?(), new ModCompat.AsAboveSoBelow.TargetBand?(), skipFirstCell, validator);
  }

  public static bool LineOfSightToEdges(
    IntVec3 start,
    IntVec3 end,
    Map map,
    ModCompat.AsAboveSoBelow.TargetBand? sourceBand,
    ModCompat.AsAboveSoBelow.TargetBand? targetBand,
    bool skipFirstCell = false,
    Func<IntVec3, bool> validator = null)
  {
    if (GenSightOnVehicle.LineOfSight(start, end, map, sourceBand, targetBand, skipFirstCell, validator))
      return true;
    int squared = IntVec3Utility.DistanceToSquared(IntVec3.op_Multiply(start, 2), IntVec3.op_Multiply(end, 2));
    for (int index = 0; index < 4; ++index)
    {
      if (IntVec3Utility.DistanceToSquared(IntVec3.op_Multiply(start, 2), IntVec3.op_Addition(IntVec3.op_Multiply(end, 2), GenAdj.CardinalDirections[index])) <= squared && GenSightOnVehicle.LineOfSight(start, end, map, sourceBand, targetBand, skipFirstCell, validator, GenAdj.CardinalDirections[index].x, GenAdj.CardinalDirections[index].z))
        return true;
    }
    return false;
  }

  public static bool CanBeSeenOverOnVehicle(this IntVec3 c, Map map)
  {
    return c.CanBeSeenOverOnVehicle(map, new ModCompat.AsAboveSoBelow.TargetBand?(), new ModCompat.AsAboveSoBelow.TargetBand?());
  }

  public static bool CanBeSeenOverOnVehicle(
    this IntVec3 c,
    Map map,
    ModCompat.AsAboveSoBelow.TargetBand? sourceBand,
    ModCompat.AsAboveSoBelow.TargetBand? targetBand)
  {
    if (!GenGrid.InBounds(c, map))
      return false;
    VehiclePawnWithMap vehicle;
    if (c.TryGetVehicleMap(map, out vehicle))
    {
      IntVec3 targetBand1 = c.ToVehicleMapCoord(vehicle).TranslateToTargetBand(vehicle.VehicleMap, sourceBand, targetBand);
      if (GenGrid.InBounds(targetBand1, vehicle.VehicleMap))
      {
        Building edifice = GridsUtility.GetEdifice(targetBand1, vehicle.VehicleMap);
        if (edifice != null && !GenGrid.CanBeSeenOver(edifice))
          return false;
      }
    }
    c = c.TranslateToTargetBand(map, sourceBand, targetBand);
    Building edifice1 = GridsUtility.GetEdifice(c, map);
    return edifice1 == null || GenGrid.CanBeSeenOver(edifice1);
  }

  public static bool CanBeSeenOverOnVehicleFast(this IntVec3 c, Map map)
  {
    return c.CanBeSeenOverOnVehicleFast(map, new ModCompat.AsAboveSoBelow.TargetBand?(), new ModCompat.AsAboveSoBelow.TargetBand?());
  }

  public static bool CanBeSeenOverOnVehicleFast(
    this IntVec3 c,
    Map map,
    ModCompat.AsAboveSoBelow.TargetBand? sourceBand,
    ModCompat.AsAboveSoBelow.TargetBand? targetBand)
  {
    VehiclePawnWithMap vehicle;
    if (c.TryGetVehicleMap(map, out vehicle))
    {
      IntVec3 targetBand1 = c.ToVehicleMapCoord(vehicle).TranslateToTargetBand(vehicle.VehicleMap, sourceBand, targetBand);
      if (GenGrid.InBounds(targetBand1, vehicle.VehicleMap))
      {
        Building edifice = GridsUtility.GetEdifice(targetBand1, vehicle.VehicleMap);
        if (edifice != null && !GenGrid.CanBeSeenOver(edifice))
          return false;
      }
    }
    c = c.TranslateToTargetBand(map, sourceBand, targetBand);
    Building edifice1 = GridsUtility.GetEdifice(c, map);
    return edifice1 == null || GenGrid.CanBeSeenOver(edifice1);
  }

  public static IntVec3 TranslateToTargetBand(
    this IntVec3 c,
    Map map,
    ModCompat.AsAboveSoBelow.TargetBand? sourceBand,
    ModCompat.AsAboveSoBelow.TargetBand? targetBand)
  {
    if (!ModCompat.CompatBase<ModCompat.AsAboveSoBelow>.Active)
      return c;
    IntVec3 targetBand1 = c;
    if (sourceBand.HasValue)
    {
      ModCompat.AsAboveSoBelow.TargetBand valueOrDefault = sourceBand.GetValueOrDefault();
      if (valueOrDefault.Map == map)
      {
        targetBand1 = ModCompat.AsAboveSoBelow.Translate(valueOrDefault.comp, targetBand1, valueOrDefault.band);
        goto label_12;
      }
    }
    if (targetBand.HasValue)
    {
      ModCompat.AsAboveSoBelow.TargetBand valueOrDefault = targetBand.GetValueOrDefault();
      if (valueOrDefault.Map == map)
      {
        targetBand1 = ModCompat.AsAboveSoBelow.Translate(valueOrDefault.comp, targetBand1, valueOrDefault.band);
        goto label_12;
      }
    }
    MapComponent mapComponent = ModCompat.AsAboveSoBelow.CompOf(map);
    if (mapComponent != null && ModCompat.AsAboveSoBelow.Banded(mapComponent))
    {
      ModCompat.AsAboveSoBelow.TargetBand? nullable = targetBand ?? sourceBand;
      if (nullable.HasValue)
      {
        ModCompat.AsAboveSoBelow.TargetBand valueOrDefault = nullable.GetValueOrDefault();
        int num1 = ModCompat.AsAboveSoBelow.surfaceBand.Invoke(mapComponent);
        int num2 = num1 + valueOrDefault.band - ModCompat.AsAboveSoBelow.surfaceBand.Invoke(valueOrDefault.comp);
        if (num1 != num2)
          targetBand1 = ModCompat.AsAboveSoBelow.Translate(mapComponent, targetBand1, num2);
      }
    }
label_12:
    return targetBand1;
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u0024EBA7D8B16BF040CED4E3DDA13865FC4E
  {
    [ExtensionMarker("<M>$CDF12A6F7EBB5A4C3C551878406C614D")]
    public bool CanBeSeenOverOnVehicle(Map map) => throw new NotSupportedException();

    [ExtensionMarker("<M>$CDF12A6F7EBB5A4C3C551878406C614D")]
    public bool CanBeSeenOverOnVehicle(
      Map map,
      ModCompat.AsAboveSoBelow.TargetBand? sourceBand,
      ModCompat.AsAboveSoBelow.TargetBand? targetBand)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$CDF12A6F7EBB5A4C3C551878406C614D")]
    public bool CanBeSeenOverOnVehicleFast(Map map) => throw new NotSupportedException();

    [ExtensionMarker("<M>$CDF12A6F7EBB5A4C3C551878406C614D")]
    public bool CanBeSeenOverOnVehicleFast(
      Map map,
      ModCompat.AsAboveSoBelow.TargetBand? sourceBand,
      ModCompat.AsAboveSoBelow.TargetBand? targetBand)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$CDF12A6F7EBB5A4C3C551878406C614D")]
    public IntVec3 TranslateToTargetBand(
      Map map,
      ModCompat.AsAboveSoBelow.TargetBand? sourceBand,
      ModCompat.AsAboveSoBelow.TargetBand? targetBand)
    {
      throw new NotSupportedException();
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
}
