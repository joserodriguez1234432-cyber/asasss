// Decompiled with JetBrains decompiler
// Type: Vehicles.WorkGiver_PackVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public class WorkGiver_PackVehicle : WorkGiver_CarryToVehicle<TransferableOneWay>
{
  protected override bool JobAvailable(Pawn pawn, VehiclePawn vehicle)
  {
    return vehicle.cargoToLoad.Count != 0 && !MassUtility.IsOverEncumbered((Pawn) vehicle);
  }

  protected override List<TransferableOneWay> GetThingsToLoad(VehiclePawn vehicle, Pawn pawn)
  {
    return vehicle.cargoToLoad;
  }

  protected override Thing FindThingToPack(
    VehiclePawn vehicle,
    Pawn pawn,
    List<TransferableOneWay> things)
  {
    return JobDriver_LoadVehicle.FindThingToPack(pawn, this.JobDef, things);
  }
}
