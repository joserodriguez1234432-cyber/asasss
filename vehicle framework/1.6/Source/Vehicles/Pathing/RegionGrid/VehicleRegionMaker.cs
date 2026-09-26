// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleRegionMaker
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using LudeonTK;
using SmashTools;
using SmashTools.Algorithms;
using SmashTools.Performance;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[StaticConstructorOnStartup]
public class VehicleRegionMaker : VehicleGridManager
{
  private VehicleRegionGrid regionGrid;
  private readonly HashSet<IntVec3> regionCells = new HashSet<IntVec3>();
  private readonly HashSet<IntVec3>[] linksProcessedAt = new HashSet<IntVec3>[4]
  {
    new HashSet<IntVec3>(),
    new HashSet<IntVec3>(),
    new HashSet<IntVec3>(),
    new HashSet<IntVec3>()
  };
  internal readonly ObjectPool<VehicleRegionLink> linkPool;
  internal readonly ObjectPool<VehicleRegion> regionPool;
  private readonly ConcurrentDictionary<ulong, VehicleRegionLink> activeLinks = new ConcurrentDictionary<ulong, VehicleRegionLink>();
  private readonly BFS<IntVec3> floodfiller = new BFS<IntVec3>();
  private int nextId = 1;

  public VehicleRegionMaker(VehiclePathingSystem mapping, VehicleDef createdFor)
    : base(mapping, createdFor)
  {
    int size1 = Mathf.CeilToInt((float) ((double) mapping.map.Size.x / 12.0 * ((double) mapping.map.Size.z / 12.0)) * 0.5f);
    int size2 = Mathf.CeilToInt((float) (size1 * 4));
    this.regionPool = new ObjectPool<VehicleRegion>(size1);
    this.linkPool = new ObjectPool<VehicleRegionLink>(size2);
  }

  private bool CreatingRegions { get; set; }

  public override void PostInit()
  {
    base.PostInit();
    this.regionGrid = this.mapping[this.createdFor].VehicleRegionGrid;
  }

  [Profile]
  public VehicleRegionMaker.RegionResult TryGenerateRegionFrom(
    IntVec3 root,
    ref VehicleRegion region)
  {
    RegionType expectedRegionType = VehicleRegionTypeUtility.GetExpectedRegionType(root, this.mapping, this.createdFor);
    if (expectedRegionType == null)
      return VehicleRegionMaker.RegionResult.NoRegion;
    if (this.CreatingRegions)
    {
      Log.Error("Trying to generate a new region while already in the process. Nested calls not allowed.");
      return VehicleRegionMaker.RegionResult.Failed;
    }
    using (new ClearOnDispose<IntVec3>((ICollection<IntVec3>) this.regionCells))
    {
      try
      {
        this.CreatingRegions = true;
        region = this.GetRegion(root);
        region.type = expectedRegionType;
        this.FloodFillAndAddCells(region, root);
        this.CreateLinks(region);
      }
      catch (Exception ex)
      {
        SmashLog.ErrorLabel("[VehicleFramework]", $"Exception thrown while generating region at {root}. Exception={ex}");
        region = (VehicleRegion) null;
        return VehicleRegionMaker.RegionResult.Failed;
      }
      finally
      {
        this.CreatingRegions = false;
      }
      return VehicleRegionMaker.RegionResult.Success;
    }
  }

  private static IEnumerable<IntVec3> GetFloodFillNeighbors(IntVec3 root)
  {
    IntVec3[] intVec3Array = GenAdj.CardinalDirectionsAround;
    for (int index = 0; index < intVec3Array.Length; ++index)
      yield return IntVec3.op_Addition(root, intVec3Array[index]);
    intVec3Array = (IntVec3[]) null;
  }

  [Profile]
  private void FloodFillAndAddCells(VehicleRegion region, IntVec3 root)
  {
    this.regionCells.Clear();
    BFS<IntVec3> floodfiller = this.floodfiller;
    IntVec3 start = root;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    Func<IntVec3, IEnumerable<IntVec3>> neighbors = VehicleRegionMaker.\u003C\u003EO.\u003C0\u003E__GetFloodFillNeighbors ?? (VehicleRegionMaker.\u003C\u003EO.\u003C0\u003E__GetFloodFillNeighbors = new Func<IntVec3, IEnumerable<IntVec3>>(VehicleRegionMaker.GetFloodFillNeighbors));
    Func<IntVec3, bool> func = new Func<IntVec3, bool>(Validator);
    Action<IntVec3> processor = new Action<IntVec3>(Processor);
    Func<IntVec3, bool> canEnter = func;
    floodfiller.FloodFill(start, neighbors, processor, canEnter);

    bool Validator(IntVec3 cell)
    {
      return GenGrid.InBounds(cell, this.mapping.map) && ((CellRect) ref region.extentsLimit).Contains(cell) && VehicleRegionTypeUtility.GetExpectedRegionType(cell, this.mapping, this.createdFor) == region.type;
    }

    void Processor(IntVec3 cell) => this.AddCell(region, cell);
  }

  private void AddCell(VehicleRegion region, IntVec3 cell)
  {
    this.regionGrid.SetRegionAt(cell, region);
    this.regionCells.Add(cell);
    if (region.extentsClose.minX > cell.x)
      region.extentsClose.minX = cell.x;
    if (region.extentsClose.maxX < cell.x)
      region.extentsClose.maxX = cell.x;
    if (region.extentsClose.minZ > cell.z)
      region.extentsClose.minZ = cell.z;
    if (region.extentsClose.maxZ < cell.z)
      region.extentsClose.maxZ = cell.z;
    if (cell.x != this.createdFor.SizePadding && cell.x != this.mapping.map.Size.x - 1 - this.createdFor.SizePadding && cell.z != this.createdFor.SizePadding && cell.z != this.mapping.map.Size.z - 1 - this.createdFor.SizePadding)
      return;
    region.touchesMapEdge = true;
  }

  private void ClearProcessedLinks()
  {
    foreach (HashSet<IntVec3> intVec3Set in this.linksProcessedAt)
      intVec3Set.Clear();
  }

  private VehicleRegionLink LinkFrom(EdgeSpan span)
  {
    ulong key = ((EdgeSpan) ref span).UniqueHashCode();
    VehicleRegionLink vehicleRegionLink;
    if (!this.activeLinks.TryGetValue(key, out vehicleRegionLink))
    {
      vehicleRegionLink = this.linkPool.Get();
      vehicleRegionLink.SetNew(span);
      this.activeLinks.TryAdd(key, vehicleRegionLink);
    }
    return vehicleRegionLink;
  }

  [Profile]
  private void CreateLinks(VehicleRegion region)
  {
    foreach (IntVec3 regionCell in this.regionCells)
    {
      this.SweepInTwoDirectionsAndTryToCreateLink(region, Rot4.North, regionCell);
      this.SweepInTwoDirectionsAndTryToCreateLink(region, Rot4.South, regionCell);
      this.SweepInTwoDirectionsAndTryToCreateLink(region, Rot4.East, regionCell);
      this.SweepInTwoDirectionsAndTryToCreateLink(region, Rot4.West, regionCell);
    }
    this.ClearProcessedLinks();
  }

  private void SweepInTwoDirectionsAndTryToCreateLink(
    VehicleRegion region,
    Rot4 potentialOtherRegionDir,
    IntVec3 cell)
  {
    if (!((Rot4) ref potentialOtherRegionDir).IsValid)
      return;
    HashSet<IntVec3> intVec3Set = this.linksProcessedAt[((Rot4) ref potentialOtherRegionDir).AsInt];
    if (intVec3Set.Contains(cell))
      return;
    IntVec3 cell1 = IntVec3.op_Addition(cell, ((Rot4) ref potentialOtherRegionDir).FacingCell);
    if (GenGrid.InBounds(cell1, this.mapping.map) && this.regionGrid.GetRegionAt(cell1) == region)
      return;
    RegionType expectedRegionType = VehicleRegionTypeUtility.GetExpectedRegionType(cell1, this.mapping, this.createdFor);
    if (expectedRegionType == null)
      return;
    Rot4 rot4 = ((Rot4) ref potentialOtherRegionDir).Rotated((RotationDirection) 1);
    intVec3Set.Add(cell);
    int num1 = 0;
    int num2 = 0;
    if (!RegionTypeUtility.IsOneCellRegion(expectedRegionType))
    {
      for (num1 = 0; num1 <= 12; ++num1)
      {
        IntVec3 cell2 = IntVec3.op_Addition(cell, IntVec3.op_Multiply(((Rot4) ref rot4).FacingCell, num1 + 1));
        if (!this.InvalidForLinking(region, cell2, potentialOtherRegionDir, expectedRegionType))
        {
          if (!intVec3Set.Add(cell2))
            Log.Error("Attempting to process the same cell twice.");
        }
        else
          break;
      }
      for (num2 = 0; num2 <= 12; ++num2)
      {
        IntVec3 cell3 = IntVec3.op_Subtraction(cell, IntVec3.op_Multiply(((Rot4) ref rot4).FacingCell, num2 + 1));
        if (!this.InvalidForLinking(region, cell3, potentialOtherRegionDir, expectedRegionType))
        {
          if (!intVec3Set.Add(cell3))
            Log.Error("Attempting to process the same cell twice.");
        }
        else
          break;
      }
    }
    int num3 = num1 + num2 + 1;
    SpanDirection spanDirection;
    IntVec3 intVec3;
    if (Rot4.op_Equality(potentialOtherRegionDir, Rot4.North))
    {
      spanDirection = (SpanDirection) 1;
      intVec3 = IntVec3.op_Subtraction(cell, IntVec3.op_Multiply(((Rot4) ref rot4).FacingCell, num2));
      ++intVec3.z;
    }
    else if (Rot4.op_Equality(potentialOtherRegionDir, Rot4.South))
    {
      spanDirection = (SpanDirection) 1;
      intVec3 = IntVec3.op_Addition(cell, IntVec3.op_Multiply(((Rot4) ref rot4).FacingCell, num1));
    }
    else if (Rot4.op_Equality(potentialOtherRegionDir, Rot4.East))
    {
      spanDirection = (SpanDirection) 0;
      intVec3 = IntVec3.op_Addition(cell, IntVec3.op_Multiply(((Rot4) ref rot4).FacingCell, num1));
      ++intVec3.x;
    }
    else
    {
      spanDirection = (SpanDirection) 0;
      intVec3 = IntVec3.op_Subtraction(cell, IntVec3.op_Multiply(((Rot4) ref rot4).FacingCell, num2));
    }
    EdgeSpan span;
    // ISSUE: explicit constructor call
    ((EdgeSpan) ref span).\u002Ector(intVec3, spanDirection, num3);
    VehicleRegionLink regionLink = this.LinkFrom(span);
    regionLink.Register(region, potentialOtherRegionDir);
    region.AddLink(regionLink);
  }

  public void Return(VehicleRegion region) => this.regionPool.Return(region);

  public void Return(VehicleRegionLink regionLink)
  {
    this.activeLinks.TryRemove(regionLink.UniqueHashCode(), out VehicleRegionLink _);
    this.linkPool.Return(regionLink);
  }

  private VehicleRegion GetRegion(IntVec3 root)
  {
    VehicleRegion regionAt = this.regionGrid.GetRegionAt(root);
    if (regionAt == null)
      return this.CreateNew(root);
    this.regionGrid.ClearFromGrid(regionAt);
    this.SetNew(regionAt, root);
    return regionAt;
  }

  private VehicleRegion CreateNew(IntVec3 root)
  {
    VehicleRegion region = this.regionPool.Get();
    this.SetNew(region, root);
    return region;
  }

  private void SetNew(VehicleRegion region, IntVec3 root)
  {
    if (region == null)
    {
      Log.Warning("Attempting to populate null region.");
    }
    else
    {
      int regionId = this.GetRegionId();
      region.Init(this.createdFor, regionId);
      region.Map = this.mapping.map;
      VehicleRegion vehicleRegion1 = region;
      CellRect cellRect1 = new CellRect();
      cellRect1.minX = root.x;
      cellRect1.maxX = root.x;
      cellRect1.minZ = root.z;
      cellRect1.maxZ = root.z;
      CellRect cellRect2 = cellRect1;
      vehicleRegion1.extentsClose = cellRect2;
      VehicleRegion vehicleRegion2 = region;
      cellRect1 = VehicleRegion.ChunkAt(root);
      CellRect cellRect3 = ((CellRect) ref cellRect1).ClipInsideMap(this.mapping.map);
      vehicleRegion2.extentsLimit = cellRect3;
    }
  }

  private bool InvalidForLinking(
    VehicleRegion region,
    IntVec3 cell,
    Rot4 rot,
    RegionType expectedRegionType)
  {
    return !GenGrid.InBounds(cell, this.mapping.map) || this.regionGrid.GetRegionAt(cell) != region || VehicleRegionTypeUtility.GetExpectedRegionType(IntVec3.op_Addition(cell, ((Rot4) ref rot).FacingCell), this.mapping, this.createdFor) != expectedRegionType;
  }

  private int GetRegionId() => Interlocked.Increment(ref this.nextId);

  [DebugAction("Vehicle Framework", null, false, false, false, false, false, 0, false)]
  private static List<DebugActionNode> ForceRegenerateRegion()
  {
    List<DebugActionNode> debugActionNodeList = new List<DebugActionNode>();
    if (!GenList.NullOrEmpty<VehicleDef>((IList<VehicleDef>) VehicleHarmony.AllMoveableVehicleDefs))
    {
      foreach (VehicleDef moveableVehicleDef in VehicleHarmony.AllMoveableVehicleDefs)
      {
        VehicleDef vehicleDef = moveableVehicleDef;
        debugActionNodeList.Add(new DebugActionNode(((Def) vehicleDef).defName, (DebugActionType) 1, (Action) null, (Action<Pawn>) null)
        {
          action = (Action) (() =>
          {
            Map currentMap = Find.CurrentMap;
            if (currentMap == null)
            {
              Log.Error("Attempting to use DebugRegionOptions with null map.");
            }
            else
            {
              DebugHelper.Local.VehicleDef = vehicleDef;
              DebugHelper.Local.DebugType = DebugRegionType.Regions | DebugRegionType.Links;
              IntVec3 cell = UI.MouseCell();
              currentMap.GetCachedMapComponent<VehiclePathingSystem>()[vehicleDef].VehicleRegionDirtyer.NotifyWalkabilityChanged(cell);
            }
          })
        });
      }
    }
    return debugActionNodeList;
  }

  public enum RegionResult
  {
    Failed,
    NoRegion,
    Success,
  }
}
