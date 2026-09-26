// Decompiled with JetBrains decompiler
// Type: Vehicles.CombatPositionFinder
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public static class CombatPositionFinder
{
  public static bool TryFindCastPosition(in CastPositionRequest req, out IntVec3 dest)
  {
    dest = ((Thing) req.vehicle).Position;
    VehiclePawn vehicle = req.vehicle;
    Map map = ((Thing) vehicle).Map;
    IntVec3 vehiclePos = ((Thing) vehicle).Position;
    LocalTargetInfo target = req.target;
    IntVec3 targetPos = ((LocalTargetInfo) ref target).Cell;
    float maxRangeSquared = req.maxRange * req.maxRange;
    float maxRangeFromLocusSquared = req.maxRangeFromLocus * req.maxRangeFromLocus;
    IntVec3 intVec3_1 = IntVec3.op_Subtraction(vehiclePos, targetPos);
    float rangeFromTarget = ((IntVec3) ref intVec3_1).LengthHorizontal;
    IntVec3 intVec3_2 = IntVec3.op_Subtraction(vehiclePos, targetPos);
    float rangeFromTargetSquared = (float) ((IntVec3) ref intVec3_2).LengthHorizontalSquared;
    float optimalRangeSquared = req.range * req.range;
    float rangeFromTargetToCellSquared = float.NaN;
    float rangeFromCasterToCellSquared = float.NaN;
    AvoidGrid avoidGrid;
    if (!PawnUtility.TryGetAvoidGrid((Pawn) vehicle, ref avoidGrid, true))
      Log.Warning("Null avoid grid for position finder.");
    CellRect cellRect1 = CellRect.WholeMap(map);
    int maxRegions = req.maxRegions;
    if (req.maxRegions > 0)
    {
      VehicleRegion root = VehicleRegionAndRoomQuery.RegionAt(vehiclePos, map, vehicle.VehicleDef);
      if (root == null)
      {
        Log.Error("TryFindCastPosition requiring region traversal but root region is null.");
        dest = IntVec3.Invalid;
        return false;
      }
      int inRadiusMark = Rand.Int;
      VehicleRegionTraverser.MarkRegionsBFS(root, (VehicleRegionTraverser.VehicleRegionEntry) null, req.maxRegions, inRadiusMark);
      if ((double) req.maxRangeFromLocus > 0.0099999997764825821)
      {
        VehicleRegion locusReg = VehicleRegionAndRoomQuery.RegionAt(req.locus, map, vehicle.VehicleDef);
        if (locusReg == null)
        {
          Log.Error($"locus {req.locus} has no region");
          dest = IntVec3.Invalid;
          return false;
        }
        if (locusReg.mark != inRadiusMark)
        {
          inRadiusMark = Rand.Int;
          VehicleRegionTraverser.BreadthFirstTraverse(root, (VehicleRegionTraverser.VehicleRegionEntry) null, (VehicleRegionTraverser.VehicleRegionProcessor) (reg =>
          {
            reg.mark = inRadiusMark;
            ++maxRegions;
            return reg == locusReg;
          }));
        }
      }
    }
    int num1 = Mathf.CeilToInt(req.maxRange);
    CellRect cellRect2;
    // ISSUE: explicit constructor call
    ((CellRect) ref cellRect2).\u002Ector(targetPos.x - num1, targetPos.z - num1, num1 * 2 + 1, num1 * 2 + 1);
    ((CellRect) ref cellRect1).ClipInsideRect(cellRect2);
    if ((double) req.maxRangeFromLocus > 0.0099999997764825821)
    {
      int num2 = Mathf.CeilToInt(req.maxRangeFromLocus);
      CellRect cellRect3;
      // ISSUE: explicit constructor call
      ((CellRect) ref cellRect3).\u002Ector(targetPos.x - num2, targetPos.z - num2, num2 * 2 + 1, num2 * 2 + 1);
      ((CellRect) ref cellRect1).ClipInsideRect(cellRect3);
    }
    IntVec3 bestSpot = IntVec3.Invalid;
    float bestSpotPref = 1f / 1000f;
    if (req.preferredCastPosition.HasValue)
    {
      IntVec3 intVec3_3 = req.preferredCastPosition.Value;
      if (((IntVec3) ref intVec3_3).IsValid)
      {
        EvaluateCell(in req, req.preferredCastPosition.Value);
        if (((IntVec3) ref bestSpot).IsValid && (double) bestSpotPref > 1.0 / 1000.0)
        {
          dest = req.preferredCastPosition.Value;
          return true;
        }
      }
    }
    EvaluateCell(in req, vehiclePos);
    if ((double) bestSpotPref >= 1.0)
    {
      dest = vehiclePos;
      return true;
    }
    CellLine cellLine1 = CellLine.Between(targetPos, vehiclePos);
    float num3 = (float) (-1.0 / (double) ((CellLine) ref cellLine1).Slope);
    CellLine cellLine2;
    // ISSUE: explicit constructor call
    ((CellLine) ref cellLine2).\u002Ector(targetPos, num3);
    bool flag = ((CellLine) ref cellLine2).CellIsAbove(vehiclePos);
    foreach (IntVec3 cell in cellRect1)
    {
      if (((CellLine) ref cellLine2).CellIsAbove(cell) == flag && ((CellRect) ref cellRect1).Contains(cell))
        EvaluateCell(in req, cell);
    }
    if (((IntVec3) ref bestSpot).IsValid && (double) bestSpotPref > 0.33000001311302185)
    {
      dest = bestSpot;
      return true;
    }
    foreach (IntVec3 cell in cellRect1)
    {
      if (((CellLine) ref cellLine2).CellIsAbove(cell) != flag && ((CellRect) ref cellRect1).Contains(cell))
        EvaluateCell(in req, cell);
    }
    if (((IntVec3) ref bestSpot).IsValid)
    {
      dest = bestSpot;
      return true;
    }
    dest = vehiclePos;
    return false;

    void EvaluateCell(in CastPositionRequest req, IntVec3 cell)
    {
      if (req.validator != null && !req.validator(cell))
        return;
      if ((double) maxRangeFromLocusSquared > 0.0099999997764825821)
      {
        IntVec3 intVec3 = IntVec3.op_Subtraction(cell, req.locus);
        if ((double) ((IntVec3) ref intVec3).LengthHorizontalSquared > (double) maxRangeFromLocusSquared)
        {
          if (!DebugViewSettings.drawCastPositionSearch)
            return;
          map.debugDrawer.FlashCell(cell, 0.1f, "home", 50);
          return;
        }
      }
      if ((double) maxRangeSquared > 0.0099999997764825821)
      {
        IntVec3 intVec3 = IntVec3.op_Subtraction(cell, ((Thing) vehicle).Position);
        rangeFromCasterToCellSquared = (float) ((IntVec3) ref intVec3).LengthHorizontalSquared;
        if ((double) rangeFromCasterToCellSquared > (double) maxRangeSquared)
        {
          if (!DebugViewSettings.drawCastPositionSearch)
            return;
          map.debugDrawer.FlashCell(cell, 0.2f, "cstr", 50);
          return;
        }
      }
      if (!GenGrid.Standable(cell, map))
        return;
      int num1 = Rand.Int;
      if (req.maxRegions > 0 && VehicleRegionAndRoomQuery.RegionAt(cell, map, vehicle.VehicleDef).mark != num1)
      {
        if (!DebugViewSettings.drawCastPositionSearch)
          return;
        map.debugDrawer.FlashCell(cell, 0.64f, "rad mark", 50);
      }
      else if (!vehicle.CanReachVehicle(LocalTargetInfo.op_Implicit(cell), (PathEndMode) 1, (Danger) 2, (TraverseMode) 0))
      {
        if (!DebugViewSettings.drawCastPositionSearch)
          return;
        map.debugDrawer.FlashCell(cell, 0.4f, "can't reach", 50);
      }
      else
      {
        float num2 = CastPositionPreference(in req, cell);
        if (avoidGrid != null)
        {
          byte num3 = avoidGrid[cell];
          num2 *= Mathf.Max(0.1f, (float) ((37.5 - (double) num3) / 37.5));
        }
        if (DebugViewSettings.drawCastPositionSearch)
          map.debugDrawer.FlashCell(cell, num2 / 4f, num2.ToString("F3"), 50);
        if ((double) num2 < (double) bestSpotPref)
          return;
        if (!map.pawnDestinationReservationManager.CanReserve(cell, (Pawn) vehicle, false))
        {
          if (!DebugViewSettings.drawCastPositionSearch)
            return;
          map.debugDrawer.FlashCell(cell, num2 * 0.9f, "resvd", 50);
        }
        else if (PawnUtility.KnownDangerAt(cell, map, (Pawn) vehicle))
        {
          if (!DebugViewSettings.drawCastPositionSearch)
            return;
          map.debugDrawer.FlashCell(cell, 0.9f, "danger", 50);
        }
        else
        {
          bestSpot = cell;
          bestSpotPref = num2;
        }
      }
    }

    float CastPositionPreference(in CastPositionRequest req, IntVec3 cell)
    {
      bool flag = true;
      List<Thing> thingList = map.thingGrid.ThingsListAtFast(cell);
      for (int index = 0; index < thingList.Count; ++index)
      {
        Thing thing = thingList[index];
        if (thing is Fire fire && ((AttachableThing) fire).parent == null)
          return -1f;
        if (((BuildableDef) thing.def).passability == 1)
          flag = false;
      }
      float num1 = 0.3f;
      if (vehicle.kindDef.aiAvoidCover)
        num1 += 8f - CoverUtility.TotalSurroundingCoverScore(cell, map);
      IntVec3 intVec3_1 = IntVec3.op_Subtraction(vehiclePos, cell);
      float num2 = ((IntVec3) ref intVec3_1).LengthHorizontal;
      if ((double) rangeFromTarget > 100.0)
      {
        num2 -= rangeFromTarget - 100f;
        if ((double) num2 < 0.0)
          num2 = 0.0f;
      }
      float num3 = num1 * Mathf.Pow(0.967f, num2);
      IntVec3 intVec3_2 = IntVec3.op_Subtraction(cell, targetPos);
      rangeFromTargetToCellSquared = (float) ((IntVec3) ref intVec3_2).LengthHorizontalSquared;
      float num4 = (float) (0.699999988079071 + 0.30000001192092896 * (double) (1f - Mathf.Abs(rangeFromTargetToCellSquared - optimalRangeSquared) / optimalRangeSquared));
      if ((double) rangeFromTargetToCellSquared < 25.0)
        num4 *= 0.5f;
      float num5 = num3 * num4;
      if ((double) rangeFromCasterToCellSquared > (double) rangeFromTargetSquared)
        num5 *= 0.4f;
      if (!flag)
        num5 *= 0.2f;
      return num5;
    }
  }
}
