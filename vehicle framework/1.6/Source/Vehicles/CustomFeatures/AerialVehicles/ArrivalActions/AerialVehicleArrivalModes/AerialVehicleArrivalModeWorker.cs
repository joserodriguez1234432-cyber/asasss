// Decompiled with JetBrains decompiler
// Type: Vehicles.AerialVehicleArrivalModeWorker
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using Verse;

#nullable disable
namespace Vehicles;

public abstract class AerialVehicleArrivalModeWorker
{
  public AerialVehicleArrivalModeDef def;

  public abstract void VehicleArrived(VehiclePawn vehicle, LaunchProtocol protocol, Map map);

  public abstract bool TryResolveRaidSpawnCenter(IncidentParms parms);

  public virtual bool CanUseWith(IncidentParms parms)
  {
    if (parms.faction != null && this.def.minTechLevel != null && parms.faction.def.techLevel < this.def.minTechLevel)
      return false;
    return !parms.raidArrivalModeForQuickMilitaryAid || this.def.forQuickMilitaryAid;
  }

  public virtual float GetSelectionWeight(IncidentParms parms)
  {
    return this.def.selectionWeightCurve != null ? this.def.selectionWeightCurve.Evaluate(parms.points) : 0.0f;
  }
}
