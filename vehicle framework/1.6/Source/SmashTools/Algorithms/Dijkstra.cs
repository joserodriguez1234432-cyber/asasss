// Decompiled with JetBrains decompiler
// Type: SmashTools.Algorithms.Dijkstra`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace SmashTools.Algorithms;

[UsedImplicitly]
public class Dijkstra<T>
{
  private readonly PriorityQueue<T, int> openQueue = new PriorityQueue<T, int>();
  private readonly Dictionary<T, Dijkstra<T>.Node> nodes = new Dictionary<T, Dijkstra<T>.Node>();
  private readonly Func<T, bool> canEnter;
  private readonly Func<T, IEnumerable<T>> neighbors;
  private readonly Func<T, T, int> cost;

  public Dijkstra(IPathfinder<T> pathfinder)
  {
    this.cost = new Func<T, T, int>(pathfinder.Cost);
    this.canEnter = new Func<T, bool>(pathfinder.CanEnter);
    this.neighbors = new Func<T, IEnumerable<T>>(pathfinder.Neighbors);
  }

  public Dijkstra(Func<T, T, int> cost, Func<T, List<T>> neighbors, Func<T, bool> canEnter = null)
  {
    this.cost = cost;
    this.neighbors = (Func<T, IEnumerable<T>>) neighbors;
    this.canEnter = canEnter;
  }

  public bool IsRunning { get; private set; }

  public void Run(T start, T destination, List<T> path)
  {
    this.IsRunning = true;
    try
    {
      this.openQueue.Clear();
      this.openQueue.Enqueue(start, 0);
      T current;
      int num;
      while (this.openQueue.Count > 0 && this.openQueue.TryDequeue(ref current, ref num))
      {
        if (current.Equals((object) destination))
        {
          this.SolvePath(start, destination, path);
          return;
        }
        foreach (T obj in this.neighbors(current))
        {
          Dijkstra<T>.Node node;
          if ((this.canEnter == null || this.canEnter(obj)) && this.CreateNode(current, obj, out node))
          {
            this.nodes[obj] = node;
            this.openQueue.Enqueue(obj, node.cost + node.heuristicCost);
          }
        }
      }
      Log.Error($"Unable to find path from {start} to {destination}.");
    }
    finally
    {
      this.IsRunning = false;
    }
  }

  protected virtual bool CreateNode(T current, T neighbor, out Dijkstra<T>.Node node)
  {
    if (this.nodes.TryGetValue(neighbor, out node) && node.closed)
      return false;
    node = new Dijkstra<T>.Node()
    {
      parent = current,
      cost = this.cost(current, neighbor),
      heuristicCost = 0,
      closed = true
    };
    return true;
  }

  protected void SolvePath(T start, T destination, List<T> path)
  {
    T key = destination;
    Dijkstra<T>.Node node = this.nodes[key];
    while (!start.Equals((object) key))
    {
      path.Add(key);
      key = node.parent;
      node = this.nodes[key];
    }
    path.Add(start);
    if (path[path.Count - 1].Equals((object) start))
      return;
    SmashLog.Error($"BFS was unable to solve path from {start} to {destination}.");
  }

  protected struct Node
  {
    public T parent;
    public int cost;
    public bool closed;
    public int heuristicCost;
  }
}
