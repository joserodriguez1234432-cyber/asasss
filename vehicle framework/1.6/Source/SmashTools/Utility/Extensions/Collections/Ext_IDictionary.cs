// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_IDictionary
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;

#nullable disable
namespace SmashTools;

[PublicAPI]
public static class Ext_IDictionary
{
  public static void AddOrAppend<K, C, V>(this IDictionary<K, C> dictionary, K key, V value) where C : ICollection<V>, new()
  {
    if (dictionary == null)
      throw new ArgumentNullException(nameof (dictionary), "Dictionary cannot be null.");
    if ((object) key == null)
      throw new ArgumentNullException(nameof (key), "Key cannot be null.");
    if (!dictionary.ContainsKey(key))
      dictionary.Add(key, new C());
    dictionary[key].Add(value);
  }
}
