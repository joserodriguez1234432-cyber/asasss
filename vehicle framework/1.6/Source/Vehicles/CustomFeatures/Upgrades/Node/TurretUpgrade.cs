// Decompiled with JetBrains decompiler
// Type: Vehicles.TurretUpgrade
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class TurretUpgrade : Upgrade
{
  public List<VehicleTurret> turrets;
  public List<string> removeTurrets;

  public override bool UnlockOnLoad => false;

  public override bool HasGraphics
  {
    get
    {
      return this.turrets.NotNullAndAny<VehicleTurret>((Predicate<VehicleTurret>) (turret => !turret.NoGraphic)) || !GenList.NullOrEmpty<string>((IList<string>) this.removeTurrets);
    }
  }

  public override void Unlock(VehiclePawn vehicle, bool unlockingPostLoad)
  {
    if (!unlockingPostLoad)
    {
      if (!GenList.NullOrEmpty<string>((IList<string>) this.removeTurrets))
      {
        foreach (string removeTurret in this.removeTurrets)
        {
          if (!vehicle.CompVehicleTurrets.RemoveTurret(removeTurret))
            Log.Error($"Unable to remove {removeTurret} from {vehicle}. Turret not found.");
        }
      }
      if (!GenList.NullOrEmpty<VehicleTurret>((IList<VehicleTurret>) this.turrets))
      {
        foreach (VehicleTurret turret in this.turrets)
        {
          try
          {
            vehicle.CompVehicleTurrets.CopyAndAddTurret(turret, this.node.key);
          }
          catch (Exception ex)
          {
            Log.Error($"{"[VehicleFramework]"} Unable to unlock {this.GetType()} to {((Entity) vehicle).LabelShort}. \nException: {ex}");
          }
        }
      }
    }
    vehicle.CompVehicleTurrets.CheckDuplicateKeys();
  }

  public override void Refund(VehiclePawn vehicle)
  {
    if (!GenList.NullOrEmpty<VehicleTurret>((IList<VehicleTurret>) this.turrets))
    {
      foreach (VehicleTurret turret in this.turrets)
      {
        if (!vehicle.CompVehicleTurrets.RemoveTurret(turret.key))
          Log.Error($"Unable to remove {turret.key} from {vehicle}. Turret not found.");
      }
    }
    if (!GenList.NullOrEmpty<string>((IList<string>) this.removeTurrets))
    {
      foreach (string removeTurret in this.removeTurrets)
      {
        string key = removeTurret;
        VehicleTurret reference = GenCollection.FirstOrDefault<VehicleTurret>(vehicle.CompVehicleTurrets.Props.turrets, (Predicate<VehicleTurret>) (turret => turret.key == key));
        if (reference == null)
          Log.Error($"Unable to add {key} to {vehicle}. Turret must be defined in the VehicleDef in order to be re-added post-refund.");
        else
          vehicle.CompVehicleTurrets.CopyAndAddTurret(reference);
      }
    }
    vehicle.CompVehicleTurrets.CheckDuplicateKeys();
  }
}
