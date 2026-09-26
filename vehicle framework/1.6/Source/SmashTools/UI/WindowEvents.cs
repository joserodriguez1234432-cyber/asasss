// Decompiled with JetBrains decompiler
// Type: SmashTools.WindowEvents
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace SmashTools;

public static class WindowEvents
{
  private static readonly WindowEvents.EventDataCache EventCache = new WindowEvents.EventDataCache();
  private static readonly List<IHighPriorityOnGUI> HighPriorityOnGui = new List<IHighPriorityOnGUI>();

  public static void Register(
    this IWindowEventListener listener,
    Window sender,
    WindowEvents.OnEvent onEvent,
    WindowEvents.Event ev)
  {
    if (sender == null)
      throw new ArgumentNullException(nameof (sender));
    WindowEvents.EventCache.RegisterImpl(listener, sender, onEvent, ev);
  }

  public static void Deregister(this IWindowEventListener listener)
  {
    WindowEvents.EventCache.DeregisterImpl(listener);
  }

  internal static void WindowAddedToStack(Window window)
  {
    if (window is IHighPriorityOnGUI highPriorityOnGui)
      WindowEvents.HighPriorityOnGui.Add(highPriorityOnGui);
    WindowEvents.EventCache.Raise(WindowEvents.Event.Opened);
  }

  internal static void WindowRemovedFromStack(Window window, bool __result)
  {
    if (!__result)
      return;
    if (window is IHighPriorityOnGUI highPriorityOnGui)
      WindowEvents.HighPriorityOnGui.Remove(highPriorityOnGui);
    WindowEvents.EventCache.Raise(WindowEvents.Event.Closed);
  }

  internal static void HighPriorityOnGUI()
  {
    for (int index = WindowEvents.HighPriorityOnGui.Count - 1; index >= 0; --index)
      WindowEvents.HighPriorityOnGui[index].OnGUIHighPriority();
  }

  public delegate void OnEvent();

  public enum Event
  {
    Closed,
    Opened,
  }

  private class EventDataCache
  {
    private readonly List<WindowEvents.EventDataCache.Data> observerData = new List<WindowEvents.EventDataCache.Data>();
    private readonly List<IWindowEventListener> listenersToRemove = new List<IWindowEventListener>();
    private bool eventRaising;

    public void RegisterImpl(
      IWindowEventListener listener,
      Window sender,
      WindowEvents.OnEvent onEvent,
      WindowEvents.Event ev)
    {
      WindowEvents.EventDataCache.Data data1 = GenCollection.FirstOrDefault<WindowEvents.EventDataCache.Data>(this.observerData, (Predicate<WindowEvents.EventDataCache.Data>) (data => data.listener == listener));
      if (data1 == null)
      {
        data1 = new WindowEvents.EventDataCache.Data(listener, sender);
        this.observerData.Add(data1);
      }
      data1.events[ev] = onEvent;
    }

    public void DeregisterImpl(IWindowEventListener listener)
    {
      for (int index = this.observerData.Count - 1; index >= 0; --index)
      {
        if (this.observerData[index].listener == listener)
        {
          this.observerData.RemoveAt(index);
          break;
        }
      }
    }

    private void FlagForRemoval(IWindowEventListener listener)
    {
      this.listenersToRemove.Add(listener);
    }

    public void Raise(WindowEvents.Event ev)
    {
      using (new ScopedValueRollback<bool>(ref this.eventRaising))
      {
        foreach (WindowEvents.EventDataCache.Data data in this.observerData)
        {
          WindowEvents.OnEvent onEvent;
          if (data.events.TryGetValue(ev, out onEvent))
            onEvent();
        }
      }
      this.RemoveListeners();
    }

    private void RemoveListeners()
    {
      foreach (IWindowEventListener listener in this.listenersToRemove)
        listener.Deregister();
      this.listenersToRemove.Clear();
    }

    private class Data(IWindowEventListener listener, Window sender)
    {
      public readonly IWindowEventListener listener = listener;
      public readonly Window sender = sender;
      public readonly Dictionary<WindowEvents.Event, WindowEvents.OnEvent> events = new Dictionary<WindowEvents.Event, WindowEvents.OnEvent>();
    }
  }
}
