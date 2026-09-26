// Decompiled with JetBrains decompiler
// Type: Vehicles.JobDriver_GiveToUpgradeContainer
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public sealed class JobDriver_GiveToUpgradeContainer : JobDriverGetItemForVehicleBase
{
  protected override string ListerTag => "LoadUpgradeMaterials";

  protected override IEnumerable<ThingDefCountClass> ThingsToLoad
  {
    get
    {
      return !this.Vehicle.CompUpgradeTree.Upgrading ? (IEnumerable<ThingDefCountClass>) null : this.Vehicle.CompUpgradeTree.upgrade.node.MaterialsRequired(this.Vehicle);
    }
  }

  protected override bool ShouldFailJob()
  {
    return !this.Vehicle.CompUpgradeTree.Upgrading || !this.Vehicle.CompUpgradeTree.NodeUnlocking.AvailableSpace(this.Vehicle, this.ToHaul) || base.ShouldFailJob();
  }

  protected override bool AddItemToVehicle(VehiclePawn vehicle, Thing thing)
  {
    ThingDefCountClass thingDefCountClass = vehicle.CompUpgradeTree.NodeUnlocking.MaterialsRequired(vehicle).FirstOrDefault<ThingDefCountClass>((Func<ThingDefCountClass, bool>) (countClass => countClass.thingDef == thing.def));
    if (thingDefCountClass == null || thingDefCountClass.count <= 0)
    {
      this.pawn.jobs.EndCurrentJob((JobCondition) 4, true, true);
      return false;
    }
    int count = Mathf.Min(thingDefCountClass.count, thing.stackCount);
    vehicle.CompUpgradeTree.AddToContainer(thing, count);
    return true;
  }
}
