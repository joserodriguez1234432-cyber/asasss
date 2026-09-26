// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.ShootLeanUtilityOnVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public static class ShootLeanUtilityOnVehicle
{
  private static readonly Queue<bool[]> blockedArrays = new Queue<bool[]>();

  private static bool[] GetWorkingBlockedArray()
  {
    return ShootLeanUtilityOnVehicle.blockedArrays.Count <= 0 ? new bool[8] : ShootLeanUtilityOnVehicle.blockedArrays.Dequeue();
  }

  private static void ReturnWorkingBlockedArray(bool[] ar)
  {
    ShootLeanUtilityOnVehicle.blockedArrays.Enqueue(ar);
    if (ShootLeanUtilityOnVehicle.blockedArrays.Count <= 128 /*0x80*/)
      return;
    Log.ErrorOnce("Too many blocked arrays to be feasible. >128", 388121);
  }

  public static void CalcShootableCellsOf(
    List<IntVec3> outCells,
    Thing t,
    IntVec3 shooterPosOnBaseMap,
    ModCompat.AsAboveSoBelow.TargetBand? sourceBand,
    ModCompat.AsAboveSoBelow.TargetBand? targetBand)
  {
    outCells.Clear();
    VehiclePawnWithMap vehicle = t as VehiclePawnWithMap;
    if (vehicle == null)
    {
      if (t is Pawn)
      {
        ShootLeanUtilityOnVehicle.LeanShootingSourcesFromTo(t.Position, shooterPosOnBaseMap, t.Map, outCells, sourceBand, targetBand);
      }
      else
      {
        outCells.Add(t.Position);
        if (t.def.size.x == 1 && t.def.size.z == 1)
          return;
        outCells.AddRange(((IEnumerable<IntVec3>) (object) GenAdj.OccupiedRect(t)).Where<IntVec3>((Func<IntVec3, bool>) (intVec => IntVec3.op_Inequality(intVec, t.Position))));
      }
    }
    else
    {
      IntVec3 shooterLoc = GenSight.LastPointOnLineOfSight(shooterPosOnBaseMap, t.Position, (Func<IntVec3, bool>) (c =>
      {
        VehiclePawnWithMap vehicle1;
        if (c.TryGetVehicleMap(t.Map, out vehicle1) && vehicle == vehicle1)
        {
          Building edificeSafe = GridsUtility.GetEdificeSafe(c.ToVehicleMapCoord(vehicle), vehicle.VehicleMap);
          if (edificeSafe != null && !GenGrid.CanBeSeenOver(edificeSafe))
            return false;
        }
        return true;
      }), false);
      if (IntVec3.op_Equality(shooterLoc, IntVec3.Invalid))
        shooterLoc = t.Position;
      ShootLeanUtilityOnVehicle.LeanShootingSourcesFromTo(shooterLoc, shooterPosOnBaseMap, t.Map, outCells, sourceBand, targetBand);
    }
  }

  public static void LeanShootingSourcesFromTo(
    IntVec3 shooterLoc,
    IntVec3 targetPosBaseCol,
    Map map,
    List<IntVec3> listToFill)
  {
    ShootLeanUtilityOnVehicle.LeanShootingSourcesFromTo(shooterLoc, targetPosBaseCol, map, listToFill, new ModCompat.AsAboveSoBelow.TargetBand?(), new ModCompat.AsAboveSoBelow.TargetBand?());
  }

  public static void LeanShootingSourcesFromTo(
    IntVec3 shooterLoc,
    IntVec3 targetPosBaseCol,
    Map map,
    List<IntVec3> listToFill,
    ModCompat.AsAboveSoBelow.TargetBand? sourceBand,
    ModCompat.AsAboveSoBelow.TargetBand? targetBand)
  {
    IntVec3 c = shooterLoc;
    Map map1 = map.BaseMap();
    VehiclePawnWithMap vehicle;
    if (map.IsVehicleMapOf(out vehicle))
      c = shooterLoc.ToBaseMapCoord(vehicle);
    listToFill.Clear();
    IntVec3 intVec3_1 = IntVec3.op_Subtraction(targetPosBaseCol, c);
    Vector3 vector3 = ((IntVec3) ref intVec3_1).ToVector3();
    if (vehicle != null)
      vector3 = Vector3Utility.RotatedBy(vector3, -VehicleMapUtility.get_FullAngle((VehiclePawn) vehicle));
    float num = Vector3Utility.AngleFlat(vector3);
    bool flag1 = (double) num > 270.0 || (double) num < 90.0;
    bool flag2 = (double) num > 90.0 && (double) num < 270.0;
    bool flag3 = (double) num > 180.0;
    bool flag4 = (double) num < 180.0;
    bool[] workingBlockedArray = ShootLeanUtilityOnVehicle.GetWorkingBlockedArray();
    for (int index = 0; index < 8; ++index)
    {
      IntVec3 intVec3_2 = IntVec3.op_Addition(shooterLoc, GenAdj.AdjacentCells[index]);
      if (vehicle != null)
        intVec3_2 = intVec3_2.ToBaseMapCoord(vehicle);
      workingBlockedArray[index] = !intVec3_2.CanBeSeenOverOnVehicle(map1, sourceBand, targetBand);
    }
    if (!workingBlockedArray[1] && (((!workingBlockedArray[0] ? 0 : (!workingBlockedArray[5] ? 1 : 0)) & (flag1 ? 1 : 0)) != 0 || ((!workingBlockedArray[2] ? 0 : (!workingBlockedArray[4] ? 1 : 0)) & (flag2 ? 1 : 0)) != 0))
      listToFill.Add(IntVec3.op_Addition(shooterLoc, new IntVec3(1, 0, 0)));
    if (!workingBlockedArray[3] && (((!workingBlockedArray[0] ? 0 : (!workingBlockedArray[6] ? 1 : 0)) & (flag1 ? 1 : 0)) != 0 || ((!workingBlockedArray[2] ? 0 : (!workingBlockedArray[7] ? 1 : 0)) & (flag2 ? 1 : 0)) != 0))
      listToFill.Add(IntVec3.op_Addition(shooterLoc, new IntVec3(-1, 0, 0)));
    if (!workingBlockedArray[2] && (((!workingBlockedArray[3] ? 0 : (!workingBlockedArray[7] ? 1 : 0)) & (flag3 ? 1 : 0)) != 0 || ((!workingBlockedArray[1] ? 0 : (!workingBlockedArray[4] ? 1 : 0)) & (flag4 ? 1 : 0)) != 0))
      listToFill.Add(IntVec3.op_Addition(shooterLoc, new IntVec3(0, 0, -1)));
    if (!workingBlockedArray[0] && (((!workingBlockedArray[3] ? 0 : (!workingBlockedArray[6] ? 1 : 0)) & (flag3 ? 1 : 0)) != 0 || ((!workingBlockedArray[1] ? 0 : (!workingBlockedArray[5] ? 1 : 0)) & (flag4 ? 1 : 0)) != 0))
      listToFill.Add(IntVec3.op_Addition(shooterLoc, new IntVec3(0, 0, 1)));
    if (c.CanBeSeenOverOnVehicle(map1, sourceBand, targetBand))
      listToFill.Add(shooterLoc);
    for (int index = 0; index < 4; ++index)
    {
      IntVec3 intVec3_3 = IntVec3.op_Addition(shooterLoc, GenAdj.AdjacentCells[index]);
      if (!workingBlockedArray[index] && index != 0 | flag1 && index != 1 | flag4 && index != 2 | flag2 && index != 3 | flag3 && GenGrid.InBounds(intVec3_3, map) && GridsUtility.GetCover(intVec3_3, map) != null)
        listToFill.Add(intVec3_3);
    }
    ShootLeanUtilityOnVehicle.ReturnWorkingBlockedArray(workingBlockedArray);
  }
}
