// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleEventDefOf
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;

#nullable disable
namespace Vehicles;

[DefOf]
public static class VehicleEventDefOf
{
  public static VehicleEventDef IgnitionOn;
  public static VehicleEventDef IgnitionOff;
  public static VehicleEventDef Braking;
  public static VehicleEventDef MoveStart;
  public static VehicleEventDef MoveStop;
  public static VehicleEventDef CargoAdded;
  public static VehicleEventDef CargoRemoved;
  public static VehicleEventDef PawnEntered;
  public static VehicleEventDef PawnExited;
  public static VehicleEventDef PawnChangedSeats;
  public static VehicleEventDef PawnCapacitiesDirty;
  public static VehicleEventDef PawnKilled;
  public static VehicleEventDef PawnRemoved;
  public static VehicleEventDef ScanShort;
  public static VehicleEventDef ScanRare;
  public static VehicleEventDef OutOfFuel;
  public static VehicleEventDef Refueled;
  public static VehicleEventDef Deployed;
  public static VehicleEventDef Undeployed;
  public static VehicleEventDef HealthChanged;
  public static VehicleEventDef DamageTaken;
  public static VehicleEventDef Repaired;
  public static VehicleEventDef Spawned;
  public static VehicleEventDef Despawned;
  public static VehicleEventDef Destroyed;
  public static VehicleEventDef AerialVehicleLaunch;
  public static VehicleEventDef AerialVehicleLanding;
  public static VehicleEventDef AerialVehicleCrashLanding;
  public static VehicleEventDef AerialVehicleLeftMap;
  public static VehicleEventDef AerialVehicleOrdered;
  public static VehicleEventDef UpgradeEnqueued;
  public static VehicleEventDef UpgradeCompleted;
  public static VehicleEventDef UpgradeCanceled;
  public static VehicleEventDef UpgradeRefundEnqueued;
  public static VehicleEventDef UpgradeRefundCompleted;
  public static VehicleEventDef ColorChanged;

  static VehicleEventDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof (VehicleEventDefOf));
}
