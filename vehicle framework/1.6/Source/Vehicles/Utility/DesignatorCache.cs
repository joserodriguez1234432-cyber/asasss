// Decompiled with JetBrains decompiler
// Type: Vehicles.DesignatorCache
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public static class DesignatorCache
{
  private static Dictionary<System.Type, Designator> designators = new Dictionary<System.Type, Designator>();

  public static T Get<T>() where T : Designator
  {
    Designator instance;
    if (!DesignatorCache.designators.TryGetValue(typeof (T), out instance))
    {
      instance = (Designator) Activator.CreateInstance(typeof (T));
      DesignatorCache.designators[typeof (T)] = instance;
    }
    return (T) instance;
  }

  public static Designator Get(System.Type type)
  {
    if (!type.IsSubclassOf(typeof (Designator)))
    {
      Log.Error($"Attempting to retrieve {type} from DesignatorCache. Type must be of Designator type.");
      return (Designator) null;
    }
    Designator instance;
    if (!DesignatorCache.designators.TryGetValue(type, out instance))
    {
      instance = (Designator) Activator.CreateInstance(type);
      DesignatorCache.designators[type] = instance;
    }
    return instance;
  }
}
