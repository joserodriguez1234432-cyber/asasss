// Decompiled with JetBrains decompiler
// Type: Vehicles.TransferableComparerBase
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System.Diagnostics;

#nullable disable
namespace Vehicles;

public abstract class TransferableComparerBase : TransferableComparer
{
  public virtual int Compare(Transferable lhs, Transferable rhs)
  {
    if (!(lhs?.ThingDef is VehicleDef thingDef1))
    {
      Trace.Fail($"Using vehicle comparer with non vehicle entity {lhs?.ThingDef}");
      return 0;
    }
    if (!(rhs?.ThingDef is VehicleDef thingDef2))
    {
      Trace.Fail($"Using vehicle comparer with non vehicle entity {rhs?.ThingDef}");
      return 0;
    }
    return lhs.AnyThing is VehiclePawn anyThing1 && rhs.AnyThing is VehiclePawn anyThing2 ? this.Compare(anyThing1, anyThing2) : this.Compare(thingDef1, thingDef2);
  }

  protected virtual int Compare(VehiclePawn vehicle, VehiclePawn otherVehicle)
  {
    return this.Compare(vehicle.VehicleDef, otherVehicle.VehicleDef);
  }

  protected abstract int Compare(VehicleDef vehicleDef, VehicleDef otherVehicleDef);
}
