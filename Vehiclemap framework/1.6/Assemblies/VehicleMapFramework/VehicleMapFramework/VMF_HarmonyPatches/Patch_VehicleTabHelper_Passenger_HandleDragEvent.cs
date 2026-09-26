// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_VehicleTabHelper_Passenger_HandleDragEvent
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Vehicles.World;
using Verse;
using Verse.Sound;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (VehicleTabHelper_Passenger), "HandleDragEvent")]
[PatchLevel(Level.Safe)]
public static class Patch_VehicleTabHelper_Passenger_HandleDragEvent
{
  public static bool Prefix(
    ref Pawn ___draggedPawn,
    IThingHolder ___transferToHolder,
    Pawn ___hoveringOverPawn)
  {
    if (Event.current.type == 1 && Event.current.button == 0 && ___draggedPawn != null && ((Thing) ___draggedPawn).Faction == Faction.OfPlayer && ___transferToHolder != null)
    {
      VehiclePawnWithMap vehicle1;
      if (___transferToHolder is Map map1 && map1.IsVehicleMapOf(out vehicle1))
      {
        VehicleCaravan vehicleCaravan = Ext_Vehicles.GetVehicleCaravan(___draggedPawn);
        if (((Thing) ___draggedPawn).ParentHolder is VehicleRoleHandler parentHolder)
        {
          IntVec3 spot;
          Map map;
          if (!((Thing) ___draggedPawn).Spawned && Patch_VehicleTabHelper_Passenger_HandleDragEvent.TryFindSpawnSpot(vehicle1, parentHolder, out spot, out map))
          {
            vehicle1.RemovePawn(___draggedPawn);
            GenSpawn.Spawn((Thing) ___draggedPawn, spot, map, (WipeMode) 0);
            parentHolder.vehicle.EventRegistry[VehicleEventDefOf.PawnExited].ExecuteEvents();
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
            ___draggedPawn = (Pawn) null;
            return false;
          }
        }
        else
        {
          IntVec3 spot;
          Map map;
          if (!((Thing) ___draggedPawn).Spawned && WorldPawnsUtility.IsWorldPawn(___draggedPawn) && Patch_VehicleTabHelper_Passenger_HandleDragEvent.TryFindSpawnSpot(vehicle1, (VehicleRoleHandler) null, out spot, out map))
          {
            Find.WorldPawns.RemovePawn(___draggedPawn);
            GenSpawn.Spawn((Thing) ___draggedPawn, spot, map, (WipeMode) 0);
            vehicleCaravan?.RecacheVehicles();
            if (vehicleCaravan != null)
            {
              IEnumerable<InspectTabBase> inspectTabs = ((WorldObject) vehicleCaravan).GetInspectTabs();
              if (inspectTabs != null)
                inspectTabs.FirstOrDefault<InspectTabBase>((Func<InspectTabBase, bool>) (t => t is WITab_Vehicle_Manifest))?.OnOpen();
            }
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
            ___draggedPawn = (Pawn) null;
            return false;
          }
          VehiclePawnWithMap vehicle2;
          if (((Thing) ___draggedPawn).IsOnVehicleMapOf(out vehicle2) && vehicle1 != vehicle2 && Patch_VehicleTabHelper_Passenger_HandleDragEvent.TryFindSpawnSpot(vehicle2, (VehicleRoleHandler) null, out spot, out map))
          {
            ((Entity) ___draggedPawn).DeSpawn((DestroyMode) 0);
            GenSpawn.Spawn((Thing) ___draggedPawn, spot, map, (WipeMode) 0);
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
            ___draggedPawn = (Pawn) null;
            return false;
          }
        }
        Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VMF_CannotSpawn", NamedArgument.op_Implicit((Thing) ___draggedPawn))), MessageTypeDefOf.RejectInput, false);
        ___draggedPawn = (Pawn) null;
        return false;
      }
      if (((Thing) ___draggedPawn).IsOnVehicleMapOf(out vehicle1))
      {
        if (___transferToHolder is VehicleRoleHandler vehicleHandler)
        {
          if (!vehicleHandler.CanOperateRole(___draggedPawn))
          {
            Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_HandlerNotEnoughRoom", NamedArgument.op_Implicit((Thing) ___draggedPawn), NamedArgument.op_Implicit(vehicleHandler.role.label))), MessageTypeDefOf.RejectInput, false);
            ___draggedPawn = (Pawn) null;
            return false;
          }
          if (!vehicleHandler.AreSlotsAvailable)
          {
            if (___hoveringOverPawn != null)
            {
              IntVec3 spot;
              Map map2;
              if (Patch_VehicleTabHelper_Passenger_HandleDragEvent.TryFindSpawnSpot(vehicle1, vehicleHandler, out spot, out map2))
              {
                vehicle1.RemovePawn(___hoveringOverPawn);
                GenSpawn.Spawn((Thing) ___hoveringOverPawn, spot, map2, (WipeMode) 0);
                vehicleHandler.vehicle.EventRegistry[VehicleEventDefOf.PawnExited].ExecuteEvents();
              }
              else
              {
                Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VMF_CannotSpawn", NamedArgument.op_Implicit((Thing) ___hoveringOverPawn))), MessageTypeDefOf.RejectInput, false);
                ___draggedPawn = (Pawn) null;
                return false;
              }
            }
            else
            {
              Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_HandlerNotEnoughRoom", NamedArgument.op_Implicit((Thing) ___draggedPawn), NamedArgument.op_Implicit(vehicleHandler.role.label))), MessageTypeDefOf.RejectInput, false);
              ___draggedPawn = (Pawn) null;
              return false;
            }
          }
        }
        IntVec3 position = ((Thing) ___draggedPawn).Position;
        ((Entity) ___draggedPawn).DeSpawn((DestroyMode) 0);
        if (___transferToHolder.GetDirectlyHeldThings().TryAddOrTransfer((Thing) ___draggedPawn, false))
        {
          SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
          if (___transferToHolder is VehicleRoleHandler vehicleRoleHandler)
            vehicleRoleHandler.vehicle.EventRegistry[VehicleEventDefOf.PawnEntered].ExecuteEvents();
          else if (!WorldPawnsUtility.IsWorldPawn(___draggedPawn))
            Find.WorldPawns.PassToWorld(___draggedPawn, (PawnDiscardDecideMode) 0);
          if (___transferToHolder is VehicleCaravan vehicleCaravan)
          {
            vehicleCaravan.RecacheVehicles();
            IEnumerable<InspectTabBase> inspectTabs = ((WorldObject) vehicleCaravan).GetInspectTabs();
            if (inspectTabs != null)
              inspectTabs.FirstOrDefault<InspectTabBase>((Func<InspectTabBase, bool>) (t => t is WITab_Vehicle_Manifest))?.OnOpen();
          }
        }
        else
          GenSpawn.Spawn((Thing) ___draggedPawn, position, vehicle1.VehicleMap, (WipeMode) 0);
        ___draggedPawn = (Pawn) null;
        return false;
      }
    }
    return true;
  }

  private static bool TryFindSpawnSpot(
    VehiclePawnWithMap vehicle,
    VehicleRoleHandler vehicleHandler,
    out IntVec3 spot,
    out Map map)
  {
    if (vehicleHandler != null && vehicleHandler.vehicle == vehicle && vehicleHandler.role is VehicleRoleBuildable role)
    {
      ThingWithComps parent = role.upgradeComp.parent;
      CellRect cellRect1 = GenAdj.OccupiedRect((Thing) parent);
      CellRect cellRect2 = ((CellRect) ref cellRect1).ExpandedBy(1);
      if (GenCollection.TryRandomElement<IntVec3>(((CellRect) ref cellRect2).EdgeCells.Where<IntVec3>((Func<IntVec3, bool>) (c => GenGrid.InBounds(c, ((Thing) parent).Map) && Predicate(c, ((Thing) parent).Map) && !Ext_IList.NotNullAndAny<Thing>(GridsUtility.GetThingList(c, ((Thing) parent).Map), (Predicate<Thing>) (t => t is Pawn)))), ref spot))
      {
        map = ((Thing) parent).Map;
        return true;
      }
      spot = IntVec3.Invalid;
      map = (Map) null;
      return false;
    }
    if (GenCollection.Any<CompVehicleEnterSpot>(vehicle.EnterComps) && GenCollection.TryRandomElement<IntVec3>(vehicle.EnterComps.Select<CompVehicleEnterSpot, IntVec3>((Func<CompVehicleEnterSpot, IntVec3>) (c => ((Thing) c.parent).Position)), (Predicate<IntVec3>) (c => Predicate(c, vehicle.VehicleMap)), ref spot) || GenCollection.TryRandomElement<IntVec3>((IEnumerable<IntVec3>) vehicle.CachedMapEdgeCells, (Predicate<IntVec3>) (c => Predicate(c, vehicle.VehicleMap)), ref spot))
    {
      map = vehicle.VehicleMap;
      return true;
    }
    if (RCellFinder.TryFindRandomCellNearWith(GenCollection.RandomElement<IntVec3>((IEnumerable<IntVec3>) vehicle.CachedMapEdgeCells), (Predicate<IntVec3>) (c => Predicate(c, vehicle.VehicleMap)), vehicle.VehicleMap, ref spot, 5, int.MaxValue))
    {
      map = vehicle.VehicleMap;
      return true;
    }
    spot = IntVec3.Invalid;
    map = (Map) null;
    return false;

    static bool Predicate(IntVec3 c, Map map)
    {
      return (GenGrid.Standable(c, map) || GridsUtility.GetDoor(c, map) != null) && GridsUtility.GetFirstPawn(c, map) == null;
    }
  }
}
