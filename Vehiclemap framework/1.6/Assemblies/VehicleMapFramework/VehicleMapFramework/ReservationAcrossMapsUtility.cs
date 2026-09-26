// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.ReservationAcrossMapsUtility
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public static class ReservationAcrossMapsUtility
{
  private static readonly List<ReservationManager.Reservation> tmpReservations = new List<ReservationManager.Reservation>();

  private static bool RespectsReservationsOf(Pawn newClaimant, Pawn oldClaimant)
  {
    return newClaimant == oldClaimant || ((Thing) newClaimant).Faction != null && ((Thing) oldClaimant).Faction != null && (((Thing) newClaimant).Faction == ((Thing) oldClaimant).Faction || !FactionUtility.HostileTo(((Thing) newClaimant).Faction, ((Thing) oldClaimant).Faction) || oldClaimant.HostFaction != null && oldClaimant.HostFaction == newClaimant.HostFaction || newClaimant.HostFaction != null && (oldClaimant.HostFaction != null || newClaimant.HostFaction == ((Thing) oldClaimant).Faction));
  }

  public static bool CanReserve(
    this Pawn p,
    LocalTargetInfo target,
    int maxPawns,
    int stackCount,
    ReservationLayerDef layer,
    bool ignoreOtherReservations,
    Map map)
  {
    if (p == null)
    {
      Log.Error("CanReserve with null claimant");
      return false;
    }
    if (!((Thing) p).Spawned || VehicleMapUtility.get_BaseMapOrCaravan((Thing) p) != VehicleMapUtility.get_BaseMapOrCaravan(map) || !((LocalTargetInfo) ref target).IsValid || ((LocalTargetInfo) ref target).ThingDestroyed || ((LocalTargetInfo) ref target).HasThing && ((LocalTargetInfo) ref target).Thing.SpawnedOrAnyParentSpawned && ((LocalTargetInfo) ref target).Thing.MapHeld != map)
      return false;
    int num1 = ((LocalTargetInfo) ref target).HasThing ? ((LocalTargetInfo) ref target).Thing.stackCount : 1;
    int num2 = stackCount == -1 ? num1 : stackCount;
    if (num2 > num1)
      return false;
    if (ignoreOtherReservations)
      return true;
    if (map.physicalInteractionReservationManager.IsReserved(target) && !map.physicalInteractionReservationManager.IsReservedBy(p, target) || ModCompat.CompatBase<ModCompat.MultiFloors>.Active && map != ((Thing) p).Map && ((Thing) p).Map.physicalInteractionReservationManager.IsReserved(target) && !((Thing) p).Map.physicalInteractionReservationManager.IsReservedBy(p, target))
      return false;
    ReservationAcrossMapsUtility.tmpReservations.Clear();
    ReservationAcrossMapsUtility.tmpReservations.AddRange((IEnumerable<ReservationManager.Reservation>) map.reservationManager.ReservationsReadOnly);
    if (ModCompat.CompatBase<ModCompat.MultiFloors>.Active && map != ((Thing) p).Map)
      ReservationAcrossMapsUtility.tmpReservations.AddRange((IEnumerable<ReservationManager.Reservation>) ((Thing) p).Map.reservationManager.ReservationsReadOnly);
    if (GenCollection.Any<ReservationManager.Reservation>(ReservationAcrossMapsUtility.tmpReservations, (Predicate<ReservationManager.Reservation>) (reservation =>
    {
      if (!LocalTargetInfo.op_Equality(reservation.Target, target) || reservation.Layer != layer || reservation.Claimant != p)
        return false;
      return reservation.StackCount == -1 || reservation.StackCount >= num2;
    })))
      return true;
    if (((LocalTargetInfo) ref target).HasThing && ((LocalTargetInfo) ref target).Thing is Building thing && ((Thing) thing).def.hasInteractionCell)
    {
      IntVec3 interactionCell = ((Thing) thing).InteractionCell;
      Building edifice = GridsUtility.GetEdifice(interactionCell, map);
      if (edifice != null)
      {
        Pawn pawn;
        if (map.reservationManager.TryGetReserver(LocalTargetInfo.op_Implicit((Thing) edifice), ((Thing) p).Faction, ref pawn) && ((Thing) pawn).Spawned && pawn != p)
          return false;
      }
      else
      {
        Pawn pawn;
        if (map.reservationManager.TryGetReserver(LocalTargetInfo.op_Implicit(interactionCell), ((Thing) p).Faction, ref pawn) && ((Thing) pawn).Spawned && pawn != p)
          return false;
      }
    }
    int num3 = 0;
    int num4 = 0;
    foreach (ReservationManager.Reservation reservation in ReservationAcrossMapsUtility.tmpReservations.Where<ReservationManager.Reservation>((Func<ReservationManager.Reservation, bool>) (reservation => LocalTargetInfo.op_Equality(reservation.Target, target) && reservation.Layer == layer && reservation.Claimant != p && ReservationAcrossMapsUtility.RespectsReservationsOf(p, reservation.Claimant))))
    {
      if (reservation.MaxPawns != maxPawns)
        return false;
      ++num3;
      if (reservation.StackCount == -1)
        num4 += num1;
      else
        num4 += reservation.StackCount;
      if (num3 >= maxPawns || num2 + num4 > num1)
        return false;
    }
    return true;
  }

  public static bool CanReserveNew(this Pawn p, LocalTargetInfo target, Map destMap)
  {
    return ((LocalTargetInfo) ref target).IsValid && !p.HasReserved(target, (Job) null, destMap) && p.CanReserve(target, 1, -1, (ReservationLayerDef) null, false, destMap);
  }

  public static bool HasReserved(this Pawn p, LocalTargetInfo target, Job job, Map destMap)
  {
    return ((Thing) p).Spawned && destMap.reservationManager.ReservedBy(target, p, job);
  }

  public static bool Reserve(
    this Pawn p,
    Map map,
    LocalTargetInfo target,
    Job job,
    int maxPawns = 1,
    int stackCount = -1,
    ReservationLayerDef layer = null,
    bool errorOnFailed = true,
    bool ignoreOtherReservations = false)
  {
    if (map == null && ((LocalTargetInfo) ref target).HasThing)
      map = ((LocalTargetInfo) ref target).Thing.MapHeld;
    return map != null && map.reservationManager.Reserve(p, job, target, maxPawns, stackCount, layer, errorOnFailed, ignoreOtherReservations, true);
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u00242F59771236D7C1AA57ADCD68358D448A
  {
    [ExtensionMarker("<M>$8B425F2470FE840097539A9F6CCCF83E")]
    public bool CanReserve(
      LocalTargetInfo target,
      int maxPawns,
      int stackCount,
      ReservationLayerDef layer,
      bool ignoreOtherReservations,
      Map map)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$8B425F2470FE840097539A9F6CCCF83E")]
    public bool CanReserveNew(LocalTargetInfo target, Map destMap)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$8B425F2470FE840097539A9F6CCCF83E")]
    public bool HasReserved(LocalTargetInfo target, Job job, Map destMap)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$8B425F2470FE840097539A9F6CCCF83E")]
    public bool Reserve(
      Map map,
      LocalTargetInfo target,
      Job job,
      int maxPawns = 1,
      int stackCount = -1,
      ReservationLayerDef layer = null,
      bool errorOnFailed = true,
      bool ignoreOtherReservations = false)
    {
      throw new NotSupportedException();
    }

    [SpecialName]
    public static class \u003CM\u003E\u00248B425F2470FE840097539A9F6CCCF83E
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(Pawn p)
      {
      }
    }
  }
}
