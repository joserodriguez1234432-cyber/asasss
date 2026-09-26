// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleTurretAlternating
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public class VehicleTurretAlternating : VehicleTurret
{
  public VehicleTurretAlternating(VehiclePawn vehicle)
    : base(vehicle)
  {
  }

  public VehicleTurretAlternating(VehiclePawn vehicle, VehicleTurretAlternating reference)
    : base(vehicle, (VehicleTurret) reference)
  {
  }

  public override CompVehicleTurrets.TurretData GenerateTurretData()
  {
    return new CompVehicleTurrets.TurretData()
    {
      shots = ((IntRange) ref this.CurrentFireMode.shotsPerBurst).RandomInRange,
      ticksTillShot = this.CurrentFireMode.ticksBetweenShots / this.GroupTurrets.Count * this.GroupTurrets.FindIndex((Predicate<VehicleTurret>) (t => t == this)),
      turret = (VehicleTurret) this
    };
  }

  public override IEnumerable<string> ConfigErrors(VehicleDef vehicleDef)
  {
    VehicleTurretAlternating turretAlternating = this;
    // ISSUE: reference to a compiler-generated method
    foreach (string str in turretAlternating.\u003C\u003En__0(vehicleDef))
      yield return str;
    if (GenText.NullOrEmpty(turretAlternating.groupKey))
      yield return "<field>groupKey</field> must be populated for a turret of type <type>VehicleTurretAlternating</type>".ConvertRichText();
  }
}
