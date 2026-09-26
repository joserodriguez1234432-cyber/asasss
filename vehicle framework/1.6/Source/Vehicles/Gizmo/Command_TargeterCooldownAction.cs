// Decompiled with JetBrains decompiler
// Type: Vehicles.Rendering.Command_TargeterCooldownAction
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using Verse;

#nullable disable
namespace Vehicles.Rendering;

public class Command_TargeterCooldownAction : Command_CooldownAction
{
  public override void FireTurrets()
  {
    this.FireTurret(this.turret);
    if (GenText.NullOrEmpty(this.turret.groupKey))
      return;
    Log.Warning("groupKey is not yet supported for Rotatable turrets.");
  }

  public override void FireTurret(VehicleTurret turret)
  {
    if (turret.ReloadTicks > 0)
      return;
    turret.SetTarget(LocalTargetInfo.Invalid);
    TurretTargeter.BeginTargeting(this.targetingParams, (Action<LocalTargetInfo>) (target =>
    {
      turret.SetTarget(target);
      turret.ResetPrefireTimer();
    }), turret);
  }
}
