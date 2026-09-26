// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleTurretEventDefOf
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;

#nullable disable
namespace Vehicles;

[DefOf]
public static class VehicleTurretEventDefOf
{
  public static VehicleTurretEventDef Queued;
  public static VehicleTurretEventDef Dequeued;
  public static VehicleTurretEventDef ShotFired;
  public static VehicleTurretEventDef Reload;
  public static VehicleTurretEventDef Warmup;
  public static VehicleTurretEventDef Cooldown;

  static VehicleTurretEventDefOf()
  {
    DefOfHelper.EnsureInitializedInCtor(typeof (VehicleTurretEventDefOf));
  }
}
