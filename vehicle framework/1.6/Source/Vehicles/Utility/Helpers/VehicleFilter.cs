// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleFilter
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public static class VehicleFilter
{
  public static IEnumerable<Pawn> FilterOutPassengers(IEnumerable<Pawn> pawns)
  {
    List<Pawn> pawnsInVehicles = new List<Pawn>();
    HashSet<VehiclePawn> vehicles = new HashSet<VehiclePawn>();
    foreach (Pawn pawn in pawns)
    {
      if (pawn.InVehicle())
      {
        pawnsInVehicles.Add(pawn);
      }
      else
      {
        if (pawn is VehiclePawn vehiclePawn)
          vehicles.Add(vehiclePawn);
        yield return pawn;
      }
    }
    foreach (Pawn pawn in pawnsInVehicles)
    {
      if (!vehicles.Contains(pawn.GetVehicle()))
        yield return pawn;
    }
  }
}
