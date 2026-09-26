// Decompiled with JetBrains decompiler
// Type: UpdateLogTool.Lookup
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

using System.Collections.Generic;

#nullable disable
namespace UpdateLogTool;

public class Lookup
{
  private Dictionary<string, object> innerDictionary = new Dictionary<string, object>();

  public object this[string key]
  {
    set => this.innerDictionary[key.ToUpperInvariant()] = value;
  }

  public T Get<T>(string key, T fallback = null)
  {
    object obj;
    return this.innerDictionary.TryGetValue(key.ToUpperInvariant(), out obj) ? (T) obj : fallback;
  }
}
