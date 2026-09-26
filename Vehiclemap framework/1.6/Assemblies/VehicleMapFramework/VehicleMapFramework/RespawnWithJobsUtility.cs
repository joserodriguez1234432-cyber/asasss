// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.RespawnWithJobsUtility
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public static class RespawnWithJobsUtility
{
  private static readonly AccessTools.FieldRef<LordJob_Ritual, List<RitualStage>> stages = AccessTools.FieldRefAccess<LordJob_Ritual, List<RitualStage>>(nameof (stages));
  private static readonly AccessTools.FieldRef<LordJob_Ritual, List<RitualStagePositions>> ritualStagePositions = AccessTools.FieldRefAccess<LordJob_Ritual, List<RitualStagePositions>>(nameof (ritualStagePositions));
  private static readonly AccessTools.FieldRef<LordJob_Joinable_Gathering, IntVec3> spot = AccessTools.FieldRefAccess<LordJob_Joinable_Gathering, IntVec3>(nameof (spot));

  public static void DeSpawnWithoutJobClear(this Pawn pawn, DestroyMode mode = 0)
  {
    if (((Thing) pawn).Destroyed)
      Log.Error($"Tried to despawn {Gen.ToStringSafe<Thing>((Thing) pawn)} which is already destroyed.");
    else if (!((Thing) pawn).Spawned)
    {
      Log.Error($"Tried to despawn {Gen.ToStringSafe<Thing>((Thing) pawn)} which is not spawned.");
    }
    else
    {
      Map map = ((Thing) pawn).Map;
      map.overlayDrawer.DisposeHandle((Thing) pawn);
      RegionListersUpdater.DeregisterInRegions((Thing) pawn, map);
      map.spawnedThings.Remove((Thing) pawn);
      map.listerThings.Remove((Thing) pawn);
      map.thingGrid.Deregister((Thing) pawn, false);
      map.coverGrid.DeRegister((Thing) pawn);
      if (((Thing) pawn).def.receivesSignals)
        Find.SignalManager.DeregisterReceiver((ISignalReceiver) pawn);
      map.tooltipGiverList.Notify_ThingDespawned((Thing) pawn);
      if (((Thing) pawn).def.CanAffectLinker)
      {
        map.linkGrid.Notify_LinkerCreatedOrDestroyed((Thing) pawn);
        map.mapDrawer.MapMeshDirty(((Thing) pawn).Position, MapMeshFlagDef.op_Implicit(MapMeshFlagDefOf.Things), true, false);
      }
      VehiclePawnWithMap vehicle;
      if (Find.Selector.IsSelected((object) pawn) && map.IsVehicleMapOf(out vehicle) && !((Thing) vehicle).Spawned)
      {
        Find.Selector.Deselect((object) pawn);
        Find.MainButtonsRoot.tabs.Notify_SelectedObjectDespawned();
      }
      ((Thing) pawn).DirtyMapMesh(map);
      if (((Thing) pawn).def.drawerType != 2)
        map.dynamicDrawManager.DeRegisterDrawable((Thing) pawn);
      map.regionGrid.GetValidRegionAt_NoRebuild(((Thing) pawn).Position)?.Room?.Notify_ContainedThingSpawnedOrDespawned((Thing) pawn);
      Find.TickManager.DeRegisterAllTickabilityFor((Thing) pawn);
      ((Thing) pawn).ForceSetStateToUnspawned();
      map.attackTargetsCache.Notify_ThingDespawned((Thing) pawn);
      if (pawn is IHaulEnroute ihaulEnroute)
        map.enrouteManager.Notify_ContainerDespawned(ihaulEnroute);
      StealAIDebugDrawer.Notify_ThingChanged((Thing) pawn);
      if (pawn is IHaulDestination ihaulDestination)
        map.haulDestinationManager.RemoveHaulDestination(ihaulDestination);
      if (pawn is IHaulSource ihaulSource)
        map.haulDestinationManager.RemoveHaulSource(ihaulSource);
      if (Find.ColonistBar != null)
        Find.ColonistBar.MarkColonistsDirty();
      if (((Thing) pawn).def.category == 2)
      {
        SlotGroup slotGroup = StoreUtility.GetSlotGroup(((Thing) pawn).Position, map);
        if (slotGroup != null && slotGroup.parent != null)
          slotGroup.parent.Notify_LostThing((Thing) pawn);
      }
      QuestUtility.SendQuestTargetSignals(((Thing) pawn).questTags, "Despawned", NamedArgumentUtility.Named((object) pawn, "SUBJECT"));
      ((Thing) pawn).spawnedTick = -1;
      if (((ThingWithComps) pawn).AllComps != null)
      {
        for (int index = 0; index < ((ThingWithComps) pawn).AllComps.Count; ++index)
          ((ThingWithComps) pawn).AllComps[index].PostDeSpawn(map, (DestroyMode) 0);
      }
      pawn.pather?.StopDead();
      pawn.mindState.droppedWeapon = (Thing) null;
      pawn.needs?.mood?.thoughts.situational.Notify_SituationalThoughtsDirty();
      pawn.meleeVerbs?.Notify_PawnDespawned();
      pawn.mechanitor?.Notify_DeSpawned(mode);
      if (map != null)
      {
        map.mapPawns.DeRegisterPawn(pawn);
        map.autoSlaughterManager.Notify_PawnDespawned();
      }
      if (!PawnUtility.IsCarrying(pawn))
        return;
      map.designationManager.RemoveAllDesignationsOn(pawn.carryTracker.CarriedThing, false);
    }
  }

  public static void DeSpawnWithoutJobClearVehicle(this VehiclePawn vehicle, DestroyMode mode = 0)
  {
    vehicle.vehiclePather?.StopDead();
    ComponentCache.GetDetachedMapComponent<VehiclePositionManager>(((Thing) vehicle).Map).ReleaseClaimed(vehicle);
    VehicleReservationManager cachedMapComponent = ComponentCache.GetCachedMapComponent<VehicleReservationManager>(((Thing) vehicle).Map);
    cachedMapComponent.ClearReservedFor(vehicle);
    cachedMapComponent.RemoveAllListerFor(vehicle);
    vehicle.cargoToLoad.Clear();
    ComponentCache.GetCachedMapComponent<ListerVehiclesRepairable>(((Thing) vehicle).Map).NotifyVehicleDespawned(vehicle);
    vehicle.EventRegistry[VehicleEventDefOf.Despawned].ExecuteEvents();
    if (!GenList.NullOrEmpty<ThingComp>((IList<ThingComp>) ((ThingWithComps) vehicle).AllComps))
    {
      for (int index = 0; index < ((ThingWithComps) vehicle).AllComps.Count; ++index)
      {
        if (((ThingWithComps) vehicle).AllComps[index] is VehicleComp allComp)
          allComp.OnDeSpawn();
      }
    }
    ((Pawn) vehicle).DeSpawnWithoutJobClear(mode);
    vehicle.SoundCleanup();
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u00242F59771236D7C1AA57ADCD68358D448A
  {
    [ExtensionMarker("<M>$E15D13A036DCD41255EBD6266F8556E9")]
    public void DeSpawnWithoutJobClear(DestroyMode mode = 0) => throw new NotSupportedException();

    [SpecialName]
    public static class \u003CM\u003E\u0024E15D13A036DCD41255EBD6266F8556E9
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(Pawn pawn)
      {
      }
    }
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u0024ABFF6B6B8A5941EB4E8CC8D86BA457C5
  {
    [ExtensionMarker("<M>$F9A44A31E7799B8454B27FC6F4A26CDD")]
    public void DeSpawnWithoutJobClearVehicle(DestroyMode mode = 0)
    {
      throw new NotSupportedException();
    }

    [SpecialName]
    public static class \u003CM\u003E\u0024F9A44A31E7799B8454B27FC6F4A26CDD
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(VehiclePawn vehicle)
      {
      }
    }
  }
}
