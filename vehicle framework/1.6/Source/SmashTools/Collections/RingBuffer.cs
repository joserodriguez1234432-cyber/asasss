// Decompiled with JetBrains decompiler
// Type: SmashTools.RingBuffer`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using Verse;

#nullable disable
namespace SmashTools;

[PublicAPI]
public class RingBuffer<T>
{
  private readonly T[] array;
  private int head;
  private int tail;
  private readonly object syncRoot = new object();

  public RingBuffer(int size)
  {
    this.array = size >= 0 ? new T[size] : throw new ArgumentOutOfRangeException(nameof (size));
  }

  public int Length => this.array.Length;

  public T[] InnerArray => this.array;

  public T this[int index]
  {
    get
    {
      lock (this.syncRoot)
        return this.array[GenMath.PositiveMod(this.tail + index, this.Length)];
    }
  }

  public T Push(T item)
  {
    lock (this.syncRoot)
    {
      T obj = this.array[this.head];
      this.array[this.head] = item;
      this.head = GenMath.PositiveMod(++this.head, this.Length);
      if (this.head == this.tail)
        this.tail = GenMath.PositiveMod(++this.tail, this.Length);
      return obj;
    }
  }

  public void RemoveAt(int index)
  {
    lock (this.syncRoot)
      this.array[GenMath.PositiveMod(this.tail + index, this.Length)] = default (T);
  }
}
