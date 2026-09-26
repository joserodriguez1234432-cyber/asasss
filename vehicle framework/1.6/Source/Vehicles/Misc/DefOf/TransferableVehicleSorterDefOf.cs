// Decompiled with JetBrains decompiler
// Type: Vehicles.TransferableVehicleSorterDefOf
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;

#nullable disable
namespace Vehicles;

[DefOf]
[PublicAPI]
public static class TransferableVehicleSorterDefOf
{
  public static TransferableVehicleSorterDef Type;
  public static TransferableVehicleSorterDef MoveSpeed;
  public static TransferableVehicleSorterDef CargoCapacity;
  public static TransferableVehicleSorterDef Mass;

  static TransferableVehicleSorterDefOf()
  {
    DefOfHelper.EnsureInitializedInCtor(typeof (TransferableVehicleSorterDefOf));
  }
}
