// Decompiled with JetBrains decompiler
// Type: SmashTools.UnityObjectPool`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SmashTools;

[PublicAPI]
public class UnityObjectPool<T> where T : Object
{
  private readonly T[] pool;
  private int head;
  private Func<T> factory;
  private Action<T> onDestroy;

  public UnityObjectPool(Func<T> factory, int size, Action<T> onDestroy = null, int preWarm = 0)
  {
    this.factory = factory;
    this.onDestroy = onDestroy;
    this.pool = new T[size];
    this.PreWarm(preWarm);
  }

  public int Count => this.head;

  public void Return(T item)
  {
    if (((IList<T>) this.pool).OutOfBounds<T>(this.head))
    {
      Action<T> onDestroy = this.onDestroy;
      if (onDestroy != null)
        onDestroy(item);
      Object.Destroy((Object) item);
    }
    else
    {
      this.pool[this.head] = item;
      if (this.head >= this.pool.Length - 1)
        return;
      ++this.head;
    }
  }

  public T Get()
  {
    if (this.head == 0)
      return this.factory();
    --this.head;
    T obj = this.pool[this.head];
    this.pool[this.head] = default (T);
    return obj;
  }

  public void PreWarm(int count)
  {
    int num = count - this.head;
    if (num <= 0)
      return;
    for (int index = 0; index < num; ++index)
      this.Return(this.factory());
  }

  public void Dump()
  {
    while (this.head > 0)
    {
      T obj = this.Get();
      Action<T> onDestroy = this.onDestroy;
      if (onDestroy != null)
        onDestroy(obj);
      Object.Destroy((Object) obj);
    }
  }
}
