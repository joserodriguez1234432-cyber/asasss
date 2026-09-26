// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_IList
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace SmashTools;

public static class Ext_IList
{
  public static void Populate<T>(this IList<T> list, T value, int count)
  {
    list.Clear();
    for (int index = 0; index < count; ++index)
      list.Add(value);
  }

  public static T PopRandom<T>(this IList<T> list)
  {
    if (!list.NotNullAndAny<T>())
      return default (T);
    Rand.PushState();
    int index = Rand.Range(0, list.Count);
    T obj = list.PopAt<T>(index);
    Rand.PopState();
    return obj;
  }

  public static T PopAt<T>(this IList<T> list, int index)
  {
    T obj = list[index];
    list.RemoveAt(index);
    return obj;
  }

  public static T Next<T>(this IList<T> list, T current)
  {
    int index = list.IndexOf(current) + 1;
    if (index >= list.Count)
      index = 0;
    return list[index];
  }

  public static List<T> ReorderOn<T>(this List<T> list, T item)
  {
    int count = list.IndexOf(item);
    if (count < 0)
    {
      Log.Error("Unable to ReorderOn list, item does not exist.");
      return list;
    }
    List<T> range = list.GetRange(0, count);
    List<T> objList = new List<T>((IEnumerable<T>) list.GetRange(count + 1, list.Count - 1));
    objList.AddRange((IEnumerable<T>) range);
    return objList;
  }

  public static bool OutOfBounds<T>(this IList<T> list, int index)
  {
    return index < 0 || index >= list.Count;
  }

  [ContractAnnotation("list:null => false;")]
  public static bool NotNullAndAny<T>(this List<T> list, Predicate<T> predicate = null)
  {
    if (list == null)
      return false;
    return predicate != null ? list.Exists(predicate) : list.Count > 0;
  }
}
