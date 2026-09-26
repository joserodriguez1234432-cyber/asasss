// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.Selector
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace SmashTools.Animations;

public class Selector
{
  private readonly Dictionary<Type, HashSet<ISelectableUI>> selected = new Dictionary<Type, HashSet<ISelectableUI>>();

  public bool AnySelected<T>() where T : ISelectableUI
  {
    if (!this.selected.ContainsKey(typeof (T)))
      this.selected[typeof (T)] = new HashSet<ISelectableUI>();
    return this.selected[typeof (T)].Count > 0;
  }

  public bool IsSelected(ISelectableUI item)
  {
    Type type = item.GetType();
    if (this.selected.ContainsKey(type))
      return this.selected[type].Contains(item);
    this.selected[type] = new HashSet<ISelectableUI>();
    return false;
  }

  public HashSet<ISelectableUI> GetSelected<T>()
  {
    if (!this.selected.ContainsKey(typeof (T)))
      this.selected[typeof (T)] = new HashSet<ISelectableUI>();
    return this.selected[typeof (T)];
  }

  public void Select(ISelectableUI item, bool clear = true)
  {
    Type type = item.GetType();
    if (!this.selected.ContainsKey(type))
      this.selected[type] = new HashSet<ISelectableUI>();
    if (clear)
      this.selected[type].Clear();
    this.selected[type].Add(item);
  }

  public void Deselect(ISelectableUI item)
  {
    Type type = item.GetType();
    if (!this.selected.ContainsKey(type))
      return;
    this.selected[type].Remove(item);
  }

  public void DeselectAll<T>()
  {
    if (!this.selected.ContainsKey(typeof (T)))
      return;
    this.selected[typeof (T)].Clear();
  }
}
