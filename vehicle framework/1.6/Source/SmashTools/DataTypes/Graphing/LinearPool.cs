// Decompiled with JetBrains decompiler
// Type: SmashTools.LinearPool`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public class LinearPool<T>
{
  public List<T> items = new List<T>();
  public FloatRange range = FloatRange.ZeroToOne;

  public LinearPool()
  {
  }

  public LinearPool(List<T> items) => this.items = items;

  public LinearPool(List<T> items, FloatRange range)
  {
    this.items = items;
    this.range = range;
    if ((double) range.max > (double) range.min)
      return;
    Log.Error("Attempting to initialize LinearPool with non-sequential bounderies.  This is not allowed!");
    range = FloatRange.ZeroToOne;
  }

  public int PointsCount => this.items.Count;

  public bool IsValid => !this.items.NullOrEmpty<T>();

  public T this[float value] => this.Evaluate(value);

  public virtual T Evaluate(float value)
  {
    if (this.items.NullOrEmpty<T>())
      return default (T);
    if (this.items.Count == 1)
      return this.items[0];
    if ((double) value <= (double) this.range.min)
      return this.items.FirstOrDefault<T>();
    return (double) value >= (double) this.range.max ? this.items.LastOrDefault<T>() : this.items[Mathf.Clamp(Mathf.RoundToInt((float) this.items.Count * (value / (this.range.max - this.range.min))), 0, this.items.Count - 1)];
  }
}
