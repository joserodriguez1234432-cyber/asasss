// Decompiled with JetBrains decompiler
// Type: SmashTools.Performance.AsyncPool`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

#nullable disable
namespace SmashTools.Performance;

public static class AsyncPool<T> where T : class, new()
{
  private static ConcurrentBag<T> bag = new ConcurrentBag<T>();
  private static int counter;

  public static int Count => AsyncPool<T>.bag.Count;

  public static T Get()
  {
    T result;
    if (!AsyncPool<T>.bag.TryTake(out result))
      result = new T();
    return result;
  }

  public static void Return(T item) => AsyncPool<T>.bag.Add(item);

  [Conditional("DEBUG")]
  private static void ItemReturned() => Interlocked.Increment(ref AsyncPool<T>.counter);

  [Conditional("DEBUG")]
  private static void ItemRemoved() => Interlocked.Decrement(ref AsyncPool<T>.counter);

  public static void PreWarm(int count)
  {
    int num = count - AsyncPool<T>.Count;
    if (num <= 0)
      return;
    for (int index = 0; index < num; ++index)
      AsyncPool<T>.Return(new T());
  }

  internal static void Clear()
  {
    AsyncPool<T>.bag = new ConcurrentBag<T>();
    AsyncPool<T>.counter = 0;
  }

  [PublicAPI]
  public readonly struct Scope : IDisposable
  {
    private readonly T item;

    public Scope(out T item)
    {
      this.item = AsyncPool<T>.Get();
      item = this.item;
    }

    void IDisposable.Dispose() => AsyncPool<T>.Return(this.item);
  }
}
