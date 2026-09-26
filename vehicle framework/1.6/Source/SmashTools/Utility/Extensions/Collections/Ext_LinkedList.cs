// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_LinkedList
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace SmashTools;

public static class Ext_LinkedList
{
  public static void Populate<T>(this LinkedList<T> list, IEnumerable<T> collection)
  {
    if (list == null)
      throw new ArgumentNullException(nameof (list));
    if (collection == null)
      throw new ArgumentNullException(nameof (collection));
    foreach (T obj in collection)
      list.AddLast(obj);
  }

  public static LinkedListNode<T> Pop<T>(this LinkedList<T> list)
  {
    LinkedListNode<T> first = list.First;
    list.RemoveFirst();
    return first;
  }
}
