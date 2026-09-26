// Decompiled with JetBrains decompiler
// Type: SmashTools.ComponentCache
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;

#nullable disable
namespace SmashTools;

[StaticConstructorOnModInit]
public static class ComponentCache
{
  private static readonly List<Type> PriorityComponentTypes = GenTypes.AllSubclassesNonAbstract(typeof (MapComponent)).ToList<Type>();
  private static readonly List<Type> DetachedComponentTypes = GenTypes.AllSubclassesNonAbstract(typeof (DetachedMapComponent)).ToList<Type>();

  internal static int PriorityComponentTypeCount => ComponentCache.PriorityComponentTypes.Count;

  internal static int DetachedComponentTypeCount => ComponentCache.DetachedComponentTypes.Count;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T GetCachedMapComponent<T>(this Map map) where T : MapComponent
  {
    return MapComponentCache<T>.GetComponent(map);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T GetDetachedMapComponent<T>(this Map map) where T : DetachedMapComponent
  {
    return DetachedMapComponentCache<T>.GetComponent(map);
  }

  internal static void PreCacheInst(Map __instance)
  {
    foreach (Type detachedComponentType in ComponentCache.DetachedComponentTypes)
      GenGeneric.InvokeStaticMethodOnGenericType(typeof (DetachedMapComponentCache<>), detachedComponentType, "AddComponent", new object[1]
      {
        (object) __instance
      });
  }

  internal static void PreCache(Map map)
  {
    foreach (Type detachedComponentType in ComponentCache.DetachedComponentTypes)
      GenGeneric.InvokeStaticMethodOnGenericType(typeof (DetachedMapComponentCache<>), detachedComponentType, "AddComponent", new object[1]
      {
        (object) map
      });
  }

  internal static void ClearMap(Map map)
  {
    foreach (Type priorityComponentType in ComponentCache.PriorityComponentTypes)
      GenGeneric.InvokeStaticMethodOnGenericType(typeof (MapComponentCache<>), priorityComponentType, nameof (ClearMap), new object[1]
      {
        (object) map
      });
    foreach (Type detachedComponentType in ComponentCache.DetachedComponentTypes)
      GenGeneric.InvokeStaticMethodOnGenericType(typeof (DetachedMapComponentCache<>), detachedComponentType, nameof (ClearMap), new object[1]
      {
        (object) map
      });
  }

  internal static void ClearAll()
  {
    foreach (Type priorityComponentType in ComponentCache.PriorityComponentTypes)
      GenGeneric.InvokeStaticMethodOnGenericType(typeof (MapComponentCache<>), priorityComponentType, nameof (ClearAll));
    foreach (Type detachedComponentType in ComponentCache.DetachedComponentTypes)
      GenGeneric.InvokeStaticMethodOnGenericType(typeof (DetachedMapComponentCache<>), detachedComponentType, nameof (ClearAll));
  }

  public static int PriorityComponentCount()
  {
    int num = 0;
    foreach (Type priorityComponentType in ComponentCache.PriorityComponentTypes)
      num += (int) GenGeneric.InvokeStaticMethodOnGenericType(typeof (MapComponentCache<>), priorityComponentType, "Count");
    return num;
  }

  public static int DetachedComponentCount()
  {
    int num = 0;
    foreach (Type detachedComponentType in ComponentCache.DetachedComponentTypes)
      num += (int) GenGeneric.InvokeStaticMethodOnGenericType(typeof (DetachedMapComponentCache<>), detachedComponentType, "Count");
    return num;
  }
}
