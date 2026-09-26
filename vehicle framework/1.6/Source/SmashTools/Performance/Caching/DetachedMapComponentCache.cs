// Decompiled with JetBrains decompiler
// Type: SmashTools.DetachedMapComponentCache`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace SmashTools;

public static class DetachedMapComponentCache<T> where T : DetachedMapComponent
{
  private static readonly Dictionary<int, T> MapComps = new Dictionary<int, T>();

  public static void AddComponent(Map map)
  {
    T instance = (T) Activator.CreateInstance(typeof (T), (object) map);
    DetachedMapComponentCache<T>.MapComps.Add(map.uniqueID, instance);
  }

  public static T GetComponent(Map map) => DetachedMapComponentCache<T>.MapComps[map.uniqueID];

  public static void ClearMap(Map map)
  {
    DetachedMapComponentCache<T>.MapComps.Remove(map.uniqueID);
  }

  public static void ClearAll() => DetachedMapComponentCache<T>.MapComps.Clear();

  internal static int Count() => DetachedMapComponentCache<T>.MapComps.Count;
}
