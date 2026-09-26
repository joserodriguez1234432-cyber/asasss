// Decompiled with JetBrains decompiler
// Type: SmashTools.ThingDefCountList
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools.Performance;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public sealed class ThingDefCountList : IPoolable, IEnumerable<ThingDefCountClass>, IEnumerable
{
  private readonly List<ThingDefCountClass> items = new List<ThingDefCountClass>();
  private ThingDefCountClass lastUsedCountClass;

  bool IPoolable.InPool { get; set; }

  public int Count => this.items.Count;

  public List<ThingDefCountClass> InnerListForReading => this.items;

  [MustUseReturnValue]
  public ThingDefCountClass Find(ThingDef thingDef)
  {
    if (this.lastUsedCountClass?.thingDef == thingDef)
      return this.lastUsedCountClass;
    foreach (ThingDefCountClass thingDefCountClass in this.items)
    {
      if (thingDefCountClass.thingDef == thingDef)
      {
        this.lastUsedCountClass = thingDefCountClass;
        return thingDefCountClass;
      }
    }
    return (ThingDefCountClass) null;
  }

  public void Add(ThingDefCountClass countClass)
  {
    this.lastUsedCountClass = countClass;
    this.items.Add(countClass);
  }

  public void Reset()
  {
    foreach (ThingDefCountClass thingDefCountClass in this.items)
    {
      thingDefCountClass.thingDef = (ThingDef) null;
      thingDefCountClass.stuff = (ThingDef) null;
      thingDefCountClass.count = 0;
      thingDefCountClass.color = new Color?();
      thingDefCountClass.chance = new float?();
      thingDefCountClass.quality = (QualityCategory) 0;
      SimplePool<ThingDefCountClass>.Return(thingDefCountClass);
    }
    this.lastUsedCountClass = (ThingDefCountClass) null;
    this.items.Clear();
  }

  public List<ThingDefCountClass>.Enumerator GetEnumerator() => this.items.GetEnumerator();

  IEnumerator<ThingDefCountClass> IEnumerable<ThingDefCountClass>.GetEnumerator()
  {
    return (IEnumerator<ThingDefCountClass>) this.GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.GetEnumerator();
}
