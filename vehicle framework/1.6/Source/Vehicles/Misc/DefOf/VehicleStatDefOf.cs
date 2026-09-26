// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleStatDefOf
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;

#nullable disable
namespace Vehicles;

[DefOf]
public static class VehicleStatDefOf
{
  public static VehicleStatDef MoveSpeed;
  public static VehicleStatDef Mass;
  public static VehicleStatDef CargoCapacity;
  public static VehicleStatDef RepairRate;
  public static VehicleStatDef BodyIntegrity;
  public static VehicleStatDef WorkToSabotage;
  public static VehicleStatDef FlightSpeed;
  public static VehicleStatDef FlightControl;

  static VehicleStatDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof (VehicleStatDefOf));
}
