// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Map_ExposeData
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_MultiFloors")]
[HarmonyPatch(typeof (Map), "ExposeData")]
[PatchLevel(Level.Mandatory)]
public static class Patch_Map_ExposeData
{
  private static readonly AccessTools.FieldRef<Map, List<Thing>> loadedFullThings = AccessTools.FieldRefAccess<Map, List<Thing>>(nameof (loadedFullThings));

  public static void Postfix(Map __instance, List<Thing> ___loadedFullThings)
  {
    if (Scribe.mode != 2)
      return;
    List<string> thingIDs = ___loadedFullThings.OfType<Pawn>().Select<Pawn, string>((Func<Pawn, string>) (t => ((Thing) t).ThingID)).ToList<string>();
    foreach (string str in thingIDs.GroupBy<string, string>((Func<string, string>) (id => id)).Where<IGrouping<string, string>>((Func<IGrouping<string, string>, bool>) (id => id.Count<string>() > 1)).Select<IGrouping<string, string>, string>((Func<IGrouping<string, string>, string>) (group => group.Key)).ToList<string>())
    {
      string duplicate = str;
      Thing last = ___loadedFullThings.FindLast((Predicate<Thing>) (t => t.ThingID == duplicate));
      if (last != null)
      {
        VMF_Log.Warning($"Duplicated pawn found: {last}");
        ___loadedFullThings.Remove(last);
      }
    }
    foreach (Map map in Find.Maps)
    {
      if (map != __instance)
      {
        thingIDs.Clear();
        thingIDs.AddRange(Patch_Map_ExposeData.loadedFullThings.Invoke(map).OfType<Pawn>().Select<Pawn, string>((Func<Pawn, string>) (t => ((Thing) t).ThingID)));
        Thing last = ___loadedFullThings.FindLast((Predicate<Thing>) (t => thingIDs.Contains(t.ThingID)));
        if (last != null)
        {
          VMF_Log.Warning($"Duplicated pawn found: {last}");
          ___loadedFullThings.Remove(last);
        }
      }
    }
  }
}
