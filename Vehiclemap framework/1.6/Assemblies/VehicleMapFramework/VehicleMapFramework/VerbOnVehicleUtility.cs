// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VerbOnVehicleUtility
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using SmashTools;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public static class VerbOnVehicleUtility
{
  private static readonly List<Thing> cellThingsFiltered = new List<Thing>();
  private static readonly List<IntVec3> tempLeanShootSources = new List<IntVec3>();
  private static readonly List<IntVec3> tempDestList = new List<IntVec3>();
  private static readonly List<IntVec3> tmpCellList = new List<IntVec3>();

  public static bool TryFindShootLineFromToOnVehicle(
    this Verb verb,
    IntVec3 root,
    LocalTargetInfo targ,
    out ShootLine resultingLine,
    bool ignoreRange = false)
  {
    resultingLine = new ShootLine();
    VehiclePawnWithMap vehicle1;
    bool flag1 = verb.caster.IsOnVehicleMapOf(out vehicle1) && ((Thing) vehicle1).Spawned;
    VehiclePawnWithMap vehicle2;
    bool flag2 = ((LocalTargetInfo) ref targ).Thing.IsOnVehicleMapOf(out vehicle2) && ((Thing) vehicle2).Spawned;
    VehiclePawnWithMap vehicle3 = (VehiclePawnWithMap) null;
    Map map1;
    bool flag3 = verb.caster.TryGetTargetMap(out map1) && map1.IsVehicleMapOf(out vehicle3);
    IntVec3 positionOnBaseMap = VehicleMapUtility.get_PositionOnBaseMap(verb.caster);
    IntVec3 sourceCellBaseCol = !flag1 || IntVec3.op_Inequality(root, verb.caster.Position) ? root : positionOnBaseMap;
    Map map2 = verb.caster.BaseMap();
    IntVec3 intVec3_1 = targ.TargetCellOnBaseMap(verb.caster);
    if (((LocalTargetInfo) ref targ).HasThing && VehicleMapUtility.get_BaseMapOrCaravan(((LocalTargetInfo) ref targ).Thing) != VehicleMapUtility.get_BaseMapOrCaravan(verb.caster))
      return false;
    VehiclePawnWithMap vehicle4;
    if (flag1 && !flag2 && GenGrid.InBounds(((LocalTargetInfo) ref targ).Cell, map2) && ((LocalTargetInfo) ref targ).Cell.TryGetVehicleMap(map2, out vehicle4) && vehicle4 == vehicle2 || !flag1 & flag2 && verb.caster.Position.TryGetVehicleMap(map2, out vehicle4) && vehicle4 == vehicle1 || !flag1 & flag3 && verb.caster.Position.TryGetVehicleMap(map2, out vehicle4) && vehicle4 == vehicle3)
    {
      resultingLine = new ShootLine(sourceCellBaseCol, intVec3_1);
      return false;
    }
    if (verb.verbProps.IsMeleeAttack || (double) verb.EffectiveRange <= 1.4199999570846558)
    {
      resultingLine = new ShootLine(sourceCellBaseCol, intVec3_1);
      return ReachabilityImmediate.CanReachImmediate(verb.caster.Position, targ, verb.caster.Map, (PathEndMode) 2, (Pawn) null);
    }
    CellRect cellRect1 = ((LocalTargetInfo) ref targ).HasThing ? ((LocalTargetInfo) ref targ).Thing.MovedOccupiedRect() : CellRect.SingleCell(intVec3_1);
    if (!ignoreRange && verb.OutOfRange(sourceCellBaseCol, targ, cellRect1))
    {
      resultingLine = new ShootLine(sourceCellBaseCol, intVec3_1);
      return false;
    }
    if (!verb.verbProps.requireLineOfSight)
    {
      resultingLine = new ShootLine(sourceCellBaseCol, intVec3_1);
      return true;
    }
    if (verb.CasterIsPawn)
    {
      IntVec3 goodDest;
      if (verb.CanHitFromCellIgnoringRange(sourceCellBaseCol, targ, out goodDest))
      {
        resultingLine = new ShootLine(sourceCellBaseCol, goodDest);
        return true;
      }
      ModCompat.AsAboveSoBelow.TargetBand? targetBand1 = ModCompat.AsAboveSoBelow.GetTargetBand(verb.caster);
      ModCompat.AsAboveSoBelow.TargetBand? targetBand2 = ModCompat.AsAboveSoBelow.GetTargetBand(((LocalTargetInfo) ref targ).Thing);
      ShootLeanUtilityOnVehicle.LeanShootingSourcesFromTo(verb.caster.Position, ((CellRect) ref cellRect1).ClosestCellTo(positionOnBaseMap), verb.caster.Map, VerbOnVehicleUtility.tempLeanShootSources, targetBand1, targetBand2);
      for (int index = 0; index < VerbOnVehicleUtility.tempLeanShootSources.Count; ++index)
      {
        IntVec3 thingBaseMapCoord = VerbOnVehicleUtility.tempLeanShootSources[index].ToThingBaseMapCoord(verb.caster);
        if (verb.CanHitFromCellIgnoringRange(thingBaseMapCoord, targ, out goodDest))
        {
          resultingLine = new ShootLine(!flag1 ? VerbOnVehicleUtility.tempLeanShootSources[index] : thingBaseMapCoord, goodDest);
          return true;
        }
      }
    }
    else
    {
      CellRect cellRect2 = verb.Caster.MovedOccupiedRect();
      foreach (IntVec3 intVec3_2 in cellRect2)
      {
        IntVec3 goodDest;
        if (verb.CanHitFromCellIgnoringRange(intVec3_2, targ, out goodDest))
        {
          resultingLine = new ShootLine(!flag1 ? intVec3_2.ToThingMapCoord(verb.caster) : intVec3_2, goodDest);
          return true;
        }
      }
    }
    resultingLine = new ShootLine(sourceCellBaseCol, intVec3_1);
    return false;
  }

  public static bool CanHitFromCellIgnoringRange(
    this Verb verb,
    IntVec3 sourceCellBaseCol,
    LocalTargetInfo targ,
    out IntVec3 goodDest)
  {
    IntVec3 targetLoc = targ.TargetCellOnBaseMap(verb.caster);
    ModCompat.AsAboveSoBelow.TargetBand? targetBand1 = ModCompat.AsAboveSoBelow.GetTargetBand(verb.caster);
    if (((LocalTargetInfo) ref targ).HasThing)
    {
      ModCompat.AsAboveSoBelow.TargetBand? targetBand2 = ModCompat.AsAboveSoBelow.GetTargetBand(((LocalTargetInfo) ref targ).Thing);
      if (VehicleMapUtility.get_BaseMapOrCaravan(((LocalTargetInfo) ref targ).Thing) != VehicleMapUtility.get_BaseMapOrCaravan(verb.caster))
      {
        goodDest = IntVec3.Invalid;
        return false;
      }
      ShootLeanUtilityOnVehicle.CalcShootableCellsOf(VerbOnVehicleUtility.tempDestList, ((LocalTargetInfo) ref targ).Thing, sourceCellBaseCol, targetBand1, targetBand2);
      IntVec3 thingMapCoord = sourceCellBaseCol.ToThingMapCoord(((LocalTargetInfo) ref targ).Thing);
      for (int index = 0; index < VerbOnVehicleUtility.tempDestList.Count; ++index)
      {
        if (verb.CanHitCellFromCellIgnoringRange(thingMapCoord, VerbOnVehicleUtility.tempDestList[index], ((LocalTargetInfo) ref targ).Thing.Map, targetBand1, targetBand2, ((LocalTargetInfo) ref targ).Thing.def.Fillage == 2))
        {
          goodDest = VerbOnVehicleUtility.tempDestList[index].ToThingBaseMapCoord(((LocalTargetInfo) ref targ).Thing);
          return true;
        }
      }
    }
    else if (verb.CanHitCellFromCellIgnoringRange(sourceCellBaseCol, targetLoc, verb.Caster.BaseMap(), targetBand1, new ModCompat.AsAboveSoBelow.TargetBand?()))
    {
      goodDest = targetLoc;
      return true;
    }
    goodDest = IntVec3.Invalid;
    return false;
  }

  private static bool CanHitCellFromCellIgnoringRange(
    this Verb verb,
    IntVec3 sourceSq,
    IntVec3 targetLoc,
    Map map,
    ModCompat.AsAboveSoBelow.TargetBand? sourceBand,
    ModCompat.AsAboveSoBelow.TargetBand? targetBand,
    bool includeCorners = false)
  {
    if (verb.verbProps.mustCastOnOpenGround && (!GenGrid.Standable(targetLoc, map) || map.thingGrid.CellContains(targetLoc, (ThingCategory) 1)))
      return false;
    if (verb.verbProps.requireLineOfSight)
    {
      if (!includeCorners)
      {
        if (!GenSightOnVehicle.LineOfSight(sourceSq, targetLoc, map, sourceBand, targetBand))
          return false;
      }
      else if (!GenSightOnVehicle.LineOfSightToEdges(sourceSq, targetLoc, map, sourceBand, targetBand))
        return false;
    }
    return true;
  }

  public static bool ShouldConsiderCrossMap(Thing caster, IntVec3 root, LocalTargetInfo targ)
  {
    if (!((IntVec3) ref root).IsValid || !caster.Spawned || VehiclePawnWithMapCache.AllVehiclesOn(VehicleMapUtility.get_GroundMap(caster)).Count == 0)
      return false;
    VehiclePawnWithMap vehicle;
    Map map1;
    if (caster.IsOnVehicleMapOf(out vehicle) && ((Thing) vehicle).Spawned || ((LocalTargetInfo) ref targ).Thing.IsOnVehicleMapOf(out vehicle) && ((Thing) vehicle).Spawned || caster.TryGetTargetMap(out map1) && map1.IsVehicleMapOf(out vehicle) && ((Thing) vehicle).Spawned)
      return true;
    Map map2 = caster.Map;
    VehicleMapGrid cachedMapComponent = map2 != null ? ComponentCache.GetCachedMapComponent<VehicleMapGrid>(map2) : (VehicleMapGrid) null;
    if (cachedMapComponent == null)
      return false;
    VerbOnVehicleUtility.tmpCellList.Clear();
    GenSight.PointsOnLineOfSight(root, ((LocalTargetInfo) ref targ).Cell, (Action<IntVec3>) (c => VerbOnVehicleUtility.tmpCellList.Add(c)));
    ReadOnlySpan<IntVec3> readOnlySpan = GenList.AsReadOnlySpan<IntVec3>(VerbOnVehicleUtility.tmpCellList);
    for (int index = 0; index < readOnlySpan.Length; ++index)
    {
      IntVec3 c = readOnlySpan[index];
      if (GenGrid.InBounds(c, map2) && cachedMapComponent.VehicleAt(c) != null)
        return true;
    }
    return false;
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u00243E43D0E7D2B4805276B1289916C3688C
  {
    [ExtensionMarker("<M>$E97B3E5AA17EBA0E1008330DC1A71087")]
    public bool TryFindShootLineFromToOnVehicle(
      IntVec3 root,
      LocalTargetInfo targ,
      out ShootLine resultingLine,
      bool ignoreRange = false)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$E97B3E5AA17EBA0E1008330DC1A71087")]
    public bool CanHitFromCellIgnoringRange(
      IntVec3 sourceCellBaseCol,
      LocalTargetInfo targ,
      out IntVec3 goodDest)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$E97B3E5AA17EBA0E1008330DC1A71087")]
    private bool CanHitCellFromCellIgnoringRange(
      IntVec3 sourceSq,
      IntVec3 targetLoc,
      Map map,
      ModCompat.AsAboveSoBelow.TargetBand? sourceBand,
      ModCompat.AsAboveSoBelow.TargetBand? targetBand,
      bool includeCorners = false)
    {
      throw new NotSupportedException();
    }

    [SpecialName]
    public static class \u003CM\u003E\u0024E97B3E5AA17EBA0E1008330DC1A71087
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(Verb verb)
      {
      }
    }
  }
}
