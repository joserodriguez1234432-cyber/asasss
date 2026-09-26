// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleMapFollower
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class VehicleMapFollower(VehiclePawnWithMap vehicle)
{
  public readonly VehiclePawnWithMap vehicle = vehicle;
  private HashSet<IntVec3> prevOccupiedCells = new HashSet<IntVec3>();
  private HashSet<IntVec3> tmpOccupiedCells = new HashSet<IntVec3>();
  private IntVec3 prevCell = IntVec3.Invalid;
  private Rot8 prevRot = Rot8.Invalid;
  private float ticksToMove;
  private bool updated;

  public void MapFollowerTick()
  {
    if (!((Thing) this.vehicle).Spawned)
      return;
    if (IntVec3.op_Inequality(((Thing) this.vehicle).Position, this.prevCell))
    {
      if ((double) this.ticksToMove > 0.0)
        this.UpdatePositionAndRotation();
      this.ticksToMove = VehiclePathFollower.MoveTicksAt((VehiclePawn) this.vehicle, this.prevCell, ((Thing) this.vehicle).Position);
      this.prevCell = ((Thing) this.vehicle).Position;
      this.updated = false;
      this.vehicle.MapVehicleEventManager[VMF_DefOf.EnterNextCell].ExecuteEvents();
    }
    else
      --this.ticksToMove;
    if (!this.updated && (double) this.ticksToMove <= 0.0)
    {
      this.UpdatePositionAndRotation();
      this.updated = true;
    }
    if (!Rot8.op_Inequality(this.vehicle.FullRotation, this.prevRot))
      return;
    this.vehicle.enterPositionsDirty = true;
    CrossMapReachabilityCache.ClearCacheFor(this.vehicle.VehicleMap);
    this.UpdatePositionAndRotation();
    this.prevRot = this.vehicle.FullRotation;
  }

  public void RegisterVehicle()
  {
    this.CalculateMapCells();
    VehicleMapGrid component = MapComponentCache<VehicleMapGrid>.GetComponent(((Thing) this.vehicle).Map);
    foreach (IntVec3 tmpOccupiedCell in this.tmpOccupiedCells)
      component.Register(tmpOccupiedCell, this.vehicle);
    component.OccupiedCells[this.vehicle] = this.tmpOccupiedCells;
    this.prevCell = ((Thing) this.vehicle).Position;
    this.prevRot = this.vehicle.FullRotation;
  }

  public void DeRegisterVehicle()
  {
    VehicleMapGrid component = MapComponentCache<VehicleMapGrid>.GetComponent(((Thing) this.vehicle).Map);
    foreach (IntVec3 prevOccupiedCell in this.prevOccupiedCells)
    {
      IntVec3 c = prevOccupiedCell;
      VehiclePawnWithMap key = component.OccupiedCells.FirstOrDefault<KeyValuePair<VehiclePawnWithMap, HashSet<IntVec3>>>((Func<KeyValuePair<VehiclePawnWithMap, HashSet<IntVec3>>, bool>) (pair => pair.Value.Contains(c) && pair.Key != this.vehicle)).Key;
      if (key != null)
        component.Register(c, key);
      else
        component.DeRegister(c);
    }
    component.OccupiedCells.Remove(this.vehicle);
  }

  private void UpdatePositionAndRotation()
  {
    this.CalculateMapCells();
    VehicleMapGrid component = MapComponentCache<VehicleMapGrid>.GetComponent(((Thing) this.vehicle).Map);
    foreach (IntVec3 tmpOccupiedCell in this.tmpOccupiedCells)
    {
      if (!this.prevOccupiedCells.Contains(tmpOccupiedCell))
        component.Register(tmpOccupiedCell, this.vehicle);
    }
    foreach (IntVec3 prevOccupiedCell in this.prevOccupiedCells)
    {
      IntVec3 c = prevOccupiedCell;
      if (!this.tmpOccupiedCells.Contains(c))
      {
        VehiclePawnWithMap key = component.OccupiedCells.FirstOrDefault<KeyValuePair<VehiclePawnWithMap, HashSet<IntVec3>>>((Func<KeyValuePair<VehiclePawnWithMap, HashSet<IntVec3>>, bool>) (pair => pair.Value.Contains(c) && pair.Key != this.vehicle)).Key;
        if (key != null)
          component.Register(c, key);
        else
          component.DeRegister(c);
      }
    }
    HashSet<IntVec3> tmpOccupiedCells = this.tmpOccupiedCells;
    HashSet<IntVec3> prevOccupiedCells = this.prevOccupiedCells;
    this.prevOccupiedCells = tmpOccupiedCells;
    this.tmpOccupiedCells = prevOccupiedCells;
    component.OccupiedCells[this.vehicle] = this.prevOccupiedCells;
  }

  private void CalculateMapCells()
  {
    this.tmpOccupiedCells.Clear();
    IntVec3 mapSize = this.vehicle.MapSize;
    IntVec3 baseMapCoord1 = new IntVec3(0, 0, 0).ToBaseMapCoord(this.vehicle);
    IntVec3 baseMapCoord2 = new IntVec3(mapSize.x - 1, 0, 0).ToBaseMapCoord(this.vehicle);
    IntVec3 baseMapCoord3 = new IntVec3(0, 0, mapSize.z - 1).ToBaseMapCoord(this.vehicle);
    IntVec3 baseMapCoord4 = new IntVec3(mapSize.x - 1, 0, mapSize.z - 1).ToBaseMapCoord(this.vehicle);
    CellRect cellRect = CellRect.FromLimits(Mathf.Min(new int[4]
    {
      baseMapCoord1.x,
      baseMapCoord2.x,
      baseMapCoord3.x,
      baseMapCoord4.x
    }), Mathf.Min(new int[4]
    {
      baseMapCoord1.z,
      baseMapCoord2.z,
      baseMapCoord3.z,
      baseMapCoord4.z
    }), Mathf.Max(new int[4]
    {
      baseMapCoord1.x,
      baseMapCoord2.x,
      baseMapCoord3.x,
      baseMapCoord4.x
    }), Mathf.Max(new int[4]
    {
      baseMapCoord1.z,
      baseMapCoord2.z,
      baseMapCoord3.z,
      baseMapCoord4.z
    }));
    Map map = ((Thing) this.vehicle).Map;
    foreach (IntVec3 intVec3_1 in cellRect)
    {
      if (((IntVec3) ref intVec3_1).ToVector3Shifted().TryGetVehicleMap(this.vehicle))
      {
        IntVec3[] adjacentCellsAndInside = GenAdj.AdjacentCellsAndInside;
        for (int index = 0; index < 5; ++index)
        {
          IntVec3 intVec3_2 = IntVec3.op_Addition(intVec3_1, adjacentCellsAndInside[index]);
          if (GenGrid.InBounds(intVec3_2, map))
            this.tmpOccupiedCells.Add(intVec3_2);
        }
      }
    }
  }
}
