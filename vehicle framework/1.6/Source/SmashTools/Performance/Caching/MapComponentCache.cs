// Decompiled with JetBrains decompiler
// Type: SmashTools.MapComponentCache`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace SmashTools;

public static class MapComponentCache<T> where T : MapComponent
{
  private static T recentAccess;
  private static readonly Dictionary<int, T> MapComps = new Dictionary<int, T>();

  public static T GetComponent(Map map)
  {
    if (map.Disposed)
      return default (T);
    if ((object) MapComponentCache<T>.recentAccess != null && MapComponentCache<T>.recentAccess.map.uniqueID == map.uniqueID)
      return MapComponentCache<T>.recentAccess;
    T component;
    if (!MapComponentCache<T>.MapComps.TryGetValue(map.uniqueID, out component))
    {
      component = map.GetComponent<T>();
      MapComponentCache<T>.MapComps[map.uniqueID] = component;
    }
    MapComponentCache<T>.recentAccess = component;
    return component;
  }

  public static void ClearMap(Map map)
  {
    MapComponentCache<T>.recentAccess = default (T);
    MapComponentCache<T>.MapComps.Remove(map.uniqueID);
  }

  public static void ClearAll()
  {
    MapComponentCache<T>.recentAccess = default (T);
    MapComponentCache<T>.MapComps.Clear();
  }

  public static int ClearAllDisposed()
  {
    MapComponentCache<T>.recentAccess = default (T);
    List<int> intList = new List<int>();
    foreach (KeyValuePair<int, T> mapComp in MapComponentCache<T>.MapComps)
    {
      int num1;
      T obj;
      mapComp.Deconstruct(ref num1, ref obj);
      int num2 = num1;
      Map map = obj?.map;
      if (map == null || map.Disposed)
        intList.Add(num2);
    }
    foreach (int key in intList)
      MapComponentCache<T>.MapComps.Remove(key);
    return intList.Count;
  }

  internal static T GetComponent(int mapId)
  {
    return GenCollection.TryGetValue<int, T>((IReadOnlyDictionary<int, T>) MapComponentCache<T>.MapComps, mapId, default (T));
  }

  internal static int Count()
  {
    return MapComponentCache<T>.MapComps.Values.CountWhere<T>((Predicate<T>) (item => (object) item != null));
  }
}
