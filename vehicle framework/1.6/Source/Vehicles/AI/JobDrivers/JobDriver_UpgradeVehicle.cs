// Decompiled with JetBrains decompiler
// Type: Vehicles.JobDriver_UpgradeVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class JobDriver_UpgradeVehicle : JobDriver_WorkVehicle
{
  protected override JobDef JobDef => JobDefOf_Vehicles.UpgradeVehicle;

  protected override StatDef Stat => StatDefOf.ConstructionSpeed;

  protected override float TotalWork
  {
    get
    {
      return this.Vehicle.CompUpgradeTree.upgrade.Removal ? this.Vehicle.CompUpgradeTree.NodeUnlocking.work * 0.3f : this.Vehicle.CompUpgradeTree.NodeUnlocking.work;
    }
  }

  protected override float Work
  {
    get => this.Vehicle == null ? 0.0f : this.Vehicle.CompUpgradeTree.upgrade.WorkLeft;
    set
    {
      if (this.Vehicle == null)
        return;
      this.Vehicle.CompUpgradeTree.upgrade.WorkLeft = value;
    }
  }

  protected override void WorkComplete(Pawn actor)
  {
    this.Vehicle.CompUpgradeTree.FinishUnlock(this.Vehicle.CompUpgradeTree.NodeUnlocking);
    this.Vehicle.CompUpgradeTree.ClearUpgrade();
    actor.jobs.EndCurrentJob((JobCondition) 2, true, true);
  }

  protected override void ResetWork()
  {
  }
}
