// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Avatar_ProcessMovement
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_PerspectiveShift")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_Avatar_ProcessMovement
{
  private static CompZipline compZipline;
  private static readonly AccessTools.FieldRef<PawnTweener, Vector3> tweenedPos = AccessTools.FieldRefAccess<PawnTweener, Vector3>(nameof (tweenedPos));

  public static bool Prefix(
    Vector3 ___moveInput,
    ref Vector3? ___physicsPosition,
    ref IntVec3 ___prevCell,
    Pawn ___pawn)
  {
    if (((Thing) ___pawn).Map == null || !___physicsPosition.HasValue)
      return true;
    VehiclePawnWithMap vehicle;
    ((Thing) ___pawn).IsOnVehicleMapOf(out vehicle);
    if (Patch_Avatar_ProcessMovement.compZipline != null)
    {
      ThingWithComps parent = Patch_Avatar_ProcessMovement.compZipline.parent;
      if ((parent == null || ((Thing) parent).Spawned) && !IntVec3.op_Inequality(((Thing) ___pawn).Position, ((Thing) Patch_Avatar_ProcessMovement.compZipline.parent).Position))
      {
        Thing pair = Patch_Avatar_ProcessMovement.compZipline.Pair;
        if (pair != null && pair.Spawned)
        {
          if (Vector3.op_Inequality(___moveInput, Vector3.zero))
          {
            Vector3 drawPos1 = ((Thing) ___pawn).DrawPos;
            Vector3 drawPos2 = Patch_Avatar_ProcessMovement.compZipline.Pair.DrawPos;
            Vector3 drawPos3 = ((Thing) Patch_Avatar_ProcessMovement.compZipline.parent).DrawPos;
            Vector3 vector3_1 = Vector3.op_Subtraction(drawPos2, drawPos1);
            float num1 = GenGeo.MagnitudeHorizontal(vector3_1);
            float num2 = GenGeo.MagnitudeHorizontalSquared(Vector3.op_Subtraction(drawPos3, drawPos2));
            if ((double) num2 < (double) num1 * (double) num1)
            {
              Patch_Avatar_ProcessMovement.compZipline = (CompZipline) null;
              return true;
            }
            Vector3 vector3_2;
            // ISSUE: explicit constructor call
            ((Vector3) ref vector3_2).\u002Ector(vector3_1.x / num1, 0.0f, vector3_1.z / num1);
            if (vehicle != null)
              vector3_2 = Vector3Utility.RotatedBy(vector3_2, VehicleMapUtility.get_FullAngle((VehiclePawn) vehicle));
            bool flag = (double) Vector3.Dot(vector3_2, ___moveInput) < 0.0;
            float num3 = Patch_Avatar_ProcessMovement.compZipline.IsZiplineEnd ^ flag ? 0.0375f : 0.075f;
            if (flag)
              num3 *= -1f;
            ref Vector3? local = ref ___physicsPosition;
            Vector3? nullable1 = ___physicsPosition;
            Vector3 vector3_3 = Vector3.op_Multiply(vector3_2, num3);
            Vector3? nullable2 = nullable1.HasValue ? new Vector3?(Vector3.op_Addition(nullable1.GetValueOrDefault(), vector3_3)) : new Vector3?();
            local = nullable2;
            if ((double) GenGeo.MagnitudeHorizontalSquared(Vector3.op_Subtraction(drawPos3, drawPos1)) > (double) num2)
            {
              RespawnPawn(___pawn, Patch_Avatar_ProcessMovement.compZipline.Pair.Position, Patch_Avatar_ProcessMovement.compZipline.Pair.Map, out ___prevCell);
              ___physicsPosition = new Vector3?(___physicsPosition.Value.ToThingBaseMapCoord((Thing) Patch_Avatar_ProcessMovement.compZipline.parent).ToNonFocusedThingMapCoord(Patch_Avatar_ProcessMovement.compZipline.Pair));
              Patch_Avatar_ProcessMovement.tweenedPos.Invoke(___pawn.Drawer.tweener) = ___physicsPosition.Value;
              Patch_Avatar_ProcessMovement.compZipline = (CompZipline) null;
            }
          }
          return false;
        }
      }
      Patch_Avatar_ProcessMovement.compZipline = (CompZipline) null;
      return true;
    }
    CompZipline compZipline = GridsUtility.GetThingList(((Thing) ___pawn).Position, ((Thing) ___pawn).Map).Select<Thing, CompZipline>((Func<Thing, CompZipline>) (t => ThingCompUtility.TryGetComp<CompZipline>(t))).FirstOrDefault<CompZipline>();
    if (compZipline != null)
    {
      Thing pair = compZipline.Pair;
      if (pair != null && pair.Spawned && (double) GenGeo.MagnitudeHorizontalSquared(Vector3.op_Subtraction(compZipline.Pair.DrawPos, ((Thing) ___pawn).DrawPos)) < (double) GenGeo.MagnitudeHorizontalSquared(Vector3.op_Subtraction(compZipline.Pair.DrawPos, ((Thing) compZipline.parent).DrawPos)))
      {
        Patch_Avatar_ProcessMovement.compZipline = compZipline;
        return true;
      }
    }
    if (Vector3.op_Equality(___moveInput, Vector3.zero))
    {
      if (GenGrid.WalkableBy(IntVec3Utility.ToIntVec3(___physicsPosition.Value), ((Thing) ___pawn).Map, ___pawn))
        return true;
      ___pawn.pather.TryRecoverFromUnwalkablePosition(false);
      ref Vector3? local = ref ___physicsPosition;
      IntVec3 position = ((Thing) ___pawn).Position;
      Vector3? nullable = new Vector3?(((IntVec3) ref position).ToVector3Shifted());
      local = nullable;
      return false;
    }
    VehiclePawnWithMap vehiclePawnWithMap = vehicle;
    if (vehiclePawnWithMap != null)
    {
      if (((Thing) vehiclePawnWithMap).Spawned && vehicle.CachedWalkableMapEdgeCells.Keys.Contains<IntVec3>(((Thing) ___pawn).Position))
      {
        Vector3 baseMapCoord = ___physicsPosition.Value.ToBaseMapCoord(vehicle);
        Vector3 vector3_4 = Vector3Utility.RotatedBy(___moveInput, VehicleMapUtility.get_FullAngle((VehiclePawn) vehicle));
        if (Vector3.op_Addition(baseMapCoord, Vector3.op_Division(vector3_4, 2f)).TryGetVehicleMap(vehicle, VehicleMapFlag.None))
        {
          if (!GenGrid.WalkableBy(IntVec3Utility.ToIntVec3(___physicsPosition.Value), ((Thing) ___pawn).Map, ___pawn))
          {
            ___pawn.pather.TryRecoverFromUnwalkablePosition(false);
            ref Vector3? local = ref ___physicsPosition;
            IntVec3 position = ((Thing) ___pawn).Position;
            Vector3? nullable = new Vector3?(((IntVec3) ref position).ToVector3Shifted());
            local = nullable;
          }
          return true;
        }
        Map map = ((Thing) vehicle).Map;
        float num = ___pawn.TicksPerMoveCardinal * 4f;
        CompVehicleEnterSpot vehicleEnterSpot;
        if (!GenCollection.Any<Thing>(GridsUtility.GetThingList(((Thing) ___pawn).Position, vehicle.VehicleMap), (Predicate<Thing>) (t => ThingCompUtility.TryGetComp<CompVehicleEnterSpot>(t, ref vehicleEnterSpot) && !(vehicleEnterSpot is CompZipline))))
          num *= 2f;
        Vector3 vector3_5 = Vector3.op_Multiply(Time.deltaTime * (60f / num), vector3_4);
        Vector3 vector3_6 = Vector3.op_Addition(baseMapCoord, vector3_5);
        IntVec3 intVec3 = IntVec3Utility.ToIntVec3(vector3_6);
        if (!GenGrid.InBounds(intVec3, map))
          return true;
        List<Thing> thingList = GridsUtility.GetThingList(intVec3, map);
        bool flag = false;
        for (int index = 0; index < thingList.Count; ++index)
        {
          if (thingList[index] == vehicle)
          {
            flag = true;
            break;
          }
        }
        if (flag)
        {
          ref Vector3? local = ref ___physicsPosition;
          Vector3? nullable3 = ___physicsPosition;
          Vector3 vector3_7 = Vector3.op_Multiply(Time.deltaTime * (60f / num), ___moveInput);
          Vector3? nullable4 = nullable3.HasValue ? new Vector3?(Vector3.op_Addition(nullable3.GetValueOrDefault(), vector3_7)) : new Vector3?();
          local = nullable4;
          Rot8 rot = Rot8.FromAngle(Vector3Utility.AngleFlat(___moveInput));
          Rot4 rotation = ((Thing) vehicle).Rotation;
          if (((Rot4) ref rotation).IsHorizontal)
            rot = Rot8.op_Implicit(rot.RotForVehicleDraw());
          ((Thing) ___pawn).Rotation = Rot8.op_Implicit(rot);
          Building_VehicleRamp buildingVehicleRamp;
          if (GridsUtility.TryGetFirstThing<Building_VehicleRamp>(((Thing) ___pawn).Position, vehicle.VehicleMap, ref buildingVehicleRamp))
            buildingVehicleRamp.StartManualOpenBy(___pawn);
          return false;
        }
        if (GenGrid.Walkable(intVec3, map))
        {
          RespawnPawn(___pawn, intVec3, map, out ___prevCell);
          ___physicsPosition = new Vector3?(vector3_6);
          Patch_Avatar_ProcessMovement.tweenedPos.Invoke(___pawn.Drawer.tweener) = ___physicsPosition.Value;
          return false;
        }
      }
    }
    else
    {
      Vector3 original = Vector3.op_Addition(___physicsPosition.Value, ___moveInput);
      IntVec3 intVec3_1 = IntVec3Utility.ToIntVec3(original);
      if (GenGrid.InBounds(intVec3_1, ((Thing) ___pawn).Map) && GridsUtility.TryGetFirstThing<VehiclePawnWithMap>(intVec3_1, ((Thing) ___pawn).Map, ref vehicle))
      {
        IntVec3 intVec3_2 = IntVec3Utility.ToIntVec3(original.ToVehicleMapCoord(vehicle));
        IntVec3 intVec3_3 = intVec3_1.ClosestEdgeCell(vehicle);
        if (!((IntVec3) ref intVec3_3).IsValid)
          return true;
        float num = ___pawn.TicksPerMoveCardinal * 4f;
        if (GenAdj.AdjacentTo8WayOrInside(intVec3_3, intVec3_2))
        {
          Building_Door door = GridsUtility.GetDoor(intVec3_3, vehicle.VehicleMap);
          if (door != null && door.PawnCanOpen(___pawn))
            door.StartManualOpenBy(___pawn);
          Building_VehicleRamp buildingVehicleRamp;
          if (GridsUtility.TryGetFirstThing<Building_VehicleRamp>(intVec3_3, vehicle.VehicleMap, ref buildingVehicleRamp))
            buildingVehicleRamp.StartManualOpenBy(___pawn);
          CompVehicleEnterSpot vehicleEnterSpot;
          if (!GenCollection.Any<Thing>(GridsUtility.GetThingList(intVec3_3, vehicle.VehicleMap), (Predicate<Thing>) (t => ThingCompUtility.TryGetComp<CompVehicleEnterSpot>(t, ref vehicleEnterSpot) && !(vehicleEnterSpot is CompZipline))))
            num *= 2f;
        }
        Vector3 vector3_8 = Vector3.op_Multiply(Time.deltaTime * (60f / num), ___moveInput);
        ref Vector3? local1 = ref ___physicsPosition;
        Vector3? nullable5 = ___physicsPosition;
        Vector3 vector3_9 = vector3_8;
        Vector3? nullable6 = nullable5.HasValue ? new Vector3?(Vector3.op_Addition(nullable5.GetValueOrDefault(), vector3_9)) : new Vector3?();
        local1 = nullable6;
        ((Thing) ___pawn).Rotation = Rot8.op_Implicit(Rot8.FromAngle(Vector3Utility.AngleFlat(___moveInput)));
        if (___physicsPosition.Value.TryGetVehicleMap(vehicle, VehicleMapFlag.None))
        {
          Vector3 vehicleMapCoord = ___physicsPosition.Value.ToVehicleMapCoord(vehicle);
          IntVec3 intVec3_4 = IntVec3Utility.ToIntVec3(vehicleMapCoord);
          if (GenGrid.Walkable(intVec3_4, vehicle.VehicleMap))
          {
            RespawnPawn(___pawn, intVec3_4, vehicle.VehicleMap, out ___prevCell);
            ___physicsPosition = new Vector3?(vehicleMapCoord);
            Patch_Avatar_ProcessMovement.tweenedPos.Invoke(___pawn.Drawer.tweener) = ___physicsPosition.Value;
            return false;
          }
          ref Vector3? local2 = ref ___physicsPosition;
          nullable5 = ___physicsPosition;
          Vector3 vector3_10 = vector3_8;
          Vector3? nullable7 = nullable5.HasValue ? new Vector3?(Vector3.op_Subtraction(nullable5.GetValueOrDefault(), vector3_10)) : new Vector3?();
          local2 = nullable7;
        }
        return false;
      }
    }
    return true;

    static void RespawnPawn(Pawn pawn, IntVec3 cell, Map map, out IntVec3 prevCell)
    {
      pawn.DeSpawnWithoutJobClear((DestroyMode) 0);
      GenSpawn.Spawn((Thing) pawn, cell, map, (WipeMode) 0);
      prevCell = IntVec3.Invalid;
    }
  }
}
