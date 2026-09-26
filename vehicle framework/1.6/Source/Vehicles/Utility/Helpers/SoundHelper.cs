// Decompiled with JetBrains decompiler
// Type: Vehicles.SoundHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

public static class SoundHelper
{
  public static void PlayImpactSound(
    this VehiclePawn vehicle,
    VehicleComponent.DamageResult damageResult)
  {
    if (!((Thing) vehicle).Spawned)
      return;
    SoundDef soundDef1;
    switch (damageResult.penetration)
    {
      case VehicleComponent.Penetration.NonPenetrated:
        soundDef1 = vehicle.VehicleDef.BodyType.soundImpactNonPenetrated;
        break;
      case VehicleComponent.Penetration.Deflected:
        soundDef1 = vehicle.VehicleDef.BodyType.soundImpactDeflect;
        break;
      case VehicleComponent.Penetration.Diminished:
        soundDef1 = vehicle.VehicleDef.BodyType.soundImpactDiminished;
        break;
      case VehicleComponent.Penetration.Penetrated:
        soundDef1 = vehicle.VehicleDef.BodyType.soundImpactPenetrated;
        break;
      default:
        throw new NotImplementedException("Unhandled Penetration result.");
    }
    SoundDef soundDef2 = soundDef1;
    if (SoundDefHelper.NullOrUndefined(soundDef2))
      soundDef2 = SoundDefOf.BulletImpact_Ground;
    SoundStarter.PlayOneShot(soundDef2, SoundInfo.op_Implicit(new TargetInfo(((Thing) vehicle).PositionHeld, ((Thing) vehicle).Map, false)));
  }
}
