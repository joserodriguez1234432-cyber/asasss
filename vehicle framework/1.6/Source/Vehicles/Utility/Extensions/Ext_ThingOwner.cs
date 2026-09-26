// Decompiled with JetBrains decompiler
// Type: Vehicles.Ext_ThingOwner
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using Verse;

#nullable disable
namespace Vehicles;

public static class Ext_ThingOwner
{
  public static int CountWhere<T>(this ThingOwner<T> list, Predicate<T> predicate) where T : Thing
  {
    int num = 0;
    foreach (T obj in list)
    {
      if (predicate(obj))
        ++num;
    }
    return num;
  }
}
