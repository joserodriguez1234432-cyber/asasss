// Decompiled with JetBrains decompiler
// Type: Vehicles.RoleHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public static class RoleHelper
{
  public static void Distribute(in List<VehiclePawn> vehicles, List<Pawn> pawns)
  {
    if (GenList.NullOrEmpty<VehiclePawn>((IList<VehiclePawn>) vehicles))
    {
      Trace.Fail("Trying to distribute to pawns with no vehicles listed.");
    }
    else
    {
      RoleHelper.Distributor distributor = new RoleHelper.Distributor(vehicles, pawns);
      distributor.DistributeOnPriority(HandlingType.Movement);
      distributor.DistributeOnPriority(HandlingType.Turret);
      distributor.DistributeToAnyRole();
    }
  }

  public static void DistributeAll(in List<VehiclePawn> vehicles, List<Pawn> pawns)
  {
    if (GenList.NullOrEmpty<VehiclePawn>((IList<VehiclePawn>) vehicles))
    {
      Trace.Fail("Trying to distribute to pawns with no vehicles listed.");
    }
    else
    {
      RoleHelper.Distributor distributor = new RoleHelper.Distributor(vehicles, pawns);
      distributor.DistributeNonColonistsToCargo();
      distributor.DistributeOnPriority(HandlingType.Movement);
      distributor.DistributeOnPriority(HandlingType.Turret);
      distributor.DistributeToAnyRole();
      distributor.DistributeFallbackToCargo();
    }
  }

  private class Distributor
  {
    private readonly RotatingList<VehiclePawn> vehicles;
    private readonly List<Pawn> pawns;

    public Distributor(List<VehiclePawn> vehicles, List<Pawn> pawns)
    {
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      pawns.RemoveAll(RoleHelper.Distributor.\u003C\u003EO.\u003C0\u003E__InVehicle ?? (RoleHelper.Distributor.\u003C\u003EO.\u003C0\u003E__InVehicle = new Predicate<Pawn>(Ext_Vehicles.InVehicle)));
      this.pawns = pawns;
      RotatingList<VehiclePawn> rotatingList = new RotatingList<VehiclePawn>();
      foreach (VehiclePawn vehicle in vehicles)
        rotatingList.Add(vehicle);
      this.vehicles = rotatingList;
    }

    private VehiclePawn GetNextAvailableVehicle(Pawn pawn, Func<VehiclePawn, Pawn, bool> predicate)
    {
      int index = this.vehicles.Index;
      do
      {
        VehiclePawn next = this.vehicles.Next;
        if (predicate(next, pawn))
          return next;
      }
      while (this.vehicles.Index != index);
      return (VehiclePawn) null;
    }

    private bool CanAddToCargo(VehiclePawn vehicle, Pawn pawn)
    {
      return pawn.CanBeTransferredToVehiclesCargo() && !MassUtility.IsOverEncumbered((Pawn) vehicle) && (double) MassUtility.InventoryMass(pawn) + (double) MassUtility.GearAndInventoryMass(pawn) <= (double) vehicle.GetStatValue(VehicleStatDefOf.CargoCapacity);
    }

    public void DistributeNonColonistsToCargo()
    {
      for (int index = this.pawns.Count - 1; index >= 0; --index)
      {
        Pawn pawn = this.pawns[index];
        if (pawn.ShouldAlwaysTransferToVehiclesCargo())
        {
          VehiclePawn availableVehicle = this.GetNextAvailableVehicle(pawn, new Func<VehiclePawn, Pawn, bool>(this.CanAddToCargo));
          if (availableVehicle != null)
          {
            availableVehicle.AddOrTransfer((Thing) pawn);
            this.pawns.RemoveAt(index);
          }
        }
      }
    }

    public void DistributeFallbackToCargo()
    {
      for (int index = this.pawns.Count - 1; index >= 0; --index)
      {
        Pawn pawn = this.pawns[index];
        if (pawn.CanBeTransferredToVehiclesCargo())
        {
          VehiclePawn availableVehicle = this.GetNextAvailableVehicle(pawn, new Func<VehiclePawn, Pawn, bool>(this.CanAddToCargo));
          if (availableVehicle != null)
          {
            availableVehicle.AddOrTransfer((Thing) pawn);
            this.pawns.RemoveAt(index);
          }
        }
      }
    }

    public void DistributeOnPriority(HandlingType handlingType)
    {
      for (int index = this.pawns.Count - 1; index >= 0; --index)
      {
        Pawn pawn = this.pawns[index];
        if (!pawn.ShouldAlwaysTransferToVehiclesCargo())
        {
          VehicleRoleHandler availableHandler = GetAvailableHandler(pawn, (List<VehiclePawn>) this.vehicles, handlingType);
          if (availableHandler != null && availableHandler.vehicle.TryAddPawn(pawn, availableHandler))
            this.pawns.RemoveAt(index);
        }
      }

      static VehicleRoleHandler GetAvailableHandler(
        Pawn pawn,
        List<VehiclePawn> vehicles,
        HandlingType handlingType)
      {
        foreach (VehiclePawn vehicle in vehicles)
        {
          VehicleRoleHandler availableHandler = vehicle.GetNextAvailableHandler(pawn, handlingType);
          if (availableHandler != null)
            return availableHandler;
        }
        return (VehicleRoleHandler) null;
      }
    }

    public void DistributeToAnyRole()
    {
      for (int index = this.pawns.Count - 1; index >= 0; --index)
      {
        Pawn pawn = this.pawns[index];
        VehiclePawn availableVehicle = this.GetNextAvailableVehicle(pawn, new Func<VehiclePawn, Pawn, bool>(CanAddToVehicle));
        if (availableVehicle != null && !availableVehicle.TryAddPawn(pawn))
          Log.Error($"Unable to add {pawn} to vehicle {availableVehicle}.");
      }

      static bool CanAddToVehicle(VehiclePawn vehicle, Pawn pawn)
      {
        foreach (VehicleRoleHandler handler in vehicle.handlers)
        {
          if (((handler.role.handlingTypes & HandlingType.Movement) == HandlingType.None || handler.CanOperateRole(pawn)) && handler.AreSlotsAvailable)
            return true;
        }
        return false;
      }
    }
  }
}
