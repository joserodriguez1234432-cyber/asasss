// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_Type
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

#nullable disable
namespace SmashTools;

[PublicAPI]
public static class Ext_Type
{
  public static bool IsNumericType([NotNull] this Type type)
  {
    if (type == (Type) null)
      throw new ArgumentNullException(nameof (type));
    switch (Type.GetTypeCode(type))
    {
      case TypeCode.SByte:
      case TypeCode.Byte:
      case TypeCode.Int16:
      case TypeCode.UInt16:
      case TypeCode.Int32:
      case TypeCode.UInt32:
      case TypeCode.Int64:
      case TypeCode.UInt64:
      case TypeCode.Single:
      case TypeCode.Double:
      case TypeCode.Decimal:
        return true;
      default:
        return false;
    }
  }

  public static bool IsIList(this Type type)
  {
    foreach (Type type1 in type.GetInterfaces())
    {
      if (type1.IsGenericType && type1.GetGenericTypeDefinition() == typeof (IList<>))
        return true;
    }
    return false;
  }

  public static void SetStaticFieldsDefault(this Type type)
  {
    foreach (FieldInfo field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
    {
      if (!field.IsInitOnly)
        field.SetValue((object) null, field.FieldType.GetDefaultValue());
    }
  }

  public static object GetDefaultValue(this Type type)
  {
    return !type.IsValueType ? (object) null : Activator.CreateInstance(type);
  }

  public static bool HasInterface(this Type type, Type interfaceType)
  {
    if (!interfaceType.IsInterface)
    {
      Log.Error($"Attempting to find type implementation as interface for non-interface type {interfaceType}.");
      return false;
    }
    if (type == interfaceType)
      return false;
    if (interfaceType.IsAssignableFrom(type))
      return true;
    foreach (Type type1 in type.GetInterfaces())
    {
      if (type1.IsGenericType && type1.GetGenericTypeDefinition() == interfaceType)
        return true;
    }
    return false;
  }

  public static IEnumerable<Type> AllInterfaceClassImplementations<T>(this ModContentPack mod)
  {
    foreach (Assembly loadedAssembly in mod.assemblies.loadedAssemblies)
    {
      Type[] typeArray = loadedAssembly.GetTypes();
      for (int index = 0; index < typeArray.Length; ++index)
      {
        Type type = typeArray[index];
        if (type.HasInterface(typeof (T)) && type.IsClass && !type.IsAbstract)
          yield return type;
      }
      typeArray = (Type[]) null;
    }
  }
}
