// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.PlaceWorker_UniqueVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class PlaceWorker_UniqueVehicle : PlaceWorker
{
  public virtual AcceptanceReport AllowsPlacing(
    BuildableDef checkingDef,
    IntVec3 loc,
    Rot4 rot,
    Map map,
    Thing thingToIgnore = null,
    Thing thing = null)
  {
    if (!(checkingDef is VehicleBuildDef vehicleBuildDef))
      return AcceptanceReport.op_Implicit(true);
    VehicleDef thingToSpawn = vehicleBuildDef.thingToSpawn;
    return !UniqueVehicleUtility.AllowGenerate(thingToSpawn) ? AcceptanceReport.op_Implicit(TranslatorFormattedStringExtensions.Translate("VMF_UniqueVehicleExceedsLimit", NamedArgument.op_Implicit(((Def) thingToSpawn).label))) : AcceptanceReport.op_Implicit(true);
  }
}
