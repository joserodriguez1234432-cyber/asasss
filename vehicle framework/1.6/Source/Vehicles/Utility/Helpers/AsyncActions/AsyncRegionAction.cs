// Decompiled with JetBrains decompiler
// Type: Vehicles.AsyncRegionAction
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools.Performance;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public class AsyncRegionAction : AsyncAction
{
  private VehiclePathingSystem mapping;
  private List<VehicleDef> vehicleDefs;
  private CellRect cellRect;
  private bool spawned;

  public override bool IsValid
  {
    get
    {
      VehiclePathingSystem mapping = this.mapping;
      if (mapping == null)
        return false;
      int? index = mapping.map?.Index;
      int num = -1;
      return index.GetValueOrDefault() > num & index.HasValue;
    }
  }

  public void Set(
    VehiclePathingSystem mapping,
    List<VehicleDef> vehicleDefs,
    CellRect cellRect,
    bool spawned)
  {
    this.mapping = mapping;
    this.vehicleDefs = vehicleDefs;
    this.cellRect = cellRect;
    this.spawned = spawned;
  }

  public override void Invoke()
  {
    if (this.spawned)
      PathingHelper.ThingInRegionSpawned(this.cellRect, this.mapping, this.vehicleDefs);
    else
      PathingHelper.ThingInRegionDespawned(this.cellRect, this.mapping, this.vehicleDefs);
  }

  public override void ReturnToPool()
  {
    this.mapping = (VehiclePathingSystem) null;
    this.vehicleDefs = (List<VehicleDef>) null;
    AsyncPool<AsyncRegionAction>.Return(this);
  }
}
