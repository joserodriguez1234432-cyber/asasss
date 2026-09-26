// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleRegionConnector
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using LudeonTK;
using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class VehicleRegionConnector : VehicleGridManager
{
  private const int ChunkCellCount = 144 /*0x90*/;
  private VehicleRegionGrid regionGrid;
  private VehiclePathGrid pathGrid;
  private readonly ObjectPool<VehicleRegionConnector.ConnectorGroup> connectorPool;
  private readonly ThreadLocal<VehicleRegionConnector.CostFinder> costFinder;
  private readonly List<VehicleRegion> regions = new List<VehicleRegion>();
  private readonly ConcurrentDictionary<VehicleRegion, VehicleRegionConnector.ConnectorGroup> connectors = new ConcurrentDictionary<VehicleRegion, VehicleRegionConnector.ConnectorGroup>();

  public VehicleRegionConnector(VehiclePathingSystem mapping, VehicleDef createdFor)
    : base(mapping, createdFor)
  {
    this.connectorPool = new ObjectPool<VehicleRegionConnector.ConnectorGroup>(Mathf.CeilToInt((float) ((double) mapping.map.Size.x / 12.0 * ((double) mapping.map.Size.z / 12.0) * 8.0 * 0.5)));
    this.costFinder = new ThreadLocal<VehicleRegionConnector.CostFinder>((Func<VehicleRegionConnector.CostFinder>) (() => new VehicleRegionConnector.CostFinder(this)));
  }

  public bool GridConnected { get; private set; }

  public bool IsDisabled { get; internal set; }

  public override void PostInit()
  {
    base.PostInit();
    this.pathGrid = this.mapping[this.createdFor].VehiclePathGrid;
    this.regionGrid = this.mapping[this.createdFor].VehicleRegionGrid;
  }

  public void RebuildAllConnections()
  {
    this.regionGrid.GetAllRegions(this.regions);
    foreach (VehicleRegion region in this.regions)
      this.RecalculateWeights(region);
  }

  internal void RecalculateWeights(VehicleRegion region)
  {
    this.GridConnected = false;
    using (ListSnapshot<VehicleRegionLink> links = region.Links)
    {
      VehicleRegionConnector.ConnectorGroup group;
      if (!this.connectors.TryGetValue(region, out group))
      {
        group = this.connectorPool.Get();
        this.connectors[region] = group;
      }
      group.Reset();
      for (int index1 = 0; index1 < links.Count; ++index1)
      {
        VehicleRegionLink vehicleRegionLink1 = links.items[index1];
        IntVec3 end1 = vehicleRegionLink1.End;
        for (int index2 = index1 + 1; index2 < links.Count; ++index2)
        {
          VehicleRegionLink vehicleRegionLink2 = links.items[index2];
          IntVec3 end2 = vehicleRegionLink2.End;
          this.Connect(region, group, vehicleRegionLink1.Root, vehicleRegionLink2.Root);
          this.Connect(region, group, vehicleRegionLink1.Root, end2);
          this.Connect(region, group, end1, vehicleRegionLink2.Root);
          this.Connect(region, group, end1, end2);
        }
      }
      this.GridConnected = true;
    }
  }

  private void AdjustInfacing(VehicleRegion region, VehicleRegionLink linkA)
  {
  }

  private void Connect(
    VehicleRegion region,
    VehicleRegionConnector.ConnectorGroup group,
    IntVec3 from,
    IntVec3 to)
  {
    float cost = this.costFinder.Value.ConnectionCost(region, from, to);
    ulong hash = ((IntVec3) ref from).UniqueHashCode();
    int index1 = ((CellIndices) ref this.mapping.map.cellIndices).CellToIndex(from);
    int index2 = ((CellIndices) ref this.mapping.map.cellIndices).CellToIndex(from);
    group.Add(hash, index1, index2, cost);
  }

  public static int SymmetricHash(int x1, int z1, int x2, int z2)
  {
    int num1 = QuickHash(x1, z1);
    int num2 = QuickHash(x2, z2);
    return num1 + num2 + (num1 ^ num2);

    static int QuickHash(int a, int b) => (17 * 31 /*0x1F*/ + a) * 31 /*0x1F*/ + b;
  }

  [DebugAction(null, null, false, false, false, false, false, 0, false)]
  private static void DebugRebuildAllConnections()
  {
    VehiclePathingSystem mapping = Find.CurrentMap.GetCachedMapComponent<VehiclePathingSystem>();
    VehicleDef vehicleDef = ((IEnumerable<VehicleDef>) mapping.GridOwners.AllOwners).FirstOrDefault<VehicleDef>((Func<VehicleDef, bool>) (def => !mapping[def].Suspended));
    VehicleRegionConnector vehicleRegionConnector = mapping[vehicleDef].VehicleRegionConnector;
    DeepProfiler.Start("Rebuild All Connections");
    vehicleRegionConnector.RebuildAllConnections();
    DeepProfiler.End();
  }

  public readonly struct Disabler : IDisposable
  {
    private readonly VehicleRegionConnector connector;

    public Disabler(VehicleRegionConnector connector)
    {
      this.connector = connector;
      this.connector.IsDisabled = true;
    }

    void IDisposable.Dispose() => this.connector.IsDisabled = false;
  }

  private class ConnectorGroup : IPoolable
  {
    private readonly Dictionary<ulong, List<Connection>> roots = new Dictionary<ulong, List<Connection>>();
    private readonly object dictLock = new object();

    public bool InPool { get; set; }

    public void Add(ulong hash, int root, int to, float cost)
    {
      lock (this.dictLock)
      {
        List<Connection> connectionList;
        if (!this.roots.TryGetValue(hash, out connectionList))
        {
          connectionList = SimplePool<List<Connection>>.Get();
          this.roots[hash] = connectionList;
        }
        connectionList.Add(new Connection(root, to, cost));
      }
    }

    public void Reset()
    {
      lock (this.dictLock)
        GenList.ClearAndPoolValueLists<ulong, Connection>(this.roots);
    }
  }

  private class CostFinder
  {
    private readonly VehicleRegionConnector regionConnector;
    private readonly PriorityQueue<int, int> openQueue = new PriorityQueue<int, int>();
    private readonly VehicleRegionConnector.Node[] nodes = new VehicleRegionConnector.Node[144 /*0x90*/];
    private readonly BoolGrid visited = new BoolGrid(13, 13);
    private readonly CellIndices cellIndices;

    public CostFinder(VehicleRegionConnector regionConnector)
    {
      this.regionConnector = regionConnector;
      this.cellIndices = new CellIndices(regionConnector.mapping.map);
    }

    private bool IsRunning { get; set; }

    public float ConnectionCost(VehicleRegion region, IntVec3 start, IntVec3 destination)
    {
      this.IsRunning = true;
      try
      {
        CellRect chunkRect = VehicleRegion.ChunkAt(start);
        int index1 = ((CellIndices) ref this.cellIndices).CellToIndex(start);
        int index2 = ((CellIndices) ref this.cellIndices).CellToIndex(destination);
        using (ListSnapshot<VehicleRegionLink> links = region.Links)
        {
          this.openQueue.Enqueue(index1, 0);
          while (this.openQueue.Count > 0)
          {
            int num1;
            int num2;
            if (this.openQueue.TryDequeue(ref num1, ref num2))
            {
              if (num1.Equals(index2))
                return this.TotalCost(index1, index2, ref chunkRect);
              int current = this.RelativeIndex(num1, ref chunkRect);
              foreach (int index3 in this.NeighborsAt(region, links.items, num1))
              {
                int neighbor = this.RelativeIndex(index3, ref chunkRect);
                if (!this.visited[neighbor])
                {
                  VehicleRegionConnector.Node node = this.CreateNode(current, neighbor);
                  this.nodes[neighbor] = node;
                  this.openQueue.Enqueue(index3, node.cost);
                }
              }
            }
            else
              break;
          }
        }
      }
      finally
      {
        this.IsRunning = false;
        this.openQueue.Clear();
        this.visited.Clear();
      }
      Log.Error($"Ran out of cells to process from {start} to {destination}.");
      return 0.0f;
    }

    private IEnumerable<int> NeighborsAt(
      VehicleRegion region,
      List<VehicleRegionLink> links,
      int current)
    {
      IntVec3 cell = ((CellIndices) ref this.cellIndices).IndexToCell(current);
      for (int i = 0; i < 8; ++i)
      {
        int x = cell.x + VehiclePathFinder.neighborOffsets[i];
        int z = cell.z + VehiclePathFinder.neighborOffsets[i + 8];
        int index = ((CellIndices) ref this.cellIndices).CellToIndex(x, z);
        if (this.regionConnector.regionGrid.GetRegionAt(index) == region)
        {
          yield return index;
        }
        else
        {
          foreach (VehicleRegionLink link in links)
          {
            if (link.Root.x == x && link.Root.z == z || link.End.x == x && link.End.z == z)
              yield return index;
          }
        }
      }
    }

    private VehicleRegionConnector.Node CreateNode(int current, int neighbor)
    {
      return new VehicleRegionConnector.Node()
      {
        parent = current,
        cost = this.regionConnector.pathGrid.innerArray[neighbor]
      };
    }

    private int RelativeIndex(int index, [RequiresLocation, In] ref CellRect chunkRect)
    {
      IntVec3 cell = ((CellIndices) ref this.cellIndices).IndexToCell(index);
      return (cell.z - chunkRect.minZ) * 12 + (cell.x - chunkRect.minX);
    }

    private float TotalCost(int start, int destination, [RequiresLocation, In] ref CellRect chunkRect)
    {
      int num = this.RelativeIndex(start, ref chunkRect);
      VehicleRegionConnector.Node node = this.nodes[this.RelativeIndex(destination, ref chunkRect)];
      float cost = (float) node.cost;
      for (int index = 0; index < 144 /*0x90*/; ++index)
      {
        int parent = node.parent;
        node = this.nodes[parent];
        cost += (float) node.cost;
        if (parent == num)
          return cost;
      }
      Log.Error("Misconfigured path in region connector. Unable to resolve total cost");
      return cost;
    }
  }

  private struct Node
  {
    public int parent;
    public int cost;
  }
}
