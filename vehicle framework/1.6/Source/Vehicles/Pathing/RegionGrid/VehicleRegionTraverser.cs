// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleRegionTraverser
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Threading;
using Verse;

#nullable disable
namespace Vehicles;

public static class VehicleRegionTraverser
{
  public const int WorkerCount = 8;
  private static readonly ThreadLocal<Queue<VehicleRegionTraverser.BFSWorker>> workers = new ThreadLocal<Queue<VehicleRegionTraverser.BFSWorker>>(new Func<Queue<VehicleRegionTraverser.BFSWorker>>(VehicleRegionTraverser.CreateWorkers));

  public static bool WithinRegions(
    this IntVec3 A,
    IntVec3 B,
    Map map,
    VehicleDef vehicleDef,
    int regionLookCount,
    TraverseParms traverseParams,
    RegionType traversableRegionTypes = 14)
  {
    VehicleRegion root = VehicleRegionAndRoomQuery.RegionAt(A, map, vehicleDef, traversableRegionTypes);
    if (root == null)
      return false;
    VehicleRegion regionB = VehicleRegionAndRoomQuery.RegionAt(B, map, vehicleDef, traversableRegionTypes);
    if (regionB == null)
      return false;
    if (root == regionB)
      return true;
    bool found = false;
    VehicleRegionTraverser.BreadthFirstTraverse(root, new VehicleRegionTraverser.VehicleRegionEntry(entryCondition), new VehicleRegionTraverser.VehicleRegionProcessor(regionProcessor), regionLookCount, traversableRegionTypes);
    return found;

    bool entryCondition(VehicleRegion from, VehicleRegion to) => to.Allows(traverseParams);

    bool regionProcessor(VehicleRegion region)
    {
      if (region != regionB)
        return false;
      found = true;
      return true;
    }
  }

  public static void MarkRegionsBFS(
    VehicleRegion root,
    VehicleRegionTraverser.VehicleRegionEntry entryCondition,
    int maxRegions,
    int inRadiusMark,
    RegionType traversableRegionTypes = 14)
  {
    VehicleRegionTraverser.BreadthFirstTraverse(root, entryCondition, (VehicleRegionTraverser.VehicleRegionProcessor) (region =>
    {
      region.mark = inRadiusMark;
      return false;
    }), maxRegions, traversableRegionTypes);
  }

  private static Queue<VehicleRegionTraverser.BFSWorker> CreateWorkers()
  {
    Queue<VehicleRegionTraverser.BFSWorker> workers = new Queue<VehicleRegionTraverser.BFSWorker>(8);
    for (int closedArrayPos = 0; closedArrayPos < 8; ++closedArrayPos)
      workers.Enqueue(new VehicleRegionTraverser.BFSWorker(closedArrayPos));
    return workers;
  }

  public static void BreadthFirstTraverse(
    IntVec3 start,
    Map map,
    VehicleDef vehicleDef,
    VehicleRegionTraverser.VehicleRegionEntry entryCondition,
    VehicleRegionTraverser.VehicleRegionProcessor regionProcessor,
    int maxRegions = 999999,
    RegionType traversableRegionTypes = 14)
  {
    VehicleRegion root = VehicleRegionAndRoomQuery.RegionAt(start, map, vehicleDef, traversableRegionTypes);
    if (root == null)
      return;
    VehicleRegionTraverser.BreadthFirstTraverse(root, entryCondition, regionProcessor, maxRegions, traversableRegionTypes);
  }

  public static void BreadthFirstTraverse(
    VehicleRegion root,
    VehicleRegionTraverser.VehicleRegionEntry entryCondition,
    VehicleRegionTraverser.VehicleRegionProcessor regionProcessor,
    int maxRegions = 999999,
    RegionType traversableRegionTypes = 14)
  {
    if (root == null)
      Log.Error("BFS with null root region.");
    else if (VehicleRegionTraverser.workers.Value.Count == 0)
    {
      Log.Error($"No free workers for BFS. BFS recurred deeper than {8}, or a bug has put this system in an inconsistent state.");
    }
    else
    {
      VehicleRegionTraverser.BFSWorker bfsWorker = VehicleRegionTraverser.workers.Value.Dequeue();
      try
      {
        bfsWorker.BreadthFirstTraverseWork(root, entryCondition, regionProcessor, maxRegions, traversableRegionTypes);
      }
      catch (Exception ex)
      {
        Log.Error("Exception in BreadthFirstTraverse: " + ex.ToString());
      }
      finally
      {
        bfsWorker.Clear();
        VehicleRegionTraverser.workers.Value.Enqueue(bfsWorker);
      }
    }
  }

  public static VehicleRoom FloodAndSetRooms(
    VehicleRegion region,
    Map map,
    VehicleDef vehicleDef,
    VehicleRoom existingRoom)
  {
    VehicleRoom floodingRoom = existingRoom != null ? existingRoom : VehicleRoom.MakeNew(map, vehicleDef);
    region.Room = floodingRoom;
    if (!RegionTypeUtility.AllowsMultipleRegionsPerDistrict(region.type))
      return floodingRoom;
    VehicleRegionTraverser.BreadthFirstTraverse(region, new VehicleRegionTraverser.VehicleRegionEntry(entryCondition), new VehicleRegionTraverser.VehicleRegionProcessor(regionProcessor), traversableRegionTypes: (RegionType) 15);
    return floodingRoom;

    bool entryCondition(VehicleRegion from, VehicleRegion r)
    {
      return r.type == region.type && r.Room != floodingRoom;
    }

    bool regionProcessor(VehicleRegion r)
    {
      r.Room = floodingRoom;
      return false;
    }
  }

  public static void FloodAndSetNewRegionIndex(VehicleRegion root, int newRegionGroupIndex)
  {
    root.newRegionGroupIndex = newRegionGroupIndex;
    if (!RegionTypeUtility.AllowsMultipleRegionsPerDistrict(root.type))
      return;
    VehicleRegionTraverser.BreadthFirstTraverse(root, new VehicleRegionTraverser.VehicleRegionEntry(entryCondition), new VehicleRegionTraverser.VehicleRegionProcessor(regionProcessor), traversableRegionTypes: (RegionType) 15);

    bool entryCondition(VehicleRegion from, VehicleRegion r)
    {
      return r.type == root.type && r.newRegionGroupIndex < 0;
    }

    bool regionProcessor(VehicleRegion r)
    {
      r.newRegionGroupIndex = newRegionGroupIndex;
      return false;
    }
  }

  public delegate bool VehicleRegionEntry(VehicleRegion from, VehicleRegion to);

  public delegate bool VehicleRegionProcessor(VehicleRegion reg);

  private class BFSWorker
  {
    private readonly Queue<VehicleRegion> open = new Queue<VehicleRegion>();
    private int numRegionsProcessed;
    private uint closedIndex = 1;
    private readonly int closedArrayPos;

    public BFSWorker(int closedArrayPos) => this.closedArrayPos = closedArrayPos;

    public void Clear() => this.open.Clear();

    private void QueueNewOpenRegion(VehicleRegion region)
    {
      if ((int) region.closedIndex.Value[this.closedArrayPos] == (int) this.closedIndex)
        Log.Warning("Already closed");
      this.open.Enqueue(region);
      region.closedIndex.Value[this.closedArrayPos] = this.closedIndex;
    }

    public void BreadthFirstTraverseWork(
      VehicleRegion root,
      VehicleRegionTraverser.VehicleRegionEntry entryCondition,
      VehicleRegionTraverser.VehicleRegionProcessor regionProcessor,
      int maxRegions,
      RegionType traversableRegionTypes)
    {
      if (root.type == null)
        return;
      ++this.closedIndex;
      this.open.Clear();
      this.numRegionsProcessed = 0;
      this.QueueNewOpenRegion(root);
      while (this.open.Count > 0)
      {
        VehicleRegion vehicleRegion = this.open.Dequeue();
        if (regionProcessor != null && regionProcessor(vehicleRegion))
          break;
        ++this.numRegionsProcessed;
        if (this.numRegionsProcessed >= maxRegions)
          break;
        using (ListSnapshot<VehicleRegionLink> links = vehicleRegion.Links)
        {
          foreach (VehicleRegionLink vehicleRegionLink in links)
            ProcessRegion(vehicleRegion, vehicleRegionLink.GetOtherRegion(vehicleRegion));
        }
      }

      void ProcessRegion(VehicleRegion region, VehicleRegion linkedRegion)
      {
        if (linkedRegion == null || (int) linkedRegion.closedIndex.Value[this.closedArrayPos] == (int) this.closedIndex || (linkedRegion.type & traversableRegionTypes) == null || entryCondition != null && !entryCondition(region, linkedRegion))
          return;
        this.QueueNewOpenRegion(linkedRegion);
      }
    }
  }
}
