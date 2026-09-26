// Decompiled with JetBrains decompiler
// Type: SmashTools.Algorithms.AStar`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace SmashTools.Algorithms;

public class AStar<T> : Dijkstra<T>
{
  private readonly Func<T, int> getHeuristic;

  public AStar(IPathfinder<T> pathfinder, Func<T, int> getHeuristic)
    : base(pathfinder)
  {
    this.getHeuristic = getHeuristic;
  }

  public AStar(
    Func<T, int> getHeuristic,
    Func<T, T, int> cost,
    Func<T, List<T>> neighbors,
    Func<T, bool> canEnter = null)
    : base(cost, neighbors, canEnter)
  {
    this.getHeuristic = getHeuristic;
  }

  protected override bool CreateNode(T current, T neighbor, out Dijkstra<T>.Node node)
  {
    bool node1 = base.CreateNode(current, neighbor, out node);
    if (node1)
      node.heuristicCost = this.getHeuristic(neighbor);
    return node1;
  }
}
