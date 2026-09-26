// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.PlaceWorker_VehicleRelatedBuildings
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using Verse;

#nullable disable
namespace VehicleMapFramework;

public class PlaceWorker_VehicleRelatedBuildings : PlaceWorker
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
    return map.IsVehicleMapOf(out vehicle) && checkingDef is ThingDef thingDef && ((ThingDef) vehicle.VehicleDef.buildDef).building?.relatedBuildCommands?.Contains(thingDef).GetValueOrDefault() ? AcceptanceReport.op_Implicit(true) : AcceptanceReport.op_Implicit(Translator.Translate("VMF_ForceOnRelatedVehicle"));
  }
}
