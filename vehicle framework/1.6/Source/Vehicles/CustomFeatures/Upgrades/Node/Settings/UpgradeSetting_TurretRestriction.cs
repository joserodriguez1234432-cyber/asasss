// Decompiled with JetBrains decompiler
// Type: Vehicles.UpgradeSetting_TurretRestriction
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using Verse;

#nullable disable
namespace Vehicles;

public class UpgradeSetting_TurretRestriction : UpgradeSetting_Turret
{
  public System.Type restrictionType;
  public UpgradeSetting_TurretRestriction.Operation operation;

  public override void Unlocked(VehiclePawn vehicle, bool unlockingPostLoad)
  {
    VehicleTurret turret = vehicle.CompVehicleTurrets.GetTurret(this.turretKey);
    if (turret == null)
    {
      Log.ErrorOnce($"Unable to locate turret with key={this.turretKey}. The turret must be part of the vehicle to add upgrades.", GenString.GetHashCodeSafe(this.turretKey));
    }
    else
    {
      if (!(this.restrictionType != (System.Type) null))
        return;
      switch (this.operation)
      {
        case UpgradeSetting_TurretRestriction.Operation.Add:
          turret.SetTurretRestriction(this.restrictionType);
          break;
        case UpgradeSetting_TurretRestriction.Operation.Remove:
          turret.RemoveTurretRestriction();
          break;
      }
    }
  }

  public override void Refunded(VehiclePawn vehicle)
  {
    VehicleTurret turret = vehicle.CompVehicleTurrets.GetTurret(this.turretKey);
    if (turret == null)
    {
      Log.ErrorOnce($"Unable to locate turret with key={this.turretKey}. The turret must be part of the vehicle to add upgrades.", GenString.GetHashCodeSafe(this.turretKey));
    }
    else
    {
      if (!(this.restrictionType != (System.Type) null))
        return;
      switch (this.operation)
      {
        case UpgradeSetting_TurretRestriction.Operation.Add:
          turret.RemoveTurretRestriction();
          break;
        case UpgradeSetting_TurretRestriction.Operation.Remove:
          turret.SetTurretRestriction(this.restrictionType);
          break;
      }
    }
  }

  public enum Operation
  {
    Add,
    Remove,
  }
}
