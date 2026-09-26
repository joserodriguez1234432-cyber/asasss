// Decompiled with JetBrains decompiler
// Type: SmashTools.Performance.GlobalObjectPool
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

#nullable disable
namespace SmashTools.Performance;

[PublicAPI]
public static class GlobalObjectPool
{
  public static GlobalObjectPool.Receipt<T> Get<T>(out T obj) where T : new()
  {
    return new GlobalObjectPool.Receipt<T>(out obj);
  }

  public static GlobalObjectPool.StringBuilderReceipt Get(out StringBuilder stringBuilder)
  {
    return new GlobalObjectPool.StringBuilderReceipt(out stringBuilder);
  }

  public static GlobalObjectPool.CollectionReceipt<List<T>, T> Get<T>(out List<T> list)
  {
    return new GlobalObjectPool.CollectionReceipt<List<T>, T>(out list);
  }

  public static GlobalObjectPool.CollectionReceipt<HashSet<T>, T> Get<T>(out HashSet<T> set)
  {
    return new GlobalObjectPool.CollectionReceipt<HashSet<T>, T>(out set);
  }

  [PublicAPI]
  public readonly struct Receipt<T> : IDisposable where T : new()
  {
    private readonly T obj;

    public Receipt(out T obj)
    {
      this.obj = SimplePool<T>.Get();
      obj = this.obj;
    }

    void IDisposable.Dispose() => SimplePool<T>.Return(this.obj);
  }

  [PublicAPI]
  public readonly struct CollectionReceipt<C, T> : IDisposable where C : ICollection<T>, new()
  {
    private readonly C collection;

    public CollectionReceipt(out C collection)
    {
      this.collection = SimplePool<C>.Get();
      this.collection.Clear();
      collection = this.collection;
    }

    void IDisposable.Dispose()
    {
      this.collection.Clear();
      SimplePool<C>.Return(this.collection);
    }
  }

  [PublicAPI]
  public readonly struct StringBuilderReceipt : IDisposable
  {
    private readonly StringBuilder stringBuilder;

    public StringBuilderReceipt(out StringBuilder stringBuilder)
    {
      this.stringBuilder = SimplePool<StringBuilder>.Get();
      stringBuilder = this.stringBuilder;
      stringBuilder.Clear();
    }

    void IDisposable.Dispose()
    {
      this.stringBuilder.Clear();
      SimplePool<StringBuilder>.Return(this.stringBuilder);
    }
  }
}
