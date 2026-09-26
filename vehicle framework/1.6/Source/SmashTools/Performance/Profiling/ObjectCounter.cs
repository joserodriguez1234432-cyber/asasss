// Decompiled with JetBrains decompiler
// Type: SmashTools.Performance.ObjectCounter
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

#nullable disable
namespace SmashTools.Performance;

public static class ObjectCounter
{
  private static readonly ConcurrentDictionary<Type, int> counter = new ConcurrentDictionary<Type, int>();
  private static readonly ConcurrentDictionary<Type, int> countWatched = new ConcurrentDictionary<Type, int>();

  public static bool Clear<T>() => ObjectCounter.counter.TryRemove(typeof (T), out int _);

  public static void ClearAll() => ObjectCounter.counter.Clear();

  public static void Increment<T>()
  {
    if (!ObjectCounter.counter.ContainsKey(typeof (T)))
      ObjectCounter.counter[typeof (T)] = 0;
    ObjectCounter.counter[typeof (T)]++;
  }

  public static void LogAll()
  {
    foreach (Type key in (IEnumerable<Type>) ObjectCounter.counter.Keys)
      ObjectCounter.Log(key);
  }

  public static void Log(Type type)
  {
    int num;
    if (!ObjectCounter.counter.TryGetValue(type, out num))
      num = 0;
    Verse.Log.Message($"{type.Name} = {num}");
  }

  public static void StartWatcher<T>()
  {
    int num;
    if (!ObjectCounter.counter.TryGetValue(typeof (T), out num))
      num = 0;
    ObjectCounter.countWatched.TryAdd(typeof (T), num);
  }

  public static int GetWatchedCount<T>()
  {
    int num1;
    if (!ObjectCounter.countWatched.TryGetValue(typeof (T), out num1))
    {
      Trace.Fail("Ending watcher which hasn't been started.");
      return 0;
    }
    int num2;
    if (!ObjectCounter.counter.TryGetValue(typeof (T), out num2))
      num2 = 0;
    return num2 - num1;
  }

  public static int EndWatcher<T>()
  {
    int watchedCount = ObjectCounter.GetWatchedCount<T>();
    ObjectCounter.countWatched.TryRemove(typeof (T), out int _);
    return watchedCount;
  }
}
