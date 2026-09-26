// Decompiled with JetBrains decompiler
// Type: Vehicles.ListerVehiclesRepairable
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class ListerVehiclesRepairable(Map map) : MapComponent(map)
{
  private readonly Dictionary<Faction, HashSet<VehiclePawn>> vehiclesToRepair = new Dictionary<Faction, HashSet<VehiclePawn>>();

  private static HashSet<VehiclePawn> Empty { get; } = new HashSet<VehiclePawn>();

  public HashSet<VehiclePawn> RepairsForFaction(Faction faction)
  {
    if (faction == null)
      return ListerVehiclesRepairable.Empty;
    HashSet<VehiclePawn> vehiclePawnSet;
    if (!this.vehiclesToRepair.TryGetValue(faction, out vehiclePawnSet))
      this.vehiclesToRepair[faction] = vehiclePawnSet = new HashSet<VehiclePawn>();
    return vehiclePawnSet;
  }

  public void NotifyVehicleSpawned(VehiclePawn vehicle)
  {
    this.NotifyVehicleRepaired(vehicle);
    this.NotifyVehicleTookDamage(vehicle);
  }

  public void NotifyVehicleDespawned(VehiclePawn vehicle)
  {
    HashSet<VehiclePawn> vehiclePawnSet;
    if (((Thing) vehicle).Faction == null || !this.vehiclesToRepair.TryGetValue(((Thing) vehicle).Faction, out vehiclePawnSet))
      return;
    vehiclePawnSet.Remove(vehicle);
  }

  public void NotifyVehicleTookDamage(VehiclePawn vehicle)
  {
    if (((Thing) vehicle).Faction == null || !vehicle.statHandler.NeedsRepairs || Mathf.Approximately(vehicle.GetStatValue(VehicleStatDefOf.BodyIntegrity), 0.0f))
      return;
    HashSet<VehiclePawn> vehiclePawnSet;
    if (!this.vehiclesToRepair.TryGetValue(((Thing) vehicle).Faction, out vehiclePawnSet))
      this.vehiclesToRepair[((Thing) vehicle).Faction] = vehiclePawnSet = new HashSet<VehiclePawn>();
    if (((Thing) vehicle).Spawned)
      vehiclePawnSet.Add(vehicle);
    else
      vehiclePawnSet.Remove(vehicle);
  }

  public void NotifyVehicleRepaired(VehiclePawn vehicle)
  {
    HashSet<VehiclePawn> vehiclePawnSet;
    if (((Thing) vehicle).Faction == null || vehicle.statHandler.NeedsRepairs || !this.vehiclesToRepair.TryGetValue(((Thing) vehicle).Faction, out vehiclePawnSet))
      return;
    vehiclePawnSet.Remove(vehicle);
  }
}
