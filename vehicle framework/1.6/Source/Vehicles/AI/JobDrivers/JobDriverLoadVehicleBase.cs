// Decompiled with JetBrains decompiler
// Type: Vehicles.JobDriverLoadVehicleBase
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

[PublicAPI]
public abstract class JobDriverLoadVehicleBase : JobDriver
{
  private const int WaitTicks = 25;
  private const int MaxTicksGatherItems = 7500;
  private const int LoopBackstop = 500;
  internal static readonly AccessTools.FieldRef<Pawn_InventoryTracker, List<Thing>> UnpackedCaravanItems = (AccessTools.FieldRef<Pawn_InventoryTracker, List<Thing>>) AccessTools.FieldRefAccess<List<Thing>>(typeof (Pawn_InventoryTracker), "unpackedCaravanItems");
  private int toilLoops;
  private int pickedUpFirstItemTicks = -1;
  private PrepareCaravanGatherState gatherState;

  [CanBeNull]
  public Thing ToHaul
  {
    get
    {
      LocalTargetInfo target = this.job.GetTarget((TargetIndex) 1);
      return ((LocalTargetInfo) ref target).Thing;
    }
  }

  protected Pawn Carrier
  {
    get
    {
      LocalTargetInfo target = this.job.GetTarget((TargetIndex) 2);
      return ((LocalTargetInfo) ref target).Thing as Pawn;
    }
  }

  protected virtual bool ShouldHaulItems
  {
    get
    {
      return MassUtility.IsOverEncumbered(this.pawn) && !this.pawn.inventory.HasAnyUnpackedCaravanItems;
    }
  }

  protected abstract bool ShouldFailJob();

  protected abstract int CountLeftToTransfer();

  protected abstract Thing FindThingToHaul();

  protected abstract bool IsUsableCarrier(Pawn carrier, bool allowColonists = true);

  protected abstract bool HasDuplicateOpportunity(Thing thing);

  protected virtual void OnThingAddedToInventory(Thing thing)
  {
  }

  public virtual bool TryMakePreToilReservations(bool errorOnFailed)
  {
    if (!ReservationUtility.Reserve(this.pawn, LocalTargetInfo.op_Implicit(this.ToHaul), this.job, 1, -1, (ReservationLayerDef) null, errorOnFailed, false))
      return false;
    ReservationUtility.ReserveAsManyAsPossible(this.pawn, this.job.GetTargetQueue((TargetIndex) 1), this.job, 1, -1, (ReservationLayerDef) null);
    return true;
  }

  protected virtual IEnumerable<Toil> MakeNewToils()
  {
    if (this.gatherState == null)
      this.gatherState = this.ShouldHaulItems ? (PrepareCaravanGatherState) 2 : (PrepareCaravanGatherState) 1;
    this.AddFinishAction(new Action<JobCondition>(this.DumpAllUnpackedItems));
    return this.gatherState == 2 ? this.MakeNewToilsCarry() : this.MakeNewToilsHaulInInventory();
  }

  private IEnumerable<Toil> MakeNewToilsCarry()
  {
    JobDriverLoadVehicleBase driverLoadVehicleBase = this;
    ToilFailConditions.FailOn<JobDriverLoadVehicleBase>(driverLoadVehicleBase, new Func<bool>(driverLoadVehicleBase.ShouldFailJob));
    Toil reserve = ToilFailConditions.FailOnDespawnedOrNull<Toil>(Toils_Reserve.Reserve((TargetIndex) 1, 1, -1, (ReservationLayerDef) null, false), (TargetIndex) 1);
    yield return reserve;
    bool inInventory = HaulAIUtility.IsInHaulableInventory(driverLoadVehicleBase.ToHaul);
    yield return Toils_Goto.GotoThing((TargetIndex) 1, (PathEndMode) 2, inInventory);
    yield return driverLoadVehicleBase.DetermineNumToHaul();
    yield return Toils_Haul.StartCarryThing((TargetIndex) 1, false, true, false, true, inInventory);
    Toil toil = driverLoadVehicleBase.StartedCarryingThing();
    if (toil != null)
      yield return toil;
    yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserve, (TargetIndex) 1, (TargetIndex) 0, true, new Predicate<Thing>(driverLoadVehicleBase.HasDuplicateOpportunity));
    Toil findCarrier = driverLoadVehicleBase.FindCarrier();
    yield return findCarrier;
    // ISSUE: reference to a compiler-generated method
    yield return ToilJumpConditions.JumpIf(Toils_Goto.GotoThing((TargetIndex) 2, (PathEndMode) 2, false), new Func<bool>(driverLoadVehicleBase.\u003CMakeNewToilsCarry\u003Eb__21_0), findCarrier);
    // ISSUE: reference to a compiler-generated method
    yield return ToilEffects.WithProgressBarToilDelay(ToilJumpConditions.JumpIf(Toils_General.Wait(25, (TargetIndex) 0), new Func<bool>(driverLoadVehicleBase.\u003CMakeNewToilsCarry\u003Eb__21_1), findCarrier), (TargetIndex) 2, false, -0.5f);
    yield return driverLoadVehicleBase.PlaceTargetInCarrierInventory();
  }

  private IEnumerable<Toil> MakeNewToilsHaulInInventory()
  {
    JobDriverLoadVehicleBase driverLoadVehicleBase = this;
    ToilFailConditions.FailOn<JobDriverLoadVehicleBase>(driverLoadVehicleBase, new Func<bool>(driverLoadVehicleBase.ShouldFailJob));
    bool inInventory = HaulAIUtility.IsInHaulableInventory(driverLoadVehicleBase.ToHaul);
    Toil reserve = ToilFailConditions.FailOnDestroyedOrNull<Toil>(Toils_Reserve.Reserve((TargetIndex) 1, 1, -1, (ReservationLayerDef) null, false), (TargetIndex) 1);
    Toil findCarrier = driverLoadVehicleBase.FindCarrier();
    yield return reserve;
    yield return ToilJumpConditions.JumpIf(Toils_Goto.GotoThing((TargetIndex) 1, (PathEndMode) 2, inInventory), new Func<bool>(driverLoadVehicleBase.IsFinishedCollectingItems), findCarrier);
    yield return driverLoadVehicleBase.DetermineNumToHaul(findCarrier);
    yield return Toils_Haul.StartCarryThing((TargetIndex) 1, false, true, false, true, inInventory);
    Toil toil = driverLoadVehicleBase.StartedCarryingThing();
    if (toil != null)
      yield return toil;
    Toil pickUpHauledItem = driverLoadVehicleBase.HaulCaravanItemInInventory(reserve);
    yield return ToilEffects.WithProgressBarToilDelay(Toils_General.Wait(25, (TargetIndex) 0), (TargetIndex) 1, false, -0.5f);
    yield return pickUpHauledItem;
    yield return findCarrier;
    // ISSUE: reference to a compiler-generated method
    yield return ToilJumpConditions.JumpIf(Toils_Goto.GotoThing((TargetIndex) 2, (PathEndMode) 2, false), new Func<bool>(driverLoadVehicleBase.\u003CMakeNewToilsHaulInInventory\u003Eb__22_0), findCarrier);
    // ISSUE: reference to a compiler-generated method
    yield return ToilEffects.WithProgressBarToilDelay(ToilJumpConditions.JumpIf(Toils_General.Wait(25, (TargetIndex) 0), new Func<bool>(driverLoadVehicleBase.\u003CMakeNewToilsHaulInInventory\u003Eb__22_1), findCarrier), (TargetIndex) 2, false, -0.5f);
    yield return driverLoadVehicleBase.AddHauledItemsToCarrier(findCarrier);
  }

  private bool IsFinishedCollectingItems()
  {
    if (this.pawn.carryTracker.CarriedThing is Pawn || MassUtility.IsOverEncumbered(this.pawn))
      return true;
    return this.pickedUpFirstItemTicks > -1 && Find.TickManager.TicksGame > this.pickedUpFirstItemTicks + 7500;
  }

  private Toil HaulCaravanItemInInventory(Toil reserve)
  {
    Toil toil = ToilMaker.MakeToil(nameof (HaulCaravanItemInInventory));
    toil.initAction = (Action) (() =>
    {
      if (this.pickedUpFirstItemTicks == -1)
        this.pickedUpFirstItemTicks = Find.TickManager.TicksGame;
      this.pawn.records?.Increment(RecordDefOf.ThingsHauled);
      if (!(this.pawn.carryTracker.CarriedThing is Pawn))
        this.pawn.inventory.AddHauledCaravanItem(this.pawn.carryTracker.CarriedThing);
      if (this.IsFinishedCollectingItems())
        return;
      this.SetNewHaulTargetAndJumpToReserve(reserve);
    });
    return toil;
  }

  private Toil AddHauledItemsToCarrier(Toil findCarrier)
  {
    Toil carrier1 = ToilMaker.MakeToil(nameof (AddHauledItemsToCarrier));
    carrier1.initAction = (Action) (() =>
    {
      if (this.Carrier == this.pawn)
      {
        this.pawn.inventory.ClearHaulingCaravanCache();
      }
      else
      {
        TransferCaravanItemsToCarrier(this.pawn, this.Carrier);
        if (!this.pawn.inventory.HasAnyUnpackedCaravanItems || !this.CheckToilLoopBackstop())
          return;
        this.pawn.jobs.curDriver.JumpToToil(findCarrier);
      }
    });
    return carrier1;

    void TransferCaravanItemsToCarrier(Pawn pawn, Pawn carrier)
    {
      if (this.Carrier is VehiclePawn carrier2 && this.ToHaul is Pawn toHaul && (carrier2.TryAddPawn(toHaul) || !toHaul.CanBeTransferredToVehiclesCargo()))
        return;
      List<Thing> thingList = JobDriverLoadVehicleBase.UnpackedCaravanItems.Invoke(pawn.inventory);
      for (int index = thingList.Count - 1; index >= 0 && !MassUtility.IsOverEncumbered(carrier); --index)
      {
        Thing thing = thingList[index];
        this.OnThingAddedToInventory(thing);
        if (carrier2 != null)
          this.AddItemToVehicle(carrier2, thing);
        else
          ((ThingOwner) pawn.inventory.innerContainer).TryTransferToContainer(thing, (ThingOwner) carrier.inventory.innerContainer, thing.stackCount, true);
        thingList.Remove(thing);
      }
    }
  }

  private void SetNewHaulTargetAndJumpToReserve(Toil reserve)
  {
    if (!this.CheckToilLoopBackstop())
      return;
    Thing thingToHaul = this.FindThingToHaul();
    if (thingToHaul == null)
      return;
    this.job.SetTarget((TargetIndex) 1, LocalTargetInfo.op_Implicit(thingToHaul));
    this.pawn.jobs.curDriver.JumpToToil(reserve);
  }

  private bool CheckToilLoopBackstop()
  {
    if (++this.toilLoops <= 500)
      return true;
    Log.Error($"Prepare caravan gather items job for pawn {((Entity) this.pawn).Label} looped through toils too many times.");
    this.EndJobWith((JobCondition) 64 /*0x40*/);
    return false;
  }

  private Toil DetermineNumToHaul(Toil findCarrier = null)
  {
    Toil numToHaul = ToilMaker.MakeToil(nameof (DetermineNumToHaul));
    numToHaul.initAction = (Action) (() =>
    {
      int transfer = this.CountLeftToTransfer();
      if (this.pawn.carryTracker.CarriedThing != null)
        transfer -= this.pawn.carryTracker.CarriedThing.stackCount;
      if (transfer > 0)
        this.job.count = transfer;
      else if (findCarrier == null || !this.pawn.inventory.HasAnyUnpackedCaravanItems)
        this.pawn.jobs.EndCurrentJob((JobCondition) 2, true, true);
      else
        this.pawn.jobs.curDriver.JumpToToil(findCarrier);
    });
    numToHaul.defaultCompleteMode = (ToilCompleteMode) 1;
    numToHaul.atomicWithPrevious = true;
    return numToHaul;
  }

  protected virtual Toil StartedCarryingThing() => (Toil) null;

  private Toil FindCarrier()
  {
    return new Toil()
    {
      initAction = (Action) (() =>
      {
        LocalTargetInfo target = this.job.GetTarget((TargetIndex) 2);
        if (((LocalTargetInfo) ref target).Pawn != null)
          return;
        Pawn carrier;
        if (!TryGetBestCarrier(out carrier))
          this.EndJobWith((JobCondition) 4);
        else
          this.job.SetTarget((TargetIndex) 2, LocalTargetInfo.op_Implicit((Thing) carrier));
      })
    };

    bool TryGetBestCarrier(out Pawn carrier1)
    {
      carrier1 = (Pawn) this.FindBestCarrier();
      if (carrier1 == null)
        carrier1 = this.FindBestBackupCarrier(true);
      if (carrier1 != null)
        return true;
      if (this.job.lord == null)
        return false;
      bool flag = LordUtility.GetLord(this.pawn) == this.job.lord;
      if (flag && !MassUtility.IsOverEncumbered(this.pawn))
      {
        carrier1 = this.pawn;
        return true;
      }
      carrier1 = this.FindBestBackupCarrier(false);
      if (carrier1 != null)
        return true;
      if (flag)
      {
        carrier1 = this.pawn;
        return true;
      }
      List<Pawn> list = this.job.lord.ownedPawns.Where<Pawn>((Func<Pawn, bool>) (carrier2 => this.IsUsableCarrier(carrier2))).ToList<Pawn>();
      carrier1 = GenCollection.RandomElementWithFallback<Pawn>((IEnumerable<Pawn>) list, (Pawn) null);
      return carrier1 != null;
    }
  }

  private Toil PlaceTargetInCarrierInventory()
  {
    Toil toil = ToilMaker.MakeToil(nameof (PlaceTargetInCarrierInventory));
    toil.initAction = (Action) (() =>
    {
      if (this.ToHaul == null || this.ToHaul.stackCount == 0)
      {
        this.pawn.jobs.EndCurrentJob((JobCondition) 4, true, true);
      }
      else
      {
        Pawn_CarryTracker carryTracker = this.pawn.carryTracker;
        Thing carriedThing = carryTracker.CarriedThing;
        if (((ThingOwner) carryTracker.innerContainer).Count == 0)
          carryTracker.pawn.Drawer.renderer.SetAllGraphicsDirty();
        this.OnThingAddedToInventory(carriedThing);
        Thing thing;
        if (this.Carrier is VehiclePawn carrier2)
        {
          if (carriedThing is Pawn pawn2 && (carrier2.TryAddPawn(pawn2) || !pawn2.CanBeTransferredToVehiclesCargo()))
            return;
          thing = this.AddItemToVehicle(carrier2, carriedThing) ? carriedThing : (Thing) null;
        }
        else
          carryTracker.innerContainer.TryTransferToContainer(carriedThing, (ThingOwner) this.Carrier.inventory.innerContainer, carriedThing.stackCount, ref thing, true);
        CompForbiddable comp = thing != null ? ThingCompUtility.TryGetComp<CompForbiddable>(thing) : (CompForbiddable) null;
        if (comp == null)
          return;
        comp.Forbidden = false;
      }
    });
    return toil;
  }

  protected virtual bool AddItemToVehicle(VehiclePawn vehicle, Thing thing)
  {
    return vehicle.AddOrTransfer(thing, thing.stackCount) > 0;
  }

  private void DumpAllUnpackedItems(JobCondition condition)
  {
    Pawn_InventoryTracker inventory = this.pawn.inventory;
    if (inventory == null || !inventory.HasAnyUnpackedCaravanItems)
      return;
    this.pawn.inventory.DropAllPackingCaravanThings();
  }

  private VehiclePawn FindBestCarrier()
  {
    if (this.job.lord == null)
      return (VehiclePawn) null;
    float num = 0.0f;
    VehiclePawn bestCarrier = (VehiclePawn) null;
    foreach (Pawn ownedPawn in this.job.lord.ownedPawns)
    {
      if (ownedPawn != this.pawn && ownedPawn is VehiclePawn carrier && this.IsUsableCarrier((Pawn) carrier))
      {
        float carrierScore = JobDriverLoadVehicleBase.GetCarrierScore(ownedPawn, this.pawn);
        if (bestCarrier == null || (double) carrierScore > (double) num)
        {
          bestCarrier = carrier;
          num = carrierScore;
        }
      }
    }
    return bestCarrier;
  }

  private Pawn FindBestBackupCarrier(bool onlyAnimals)
  {
    if (this.job.lord == null)
      return (Pawn) null;
    float num = 0.0f;
    Pawn bestBackupCarrier = (Pawn) null;
    foreach (Pawn ownedPawn in this.job.lord.ownedPawns)
    {
      if (ownedPawn != this.pawn && (!onlyAnimals || ownedPawn.RaceProps.Animal) && this.IsUsableCarrier(ownedPawn, false))
      {
        float carrierScore = JobDriverLoadVehicleBase.GetCarrierScore(ownedPawn, this.pawn);
        if (bestBackupCarrier == null || (double) carrierScore > (double) num)
        {
          bestBackupCarrier = ownedPawn;
          num = carrierScore;
        }
      }
    }
    return bestBackupCarrier;
  }

  public virtual void ExposeData()
  {
    base.ExposeData();
    Scribe_Values.Look<int>(ref this.pickedUpFirstItemTicks, "pickedUpFirstItemTicks", -1, false);
    Scribe_Values.Look<int>(ref this.toilLoops, "toilLoops", 0, false);
    Scribe_Values.Look<PrepareCaravanGatherState>(ref this.gatherState, "gatherState", (PrepareCaravanGatherState) 0, false);
  }

  private static float GetCarrierScore(Pawn carrier, Pawn hauler)
  {
    double num1 = (double) (1f - MassUtility.EncumbrancePercent(carrier));
    IntVec3 intVec3 = IntVec3.op_Subtraction(((Thing) carrier).Position, ((Thing) hauler).Position);
    double num2 = (double) ((IntVec3) ref intVec3).LengthHorizontal / 10.0 * 0.20000000298023224;
    return (float) (num1 - num2);
  }

  private static int CountBeingCarried(Pawn pawn, ISharedJobSearch searcher)
  {
    int num = 0;
    foreach (Thing thing in JobDriverLoadVehicleBase.UnpackedCaravanItems.Invoke(pawn.inventory))
      num += searcher.IsMatchingThing(thing) ? thing.stackCount : 0;
    return num;
  }

  public static class Search
  {
    [Profile]
    public static Thing FindNearestThing(Pawn pawn, Predicate<Thing> extraValidator)
    {
      return ClosestHaulable(pawn, (ThingRequestGroup) 12, new Predicate<Thing>(ValidThing)) ?? ClosestHaulable(pawn, (ThingRequestGroup) 3, new Predicate<Thing>(ValidThing));

      static Thing ClosestHaulable(
        Pawn pawn,
        ThingRequestGroup thingRequestGroup,
        Predicate<Thing> validator)
      {
        return GenClosest.ClosestThingReachable(((Thing) pawn).Position, ((Thing) pawn).Map, ThingRequest.ForGroup(thingRequestGroup), (PathEndMode) 2, TraverseParms.For(pawn, (Danger) 3, (TraverseMode) 0, false, false, false, true), 9999f, validator, (IEnumerable<Thing>) null, 0, -1, false, (RegionType) 14, false, false);
      }

      bool ValidThing(Thing thing)
      {
        return extraValidator(thing) && ReservationUtility.CanReserve(pawn, LocalTargetInfo.op_Implicit(thing), 1, -1, (ReservationLayerDef) null, false) && !ForbidUtility.IsForbidden(thing, ((Thing) pawn).Faction);
      }
    }

    [Profile]
    public static int CountAlreadyBeingPacked(Pawn pawn, ISharedJobSearch jobSearcher)
    {
      int num1 = 0;
      if (ModsConfig.BiotechActive)
        num1 = JobDriverLoadVehicleBase.Search.HauledByOthers(pawn, jobSearcher, ((Thing) pawn).Map.mapPawns.SpawnedColonyMechs);
      int num2 = 0;
      if (ModsConfig.IdeologyActive)
        num2 = JobDriverLoadVehicleBase.Search.HauledByOthers(pawn, jobSearcher, ((Thing) pawn).Map.mapPawns.SlavesOfColonySpawned);
      int num3 = JobDriverLoadVehicleBase.Search.HauledByOthers(pawn, jobSearcher, ((Thing) pawn).Map.mapPawns.FreeColonistsSpawned);
      int num4 = 0;
      foreach (Thing thing in JobDriverLoadVehicleBase.UnpackedCaravanItems.Invoke(pawn.inventory))
        num4 += thing.def == jobSearcher.ThingDef ? thing.stackCount : 0;
      return num1 + num2 + num4 + num3;
    }

    private static int HauledByOthers(Pawn pawn, ISharedJobSearch jobSearcher, List<Pawn> pawns)
    {
      int num = 0;
      foreach (Pawn pawn1 in pawns)
        num += JobDriverLoadVehicleBase.Search.CountFromJob(pawn, pawn1, jobSearcher);
      return num;
    }

    private static int CountFromJob(Pawn pawn, Pawn otherPawn, ISharedJobSearch jobSearcher)
    {
      if (pawn == otherPawn || otherPawn.CurJob == null || !jobSearcher.ShouldConsiderPawn(otherPawn) || !(otherPawn.jobs.curDriver is JobDriverLoadVehicleBase curDriver))
        return 0;
      Thing toHaul = curDriver.ToHaul;
      return (toHaul != null ? toHaul.stackCount : 0) + JobDriverLoadVehicleBase.CountBeingCarried(otherPawn, jobSearcher);
    }
  }
}
