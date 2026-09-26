// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.PlaceWorker_NonFueledVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class PlaceWorker_NonFueledVehicle : PlaceWorker
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
    if (map.IsVehicleMapOf(out vehicle) && ((Thing) vehicle).def.HasComp<CompFueledTravel>())
      return AcceptanceReport.op_Implicit(true);
    if (!ModsConfig.OdysseyActive)
      return AcceptanceReport.op_Implicit(Translator.Translate("VMF_ForbidOnHumanPoweredVehicle"));
    CellRect source = GenAdj.OccupiedRect(loc, rot, checkingDef is ThingDef thingDef ? ((BuildableDef) thingDef).Size : IntVec2.One);
    Building_GravEngine gravEngineNewTemp = GravshipUtility.GetPlayerGravEngine_NewTemp(map);
    return gravEngineNewTemp != null && ((IEnumerable<IntVec3>) (object) source).All<IntVec3>(new Func<IntVec3, bool>(gravEngineNewTemp.ValidSubstructureAt)) ? AcceptanceReport.op_Implicit(true) : AcceptanceReport.op_Implicit(Translator.Translate("VMF_ForbidOnHumanPoweredVehicle"));
  }
}
