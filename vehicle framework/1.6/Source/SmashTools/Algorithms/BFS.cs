// Decompiled with JetBrains decompiler
// Type: SmashTools.Algorithms.BFS`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Threading;
using Verse;

#nullable disable
namespace SmashTools.Algorithms;

[PublicAPI]
public class BFS<T>
{
  private readonly Queue<T> openQueue = new Queue<T>();
  private readonly HashSet<T> visited = new HashSet<T>();

  public bool IsRunning { get; private set; }

  public bool LogRetraceAttempts { get; set; }

  public void Stop()
  {
    this.openQueue.Clear();
    this.visited.Clear();
  }

  public void FloodFill(
    T start,
    Func<T, IEnumerable<T>> neighbors,
    Action<T> processor,
    Func<T, bool> canEnter = null)
  {
    this.FloodFill(start, neighbors, processor, (Action<T>) null, (Action<T>) null, canEnter);
  }

  public void FloodFill(
    T start,
    Func<T, IEnumerable<T>> neighbors,
    Action<T> processor,
    Action<T> onEntered,
    Action<T> onSkipped,
    Func<T, bool> canEnter = null)
  {
    this.FloodFill(start, neighbors, processor, onEntered, onSkipped, CancellationToken.None, canEnter);
  }

  public void FloodFill(
    T start,
    Func<T, IEnumerable<T>> neighbors,
    Action<T> processor,
    Action<T> onEntered,
    Action<T> onSkipped,
    CancellationToken token,
    Func<T, bool> canEnter = null)
  {
    if (this.IsRunning)
    {
      Log.Error("Attempting to run FloodFill while it's already in use.");
    }
    else
    {
      if (canEnter != null && !canEnter(start))
        return;
      this.IsRunning = true;
      try
      {
        this.openQueue.Clear();
        this.openQueue.Enqueue(start);
        this.visited.Add(start);
        if (onEntered != null)
          onEntered(start);
        while (this.openQueue.Count > 0 && !token.IsCancellationRequested)
        {
          T obj1 = this.openQueue.Dequeue();
          if (processor != null)
            processor(obj1);
          foreach (T obj2 in neighbors(obj1))
          {
            if (token.IsCancellationRequested)
              return;
            if (this.visited.Contains(obj2))
            {
              if (this.LogRetraceAttempts)
                Log.Error($"Attempting to open closed node {obj2}. Skipping to avoid infinite loop.");
            }
            else if (canEnter == null || canEnter(obj2))
            {
              this.visited.Add(obj2);
              this.openQueue.Enqueue(obj2);
              if (onEntered != null)
                onEntered(obj2);
            }
            else if (onSkipped != null)
              onSkipped(obj2);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error($"Exception thrown while performing BFS FloodFill.\n{ex}");
      }
      finally
      {
        this.IsRunning = false;
        this.Stop();
      }
    }
  }

  public List<T> FloodFill(T start, Func<T, IEnumerable<T>> neighbors, Func<T, bool> canEnter = null)
  {
    List<T> objList = new List<T>();
    T start1 = start;
    Func<T, IEnumerable<T>> neighbors1 = neighbors;
    Func<T, bool> func = canEnter;
    Action<T> processor = new Action<T>(objList.Add);
    Func<T, bool> canEnter1 = func;
    this.FloodFill(start1, neighbors1, processor, canEnter1);
    return objList;
  }
}
