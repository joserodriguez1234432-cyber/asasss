// Decompiled with JetBrains decompiler
// Type: Vehicles.JobDriver_PrepareVehicleCaravan_GatheringItems
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class JobDriver_PrepareVehicleCaravan_GatheringItems : JobDriverLoadVehicleBase
{
  private List<TransferableOneWay> ThingsToLoad
  {
    get => GatherItemsForVehicleCaravanUtility.GetCaravanTransferables(this.job.lord);
  }

  private TransferableOneWay Transferable
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

  protected override void OnThingAddedToInventory(Thing thing)
  {
    TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatchingDesperate(thing, this.ThingsToLoad, (TransferAsOneMode) 1);
    ((RimWorld.Transferable) transferableOneWay).AdjustTo(Mathf.Max(((RimWorld.Transferable) transferableOneWay).CountToTransfer - thing.stackCount, 0));
  }

  protected override bool HasDuplicateOpportunity(Thing thing)
  {
    return this.Transferable.things.Contains(thing);
  }

  protected override bool ShouldFailJob() => !this.Map.lordManager.lords.Contains(this.job.lord);

  protected override int CountLeftToTransfer()
  {
    return JobDriver_LoadVehicle.CountLeftToPack(this.pawn, this.job.def, this.Transferable, this.job.lord);
  }

  protected override Thing FindThingToHaul()
  {
    return JobDriver_LoadVehicle.FindThingToPack(this.pawn, this.job.def, this.ThingsToLoad, this.job.lord);
  }

  protected override bool IsUsableCarrier(Pawn carrier, bool allowColonists = true)
  {
    return GatherItemsForVehicleCaravanUtility.IsUsableCarrier(carrier, this.pawn, allowColonists);
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
}
