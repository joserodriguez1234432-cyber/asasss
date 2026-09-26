// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleIncidentSwapper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace Vehicles;

public static class VehicleIncidentSwapper
{
  public static HashSet<System.Type> vehicleLordCategories = new HashSet<System.Type>();

  public static void RegisterLordType(System.Type type)
  {
    VehicleIncidentSwapper.vehicleLordCategories.Add(type);
  }

  public static void RegisterIncident() => throw new NotImplementedException();
}
