// Decompiled with JetBrains decompiler
// Type: Vehicles.AnimationParameterDefOf
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools.Animations;

#nullable disable
namespace Vehicles;

[DefOf]
public static class AnimationParameterDefOf
{
  public static AnimationParameterDef VF_VehicleIsMoving;
  public static AnimationParameterDef VF_VehicleIsDisabled;
  public static AnimationParameterDef VF_VehicleIsIgnitionOn;
  public static AnimationParameterDef VF_VehicleIsTakingOff;
  public static AnimationParameterDef VF_VehicleIsLanding;
  public static AnimationParameterDef VF_VehicleIsLoitering;

  static AnimationParameterDefOf()
  {
    DefOfHelper.EnsureInitializedInCtor(typeof (AnimationParameterDefOf));
  }
}
