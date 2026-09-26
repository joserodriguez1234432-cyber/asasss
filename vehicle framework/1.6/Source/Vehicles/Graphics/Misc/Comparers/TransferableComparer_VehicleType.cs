// Decompiled with JetBrains decompiler
// Type: Vehicles.TransferableComparer_VehicleType
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;

#nullable disable
namespace Vehicles;

public class TransferableComparer_VehicleType : TransferableComparerBase
{
  protected override int Compare(VehicleDef vehicleDef, VehicleDef otherVehicleDef)
  {
    return CompareTypePriority(vehicleDef.type).CompareTo(CompareTypePriority(otherVehicleDef.type));

    static int CompareTypePriority(VehicleType type)
    {
      switch (type)
      {
        case VehicleType.Sea:
          return 2;
        case VehicleType.Air:
          return 3;
        case VehicleType.Land:
          return 1;
        case VehicleType.Universal:
          return 0;
        default:
          throw new NotImplementedException("VehicleType");
      }
    }
  }
}
