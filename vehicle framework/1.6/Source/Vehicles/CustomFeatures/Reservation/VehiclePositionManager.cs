// Decompiled with JetBrains decompiler
// Type: Vehicles.VehiclePositionManager
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class VehiclePositionManager(Map map) : DetachedMapComponent(map)
{
  private readonly ConcurrentDictionary<IntVec3, VehiclePawn> occupiedCells = new ConcurrentDictionary<IntVec3, VehiclePawn>();
  private readonly ConcurrentDictionary<VehiclePawn, CellRect> occupiedRects = new ConcurrentDictionary<VehiclePawn, CellRect>();
  private readonly List<VehiclePawn> claimants = new List<VehiclePawn>();

  public List<VehiclePawn> AllClaimants => this.claimants;

  public bool PositionClaimed(IntVec3 cell) => this.ClaimedBy(cell) != null;

  public VehiclePawn ClaimedBy(IntVec3 cell)
  {
    return GenCollection.TryGetValue<IntVec3, VehiclePawn>((IReadOnlyDictionary<IntVec3, VehiclePawn>) this.occupiedCells, cell, (VehiclePawn) null);
  }

  public CellRect ClaimedBy(VehiclePawn vehicle)
  {
    return GenCollection.TryGetValue<VehiclePawn, CellRect>((IReadOnlyDictionary<VehiclePawn, CellRect>) this.occupiedRects, vehicle, new CellRect());
  }

  public void ClaimPosition(VehiclePawn vehicle)
  {
    this.ClaimPosition(vehicle, ((Thing) vehicle).Position, ((Thing) vehicle).Rotation);
  }

  public void ClaimPosition(VehiclePawn vehicle, IntVec3 cell, Rot4 rot)
  {
    this.ReleaseClaimed(vehicle);
    CellRect cellRect = vehicle.VehicleRect(cell, rot);
    this.occupiedRects[vehicle] = cellRect;
    foreach (IntVec3 key in cellRect)
      this.occupiedCells[key] = vehicle;
    this.claimants.Add(vehicle);
    if (!((Thing) vehicle).Spawned)
      return;
    vehicle.RecalculateFollowerCell();
    this.ClaimedBy(vehicle.FollowerCell)?.RecalculateFollowerCell();
  }

  public void ReleaseClaimed(VehiclePawn vehicle)
  {
    CellRect cellRect;
    if (this.occupiedRects.TryGetValue(vehicle, out cellRect))
    {
      foreach (IntVec3 key in cellRect)
        this.occupiedCells.TryRemove(key, out VehiclePawn _);
    }
    this.occupiedRects.TryRemove(vehicle, out CellRect _);
    this.claimants.Remove(vehicle);
  }
}
