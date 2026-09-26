// Decompiled with JetBrains decompiler
// Type: SmashTools.EventTrigger
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;

#nullable disable
namespace SmashTools;

[PublicAPI]
public class EventTrigger : IEventControl
{
  private bool enabled = true;
  private readonly List<EventTrigger.Trigger> persistents = new List<EventTrigger.Trigger>();
  private readonly List<EventTrigger.Trigger> singles = new List<EventTrigger.Trigger>();
  private readonly IEventControl manager;

  public EventTrigger(IEventControl manager) => this.manager = manager;

  public bool Enabled
  {
    get => this.enabled;
    private set => this.enabled = value;
  }

  bool IEventControl.Enabled
  {
    get => this.enabled;
    set => this.enabled = value;
  }

  public int TotalEventCount => this.singles.Count + this.persistents.Count;

  public bool Contains(string key)
  {
    foreach (EventTrigger.Trigger persistent in this.persistents)
    {
      if (key != null && persistent.key != null && key == persistent.key)
        return true;
    }
    return false;
  }

  public bool Contains(Action action)
  {
    foreach (EventTrigger.Trigger persistent in this.persistents)
    {
      if (persistent.action == action)
        return true;
    }
    return false;
  }

  public void Add(string key, Action action)
  {
    this.persistents.Add(new EventTrigger.Trigger(key, action));
  }

  public void AddSingle(string key, Action action)
  {
    this.singles.Add(new EventTrigger.Trigger(key, action));
  }

  public int Remove(string key)
  {
    int num = 0;
    for (int index = this.persistents.Count - 1; index >= 0; --index)
    {
      if (this.persistents[index].key == key)
      {
        this.persistents.RemoveAt(index);
        ++num;
      }
    }
    return num;
  }

  public int Remove(Action action)
  {
    int num = 0;
    for (int index = this.persistents.Count - 1; index >= 0; --index)
    {
      if (this.persistents[index].action == action)
      {
        this.persistents.RemoveAt(index);
        ++num;
      }
    }
    return num;
  }

  public int RemoveSingle(string key)
  {
    int num = 0;
    for (int index = this.singles.Count - 1; index >= 0; --index)
    {
      if (this.singles[index].key == key)
      {
        this.singles.RemoveAt(index);
        ++num;
      }
    }
    return num;
  }

  public int RemoveSingle(Action action)
  {
    int num = 0;
    for (int index = this.singles.Count - 1; index >= 0; --index)
    {
      if (this.singles[index].action == action)
      {
        this.singles.RemoveAt(index);
        ++num;
      }
    }
    return num;
  }

  public void ClearAll()
  {
    this.singles.Clear();
    this.persistents.Clear();
  }

  public void ExecuteEvents()
  {
    if (!this.Enabled || !this.manager.Enabled)
      return;
    foreach (EventTrigger.Trigger persistent in this.persistents)
      persistent.action();
    for (int index = this.singles.Count - 1; index >= 0; --index)
    {
      this.singles[index].action();
      this.singles.RemoveAt(index);
    }
  }

  private readonly struct Trigger(string key, Action action)
  {
    public readonly string key = key;
    public readonly Action action = action;
  }
}
