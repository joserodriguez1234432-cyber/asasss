// Decompiled with JetBrains decompiler
// Type: Vehicles.AerialVehicleArrivalModeDefOf
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;

#nullable disable
namespace Vehicles;

[DefOf]
public static class AerialVehicleArrivalModeDefOf
{
  public static AerialVehicleArrivalModeDef EdgeDrop;
  public static AerialVehicleArrivalModeDef CenterDrop;
  public static AerialVehicleArrivalModeDef TargetedLanding;

  static AerialVehicleArrivalModeDefOf()
  {
    DefOfHelper.EnsureInitializedInCtor(typeof (AerialVehicleArrivalModeDefOf));
  }
}
