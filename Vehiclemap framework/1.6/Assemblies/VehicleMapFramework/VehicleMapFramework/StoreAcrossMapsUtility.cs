// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.StoreAcrossMapsUtility
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public static class StoreAcrossMapsUtility
{
  public static Map tmpDestMap;

  public static bool TryFindBestBetterStoreCellFor(
    Thing t,
    Pawn carrier,
    Map map,
    StoragePriority currentPriority,
    Faction faction,
    ref IntVec3 foundCell,
    bool needAccurateResult)
  {
    StoreAcrossMapsUtility.tmpDestMap = (Map) null;
    Map map1 = map.BaseMap();
    VehiclePawnWithMap vehicle;
    List<SlotGroup> listInPriorityOrder;
    if (map1.IsVehicleMapOf(out vehicle))
    {
      WorldObject orStashedVehicle = vehicle.VehicleCaravanOrStashedVehicle;
      CaravanHaulDestinationManager destinationManager;
      if (orStashedVehicle != null && orStashedVehicle.TryGetComponent<CaravanHaulDestinationManager>(ref destinationManager))
      {
        listInPriorityOrder = destinationManager.AllGroupsListInPriorityOrder;
        goto label_4;
      }
    }
    listInPriorityOrder = ComponentCache.GetCachedMapComponent<CrossMapHaulDestinationManager>(map1).AllGroupsListInPriorityOrder;
label_4:
    List<SlotGroup> slotGroupList = listInPriorityOrder;
    if (slotGroupList.Count == 0)
      return false;
    StoragePriority foundPriority = currentPriority;
    float maxValue = (float) int.MaxValue;
    IntVec3 invalid = IntVec3.Invalid;
    foreach (SlotGroup slotGroup in slotGroupList)
    {
      Map map2 = ((IHaulDestination) slotGroup.parent)?.Map;
      if (map2 != null && map != map2)
      {
        StoragePriority priority = slotGroup.Settings.Priority;
        if (priority >= foundPriority)
        {
          if (priority > currentPriority)
            StoreAcrossMapsUtility.TryFindBestBetterStoreCellForWorker(t, carrier, map2, faction, (ISlotGroup) slotGroup, needAccurateResult, ref invalid, ref maxValue, ref foundPriority);
          else
            break;
        }
        else
          break;
      }
    }
    if (!((IntVec3) ref invalid).IsValid)
      return false;
    foundCell = invalid;
    return true;
  }

  public static void TryFindBestBetterStoreCellForWorker(
    Thing t,
    Pawn carrier,
    Map map,
    Faction faction,
    ISlotGroup slotGroup,
    bool needAccurateResult,
    ref IntVec3 closestSlot,
    ref float closestDistSquared,
    ref StoragePriority foundPriority)
  {
    if (slotGroup == null || !slotGroup.Settings.AllowedToAccept(t))
      return;
    IntVec3 intVec3_1 = t.SpawnedOrAnyParentSpawned ? VehicleMapUtility.get_PositionHeldOnBaseMap(t).CellOnAnotherMap(map) : VehicleMapUtility.get_PositionHeldOnBaseMap((Thing) carrier).CellOnAnotherMap(map);
    List<IntVec3> cellsList = slotGroup.CellsList;
    int count = cellsList.Count;
    int num = needAccurateResult ? Mathf.FloorToInt((float) count * Rand.Range(0.005f, 0.018f)) : 0;
    for (int index = 0; index < count; ++index)
    {
      IntVec3 c = cellsList[index];
      IntVec3 intVec3_2 = IntVec3.op_Subtraction(intVec3_1, c);
      float horizontalSquared = (float) ((IntVec3) ref intVec3_2).LengthHorizontalSquared;
      if ((double) horizontalSquared <= (double) closestDistSquared && StoreAcrossMapsUtility.IsGoodStoreCell(c, map, t, carrier, faction))
      {
        closestSlot = c;
        closestDistSquared = horizontalSquared;
        // ISSUE: cast to a reference type
        // ISSUE: explicit reference operation
        ^(sbyte&) ref foundPriority = (sbyte) slotGroup.Settings.Priority;
        StoreAcrossMapsUtility.tmpDestMap = map;
        if (index >= num)
          break;
      }
    }
  }

  public static bool IsGoodStoreCell(IntVec3 c, Map map, Thing t, Pawn carrier, Faction faction)
  {
    if (carrier != null && c.IsForbidden(carrier, map) || !StoreUtility.IsValidStorageFor(c, map, t))
      return false;
    if (carrier != null)
    {
      if (!carrier.CanReserveNew(LocalTargetInfo.op_Implicit(c), map))
        return false;
    }
    else if (faction != null && map.reservationManager.IsReservedByAnyoneOf(LocalTargetInfo.op_Implicit(c), faction))
      return false;
    if (FireUtility.ContainsStaticFire(c, map) || GenCollection.Any<Thing>(GridsUtility.GetThingList(c, map), (Predicate<Thing>) (t1 => t1 is IConstructible && GenConstruct.BlocksConstruction(t1, t))))
      return false;
    if (carrier == null)
      return true;
    Thing spawnedParentOrMe = t.SpawnedParentOrMe;
    Map departMap;
    IntVec3 root;
    if (spawnedParentOrMe != null)
    {
      departMap = spawnedParentOrMe.Map;
      root = spawnedParentOrMe == t || !spawnedParentOrMe.def.hasInteractionCell ? spawnedParentOrMe.Position : spawnedParentOrMe.InteractionCell;
    }
    else
    {
      departMap = CrossMapReachabilityUtility.get_DepartMapOrPawnMap(carrier);
      root = ((Thing) carrier).PositionHeld;
    }
    return CrossMapReachabilityUtility.CanReach(departMap, root, LocalTargetInfo.op_Implicit(c), (PathEndMode) 3, TraverseParms.For(carrier, (Danger) 3, (TraverseMode) 0, false, false, false, true), map);
  }

  public static bool TryFindBestBetterNonSlotGroupStorageFor(
    Thing t,
    Pawn carrier,
    Map map,
    StoragePriority currentPriority,
    Faction faction,
    ref IHaulDestination haulDestination,
    bool acceptSamePriority,
    bool requiresDestReservation)
  {
    Map map1 = map.BaseMap();
    VehiclePawnWithMap vehicle;
    List<IHaulDestination> listInPriorityOrder;
    if (map1.IsVehicleMapOf(out vehicle))
    {
      WorldObject orStashedVehicle = vehicle.VehicleCaravanOrStashedVehicle;
      CaravanHaulDestinationManager destinationManager;
      if (orStashedVehicle != null && orStashedVehicle.TryGetComponent<CaravanHaulDestinationManager>(ref destinationManager))
      {
        listInPriorityOrder = destinationManager.AllHaulDestinationsListInPriorityOrder;
        goto label_4;
      }
    }
    listInPriorityOrder = ComponentCache.GetCachedMapComponent<CrossMapHaulDestinationManager>(map1).AllHaulDestinationsListInPriorityOrder;
label_4:
    Map map2 = t.SpawnedOrAnyParentSpawned ? t.MapHeld : ((Thing) carrier).MapHeld;
    IntVec3 root = t.SpawnedOrAnyParentSpawned ? t.PositionHeld : ((Thing) carrier).PositionHeld;
    IntVec3 intVec3 = t.SpawnedOrAnyParentSpawned ? VehicleMapUtility.get_PositionHeldOnBaseMap(t) : VehicleMapUtility.get_PositionHeldOnBaseMap((Thing) carrier);
    float num = float.MaxValue;
    StoragePriority storagePriority = (StoragePriority) 0;
    foreach (IHaulDestination dest in listInPriorityOrder)
    {
      Map map3 = dest.Map;
      if (map3 != null && map3 != map)
      {
        switch (dest)
        {
          case ISlotGroupParent _:
            continue;
          case Building_Grave _:
            if (!PawnUtility.CanBeBuried(t))
              continue;
            break;
        }
        StoragePriority priority = ((IStoreSettingsParent) dest).GetStoreSettings().Priority;
        if (priority >= storagePriority)
        {
          if (acceptSamePriority)
          {
            if (priority < currentPriority)
              break;
          }
          if (!acceptSamePriority)
          {
            if (priority <= currentPriority)
              break;
          }
          float squared = (float) IntVec3Utility.DistanceToSquared(intVec3, dest.PositionOnBaseMap());
          if ((double) squared <= (double) num && dest.Accepts(t) && (!(dest is Thing thing) || thing.Faction == faction))
          {
            if (thing != null)
            {
              if (carrier != null)
              {
                if (ForbidUtility.IsForbidden(thing, carrier))
                  continue;
              }
              else if (faction != null && ForbidUtility.IsForbidden(thing, faction))
                continue;
            }
            if (thing != null & requiresDestReservation)
            {
              if (thing is IHaulEnroute ihaulEnroute)
              {
                if (!map2.reservationManager.OnlyReservationsForJobDef(LocalTargetInfo.op_Implicit(thing), JobDefOf.HaulToContainer, false) || EnrouteUtility.GetSpaceRemainingWithEnroute(ihaulEnroute, t.def, (Pawn) null) <= 0)
                  continue;
              }
              else if (carrier != null)
              {
                if (!carrier.CanReserveNew(LocalTargetInfo.op_Implicit(thing), map2))
                  continue;
              }
              else if (faction != null && map2.reservationManager.IsReservedByAnyoneOf(LocalTargetInfo.op_Implicit(thing), faction))
                continue;
            }
            if (carrier != null)
            {
              if (thing != null)
              {
                if (!CrossMapReachabilityUtility.CanReach(map2, root, LocalTargetInfo.op_Implicit(thing), (PathEndMode) 3, TraverseParms.For(carrier, (Danger) 3, (TraverseMode) 0, false, false, false, true), thing.Map))
                  continue;
              }
              else if (!CrossMapReachabilityUtility.CanReach(map2, root, LocalTargetInfo.op_Implicit(dest.Position), (PathEndMode) 3, TraverseParms.For(carrier, (Danger) 3, (TraverseMode) 0, false, false, false, true), dest.Map))
                continue;
            }
            num = squared;
            storagePriority = priority;
            haulDestination = dest;
          }
        }
        else
          break;
      }
    }
    return haulDestination != null;
  }

  public static bool NoStorageBlockersIn(IntVec3 c, Map map, Thing thing)
  {
    List<Thing> thingList = map.thingGrid.ThingsListAt(c);
    bool flag = false;
    for (int index = 0; index < thingList.Count; ++index)
    {
      Thing thing1 = thingList[index];
      if (!flag && thing1.def.EverStorable(false) && thing1.CanStackWith(thing) && thing1.stackCount < thing1.def.stackLimit)
        flag = true;
      if (thing1.def.entityDefToBuild != null && thing1.def.entityDefToBuild.passability != null || thing1.def.surfaceType == null && ((BuildableDef) thing1.def).passability != null && (GridsUtility.GetMaxItemsAllowedInCell(c, map) <= 1 || thing1.def.category != 2))
        return false;
    }
    return flag || GridsUtility.GetItemCount(c, map) < GridsUtility.GetMaxItemsAllowedInCell(c, map);
  }
}
