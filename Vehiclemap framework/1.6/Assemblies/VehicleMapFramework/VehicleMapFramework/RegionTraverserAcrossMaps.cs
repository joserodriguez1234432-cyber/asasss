// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.RegionTraverserAcrossMaps
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public static class RegionTraverserAcrossMaps
{
  private static readonly Queue<RegionTraverserAcrossMaps.BFSWorker> freeWorkers;
  public static int NumWorkers;
  public static readonly RegionEntryPredicate PassAll;

  public static IReadOnlyList<ThingDef> EnterSpotDefs { get; }

  public static District FloodAndSetDistricts(Region root, Map map, District existingRoom)
  {
    // ISSUE: object of a compiler-generated type is created
    // ISSUE: variable of a compiler-generated type
    RegionTraverserAcrossMaps.\u003C\u003Ec__DisplayClass7_0 cDisplayClass70 = new RegionTraverserAcrossMaps.\u003C\u003Ec__DisplayClass7_0()
    {
      root = root,
      floodingDistrict = existingRoom ?? District.MakeNew(map)
    };
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    cDisplayClass70.root.District = cDisplayClass70.floodingDistrict;
    // ISSUE: reference to a compiler-generated field
    if (!RegionTypeUtility.AllowsMultipleRegionsPerDistrict(cDisplayClass70.root.type))
    {
      // ISSUE: reference to a compiler-generated field
      return cDisplayClass70.floodingDistrict;
    }
    // ISSUE: reference to a compiler-generated field
    // ISSUE: method pointer
    // ISSUE: method pointer
    RegionTraverserAcrossMaps.BreadthFirstTraverse(cDisplayClass70.root, new RegionEntryPredicate((object) cDisplayClass70, __methodptr(\u003CFloodAndSetDistricts\u003Eg__entryCondition\u007C0)), new RegionProcessor((object) cDisplayClass70, __methodptr(\u003CFloodAndSetDistricts\u003Eg__regionProcessor\u007C1)), traversableRegionTypes: (RegionType) 15);
    // ISSUE: reference to a compiler-generated field
    return cDisplayClass70.floodingDistrict;
  }

  public static void FloodAndSetNewRegionIndex(Region root, int newRegionGroupIndex)
  {
    // ISSUE: object of a compiler-generated type is created
    // ISSUE: variable of a compiler-generated type
    RegionTraverserAcrossMaps.\u003C\u003Ec__DisplayClass8_0 cDisplayClass80 = new RegionTraverserAcrossMaps.\u003C\u003Ec__DisplayClass8_0()
    {
      root = root,
      newRegionGroupIndex = newRegionGroupIndex
    };
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    cDisplayClass80.root.newRegionGroupIndex = cDisplayClass80.newRegionGroupIndex;
    // ISSUE: reference to a compiler-generated field
    if (!RegionTypeUtility.AllowsMultipleRegionsPerDistrict(cDisplayClass80.root.type))
      return;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: method pointer
    // ISSUE: method pointer
    RegionTraverserAcrossMaps.BreadthFirstTraverse(cDisplayClass80.root, new RegionEntryPredicate((object) cDisplayClass80, __methodptr(\u003CFloodAndSetNewRegionIndex\u003Eg__entryCondition\u007C0)), new RegionProcessor((object) cDisplayClass80, __methodptr(\u003CFloodAndSetNewRegionIndex\u003Eg__regionProcessor\u007C1)), traversableRegionTypes: (RegionType) 15);
  }

  public static bool WithinRegions(
    this IntVec3 A,
    IntVec3 B,
    Map map,
    int regionLookCount,
    TraverseParms traverseParams,
    RegionType traversableRegionTypes = 14)
  {
    // ISSUE: object of a compiler-generated type is created
    // ISSUE: variable of a compiler-generated type
    RegionTraverserAcrossMaps.\u003C\u003Ec__DisplayClass9_0 cDisplayClass90 = new RegionTraverserAcrossMaps.\u003C\u003Ec__DisplayClass9_0();
    // ISSUE: reference to a compiler-generated field
    cDisplayClass90.traverseParams = traverseParams;
    Region region = GridsUtility.GetRegion(A, map, traversableRegionTypes);
    if (region == null)
      return false;
    // ISSUE: reference to a compiler-generated field
    cDisplayClass90.regB = GridsUtility.GetRegion(B, map, traversableRegionTypes);
    // ISSUE: reference to a compiler-generated field
    if (cDisplayClass90.regB == null)
      return false;
    // ISSUE: reference to a compiler-generated field
    if (region == cDisplayClass90.regB)
      return true;
    // ISSUE: reference to a compiler-generated field
    cDisplayClass90.found = false;
    // ISSUE: method pointer
    // ISSUE: method pointer
    RegionTraverserAcrossMaps.BreadthFirstTraverse(region, new RegionEntryPredicate((object) cDisplayClass90, __methodptr(\u003CWithinRegions\u003Eg__entryCondition\u007C0)), new RegionProcessor((object) cDisplayClass90, __methodptr(\u003CWithinRegions\u003Eg__regionProcessor\u007C1)), regionLookCount, traversableRegionTypes);
    // ISSUE: reference to a compiler-generated field
    return cDisplayClass90.found;
  }

  public static void MarkRegionsBFS(
    Region root,
    RegionEntryPredicate entryCondition,
    int maxRegions,
    int inRadiusMark,
    RegionType traversableRegionTypes = 14)
  {
    // ISSUE: object of a compiler-generated type is created
    // ISSUE: method pointer
    RegionTraverserAcrossMaps.BreadthFirstTraverse(root, entryCondition, new RegionProcessor((object) new RegionTraverserAcrossMaps.\u003C\u003Ec__DisplayClass10_0()
    {
      inRadiusMark = inRadiusMark
    }, __methodptr(\u003CMarkRegionsBFS\u003Eb__0)), maxRegions, traversableRegionTypes);
  }

  public static bool ShouldCountRegion(Region r) => !r.IsDoorway;

  static RegionTraverserAcrossMaps()
  {
    List<ThingDef> items = new List<ThingDef>();
    items.AddRange(DefDatabase<ThingDef>.AllDefs.Where<ThingDef>((Func<ThingDef, bool>) (d => d.HasComp<CompVehicleEnterSpot>())));
    // ISSUE: object of a compiler-generated type is created
    RegionTraverserAcrossMaps.EnterSpotDefs = (IReadOnlyList<ThingDef>) new \u003C\u003Ez__ReadOnlyList<ThingDef>(items);
    RegionTraverserAcrossMaps.freeWorkers = new Queue<RegionTraverserAcrossMaps.BFSWorker>();
    RegionTraverserAcrossMaps.NumWorkers = 8;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: method pointer
    RegionTraverserAcrossMaps.PassAll = new RegionEntryPredicate((object) RegionTraverserAcrossMaps.\u003C\u003Ec.\u003C\u003E9, __methodptr(\u003C\u002Ecctor\u003Eb__12_0));
    RegionTraverserAcrossMaps.RecreateWorkers();
  }

  public static void RecreateWorkers()
  {
    RegionTraverserAcrossMaps.freeWorkers.Clear();
    for (int index = 0; index < RegionTraverserAcrossMaps.NumWorkers; ++index)
      RegionTraverserAcrossMaps.freeWorkers.Enqueue(new RegionTraverserAcrossMaps.BFSWorker());
  }

  public static void BreadthFirstTraverse(
    IntVec3 start,
    Map map,
    RegionEntryPredicate entryCondition,
    RegionProcessor regionProcessor,
    int maxRegions = 999999,
    RegionType traversableRegionTypes = 14)
  {
    Region region = GridsUtility.GetRegion(start, map, traversableRegionTypes);
    if (region == null)
      return;
    RegionTraverserAcrossMaps.BreadthFirstTraverse(region, entryCondition, regionProcessor, maxRegions, traversableRegionTypes);
  }

  public static void BreadthFirstTraverse(
    Region root,
    RegionProcessorDelegateCache processor,
    int maxRegions = 999999,
    RegionType traversableRegionTypes = 14)
  {
    RegionTraverserAcrossMaps.BreadthFirstTraverse(root, processor.RegionEntryPredicateDelegate, processor.RegionProcessorDelegate, maxRegions, traversableRegionTypes);
  }

  public static void BreadthFirstTraverse(
    Region root,
    RegionEntryPredicate entryCondition,
    RegionProcessor regionProcessor,
    int maxRegions = 999999,
    RegionType traversableRegionTypes = 14)
  {
    if (RegionTraverserAcrossMaps.freeWorkers.Count == 0)
      Log.Error($"No free workers for breadth-first traversal. Either BFS recurred deeper than {RegionTraverserAcrossMaps.NumWorkers.ToString()}, or a bug has put this system in an inconsistent state. Resetting.");
    else if (root == null)
    {
      Log.Error("BreadthFirstTraverse with null root region.");
    }
    else
    {
      RegionTraverserAcrossMaps.BFSWorker bfsWorker = RegionTraverserAcrossMaps.freeWorkers.Dequeue();
      try
      {
        bfsWorker.BreadthFirstTraverseWork(root, entryCondition, regionProcessor, maxRegions, traversableRegionTypes);
      }
      catch (Exception ex)
      {
        Log.Error("Exception in BreadthFirstTraverse: " + ex?.ToString());
      }
      finally
      {
        bfsWorker.Clear();
        RegionTraverserAcrossMaps.freeWorkers.Enqueue(bfsWorker);
      }
    }
  }

  private class BFSWorker
  {
    private readonly Queue<Region> open = new Queue<Region>();
    private readonly HashSet<Region> close = new HashSet<Region>();
    private int numRegionsProcessed;
    private const int skippableRegionSize = 4;

    public void Clear()
    {
      this.open.Clear();
      this.close.Clear();
    }

    private void QueueNewOpenRegion(Region region)
    {
      this.open.Enqueue(region);
      this.close.Add(region);
    }

    private void FinalizeSearch()
    {
    }

    public void BreadthFirstTraverseWork(
      Region root,
      RegionEntryPredicate entryCondition,
      RegionProcessor regionProcessor,
      int maxRegions,
      RegionType traversableRegionTypes)
    {
      if ((root.type & traversableRegionTypes) == null)
        return;
      this.Clear();
      this.numRegionsProcessed = 0;
      this.QueueNewOpenRegion(root);
      while (this.open.Count > 0)
      {
        Region region1 = this.open.Dequeue();
        if (DebugViewSettings.drawRegionTraversal)
          region1.Debug_Notify_Traversed();
        if (regionProcessor != null && regionProcessor.Invoke(region1))
        {
          this.FinalizeSearch();
          return;
        }
        if (RegionTraverserAcrossMaps.ShouldCountRegion(region1))
          ++this.numRegionsProcessed;
        if (this.numRegionsProcessed >= maxRegions)
        {
          this.FinalizeSearch();
          return;
        }
        foreach (Thing thing in region1.ListerThings.ThingsInGroup((ThingRequestGroup) 12))
        {
          if (thing is VehiclePawnWithMap vehiclePawnWithMap)
          {
            foreach (District allDistrict in vehiclePawnWithMap.VehicleMap.regionGrid.allDistricts)
            {
              bool flag = false;
              foreach (Region region2 in allDistrict.Regions)
              {
                if (ValidateRegion(region1, region2))
                {
                  this.QueueNewOpenRegion(region2);
                  flag = true;
                  break;
                }
              }
              if (flag)
                break;
            }
          }
        }
        for (int index1 = 0; index1 < region1.links.Count; ++index1)
        {
          RegionLink link = region1.links[index1];
          for (int index2 = 0; index2 < 2; ++index2)
          {
            Region region3 = link.regions[index2];
            if (ValidateRegion(region1, region3))
              this.QueueNewOpenRegion(region3);
          }
        }
        VehiclePawnWithMap vehicle;
        if (region1.Map.IsVehicleMapOf(out vehicle))
        {
          if (((Thing) vehicle).Spawned)
          {
            Region region4 = GridsUtility.GetRegion(((Thing) vehicle).Position, ((Thing) vehicle).Map, traversableRegionTypes);
            if (ValidateRegion(region1, region4))
            {
              this.QueueNewOpenRegion(region4);
              continue;
            }
          }
          foreach (ThingDef enterSpotDef in (IEnumerable<ThingDef>) RegionTraverserAcrossMaps.EnterSpotDefs)
          {
            foreach (Thing thing in region1.ListerThings.ThingsOfDef(enterSpotDef))
            {
              CompVehicleEnterSpot comp = ThingCompUtility.TryGetComp<CompVehicleEnterSpot>(thing);
              if (comp != null)
              {
                TargetInfo availableAccessSpot = comp.AvailableAccessSpot;
                if (((TargetInfo) ref availableAccessSpot).IsValid)
                {
                  Region region5 = GridsUtility.GetRegion(((TargetInfo) ref availableAccessSpot).Cell, ((TargetInfo) ref availableAccessSpot).Map, (RegionType) 14);
                  if (ValidateRegion(region1, region5))
                    this.QueueNewOpenRegion(region5);
                }
              }
            }
          }
        }
      }
      this.FinalizeSearch();

      bool ValidateRegion(Region from, Region to)
      {
        if (to == null || this.close.Contains(to) || (to.type & traversableRegionTypes) == null)
          return false;
        return entryCondition == null || entryCondition.Invoke(from, to);
      }
    }
  }
}
