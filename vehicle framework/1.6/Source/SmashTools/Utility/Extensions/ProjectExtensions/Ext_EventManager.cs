// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_EventManager
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace SmashTools;

[PublicAPI]
public static class Ext_EventManager
{
  public static void FillEvents<T>(this IEventManager<T> manager, IEnumerable<T> events)
  {
    manager.EventRegistry = new EventManager<T>();
    foreach (T @event in events)
      manager.RegisterEventType<T>(@event);
  }

  public static void FillEventsEnum<T>(this IEventManager<T> manager)
  {
    if (!typeof (T).IsEnum)
      throw new ArgumentException($"Tried to fill IEventManager with enum values and non-enum type. Type=\"{typeof (T)}\" Manager=\"{manager}\"");
    manager.EventRegistry = new EventManager<T>();
    foreach (T @event in Enum.GetValues(typeof (T)))
      manager.RegisterEventType<T>(@event);
  }

  public static void FillEventsDef<T>(this IEventManager<T> manager) where T : Def
  {
    manager.EventRegistry = new EventManager<T>();
    foreach (T @event in DefDatabase<T>.AllDefsListForReading)
      manager.RegisterEventType<T>(@event);
  }

  public static void RegisterEventType<T>(this IEventManager<T> manager, T @event)
  {
    manager.EventRegistry[@event] = new EventTrigger((IEventControl) manager.EventRegistry);
  }

  public static void AddEvent<T>(
    this IEventManager<T> manager,
    T @event,
    Action action,
    params Action[] actions)
  {
    manager.EventRegistry[@event].Add((string) null, action);
    if (((IEnumerable<Action>) actions).NullOrEmpty<Action>())
      return;
    foreach (Action action1 in actions)
      manager.EventRegistry[@event].Add((string) null, action1);
  }

  public static void AddEvent<T>(
    this IEventManager<T> manager,
    T @event,
    Action action,
    string key,
    params Action[] actions)
  {
    manager.EventRegistry[@event].Add(key, action);
    if (((IEnumerable<Action>) actions).NullOrEmpty<Action>())
      return;
    foreach (Action action1 in actions)
      manager.EventRegistry[@event].Add(key, action1);
  }

  public static void AddSingleEvent<T>(
    this IEventManager<T> manager,
    T @event,
    Action action,
    params Action[] actions)
  {
    manager.EventRegistry[@event].AddSingle((string) null, action);
    if (((IEnumerable<Action>) actions).NullOrEmpty<Action>())
      return;
    foreach (Action action1 in actions)
      manager.EventRegistry[@event].AddSingle((string) null, action1);
  }

  public static void AddSingleEvent<T>(
    this IEventManager<T> manager,
    T @event,
    Action action,
    string key,
    params Action[] actions)
  {
    manager.EventRegistry[@event].AddSingle(key, action);
    if (((IEnumerable<Action>) actions).NullOrEmpty<Action>())
      return;
    foreach (Action action1 in actions)
      manager.EventRegistry[@event].AddSingle(key, action1);
  }

  public static void RemoveEvent<T>(this IEventManager<T> manager, T @event, string key)
  {
    manager.EventRegistry[@event].Remove(key);
  }

  public static void RemoveEvent<T>(this IEventManager<T> manager, T @event, Action action)
  {
    manager.EventRegistry[@event].Remove(action);
  }

  public static void RemoveSingleEvent<T>(this IEventManager<T> manager, T @event, string key)
  {
    manager.EventRegistry[@event].RemoveSingle(key);
  }

  public static void RemoveSingleEvent<T>(this IEventManager<T> manager, T @event, Action action)
  {
    manager.EventRegistry[@event].RemoveSingle(action);
  }

  public static void ClearAll<T>(this IEventManager<T> manager, T @event)
  {
    manager.EventRegistry[@event].ClearAll();
  }

  public static bool Initialized<T>(this EventManager<T> manager)
  {
    return manager != null && !manager.map.NullOrEmpty<KeyValuePair<T, EventTrigger>>();
  }
}
