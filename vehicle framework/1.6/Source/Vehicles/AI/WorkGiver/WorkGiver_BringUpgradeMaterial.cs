// Decompiled with JetBrains decompiler
// Type: Vehicles.WorkGiver_BringUpgradeMaterial
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace Vehicles;

public class WorkGiver_BringUpgradeMaterial : WorkGiver_CarryToVehicle<ThingDefCountClass>
{
  public override string ReservationName => "LoadUpgradeMaterials";

  public override JobDef JobDef => JobDefOf_Vehicles.LoadUpgradeMaterials;

  protected override bool JobAvailable(Pawn pawn, VehiclePawn vehicle)
  {
    if (!((Thing) vehicle).Spawned)
      return false;
    CompUpgradeTree compUpgradeTree = vehicle.CompUpgradeTree;
    return compUpgradeTree != null && compUpgradeTree.Upgrading && !vehicle.CompUpgradeTree.upgrade.Removal && ((Thing) vehicle).Map.GetCachedMapComponent<VehicleReservationManager>().VehicleListed(vehicle, "LoadUpgradeMaterials") && !vehicle.CompUpgradeTree.StoredCostSatisfied;
  }

  protected override List<ThingDefCountClass> GetThingsToLoad(VehiclePawn vehicle, Pawn pawn)
  {
    return !vehicle.CompUpgradeTree.Upgrading ? (List<ThingDefCountClass>) null : vehicle.CompUpgradeTree.upgrade.node.MaterialsRequired(vehicle).ToList<ThingDefCountClass>();
  }

  protected override Thing FindThingToPack(
    VehiclePawn vehicle,
    Pawn pawn,
    List<ThingDefCountClass> things)
  {
    return JobDriverGetItemForVehicleBase.FindThingToPack(vehicle, pawn, this.JobDef, (IEnumerable<ThingDefCountClass>) things);
  }
}
