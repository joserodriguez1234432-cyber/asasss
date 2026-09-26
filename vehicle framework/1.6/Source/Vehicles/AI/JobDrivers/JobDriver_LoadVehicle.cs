// Decompiled with JetBrains decompiler
// Type: Vehicles.JobDriver_LoadVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class JobDriver_LoadVehicle : JobDriverLoadVehicleBase
{
  private static readonly ObjectPool<JobDriver_LoadVehicle.ThingSet> SetPool = new ObjectPool<JobDriver_LoadVehicle.ThingSet>(5);
  private static readonly ObjectPool<TransferableSearch> SearchPool = new ObjectPool<TransferableSearch>(5);

  protected virtual List<TransferableOneWay> ThingsToLoad => this.Vehicle.cargoToLoad;

  protected virtual string ListerTag => "LoadVehicle";

  protected VehiclePawn Vehicle => this.Carrier as VehiclePawn;

  protected TransferableOneWay Transferable
  {
    get
    {
      TransferableOneWay transferable = TransferableUtility.TransferableMatchingDesperate(this.ToHaul, this.ThingsToLoad, (TransferAsOneMode) 1);
      if (transferable != null)
        return transferable;
      Trace.Fail("Could not find any matching transferable.");
      return (TransferableOneWay) null;
    }
  }

  protected override bool HasDuplicateOpportunity(Thing thing)
  {
    return this.Transferable.things.Contains(thing);
  }

  [UsedImplicitly]
  [Obsolete("Deprecated. Call ShouldFailJob instead.", true)]
  protected bool FailJob() => this.ShouldFailJob();

  protected override bool ShouldFailJob()
  {
    return MassUtility.IsOverEncumbered((Pawn) this.Vehicle) || !MapComponentCache<VehicleReservationManager>.GetComponent(this.Map).VehicleListed(this.Vehicle, this.ListerTag);
  }

  protected override int CountLeftToTransfer()
  {
    return JobDriver_LoadVehicle.CountLeftToPack(this.pawn, this.job.def, this.Transferable);
  }

  protected override Thing FindThingToHaul()
  {
    return JobDriver_LoadVehicle.FindThingToPack(this.pawn, this.job.def, this.ThingsToLoad);
  }

  protected override Toil StartedCarryingThing()
  {
    Toil toil = ToilMaker.MakeToil(nameof (StartedCarryingThing));
    toil.initAction = new Action(this.AddToTransferable);
    toil.defaultCompleteMode = (ToilCompleteMode) 1;
    toil.atomicWithPrevious = true;
    return toil;
  }

  private void AddToTransferable()
  {
    TransferableOneWay transferable = this.Transferable;
    if (transferable.things.Contains(this.pawn.carryTracker.CarriedThing))
      return;
    transferable.things.Add(this.pawn.carryTracker.CarriedThing);
  }

  protected override bool IsUsableCarrier(Pawn carrier, bool allowColonists = true)
  {
    return !ThingUtility.DestroyedOrNull((Thing) carrier) && ((Thing) carrier).Spawned && ((Thing) carrier).Faction == ((Thing) this.pawn).Faction && !FireUtility.IsBurning((Thing) carrier) && !MassUtility.IsOverEncumbered(carrier) && (!(carrier is VehiclePawn vehiclePawn) ? 0 : (vehiclePawn.movementStatus == VehicleMovementStatus.Offline ? 1 : 0)) == 0;
  }

  public static Thing FindThingToPack(
    Pawn pawn,
    JobDef jobDef,
    List<TransferableOneWay> transferables,
    Lord lord = null)
  {
    if (GenList.NullOrEmpty<TransferableOneWay>((IList<TransferableOneWay>) transferables))
      return (Thing) null;
    JobDriver_LoadVehicle.ThingSet thingSet;
    using (JobDriver_LoadVehicle.SetPool.GetTemporary(out thingSet))
    {
      foreach (TransferableOneWay transferable in transferables)
      {
        if (JobDriver_LoadVehicle.CountLeftToPack(pawn, jobDef, transferable, lord) > 0)
        {
          foreach (Thing thing in transferable.things)
            thingSet.Add(thing);
        }
      }
      return thingSet.Count == 0 ? (Thing) null : JobDriverLoadVehicleBase.Search.FindNearestThing(pawn, new Predicate<Thing>(thingSet.IsValid));
    }
  }

  public static int CountLeftToPack(
    Pawn pawn,
    JobDef jobDef,
    TransferableOneWay transferable,
    Lord lord = null)
  {
    if (transferable == null || !((RimWorld.Transferable) transferable).HasAnyThing || ((RimWorld.Transferable) transferable).CountToTransfer <= 0)
      return 0;
    TransferableSearch jobSearcher;
    using (JobDriver_LoadVehicle.SearchPool.GetTemporary(out jobSearcher))
    {
      jobSearcher.Init(jobDef, transferable, lord);
      int num = JobDriverLoadVehicleBase.Search.CountAlreadyBeingPacked(pawn, (ISharedJobSearch) jobSearcher);
      return Mathf.Max(((RimWorld.Transferable) transferable).CountToTransfer - num, 0);
    }
  }

  private sealed class ThingSet : IPoolable
  {
    private readonly HashSet<Thing> neededThings = new HashSet<Thing>();

    public int Count => this.neededThings.Count;

    bool IPoolable.InPool { get; set; }

    public void Add(Thing thing) => this.neededThings.Add(thing);

    public bool IsValid(Thing thing) => this.neededThings.Contains(thing);

    void IPoolable.Reset() => this.neededThings.Clear();
  }
}
