// Decompiled with JetBrains decompiler
// Type: Vehicles.WorldObjectDefOfVehicles
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;

#nullable disable
namespace Vehicles;

[DefOf]
public static class WorldObjectDefOfVehicles
{
  public static WorldObjectDef DebugSettlement;
  public static WorldObjectDef StashedVehicle;
  public static WorldObjectDef VehicleCaravan;
  public static WorldObjectDef AerialVehicle;
  public static WorldObjectDef CrashedShipSite;

  static WorldObjectDefOfVehicles()
  {
    DefOfHelper.EnsureInitializedInCtor(typeof (WorldObjectDefOfVehicles));
  }
}
