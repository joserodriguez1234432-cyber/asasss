// Decompiled with JetBrains decompiler
// Type: Vehicles.ModSettingsHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public static class ModSettingsHelper
{
  public static FloatRange BeachMultiplier(FloatRange coastOffset)
  {
    float beachMultiplier = VehicleMod.settings.main.beachMultiplier;
    if (Mathf.Approximately(beachMultiplier, 0.0f))
      return coastOffset;
    float num = beachMultiplier + 1f;
    return new FloatRange(coastOffset.min * num, coastOffset.max * num);
  }

  public static float RiverMultiplier
  {
    get
    {
      return (float) (1.0 + (Mathf.Approximately(VehicleMod.settings.main.riverMultiplier, 0.0f) ? 0.0 : (double) VehicleMod.settings.main.riverMultiplier));
    }
  }

  public static float RiverSizeWithMultiplier(RiverDef riverDef)
  {
    return riverDef.widthOnMap * ModSettingsHelper.RiverMultiplier;
  }
}
