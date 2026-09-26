// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.ExpansionUpgrade
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using JetBrains.Annotations;
using SmashTools.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

[UsedImplicitly]
public class ExpansionUpgrade : Upgrade
{
  public Dictionary<VehicleDef, VehicleDef> defTransitions;
  public List<CellRect> expandAreas;

  public virtual bool UnlockOnLoad => false;

  public virtual void Unlock(VehiclePawn vehicle, bool unlockingPostLoad)
  {
    VehicleDef vehicleDef = vehicle.VehicleDef;
    VehicleDef to;
    if (!this.defTransitions.TryGetValue(vehicle.VehicleDef, out to))
      return;
    VehicleResizeUtility.PreResize(vehicle);
    ((Thing) vehicle).def = (ThingDef) to;
    if (!(vehicle is VehiclePawnWithMap vehicle1))
      return;
    foreach (CellRect expandArea in this.expandAreas)
    {
      CellRect cellRect = ((CellRect) ref expandArea).MovedBy(IntVec2.One);
      foreach (IntVec3 intVec3 in cellRect)
        vehicle1.VehicleMap.terrainGrid.SetTerrain(intVec3, VMF_DefOf.VMF_VehicleFloor);
    }
    this.Finalize(vehicle1, vehicleDef, to);
  }

  public virtual void Refund(VehiclePawn vehicle)
  {
    VehicleDef vehicleDef = vehicle.VehicleDef;
    VehicleDef key = this.defTransitions.FirstOrDefault<KeyValuePair<VehicleDef, VehicleDef>>((Func<KeyValuePair<VehicleDef, VehicleDef>, bool>) (pair => pair.Value == vehicle.VehicleDef)).Key;
    if (key == null)
      return;
    VehicleResizeUtility.PreResize(vehicle);
    ((Thing) vehicle).def = (ThingDef) key;
    if (!(vehicle is VehiclePawnWithMap vehicle1))
      return;
    foreach (CellRect expandArea in this.expandAreas)
    {
      CellRect cellRect = ((CellRect) ref expandArea).MovedBy(IntVec2.One);
      foreach (IntVec3 intVec3 in cellRect)
        vehicle1.VehicleMap.terrainGrid.SetTerrain(intVec3, VMF_DefOf.VMF_ImpassableFloor);
      try
      {
        Thing.allowDestroyNonDestroyable = true;
        foreach (IntVec3 intVec3 in cellRect)
        {
          List<Thing> thingList = vehicle1.VehicleMap.thingGrid.ThingsListAtFast(intVec3);
          for (int index = thingList.Count - 1; index >= 0; --index)
          {
            Thing thing = thingList[index];
            if (thing is Pawn pawn)
              pawn.pather.TryRecoverFromUnwalkablePosition(false);
            else
              thing.Destroy((DestroyMode) 0);
          }
        }
      }
      finally
      {
        Thing.allowDestroyNonDestroyable = false;
      }
    }
    this.Finalize(vehicle1, vehicleDef, key);
  }

  private void Finalize(VehiclePawnWithMap vehicle, VehicleDef from, VehicleDef to)
  {
    if (!((Thing) vehicle).Spawned)
      return;
    vehicle.ResetGraphic();
    GraphicOverlayRenderer overlayRenderer = vehicle.DrawTracker.overlayRenderer;
    foreach (GraphicOverlay overlay in overlayRenderer.Overlays)
    {
      overlayRenderer.AllOverlaysListForReading.Remove(overlay);
      vehicle.DrawTracker.RemoveRenderer((IParallelRenderer) overlay);
      overlay.Destroy();
    }
    overlayRenderer.Init();
    List<VehicleComponent> list = vehicle.statHandler.components.ToList<VehicleComponent>();
    vehicle.statHandler.InitializeComponents();
    foreach (VehicleComponent component in vehicle.statHandler.components)
    {
      foreach (VehicleComponent vehicleComponent in list)
      {
        if (component.props.key == vehicleComponent.props.key)
          component.SetHealth(vehicleComponent.Health);
      }
    }
    IntVec3 position = ((Thing) vehicle).Position;
    VehicleResizeUtility.Reposition(ref position, (VehiclePawn) vehicle, Vector3.op_Subtraction(((GraphicData) from.graphicData).DrawOffsetForRot(Rot4.North), ((GraphicData) to.graphicData).DrawOffsetForRot(Rot4.North)));
    FrameDelay.DelayOne<(VehiclePawnWithMap, IntVec3)>((Action<(VehiclePawnWithMap, IntVec3)>) (state => VehicleResizeUtility.Respawn(state.vehicle, state.pos)), (vehicle, position));
  }
}
