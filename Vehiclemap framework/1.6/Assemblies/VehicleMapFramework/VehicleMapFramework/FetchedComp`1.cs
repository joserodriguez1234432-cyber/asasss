// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.FetchedComp`1
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class FetchedComp<T>(ThingWithComps parent, Type compClass = null) where T : ThingComp
{
  private bool fetched;

  public T Value
  {
    get
    {
      if (!this.fetched)
      {
        this.\u003CValue\u003Ek__BackingField = (object) compClass != null ? this.GetComp(compClass) : parent.GetComp<T>();
        this.fetched = true;
      }
      return this.\u003CValue\u003Ek__BackingField;
    }
  }

  public T GetComp(Type type)
  {
    foreach (ThingComp allComp in parent.AllComps)
    {
      if (GenTypes.SameOrSubclassOf(allComp.props.compClass, type) && allComp is T comp)
        return comp;
    }
    return default (T);
  }
}
