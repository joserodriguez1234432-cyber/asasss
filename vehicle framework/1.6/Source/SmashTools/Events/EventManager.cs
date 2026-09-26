// Decompiled with JetBrains decompiler
// Type: SmashTools.EventManager`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System.Collections.Generic;

#nullable disable
namespace SmashTools;

[PublicAPI]
public class EventManager<T> : IEventControl
{
  private bool enabled = true;
  public readonly Dictionary<T, EventTrigger> map = new Dictionary<T, EventTrigger>();

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

  public EventTrigger this[T key]
  {
    get
    {
      if (!this.map.ContainsKey(key))
        this.map[key] = new EventTrigger((IEventControl) this);
      return this.map[key];
    }
    set => this.map[key] = value;
  }
}
