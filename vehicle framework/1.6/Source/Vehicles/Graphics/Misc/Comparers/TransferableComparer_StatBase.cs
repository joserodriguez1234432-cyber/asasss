// Decompiled with JetBrains decompiler
// Type: Vehicles.TransferableComparer_StatBase
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using UnityEngine;

#nullable disable
namespace Vehicles;

public abstract class TransferableComparer_StatBase : TransferableComparerBase
{
  protected abstract VehicleStatDef StatDef { get; }

  protected virtual TransferableComparer_StatBase.CompareType Type
  {
    get => TransferableComparer_StatBase.CompareType.Higher;
  }

  protected override int Compare(VehiclePawn vehicle, VehiclePawn otherVehicle)
  {
    return this.CompareStatValues(vehicle.GetStatValue(this.StatDef), otherVehicle.GetStatValue(this.StatDef));
  }

  protected override int Compare(VehicleDef vehicleDef, VehicleDef otherVehicleDef)
  {
    return this.CompareStatValues(vehicleDef.GetStatValueAbstract(this.StatDef), otherVehicleDef.GetStatValueAbstract(this.StatDef));
  }

  private int CompareStatValues(float value, float otherValue)
  {
    if (Mathf.Approximately(value, otherValue))
      return 0;
    return this.Type != TransferableComparer_StatBase.CompareType.Lower ? ((double) value <= (double) otherValue ? 1 : -1) : ((double) value >= (double) otherValue ? 1 : -1);
  }

  protected enum CompareType
  {
    Lower,
    Higher,
  }
}
