// Decompiled with JetBrains decompiler
// Type: Vehicles.SettingsUpgrade
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public class SettingsUpgrade : Upgrade
{
  public UpgradeState state;

  public override bool UnlockOnLoad => true;

  public override void Unlock(VehiclePawn vehicle, bool unlockingPostLoad)
  {
    vehicle.CompUpgradeTree.AddSettings(this.state);
    if (GenList.NullOrEmpty<UpgradeState.Setting>((IList<UpgradeState.Setting>) this.state.settings))
      return;
    foreach (UpgradeState.Setting setting in this.state.settings)
      setting.Unlocked(vehicle, unlockingPostLoad);
  }

  public override void Refund(VehiclePawn vehicle)
  {
    vehicle.CompUpgradeTree.RemoveSettings(this.state);
    if (GenList.NullOrEmpty<UpgradeState.Setting>((IList<UpgradeState.Setting>) this.state.settings))
      return;
    foreach (UpgradeState.Setting setting in this.state.settings)
      setting.Refunded(vehicle);
  }
}
