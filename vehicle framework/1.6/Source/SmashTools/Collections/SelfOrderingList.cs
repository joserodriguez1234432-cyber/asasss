// Decompiled with JetBrains decompiler
// Type: SmashTools.SelfOrderingList`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

#nullable disable
namespace SmashTools;

[PublicAPI]
public sealed class SelfOrderingList<T> : 
  IList<T>,
  ICollection<T>,
  IEnumerable<T>,
  IEnumerable,
  IReadOnlyList<T>,
  IReadOnlyCollection<T>
{
  private const int DefaultSize = 3;
  private static readonly T[] EmptyContents = Array.Empty<T>();
  private static readonly uint[] EmptyCounters = Array.Empty<uint>();
  private T[] contents;
  private uint[] counters;
  private int size;
  private int version;
  private EqualityComparer<T> comparer = EqualityComparer<T>.Default;

  public SelfOrderingList()
  {
    this.contents = SelfOrderingList<T>.EmptyContents;
    this.counters = SelfOrderingList<T>.EmptyCounters;
  }

  public SelfOrderingList(int capacity)
  {
    if (capacity < 0)
      throw new ArgumentOutOfRangeException(nameof (capacity), $"{capacity} is not within bounds of list. Capacity of list must be >= 0");
    if (capacity == 0)
    {
      this.contents = SelfOrderingList<T>.EmptyContents;
      this.counters = SelfOrderingList<T>.EmptyCounters;
    }
    else
    {
      this.contents = new T[capacity];
      this.counters = new uint[capacity];
    }
  }

  public SelfOrderingList(IEnumerable<T> enumerable)
  {
    if (enumerable == null)
      throw new ArgumentNullException(nameof (enumerable));
    if (enumerable is ICollection<T> objs)
    {
      int count = objs.Count;
      if (count == 0)
      {
        this.contents = SelfOrderingList<T>.EmptyContents;
        this.counters = SelfOrderingList<T>.EmptyCounters;
      }
      else
      {
        this.contents = new T[count];
        this.counters = new uint[count];
        objs.CopyTo(this.contents, 0);
        this.size = count;
      }
    }
    else
    {
      this.size = 0;
      this.contents = SelfOrderingList<T>.EmptyContents;
      this.counters = SelfOrderingList<T>.EmptyCounters;
      foreach (T obj in enumerable)
        this.Add(obj);
    }
  }

  bool ICollection<T>.IsReadOnly => false;

  public int Count => this.size;

  public int Capacity
  {
    get => this.contents.Length;
    set
    {
      if (value < this.size)
        throw new ArgumentOutOfRangeException(nameof (value), "Setting capacity lower than current item count.");
      if (value == this.contents.Length)
        return;
      if (value > 0)
      {
        T[] destinationArray1 = new T[value];
        uint[] destinationArray2 = new uint[value];
        if (this.size > 0)
        {
          Array.Copy((Array) this.contents, 0, (Array) destinationArray1, 0, this.size);
          Array.Copy((Array) this.counters, 0, (Array) destinationArray2, 0, this.size);
        }
        this.contents = destinationArray1;
        this.counters = destinationArray2;
      }
      else
      {
        this.contents = SelfOrderingList<T>.EmptyContents;
        this.counters = SelfOrderingList<T>.EmptyCounters;
      }
      ++this.version;
    }
  }

  public T this[int index]
  {
    get => this.contents[index];
    set
    {
      this.contents[index] = value;
      ++this.version;
    }
  }

  private static bool IsCompatibleObject(object value)
  {
    if (value is T)
      return true;
    return value == null && (object) default (T) == null;
  }

  public void Add(T item)
  {
    if (this.size == this.contents.Length)
      this.EnsureCapacity(this.size + 1);
    this.contents[this.size] = item;
    this.counters[this.size] = 0U;
    ++this.size;
    ++this.version;
  }

  public void AddRange(IEnumerable<T> enumerable)
  {
    foreach (T obj in enumerable)
      this.Add(obj);
  }

  public void InsertRange(int index, IEnumerable<T> enumerable)
  {
    if (enumerable == null)
      throw new ArgumentNullException(nameof (enumerable));
    if (index < 0 || index > this.size)
      throw new IndexOutOfRangeException(nameof (index));
    if (this == enumerable)
      throw new NotSupportedException("Self insertion is not supported");
    if (enumerable is ICollection<T> objs)
    {
      int count = objs.Count;
      if (count == 0)
        return;
      this.EnsureCapacity(this.size + count);
      Array.Copy((Array) this.contents, index, (Array) this.contents, index + count, this.size - index);
      Array.Copy((Array) this.counters, index, (Array) this.counters, index + count, this.size - index);
      objs.CopyTo(this.contents, index);
      this.size += count;
      ++this.version;
    }
    else
    {
      foreach (T obj in enumerable)
        this.Insert(index++, obj);
    }
  }

  public IEnumerator GetEnumerator() => (IEnumerator) new SelfOrderingList<T>.Enumerator(this);

  IEnumerator<T> IEnumerable<T>.GetEnumerator()
  {
    return (IEnumerator<T>) new SelfOrderingList<T>.Enumerator(this);
  }

  public ReadOnlyCollection<T> AsReadOnly() => new ReadOnlyCollection<T>((IList<T>) this);

  public void Clear()
  {
    if (this.size > 0)
    {
      Array.Clear((Array) this.contents, 0, this.size);
      Array.Clear((Array) this.counters, 0, this.size);
      this.size = 0;
    }
    ++this.version;
  }

  public bool Contains(T item) => this.size > 0 && this.IndexOf(item) >= 0;

  private void EnsureCapacity(int min)
  {
    if (this.contents.Length >= min)
      return;
    int num = this.contents.Length == 0 ? 3 : this.contents.Length * 2;
    if ((uint) num > 2146435071U)
      num = 2146435071;
    if (num < min)
      num = min;
    this.Capacity = num;
  }

  public T Grab(T item)
  {
    T result;
    this.TryGrab(item, out result);
    return result;
  }

  public bool TryGrab(T item, out T result)
  {
    for (int index = 0; index < this.size; ++index)
    {
      if (this.comparer.Equals(this.contents[index], item))
      {
        result = this.contents[index];
        this.Touch(index);
        return true;
      }
    }
    result = default (T);
    return false;
  }

  public void Touch(int index)
  {
    if (index < 0 || index >= this.size)
      throw new ArgumentOutOfRangeException(nameof (index));
    ++this.counters[index];
    if (index > 0 && this.counters[index] > this.counters[index - 1])
      this.Bump(index);
    if (this.counters[index] != uint.MaxValue)
      return;
    this.ResetCounters();
  }

  private void ResetCounters()
  {
    for (int index = 0; index < this.size; ++index)
      this.counters[index] = (uint) (this.size - index);
  }

  [Pure]
  public int IndexOf(T item) => Array.IndexOf<T>(this.contents, item, 0, this.size);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void Bump(int index)
  {
    T[] contents1 = this.contents;
    int index1 = index;
    T[] contents2 = this.contents;
    int num1 = index - 1;
    T content1 = this.contents[index - 1];
    T content2 = this.contents[index];
    contents1[index1] = content1;
    int index2 = num1;
    T obj = content2;
    contents2[index2] = obj;
    ref uint local1 = ref this.counters[index];
    ref uint local2 = ref this.counters[index - 1];
    uint counter1 = this.counters[index - 1];
    uint counter2 = this.counters[index];
    local1 = counter1;
    int num2 = (int) counter2;
    local2 = (uint) num2;
    ++this.version;
  }

  public void Insert(int index, T item)
  {
    if ((uint) index > (uint) this.size)
      throw new ArgumentOutOfRangeException(nameof (index));
    if (this.size == this.contents.Length)
      this.EnsureCapacity(this.size + 1);
    if (index < this.size)
      Array.Copy((Array) this.contents, index, (Array) this.contents, index + 1, this.size - index);
    this.contents[index] = item;
    ++this.size;
    ++this.version;
  }

  public bool Remove(T item)
  {
    int index = this.IndexOf(item);
    if (index < 0)
      return false;
    this.RemoveAt(index);
    return true;
  }

  public void RemoveAt(int index)
  {
    if (index < 0 || index >= this.size)
      throw new ArgumentOutOfRangeException(nameof (index));
    --this.size;
    Array.Copy((Array) this.contents, index + 1, (Array) this.contents, index, this.size - index);
    Array.Copy((Array) this.counters, index + 1, (Array) this.counters, index, this.size - index);
    this.contents[this.size] = default (T);
    this.counters[this.size] = 0U;
    ++this.version;
  }

  public void CopyTo(T[] array) => this.CopyTo(array, 0);

  public void CopyTo(T[] array, int arrayIndex)
  {
    if (array != null && array.Rank != 1)
      throw new ArgumentException("Multi-rank dimensions are not supported for SelfOrderingList", nameof (array));
    Array.Copy((Array) this.contents, 0, (Array) array, arrayIndex, this.size);
  }

  [Serializable]
  public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
  {
    private readonly SelfOrderingList<T> list;
    private int index;
    private readonly int version;
    private T current;

    internal Enumerator(SelfOrderingList<T> list)
    {
      this.list = list;
      this.index = 0;
      this.version = list.version;
      this.current = default (T);
    }

    public T Current => this.current;

    object IEnumerator.Current
    {
      get
      {
        if (this.index == 0 || this.index == this.list.size + 1)
          throw new InvalidOperationException();
        return (object) this.Current;
      }
    }

    void IEnumerator.Reset()
    {
      if (this.version != this.list.version)
        throw new InvalidOperationException();
      this.index = 0;
      this.current = default (T);
    }

    public void Dispose()
    {
    }

    public bool MoveNext()
    {
      SelfOrderingList<T> list = this.list;
      if (this.version != list.version || (uint) this.index >= (uint) list.size)
        return this.MoveNextRare();
      this.current = list.contents[this.index];
      ++this.index;
      return true;
    }

    private bool MoveNextRare()
    {
      if (this.version != this.list.version)
        throw new InvalidOperationException();
      this.index = this.list.size + 1;
      this.current = default (T);
      return false;
    }
  }
}
