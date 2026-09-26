// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_ThingOwner
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using Verse;

#nullable disable
namespace SmashTools;

public static class Ext_ThingOwner
{
  public static void Swap<T>(
    this ThingOwner<T> thingOwner1,
    ThingOwner<T> thingOwner2,
    T thing1,
    T thing2)
    where T : Thing
  {
    if (((ThingOwner) thingOwner1).Contains((Thing) thing2) && ((ThingOwner) thingOwner2).Contains((Thing) thing2))
    {
      T obj = thing1;
      thing1 = thing2;
      thing2 = obj;
    }
    ((ThingOwner) thingOwner1).Remove((Thing) thing1);
    ((ThingOwner) thingOwner2).Remove((Thing) thing2);
    ((ThingOwner) thingOwner1).TryAdd((Thing) thing2, true);
    ((ThingOwner) thingOwner2).TryAdd((Thing) thing1, true);
  }
}
