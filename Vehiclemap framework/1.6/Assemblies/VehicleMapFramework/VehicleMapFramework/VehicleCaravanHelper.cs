// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleCaravanHelper
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vehicles;
using Vehicles.World;

#nullable disable
namespace VehicleMapFramework;

public static class VehicleCaravanHelper
{
  public static IEnumerable<VehiclePawn> get_Vehicles(WorldObject vehicleCaravanOrStashedVehicle)
  {
    IEnumerable<VehiclePawn> vehicles;
    switch (vehicleCaravanOrStashedVehicle)
    {
      case VehicleCaravan vehicleCaravan:
        vehicles = vehicleCaravan.Vehicles;
        break;
      case StashedVehicle stashedVehicle:
        vehicles = stashedVehicle.Vehicles;
        break;
      default:
        vehicles = (IEnumerable<VehiclePawn>) Array.Empty<VehiclePawn>();
        break;
    }
    return vehicles;
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u00245F87F2C460C524EC32F8840F2A088372
  {
    [ExtensionMarker("<M>$D0F7D954008A1B9B365F2CE0D7CAA37A")]
    public IEnumerable<VehiclePawn> Vehicles
    {
      [ExtensionMarker("<M>$D0F7D954008A1B9B365F2CE0D7CAA37A")] get
      {
        throw new NotSupportedException();
      }
    }

    [SpecialName]
    public static class \u003CM\u003E\u0024D0F7D954008A1B9B365F2CE0D7CAA37A
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(WorldObject vehicleCaravanOrStashedVehicle)
      {
      }
    }
  }
}
