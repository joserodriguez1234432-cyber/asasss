// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CastPositionFinderOnVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public static class CastPositionFinderOnVehicle
{
  private static CastPositionRequest req;
  private static IntVec3 casterLoc;
  private static IntVec3 targetLoc;
  private static Verb verb;
  private static float rangeFromTarget;
  private static float rangeFromTargetSquared;
  private static float optimalRangeSquared;
  private static float rangeFromCasterToCellSquared;
  private static float rangeFromTargetToCellSquared;
  private static int inRadiusMark;
  private static NativeArray<byte>.ReadOnly avoidGrid;
  private static float maxRangeFromCasterSquared;
  private static float maxRangeFromTargetSquared;
  private static float maxRangeFromLocusSquared;
  private static IntVec3 bestSpot = IntVec3.Invalid;
  private static float bestSpotPref = 1f / 1000f;
  private static NativeArray<byte> emptyByteArray = NativeArrayUtility.EmptyArray<byte>();
  private const float BaseAIPreference = 0.3f;
  private const float MinimumPreferredRange = 5f;
  private const float OptimalRangeFactor = 0.8f;
  private const float OptimalRangeFactorImportance = 0.3f;
  private const float CoverPreferenceFactor = 0.55f;

  public static bool TryFindCastPosition(CastPositionRequest newReq, out IntVec3 dest)
  {
    // ISSUE: object of a compiler-generated type is created
    // ISSUE: variable of a compiler-generated type
    CastPositionFinderOnVehicle.\u003C\u003Ec__DisplayClass0_0 cDisplayClass00 = new CastPositionFinderOnVehicle.\u003C\u003Ec__DisplayClass0_0();
    CastPositionFinderOnVehicle.req = newReq;
    CastPositionFinderOnVehicle.casterLoc = ((Thing) CastPositionFinderOnVehicle.req.caster).Position;
    CastPositionFinderOnVehicle.targetLoc = CastPositionFinderOnVehicle.req.target.PositionOnAnotherThingMap((Thing) CastPositionFinderOnVehicle.req.caster);
    CastPositionFinderOnVehicle.verb = CastPositionFinderOnVehicle.req.verb;
    AvoidGrid avoidGrid;
    CastPositionFinderOnVehicle.avoidGrid = PawnUtility.TryGetAvoidGrid(newReq.caster, ref avoidGrid, true) ? avoidGrid.Grid : CastPositionFinderOnVehicle.emptyByteArray.AsReadOnly();
    if (CastPositionFinderOnVehicle.verb == null)
    {
      Log.Error(CastPositionFinderOnVehicle.req.caster?.ToString() + " tried to find casting position without a verb.");
      dest = IntVec3.Invalid;
      return false;
    }
    if (CastPositionFinderOnVehicle.req.maxRegions > 0)
    {
      Region region = GridsUtility.GetRegion(CastPositionFinderOnVehicle.casterLoc, ((Thing) CastPositionFinderOnVehicle.req.caster).Map, (RegionType) 14);
      if (region == null)
      {
        Log.Error("TryFindCastPosition requiring region traversal but root region is null.");
        dest = IntVec3.Invalid;
        return false;
      }
      CastPositionFinderOnVehicle.inRadiusMark = Rand.Int;
      RegionTraverser.MarkRegionsBFS(region, (RegionEntryPredicate) null, newReq.maxRegions, CastPositionFinderOnVehicle.inRadiusMark, (RegionType) 14);
      if ((double) CastPositionFinderOnVehicle.req.maxRangeFromLocus > 0.0099999997764825821)
      {
        // ISSUE: object of a compiler-generated type is created
        // ISSUE: variable of a compiler-generated type
        CastPositionFinderOnVehicle.\u003C\u003Ec__DisplayClass0_1 cDisplayClass01 = new CastPositionFinderOnVehicle.\u003C\u003Ec__DisplayClass0_1();
        // ISSUE: reference to a compiler-generated field
        cDisplayClass01.locusReg = GridsUtility.GetRegion(CastPositionFinderOnVehicle.req.locus, ((Thing) CastPositionFinderOnVehicle.req.caster).Map, (RegionType) 14);
        // ISSUE: reference to a compiler-generated field
        if (cDisplayClass01.locusReg == null)
        {
          Log.Error($"locus {CastPositionFinderOnVehicle.req.locus.ToString()} has no region");
          dest = IntVec3.Invalid;
          return false;
        }
        // ISSUE: reference to a compiler-generated field
        if (cDisplayClass01.locusReg.mark != CastPositionFinderOnVehicle.inRadiusMark)
        {
          CastPositionFinderOnVehicle.inRadiusMark = Rand.Int;
          // ISSUE: method pointer
          RegionTraverser.BreadthFirstTraverse(region, (RegionEntryPredicate) null, new RegionProcessor((object) cDisplayClass01, __methodptr(\u003CTryFindCastPosition\u003Eb__0)), 999999, (RegionType) 14);
        }
      }
    }
    // ISSUE: reference to a compiler-generated field
    cDisplayClass00.cellRect = CellRect.WholeMap(((Thing) CastPositionFinderOnVehicle.req.caster).Map);
    if ((double) CastPositionFinderOnVehicle.req.maxRangeFromCaster > 0.0099999997764825821)
    {
      int num = Mathf.CeilToInt(CastPositionFinderOnVehicle.req.maxRangeFromCaster);
      CellRect cellRect = new CellRect(CastPositionFinderOnVehicle.casterLoc.x - num, CastPositionFinderOnVehicle.casterLoc.z - num, num * 2 + 1, num * 2 + 1);
      // ISSUE: reference to a compiler-generated field
      ((CellRect) ref cDisplayClass00.cellRect).ClipInsideRect(cellRect);
    }
    int num1 = Mathf.CeilToInt(CastPositionFinderOnVehicle.req.maxRangeFromTarget);
    CellRect cellRect1 = new CellRect(CastPositionFinderOnVehicle.targetLoc.x - num1, CastPositionFinderOnVehicle.targetLoc.z - num1, num1 * 2 + 1, num1 * 2 + 1);
    // ISSUE: reference to a compiler-generated field
    ((CellRect) ref cDisplayClass00.cellRect).ClipInsideRect(cellRect1);
    if ((double) CastPositionFinderOnVehicle.req.maxRangeFromLocus > 0.0099999997764825821)
    {
      int num2 = Mathf.CeilToInt(CastPositionFinderOnVehicle.req.maxRangeFromLocus);
      CellRect cellRect2 = new CellRect(CastPositionFinderOnVehicle.targetLoc.x - num2, CastPositionFinderOnVehicle.targetLoc.z - num2, num2 * 2 + 1, num2 * 2 + 1);
      // ISSUE: reference to a compiler-generated field
      ((CellRect) ref cDisplayClass00.cellRect).ClipInsideRect(cellRect2);
    }
    CastPositionFinderOnVehicle.bestSpot = IntVec3.Invalid;
    CastPositionFinderOnVehicle.bestSpotPref = 1f / 1000f;
    CastPositionFinderOnVehicle.maxRangeFromCasterSquared = CastPositionFinderOnVehicle.req.maxRangeFromCaster * CastPositionFinderOnVehicle.req.maxRangeFromCaster;
    CastPositionFinderOnVehicle.maxRangeFromTargetSquared = CastPositionFinderOnVehicle.req.maxRangeFromTarget * CastPositionFinderOnVehicle.req.maxRangeFromTarget;
    CastPositionFinderOnVehicle.maxRangeFromLocusSquared = CastPositionFinderOnVehicle.req.maxRangeFromLocus * CastPositionFinderOnVehicle.req.maxRangeFromLocus;
    IntVec3 intVec3_1 = IntVec3.op_Subtraction(CastPositionFinderOnVehicle.casterLoc, CastPositionFinderOnVehicle.targetLoc);
    CastPositionFinderOnVehicle.rangeFromTarget = ((IntVec3) ref intVec3_1).LengthHorizontal;
    IntVec3 intVec3_2 = IntVec3.op_Subtraction(CastPositionFinderOnVehicle.casterLoc, CastPositionFinderOnVehicle.targetLoc);
    CastPositionFinderOnVehicle.rangeFromTargetSquared = (float) ((IntVec3) ref intVec3_2).LengthHorizontalSquared;
    CastPositionFinderOnVehicle.optimalRangeSquared = (float) ((double) CastPositionFinderOnVehicle.verb.verbProps.range * 0.800000011920929 * ((double) CastPositionFinderOnVehicle.verb.verbProps.range * 0.800000011920929));
    IntVec3? preferredCastPosition = CastPositionFinderOnVehicle.req.preferredCastPosition;
    if (preferredCastPosition.HasValue)
    {
      IntVec3 valueOrDefault = preferredCastPosition.GetValueOrDefault();
      if (((IntVec3) ref valueOrDefault).IsValid)
      {
        CastPositionFinderOnVehicle.EvaluateCell(CastPositionFinderOnVehicle.req.preferredCastPosition.Value);
        if (((IntVec3) ref CastPositionFinderOnVehicle.bestSpot).IsValid && (double) CastPositionFinderOnVehicle.bestSpotPref > 1.0 / 1000.0)
        {
          dest = CastPositionFinderOnVehicle.req.preferredCastPosition.Value;
          return true;
        }
      }
    }
    CastPositionFinderOnVehicle.EvaluateCell(CastPositionFinderOnVehicle.casterLoc);
    if ((double) CastPositionFinderOnVehicle.bestSpotPref >= 1.0)
    {
      dest = CastPositionFinderOnVehicle.casterLoc;
      return true;
    }
    CellLine cellLine = CellLine.Between(CastPositionFinderOnVehicle.targetLoc, CastPositionFinderOnVehicle.casterLoc);
    float num3 = (float) (-1.0 / (double) ((CellLine) ref cellLine).Slope);
    // ISSUE: reference to a compiler-generated field
    cDisplayClass00.cellLine = new CellLine(CastPositionFinderOnVehicle.targetLoc, num3);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    cDisplayClass00.flag = ((CellLine) ref cDisplayClass00.cellLine).CellIsAbove(CastPositionFinderOnVehicle.casterLoc);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated method
    foreach (IntVec3 c in ((IEnumerable<IntVec3>) (object) cDisplayClass00.cellRect).Where<IntVec3>(cDisplayClass00.\u003C\u003E9__1 ?? (cDisplayClass00.\u003C\u003E9__1 = new Func<IntVec3, bool>(cDisplayClass00.\u003CTryFindCastPosition\u003Eb__1))))
      CastPositionFinderOnVehicle.EvaluateCell(c);
    if (((IntVec3) ref CastPositionFinderOnVehicle.bestSpot).IsValid && (double) CastPositionFinderOnVehicle.bestSpotPref > 0.33000001311302185)
    {
      dest = CastPositionFinderOnVehicle.bestSpot;
      return true;
    }
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated method
    foreach (IntVec3 c in ((IEnumerable<IntVec3>) (object) cDisplayClass00.cellRect).Where<IntVec3>(cDisplayClass00.\u003C\u003E9__2 ?? (cDisplayClass00.\u003C\u003E9__2 = new Func<IntVec3, bool>(cDisplayClass00.\u003CTryFindCastPosition\u003Eb__2))))
      CastPositionFinderOnVehicle.EvaluateCell(c);
    if (((IntVec3) ref CastPositionFinderOnVehicle.bestSpot).IsValid)
    {
      dest = CastPositionFinderOnVehicle.bestSpot;
      return true;
    }
    dest = CastPositionFinderOnVehicle.casterLoc;
    return false;
  }

  private static void EvaluateCell(IntVec3 c)
  {
    Map map = ((Thing) CastPositionFinderOnVehicle.req.caster).Map;
    if (CastPositionFinderOnVehicle.req.validator != null && !CastPositionFinderOnVehicle.req.validator(c))
      return;
    float fromTargetSquared = CastPositionFinderOnVehicle.maxRangeFromTargetSquared;
    if ((double) fromTargetSquared > 0.0099999997764825821 && (double) fromTargetSquared < 250000.0)
    {
      IntVec3 intVec3 = IntVec3.op_Subtraction(c, CastPositionFinderOnVehicle.targetLoc);
      if ((double) ((IntVec3) ref intVec3).LengthHorizontalSquared > (double) CastPositionFinderOnVehicle.maxRangeFromTargetSquared)
      {
        if (!DebugViewSettings.drawCastPositionSearch)
          return;
        map.debugDrawer.FlashCell(c, 0.0f, "range target", 50);
        return;
      }
    }
    if ((double) CastPositionFinderOnVehicle.maxRangeFromLocusSquared > 0.0099999997764825821)
    {
      IntVec3 intVec3 = IntVec3.op_Subtraction(c, CastPositionFinderOnVehicle.req.locus);
      if ((double) ((IntVec3) ref intVec3).LengthHorizontalSquared > (double) CastPositionFinderOnVehicle.maxRangeFromLocusSquared)
      {
        if (!DebugViewSettings.drawCastPositionSearch)
          return;
        map.debugDrawer.FlashCell(c, 0.1f, "range home", 50);
        return;
      }
    }
    if ((double) CastPositionFinderOnVehicle.maxRangeFromCasterSquared > 0.0099999997764825821)
    {
      IntVec3 intVec3 = IntVec3.op_Subtraction(c, CastPositionFinderOnVehicle.casterLoc);
      CastPositionFinderOnVehicle.rangeFromCasterToCellSquared = (float) ((IntVec3) ref intVec3).LengthHorizontalSquared;
      if ((double) CastPositionFinderOnVehicle.rangeFromCasterToCellSquared > (double) CastPositionFinderOnVehicle.maxRangeFromCasterSquared)
      {
        if (!DebugViewSettings.drawCastPositionSearch)
          return;
        map.debugDrawer.FlashCell(c, 0.2f, "range caster", 50);
        return;
      }
    }
    if (!GenGrid.Standable(c, map))
      return;
    if (CastPositionFinderOnVehicle.req.maxRegions > 0 && GridsUtility.GetRegion(c, map, (RegionType) 14).mark != CastPositionFinderOnVehicle.inRadiusMark)
    {
      if (!DebugViewSettings.drawCastPositionSearch)
        return;
      map.debugDrawer.FlashCell(c, 0.64f, "reg radius", 50);
    }
    else
    {
      VehiclePawnWithMap vehicle;
      if (!CrossMapReachabilityUtility.CanReach(map, ((Thing) CastPositionFinderOnVehicle.req.caster).Position, LocalTargetInfo.op_Implicit(c), (PathEndMode) 1, TraverseParms.For(CastPositionFinderOnVehicle.req.caster, (Danger) 2, (TraverseMode) 0, false, false, false, true), CastPositionFinderOnVehicle.req.target.Map) && !((Thing) CastPositionFinderOnVehicle.req.caster).IsOnVehicleMapOf(out vehicle) && !CastPositionFinderOnVehicle.req.target.IsOnVehicleMapOf(out vehicle))
      {
        if (!DebugViewSettings.drawCastPositionSearch)
          return;
        map.debugDrawer.FlashCell(c, 0.4f, "can't reach", 50);
      }
      else
      {
        float num1 = CastPositionFinderOnVehicle.CastPositionPreference(c);
        if (CastPositionFinderOnVehicle.avoidGrid.Length > 0)
        {
          byte num2 = CastPositionFinderOnVehicle.avoidGrid[((CellIndices) ref ((Thing) CastPositionFinderOnVehicle.req.caster).Map.cellIndices).CellToIndex(c)];
          num1 *= Mathf.Max(0.1f, (float) ((37.5 - (double) num2) / 37.5));
        }
        if (DebugViewSettings.drawCastPositionSearch)
          map.debugDrawer.FlashCell(c, num1 / 4f, num1.ToString("F3"), 50);
        if ((double) num1 < (double) CastPositionFinderOnVehicle.bestSpotPref)
          return;
        IntVec3 thingBaseMapCoord = c.ToThingBaseMapCoord((Thing) CastPositionFinderOnVehicle.req.caster);
        if (!CastPositionFinderOnVehicle.verb.CanHitTargetFrom(thingBaseMapCoord, LocalTargetInfo.op_Implicit(CastPositionFinderOnVehicle.req.target)))
        {
          if (!DebugViewSettings.drawCastPositionSearch)
            return;
          map.debugDrawer.FlashCell(c, 0.6f, "can't hit", 50);
        }
        else if (!map.pawnDestinationReservationManager.CanReserve(c, CastPositionFinderOnVehicle.req.caster, false))
        {
          if (!DebugViewSettings.drawCastPositionSearch)
            return;
          map.debugDrawer.FlashCell(c, num1 * 0.9f, "resvd", 50);
        }
        else if (PawnUtility.KnownDangerAt(c, map, CastPositionFinderOnVehicle.req.caster))
        {
          if (!DebugViewSettings.drawCastPositionSearch)
            return;
          map.debugDrawer.FlashCell(c, 0.9f, "danger", 50);
        }
        else
        {
          CastPositionFinderOnVehicle.bestSpot = c;
          CastPositionFinderOnVehicle.bestSpotPref = num1;
        }
      }
    }
  }

  private static float CastPositionPreference(IntVec3 c)
  {
    bool flag = true;
    foreach (Thing thing in ((Thing) CastPositionFinderOnVehicle.req.caster).Map.thingGrid.ThingsAt(c))
    {
      if (thing is Fire fire && ((AttachableThing) fire).parent == null)
        return -1f;
      if (((BuildableDef) thing.def).passability == 1)
        flag = false;
    }
    float num1 = 0.3f;
    if (CastPositionFinderOnVehicle.req.caster.kindDef.aiAvoidCover)
      num1 += 8f - CoverUtility.TotalSurroundingCoverScore(c, ((Thing) CastPositionFinderOnVehicle.req.caster).Map);
    if (CastPositionFinderOnVehicle.req.wantCoverFromTarget)
      num1 += CoverUtility.CalculateOverallBlockChance(LocalTargetInfo.op_Implicit(c), CastPositionFinderOnVehicle.targetLoc, ((Thing) CastPositionFinderOnVehicle.req.caster).Map) * 0.55f;
    IntVec3 intVec3_1 = IntVec3.op_Subtraction(CastPositionFinderOnVehicle.casterLoc, c);
    float num2 = ((IntVec3) ref intVec3_1).LengthHorizontal;
    if ((double) CastPositionFinderOnVehicle.rangeFromTarget > 100.0)
    {
      num2 -= CastPositionFinderOnVehicle.rangeFromTarget - 100f;
      if ((double) num2 < 0.0)
        num2 = 0.0f;
    }
    float num3 = num1 * Mathf.Pow(0.967f, num2);
    float num4 = 1f;
    IntVec3 intVec3_2 = IntVec3.op_Subtraction(c, CastPositionFinderOnVehicle.targetLoc);
    CastPositionFinderOnVehicle.rangeFromTargetToCellSquared = (float) ((IntVec3) ref intVec3_2).LengthHorizontalSquared;
    float num5 = (float) (0.699999988079071 + 0.30000001192092896 * (double) (1f - Mathf.Abs(CastPositionFinderOnVehicle.rangeFromTargetToCellSquared - CastPositionFinderOnVehicle.optimalRangeSquared) / CastPositionFinderOnVehicle.optimalRangeSquared));
    float num6 = num4 * num5;
    if ((double) CastPositionFinderOnVehicle.rangeFromTargetToCellSquared < 25.0)
      num6 *= 0.5f;
    float num7 = num3 * num6;
    if ((double) CastPositionFinderOnVehicle.rangeFromCasterToCellSquared > (double) CastPositionFinderOnVehicle.rangeFromTargetSquared)
      num7 *= 0.4f;
    if (!flag)
      num7 *= 0.2f;
    return num7;
  }
}
