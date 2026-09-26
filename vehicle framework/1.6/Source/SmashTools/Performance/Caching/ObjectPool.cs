// Decompiled with JetBrains decompiler
// Type: SmashTools.Performance.ObjectPool`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;

#nullable disable
namespace SmashTools.Performance;

public class ObjectPool<T> where T : class, IPoolable, new()
{
  private readonly T[] pool;
  private int head;
  private readonly object poolLock = new object();

  public ObjectPool(int size) => this.pool = new T[size];

  public ObjectPool(int size, int preWarm)
  {
    this.pool = new T[size];
    this.PreWarm(preWarm);
  }

  public int Count => this.head;

  public void Return(T item)
  {
    lock (this.poolLock)
    {
      if (((IList<T>) this.pool).OutOfBounds<T>(this.head))
        return;
      item.Reset();
      this.pool[this.head] = item;
      if (this.head < this.pool.Length - 1)
        ++this.head;
      item.InPool = true;
    }
  }

  public T Get()
  {
    lock (this.poolLock)
    {
      if (this.head == 0)
        return new T();
      --this.head;
      T obj = this.pool[this.head];
      this.pool[this.head] = default (T);
      obj.InPool = false;
      return obj;
    }
  }

  public ObjectPool<T>.Scope GetTemporary(out T obj)
  {
    obj = this.Get();
    return new ObjectPool<T>.Scope(this, in obj);
  }

  public void PreWarm(int count)
  {
    lock (this.poolLock)
    {
      int num = count - this.head;
      if (num <= 0)
        return;
      for (int index = 0; index < num; ++index)
        this.Return(new T());
    }
  }

  public void Dump()
  {
    lock (this.poolLock)
    {
      while (this.head > 0)
        this.Get();
    }
  }

  [PublicAPI]
  public readonly struct Scope(ObjectPool<T> pool, in T item) : IDisposable
  {
    private readonly ObjectPool<T> pool = pool;
    private readonly T item = item;

    void IDisposable.Dispose() => this.pool.Return(this.item);
  }
}
