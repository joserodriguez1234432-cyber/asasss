// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleResizeUtility
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public static class VehicleResizeUtility
{
  public static void ResizeNow(this VehiclePawnWithMap vehicle, bool reposition = true)
  {
    VehicleDef vehicleDef = vehicle.VehicleDef;
    IntVec2 size1 = ((BuildableDef) vehicleDef).Size;
    CellRect cellRect = CellRect.WholeMap(vehicle.VehicleMap);
    CellRect validMapRect = vehicle.ValidMapRect;
    IntVec2 size2 = ((CellRect) ref validMapRect).Size;
    IntVec2 intVec2 = size2;
    if (!IntVec2.op_Inequality(size1, intVec2))
      return;
    VehicleResizeUtility.PreResize((VehiclePawn) vehicle);
    ((ThingDef) vehicleDef).size = size2;
    Vector3 vector3_1 = Vector3.op_Subtraction(((CellRect) ref cellRect).CenterVector3, ((CellRect) ref validMapRect).CenterVector3);
    CompVehicleDrawOffset vehicleDrawOffset = vehicle.CompVehicleDrawOffset;
    Vector3 vector3_2 = vehicleDrawOffset != null ? vehicleDrawOffset.drawOffset : Vector3.zero;
    if (vehicleDrawOffset != null)
    {
      vehicleDrawOffset.drawOffset = vector3_1;
      vehicleDrawOffset.drawOffsetNorth = new Vector3?(vector3_1);
      vehicleDrawOffset.drawOffsetEast = new Vector3?(Vector3Utility.RotatedBy(vector3_1, Rot4.East));
      vehicleDrawOffset.drawOffsetSouth = new Vector3?(Vector3Utility.RotatedBy(vector3_1, Rot4.South));
      vehicleDrawOffset.drawOffsetWest = new Vector3?(Vector3Utility.RotatedBy(vector3_1, Rot4.West));
    }
    if (vehicle.VehicleMapProps is VehicleMapProps_Unique vehicleMapProps)
    {
      VehicleDef baseDef = vehicleMapProps.baseDef;
      if (baseDef != null)
        ((ThingDef) vehicleDef).uiIconScale = (float) Mathf.Max(((ThingDef) baseDef).size.x, ((ThingDef) baseDef).size.z) / ((float) Mathf.Max(size2.x, size2.z) + 1f);
    }
    UniqueVehicleUtility.ReinitializeComponents(vehicleDef);
    VehicleResizeUtility.PostResize((VehiclePawn) vehicle);
    if (((Thing) vehicle).Spawned)
    {
      IntVec3 position = ((Thing) vehicle).Position;
      if (reposition)
        VehicleResizeUtility.Reposition(ref position, (VehiclePawn) vehicle, Vector3.op_Subtraction(vector3_2, vector3_1));
      VehicleResizeUtility.Respawn(vehicle, position);
    }
    else
    {
      VehicleFormationComp component = vehicle.VehicleCaravanOrStashedVehicle?.GetComponent<VehicleFormationComp>();
      VehicleFormationComp.DrawData drawData1;
      if (component != null && component.DrawPositions.TryGetValue((VehiclePawn) vehicle, out drawData1))
      {
        IntVec3 centerCell = ((CellRect) ref drawData1.cellRect).CenterCell;
        Vector3 vector3_3 = Vector3.op_Subtraction(vector3_2, vector3_1);
        IntVec3 intVec3 = IntVec3.op_Addition(centerCell, new IntVec3((int) MathF.Truncate(vector3_3.x), 0, (int) MathF.Truncate(vector3_3.z)));
        if ((double) vector3_3.x < 0.0 == (((BuildableDef) vehicle.VehicleDef).Size.x % 2 == 1))
          intVec3 = IntVec3.op_Addition(intVec3, IntVec3.op_Multiply(IntVec3.East, (int) ((double) vector3_3.x % 1.0 * 2.0)));
        if ((double) vector3_3.z < 0.0 == (((BuildableDef) vehicle.VehicleDef).Size.z % 2 == 1))
          intVec3 = IntVec3.op_Addition(intVec3, IntVec3.op_Multiply(IntVec3.North, (int) ((double) vector3_3.z % 1.0 * 2.0)));
        drawData1.cellRect = CellRect.CenteredOn(intVec3, size2);
        component.DrawPositions[(VehiclePawn) vehicle] = drawData1;
        foreach (KeyValuePair<VehiclePawn, VehicleFormationComp.DrawData> keyValuePair in component.DrawPositions.ToArray<KeyValuePair<VehiclePawn, VehicleFormationComp.DrawData>>())
        {
          VehiclePawn vehiclePawn1;
          VehicleFormationComp.DrawData drawData2;
          keyValuePair.Deconstruct(ref vehiclePawn1, ref drawData2);
          VehiclePawn vehiclePawn2 = vehiclePawn1;
          VehicleFormationComp.DrawData drawData3 = drawData2;
          if (vehicle != vehiclePawn2 && ((CellRect) ref drawData1.cellRect).Overlaps(drawData3.cellRect))
          {
            component.DrawPositions.Remove((VehiclePawn) vehicle);
            component.FindVehiclePosition((VehiclePawn) vehicle);
            break;
          }
        }
        component.CenteredDrawPositions();
      }
    }
    if (UnityData.IsInMainThread)
      vehicle.VehicleMapGizmo.portrait.MarkDirty();
    else
      LongEventHandler.ExecuteWhenFinished((Action) (() => vehicle.VehicleMapGizmo.portrait.MarkDirty()));
  }

  public static void PreResize(VehiclePawn vehicle)
  {
    if (((Thing) vehicle).Spawned)
    {
      RegionListersUpdater.DeregisterInRegions((Thing) vehicle, ((Thing) vehicle).Map);
      ((Thing) vehicle).Map.thingGrid.Deregister((Thing) vehicle, false);
      ((Thing) vehicle).Map.coverGrid.DeRegister((Thing) vehicle);
    }
    if (!(vehicle is VehiclePawnWithMap state))
      return;
    FrameDelay.DelayOne<VehiclePawnWithMap>((Action<VehiclePawnWithMap>) (_vehicle =>
    {
      _vehicle.impassableCellsDirty = true;
      _vehicle.mapEdgeCellsDirty = true;
      _vehicle.walkableCellsDirty = true;
      _vehicle.enterPositionsDirty = true;
    }), state);
  }

  public static void PostResize(VehiclePawn vehicle)
  {
    if (!(vehicle is VehiclePawnWithMap vehicle1))
      return;
    vehicle1.RecacheDrawPos(((Thing) vehicle1).DrawPos);
    foreach (VehicleRoleHandler handler in vehicle.Handlers)
    {
      if (handler.role is VehicleRoleBuildable role)
      {
        PawnOverlayRenderer pawnRenderer = role.pawnRenderer;
        if (pawnRenderer != null)
          pawnRenderer.SetDrawOffsets(vehicle1, role);
      }
    }
  }

  public static void Reposition(ref IntVec3 pos, VehiclePawn vehicle, Vector3 delta)
  {
    Rot4 rotation = ((Thing) vehicle).Rotation;
    pos = IntVec3.op_Addition(pos, IntVec3Utility.RotatedBy(new IntVec3((int) MathF.Truncate(delta.x), 0, (int) MathF.Truncate(delta.z)), rotation));
    int int32 = Convert.ToInt32(((Rot4) ref rotation).AsInt > 1);
    if ((double) delta.x < 0.0 == (((BuildableDef) vehicle.VehicleDef).Size.x % 2 == int32))
      pos = IntVec3.op_Addition(pos, IntVec3Utility.RotatedBy(IntVec3.op_Multiply(IntVec3.East, (int) ((double) delta.x % 1.0 * 2.0)), rotation));
    if ((double) delta.z < 0.0 != (((BuildableDef) vehicle.VehicleDef).Size.z % 2 == int32))
      return;
    pos = IntVec3.op_Addition(pos, IntVec3Utility.RotatedBy(IntVec3.op_Multiply(IntVec3.North, (int) ((double) delta.z % 1.0 * 2.0)), rotation));
  }

  public static void Respawn(VehiclePawnWithMap vehicle, IntVec3 pos)
  {
    Rot4 rotation = ((Thing) vehicle).Rotation;
    Map map = ((Thing) vehicle).Map;
    int num = Find.Selector.IsSelected((object) vehicle) ? 1 : 0;
    vehicle.DeSpawnWithoutJobClearVehicle((DestroyMode) 1);
    bool flag = ((Rot4) ref rotation).AsInt > 1;
    if (((BuildableDef) vehicle.VehicleDef).Size.x % 2 == 0 & flag)
      ++pos.x;
    if (((BuildableDef) vehicle.VehicleDef).Size.z % 2 == 0 & flag)
      pos.z += Rot4.op_Equality(rotation, Rot4.West) ? -1 : 1;
    GenSpawn.Spawn((Thing) vehicle, pos, map, rotation, (WipeMode) 0, false, false);
    if (num == 0)
      return;
    Find.Selector.Select((object) vehicle, false, false);
  }
}
