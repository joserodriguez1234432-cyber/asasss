// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.PlaceWorker_MapExpander
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class PlaceWorker_MapExpander : PlaceWorker
{
  public virtual AcceptanceReport AllowsPlacing(
    BuildableDef checkingDef,
    IntVec3 loc,
    Rot4 rot,
    Map map,
    Thing thingToIgnore = null,
    Thing thing = null)
  {
    VehiclePawnWithMap vehicle;
    if (!map.IsVehicleMapOf(out vehicle))
      return AcceptanceReport.op_Implicit(Translator.Translate("VMF_ForbidOnVehicle"));
    return !vehicle.ExpandableGrid[loc] || ((IEnumerable<IntVec3>) (object) GenAdj.OccupiedRect(loc, rot, checkingDef.Size)).Any<IntVec3>((Func<IntVec3, bool>) (c =>
    {
      IntVec3 intVec3 = c;
      Rot4 insideMap = c.DirectionToInsideMap(vehicle);
      IntVec3 asIntVec3 = ((Rot4) ref insideMap).AsIntVec3;
      return GenCollection.Any<Thing>(GridsUtility.GetThingList(IntVec3.op_Addition(intVec3, asIntVec3), vehicle.VehicleMap), (Predicate<Thing>) (t =>
      {
        List<PlaceWorker> placeWorkers = ((BuildableDef) t.def).PlaceWorkers;
        return placeWorkers != null && GenCollection.Any<PlaceWorker>(placeWorkers, (Predicate<PlaceWorker>) (p => p is PlaceWorker_ForceOnVehicleMapEdge));
      }));
    })) ? AcceptanceReport.op_Implicit(Translator.Translate("VMF_ForceOnExpandableCell")) : AcceptanceReport.op_Implicit(true);
  }
}
