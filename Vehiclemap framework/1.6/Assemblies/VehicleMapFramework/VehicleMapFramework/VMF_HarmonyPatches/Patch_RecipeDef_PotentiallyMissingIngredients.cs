// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_RecipeDef_PotentiallyMissingIngredients
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (RecipeDef), "PotentiallyMissingIngredients")]
[PatchLevel(Level.Safe)]
public static class Patch_RecipeDef_PotentiallyMissingIngredients
{
  public static IEnumerable<ThingDef> Postfix(
    IEnumerable<ThingDef> values,
    Pawn billDoer,
    Map map,
    RecipeDef __instance)
  {
    return values.Select(thingDef => new
    {
      thingDef = thingDef,
      found = __instance.ingredients.Where<IngredientCount>((Func<IngredientCount, bool>) (ing => ing.IsFixedIngredient && thingDef == ing.FixedIngredient || ing.filter.Allows(thingDef))).Any<IngredientCount>((Func<IngredientCount, bool>) (ing => map.BaseMapAndVehicleMaps(false).Any<Map>((Func<Map, bool>) (map2 => map2.listerThings.ThingsInGroup((ThingRequestGroup) 3).Exists((Predicate<Thing>) (t => t.def == thingDef && (billDoer == null || !ForbidUtility.IsForbidden(t, billDoer)) && !GridsUtility.Fogged(t.Position, map2) && (ing.IsFixedIngredient || __instance.fixedIngredientFilter.Allows(t)) && ing.filter.Allows(t)))))))
    }).Where(_param1 => !_param1.found).Select(_param1 => _param1.thingDef);
  }
}
