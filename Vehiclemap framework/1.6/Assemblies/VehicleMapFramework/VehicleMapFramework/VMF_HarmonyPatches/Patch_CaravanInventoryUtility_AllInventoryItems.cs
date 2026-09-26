// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_CaravanInventoryUtility_AllInventoryItems
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Vehicles.World;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (CaravanInventoryUtility), "AllInventoryItems")]
[HarmonyPriority(600)]
public static class Patch_CaravanInventoryUtility_AllInventoryItems
{
  [PatchLevel(Level.Mandatory)]
  [HarmonyReversePatch]
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static List<Thing> AllInventoryItems(Caravan caravan)
  {
    throw new NotImplementedException();
  }

  [PatchLevel(Level.Safe)]
  public static void Postfix(Caravan caravan, List<Thing> __result)
  {
    if (!VehicleMapFramework.VehicleMapFramework.settings.includeMapThings || !(caravan is VehicleCaravan vehicleCaravan))
      return;
    __result.AddRange(vehicleCaravan.Vehicles.OfType<VehiclePawnWithMap>().SelectMany<VehiclePawnWithMap, Thing>((Func<VehiclePawnWithMap, IEnumerable<Thing>>) (v => v.VehicleMap.listerThings.GetAllThings((Predicate<Thing>) (t =>
    {
      if (t.def.category == 2)
        return true;
      return t is Pawn pawn2 && pawn2.IsSlaveOfColony;
    }), false))));
  }
}
