// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.GenUIOnVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public static class GenUIOnVehicle
{
  private static readonly List<Thing> cellThings = new List<Thing>(32 /*0x20*/);
  public static VehiclePawnWithMap vehicleForSelector;

  public static List<Thing> ThingsUnderMouse(
    Vector3 clickPos,
    float pawnWideClickRadius,
    TargetingParameters clickParams,
    ITargetingSource source)
  {
    return GenUIOnVehicle.ThingsUnderMouse(clickPos, pawnWideClickRadius, clickParams, source, GenUIOnVehicle.vehicleForSelector);
  }

  public static List<Thing> ThingsUnderMouse(
    Vector3 clickPos,
    float pawnWideClickRadius,
    TargetingParameters clickParams,
    ITargetingSource source,
    VehiclePawnWithMap vehicle)
  {
    Vector3 mouseMapPosition = UI.MouseMapPosition();
    IntVec3 intVec = IntVec3.FromVector3(clickPos);
    Map map = vehicle != null ? vehicle.CurrentLevel : Find.CurrentMap;
    List<Thing> list = new List<Thing>();
    IReadOnlyList<Pawn> allPawnsSpawned = Find.CurrentMap.mapPawns.AllPawnsSpawned;
    foreach (Pawn pawn in (IEnumerable<Pawn>) allPawnsSpawned)
    {
      if (pawn != vehicle && (double) GenGeo.MagnitudeHorizontal(Vector3.op_Subtraction(((Thing) pawn).DrawPos, mouseMapPosition)) < 0.40000000596046448 && clickParams.CanTarget(TargetInfo.op_Implicit((Thing) pawn), source))
      {
        list.Add((Thing) pawn);
        list.AddRange(ContainingSelectionUtility.SelectableContainedThings((Thing) pawn));
      }
    }
    list.Sort(new Comparison<Thing>(CompareThingsByDistanceToMousePointer));
    GenUIOnVehicle.cellThings.Clear();
    foreach (Thing thing in map.thingGrid.ThingsAt(intVec))
    {
      if (!list.Contains(thing) && clickParams.CanTarget(TargetInfo.op_Implicit(thing), source))
      {
        GenUIOnVehicle.cellThings.Add(thing);
        GenUIOnVehicle.cellThings.AddRange(ContainingSelectionUtility.SelectableContainedThings(thing));
      }
    }
    foreach (IntVec3 adjacentCell in GenAdj.AdjacentCells)
    {
      IntVec3 intVec3 = IntVec3.op_Addition(adjacentCell, intVec);
      if (GenGrid.InBounds(intVec3, map) && GridsUtility.GetItemCount(intVec3, map) > 1)
      {
        foreach (Thing thing in map.thingGrid.ThingsAt(intVec3))
        {
          if (thing.def.category == 2 && (double) GenGeo.MagnitudeHorizontalSquared(Vector3.op_Subtraction(GenThing.TrueCenter(thing), mouseMapPosition)) <= 0.25 && !list.Contains(thing) && clickParams.CanTarget(TargetInfo.op_Implicit(thing), source))
            GenUIOnVehicle.cellThings.Add(thing);
        }
      }
    }
    foreach (Thing thing in map.listerThings.ThingsInGroup((ThingRequestGroup) 53).Where<Thing>((Func<Thing, bool>) (thing3 =>
    {
      if (thing3.CustomRectForSelector.HasValue)
      {
        CellRect cellRect = thing3.CustomRectForSelector.Value;
        if (((CellRect) ref cellRect).Contains(intVec) && !list.Contains(thing3))
          return clickParams.CanTarget(TargetInfo.op_Implicit(thing3), source);
      }
      return false;
    })))
      GenUIOnVehicle.cellThings.Add(thing);
    GenUIOnVehicle.cellThings.Sort(new Comparison<Thing>(CompareThingsByDrawAltitudeOrDistToItem));
    list.AddRange((IEnumerable<Thing>) GenUIOnVehicle.cellThings);
    GenUIOnVehicle.cellThings.Clear();
    foreach (Pawn pawn in (IEnumerable<Pawn>) allPawnsSpawned)
    {
      if (pawn != vehicle && (double) GenGeo.MagnitudeHorizontal(Vector3.op_Subtraction(((Thing) pawn).DrawPos, mouseMapPosition)) < (double) pawnWideClickRadius && clickParams.CanTarget(TargetInfo.op_Implicit((Thing) pawn), source))
        GenUIOnVehicle.cellThings.Add((Thing) pawn);
    }
    GenUIOnVehicle.cellThings.Sort(new Comparison<Thing>(CompareThingsByDistanceToMousePointer));
    foreach (Thing thing in GenUIOnVehicle.cellThings.Where<Thing>((Func<Thing, bool>) (t => !list.Contains(t))))
    {
      list.Add(thing);
      list.AddRange(ContainingSelectionUtility.SelectableContainedThings(thing));
    }
    list.RemoveAll((Predicate<Thing>) (thing => !clickParams.CanTarget(TargetInfo.op_Implicit(thing), source)));
    list.RemoveAll((Predicate<Thing>) (thing => thing is Pawn pawn1 && InvisibilityUtility.IsHiddenFromPlayer(pawn1)));
    list.Remove((Thing) vehicle);
    return list;

    int CompareThingsByDistanceToMousePointer(Thing a, Thing b)
    {
      float num1 = GenGeo.MagnitudeHorizontalSquared(Vector3.op_Subtraction(a.DrawPosHeld.Value, mouseMapPosition));
      float num2 = GenGeo.MagnitudeHorizontalSquared(Vector3.op_Subtraction(b.DrawPosHeld.Value, mouseMapPosition));
      if ((double) num1 < (double) num2)
        return -1;
      return !Mathf.Approximately(num1, num2) ? 1 : b.Spawned.CompareTo(a.Spawned);
    }

    int CompareThingsByDrawAltitudeOrDistToItem(Thing A, Thing B)
    {
      if (A.def.category == 2 && B.def.category == 2)
        return GenGeo.MagnitudeHorizontalSquared(Vector3.op_Subtraction(GenThing.TrueCenter(A), mouseMapPosition)).CompareTo(GenGeo.MagnitudeHorizontalSquared(Vector3.op_Subtraction(GenThing.TrueCenter(B), mouseMapPosition)));
      Thing spawnedParentOrMe1 = A.SpawnedParentOrMe;
      Thing spawnedParentOrMe2 = B.SpawnedParentOrMe;
      return Mathf.Approximately(((BuildableDef) spawnedParentOrMe1.def).Altitude, ((BuildableDef) spawnedParentOrMe2.def).Altitude) ? B.Spawned.CompareTo(A.Spawned) : ((BuildableDef) spawnedParentOrMe2.def).Altitude.CompareTo(((BuildableDef) spawnedParentOrMe1.def).Altitude);
    }
  }

  public static IEnumerable<LocalTargetInfo> TargetsAtMouse(
    TargetingParameters clickParams,
    bool thingsOnly = false,
    ITargetingSource source = null)
  {
    Vector3 vector3 = UI.MouseMapPosition();
    if (source != null)
      TargetMapUtility.set_TargetMap(source.Caster, Find.CurrentMap);
    VehiclePawnWithMap vehicle;
    bool flag1 = !vector3.TryGetVehicleMap(Find.CurrentMap, out vehicle, VehicleMapFlag.None);
    if (!flag1)
    {
      bool flag2;
      switch (source)
      {
        case Verb_Jump _:
        case Verb_CastAbilityJump _:
        case Verb_LaunchZipline _:
          flag2 = true;
          break;
        default:
          flag2 = false;
          break;
      }
      flag1 = !flag2;
    }
    if (flag1)
      return GenUIOnVehicle.TargetsAt(vector3, clickParams, thingsOnly, source, vehicle, false);
    TargetMapUtility.set_TargetMap(source.Caster, vehicle.VehicleMap);
    return GenUIOnVehicle.TargetsAt(vector3, clickParams, thingsOnly, source, vehicle);
  }

  public static IEnumerable<LocalTargetInfo> TargetsAt(
    Vector3 clickPos,
    TargetingParameters clickParams,
    bool thingsOnly,
    ITargetingSource source = null,
    bool convToVehicleMap = true)
  {
    return GenUIOnVehicle.TargetsAt(clickPos, clickParams, thingsOnly, source, GenUIOnVehicle.vehicleForSelector, convToVehicleMap);
  }

  public static IEnumerable<LocalTargetInfo> TargetsAt(
    Vector3 clickPos,
    TargetingParameters clickParams,
    bool thingsOnly,
    ITargetingSource source,
    VehiclePawnWithMap vehicle,
    bool convToVehicleMap = true)
  {
    List<Thing> clickableList = vehicle != null ? GenUIOnVehicle.ThingsUnderMouse(clickPos.ToVehicleMapCoord(vehicle), 0.8f, clickParams, source, vehicle) : GenUI.ThingsUnderMouse(clickPos, 0.8f, clickParams, source);
    Thing caster = source?.Caster;
    int num;
    for (int i = 0; i < clickableList.Count; i = num + 1)
    {
      if (clickableList[i] is VehiclePawn vehiclePawn && vehiclePawn == FloatMenuMakerMap.makingFor)
      {
        num = i;
      }
      else
      {
        if (!(clickableList[i] is Pawn pawn) || !InvisibilityUtility.IsPsychologicallyInvisible(pawn) || caster == null || caster.Faction == ((Thing) pawn).Faction)
          yield return LocalTargetInfo.op_Implicit(clickableList[i]);
        num = i;
      }
    }
    if (!thingsOnly)
    {
      IntVec3 intVec3 = !convToVehicleMap || vehicle == null ? IntVec3Utility.ToIntVec3(clickPos) : IntVec3Utility.ToIntVec3(clickPos.ToVehicleMapCoord(vehicle));
      Map map = !convToVehicleMap || vehicle == null ? Find.CurrentMap : vehicle.VehicleMap;
      if (GenGrid.InBounds(intVec3, map, clickParams.mapBoundsContractedBy) && clickParams.CanTarget(new TargetInfo(intVec3, map, false), source))
        yield return LocalTargetInfo.op_Implicit(intVec3);
    }
  }
}
