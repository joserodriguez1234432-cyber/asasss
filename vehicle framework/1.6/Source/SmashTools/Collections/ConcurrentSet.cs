// Decompiled with JetBrains decompiler
// Type: SmashTools.ConcurrentSet`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System.Collections.Concurrent;

#nullable disable
namespace SmashTools;

public class ConcurrentSet<T> : ConcurrentDictionary<T, byte>
{
  public bool Add(T item) => this.TryAdd(item, (byte) 0);

  public bool Remove(T item) => this.TryRemove(item, out byte _);

  public bool Contains(T item) => this.ContainsKey(item);
}
