// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.AnimationPropertyRegistry
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools.Animations;

public static class AnimationPropertyRegistry
{
  private static readonly Dictionary<IAnimator, List<AnimationPropertyParent>> cachedProperties = new Dictionary<IAnimator, List<AnimationPropertyParent>>();
  private static readonly Dictionary<Type, List<FieldInfo>> fieldRegistry = new Dictionary<Type, List<FieldInfo>>();
  private static readonly Dictionary<Type, List<PropertyInfo>> propertyRegistry = new Dictionary<Type, List<PropertyInfo>>();
  private static readonly Queue<AnimationPropertyRegistry.SearchContext> recursionQueue = new Queue<AnimationPropertyRegistry.SearchContext>();
  private static readonly Dictionary<string, Type> typeNames = new Dictionary<string, Type>();
  private static readonly HashSet<object> processedObjects = new HashSet<object>();

  static AnimationPropertyRegistry()
  {
    AnimationPropertyRegistry.RegisterType<Color>("r", "g", "b", "a");
    AnimationPropertyRegistry.RegisterType<Vector2>("x", "y");
    AnimationPropertyRegistry.RegisterType<Vector3>("x", "y", "z");
    AnimationPropertyRegistry.RegisterType<IntVec2>("x", "z");
    AnimationPropertyRegistry.RegisterType<IntVec3>("x", "y", "z");
  }

  public static bool CachedTypeByName(string name, out Type type)
  {
    return AnimationPropertyRegistry.typeNames.TryGetValue(name, out type);
  }

  internal static void ClearCache()
  {
    AnimationPropertyRegistry.processedObjects.Clear();
    AnimationPropertyRegistry.cachedProperties.Clear();
  }

  private static List<AnimationPropertyParent> RunQueue(IAnimator animator)
  {
    List<AnimationPropertyParent> result = new List<AnimationPropertyParent>();
    AnimationPropertyRegistry.recursionQueue.Enqueue(new AnimationPropertyRegistry.SearchContext((object) animator, Array.Empty<ObjectPath>()));
    while (!AnimationPropertyRegistry.recursionQueue.NullOrEmpty<AnimationPropertyRegistry.SearchContext>())
    {
      AnimationPropertyRegistry.SearchContext context = AnimationPropertyRegistry.recursionQueue.Dequeue();
      AnimationPropertyRegistry.GetAnimationProperties(ref context, result);
    }
    return result;
  }

  public static List<AnimationPropertyParent> GetAnimationProperties(IAnimator animator)
  {
    List<AnimationPropertyParent> animationProperties;
    if (!AnimationPropertyRegistry.cachedProperties.TryGetValue(animator, out animationProperties))
    {
      animationProperties = AnimationPropertyRegistry.RunQueue(animator);
      AnimationPropertyRegistry.cachedProperties.Add(animator, animationProperties);
      AnimationPropertyRegistry.processedObjects.Clear();
    }
    return animationProperties;
  }

  private static void GetAnimationProperties(
    [RequiresLocation, In] ref AnimationPropertyRegistry.SearchContext context,
    List<AnimationPropertyParent> result)
  {
    if (!AnimationPropertyRegistry.processedObjects.Add(context.parent))
      return;
    Type type = context.parent.GetType();
    foreach (FieldInfo field1 in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
    {
      AnimationPropertyAttribute propertyAttribute;
      if (GenAttribute.TryGetAttribute<AnimationPropertyAttribute>((MemberInfo) field1, ref propertyAttribute))
      {
        if (field1.FieldType.IsClass)
        {
          object obj1 = field1.GetValue(context.parent);
          ReadOnlySpan<ObjectPath> readOnlySpan;
          if (obj1 is IList list)
          {
            Type genericArgument = field1.FieldType.GetGenericArguments()[0];
            if (!genericArgument.HasInterface(typeof (IAnimationObject)))
            {
              Log.Error($"{genericArgument} must implement IAnimationObject if it's to be animated from a list.");
            }
            else
            {
              for (int index1 = 0; index1 < list.Count; ++index1)
              {
                object obj2 = list[index1];
                Queue<AnimationPropertyRegistry.SearchContext> recursionQueue = AnimationPropertyRegistry.recursionQueue;
                object parent = obj2;
                ObjectPath[] path1 = context.path;
                int num = 0;
                ObjectPath[] objectPathArray = new ObjectPath[1 + path1.Length];
                readOnlySpan = new ReadOnlySpan<ObjectPath>(path1);
                readOnlySpan.CopyTo(new Span<ObjectPath>(objectPathArray).Slice(num, readOnlySpan.Length));
                int index2 = num + readOnlySpan.Length;
                objectPathArray[index2] = new ObjectPath(field1, index1);
                ObjectPath[] path2 = objectPathArray;
                int index3 = index1;
                AnimationPropertyRegistry.SearchContext searchContext = new AnimationPropertyRegistry.SearchContext(parent, path2, index3);
                recursionQueue.Enqueue(searchContext);
              }
            }
          }
          else
          {
            Queue<AnimationPropertyRegistry.SearchContext> recursionQueue = AnimationPropertyRegistry.recursionQueue;
            object parent = obj1;
            ObjectPath[] path3 = context.path;
            int num = 0;
            ObjectPath[] objectPathArray = new ObjectPath[1 + path3.Length];
            readOnlySpan = new ReadOnlySpan<ObjectPath>(path3);
            readOnlySpan.CopyTo(new Span<ObjectPath>(objectPathArray).Slice(num, readOnlySpan.Length));
            int index = num + readOnlySpan.Length;
            objectPathArray[index] = new ObjectPath(field1);
            ObjectPath[] path4 = objectPathArray;
            AnimationPropertyRegistry.SearchContext searchContext = new AnimationPropertyRegistry.SearchContext(parent, path4);
            recursionQueue.Enqueue(searchContext);
          }
        }
        else if (!AnimationPropertyRegistry.HandlesType(field1.FieldType))
        {
          Log.Error($"Type {field1.FieldType} is not supported as an animation property. It must be registered \r\nin the AnimationPropertyRegistry or be a class type for recursion.");
        }
        else
        {
          string name = propertyAttribute.Name;
          if (name.NullOrEmpty<char>())
            name = field1.Name;
          AnimationPropertyParent animationPropertyParent = AnimationPropertyParent.Create(context.Indexer ? ((IAnimationObject) context.parent).ObjectId : (string) null, name, field1, ((IEnumerable<ObjectPath>) context.path).ToList<ObjectPath>());
          if (AnimationPropertyRegistry.IsSupportedPrimitive(field1.FieldType))
          {
            AnimationProperty property = AnimationProperty.Create(type, name, field1, (ObjectPath) null);
            animationPropertyParent.SetSingle(property);
            result.Add(animationPropertyParent);
          }
          else if (AnimationPropertyRegistry.IsContainerProperty(field1.FieldType))
          {
            foreach (FieldInfo field2 in field1.FieldType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
              if (!AnimationPropertyRegistry.HandlesType(field2.FieldType) || !AnimationPropertyRegistry.IsSupportedPrimitive(field2.FieldType))
              {
                Log.Error($"Type {field2.FieldType} is not supported as an animation property. Nested fields must be a \r\nsupported primitive type {{ int, float, bool }}");
              }
              else
              {
                AnimationProperty property = AnimationProperty.Create(type, field2.Name, field2, (ObjectPath) field1);
                animationPropertyParent.Add(property);
              }
            }
            result.Add(animationPropertyParent);
          }
        }
      }
    }
  }

  private static bool IsSupportedPrimitive(Type type)
  {
    return type == typeof (float) || type == typeof (int) || type == typeof (bool);
  }

  private static bool IsContainerProperty(Type type)
  {
    return AnimationPropertyRegistry.fieldRegistry.ContainsKey(type) || AnimationPropertyRegistry.propertyRegistry.ContainsKey(type);
  }

  public static bool HandlesType(Type type)
  {
    return AnimationPropertyRegistry.IsSupportedPrimitive(type) || AnimationPropertyRegistry.fieldRegistry.ContainsKey(type) || AnimationPropertyRegistry.propertyRegistry.ContainsKey(type);
  }

  public static void RegisterType<T>(params string[] fieldNames)
  {
    if (AnimationPropertyRegistry.fieldRegistry.ContainsKey(typeof (T)))
      Log.Error($"{typeof (T)} has already been registered. Skipping to avoid duplicate field entries.");
    else if (((IEnumerable<string>) fieldNames).NullOrEmpty<string>())
    {
      Log.Error("Trying to register AnimationProperty in registry with no fields.");
    }
    else
    {
      AnimationPropertyRegistry.typeNames[GenTypes.GetTypeNameWithoutIgnoredNamespaces(typeof (T))] = typeof (T);
      foreach (string fieldName in fieldNames)
      {
        FieldInfo fieldInfo = AccessTools.Field(typeof (T), fieldName);
        if (fieldInfo == (FieldInfo) null)
          Log.Error($"Unable to locate {typeof (T)}.{fieldName}");
        else
          AnimationPropertyRegistry.fieldRegistry.AddOrAppend<Type, List<FieldInfo>, FieldInfo>(typeof (T), fieldInfo);
      }
    }
  }

  private readonly struct SearchContext(object parent, ObjectPath[] path, int index = -1)
  {
    public readonly object parent = parent;
    public readonly ObjectPath[] path = path;
    public readonly int index = index;

    public bool Indexer => this.index >= 0;
  }
}
