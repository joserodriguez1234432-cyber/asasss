// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleTurret_Manual
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Collections.Generic;
using System.Linq;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class VehicleTurret_Manual : VehicleTurret
{
  public VehicleTurret_Manual()
  {
  }

  public VehicleTurret_Manual(VehiclePawn vehicle)
    : base(vehicle)
  {
  }

  public VehicleTurret_Manual(VehiclePawn vehicle, VehicleTurret reference)
    : base(vehicle, reference)
  {
  }

  public virtual void RecacheMannedStatus()
  {
    if (VehicleMod.settings.debug.debugShootAnyTurret)
    {
      this.IsManned = true;
    }
    else
    {
      List<VehicleRoleHandler> all = this.vehicle.handlers.FindAll((Predicate<VehicleRoleHandler>) (h =>
      {
        if ((h.role.HandlingTypes & 2) == null)
          return false;
        return h.role.TurretIds.Contains(this.key) || h.role.TurretIds.Contains(this.groupKey);
      }));
      if (GenCollection.Empty<VehicleRoleHandler>(all))
        this.IsManned = false;
      else
        this.IsManned = all.All<VehicleRoleHandler>((Func<VehicleRoleHandler, bool>) (h => h.RoleFulfilled));
    }
  }
}
