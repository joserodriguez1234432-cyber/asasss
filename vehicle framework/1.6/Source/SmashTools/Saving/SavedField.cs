// Decompiled with JetBrains decompiler
// Type: SmashTools.SavedField`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using Verse;

#nullable disable
namespace SmashTools;

public struct SavedField<T1> : IEquatable<SavedField<T1>>, INestedType
{
  private T1 value;
  private T1 endValue;

  public SavedField(T1 value)
  {
    this.value = value;
    this.endValue = value;
  }

  public SavedField(T1 value, T1 endValue)
  {
    this.value = value;
    this.endValue = endValue;
  }

  public Type InnerType => this.value.GetType();

  public T1 First
  {
    get => this.value;
    set
    {
      if ((object) value != null)
        this.value = value;
      else
        Log.Error($"Tried to assign value of different type to SavedField. T1: {typeof (T1)?.ToString()} value: {value.GetType()?.ToString()}");
    }
  }

  public T1 EndValue
  {
    get => this.endValue;
    set
    {
      if ((object) value != null)
        this.endValue = value;
      else
        Log.Error($"Tried to assign value of different type to SavedField. T2: {typeof (T1)?.ToString()} value: {value.GetType()?.ToString()}");
    }
  }

  public override string ToString() => $"({this.value},{this.endValue})";

  public static object FromTypedString(string entry, Type objType)
  {
    entry = entry.TrimStart('(').TrimEnd(')');
    string[] strArray = entry.Split(',');
    try
    {
      CultureInfo invariantCulture = CultureInfo.InvariantCulture;
      return (object) new SavedField<object>(AccessTools.Method(typeof (ParseHelper), "FromString", new Type[2]
      {
        typeof (string),
        typeof (Type)
      }, (Type[]) null).Invoke((object) null, new object[2]
      {
        (object) strArray[0],
        (object) objType
      }), AccessTools.Method(typeof (ParseHelper), "FromString", new Type[2]
      {
        typeof (string),
        typeof (Type)
      }, (Type[]) null).Invoke((object) null, new object[2]
      {
        (object) strArray[1],
        (object) objType
      }));
    }
    catch (Exception ex)
    {
      Log.Error($"{entry} is not a valid SavedField format. Exception: {ex}");
      return (object) new SavedField<object>((object) null);
    }
  }

  public override int GetHashCode()
  {
    int hashCode = 0;
    if ((object) this.First != null)
      hashCode = EqualityComparer<T1>.Default.GetHashCode(this.First);
    if ((object) this.EndValue != null)
      hashCode = (hashCode << 3) + hashCode ^ EqualityComparer<T1>.Default.GetHashCode(this.EndValue);
    return hashCode;
  }

  public override bool Equals(object obj) => obj is SavedField<T1> other && this.Equals(other);

  public bool Equals(SavedField<T1> other)
  {
    return EqualityComparer<T1>.Default.Equals(this.value, other.value) && EqualityComparer<T1>.Default.Equals(this.endValue, other.endValue);
  }

  public static bool operator ==(SavedField<T1> lhs, SavedField<T1> rhs) => lhs.Equals(rhs);

  public static bool operator !=(SavedField<T1> lhs, SavedField<T1> rhs) => !(lhs == rhs);
}
