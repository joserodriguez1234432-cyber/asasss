// Decompiled with JetBrains decompiler
// Type: SmashTools.Performance.ListSnapshot`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections;
using System.Collections.Generic;

#nullable disable
namespace SmashTools.Performance;

public readonly struct ListSnapshot<T> : IDisposable, IEnumerable<T>, IEnumerable
{
  public readonly List<T> items;

  public ListSnapshot(List<T> listToCopy)
  {
    this.items = AsyncPool<List<T>>.Get();
    this.items.AddRange((IEnumerable<T>) listToCopy);
  }

  public int Count => this.items.Count;

  void IDisposable.Dispose()
  {
    this.items.Clear();
    AsyncPool<List<T>>.Return(this.items);
  }

  IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.items.GetEnumerator();

  IEnumerator<T> IEnumerable<T>.GetEnumerator() => (IEnumerator<T>) this.items.GetEnumerator();
}
