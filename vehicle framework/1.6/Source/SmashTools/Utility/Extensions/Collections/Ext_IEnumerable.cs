// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_IEnumerable
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace SmashTools;

public static class Ext_IEnumerable
{
  public static T RandomOrDefault<T>(this IEnumerable<T> enumerable, Predicate<T> predicate = null)
  {
    T obj;
    return GenCollection.TryRandomElement<T>(enumerable.Where<T>((Func<T, bool>) (item => predicate == null || predicate(item))), ref obj) ? obj : default (T);
  }

  public static T RandomOrFallback<T>(
    this IEnumerable<T> enumerable,
    Predicate<T> predicate = null,
    T fallback = null)
  {
    T obj;
    return GenCollection.TryRandomElement<T>(enumerable.Where<T>((Func<T, bool>) (item => predicate == null || predicate(item))), ref obj) ? obj : fallback;
  }

  [ContractAnnotation("enumerable:null => false;")]
  public static bool NotNullAndAny<T>(this IEnumerable<T> enumerable, Predicate<T> predicate = null)
  {
    if (enumerable == null)
      return false;
    return predicate != null ? enumerable.Any<T>((Func<T, bool>) (e => predicate(e))) : enumerable.Any<T>();
  }

  [ContractAnnotation("enumerable:null => false;")]
  public static bool NullOrEmpty<T>(this IEnumerable<T> enumerable)
  {
    return enumerable == null || !enumerable.Any<T>();
  }

  public static int CountWhere<T>(this IEnumerable<T> list, [NotNull] Predicate<T> predicate)
  {
    int num = 0;
    foreach (T obj in list)
    {
      if (predicate(obj))
        ++num;
    }
    return num;
  }

  public static bool ContainsAllOf<T>(this IEnumerable<T> source, IEnumerable<T> target)
  {
    if (source == null)
      throw new ArgumentNullException(nameof (source));
    if (target == null)
      throw new ArgumentNullException(nameof (target));
    if (source is ICollection<T> objs && objs.Count >= 25)
    {
      HashSet<T> hashSet = source.ToHashSet<T>();
      foreach (T obj in target)
      {
        if (!hashSet.Contains(obj))
          return false;
      }
      return true;
    }
    foreach (T obj in target)
    {
      if (!source.Contains<T>(obj))
        return false;
    }
    return true;
  }

  public static RotatingList<T> ToRotatingList<T>(this IEnumerable<T> sourceCollection)
  {
    return new RotatingList<T>(sourceCollection);
  }

  public static string ToReadableString<T>(this IEnumerable<T> enumerable)
  {
    return string.Join<T>(",", enumerable);
  }
}
