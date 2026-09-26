// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_Comp
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace SmashTools;

public static class Ext_Comp
{
  private static readonly AccessTools.FieldRef<ThingWithComps, List<ThingComp>> CompList = AccessTools.FieldRefAccess<ThingWithComps, List<ThingComp>>("comps");

  [Pure]
  public static bool TryAddComp<T>(this ThingWithComps thingWithComps, T comp) where T : ThingComp
  {
    try
    {
      thingWithComps.EnsureUncachedCompList();
      thingWithComps.AllComps.Add((ThingComp) comp);
      return true;
    }
    catch (Exception ex)
    {
      SmashLog.Error($"Exception thrown while trying to reflectively add <type>{((object) comp).GetType()}</type> to {thingWithComps}.\nException={ex}");
    }
    return false;
  }

  private static void EnsureUncachedCompList(this ThingWithComps thingWithComps)
  {
    try
    {
      ref List<ThingComp> local = ref Ext_Comp.CompList.Invoke(thingWithComps);
      if (local != null)
        return;
      local = new List<ThingComp>();
    }
    catch (Exception ex)
    {
      SmashLog.Error($"Exception thrown while trying to uncache compList for {thingWithComps}.\nException={ex}");
    }
  }
}
