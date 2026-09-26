// Decompiled with JetBrains decompiler
// Type: Vehicles.Ext_Thing
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

[PublicAPI]
public static class Ext_Thing
{
  public static bool ShouldAlwaysTransferToVehiclesCargo(this Pawn pawn)
  {
    return pawn.IsAnimal || pawn.IsColonyMech;
  }

  public static bool CanBeTransferredToVehiclesCargo(this Pawn pawn)
  {
    return pawn.Downed || pawn.ShouldAlwaysTransferToVehiclesCargo();
  }

  public static bool CanBeHauledToVehicle(this Thing thing)
  {
    if (!thing.Spawned)
      return false;
    if (thing is Pawn pawn && (((Thing) pawn).Faction == Faction.OfPlayer && (pawn.Downed || pawn.IsAnimal || pawn.IsColonyMech) || pawn.IsPrisonerOfColony))
      return true;
    return (!thing.Map.IsPlayerHome || ((Area) thing.Map.areaManager.Home)[thing.Position]) && thing.def.EverHaulable;
  }

  public static IEnumerable<VehiclePawn> GetVehiclesToBeTransferredTo(this Thing thing)
  {
    Map mapHeld = thing.MapHeld;
    if (mapHeld != null)
    {
      foreach (VehiclePawn vehicleLister in mapHeld.GetCachedMapComponent<VehicleReservationManager>().VehicleListers("LoadVehicle"))
      {
        List<TransferableOneWay> cargoToLoad = vehicleLister.cargoToLoad;
        if ((cargoToLoad != null ? cargoToLoad.FindTransferableFor(thing) : (TransferableOneWay) null) != null)
          yield return vehicleLister;
      }
    }
  }

  public static bool IsOrderedToBeTransferredToAnyVehicle(this Thing thing)
  {
    return thing.GetVehiclesToBeTransferredTo().Any<VehiclePawn>();
  }

  public static void TransferToVehicle(this IEnumerable<Thing> things, VehiclePawn vehicle)
  {
    foreach (Thing thing in things)
    {
      thing.CancelTransferToAnyOtherVehicle(vehicle);
      thing.TransferToVehicle(vehicle);
    }
    Map mapHeld = ((Thing) vehicle).MapHeld;
    if (mapHeld == null)
      return;
    mapHeld.GetCachedMapComponent<VehicleReservationManager>().RegisterLister(vehicle, "LoadVehicle");
  }

  public static void TransferToVehicle(this Thing thing, VehiclePawn vehicle)
  {
    VehiclePawn vehiclePawn = vehicle;
    (vehiclePawn.cargoToLoad ?? (vehiclePawn.cargoToLoad = new List<TransferableOneWay>())).AddThing(thing);
  }

  public static void CancelTransferToVehicle(this Thing thing, VehiclePawn vehicle)
  {
    List<TransferableOneWay> cargoToLoad = vehicle.cargoToLoad;
    if ((cargoToLoad != null ? (cargoToLoad.RemoveThing(thing) ? 1 : 0) : 0) == 0)
      return;
    thing.CancelRelatedJob(JobDefOf_Vehicles.LoadVehicle, (JobCondition) 32 /*0x20*/);
  }

  public static void CancelTransferToAnyOtherVehicle(this Thing thing, VehiclePawn vehicle)
  {
    foreach (VehiclePawn vehicle1 in thing.GetVehiclesToBeTransferredTo())
    {
      if (vehicle1 != vehicle)
        thing.CancelTransferToVehicle(vehicle1);
    }
  }

  public static void CancelTransferToAnyVehicle(this Thing thing)
  {
    foreach (VehiclePawn vehicle in thing.GetVehiclesToBeTransferredTo())
      thing.CancelTransferToVehicle(vehicle);
  }

  public static void CancelRelatedJob(
    this Thing thing,
    JobDef jobType,
    JobCondition cancelCondition)
  {
    Map mapHeld = thing.MapHeld;
    ReservationManager.Reservation reservation = mapHeld != null ? GenCollection.FirstOrFallback<ReservationManager.Reservation>((IEnumerable<ReservationManager.Reservation>) mapHeld.reservationManager.ReservationsReadOnly, (Func<ReservationManager.Reservation, bool>) (res =>
    {
      Thing thing1;
      if (res == null)
      {
        thing1 = (Thing) null;
      }
      else
      {
        LocalTargetInfo target = res.Target;
        thing1 = ((LocalTargetInfo) ref target).Thing;
      }
      Thing thing2 = thing;
      return thing1 == thing2;
    }), (ReservationManager.Reservation) null) : (ReservationManager.Reservation) null;
    if (reservation == null || reservation.Job.def != jobType)
      return;
    reservation.Job.GetCachedDriver(reservation.Claimant).EndJobWith(cancelCondition);
  }
}
