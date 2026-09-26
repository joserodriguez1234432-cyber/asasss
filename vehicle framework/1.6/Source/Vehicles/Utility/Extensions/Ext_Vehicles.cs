// Decompiled with JetBrains decompiler
// Type: Vehicles.Ext_Vehicles
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Vehicles.World;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

#nullable disable
namespace Vehicles;

[PublicAPI]
[StaticConstructorOnStartup]
public static class Ext_Vehicles
{
  public static void SpawnPawnNearVehicle(this VehiclePawn vehicle, Pawn pawn)
  {
    if (((Thing) pawn).Spawned)
      return;
    CellRect cellRect1 = GenAdj.OccupiedRect((Thing) vehicle);
    CellRect cellRect2 = ((CellRect) ref cellRect1).ExpandedBy(1);
    IntVec3 intVec3_1 = ((Thing) vehicle).Position;
    IntVec3 intVec3_2;
    if (GenCollection.TryRandomElement<IntVec3>(((CellRect) ref cellRect2).EdgeCells.Where<IntVec3>((Func<IntVec3, bool>) (cell => GenGrid.InBounds(cell, ((Thing) vehicle).Map) && GenGrid.Standable(cell, ((Thing) vehicle).Map) && !GridsUtility.GetThingList(cell, ((Thing) vehicle).Map).NotNullAndAny<Thing>((Predicate<Thing>) (thing => thing is Pawn)))), ref intVec3_2))
      intVec3_1 = intVec3_2;
    GenSpawn.Spawn((Thing) pawn, intVec3_1, ((Thing) vehicle).MapHeld, (WipeMode) 0);
    if (!GenGrid.Standable(intVec3_1, ((Thing) vehicle).Map))
      pawn.pather.TryRecoverFromUnwalkablePosition(false);
    if (vehicle.lord == null)
      return;
    LordUtility.GetLord(pawn)?.Notify_PawnLost(pawn, (PawnLostCondition) 9, new DamageInfo?());
    vehicle.lord.AddPawn(pawn);
  }

  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsRoofed(IntVec3 cell, Map map) => GridsUtility.Roofed(cell, map);

  [Pure]
  public static bool IsRoofRestricted(VehicleDef vehicleDef, IntVec3 cell, Map map)
  {
    CompProperties_VehicleLauncher sortedCompProperties = vehicleDef.GetSortedCompProperties<CompProperties_VehicleLauncher>();
    if (sortedCompProperties == null)
      return true;
    bool canRoofPunch = SettingsCache.TryGetValue<bool>(vehicleDef, typeof (CompProperties_VehicleLauncher), "canRoofPunch", sortedCompProperties.canRoofPunch);
    return Ext_Vehicles.IsRoofRestricted(cell, map, canRoofPunch);
  }

  [Pure]
  private static bool IsRoofRestricted(IntVec3 cell, Map map, bool canRoofPunch)
  {
    if (!canRoofPunch)
      return Ext_Vehicles.IsRoofed(cell, map);
    RoofDef roof = GridsUtility.GetRoof(cell, map);
    return roof != null && roof.isThickRoof;
  }

  [MustUseReturnValue]
  public static bool HasRoomFor(this VehiclePawn vehicle, List<Pawn> pawns)
  {
    VehicleReservationManager reservationManager = (VehicleReservationManager) null;
    if (((Thing) vehicle).Spawned)
      reservationManager = ((Thing) vehicle).Map.GetCachedMapComponent<VehicleReservationManager>();
    int num1 = 0;
    int num2 = 0;
    foreach (VehicleRoleHandler handler in vehicle.handlers)
    {
      num1 += handler.role.Slots;
      VehicleHandlerReservation reservation = reservationManager?.GetReservation<VehicleHandlerReservation>(vehicle);
      if (reservation != null)
        num2 += reservation.ClaimantsOnHandler(handler);
    }
    return pawns.Count <= num1 - num2;
  }

  [Pure]
  public static IntVec2 MirrorRotatedBy(this IntVec2 cell, Rot4 rot, IntVec2 size)
  {
    if (size.x == 1 && size.z == 1)
      return cell;
    IntVec2 intVec2 = cell.RotatedBy(rot, size);
    switch (((Rot4) ref rot).AsInt)
    {
      case 1:
        intVec2.x *= -1;
        intVec2.z *= -1;
        break;
      case 3:
        if (size.x.IsEven())
        {
          ++intVec2.z;
          --intVec2.x;
        }
        if (size.z.IsEven())
        {
          --intVec2.z;
          --intVec2.x;
        }
        intVec2.x *= -1;
        intVec2.z *= -1;
        break;
    }
    return intVec2;
  }

  [Pure]
  public static IntVec2 RotatedBy(this IntVec2 cell, Rot4 rot, IntVec2 size)
  {
    if (size.x == 1 && size.z == 1)
      return cell;
    switch (((Rot4) ref rot).AsInt)
    {
      case 0:
        return cell;
      case 1:
        IntVec2 intVec2_1;
        // ISSUE: explicit constructor call
        ((IntVec2) ref intVec2_1).\u002Ector(-cell.z, cell.x);
        return intVec2_1;
      case 2:
        IntVec2 intVec2_2;
        // ISSUE: explicit constructor call
        ((IntVec2) ref intVec2_2).\u002Ector(-cell.x, -cell.z);
        if (size.x.IsEven())
          ++intVec2_2.x;
        if (size.z.IsEven())
          ++intVec2_2.z;
        return intVec2_2;
      case 3:
        IntVec2 intVec2_3;
        // ISSUE: explicit constructor call
        ((IntVec2) ref intVec2_3).\u002Ector(cell.z, -cell.x);
        if (size.x.IsEven())
          ++intVec2_3.x;
        if (size.z.IsEven())
          ++intVec2_3.z;
        return intVec2_3;
      default:
        return cell;
    }
  }

  public static void RemoveBoardedPawnsFromLord(this LordJob lordJob, PawnLostCondition condition)
  {
    foreach (Pawn ownedPawn in lordJob.lord.ownedPawns)
    {
      if (ownedPawn is VehiclePawn vehiclePawn)
      {
        foreach (Pawn pawn in vehiclePawn.AllPawnsAboard)
        {
          LordUtility.GetLord(pawn)?.Notify_PawnLost(ownedPawn, condition, new DamageInfo?());
          lordJob.Map.attackTargetsCache.UpdateTarget((IAttackTarget) pawn);
          if (lordJob.EndPawnJobOnCleanup(pawn) && ((Thing) pawn).Spawned && pawn.CurJob != null && (!lordJob.DontInterruptLayingPawnsOnCleanup || !RestUtility.IsLayingForJobCleanup(pawn)))
            pawn.jobs.EndCurrentJob((JobCondition) 16 /*0x10*/, true, true);
        }
      }
    }
  }

  [Pure]
  public static CellRect VehicleRect(this VehiclePawn vehicle, bool maxSizePossible = false)
  {
    return vehicle.VehicleRect(((Thing) vehicle).Position, ((Thing) vehicle).Rotation, maxSizePossible);
  }

  [Pure]
  public static CellRect VehicleRect(
    this VehiclePawn vehicle,
    IntVec3 center,
    Rot4 rot,
    bool maxSizePossible = false)
  {
    return vehicle.VehicleDef.VehicleRect(center, rot, maxSizePossible);
  }

  [Pure]
  public static CellRect VehicleRect(
    this VehicleDef vehicleDef,
    IntVec3 center,
    Rot4 rot,
    bool maxSizePossible = false)
  {
    IntVec2 size = vehicleDef.size;
    Ext_Vehicles.AdjustForVehicleOccupiedRect(ref size, ref rot, maxSizePossible);
    return GenAdj.OccupiedRect(center, rot, size);
  }

  public static void AdjustForVehicleOccupiedRect(
    ref IntVec2 size,
    ref Rot4 rot,
    bool maxSizePossible = false)
  {
    if (Rot4.op_Equality(rot, Rot4.West))
      rot = Rot4.East;
    if (Rot4.op_Equality(rot, Rot4.South))
      rot = Rot4.North;
    if (!maxSizePossible)
      return;
    int num = Mathf.Max(size.x, size.z);
    size.x = num;
    size.z = num;
  }

  [Pure]
  public static IntVec3 PadForHitbox(this IntVec3 cell, Map map, VehiclePawn vehicle)
  {
    return cell.PadForHitbox(map, vehicle.VehicleDef);
  }

  [Pure]
  public static IntVec3 PadForHitbox(this IntVec3 cell, Map map, VehicleDef vehicleDef)
  {
    int num1 = Mathf.Max(((BuildableDef) vehicleDef).Size.x, ((BuildableDef) vehicleDef).Size.z);
    bool flag = num1 % 2 == 0;
    int num2 = Mathf.CeilToInt((float) num1 / 2f);
    if (flag)
      ++num2;
    if (cell.x < num2)
      cell.x = num2;
    else if (cell.x + num2 > map.Size.x)
      cell.x = map.Size.x - num2;
    if (cell.z < num2)
      cell.z = num2;
    else if (cell.z + num2 > map.Size.z)
      cell.z = map.Size.z - num2;
    return cell;
  }

  public static void PlayOneShotOnVehicle<T>(
    this VehiclePawn vehicle,
    VehicleSoundEventEntry<T> soundEventEntry)
  {
    if (!((Thing) vehicle).Spawned)
      return;
    SoundStarter.PlayOneShot(soundEventEntry.value, SoundInfo.op_Implicit((Thing) vehicle));
  }

  public static void StartSustainerOnVehicle<T>(
    this VehiclePawn vehicle,
    VehicleSustainerEventEntry<T> soundEventEntry)
  {
    if (((Thing) vehicle).Spawned)
    {
      vehicle.sustainers.Spawn(vehicle, soundEventEntry.value);
    }
    else
    {
      if (vehicle.SustainerTarget == null)
        return;
      vehicle.sustainers.Spawn(vehicle.SustainerTarget, soundEventEntry.value);
    }
  }

  public static void StopSustainerOnVehicle<T>(
    this VehiclePawn vehicle,
    VehicleSustainerEventEntry<T> soundEventEntry)
  {
    vehicle.sustainers.EndAll(soundEventEntry.value);
  }

  [Pure]
  public static bool DeconstructibleBy(this VehiclePawn vehicle, Faction faction)
  {
    return DebugSettings.godMode || ((Thing) vehicle).Faction == faction || AcceptanceReport.op_Implicit(((Thing) vehicle).ClaimableBy(faction));
  }

  public static void RefundMaterials(this VehiclePawn vehicle, Map map, DestroyMode mode)
  {
    float multiplier = Ext_Vehicles.RefundMaterialCount(vehicle.VehicleDef, mode);
    vehicle.RefundMaterials(map, mode, multiplier);
  }

  [Pure]
  public static float RefundMaterialCount(VehicleDef vehicleDef, DestroyMode mode)
  {
    switch ((int) mode)
    {
      case 0:
        return 0.0f;
      case 1:
        return 0.0f;
      case 2:
        return 0.25f;
      case 3:
        return 0.0f;
      case 4:
        return ((BuildableDef) vehicleDef).resourcesFractionWhenDeconstructed;
      case 5:
        return 0.5f;
      case 6:
        return 1f;
      case 7:
        return 1f;
      case 8:
        return 0.0f;
      default:
        throw new ArgumentException("Unknown destroy mode " + mode.ToString());
    }
  }

  public static void RefundMaterials(
    this VehiclePawn vehicle,
    Map map,
    DestroyMode mode,
    float multiplier)
  {
    ThingOwner<Thing> container = new ThingOwner<Thing>();
    foreach (ThingDefCountClass thingDefCountClass in CostListCalculator.CostListAdjusted((BuildableDef) vehicle.VehicleDef.buildDef, ((Thing) vehicle).Stuff, true))
    {
      if (thingDefCountClass.thingDef != ThingDefOf.ReinforcedBarrel || Find.Storyteller.difficulty.classicMortars)
      {
        if (mode == 2 && ((Thing) vehicle).def.killedLeavings != null)
        {
          foreach (ThingDefCountClass killedLeaving in ((Thing) vehicle).def.killedLeavings)
          {
            Thing thing = ThingMaker.MakeThing(killedLeaving.thingDef, (ThingDef) null);
            thing.stackCount = killedLeaving.count;
            ((ThingOwner) container).TryAdd(thing, true);
          }
        }
        int num1 = GenMath.RoundRandom(multiplier * (float) thingDefCountClass.count);
        if (num1 > 0 && mode == 2 && thingDefCountClass.thingDef.slagDef != null)
        {
          int count = thingDefCountClass.thingDef.slagDef.smeltProducts.First<ThingDefCountClass>((Func<ThingDefCountClass, bool>) (sp => sp.thingDef == ThingDefOf.Steel)).count;
          int num2 = Mathf.Min(num1 / count, ((IntVec2) ref ((Thing) vehicle).def.size).Area / 2);
          for (int index = 0; index < num2; ++index)
            ((ThingOwner) container).TryAdd(ThingMaker.MakeThing(thingDefCountClass.thingDef.slagDef, (ThingDef) null), true);
          num1 -= num2 * count;
        }
        if (num1 > 0)
        {
          Thing thing = ThingMaker.MakeThing(thingDefCountClass.thingDef, (ThingDef) null);
          thing.stackCount = num1;
          ((ThingOwner) container).TryAdd(thing, true);
        }
      }
    }
    for (int index = ((ThingOwner) vehicle.inventory.innerContainer).Count - 1; index >= 0; --index)
    {
      Thing thing = vehicle.inventory.innerContainer[index];
      ((ThingOwner) container).TryAddOrTransfer(thing, true);
    }
    foreach (ThingComp allComp in ((ThingWithComps) vehicle).AllComps)
    {
      if (allComp is IRefundable refundable)
      {
        foreach ((ThingDef thingDef, float count) in refundable.Refunds)
        {
          if (thingDef != null)
          {
            Thing thing = ThingMaker.MakeThing(thingDef, (ThingDef) null);
            thing.stackCount = GenMath.RoundRandom(count * multiplier);
            ((ThingOwner) container).TryAdd(thing, true);
          }
        }
      }
    }
    ((ThingOwner) container).TryDropAllOutsideVehicle(map, GenAdj.OccupiedRect((Thing) vehicle), (DestroyMode) 7);
  }

  public static bool TryDropOutsideVehicle(
    this ThingOwner container,
    Thing thing,
    Map map,
    CellRect cellRect,
    DestroyMode mode = 7)
  {
    IntVec3 intVec3 = GenCollection.RandomElement<IntVec3>(((CellRect) ref cellRect).EdgeCells);
    if (mode == 2 && !((Area) map.areaManager.Home)[intVec3])
      ForbidUtility.SetForbidden(thing, true, false);
    Thing thing1;
    return container.TryDrop(thing, (ThingPlaceMode) 1, thing.stackCount, ref thing1, (Action<Thing, int>) null, new Predicate<IntVec3>(CanPlaceAt));

    bool CanPlaceAt(IntVec3 canPlaceAtCell)
    {
      return GenGrid.InBounds(canPlaceAtCell, map) && map.thingGrid.ThingAt<VehiclePawn>(canPlaceAtCell) == null && map.pathing.Normal.pathGrid.WalkableFast(canPlaceAtCell);
    }
  }

  public static bool TryDropAllOutsideVehicle(
    this ThingOwner container,
    Map map,
    CellRect cellRect,
    DestroyMode mode = 7)
  {
    RotatingList<IntVec3> rotatingList = GenCollection.InRandomOrder<IntVec3>(((CellRect) ref cellRect).EdgeCells, (IList<IntVec3>) null).ToRotatingList<IntVec3>();
    while (container.Count > 0)
    {
      IntVec3 next = rotatingList.Next;
      if (mode == 2 && !((Area) map.areaManager.Home)[next])
        ForbidUtility.SetForbidden(container[0], true, false);
      Thing thing;
      if (!container.TryDrop(container[0], next, map, (ThingPlaceMode) 1, ref thing, (Action<Thing, int>) null, new Predicate<IntVec3>(CanPlaceAt), true))
      {
        Log.Warning($"Failing to drop all from container {container.Owner}");
        return false;
      }
    }
    return true;

    bool CanPlaceAt(IntVec3 cell)
    {
      return GenGrid.InBounds(cell, map) && map.thingGrid.ThingAt<VehiclePawn>(cell) == null && map.pathing.Normal.pathGrid.WalkableFast(cell);
    }
  }

  [Pure]
  public static bool InAerialVehicle(this Pawn pawn) => pawn.GetAerialVehicle() != null;

  [Pure]
  public static AerialVehicleInFlight GetAerialVehicle(this Pawn pawn)
  {
    if (Find.World.GetComponent<VehicleWorldObjectsHolder>()?.AerialVehicles == null)
      return (AerialVehicleInFlight) null;
    foreach (AerialVehicleInFlight aerialVehicle in Find.World.GetComponent<VehicleWorldObjectsHolder>().AerialVehicles)
    {
      if (aerialVehicle.Vehicle == pawn || aerialVehicle.Vehicle.AllPawnsAboard.Contains(pawn))
        return aerialVehicle;
    }
    return (AerialVehicleInFlight) null;
  }

  [MustUseReturnValue]
  public static List<VehicleDef> UniqueVehicleDefs(this IEnumerable<VehiclePawn> vehicles)
  {
    HashSet<VehicleDef> set;
    using (GlobalObjectPool.Get<VehicleDef>(out set))
    {
      List<VehicleDef> vehicleDefList = new List<VehicleDef>();
      foreach (VehiclePawn vehicle in vehicles)
      {
        if (set.Add(vehicle.VehicleDef))
          vehicleDefList.Add(vehicle.VehicleDef);
      }
      return vehicleDefList;
    }
  }

  [MustUseReturnValue]
  public static List<VehicleDef> UniqueVehicleDefsInList(this List<VehiclePawn> vehicles)
  {
    HashSet<VehicleDef> set;
    using (GlobalObjectPool.Get<VehicleDef>(out set))
    {
      List<VehicleDef> vehicleDefList = new List<VehicleDef>();
      foreach (VehiclePawn vehicle in vehicles)
      {
        if (set.Add(vehicle.VehicleDef))
          vehicleDefList.Add(vehicle.VehicleDef);
      }
      return vehicleDefList;
    }
  }

  [MustUseReturnValue]
  public static List<VehicleDef> UniqueVehicleDefsInList(this List<Pawn> pawns)
  {
    HashSet<VehicleDef> set;
    using (GlobalObjectPool.Get<VehicleDef>(out set))
    {
      List<VehicleDef> vehicleDefList = new List<VehicleDef>();
      foreach (Pawn pawn in pawns)
      {
        if (pawn is VehiclePawn vehiclePawn && set.Add(vehiclePawn.VehicleDef))
          vehicleDefList.Add(vehiclePawn.VehicleDef);
      }
      return vehicleDefList;
    }
  }

  [Pure]
  public static bool IsBoat(this Thing thing)
  {
    return thing is VehiclePawn vehiclePawn && vehiclePawn.VehicleDef.type == VehicleType.Sea;
  }

  [Pure]
  public static bool HasVehicle(this List<Pawn> pawns)
  {
    return pawns.Exists((Predicate<Pawn>) (pawn => pawn is VehiclePawn));
  }

  [Pure]
  public static bool HasBoat(this List<Pawn> pawns)
  {
    return pawns.Exists((Predicate<Pawn>) (pawn => ((Thing) pawn).IsBoat()));
  }

  [Pure]
  public static bool IsFormingVehicleCaravan(this Pawn pawn)
  {
    return LordUtility.GetLord(pawn)?.LordJob is LordJob_FormAndSendVehicles;
  }

  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool InVehicleCaravan(this Pawn pawn) => pawn.GetVehicleCaravan() != null;

  [Pure]
  public static VehicleCaravan GetVehicleCaravan(this Pawn pawn)
  {
    IThingHolder parentHolder = ((Thing) pawn).ParentHolder;
    while (true)
    {
      VehiclePawn vehicle = parentHolder.GetVehicle();
      if (vehicle != null)
        parentHolder = ((Thing) vehicle).ParentHolder;
      else
        break;
    }
    return parentHolder as VehicleCaravan;
  }

  [Pure]
  public static bool CoastalTravel(this VehicleDef vehicleDef, PlanetTile tile)
  {
    float num1;
    if (vehicleDef.properties.customBiomeCosts.TryGetValue(BiomeDefOf.Ocean, out num1) && (double) num1 < 1000.0)
    {
      WorldGrid worldGrid = Find.WorldGrid;
      List<PlanetTile> planetTileList = new List<PlanetTile>();
      worldGrid.GetTileNeighbors(tile, planetTileList);
      foreach (PlanetTile planetTile in planetTileList)
      {
        int num2 = PlanetTile.op_Implicit(planetTile);
        if (((Tile) worldGrid[num2]).PrimaryBiome == BiomeDefOf.Ocean)
          return true;
      }
    }
    return false;
  }

  [MustUseReturnValue]
  public static bool Drivable(this VehiclePawn vehicle, IntVec3 cell)
  {
    return GenGrid.InBounds(cell, ((Thing) vehicle).Map) && vehicle.DrivableFast(cell);
  }

  [MustUseReturnValue]
  public static bool DrivableFast(this VehiclePawn vehicle, int x, int z)
  {
    return vehicle.DrivableFast(((CellIndices) ref ((Thing) vehicle).Map.cellIndices).CellToIndex(x, z));
  }

  [MustUseReturnValue]
  public static bool DrivableFast(this VehiclePawn vehicle, IntVec3 cell)
  {
    int index = ((CellIndices) ref ((Thing) vehicle).Map.cellIndices).CellToIndex(cell);
    return vehicle.DrivableFast(index);
  }

  [MustUseReturnValue]
  public static bool DrivableFast(this VehiclePawn vehicle, int index)
  {
    VehiclePawn vehiclePawn = ((Thing) vehicle).Map.GetDetachedMapComponent<VehiclePositionManager>().ClaimedBy(((CellIndices) ref ((Thing) vehicle).Map.cellIndices).IndexToCell(index));
    return (vehiclePawn == null || vehiclePawn == vehicle) && ((Thing) vehicle).Map.GetCachedMapComponent<VehiclePathingSystem>()[vehicle.VehicleDef].VehiclePathGrid.WalkableFast(index);
  }

  [Pure]
  public static bool LocationRestrictedBySize(
    this VehiclePawn vehicle,
    Map map,
    IntVec3 dest,
    Rot8 rot)
  {
    CellRect cellRect = vehicle.VehicleRect(dest, (Rot4) rot);
    foreach (IntVec3 cell in cellRect)
    {
      if (!cell.Walkable(vehicle.VehicleDef, map))
        return true;
    }
    return false;
  }

  [Pure]
  public static CellRect MinRect(this VehiclePawn vehicle, IntVec3 cell)
  {
    int num = Mathf.Min(((BuildableDef) vehicle.VehicleDef).Size.x, ((BuildableDef) vehicle.VehicleDef).Size.z);
    return CellRect.CenteredOn(cell, Mathf.FloorToInt((float) num / 2f));
  }

  [Pure]
  public static CellRect MaxRect(this VehiclePawn vehicle, IntVec3 cell)
  {
    int num = Mathf.Max(((BuildableDef) vehicle.VehicleDef).Size.x, ((BuildableDef) vehicle.VehicleDef).Size.z);
    return CellRect.CenteredOn(cell, Mathf.FloorToInt((float) num / 2f));
  }

  [Pure]
  public static bool DrivableRectOnCell(
    this VehiclePawn vehicle,
    IntVec3 cell,
    Ext_Vehicles.DestinationHitboxReq hitboxReq = Ext_Vehicles.DestinationHitboxReq.MinSize)
  {
    if (hitboxReq == Ext_Vehicles.DestinationHitboxReq.MinSize)
    {
      CellRect cellRect = vehicle.MinRect(cell);
      return ((CellRect) ref cellRect).Cells.All<IntVec3>(new Func<IntVec3, bool>(((Ext_Vehicles) vehicle).Drivable));
    }
    bool flag = DrivableRect(vehicle, cell, Rot8.North);
    return hitboxReq == Ext_Vehicles.DestinationHitboxReq.AnyRotation ? flag || DrivableRect(vehicle, cell, Rot8.East) : flag && DrivableRect(vehicle, cell, Rot8.East);

    static bool DrivableRect(VehiclePawn vehicle, IntVec3 cell, Rot8 rot)
    {
      CellRect cellRect = vehicle.VehicleRect(cell, (Rot4) rot);
      foreach (IntVec3 cell1 in cellRect)
      {
        if (!vehicle.Drivable(cell1))
          return false;
      }
      return true;
    }
  }

  [Pure]
  public static bool FitsOnCell(this VehiclePawn vehicle, IntVec3 cell)
  {
    int num = Mathf.Min(((BuildableDef) vehicle.VehicleDef).Size.x, ((BuildableDef) vehicle.VehicleDef).Size.z);
    CellRect cellRect = CellRect.CenteredOn(cell, Mathf.FloorToInt((float) num / 2f));
    return ((CellRect) ref cellRect).Cells.All<IntVec3>((Func<IntVec3, bool>) (cellRectCell => cellRectCell.Walkable(vehicle.VehicleDef, ((Thing) vehicle).Map)));
  }

  [Pure]
  public static bool CellRectStandable(this VehiclePawn vehicle, Map map, IntVec3? c = null, Rot4? rot = null)
  {
    IntVec3 center = c ?? ((Thing) vehicle).Position;
    Rot4 rot1 = rot ?? ((Thing) vehicle).Rotation;
    CellRect cellRect = vehicle.VehicleDef.VehicleRect(center, rot1);
    foreach (IntVec3 cell in cellRect)
    {
      if (!cell.Standable(vehicle, map))
        return false;
    }
    return true;
  }

  [Pure]
  public static bool CellRectStandable(
    this VehicleDef vehicleDef,
    Map map,
    IntVec3 position,
    Rot4 rot)
  {
    CellRect cellRect = vehicleDef.VehicleRect(position, rot);
    foreach (IntVec3 cell in cellRect)
    {
      if (!cell.Standable(vehicleDef, map))
        return false;
    }
    return true;
  }

  [Pure]
  [Obsolete("Use VehicleDef->FullRectWalkable extension method instead.", true)]
  public static bool WidthStandable(this VehicleDef vehicleDef, Map map, IntVec3 cell)
  {
    CellRect cellRect = CellRect.CenteredOn(cell, vehicleDef.SizePadding);
    foreach (IntVec3 cell1 in cellRect)
    {
      if (!cell1.Walkable(vehicleDef, map))
        return false;
    }
    return true;
  }

  [Pure]
  public static bool FullRectWalkable(
    this VehicleDef vehicleDef,
    VehiclePathingSystem pathing,
    IntVec3 cell,
    Rot4 rot)
  {
    VehiclePathingSystem.VehiclePathData vehiclePathData = pathing[vehicleDef];
    CellRect cellRect = vehicleDef.VehicleRect(cell, rot);
    foreach (IntVec3 loc in cellRect)
    {
      if (!vehiclePathData.VehiclePathGrid.Walkable(loc))
        return false;
    }
    return true;
  }

  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int CountAssignedToVehicle(this VehiclePawn vehicle)
  {
    return CaravanHelper.assignedSeats.GetAssignments(vehicle).Count;
  }

  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static VehiclePawn GetVehicle(this Pawn pawn) => ((Thing) pawn).ParentHolder.GetVehicle();

  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static VehiclePawn GetVehicle(this IThingHolder thingHolder)
  {
    return (thingHolder is VehicleRoleHandler vehicleRoleHandler ? vehicleRoleHandler.vehicle : (VehiclePawn) null) ?? (thingHolder is Pawn_InventoryTracker inventoryTracker ? inventoryTracker.pawn : (Pawn) null) as VehiclePawn;
  }

  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool InVehicle(this Pawn pawn)
  {
    bool flag;
    switch (((Thing) pawn).ParentHolder)
    {
      case VehicleRoleHandler _:
label_2:
        flag = true;
        break;
      case Pawn_InventoryTracker inventoryTracker:
        if (!(inventoryTracker.pawn is VehiclePawn))
          goto default;
        goto label_2;
      default:
        flag = false;
        break;
    }
    return flag;
  }

  [Pure]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool InVehicle(this Thing thing)
  {
    bool flag;
    switch (thing.ParentHolder)
    {
      case VehicleRoleHandler _:
label_2:
        flag = true;
        break;
      case Pawn_InventoryTracker inventoryTracker:
        if (!(inventoryTracker.pawn is VehiclePawn))
          goto default;
        goto label_2;
      default:
        flag = false;
        break;
    }
    return flag;
  }

  [MustUseReturnValue]
  public static float GetStatValueAbstract(this VehicleDef vehicleDef, VehicleStatDef statDef)
  {
    return statDef.Worker.GetValueAbstract(vehicleDef);
  }

  public enum DestinationHitboxReq
  {
    MinSize,
    AnyRotation,
    AllRotations,
  }
}
