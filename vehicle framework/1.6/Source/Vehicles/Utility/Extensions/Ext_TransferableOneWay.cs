// Decompiled with JetBrains decompiler
// Type: Vehicles.Ext_TransferableOneWay
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public static class Ext_TransferableOneWay
{
  public static void AddThing(this TransferableOneWay transferable, Thing thing)
  {
    if (transferable.things.Contains(thing))
      return;
    transferable.things.Add(thing);
    ((Transferable) transferable).AdjustTo(((Transferable) transferable).CountToTransfer + thing.stackCount);
  }

  public static bool RemoveThing(this TransferableOneWay transferable, Thing thing)
  {
    if (!transferable.things.Remove(thing))
      return false;
    ((Transferable) transferable).AdjustTo(((Transferable) transferable).CountToTransfer - thing.stackCount);
    return true;
  }

  public static void AddThing(
    this List<TransferableOneWay> transferables,
    Thing thing,
    TransferAsOneMode mode = 1)
  {
    TransferableOneWay transferable = TransferableUtility.TransferableMatching<TransferableOneWay>(thing, transferables, mode);
    if (transferable == null)
    {
      transferable = new TransferableOneWay();
      transferables.Add(transferable);
    }
    transferable.AddThing(thing);
  }

  public static bool RemoveThing(this List<TransferableOneWay> transferables, Thing thing)
  {
    TransferableOneWay transferableFor = transferables.FindTransferableFor(thing);
    if (transferableFor == null || !transferableFor.RemoveThing(thing))
      return false;
    if (!((Transferable) transferableFor).HasAnyThing)
      transferables.Remove(transferableFor);
    return true;
  }

  public static TransferableOneWay FindTransferableFor(
    this List<TransferableOneWay> transferables,
    Thing thing)
  {
    return GenCollection.FirstOrFallback<TransferableOneWay>((IEnumerable<TransferableOneWay>) transferables, (Func<TransferableOneWay, bool>) (transferable => transferable != null && transferable.things.Contains(thing)), (TransferableOneWay) null);
  }
}
