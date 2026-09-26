// Decompiled with JetBrains decompiler
// Type: SmashTools.EventListener`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;

#nullable disable
namespace SmashTools;

public class EventListener<T> : IDisposable
{
  private readonly IEventManager<T> eventManager;
  private readonly T key;
  private int eventsRaised;

  public EventListener(IEventManager<T> eventManager, T key)
  {
    this.eventManager = eventManager;
    this.key = key;
    this.eventsRaised = 0;
    this.eventManager.AddEvent<T>(key, new Action(this.EventRaised));
  }

  public int CountRaised => this.eventsRaised;

  public void Dispose()
  {
    IEventManager<T> eventManager = this.eventManager;
    if (eventManager == null)
      return;
    eventManager.RemoveEvent<T>(this.key, new Action(this.EventRaised));
  }

  private void EventRaised() => ++this.eventsRaised;
}
