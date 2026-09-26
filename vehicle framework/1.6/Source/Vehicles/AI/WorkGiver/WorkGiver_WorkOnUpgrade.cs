// Decompiled with JetBrains decompiler
// Type: Vehicles.WorkGiver_WorkOnUpgrade
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System.Collections.Generic;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class WorkGiver_WorkOnUpgrade : VehicleWorkGiver
{
  public override PathEndMode PathEndMode => (PathEndMode) 2;

  public override JobDef JobDef => JobDefOf_Vehicles.UpgradeVehicle;

  public virtual IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
  {
    return (IEnumerable<Thing>) ((Thing) pawn).Map.GetCachedMapComponent<VehicleReservationManager>().VehicleListers("Upgrade");
  }

  public override bool CanBeWorkedOn(VehiclePawn vehicle)
  {
    if (vehicle.CompUpgradeTree == null || !vehicle.CompUpgradeTree.Upgrading)
      return false;
    return vehicle.CompUpgradeTree.upgrade.Removal || vehicle.CompUpgradeTree.StoredCostSatisfied;
  }
}
