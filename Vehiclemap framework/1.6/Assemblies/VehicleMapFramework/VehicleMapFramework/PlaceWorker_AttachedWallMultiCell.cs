// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.PlaceWorker_AttachedWallMultiCell
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class PlaceWorker_AttachedWallMultiCell : Placeworker_AttachedToWall
{
  public virtual AcceptanceReport AllowsPlacing(
    BuildableDef checkingDef,
    IntVec3 loc,
    Rot4 rot,
    Map map,
    Thing thingToIgnore = null,
    Thing thing = null)
  {
    if (((IEnumerable<IntVec3>) (object) GenAdj.OccupiedRect(loc, rot, checkingDef.Size)).Any<IntVec3>((Func<IntVec3, bool>) (c => GenCollection.Any<Thing>(GridsUtility.GetThingList(c, map), (Predicate<Thing>) (t =>
    {
      if (t == thingToIgnore)
        return false;
      ThingDef def = t.def;
      bool? nullable;
      if (def == null)
      {
        nullable = new bool?();
      }
      else
      {
        List<PlaceWorker> placeWorkers = ((BuildableDef) def).PlaceWorkers;
        nullable = placeWorkers != null ? new bool?(GenCollection.Any<PlaceWorker>(placeWorkers, (Predicate<PlaceWorker>) (p => p is PlaceWorker_AttachedWallMultiCell))) : new bool?();
      }
      return nullable.GetValueOrDefault();
    })))))
      return AcceptanceReport.op_Implicit(Translator.Translate("SpaceAlreadyOccupied"));
    CellRect cellRect = GenAdj.OccupiedRect(loc, rot, checkingDef.Size);
    AcceptanceReport acceptanceReport = base.AllowsPlacing(checkingDef, ((CellRect) ref cellRect).GetCenterCellOnEdge(rot), rot, map, thingToIgnore, thing);
    if (((AcceptanceReport) ref acceptanceReport).Accepted || ((CellRect) ref cellRect).GetSideLength(rot) % 2 != 0)
      return acceptanceReport;
    acceptanceReport = base.AllowsPlacing(checkingDef, ((CellRect) ref cellRect).GetCenterCellOnEdge(rot, -1), rot, map, thingToIgnore, thing);
    return acceptanceReport;
  }
}
