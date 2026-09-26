// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.PlaceWorker_ForceOnVehicleMapEdge
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class PlaceWorker_ForceOnVehicleMapEdge : PlaceWorker
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
      return AcceptanceReport.op_Implicit(Translator.Translate("VMF_ForceOnVehicle"));
    return ((IEnumerable<IntVec3>) (object) GenAdj.OccupiedRect(loc, rot, checkingDef.Size)).Select<IntVec3, IntVec3>((Func<IntVec3, IntVec3>) (cell => IntVec3.op_Subtraction(cell, ((Rot4) ref rot).FacingCell))).Any<IntVec3>((Func<IntVec3, bool>) (facingCell =>
    {
      if (vehicle.OutOfBoundsGrid[facingCell])
        return false;
      return !vehicle.ExpandableGrid[facingCell] || !vehicle.ImpassableCellGrid[facingCell];
    })) ? AcceptanceReport.op_Implicit(Translator.Translate("VMF_ForceOnVehicleMapEdge")) : AcceptanceReport.op_Implicit(true);
  }
}
