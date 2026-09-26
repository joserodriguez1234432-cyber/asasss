// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompMapExpander
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompMapExpander : ThingComp
{
  private static readonly List<IntVec3> tmpCells = new List<IntVec3>(8);
  public static bool debugDraw;
  private bool? cachedIsBridge;
  private bool? cachedIsOnlyBridge;
  private bool validCellsDirty;
  [CompilerGenerated]
  private bool[] \u003CValidCells\u003Ek__BackingField = new bool[8];

  private bool[] ValidCells
  {
    get
    {
      if (this.validCellsDirty)
      {
        IntVec3[] adjacentCellsAround = GenAdj.AdjacentCellsAround;
        for (int index = 0; index < 8; ++index)
        {
          this.\u003CValidCells\u003Ek__BackingField[index] = false;
          if (this.ValidCell(IntVec3.op_Addition(((Thing) this.parent).Position, adjacentCellsAround[index])))
            this.\u003CValidCells\u003Ek__BackingField[index] = true;
        }
      }
      return this.\u003CValidCells\u003Ek__BackingField;
    }
  }

  public bool IsOnlyBridge
  {
    get
    {
      if (!this.IsBridge)
        return false;
      this.cachedIsOnlyBridge.GetValueOrDefault();
      if (!this.cachedIsOnlyBridge.HasValue)
        this.cachedIsOnlyBridge = new bool?(IsOnlyBridgeStatus());
      return this.cachedIsOnlyBridge.Value;

      bool IsOnlyBridgeStatus()
      {
        if (!((Thing) this.parent).Spawned)
          return false;
        bool[] validCells = this.ValidCells;
        CompMapExpander.tmpCells.Clear();
        for (int index = 0; index < 8; ++index)
        {
          if (validCells[index])
            CompMapExpander.tmpCells.Add(IntVec3.op_Addition(((Thing) this.parent).Position, GenAdj.AdjacentCellsAround[index]));
        }
        bool result = true;
        ((Thing) this.parent).Map.floodFiller.FloodFill(GenCollection.PopFront<IntVec3>(CompMapExpander.tmpCells), (Predicate<IntVec3>) (c => this.ValidCell(c) && IntVec3.op_Inequality(c, ((Thing) this.parent).Position)), (Func<IntVec3, bool>) (c =>
        {
          if (CompMapExpander.tmpCells.Contains(c))
          {
            CompMapExpander.tmpCells.Remove(c);
            if (GenCollection.Empty<IntVec3>(CompMapExpander.tmpCells))
            {
              result = false;
              return true;
            }
          }
          return false;
        }), int.MaxValue, false, (IEnumerable<IntVec3>) null);
        return result;
      }
    }
  }

  public bool IsBridge
  {
    get
    {
      this.cachedIsBridge.GetValueOrDefault();
      if (!this.cachedIsBridge.HasValue)
        this.cachedIsBridge = new bool?(IsBridgeStatus());
      return this.cachedIsBridge.Value;

      bool IsBridgeStatus()
      {
        if (!((Thing) this.parent).Spawned)
          return false;
        bool[] validCells = this.ValidCells;
        bool[] flagArray = validCells;
        bool flag1 = flagArray[flagArray.Length - 1];
        bool flag2 = false;
        for (int index = 0; index < 8; ++index)
        {
          if (validCells[index])
          {
            if (!flag1)
            {
              if (flag2)
                return true;
              flag2 = true;
              flag1 = true;
            }
          }
          else if (flag1)
            flag1 = false;
        }
        return false;
      }
    }
  }

  private bool ValidCell(IntVec3 c)
  {
    return GenGrid.InBounds(c, ((Thing) this.parent).Map) && GridsUtility.GetTerrain(c, ((Thing) this.parent).Map) != VMF_DefOf.VMF_ImpassableFloor;
  }

  public virtual void PostSpawnSetup(bool respawningAfterLoad)
  {
    if (respawningAfterLoad)
      FrameDelay.DelayOne<CompMapExpander>(new Action<CompMapExpander>(Process), this);
    else
      Process(this);

    static void Process(CompMapExpander comp)
    {
      VehiclePawnWithMap vehicle;
      if (!((Thing) comp.parent).IsOnVehicleMapOf(out vehicle))
        return;
      Map map = ((Thing) comp.parent).Map;
      MapComponent mapComponent = (MapComponent) null;
      bool flag = ModCompat.CompatBase<ModCompat.AsAboveSoBelow>.Active && (mapComponent = ModCompat.AsAboveSoBelow.CompOf(map)) != null && ModCompat.AsAboveSoBelow.Banded(mapComponent);
      CellRect cellRect = GenAdj.OccupiedRect((Thing) comp.parent);
      foreach (IntVec3 intVec3 in cellRect)
      {
        map.terrainGrid.SetTerrain(intVec3, VMF_DefOf.VMF_VehicleFloor);
        if (flag)
        {
          for (int index = 1; index <= ModCompat.AsAboveSoBelow.UpperLevels(); ++index)
            map.terrainGrid.SetTerrain(ModCompat.AsAboveSoBelow.Translate(mapComponent, intVec3, index), VMF_DefOf.VMF_VehicleFloor);
        }
      }
      vehicle.MapExpanderComps.Add(comp);
      comp.DirtySelfAndAdjacentComps(((Thing) comp.parent).Map);
      vehicle.impassableCellsDirty = true;
      vehicle.resizeRequest = true;
      CrossMapReachabilityCache.ClearCacheFor(vehicle.VehicleMap);
    }
  }

  public virtual void PostDeSpawn(Map map, DestroyMode mode = 0)
  {
    CellRect cellRect = GenAdj.OccupiedRect((Thing) this.parent);
    VehiclePawnWithMap vehicle;
    if (map.IsVehicleMapOf(out vehicle))
    {
      MapComponent mapComponent = (MapComponent) null;
      bool flag = ModCompat.CompatBase<ModCompat.AsAboveSoBelow>.Active && (mapComponent = ModCompat.AsAboveSoBelow.CompOf(map)) != null && ModCompat.AsAboveSoBelow.Banded(mapComponent);
      foreach (IntVec3 intVec3 in cellRect)
      {
        map.terrainGrid.SetTerrain(intVec3, VMF_DefOf.VMF_ImpassableFloor);
        if (flag)
        {
          for (int index = 1; index <= ModCompat.AsAboveSoBelow.UpperLevels(); ++index)
            map.terrainGrid.SetTerrain(ModCompat.AsAboveSoBelow.Translate(mapComponent, intVec3, index), VMF_DefOf.VMF_ImpassableFloor);
        }
      }
      vehicle.MapExpanderComps.Remove(this);
      if (this.IsBridge)
        vehicle.MapExpanderComps.ForEach((Action<CompMapExpander>) (c => c.cachedIsOnlyBridge = new bool?()));
      this.DirtySelfAndAdjacentComps(map);
      vehicle.impassableCellsDirty = true;
      vehicle.resizeRequest = true;
      CrossMapReachabilityCache.ClearCacheFor(vehicle.VehicleMap);
    }
    foreach (IntVec3 intVec3 in cellRect)
    {
      List<Thing> thingList = map.thingGrid.ThingsListAtFast(intVec3);
      for (int index = thingList.Count - 1; index >= 0; --index)
      {
        Thing thing = thingList[index];
        if (!(thing is Pawn))
        {
          if (thing.def.Minifiable)
            MinifyUtility.Uninstall(thing);
          else
            thing.Destroy((DestroyMode) 4);
        }
      }
    }
  }

  private void DirtySelfAndAdjacentComps(Map map)
  {
    this.validCellsDirty = true;
    this.cachedIsBridge = new bool?();
    this.cachedIsOnlyBridge = new bool?();
    foreach (IntVec3 intVec3 in GenAdj.CellsAdjacent8Way((Thing) this.parent).Where<IntVec3>((Func<IntVec3, bool>) (c => GenGrid.InBounds(c, map))))
    {
      foreach (Thing thing in map.thingGrid.ThingsListAtFast(intVec3))
      {
        CompMapExpander compMapExpander;
        if (ThingCompUtility.TryGetComp<CompMapExpander>(thing, ref compMapExpander))
        {
          compMapExpander.validCellsDirty = true;
          compMapExpander.cachedIsBridge = new bool?();
          compMapExpander.cachedIsOnlyBridge = new bool?();
          break;
        }
      }
    }
  }

  public static void DebugDraw(List<CompMapExpander> comps)
  {
    VehiclePawnWithMap vehicle;
    if (!CompMapExpander.debugDraw || !VehicleMapUtility.FocusedOnVehicleMap(out vehicle))
      return;
    Quaternion fullAngleQuat = VehicleMapUtility.get_FullAngleQuat((VehiclePawn) vehicle);
    foreach (CompMapExpander comp in comps)
    {
      if (comp.IsBridge)
      {
        Material material = DebugMatsSpectrum.Mat(comp.IsOnlyBridge ? 10 : 30, true);
        IntVec3 position = ((Thing) comp.parent).Position;
        Vector3 baseMapCoord = ((IntVec3) ref position).ToVector3ShiftedWithAltitude((AltitudeLayer) 39).ToBaseMapCoord();
        Graphics.DrawMesh(MeshPool.plane10, baseMapCoord, fullAngleQuat, material, 0);
      }
    }
  }
}
