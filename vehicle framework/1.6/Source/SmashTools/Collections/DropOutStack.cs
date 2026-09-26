// Decompiled with JetBrains decompiler
// Type: SmashTools.DropOutStack`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public class DropOutStack<T> : IEnumerable<T>, IEnumerable
{
  private readonly T[] items;
  private int top;
  private object lockObj = new object();

  public DropOutStack(int capacity) => this.items = new T[capacity];

  public int Count { get; private set; }

  public void Push(T item)
  {
    lock (this.lockObj)
    {
      this.items[this.top] = item;
      this.top = GenMath.PositiveMod(++this.top, this.items.Length);
      this.Count = Mathf.Clamp(++this.Count, 0, this.items.Length);
    }
  }

  public T Pop()
  {
    if (this.Count == 0)
      throw new InvalidOperationException("Empty stack");
    lock (this.lockObj)
    {
      this.top = GenMath.PositiveMod(this.items.Length + --this.top, this.items.Length);
      T obj = this.items[this.top];
      this.items[this.top] = default (T);
      this.Count = Mathf.Clamp(--this.Count, 0, this.items.Length);
      return obj;
    }
  }

  public bool TryPop(out T item)
  {
    if (this.Count == 0)
    {
      item = default (T);
      return false;
    }
    item = this.Pop();
    return true;
  }

  public T Peek() => this.items[this.top];

  public IEnumerator<T> GetEnumerator()
  {
    lock (this.lockObj)
    {
      for (int i = 0; i < this.Count; ++i)
        yield return this.items[i];
    }
  }

  IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.GetEnumerator();
}
