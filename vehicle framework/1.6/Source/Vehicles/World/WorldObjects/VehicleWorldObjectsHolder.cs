// Decompiled with JetBrains decompiler
// Type: Vehicles.World.VehicleWorldObjectsHolder
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace Vehicles.World;

[PublicAPI]
public class VehicleWorldObjectsHolder : WorldComponent
{
  private static List<AerialVehicleInFlight> aerialVehicles = new List<AerialVehicleInFlight>();
  private static List<VehicleCaravan> vehicleCaravans = new List<VehicleCaravan>();
  private static List<StashedVehicle> stashedVehicles = new List<StashedVehicle>();

  public VehicleWorldObjectsHolder(RimWorld.Planet.World world)
    : base(world)
  {
    VehicleWorldObjectsHolder.aerialVehicles.RemoveAll((Predicate<AerialVehicleInFlight>) (a => a == null));
    VehicleWorldObjectsHolder.vehicleCaravans.RemoveAll((Predicate<VehicleCaravan>) (c => c == null));
    VehicleWorldObjectsHolder.stashedVehicles.RemoveAll((Predicate<StashedVehicle>) (b => b == null));
  }

  [Obsolete("Just fetch from WorldComponents or cache. Singleton statics will be removed.")]
  public static VehicleWorldObjectsHolder Instance
  {
    get => Find.World.GetComponent<VehicleWorldObjectsHolder>();
  }

  public List<AerialVehicleInFlight> AerialVehicles => VehicleWorldObjectsHolder.aerialVehicles;

  public List<VehicleCaravan> VehicleCaravans => VehicleWorldObjectsHolder.vehicleCaravans;

  public List<StashedVehicle> StashedVehicles => VehicleWorldObjectsHolder.stashedVehicles;

  public AerialVehicleInFlight AerialVehicleObject(VehiclePawn vehicle)
  {
    foreach (AerialVehicleInFlight aerialVehicle in this.AerialVehicles)
    {
      if (aerialVehicle.Vehicle == vehicle)
        return aerialVehicle;
    }
    return (AerialVehicleInFlight) null;
  }

  public VehicleCaravan VehicleCaravanObject(VehiclePawn vehicle)
  {
    foreach (VehicleCaravan vehicleCaravan in this.VehicleCaravans)
    {
      if (vehicleCaravan.VehiclesListForReading.Contains(vehicle))
        return vehicleCaravan;
    }
    return (VehicleCaravan) null;
  }

  public StashedVehicle StashedVehicleObject(VehiclePawn vehicle)
  {
    foreach (StashedVehicle stashedVehicle in this.StashedVehicles)
    {
      if (stashedVehicle.Vehicles.Contains<VehiclePawn>(vehicle))
        return stashedVehicle;
    }
    return (StashedVehicle) null;
  }

  public void Recache()
  {
    VehicleWorldObjectsHolder.aerialVehicles.Clear();
    VehicleWorldObjectsHolder.vehicleCaravans.Clear();
    VehicleWorldObjectsHolder.stashedVehicles.Clear();
  }

  public void AddToCache(WorldObject obj)
  {
    switch (obj)
    {
      case AerialVehicleInFlight aerialVehicleInFlight:
        VehicleWorldObjectsHolder.aerialVehicles.Add(aerialVehicleInFlight);
        break;
      case VehicleCaravan vehicleCaravan:
        VehicleWorldObjectsHolder.vehicleCaravans.Add(vehicleCaravan);
        break;
      case StashedVehicle stashedVehicle:
        VehicleWorldObjectsHolder.stashedVehicles.Add(stashedVehicle);
        break;
    }
  }

  public void RemoveFromCache(WorldObject obj)
  {
    switch (obj)
    {
      case AerialVehicleInFlight aerialVehicleInFlight:
        VehicleWorldObjectsHolder.aerialVehicles.Remove(aerialVehicleInFlight);
        break;
      case VehicleCaravan vehicleCaravan:
        VehicleWorldObjectsHolder.vehicleCaravans.Remove(vehicleCaravan);
        break;
      case StashedVehicle stashedVehicle:
        VehicleWorldObjectsHolder.stashedVehicles.Remove(stashedVehicle);
        break;
    }
    AirDefensePositionTracker.airDefenseCache.Remove(obj);
  }

  public virtual void ExposeData()
  {
    base.ExposeData();
    Scribe_Collections.Look<AerialVehicleInFlight>(ref VehicleWorldObjectsHolder.aerialVehicles, "aerialVehicles", (LookMode) 3, Array.Empty<object>());
    Scribe_Collections.Look<VehicleCaravan>(ref VehicleWorldObjectsHolder.vehicleCaravans, "vehicleCaravans", (LookMode) 3, Array.Empty<object>());
    Scribe_Collections.Look<StashedVehicle>(ref VehicleWorldObjectsHolder.stashedVehicles, "stashedVehicles", (LookMode) 3, Array.Empty<object>());
    if (Scribe.mode != 4)
      return;
    VehicleWorldObjectsHolder.aerialVehicles.RemoveAll((Predicate<AerialVehicleInFlight>) (a => a == null));
    VehicleWorldObjectsHolder.vehicleCaravans.RemoveAll((Predicate<VehicleCaravan>) (c => c == null));
    VehicleWorldObjectsHolder.stashedVehicles.RemoveAll((Predicate<StashedVehicle>) (b => b == null));
  }
}
